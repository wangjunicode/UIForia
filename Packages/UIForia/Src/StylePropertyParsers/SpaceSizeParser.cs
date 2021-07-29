using UIForia.Compilers;
using UIForia.Extensions;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class SpaceSizeParser : IStylePropertyParser {

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            CharStream stream = context.charStream;

            if (!stream.TryParseSpaceSize(out UISpaceSize length, true)) {
                context.diagnostics.LogError($"Unable to parse {stream} as a {typeof(UISpaceSize).GetTypeName()}");
                valueRange = default;
                return false;
            }

            valueRange = valueBuffer.WriteWithRange(length);
            return true;

        }

    }

}