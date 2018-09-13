using System;
using Rendering;
using Src.StyleBindings;
using System.Collections.Generic;
using System.Text;
using Src.Extensions;
using UnityEngine;

namespace Src.Systems {

    public delegate void PaintChanged(int elementId, Paint paint);

    public delegate void RectPropertyChanged(int elementId, Dimensions rect);

    public delegate void ContentBoxChanged(int elementId, ContentBoxRect rect);

    public delegate void ConstraintChanged(int elementId, LayoutConstraints constraints);

    public delegate void LayoutChanged(int elementId, LayoutParameters layoutParameters);

    public delegate void BorderRadiusChanged(int elementId, BorderRadius radius);

    public delegate void FontPropertyChanged(int elementId, TextStyle textStyle);

    public delegate void AvailableStatesChanged(int elementId, StyleState state);

    public delegate void TextContentChanged(int elementId, string text);

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
        public event Action<int, UITransform> onTransformChanged;
        
        private readonly IElementRegistry elementRegistry;
        private readonly SkipTree<UIElement> fontTree;

        public StyleSystem(IElementRegistry elementRegistry) {
            this.elementRegistry = elementRegistry;
            this.fontTree = new SkipTree<UIElement>();
        }

        public void OnReset() { }

        public void OnElementCreated(MetaData elementData) {
            UIElement element = elementData.element;

            if ((element.flags & UIElementFlags.TextElement) != 0) {
                ((UITextElement) element).onTextChanged += HandleTextChanged;
            }

            if ((element.flags & FlagCheck) != 0) {
                UITemplateContext context = elementData.context;
                List<UIStyle> baseStyles = elementData.baseStyles;
                List<StyleBinding> constantStyleBindings = elementData.constantStyleBindings;

                element.style = new UIStyleSet(element.id, this, this);
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
                OnElementCreated(elementData.children[i]);
            }
        }

        public void OnUpdate() { }

        public void OnDestroy() { }

        public void OnReady() { }

        public void OnInitialize() { }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        public void EnterState(int elementId, StyleState state) {
            elementRegistry.GetElement(elementId).style.EnterState(state);
        }

        public void ExitState(int elementId, StyleState state) {
            elementRegistry.GetElement(elementId).style.ExitState(state);
        }

        public void SetDimensions(int elementId, Dimensions rect) {
            onRectChanged?.Invoke(elementId, rect);
        }

        public void SetMargin(int elementId, ContentBoxRect margin) {
            onMarginChanged?.Invoke(elementId, margin);
        }

        public void SetPadding(int elementId, ContentBoxRect padding) {
            onPaddingChanged?.Invoke(elementId, padding);
        }

        public void SetBorder(int elementId, ContentBoxRect border) {
            onBorderChanged?.Invoke(elementId, border);
        }

        public void SetBorderRadius(int elementId, BorderRadius radius) {
            onBorderRadiusChanged?.Invoke(elementId, radius);
        }

        public void SetLayout(int elementId, LayoutParameters layoutParameters) {
            onLayoutChanged?.Invoke(elementId, layoutParameters);
        }

        public void SetConstraints(int elementId, LayoutConstraints constraints) {
            onConstraintChanged?.Invoke(elementId, constraints);
        }

        public void SetTextStyle(int elementId, TextStyle textStyle) {
            onFontPropertyChanged?.Invoke(elementId, textStyle);
        }

        public void SetPaint(int elementId, Paint paint) {
            onPaintChanged?.Invoke(elementId, paint);
        }

        public void SetAvailableStates(int elementId, StyleState availableStates) {
            onAvailableStatesChanged?.Invoke(elementId, availableStates);
        }

        public void SetTransform(int elementId, UITransform transform) {
            onTransformChanged?.Invoke(elementId, transform);
        }

        public UIStyleSet GetStyleForElement(int elementId) {
            return elementRegistry.GetElement(elementId).style;
        }

        // todo all nodes are currently in the font tree -- bad!
        internal int SetFontSize(int elementId, int fontSize) {
            UIElement element = elementRegistry.GetElement(elementId);

            if (!IntUtil.IsDefined(fontSize)) {
                fontSize = element.parent?.style.fontSize ?? UIStyle.Default.textStyle.fontSize;
            }

            ValueTuple<StyleSystem, int> v = ValueTuple.Create(this, fontSize);
            
            fontTree.ConditionalTraversePreOrder(element, v, (item, tuple) => {
                
                if (IntUtil.IsDefined(item.style.ownTextStyle.fontSize)) {
                    return false;
                }

                item.style.computedStyle.textStyle.fontSize = tuple.Item2;
                tuple.Item1.onFontPropertyChanged?.Invoke(item.id, item.style.textStyle);
                
                return true;
                
            });
            
            element.style.computedStyle.textStyle.fontSize = fontSize;
            return fontSize;
        }
        
        internal Color SetFontColor(int elementId, Color color) {
            UIElement element = elementRegistry.GetElement(elementId);

            if (!color.IsDefined()) {
                color = element.parent?.style.textColor ?? UIStyle.Default.textStyle.color;
            }

            ValueTuple<StyleSystem, Color> v = ValueTuple.Create(this, color);
            
            fontTree.ConditionalTraversePreOrder(element, v, (item, tuple) => {
                
                if (item.style.ownTextStyle.color.IsDefined()) {
                    return false;
                }

                item.style.computedStyle.textStyle.color = tuple.Item2;
                tuple.Item1.onFontPropertyChanged?.Invoke(item.id, item.style.textStyle);
                
                return true;
                
            });
            
            element.style.computedStyle.textStyle.color = color;
            return color;
        }

        private void HandleTextChanged(UITextElement element, string text) {
            element.style.textContent = text;
            onTextContentChanged?.Invoke(element.id, text);
        }

    }

}