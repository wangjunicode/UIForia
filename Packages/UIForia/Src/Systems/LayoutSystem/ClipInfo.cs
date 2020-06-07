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

        // somebody needs to know if this shape is culled or not
        // input culling != render culling
        // except when it does
        // input culling ->
        // is the point inside my input box?
        // what is the input box? 
        // what happens with transforms + border | padding | content insets? edge case to ignore?
        // different box definition? can just inset from aligned corners then test that box
        // compute sdf value might give good results, unsure about transform

        // renderer wants oriented bounds, probably always
        // wants to check that against its frustrum also

        // render culling ->
        // dont draw me if this box isn't at least partly within the clipper
        // does it matter if its the border space or not? doubtful
        // wants to cull against a camera frustrum, not just the 2d screen probably

    }

}