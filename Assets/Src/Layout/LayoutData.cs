using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public class LayoutData {

        public float x;
        public float y;
        public float width;
        public float height;
        public float minWidth;
        public float maxWidth;
        public float minHeight;
        public float maxHeight;
        public LayoutData parent;
        public List<LayoutData> children;
        public UIUnit relativeToWidth;
        public UIUnit relativeToHeight;
        public int elementId;
        public bool isInFlow;

        public LayoutData(LayoutData parent, int elementId) {
            this.parent = parent;
            this.children = new List<LayoutData>();
            this.elementId = elementId;
            this.isInFlow = true;
            parent?.children.Add(this);
        }

        public Rect layoutRect => new Rect(x, y, width, height);

    }

}