using System;
using System.Collections.Generic;
using System.IO;
using UIForia.Parsing.Style;

namespace UIForia.Compilers.Style {
    public class StyleSheetImporter {

        private static readonly List<string> s_CurrentlyImportingStylesheets = new List<string>();
        
        private static readonly Dictionary<string, StyleSheet> s_CachedStyleSheets = new Dictionary<string, StyleSheet>();

        private StyleSheetCompiler compiler;

        public StyleSheetImporter() {
            this.compiler = new StyleSheetCompiler(this);
        }

        public StyleSheet importStyleSheet(string id, string literalTemplate) {
            StyleSheet styleSheet = compiler.Compile(StyleParser2.Parse(literalTemplate));
            s_CachedStyleSheets.Add(id, styleSheet);
            return styleSheet;
        }

        public StyleSheet importStyleSheetFromFile(string fileName) {
            if (s_CurrentlyImportingStylesheets.Contains(fileName)) {
                throw new CompileException($"Cannot import style sheet '{fileName}' because it references itself.");
            }
            
            // pass 1: load file F(1), read imports
            
            // pass 1a: import file F(n) no further imports? => collect consts + exports, compile file
            
            // pass 2: recursively add imported consts and compile whole file for each F(n-1) until F(1) is hit again

            if (File.Exists(UnityEngine.Application.dataPath + "/" + fileName)) {
                string contents = File.ReadAllText(UnityEngine.Application.dataPath + "/" + fileName);
                s_CurrentlyImportingStylesheets.Add(fileName);
                StyleSheet result = importStyleSheet(fileName, contents);
                s_CurrentlyImportingStylesheets.Remove(fileName);
                return result;
            }

            throw new ParseException($"Cannot find style file {fileName}");
        }
    }
}
