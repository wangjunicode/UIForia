using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Style;

namespace UIForia {

    [AssertSize(16)]
    public unsafe struct InstanceStyleChangeSet {

        public int styleDataId;
        public int propertyCount;
        public StyleProperty2* properties;

    }

    public unsafe struct SharedStyleChangeSetDebugView {

        public StyleState2 newState;
        public StyleState2 originalState;

        public StyleId[] oldStyles;
        public StyleId[] newStyles;

        public SharedStyleChangeSetDebugView(SharedStyleChangeSet target) {
            this.newState = (StyleState2)target.newState;
            this.originalState = (StyleState2)target.originalState;
            this.newStyles = new StyleId[target.newStyleCount];
            for (int i = 0; i < target.newStyleCount; i++) {
                newStyles[i] = target.newStyles[i];
            }

            this.oldStyles = new StyleId[target.oldStyleCount];
            for (int i = 0; i < target.oldStyleCount; i++) {
                oldStyles[i] = target.oldStyles[i];
            }
        }

    }

    [AssertSize(128)]
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerTypeProxy(typeof(SharedStyleChangeSetDebugView))]
    public unsafe struct SharedStyleChangeSet {

        // the first {oldStyleCount} elements in *styles are old, the rest are new
        public int styleDataId; // can be ushort probably
        public int propertyCount;
        public StyleProperty2* properties;
        public StyleState2Byte newState;
        public StyleState2Byte originalState; 
        public byte oldStyleCount; 
        public byte newStyleCount; 
        private fixed long styles[StyleSet.k_MaxSharedStyles * 2]; // 7 old styles, 7 new ones

        public StyleId* oldStyles {
            get {
                fixed (long* p = styles) {
                    return (StyleId*) p;
                }
            }
        }

        public StyleId* newStyles {
            get {
                fixed (long* p = styles) {
                    return (StyleId*) (p + oldStyleCount);
                }
            }
        }

    }

}