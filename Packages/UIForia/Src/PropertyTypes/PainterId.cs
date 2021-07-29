using System;

namespace UIForia {

    public struct PainterId : IEquatable<PainterId> {

        internal int id;

        public bool Equals(PainterId other) {
            return id == other.id;
        }

        public override bool Equals(object obj) {
            return obj is PainterId other && Equals(other);
        }

        public override int GetHashCode() {
            return id;
        }

    }

}