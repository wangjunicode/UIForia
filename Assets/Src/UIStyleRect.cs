public struct UIStyleRect {

    public float top;
    public float right;
    public float bottom;
    public float left;

    public UIStyleRect(float top, float right, float bottom, float left) {
        this.top = top;
        this.right = right;
        this.bottom = bottom;
        this.left = left;
    }

    public UIStyleRect(float value) {
        this.top = value;
        this.right = value;
        this.bottom = value;
        this.left = value;
    }
    
    public float horizontal => right + left;
    public float vertical => top     + bottom;

    public bool Equals(UIStyleRect other) {
        return top       == other.top
               && right  == other.right
               && bottom == other.bottom
               && left   == other.left;
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is UIStyleRect && Equals((UIStyleRect) obj);
    }

    public override int GetHashCode() {
        unchecked {
            int hashCode = top.GetHashCode();
            hashCode = (hashCode * 397) ^ right.GetHashCode();
            hashCode = (hashCode * 397) ^ bottom.GetHashCode();
            hashCode = (hashCode * 397) ^ left.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(UIStyleRect self, UIStyleRect other) {
        return self.top       == other.top
               && self.left   == other.left
               && self.right  == other.right
               && self.bottom == other.bottom;
    }

    public static bool operator !=(UIStyleRect self, UIStyleRect other) {
        return !(self == other);
    }

}