using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Style;
using UIForia.Util;

namespace UIForia {

    public unsafe struct SharedStyleChange {

        public int styleSetId;
        public StyleState2 state;
        public ushort count;
        public fixed int sharedStyles[7];

    }

    [AssertSize(16)]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StyleRebuildResult {

        public int styleSetId;
        public int styleCount;
        public StyleProperty2* rebuiltStyles;

    }

    public struct ElementTraversalInfo {

        public int depth;
        public ushort ftbIndex;
        public ushort btfIndex;

        public bool IsDescendentOf(in ElementTraversalInfo info) {
            return ftbIndex > info.ftbIndex && btfIndex > info.btfIndex;
        }

        public bool IsAncestorOf(in ElementTraversalInfo info) {
            return ftbIndex < info.ftbIndex && btfIndex < info.btfIndex;
        }

        public bool IsParentOf(in ElementTraversalInfo info) {
            return depth == info.depth + 1 && ftbIndex < info.ftbIndex && btfIndex < info.btfIndex;
        }

    }

    public unsafe struct ElementInfo2 {

        public int id;
        public ushort childCount;
        public ushort depth;
        internal UIElementFlags flags;
        public int firstChildIndex;

    }
    

}