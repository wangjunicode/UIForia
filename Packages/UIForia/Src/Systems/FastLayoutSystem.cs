using System;
using System.Collections.Generic;
using SVGX;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout {

    public enum ClipBehavior {

        Never,
        Normal,
        View

    }

}

namespace UIForia.Systems {

    public class FastLayoutSystem : ILayoutSystem {

        public const int TextLayoutPoolKey = 100;
        public const int ImageLayoutPoolKey = 200;
        public readonly Application application;
        public readonly IStyleSystem styleSystem;

        private readonly LightList<LayoutOwner> layoutOwners;

        public FastLayoutSystem(Application application, IStyleSystem styleSystem) {
            this.application = application;
            this.styleSystem = styleSystem;
            this.layoutOwners = new LightList<LayoutOwner>();
        }

        public void OnReset() {
            layoutOwners.Clear();
        }

        public void OnUpdate() {
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

//            public LayoutRenderFlag flags;
            public int clipGroupIndex;

        }

        public class LayoutOwner {

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
            internal readonly Queue<int> queue;

            private readonly Dictionary<int, FastLayoutBoxPool> layoutBoxPoolMap;

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

                this.layoutBoxPoolMap = new Dictionary<int, FastLayoutBoxPool>();
                this.layoutBoxPoolMap[(int) LayoutType.Flex] = new FastLayoutBoxPool<FastFlexLayoutBox>();
                this.layoutBoxPoolMap[TextLayoutPoolKey] = new FastLayoutBoxPool<FastTextLayoutBox>();

                toLayout.Add(root);
            }

            public void RunLayout() {
                // if nothing to layout, just return and be done with it
                // might be able to short cut the traversal gather step if nothing was enabled / disabled this frame

                if (toLayout.size == 0 && enabledBoxList.size == 0) {
                    return;
                }

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
                    layoutResult.matrix = worldMatrices[idx];
                    layoutResult.actualSize = sizeSet.size;
                    layoutResult.allocatedSize = sizeSet.allocatedSize;
                    layoutResult.screenPosition = layoutResult.matrix.position;
                    layoutResult.clipRect = new Rect(0, 0, Screen.width, Screen.height); // todo -- temp
                }
            }

            private FastLayoutBox CreateLayoutBox(UIElement element) {
                if ((element is UITextElement)) {
                    return (FastTextLayoutBox) layoutBoxPoolMap[TextLayoutPoolKey].Get(this, element);
                }
                else if ((element is UIImageElement)) {
                    //retn = layoutBoxPoolMap[ImageLayoutPoolKey].Get(element);
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
                            //retn = layoutBoxPoolMap[(int) LayoutType.Grid].Get(element);
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

                if (box == null) {
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

                        if ((childElement.flags & UIElementFlags.EnabledThisFrame) != 0) {
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
                        data.element.flags &= ~UIElementFlags.EnabledThisFrame;
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

            private void GatherClipGroups() {
                return;

//                LayoutData[] enabledBoxes = enabledBoxList.array;
//
//                StructList<ClipGroup> clipGroups = new StructList<ClipGroup>();
//
//                for (int i = 0; i < enabledBoxList.size; i++) {
//                    ref LayoutData current = ref enabledBoxes[i];
//
//                    int start = current.childStart;
//                    int end = current.childEnd;
//
//                    if (end - start == 0) {
//                        continue;
//                    }
//
//                    int clipIndex;
//
//                    if ((current.layoutBox.flags & LayoutRenderFlag.Clip) != 0) {
//                        clipIndex = clipGroups.size;
//                        clipGroups.Add(new ClipGroup() {
//                            root = current.layoutBox,
//                            members = LightList<FastLayoutBox>.Get(),
//                        });
//                    }
//                    else {
//                        clipIndex = current.clipGroupIndex;
//                    }
//
//                    clipGroups[clipIndex].members.EnsureAdditionalCapacity(end - start);
//
//                    for (int j = start; j < end; j++) {
//                        clipGroups[clipIndex].members.Add(enabledBoxes[j].layoutBox);
//                        enabledBoxes[j].clipGroupIndex = clipIndex;
//                    }
//                }
            }

            public void Release() {
                
            }

        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
            view.RootElement.layoutBox = new ViewRootLayoutBox();
            view.RootElement.layoutBox.element = view.RootElement;
            view.RootElement.layoutBox.flags |= LayoutRenderFlag.NeedsLayout;

            LayoutOwner owner = new LayoutOwner(view);

            view.RootElement.layoutBox.owner = owner;

            layoutOwners.Add(owner);
        }

        public void OnViewRemoved(UIView view) {
            // kill layout owner here
            for (int i = 0; i < layoutOwners.size; i++) {
                if (layoutOwners[i].view == view) {
                    layoutOwners[i].Release();
                    layoutOwners.RemoveAt(i);
                    return;
                }
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