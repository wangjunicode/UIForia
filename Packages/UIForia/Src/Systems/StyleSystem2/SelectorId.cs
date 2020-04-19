using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UIForia {

    [StructLayout(LayoutKind.Explicit)]
    public struct SelectorId {

        [FieldOffset(0)] public readonly long id;
        [FieldOffset(0)] public readonly ushort styleSheetId; // style sheet id without module 
        [FieldOffset(2)] public readonly ushort sourceStyleIndex;
        [FieldOffset(4)] public readonly ushort index; // index into style sheet's selector definition list 
        [FieldOffset(6)] public readonly ushort flags; // encode state & has enter/exit hooks here, have extra space for sure, only need 6 bits

        public SelectorId(ushort styleSheetId, ushort styleIndex, ushort selectorIndex, StyleState2 state, StyleEventFlags flags) {
            this.id = 0;
            this.styleSheetId = styleSheetId;
            this.sourceStyleIndex = styleIndex;
            this.index = selectorIndex;
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
        public static implicit operator long(SelectorId styleId) {
            return styleId.id;
        }

    }

}