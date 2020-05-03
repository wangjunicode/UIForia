using System.Runtime.InteropServices;

namespace UIForia {

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StyleSetData {

        public ushort changeSetId; // kill this?
        public StyleState2Byte state;
        public byte padding;
        private ListHandle sharedStyles;

        public ListHandle<StyleId> styleIds {
            get => new ListHandle<StyleId>(sharedStyles);
            set => sharedStyles = value.ToUntypedHandle();
        }

    }

}