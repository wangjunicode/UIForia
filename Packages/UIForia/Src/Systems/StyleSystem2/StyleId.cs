using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UIForia {

    [Flags]
    public enum HookForState {

        Normal_Enter = 1 << 0,
        Normal_Exit = 1 << 1,
        ActiveEnter = 1 << 2,
        ActiveExit = 1 << 3,
        FocusEnter = 1 << 4,
        FocusExit = 1 << 5,
        HoverEnter = 1 << 6,
        HoverExit = 1 << 7

    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct LocalStyleId {

        [FieldOffset(0)] public readonly int id;
        [FieldOffset(0)] public readonly ushort index;
        [FieldOffset(2)] public readonly byte definedStates; // this is really only 4 bits
        [FieldOffset(3)] public readonly byte hasSelectorsPerState; // this is also really only 4 bits

        public LocalStyleId(ushort styleIndex, StyleState2 definedStates, StyleState2 hasSelectorsPerState) {
            this.id = 0;
            this.index = styleIndex;
            this.definedStates = (byte) definedStates;
            this.hasSelectorsPerState = (byte) hasSelectorsPerState;
        }

        public static implicit operator int(LocalStyleId styleId) {
            return styleId.id;
        }

        public bool Equals(LocalStyleId other) {
            return id == other.id;
        }

        public static bool operator ==(in LocalStyleId a, in LocalStyleId b) {
            return a.id == b.id;
        }

        public static bool operator !=(LocalStyleId a, LocalStyleId b) {
            return a.id != b.id;
        }

        public override bool Equals(object obj) {
            return obj is StyleId other && Equals(other);
        }

        public override int GetHashCode() {
            return id;
        }

    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct StyleId : IComparable<StyleId>, IEquatable<StyleId> {

        [FieldOffset(0)] public readonly long id;
        [FieldOffset(0)] public readonly StyleSheetId styleSheetId;
        [FieldOffset(4)] public readonly LocalStyleId localStyleId;

        // todo -- bitset for has enter/exit action per state use HookForState

        public StyleId(long id) : this() {
            this.id = id;
        }

        public StyleId(StyleSheetId styleSheetId, LocalStyleId localStyleId) : this() {
            this.styleSheetId = styleSheetId;
            this.localStyleId = localStyleId;
        }

        public StyleState2 definedStates {
            get => (StyleState2) localStyleId.definedStates;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasSelectorsInState(StyleState2 state) {
            return (localStyleId.hasSelectorsPerState & (int) state) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DefinesState(StyleState2 state) {
            return (localStyleId.definedStates & (int) state) != 0;
        }

        public int CompareTo(StyleId other) {
            if (id < other.id) {
                return -1;
            }

            return id > other.id ? 1 : 0;
        }

        public static implicit operator long(StyleId styleId) {
            return styleId.id;
        }

        public static implicit operator StyleId(long styleId) {
            return new StyleId(styleId);
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
            return (int) id ^ (int) (id >> 32);
        }

    }

}