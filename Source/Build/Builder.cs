
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace JanusBuildTool {

    class Builder {

        public static readonly Assembly[] DefaultReferences =
        {
            typeof(Enumerable).Assembly, // System.Linq.dll
            typeof(ISet<>).Assembly, // System.dll
            typeof(Builder).Assembly, // Flax.Build.exe
        };

        public static int Build() 
        {
            BuildOptions buildOptions = new BuildOptions();
            var files = GetProjectBuildFiles();
            Assembly assembly;
            Dictionary<string, string> providerOptions = new Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", "v4.0");
            CodeDomProvider provider = new Microsoft.CSharp.CSharpCodeProvider(providerOptions);

            HashSet<string> references = new HashSet<string>();
            foreach (var defaultReference in DefaultReferences)
                references.Add(defaultReference.Location);


            CompilerParameters cp = new CompilerParameters();
            cp.GenerateExecutable = false;
            cp.WarningLevel = 4;
            cp.TreatWarningsAsErrors = false;
            cp.ReferencedAssemblies.AddRange(references.ToArray());
            cp.GenerateInMemory = true;

            CompilerResults cr = provider.CompileAssemblyFromFile(cp, files.ToArray());

            bool hasError = false;
            foreach (CompilerError ce in cr.Errors)
            {
                if (ce.IsWarning)
                {
                    Console.WriteLine(string.Format("{0} at {1}: {2}", ce.FileName, ce.Line, ce.ErrorText));
                }
                else
                {
                    Console.WriteLine(string.Format("{0} at line {1}: {2}", ce.FileName, ce.Line, ce.ErrorText));
                    hasError = true;
                }
            }
            if (hasError)
                throw new Exception("Failed to build assembly.");
            assembly = cr.CompiledAssembly;
            
            
            
            foreach(Type type in assembly.GetTypes())
            {
                if(type.IsAbstract | !type.IsClass)
                    continue;
                if(type.IsSubclassOf(typeof(Module)))
                {
                    var module = (Module)Activator.CreateInstance(type);
                    var moduleFilename = module.Name + ".Build.cs";
                    module.FilePath = files.FirstOrDefault(path => string.Equals(Path.GetFileName(path), moduleFilename, StringComparison.OrdinalIgnoreCase));
                    if (module.FilePath == null)
                    {
                        throw new Exception(string.Format("Failed to find source file path for {0}", module));
                    }
                    module.FolderPath = Path.GetDirectoryName(module.FilePath);
                    module.Init(buildOptions);
                }
            }

            var toolchain = Platform.NativePlatform.GetToolchain(Architecture.x64);

            //string vcToolPath = "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/VC/Tools/MSVC/14.28.29910";
            //string compilerPath = Path.Combine(vcToolPath, "bin","Hostx64","x64", "cl.exe");
            //string platformInclude = Path.Combine(vcToolPath, "include");


            toolchain.SetupEnvironment(buildOptions);
            toolchain.CompileCPP(buildOptions);
            
            return 0;
        }
        private static List<string> GetProjectBuildFiles()
        {
            var files = new List<string>();
            var sources = Path.Combine(Global.Root, "src");
            if(Directory.Exists(sources))
            {   
                FindBuildFiles(sources, files);
            }
            return files;
        }

        private static void FindBuildFiles(string directory, List<string> result)
        {
            string[] files = Directory.GetFiles(directory);
            foreach(string f in files)
            {
                if(f.EndsWith("Build.cs")) {
                    result.Add(f);
                }
            }

            string[] directories = Directory.GetDirectories(directory);
            foreach(string d in directories)
            {
                FindBuildFiles(d,result);
            }
        }
    }
}