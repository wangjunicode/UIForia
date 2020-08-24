using System;
using System.Collections.Generic;
using UIForia.Util;

namespace UIForia.Rendering {

    public static partial class StyleUtil {

        public static readonly StylePropertyId[] StylePropertyIdList;
        public static readonly List<StylePropertyId> InheritedProperties;

        private static readonly IntMap_Deprecated<string> s_NameMap;
        private static readonly SizedArray<int> s_CustomTypeMap;

        static StyleUtil() {
            s_NameMap = new IntMap_Deprecated<string>();
            InheritedProperties = new List<StylePropertyId>();
            StylePropertyId[] values = (StylePropertyId[]) Enum.GetValues(typeof(StylePropertyId));
            StylePropertyId[] ignored = { };
            int idx = 0;
            StylePropertyIdList = new StylePropertyId[values.Length - ignored.Length];
            for (int i = 0; i < values.Length; i++) {
                if (Array.IndexOf(ignored, values[i]) != -1) {
                    continue;
                }

                StylePropertyIdList[idx++] = values[i];
                s_NameMap.Add((int) values[i], values[i].ToString());
                if (IsInherited(values[i])) {
                    InheritedProperties.Add(values[i]);
                }
            }
        }

        public static readonly int CustomPropertyStart = 1000;
        
        public static string GetPropertyName(StyleProperty property) {
            string name;
            s_NameMap.TryGetValue((int) property.propertyId, out name);
            return name;
        }

        public static string GetPropertyName(StylePropertyId propertyId) {
            string name;
            s_NameMap.TryGetValue((int) propertyId, out name);
            return name;
        }

        public static int GetCustomPropertyType(StylePropertyId propertyId) {
            if ((int)propertyId < CustomPropertyStart) return 0;
            int idx = (int) propertyId - CustomPropertyStart;
            if (idx < s_CustomTypeMap.size) {
                return s_CustomTypeMap[idx];
            }

            return 0;
        }

    }

}