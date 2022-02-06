using System;
using System.Collections.Generic;
using System.IO;

namespace JanusBuildTool
{
    public enum TargetType
    {
        NativeCpp,
        DotNet,
    }

    public enum TargetOutputType
    {
        Executable,
        Library,
    }
    public enum TargetLinkType
    {
        Monolithic,
        Modular,
    }

    public class Target
    {
        public string Name;
        public string ProjectName;
        public string OutputName;
        public string FilePath;
        public string FolderPath;
        public TargetType Type = TargetType.NativeCpp;
        public TargetOutputType OutputType = TargetOutputType.Executable;
        public TargetLinkType LinkType = TargetLinkType.Monolithic;
        public PlatformType[] Platforms = Global.AllPlatformTypes;
        public Architecture[] Architectures = Global.AllArchitectures;
        public ConfigurationType[] Configurations = Global.AllConfigurationTypes;
        public string ConfigurationName;
        public List<string> GlobalDefinitions = new List<string>();
        public List<string> Modules = new List<string>();
        public Target()
        {
            var type = GetType();
            Name = type.Name;
            ProjectName = Name;
            OutputName = Name;
        }
        public virtual void Init()
        {
            GlobalDefinitions.Add("UNICODE");
            GlobalDefinitions.Add("_UNICODE");
        }
        public virtual Architecture[] GetArchitectures(PlatformType platform)
        {
            return Architectures;
        }
        public virtual string GetOutputFilePath(BuildOptions options)
        {
            LinkerOutput linkerOutput;
            switch (OutputType)
            {
            case TargetOutputType.Executable:
                linkerOutput = LinkerOutput.Executable;
                break;
            default: throw new ArgumentOutOfRangeException();
            }
            return Path.Combine(options.OutputFolder, options.Platform.GetLinkOutputFileName(OutputName, linkerOutput));
        }
        public virtual void SetupTargetEnvironment(BuildOptions options)
        {
            options.CompileEnv.PreprocessorDefinitions.AddRange(GlobalDefinitions);
            LinkType = TargetLinkType.Monolithic;
            OutputType = TargetOutputType.Executable;
            options.LinkEnv.Output = LinkerOutput.Executable;

            switch (options.Configuration)
            {
            case ConfigurationType.Debug:
                options.CompileEnv.PreprocessorDefinitions.Add("BUILD_DEBUG");
                options.CompileEnv.UseDebugCRT = true;
                break;
            case ConfigurationType.Development:
                options.CompileEnv.PreprocessorDefinitions.Add("BUILD_DEVELOPMENT");
                break;
            case ConfigurationType.Release:
                options.CompileEnv.PreprocessorDefinitions.Add("BUILD_RELEASE");
                break;
            default: throw new ArgumentOutOfRangeException();
            }
            /*
            if (options.CompileEnv.UseDebugCRT)
                options.CompileEnv.PreprocessorDefinitions.Add("_DEBUG");
            else
                options.CompileEnv.PreprocessorDefinitions.Add("NDEBUG");
                */
        }
        public virtual void PreBuild()
        {
        }

        public virtual void PreBuild(BuildOptions buildOptions)
        {
        }

        public virtual void PostBuild(BuildOptions buildOptions)
        {
        }
        public virtual void PostBuild()
        {
        }
    }
}
