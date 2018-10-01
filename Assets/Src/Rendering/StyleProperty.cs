namespace Rendering {

    public struct StyleProperty {

        public readonly int valuePart0;
        public readonly int valuePart1;
        public readonly StylePropertyId propertyId;

        public StyleProperty(StylePropertyId propertyId, int value0, int value1 = 0) {
            this.propertyId = propertyId;
            this.valuePart0 = value0;
            this.valuePart1 = value1;
        }

        public bool IsDefined => IntUtil.IsDefined(valuePart0) && IntUtil.IsDefined(valuePart1);

    }

}