using UnityEngine;

namespace UIForia.NewStyleParsing {

    public struct StyleASTNode {

        public StyleNodeType nodeType;
        public RangeInt contentRange;
        public int index;
        public int parentIndex;
        public int firstChildIndex;
        public int childCount;
        public int nextSiblingIndex;

    }

}