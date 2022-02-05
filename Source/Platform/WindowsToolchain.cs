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

            options.PreprocessorDefinitions.Add("PLATFORM_WINDOWS");

            options.InputLibraries.Add("dwmapi.lib");
            options.InputLibraries.Add("kernel32.lib");
            options.InputLibraries.Add("user32.lib");
            options.InputLibraries.Add("comdlg32.lib");
            options.InputLibraries.Add("advapi32.lib");
            options.InputLibraries.Add("shell32.lib");
            options.InputLibraries.Add("ole32.lib");
            options.InputLibraries.Add("oleaut32.lib");
            options.InputLibraries.Add("delayimp.lib");

        }

        public override void PreBuild(BuildOptions buildOptions)
        {

        }

        public override void PostBuild(BuildOptions buildOptions)
        {

        }

        public override bool CompileCPP(BuildOptions buildOptions) 
        {
            buildOptions.args.Clear();
            buildOptions.CompilerPath = _compilerPath;

            foreach(string file in buildOptions.SourceFiles) {

                var sourceFilename = Path.GetFileNameWithoutExtension(file);

                buildOptions.args.Add("/nologo");
                buildOptions.args.Add("/c");
                buildOptions.args.Add("/EHsc");
                foreach(var includePath in buildOptions.IncludePaths)
                {
                    buildOptions.args.Add($"/I\"{includePath}\"");
                }

                var objFile = Path.Combine(Global.Root, sourceFilename + ".obj");
                buildOptions.args.Add($"/Fo\"{objFile}\"");
                buildOptions.args.Add($"\"{file}\""); 

                string CommandArguments = string.Join(" ", buildOptions.args);
                Console.WriteLine(CommandArguments);


                var startInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Global.Root,
                    FileName = buildOptions.CompilerPath,
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
            }
            return true;
        }

        public override bool LinkCPP()
        {
            return false;
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