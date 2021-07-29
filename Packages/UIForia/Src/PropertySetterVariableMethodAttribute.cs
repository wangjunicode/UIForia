using System;

namespace UIForia {

    internal class PropertySetterVariableMethodAttribute : Attribute {

        public ushort propertyIndex;

        public PropertySetterVariableMethodAttribute(ushort propertyIndex) {
            this.propertyIndex = propertyIndex;
        }

    }

}