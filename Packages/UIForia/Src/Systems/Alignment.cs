namespace UIForia.Layout {

    public struct Alignment {

        public float pivot;
        public UIFixedLength value;
        public AlignmentTarget target;

        public Alignment(UIFixedLength value, float pivot, AlignmentTarget target) {
            this.value = value;
            this.pivot = pivot;
            this.target = target;
        }
        
        public static bool operator ==(in Alignment self, in Alignment other) {
            return self.value == other.value && self.pivot == other.pivot && self.target == other.target;
        }

        public static bool operator !=(Alignment self, Alignment other) {
            return !(self == other);
        }
        
    }

    public enum AlignmentBehavior {

        Unset = 0,
        Self = 1,
        Layout = 2,
        Default = 3

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