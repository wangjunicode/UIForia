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
            for (int i = 0; i < properties.size; i++) {
                ref StyleProperty property = ref properties.array[i];
                // todo -- these flags can maybe probably be baked into setting the property
                switch (property.propertyId) {
                    case StylePropertyId.LayoutType:
                        element.flags |= UIElementFlags.LayoutTypeDirty;
                        break;
                    case StylePropertyId.LayoutBehavior:
                        element.flags |= UIElementFlags.LayoutBehaviorDirty;
                        break;
                    case StylePropertyId.TransformRotation:
                    case StylePropertyId.TransformPositionX:
                    case StylePropertyId.TransformPositionY:
                    case StylePropertyId.TransformScaleX:
                    case StylePropertyId.TransformScaleY:
                        element.flags |= UIElementFlags.LayoutTransformDirty;
                        break;
                    case StylePropertyId.AlignmentBehaviorX:
                    case StylePropertyId.AlignmentOriginX:
                    case StylePropertyId.AlignmentOffsetX:
                        element.flags |= UIElementFlags.LayoutAlignmentWidthDirty;
                        break;
                    case StylePropertyId.AlignmentBehaviorY:
                    case StylePropertyId.AlignmentOriginY:
                    case StylePropertyId.AlignmentOffsetY:
                        element.flags |= UIElementFlags.LayoutAlignmentHeightDirty;
                        break;
                    case StylePropertyId.MinWidth:
                    case StylePropertyId.MaxWidth:
                    case StylePropertyId.PreferredWidth:
                        element.flags |= UIElementFlags.LayoutSizeWidthDirty;
                        element.awesomeLayoutBox?.UpdateBlockProviderWidth();
                        break;
                    case StylePropertyId.MinHeight:
                    case StylePropertyId.MaxHeight:
                    case StylePropertyId.PreferredHeight:
                        element.flags |= UIElementFlags.LayoutSizeHeightDirty;
                        element.awesomeLayoutBox?.UpdateBlockProviderHeight();
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

            if (element.awesomeLayoutBox != null) {
                element.awesomeLayoutBox.OnStyleChanged(properties);
                // don't need to null check since root box will never have a style changed
                element.awesomeLayoutBox.OnChildStyleChanged(element.awesomeLayoutBox, properties);
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
            throw new NotImplementedException("Todo");
        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void OnElementCreated(UIElement element) { }

        public IList<UIElement> QueryPoint(Vector2 point, IList<UIElement> retn) {
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