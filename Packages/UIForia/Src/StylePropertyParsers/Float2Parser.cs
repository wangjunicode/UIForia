using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Style {

    internal class Float2Parser : IStylePropertyParser {

        private static Float2Parser instance;

        public static Float2Parser Instance => instance ??= new Float2Parser();

        public bool TryParse(ref PropertyParseContext context, out float2 value) {
            value = default;

            float x = 0;
            float y = 0;

            if (!context.charStream.TryParseFloat(out x)) {
                context.diagnostics.LogError($"Unable to parse a float value from {context.charStream}");
                return false;
            }

            if (context.charStream.HasMoreTokens) {

                if (!context.charStream.TryParseCharacter(',')) {
                    context.diagnostics.LogError($"Unable to parse a float2 value from {context.charStream}, expected a `,` between the values");
                    return false;
                }

                if (!context.charStream.TryParseFloat(out y, true)) {
                    context.diagnostics.LogError($"Unable to parse a float2 value from {context.charStream}");
                    return false;
                }

            }
            else {
                y = x;
            }

            value = new float2(x, y);

            return true;
        }

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            valueRange = default;

            if (TryParse(ref context, out float2 value)) {
                valueRange = valueBuffer.WriteWithRange(value);
                return true;
            }

            return false;

        }

    }

}