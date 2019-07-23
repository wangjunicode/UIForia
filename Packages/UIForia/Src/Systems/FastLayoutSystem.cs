using System;
using System.Collections.Generic;
using SVGX;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace UIForia.Systems {

    [Flags]
    public enum LayoutRenderFlag {

        NeedsLayout = 1 << 0,
        Ignored = 1 << 1,
        Transclude = 1 << 2,
        ClipWidth = 1 << 3,
        ClipHeight = 1 << 4,
        IgnoreClip = 1 << 5,
        Clip = 1 << 6

    }

    public enum ClipBehavior {

        Never,
        Normal,
        View

    }

    public class FastLayoutSystem : ILayoutSystem {

        public readonly LightList<FastLayoutBox> nodesNeedingLayout;
        private readonly Dictionary<int, LayoutBoxPool> layoutBoxPoolMap;

        public const int TextLayoutPoolKey = 100;
        public const int ImageLayoutPoolKey = 200;
        public readonly Application application;
        public readonly IStyleSystem styleSystem;

        private readonly LightList<UIElement> enabledElements;
        private readonly LightList<FastLayoutBox> toAlign;
        private readonly StructList<float3x2> toMultiply;
        private readonly LightList<LayoutOwner> layoutOwners;

        public FastLayoutSystem(Application application, IStyleSystem styleSystem) {
            this.application = application;
            this.styleSystem = styleSystem;
            this.layoutOwners = new LightList<LayoutOwner>();
            this.nodesNeedingLayout = new LightList<FastLayoutBox>(32);
            this.layoutBoxPoolMap = new Dictionary<int, LayoutBoxPool>();
            this.layoutBoxPoolMap[(int) LayoutType.Flex] = new LayoutBoxPool<FlexLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Grid] = new LayoutBoxPool<GridLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Radial] = new LayoutBoxPool<RadialLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Fixed] = new LayoutBoxPool<FixedLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Flow] = new LayoutBoxPool<FlowLayoutBox>();
            this.layoutBoxPoolMap[TextLayoutPoolKey] = new LayoutBoxPool<TextLayoutBox>();
            this.layoutBoxPoolMap[ImageLayoutPoolKey] = new LayoutBoxPool<ImageLayoutBox>();
            this.enabledElements = new LightList<UIElement>();
        }

        public void OnReset() { }

        public void OnUpdate() {
            UpdateEnabledBoxes();

            // todo -- can totally thread this.
            for (int i = 0; i < layoutOwners.size; i++) {
                layoutOwners[i].RunLayout();
            }

            // combine clip trees here
        }

        public struct LayoutData {

            public int parentIndex;
            public int childStart;
            public int childEnd;
            public FastLayoutBox layoutBox;
            public UIElement element;
            public int idx;
            public LayoutRenderFlag flags;
            public int clipGroupIndex;

        }

        public class LayoutOwner {

            public bool dirty;
            public UIView view;
            public FastLayoutBox root;
            internal readonly StructList<LayoutData> enabledBoxList;
            internal readonly StructList<SVGXMatrix> matrixList;
            internal readonly StructList<SVGXMatrix> localMatrixList;
            internal readonly Queue<int> queue;

            public LayoutOwner() {
                queue = new Queue<int>(32);
                enabledBoxList = new StructList<LayoutData>(0);
                matrixList = new StructList<SVGXMatrix>(0);
                localMatrixList = new StructList<SVGXMatrix>(0);
            }

            public void RunLayout() {
                // if nothing to layout, just return and be done with it
                // might be able to short cut the traversal gather step if nothing was enabled / disabled this frame
                //  -- still need to gather local matrices or re-use from last frame


                if (!dirty) return;
                dirty = false;

//            nodesNeedingLayout.Sort((a, b) => a.depth - b.depth);
//
//            FastLayoutBox[] toLayout = nodesNeedingLayout.array;
//
//            // layout only nodes needing update. can be threaded in the future as long as no two threads work on the same hierarchy
//
//            // when marked or size changed need to layout
//            // when marked, mark parent until parent doesn't care what size you are
//            // parent won't need to layout a child whos width changed if they only care about height
//            // parent won't need to layout a child whos padding or border changes
//            // if content changes and parent is content sized mark for layout
//
//            for (int i = 0; i < nodesNeedingLayout.size; i++) {
//                if ((toLayout[i].flags & LayoutRenderFlag.NeedsLayout) != 0) {
//                    toLayout[i].PerformLayout();
//                    toLayout[i].flags &= ~LayoutRenderFlag.NeedsLayout;
//                }
//            }

                // nodesNeedingLayout.QuickClear();
                
                queue.Enqueue(0);

                StructList<LayoutData> toLayout = new StructList<LayoutData>();

                // todo -- should really just be number of enabled elements

                enabledBoxList.EnsureCapacity(view.elements.size);
                matrixList.EnsureCapacity(view.elements.size);
                localMatrixList.EnsureCapacity(view.elements.size);

                LayoutData[] enabledBoxes = enabledBoxList.array;
                SVGXMatrix[] localMatrices = localMatrixList.array;
                SVGXMatrix[] worldMatrices = matrixList.array;

                int idx = 0;

                while (queue.Count > 0) {
                    // queue might be just integers and use it to look up in enabledBox array
                    int currentIndex = queue.Dequeue();

                    ref LayoutData data = ref enabledBoxes[currentIndex];

                    data.childStart = idx;

                    int count = 0;
                    int childCount = data.element.children.size;
                    UIElement[] childrenElements = data.element.children.array;

                    for (int i = 0; i < childCount; i++) {
                        UIElement childElement = childrenElements[i];

                        if (!childElement.isEnabled) continue;

                        FastLayoutBox childBox = childElement.layoutBox;

                        count++;

                        toLayout[toLayout.size++] = new LayoutData();

                        localMatrices[idx] = childBox.localMatrix;

                        // overflowX & Y, clip behavior, scroll behavior, other shit from style that doesn't change at this point

                        enabledBoxes[idx] = new LayoutData() {
                            parentIndex = data.idx,
                            idx = idx,
                            element = childElement
                        };

                        queue.Enqueue(idx);

                        idx++;
                    }

                    data.childEnd = data.childStart + count;
                }

                for (int i = 0; i < toLayout.size; i++) {
                    // call layout on children recursively, they will naturally not do work if they don't have to. 
                    toLayout[i].layoutBox.Layout();
                }

                toLayout.QuickClear();

                // works as long as we never process the current node, parent always handles children
                // nothing to do with visibility sort, that needs to be done per-clip group anyway
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
                
                StructList<FastLayoutBox.ClipGroup> clipGroups = new StructList<FastLayoutBox.ClipGroup>();
                
                for (int i = 0; i < enabledBoxList.size; i++) {

                    ref LayoutData current = ref enabledBoxes[i];
                    
                    int start = current.childStart;
                    int end = current.childEnd;
                    
                    if (end - start == 0) {
                        continue;
                    }
                    
                    int clipIndex;
                    
                    if ((current.flags & LayoutRenderFlag.Clip) != 0) {
                        clipIndex = clipGroups.size;
                        clipGroups.Add(new FastLayoutBox.ClipGroup() {
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
                
                // can even do in parallel with matrix transforms since the gather phase has no dependencies on it
                // now can cull w/ jobs in parallel! fuck yeah! (might have to join all our threads before dishing out to job system)
                // after matrices & gather, do cull jobs (parallel)
                
            }

            public void OnElementEnabled(UIElement element) { }

            public void OnElementDisabled(UIElement element) { }

        }

        private struct Child {

            public bool shouldPopClip;
            public FastLayoutBox child;
            public int parentIndex;

        }

        private void ApplyAlignment() {
            for (int i = 0; i < toAlign.size; i++) {
//                toAlign[i].ApplyAlignment();
            }
        }

        // apply scroll offsets & sticky / fixed
        private void ApplyScrollBehavior() { }

        private void MultiplyMatrices() {
            // build local matrix
            // localPosition + alignedLocalPosition
            // pivot
            // parent screen matrix
        }

        private void BroadPhaseCull() { }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
            LayoutOwner owner = new LayoutOwner();

            owner.dirty = true;
            owner.view = view;

            view.RootElement.layoutBox = new FastFlexLayoutBox(view.RootElement);
            view.RootElement.layoutBox.flags |= LayoutRenderFlag.NeedsLayout;
            view.RootElement.layoutBox.owner = owner;
            owner.root = view.RootElement.layoutBox;

            nodesNeedingLayout.Add(view.RootElement.layoutBox);

            layoutOwners.Add(owner);
        }

        public void OnViewRemoved(UIView view) { }

        private void UpdateEnabledBoxes() {
            enabledElements.Sort((a, b) => a.traversalIndex - b.traversalIndex);

            int thisFrame = Time.frameCount;

            // need to figure out if the loop processed this element via traversing children already 
            for (int i = 0; i < enabledElements.size; i++) {
                UIElement element = enabledElements[i];

                if (element.layoutBox == null || element.layoutBox.enabledFrame != thisFrame) {
                    UpdateLayoutBoxes(element, thisFrame);
                }
            }

            enabledElements.QuickClear();
        }

        private void UpdateLayoutBoxes(UIElement element, int frame) {
            LightStack<UIElement> stack = LightStack<UIElement>.Get();

            stack.Push(element);

            LightList<FastLayoutBox> container = LightList<FastLayoutBox>.Get();

            CreateOrUpdateLayoutBox(element, frame);

            while (stack.size > 0) {
                UIElement current = stack.array[--stack.size];

                for (int i = 0; i < current.children.size; i++) {
                    UIElement child = current.children.array[i];

                    if (!child.isEnabled) {
                        continue;
                    }

                    CreateOrUpdateLayoutBox(child, frame);
                    container.Add(child.layoutBox);
                    stack.Push(child);
                }

                current.layoutBox.SetChildren(container);
                container.size = 0;
            }

            stack.Release();
            LightList<FastLayoutBox>.Release(ref container);

            FindParent(element).AddChild(element.layoutBox);
        }

        private static FastLayoutBox FindParent(UIElement element) {
            UIElement ptr = element.parent;
            while (ptr.layoutBox is TranscludeLayoutBox) {
                ptr = ptr.parent;
            }

            return ptr.layoutBox;
        }

        private void CreateOrUpdateLayoutBox(UIElement element, int frameId) {
            FastLayoutBox box = element.layoutBox;

            switch (element.style.LayoutType) {
                case LayoutType.Unset:
                    break;

                case LayoutType.Flow:
                    break;

                case LayoutType.Flex:
                    if (box is FastFlexLayoutBox) {
                        return;
                    }

                    CreateLayoutBox(element, frameId);
                    break;

                case LayoutType.Fixed:
                    break;

                case LayoutType.Grid:
                    break;

                case LayoutType.Radial:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnElementEnabled(UIElement element) {
            UIView view = element.View;
            for (int i = 0; i < layoutOwners.size; i++) {
                if (layoutOwners.array[i].view == view) {
                    layoutOwners.array[i].OnElementEnabled(element);
                    break;
                }
            }
        }

        public void OnElementDisabled(UIElement element) {
            UIView view = element.View;
            for (int i = 0; i < layoutOwners.size; i++) {
                if (layoutOwners.array[i].view == view) {
                    layoutOwners.array[i].OnElementDisabled(element);
                    break;
                }
            }

            element.layoutBox?.parent.RemoveChild(element.layoutBox);

            // need a placeholder layout box for transclusion

            // layout type = transclude
            // on child added()
            //     parent.onChildAdded()
            // 1 way link to parent, parent doesn't know this exists but this calls child added / removed on parent
            // if transcluded is disabled, remove all children from parent
            // transclude always has zero size, never positions, never aligns
            // transclude is ignored by default since parent doesn't know about it.
        }

        public void OnElementDestroyed(UIElement element) {
            element.layoutBox?.parent.RemoveChild(element.layoutBox);
            // todo -- recycle all children boxes
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void OnElementCreated(UIElement element) { }

        private void CreateLayoutBox(UIElement element, int frameId) {
            FastLayoutBox retn = null;
            if ((element is UITextElement)) {
                TextLayoutBox textLayout = (TextLayoutBox) layoutBoxPoolMap[TextLayoutPoolKey].Get(element);
                // m_TextLayoutBoxes.Add(textLayout);
                //retn = textLayout;
            }
            else if ((element is UIImageElement)) {
                //retn = layoutBoxPoolMap[ImageLayoutPoolKey].Get(element);
            }
            else {
                switch (element.style.LayoutType) {
                    case LayoutType.Flex:
                        retn = new FastFlexLayoutBox(element); //layoutBoxPoolMap[(int) LayoutType.Flex].Get(element);
                        break;

                    case LayoutType.Flow:
                        //retn = layoutBoxPoolMap[(int) LayoutType.Flow].Get(element);
                        break;

                    case LayoutType.Fixed:
                        //retn = layoutBoxPoolMap[(int) LayoutType.Fixed].Get(element);
                        break;

                    case LayoutType.Grid:
                        //retn = layoutBoxPoolMap[(int) LayoutType.Grid].Get(element);
                        break;

                    case LayoutType.Radial:
                        //retn = layoutBoxPoolMap[(int) LayoutType.Radial].Get(element);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            element.layoutBox = retn;

            Debug.Assert(retn != null, nameof(retn) + " != null");

            retn.enabledFrame = frameId;
//            UpdateLayoutBoxes(element, frameId);
        }

        public IList<UIElement> QueryPoint(Vector2 point, IList<UIElement> retn) {
            return retn;
        }

        public OffsetRect GetPaddingRect(UIElement element) {
            return default;
        }

        public OffsetRect GetMarginRect(UIElement element) {
            return default;
        }

        public OffsetRect GetBorderRect(UIElement element) {
            return default;
        }

        public LayoutBox GetBoxForElement(UIElement itemElement) {
            return default;
        }

        public LightList<UIElement> GetVisibleElements(LightList<UIElement> retn = null) {
            return retn;
        }

    }

}