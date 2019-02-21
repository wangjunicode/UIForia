using System.Diagnostics;
using JetBrains.Annotations;
using UIForia.Rendering;

namespace UIForia {

    public struct TransformOffset {

        public float value;
        public TransformUnit unit;

        public TransformOffset(float value, TransformUnit unit = TransformUnit.Pixel) {
            this.value = value;
            this.unit = unit;
        }

        public static TransformOffset Unset => new TransformOffset(FloatUtil.UnsetValue, 0);

        [Pure]
        [DebuggerStepThrough]
        public bool IsDefined() {
            return FloatUtil.IsDefined(value);
        }
        
        public bool Equals(TransformOffset other) {
            return value.Equals(other.value) && unit == other.unit;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TransformOffset a && Equals(a);
        }

        public override int GetHashCode() {
            unchecked {
                return (value.GetHashCode() * 397) ^ (int) unit;
            }
        }
        
        public static bool operator ==(TransformOffset self, TransformOffset other) {
            if (float.IsNaN(self.value) && float.IsNaN(other.value)) {
                return self.unit == other.unit;
            }

            return self.value == other.value && self.unit == other.unit;
        }

        public static bool operator !=(TransformOffset self, TransformOffset other) {
            return !(self == other);
        }

        public static implicit operator TransformOffset(int value) {            
            return new TransformOffset(value, TransformUnit.Pixel);
        }

        public static implicit operator TransformOffset(float value) {
            return new TransformOffset(value, TransformUnit.Pixel);
        }

        public static implicit operator TransformOffset(double value) {
            return new TransformOffset((float) value, TransformUnit.Pixel);
        }

        public override string ToString() {
            return $"{value} {unit}";
        }
    }

    public struct TransformOffsetPair {

        public TransformOffset x;
        public TransformOffset y;

        public TransformOffsetPair(TransformOffset x, TransformOffset y) {
            this.x = x;
            this.y = y;
        }
        
        public bool IsDefined() {
            return x.IsDefined() && y.IsDefined();
        }
        
        public static bool operator ==(TransformOffsetPair self, TransformOffsetPair other) {
            return self.x == other.x && self.y == other.y;
        }

        public static bool operator !=(TransformOffsetPair self, TransformOffsetPair other) {
            return !(self == other);
        }

        public bool Equals(TransformOffsetPair other) {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TransformOffsetPair a && Equals(a);
        }

        public override int GetHashCode() {
            unchecked {
                return (x.GetHashCode() * 397) ^ y.GetHashCode();
            }
        }

    }

}