using UnityEngine;

namespace Src.Systems {

    

    public struct Size {

        public float width;
        public float height;

        public Size(float width, float height) {
            this.width = width;
            this.height = height;
        }

        public static implicit operator Vector2(Size size) {
            return new Vector2(size.width, size.height);
        }
    }
    
    public struct LayoutResult {

        public readonly UIElement element;
        public Vector2 localPosition;
        public Vector2 screenPosition;
        public Size size;
        public Size contentSize;
        
        public LayoutResult(UIElement element) {
            this.element = element;
            this.localPosition = new Vector2();
            this.screenPosition = new Vector2();
            this.size = new Size();
            this.contentSize = new Size();
        }
        
        public Rect ScreenRect => new Rect(screenPosition, new Vector2(size.width, size.height));
        public Rect LocalRect => new Rect(localPosition, new Vector2(size.width, size.height));

        public float width => size.width;
        public float height => size.height;

        public float contentWidth => contentSize.width;
        public float contentHeight => contentSize.height;
        

    }

}