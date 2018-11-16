//using JetBrains.Annotations;
//using Shapes2D;
//using UIForia;
//using UIForia.Extensions;
//using UIForia.Rendering;
//using UIForia.StyleBindings;
//using UnityEngine;
//
//namespace UIForia.Rendering {
//
//    public partial class UIStyleSet {
//
//        [PublicAPI]
//        public Color GetBackgroundColor(StyleState state) {
//            StyleProperty property = GetPropertyValueInState(StylePropertyId.BackgroundColor, state);
//            return property.IsDefined
//                ? (Color) new StyleColor(property.valuePart0)
//                : ColorUtil.UnsetValue;
//        }
//        
//        [PublicAPI]
//        public Color GetBorderColor(StyleState state) {
//            StyleProperty property = GetPropertyValueInState(StylePropertyId.BorderColor, state);
//            return property.IsDefined
//                ? (Color) new StyleColor(property.valuePart0)
//                : ColorUtil.UnsetValue;
//        }
//
//        [PublicAPI]
//        public Texture2D GetBackgroundImage(StyleState state) {
//            return GetPropertyValueInState(StylePropertyId.BorderColor, state).AsTexture;
//        }
//        
//        [PublicAPI]
//        public void SetBackgroundColor(Color color, StyleState state) {
//            SetColorProperty(StylePropertyId.BackgroundColor, color, state);
//        }
//
//        [PublicAPI]
//        public void SetBackgroundImage(Texture2D image, StyleState state) {
//            SetObjectProperty(StylePropertyId.BackgroundImage, image, state);
//        }
//
//        [PublicAPI]
//        public void SetBorderColor(Color color, StyleState state) {
//            SetColorProperty(StylePropertyId.BorderColor, color, state);
//        }
//        
//        public void SetBackgroundColorSecondary(Color color, StyleState state) {
//            SetColorProperty(StylePropertyId.BackgroundColorSecondary, color, state);
//        }
//
//        public void SetBackgroundShapeType(BackgroundShapeType shapeType, StyleState state) {
//            SetEnumProperty(StylePropertyId.BackgroundShapeType, (int)shapeType, state);
//        }
//        
//        public void SetBackgroundGradientType(GradientType gradientType, StyleState state) {
//            SetEnumProperty(StylePropertyId.BackgroundGradientType, (int)gradientType, state);
//        }
//        
//        public void SetBackgroundGradientAxis(GradientAxis gradientAxis, StyleState state) {
//            SetEnumProperty(StylePropertyId.BackgroundGradientAxis, (int)gradientAxis, state);
//        }
//        
//        public void SetBackgroundGradientStart(float gradientStart, StyleState state) {
//            SetFloatProperty(StylePropertyId.BackgroundGradientStart, gradientStart, state);
//        }
//        
////        public void SetBackgroundGridSize(float gridSize, StyleState state) {
////            SetFloatProperty(StylePropertyId.BackgroundGridSize, gridSize, state);
////        }
////
////        public void SetBackgroundLineSize(float lineSize, StyleState state) {
////            SetFloatProperty(StylePropertyId.BackgroundLineSize, lineSize, state);
////        }
//        
//        public void SetBackgroundFillOffsetX(float offset, StyleState state) {
//            SetFloatProperty(StylePropertyId.BackgroundFillOffsetX, offset, state);
//        }
//        
//        public void SetBackgroundFillOffsetY(float offset, StyleState state) {
//            SetFloatProperty(StylePropertyId.BackgroundFillOffsetY, offset, state);
//        }
//        
//        public void SetBackgroundFillScaleX(float scale, StyleState state) {
//            SetFloatProperty(StylePropertyId.BackgroundFillScaleX, scale, state);
//        }
//        
//        public void SetBackgroundFillScaleY(float scale, StyleState state) {
//            SetFloatProperty(StylePropertyId.BackgroundFillScaleY, scale, state);
//        }
//        
//    }
//
//}