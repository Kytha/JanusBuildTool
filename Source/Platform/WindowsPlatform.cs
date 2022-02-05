// Copyright (c) Kyle Thatcher. All rights reserved.

using System;

namespace JanusBuildTool
{
    public class WindowsPlatform : Platform
    {
        public override PlatformType Type => PlatformType.Windows;
        public override string ExecutableFileExtension => ".exe";
        public override string ExecutableFilePrefix => "";
        public override string StaticLibraryFileExtension => ".lib";
        public override string StaticLibraryFilePrefix => "";
        private bool _hasRequiredSDKsInstalled;
        public override bool HasRequiredSDKsInstalled => _hasRequiredSDKsInstalled;

        private static string _sdk = string.Empty;
        private static string _toolset = string.Empty;
        public WindowsPlatform()
        {
            var sdk = WindowsPlatform.GetSDK();
            var toolset = WindowsPlatform.GetToolset();
            _hasRequiredSDKsInstalled = !string.IsNullOrEmpty(sdk) && !string.IsNullOrEmpty(toolset);
        }

        protected override Toolchain CreateToolchain(Architecture architecture)
        {
            return new WindowsToolchain(this, architecture);
        }

        public static string GetToolset()
        {
            if(string.IsNullOrEmpty(_toolset))
            {
                // TO DO: Determine path by reading registry keys
                _toolset = "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/VC/Tools/MSVC/14.28.29910";
            }
            return _toolset;
        }

        public static string GetSDK()
        {
            if(string.IsNullOrEmpty(_sdk))
            {
                _sdk = "C:/Program Files (x86)/Windows Kits/10";
            }
            return _sdk;
        }
    }
}
