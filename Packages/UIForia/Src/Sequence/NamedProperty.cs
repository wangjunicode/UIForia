using System.Runtime.InteropServices;

namespace UIForia {

    public struct NamedProperty {

        public string name;
        public SequenceProperty property;

        public NamedProperty(string name, int i) {
            this.name = name;
            this.property = new SequenceProperty(i);
        }

        public NamedProperty(string name, string val) {
            this.name = name;
            this.property = new SequenceProperty(val);
        }

        public NamedProperty(string name, bool val) {
            this.name = name;
            this.property = new SequenceProperty(val);
        }

        public NamedProperty(string name, float val) {
            this.name = name;
            this.property = new SequenceProperty(val);
        }

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SequenceProperty {

        [FieldOffset(0)] public int intVal;
        [FieldOffset(0)] public float floatVal;
        [FieldOffset(0)] public string strVal;
        [FieldOffset(0)] public bool boolVal;
        [FieldOffset(8)] internal SequencePropertyType propertyType;

        public SequenceProperty(string str) {
            intVal = 0;
            floatVal = 0;
            boolVal = false;
            this.strVal = str;
            this.propertyType = SequencePropertyType.String;
        }

        public SequenceProperty(int intVal) {
            this.propertyType = SequencePropertyType.Int;
            floatVal = 0;
            strVal = null;
            boolVal = false;
            this.intVal = intVal;
        }

        public SequenceProperty(float floatVal) {
            this.propertyType = SequencePropertyType.Float;
            strVal = null;
            boolVal = false;
            intVal = 0;
            this.floatVal = floatVal;
        }

        public SequenceProperty(bool boolVal) {
            this.propertyType = SequencePropertyType.Bool;
            strVal = null;
            intVal = 0;
            floatVal = 0;
            this.boolVal = boolVal;
        }

    }

    internal enum SequencePropertyType {

        Int,
        Float,
        String,
        Bool

    }

}