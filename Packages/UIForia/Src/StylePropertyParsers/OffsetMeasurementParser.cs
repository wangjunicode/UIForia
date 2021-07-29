using UIForia.Extensions;
using UIForia.Compilers;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class OffsetMeasurementParser : IStylePropertyParser {

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            CharStream stream = context.charStream;

            if (!stream.TryParseOffsetMeasurement(out UIOffset length, true)) {
                valueRange = default;
                context.diagnostics.LogError($"Unable to parse {stream} as a {typeof(UIOffset).GetTypeName()}");
                return false;
            }

            valueRange = valueBuffer.WriteWithRange(length);

            return true;
        }

    }

}