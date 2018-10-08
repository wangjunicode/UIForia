namespace Rendering {

    public enum StylePropertyId {

        OverflowX = 1,
        OverflowY = 2,

        BackgroundColor = 100,
        BorderColor = 101,
        BackgroundImage = 102,

        GridItemColStart = 200,
        GridItemColSpan = 201,
        GridItemRowStart = 202,
        GridItemRowSpan = 203,

        GridFlowDirection = 204,
        GridPlacementDensity = 205,
        GridColTemplate = 206,
        GridRowTemplate = 207,
        GridColAutoSize = 208,
        GridRowAutoSize = 209,
        GridColGap = 210,
        GridRowGap = 211,

        FlexWrap = 300,
        FlexDirection = 301,
        FlexMainAxisAlignment = 302,
        FlexCrossAxisAlignment = 303,

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

        __TextPropertyStart__ = 1000,
        TextColor = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextColor,
        TextFontAsset = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextFontAsset,
        TextFontSize = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextFontSize,
        TextFontStyle = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextFontStyle,
        TextAnchor = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextAnchor,
        TextWhitespaceMode = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextWhitespaceMode,
        TextWrapMode = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextWrapMode,
        TextHorizontalOverflow = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextHorizontalOverflow, // Truncate | Overflow | Wrap
        TextVerticalOverflow = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextVerticalOverflow, // Truncate | Overflow | Ellipsis
        TextIndentFirstLine = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextIndentFirstLine,
        TextIndentNewLine = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextIndentNewLine,
        TextLayoutStyle = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextLayoutStyle, // UseKerning | Monospace
        TextAutoSize = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextAutoSize,
        TextTransform = __TextPropertyStart__ + UIStyle.TextPropertyIdFlag.TextTransform,
        __TextPropertyEnd__ = 1013,

        MinWidth = 1100,
        MaxWidth = 1101,
        PreferredWidth = 1102,
        MinHeight = 1103,
        MaxHeight = 1104,
        PreferredHeight = 1105,

        LayoutType = 1201,
        IsInLayoutFlow = 1202,
        LayoutBehavior = 1203
        
        // total = 70


    }

}