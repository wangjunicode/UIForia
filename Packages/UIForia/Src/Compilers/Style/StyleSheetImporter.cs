using System.Collections.Generic;
using System.IO;
using UIForia.Exceptions;
using UIForia.Parsing.Style;

namespace UIForia.Compilers.Style {

    public class StyleSheetImporter {

        public readonly Application app;
        private readonly List<string> m_CurrentlyImportingStylesheets;
        private readonly Dictionary<string, StyleSheet> m_CachedStyleSheets;
        private readonly StyleSheetCompiler m_Compiler;
        
        public StyleSheetImporter(Application app) {
            this.app = app;
            m_Compiler = new StyleSheetCompiler(this);
            m_CurrentlyImportingStylesheets = new List<string>();
            m_CachedStyleSheets = new Dictionary<string, StyleSheet>();
        }
        
        public StyleSheet ImportStyleSheetFromString(string id, string literalTemplate) {
            if (id != null && m_CachedStyleSheets.TryGetValue(id, out StyleSheet sheet)) {
                return sheet;
            }

            try {
                StyleSheet styleSheet = m_Compiler.Compile(id, StyleParser.Parse(literalTemplate));
                if (id != null) {
                    m_CachedStyleSheets.Add(id, styleSheet);
                }

                return styleSheet;
            }
            catch (ParseException ex) {
                ex.SetFileName(id); 
                throw;
            }
        }

        public StyleSheet ImportStyleSheetFromFile(string fileName) {
            if (m_CurrentlyImportingStylesheets.Contains(fileName)) {
                throw new CompileException($"Cannot import style sheet '{fileName}' because it references itself.");
            }

            // pass 1: load file F(1), read imports

            // pass 1a: import file F(n) no further imports? => collect consts + exports, compile file

            // pass 2: recursively add imported consts and compile whole file for each F(n-1) until F(1) is hit again

            
            // null check is for test cases without an app so that the importer can be used stand-alone

            string path = null;
//            if (Application.Settings.loadTemplatesFromStreamingAssets) {
//                path = Path.Combine(Application.Settings.StreamingAssetPath, fileName);
//            }
//            else {
//                path = app == null ? UnityEngine.Application.dataPath + "/" + fileName : app.TemplateRootPath + "/" + fileName;
//            }

            path = app == null ? UnityEngine.Application.dataPath + "/" + fileName : Application.Settings.GetStylePath(app.TemplateRootPath, fileName);
            
            if (File.Exists(path)) {
                string contents = File.ReadAllText(path);
                m_CurrentlyImportingStylesheets.Add(fileName);
                StyleSheet result = ImportStyleSheetFromString(fileName, contents);
                m_CurrentlyImportingStylesheets.Remove(fileName);
                return result;
            }

            throw new ParseException($"Cannot find style file {fileName}");
        }

        public void Reset() {
            m_CachedStyleSheets.Clear();
            m_CurrentlyImportingStylesheets.Clear();
        }

    }

}