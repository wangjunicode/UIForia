using System;
using System.Collections.Generic;
using UIForia.Compilers.Style;
using UIForia.Util;

namespace UIForia.Compilers {

    public struct StyleSheetReference {

        public string alias;
        public StyleSheet styleSheet;

        public StyleSheetReference(string alias, StyleSheet styleSheet) {
            this.alias = alias;
            this.styleSheet = styleSheet;
        }

    }

    public class TemplateMetaData {

        public int id;
        public string filePath;
        public int usages;
        public StyleSheetReference[] styleReferences; // dictionary? sorted by name? trie?

        public CompiledTemplateData compiledTemplateData;
        internal UIStyleGroupContainer[] styleMap;

        private IndexedStyleRef[] searchMap;

        private static readonly IndexedStyleRef[] s_EmptySearchMap = { };

        public TemplateMetaData(int id, string filePath, UIStyleGroupContainer[] styleMap, StyleSheetReference[] styleReferences) {
            this.id = id;
            this.filePath = filePath;
            this.styleReferences = styleReferences;
            this.styleMap = styleMap;
            BuildSearchMap();
        }

        private void BuildSearchMap() {
            if (styleReferences == null) {
                searchMap = s_EmptySearchMap;
                return;
            }

            int size = 0;

            for (int i = 0; i < styleReferences.Length; i++) {
                size += styleReferences[i].styleSheet.styleGroupContainers.Length;
            }

            if (size == 0) {
                searchMap = s_EmptySearchMap;
                return;
            }

            searchMap = new IndexedStyleRef[size];

            int cnt = 0;

            // todo -- this is ignoring aliases atm

            for (int i = 0; i < styleReferences.Length; i++) {
                string alias = styleReferences[i].alias;
                StyleSheet sheet = styleReferences[i].styleSheet;

                for (int j = 0; j < sheet.styleGroupContainers.Length; j++) {
                    searchMap[cnt++] = new IndexedStyleRef(alias, sheet.styleGroupContainers[j]);
                }
            }

            Array.Sort(searchMap, (a, b) => string.CompareOrdinal(a.name, b.name));
        }

        private struct IndexedStyleRef {

            public readonly string name;
            public readonly string alias;
            public readonly UIStyleGroupContainer container;

            public IndexedStyleRef(string alias, UIStyleGroupContainer container) {
                this.alias = alias;
                this.name = container.name;
                this.container = container;
            }

        }

        public UIStyleGroupContainer ResolveStyleByName(string name) {
            if (string.IsNullOrEmpty(name)) return null;
            int idx = BinarySearch(name);
            if (idx >= 0) {
                return searchMap[idx].container;
            }

            return null;
        }

        public UIStyleGroupContainer ResolveStyleByName(char[] name) {
            if (name == null || name.Length == 0) return null;
            int idx = BinarySearch(name);
            if (idx >= 0) {
                return searchMap[idx].container;
            }

            return null;
        }


        private int BinarySearch(string name) {
            int num1 = 0;
            int num2 = searchMap.Length - 1;
            while (num1 <= num2) {
                int index1 = num1 + (num2 - num1 >> 1);
                int num3 = string.CompareOrdinal(searchMap[index1].name, name);
                if (num3 == 0) {
                    return index1;
                }

                if (num3 < 0) {
                    num1 = index1 + 1;
                }
                else {
                    num2 = index1 - 1;
                }
            }

            return ~num1;
        }

        private int BinarySearch(char[] name) {
            int num1 = 0;
            int num2 = searchMap.Length - 1;
            while (num1 <= num2) {
                int index1 = num1 + (num2 - num1 >> 1);
                int num3 = StringUtil.CharCompareOrdinal(searchMap[index1].name, name);
                if (num3 == 0) {
                    return index1;
                }

                if (num3 < 0) {
                    num1 = index1 + 1;
                }
                else {
                    num2 = index1 - 1;
                }
            }

            return ~num1;
        }

        public UIStyleGroupContainer ResolveStyleByName(char[] alias, string name) {
            if (string.IsNullOrEmpty(name)) return null;

            if (name.Contains(".")) {
                throw new NotImplementedException("Cannot resolve style name with aliases yet");
            }

            for (int i = 0; i < styleReferences.Length; i++) {
                if (styleReferences[i].alias == name) {
                    return styleReferences[i].styleSheet.GetStyleByName(name);
                }
            }

            return null;
        }

        public UIStyleGroupContainer GetStyleById(int styleId) {
            return styleMap[styleId];
        }

    }

}