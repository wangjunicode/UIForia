using UnityEngine;
using UnityEngine.UI;

namespace Src.Systems {

    public class RenderData : IHierarchical {

        public UIElement element;
        public RectTransform rootTransform;
        public RectTransform unityTransform;
        public Graphic maskComponent;
        public Graphic imageComponent;
        public RenderPrimitiveType primitiveType;
        
        public RenderData(UIElement element, RenderPrimitiveType primitiveType, RectTransform unityTransform, RectTransform rootTransform) {
            this.element = element;
            this.primitiveType = primitiveType;
            this.unityTransform = unityTransform;
            this.rootTransform = rootTransform;
        }

        // todo replae w/ callback on tree itself
//        public void OnParentChanged(ISkipTreeTraversable newParent) {
//            RenderData parent = (RenderData) newParent;
//            unityTransform.SetParent(parent == null ? rootTransform : parent.unityTransform);
//        }

        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

    }

}