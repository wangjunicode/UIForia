using SVGX;
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

        public Size scrollbarHorizontalSize;
        public Size scrollbarVerticalSize;
        public Size contentSize;

        public Rect ScreenRect => new Rect(screenPosition, new Vector2(actualSize.width, actualSize.height));
        public Rect AllocatedRect => new Rect(screenPosition, new Vector2(allocatedSize.width, allocatedSize.height));

        public Rect LocalRect => new Rect(localPosition, new Vector2(actualSize.width, actualSize.height));

        public float AllocatedWidth => allocatedSize.width; // this should be size with padding & border already subtracted
        public float AllocatedHeight => allocatedSize.height;

        public float ActualWidth => actualSize.width;
        public float ActualHeight => actualSize.height;

        public float ContentWidth => allocatedSize.width - padding.left - border.left - padding.right - border.right;
        public float ContentHeight => allocatedSize.height - padding.top - border.top - padding.bottom - border.bottom;

        public Rect ContentRect => new Rect(
            padding.left + border.left,
            padding.top + border.top,
            allocatedSize.width - padding.left - border.left - padding.right - border.right,
            allocatedSize.height - padding.top - border.top - padding.bottom - border.bottom
        );


        public bool HasScrollbarVertical => scrollbarVerticalSize.IsDefined();
        public bool HasScrollbarHorizontal => scrollbarHorizontalSize.IsDefined();

        public LayoutResult() {
            scrollbarVerticalSize = Size.Unset;
            scrollbarHorizontalSize = Size.Unset;
            this.matrix = SVGXMatrix.identity;
        }

    }

}