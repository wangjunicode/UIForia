using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UIForia.Style {
 

    [Serializable]
    [DebuggerDisplay("{PropertyParsers.s_PropertyNames[index]}")]
    [StructLayout(LayoutKind.Explicit)]
    public partial struct PropertyId {

        [FieldOffset(0)] public readonly int id;
        [FieldOffset(0)] public readonly ushort index;
        [FieldOffset(2)] public readonly PropertyTypeFlags typeFlags;
        [FieldOffset(3)] internal StyleState2Byte state;

        public PropertyId(ushort id, PropertyTypeFlags typeFlags) {
            this.id = 0;
            this.state = 0;
            this.index = id;
            this.typeFlags = typeFlags;
        }

        private PropertyId(int id) {
            this.index = 0;
            this.typeFlags = 0;
            this.state = 0;
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
            if (index < PropertyParsers.s_PropertyNames.Length) {
                return PropertyParsers.s_PropertyNames[index];
            }
            else {
                return "Invalid";
            }
        }

    }

}