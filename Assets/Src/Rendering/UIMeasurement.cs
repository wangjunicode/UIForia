using System.Diagnostics;
using JetBrains.Annotations;
using Rendering;

namespace Src {

//    todo -- make varieties of UIMeasurement w/ conversions for different scenarios, ie only fixed + percent but no content    

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

        [Pure]
        [DebuggerStepThrough]
        public bool IsDefined() {
            return FloatUtil.IsDefined(value);
        }

        public bool IsFixed => !IsParentBased;

        public bool IsParentBased => (unit & (UIUnit.ParentSize | UIUnit.ParentContentArea)) != 0;

        public bool IsContentBased => unit == UIUnit.Content;
        
        //public bool IsContentRelative => (unit & (UIUnit.Content | UIUnit.FitContent | UIUnit.MaxContent | UIUnit.MinContent)) != 0;

        public static UIMeasurement ContentArea => new UIMeasurement(1f, UIUnit.ParentContentArea);
        public static UIMeasurement Parent100 => new UIMeasurement(1f, UIUnit.ParentSize);
        public static UIMeasurement Content100 => new UIMeasurement(1f, UIUnit.Content);
        public static UIMeasurement Unset => new UIMeasurement(FloatUtil.UnsetValue);

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
            if (float.IsNaN(self.value) && float.IsNaN(other.value)) {
                return self.unit == other.unit;
            }

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

        public static UIMeasurement Decode(int value, int unit) {
            return new UIMeasurement(FloatUtil.DecodeToFloat(value), (UIUnit) unit);
        }

    }

}