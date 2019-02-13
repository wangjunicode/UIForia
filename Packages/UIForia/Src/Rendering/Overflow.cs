namespace UIForia.Rendering {

    public enum Overflow {

        Unset = 0,
        None = 1 << 0, 
        Hidden  = 1 << 1,
        Scroll = 1 << 2,
        ScrollAndAutoHide = 1 << 3

    }

}