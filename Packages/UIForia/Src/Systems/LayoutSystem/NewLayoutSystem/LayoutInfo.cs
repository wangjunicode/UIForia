using UIForia.Rendering;

namespace UIForia.Layout {

    public struct LayoutInfo {

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
        public float bgSize;

        public bool isBlockProvider;
        public bool requiresLayout;
        public bool isContentBased;
        public bool requiresContentSizeLayout;

        public float ContentAreaSize {
            get => finalSize - (paddingBorderStart + paddingBorderEnd);
        }

    }

}