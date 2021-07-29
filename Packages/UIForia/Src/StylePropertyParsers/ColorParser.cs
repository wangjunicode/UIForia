using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {
    
    internal class UIColorParser : IStylePropertyParser {
        
        private static UIColorParser instance;

        public static UIColorParser Instance => instance ??= new UIColorParser();

        public bool TryParse(ref PropertyParseContext context, out UIColor value) {
            value = default;
            if (!context.charStream.TryParseColorProperty(out Color32 color)) {
                LineInfo lineInfo = context.rawValueSpan.GetLineInfo();
                context.diagnostics.LogError($"Unable to parse a color value from `{context.charStream}`", context.fileName, lineInfo.line, lineInfo.column);
                return false;
            }

            value = (UIColor) color;
            return true;
        }

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            valueRange = default;
            if (TryParse(ref context, out UIColor value)) {
                valueRange = valueBuffer.WriteWithRange(value);
                return true;
            }

            return false;
       
        }

    }
}