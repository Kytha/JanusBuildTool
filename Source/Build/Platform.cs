// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace JanusBuildTool
{
    public abstract class Platform
    {
        private List<Toolchain> _toolchains;
        public abstract PlatformType Type { get; }

        public abstract string ExecutableFileExtension { get; }
        public abstract string ExecutableFilePrefix { get; }
        public abstract string StaticLibraryFileExtension { get; }
        public abstract string StaticLibraryFilePrefix { get; }
        public abstract bool HasRequiredSDKsInstalled { get; }
        protected abstract Toolchain CreateToolchain(Architecture architecture);

        public string GetLinkOutputFileName(string name, LinkerOutput output)
        {
            switch(output)
            {
                case LinkerOutput.Executable: return ExecutableFilePrefix + name + ExecutableFileExtension;
                case LinkerOutput.StaticLibrary: return StaticLibraryFilePrefix + name + StaticLibraryFileExtension;
                default: throw new Exception($"Linker output type {output} not supported");
            }
        }

        public Toolchain GetToolchain(Architecture architecture)
        {
            if(!HasRequiredSDKsInstalled)
                throw new Exception($"The required SDK for {Type} is not installed and therefore cannot be used");
            
            if(_toolchains == null)
            {
                _toolchains = new List<Toolchain>();
            }

            foreach(var t in _toolchains)
            {   
                if(t.architecture == architecture)
                    return t;
            }

            var toolchain = CreateToolchain(architecture);
            _toolchains.Add(toolchain);
            return toolchain;
        }
        private static PlatformType _nativePlatformType = PlatformType.None; 

        private static Platform[] _platforms; 

        public static Platform GetPlatform(PlatformType type) {

            if(_platforms == null)
            {
                _platforms = Global.Types.Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(Platform))).Select(Activator.CreateInstance).Cast<Platform>().ToArray();
            }

            foreach(var platform in _platforms)
            {
                if(platform.Type == type){
                    return platform;
                }
            }
            throw new Exception($"Platform {type} is not supported");
        }
        public static Platform NativePlatform {
            get {
                return GetPlatform(NativePlatformType);   
            }
        }

        public static PlatformType NativePlatformType {
            get
            {
                if(_nativePlatformType == PlatformType.None)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        _nativePlatformType = PlatformType.Windows;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        _nativePlatformType = PlatformType.Linux;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        _nativePlatformType = PlatformType.MacOS;
                    
                }
                return _nativePlatformType;
            }
        } 
    }

    public enum PlatformType 
    {
        None,
        Windows,
        Linux,
        MacOS
    }
}