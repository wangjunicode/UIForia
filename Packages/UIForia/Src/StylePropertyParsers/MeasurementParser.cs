using System;
using UIForia.Rendering;
using UIForia.Compilers;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {
    //
    // public class MinSizeParser : IStyleShorthandParser {
    //
    //
    //
    // }
    //
    // public class MaxSizeParser : IStyleShorthandParser {
    //
    //     
    //
    // }
    //
    // public class PreferredSizeParser : IStyleShorthandParser {
    //
    //    
    //
    // }

    internal class MeasurementParser : IStylePropertyParser {

        
        public bool TryParseFromStyleSheet(ref PropertyParseContext context, ref ManagedByteBuffer valueBuffer, out RangeInt valueRange) {

            valueRange = default;

            if (!context.charStream.TryParseMeasurement(out UIMeasurement measurement, true)) {
                LineInfo lineInfo = context.rawValueSpan.GetLineInfo();
                context.diagnostics.LogError(context.fileName, $"Unable to parse a measurement value from {context.charStream}", lineInfo.line, lineInfo.column);
                return false;
            }

            valueRange = valueBuffer.WriteWithRange(measurement);

            return true;
        }

    }

}