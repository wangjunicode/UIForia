using System;
using UIForia.NewStyleParsing;

namespace UIForia {

    public class StyleFile {

        public Module module;
        public DateTime lastWriteTime;
        public string filePath;
        public string contents;
        public string parseCachePath;
        public string compileCachePath;
        public ParsedStyleFile parseResult;
        public CompiledStyleFile compileResult;

    }

}