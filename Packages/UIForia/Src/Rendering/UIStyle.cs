using System;
using System.Collections.Generic;
using System.Diagnostics;
using Shapes2D;
using UIForia.Layout;
using UIForia.Text;
using TMPro;
using UIForia.Layout.LayoutTypes;
using UIForia.Util;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Rendering {

    [DebuggerDisplay("{nameof(Id)}")]
    public partial class UIStyle {

        [Flags]
        public enum TextPropertyIdFlag {

            TextColor = 1 << 0,
            TextFontAsset = 1 << 1,
            TextFontSize = 1 << 2,
            TextFontStyle = 1 << 3,
            TextAnchor = 1 << 4,
            TextWhitespaceMode = 1 << 5,
            TextWrapMode = 1 << 6,
            TextHorizontalOverflow = 1 << 7,
            TextVerticalOverflow = 1 << 8,
            TextIndentFirstLine = 1 << 9,
            TextIndentNewLine = 1 << 10,
            TextLayoutStyle = 1 << 11,
            TextAutoSize = 1 << 12,
            TextTransform = 1 << 13

        }

        private static int NextStyleId;
        private List<StyleProperty> m_StyleProperties;

        public int Id { get; set; } = NextStyleId++;

        public UIStyle() {
            m_StyleProperties = ListPool<StyleProperty>.Get();
        }

        public UIStyle(UIStyle toCopy) {
            m_StyleProperties.AddRange(toCopy.m_StyleProperties);
        }

        public IReadOnlyList<StyleProperty> Properties => m_StyleProperties;

        public BorderRadius BorderRadius {
            set {
                SetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopLeft, value.topLeft);
                SetUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight, value.topRight);
                SetUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomLeft, value.bottomLeft);
                SetUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomRight, value.bottomRight);
            }
        }

        public FixedLengthRect Padding {
            get { return new FixedLengthRect(PaddingTop, PaddingRight, PaddingBottom, PaddingLeft); }
            set {
                PaddingTop = value.top;
                PaddingRight = value.right;
                PaddingBottom = value.bottom;
                PaddingLeft = value.left;
            }
        }

        internal void OnSpawn() {
            m_StyleProperties = ListPool<StyleProperty>.Get();
        }

        internal void OnDestroy() {
            ListPool<StyleProperty>.Release(ref m_StyleProperties);
        }

        public bool DefinesProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) return true;
            }

            return false;
        }

        private GridTrackSize FindGridTrackSizeProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    return new GridTrackSize(
                        FloatUtil.DecodeToFloat(m_StyleProperties[i].valuePart0),
                        (GridTemplateUnit) m_StyleProperties[i].valuePart1
                    );
                }
            }

            return GridTrackSize.Unset;
        }

        private UIMeasurement FindUIMeasurementProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    return new UIMeasurement(
                        FloatUtil.DecodeToFloat(m_StyleProperties[i].valuePart0),
                        (UIMeasurementUnit) m_StyleProperties[i].valuePart1
                    );
                }
            }

            return UIMeasurement.Unset;
        }

        private UIFixedLength FindUIFixedLengthProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    return new UIFixedLength(
                        FloatUtil.DecodeToFloat(m_StyleProperties[i].valuePart0),
                        (UIFixedUnit) m_StyleProperties[i].valuePart1
                    );
                }
            }

            return UIFixedLength.Unset;
        }

        private void RemoveProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    m_StyleProperties.RemoveAt(i);
                    return;
                }
            }
        }

        public StyleProperty GetProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    return m_StyleProperties[i];
                }
            }

            return new StyleProperty(propertyId, int.MaxValue, int.MaxValue);
        }

        private float FindFloatProperty(StylePropertyId propertyId) {
            StyleProperty property = GetProperty(propertyId);
            return property.IsDefined ? FloatUtil.DecodeToFloat(property.valuePart0) : FloatUtil.UnsetValue;
        }

        private int FindIntProperty(StylePropertyId propertyId) {
            StyleProperty property = GetProperty(propertyId);
            return property.IsDefined ? property.valuePart0 : IntUtil.UnsetValue;
        }

        private int FindEnumProperty(StylePropertyId propertyId) {
            StyleProperty property = GetProperty(propertyId);
            return property.IsDefined ? property.valuePart0 : 0;
        }

        private Color FindColorProperty(StylePropertyId propertyId) {
            StyleProperty property = GetProperty(propertyId);
            return property.IsDefined ? (Color) new StyleColor(property.valuePart0) : ColorUtil.UnsetValue;
        }

        internal void SetUIMeasurementProperty(StylePropertyId propertyId, UIMeasurement measurement) {
            if (measurement.unit == UIMeasurementUnit.Unset || !FloatUtil.IsDefined(measurement.value)) {
                RemoveProperty(propertyId);
            }
            else {
                SetProperty(propertyId, FloatUtil.EncodeToInt(measurement.value), (int) measurement.unit);
            }
        }

        internal void SetGridTrackSizeProperty(StylePropertyId propertyId, GridTrackSize size) {
            if (size.minUnit == GridTemplateUnit.Unset || !FloatUtil.IsDefined(size.minValue)) {
                RemoveProperty(propertyId);
            }
            else {
                SetProperty(propertyId, FloatUtil.EncodeToInt(size.minValue), (int) size.minUnit);
            }
        }

        internal void SetObjectProperty(StylePropertyId propertyId, object value) {
            if (value == null) {
                RemoveProperty(propertyId);
            }
            else {
                for (int i = 0; i < m_StyleProperties.Count; i++) {
                    if (m_StyleProperties[i].propertyId == propertyId) {
                        m_StyleProperties[i] = new StyleProperty(propertyId, 0, 0, value);
                        return;
                    }
                }

                m_StyleProperties.Add(new StyleProperty(propertyId, 0, 0, value));
            }
        }

        internal void SetUIFixedLengthProperty(StylePropertyId propertyId, UIFixedLength length) {
            if (length.unit == UIFixedUnit.Unset || !FloatUtil.IsDefined(length.value)) {
                RemoveProperty(propertyId);
            }
            else {
                SetProperty(propertyId, FloatUtil.EncodeToInt(length.value), (int) length.unit);
            }
        }

        internal void SetProperty(StylePropertyId propertyId, int value0, int value1 = 0) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    m_StyleProperties[i] = new StyleProperty(propertyId, value0, value1);
                    return;
                }
            }

            m_StyleProperties.Add(new StyleProperty(propertyId, value0, value1));
        }

        internal void SetIntProperty(StylePropertyId propertyId, int value) {
            if (!IntUtil.IsDefined(value)) {
                RemoveProperty(propertyId);
                return;
            }

            SetProperty(propertyId, value);
        }

        internal void SetFloatProperty(StylePropertyId propertyId, float value) {
            if (!FloatUtil.IsDefined(value)) {
                RemoveProperty(propertyId);
                return;
            }

            SetProperty(propertyId, FloatUtil.EncodeToInt(value));
        }

        internal void SetEnumProperty(StylePropertyId propertyId, int value0) {
            if (value0 == 0 || !IntUtil.IsDefined(value0)) {
                RemoveProperty(propertyId);
                return;
            }

            SetProperty(propertyId, value0);
        }

        internal void SetColorProperty(StylePropertyId propertyId, Color color) {
            if (!ColorUtil.IsDefined(color)) {
                RemoveProperty(propertyId);
                return;
            }

            SetProperty(propertyId, new StyleColor(color).rgba);
        }

    }

}