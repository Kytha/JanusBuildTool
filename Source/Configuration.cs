// Copyright (c) Kyle Thatcher. All rights reserved.

using System.Collections.Generic;

namespace JanusBuildTool
{
    public class Configuration
    {
        [CommandLine("consoleLog", "Enables logging to the console" )]
        public static bool ConsoleLog = true;

        [CommandLine("logFile", "<path>", "Specifies the path to the log file" )]
        public static string LogFilePath = "Cache/Intermediate/Log.txt";

        [CommandLine("timestamps", "Includes timestamping in log" )]
        public static bool Timestamps = true;
        
        [CommandLine("build", "Builds project" )]
        public static bool build = true;

        [CommandLine("buildPlatforms", "Windows,Linux","Specifies the platforms to build for" )]
        public static string[] BuildPlatforms;

        [CommandLine("architecture", "x86,x64,ARM","Specifies the architectures to build for" )]
        public static string[] BuildArchitectures;

        [CommandLine("binaries", "<path>", "Specifies the binaries path" )]
        public static string BinariesFolder = "Binaries";

        [CommandLine("intermediates", "<path>", "Specifies the intermediates path" )]
        public static string IntermediateFolder = "Cache/Intermediate";

        [CommandLine("genProject", "<path>", "Generates the projects for the workspace" )]
        public static bool GenProjects = false;

        [CommandLine("vscode", "<path>", "Generates visual studio code project files" )]
        public static bool ProjectFormatVSCode = true;
        public static List<string> CustomDefines = new List<string>();
    }
}
