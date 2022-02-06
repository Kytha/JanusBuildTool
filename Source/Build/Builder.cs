// Copyright (c) Kyle Thatcher. All rights reserved.
using System;
using System.Collections.Generic;
using System.IO;


namespace JanusBuildTool {

    class Builder {

        public static int Build() 
        {

            var rules = Rules.GenerateRules();

            foreach(var target in rules.targets)
            {
                foreach(PlatformType platformType in target.Platforms)
                {
                    foreach(Architecture architecture in target.Architectures)
                    {
                        foreach(ConfigurationType configurationType in target.Configurations)
                        {
                            var platform = Platform.GetPlatform(platformType);
                            var toolchain = platform.GetToolchain(architecture);
                            var buildContext = new Dictionary<Target, BuildData>();
                            BuildTargetNativeCpp(rules, target, toolchain, configurationType, buildContext);
                        }
                    }
                }
            }

            //string vcToolPath = "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/VC/Tools/MSVC/14.28.29910";
            //string compilerPath = Path.Combine(vcToolPath, "bin","Hostx64","x64", "cl.exe");
            //string platformInclude = Path.Combine(vcToolPath, "include");
            return 0;
        }

        private static void BuildTargetNativeCpp(Rules rules, Target target, Toolchain toolchain, ConfigurationType configuration, Dictionary<Target, BuildData> buildContext) 
        {
            var project = Global.project;

            var targetBuildOptions = Rules.GetBuildOptions(target, toolchain, toolchain.platform, toolchain.architecture, configuration, project.ProjectFolderPath);
            var buildData = new BuildData
            {
                Project = project,
                Platform = toolchain.platform,
                Toolchain = toolchain,
                Architecture = toolchain.architecture,
                ConfigurationType = configuration,
                Target = target,
                TargetOptions = targetBuildOptions,
                Rules = rules
            };
            buildContext.Add(target, buildData);

            /*
            foreach(string moduleName in target.Modules)
            {
                moduleLookup.TryGetValue(moduleName, out var module);
                if(module == null)
                    throw new Exception($"Could not find module {moduleName}");
                module.SetUp(buildOptions);
                toolchain.CompileCPP(buildOptions);
            }
            toolchain.LinkCPP(buildOptions);
            */

            foreach (var moduleName in target.Modules)
            {
                var module = rules.GetModule(moduleName);
                if (module != null)
                {
                    CollectModules(buildData, module);
                }
            }

            foreach (var module in buildData.ModulesOrderList)
            {
                BuildOptions moduleOptions = BuildModule(buildData, module); 
                foreach (var e in moduleOptions.OutputFiles)
                            buildData.TargetOptions.LinkEnv.InputFiles.Add(e);
                foreach (var e in moduleOptions.DependencyFiles)
                            buildData.TargetOptions.DependencyFiles.Add(e);
                buildData.TargetOptions.Libraries.AddRange(moduleOptions.Libraries);
            }
            var outputTargetFilePath = target.GetOutputFilePath(targetBuildOptions);
            var outputPath = Path.GetDirectoryName(outputTargetFilePath);
            targetBuildOptions.Toolchain.LinkCPP(buildData, targetBuildOptions, outputTargetFilePath);
        }

        private static BuildOptions BuildModule(BuildData buildData, Module module)
        {
            if (buildData.Modules.TryGetValue(module, out var moduleOptions))
            {
                BuildModuleInner(buildData, module, moduleOptions);
            }
            return moduleOptions;
        }
        internal static void BuildModuleInner(BuildData buildData, Module module, BuildOptions moduleOptions, bool withApi = true)
        {
            // Inherit build environment from dependent modules
            foreach (var moduleName in moduleOptions.PrivateDependencies)
            {
                var dependencyModule = buildData.Rules.GetModule(moduleName);
                if (dependencyModule != null && buildData.Modules.TryGetValue(dependencyModule, out var dependencyOptions))
                {
                    foreach (var e in dependencyOptions.OutputFiles)
                        moduleOptions.LinkEnv.InputFiles.Add(e);
                    foreach (var e in dependencyOptions.DependencyFiles)
                        moduleOptions.DependencyFiles.Add(e);
                    foreach (var e in dependencyOptions.OptionalDependencyFiles)
                        moduleOptions.OptionalDependencyFiles.Add(e);
                    foreach (var e in dependencyOptions.PublicIncludePaths)
                        moduleOptions.PrivateIncludePaths.Add(e);
                    moduleOptions.Libraries.AddRange(dependencyOptions.Libraries);
                }
            }
            foreach (var moduleName in moduleOptions.PublicDependencies)
            {
                Console.WriteLine($"The module {moduleName} is a dependency of {module}");
                Console.WriteLine("");
                
                var dependencyModule = buildData.Rules.GetModule(moduleName);
                if (dependencyModule != null && buildData.Modules.TryGetValue(dependencyModule, out var dependencyOptions))
                {
                    foreach (var e in dependencyOptions.OutputFiles)
                        moduleOptions.LinkEnv.InputFiles.Add(e);
                    foreach (var e in dependencyOptions.DependencyFiles)
                        moduleOptions.DependencyFiles.Add(e);
                    foreach (var e in dependencyOptions.OptionalDependencyFiles)
                        moduleOptions.OptionalDependencyFiles.Add(e);
                    foreach (var e in dependencyOptions.PublicIncludePaths)
                        moduleOptions.PublicIncludePaths.Add(e);
                    moduleOptions.Libraries.AddRange(dependencyOptions.Libraries);
                }
            }

            // Setup actual build environment
            module.SetUpEnvironment(moduleOptions);
            moduleOptions.MergeSourcePathsIntoSourceFiles();

            // Collect all files to compile
            var cppFiles = new List<string>(moduleOptions.SourceFiles.Count / 2);
            for (int i = 0; i < moduleOptions.SourceFiles.Count; i++)
            {
                if (moduleOptions.SourceFiles[i].EndsWith(".cpp", StringComparison.OrdinalIgnoreCase))
                    cppFiles.Add(moduleOptions.SourceFiles[i]);
            }

            // Compile all source files
            var compilationOutput = buildData.Toolchain.CompileCPP(moduleOptions, cppFiles);
            foreach (var e in compilationOutput.ObjectFiles)
                moduleOptions.LinkEnv.InputFiles.Add(e);
            if (buildData.TargetOptions.LinkEnv.GenerateDocumentation)
            {
                // TODO: find better way to add generated doc files to the target linker (module exports the output doc files?)
                buildData.TargetOptions.LinkEnv.DocumentationFiles.AddRange(compilationOutput.DocumentationFiles);
            }
            {
                // Use direct linking of the module object files into the target
                moduleOptions.OutputFiles.AddRange(compilationOutput.ObjectFiles);

                // Forward the library includes required by this module
                moduleOptions.OutputFiles.AddRange(moduleOptions.LinkEnv.InputFiles);
            }
            
        }

        private static BuildOptions CollectModules(BuildData buildData, Module module)
        {
            if (!buildData.Modules.TryGetValue(module, out var moduleOptions))
            {
                var outputPath = Path.Combine(buildData.TargetOptions.IntermediateFolder, module.Name);
                Directory.CreateDirectory(outputPath);
                moduleOptions = new BuildOptions
                {
                    Target = buildData.Target,
                    Platform = buildData.Platform,
                    Toolchain = buildData.Toolchain,
                    Architecture = buildData.Architecture,
                    Configuration = buildData.ConfigurationType,
                    CompileEnv = (CompileEnv)buildData.TargetOptions.CompileEnv.Clone(),
                    LinkEnv = (LinkEnv)buildData.TargetOptions.LinkEnv.Clone(),
                    IntermediateFolder = outputPath,
                    OutputFolder = outputPath,
                    WorkingDirectory = buildData.TargetOptions.WorkingDirectory,
                };
                moduleOptions.SourcePaths.Add(module.FolderPath);
                module.SetUp(moduleOptions);
                moduleOptions.MergeSourcePathsIntoSourceFiles();

                foreach (var moduleName in moduleOptions.PrivateDependencies)
                {
                    var dependencyModule = buildData.Rules.GetModule(moduleName);
                    if (dependencyModule != null)
                    {
                        var dependencyOptions = CollectModules(buildData, dependencyModule);
                        foreach (var e in dependencyOptions.PublicDefinitions)
                            moduleOptions.PrivateDefinitions.Add(e);
                    } else 
                    {
                        throw new Exception($"Missing Module {dependencyModule.Name} referenced by module {module.Name}");
                    }
                }

                foreach (var moduleName in moduleOptions.PublicDependencies)
                {
                    var dependencyModule = buildData.Rules.GetModule(moduleName);
                    if (dependencyModule != null)
                    {
                        var dependencyOptions = CollectModules(buildData, dependencyModule);
                        foreach (var e in dependencyOptions.PublicDefinitions)
                            moduleOptions.PublicDefinitions.Add(e);
                    } else 
                    {
                        Console.WriteLine($"Missing module");
                        throw new Exception($"Missing Module {dependencyModule.Name} referenced by module {module.Name}");
                    }
                }
                buildData.Modules.Add(module, moduleOptions);
                buildData.ModulesOrderList.Add(module);


            }
            return moduleOptions;
        }

    }
}