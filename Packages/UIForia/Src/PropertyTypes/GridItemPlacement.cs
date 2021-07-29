using System;

namespace UIForia {

    public struct GridItemPlacement : IEquatable<GridItemPlacement> {

        internal short place;
        internal ushort span;
        
        public GridItemPlacement(short place, ushort span = 1) : this() {
            this.place = place;
            this.span = (ushort) (span <= 0 ? 1 : span);
        }

        public int end => place + span;
        
        public short Place => place;
        public ushort Span => span;

        public bool Equals(GridItemPlacement other) {
            return place == other.place && span == other.span;
        }

        public override bool Equals(object obj) {
            return obj is GridItemPlacement other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (place.GetHashCode() * 397) ^ span.GetHashCode();
            }
        }

    }

}