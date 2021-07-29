using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UIForia.Style {

    // [AssertSize(16)]
    [DebuggerDisplay("IndexInSource = {sortKey.indexInSource}")]
    [StructLayout(LayoutKind.Sequential)]
    internal struct BlockUsage {

        public BlockUsageSortKey sortKey;
        public int propertyStart;
        public int transitionStart;
        public ushort propertyCount;
        public ushort transitionCount;

    }

}