using System;
using Rendering;
using Src.StyleBindings;
using System.Collections.Generic;
using Src.Extensions;
using UnityEngine;

namespace Src.Systems {

    public delegate void PaintChanged(UIElement element, Paint paint);

    public delegate void RectPropertyChanged(UIElement element, Dimensions rect);

    public delegate void ContentBoxChanged(UIElement element, ContentBoxRect rect);

    public delegate void ConstraintChanged(UIElement element, LayoutConstraints constraints);

    public delegate void LayoutChanged(UIElement element, LayoutParameters layoutParameters);

    public delegate void BorderRadiusChanged(UIElement element, BorderRadius radius);

    public delegate void FontPropertyChanged(UIElement element, TextStyle textStyle);

    public delegate void AvailableStatesChanged(UIElement element, StyleState state);

    public delegate void TextContentChanged(UIElement element, string text);

    public class StyleSystem : ISystem, IStyleSystem {

        private const UIElementFlags FlagCheck = UIElementFlags.RequiresRendering | UIElementFlags.RequiresLayout | UIElementFlags.TextElement;

        public event PaintChanged onPaintChanged;
        public event LayoutChanged onLayoutChanged;
        public event RectPropertyChanged onRectChanged;
        public event ContentBoxChanged onMarginChanged;
        public event ContentBoxChanged onBorderChanged;
        public event ContentBoxChanged onPaddingChanged;
        public event ConstraintChanged onConstraintChanged;
        public event BorderRadiusChanged onBorderRadiusChanged;
        public event FontPropertyChanged onFontPropertyChanged;
        public event AvailableStatesChanged onAvailableStatesChanged;
        public event TextContentChanged onTextContentChanged;
        public event Action<UIElement, UITransform> onTransformChanged;
        public event Action<UIElement> onOverflowPropertyChanged;
        
        private readonly SkipTree<UIElement> fontTree;

        public StyleSystem() {
            this.fontTree = new SkipTree<UIElement>();
        }

        public void OnReset() { }

        public void OnElementCreatedFromTemplate(MetaData elementData) {
            UIElement element = elementData.element;

            if ((element.flags & UIElementFlags.TextElement) != 0) {
                ((UITextElement) element).onTextChanged += HandleTextChanged;
            }

            if ((element.flags & FlagCheck) != 0) {
                UITemplateContext context = elementData.context;
                List<UIStyle> baseStyles = elementData.baseStyles;
                List<StyleBinding> constantStyleBindings = elementData.constantStyleBindings;

                element.style = new UIStyleSet(element, this, this);
                for (var i = 0; i < constantStyleBindings.Count; i++) {
                    constantStyleBindings[i].Apply(element.style, context);
                }

                for (int i = 0; i < baseStyles.Count; i++) {
                    element.style.AddBaseStyle(baseStyles[i]);
                }

                element.style.Refresh();
            }

            // todo -- probably not the right move
            fontTree.AddItem(element);

            for (int i = 0; i < elementData.children.Count; i++) {
                OnElementCreatedFromTemplate(elementData.children[i]);
            }
        }

        public void OnUpdate() { }

        public void OnDestroy() { }

        public void OnReady() { }

        public void OnInitialize() { }

        public void OnElementCreated(UIElement element) {
            if (element.style == null) {
                element.style = new UIStyleSet(element, this, this);
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
                element.style = new UIStyleSet(element, this, this);
            }
        }


        public void SetDimensions(UIElement element, Dimensions rect) {
            onRectChanged?.Invoke(element, rect);
        }

        public void SetMargin(UIElement element, ContentBoxRect margin) {
            onMarginChanged?.Invoke(element, margin);
        }

        public void SetPadding(UIElement element, ContentBoxRect padding) {
            onPaddingChanged?.Invoke(element, padding);
        }

        public void SetBorder(UIElement element, ContentBoxRect border) {
            onBorderChanged?.Invoke(element, border);
        }

        public void SetBorderRadius(UIElement element, BorderRadius radius) {
            onBorderRadiusChanged?.Invoke(element, radius);
        }

        public void SetLayout(UIElement element, LayoutParameters layoutParameters) {
            onLayoutChanged?.Invoke(element, layoutParameters);
        }

        public void SetConstraints(UIElement element, LayoutConstraints constraints) {
            onConstraintChanged?.Invoke(element, constraints);
        }

        public void SetTextStyle(UIElement element, TextStyle textStyle) {
            onFontPropertyChanged?.Invoke(element, textStyle);
        }

        public void SetPaint(UIElement element, Paint paint) {
            onPaintChanged?.Invoke(element, paint);
        }

        public void SetAvailableStates(UIElement element, StyleState availableStates) {
            onAvailableStatesChanged?.Invoke(element, availableStates);
        }

        public void SetTransform(UIElement element, UITransform transform) {
            onTransformChanged?.Invoke(element, transform);
        }

        // todo all nodes are currently in the font tree -- bad!
        internal int SetFontSize(UIElement element, int fontSize) {
            if (!IntUtil.IsDefined(fontSize)) {
                fontSize = element.parent?.style.fontSize ?? UIStyle.Default.textStyle.fontSize;
            }

            ValueTuple<StyleSystem, int> v = ValueTuple.Create(this, fontSize);

            fontTree.ConditionalTraversePreOrder(element, v, (item, tuple) => {
                if (IntUtil.IsDefined(item.style.ownTextStyle.fontSize)) {
                    return false;
                }

                item.style.computedStyle.textStyle.fontSize = tuple.Item2;
                tuple.Item1.onFontPropertyChanged?.Invoke(item, item.style.textStyle);

                return true;
            });

            element.style.computedStyle.textStyle.fontSize = fontSize;
            return fontSize;
        }

        internal Color SetFontColor(UIElement element, Color color) {
            if (!color.IsDefined()) {
                color = element.parent?.style.textColor ?? UIStyle.Default.textStyle.color;
            }

            ValueTuple<StyleSystem, Color> v = ValueTuple.Create(this, color);

            fontTree.ConditionalTraversePreOrder(element, v, (item, tuple) => {
                if (item.style.ownTextStyle.color.IsDefined()) {
                    return false;
                }

                item.style.computedStyle.textStyle.color = tuple.Item2;
                tuple.Item1.onFontPropertyChanged?.Invoke(item, item.style.textStyle);

                return true;
            });

            element.style.computedStyle.textStyle.color = color;
            return color;
        }

        private void HandleTextChanged(UITextElement element, string text) {
            element.style.textContent = text;
            onTextContentChanged?.Invoke(element, text);
        }

    }

}