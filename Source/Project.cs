// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
namespace JanusBuildTool
{
    public class Project
    {
        static Project _instance;
        public string IntermediatePath = "Cache/Intermediate/Program";
        public string OutputPath = "Binaries/Program";

        public Project() {
            if(_instance != null)
            {
                throw new Exception("Cannot initialize project. A project is already loaded.");
            }
            else
                _instance = this;
        } 

        public static Project Load(string folderPath)
        {
            return new Project();
        } 

        public static Project Current {
            get {
                return _instance;
            }   
        }

    }
}