using UIForia.Util;

namespace UIForia.Style2 {

    public struct PendingConstant {

        public int sourceId;
        public bool isExported;
        public bool isLocal;
        public CharSpan name;
        public CharSpan defaultValue;
        public CharSpan resolvedValue;
        public Range16 partRange;

        public bool HasDefinition => defaultValue.HasValue;

    }

}
