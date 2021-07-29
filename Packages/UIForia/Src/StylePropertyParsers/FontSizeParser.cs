using UIForia.Compilers;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class FontSizeParser : IStylePropertyParser {

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            valueRange = default;
            CharStream stream = context.charStream;

            if (!stream.TryParseFontSize(out UIFontSize fontSize, true)) {
                context.diagnostics.LogError($"Unable to parse {stream} as a {typeof(UIFontSize).GetTypeName()}");
                return false;
            }

            valueRange = valueBuffer.WriteWithRange(fontSize);

            return true;
            
        }

    }

}