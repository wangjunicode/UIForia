using System.Runtime.InteropServices;

namespace UIForia {

    [AssertSize(8)]
    [StructLayout(LayoutKind.Explicit)]
    public struct StyleStatePair {

        [FieldOffset(0)] public readonly long val;
        [FieldOffset(0)] public readonly StyleId styleId;
        [FieldOffset(4)] public readonly StyleState2 state;

        public StyleStatePair(StyleId styleId, StyleState2 state) {
            this = default;
            this.styleId = styleId;
            this.state = state;
        }

    }

}