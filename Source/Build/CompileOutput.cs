using System.Collections.Generic;

namespace JanusBuildTool
{
    public class CompileOutput
    {
        public readonly List<string> ObjectFiles = new List<string>();
        public readonly List<string> DebugDataFiles = new List<string>();
        public readonly List<string> DocumentationFiles = new List<string>();
    }
}