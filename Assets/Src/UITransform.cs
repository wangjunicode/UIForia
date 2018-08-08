using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Src {

    public class UITransform {

        public UIFlags flags;
        public int siblingIndex;
        public UITransform parent;
        public List<UITransform> children;

        public Vector2 position;
        public Vector2 scale;
        public Vector2 pivot;
        public float rotation;

        public float preferredWidth;
        public float minWidth;
        public float maxWidth;

        public float preferredHeight;
        public float minHeight;
        public float maxHeight;

    }

}