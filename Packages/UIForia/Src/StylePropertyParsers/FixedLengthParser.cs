using UIForia.Extensions;
using UIForia.Compilers;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class FixedLengthParser : IStylePropertyParser {

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            valueRange = default;
            CharStream stream = context.charStream;

            if (!stream.TryParseFixedLength(out UIFixedLength length, true)) {
                context.diagnostics.LogError($"Unable to parse {stream} as a {typeof(UIFixedLength).GetTypeName()}");
                return false;
            }

            valueRange = valueBuffer.WriteWithRange(length);

            return true;
            
        }

    }

}