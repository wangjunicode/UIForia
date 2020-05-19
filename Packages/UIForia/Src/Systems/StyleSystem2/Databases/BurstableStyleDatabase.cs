namespace UIForia {

    public unsafe struct BurstableStyleDatabase {

        public byte* staticStyleProperties;
        public ModuleTable<ModuleCondition> conditionTable;
        public StyleTable<StaticStyleInfo> sharedStyleTable;
        public SelectorTable<SelectorStyleEffect> selectorStyleTable;

        public int GetSelectorProperties(SelectorId selectorId, out StaticPropertyId* keys, out PropertyData* values, out ModuleCondition conditionSet) {
            keys = default;
            values = default;
            conditionSet = default;
            return 0;
        }

        public int GetStyleProperties(StyleId styleId, StyleState2 state, out StaticPropertyId* keys, out PropertyData* values, out ModuleCondition conditionSet) {

            ref StaticStyleInfo staticStyle = ref sharedStyleTable[styleId];

            conditionSet = conditionTable[staticStyle.moduleId];

            int offset = 0;
            int count = 0;

            switch (state) {
                case StyleState2.Normal:
                    offset = staticStyle.normalOffset;
                    count = staticStyle.normalCount;
                    break;

                case StyleState2.Hover:
                    offset = staticStyle.hoverOffset;
                    count = staticStyle.hoverCount;
                    break;

                case StyleState2.Focused:
                    offset = staticStyle.focusOffset;
                    count = staticStyle.focusCount;
                    break;

                case StyleState2.Active:
                    offset = staticStyle.activeOffset;
                    count = staticStyle.activeCount;
                    break;
            }

            keys = (StaticPropertyId*) (staticStyleProperties + staticStyle.propertyOffsetInBytes + offset);
            values = (PropertyData*) (keys + count);

            return count;

        }

    }

}