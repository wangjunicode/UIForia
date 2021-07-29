using UIForia.Compilers;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class UShortParser : IStylePropertyParser {

        private static UShortParser instance;

        public static UShortParser Instance => instance ??= new UShortParser();

        public bool TryParse(ref PropertyParseContext context, out ushort value) {
            value = default;
            if (!context.charStream.TryParseUInt(out uint fvalue)) {
                context.diagnostics.LogError($"Unable to parse a ushort value from {context.charStream}");
                return false;
            }

            value = (ushort) fvalue;
            return true;
        }

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            valueRange = default;

            if (TryParse(ref context, out ushort value)) {
                valueRange = valueBuffer.WriteWithRange(value);
                return true;
            }

            return false;

        }

    }

}