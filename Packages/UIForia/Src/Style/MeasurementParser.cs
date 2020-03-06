using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Style {

    public class MinSizeParser : IStyleShorthandParser {

        public bool TryParse(CharStream stream, StructList<StyleProperty2> output) {
            return MeasurementParser.TryParseSize(stream, output, PropertyId.MinWidth, PropertyId.MinHeight);
        }

    }

    public class MaxSizeParser : IStyleShorthandParser {

        public bool TryParse(CharStream stream, StructList<StyleProperty2> output) {
            return MeasurementParser.TryParseSize(stream, output, PropertyId.MaxWidth, PropertyId.MaxHeight);
        }

    }

    public class PreferredSizeParser : IStyleShorthandParser {

        public bool TryParse(CharStream stream, StructList<StyleProperty2> output) {
            return MeasurementParser.TryParseSize(stream, output, PropertyId.PreferredWidth, PropertyId.PreferredHeight);
        }

    }

    public class MeasurementParser : IStylePropertyParser {

        public static bool TryParseSize(CharStream stream, StructList<StyleProperty2> output, PropertyId width, PropertyId height) {
            UIMeasurement first = default;
            UIMeasurement second = default;

            if (!Parse(ref stream, out first)) {
                return false;
            }

            stream.ConsumeWhiteSpaceAndComments();

            if (!stream.HasMoreTokens) {
                output.Add(StyleProperty2.FromValue(width, first));
                output.Add(StyleProperty2.FromValue(height, first));
                return true;
            }

            // optional comma support
            stream.TryParseCharacter(',');
            
            if (!Parse(ref stream, out second)) {
                return false;
            }

            // there might be remaining tokens here. we just ignore them since checking has no real benefit and hurts performance

            output.Add(StyleProperty2.FromValue(width, first));
            output.Add(StyleProperty2.FromValue(height, second));

            return true;
        }


        public static bool Parse(ref CharStream stream, out UIMeasurement measurement) {
            if (!stream.TryParseFloat(out float value)) {
                measurement = default;
                return false;
            }

            stream.ConsumeWhiteSpaceAndComments();

            if (!stream.HasMoreTokens || stream.TryMatchRangeIgnoreCase("px")) {
                measurement = new UIMeasurement(value);
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("%")) {
                measurement = new UIMeasurement(value * 0.01f, UIMeasurementUnit.Percentage);
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("pca")) {
                measurement = new UIMeasurement(value, UIMeasurementUnit.ParentContentArea);
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("psz")) {
                measurement = new UIMeasurement(value, UIMeasurementUnit.BlockSize);
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("em")) {
                measurement = new UIMeasurement(value, UIMeasurementUnit.Em);
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("cnt")) {
                measurement = new UIMeasurement(value, UIMeasurementUnit.Content);
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("vw")) {
                measurement = new UIMeasurement(value, UIMeasurementUnit.ViewportWidth);
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("vh")) {
                measurement = new UIMeasurement(value, UIMeasurementUnit.ViewportHeight);
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("auto")) {
                measurement = new UIMeasurement(value, UIMeasurementUnit.Auto);
                return true;
            }

            measurement = default;
            return false;
        }

        public bool TryParse(CharStream stream, PropertyId propertyId, out StyleProperty2 property) {
            if (Parse(ref stream, out UIMeasurement measurement)) {
                property = StyleProperty2.FromValue(propertyId, measurement);
                return true;
            }

            property = default;
            return false;
        }

    }

}