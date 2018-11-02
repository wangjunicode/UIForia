using System;
using System.Collections.Generic;
using Shapes2D;
using Src;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Rendering;
using Src.Util;
using TMPro;
using UnityEngine;

namespace Rendering {

    public static class DefaultStyleValues {

        public static readonly UIFixedLength BorderRadiusTopLeft = new UIFixedLength(0);
        public static readonly UIFixedLength BorderRadiusTopRight = new UIFixedLength(0);
        public static readonly UIFixedLength BorderRadiusBottomRight = new UIFixedLength(0);
        public static readonly UIFixedLength BorderRadiusBottomLeft = new UIFixedLength(0);

        public static readonly Color BorderColor = ColorUtil.UnsetValue;
        public static readonly Color BackgroundColor = ColorUtil.UnsetValue;
        public static readonly Texture2D BackgroundImage = null;
        public static readonly Color BackgroundColorSecondary = Color.white;

        public const Overflow OverflowX = Overflow.None;
        public const Overflow OverflowY = Overflow.None;

        public const int FlexItemOrder = ushort.MaxValue;
        public const int FlexItemGrow = 0;
        public const int FlexItemShrink = 0;
        public static readonly CrossAxisAlignment FlexItemSelfAlignment = CrossAxisAlignment.Unset;

        public const LayoutWrap FlexLayoutWrap = LayoutWrap.None;
        public const LayoutDirection FlexLayoutDirection = LayoutDirection.Row;
        public const MainAxisAlignment FlexLayoutMainAxisAlignment = MainAxisAlignment.Start;
        public const CrossAxisAlignment FlexLayoutCrossAxisAlignment = CrossAxisAlignment.Start;

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

        public static readonly UIFixedLength AnchorTop = new UIFixedLength(0f, UIFixedUnit.Percent);
        public static readonly UIFixedLength AnchorRight = new UIFixedLength(1f, UIFixedUnit.Percent);
        public static readonly UIFixedLength AnchorBottom = new UIFixedLength(1f, UIFixedUnit.Percent);
        public static readonly UIFixedLength AnchorLeft = new UIFixedLength(0f, UIFixedUnit.Percent);
        public const AnchorTarget AnchorTarget = Rendering.AnchorTarget.Parent;

        public static readonly LayoutType LayoutType = LayoutType.Flex;
        public const int TextFontSize = 18;
        public static readonly Color TextColor = Color.black;
        public const TextUtil.FontStyle TextFontStyle = TextUtil.FontStyle.Normal;
        public static readonly TMP_FontAsset TextFontAsset = TMP_FontAsset.defaultFontAsset;
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

        public const CrossAxisAlignment GridLayoutColAlignment = CrossAxisAlignment.Start;
        public const CrossAxisAlignment GridLayoutRowAlignment = CrossAxisAlignment.Start;

//        public static readonly float RadialLayoutStartAngle = 0f;
//        public static readonly float RadialLayoutRangeLimit = 0f;
//        public static readonly UIFixedLength RadialLayoutOffsetDistance = 0f;

        public const int ZIndex = 0;
        public const RenderLayer RenderLayer = Rendering.RenderLayer.Default;
        public const int RenderLayerOffset = 0;

        public const GradientType BackgroundGradientType = GradientType.Linear;
        public const GradientAxis BackgroundGradientAxis = GradientAxis.Horizontal;
        public const BackgroundShapeType BackgroundShapeType = Src.Rendering.BackgroundShapeType.Rectangle;
        public const BackgroundFillType BackgroundFillType = Src.Rendering.BackgroundFillType.Normal;

        public const float BackgroundFillRotation = 0f;
        public const float BackgroundGradientStart = 0;
        public const float BackgroundFillOffsetX = 0;
        public const float BackgroundFillOffsetY = 0;
        public const float BackgroundFillScaleX = 1;
        public const float BackgroundFillScaleY = 1;
        public const float BackgroundGridSize = 0;
        public const float BackgroundLineSize = 0;
        public const float BackgroundImageTileX = 0;
        public const float BackgroundImageTileY = 0;
        public const float BackgroundImageOffsetX = 0;
        public const float BackgroundImageOffsetY = 0;
        public const float Opacity = 1;

        public const Texture2D BackgroundImage1 = null;
        public const Texture2D BackgroundImage2 = null;

        public const Texture2D Cursor = null;

        public const TextUtil.TextAlignment TextAnchor = TextUtil.TextAlignment.Left;
        public const WhitespaceMode TextWhitespaceMode = WhitespaceMode.Preserve;

        public static StyleProperty GetPropertyValue(StylePropertyId propertyId) {
            switch (propertyId) {
                case StylePropertyId.OverflowX:
                    return new StyleProperty(propertyId, (int) OverflowX);

                case StylePropertyId.OverflowY:
                    return new StyleProperty(propertyId, (int) OverflowY);

                case StylePropertyId.BackgroundColor:
                    return StyleProperty.BackgroundColor(BackgroundColor);

                case StylePropertyId.BorderColor:
                    return StyleProperty.BorderColor(BorderColor);

                case StylePropertyId.BackgroundImage:
                    return StyleProperty.BackgroundImage(BackgroundImage);

                case StylePropertyId.BackgroundFillType:
                    return new StyleProperty(propertyId, (int) BackgroundFillType);

                case StylePropertyId.BackgroundGradientType:
                    return new StyleProperty(propertyId, (int) BackgroundGradientType);

                case StylePropertyId.BackgroundGradientAxis:
                    return new StyleProperty(propertyId, (int) BackgroundGradientAxis);

                case StylePropertyId.BackgroundGradientStart:
                    return new StyleProperty(propertyId, BackgroundGradientStart);

                case StylePropertyId.BackgroundGridSize:
                    return new StyleProperty(propertyId, BackgroundGridSize);

                case StylePropertyId.BackgroundLineSize:
                    return new StyleProperty(propertyId, BackgroundLineSize);

                case StylePropertyId.BackgroundFillOffsetX:
                    return new StyleProperty(propertyId, BackgroundFillOffsetX);

                case StylePropertyId.BackgroundFillOffsetY:
                    return new StyleProperty(propertyId, BackgroundFillOffsetY);

                case StylePropertyId.BackgroundFillScaleX:
                    return new StyleProperty(propertyId, BackgroundFillScaleX);

                case StylePropertyId.BackgroundFillScaleY:
                    return new StyleProperty(propertyId, BackgroundFillScaleY);

                case StylePropertyId.BackgroundFillRotation:
                    return new StyleProperty(propertyId, BackgroundFillRotation);

                case StylePropertyId.BackgroundImageTileX:
                    return new StyleProperty(propertyId, BackgroundImageTileX);

                case StylePropertyId.BackgroundImageTileY:
                    return new StyleProperty(propertyId, BackgroundImageTileY);

                case StylePropertyId.BackgroundImageOffsetX:
                    return new StyleProperty(propertyId, BackgroundImageOffsetX);

                case StylePropertyId.BackgroundImageOffsetY:
                    return new StyleProperty(propertyId, BackgroundImageOffsetY);

                case StylePropertyId.BackgroundImage1:
                    return new StyleProperty(propertyId, 0, 0, BackgroundImage1);

                case StylePropertyId.BackgroundImage2:
                    return new StyleProperty(propertyId, 0, 0, BackgroundImage2);

                case StylePropertyId.BackgroundColorSecondary:
                    return new StyleProperty(propertyId, BackgroundColorSecondary);

                case StylePropertyId.BackgroundShapeType:
                    return new StyleProperty(propertyId, (int) BackgroundShapeType);

                case StylePropertyId.Opacity:
                    return new StyleProperty(propertyId, Opacity);

                case StylePropertyId.Cursor:
                    return new StyleProperty(propertyId, 0, 0, Cursor);

                case StylePropertyId.GridItemColStart:
                    return new StyleProperty(propertyId, GridItemColStart);

                case StylePropertyId.GridItemColSpan:
                    return new StyleProperty(propertyId, GridItemColSpan);

                case StylePropertyId.GridItemRowStart:
                    return new StyleProperty(propertyId, GridItemRowStart);

                case StylePropertyId.GridItemRowSpan:
                    return new StyleProperty(propertyId, GridItemRowSpan);

                case StylePropertyId.GridItemColSelfAlignment:
                    return new StyleProperty(propertyId, GridItemColSelfAlignment);

                case StylePropertyId.GridItemRowSelfAlignment:
                    return new StyleProperty(propertyId, GridItemRowSelfAlignment);

                case StylePropertyId.GridLayoutDirection:
                    return new StyleProperty(propertyId, GridLayoutDirection);

                case StylePropertyId.GridLayoutDensity:
                    return new StyleProperty(propertyId, (int) GridLayoutDensity);

                case StylePropertyId.GridLayoutColTemplate:
                    return new StyleProperty(propertyId, 0, 0, GridLayoutColTemplate);

                case StylePropertyId.GridLayoutRowTemplate:
                    return new StyleProperty(propertyId, 0, 0, GridLayoutRowTemplate);

                case StylePropertyId.GridLayoutColAutoSize:
                    return new StyleProperty(propertyId, GridLayoutColAutoSize);

                case StylePropertyId.GridLayoutRowAutoSize:
                    return new StyleProperty(propertyId, GridLayoutRowAutoSize);

                case StylePropertyId.GridLayoutColGap:
                    return new StyleProperty(propertyId, GridLayoutColGap);

                case StylePropertyId.GridLayoutRowGap:
                    return new StyleProperty(propertyId, GridLayoutRowGap);

                case StylePropertyId.GridLayoutColAlignment:
                    return new StyleProperty(propertyId, GridLayoutColAlignment);

                case StylePropertyId.GridLayoutRowAlignment:
                    return new StyleProperty(propertyId, GridLayoutRowAlignment);

                case StylePropertyId.FlexLayoutWrap:
                    return new StyleProperty(propertyId, (int) FlexLayoutWrap);

                case StylePropertyId.FlexLayoutDirection:
                    return new StyleProperty(propertyId, FlexLayoutDirection);

                case StylePropertyId.FlexLayoutMainAxisAlignment:
                    return new StyleProperty(propertyId, FlexLayoutMainAxisAlignment);

                case StylePropertyId.FlexLayoutCrossAxisAlignment:
                    return new StyleProperty(propertyId, FlexLayoutCrossAxisAlignment);

                case StylePropertyId.FlexItemSelfAlignment:
                    return new StyleProperty(propertyId, FlexItemSelfAlignment);

                case StylePropertyId.FlexItemOrder:
                    return new StyleProperty(propertyId, FlexItemOrder);

                case StylePropertyId.FlexItemGrow:
                    return new StyleProperty(propertyId, FlexItemGrow);

                case StylePropertyId.FlexItemShrink:
                    return new StyleProperty(propertyId, FlexItemShrink);

                case StylePropertyId.MarginTop:
                    return new StyleProperty(propertyId, MarginTop);

                case StylePropertyId.MarginRight:
                    return new StyleProperty(propertyId, MarginRight);

                case StylePropertyId.MarginBottom:
                    return new StyleProperty(propertyId, MarginBottom);

                case StylePropertyId.MarginLeft:
                    return new StyleProperty(propertyId, MarginLeft);

                case StylePropertyId.BorderTop:
                    return new StyleProperty(propertyId, BorderTop);

                case StylePropertyId.BorderRight:
                    return new StyleProperty(propertyId, BorderRight);

                case StylePropertyId.BorderBottom:
                    return new StyleProperty(propertyId, BorderBottom);

                case StylePropertyId.BorderLeft:
                    return new StyleProperty(propertyId, BorderLeft);

                case StylePropertyId.PaddingTop:
                    return new StyleProperty(propertyId, PaddingTop);

                case StylePropertyId.PaddingRight:
                    return new StyleProperty(propertyId, PaddingRight);

                case StylePropertyId.PaddingBottom:
                    return new StyleProperty(propertyId, PaddingBottom);

                case StylePropertyId.PaddingLeft:
                    return new StyleProperty(propertyId, PaddingLeft);

                case StylePropertyId.BorderRadiusTopLeft:
                    return new StyleProperty(propertyId, BorderRadiusTopLeft);

                case StylePropertyId.BorderRadiusTopRight:
                    return new StyleProperty(propertyId, BorderRadiusTopRight);

                case StylePropertyId.BorderRadiusBottomLeft:
                    return new StyleProperty(propertyId, BorderRadiusBottomLeft);

                case StylePropertyId.BorderRadiusBottomRight:
                    return new StyleProperty(propertyId, BorderRadiusBottomRight);

                case StylePropertyId.TransformPositionX:
                    return new StyleProperty(propertyId, TransformPositionX);

                case StylePropertyId.TransformPositionY:
                    return new StyleProperty(propertyId, TransformPositionY);

                case StylePropertyId.TransformScaleX:
                    return new StyleProperty(propertyId, TransformScaleX);

                case StylePropertyId.TransformScaleY:
                    return new StyleProperty(propertyId, TransformScaleY);

                case StylePropertyId.TransformPivotX:
                    return new StyleProperty(propertyId, TransformPivotX);

                case StylePropertyId.TransformPivotY:
                    return new StyleProperty(propertyId, TransformPivotY);

                case StylePropertyId.TransformRotation:
                    return new StyleProperty(propertyId, TransformRotation);

                case StylePropertyId.TransformBehaviorX:
                    return new StyleProperty(propertyId, (int) TransformBehaviorX);

                case StylePropertyId.TransformBehaviorY:
                    return new StyleProperty(propertyId, (int) TransformBehaviorY);
   
                case StylePropertyId.TextColor:
                    return new StyleProperty(propertyId, TextColor);

                case StylePropertyId.TextFontAsset:
                    return new StyleProperty(propertyId, 0, 0, TextFontAsset);

                case StylePropertyId.TextFontSize:
                    return new StyleProperty(propertyId, TextFontSize);

                case StylePropertyId.TextFontStyle:
                    return new StyleProperty(propertyId, (int) TextFontStyle);

                case StylePropertyId.TextAlignment:
                    return new StyleProperty(propertyId, (int) TextAnchor);

                case StylePropertyId.TextWhitespaceMode:
                    return new StyleProperty(propertyId, (int) TextWhitespaceMode);

                case StylePropertyId.TextTransform:
                    return new StyleProperty(propertyId, (int) TextTransform);
                
                case StylePropertyId.__TextPropertyStart__:
                case StylePropertyId.__TextPropertyEnd__:
                    break;
                
                case StylePropertyId.MinWidth:
                    return new StyleProperty(propertyId, MinWidth);

                case StylePropertyId.MaxWidth:
                    return new StyleProperty(propertyId, MaxWidth);

                case StylePropertyId.PreferredWidth:
                    return new StyleProperty(propertyId, PreferredWidth);

                case StylePropertyId.MinHeight:
                    return new StyleProperty(propertyId, MinHeight);

                case StylePropertyId.MaxHeight:
                    return new StyleProperty(propertyId, MaxHeight);

                case StylePropertyId.PreferredHeight:
                    return new StyleProperty(propertyId, PreferredHeight);

                case StylePropertyId.LayoutType:
                    return new StyleProperty(propertyId, (int) LayoutType);

                case StylePropertyId.LayoutBehavior:
                    return new StyleProperty(propertyId, (int) LayoutBehavior);

                case StylePropertyId.AnchorTop:
                    return new StyleProperty(propertyId, AnchorTop);

                case StylePropertyId.AnchorRight:
                    return new StyleProperty(propertyId, AnchorRight);

                case StylePropertyId.AnchorBottom:
                    return new StyleProperty(propertyId, AnchorBottom);

                case StylePropertyId.AnchorLeft:
                    return new StyleProperty(propertyId, AnchorLeft);

                case StylePropertyId.AnchorTarget:
                    return new StyleProperty(propertyId, (int) AnchorTarget);

                case StylePropertyId.ZIndex:
                    return new StyleProperty(propertyId, ZIndex);

                case StylePropertyId.RenderLayer:
                    return new StyleProperty(propertyId, (int) RenderLayer);

                case StylePropertyId.RenderLayerOffset:
                    return new StyleProperty(propertyId, RenderLayerOffset);

                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyId), propertyId, null);
            }

            throw new ArgumentOutOfRangeException(nameof(propertyId), propertyId, null);
        }

    }

}