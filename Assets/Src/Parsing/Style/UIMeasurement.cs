using System.Diagnostics;
using Rendering;

namespace Src {

    [DebuggerDisplay("{value} {unit}")]
    public struct UIMeasurement {

        public readonly float value;
        public readonly UIUnit unit;

        public UIMeasurement(float value, UIUnit unit) {
            this.value = value;
            this.unit = unit;
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

    }

}