using UIForia.Extensions;
using UIForia.Compilers;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class UIAngleParser : IStylePropertyParser {

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            CharStream stream = context.charStream;

            if (!stream.TryParseAngle(out UIAngle angle, true)) {
                valueRange = default;
                context.diagnostics.LogError($"Unable to parse {stream} as a {typeof(UIAngle).GetTypeName()}");
                return false;
            }

            valueRange = valueBuffer.WriteWithRange(angle);

            return true;
        }

    }

}