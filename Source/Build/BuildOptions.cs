// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.IO;
using System.Collections.Generic;

namespace JanusBuildTool
{
    public class BuildOptions
    {
        public Target Target;
        public Platform Platform;
        public Toolchain Toolchain;
        public Architecture Architecture;
        public ConfigurationType Configuration;
        public CompileEnv CompileEnv;
        public LinkEnv LinkEnv;
        public List<string> SourcePaths = new List<string>();
        public List<string> SourceFiles = new List<string>();
        public List<string> PublicDependencies = new List<string>();
        public List<string> PrivateDependencies = new List<string>();
        public readonly HashSet<string> PublicDefinitions = new HashSet<string>();
        public readonly HashSet<string> PrivateDefinitions = new HashSet<string>();
        public readonly HashSet<string> PublicIncludePaths = new HashSet<string>();
        public readonly HashSet<string> PrivateIncludePaths = new HashSet<string>();
        public HashSet<string> DependencyFiles = new HashSet<string>();
        public HashSet<string> OptionalDependencyFiles = new HashSet<string>();
        public HashSet<string> Libraries = new HashSet<string>();
        public HashSet<string> DelayLoadLibraries = new HashSet<string>();
        public List<string> OutputFiles = new List<string>();
        public string IntermediateFolder;
        public string OutputFolder;
        public string WorkingDirectory;

        internal void MergeSourcePathsIntoSourceFiles()
        {
            if (SourcePaths.Count == 0)
                return;


            for (var i = 0; i < SourcePaths.Count; i++)
            {
                var path = SourcePaths[i];
                if (!Directory.Exists(path))
                    continue;
                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                var count = SourceFiles.Count;
                if (SourceFiles.Count == 0)
                {
                    SourceFiles.AddRange(files);
                }
                else
                {
                    for (int j = 0; j < files.Length; j++)
                    {
                        bool unique = true;
                        for (int k = 0; k < count; k++)
                        {
                            if (SourceFiles[k] == files[j])
                            {
                                unique = false;
                                break;
                            }
                        }
                        if (unique)
                            SourceFiles.Add(files[j]);
                    }
                }
            }
            SourcePaths.Clear();
        }
    }
}