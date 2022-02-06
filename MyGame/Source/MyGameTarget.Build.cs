// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using JanusBuildTool;

public class MyGameTarget : GameTarget
{
    public override void Init()
    {
        base.Init();
        Modules.Add("MyGame");
    }
}