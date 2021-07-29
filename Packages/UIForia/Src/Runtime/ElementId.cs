using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia {

    /// <summary>
    /// 1 based id. If you find an element id to be 0 it's not a real id, it's just a default value.
    /// </summary>
    [DebuggerDisplay("Index = {index} generation = {generation}, DisplayName = {GetDebugDisplayName()}")]
    [StructLayout(LayoutKind.Explicit)]
    public struct ElementId : IComparable<ElementId>, IEquatable<ElementId>, IToStringBuffer {

        private const int ENTITY_INDEX_BITS = 24;
        internal const int k_IndexMask = (1 << ENTITY_INDEX_BITS) - 1;
        private const int ENTITY_GENERATION_BITS = 8;
        internal const int ENTITY_GENERATION_MASK = (1 << ENTITY_GENERATION_BITS) - 1;

        [FieldOffset(0)] public readonly int id;
        [FieldOffset(3)] internal byte generation;

        internal ElementId(int index, byte generation) {
            // todo -- not totally sure of this
            // this might be better -> (high << 24) | (low & 0xffffff);
            this.id = (index & k_IndexMask) | (generation << ENTITY_INDEX_BITS);
            this.generation = generation;
        }

        public ElementId(int id) {
            this.generation = 0;
            this.id = id;
        }

        public int index {
            [DebuggerStepThrough] get => (id & k_IndexMask);
        }
        //
        // public int generation {
        //     [DebuggerStepThrough] get => ((id >> ENTITY_INDEX_BITS) & ENTITY_GENERATION_MASK);
        // }

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

        public static implicit operator ElementId(UIElement element) {
            return element.elementId;
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

        public void ToStringBuffer(CharStringBuilder builder) {
            builder.Append("Index = ");
            builder.Append(index);
            builder.Append(" generation = ");
            builder.Append(generation);
        }

        public string GetDebugDisplayName() {
            if (UIApplication.s_DebuggedApplication == null) return "Unknown";
            if (index == 0 || index >= UIApplication.s_DebuggedApplication.instanceTable.Length) {
                return "Invalid";
            }

            return UIApplication.s_DebuggedApplication.instanceTable[index].GetDisplayName();
        }


    }

}