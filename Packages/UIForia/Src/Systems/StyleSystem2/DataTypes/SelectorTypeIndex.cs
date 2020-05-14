using System.Runtime.InteropServices;

namespace UIForia {

    [AssertSize(4)]
    [StructLayout(LayoutKind.Sequential)]
    public struct SelectorTypeIndex {

        public ushort index;
        public bool usesIndices;
        public SelectionSource source;

    }

}