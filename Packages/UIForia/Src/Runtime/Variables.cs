using System;
using UIForia.Layout;
using UIForia.Style;
using Unity.Mathematics;

namespace UIForia {

    internal interface IStyleVariable {

        ElementId ElementId { get; }
        TraversalInfo TraversalInfo { get; set; }

    }

    internal struct TextureVariable : IStyleVariable, IComparable<TextureVariable> {

        public ElementId elementId;
        public ushort variableNameId;
        public ushort enumTypeId;
        public TraversalInfo traversalInfo;
        public TextureInfo textureInfo;

        public bool TryConvertToTextureInfo(out TextureInfo textureInfo) {
            textureInfo = this.textureInfo;
            return true;
        }

        public ElementId ElementId => elementId;

        public TraversalInfo TraversalInfo {
            get => traversalInfo;
            set { traversalInfo = value; }
        }

        public int CompareTo(TextureVariable other) {
            return (int) variableNameId - (int) other.variableNameId;
        }

    }

    internal struct GridTemplateVariable : IStyleVariable, IComparable<GridTemplateVariable> {

        public ElementId elementId;
        public ushort variableNameId;
        public ushort enumTypeId;
        public TraversalInfo traversalInfo;
        public GridLayoutTemplate value;

        public ElementId ElementId => elementId;

        public TraversalInfo TraversalInfo {
            get => traversalInfo;
            set { traversalInfo = value; }
        }

        public bool TryConvertToGridLayoutTemplate(out GridLayoutTemplate gridLayoutTemplate) {
            gridLayoutTemplate = value;
            return true;
        }

        public int CompareTo(GridTemplateVariable other) {
            return (int) variableNameId - (int) other.variableNameId;
        }

    }

    internal struct FontAssetVariable : IStyleVariable, IComparable<FontAssetVariable> {

        public ElementId elementId;
        public ushort variableNameId;
        public ushort enumTypeId;
        public TraversalInfo traversalInfo;
        public FontAssetId fontAssetId;

        public ElementId ElementId => elementId;

        public TraversalInfo TraversalInfo {
            get => traversalInfo;
            set { traversalInfo = value; }
        }

        public bool TryConvertToFontAssetId(out FontAssetId fontAssetId) {
            fontAssetId = this.fontAssetId;
            return true;
        }

        public int CompareTo(FontAssetVariable other) {
            return (int) variableNameId - (int) other.variableNameId;
        }

    }

    internal struct PainterVariableType : IStyleVariable, IComparable<PainterVariableType> {

        public ElementId elementId;
        public ushort variableNameId;
        public ushort enumTypeId;
        public TraversalInfo traversalInfo;
        public PainterId painterId;

        public ElementId ElementId => elementId;

        public TraversalInfo TraversalInfo {
            get => traversalInfo;
            set { traversalInfo = value; }
        }

        public bool TryConvertToPainterId(out PainterId painterId) {
            painterId = this.painterId;
            return true;
        }

        public int CompareTo(PainterVariableType other) {
            return (int) variableNameId - (int) other.variableNameId;
        }

    }

    internal struct VectorVariable : IStyleVariable, IComparable<VectorVariable> {
        public ElementId elementId;

        public ElementId ElementId => elementId;

        public TraversalInfo traversalInfo;
        
        public TraversalInfo TraversalInfo {
            get => traversalInfo;
            set { traversalInfo = value; }
        }

        public float4 value;

        public ushort variableNameId;

        public int CompareTo(VectorVariable other) {
            return (int) variableNameId - (int) other.variableNameId;
        }

        public bool TryConvertTofloat2(out float2 retn) {
            retn = new float2(value.x, value.y);
            return true;
        }

        public bool TryConvertTofloat3(out float3 retn) {
            retn = new float3(value.x, value.y, value.z);
            return true;
        }

        public bool TryConvertTofloat4(out float4 retn) {
            retn = value;
            return true;
        }


    }

    internal struct ValueVariable : IStyleVariable, IComparable<ValueVariable> {

        public UIValue value;

        public ElementId elementId;
        public ushort variableNameId;
        public ushort enumTypeId;

        // assigned per-frame, denormalized
        public TraversalInfo traversalInfo;

        public ElementId ElementId => elementId;

        public TraversalInfo TraversalInfo {
            get => traversalInfo;
            set { traversalInfo = value; }
        }

        public int CompareTo(ValueVariable other) {
            return (int) variableNameId - (int) other.variableNameId;
        }

        public bool TryConvertToByte(out byte retn) {
            retn = (byte) value;
            return value.unit == Unit.Unset;
        }

        public bool TryConvertToUInt16(out ushort retn) {
            retn = (ushort) value;
            return value.unit == Unit.Unset;
        }

        public bool TryConvertToInt32(out int retn) {
            retn = (int) value;
            return value.unit == Unit.Unset;
        }

        public bool TryConvertToUInt32(out int retn) {
            retn = (int) value;
            return value.unit == Unit.Unset;
        }

        public bool TryConvertToFloat(out float retn) {
            retn = (float) value;
            return value.unit == Unit.Unset;
        }

        public bool TryConvertTohalf(out half retn) {
            retn = (half) value;
            return value.unit == Unit.Unset;
        }

        public bool TryConvertToUIFixedLength(out UIFixedLength uiFixedLength) {
            UIFixedUnit fixedUnit = (UIFixedUnit) (int) value.unit;
            switch (fixedUnit) {
                case UIFixedUnit.Unset:
                case UIFixedUnit.Pixel:
                case UIFixedUnit.Percent:
                case UIFixedUnit.Em:
                case UIFixedUnit.ViewportWidth:
                case UIFixedUnit.ViewportHeight:
                    uiFixedLength = new UIFixedLength((float) value.value, fixedUnit);
                    return true;

                default:
                    uiFixedLength = default;
                    return false;
            }
        }

        public bool TryConvertToUIAngle(out UIAngle uiAngle) {
            UIAngleUnit unit = (UIAngleUnit) value.unit;
            switch (unit) {
                case UIAngleUnit.Degrees:
                case UIAngleUnit.Percent:
                case UIAngleUnit.Radians:
                    uiAngle = new UIAngle((float) value.value, unit);
                    return true;

                default:
                    uiAngle = default;
                    return false;
            }
        }

        public bool TryConvertToUIOffset(out UIOffset uiOffset) {
            UIOffsetUnit unit = (UIOffsetUnit) value.unit;
            switch (unit) {
                case UIOffsetUnit.Unset:
                case UIOffsetUnit.Pixel:
                case UIOffsetUnit.Em:
                case UIOffsetUnit.Width:
                case UIOffsetUnit.Height:
                case UIOffsetUnit.ContentWidth:
                case UIOffsetUnit.ContentHeight:
                case UIOffsetUnit.ViewportWidth:
                case UIOffsetUnit.ViewportHeight:
                case UIOffsetUnit.ParentWidth:
                case UIOffsetUnit.ParentHeight:
                case UIOffsetUnit.ContentAreaWidth:
                case UIOffsetUnit.ContentAreaHeight:
                case UIOffsetUnit.ScreenWidth:
                case UIOffsetUnit.ScreenHeight:
                    uiOffset = new UIOffset((float) value.value, unit);
                    return true;

                default:
                    uiOffset = default;
                    return false;
            }
        }

        public bool TryConvertToUIMeasurement(out UIMeasurement uiMeasurement) {
            UIMeasurementUnit unit = (UIMeasurementUnit) (int) value.unit;
            switch (unit) {
                case UIMeasurementUnit.Percent:
                    uiMeasurement = new UIMeasurement((float) value.value * 100f, unit);
                    return true;

                case UIMeasurementUnit.Unset:
                case UIMeasurementUnit.Pixel:
                case UIMeasurementUnit.Content:
                case UIMeasurementUnit.ViewportWidth:
                case UIMeasurementUnit.ViewportHeight:
                case UIMeasurementUnit.Em:

                case UIMeasurementUnit.BackgroundImageWidth:
                case UIMeasurementUnit.BackgroundImageHeight:
                case UIMeasurementUnit.Stretch:
                case UIMeasurementUnit.ApplicationWidth:
                case UIMeasurementUnit.ApplicationHeight:
                case UIMeasurementUnit.MaxChild:
                case UIMeasurementUnit.MinChild:
                    uiMeasurement = new UIMeasurement((float) value.value, unit);
                    return true;

                default:
                    uiMeasurement = default;
                    return false;
            }

        }

        public bool TryConvertToUIFontSize(out UIFontSize uiFontSize) {
            UIFontSizeUnit unit = (UIFontSizeUnit) value.unit;
            switch (unit) {
                case UIFontSizeUnit.Default:
                case UIFontSizeUnit.Pixel:
                case UIFontSizeUnit.Point:
                case UIFontSizeUnit.Em:
                    uiFontSize = new UIFontSize((float) value.value, unit);
                    return true;

                default:
                    uiFontSize = default;
                    return false;
            }
        }

        public bool TryConvertToUISpaceSize(out UISpaceSize uiSpaceSize) {
            UISpaceSizeUnit unit = (UISpaceSizeUnit) (int) value.unit;
            switch (unit) {
                case UISpaceSizeUnit.Pixel:
                case UISpaceSizeUnit.Em:
                case UISpaceSizeUnit.Stretch:
                case UISpaceSizeUnit.ViewportWidth:
                case UISpaceSizeUnit.ViewportHeight:
                case UISpaceSizeUnit.ApplicationWidth:
                case UISpaceSizeUnit.ApplicationHeight:
                    uiSpaceSize = new UISpaceSize((float) value.value, unit);
                    return true;

                default:
                    uiSpaceSize = default;
                    return false;
            }
        }

        public bool TryConvertToUISizeConstraint(out UISizeConstraint uiSizeConstraint) {
            UISizeConstraintUnit unit = (UISizeConstraintUnit) value.unit;
            switch (unit) {
                case UISizeConstraintUnit.Pixel:
                case UISizeConstraintUnit.Em:
                case UISizeConstraintUnit.Content:
                case UISizeConstraintUnit.MaxChild:
                case UISizeConstraintUnit.MinChild:
                case UISizeConstraintUnit.BackgroundImageWidth:
                case UISizeConstraintUnit.BackgroundImageHeight:
                case UISizeConstraintUnit.ApplicationWidth:
                case UISizeConstraintUnit.ApplicationHeight:
                case UISizeConstraintUnit.ViewportWidth:
                case UISizeConstraintUnit.ViewportHeight:
                case UISizeConstraintUnit.ParentSize:
                case UISizeConstraintUnit.Percent:
                    uiSizeConstraint = new UISizeConstraint((float) value.value, unit);
                    return true;

                default:
                    uiSizeConstraint = default;
                    return false;
            }
        }

        // todo create separate variable types for these

        public bool TryConvertToGridItemPlacement(out GridItemPlacement gridItemPlacement) {
            throw new NotImplementedException();
        }

        public bool TryConvertToAspectRatio(out AspectRatio aspectRatio) {
            throw new NotImplementedException();
        }

        public bool TryConvertToTTEMPLATE(out TTEMPLATE resolved) {
            throw new NotImplementedException();
        }

        public bool TryConvertToSingle(out float resolved) {
            resolved = (float) value;
            return value.unit == Unit.Unset;
        }

        public bool TryConvertToEnumValue(out EnumValue resolved) {
            resolved = default;
            return false;
        }

    }

    internal struct ColorVariable : IStyleVariable, IComparable<ColorVariable> {

        public UIColor color;
        public ElementId elementId;
        public ushort variableNameId;
        public ushort enumTypeId;

        // assigned per-frame, denormalized
        public TraversalInfo traversalInfo;

        public ElementId ElementId => elementId;

        public TraversalInfo TraversalInfo {
            get => traversalInfo;
            set { traversalInfo = value; }
        }

        public bool TryConvertToUIColor(out UIColor uiColor) {
            uiColor = color;
            return true;
        }

        public int CompareTo(ColorVariable other) {
            return (int) variableNameId - (int) other.variableNameId;
        }

    }

}