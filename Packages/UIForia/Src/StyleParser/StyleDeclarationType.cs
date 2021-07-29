namespace UIForia.Parsing {

    internal enum StyleDeclarationType {

        Property,
        ShortHand,
        MaterialVar,
        PainterVar,
        Transition,
        MixinUsage,
        MixinProperty, // some string prefixed with the mixin specifier character '%'
        // formerly known as RunCommand
        Action,
        CustomProperty

    }

}