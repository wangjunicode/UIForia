using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Text;
using UIForia.UIInput;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Rendering {

    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("type = {propertyId.ToString()}")]
    public readonly partial struct StyleProperty {

        [FieldOffset(0)] internal readonly object objectField; // must be at offset 0 8 or 16 because of alignment, cannot overlap non reference types

        [FieldOffset(8)] public readonly StylePropertyId propertyId;
        
        [FieldOffset(10)] internal readonly ushort flags;
        
        [FieldOffset(12)] internal readonly int int0;

        [FieldOffset(12)] internal readonly float float0;

        [FieldOffset(16)] internal readonly int int1; // can merge this with flags (except for 1 bit at end) if we make sure each measurement type fits in ushort - 1 bit 
        
        [FieldOffset(16)] internal readonly int float1; // unused atm. if continues to be unused can merge this with flags (except for 1 bit at end) if we make sure each measurement type fits in ushort - 1 bit 


        public bool hasValue => flags != 0;
        
        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId) {
            this.propertyId = propertyId;
            this.int0 = 0;
            this.int1 = 0;
            this.float0 = 0;
            this.float1 = 0;
            this.flags = 0;
            this.objectField = null;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in Color color) {
            this.propertyId = propertyId;
            this.objectField = null;
            this.float0 = 0;
            this.float1 = 0;
            this.int0 = new StyleColor(color).rgba;
            this.int1 = 1;
            this.flags = 1;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in Color? color) {
            this.propertyId = propertyId;
            this.objectField = null;
            this.float0 = 0;
            this.float1 = 0;
            this.int0 = 0;
            this.int1 = 0;
            this.flags = 0;
            if (color.HasValue) {
                this.int0 = new StyleColor(color.Value).rgba;
                this.int1 = 1;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in UIFixedLength length) {
            this.propertyId = propertyId;
            this.int0 = 0;
            this.flags = 1;
            this.float1 = 0;
            this.objectField = null;
            this.float0 = length.value;
            this.int1 = (int) length.unit;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in UIFixedLength? length) {
            this.propertyId = propertyId;
            this.int0 = 0;
            this.int1 = 0;
            this.flags = 0;
            this.float0 = 0;
            this.float1 = 0;
            this.objectField = null;
            if (length.HasValue) {
                UIFixedLength v = length.Value;
                this.float0 = v.value;
                this.int1 = (int) v.unit;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in UIMeasurement measurement) {
            this.propertyId = propertyId;
            this.int0 = 0;
            this.float1 = 0;
            this.flags = 1;
            this.objectField = null;
            this.float0 = measurement.value;
            this.int1 = (int) measurement.unit;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in UIMeasurement? measurement) {
            this.propertyId = propertyId;
            this.int0 = 0;
            this.int1 = 0;
            this.flags = 0;
            this.float0 = 0;
            this.float1 = 0;
            this.objectField = null;
            if (measurement.HasValue) {
                UIMeasurement val = measurement.Value;
                this.float0 = val.value;
                this.int1 = (int) val.unit;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in OffsetMeasurement offset) {
            this.propertyId = propertyId;
            this.objectField = null;
            this.int0 = 0;
            this.flags = 1;
            this.float1 = 0;
            this.float0 = offset.value;
            this.int1 = (int) offset.unit;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in OffsetMeasurement? offset) {
            this.propertyId = propertyId;
            this.objectField = null;
            this.int0 = 0;
            this.int1 = 0;
            this.flags = 0;
            this.float0 = 0;
            this.float1 = 0;
            if (offset.HasValue) {
                OffsetMeasurement val = offset.Value;
                this.float0 = val.value;
                this.int1 = (int) val.unit;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, float float0) {
            this.propertyId = propertyId;
            this.int0 = 0;
            this.int1 = 0;
            this.flags = 1;
            this.float1 = 0;
            this.objectField = null;
            this.float0 = float0;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in float? floatValue) {
            this.propertyId = propertyId;
            this.int0 = 0;
            this.int1 = 0;
            this.flags = 0;
            this.float0 = 0;
            this.float1 = 0;
            this.objectField = null;
            if (floatValue.HasValue) {
                this.float0 = floatValue.Value;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, int intValue) {
            this.propertyId = propertyId;
            this.float0 = 0;
            this.float1 = 0;
            this.int1 = 0;
            this.flags = 1;
            this.objectField = null;
            this.int0 = intValue;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, int? intValue) {
            this.propertyId = propertyId;
            this.float0 = 0;
            this.float1 = 0;
            this.int0 = 0;
            this.int1 = 0;
            this.flags = 0;
            this.objectField = null;
            if (intValue.HasValue) {
                this.int0 = intValue.Value;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in GridItemPlacement placement) {
            this.propertyId = propertyId;
            this.float0 = 0;
            this.float1 = 0;
            this.int0 = placement.index;
            this.int1 = 0;
            this.flags = 1;
            this.objectField = placement.name;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, GridItemPlacement? placement) {
            this.propertyId = propertyId;
            this.float0 = 0;
            this.float1 = 0;
            this.int0 = 0;
            this.int1 = 0;
            this.flags = 0;
            this.objectField = null;
            if (placement.HasValue) {
                GridItemPlacement val = placement.Value;
                this.objectField = val.name;
                this.int0 = val.index;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, string objectField) {
            this.propertyId = propertyId;
            this.float0 = 0;
            this.float1 = 0;
            this.int1 = 0;
            this.int0 = 0;
            this.flags = objectField != null ? (ushort)1 : (ushort)0;
            this.objectField = objectField;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, Texture objectField) {
            this.propertyId = propertyId;
            this.float0 = 0;
            this.float1 = 0;
            this.int1 = 0;
            this.int0 = 0;
            this.flags = !ReferenceEquals(objectField, null) ? (ushort)1 : (ushort)0;
            this.objectField = objectField;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, CursorStyle objectField) {
            this.propertyId = propertyId;
            this.float0 = 0;
            this.float1 = 0;
            this.int1 = 0;
            this.int0 = 0;
            this.flags = objectField != null ? (ushort)1 : (ushort)0;
            this.objectField = objectField;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, IReadOnlyList<GridTrackSize> objectField) {
            this.propertyId = propertyId;
            this.float0 = 0;
            this.float1 = 0;
            this.int1 = 0;
            this.int0 = 0;
            this.flags = objectField != null ? (ushort)1 : (ushort)0;
            this.objectField = objectField;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, FontAsset objectField) {
            this.propertyId = propertyId;
            this.float0 = 0;
            this.float1 = 0;
            this.int1 = 0;
            this.int0 = 0;
            this.flags = objectField != null ? (ushort)1 : (ushort)0;
            this.objectField = objectField;
        }

        public WhitespaceMode AsWhitespaceMode => (WhitespaceMode) int0;

        public int AsInt => int0;
        public float AsFloat => float0;
        public GridAxisAlignment AsGridAxisAlignment => (GridAxisAlignment) int0;
        public SpaceDistribution AsSpaceDistribution => (SpaceDistribution) int0;
        public AlignmentBoundary AsAlignmentBoundary => (AlignmentBoundary) int0;
        public Overflow AsOverflow => (Overflow) int0;
        public ClipBehavior AsClipBehavior => (ClipBehavior) int0;
        public Color AsColor => new StyleColor(int0);

        public FontAsset AsFont => (FontAsset) objectField;
        public Texture2D AsTexture => (Texture2D) objectField;

        public FontStyle AsFontStyle => (FontStyle) int0;
        public TextAlignment AsTextAlignment => (TextAlignment) int0;
        public LayoutDirection AsLayoutDirection => (LayoutDirection) int0;
        public LayoutWrap AsLayoutWrap => (LayoutWrap) int0;

        public UIMeasurement AsUIMeasurement => new UIMeasurement(float0, (UIMeasurementUnit) int1);
        public UIFixedLength AsUIFixedLength => new UIFixedLength(float0, (UIFixedUnit) int1);
        public OffsetMeasurement AsOffsetMeasurement => new OffsetMeasurement(float0, (OffsetMeasurementUnit) int1);

        public GridItemPlacement AsGridItemPlacement {
            get {
                if (objectField is string name) {
                    return new GridItemPlacement(name);
                }

                return new GridItemPlacement(int0);
            }
        }

        public IReadOnlyList<GridTrackSize> AsGridTrackTemplate => (IReadOnlyList<GridTrackSize>) objectField;

        public RenderLayer AsRenderLayer => (RenderLayer) int0;
        public Texture2D AsTexture2D => (Texture2D) objectField;
        public GridLayoutDensity AsGridLayoutDensity => (GridLayoutDensity) int0;
        public LayoutType AsLayoutType => (LayoutType) int0;
        public TextTransform AsTextTransform => (TextTransform) int0;
        public LayoutBehavior AsLayoutBehavior => (LayoutBehavior) int0;

        public IReadOnlyList<GridTrackSize> AsGridTemplate => (IReadOnlyList<GridTrackSize>) objectField;
        public Visibility AsVisibility => (Visibility) int0;
        public CursorStyle AsCursorStyle => (CursorStyle) objectField;
        public string AsString => (string) objectField;
        public UnderlayType AsUnderlayType => (UnderlayType) int0;
        public LayoutFit AsLayoutFit => (LayoutFit) int0;
        public BackgroundFit AsBackgroundFit => (BackgroundFit) int0;
        public ClipBounds AsClipBounds => (ClipBounds) int0;
        public PointerEvents AsPointerEvents => (PointerEvents) int0;

        public AlignmentTarget AsAlignmentTarget => (AlignmentTarget) int0;
        public AlignmentDirection AsAlignmentDirection => (AlignmentDirection)int0;

        public static bool operator ==(in StyleProperty a, in StyleProperty b) {
            return a.propertyId == b.propertyId &&
                   a.int0 == b.int0 &&
                   a.int1 == b.int1 &&
                   a.flags == b.flags &&
                   a.objectField == b.objectField;
        }

        public static bool operator !=(in StyleProperty a, in StyleProperty b) {
            return a.propertyId != b.propertyId ||
                   a.int0 != b.int0 ||
                   a.int1 != b.int1 ||
                   a.flags != b.flags ||
                   a.objectField != b.objectField;
        }

        public bool Equals(in StyleProperty other) {
            return propertyId == other.propertyId &&
                   int0 == other.int0 &&
                   int1 == other.int1 &&
                   flags == other.flags &&
                   objectField == other.objectField;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is StyleProperty property && Equals(property);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (int) propertyId;
                hashCode = (hashCode * 397) ^ int0;
                hashCode = (hashCode * 397) ^ int1;
                hashCode = (hashCode * 397) ^ flags;
                hashCode = (hashCode * 397) ^ (objectField != null ? objectField.GetHashCode() : 0);
                return hashCode;
            }
        }

        [DebuggerStepThrough]
        public static StyleProperty Unset(StylePropertyId propertyId) {
            return new StyleProperty(propertyId);
        }

    }

}