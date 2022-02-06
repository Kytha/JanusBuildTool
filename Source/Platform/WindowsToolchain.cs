// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace JanusBuildTool {
    public class WindowsToolchain : Toolchain
    {
        public List<string> Sdks = new List<string>();
        private string _toolsetPath;

        private string _compilerPath;

        private string _linkerPath;

        private string _libToolPath;

        private string _xdcmakePath;

        private string _resourceCompilerPath;

        private string _makepriPath;

        public WindowsToolchain(Platform platform, Architecture architecture): base(platform,architecture)
        {
            _toolsetPath = WindowsPlatform.GetToolset();
            var sdk = WindowsPlatform.GetSDK();
            Type = platform.Type;
            _compilerPath = Path.Combine(_toolsetPath,"bin", "HostX64", "x64", "cl.exe");
            _linkerPath = Path.Combine(_toolsetPath,"bin", "HostX64", "x64", "link.exe");
            _libToolPath = Path.Combine(_toolsetPath,"bin", "HostX64", "x64", "lib.exe");
            _xdcmakePath = Path.Combine(_toolsetPath,"bin", "HostX64", "x64", "xdcmake.exe"); 

            SystemIncludePaths.Add(Path.Combine(_toolsetPath, "include"));
            SystemLibraryPaths.Add(Path.Combine(_toolsetPath, "lib", "x64"));

            string includeRootSDK = Path.Combine(sdk, "include", "10.0.19041.0");

            SystemIncludePaths.Add(Path.Combine(includeRootSDK, "ucrt"));
            SystemIncludePaths.Add(Path.Combine(includeRootSDK, "shared"));
            SystemIncludePaths.Add(Path.Combine(includeRootSDK, "um"));
            SystemIncludePaths.Add(Path.Combine(includeRootSDK, "winrt"));

            string libraryRootSDK = Path.Combine(sdk, "lib", "10.0.19041.0");

            SystemLibraryPaths.Add(Path.Combine(libraryRootSDK, "ucrt", "x64"));
            SystemLibraryPaths.Add(Path.Combine(libraryRootSDK, "um", "x64"));
            var binRootSDK = Path.Combine(sdk, "bin", "10.0.19041.0", "x64");
            _resourceCompilerPath = Path.Combine(binRootSDK, "rc.exe");
            _makepriPath = Path.Combine(binRootSDK, "makepri.exe");

        }

        public override void SetupEnvironment(BuildOptions options)
        {
            base.SetupEnvironment(options);

            options.CompileEnv.PreprocessorDefinitions.Add("PLATFORM_WINDOWS");
            options.CompileEnv.PreprocessorDefinitions.Add("WIN64");
            options.CompileEnv.PreprocessorDefinitions.Add("PLATFORM_WIN32");
            options.CompileEnv.PreprocessorDefinitions.Add("WIN32");
            options.CompileEnv.PreprocessorDefinitions.Add("_CRT_SECURE_NO_DEPRECATE");
            options.CompileEnv.PreprocessorDefinitions.Add("_CRT_SECURE_NO_WARNINGS");
            options.CompileEnv.PreprocessorDefinitions.Add("_WINDOWS");

            options.LinkEnv.InputLibraries.Add("dwmapi.lib");
            options.LinkEnv.InputLibraries.Add("kernel32.lib");
            options.LinkEnv.InputLibraries.Add("user32.lib");
            options.LinkEnv.InputLibraries.Add("comdlg32.lib");
            options.LinkEnv.InputLibraries.Add("advapi32.lib");
            options.LinkEnv.InputLibraries.Add("shell32.lib");
            options.LinkEnv.InputLibraries.Add("ole32.lib");
            options.LinkEnv.InputLibraries.Add("oleaut32.lib");
            options.LinkEnv.InputLibraries.Add("delayimp.lib");

        }

        public override void PreBuild(BuildOptions buildOptions)
        {

        }

        public override void PostBuild(BuildOptions buildOptions)
        {

        }

        public override CompileOutput CompileCPP(BuildOptions buildOptions, List<string> cppFiles) 
        {

            var compileEnvironment = buildOptions.CompileEnv;
            var output = new CompileOutput();

            var commonArgs = new List<string>();
            commonArgs.Add("/nologo");
            commonArgs.Add("/c");
            commonArgs.Add("/EHsc");

            foreach (var definition in compileEnvironment.PreprocessorDefinitions)
            {
                commonArgs.Add(string.Format("/D \"{0}\"", definition));
            }
            
            foreach (var includePath in compileEnvironment.IncludePaths)
            {
                commonArgs.Add($"/I\"{includePath}\"");
            }

            var args = new List<string>();
            foreach(string file in cppFiles) {
                args.Clear();
                args.AddRange(commonArgs);
                var sourceFilename = Path.GetFileNameWithoutExtension(file);

                var objFile = Path.Combine(buildOptions.IntermediateFolder, sourceFilename + ".obj");
                args.Add($"/Fo\"{objFile}\"");
                args.Add($"\"{file}\""); 

                string CommandArguments = string.Join(" ", args);
                Console.WriteLine(CommandArguments);
                Console.WriteLine("\n\n");


                var startInfo = new ProcessStartInfo
                {
                    WorkingDirectory = buildOptions.WorkingDirectory,
                    FileName = _compilerPath,
                    Arguments = CommandArguments,
                    UseShellExecute = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                Process process = null;
                try
                {
                    try
                    {
                        process = new Process();
                        process.StartInfo = startInfo;
                        process.OutputDataReceived += ProcessDebugOutput;
                        process.ErrorDataReceived += ProcessDebugOutput;
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Failed to start local process for task");
                        Console.WriteLine($"{ex.Message}"); 
                    }
                    process.WaitForExit();
                }
                finally
                {
                    output.ObjectFiles.Add(objFile);
                    process?.Close();
                }
            }
            return output;
        }

        public override bool LinkCPP(BuildData targetBuildData, BuildOptions buildOptions, string outputFilePath)
        {

            var linkEnvironment = buildOptions.LinkEnv;
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
            var args = new List<string>();

            args.Add("/NOLOGO");
            args.Add("/ERRORREPORT:PROMPT");

            args.Add($"/OUT:\"{outputFilePath}\"");
            args.Add("/MACHINE:x64");
            args.Add("/SUBSYSTEM:CONSOLE");

            args.Add("/MANIFEST:NO");
            
                    // Fixed Base Address
            args.Add("/FIXED:NO");
            
            // Additional lib paths
            foreach (var libpath in linkEnvironment.LibraryPaths)
            {
                args.Add($"/LIBPATH:\"{libpath}\"");
            }

            // Input libraries
            foreach (var library in linkEnvironment.InputLibraries)
            {
                args.Add($"\"{library}\"");
            }

            // Input files
            foreach (var file in linkEnvironment.InputFiles)
            {
                args.Add($"\"{file}\"");
            }

            string CommandArguments = string.Join(" ", args);
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = buildOptions.WorkingDirectory,
                FileName = _linkerPath,
                Arguments = CommandArguments,
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            Process process = null;
            try
            {
                try
                {
                    process = new Process();
                    process.StartInfo = startInfo;
                    process.OutputDataReceived += ProcessDebugOutput;
                    process.ErrorDataReceived += ProcessDebugOutput;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Failed to start local process for task");
                    Console.WriteLine($"{ex.Message}");
                    return false; 
                }
                process.WaitForExit();
            }
            finally
            {
                process?.Close();
            }
            return true;
        }

        private static void ProcessDebugOutput(object sender, DataReceivedEventArgs e)
        {
            string output = e.Data;
            if (output != null)
            {
                Console.Write(output);
            }
        }

    }
}