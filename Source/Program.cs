// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.Diagnostics;

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
            try {

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