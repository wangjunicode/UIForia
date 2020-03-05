using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Style {

    public class MeasurementParser : IStylePropertyParser {

        public bool TryParse(CharStream stream, PropertyId propertyId, out StyleProperty2 property) {
            
            if (!stream.TryParseFloat(out float value)) {
                property = default;
                return false;
            }

            stream.ConsumeWhiteSpace();
            
            if (!stream.HasMoreTokens || stream.TryMatchRangeIgnoreCase("px")) {
                property = StyleProperty2.FromValue(propertyId, new UIMeasurement(value));
                return true;
            }
            
            if (stream.TryMatchRangeIgnoreCase("%")) {
                property = StyleProperty2.FromValue(propertyId, new UIMeasurement(value * 0.01f, UIMeasurementUnit.Percentage));
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("pca")) {
                property = StyleProperty2.FromValue(propertyId, new UIMeasurement(value, UIMeasurementUnit.ParentContentArea));
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("psz")) {
                property = StyleProperty2.FromValue(propertyId, new UIMeasurement(value, UIMeasurementUnit.BlockSize));
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("em")) {
                property = StyleProperty2.FromValue(propertyId, new UIMeasurement(value, UIMeasurementUnit.Em));
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("cnt")) {
                property = StyleProperty2.FromValue(propertyId, new UIMeasurement(value, UIMeasurementUnit.Content));
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("vw")) {
                property = StyleProperty2.FromValue(propertyId, new UIMeasurement(value, UIMeasurementUnit.ViewportWidth));
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("vh")) {
                property = StyleProperty2.FromValue(propertyId, new UIMeasurement(value, UIMeasurementUnit.ViewportHeight));
                return true;
            }

            if (stream.TryMatchRangeIgnoreCase("auto")) {
                property = StyleProperty2.FromValue(propertyId, new UIMeasurement(value, UIMeasurementUnit.Auto));
                return true;
            }

            property = default;
            return false;
        }

    }

}