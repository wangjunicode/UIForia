using UIForia.Util;

namespace UIForia.Style2 {

    public struct PendingConstant {

        public int sourceId;
        public bool isExported;
        public CharSpan name;
        public CharSpan alias;
        public CharSpan defaultValue;
        public PendingConstantBranch[] conditions;

    }

}