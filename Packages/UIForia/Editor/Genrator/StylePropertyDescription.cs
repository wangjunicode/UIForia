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
        public readonly PropertyTypeFlags typeFlags;
        public readonly string defaultValue;

        public PropertyId propertyId;

        public StylePropertyDescription(string name, Type type, string defaultValue, PropertyTypeFlags typeFlags = 0) {
            this.name = name;
            this.type = type;
            this.typeFlags = typeFlags;
            this.loweredName = name.ToLower();
            this.defaultValue = defaultValue;
            this.propertyId = default;
            if (type.IsClass) {
                typeFlags |= PropertyTypeFlags.RequireDestruction;
            }
        }

    }

}