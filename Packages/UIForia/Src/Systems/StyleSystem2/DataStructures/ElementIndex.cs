using UIForia.Util.Unsafe;

namespace UIForia {

    public struct ElementIndex {

        public IntListMap<ElementId> tagIndex;
        public IntListMap<ElementId> styleIndex;
        public IntListMap<ElementId> attributeIndex;
        public IntListMap<ElementId> stateIndex;

    }

}