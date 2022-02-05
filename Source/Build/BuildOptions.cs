// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.Collections.Generic;

namespace JanusBuildTool
{
    public class BuildOptions
    {
        public List<string> SourceFiles = new List<string>();
        public List<string> IncludePaths = new List<string>();
        public List<string> LibraryPaths = new List<string>();

        public List<string> InputLibraries = new List<string>();

        public List<string> PreprocessorDefinitions = new List<string>();

        public List<string> args = new List<string>();


        public string CompilerPath;
    }
}