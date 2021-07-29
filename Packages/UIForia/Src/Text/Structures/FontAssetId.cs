using System;

namespace UIForia {

    public struct FontAssetId : IEquatable<FontAssetId> {

        internal readonly int id;// todo -- make a ushort

        internal FontAssetId(ushort id) {
            this.id = id;
        }

        public bool Equals(FontAssetId other) {
            return id == other.id;
        }

        public override bool Equals(object obj) {
            return obj is FontAssetId other && Equals(other);
        }

        public override int GetHashCode() {
            return id;
        }

    }

}