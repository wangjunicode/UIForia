using System;
using UIForia;

namespace UIForia.Rendering {

    public static partial class StyleUtil {

        public static readonly StylePropertyId[] StylePropertyIdList;
        private static readonly IntMap<string> s_NameMap;
        
        static StyleUtil() {
            s_NameMap = new IntMap<string>();
            StylePropertyId[] values = (StylePropertyId[]) Enum.GetValues(typeof(StylePropertyId));
            StylePropertyId[] ignored = {
                StylePropertyId.__TextPropertyStart__,
                StylePropertyId.__TextPropertyEnd__
            };
            int idx = 0;
            StylePropertyIdList = new StylePropertyId[values.Length - ignored.Length];
            for (int i = 0; i < values.Length; i++) {
                if (Array.IndexOf(ignored, values[i]) != -1) {
                    continue;
                }

                StylePropertyIdList[idx++] = values[i];
                s_NameMap.Add((int)values[i], values[i].ToString());
            }
        }

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

        public static bool IsPropertyInherited(StylePropertyId propertyId) {
            switch (propertyId) {
                case StylePropertyId.Opacity: return true;
                case StylePropertyId.TextFontSize: return true;    
                case StylePropertyId.TextFontAsset: return true;    
                case StylePropertyId.TextFontStyle: return true;    
                case StylePropertyId.TextTransform: return true;    
            }

            return false;
        }

    }

}