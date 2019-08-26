namespace UIForia.Layout {

    public enum AlignmentDirection {

        Start = 0,
        End = 1,

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