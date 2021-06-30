using System;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public partial class UIStyle {

        internal StyleProperty[] array;
        
        public UIStyle(int capacity = 8) {
            if (capacity <= 0) capacity = 8;
            this.array = new StyleProperty[capacity];
            this.propertyCount = 0;
        }

        public UIStyle(UIStyle toCopy) : this() {
            this.propertyCount = toCopy.propertyCount;
            this.array = new StyleProperty[toCopy.propertyCount];
            Array.Copy(toCopy.array, 0, array, 0, toCopy.propertyCount);
        }

        public int propertyCount;

        public StyleProperty this[int index] {
            get {
                if (index < 0 || index >= propertyCount) return default;
                return array[index];
            }
        }
        
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
            for (int i = 0; i < propertyCount; i++) {
                if (array[i].propertyId == propertyId) return true;
            }

            return false;
        }
        

        private UIMeasurement FindUIMeasurementProperty(StylePropertyId propertyId) {
            for (int i = 0; i < propertyCount; i++) {
                if (array[i].propertyId == propertyId) {
                    return array[i].AsUIMeasurement;
                }
            }

            return UIMeasurement.Unset;
        }
        
        private MaterialId FindMaterialIdProperty(StylePropertyId propertyId) {
            for (int i = 0; i < propertyCount; i++) {
                if (array[i].propertyId == propertyId) {
                    return array[i].AsMaterialId;
                }
            }

            return (MaterialId) 0;
        }
        
        private OffsetMeasurement FindOffsetMeasurementProperty(StylePropertyId propertyId) {
            for (int i = 0; i < propertyCount; i++) {
                if (array[i].propertyId == propertyId) {
                    return array[i].AsOffsetMeasurement;
                }
            }

            return OffsetMeasurement.Unset;
        }

        private UIFixedLength FindUIFixedLengthProperty(StylePropertyId propertyId) {
            for (int i = 0; i < propertyCount; i++) {
                if (array[i].propertyId == propertyId) {
                    return array[i].AsUIFixedLength;
                }
            }

            return UIFixedLength.Unset;
        }

        private float FindFloatProperty(StylePropertyId propertyId) {
            StyleProperty property = GetProperty(propertyId);
            return !property.hasValue ? FloatUtil.UnsetValue : property.AsFloat;
        }

        private int FindIntProperty(StylePropertyId propertyId) {
            StyleProperty property = GetProperty(propertyId);
            return !property.hasValue ? IntUtil.UnsetValue : property.int0;
        }

        private int FindEnumProperty(StylePropertyId propertyId) {
            StyleProperty property = GetProperty(propertyId);
            return !property.hasValue ? 0 : property.int0;
        }

        private Color FindColorProperty(StylePropertyId propertyId) {
            StyleProperty property = GetProperty(propertyId);
            return !property.hasValue ?  ColorUtil.UnsetValue :(Color) new StyleColor(property.int0);
        }
        
        internal void SetProperty(in StyleProperty property) {
            StylePropertyId propertyId = property.propertyId;
            if (!property.hasValue) {
                for (int i = 0; i < propertyCount; i++) {
                    if (array[i].propertyId == propertyId) {
                        RemoveAt(i);
                        return;
                    }
                }
                return;
            }

            // todo -- binary search or int map
            for (int i = 0; i < propertyCount; i++) {
                if (array[i].propertyId == propertyId) {
                    array[i] = property;
                    return;
                }
            }

            if (propertyCount + 1 >= array.Length) {
                Array.Resize(ref array, array.Length + 8);
            }

            array[propertyCount++] = property;
        }
        
        private  void RemoveAt(int index) {
            if ((uint) index >= (uint) propertyCount) return;
            if (index == propertyCount - 1) {
                array[--propertyCount] = default;
            }
            else {
                for (int j = index; j < propertyCount - 1; j++) {
                    array[j] = array[j + 1];
                }

                array[--propertyCount] = default;
            }
        }
        
        public StyleProperty GetProperty(StylePropertyId propertyId) {
            for (int i = 0; i < propertyCount; i++) {
                if (array[i].propertyId == propertyId) {
                    return array[i];
                }
            }

            return new StyleProperty(propertyId);
        }
       
        public bool TryGetProperty(StylePropertyId propertyId, out StyleProperty property) {
            for (int i = 0; i < propertyCount; i++) {
                if (array[i].propertyId == propertyId) {
                    property = array[i];
                    return true;
                }
            }

            property = default(StyleProperty);
            return false;
        }

        public static UIStyle Merge(UIStyle destination, UIStyle source) {
            if (source == null || source.propertyCount == 0) {
                return destination;
            }

            if (destination == null) {
                return new UIStyle(source);
            }

            for (int pIndex = 0; pIndex < source.propertyCount; pIndex++) {
                StyleProperty prop = source.array[pIndex];
                destination.SetProperty(prop);
            }

            return destination;
        }


    }

}