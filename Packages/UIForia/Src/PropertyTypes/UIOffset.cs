using System;
using System.Diagnostics;
using JetBrains.Annotations;
using UIForia.Util;

namespace UIForia {

    public struct UIOffset : IEquatable<UIOffset> {

        public float value;
        public UIOffsetUnit unit;

        public UIOffset(float value, UIOffsetUnit unit = UIOffsetUnit.Pixel) {
            this.value = value;
            this.unit = unit;
        }
        
        public bool Equals(UIOffset other) {
            return value.Equals(other.value) && unit == other.unit;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is UIOffset a && Equals(a);
        }

        public override int GetHashCode() {
            unchecked {
                return (value.GetHashCode() * 397) ^ (int) unit;
            }
        }

        public static bool operator ==(UIOffset self, UIOffset other) {
            if (float.IsNaN(self.value) && float.IsNaN(other.value)) {
                return self.unit == other.unit;
            }

            return self.value == other.value && self.unit == other.unit;
        }

        public static bool operator !=(UIOffset self, UIOffset other) {
            return !(self == other);
        }

        public static implicit operator UIOffset(int value) {
            return new UIOffset(value, UIOffsetUnit.Pixel);
        }

        public static implicit operator UIOffset(float value) {
            return new UIOffset(value, UIOffsetUnit.Pixel);
        }

        public static implicit operator UIOffset(double value) {
            return new UIOffset((float) value, UIOffsetUnit.Pixel);
        }

        public override string ToString() {
            return $"{value} {unit}";
        }

    }

}