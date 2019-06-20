using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Shapes2D;
using TMPro;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Rendering {

//    [StructLayout(LayoutKind.Explicit)]
    public partial struct StyleProperty {

        // element.style.SetGridRowTemplate(new GridRowTemplate(new [] {new Repeat(count/auto-fill/auto-fit, gridTemplate[])});
        // element.style.GridRowTemplate[0].size = xx;
        // GridRowStatic[elementId] => return struct
//        [FieldOffset(0)]
        public readonly StylePropertyId propertyId;
//        [FieldOffset(4)]
        public readonly int valuePart0;
//        [FieldOffset(8)]
        public readonly int valuePart1;
//        [FieldOffset(8)]
        public readonly float floatValue;
//        [FieldOffset(8)]
        public readonly object objectField;

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, int value0, int value1, float floatValue, object objectField) {
            this.propertyId = propertyId;
            this.floatValue = floatValue;
            this.valuePart0 = value0;
            this.valuePart1 = value1;
            this.objectField = objectField;
        }
        
        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, int value0, int value1 = 0, object objectField = null) {
            this.propertyId = propertyId;
            this.floatValue = 0;
            this.valuePart0 = value0;
            this.valuePart1 = value1;
            this.objectField = objectField;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, float value0, int value1 = 0, object objectField = null) {
            this.propertyId = propertyId;
            this.valuePart0 = 0;
            this.floatValue = value0;
            this.valuePart1 = value1;
            this.objectField = objectField;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, Color color) {
            this.floatValue = 0;
            this.propertyId = propertyId;
            this.valuePart0 = new StyleColor(color).rgba;
            this.valuePart1 = ColorUtil.IsDefined(color) ? 1 : 0;
            this.objectField = null;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, UIFixedLength length) {
            this.valuePart0 = 0;
            this.propertyId = propertyId;
            this.floatValue = length.value;
            this.valuePart1 = (int) length.unit;
            this.objectField = null;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, UIMeasurement measurement) {
            this.propertyId = propertyId;
            this.valuePart0 = 0;
            this.floatValue = measurement.value;
            this.valuePart1 = (int) measurement.unit;
            this.objectField = null;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, TransformOffset offset) {
            this.propertyId = propertyId;
            this.valuePart0 = 0;
            this.floatValue = offset.value;
            this.valuePart1 = (int) offset.unit;
            this.objectField = null;
        }
        
        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, GridTrackSize trackSize) {
            this.valuePart0 = 0;
            this.propertyId = propertyId;
            this.floatValue = trackSize.minValue;
            this.valuePart1 = (int) trackSize.minUnit;
            this.objectField = null;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, float floatValue) {
            this.propertyId = propertyId;
            this.valuePart0 = 0;
            this.floatValue = floatValue;
            this.valuePart1 = 0;
            this.objectField = null;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, int intValue) {
            this.floatValue = 0;
            this.propertyId = propertyId;
            this.valuePart0 = intValue;
            this.valuePart1 = 0;
            this.objectField = null;
        }

        public bool IsDefined {
            [DebuggerStepThrough] get { return !IsUnset; }
        }

        public WhitespaceMode AsWhitespaceMode => (WhitespaceMode)valuePart0;
        
        public int AsInt => valuePart0;
        public float AsFloat => floatValue;
        public GridAxisAlignment AsGridAxisAlignment => (GridAxisAlignment) valuePart0;
        public CrossAxisAlignment AsCrossAxisAlignment => (CrossAxisAlignment) valuePart0;
        public MainAxisAlignment AsMainAxisAlignment => (MainAxisAlignment) valuePart0;
        public Overflow AsOverflow => (Overflow) valuePart0;
        public Color AsColor => valuePart1 == 0 ? ColorUtil.UnsetValue : (Color)new StyleColor(valuePart0);

        public TMP_FontAsset AsFont => (TMP_FontAsset) objectField;
        public Texture2D AsTexture => (Texture2D) objectField;

        public FontStyle AsFontStyle => (FontStyle) valuePart0;
        public TextAlignment AsTextAlignment => (TextAlignment) valuePart0;
        public LayoutDirection AsLayoutDirection => (LayoutDirection) valuePart0;
        public LayoutWrap AsLayoutWrap => (LayoutWrap) valuePart0;

        public GridTrackSize AsGridTrackSize => new GridTrackSize(floatValue, (GridTemplateUnit) valuePart1);
        public UIMeasurement AsUIMeasurement => new UIMeasurement(floatValue, (UIMeasurementUnit) valuePart1);
        public UIFixedLength AsUIFixedLength => new UIFixedLength(floatValue, (UIFixedUnit) valuePart1);
        public TransformOffset AsTransformOffset => new TransformOffset(floatValue, (TransformUnit) valuePart1);

        public IReadOnlyList<GridTrackSize> AsGridTrackTemplate => (IReadOnlyList<GridTrackSize>) objectField;

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

        public IReadOnlyList<GridTrackSize> AsGridTemplate => (IReadOnlyList<GridTrackSize>) objectField;
        public VerticalScrollbarAttachment AsVerticalScrollbarAttachment => (VerticalScrollbarAttachment) valuePart0;
        public HorizontalScrollbarAttachment AsHorizontalScrollbarAttachment => (HorizontalScrollbarAttachment) valuePart0;
        public ScrollbarButtonPlacement AsScrollbarButtonPlacement => (ScrollbarButtonPlacement) valuePart0;
        public Visibility AsVisibility => (Visibility) valuePart0;
        public CursorStyle AsCursorStyle => (CursorStyle) objectField;
        public string AsString => (string) objectField;
        public ShadowType AsShadowType => (ShadowType) valuePart0;
        
        public static bool operator ==(StyleProperty a, StyleProperty b) {
            bool baseCase =
                a.propertyId == b.propertyId &&
                a.valuePart0 == b.valuePart0 &&
                a.valuePart1 == b.valuePart1 &&
                a.objectField == b.objectField;

            if (baseCase) return true;
            bool aIsNan = float.IsNaN(a.floatValue);
            bool bIsNan = float.IsNaN(b.floatValue);
            return aIsNan == bIsNan && Mathf.Approximately(a.floatValue, b.floatValue);       
        }

        public static bool operator !=(StyleProperty a, StyleProperty b) {
            bool baseCase =
                a.propertyId != b.propertyId ||
                a.valuePart0 != b.valuePart0 ||
                a.valuePart1 != b.valuePart1 ||
                a.objectField != b.objectField;

            if (baseCase) return true;
            bool aIsNan = float.IsNaN(a.floatValue);
            bool bIsNan = float.IsNaN(b.floatValue);
            return aIsNan != bIsNan || !Mathf.Approximately(a.floatValue, b.floatValue);       
        }


        public bool Equals(StyleProperty other) {
            return propertyId == other.propertyId && valuePart0 == other.valuePart0 && valuePart1 == other.valuePart1 && Equals(objectField, other.objectField);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is StyleProperty && Equals((StyleProperty) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) propertyId;
                hashCode = (hashCode * 397) ^ valuePart0;
                hashCode = (hashCode * 397) ^ valuePart1;
                hashCode = (hashCode * 397) ^ (objectField != null ? objectField.GetHashCode() : 0);
                return hashCode;
            }
        }

        [DebuggerStepThrough]
        public static StyleProperty Unset(StylePropertyId propertyId) {
            return new StyleProperty(propertyId, IntUtil.UnsetValue, IntUtil.UnsetValue, FloatUtil.UnsetValue, null);
        }

        public static StyleProperty TransformPositionX(TransformOffset length) {
            return new StyleProperty(StylePropertyId.TransformPositionX, length);
        }

        public static StyleProperty TransformPositionY(TransformOffset length) {
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
            return new StyleProperty(StylePropertyId.GridItemColSelfAlignment, (int) alignment);
        }

        public static StyleProperty GridItemRowSelfAlignment(CrossAxisAlignment alignment) {
            return new StyleProperty(StylePropertyId.GridItemRowSelfAlignment, (int) alignment);
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
            return new StyleProperty(StylePropertyId.GridLayoutDirection, (int) direction);
        }

        public static StyleProperty GridLayoutColAutoSize(GridTrackSize autoColSize) {
            return new StyleProperty(StylePropertyId.GridLayoutMainAxisAutoSize, autoColSize.minValue, (int) autoColSize.minUnit);
        }

        public static StyleProperty GridLayoutRowAutoSize(GridTrackSize autoRowSize) {
            return new StyleProperty(StylePropertyId.GridLayoutCrossAxisAutoSize, autoRowSize.minValue, (int) autoRowSize.minUnit);
        }

        public static StyleProperty GridLayoutColGap(float colGap) {
            return new StyleProperty(StylePropertyId.GridLayoutColGap, colGap);
        }

        public static StyleProperty GridLayoutRowGap(float rowGap) {
            return new StyleProperty(StylePropertyId.GridLayoutRowGap, rowGap);
        }

        public static StyleProperty GridLayoutColAlignment(CrossAxisAlignment alignment) {
            return new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int) alignment);
        }

        public static StyleProperty GridLayoutRowAlignment(CrossAxisAlignment alignment) {
            return new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int) alignment);
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
            return new StyleProperty(StylePropertyId.AnchorTop, anchor.value, (int) anchor.unit);
        }

        public static StyleProperty AnchorRight(UIFixedLength anchor) {
            return new StyleProperty(StylePropertyId.AnchorRight, anchor.value, (int) anchor.unit);
        }

        public static StyleProperty AnchorBottom(UIFixedLength anchor) {
            return new StyleProperty(StylePropertyId.AnchorBottom, anchor.value, (int) anchor.unit);
        }

        public static StyleProperty AnchorLeft(UIFixedLength anchor) {
            return new StyleProperty(StylePropertyId.AnchorLeft, anchor.value, (int) anchor.unit);
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