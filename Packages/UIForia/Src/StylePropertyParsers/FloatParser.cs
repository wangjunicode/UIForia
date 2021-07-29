using UIForia.Compilers;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class FloatParser : IStylePropertyParser {

        private static FloatParser instance;

        public static FloatParser Instance => instance ??= new FloatParser();

        
        public bool TryParse(ref PropertyParseContext context, out float value) {
            value = default;

            if (!context.charStream.TryParseFloat(out value)) {
                context.diagnostics.LogError($"Unable to parse a float value from {context.charStream}");
                return false;
            }

            return true;
        }

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            valueRange = default;

            if (TryParse(ref context, out float value)) {
                valueRange = valueBuffer.WriteWithRange(value);
                return true;
            }

            return false;
        }

    }

}