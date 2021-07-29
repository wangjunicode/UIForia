
namespace UIForia.Style {

    internal struct StyleDesc {

        public StyleId styleId;

        public int nameId;
        public int selectorCount;
        public QueryLocationInfo queryLocationInfo;

        public int rootBlockId => styleId.blockOffset;
        public int blockCount => styleId.blockCount;

    }

}