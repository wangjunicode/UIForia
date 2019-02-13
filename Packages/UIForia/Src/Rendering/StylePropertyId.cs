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
//        BackgroundGridSize = 107,
//        BackgroundLineSize = 108,
        BackgroundFillOffsetX = 109,
        BackgroundFillOffsetY = 110,
        BackgroundFillScaleX = 111,
        BackgroundFillScaleY = 112,
        BackgroundFillRotation = 113,
//        BackgroundImageTileX = 114,
//        BackgroundImageTileY = 115,
//        BackgroundImageOffsetX = 116,
//        BackgroundImageOffsetY = 117,
        BackgroundImage1 = 118,
        BackgroundImage2 = 119,
        BackgroundColorSecondary = 120,
        BackgroundShapeType = 121,
        Opacity = 122,
        Cursor = 123,
        Visibility = 124, 
        
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
        GridLayoutMainAxisAutoSize = 2208,
        GridLayoutCrossAxisAutoSize = 2209,
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
//        TextWhitespaceMode = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextWhitespaceMode,
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
        
        // Scrollbar Vertical
        ScrollbarVerticalTrackSize = 1501,
        ScrollbarVerticalTrackBorderRadius = 1502,
        ScrollbarVerticalTrackBorderSize = 1503,
        ScrollbarVerticalTrackBorderColor = 1504,
        ScrollbarVerticalTrackImage = 1505,
        ScrollbarVerticalTrackColor = 1506,
        
        ScrollbarVerticalHandleSize = 1521,
        ScrollbarVerticalHandleBorderRadius = 1522,
        ScrollbarVerticalHandleBorderSize = 1523,
        ScrollbarVerticalHandleBorderColor = 1524,
        ScrollbarVerticalHandleImage = 1525,
        ScrollbarVerticalHandleColor = 1526,
        
        ScrollbarVerticalAttachment = 1540,
        ScrollbarVerticalButtonPlacement = 1541,
        
        ScrollbarVerticalIncrementSize = 1551,
        ScrollbarVerticalIncrementBorderRadius = 1552,
        ScrollbarVerticalIncrementBorderSize = 1553,
        ScrollbarVerticalIncrementBorderColor = 1554,
        ScrollbarVerticalIncrementImage = 1555,
        ScrollbarVerticalIncrementColor = 1556,
        
        ScrollbarVerticalDecrementSize = 1571,
        ScrollbarVerticalDecrementBorderRadius = 1572,
        ScrollbarVerticalDecrementBorderSize = 1573,
        ScrollbarVerticalDecrementBorderColor = 1574,
        ScrollbarVerticalDecrementImage = 1575,
        ScrollbarVerticalDecrementColor = 1576,
        
        // Scrollbar Horizontal
        ScrollbarHorizontalTrackSize = 1601,
        ScrollbarHorizontalTrackBorderRadius = 1602,
        ScrollbarHorizontalTrackBorderSize = 1603,
        ScrollbarHorizontalTrackBorderColor = 1604,
        ScrollbarHorizontalTrackImage = 1605,
        ScrollbarHorizontalTrackColor = 1606,
        
        ScrollbarHorizontalHandleSize = 1621,
        ScrollbarHorizontalHandleBorderRadius = 1622,
        ScrollbarHorizontalHandleBorderSize = 1623,
        ScrollbarHorizontalHandleBorderColor = 1624,
        ScrollbarHorizontalHandleImage = 1625,
        ScrollbarHorizontalHandleColor = 1626,
        
        ScrollbarHorizontalAttachment = 1640,
        ScrollbarHorizontalButtonPlacement = 1641,
        
        ScrollbarHorizontalIncrementSize = 1651,
        ScrollbarHorizontalIncrementBorderRadius = 1652,
        ScrollbarHorizontalIncrementBorderSize = 1653,
        ScrollbarHorizontalIncrementBorderColor = 1654,
        ScrollbarHorizontalIncrementImage = 1655,
        ScrollbarHorizontalIncrementColor = 1656,
        
        ScrollbarHorizontalDecrementSize = 1671,
        ScrollbarHorizontalDecrementBorderRadius = 1672,
        ScrollbarHorizontalDecrementBorderSize = 1673,
        ScrollbarHorizontalDecrementBorderColor = 1674,
        ScrollbarHorizontalDecrementImage = 1675,
        ScrollbarHorizontalDecrementColor = 1676,
 
    }

}