using System;

namespace UIForia {

    internal class PropertySetterMethodAttribute : Attribute {

        public ushort propertyIndex;

        public PropertySetterMethodAttribute(ushort propertyIndex) {
            this.propertyIndex = propertyIndex;
        }

    }

}