using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Threading;
using System.Diagnostics;

namespace JanusBuildTool
{
    public class Rules 
    {
        public static readonly Assembly[] DefaultReferences =
        {
            typeof(Enumerable).Assembly, // System.Linq.dll
            typeof(ISet<>).Assembly, // System.dll
            typeof(Builder).Assembly, // JanusBuildTool.exe
        };
        private static Rules _rules;
        public readonly Assembly assembly;
        private readonly Dictionary<string, Module> _modulesLookup = new Dictionary<string, Module>();
        public readonly Target[] targets;
        public readonly Module[] modules;

        public Rules(Assembly assembly, Target[] targets, Module[] modules) 
        {  
            foreach(var module in modules)
            {
                _modulesLookup.Add(module.Name, module);
            }
            this.modules = modules;
            this.targets = targets;
            this.assembly = assembly;
        }

        public Target GetTarget(string name)
        {
            return targets.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        public Module GetModule(string name)
        {
            if (!_modulesLookup.TryGetValue(name, out var module))
                module = modules.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            return module;
        }

        public static BuildOptions GetBuildOptions(Target target, Toolchain toolchain, Platform platform, Architecture architecture, ConfigurationType configuration, string WorkingDirectory)
        {
            string platformName = platform.Type.ToString();
            string architectureName = architecture.ToString();
            string configurationName = configuration.ToString();
            var options = new BuildOptions
            {
                CompileEnv = new CompileEnv(),
                LinkEnv = new LinkEnv(),
                IntermediateFolder = Path.Combine(WorkingDirectory, Configuration.IntermediateFolder, target.Name, platformName, architectureName, configurationName),
                OutputFolder = Path.Combine(WorkingDirectory, Configuration.BinariesFolder, target.Name, platformName, architectureName, configurationName),
                WorkingDirectory = WorkingDirectory,
            };
            options.Architecture = architecture;
            options.Configuration = configuration;
            options.Platform = platform;
            options.Target = target;
            options.Toolchain = toolchain;

            toolchain?.SetupEnvironment(options);
            target.SetupTargetEnvironment(options);
            return options;
        }

        public static Rules GenerateRules() 
        {
            if(_rules == null) 
            {

                var projects = Global.project.GetAllProjects();
                List<string> files = new List<string>();
                foreach (var project in projects)
                {
                    files.AddRange(GetProjectBuildFiles(project));
                }

                Assembly assembly;
                Dictionary<string, string> providerOptions = new Dictionary<string, string>();
                providerOptions.Add("CompilerVersion", "v4.0");
                CodeDomProvider provider = new Microsoft.CSharp.CSharpCodeProvider(providerOptions);

                HashSet<string> references = new HashSet<string>();
                foreach (var defaultReference in DefaultReferences)
                    references.Add(defaultReference.Location);


                CompilerParameters cp = new CompilerParameters();
                cp.GenerateExecutable = false;
                cp.WarningLevel = 4;
                cp.TreatWarningsAsErrors = false;
                cp.ReferencedAssemblies.AddRange(references.ToArray());
                cp.GenerateInMemory = true;

                CompilerResults cr = provider.CompileAssemblyFromFile(cp, files.ToArray());

                bool hasError = false;
                foreach (CompilerError ce in cr.Errors)
                {
                    if (ce.IsWarning)
                    {
                        Console.WriteLine(string.Format("{0} at {1}: {2}", ce.FileName, ce.Line, ce.ErrorText));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("{0} at line {1}: {2}", ce.FileName, ce.Line, ce.ErrorText));
                        hasError = true;
                    }
                }
                if (hasError)
                    throw new Exception("Failed to build assembly.");
                assembly = cr.CompiledAssembly;
                
                var targetObjects = new List<Target>(16);
                var moduleObjects = new List<Module>(256);
                var moduleLookup = new Dictionary<string, Module>();

                // Get Rules
                foreach(Type type in assembly.GetTypes())
                {
                    if(type.IsAbstract | !type.IsClass)
                        continue;

                    if(type.IsSubclassOf(typeof(Target)))
                    {
                        var target = (Target)Activator.CreateInstance(type);
                        var targetFilename = target.Name + ".Build.cs";
                        target.FilePath = files.FirstOrDefault(path => string.Equals(Path.GetFileName(path), targetFilename, StringComparison.OrdinalIgnoreCase));
                        if (target.FilePath == null)
                        {
                            throw new Exception(string.Format("Failed to find source file path for {0}", target));
                        }
                        target.FolderPath = Path.GetDirectoryName(target.FilePath);
                        target.Init();
                        targetObjects.Add(target);
                    }
                    if(type.IsSubclassOf(typeof(Module)))
                    {
                        var module = (Module)Activator.CreateInstance(type);
                        var moduleFilename = module.Name + ".Build.cs";
                        module.FilePath = files.FirstOrDefault(path => string.Equals(Path.GetFileName(path), moduleFilename, StringComparison.OrdinalIgnoreCase));
                        if (module.FilePath == null)
                        {
                            throw new Exception(string.Format("Failed to find source file path for {0}", module));
                        }
                        module.FolderPath = Path.GetDirectoryName(module.FilePath);
                        module.Init();
                        moduleObjects.Add(module);
                    }
                }

                _rules = new Rules(assembly, targetObjects.ToArray(), moduleObjects.ToArray());
            }
            return _rules;
        }

        private static List<string> GetProjectBuildFiles(ProjectInfo project)
        {
            var files = new List<string>();
            var sources = Path.Combine(project.ProjectFolderPath, "Source");
            if(Directory.Exists(sources))
            {   
                FindBuildFiles(sources, files);
            }
            return files;
        }

        private static void FindBuildFiles(string directory, List<string> result)
        {
            string[] files = Directory.GetFiles(directory);
            foreach(string f in files)
            {
                if(f.EndsWith("Build.cs")) {
                    result.Add(f);
                }
            }

            string[] directories = Directory.GetDirectories(directory);
            foreach(string d in directories)
            {
                FindBuildFiles(d,result);
            }
        }
    }
}