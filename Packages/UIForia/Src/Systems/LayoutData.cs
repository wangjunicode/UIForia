using UIForia.Elements;

namespace UIForia.Layout {

    public struct LayoutData {

        public int parentIndex;
        public int childStart;
        public int childEnd;
        public FastLayoutBox layoutBox;
        public UIElement element;

        public int idx;

//            public LayoutRenderFlag flags;
        public int clipGroupIndex;

    }

}