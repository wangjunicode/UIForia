using System.Collections.Generic;
using Src;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Rendering;
using Src.Util;
using UnityEngine;

namespace Rendering {

    public static class DefaultStyleValues {

        // todo border radius & margin need to be fixed lengths
        public static readonly UIMeasurement BorderRadiusTopLeft = new UIMeasurement(0);
        public static readonly UIMeasurement BorderRadiusTopRight = new UIMeasurement(0);
        public static readonly UIMeasurement BorderRadiusBottomRight = new UIMeasurement(0);
        public static readonly UIMeasurement BorderRadiusBottomLeft = new UIMeasurement(0);

        public static readonly Color BorderColor = ColorUtil.UnsetValue;
        public static readonly Color BackgroundColor = ColorUtil.UnsetValue;
        public static readonly Texture2DAssetReference BackgroundImage = new Texture2DAssetReference(IntUtil.UnsetValue);

        public static readonly Overflow OverflowX = Overflow.None;
        public static readonly Overflow OverflowY = Overflow.None;

        public const int FlexItemOrder = ushort.MaxValue;
        public const int FlexItemGrow = 0;
        public const int FlexItemShrink = 0;
        public static readonly CrossAxisAlignment FlexItemSelfAlignment = CrossAxisAlignment.Unset;

        public static readonly LayoutWrap FlexWrap = LayoutWrap.None;
        public static readonly LayoutDirection FlexLayoutDirection = LayoutDirection.Row;
        public static readonly MainAxisAlignment FlexLayoutMainAxisAlignment = MainAxisAlignment.Start;
        public static readonly CrossAxisAlignment FlexLayoutCrossAxisAlignment = CrossAxisAlignment.Start;

        public static readonly UIMeasurement MinWidth = new UIMeasurement(0);
        public static readonly UIMeasurement MaxWidth = new UIMeasurement(float.MaxValue);
        public static readonly UIMeasurement PreferredWidth = UIMeasurement.Content100;
        public static readonly UIMeasurement MinHeight = new UIMeasurement(0);
        public static readonly UIMeasurement MaxHeight = new UIMeasurement(float.MaxValue);
        public static readonly UIMeasurement PreferredHeight = UIMeasurement.Content100;

        public static readonly UIMeasurement MarginTop = new UIMeasurement(0);
        public static readonly UIMeasurement MarginRight = new UIMeasurement(0);
        public static readonly UIMeasurement MarginLeft = new UIMeasurement(0);
        public static readonly UIMeasurement MarginBottom = new UIMeasurement(0);

        public static readonly UIFixedLength PaddingTop = new UIFixedLength(0);
        public static readonly UIFixedLength PaddingRight = new UIFixedLength(0);
        public static readonly UIFixedLength PaddingLeft = new UIFixedLength(0);
        public static readonly UIFixedLength PaddingBottom = new UIFixedLength(0);

        public static readonly UIFixedLength BorderTop = new UIFixedLength(0);
        public static readonly UIFixedLength BorderRight = new UIFixedLength(0);
        public static readonly UIFixedLength BorderLeft = new UIFixedLength(0);
        public static readonly UIFixedLength BorderBottom = new UIFixedLength(0);

        public static readonly UIFixedLength TransformPositionX = new UIFixedLength(0);
        public static readonly UIFixedLength TransformPositionY = new UIFixedLength(0);
        public static readonly UIFixedLength TransformPivotX = new UIFixedLength(0.5f, UIFixedUnit.Percent);
        public static readonly UIFixedLength TransformPivotY = new UIFixedLength(0.5f, UIFixedUnit.Percent);
        
        public const TransformBehavior TransformBehaviorX = TransformBehavior.Default;
        public const TransformBehavior TransformBehaviorY = TransformBehavior.Default;
        
        public const float TransformScaleX = 1;
        public const float TransformScaleY = 1;
        public const float TransformRotation = 0;

        public static readonly UIFixedLength AnchorTop = 0f;
        public static readonly UIFixedLength AnchorRight = 1f;
        public static readonly UIFixedLength AnchorBottom = 1f;
        public static readonly UIFixedLength AnchorLeft = 0f;
        public const AnchorTarget AnchorTarget = Rendering.AnchorTarget.Parent;
        
        public static readonly LayoutType LayoutType = LayoutType.Flex;
        public const int TextFontSize = 18;
        public static readonly Color TextColor = Color.black;
        public const TextUtil.FontStyle TextFontStyle = TextUtil.FontStyle.Normal;
        public static readonly FontAssetReference TextFontAsset = new FontAssetReference("default");
        public const TextUtil.TextAlignment TextAlignment = TextUtil.TextAlignment.Left;
        public const TextUtil.TextTransform TextTransform = TextUtil.TextTransform.None;

        public static readonly LayoutBehavior LayoutBehavior = LayoutBehavior.Normal;
        public const int GridItemColStart = IntUtil.UnsetValue;
        public const int GridItemRowStart = IntUtil.UnsetValue;
        public const int GridItemRowSpan = 1;
        public const int GridItemColSpan = 1;
        public const CrossAxisAlignment GridItemColSelfAlignment = CrossAxisAlignment.Unset;
        public const CrossAxisAlignment GridItemRowSelfAlignment = CrossAxisAlignment.Unset;

        public const LayoutDirection GridLayoutDirection = LayoutDirection.Row;
        public static readonly GridLayoutDensity GridLayoutDensity = GridLayoutDensity.Sparse;

        public const float GridLayoutColGap = 0f;
        public const float GridLayoutRowGap = 0f;

        public static readonly GridTrackSize GridLayoutColAutoSize = GridTrackSize.MaxContent;
        public static readonly GridTrackSize GridLayoutRowAutoSize = GridTrackSize.MaxContent;

        public static readonly IReadOnlyList<GridTrackSize> GridLayoutColTemplate = ListPool<GridTrackSize>.Empty;
        public static readonly IReadOnlyList<GridTrackSize> GridLayoutRowTemplate = ListPool<GridTrackSize>.Empty;

        public static readonly CrossAxisAlignment GridLayoutColAlignment = CrossAxisAlignment.Start;
        public static readonly CrossAxisAlignment GridLayoutRowAlignment = CrossAxisAlignment.Start;

//        public static readonly float RadialLayoutStartAngle = 0f;
//        public static readonly float RadialLayoutRangeLimit = 0f;
//        public static readonly UIFixedLength RadialLayoutOffsetDistance = 0f;

        public const int ZIndex = 0;
        public const RenderLayer RenderLayer = Rendering.RenderLayer.Default;
        public const int LayerOffset = 0;

    }

}