using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UIForia.Style {

    [AssertSize(4)]
    [DebuggerDisplay("{ToString()}")]
    [StructLayout(LayoutKind.Explicit)]
    public partial struct PropertyId {

        // todo -- clear up the confusion between id and index
        [FieldOffset(0)] public readonly int id;
        [FieldOffset(0)] public readonly ushort index;

        public PropertyId(ushort id) {
            this.id = 0;
            this.index = id;
        }

        private PropertyId(int id) {
            this.index = 0;
            this.id = id;
        }

        public static bool operator ==(PropertyId self, PropertyId other) {
            return self.index == other.index;
        }

        public static bool operator !=(PropertyId self, PropertyId other) {
            return self.index != other.index;
        }

        public static implicit operator int(PropertyId property) {
            return property.id;
        }

        public static implicit operator PropertyId(int id) {
            return new PropertyId(id);
        }

        public bool IsInherited => index <= k_InheritedPropertyCount;

        public bool Equals(PropertyId other) {
            return index == other.index;
        }

        public override bool Equals(object obj) {
            return obj is PropertyId other && Equals(other);
        }

        public override int GetHashCode() {
            return index;
        }

        public override string ToString() {
            if (StyleDatabase.s_DebuggedStyleDatabase != null) {
                if (index < PropertyParsers.s_PropertyNames.Length) {
                    return PropertyParsers.s_PropertyNames[index];
                }
                return StyleDatabase.s_DebuggedStyleDatabase.GetCustomPropertyName(this);
            }

            if (index < PropertyParsers.s_PropertyNames.Length) {
                return PropertyParsers.s_PropertyNames[index];
            }

            return "Custom Property " + index;
            
        }

    }

}