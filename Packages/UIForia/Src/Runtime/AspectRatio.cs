using System;
using UIForia.Layout;

namespace UIForia {

    public struct AspectRatio : IEquatable<AspectRatio> {

        public AspectRatioMode mode;
        public ushort width;
        public ushort height;

        public AspectRatio(AspectRatioMode mode, ushort width, ushort height) {
            this.mode = mode;
            this.width = width;
            this.height = height;
        }

        public float Ratio => (float) width / (float) height;

        public bool Equals(AspectRatio other) {
            return mode == other.mode && width == other.width && height == other.height;
        }

        public override bool Equals(object obj) {
            return obj is AspectRatio other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (int) mode;
                hashCode = (hashCode * 397) ^ width.GetHashCode();
                hashCode = (hashCode * 397) ^ height.GetHashCode();
                return hashCode;
            }
        }

    }

}