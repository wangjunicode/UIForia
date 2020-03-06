using System;
using UIForia.Style;

namespace UIForia.Editor {

    public class StyleShorthandDescription {

        public Type parserType;
        public readonly string name;
        public readonly string loweredName;

        public StyleShorthandDescription(string name, Type parserType) {
            this.name = name;
            this.loweredName = name.ToLower();
            this.parserType = parserType;
        }

    }

    public class StylePropertyDescription {

        public readonly string name;
        public readonly string loweredName;
        public readonly Type type;
        public readonly PropertyFlags flags;
        public readonly string defaultValue;

        public PropertyId propertyId;

        public StylePropertyDescription(string name, Type type, string defaultValue, PropertyFlags flags = 0) {
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