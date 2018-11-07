using System.Collections.Generic;
using System.Diagnostics;
using Shapes2D;
using TMPro;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Text;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Rendering {

    public struct StyleProperty {

        public readonly StylePropertyId propertyId;
        public readonly int valuePart0;
        public readonly int valuePart1;
        public readonly object objectField;

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, int value0, int value1 = 0, object objectField = null) {
            this.propertyId = propertyId;
            this.valuePart0 = value0;
            this.valuePart1 = value1;
            this.objectField = objectField;
        }

        public StyleProperty(StylePropertyId propertyId, Color color) {
            this.propertyId = propertyId;
            this.valuePart0 = new StyleColor(color).rgba;
            this.valuePart1 = 0;
            this.objectField = null;
        }

        public StyleProperty(StylePropertyId propertyId, Gradient gradient) {
            this.propertyId = propertyId;
            this.valuePart0 = 0;
            this.valuePart1 = (int) ColorType.Gradient;
            this.objectField = gradient;
        }

        public StyleProperty(StylePropertyId propertyId, UIFixedLength length) {
            this.propertyId = propertyId;
            this.valuePart0 = FloatUtil.EncodeToInt(length.value);
            this.valuePart1 = (int) length.unit;
            this.objectField = null;
        }

        public StyleProperty(StylePropertyId propertyId, UIMeasurement measurement) {
            this.propertyId = propertyId;
            this.valuePart0 = FloatUtil.EncodeToInt(measurement.value);
            this.valuePart1 = (int) measurement.unit;
            this.objectField = null;
        }

        public StyleProperty(StylePropertyId propertyId, GridTrackSize trackSize) {
            this.propertyId = propertyId;
            this.valuePart0 = FloatUtil.EncodeToInt(trackSize.minValue);
            this.valuePart1 = (int) trackSize.minUnit;
            this.objectField = null;
        }

        public StyleProperty(StylePropertyId propertyId, float floatValue) {
            this.propertyId = propertyId;
            this.valuePart0 = FloatUtil.EncodeToInt(floatValue);
            this.valuePart1 = 0;
            this.objectField = null;
        }

        public StyleProperty(StylePropertyId propertyId, int intValue) {
            this.propertyId = propertyId;
            this.valuePart0 = intValue;
            this.valuePart1 = 0;
            this.objectField = null;
        }

        public StyleProperty(StylePropertyId propertyId, CrossAxisAlignment alignment) {
            this.propertyId = propertyId;
            this.valuePart0 = (int) alignment;
            this.valuePart1 = 0;
            this.objectField = null;
        }

        public StyleProperty(StylePropertyId propertyId, MainAxisAlignment alignment) {
            this.propertyId = propertyId;
            this.valuePart0 = (int) alignment;
            this.valuePart1 = 0;
            this.objectField = null;
        }

        public StyleProperty(StylePropertyId propertyId, LayoutDirection direction) {
            this.propertyId = propertyId;
            this.valuePart0 = (int) direction;
            this.valuePart1 = 0;
            this.objectField = null;
        }

        public bool IsDefined {
            [DebuggerStepThrough] get { return IntUtil.IsDefined(valuePart0) && IntUtil.IsDefined(valuePart1); }
        }

        public int AsInt => valuePart0;
        public float AsFloat => FloatUtil.DecodeToFloat(valuePart0);
        public UIMeasurement AsUIMeasurement => UIMeasurement.Decode(valuePart0, valuePart1);
        public CrossAxisAlignment AsCrossAxisAlignment => (CrossAxisAlignment) valuePart0;
        public MainAxisAlignment AsMainAxisAlignment => (MainAxisAlignment) valuePart0;
        public Overflow AsOverflow => (Overflow) valuePart0;
        public Color AsColor => new StyleColor(valuePart0);

        public TMP_FontAsset AsFont => (TMP_FontAsset) objectField;
        public Texture2D AsTexture => (Texture2D) objectField;

        public Text.FontStyle AsFontStyle => (Text.FontStyle) valuePart0;
        public Text.TextAlignment AsTextAlignment => (Text.TextAlignment) valuePart0;
        public LayoutDirection AsLayoutDirection => (LayoutDirection) valuePart0;
        public LayoutWrap AsLayoutWrap => (LayoutWrap) valuePart0;
        public GridTrackSize AsGridTrackSize => new GridTrackSize(FloatUtil.DecodeToFloat(valuePart0), (GridTemplateUnit) valuePart1);

        public IReadOnlyList<GridTrackSize> AsGridTrackTemplate => (IReadOnlyList<GridTrackSize>) objectField;
        public UIFixedLength AsUIFixedLength => new UIFixedLength(FloatUtil.DecodeToFloat(valuePart0), (UIFixedUnit) valuePart1);

        public bool IsGradient => objectField != null && (ColorType) valuePart1 == ColorType.Gradient;
        public AnchorTarget AsAnchorTarget => (AnchorTarget) valuePart0;
        public RenderLayer AsRenderLayer => (RenderLayer) valuePart0;
        public Texture2D AsTexture2D => (Texture2D) objectField;
        public GradientType AsGradientType => (GradientType) valuePart0;
        public GradientAxis AsGradientAxis => (GradientAxis) valuePart0;
        public BackgroundFillType AsBackgroundFillType => (BackgroundFillType) valuePart0;
        public BackgroundShapeType AsBackgroundShapeType => (BackgroundShapeType) valuePart0;
        public GridLayoutDensity AsGridLayoutDensity => (GridLayoutDensity) valuePart0;
        public TransformBehavior AsTransformBehavior => (TransformBehavior) valuePart0;
        public LayoutType AsLayoutType => (LayoutType) valuePart0;
        public TextTransform AsTextTransform => (TextTransform) valuePart0;
        public LayoutBehavior AsLayoutBehavior => (LayoutBehavior) valuePart0;
        
        public IReadOnlyList<GridTrackSize> AsGridTemplate => (IReadOnlyList<GridTrackSize>)objectField;

        [DebuggerStepThrough]
        public static StyleProperty Unset(StylePropertyId propertyId) {
            return new StyleProperty(propertyId, IntUtil.UnsetValue, IntUtil.UnsetValue);
        }

        public static StyleProperty TransformPositionX(UIFixedLength length) {
            return new StyleProperty(StylePropertyId.TransformPositionX, length);
        }

        public static StyleProperty TransformPositionY(UIFixedLength length) {
            return new StyleProperty(StylePropertyId.TransformPositionY, length);
        }

        public static StyleProperty TransformPivotX(UIFixedLength length) {
            return new StyleProperty(StylePropertyId.TransformPivotX, length);
        }

        public static StyleProperty TransformPivotY(UIFixedLength length) {
            return new StyleProperty(StylePropertyId.TransformPivotY, length);
        }

        public static StyleProperty TransformScaleX(float scaleX) {
            return new StyleProperty(StylePropertyId.TransformScaleX, scaleX);
        }

        public static StyleProperty TransformScaleY(float scaleY) {
            return new StyleProperty(StylePropertyId.TransformScaleY, scaleY);
        }

        public static StyleProperty TransformRotation(float rotation) {
            return new StyleProperty(StylePropertyId.TransformRotation, rotation);
        }

        public static StyleProperty BackgroundColor(Color color) {
            return new StyleProperty(StylePropertyId.BackgroundColor, color);
        }

        public static StyleProperty BackgroundColor(Gradient gradient) {
            return new StyleProperty(StylePropertyId.BackgroundColor, 0, (int) ColorType.Gradient, gradient);
        }

        public static StyleProperty BackgroundImage(Texture2D texture) {
            return new StyleProperty(StylePropertyId.BackgroundImage, 0, 0, texture);
        }

        public static StyleProperty BorderColor(Color color) {
            return new StyleProperty(StylePropertyId.BorderColor, color);
        }

        public static StyleProperty Opacity(float opacity) {
            return new StyleProperty(StylePropertyId.Opacity, opacity);
        }

        public static StyleProperty Cursor(Texture2D texture) {
            return new StyleProperty(StylePropertyId.Cursor, 0, 0, texture);
        }

        public static StyleProperty GridItemColStart(int colStart) {
            return new StyleProperty(StylePropertyId.GridItemColStart, colStart);
        }

        public static StyleProperty GridItemColSpan(int colSpan) {
            return new StyleProperty(StylePropertyId.GridItemColSpan, colSpan);
        }

        public static StyleProperty GridItemRowStart(int rowStart) {
            return new StyleProperty(StylePropertyId.GridItemRowStart, rowStart);
        }

        public static StyleProperty GridItemRowSpan(int rowSpan) {
            return new StyleProperty(StylePropertyId.GridItemRowSpan, rowSpan);
        }

        public static StyleProperty GridItemColSelfAlignment(CrossAxisAlignment alignment) {
            return new StyleProperty(StylePropertyId.GridItemColSelfAlignment, alignment);
        }

        public static StyleProperty GridItemRowSelfAlignment(CrossAxisAlignment alignment) {
            return new StyleProperty(StylePropertyId.GridItemRowSelfAlignment, alignment);
        }

        public static StyleProperty GridLayoutDensity(GridLayoutDensity density) {
            return new StyleProperty(StylePropertyId.GridLayoutDensity, (int) density);
        }

        public static StyleProperty GridLayoutColTemplate(IList<GridTrackSize> colTemplate) {
            return new StyleProperty(StylePropertyId.GridLayoutColTemplate, 0, 0, colTemplate);
        }

        public static StyleProperty GridLayoutRowTemplate(IList<GridTrackSize> rowTemplate) {
            return new StyleProperty(StylePropertyId.GridLayoutRowTemplate, 0, 0, rowTemplate);
        }

        public static StyleProperty GridLayoutDirection(LayoutDirection direction) {
            return new StyleProperty(StylePropertyId.GridLayoutDirection, direction);
        }

        public static StyleProperty GridLayoutColAutoSize(GridTrackSize autoColSize) {
            return new StyleProperty(StylePropertyId.GridLayoutColAutoSize, FloatUtil.EncodeToInt(autoColSize.minValue), (int) autoColSize.minUnit);
        }

        public static StyleProperty GridLayoutRowAutoSize(GridTrackSize autoRowSize) {
            return new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, FloatUtil.EncodeToInt(autoRowSize.minValue), (int) autoRowSize.minUnit);
        }

        public static StyleProperty GridLayoutColGap(float colGap) {
            return new StyleProperty(StylePropertyId.GridLayoutColGap, FloatUtil.EncodeToInt(colGap));
        }

        public static StyleProperty GridLayoutRowGap(float rowGap) {
            return new StyleProperty(StylePropertyId.GridLayoutRowGap, FloatUtil.EncodeToInt(rowGap));
        }

        public static StyleProperty GridLayoutColAlignment(CrossAxisAlignment alignment) {
            return new StyleProperty(StylePropertyId.GridLayoutColAlignment, alignment);
        }

        public static StyleProperty GridLayoutRowAlignment(CrossAxisAlignment alignment) {
            return new StyleProperty(StylePropertyId.GridLayoutRowAlignment, alignment);
        }

        public static StyleProperty MarginTop(UIMeasurement marginTop) {
            return new StyleProperty(StylePropertyId.MarginTop, marginTop);
        }

        public static StyleProperty MarginRight(UIMeasurement marginRight) {
            return new StyleProperty(StylePropertyId.MarginRight, marginRight);
        }

        public static StyleProperty MarginBottom(UIMeasurement marginBottom) {
            return new StyleProperty(StylePropertyId.MarginBottom, marginBottom);
        }

        public static StyleProperty MarginLeft(UIMeasurement marginLeft) {
            return new StyleProperty(StylePropertyId.MarginLeft, marginLeft);
        }

        public static StyleProperty BorderTop(UIFixedLength borderTop) {
            return new StyleProperty(StylePropertyId.BorderTop, borderTop);
        }

        public static StyleProperty BorderRight(UIFixedLength borderRight) {
            return new StyleProperty(StylePropertyId.BorderRight, borderRight);
        }

        public static StyleProperty BorderBottom(UIFixedLength borderBottom) {
            return new StyleProperty(StylePropertyId.BorderBottom, borderBottom);
        }

        public static StyleProperty BorderLeft(UIFixedLength borderLeft) {
            return new StyleProperty(StylePropertyId.BorderLeft, borderLeft);
        }

        public static StyleProperty PaddingTop(UIFixedLength paddingTop) {
            return new StyleProperty(StylePropertyId.PaddingTop, paddingTop);
        }

        public static StyleProperty PaddingRight(UIFixedLength paddingRight) {
            return new StyleProperty(StylePropertyId.PaddingRight, paddingRight);
        }

        public static StyleProperty PaddingBottom(UIFixedLength paddingBottom) {
            return new StyleProperty(StylePropertyId.PaddingBottom, paddingBottom);
        }

        public static StyleProperty PaddingLeft(UIFixedLength paddingLeft) {
            return new StyleProperty(StylePropertyId.PaddingLeft, paddingLeft);
        }

        public static StyleProperty BorderRadiusTopLeft(UIFixedLength topLeftRadius) {
            return new StyleProperty(StylePropertyId.BorderRadiusTopLeft, topLeftRadius);
        }

        public static StyleProperty BorderRadiusTopRight(UIFixedLength topRightRadius) {
            return new StyleProperty(StylePropertyId.BorderRadiusTopRight, topRightRadius);
        }

        public static StyleProperty BorderRadiusBottomLeft(UIFixedLength bottomLeftRadius) {
            return new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, bottomLeftRadius);
        }

        public static StyleProperty BorderRadiusBottomRight(UIFixedLength bottomRightRadius) {
            return new StyleProperty(StylePropertyId.BorderRadiusBottomRight, bottomRightRadius);
        }

        public static StyleProperty MinWidth(UIMeasurement minWidth) {
            return new StyleProperty(StylePropertyId.MinWidth, minWidth);
        }

        public static StyleProperty MaxWidth(UIMeasurement maxWidth) {
            return new StyleProperty(StylePropertyId.MaxWidth, maxWidth);
        }

        public static StyleProperty PreferredWidth(UIMeasurement preferredWidth) {
            return new StyleProperty(StylePropertyId.PreferredWidth, preferredWidth);
        }

        public static StyleProperty MinHeight(UIMeasurement minHeight) {
            return new StyleProperty(StylePropertyId.MinHeight, minHeight);
        }

        public static StyleProperty MaxHeight(UIMeasurement maxHeight) {
            return new StyleProperty(StylePropertyId.MaxHeight, maxHeight);
        }

        public static StyleProperty PreferredHeight(UIMeasurement preferredHeight) {
            return new StyleProperty(StylePropertyId.PreferredHeight, preferredHeight);
        }

        public static StyleProperty AnchorTop(UIFixedLength anchor) {
            return new StyleProperty(StylePropertyId.AnchorTop, FloatUtil.EncodeToInt(anchor.value), (int) anchor.unit);
        }

        public static StyleProperty AnchorRight(UIFixedLength anchor) {
            return new StyleProperty(StylePropertyId.AnchorRight, FloatUtil.EncodeToInt(anchor.value), (int) anchor.unit);
        }

        public static StyleProperty AnchorBottom(UIFixedLength anchor) {
            return new StyleProperty(StylePropertyId.AnchorBottom, FloatUtil.EncodeToInt(anchor.value), (int) anchor.unit);
        }

        public static StyleProperty AnchorLeft(UIFixedLength anchor) {
            return new StyleProperty(StylePropertyId.AnchorLeft, FloatUtil.EncodeToInt(anchor.value), (int) anchor.unit);
        }

        public static StyleProperty AnchorTarget(AnchorTarget anchorTarget) {
            return new StyleProperty(StylePropertyId.AnchorTarget, (int) anchorTarget);
        }

        public static StyleProperty ZIndex(int zIndex) {
            return new StyleProperty(StylePropertyId.ZIndex, zIndex);
        }

        public static StyleProperty LayerOffset(int layerOffset) {
            return new StyleProperty(StylePropertyId.RenderLayerOffset, layerOffset);
        }

        public static StyleProperty RenderLayer(RenderLayer renderLayer) {
            return new StyleProperty(StylePropertyId.RenderLayer, (int) renderLayer);
        }

        public static StyleProperty Font(TMP_FontAsset fontAsset) {
            return new StyleProperty(StylePropertyId.TextFontAsset, 0, 0, fontAsset);
        }

    }

}