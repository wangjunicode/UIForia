using UIForia;
using UnityEngine;

public struct FixedLengthVector {

    public bool Equals(FixedLengthVector other) {
        return x.Equals(other.x) && y.Equals(other.y);
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is FixedLengthVector && Equals((FixedLengthVector) obj);
    }

    public override int GetHashCode() {
        unchecked {
            return (x.GetHashCode() * 397) ^ y.GetHashCode();
        }
    }

    public readonly UIFixedLength x;
    public readonly UIFixedLength y;

    public FixedLengthVector(UIFixedLength x, UIFixedLength y) {
        this.x = x;
        this.y = y;
    }

    public static implicit operator FixedLengthVector(Vector2 vec) {
        return new FixedLengthVector(vec.x, vec.y);
    }

    public static bool operator ==(FixedLengthVector self, FixedLengthVector other) {
        return self.x == other.x && self.y == other.y;
    }

    public static bool operator !=(FixedLengthVector self, FixedLengthVector other) {
        return !(self == other);
    }

    public bool IsDefined() {
        return x.IsDefined() && y.IsDefined();
    }

}