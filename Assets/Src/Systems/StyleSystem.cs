using System;
using System.Collections.Generic;
using System.Linq;
using Rendering;
using Src.StyleBindings;

namespace Src.Systems {

    public delegate void PaintChanged(int elementId, Paint paint);

    public delegate void RectPropertyChanged(int elementId, LayoutRect rect);

    public delegate void ContentBoxChanged(int elementId, ContentBoxRect rect);

    public delegate void ConstraintChanged(int elementId, LayoutConstraints constraints);

    public delegate void LayoutChanged(int elementId, LayoutParameters layoutParameters);

    public delegate void BorderRadiusChanged(int elementId, BorderRadius radius);

    public delegate void FontPropertyChanged(int elementId, TextStyle textStyle);

    public delegate void AvailableStatesChanged(int elementId, StyleState state);

    public class StyleSystem : ISystem, IStyleSystem {

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

        private readonly Dictionary<int, UIStyleSet> styleMap;

        public StyleSystem() {
            this.styleMap = new Dictionary<int, UIStyleSet>();
        }

        public void OnReset() {
            styleMap.Clear();
        }

        public void OnUpdate() { }

        public void OnDestroy() { }

        public void OnInitialize() { }

        public void OnElementCreated(UIElementCreationData elementData) {
            UIElement element = elementData.element;
            UITemplateContext context = elementData.context;

            List<UIStyle> baseStyles = elementData.baseStyles;
            List<StyleBinding> constantStyleBindings = elementData.constantStyleBindings;

            // todo -- this will create a style for all elements, 
            // we can optimize this away w/ flags
            element.style = new UIStyleSet(element.id, this);

            styleMap[element.id] = element.style;

            for (var i = 0; i < constantStyleBindings.Count; i++) {
                constantStyleBindings[i].Apply(element.style, context);
            }

            for (int i = 0; i < baseStyles.Count; i++) {
                element.style.AddBaseStyle(baseStyles[i]);
            }

            element.style.Refresh();

        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) {
            styleMap.Remove(element.id);
        }

        public IReadOnlyList<UIStyleSet> GetAllStyles() {
            return styleMap.ToList().Select((kvp) => kvp.Value).ToList();
        }

        public IReadOnlyList<UIStyleSet> GetActiveStyles() {
            // todo -- don't return for disabled elements
            return styleMap.ToList().Select((kvp) => kvp.Value).ToList();
        }

        public void EnterState(int elementId, StyleState state) {
            styleMap[elementId].EnterState(state);    
        }

        public void ExitState(int elementId, StyleState state) {
            styleMap[elementId].ExitState(state);
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
            UIStyleSet style;
            styleMap.TryGetValue(elementId, out style);
            return style;
        }

    }

}