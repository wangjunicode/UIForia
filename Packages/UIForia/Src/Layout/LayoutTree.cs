using System.Diagnostics;

namespace UIForia.Layout {
    
    [DebuggerTypeProxy(typeof(LayoutTreeDebuggerView))]
    internal struct LayoutTree {

        public CheckedArray<LayoutNode> nodeList;
        public CheckedArray<ElementId> elementIdList; // denormalized list of element ids in LAYOUT ORDER this is not the same as hierarchy order. does not contain transcluded elements and ignored elements get pushed to the back 
        public CheckedArray<LayoutDepthLevel> depthLevels;
        public CheckedArray<int> elementIdToLayoutIndex;

        public int elementCount => nodeList.size;

    }

    internal sealed class LayoutTreeDebuggerView {

        public LayoutDepthLevel[] Items;
        public LayoutNode[] NodeList;

        public LayoutTreeDebuggerView(LayoutTree tree) {
            Items = tree.depthLevels.ToArray();
            NodeList = tree.nodeList.ToArray();
        }

    }


}