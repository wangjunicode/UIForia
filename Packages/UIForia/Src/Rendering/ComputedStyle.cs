using System.Diagnostics;
using UIForia.Layout.LayoutTypes;
using UnityEngine;

namespace UIForia.Rendering {

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