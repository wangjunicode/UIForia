using System;
using System.Diagnostics;

namespace UIForia {

    [DebuggerDisplay("Index = {index} generation = {generation}")]
    public struct ElementId : IComparable<ElementId> {

        private const int ENTITY_INDEX_BITS = 24;
        private const int ENTITY_INDEX_MASK = (1 << ENTITY_INDEX_BITS) - 1;
        private const int ENTITY_GENERATION_BITS = 8;
        private const int ENTITY_GENERATION_MASK = (1 << ENTITY_GENERATION_BITS) - 1;

        public readonly int id;

        internal ElementId(int index, byte generation) {
            // todo -- not totally sure of this
            this.id = (index & ENTITY_INDEX_MASK) | (generation << ENTITY_INDEX_BITS);
        }

        internal ElementId(int id) {
            this.id = id;
        }

        public int index {
            [DebuggerStepThrough] get => (id & ENTITY_INDEX_MASK);
        }

        public int generation {
            [DebuggerStepThrough] get => ((id >> ENTITY_INDEX_BITS) & ENTITY_GENERATION_MASK);
        }

        public static ElementId Invalid {
            get => new ElementId(0, 0);
        }

        public static bool operator ==(ElementId elementId, ElementId other) {
            return elementId.id == other.id;
        }

        public static bool operator !=(ElementId elementId, ElementId other) {
            return elementId.id != other.id;
        }

        public static explicit operator int(ElementId elementId) {
            return elementId.id;
        }

        public static explicit operator ElementId(int elementId) {
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

        public int CompareTo(ElementId other) {
            return id < other.id ? -1 : id > other.id ? 1 : 0;
        }

        public override string ToString() {
            return $"Index = {index} generation = {generation}";
        }

    }

}