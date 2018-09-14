using UnityEngine;
using UnityEngine.UI;

namespace Src.Systems {

    public class RenderData : IHierarchical {

        public UIElement element;
        public Graphic renderComponent;
        public RectTransform unityTransform;
        public RenderPrimitiveType primitiveType;
        
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