using UIForia.Style;

namespace UIForia {

    public struct StaticPropertyId {

        public PropertyId propertyId;
        public ModuleCondition conditionRequirement;

        public StaticPropertyId(PropertyId propertyId, ModuleCondition conditionRequirement = 0) {
            this.propertyId = propertyId;
            this.conditionRequirement = conditionRequirement;
        }

    }

}