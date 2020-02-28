using UIForia.Rendering;
using UIForia.Selectors;

namespace UIForia {

    public struct StyleGroup {

        public int id;

        public StylePropertyBlock normal;
        public StylePropertyBlock hover;
        public StylePropertyBlock active;
        public StylePropertyBlock focus;

        public Selector[] selectors;
        
        public StyleGroup(int id) {
            this.id = id;
            this.normal = new StylePropertyBlock(s_EmptyPropertyArray, s_EmptyRunCommands);
            this.hover = new StylePropertyBlock(s_EmptyPropertyArray, s_EmptyRunCommands);
            this.active = new StylePropertyBlock(s_EmptyPropertyArray, s_EmptyRunCommands);
            this.focus = new StylePropertyBlock(s_EmptyPropertyArray, s_EmptyRunCommands);
            this.selectors = null;
        }


        private static readonly StyleProperty[] s_EmptyPropertyArray = {};
        private static readonly RunCommand[] s_EmptyRunCommands = {};

    }

}