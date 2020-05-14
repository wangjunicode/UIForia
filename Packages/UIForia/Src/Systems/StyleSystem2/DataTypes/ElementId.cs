namespace UIForia {

    public struct ElementId {

    
        private const int ENTITY_INDEX_BITS = 24;
        private const int ENTITY_INDEX_MASK = (1 << ENTITY_INDEX_BITS) - 1;
        private const int ENTITY_GENERATION_BITS = 8;
        private const int ENTITY_GENERATION_MASK = (1 << ENTITY_GENERATION_BITS) - 1;

        public readonly int id;

        internal ElementId(int id, byte generation) {
            // todo -- not totally sure of this
            this.id = (id & ENTITY_INDEX_MASK) | (generation << ENTITY_INDEX_BITS);
        }

        internal ElementId(int id) {
            this.id = id;
        }

        public int index {
            get => id & ENTITY_INDEX_MASK;
        }

        public int generation {
            get => ((id >> ENTITY_INDEX_BITS) & ENTITY_GENERATION_MASK);
        }

        public static bool operator ==(ElementId elementId, ElementId other) {
            return elementId.id == other.index;
        }

        public static bool operator !=(ElementId elementId, ElementId other) {
            return elementId.id != other.index;
        }

        public static explicit operator int(ElementId elementId) {
            return elementId.id;
        }

        public static implicit operator ElementId(int elementId) {
            return new ElementId(elementId);
        }
        
        public bool Equals(ElementId other) {
            return id == other.id;
        }

        public override bool Equals(object obj) {
            return obj is ElementId other && Equals(other);
        }

        public override int GetHashCode() {
            return id;
        }


    }

}