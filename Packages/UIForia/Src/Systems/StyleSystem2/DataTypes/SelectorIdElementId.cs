using System.Runtime.InteropServices;

namespace UIForia {

    [AssertSize(8)]
    [StructLayout(LayoutKind.Explicit)]
    public struct SelectorIdElementId {

        [FieldOffset(0)] public long longVal;
        [FieldOffset(0)] public ElementId elementId;
        [FieldOffset(4)] public int selectorId;

        public SelectorIdElementId(ElementId elementId, int selectorId) {
            this.longVal = default;
            this.elementId = elementId;
            this.selectorId = selectorId;
        }

    }

}