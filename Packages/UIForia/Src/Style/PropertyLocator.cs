namespace UIForia.Style {

    internal struct PropertyLocator {

        /// <summary>
        /// Points to property types. index 27 = BackgroundColor for example. 
        /// </summary>
        public readonly ushort propertyTypeIndex; // (ushort)PropertyId.XYZ
        public readonly ushort variableNameId;
        
        /// <summary>
        /// StyleDatabase *-- property tables per type
        /// This is an index into on of those tables
        /// </summary>
        public readonly int indexInPropertyValueTable;

        public PropertyLocator(PropertyId propertyId, int indexInPropertyValueTable, ushort variableNameId = 0) {
            this.propertyTypeIndex = propertyId.index;
            this.indexInPropertyValueTable = indexInPropertyValueTable;
            this.variableNameId = variableNameId;
        }

        public PropertyId PropertyId => new PropertyId(propertyTypeIndex);

        public bool IsVariable => variableNameId != ushort.MaxValue;

    }

    internal struct TransitionLocator {

        public readonly ushort propertyIndex;
        public readonly int transitionIndex;

        public TransitionLocator(PropertyId propertyId, int transitionIndex) {
            this.propertyIndex = propertyId.index;
            this.transitionIndex = transitionIndex;
        }

        public PropertyId PropertyId => new PropertyId(propertyIndex);

    }

}