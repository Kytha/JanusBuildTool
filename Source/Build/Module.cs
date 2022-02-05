// Copyright (c) Kyle Thatcher. All rights reserved.

using System;

namespace JanusBuildTool
{
    public class Module {

        public string FolderPath;
        public string FilePath;
        public string Name;
        public virtual void Init(BuildOptions options)
        {

        }

        public Module()
        {
            var type = GetType();
            Name = type.Name;
        }

    }
}