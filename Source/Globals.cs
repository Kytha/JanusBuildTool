// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
namespace JanusBuildTool
{
    class Global 
    {
        public static string Root;

        private static Type[] _types;
        public static Type[] Types
        {
            get
            {
                if (_types == null)
                {
                    _types = typeof(Program).Assembly.GetTypes();
                }
                return _types;
            }
        }

    }

    public enum Architecture
    {
        x86,
        x64,
        ARM,
        ARMx64
    }

    public enum LinkerOutput
    {
        Executable,
        StaticLibrary
    }
}