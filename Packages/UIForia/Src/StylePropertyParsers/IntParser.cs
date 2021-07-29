using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class IntParser : IStylePropertyParser {

        private static IntParser instance;

        public static IntParser Instance => instance ??= new IntParser();

        public bool TryParse(ref PropertyParseContext context, out int value) {

            if (!context.charStream.TryParseInt(out value)) {
                context.diagnostics.LogError($"Unable to parse an int value from {context.charStream}");
                return false;
            }

            return true;

        }

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            valueRange = default;

            if (TryParse(ref context, out int value)) {
                valueRange = valueBuffer.WriteWithRange(value);
                return true;
            }

            return false;

        }

    }

}