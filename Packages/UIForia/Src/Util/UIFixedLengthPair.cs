using UnityEngine;

namespace UIForia.Util {

    public struct UIFixedLengthPair {

        public bool Equals(UIFixedLengthPair other) {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is UIFixedLengthPair && Equals((UIFixedLengthPair) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (x.GetHashCode() * 397) ^ y.GetHashCode();
            }
        }

        public readonly UIFixedLength x;
        public readonly UIFixedLength y;

        public UIFixedLengthPair(UIFixedLength x, UIFixedLength y) {
            this.x = x;
            this.y = y;
        }

        public static implicit operator UIFixedLengthPair(Vector2 vec) {
            return new UIFixedLengthPair(vec.x, vec.y);
        }

        public static bool operator ==(UIFixedLengthPair self, UIFixedLengthPair other) {
            return self.x == other.x && self.y == other.y;
        }

        public static bool operator !=(UIFixedLengthPair self, UIFixedLengthPair other) {
            return !(self == other);
        }

        public bool IsDefined() {
            return x.IsDefined() && y.IsDefined();
        }

    }
}