using System.Collections.Generic;
using UIForia.Text;

namespace UIForia.Util {

    public static class StringUtil {

        public static string ListToString(IReadOnlyList<string> list, string separator = ", ") {
            if (list == null || list.Count == 0) {
                return string.Empty;
            }
            
            string retn = null;
            TextUtil.StringBuilder.Clear();
            
            for (int i = 0; i < list.Count; i++) {
                TextUtil.StringBuilder.Append(list[i]);
                if (i != list.Count - 1 && separator != null) {
                    TextUtil.StringBuilder.Append(separator);
                }
            }

            retn = TextUtil.StringBuilder.ToString();
            TextUtil.StringBuilder.Clear();
            return retn;
        }
        
        public static string ListToString(IList<string> list, string separator = ", ") {
            if (list == null || list.Count == 0) {
                return string.Empty;
            }
            
            string retn = null;
            TextUtil.StringBuilder.Clear();
            
            for (int i = 0; i < list.Count; i++) {
                TextUtil.StringBuilder.Append(list[i]);
                if (i != list.Count - 1 && separator != null) {
                    TextUtil.StringBuilder.Append(separator);
                }
            }

            retn = TextUtil.StringBuilder.ToString();
            TextUtil.StringBuilder.Clear();
            return retn;
        }

    }

}