namespace UIForia {

    public enum SourceType : byte {

        Inherited = 0,
        Implicit = 1,
        Shared = 1 << 1,
        Selector = 1 << 2,
        Transition = 1 << 3,
        Animation = 1 << 4,
        Instance = byte.MaxValue,

    }

}