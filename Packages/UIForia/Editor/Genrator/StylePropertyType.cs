using System;
using UIForia.Style;

namespace UIForia.Editor {

    public class StylePropertyType {

        public readonly string name;
        public readonly string loweredName;
        public readonly Type type;
        public readonly PropertyFlags flags;
        public readonly string defaultValue;
        
        public PropertyId propertyId;
        
        public StylePropertyType(string name, Type type, string defaultValue, PropertyFlags flags = 0) {
            this.name = name;
            this.type = type;
            this.flags = flags;
            this.loweredName = name.ToLower();
            this.defaultValue = defaultValue;
            this.propertyId = default;
            if (type.IsClass) {
                flags |= PropertyFlags.RequireDestruction;
            }
        }

    }

}