using UnityEngine;
using UnityEngine.UI;

namespace Src.Systems {

    public class RenderData : ISkipTreeTraversable {

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

        public void OnParentChanged(ISkipTreeTraversable newParent) {
            RenderData parent = (RenderData) newParent;
            if (parent == null) {
                unityTransform.SetParent(rootTransform);
            }
            else {
                unityTransform.SetParent(parent.unityTransform);
            }
        }

        public void OnBeforeTraverse() { }

        public void OnAfterTraverse() { }

        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

    }

}