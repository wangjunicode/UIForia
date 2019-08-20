namespace UIForia.Layout {

    public struct AlignmentOrigin {

        public UIFixedLength value;
        public AlignmentDirection direction;

        public AlignmentOrigin(float length, UIFixedUnit unit, AlignmentDirection direction) {
            this.value = new UIFixedLength(length, unit);
            this.direction = direction;
        }

        public AlignmentOrigin(UIFixedLength value, AlignmentDirection direction) {
            this.value = value;
            this.direction = direction;
        }

        public static bool operator ==(AlignmentOrigin self, AlignmentOrigin other) {
            return self.value == other.value && self.direction == other.direction;
        }

        public static bool operator !=(AlignmentOrigin self, AlignmentOrigin other) {
            return self.value != other.value || self.direction != other.direction;
        }
        
        public static implicit operator AlignmentOrigin(float value) {
            return new AlignmentOrigin(value, UIFixedUnit.Pixel, AlignmentDirection.Start);
        }
        
        public bool Equals(AlignmentOrigin other) {
            return value.Equals(other.value) && direction == other.direction;
        }

        public override bool Equals(object obj) {
            return obj is AlignmentOrigin other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (value.GetHashCode() * 397) ^ (int) direction;
            }
        }

    }

    public enum AlignmentDirection : byte {

        Start = 0,
        End = 1,

    }

    public struct Alignment {

        public float origin;
        public UIFixedLength value;
        public AlignmentTarget target;

        public Alignment(UIFixedLength value, float origin, AlignmentTarget target) {
            this.value = value;
            this.origin = origin;
            this.target = target;
        }

        public static bool operator ==(in Alignment self, in Alignment other) {
            return self.value == other.value && self.origin == other.origin && self.target == other.target;
        }

        public static bool operator !=(Alignment self, Alignment other) {
            return !(self == other);
        }

    }

    public enum AlignmentBehavior {

        Unset = 0,
        Cell,
        Layout,
        Parent,
        ParentContentArea,
        Template,
        TemplateContentArea,
        View,
        Screen,

    }

    public enum AlignmentTarget : ushort {

        Unset = 0,
        AllocatedBox = 1 << 0,
        Parent = 1 << 1,
        ParentContentArea = 1 << 2,
        View = 1 << 3,
        Screen = 1 << 4,

    }

}