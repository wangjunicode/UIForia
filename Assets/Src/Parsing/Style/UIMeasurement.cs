using System.Diagnostics;
using Rendering;

namespace Src {

    [DebuggerDisplay("{unit}({value})")]
    public struct UIMeasurement {

        public readonly float value;
        public readonly UIUnit unit;

        public UIMeasurement(float value, UIUnit unit = UIUnit.Pixel) {
            this.value = value;
            this.unit = unit;
        }

        public UIMeasurement(int value, UIUnit unit = UIUnit.Pixel) {
            this.value = value;
            this.unit = unit;
        }

        public UIMeasurement(double value, UIUnit unit = UIUnit.Pixel) {
            this.value = (float) value;
            this.unit = unit;
        }

        public bool isDefined => value < UIStyle.UnsetFloatThreshold;
        
        public bool isFixed => (unit & (UIUnit.Pixel | UIUnit.Parent | UIUnit.View)) != 0;
        
        public static UIMeasurement Parent100 => new UIMeasurement(1f, UIUnit.Parent);
        public static UIMeasurement Content100 => new UIMeasurement(1f, UIUnit.Content);

        public float GetFixedPixelValue(float parentValue, float viewportValue) {
            switch (unit) {
                case UIUnit.Pixel:
                    return value;

                case UIUnit.Content:
                    return 0;

                case UIUnit.Parent:
                    return value * parentValue;

                case UIUnit.View:
                    return value * viewportValue;

                default:
                    return 0;
            }
        }

        public bool Equals(UIMeasurement other) {
            return value.Equals(other.value) && unit == other.unit;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is UIMeasurement && Equals((UIMeasurement) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (value.GetHashCode() * 397) ^ (int) unit;
            }
        }

        public static bool operator ==(UIMeasurement self, UIMeasurement other) {
            return self.value == other.value && self.unit == other.unit;
        }

        public static bool operator !=(UIMeasurement self, UIMeasurement other) {
            return !(self == other);
        }

        public static implicit operator UIMeasurement(int value) {
            return new UIMeasurement(value, UIUnit.Pixel);
        }

        public static implicit operator UIMeasurement(float value) {
            return new UIMeasurement(value, UIUnit.Pixel);
        }

        public static implicit operator UIMeasurement(double value) {
            return new UIMeasurement((float) value, UIUnit.Pixel);
        }

    }


}