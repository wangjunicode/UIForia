using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public class LayoutData : ISkipTreeTraversable {

        public float x;
        public float y;
        public float width;
        public float height;
        public float minWidth;
        public float maxWidth;
        public float minHeight;
        public float maxHeight;

        public bool isInFlow;
        public UILayout layout;
        public LayoutData parent;
        public UIElement element;
        public UIUnit relativeToWidth;
        public UIUnit relativeToHeight;
        public List<LayoutData> children;
        public LayoutDirection layoutDirection;

        public LayoutData(UIElement element) {
            this.children = new List<LayoutData>();
            this.element = element;
            this.isInFlow = true;
        }

        public float GetFixedWidth() {
            switch (relativeToWidth) {
                case UIUnit.Fixed:
                    return width;
                case UIUnit.Parent:
                    
            }
        }

        public Rect layoutRect => new Rect(x, y, width, height);

        public IHierarchical Element => this;
        public IHierarchical Parent => parent;

        public void OnParentChanged(ISkipTreeTraversable newParent) {
            parent = (LayoutData) newParent;
        }

        void ISkipTreeTraversable.OnBeforeTraverse() {
            children.Clear();
        }

        void ISkipTreeTraversable.OnAfterTraverse() {
            parent?.children.Add(this);
        }

    }

}