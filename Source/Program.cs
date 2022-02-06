// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.Diagnostics;
using System.IO;

namespace JanusBuildTool
{
    class Program
    {
        static int Main()
        {
            if(CommandLine.Args.Contains("help")) {
                Console.WriteLine(CommandLine.GetHelp(typeof(Configuration)));
                return 0;
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            CommandLine.Configure(typeof(Configuration));
            var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            Global.EngineRoot = Utilities.RemovePathRelativeParts(Path.Combine(Path.GetDirectoryName(executingAssembly.Location), ".."));
            Global.Root = Directory.GetCurrentDirectory();

            var projectFiles = Directory.GetFiles(Global.Root, "*.janusproj", SearchOption.TopDirectoryOnly);
            if (projectFiles.Length == 1)
                Global.project = ProjectInfo.Load(projectFiles[0]);

            try {
                if(Configuration.build)
                {
                    Builder.Build();
                }
            } catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return 1;
            } 
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"Finished in {stopwatch.ElapsedMilliseconds}ms");
            }
            
            return 0;
        }
    }
}