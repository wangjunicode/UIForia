using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class AwesomeLayoutSystem : ILayoutSystem {

        private Application application;
        private LightList<AwesomeLayoutRunner> runners;

        public AwesomeLayoutSystem(Application application) {
            this.application = application;
            this.runners = new LightList<AwesomeLayoutRunner>();

            for (int i = 0; i < application.m_Views.Count; i++) {
                runners.Add(new AwesomeLayoutRunner(application.m_Views[i].rootElement));
            }

            application.StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
        }

        private void HandleStylePropertyChanged(UIElement element, StructList<StyleProperty> properties) {
            bool checkAlignHorizontal = false;
            bool updateAlignVertical = false;
            bool updateTransform = false;
            for (int i = 0; i < properties.size; i++) {
                ref StyleProperty property = ref properties.array[i];
                // todo -- these flags can maybe probably be baked into setting the property
                switch (property.propertyId) {
                    
                    case StylePropertyId.ClipBehavior:
                        
                        if (element.layoutBox != null) {
                            element.layoutBox.flags |= LayoutBoxFlags.RecomputeClipping;
                            element.layoutBox.clipBehavior = property.AsClipBehavior;
                        }
                        break;
                    
                    case StylePropertyId.ClipBounds:
                        
                        break;
                    case StylePropertyId.OverflowX:
                    case StylePropertyId.OverflowY:
                        element.layoutBox?.UpdateClipper();
                        break;

                    case StylePropertyId.LayoutType:
                    case StylePropertyId.LayoutBehavior:
                        element.flags |= UIElementFlags.LayoutTypeOrBehaviorDirty;
                        break;
                    case StylePropertyId.TransformRotation:
                    case StylePropertyId.TransformPositionX:
                    case StylePropertyId.TransformPositionY:
                    case StylePropertyId.TransformScaleX:
                    case StylePropertyId.TransformScaleY:
                    case StylePropertyId.TransformPivotX:
                    case StylePropertyId.TransformPivotY:
                        updateTransform = true;
                        break;

                    case StylePropertyId.AlignmentTargetX:
                    case StylePropertyId.AlignmentOriginX:
                    case StylePropertyId.AlignmentOffsetX:
                    case StylePropertyId.AlignmentDirectionX:
                        checkAlignHorizontal = true;
                        break;
                    case StylePropertyId.AlignmentTargetY:
                    case StylePropertyId.AlignmentOriginY:
                    case StylePropertyId.AlignmentOffsetY:
                    case StylePropertyId.AlignmentDirectionY:
                        updateAlignVertical = true;
                        break;
                    case StylePropertyId.MinWidth:
                    case StylePropertyId.MaxWidth:
                    case StylePropertyId.PreferredWidth:
                        element.flags |= UIElementFlags.LayoutSizeWidthDirty;
                        element.layoutBox?.UpdateBlockProviderWidth();
                        break;
                    case StylePropertyId.MinHeight:
                    case StylePropertyId.MaxHeight:
                    case StylePropertyId.PreferredHeight:
                        element.flags |= UIElementFlags.LayoutSizeHeightDirty;
                        element.layoutBox?.UpdateBlockProviderHeight();
                        break;
                    case StylePropertyId.PaddingLeft:
                    case StylePropertyId.PaddingRight:
                    case StylePropertyId.BorderLeft:
                    case StylePropertyId.BorderRight:
                        element.flags |= UIElementFlags.LayoutBorderPaddingHorizontalDirty;
                        break;
                    case StylePropertyId.PaddingTop:
                    case StylePropertyId.PaddingBottom:
                    case StylePropertyId.BorderTop:
                    case StylePropertyId.BorderBottom:
                        element.flags |= UIElementFlags.LayoutBorderPaddingVerticalDirty;
                        break;
                    case StylePropertyId.LayoutFitHorizontal:
                        element.flags |= UIElementFlags.LayoutFitWidthDirty;
                        break;
                    case StylePropertyId.LayoutFitVertical:
                        element.flags |= UIElementFlags.LayoutFitHeightDirty;
                        break;
                }
            }

            if (updateTransform) {
                float rotation = element.style.TransformRotation;
                float scaleX = element.style.TransformScaleX;
                float scaleY = element.style.TransformScaleY;
                float positionX = element.style.TransformPositionX.value;
                float positionY = element.style.TransformPositionY.value;

                if (rotation != 0 || scaleX != 1 || scaleY != 1 || positionX != 0 || positionY != 0) {
                    element.flags |= UIElementFlags.LayoutTransformNotIdentity;
                }
                else {
                    element.flags &= ~UIElementFlags.LayoutTransformNotIdentity;
                }

                element.flags |= UIElementFlags.LayoutTransformDirty;
            }

            AwesomeLayoutBox layoutBox = element.layoutBox;
            if (layoutBox != null) {
                if (checkAlignHorizontal) {
                    layoutBox.UpdateRequiresHorizontalAlignment();
                }

                if (updateAlignVertical) {
                    layoutBox.UpdateRequiresVerticalAlignment();
                }

                layoutBox.OnStyleChanged(properties);
                // don't need to null check since root box will never have a style changed
                layoutBox.OnChildStyleChanged(element.layoutBox, properties);
            }
        }

        public void OnReset() { }

        public void OnUpdate() {
            // flags
            // width dirty
            // height dirty
            // box model dirty 
            // hierarchy dirty
            // layout type dirty
            // layout behavior dirty
            // transform dirty
            // alignment dirty
            // fit dirty

            for (int i = 0; i < runners.size; i++) {
                runners[i].RunLayout();
            }
        }

        // ideally we have gatherers and runners
        // each view and each ignored element can have its own runner
        // run all views then run all ignored

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
            runners.Add(new AwesomeLayoutRunner(view.rootElement));
        }

        public void OnViewRemoved(UIView view) {
            for (int i = 0; i < runners.size; i++) {
                if (runners[i].rootElement == view.rootElement) {
                    runners.RemoveAt(i);
                    return;
                }
            }
        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void OnElementCreated(UIElement element) { }

        public IList<UIElement> QueryPoint(Vector2 point, IList<UIElement> retn) {
            for (int i = 0; i < runners.size; i++) {
                runners[i].QueryPoint(point, retn);
            }

            return retn;
        }

        public LightList<UIElement> GetVisibleElements(LightList<UIElement> retn = null) {
            return default;
        }

        public AwesomeLayoutRunner GetLayoutRunner(UIElement viewRoot) {
            return runners[0]; // todo -- implement for real
        }

    }

}