// Copyright (c) Kyle Thatcher. All rights reserved.

using System;

namespace JanusBuildTool
{
    public class Module {

        public string FolderPath;
        public string FilePath;
        public string Name;
        public virtual void Init()
        {
        }

        public virtual void SetUpEnvironment(BuildOptions options)
        {
            options.CompileEnv.PreprocessorDefinitions.AddRange(options.PublicDefinitions);
            options.CompileEnv.PreprocessorDefinitions.AddRange(options.PrivateDefinitions);

            options.CompileEnv.IncludePaths.AddRange(options.PublicIncludePaths);
            options.CompileEnv.IncludePaths.AddRange(options.PrivateIncludePaths);
        }
        
        public virtual void SetUp(BuildOptions options)
        {
        }

        public Module()
        {
            var type = GetType();
            Name = type.Name;
        }

    }
}