using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UIForia {

    [AssertSize(4)]
    [StructLayout(LayoutKind.Explicit)]
    public struct SelectorId {

        [FieldOffset(0)] public readonly int id;
        [FieldOffset(0)] public readonly ushort index; // index into style sheet's selector definition list 
        [FieldOffset(2)] public readonly ushort flags; // encode state & has enter/exit hooks here, have extra space for sure, only need 6 bits

        public SelectorId(ushort index, StyleState2 state, StyleEventFlags flags) {
            this.id = 0;
            this.index = index;
            this.flags = (ushort) ((ushort) state | (ushort) flags);
        }

        public StyleState2 state {
            get => (StyleState2) (flags & ~((ushort) StyleEventFlags.Mask));
        }

        public bool hasEnterEvents {
            get => (flags & (ushort) StyleEventFlags.HasEnterEvents) != 0;
        }

        public bool hasExitEvents {
            get => (flags & (ushort) StyleEventFlags.HasExitEvents) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(SelectorId styleId) {
            return styleId.id;
        }

    }

}