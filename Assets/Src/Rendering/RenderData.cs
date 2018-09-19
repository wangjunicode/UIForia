using Src.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Systems {

    public class RenderData : IHierarchical {

        public UIElement element;
        public Graphic renderComponent;
        public RectTransform unityTransform;
        public RenderPrimitiveType primitiveType;
        public VirtualScrollbar horizontalScrollbar;
        public VirtualScrollbar verticalScrollbar;
        public RectMask2D mask;
        public RectTransform horizontalScrollbarHandle;
        public RectTransform verticalScrollbarHandle;
        
        public RenderData(UIElement element, RenderPrimitiveType primitiveType, RectTransform unityTransform) {
            this.element = element;
            this.primitiveType = primitiveType;
            this.unityTransform = unityTransform;
        }

        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

    }

}