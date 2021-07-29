using UIForia.Compilers;
using UIForia.Extensions;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

    internal class GridTemplateParser : IStylePropertyParser {

        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {
            
            valueRange = default;

            int start = valueBuffer.ptr;
            
            do {

                if (!context.charStream.TryParseGridCellSize(out GridCellDefinition cell)) {
                    context.diagnostics.LogError($"Unable to parse {context.charStream} as a {typeof(GridTemplate).GetTypeName()}");
                    return false;
                }
                
                valueBuffer.Write(cell);

            } while (context.charStream.HasMoreTokens && context.charStream.Current != ';');
            
            valueRange = new RangeInt(start, valueBuffer.ptr - start);
            return valueRange.length > 0;
        }

    }

}