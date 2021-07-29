using UIForia.Compilers;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class AspectRatioParser : IStylePropertyParser {

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            CharStream stream = context.charStream;

            AspectRatio aspectRatio;
            // height-ctrl-width 1:1
            if (stream.TryParseEnum<AspectRatioMode>(out int val)) {
                aspectRatio.mode = (AspectRatioMode) val;

                if (!stream.TryParseUInt(out uint widthRatio)) {
                    goto Fail;
                }

                if (!stream.TryParseCharacter(':')) {
                    goto Fail;
                }

                if (!stream.TryParseUInt(out uint heightRatio)) {
                    goto Fail;
                }

                aspectRatio.width = (ushort) widthRatio;
                aspectRatio.height = (ushort) heightRatio;

                valueRange = valueBuffer.WriteWithRange(aspectRatio);
                return true;

            }

            Fail:
            valueRange = default;
            context.diagnostics.LogError($"Unable to parse {stream} as a {typeof(AspectRatio).GetTypeName()}");
            return false;
        }

    }

}