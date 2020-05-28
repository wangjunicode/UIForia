using UIForia.Rendering;

namespace UIForia.Layout {

    [AssertSize(64)]
    public struct LayoutSizeInfo {

        // compressible to 3 floats & 3 bytes if needed
        public UIMeasurement minSize;
        public UIMeasurement maxSize;
        public UIMeasurement prefSize;

        public float paddingBorderStart;
        public float paddingBorderEnd;

        public float marginStart;
        public float marginEnd;
        public float emSize;
        public float finalSize;

        public BlockSize parentBlockSize;
        public ContentCacheInfo contentCache;
        public LayoutFit fit;

    }

}