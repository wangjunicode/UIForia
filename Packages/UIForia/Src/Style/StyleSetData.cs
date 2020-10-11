using System.Runtime.InteropServices;
using UIForia.ListTypes;

namespace UIForia.Style {

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct StyleSetData {

        public int styleChangeIndex;
        public int stateChangeIndex;
        public int instanceChangeIndex;

        public StyleState2 state;
        public List_StyleId sharedStyles;
        public List_StyleId finalStyles;
        public int styleCount;
        public StyleId* styleIds;

    }


}