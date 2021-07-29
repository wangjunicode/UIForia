namespace UIForia.Text {
    internal enum SymbolType {

        None = 0,
        NewLine,
        CharacterGroup,
        PushFont,
        PopFont,
        PushFontSize,
        PopFontSize,
        
        HorizontalSpace,
        VerticalSpace,

        PushColor,
        PopColor,

        PushCursor,
        PopCursor,

        PushAlignment,
        PopAlignment,

        PushTransform,
        PopTransform,

        PushWhitespaceMode,
        PopWhitespaceMode,

        PushHitRegion,
        PopHitRegion,

        PushLineInsets,
        PopLineInsets,

        InlineSpace,

        PushVerticalAlignment,

        PopVerticalAlignment

    }
}