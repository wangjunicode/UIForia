using SVGX;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout {

    public class LayoutResult {

        public float localRotation;
        public Vector2 localScale;
        public Vector2 localPosition;

        public Vector2 scale;
        public Vector2 screenPosition;
        public Vector2 pivot;

        public Size actualSize;
        public Size allocatedSize;

        public int zIndex;
        public Rect clipRect;
        public float rotation;
        public CullResult cullState;

        public OffsetRect border;
        public OffsetRect padding;
        public OffsetRect margin;
        public ResolvedBorderRadius borderRadius;

        public SVGXMatrix matrix;
        public SVGXMatrix localMatrix;
        public Vector2 pivotOffset;

        public Vector2 allocatedPosition; // where the parent told this element to be
        public Vector2 alignedPosition;   // where the element wants to be (might be relative to allocated, might not be) 
        // local position = actual position post transform
        
        public Rect ScreenRect => new Rect(screenPosition, new Vector2(actualSize.width, actualSize.height));
        public Rect AllocatedRect => new Rect(allocatedPosition, new Vector2(allocatedSize.width, allocatedSize.height));

        public Rect LocalRect => new Rect(localPosition, new Vector2(actualSize.width, actualSize.height));

        public float AllocatedWidth => allocatedSize.width; // this should be size with padding & border already subtracted
        public float AllocatedHeight => allocatedSize.height;

        public float ActualWidth => actualSize.width;
        public float ActualHeight => actualSize.height;

        public float ContentWidth => allocatedSize.width - padding.left - border.left - padding.right - border.right;
        public float ContentHeight => allocatedSize.height - padding.top - border.top - padding.bottom - border.bottom;

        public LayoutResult layoutParent;
        public UIElement element;

        public Rect ContentRect => new Rect(
            padding.left + border.left,
            padding.top + border.top,
            actualSize.width - padding.left - border.left - padding.right - border.right,
            actualSize.height - padding.top - border.top - padding.bottom - border.bottom
        );

        internal LayoutResult(UIElement element) {
            this.element = element;
            this.matrix = SVGXMatrix.identity;
        }

    }

}