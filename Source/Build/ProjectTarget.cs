using System;
using System.IO;
using System.Linq;

namespace JanusBuildTool
{
    public abstract class ProjectTarget : Target
    {
        public ProjectInfo Project;

        /// <inheritdoc />
        public override void Init()
        {
            base.Init();

            // Load project
            var projectFiles = Directory.GetFiles(Path.Combine(FolderPath, ".."), "*.janusproj", SearchOption.TopDirectoryOnly);
            if (projectFiles.Length == 0)
                throw new Exception("Missing project file. Folder: " + FolderPath);
            else if (projectFiles.Length > 1)
                throw new Exception("Too many project files. Don't know which to pick. Folder: " + FolderPath);
            Project = ProjectInfo.Load(projectFiles[0]);

            // Initialize
            ProjectName = OutputName = Project.Name;
        }

        /// <inheritdoc />
        public override void SetupTargetEnvironment(BuildOptions options)
        {
            base.SetupTargetEnvironment(options);

            // Add include paths for this project sources and engine third-party sources
            //var depsRoot = options.DepsFolder;
            //options.LinkEnv.LibraryPaths.Add(depsRoot);

            // Add include paths for this and all referenced projects sources
            foreach (var project in Project.GetAllProjects())
            {
                options.CompileEnv.IncludePaths.Add(Path.Combine(project.ProjectFolderPath, "Source"));
            }
        }
    }
}
