using System;

namespace UIForia.Style {

    internal unsafe struct PropertyUpdate : IComparable<PropertyUpdate> {

        public ElementId elementId;
        public int dbValueLocation; // index into style database where value lives
        // public ushort paletteIndex; // 2 bytes free here
        public ushort variableNameId;

        public ushort paletteIndex {
            get {
                int val = -dbValueLocation;
                ushort* shortVal = (ushort*) &val;
                return shortVal[0];
            }
        }

        public ushort palettePropertyIndex {
            get {
                int val = -dbValueLocation;
                ushort* shortVal = (ushort*) &val;
                return shortVal[1];
            }
        }

        public bool IsVariable => variableNameId != ushort.MaxValue;

        public int CompareTo(PropertyUpdate other) {
            int a = elementId.id & ElementId.k_IndexMask;
            int b = other.elementId.id & ElementId.k_IndexMask;
            return a - b;
        }

    }

}