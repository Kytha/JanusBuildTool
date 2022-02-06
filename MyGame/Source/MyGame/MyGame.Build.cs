// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using JanusBuildTool;

public class MyGame : GameModule
{
    public override void SetUp(BuildOptions options)
    {
        base.SetUp(options);
        Console.WriteLine("THIS IS A GAME MODULE");
        options.SourceFiles.Add(Path.Combine(FolderPath, "Game.cpp"));
    }
}