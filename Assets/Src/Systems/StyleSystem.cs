using Rendering;
using Src.StyleBindings;
using System.Collections.Generic;
using System.Text;

namespace Src.Systems {

    public delegate void PaintChanged(int elementId, Paint paint);

    public delegate void RectPropertyChanged(int elementId, LayoutRect rect);

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
        
        private readonly IElementRegistry elementRegistry;

        public StyleSystem(IElementRegistry elementRegistry) {
            this.elementRegistry = elementRegistry;
        }

        public void OnReset() { }

        public void OnElementCreated(InitData elementData) {
            UIElement element = elementData.element;

            if ((element.flags & UIElementFlags.TextElement) != 0) {
                ((UITextElement) element).onTextChanged += HandleTextChanged;
            }
            
            if ((element.flags & FlagCheck) != 0) {
                
                UITemplateContext context = elementData.context;
                List<UIStyle> baseStyles = elementData.baseStyles;
                List<StyleBinding> constantStyleBindings = elementData.constantStyleBindings;
                
                element.style = new UIStyleSet(element.id, this);
                for (var i = 0; i < constantStyleBindings.Count; i++) {
                    constantStyleBindings[i].Apply(element.style, context);
                }

                for (int i = 0; i < baseStyles.Count; i++) {
                    element.style.AddBaseStyle(baseStyles[i]);
                }

                element.style.Refresh();
            }

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

        public void SetRect(int elementId, LayoutRect rect) {
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

        public void SetText(int elementId, TextStyle textStyle) {
            onFontPropertyChanged?.Invoke(elementId, textStyle);
        }

        public void SetPaint(int elementId, Paint paint) {
            onPaintChanged?.Invoke(elementId, paint);
        }

        public void SetAvailableStates(int elementId, StyleState availableStates) {
            onAvailableStatesChanged?.Invoke(elementId, availableStates);
        }

        public UIStyleSet GetStyleForElement(int elementId) {
            return elementRegistry.GetElement(elementId).style;
        }

        private void HandleTextChanged(UITextElement element, string text) {
//            WhitespaceMode whiteSpace = element.style.textStyle.whiteSpace;
//            StringBuilder builder = new StringBuilder();
//            int ptr = 0;
//            bool collapsing = false;
//            text = text.Trim();
//            while (ptr < text.Length) {
//                if (char.IsWhiteSpace(text[ptr])) {
//                    if (!collapsing) {
//                        collapsing = true;
//                        builder.Append(text[ptr]);
//                    }
//                }
//                else {
//                    collapsing = false;
//                    builder.Append(text[ptr]);
//                }
//                ptr++;
//            }
            onTextContentChanged?.Invoke(element.id, text);
        }
    }

}