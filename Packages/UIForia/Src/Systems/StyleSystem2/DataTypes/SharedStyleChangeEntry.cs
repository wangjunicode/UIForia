using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UIForia {
    

    public unsafe struct SharedStyleChangeSetDebugView {

        public StyleState2 newState;
        public StyleState2 originalState;

        public StyleId[] oldStyles;
        public StyleId[] newStyles;

        public SharedStyleChangeSetDebugView(SharedStyleChangeEntry target) {
            this.newState = (StyleState2)target.newState;
            this.originalState = (StyleState2)target.oldState;
            this.newStyles = new StyleId[target.newStyleCount];
            for (int i = 0; i < target.newStyleCount; i++) {
                newStyles[i] = target.pNewStyles[i];
            }

            this.oldStyles = new StyleId[target.oldStyleCount];
            for (int i = 0; i < target.oldStyleCount; i++) {
                oldStyles[i] = target.pOldStyles[i];
            }
        }

    }

    [AssertSize(16)]
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerTypeProxy(typeof(SharedStyleChangeSetDebugView))]
    public unsafe struct SharedStyleChangeEntry {

        // the first {oldStyleCount} elements in *styles are old, the rest are new
        public ElementId elementId; // can be ushort probably

        public StyleState2Byte newState;
        public StyleState2Byte oldState; 
        public byte oldStyleCount; 
        public byte newStyleCount;

        public StyleId* pStyles;

        public SharedStyleChangeEntry(ElementId elementId, StyleState2Byte state) {
            this.elementId = elementId;
            this.newState = state;
            this.oldState = state;
            this.oldStyleCount = 0;
            this.newStyleCount = 0;
            this.pStyles = null;
        }

        public StyleId* pOldStyles {
            get => pStyles;
        }

        public StyleId* pNewStyles {
            get => pStyles + oldStyleCount;
        }

    }

}