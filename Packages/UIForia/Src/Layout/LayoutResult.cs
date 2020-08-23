using SVGX;
using UIForia.Systems;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Layout {

    public unsafe struct LayoutResult {

        // transform info

        // alignment info

        // width info
        // height info
        // clip info? bounds
        // layer info (zindex + layer)
        // public float localRotation;
        // public Vector2 localScale;
        // public Vector2 localPosition;

        public SVGXMatrix matrix;
        public Vector2 pivotOffset;


        public OrientedBounds orientedBounds;
//         public Vector4 axisAlignedBounds;

        public bool isCulled;

        public readonly ElementId elementId;
        private readonly LayoutSystem.LayoutDataTables* layoutSystemTablePointers;

        internal LayoutResult(ElementId elementId, LayoutSystem.LayoutDataTables* layoutSystemTablePointers) : this() {
            this.elementId = elementId;
            this.layoutSystemTablePointers = layoutSystemTablePointers;
        }

        public Size actualSize {
            get {
                float2 actual = layoutSystemTablePointers->layoutBoxInfo[elementId.index].actualSize;
                return new Size(actual.x, actual.y);
            }
        }

        public Size allocatedSize {
            get {
                float2 allocated = layoutSystemTablePointers->layoutBoxInfo[elementId.index].allocatedSize;
                return new Size(allocated.x, allocated.y);
            }
        }

        public Vector2 allocatedPosition {
            get => layoutSystemTablePointers->layoutBoxInfo[elementId.id].allocatedPosition;
        }
        
        public Vector2 alignedPosition {
            get => layoutSystemTablePointers->layoutBoxInfo[elementId.index].alignedPosition;
        }

        // todo -- return actual element since this is user facing code
        public ElementId layoutParent {
            get => layoutSystemTablePointers->layoutBoxInfo[elementId.index].layoutParentId;
        }

        public OffsetRect margin {
            get => layoutSystemTablePointers->layoutBoxInfo[elementId.index].margin;
        }

        public OffsetRect border {
            get => layoutSystemTablePointers->layoutBoxInfo[elementId.index].border;
        }

        public OffsetRect padding {
            get => layoutSystemTablePointers->layoutBoxInfo[elementId.index].padding;
        }

        public Vector2 localPosition {
            get {
                float x = layoutSystemTablePointers->localMatrices[elementId.index].c3.x;
                float y = -layoutSystemTablePointers->localMatrices[elementId.index].c3.y;
                return new Vector2(x, y);
            }
        }

        public Vector2 screenPosition {
            get {
                float x = layoutSystemTablePointers->worldMatrices[elementId.index].c3.x;
                float y = -layoutSystemTablePointers->worldMatrices[elementId.index].c3.y;
                return new Vector2(x, y);
            }
        }

        public Matrix4x4 GetWorldMatrix() {
            return layoutSystemTablePointers->worldMatrices[elementId.index];
        }

        public Rect ContentRect => new Rect(
            padding.left + border.left,
            padding.top + border.top,
            actualSize.width - padding.left - border.left - padding.right - border.right,
            actualSize.height - padding.top - border.top - padding.bottom - border.bottom
        );

        public float VerticalPaddingBorderStart => padding.top + border.top;
        public float VerticalPaddingBorderEnd => padding.bottom + border.bottom;
        public float HorizontalPaddingBorderStart => padding.left + border.left;
        public float HorizontalPaddingBorderEnd => padding.right + border.right;
        
        public float HorizontalPaddingBorder => padding.right + border.right + padding.left + border.left;
        public float VerticalPaddingBorder => padding.bottom + border.bottom + padding.top + border.top;

        public Rect ScreenRect => new Rect(screenPosition, new Vector2(actualSize.width, actualSize.height));
        public Rect AllocatedRect => new Rect(allocatedPosition, new Vector2(allocatedSize.width, allocatedSize.height));

        public Rect LocalRect => new Rect(alignedPosition, new Vector2(actualSize.width, actualSize.height));

        public float AllocatedWidth => allocatedSize.width; // this should be size with padding & border already subtracted
        public float AllocatedHeight => allocatedSize.height;

        public float ActualWidth => actualSize.width;
        public float ActualHeight => actualSize.height;

        public float ContentAreaWidth {
            get => layoutSystemTablePointers->layoutBoxInfo[elementId.index].ContentAreaWidth;
        }

        public float ContentAreaHeight {
            get => layoutSystemTablePointers->layoutBoxInfo[elementId.index].ContentAreaHeight;
        }


    }

}