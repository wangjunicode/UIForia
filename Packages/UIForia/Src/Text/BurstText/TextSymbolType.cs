namespace UIForia.Text {

    // cannot be a flag since user can define their own types
    public enum TextSymbolType {

        Character,
        NoBreakPush,
        NoBreakPop,
        HorizontalSpace,
        ColorPush,
        ColorPop,
        
        FontPush,
        FontPop,
        
        FontSizePush,
        FontSizePop,
        
        // underlay, 
        // glow
        // softness
        
        FontStylePush,
        FontStylePop,
        
        TextTransformPush,
        TextTransformPop,

        MaterialPush,
        MaterialPop,
        Sprite,

        EffectPush,

        EffectPop

    }
  
}