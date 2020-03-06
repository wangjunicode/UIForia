using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UIForia.Style {

    [DebuggerDisplay("{PropertyParsers.s_PropertyNames[index]}")]
    [StructLayout(LayoutKind.Explicit)]
    public partial struct PropertyId {

        [FieldOffset(0)] public readonly int id;
        [FieldOffset(0)] public readonly ushort index;
        [FieldOffset(2)] public readonly PropertyFlags flags;

        public PropertyId(ushort id, PropertyFlags flags) {
            this.id = 0;
            this.index = id;
            this.flags = flags;
        }

        private PropertyId(int id) {
            this.index = 0;
            this.flags = 0;
            this.id = id;
        }

        public static implicit operator int(PropertyId property) {
            return property.id;
        }

        public static implicit operator PropertyId(int id) {
            return new PropertyId(id);
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