// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.IO;
using System.Collections.Generic;

namespace JanusBuildTool
{
    public abstract class Toolchain 
    {
        public Platform platform { get; private set; }
        public Architecture architecture { get; private set; }
        public readonly List<string> SystemIncludePaths = new List<string>();
        public readonly List<string> SystemLibraryPaths = new List<string>();

        public PlatformType Type;
        protected Toolchain(Platform platform, Architecture architecture)
        {
            this.platform = platform;
            this.architecture = architecture;
        }
        public virtual void SetupEnvironment(BuildOptions options)
        {
            options.CompileEnv.IncludePaths.AddRange(SystemIncludePaths);
            options.LinkEnv.LibraryPaths.AddRange(SystemLibraryPaths);
        }

        public virtual void PreBuild(BuildOptions buildOptions)
        {

        }

        public virtual void PostBuild(BuildOptions buildOptions)
        {

        }

        public abstract CompileOutput CompileCPP(BuildOptions buildOptions, List<string> cppFiles);

        public abstract bool LinkCPP(BuildData targetBuildData, BuildOptions buildOptions, string outputFilePath);

    }
}