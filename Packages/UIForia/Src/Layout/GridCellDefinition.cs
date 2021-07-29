using System;
using UIForia.Layout.LayoutTypes;

namespace UIForia {

    public struct GridCellDefinition : IEquatable<GridCellDefinition> {
        
        public readonly float value;
        public readonly ushort stretch;
        public readonly GridTemplateUnit unit;

        public GridCellDefinition(float value, GridTemplateUnit unit = GridTemplateUnit.Pixel, ushort stretch = 0) {
            this.value = value;
            this.unit = unit;
            this.stretch = stretch;
        }

        public bool Equals(GridCellDefinition other) {
            return value.Equals(other.value) && stretch == other.stretch && unit == other.unit;
        }

        public override bool Equals(object obj) {
            return obj is GridCellDefinition other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = value.GetHashCode();
                hashCode = (hashCode * 397) ^ stretch.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) unit;
                return hashCode;
            }
        }

    }

}