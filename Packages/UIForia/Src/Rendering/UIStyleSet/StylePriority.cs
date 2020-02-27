using System.Runtime.InteropServices;
using UIForia.Rendering;

namespace UIForia {

    [StructLayout(LayoutKind.Explicit)]
    public struct StylePriority {

        [FieldOffset(0)]
        public StyleState state;
        
        [FieldOffset(1)]
        public SourceType source;

        [FieldOffset(2)] public ushort score;

        [FieldOffset(0)]
        public int value;

        public StylePriority(SourceType source, StyleState state, ushort score = 0) {
            this.value = 0;
            this.source = source;
            this.state = state;
            this.score = score;
        }

    }

}