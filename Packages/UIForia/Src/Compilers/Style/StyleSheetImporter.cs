using System;
using System.Collections.Generic;
using System.IO;
using UIForia.Exceptions;
using UIForia.Parsing.Style;
using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Compilers.Style {

    public class StyleSheetImporter {

        private readonly string basePath;
        private readonly List<string> currentlyImportingStylesheets;
        private readonly Dictionary<string, StyleSheet> cachedStyleSheets;
        private readonly StyleSheetCompiler compiler;
        private int importedStyleGroupCount;

        public StyleSheetImporter(string basePath) {
            this.basePath = basePath;
            this.compiler = new StyleSheetCompiler(this);
            this.currentlyImportingStylesheets = new List<string>();
            this.cachedStyleSheets = new Dictionary<string, StyleSheet>();
        }

        public int ImportedStyleSheetCount => cachedStyleSheets.Count;

        public int NextStyleGroupId => importedStyleGroupCount++;

        public StyleSheet Import(in StyleDefinition styleDefinition, bool storeContents = false) {
            string path = Path.Combine(basePath, styleDefinition.importPath);

            if (cachedStyleSheets.TryGetValue(path, out StyleSheet retn)) {
                return retn;
            }

            StyleSheet sheet = default;

            string contents = null;

            if (styleDefinition.body != null) {
                contents = styleDefinition.body;
            }
            else if (File.Exists(path)) {
                contents = File.ReadAllText(path);
            }
            else {
                throw new ParseException(path + " failed to parse style, file doesn't exist or body is not defined");
            }

            currentlyImportingStylesheets.Add(path);

            try {
                sheet = compiler.Compile(path, StyleParser.Parse(contents));
                if (sheet != null) {
                    sheet.path = styleDefinition.importPath;
                    sheet.source = storeContents ? contents : null;
                    cachedStyleSheets.Add(path, sheet);
                }
            }
            catch (ParseException ex) {
                cachedStyleSheets.Add(path, new StyleSheet(null, null, null)); // don't reparse failed styles
                ex.SetFileName(path);
                throw;
            }

            currentlyImportingStylesheets.Remove(path);
            
            return sheet;
        }

        public StyleSheet ImportStyleSheetFromString(string path, string literalTemplate) {
            if (path != null && cachedStyleSheets.TryGetValue(path, out StyleSheet sheet)) {
                return sheet;
            }

            try {
                StyleSheet styleSheet = compiler.Compile(path, StyleParser.Parse(literalTemplate));
                if (path != null) {
                    cachedStyleSheets.Add(path, styleSheet);
                }

                styleSheet.path = path;
                return styleSheet;
            }
            catch (ParseException ex) {
                ex.SetFileName(path);
                throw;
            }
        }

        public StyleSheet ImportStyleSheetFromFile(string fileName) {
            string path = Path.Combine(basePath, fileName);

            if (currentlyImportingStylesheets.Contains(path)) {
                throw new CompileException($"Cannot import style sheet '{fileName}' because it references itself.");
            }

            // pass 1: load file F(1), read imports

            // pass 1a: import file F(n) no further imports? => collect consts + exports, compile file

            // pass 2: recursively add imported consts and compile whole file for each F(n-1) until F(1) is hit again

            if (File.Exists(path)) {
                string contents = File.ReadAllText(path);
                currentlyImportingStylesheets.Add(path);
                StyleSheet result = ImportStyleSheetFromString(fileName, contents);
                currentlyImportingStylesheets.Remove(fileName);
                return result;
            }

            throw new ParseException($"Cannot find style file {fileName}");
        }

        public void Reset() {
            cachedStyleSheets.Clear();
            currentlyImportingStylesheets.Clear();
        }

        public StyleSheet[] GetImportedStyleSheets() {
            StyleSheet[] retn = new StyleSheet[cachedStyleSheets.Count];

            foreach (KeyValuePair<string, StyleSheet> kvp in cachedStyleSheets) {
                retn[kvp.Value.id] = kvp.Value;
            }

            Array.Sort(retn, (a, b) => a.id - b.id);
            
            return retn;
        }

    }

}