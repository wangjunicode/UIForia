using UIForia.Style;
using UIForia.Util;

namespace UIForia.Style2 {

    public unsafe struct StyleBuildData {

        public StyleSheet2 targetSheet;
        public CharSpan targetStyleName;

        public int stateIndex;
        public StyleProperty2** states;
        public IntBoolMap* maps;

    }

}