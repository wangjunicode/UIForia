using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Style {

    internal class HalfParser : IStylePropertyParser {

        private static HalfParser instance;

        public static HalfParser Instance {
            get {
                if (instance == null) instance = new HalfParser();
                return instance;
            }
        }

        public bool TryParse(ref PropertyParseContext context, out half value) {
            value = default;
            if (!context.charStream.TryParseFloat(out float fvalue)) {
                context.diagnostics.LogError($"Unable to parse a half value from {context.charStream}");
                return false;
            }

            value = (half) fvalue;
            return true;
        }

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            valueRange = default;

            if (TryParse(ref context, out half value)) {
                valueRange = valueBuffer.WriteWithRange(value);
                return true;
            }

            return false;
        }

    }

}