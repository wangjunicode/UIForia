using Rendering;
using Src.Elements;
using UnityEngine;

namespace Src.Systems {

    public class RenderData : IHierarchical {

        public UIElement element;

        public VirtualScrollbar horizontalScrollbar;
        public VirtualScrollbar verticalScrollbar;
        public RectTransform horizontalScrollbarHandle;
        public RectTransform verticalScrollbarHandle;

        public IDrawable drawable;
        public GORenderSystem renderSystem;

        public RenderData(UIElement element, GORenderSystem renderSystem) {
            this.element = element;
            this.renderSystem = renderSystem;

            IDrawable graphicElement = element as IDrawable;
            if (graphicElement != null) {
                this.drawable = graphicElement;
                graphicElement.onMeshDirty += HandleMeshDirty;
                graphicElement.onMaterialDirty += HandleMaterialDirty;
            }
            else {
                this.drawable = new StandardDrawable(element);
            }
        }

        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

        public void HandleMeshDirty(IDrawable element) {
            renderSystem.MarkGeometryDirty(element);
        }

        public void HandleMaterialDirty(IDrawable element) {
            renderSystem.MarkMaterialDirty(element);
        }

        public void OnStylePropertyChanged(StyleProperty property) {
            if (element == drawable) {
                drawable.OnStylePropertyChanged(property);
            }
            else {
                if (property.propertyId == StylePropertyId.BorderRadiusTopLeft) { }
            }
        }

    }

}