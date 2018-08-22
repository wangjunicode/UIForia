using System;
using System.Collections.Generic;
using System.Linq;
using Rendering;
using Src.Layout;
using UnityEngine;

namespace Src.Systems {

    public delegate void PaintChanged(int elementId, Paint paint);

    public delegate void RectPropertyChanged(int elementId, LayoutRect rect);

    public delegate void ContentBoxChanged(int elementId, ContentBoxRect rect);
    
    public delegate void ConstraintChanged(int elementId, LayoutConstraints constraints);

    public delegate void LayoutChanged(int elementId, LayoutParameters layoutParameters);

    public class StyleSystem : ISystem, IStyleSystem {

        public event PaintChanged onPaintChanged;
        public event LayoutChanged onLayoutChanged;
        public event RectPropertyChanged onRectChanged;
        public event ContentBoxChanged onMarginChanged;
        public event ContentBoxChanged onBorderChanged;
        public event ContentBoxChanged onPaddingChanged;
        public event ConstraintChanged onConstraintChanged;

        private readonly Dictionary<int, UIStyleSet> styleMap;

        public StyleSystem() {
            this.styleMap = new Dictionary<int, UIStyleSet>();
        }

        public void OnReset() {
            styleMap.Clear();
        }

        public void OnUpdate() { }

        public void OnDestroy() { }

        public void OnElementCreated(UIElementCreationData elementData) {
            UIElement element = elementData.element;
            StyleDefinition style = elementData.style;
            UITemplateContext context = elementData.context;

            if (style == null) return;

            element.style = new UIStyleSet(element.id, this);

            styleMap[element.id] = element.style;

            if (style.constantBindings != null) {
                for (var i = 0; i < style.constantBindings.Length; i++) {
                    style.constantBindings[i].Apply(element.style, context);
                }
            }

            // todo -- maybe this is where external style paths get resolved
            if (style.baseStyles != null) {
                for (int i = 0; i < style.baseStyles.Count; i++) {
                    element.style.AddBaseStyle(style.baseStyles[i]);
                }
            }
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
            throw new NotImplementedException();
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

        public void SetLayout(int elementId, LayoutParameters layoutParameters) {
            onLayoutChanged?.Invoke(elementId, layoutParameters);
        }

        public void SetConstraints(int elementId, LayoutConstraints constraints) {
            onConstraintChanged?.Invoke(elementId, constraints);
        }

        public void SetPaint(int elementId, Paint paint) {
            onPaintChanged?.Invoke(elementId, paint);
        }

    }

}