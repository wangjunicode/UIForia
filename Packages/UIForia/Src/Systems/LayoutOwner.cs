using System;
using System.Collections.Generic;
using SVGX;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;


namespace UIForia.Layout {

    public class LayoutOwner {

        public const int TextLayoutPoolKey = 100;
        public const int ImageLayoutPoolKey = 200;
        public const int TranscludedLayoutPoolKey = 300;

        public UIView view;
        public FastLayoutBox root;

        internal readonly StructList<LayoutData> enabledBoxList;
        internal readonly StructList<SVGXMatrix> worldMatrixList;
        internal readonly StructList<SVGXMatrix> localMatrixList;
        internal readonly LightList<UIElement> enabledThisFrame;
        internal readonly LightList<FastLayoutBox> toLayout;
        internal readonly LightList<FastLayoutBox> tempChildList;
        internal readonly LightList<FastLayoutBox> ignoredLayoutList;

        internal readonly Queue<int> queue;

        private readonly Dictionary<int, FastLayoutBoxPool> layoutBoxPoolMap;

        private static readonly DepthComparer s_DepthComparer = new DepthComparer();

        public LayoutOwner(UIView view) {
            this.view = view;
            this.root = view.RootElement.layoutBox;

            this.queue = new Queue<int>(32);

            this.enabledBoxList = new StructList<LayoutData>(0);
            this.worldMatrixList = new StructList<SVGXMatrix>(0);
            this.localMatrixList = new StructList<SVGXMatrix>(0);

            this.enabledThisFrame = new LightList<UIElement>();
            this.toLayout = new LightList<FastLayoutBox>(16);
            this.tempChildList = new LightList<FastLayoutBox>(16);
            this.ignoredLayoutList = new LightList<FastLayoutBox>(16);

            this.layoutBoxPoolMap = new Dictionary<int, FastLayoutBoxPool>();
            this.layoutBoxPoolMap[(int) LayoutType.Flex] = new FastLayoutBoxPool<FastFlexLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Grid] = new FastLayoutBoxPool<FastGridLayoutBox>();
            this.layoutBoxPoolMap[(int)LayoutType.Stack] = new FastLayoutBoxPool<StackLayoutBox>();
            this.layoutBoxPoolMap[TextLayoutPoolKey] = new FastLayoutBoxPool<FastTextLayoutBox>();
            this.layoutBoxPoolMap[ImageLayoutPoolKey] = new FastLayoutBoxPool<FastImageLayoutBox>();
            this.layoutBoxPoolMap[TranscludedLayoutPoolKey] = new FastLayoutBoxPool<TranscludeLayoutBox>();

            toLayout.Add(root);
        }

        public void RunLayout() {
            // if nothing to layout, just return and be done with it
            // might be able to short cut the traversal gather step if nothing was enabled / disabled this frame

            enabledThisFrame.QuickClear();

            // todo -- should really just be number of enabled elements
            int elementCount = view.GetElementCount();
            enabledBoxList.EnsureCapacity(elementCount);
            worldMatrixList.EnsureCapacity(elementCount);
            localMatrixList.EnsureCapacity(elementCount);


            // if (toLayout.size > 0 || enabledThisFrame.size > 0) {
            GatherBoxData();

            UpdateLayout();
            //  }

            UpdateAlignments();

            ComputeWorldTransforms();

            OutputLayoutResults();

            view.visibleElements.EnsureCapacity(enabledBoxList.size);
            view.visibleElements.size = enabledBoxList.size;

            for (int i = 0; i < enabledBoxList.size; i++) {
                view.visibleElements.array[i] = enabledBoxList.array[i].element;
            }

            // can even do in parallel with matrix transforms since the gather phase has no dependencies on it
            // now can cull w/ jobs in parallel! fuck yeah! (might have to join all our threads before dishing out to job system)
            // after matrices & gather, do cull jobs (parallel)
        }

        private void OutputLayoutResults() {
            int count = enabledBoxList.size;
            LayoutData[] enabledBoxes = enabledBoxList.array;
            SVGXMatrix[] worldMatrices = worldMatrixList.array;

            int screenWidth = Screen.width;
            int screenHeight = Screen.height;
            for (int i = 1; i < count; i++) {
                UIElement element = enabledBoxes[i].element;
                int idx = enabledBoxes[i].idx;

                LayoutResult layoutResult = element.layoutResult;

                // todo -- layout   result might work better as a proxy to layoutBox data
                layoutResult.padding = element.layoutBox.paddingBox;
                layoutResult.border = element.layoutBox.borderBox;
                layoutResult.actualSize = element.layoutBox.size;
                layoutResult.allocatedSize = element.layoutBox.allocatedSize;
                layoutResult.matrix = worldMatrices[idx];
                layoutResult.screenPosition = layoutResult.matrix.position; // todo screen position should be aligned rect position pre transform
                layoutResult.localPosition = element.layoutBox.alignedPosition;
                layoutResult.clipRect = new Rect(0, 0, screenWidth, screenHeight); // todo -- temp
                layoutResult.zIndex = element.layoutBox.zIndex;
            }
        }

        /// <summary>
        /// Figures out what needs to get enables and builds a flat list of LayoutData.
        /// </summary>
        private void GatherBoxData() {
            LayoutData[] enabledBoxes = enabledBoxList.array;
            SVGXMatrix[] localMatrices = localMatrixList.array;

            int idx = 1;
            queue.Enqueue(0);

            worldMatrixList[0] = SVGXMatrix.identity;
            localMatrices[0] = SVGXMatrix.identity;

            enabledBoxes[0] = new LayoutData() {
                parentIndex = -1,
                idx = 0,
                layoutBox = root,
                element = root.element,
            };

            while (queue.Count > 0) {
                // queue might be just integers and use it to look up in enabledBox array
                int currentIndex = queue.Dequeue();

                ref LayoutData data = ref enabledBoxes[currentIndex];

                data.childStart = idx;

                FastLayoutBox parentBox = data.layoutBox;
                int childCount = data.element.children.size;
                UIElement[] childrenElements = data.element.children.array;

                bool parentEnabledThisFrame = (data.element.flags & UIElementFlags.EnabledThisFrame) != 0;

                // Things to look for
                // layout type changed
                // behavior changed
                // enabled state changed
                bool willLayout = (parentBox.flags & LayoutRenderFlag.NeedsLayout) != 0;

                for (int i = 0; i < childCount; i++) {
                    UIElement childElement = childrenElements[i];

                    if (!willLayout && (childElement.flags & (UIElementFlags.EnabledThisFrame | UIElementFlags.DisabledThisFrame)) != 0) {
                        parentBox.MarkForLayout();
                        willLayout = true;
                    }

                    if (!childElement.isEnabled) {
                        continue;
                    }

                    FastLayoutBox childBox = childElement.layoutBox;

                    if (childBox != null && (childElement.flags & UIElementFlags.EnabledThisFrame) != 0) {
                        
                        childBox.UpdateStyleData();
                        
                        if (parentEnabledThisFrame) {
                            tempChildList.Add(childBox);
                        }
                        else {
                            data.layoutBox.AddChild(childBox);
                        }
                    }
                    // todo -- EnabledThisFrame is borked because input runs after layout and we enable on click
                    else if (childBox == null) {
                        if (!willLayout) {
                            parentBox.MarkForLayout();
                            willLayout = true;
                        }

                        childBox = CreateLayoutBox(childElement);
                        childBox.traversalIndex = idx; // parent adding children depends on this
                        childElement.layoutBox = childBox;

                        switch (childBox.layoutBehavior) {
                            case LayoutBehavior.Ignored:
                            case LayoutBehavior.TranscludeChildren:
                                childBox.parent = data.layoutBox;
                                break;

                            default: {
                                if (parentEnabledThisFrame) {
                                    tempChildList.Add(childBox);
                                }
                                else {
                                    data.layoutBox.AddChild(childBox);
                                }

                                break;
                            }
                        }
                    }

                    childBox.traversalIndex = idx;

                    // overflowX & Y, clip behavior, scroll behavior, other shit from style that doesn't change at this point

                    enabledBoxes[idx] = new LayoutData() {
                        idx = idx,
                        parentIndex = data.idx,
                        element = childElement,
                        layoutBox = childBox
                    };

                    // always layout ignored elements
                    if (data.layoutBox.layoutBehavior == LayoutBehavior.Ignored) {
                        ignoredLayoutList.Add(data.layoutBox);
                    }

#if DEBUG
                    if ((childBox.element.flags & UIElementFlags.DebugLayout) != 0) {
                        System.Diagnostics.Debugger.Break();
                    }
#endif
                    queue.Enqueue(idx);

                    idx++;
                }

                data.childEnd = idx;

                if (parentEnabledThisFrame) {
                    data.layoutBox.SetChildren(tempChildList);
                    tempChildList.QuickClear();
                }
            }

            enabledBoxList.size = idx;
            worldMatrixList.size = idx;
            localMatrixList.size = idx;
        }

        private void UpdateLayout() {
            toLayout.Sort((a, b) => a.traversalIndex - b.traversalIndex);

            // call layout on children recursively, they will naturally not do work if they don't have to. 
            for (int i = 0; i < toLayout.size; i++) {
                toLayout.array[i].Layout();
            }

            for (int i = 0; i < ignoredLayoutList.size; i++) {
                ignoredLayoutList.array[i].Layout();
            }

            toLayout.QuickClear();
            ignoredLayoutList.QuickClear();
        }

        internal void OnElementEnabled(UIElement element) {
            UIElement[] elements = enabledThisFrame.array;
            for (int i = 0; i < enabledThisFrame.size; i++) {
                if (elements[i] == element) {
                    return;
                }
            }

            enabledThisFrame.Add(element); // todo -- rethink this, maybe a counter is enough
        }

        internal void OnElementDisabled(UIElement element) {
            if (element.parent.layoutBox != null) {
                element.parent.layoutBox.RemoveChildByElement(element);
                element.parent.layoutBox.MarkForLayout();
            }

            // maybe add to-layout parent?
            enabledThisFrame.Remove(element); // todo -- rethink this, maybe a counter is enough
        }

        private void UpdateAlignments() {
            LayoutData[] enabledBoxes = enabledBoxList.array;
            SVGXMatrix[] localMatrices = localMatrixList.array;

            SVGXMatrix pivot = SVGXMatrix.identity;
            SVGXMatrix inversePivot = pivot;

            float viewportWidth = view.Viewport.width;
            float viewportHeight = view.Viewport.height;

            // todo -- this can be done without dereferencing box or parent and also in parallel

            for (int i = 0; i < enabledBoxList.size; i++) {
                FastLayoutBox box = enabledBoxes[i].layoutBox;

                // also if no changes in here we don't need to rebuild clip groups if we also didn't layout 
                // same for world matrix computation
                // todo -- lots of ways to avoid doing this. only need it for self aligning things or things with a transform position not in pixels

                Vector2 alignedPosition = box.alignedPosition;

                if (box.alignmentTargetX != AlignmentBehavior.Unset && box.alignmentTargetX != AlignmentBehavior.Default) {
                    OffsetMeasurement originX = box.element.style.AlignmentOriginX;
                    OffsetMeasurement offsetX = box.element.style.AlignmentOffsetX;
                    AlignmentDirection direction = box.element.style.AlignmentDirectionX;

                    float originBase = MeasurementUtil.ResolveOriginBaseX(box, view.position.x, box.alignmentTargetX, direction);
                    float originSize = MeasurementUtil.ResolveOffsetOriginSizeX(box, viewportWidth, box.alignmentTargetX);
                    float originOffset = MeasurementUtil.ResolveOffsetMeasurement(box, viewportWidth, viewportHeight, originX, originSize);
                    float offset = MeasurementUtil.ResolveOffsetMeasurement(box, viewportWidth, viewportHeight, offsetX, box.size.width);

                    if (direction == AlignmentDirection.End) {
                        alignedPosition.x = (originBase + originSize) - (originOffset + offset) - box.size.width;
                    }
                    else {
                        alignedPosition.x = originBase + originOffset + offset;
                    }
                }

                if (box.alignmentTargetY != AlignmentBehavior.Unset && box.alignmentTargetY != AlignmentBehavior.Default) {
                    OffsetMeasurement originY = box.element.style.AlignmentOriginY;
                    OffsetMeasurement offsetY = box.element.style.AlignmentOffsetY;
                    AlignmentDirection direction = box.element.style.AlignmentDirectionY;

                    float originBase = MeasurementUtil.ResolveOriginBaseY(box, view.position.y, box.alignmentTargetY, direction);
                    float originSize = MeasurementUtil.ResolveOffsetOriginSizeY(box, viewportHeight, box.alignmentTargetY);
                    float originOffset = MeasurementUtil.ResolveOffsetMeasurement(box, viewportWidth, viewportHeight, originY, originSize);
                    float offset = MeasurementUtil.ResolveOffsetMeasurement(box, viewportWidth, viewportHeight, offsetY, box.size.height);

                    if (direction == AlignmentDirection.End) {
                        alignedPosition.y = (originBase + originSize) - (originOffset + offset) - box.size.height;
                    }
                    else {
                        alignedPosition.y = originBase + originOffset + offset;
                    }
                }

                // todo -- don't do this for elements that aren't transformed
                alignedPosition.x += MeasurementUtil.ResolveOffsetMeasurement(box, viewportWidth, viewportHeight, box.transformPositionX, box.size.width);
                alignedPosition.y += MeasurementUtil.ResolveOffsetMeasurement(box, viewportWidth, viewportHeight, box.transformPositionY, box.size.height);
                
                // todo -- when this stops happening for every element every frame we need to store local matrix and assign it properly to the localMatrices array in GatherBoxData()
                SVGXMatrix m;

                float rotation = box.element.style.TransformRotation; // todo -- cache this properly
                if (rotation == 0) {
                    m = new SVGXMatrix(box.scaleX, 0, 0, box.scaleY, alignedPosition.x, alignedPosition.y);
                }
                else {
                    float ca = math.cos(-rotation * Mathf.Deg2Rad);
                    float sa = math.sin(-rotation * Mathf.Deg2Rad);
                    m = new SVGXMatrix(ca * box.scaleX, sa * box.scaleX, -sa * box.scaleY, ca * box.scaleY, alignedPosition.x, alignedPosition.y);
                }

                if (box.pivotX == 0 && box.pivotY == 0) {
                    localMatrices[i] = m;
                }
                else {
                    pivot.m4 = box.pivotX * box.size.width;
                    pivot.m5 = box.pivotY * box.size.height;
                    inversePivot.m4 = -pivot.m4;
                    inversePivot.m5 = -pivot.m5;

                    localMatrices[i] = pivot * m * inversePivot;
                }
            }
        }

        private void ComputeWorldTransforms() {
            LayoutData[] enabledBoxes = enabledBoxList.array;
            SVGXMatrix[] worldMatrices = worldMatrixList.array;
            SVGXMatrix[] localMatrices = localMatrixList.array;

            for (int i = 0; i < enabledBoxList.size; i++) {
                int start = enabledBoxes[i].childStart;
                int end = enabledBoxes[i].childEnd;

                SVGXMatrix parentMatrix = worldMatrices[i];
                // todo -- could skip the constructor here and assign directly
                for (int j = start; j < end; j++) {
                    SVGXMatrix m = localMatrices[j];
                    worldMatrices[j] = new SVGXMatrix(
                        parentMatrix.m0 * m.m0 + parentMatrix.m2 * m.m1,
                        parentMatrix.m1 * m.m0 + parentMatrix.m3 * m.m1,
                        parentMatrix.m0 * m.m2 + parentMatrix.m2 * m.m3,
                        parentMatrix.m1 * m.m2 + parentMatrix.m3 * m.m3,
                        parentMatrix.m0 * m.m4 + parentMatrix.m2 * m.m5 + parentMatrix.m4,
                        parentMatrix.m1 * m.m4 + parentMatrix.m3 * m.m5 + parentMatrix.m5
                    );
                }
            }
        }

        public void Release() { }


        private FastLayoutBox CreateLayoutBox(UIElement element) {
            if (element.layoutBox != null) {
                ReleaseLayoutBox(element.layoutBox);
            }

            if (element.style.LayoutBehavior == LayoutBehavior.TranscludeChildren) {
                return layoutBoxPoolMap[TranscludedLayoutPoolKey].Get(this, element);
            }

            if ((element is UITextElement)) {
                return layoutBoxPoolMap[TextLayoutPoolKey].Get(this, element);
            }
            else if ((element is UIImageElement)) {
                return layoutBoxPoolMap[ImageLayoutPoolKey].Get(this, element);
            }
            else {
                switch (element.style.LayoutType) {
                    case LayoutType.Flex:
                        return layoutBoxPoolMap[(int) LayoutType.Flex].Get(this, element);

                    case LayoutType.Flow:
                        //retn = layoutBoxPoolMap[(int) LayoutType.Flow].Get(element);
                        break;

                    case LayoutType.Fixed:
                        //retn = layoutBoxPoolMap[(int) LayoutType.Fixed].Get(element);
                        break;

                    case LayoutType.Grid:
                        return layoutBoxPoolMap[(int) LayoutType.Grid].Get(this, element);

                    case LayoutType.Radial:
                        //retn = layoutBoxPoolMap[(int) LayoutType.Radial].Get(element);
                        break;
                    case LayoutType.Stack:
                        return layoutBoxPoolMap[(int) LayoutType.Stack].Get(this, element);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return null;
        }

        private void ReleaseLayoutBox(FastLayoutBox box) {
            switch (box) {
                case FastFlexLayoutBox flexLayoutBox:
                    layoutBoxPoolMap[(int) LayoutType.Flex].Release(flexLayoutBox);
                    break;
                case FastGridLayoutBox gridLayoutBox:
                    layoutBoxPoolMap[(int) LayoutType.Grid].Release(gridLayoutBox);
                    break;
            }
        }

        public class DepthComparer : IComparer<FastLayoutBox> {

            public int Compare(FastLayoutBox a, FastLayoutBox b) {
//                if (a.zIndex != b.zIndex) {
//                    return a.zIndex - b.zIndex;
//                }
                return a.traversalIndex - b.traversalIndex;
            }

        }

        private static bool PointInClippedArea(Vector2 point, UIElement element) {
            Vector2 screenPosition = element.layoutResult.screenPosition;

            if (element.style.OverflowX != Overflow.Visible) {
                if (point.x < screenPosition.x || point.x > screenPosition.x + element.layoutResult.allocatedSize.width) {
                    return true;
                }
            }

            if (element.style.OverflowY != Overflow.Visible) {
                if (point.y < screenPosition.y || point.y > screenPosition.y + element.layoutResult.allocatedSize.height) {
                    return true;
                }
            }

            return false;
        }

        internal void GetElementsAtPoint(Vector2 point, LightList<FastLayoutBox> retn) {
            LayoutData[] layoutDatas = enabledBoxList.array;

            for (int i = 0; i < enabledBoxList.size; i++) {
                UIElement element = layoutDatas[i].element;
                        
                if (element.style.Visibility == Visibility.Hidden) continue;
                
                // todo -- convert to flag 
                if (element is IPointerQueryHandler handler) {
                    if (!handler.ContainsPoint(point)) {
                        continue;
                    }
                }
                
                if (!element.layoutResult.ScreenRect.ContainOrOverlap(point) || PointInClippedArea(point, element)) {
                    continue;
                }

                UIElement ptr = element.parent;
                while (ptr != null && !PointInClippedArea(point, ptr)) {
                    ptr = ptr.parent;
                }

                // i.e. clipped by parent
                if (ptr != null) {
                    continue;
                }

                retn.Add(element.layoutBox);
            }
        }

    }

}