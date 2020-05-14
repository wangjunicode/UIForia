using System;

namespace UIForia {

    public struct ElementId_StyleIdPair : IComparable<ElementId_StyleIdPair> {

        public ElementId elementId;
        public StyleId styleId;

        public int CompareTo(ElementId_StyleIdPair other) {
            if (styleId == other.styleId) {
                return 0;
            }

            return styleId - other.styleId;
        }

    }

}