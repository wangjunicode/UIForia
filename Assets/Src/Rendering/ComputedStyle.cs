using System;
using System.Collections.Generic;
using System.Diagnostics;
using Shapes2D;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Text;
using TMPro;
using UnityEngine;
using FontStyle = Src.Text.FontStyle;
using TextAlignment = Src.Text.TextAlignment;

namespace Src.Rendering {

    // todo -- have some way of seeing if any properties changed in a given frame
    public partial class ComputedStyle {

        private readonly UIStyleSet styleSet;
        private readonly IntMap<StyleProperty> properties;

        public ComputedStyle(UIStyleSet styleSet) {
            this.styleSet = styleSet;
            this.properties = new IntMap<StyleProperty>();
        }

        public FixedLengthRect border => new FixedLengthRect(BorderTop, BorderRight, BorderBottom, BorderLeft);
        public ContentBoxRect margin => new ContentBoxRect(MarginTop, MarginRight, MarginBottom, MarginLeft);

        public FixedLengthRect padding => new FixedLengthRect(PaddingTop, PaddingRight, PaddingBottom, PaddingLeft);

        public float EmSize = 16f;

        public FixedLengthVector TransformPosition => new FixedLengthVector(TransformPositionX, TransformPositionY);
        
        public bool HasBorderRadius =>
            BorderRadiusTopLeft.value > 0 ||
            BorderRadiusBottomLeft.value > 0 ||
            BorderRadiusTopRight.value > 0 ||
            BorderRadiusBottomLeft.value > 0;


        private void SendEvent(StyleProperty property) {
            styleSet.styleSystem.SetStyleProperty(styleSet.element, property);
        }

        internal void SetProperty3(StyleProperty property) {
            int value0 = property.valuePart0;
            int value1 = property.valuePart1;
            switch (property.propertyId) {
                case StylePropertyId.Cursor:
                case StylePropertyId.Opacity:
                    throw new NotImplementedException();

#region  Layout

                case StylePropertyId.LayoutBehavior:
                    LayoutBehavior = property.IsDefined ? (LayoutBehavior) property.valuePart0 : DefaultStyleValues.LayoutBehavior;
                    break;

                case StylePropertyId.LayoutType:
                    LayoutType = property.IsDefined ? (LayoutType) value0 : DefaultStyleValues.LayoutType;
                    break;

#endregion

#region Overflow

                case StylePropertyId.OverflowX:
                    OverflowX = property.IsDefined ? (Overflow) value0 : DefaultStyleValues.OverflowX;
                    break;
                case StylePropertyId.OverflowY:
                    OverflowY = property.IsDefined ? (Overflow) value0 : DefaultStyleValues.OverflowY;
                    break;

#endregion

#region Paint

                case StylePropertyId.BackgroundColor:
                    BackgroundColor = property.IsDefined ? (Color) new StyleColor(value0) : DefaultStyleValues.BackgroundColor;
                    break;
                case StylePropertyId.BorderColor:
                    BorderColor = property.IsDefined ? (Color) new StyleColor(value0) : DefaultStyleValues.BorderColor;
                    break;
                case StylePropertyId.BackgroundImage:
                    BackgroundImage = property.IsDefined ? property.AsTexture : DefaultStyleValues.BackgroundImage;
                    break;
                case StylePropertyId.BorderRadiusTopLeft:
                    BorderRadiusTopLeft = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderRadiusTopLeft;
                    break;
                case StylePropertyId.BorderRadiusTopRight:
                    BorderRadiusTopRight = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderRadiusTopRight;
                    break;
                case StylePropertyId.BorderRadiusBottomLeft:
                    BorderRadiusBottomLeft = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderRadiusBottomLeft;
                    break;
                case StylePropertyId.BorderRadiusBottomRight:
                    BorderRadiusBottomRight = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderRadiusBottomRight;
                    break;

                case StylePropertyId.BackgroundFillOffsetX:
                    BackgroundFillOffsetX = property.IsDefined ? property.AsFloat : DefaultStyleValues.BackgroundFillOffsetX;
                    break;
                case StylePropertyId.BackgroundFillOffsetY:
                    BackgroundFillOffsetY = property.IsDefined ? property.AsFloat : DefaultStyleValues.BackgroundFillOffsetY;
                    break;
                case StylePropertyId.BackgroundFillScaleX:
                    BackgroundFillScaleX = property.IsDefined ? property.AsFloat : DefaultStyleValues.BackgroundFillScaleX;
                    break;
                case StylePropertyId.BackgroundFillScaleY:
                    BackgroundFillScaleY = property.IsDefined ? property.AsFloat : DefaultStyleValues.BackgroundFillScaleX;
                    break;

#endregion

#region Grid Item

                case StylePropertyId.GridItemColStart:
                    GridItemColStart = property.IsDefined ? value0 : DefaultStyleValues.GridItemColStart;
                    break;
                case StylePropertyId.GridItemColSpan:
                    GridItemColSpan = property.IsDefined ? value0 : DefaultStyleValues.GridItemColSpan;
                    break;
                case StylePropertyId.GridItemRowStart:
                    GridItemRowStart = property.IsDefined ? value0 : DefaultStyleValues.GridItemRowStart;
                    break;
                case StylePropertyId.GridItemRowSpan:
                    GridItemRowSpan = property.IsDefined ? value0 : DefaultStyleValues.GridItemRowSpan;
                    break;
                case StylePropertyId.GridItemColSelfAlignment:
                    GridItemColSelfAlignment = property.IsDefined ? property.AsCrossAxisAlignment : DefaultStyleValues.GridItemColSelfAlignment;
                    break;
                case StylePropertyId.GridItemRowSelfAlignment:
                    GridItemRowSelfAlignment = property.IsDefined ? property.AsCrossAxisAlignment : DefaultStyleValues.GridItemRowSelfAlignment;
                    break;

#endregion

#region Grid Layout

                case StylePropertyId.GridLayoutDirection:
                    GridLayoutDirection = property.IsDefined ? (LayoutDirection) value0 : DefaultStyleValues.GridLayoutDirection;
                    break;
                case StylePropertyId.GridLayoutDensity:
                    GridLayoutDensity = property.IsDefined ? (GridLayoutDensity) value0 : DefaultStyleValues.GridLayoutDensity;
                    break;
                case StylePropertyId.GridLayoutColTemplate:
                    GridLayoutColTemplate = property.IsDefined ? property.AsGridTrackTemplate : DefaultStyleValues.GridLayoutColTemplate;
                    break;
                case StylePropertyId.GridLayoutRowTemplate:
                    GridLayoutRowTemplate = property.IsDefined ? property.AsGridTrackTemplate : DefaultStyleValues.GridLayoutRowTemplate;
                    break;
                case StylePropertyId.GridLayoutColAutoSize:
                    GridLayoutColAutoSize = property.IsDefined ? property.AsGridTrackSize : DefaultStyleValues.GridLayoutColAutoSize;
                    break;
                case StylePropertyId.GridLayoutRowAutoSize:
                    GridLayoutRowAutoSize = property.IsDefined ? property.AsGridTrackSize : DefaultStyleValues.GridLayoutRowAutoSize;
                    break;
                case StylePropertyId.GridLayoutColGap:
                    GridLayoutColGap = property.IsDefined ? property.AsFloat : DefaultStyleValues.GridLayoutColGap;
                    break;
                case StylePropertyId.GridLayoutRowGap:
                    GridLayoutRowGap = property.IsDefined ? property.AsFloat : DefaultStyleValues.GridLayoutRowGap;
                    break;
                case StylePropertyId.GridLayoutColAlignment:
                    GridLayoutColAlignment = property.IsDefined ? property.AsCrossAxisAlignment : DefaultStyleValues.GridLayoutColAlignment;
                    break;
                case StylePropertyId.GridLayoutRowAlignment:
                    GridLayoutRowAlignment = property.IsDefined ? property.AsCrossAxisAlignment : DefaultStyleValues.GridLayoutRowAlignment;
                    break;

#endregion

#region Flex Layout

                case StylePropertyId.FlexLayoutWrap:
                    FlexLayoutWrap = property.IsDefined ? (LayoutWrap) value0 : DefaultStyleValues.FlexLayoutWrap;
                    break;
                case StylePropertyId.FlexLayoutDirection:
                    FlexLayoutDirection = property.IsDefined ? (LayoutDirection) value0 : DefaultStyleValues.FlexLayoutDirection;
                    break;
                case StylePropertyId.FlexLayoutMainAxisAlignment:
                    FlexLayoutMainAxisAlignment = property.IsDefined ? (MainAxisAlignment) value0 : DefaultStyleValues.FlexLayoutMainAxisAlignment;
                    break;
                case StylePropertyId.FlexLayoutCrossAxisAlignment:
                    FlexLayoutCrossAxisAlignment = property.IsDefined ? (CrossAxisAlignment) value0 : DefaultStyleValues.FlexLayoutCrossAxisAlignment;
                    break;

#endregion

#region Flex Item

                case StylePropertyId.FlexItemSelfAlignment:
                    FlexItemSelfAlignment = property.IsDefined ? (CrossAxisAlignment) value0 : DefaultStyleValues.FlexItemSelfAlignment;
                    break;
                case StylePropertyId.FlexItemOrder:
                    FlexItemOrder = property.IsDefined ? value0 : DefaultStyleValues.FlexItemOrder;
                    break;
                case StylePropertyId.FlexItemGrow:
                    FlexItemGrow = property.IsDefined ? value0 : DefaultStyleValues.FlexItemGrow;
                    break;
                case StylePropertyId.FlexItemShrink:
                    FlexItemShrink = property.IsDefined ? value0 : DefaultStyleValues.FlexItemShrink;
                    break;

#endregion

#region Margin

                case StylePropertyId.MarginTop:
                    MarginTop = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MarginTop;
                    break;

                case StylePropertyId.MarginRight:
                    MarginRight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MarginRight;
                    break;

                case StylePropertyId.MarginBottom:
                    MarginBottom = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MarginBottom;
                    break;

                case StylePropertyId.MarginLeft:
                    MarginLeft = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MarginLeft;
                    break;

#endregion

#region Border

                case StylePropertyId.BorderTop:
                    BorderTop = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderTop;
                    break;
                case StylePropertyId.BorderRight:
                    BorderRight = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderRight;
                    break;
                case StylePropertyId.BorderBottom:
                    BorderBottom = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderBottom;
                    break;
                case StylePropertyId.BorderLeft:
                    BorderLeft = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.BorderLeft;
                    break;

#endregion

#region Padding

                case StylePropertyId.PaddingTop:
                    PaddingTop = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.PaddingTop;
                    break;
                case StylePropertyId.PaddingRight:
                    PaddingRight = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.PaddingRight;
                    break;
                case StylePropertyId.PaddingBottom:
                    PaddingBottom = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.PaddingBottom;
                    break;
                case StylePropertyId.PaddingLeft:
                    PaddingLeft = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.PaddingLeft;
                    break;

#endregion

#region Transform

                case StylePropertyId.TransformPositionX:
                    TransformPositionX = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.TransformPositionX;
                    break;
                case StylePropertyId.TransformPositionY:
                    TransformPositionY = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.TransformPositionY;
                    break;
                case StylePropertyId.TransformScaleX:
                    TransformScaleX = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : DefaultStyleValues.TransformScaleX;
                    break;
                case StylePropertyId.TransformScaleY:
                    TransformScaleY = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : DefaultStyleValues.TransformScaleY;
                    break;
                case StylePropertyId.TransformPivotX:
                    TransformPivotX = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.TransformPivotX;
                    break;
                case StylePropertyId.TransformPivotY:
                    TransformPivotY = property.IsDefined ? UIFixedLength.Decode(value0, value1) : DefaultStyleValues.TransformPivotY;
                    break;
                case StylePropertyId.TransformRotation:
                    TransformRotation = property.IsDefined ? FloatUtil.DecodeToFloat(value0) : DefaultStyleValues.TransformRotation;
                    break;
                case StylePropertyId.TransformBehaviorX:
                    TransformBehaviorX = property.IsDefined ? (TransformBehavior) value0 : DefaultStyleValues.TransformBehaviorX;
                    break;
                case StylePropertyId.TransformBehaviorY:
                    TransformBehaviorY = property.IsDefined ? (TransformBehavior) value0 : DefaultStyleValues.TransformBehaviorY;
                    break;

#endregion

#region Text

                case StylePropertyId.TextColor:
                    TextColor = property.IsDefined ? (Color) new StyleColor(value0) : DefaultStyleValues.TextColor;
                    break;
                case StylePropertyId.TextFontAsset:
                    TextFontAsset = property.IsDefined ? property.AsFont : DefaultStyleValues.TextFontAsset;
                    break;
                case StylePropertyId.TextFontSize:
                    TextFontSize = property.IsDefined ? value0 : DefaultStyleValues.TextFontSize;
                    break;
                case StylePropertyId.TextFontStyle:
                    TextFontStyle = property.IsDefined ? (FontStyle) value0 : DefaultStyleValues.TextFontStyle;
                    break;
                case StylePropertyId.TextAlignment:
                    TextAlignment = property.IsDefined ? (TextAlignment) value0 : DefaultStyleValues.TextAlignment;
                    break;
                case StylePropertyId.TextTransform:
                    TextTransform = property.IsDefined ? (TextTransform) value0 : DefaultStyleValues.TextTransform;
                    break;
                case StylePropertyId.TextWhitespaceMode:
                    throw new NotImplementedException();

#endregion

#region Size

                case StylePropertyId.MinWidth:
                    MinWidth = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MinWidth;
                    break;

                case StylePropertyId.MaxWidth:
                    MaxWidth = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MaxWidth;
                    break;

                case StylePropertyId.PreferredWidth:
                    PreferredWidth = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.PreferredWidth;
                    break;

                case StylePropertyId.MinHeight:
                    MinHeight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MinHeight;
                    break;

                case StylePropertyId.MaxHeight:
                    MaxHeight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.MaxHeight;
                    break;

                case StylePropertyId.PreferredHeight:
                    PreferredHeight = property.IsDefined ? UIMeasurement.Decode(value0, value1) : DefaultStyleValues.PreferredHeight;
                    break;

#endregion

#region Layer

                case StylePropertyId.ZIndex:
                    ZIndex = property.IsDefined ? property.AsInt : DefaultStyleValues.ZIndex;
                    break;
                case StylePropertyId.RenderLayerOffset:
                    RenderLayerOffset = property.IsDefined ? property.AsInt : DefaultStyleValues.RenderLayerOffset;
                    break;
                case StylePropertyId.RenderLayer:
                    RenderLayer = property.IsDefined ? property.AsRenderLayer : DefaultStyleValues.RenderLayer;
                    break;

#endregion

#region  Anchors

                case StylePropertyId.AnchorTarget:
                    AnchorTarget = property.IsDefined ? property.AsAnchorTarget : DefaultStyleValues.AnchorTarget;
                    break;
                case StylePropertyId.AnchorTop:
                    AnchorTop = property.IsDefined ? property.AsUIFixedLength : DefaultStyleValues.AnchorTop;
                    break;
                case StylePropertyId.AnchorRight:
                    AnchorRight = property.IsDefined ? property.AsUIFixedLength : DefaultStyleValues.AnchorRight;
                    break;
                case StylePropertyId.AnchorBottom:
                    AnchorBottom = property.IsDefined ? property.AsUIFixedLength : DefaultStyleValues.AnchorBottom;
                    break;
                case StylePropertyId.AnchorLeft:
                    AnchorLeft = property.IsDefined ? property.AsUIFixedLength : DefaultStyleValues.AnchorLeft;
                    break;

#endregion

                case StylePropertyId.__TextPropertyStart__:
                case StylePropertyId.__TextPropertyEnd__:
                default:
                    throw new ArgumentOutOfRangeException(nameof(property.propertyId), property.propertyId, null);
            }
        }

         public BorderRadius BorderRadius => new BorderRadius(BorderRadiusTopLeft, BorderRadiusTopRight, BorderRadiusBottomRight, BorderRadiusBottomLeft);

        public Vector4 ResolvedBorderRadius => new Vector4(
            ResolveHorizontalFixedLength(BorderRadiusTopLeft),
            ResolveHorizontalFixedLength(BorderRadiusTopRight),
            ResolveHorizontalFixedLength(BorderRadiusBottomRight),
            ResolveHorizontalFixedLength(BorderRadiusBottomLeft)
        );

        public Vector4 ResolvedBorder => new Vector4(
            ResolveVerticalFixedLength(BorderTop),
            ResolveHorizontalFixedLength(BorderRight),
            ResolveVerticalFixedLength(BorderBottom),
            ResolveHorizontalFixedLength(BorderLeft)
        );

        // I don't love having this here
        private float ResolveHorizontalFixedLength(UIFixedLength length) {
            switch (length.unit) {
                case UIFixedUnit.Pixel:
                    return length.value;

                case UIFixedUnit.Percent:
                    return styleSet.element.layoutResult.AllocatedWidth * length.value;

                case UIFixedUnit.Em:
                    return EmSize * length.value;

                case UIFixedUnit.ViewportWidth:
                    return 0;

                case UIFixedUnit.ViewportHeight:
                    return 0;

                default:
                    return 0;
            }
        }

        // I don't love having this here
        private float ResolveVerticalFixedLength(UIFixedLength length) {
            switch (length.unit) {
                case UIFixedUnit.Pixel:
                    return length.value;

                case UIFixedUnit.Percent:
                    return styleSet.element.layoutResult.AllocatedHeight * length.value;

                case UIFixedUnit.Em:
                    return EmSize * length.value;

                case UIFixedUnit.ViewportWidth:
                    return 0;

                case UIFixedUnit.ViewportHeight:
                    return 0;

                default:
                    return 0;
            }
        }
        public bool IsDefined(StylePropertyId propertyId) {
            return properties.ContainsKey((int) propertyId);
        }

        public StyleProperty GetProperty(StylePropertyId propertyId) {
            StyleProperty property;

            if (properties.TryGetValue((int) propertyId, out property)) {
                return property;
            }

            return DefaultStyleValues.GetPropertyValue(propertyId);
        }

        [DebuggerStepThrough]
        private UIFixedLength ReadFixedLengthProperty(StylePropertyId propertyId, UIFixedLength defaultValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                return retn.AsUIFixedLength;
            }

            return defaultValue;
        }

        [DebuggerStepThrough]
        private UIMeasurement ReadMeasurementProperty(StylePropertyId propertyId, UIMeasurement defaultValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                return retn.AsUIMeasurement;
            }

            return defaultValue;
        }

        [DebuggerStepThrough]
        private void WriteMeasurementProperty(StylePropertyId propertyId, UIMeasurement newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                if (retn.AsUIMeasurement == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, newValue);
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

        [DebuggerStepThrough]
        private void WriteFixedLengthProperty(StylePropertyId propertyId, UIFixedLength newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                if (retn.AsInt == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, newValue);
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

        [DebuggerStepThrough]
        private int ReadIntProperty(StylePropertyId propertyId, int defaultValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                return retn.AsInt;
            }

            return defaultValue;
        }

        private GridTrackSize ReadGridTrackSizeProperty(StylePropertyId propertyId, GridTrackSize defaultValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                return retn.AsGridTrackSize;
            }

            return defaultValue;
        }

        private Color ReadColorProperty(StylePropertyId propertyId, Color defaultValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                return retn.AsColor;
            }

            return defaultValue;
        }

        private object ReadObjectProperty(StylePropertyId propertyId, object defaultValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                return retn.objectField;
            }

            return defaultValue;
        }

        private void WriteObjectProperty(StylePropertyId propertyId, object newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) { // todo -- null?
                if (retn.objectField == newValue) {
                    return;
                }
            }

            StyleProperty property = new StyleProperty(propertyId, 0, 0, newValue);
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

        private void WriteGridTrackSizeProperty(StylePropertyId propertyId, GridTrackSize newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                if (retn.AsGridTrackSize == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, FloatUtil.EncodeToInt(newValue.minValue), (int) newValue.minUnit, null);
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

        private void WriteColorProperty(StylePropertyId propertyId, Color newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                if (retn.AsColor == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, newValue);
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

        private void WriteIntProperty(StylePropertyId propertyId, int newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                if (retn.AsInt == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, newValue);
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

        [DebuggerStepThrough]
        private float ReadFloatProperty(StylePropertyId propertyId, float defaultValue) {
            StyleProperty retn;
            return properties.TryGetValue((int) propertyId, out retn) ? retn.AsFloat : defaultValue;
        }

        private void WriteFloatProperty(StylePropertyId propertyId, float newValue) {
            StyleProperty retn;
            if (properties.TryGetValue((int) propertyId, out retn)) {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (retn.AsFloat == newValue) return;
            }

            StyleProperty property = new StyleProperty(propertyId, FloatUtil.EncodeToInt(newValue));
            properties[(int) propertyId] = property;
            SendEvent(property);
        }

    }

}