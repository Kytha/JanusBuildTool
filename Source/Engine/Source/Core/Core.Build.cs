// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.IO;
using JanusBuildTool;

class Core : Module
{
    public override void SetUp(BuildOptions options)
    {
        options.SourceFiles.Add(Path.Combine(FolderPath, "Application.cpp"));
    }
}