using UIForia.Layout;
using UIForia.Rendering;
using UIForia.UIInput;

namespace UIForia.Systems {

    public struct ClipInfo {

        public int clipperIndex;
        public Visibility visibility;
        public PointerEvents pointerEvents;
        public ClipBehavior clipBehavior;
        public ClipBounds clipBounds;
        public Overflow overflow;
        public OrientedBounds orientedBounds;
        public bool isMouseQueryHandler;
        public bool isCulled;

    }

}