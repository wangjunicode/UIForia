using UIForia.Util;

namespace UIForia.Layout {

    public struct ClipGroup {

        public FastLayoutBox root;
        public FastLayoutBox lastFrameRoot;
        public LightList<FastLayoutBox> members;
        public LightList<FastLayoutBox> lastFrameMembers;

    }

}