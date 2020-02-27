using UIForia.Rendering;

namespace UIForia {

    public struct StyleGroup {

        public int id;

        public StyleStateGroup normal;
        public StyleStateGroup hover;
        public StyleStateGroup active;
        public StyleStateGroup focus;

        public StyleGroup(int id) {
            this.id = id;
            normal = new StyleStateGroup(null, new StylePropertyBlock(s_EmptyPropertyArray, s_EmptyRunCommands));
            hover = new StyleStateGroup(null, new StylePropertyBlock(s_EmptyPropertyArray, s_EmptyRunCommands));
            active = new StyleStateGroup(null, new StylePropertyBlock(s_EmptyPropertyArray, s_EmptyRunCommands));
            focus = new StyleStateGroup(null, new StylePropertyBlock(s_EmptyPropertyArray, s_EmptyRunCommands));
        }


        private static readonly StyleProperty[] s_EmptyPropertyArray = {};
        private static readonly RunCommand[] s_EmptyRunCommands = {};

    }

}