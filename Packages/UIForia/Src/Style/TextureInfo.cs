using System;

namespace UIForia.Style {

    public struct TextureInfo : IEquatable<TextureInfo> {

        // public Texture texture;
        public int textureId;
        public ushort uvTop;
        public ushort uvRight;
        public ushort uvBottom;
        public ushort uvLeft;

        public bool Equals(TextureInfo other) {
            return textureId == other.textureId && uvTop == other.uvTop && uvRight == other.uvRight && uvBottom == other.uvBottom && uvLeft == other.uvLeft;
        }

        public override bool Equals(object obj) {
            return obj is TextureInfo other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = textureId;
                hashCode = (hashCode * 397) ^ uvTop.GetHashCode();
                hashCode = (hashCode * 397) ^ uvRight.GetHashCode();
                hashCode = (hashCode * 397) ^ uvBottom.GetHashCode();
                hashCode = (hashCode * 397) ^ uvLeft.GetHashCode();
                return hashCode;
            }
        }

    }

}