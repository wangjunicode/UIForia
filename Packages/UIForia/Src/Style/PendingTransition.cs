using System;

namespace UIForia.Style {

    internal struct PendingTransition : IComparable<PendingTransition> {

        public ElementId elementId;
        public int transitionId;

        public int CompareTo(PendingTransition other) {
            int a = elementId.id & ElementId.k_IndexMask;
            int b = other.elementId.id & ElementId.k_IndexMask;
            return a - b;
        }

    }

}