using System;
using System.Collections.Generic;
using SVGX;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;


namespace UIForia.Layout {

    public class LayoutOwner {

        public const int TextLayoutPoolKey = 100;
        public const int ImageLayoutPoolKey = 200;

        public UIView view;
        public FastLayoutBox root;

        internal readonly StructList<LayoutData> enabledBoxList;
        internal readonly StructList<SVGXMatrix> worldMatrixList;
        internal readonly StructList<SVGXMatrix> localMatrixList;
        internal readonly StructList<SizeSet> sizeSetList;
        internal readonly StructList<PositionSet> positionSetList;
        internal readonly LightList<UIElement> enabledThisFrame;
        internal readonly LightList<FastLayoutBox> toLayout;
        internal readonly LightList<FastLayoutBox> tempChildList;
        internal readonly LightList<ClipGroup> clipGroups;

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
            this.sizeSetList = new StructList<SizeSet>(0);
            this.positionSetList = new StructList<PositionSet>(0);

            this.enabledThisFrame = new LightList<UIElement>();
            this.toLayout = new LightList<FastLayoutBox>(16);
            this.tempChildList = new LightList<FastLayoutBox>(16);
            this.clipGroups = new LightList<ClipGroup>();
            
            this.layoutBoxPoolMap = new Dictionary<int, FastLayoutBoxPool>();
            this.layoutBoxPoolMap[(int) LayoutType.Flex] = new FastLayoutBoxPool<FastFlexLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Grid] = new FastLayoutBoxPool<FastGridLayoutBox>();
            this.layoutBoxPoolMap[TextLayoutPoolKey] = new FastLayoutBoxPool<FastTextLayoutBox>();
            this.layoutBoxPoolMap[ImageLayoutPoolKey] = new FastLayoutBoxPool<FastImageLayoutBox>();

            toLayout.Add(root);
        }

        public void RunLayout() {
            // if nothing to layout, just return and be done with it
            // might be able to short cut the traversal gather step if nothing was enabled / disabled this frame

            if (toLayout.size == 0 && enabledThisFrame.size == 0) {
                return;
            }
            
            enabledThisFrame.QuickClear();

            // todo -- should really just be number of enabled elements
            int elementCount = view.GetElementCount();
            enabledBoxList.EnsureCapacity(elementCount);
            worldMatrixList.EnsureCapacity(elementCount);
            localMatrixList.EnsureCapacity(elementCount);
            sizeSetList.EnsureCapacity(elementCount);
            positionSetList.EnsureCapacity(elementCount);

            GatherBoxData();

            UpdateLayout();

            ComputeWorldTransforms();

            GatherClipGroups();

            SortClipGroups();
            
            OutputLayoutResults();

            view.visibleElements.EnsureCapacity(enabledBoxList.size);
            view.visibleElements.size = enabledBoxList.size;
            for (int i = 0; i < enabledBoxList.size; i++) {
                view.visibleElements.array[i] = enabledBoxList.array[i].element;
            }
            
            Render();

            // can even do in parallel with matrix transforms since the gather phase has no dependencies on it
            // now can cull w/ jobs in parallel! fuck yeah! (might have to join all our threads before dishing out to job system)
            // after matrices & gather, do cull jobs (parallel)
        }

        private void OutputLayoutResults() {
            int count = enabledBoxList.size;
            LayoutData[] enabledBoxes = enabledBoxList.array;
            SVGXMatrix[] worldMatrices = worldMatrixList.array;
            SizeSet[] sizeSets = sizeSetList.array;
            PositionSet[] positionSets = positionSetList.array;

            for (int i = 1; i < count; i++) {
                UIElement element = enabledBoxes[i].element;
                int idx = enabledBoxes[i].idx;

                LayoutResult layoutResult = element.layoutResult;
                SizeSet sizeSet = sizeSets[idx];
                PositionSet positionSet = positionSets[idx];

                layoutResult.padding = element.layoutBox.paddingBox;
                layoutResult.border = element.layoutBox.borderBox;
                layoutResult.actualSize = sizeSet.size;
                layoutResult.allocatedSize = sizeSet.allocatedSize;
                layoutResult.matrix = worldMatrices[idx];
                Vector2 position = layoutResult.matrix.position;
                position.y = -position.y;
                layoutResult.screenPosition = position;
                layoutResult.localPosition = element.layoutBox.alignedPosition;
                layoutResult.clipRect = new Rect(0, 0, Screen.width, Screen.height); // todo -- temp
            }
        }
        
        private void GatherBoxData() {
            LayoutData[] enabledBoxes = enabledBoxList.array;
            SVGXMatrix[] localMatrices = localMatrixList.array;
            SizeSet[] sizeSets = sizeSetList.array;
            PositionSet[] positionSets = positionSetList.array;

            int idx = 1;
            queue.Enqueue(0);

            worldMatrixList[0] = SVGXMatrix.identity;
            localMatrices[0] = SVGXMatrix.identity;
            positionSets[0] = default;
            sizeSets[0] = default;

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

                int childCount = data.element.children.size;
                UIElement[] childrenElements = data.element.children.array;

                bool parentEnabledThisFrame = (data.element.flags & UIElementFlags.EnabledThisFrame) != 0;

                for (int i = 0; i < childCount; i++) {
                    UIElement childElement = childrenElements[i];

                    if (!childElement.isEnabled) continue;

                    FastLayoutBox childBox = childElement.layoutBox;

                    if (childBox == null || (childElement.flags & UIElementFlags.EnabledThisFrame) != 0) {
                        // apply up-chain selectors
                        // update style data
                        // create layout box
                        // create render box

                        childBox = CreateOrUpdateLayoutBox(childElement);
                        childElement.layoutBox = childBox;

                        if (parentEnabledThisFrame) {
                            tempChildList.Add(childBox);
                        }
                        else {
                            data.layoutBox.AddChild(childBox);
                        }
                    }
                    else {
                        SizeSet sizeSet = default;
                        sizeSet.size = childBox.size;
                        sizeSet.allocatedSize = childBox.allocatedSize;
                        sizeSet.contentSize = childBox.contentSize;

                        PositionSet positionSet = default;
                        positionSet.alignedPosition = childBox.alignedPosition;
                        positionSet.allocatedPosition = childBox.allocatedPosition;

                        sizeSets[idx] = sizeSet;
                        positionSets[idx] = positionSet;
                        localMatrices[idx] = childBox.localMatrix;
                    }

                    // overflowX & Y, clip behavior, scroll behavior, other shit from style that doesn't change at this point

                    childBox.traversalIndex = idx;
                    enabledBoxes[idx] = new LayoutData() {
                        idx = idx,
                        parentIndex = data.idx,
                        element = childElement,
                        layoutBox = childBox
                    };

                    queue.Enqueue(idx);

                    idx++;
                }

                data.childEnd = idx;

                if (parentEnabledThisFrame) {
                    data.layoutBox.SetChildren(tempChildList);
                    // todo -- this can't be done in a system since it is a shared flag
                    // data.element.flags &= ~UIElementFlags.EnabledThisFrame;
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
                toLayout[i].Layout();
            }

            toLayout.QuickClear();
        }

        internal void OnElementEnabled(UIElement element) {
            UIElement[] elements = enabledThisFrame.array;
            for (int i = 0; i < enabledThisFrame.size; i++) {
                if (elements[i] == element) {
                    return;
                }
            }

            enabledThisFrame.Add(element);
        }

        internal void OnElementDisabled(UIElement element) {
            if (element.layoutBox == null) {
                return;
            }

            enabledThisFrame.Remove(element);
        }


        private void ComputeWorldTransforms() {
            LayoutData[] enabledBoxes = enabledBoxList.array;
            SVGXMatrix[] worldMatrices = worldMatrixList.array;
            SVGXMatrix[] localMatrices = localMatrixList.array;

            for (int i = 0; i < enabledBoxList.size; i++) {
                int start = enabledBoxes[i].childStart;
                int end = enabledBoxes[i].childEnd;

                SVGXMatrix parentMatrix = worldMatrices[i];

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



        // walk through adding clip groups
        // will diff against last frame clip group w/ same id for rendering
        // will sort within clip group

        // will then sort clip groups
        // clip root defines group, can be compared per frame

        private void GatherClipGroups() {
            
            LayoutData[] enabledBoxes = enabledBoxList.array;

            clipGroups[0] = new ClipGroup() {root = enabledBoxes[0].layoutBox, members = new LightList<FastLayoutBox>()};

            for (int i = 1; i < enabledBoxList.size; i++) {
                ref LayoutData current = ref enabledBoxes[i];

                int start = current.childStart;
                int end = current.childEnd;

                if (end - start == 0) {
                    continue;
                }

                int clipIndex;

                if ((current.layoutBox.flags & LayoutRenderFlag.Clip) != 0) {
                    clipIndex = clipGroups.size;

                    // find last frame clip group if present
                    // 
                    clipGroups.Add(new ClipGroup() {
                        root = current.layoutBox,
                        members = LightList<FastLayoutBox>.Get(),
                    });
                }
                else {
                    clipIndex = current.clipGroupIndex;
                }

                clipGroups[clipIndex].members.EnsureAdditionalCapacity(end - start);

                for (int j = start; j < end; j++) {
                    clipGroups[clipIndex].members.Add(enabledBoxes[j].layoutBox);
                    enabledBoxes[j].clipGroupIndex = clipIndex;
                }
            }
        }

        public void SortClipGroups() {
            
            for (int i = 0; i < clipGroups.size; i++) {
                clipGroups[i].members.Sort(s_DepthComparer);
            }

            clipGroups.Sort((a, b) => a.root.traversalIndex - b.root.traversalIndex);
            
        }

        public void ApplyBroadPhaseCulling() {
            
            // for each element
                
                // if element.clipBehavior == Clip.Never
                // if element.clipBehavior == Clip.Normal
                // if element.clipBehavior == Clip.View
                // if element.clipBehavior == Clip.Screen
                // if element.visibility == Visibility.None

                
                
                // if !element.renderBox.ClipTest(clipShape, view)
                //    continue;
                // if !element.isEnabled || failedClipTest
                //    continue;
                // clip group.Add(element)
                //    
                
            // standard painter
                // if size.width || size.height == 0
                    // return
                
            // for each clip group
            // if group is off screen, cull whole group
            // if group is not overlapping its parent group, cull whole group
            // for each element in clipping group
            // if element corners not inside outer clip shape (polygon, circle, ellipse, rect)
            // clip element
            // would be nice to know if any descendent element is aligned or transformed outside its parent bounds (compute at layout time)
            
            // if no elements visible in group, remove em all
            // if element visibility is hidden -> clip children
            
            // within each clip group
            // if a painter sets a clip we need to respect it
            
            // view has a content group that is clipped to the view rect and an overlay group that is submitted to the same batch as the view itself with 
            
        }
 
        public void Render() {

            // do in parallel!
            // want to flatten each clip group by z-index & respect foreground render if present.
            // if painter type changed need to re-allocate a render box for the element
            
            // visibility affects children regardless of z-index
            // z-index does not change clip behavior
            
            // clipBehavior = Parent | View | Screen | Never
            
            for (int i = 0; i < clipGroups.size; i++) {
                
            }
            
        }

        public void Release() { }


        private FastLayoutBox CreateLayoutBox(UIElement element) {
            if ((element is UITextElement)) {
                return (FastTextLayoutBox) layoutBoxPoolMap[TextLayoutPoolKey].Get(this, element);
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
                        break;

                    case LayoutType.Radial:
                        //retn = layoutBoxPoolMap[(int) LayoutType.Radial].Get(element);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return null;
        }

        private FastLayoutBox CreateOrUpdateLayoutBox(UIElement element) {
            FastLayoutBox box = element.layoutBox;

            if (box == null || element is UIImageElement || element is UITextElement) {
                return CreateLayoutBox(element);
            }

            switch (element.style.LayoutType) {
                case LayoutType.Unset:
                    break;

                case LayoutType.Flow:
                    break;

                case LayoutType.Flex:

                    if (box is FastFlexLayoutBox) {
                        box.UpdateStyleData();
                        return box;
                    }

                    FastLayoutBox newBox = layoutBoxPoolMap[(int) LayoutType.Flex].Get(this, element);
                    newBox.Replace(box);
                    ReleaseLayoutBox(box);
                    return newBox;

                case LayoutType.Fixed:
                    break;

                case LayoutType.Grid:
                    break;

                case LayoutType.Radial:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private void ReleaseLayoutBox(FastLayoutBox box) {
            if (box is FastFlexLayoutBox flexLayoutBox) {
                layoutBoxPoolMap[(int) LayoutType.Flex].Release(flexLayoutBox);
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
        
        public void GetElementsAtPoint(Vector2 point, IList<UIElement> retn) {
            LayoutData[] layoutDatas = enabledBoxList.array;
            
            for (int i = 0; i < enabledBoxList.size; i++) {
                UIElement element = layoutDatas[i].element;

                if (element is IPointerQueryHandler handler) {
                    if (!handler.ContainsPoint(point)) {
                        continue;
                    }
                }

                if (!element.layoutResult.ScreenRect.ContainOrOverlap(point)) {
                    continue;
                }

                else if (!element.layoutResult.ScreenRect.ContainOrOverlap(point) || PointInClippedArea(point, element)) {
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

                retn.Add(element);
            }
            
        }

    }

}