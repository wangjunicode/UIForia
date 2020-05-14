using System.Runtime.InteropServices;

namespace UIForia {

    [StructLayout(LayoutKind.Sequential)]
    public struct StyleSetData {

        public int styleChangeIndex; 
        public int stateChangeIndex;
        public int instanceChangeIndex;
        public StyleState2 state;
        public List_StyleId sharedStyles;

    }

}