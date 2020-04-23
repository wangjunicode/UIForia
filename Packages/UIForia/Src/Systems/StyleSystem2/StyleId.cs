using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UIForia {

    [AssertSize(4)]
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerTypeProxy(typeof(StyleIdDebugProxy))]
    [DebuggerDisplay("{GetDebuggerDisplay()}")]
    public readonly struct StyleId {

        [FieldOffset(0)] public readonly int id;
        [FieldOffset(0)] public readonly ushort index;
        [FieldOffset(2)] public readonly byte stateFlags; // this is really only 4 bits
        [FieldOffset(3)] public readonly byte hasSelectorsPerState; // this is also really only 4 bits

        public StyleId(int id) {
            this.index = default;
            this.stateFlags = default;
            this.hasSelectorsPerState = default;
            this.id = id;
        }
        
        public StyleId(ushort styleIndex, StyleState2 definedStates, StyleState2 hasSelectorsPerState) {
            this.id = 0;
            this.index = styleIndex;
            this.stateFlags = (byte) definedStates;
            this.hasSelectorsPerState = (byte) hasSelectorsPerState;
        }

        public static implicit operator int(StyleId styleId) {
            return styleId.id;
        }

        public static implicit operator StyleId(int styleId) {
            return new StyleId(styleId);
        }
        
        public StyleState2 definedStates {
            get => (StyleState2) stateFlags;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasSelectorsInState(StyleState2 state) {
            return (hasSelectorsPerState & (int) state) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DefinesState(StyleState2 state) {
            return (stateFlags & (int) state) != 0;
        }

        public int CompareTo(StyleId other) {
            if (id < other.id) {
                return -1;
            }

            return id > other.id ? 1 : 0;
        }

        public bool Equals(StyleId other) {
            return id == other.id;
        }


        public static bool operator ==(in StyleId a, in StyleId b) {
            return a.id == b.id;
        }

        public static bool operator !=(StyleId a, StyleId b) {
            return a.id != b.id;
        }

        public override bool Equals(object obj) {
            return obj is StyleId other && Equals(other);
        }

        public override int GetHashCode() {
            return id;
        }

    }

    [DebuggerTypeProxy(typeof(StyleId))]
    [DebuggerDisplay("{" + nameof(sheetName) + "}/{" + nameof(styleName) + "}")]
    public struct StyleIdDebugProxy {

        public string styleName;
        public string sheetName;

        public StyleIdDebugProxy(StyleId styleId) {
            VertigoStyleSheet sheet = VertigoStyleSystem.GetSheetForStyle(styleId.id);
            this.styleName = sheet.styleNames[styleId.index];
            this.sheetName = sheet.name;
        }

    }

    // [AssertSize(4)]
    // [StructLayout(LayoutKind.Explicit)]
    // [DebuggerTypeProxy(typeof(StyleIdDebugProxy))]
    // [DebuggerDisplay("{GetDebuggerDisplay()}")]
    // public readonly struct StyleId : IComparable<StyleId>, IEquatable<StyleId> {
    //
    //     [FieldOffset(0)] public readonly int id;
    //     [FieldOffset(0)] public readonly LocalStyleId localStyleId;
    //     
    //     private string GetDebuggerDisplay() {
    //         VertigoStyleSheet styleSheet = VertigoStyleSystem.GetSheetForStyle(id);
    //         return styleSheet.styleNames[localStyleId.index] + "/" +  styleSheet.name;
    //     }
    //
    //     public StyleId(LocalStyleId localStyleId) : this() {
    //         this.localStyleId = localStyleId;
    //     }
    //
    //     public StyleState2 definedStates {
    //         get => (StyleState2) localStyleId.definedStates;
    //     }
    //
    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public bool HasSelectorsInState(StyleState2 state) {
    //         return (localStyleId.hasSelectorsPerState & (int) state) != 0;
    //     }
    //
    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public bool DefinesState(StyleState2 state) {
    //         return (localStyleId.definedStates & (int) state) != 0;
    //     }
    //
    //     public int CompareTo(StyleId other) {
    //         if (id < other.id) {
    //             return -1;
    //         }
    //
    //         return id > other.id ? 1 : 0;
    //     }
    //
    //     public static implicit operator int(StyleId styleId) {
    //         return styleId.id;
    //     }
    //
    //     public static implicit operator StyleId(int styleId) {
    //         return new StyleId((LocalStyleId)styleId);
    //     }
    //
    //     public bool Equals(StyleId other) {
    //         return id == other.id;
    //     }
    //
    //     public static bool operator ==(in StyleId a, in StyleId b) {
    //         return a.id == b.id;
    //     }
    //
    //     public static bool operator !=(StyleId a, StyleId b) {
    //         return a.id != b.id;
    //     }
    //
    //     public override bool Equals(object obj) {
    //         return obj is StyleId other && Equals(other);
    //     }
    //
    //     public override int GetHashCode() {
    //         return (int) id ^ (int) (id >> 32);
    //     }
    //
    // }

}