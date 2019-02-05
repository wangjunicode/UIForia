using System;
using UnityEngine;

namespace UIForia.Systems {

    public struct LayoutResult {

        private LayoutResultFlags flags;

        public Vector2 scale;
        public Vector2 localPosition;
        public Vector2 screenPosition;
        public Vector2 pivot;

        public Size actualSize;
        public Size allocatedSize;
        public Rect contentRect;

        public int zIndex;
        public Rect clipRect;
        public float rotation;

        public Rect ScreenRect => new Rect(screenPosition, new Vector2(allocatedSize.width, allocatedSize.height));
        public Rect ScreenOverflowRect => new Rect(screenPosition, new Vector2(actualSize.width, actualSize.height));

        public Rect LocalRect => new Rect(localPosition, new Vector2(allocatedSize.width, allocatedSize.height));
        public Rect LocalOverflowRect => new Rect(localPosition, new Vector2(actualSize.width, actualSize.height));

        public float AllocatedWidth => allocatedSize.width;
        public float AllocatedHeight => allocatedSize.height;
        
        public float ActualWidth => actualSize.width;
        public float ActualHeight => actualSize.height;

        public float ContentWidth => contentRect.width;
        public float ContentHeight => contentRect.height;
        
        public bool SizeChanged => (flags & LayoutResultFlags.SizeChanged) != 0;
        public bool TransformChanged => (flags & LayoutResultFlags.TransformChanged) != 0;
        public bool PositionChanged => (flags & LayoutResultFlags.PositionChanged) != 0;
        
        public Size AllocatedSize {
            get { return allocatedSize; }
            internal set {
                if (value != allocatedSize) {
                    allocatedSize = value;
                    flags |= LayoutResultFlags.AllocatedSizeChanged;
                }
                else {
                    flags &= ~LayoutResultFlags.AllocatedSizeChanged;
                }
            }
        }

        public Size ActualSize {
            get { return actualSize; }
            internal set {
                if (value != actualSize) {
                    actualSize = value;
                    flags |= LayoutResultFlags.ActualSizeChanged;
                }
                else {
                    flags &= ~LayoutResultFlags.ActualSizeChanged;
                }
            }
        }

        public Vector2 LocalPosition {
            get { return localPosition; }
            internal set {
                if (value != localPosition) {
                    localPosition = value;
                    flags |= LayoutResultFlags.LocalPositionChanged;
                }
                else {
                    flags &= ~LayoutResultFlags.LocalPositionChanged;
                }
            }
        }

        public Vector2 ScreenPosition {
            get { return screenPosition; }
            internal set {
                if (value != screenPosition) {
                    screenPosition = value;
                    flags |= LayoutResultFlags.ScreenPositionChanged;
                }
                else {
                    flags &= ~LayoutResultFlags.ScreenPositionChanged;
                }
            }
        }

//        public Vector2 ContentOffset {
//            get { return contentOffset; }
//            internal set {
//                if (value != contentOffset) {
//                    contentOffset = value;
//                    flags |= LayoutResultFlags.ContentRectChanged;
//                }
//                else {
//                    flags &= ~LayoutResultFlags.ContentRectChanged;
//                }
//            }
//        }

        public Vector2 Scale {
            get { return scale; }
            internal set {
                if (value != scale) {
                    scale = value;
                    flags |= LayoutResultFlags.ScaleChanged;
                }
                else {
                    flags &= ~LayoutResultFlags.ScaleChanged;
                }
            }
        }

        public float Rotation {
            get { return rotation; }
            internal set {
                float r = value;
                r = (r) % 360;
                if (r < 0) r += 360;
                if (!Mathf.Approximately(rotation, r)) {
                    rotation = r;
                    flags |= LayoutResultFlags.RotationChanged;
                }
                else {
                    flags &= ~LayoutResultFlags.RotationChanged;
                }
            }
        }

        public int ZIndex {
            get { return zIndex; }
            internal set {
                if (zIndex != value) {
                    zIndex = value;
                    flags |= LayoutResultFlags.ZIndexChanged;
                }
                else {
                    flags &= ~LayoutResultFlags.ZIndexChanged;
                }
            }
        }

        public bool AllocateSizeChanged {
            get { return (flags & LayoutResultFlags.AllocatedSizeChanged) != 0; }
            internal set {
                if (value) {
                    flags |= LayoutResultFlags.AllocatedSizeChanged;
                }
            }
        }

        public bool ActualSizeChanged {
            get { return (flags & LayoutResultFlags.ActualSizeChanged) != 0; }
            internal set {
                if (value) {
                    flags |= LayoutResultFlags.ActualSizeChanged;
                }
            }
        }

        public bool LayerChanged {
            get { return (flags & LayoutResultFlags.LayerChanged) != 0; }
            internal set {
                if (value) {
                    flags |= LayoutResultFlags.LayerChanged;
                }
            }
        }

        public bool ZIndexChanged {
            get { return (flags & LayoutResultFlags.ZIndexChanged) != 0; }
            internal set {
                if (value) {
                    flags |= LayoutResultFlags.ZIndexChanged;
                }
            }
        }

        public bool ContentOffsetChanged {
            get { return (flags & LayoutResultFlags.ContentRectChanged) != 0; }
            internal set {
                if (value) {
                    flags |= LayoutResultFlags.ContentRectChanged;
                }
            }
        }


        public bool ScaleChanged {
            get { return (flags & LayoutResultFlags.ScaleChanged) != 0; }
            internal set {
                if (value) {
                    flags |= LayoutResultFlags.ScaleChanged;
                }
            }
        }

        public bool RotationChanged {
            get { return (flags & LayoutResultFlags.RotationChanged) != 0; }
            internal set {
                if (value) {
                    flags |= LayoutResultFlags.RotationChanged;
                }
            }
        }

        public Rect ContentRect {
            get { return contentRect; }
            set {
                if (value != contentRect) {
                    contentRect = value;
                    flags |= LayoutResultFlags.ContentRectChanged;
                }
                else {
                    flags &= ~LayoutResultFlags.ContentRectChanged;
                }
            }

        }

        public Vector2 Pivot {
            get { return pivot; }
            set {
                if (value != pivot) {
                    pivot = value;
                    flags |= LayoutResultFlags.PivotChanged;
                }
                else {
                    flags &= ~LayoutResultFlags.PivotChanged;
                }
            }
        }

        [Flags]
        private enum LayoutResultFlags {

            None = 0,
            AllocatedSizeChanged = 1 << 0,
            ActualSizeChanged = 1 << 1,
            LayerChanged = 1 << 2,
            ZIndexChanged = 1 << 3,
            ContentRectChanged = 1 << 4,
            RotationChanged = 1 << 5,
            ScaleChanged = 1 << 6,
            LocalPositionChanged = 1 << 7,
            ScreenPositionChanged = 1 << 8,
            PivotChanged = 1 << 9,

            PositionChanged = LocalPositionChanged | ScreenPositionChanged,
            TransformChanged = PositionChanged | ScaleChanged | RotationChanged | PivotChanged,
            SizeChanged = AllocatedSizeChanged | ActualSizeChanged

        }

    }

}