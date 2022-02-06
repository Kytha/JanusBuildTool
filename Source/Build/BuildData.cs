
using System;
using System.Collections.Generic;

namespace JanusBuildTool
{
    public class BuildData
    {
        public Dictionary<Module, BuildOptions> Modules = new Dictionary<Module, BuildOptions>(256);
        public List<Module> ModulesOrderList = new List<Module>();  
        public Rules Rules;
        public ProjectInfo Project;
        public Target Target;
        public BuildOptions TargetOptions;
        public Platform Platform;
        public Toolchain Toolchain;
        public Architecture Architecture;
        public ConfigurationType ConfigurationType;
    } 
}