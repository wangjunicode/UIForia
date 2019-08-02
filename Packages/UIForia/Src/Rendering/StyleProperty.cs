using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Rendering {

    // todo if object is at 0 we can do better with packing & get rid of int 2, nobody uses it yet anyway
    [StructLayout(LayoutKind.Explicit)]
    public partial struct StyleProperty {

        [FieldOffset(0)] public readonly StylePropertyId propertyId;

        [FieldOffset(4)] public readonly int int0;

        [FieldOffset(4)] public readonly float float0;

        [FieldOffset(8)] public readonly int int1;

        [FieldOffset(8)] public readonly float float1;

        [FieldOffset(12)] public readonly int int2;

        [FieldOffset(16)] public readonly object objectField;

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId) {
            this.propertyId = propertyId;
            this.int0 = 0;
            this.int1 = 0;
            this.float0 = 0;
            this.float1 = 0;
            this.int2 = 0;
            this.objectField = null;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, int value0, int value1) {
            this.propertyId = propertyId;
            this.objectField = null;
            this.float0 = 0;
            this.float1 = 0;
            this.int0 = value0;
            this.int1 = value1;
            this.int2 = 0;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, float float0, int value1) {
            this.propertyId = propertyId;
            this.objectField = null;
            this.int0 = 0;
            this.float1 = 0;
            this.float0 = float0;
            this.int1 = value1;
            this.int2 = 0;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, Color color) {
            this.propertyId = propertyId;
            this.objectField = null;
            this.float0 = 0;
            this.float1 = 0;
            this.int0 = new StyleColor(color).rgba;
            this.int1 = ColorUtil.IsDefined(color) ? 1 : 0;
            this.int2 = 0;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, UIFixedLength length) {
            this.propertyId = propertyId;
            this.int0 = 0;
            this.int2 = 0;
            this.float1 = 0;
            this.objectField = null;
            this.float0 = length.value;
            this.int1 = (int) length.unit;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, UIFixedLength? length) {
            this.propertyId = propertyId;
            if (length == null) {
                this.int0 = 0;
                this.int1 = 0;
                this.int2 = 0;
                this.float0 = 0;
                this.float1 = 0;
                this.objectField = null;
            }
            else {
                UIFixedLength v = length.Value;
                this.propertyId = propertyId;
                this.int0 = 0;
                this.int2 = 0;
                this.float1 = 0;
                this.objectField = null;
                this.float0 = v.value;
                this.int1 = (int) v.unit;
            }
            
        }
        
        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, UIMeasurement measurement) {
            this.propertyId = propertyId;
            this.int0 = 0;
            this.float1 = 0;
            this.int2 = 0;
            this.objectField = null;
            this.float0 = measurement.value;
            this.int1 = (int) measurement.unit;
        }


        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, TransformOffset offset) {
            this.propertyId = propertyId;
            this.objectField = null;
            this.int0 = 0;
            this.int2 = 0;
            this.float1 = 0;
            this.float0 = offset.value;
            this.int1 = (int) offset.unit;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, GridTrackSize trackSize) {
            this.propertyId = propertyId;
            this.int0 = 0;
            this.int2 = 0;
            this.float1 = 0;
            this.objectField = null;
            this.float0 = trackSize.minValue;
            this.int1 = (int) trackSize.minUnit;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, float float0) {
            this.propertyId = propertyId;
            this.int0 = 0;
            this.int1 = 0;
            this.int2 = 0;
            this.float1 = 0;
            this.objectField = null;
            this.float0 = float0;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, int intValue) {
            this.propertyId = propertyId;
            this.float0 = 0;
            this.float1 = 0;
            this.int1 = 0;
            this.int2 = 0;
            this.objectField = null;
            this.int0 = intValue;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, object objectField) {
            this.propertyId = propertyId;
            this.float0 = 0;
            this.float1 = 0;
            this.int1 = 0;
            this.int0 = 0;
            this.int2 = 0;
            this.objectField = objectField;
        }
        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, GridItemPlacement placement) {
            this.propertyId = propertyId;
            this.float0 = 0;
            this.float1 = 0;
            this.int0 = placement.index;
            this.int1 = 0;
            this.int2 = 0;
            this.objectField = placement.name;
        }
        
        public bool IsUnset2 => int0 == int.MinValue && int1 == int.MinValue && int2 == int.MinValue && objectField == null;

        public WhitespaceMode AsWhitespaceMode => (WhitespaceMode) int0;

        public int AsInt => int0;
        public float AsFloat => float0;
        public GridAxisAlignment AsGridAxisAlignment => (GridAxisAlignment) int0;
        public CrossAxisAlignment AsCrossAxisAlignment => (CrossAxisAlignment) int0;
        public MainAxisAlignment AsMainAxisAlignment => (MainAxisAlignment) int0;
        public Overflow AsOverflow => (Overflow) int0;
        public Color AsColor => int1 == 0 ? ColorUtil.UnsetValue : (Color) new StyleColor(int0);

        public FontAsset AsFont => (FontAsset) objectField;
        public Texture2D AsTexture => (Texture2D) objectField;

        public FontStyle AsFontStyle => (FontStyle) int0;
        public TextAlignment AsTextAlignment => (TextAlignment) int0;
        public LayoutDirection AsLayoutDirection => (LayoutDirection) int0;
        public LayoutWrap AsLayoutWrap => (LayoutWrap) int0;

        public GridTrackSize AsGridTrackSize => new GridTrackSize(float0, (GridTemplateUnit) int1);
        public UIMeasurement AsUIMeasurement => new UIMeasurement(float0, (UIMeasurementUnit) int1);
        public UIFixedLength AsUIFixedLength => new UIFixedLength(float0, (UIFixedUnit) int1);
        public TransformOffset AsTransformOffset => new TransformOffset(float0, (TransformUnit) int1);

        public GridItemPlacement AsGridItemPlacement {
            get {
                if (objectField is string name) {
                    return new GridItemPlacement(name);
                }

                return new GridItemPlacement(int0);
            }
        }

        public IReadOnlyList<GridTrackSize> AsGridTrackTemplate => (IReadOnlyList<GridTrackSize>) objectField;

        public AnchorTarget AsAnchorTarget => (AnchorTarget) int0;
        public RenderLayer AsRenderLayer => (RenderLayer) int0;
        public Texture2D AsTexture2D => (Texture2D) objectField;
        public GridLayoutDensity AsGridLayoutDensity => (GridLayoutDensity) int0;
        public TransformBehavior AsTransformBehavior => (TransformBehavior) int0;
        public LayoutType AsLayoutType => (LayoutType) int0;
        public TextTransform AsTextTransform => (TextTransform) int0;
        public LayoutBehavior AsLayoutBehavior => (LayoutBehavior) int0;

        public IReadOnlyList<GridTrackSize> AsGridTemplate => (IReadOnlyList<GridTrackSize>) objectField;
        public Visibility AsVisibility => (Visibility) int0;
        public CursorStyle AsCursorStyle => (CursorStyle) objectField;
        public string AsString => (string) objectField;
        public UnderlayType AsUnderlayType => (UnderlayType) int0;
        public Fit AsFit => (Fit) int0;

        public AlignmentTarget AsAlignmentTarget => (AlignmentTarget) int0;
        public AlignmentBehavior AsAlignmentBehavior => (AlignmentBehavior) int0;

        public static bool operator ==(in StyleProperty a, in StyleProperty b) {
            return a.propertyId == b.propertyId &&
                   a.int0 == b.int0 &&
                   a.int1 == b.int1 &&
                   a.int2 == b.int2 &&
                   a.objectField == b.objectField;
        }

        public static bool operator !=(in StyleProperty a, in StyleProperty b) {
            return a.propertyId != b.propertyId ||
                   a.int0 != b.int0 ||
                   a.int1 != b.int1 ||
                   a.int2 != b.int2 ||
                   a.objectField != b.objectField;
        }

        public bool Equals(in StyleProperty other) {
            return propertyId == other.propertyId &&
                   int0 == other.int0 &&
                   int1 == other.int1 &&
                   int2 == other.int2 &&
                   objectField == other.objectField;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is StyleProperty property && Equals(property);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) propertyId;
                hashCode = (hashCode * 397) ^ int0;
                hashCode = (hashCode * 397) ^ int1;
                hashCode = (hashCode * 397) ^ int2;
                hashCode = (hashCode * 397) ^ (objectField != null ? objectField.GetHashCode() : 0);
                return hashCode;
            }
        }

        [DebuggerStepThrough]
        public static StyleProperty Unset(StylePropertyId propertyId) {
            StyleProperty retn = new StyleProperty(propertyId);
            return retn;
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
            return new StyleProperty(StylePropertyId.BackgroundImage, texture);
        }

        public static StyleProperty BorderColor(Color color) {
            return new StyleProperty(StylePropertyId.BorderColor, color);
        }

        public static StyleProperty Opacity(float opacity) {
            return new StyleProperty(StylePropertyId.Opacity, opacity);
        }

        public static StyleProperty Cursor(Texture2D texture) {
            return new StyleProperty(StylePropertyId.Cursor, texture);
        }

        public static StyleProperty GridItemColStart(int colStart) {
            return new StyleProperty(StylePropertyId.GridItemY, colStart);
        }

        public static StyleProperty GridItemColSpan(int colSpan) {
            return new StyleProperty(StylePropertyId.GridItemHeight, colSpan);
        }

        public static StyleProperty GridItemRowStart(int rowStart) {
            return new StyleProperty(StylePropertyId.GridItemX, rowStart);
        }

        public static StyleProperty GridItemRowSpan(int rowSpan) {
            return new StyleProperty(StylePropertyId.GridItemWidth, rowSpan);
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
            return new StyleProperty(StylePropertyId.GridLayoutColTemplate, colTemplate);
        }

        public static StyleProperty GridLayoutRowTemplate(IList<GridTrackSize> rowTemplate) {
            return new StyleProperty(StylePropertyId.GridLayoutRowTemplate, rowTemplate);
        }

        public static StyleProperty GridLayoutDirection(LayoutDirection direction) {
            return new StyleProperty(StylePropertyId.GridLayoutDirection, (int) direction);
        }

        public static StyleProperty GridLayoutColAutoSize(GridTrackSize autoColSize) {
            return new StyleProperty(StylePropertyId.GridLayoutColAutoSize, autoColSize.minValue, (int) autoColSize.minUnit);
        }

        public static StyleProperty GridLayoutRowAutoSize(GridTrackSize autoRowSize) {
            return new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, autoRowSize.minValue, (int) autoRowSize.minUnit);
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

        public static StyleProperty Font(FontAsset fontAsset) {
            return new StyleProperty(StylePropertyId.TextFontAsset, fontAsset);
        }

    }

}