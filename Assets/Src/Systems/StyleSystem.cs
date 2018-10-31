using System;
using System.Collections.Generic;
using Rendering;
using Src.Animation;
using Src.Rendering;
using Src.StyleBindings;
using UnityEngine;

namespace Src.Systems {

    public class StyleSystem : IStyleSystem {

        protected readonly StyleAnimator animator;
        
        public event Action<UIElement, string> onTextContentChanged;
        public event Action<UIElement, StyleProperty> onStylePropertyChanged;

        public StyleSystem() {
            this.animator = new StyleAnimator();
        }

        public void PlayAnimation(UIStyleSet styleSet, StyleAnimation animation, AnimationOptions overrideOptions = default(AnimationOptions)) {
            int animationId = animator.PlayAnimation(styleSet, animation, overrideOptions);
        }

        public void SetViewportRect(Rect viewport) {
            animator.SetViewportRect(viewport);
        }

        public void OnReset() {
            animator.Reset();
        }

        public void OnElementCreatedFromTemplate(MetaData elementData) {
            UIElement element = elementData.element;


            if ((element.flags & UIElementFlags.TextElement) != 0) {
                ((UITextElement) element).onTextChanged += HandleTextChanged;
            }

            UITemplateContext context = elementData.context;
            List<UIStyleGroup> baseStyles = elementData.baseStyles;
            List<StyleBinding> constantStyleBindings = elementData.constantStyleBindings;

            element.style = new UIStyleSet(element, this);
            for (int i = 0; i < constantStyleBindings.Count; i++) {
                constantStyleBindings[i].Apply(element.style, context);
            }

            for (int i = 0; i < baseStyles.Count; i++) {
                element.style.AddStyleGroup(baseStyles[i]);
            }

            element.style.Initialize();


            for (int i = 0; i < elementData.children.Count; i++) {
                OnElementCreatedFromTemplate(elementData.children[i]);
            }
        }

        public void OnUpdate() {
            animator.OnUpdate();
        }

        public void OnDestroy() { }

        public void OnReady() { }

        public void OnInitialize() { }

        public void OnElementCreated(UIElement element) {
            if (element.style == null) {
                element.style = new UIStyleSet(element, this);
            }
        }

        public void OnElementMoved(UIElement element, int newIndex, int oldIndex) { }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        public void OnElementParentChanged(UIElement element, UIElement oldParent, UIElement newParent) {
            if (element.style == null) {
                element.style = new UIStyleSet(element, this);
            }
        }

        // todo -- buffer & flush these instead of doing it all at once
        public void SetStyleProperty(UIElement element, StyleProperty property) {
//            if (IsTextProperty(property.propertyId)) {
//                Stack<UIElement> stack = StackPool<UIElement>.Get();
//                
//                switch (property.propertyId) {
//                    case StylePropertyId.TextAnchor:
//                        break;
//                    case StylePropertyId.TextColor:
//                        stack.Push(element);
//                        Color color = property.AsColor;
//                        while (stack.Count > 0) {
//                            UIElement current = stack.Pop();
//                            if (!current.style.DefinesTextProperty(UIStyle.TextPropertyIdFlag.TextColor)) {
//                                current.style.SetInheritedTextColor(color);
//                            }
//                        }
//
//                        break;
//                    case StylePropertyId.TextAutoSize:
//                        break;
//                    case StylePropertyId.TextFontAsset:
//                        break;
//                    case StylePropertyId.TextFontSize:
//                        break;
//                    case StylePropertyId.TextFontStyle:
//                        break;
//                    case StylePropertyId.TextHorizontalOverflow:
//                        break;
//                    case StylePropertyId.TextVerticalOverflow:
//                        break;
//                    case StylePropertyId.TextWhitespaceMode:
//                        break;
//                }
//
//                StackPool<UIElement>.Release(stack);
//            }
            onStylePropertyChanged?.Invoke(element, property);
        }

        private static bool IsTextProperty(StylePropertyId propertyId) {
            int intId = (int) propertyId;
            const int start = (int) StylePropertyId.__TextPropertyStart__;
            const int end = (int) StylePropertyId.__TextPropertyEnd__;
            return intId > start && intId < end;
        }


        private void HandleTextChanged(UITextElement element, string text) {
            onTextContentChanged?.Invoke(element, text);
        }

    }

}