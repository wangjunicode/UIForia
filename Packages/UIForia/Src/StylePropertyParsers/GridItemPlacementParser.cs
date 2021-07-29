using UIForia.Extensions;
using UIForia.Compilers;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class GridItemPlacementParser : IStylePropertyParser {

        // GridItemX = 1 span 2;
        // GridItemX = span 2;
        // GridItemX = 1;
        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {

            CharStream stream = context.charStream;
            stream.ConsumeWhiteSpace();

            if (stream.TryMatchRange("span")) {
                
                stream.ConsumeWhiteSpace();

                if (!stream.TryParseUInt(out uint spanCount)) {
                    valueRange = default;
                    context.diagnostics.LogError($"Unable to parse {stream} as a {typeof(GridItemPlacement).GetTypeName()}");
                    return false;
                }

                valueRange = valueBuffer.WriteWithRange(new GridItemPlacement() {
                    place = -1, span = (ushort) spanCount
                });

                return true;

            }

            if (stream.TryParseInt(out int placement)) {

                stream.ConsumeWhiteSpace();

                if (stream.TryMatchRange("span")) {
                    
                    stream.ConsumeWhiteSpace();

                    if (!stream.TryParseUInt(out uint spanCount)) {
                        valueRange = default;
                        context.diagnostics.LogError($"Unable to parse {stream} as a {typeof(GridItemPlacement).GetTypeName()}");
                        return false;
                    }

                    valueRange = valueBuffer.WriteWithRange(new GridItemPlacement() {
                        place = (short) placement, span = (ushort) spanCount
                    });

                    return true;

                }

                valueRange = valueBuffer.WriteWithRange(new GridItemPlacement() {
                    place = (short) placement, span = (ushort) 1
                });

                return true;

            }

            valueRange = default;
            context.diagnostics.LogError($"Unable to parse {stream} as a {typeof(GridItemPlacement).GetTypeName()}");
            return false;

        }

    }

}