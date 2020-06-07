using System.Collections.Generic;

namespace UIForia.Layout {

    public struct ElementFTBHierarchySort : IComparer<ElementId> {

        private ElementTable<ElementTraversalInfo> traversalTable;
        
        public ElementFTBHierarchySort(ElementTable<ElementTraversalInfo> traversalTable) {
            this.traversalTable = traversalTable;
        }

        public int Compare(ElementId x, ElementId y) {
            return traversalTable[x].ftbIndex - traversalTable[y].ftbIndex;
        }

    }

}