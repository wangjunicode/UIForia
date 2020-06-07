namespace UIForia.Layout {

    public struct BlockSize {

        public float outerSize;
        public float insetSize;

        public BlockSize(float outerSize, float insetSize) {
            this.outerSize = outerSize;
            this.insetSize = insetSize;
        }

        public static bool operator ==(in BlockSize a, in BlockSize b) {
            return (int) a.outerSize == (int) b.outerSize && (int) a.insetSize == (int) b.insetSize;
        }

        public static bool operator !=(BlockSize a, BlockSize b) {
            return (int) a.outerSize != (int) b.outerSize || (int) a.insetSize != (int) b.insetSize;
        }

        public bool Equals(BlockSize other) {
            return outerSize.Equals(other.outerSize) && insetSize.Equals(other.insetSize);
        }

        public override bool Equals(object obj) {
            return obj is BlockSize other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (outerSize.GetHashCode() * 397) ^ insetSize.GetHashCode();
            }
        }

    }

}