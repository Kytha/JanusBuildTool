// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.IO;
namespace JanusBuildTool
{
    class Global 
    {
        public static string Root;

        public static string EngineRoot;
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

        public static ProjectInfo project;

        public static Architecture[] AllArchitectures = 
        {
            Architecture.x64,
        };

        public static PlatformType[] AllPlatformTypes = 
        {
            PlatformType.Windows
        };

        public static ConfigurationType[] AllConfigurationTypes = 
        {
            ConfigurationType.Debug
        };

    }

    public enum Architecture
    {
        x86,
        x64,
        ARM,
        ARMx64
    }

    public enum ConfigurationType
    {
        Debug,
        Development,
        Release,
    }
}