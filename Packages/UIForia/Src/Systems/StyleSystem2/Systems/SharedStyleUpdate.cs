using System.Runtime.InteropServices;

namespace UIForia {

    [AssertSize(16)]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SharedStyleUpdate {

        public StyleState2 originalState;
        public StyleState2 updatedState;
        public ElementId elementId;
        public int updatedStyleCount;
        public int originalStyleCount;
        public StyleId* styles;

        public StyleId* originalStyles {
            get => styles;
        }

        public StyleId* updatedStyles {
            get => styles + updatedStyleCount;
        }
    }

}