namespace UIForia.Text {

    // cannot be a flag since user can define their own types. values of 256 and higher are reserved for user
    public enum TextSymbolType {

        Character,
        NoBreakPush,
        NoBreakPop,
        HorizontalSpace,
        ColorPush,
        ColorPop,
        
        CharSpacingPush,
        CharSpacingPop,
        
        FontPush,
        FontPop,
        
        FontSizePush,
        FontSizePop,

        FacePush,
        FacePop,
        UnderlayPush,
        UnderlayPop,
        OutlinePush,
        OutlinePop,
        GlowPush,
        GlowPop,
        
        FontStylePush,
        FontStylePop,
        
        TextTransformPush,
        TextTransformPop,

        Sprite,

        EffectPush,
        EffectPop,

        UnderlinePush,
        UnderlinePop,

        TexturePush,
        TexturePop,

        PushHorizontalInvert,
        PopHorizontalInvert,
        
        PushVerticalInvert,
        PopVerticalInvert,

        PushOpacity,
        PopOpacity

    }
  
}