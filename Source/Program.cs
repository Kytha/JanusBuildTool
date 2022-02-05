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
            Global.Root = Directory.GetCurrentDirectory();
            Project.Load(Global.Root);
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