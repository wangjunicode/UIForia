using System;
using System.Collections.Generic;
using System.IO;
using UIForia.Parsing.Style;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Compilers.Style {

    public class StyleSheetImporter {

        private readonly List<string> m_CurrentlyImportingStylesheets;
        private readonly Dictionary<string, StyleSheet> m_CachedStyleSheets;
        public readonly Application app;

        public StyleSheetImporter(Application app) {
            this.app = app;
            m_CurrentlyImportingStylesheets = new List<string>();
            m_CachedStyleSheets = new Dictionary<string, StyleSheet>();
        }

        public StyleSheet ImportStyleSheet(string id, string literalTemplate) {
            if (m_CachedStyleSheets.TryGetValue(id, out StyleSheet sheet)) {
                return sheet;
            }

            try {
                StyleSheet styleSheet = new StyleSheetCompiler(this).Compile(StyleParser2.Parse(literalTemplate));
                m_CachedStyleSheets.Add(id, styleSheet);
                return styleSheet;
            }
            catch (ParseException ex) {
                Debug.Log("Error compiling file: " + id);
                throw ex;
            }
        }

        public StyleSheet ImportStyleSheetFromFile(string fileName) {
            if (m_CurrentlyImportingStylesheets.Contains(fileName)) {
                throw new CompileException($"Cannot import style sheet '{fileName}' because it references itself.");
            }

            // pass 1: load file F(1), read imports

            // pass 1a: import file F(n) no further imports? => collect consts + exports, compile file

            // pass 2: recursively add imported consts and compile whole file for each F(n-1) until F(1) is hit again

            if (File.Exists(UnityEngine.Application.dataPath + "/" + fileName)) {
                string contents = File.ReadAllText(UnityEngine.Application.dataPath + "/" + fileName);
                m_CurrentlyImportingStylesheets.Add(fileName);
                StyleSheet result = ImportStyleSheet(fileName, contents);
                m_CurrentlyImportingStylesheets.Remove(fileName);
                return result;
            }

            throw new ParseException($"Cannot find style file {fileName}");
        }

        public UIStyleGroupContainer GetStyleGroupsByTagName(string idOrPath, string literalTemplate, string tagName) {
            if (literalTemplate != null) {
                return ImportStyleSheet(idOrPath, literalTemplate).GetStyleGroupsByTagName(tagName);
            }

            return ImportStyleSheetFromFile(idOrPath).GetStyleGroupsByTagName(tagName);
        }

        public UIStyleGroupContainer GetStyleGroupByStyleName(string idOrPath, string literalTemplate, string styleName) {
            if (literalTemplate != null) {
                return ImportStyleSheet(idOrPath, literalTemplate).GetStyleGroupByStyleName(styleName);
            }

            return ImportStyleSheetFromFile(idOrPath).GetStyleGroupByStyleName(styleName);
        }

        public void Reset() {
            m_CachedStyleSheets.Clear();
            m_CurrentlyImportingStylesheets.Clear();
        }

    }

}