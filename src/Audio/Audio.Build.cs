// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using JanusBuildTool;

public class Audio : Module
{
    public override void Init(BuildOptions options)
    {
        Console.WriteLine("THIS IS AUDIO");
        options.SourceFiles.Add(Path.Combine(FolderPath, "Audio.cpp"));

    }
}