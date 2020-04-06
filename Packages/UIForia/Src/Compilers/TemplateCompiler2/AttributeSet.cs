using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public struct AttributeSet {

        public int depth;
        public ReadOnlySizedArray<ReadOnlySizedArray<AttributeDefinition>> attributes;

        public int size;

    }

}