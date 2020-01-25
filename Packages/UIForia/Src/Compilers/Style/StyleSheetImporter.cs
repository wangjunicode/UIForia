using System;
using System.Collections.Generic;
using System.IO;
using UIForia.Animation;
using UIForia.Exceptions;
using UIForia.Parsing.Style;
using UIForia.Templates;

namespace UIForia.Compilers.Style {

    public class StyleSheetImporter {

        private readonly string basePath;
        private readonly Dictionary<string, StyleSheet> cachedStyleSheets;
        private readonly StyleSheetCompiler compiler;
        private int importedStyleGroupCount;
        public string importResolutionPath; // hack for now that can handle reading imports from streaming assets

        public StyleSheetImporter(string basePath, ResourceManager resourceManager = null) {
            this.basePath = basePath;
            this.compiler = new StyleSheetCompiler(this, resourceManager);
            this.cachedStyleSheets = new Dictionary<string, StyleSheet>();
        }

        public int ImportedStyleSheetCount => cachedStyleSheets.Count;

        public int NextStyleGroupId => importedStyleGroupCount++;

        public StyleSheet Import(in StyleDefinition styleDefinition, bool storeContents = false) {
            string path = Path.Combine(importResolutionPath ?? basePath, styleDefinition.importPath);

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
            
            try {
                sheet = compiler.Compile(path, StyleParser.Parse(contents));
                if (sheet != null) {
                    sheet.path = styleDefinition.importPath;
                    sheet.source = storeContents ? contents : null;
                    cachedStyleSheets.Add(path, sheet);
                }
            }
            catch (ParseException ex) {
                cachedStyleSheets.Add(path, new StyleSheet(null, null, null, null)); // don't reparse failed styles
                ex.SetFileName(path);
                throw;
            }
            
            return sheet;
        }

        public StyleSheet ImportStyleSheetFromFile(string fileName) {
            return Import(new StyleDefinition(null, fileName), true);
        }

        public void Reset() {
            cachedStyleSheets.Clear();
        }

        public AnimationData GetAnimation(string fileName, string animationName) {
            var sheet = cachedStyleSheets[fileName];
            if (sheet == null) {
                return default;
            }

            sheet.TryGetAnimationData(animationName, out AnimationData retn);

            return retn;
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