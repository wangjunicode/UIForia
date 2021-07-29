using System;

namespace UIForia.Style {

    [AssertSize(8)]
    internal struct StyleUsage : IComparable<StyleUsage> {

        private uint databits;
        public ElementId elementId;

        private const int sz0 = 24, loc0 = 0, mask0 = ((1 << sz0) - 1) << loc0;
        private const int sz1 = 8, loc1 = loc0 + sz0, mask1 = ((1 << sz1) - 1) << loc1;

        public StyleUsage(int id, int indexInStyleList, ElementId elementId) {
            this.elementId = elementId;
            this.databits = 0;
            this.databits = (uint) (databits & ~mask0 | (uint) ((id << loc0) & mask0));
            this.databits = (uint) (databits & ~mask1 | (uint) ((indexInStyleList << loc1) & mask1));
        }

        public int indexInStyleList => (int) (databits & mask1) >> loc1;

        public int id => (int) ((databits & mask0) >> loc0);

        public int CompareTo(StyleUsage other) {
            return elementId.index - other.elementId.index;
        }

    }

}