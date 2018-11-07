namespace UIForia.Rendering {

    public enum StylePropertyId {

        OverflowX = 1,
        OverflowY = 2,

        BackgroundColor = 100,
        BorderColor = 101,
        BackgroundImage = 102,
        BackgroundFillType = 103,
        BackgroundGradientType = 104,
        BackgroundGradientAxis = 105,
        BackgroundGradientStart = 106,
        BackgroundGridSize = 107,
        BackgroundLineSize = 108,
        BackgroundFillOffsetX = 109,
        BackgroundFillOffsetY = 110,
        BackgroundFillScaleX = 111,
        BackgroundFillScaleY = 112,
        BackgroundFillRotation = 113,
        BackgroundImageTileX = 114,
        BackgroundImageTileY = 115,
        BackgroundImageOffsetX = 116,
        BackgroundImageOffsetY = 117,
        BackgroundImage1 = 118,
        BackgroundImage2 = 119,
        BackgroundColorSecondary = 120,
        BackgroundShapeType = 121,
        Opacity = 122,
        Cursor = 123,

        GridItemColStart = 200,
        GridItemColSpan = 201,
        GridItemRowStart = 202,
        GridItemRowSpan = 203,
        GridItemColSelfAlignment = 204,
        GridItemRowSelfAlignment = 205,

        GridLayoutDirection = 2204,
        GridLayoutDensity = 2205,
        GridLayoutColTemplate = 2206,
        GridLayoutRowTemplate = 2207,
        GridLayoutColAutoSize = 2208,
        GridLayoutRowAutoSize = 2209,
        GridLayoutColGap = 2210,
        GridLayoutRowGap = 2211,
        GridLayoutColAlignment = 2212,
        GridLayoutRowAlignment = 2213,

        FlexLayoutWrap = 300,
        FlexLayoutDirection = 301,
        FlexLayoutMainAxisAlignment = 302,
        FlexLayoutCrossAxisAlignment = 303,

        FlexItemSelfAlignment = 400,
        FlexItemOrder = 401,
        FlexItemGrow = 402,
        FlexItemShrink = 403,

        MarginTop = 500,
        MarginRight = 501,
        MarginBottom = 502,
        MarginLeft = 503,

        BorderTop = 600,
        BorderRight = 601,
        BorderBottom = 602,
        BorderLeft = 603,

        PaddingTop = 700,
        PaddingRight = 701,
        PaddingBottom = 702,
        PaddingLeft = 703,

        BorderRadiusTopLeft = 800,
        BorderRadiusTopRight = 801,
        BorderRadiusBottomLeft = 802,
        BorderRadiusBottomRight = 803,

        TransformPositionX = 900,
        TransformPositionY = 901,
        TransformScaleX = 902,
        TransformScaleY = 903,
        TransformPivotX = 904,
        TransformPivotY = 905,
        TransformRotation = 906,
        TransformBehaviorX = 907,
        TransformBehaviorY = 908,

        __TextPropertyStart__ = 1000,
        TextColor = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextColor,
        TextFontAsset = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextFontAsset,
        TextFontSize = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextFontSize,
        TextFontStyle = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextFontStyle,
        TextAlignment = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextAnchor,
        TextWhitespaceMode = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextWhitespaceMode,
//        TextWrapMode = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextWrapMode,
//        TextHorizontalOverflow = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextHorizontalOverflow, // Truncate | Overflow | Wrap
//        TextVerticalOverflow = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextVerticalOverflow, // Truncate | Overflow | Ellipsis
       // TextIndentFirstLine = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextIndentFirstLine,
//        TextIndentNewLine = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextIndentNewLine,
//        TextLayoutStyle = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextLayoutStyle, // UseKerning | Monospace
//        TextAutoSize = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextAutoSize,
        TextTransform = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextTransform,
        __TextPropertyEnd__ = 1013,

        MinWidth = 1100,
        MaxWidth = 1101,
        PreferredWidth = 1102,
        MinHeight = 1103,
        MaxHeight = 1104,
        PreferredHeight = 1105,

        LayoutType = 1201,
        LayoutBehavior = 1202,

        AnchorTop = 1301,
        AnchorRight = 1302,
        AnchorBottom = 1303,
        AnchorLeft = 1304,
        AnchorTarget = 1305,

        ScrollbarVerticalTrackSize = 1301,
        ScrollbarVerticalTrackRadius = 1302,
        ScrollbarVerticalHandleSize = 1303,
        ScrollbarVerticalHandleRadius = 1304,
        ScrollbarVerticalTrackColor = 1305,
        ScrollbarVerticalHandleColor = 1306,
        ScrollbarVerticalAttachment = 1307,
        ScrollbarVerticalButtonPlacement = 1308,
        
//        ScrollBarVerticalFadeDelay = 1305,
//        ScrollBarVerticalHoverTrackSize = 1305,
//        ScrollBarVerticalHoverHandleSize = 1305,
//        ScrollBarVerticalDragTrackSize = 1305,
//        ScrollBarVerticalDragHandleSize = 1305,
//        ScrollBarVerticalTrackBackground = 1304,
//        
//        ScrollBarHorizontalTrackSize = 1302,
//        ScrollBarHorizontalHandleSize = 1304,
//        ScrollBarHorizontalFadeDelay = 1305,


        ZIndex = 1401,
        RenderLayer = 1402,
        RenderLayerOffset = 1403,
        
//        ScrollbarVerticalAttachment = 1501,
//        ScrollbarHorizontalAttachment = 1501,
//        ScrollbarVerticalButtonPlacement,
//        ScrollbarHorizontalButtonPlacement,
//        ScrollbarVerticalBackgroundFillType,
//        ScrollbarVertical,
        
//        BackgroundColor = 100,
//        BorderColor = 101,
//        BackgroundImage = 102,
//        BackgroundFillType = 103,
//        BackgroundGradientType = 104,
//        BackgroundGradientAxis = 105,
//        BackgroundGradientStart = 106,
//        BackgroundGridSize = 107,
//        BackgroundLineSize = 108,
//        BackgroundFillOffsetX = 109,
//        BackgroundFillOffsetY = 110,
//        BackgroundFillScaleX = 111,
//        BackgroundFillScaleY = 112,
//        BackgroundFillRotation = 113,
//        BackgroundImageTileX = 114,
//        BackgroundImageTileY = 115,
//        BackgroundImageOffsetX = 116,
//        BackgroundImageOffsetY = 117,
//        BackgroundColorSecondary = 120,
//        BackgroundShapeType = 121,
//        Opacity = 122,
//        Cursor = 123,

    }

}

/*
 *    style.Scrollbar.VerticalTrack.BackgroundColor = 
 *
 * 
*/