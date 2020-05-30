using UIForia.Style;

namespace UIForia {

    public struct StaticPropertyId {

        public PropertyId propertyId;
        // public ModuleCondition conditionRequirement;

        public ushort conditionDepth;
        public ushort conditionId;
        
        public StaticPropertyId(PropertyId propertyId, int conditionDepth = 0, int conditionId = 0) {
            this.propertyId = propertyId;
            this.conditionDepth = (ushort)conditionDepth;
            this.conditionId = (ushort)conditionId;
        }

    }

}