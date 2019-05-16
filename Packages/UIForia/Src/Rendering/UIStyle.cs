using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    // can this be a struct?
    [DebuggerDisplay("{nameof(Id)}")]
    public partial class UIStyle {

        private static int NextStyleId;

        internal readonly LightList<StyleProperty> m_StyleProperties;

        public int Id { get; set; } = NextStyleId++;

        public UIStyle(int capacity = 8) {
            m_StyleProperties = new LightList<StyleProperty>(capacity);
        }

        public UIStyle(UIStyle toCopy) : this() {
            m_StyleProperties.AddRange(toCopy.m_StyleProperties);
        }

        public IReadOnlyList<StyleProperty> Properties => m_StyleProperties;

        public BorderRadius BorderRadius {
            set {
                SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopLeft, value.topLeft));
                SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopRight, value.topRight));
                SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, value.bottomLeft));
                SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomRight, value.bottomRight));
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
                        m_StyleProperties[i].floatValue,
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
                        m_StyleProperties[i].floatValue,
                        (UIMeasurementUnit) m_StyleProperties[i].valuePart1
                    );
                }
            }

            return UIMeasurement.Unset;
        }
        
        private TransformOffset FindTransformOffsetProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    return new TransformOffset(
                        m_StyleProperties[i].floatValue,
                        (TransformUnit) m_StyleProperties[i].valuePart1
                    );
                }
            }

            return TransformOffset.Unset;
        }

        private UIFixedLength FindUIFixedLengthProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    return new UIFixedLength(
                        m_StyleProperties[i].floatValue,
                        (UIFixedUnit) m_StyleProperties[i].valuePart1
                    );
                }
            }

            return UIFixedLength.Unset;
        }

        private float FindFloatProperty(StylePropertyId propertyId) {
            StyleProperty property = GetProperty(propertyId);
            return property.IsUnset ? FloatUtil.UnsetValue : property.AsFloat;
        }

        private int FindIntProperty(StylePropertyId propertyId) {
            StyleProperty property = GetProperty(propertyId);
            return property.IsUnset ? IntUtil.UnsetValue : property.valuePart0;
        }

        private int FindEnumProperty(StylePropertyId propertyId) {
            StyleProperty property = GetProperty(propertyId);
            return property.IsUnset ? 0 : property.valuePart0;
        }

        private Color FindColorProperty(StylePropertyId propertyId) {
            StyleProperty property = GetProperty(propertyId);
            return property.IsUnset ?  ColorUtil.UnsetValue :(Color) new StyleColor(property.valuePart0);
        }
        
        internal void SetProperty(in StyleProperty property) {
            StylePropertyId propertyId = property.propertyId;
            if (property.IsUnset) {
                for (int i = 0; i < m_StyleProperties.Count; i++) {
                    if (m_StyleProperties[i].propertyId == propertyId) {
                        m_StyleProperties.RemoveAt(i);
                        return;
                    }
                }
                return;
            }

            // todo -- binary search or int map
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    m_StyleProperties[i] = property;
                    return;
                }
            }

            m_StyleProperties.Add(property);
        }
        
        public StyleProperty GetProperty(StylePropertyId propertyId) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    return m_StyleProperties[i];
                }
            }

            return new StyleProperty(propertyId, int.MaxValue, int.MaxValue);
        }
       
        public bool TryGetProperty(StylePropertyId propertyId, out StyleProperty property) {
            for (int i = 0; i < m_StyleProperties.Count; i++) {
                if (m_StyleProperties[i].propertyId == propertyId) {
                    property = m_StyleProperties[i];
                    return true;
                }
            }

            property = default(StyleProperty);
            return false;
        }

        public static UIStyle Merge(UIStyle destination, UIStyle source) {
            if (source == null || source.m_StyleProperties.Count == 0) {
                return destination;
            }

            if (destination == null) {
                return new UIStyle(source);
            }

            for (int pIndex = 0; pIndex < source.m_StyleProperties.Count; pIndex++) {
                StyleProperty prop = source.m_StyleProperties[pIndex];
                destination.SetProperty(prop);
            }

            return destination;
        }


    }

}