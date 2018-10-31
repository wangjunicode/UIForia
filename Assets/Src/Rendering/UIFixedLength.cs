using System.Diagnostics;
using JetBrains.Annotations;
using Rendering;

namespace Src {

    public struct UIFixedLength {

        public readonly float value;
        public readonly UIFixedUnit unit;

        public UIFixedLength(float value, UIFixedUnit unit = UIFixedUnit.Pixel) {
            this.value = value;
            this.unit = unit;
        }

        public static bool operator ==(UIFixedLength self, UIFixedLength other) {
            if (float.IsNaN(self.value) && float.IsNaN(other.value)) {
                return self.unit == other.unit;
            }

            return self.value == other.value && self.unit == other.unit;
        }

        public static bool operator !=(UIFixedLength self, UIFixedLength other) {
            return !(self == other);
        }

        public static UIFixedLength Unset => new UIFixedLength(FloatUtil.UnsetValue);

        public bool Equals(UIFixedLength other) {
            return value.Equals(other.value) && unit == other.unit;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is UIFixedLength && Equals((UIFixedLength) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (value.GetHashCode() * 397) ^ (int) unit;
            }
        }

        [Pure]
        [DebuggerStepThrough]
        public bool IsDefined() {
            return FloatUtil.IsDefined(value);
        }
        
        public static UIFixedLength Decode(int value0, int value1) {
            return new UIFixedLength(FloatUtil.DecodeToFloat(value0), (UIFixedUnit) value1);
        }

        public static implicit operator UIFixedLength(int value) {
            return new UIFixedLength(value, UIFixedUnit.Pixel);
        }

        public static implicit operator UIFixedLength(float value) {
            return new UIFixedLength(value, UIFixedUnit.Pixel);
        }

        public static implicit operator UIFixedLength(double value) {
            return new UIFixedLength((float) value, UIFixedUnit.Pixel);
        }

        public static UIFixedLength Percent(float value) {
            return new UIFixedLength(value, UIFixedUnit.Percent);
        }

    }

}