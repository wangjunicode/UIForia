using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UIForia.Style {

    [AssertSize(4)]
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerTypeProxy(typeof(StyleIdDebugProxy))]
    [DebuggerDisplay("{GetDebuggerDisplay()}")]
    public readonly struct StyleId {

        // can probably play games with number ranges
        [FieldOffset(0)] public readonly int id;
        [FieldOffset(0)] public readonly ushort index;
        [FieldOffset(2)] public readonly byte stateFlags_selectors; //4 bits each
        [FieldOffset(3)] public readonly byte dbSourceId; // could steal 4 bits from this to give to index if really needed.
        
        // i think id scheme is better if we segment the style database into groups of 10million or something then just subtract db.baseValue from the index.
        // if index is still too big then we're trying to lookup the style in the wrong database
        // even if i reserve the 8 high bits for flags thats still almost 17 million unique style ids. average app probably declares 5000 or so. so im pretty ok with that 
        
        public StyleId(int id) {
            this.index = default;
            this.stateFlags_selectors = default;
            this.dbSourceId = default;
            this.id = id;
        }
        
        public StyleId(ushort styleIndex, StyleState2 definedStates, StyleState2 hasSelectorsPerState, int dbIndex) {
            this.id = 0;
            this.index = styleIndex;
            byte states = (byte) definedStates;
            byte selectorsPerState = (byte) hasSelectorsPerState;
            this.stateFlags_selectors = (byte) (states | (selectorsPerState << 4));
            this.dbSourceId = (byte) dbIndex;
        }

        public static implicit operator int(StyleId styleId) {
            return styleId.id;
        }

        public static implicit operator StyleId(int styleId) {
            return new StyleId(styleId);
        }

        public StyleState2 definedStates {
            get => (StyleState2) (stateFlags_selectors & 0xF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasSelectorsInState(StyleState2 state) {
            int selectors = (stateFlags_selectors >> 4) & 0xF;
            return (selectors & (int) state) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DefinesState(StyleState2 state) {
            return ((stateFlags_selectors & 0xF) & (int) state) != 0;
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

        public string GetDebuggerDisplay() {
            return default; //StyleDatabase.GetDatabase(dbSourceId)?.Debug_ResolveStyleName(index) ?? "unresolved style";
        }

        public PropertyData GetProperty(PropertyId propertyId, StyleState2 state = StyleState2.Normal) {
            return default; //StyleDatabase.GetDatabase(dbSourceId)?.GetPropertyValue(index, propertyId, state) ?? default;
        }

    }

    [DebuggerDisplay("{styleName")]
    public struct StyleIdDebugProxy {

        public string styleName;

        public StyleIdDebugProxy(StyleId styleId) {
            styleName = styleId.GetDebuggerDisplay();
        }

    }

}