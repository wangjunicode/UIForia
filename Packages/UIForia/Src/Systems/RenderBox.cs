using UIForia.Elements;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public abstract class RenderBox {

        protected internal UIElement element;

        internal RenderBox firstChild;
        internal RenderBox nextSibling;
        public Visibility visibility;
        internal string uniqueId;
        public Overflow overflowX;
        public Overflow overflowY;
        public ClipBehavior clipBehavior;
        public bool clipped;
        public Rect clipRect;

        public abstract Rect RenderBounds { get; }

        public virtual void OnInitialize() { }

        public virtual void OnDestroy() { }

        public virtual void OnStylePropertyChanged(StructList<StyleProperty> propertyList) { }

        public abstract void PaintBackground(RenderContext ctx);

        public virtual void PaintForeground(RenderContext ctx) { }

        internal void Render(RenderContext renderContext) {
            
            if (!clipped) {
                PaintBackground(renderContext);
            }

            RenderBox ptr = firstChild;
            while (ptr != null) {
                ptr.Render(renderContext);
                ptr = ptr.nextSibling;
            }

            if (!clipped) {
                PaintForeground(renderContext);
            }
            
        }

    }

}