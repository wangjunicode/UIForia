using System.Diagnostics;

namespace UIForia.Style {

    [DebuggerDisplay("{ToString()}")]
    internal struct PropertyKeyInfo {

        public ushort index;
        public ushort variableNameId;
        
        public PropertyKeyInfo(PropertyId propertyId, ushort variableNameId = 0) {
            this.index = (ushort) propertyId;
            this.variableNameId = variableNameId;
        }

        public override string ToString() {
            
            string str = ((PropertyId) (index)).ToString();
            
            if (variableNameId != ushort.MaxValue) {
                str += "var = " + variableNameId;
            }

            return str;
        }

    }

}