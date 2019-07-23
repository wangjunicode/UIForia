using System;
using System.Collections.Generic;
using SVGX;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace UIForia.Systems {

    [Flags]
    public enum LayoutRenderFlag {

        NeedsLayout = 1 << 0,
        Ignored = 1 << 1,
        Transcluded = 1 << 2

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
        private readonly LightList<FastLayoutBox> toMultiply;

        public FastLayoutSystem(Application application, IStyleSystem styleSystem) {
            this.application = application;
            this.styleSystem = styleSystem;
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

            nodesNeedingLayout.Sort((a, b) => a.depth - b.depth);

            FastLayoutBox[] toLayout = nodesNeedingLayout.array;

            // layout only nodes needing update. can be threaded in the future as long as no two threads work on the same hierarchy

            // when marked or size changed need to layout
            // when marked, mark parent until parent doesn't care what size you are
            // parent won't need to layout a child whos width changed if they only care about height
            // parent won't need to layout a child whos padding or border changes
            // if content changes and parent is content sized mark for layout

            for (int i = 0; i < nodesNeedingLayout.size; i++) {
                if ((toLayout[i].flags & LayoutRenderFlag.NeedsLayout) != 0) {
                    toLayout[i].PerformLayout();
                    toLayout[i].flags &= ~LayoutRenderFlag.NeedsLayout;
                }
            }

            nodesNeedingLayout.QuickClear();

            LightStack<FastLayoutBox> stack = LightStack<FastLayoutBox>.Get();

            for (int i = 0; i < application.m_Views.Count; i++) {
                stack.Push(application.m_Views[i].rootElement.layoutBox);
            }

            while (stack.size > 0) {
                
                FastLayoutBox box = stack.array[--stack.size];

                box.ApplyAlignment();
                
                LayoutResult layoutResult = box.element.layoutResult;

                layoutResult.border = box.borderBox;
                layoutResult.padding = box.paddingBox;

                SVGXMatrix parentMatrix = box.parent.element.layoutResult.matrix;

                Vector2 pivot = default;
                SVGXMatrix m;

                if (box.transformRotation != 0) {
                    m = SVGXMatrix.TRS(localPosition, layoutResult.rotation, layoutResult.scale);
                }
                else {
                    m = SVGXMatrix.TranslateScale(localPosition.x, localPosition.y, localScale.x, localScale.y);
                }

                if (pivot.x != 0 || pivot.y != 0) {
                    SVGXMatrix pivotMat = SVGXMatrix.Translation(new Vector2(box.size.width * pivot.x, box.size.height * pivot.y));
                    m = pivotMat * m * pivotMat.Inverse();
                }

                m = new SVGXMatrix(
                    parentMatrix.m0 * m.m0 + parentMatrix.m2 * m.m1,
                    parentMatrix.m1 * m.m0 + parentMatrix.m3 * m.m1,
                    parentMatrix.m0 * m.m2 + parentMatrix.m2 * m.m3,
                    parentMatrix.m1 * m.m2 + parentMatrix.m3 * m.m3,
                    parentMatrix.m0 * m.m4 + parentMatrix.m2 * m.m5 + parentMatrix.m4,
                    parentMatrix.m1 * m.m4 + parentMatrix.m3 * m.m5 + parentMatrix.m5
                );

                if (box.element.style.ClipBehavior == ClipBehavior.Never) {
                    clipTree.overlay.Add(box.element);
                }
                else {
                    clipTree.Add(new RenderBox() {
                        element = box.element,
                        layoutBox = box,
                        matrix = m
                    });
                }

                bool pushClip = box.element.style.OverflowX != Overflow.Visible || box.element.style.OverflowY != Overflow.Visible;

                if (pushClip) {
                    // push a clip shape (can be non rect, is transformed by transform matrix into screen space)
                    clipTree.Push();
                }

                // do children

                FastLayoutBox child = box.firstChild;

                if (clipTree.Peek().Contains(child, matrix)) {
                    clipTree.Add
                }
                
                // need real recursion or some way to signify we are done with the children and should pop our stack
                while (child != null) {
                    stack.Push(child);   
                    child = child.nextSibling;
                }

                // clip tree needs to be hierarchical tree of clip groups
                // clip groups are render boundaries implicitly
                if (pushClip) {
                    clipList.Add(clipTree.Pop());
                }
                
            }
            
            LightStack<FastLayoutBox>.Release(ref stack);
            // for now just loop, later split into change sets and only process changes per frame.

            // the following phases can be split into threaded jobs w/o contention. run for all elements

            // transform changed

            // local position changed

            // alignment changed

            // layout changed

            // scroll behavior / scroll offset changed

            // for each item check change flags

            ApplyAlignment();

            ApplyScrollBehavior();

            MultiplyMatrices();

            // culling can be threaded but must wait for rejoin here I think since it has to happen after matrix pass

            BroadPhaseCull();
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
            view.RootElement.layoutBox = new FastFlexLayoutBox(view.RootElement);
            view.RootElement.layoutBox.flags |= LayoutRenderFlag.NeedsLayout;
            nodesNeedingLayout.Add(view.RootElement.layoutBox);
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
            UIElement[] enabledArray = enabledElements.array;
            for (int i = 0; i < enabledElements.size; i++) {
                if (enabledArray[i] == element) {
                    return;
                }
            }

            enabledElements.Add(element);
        }

        public void OnElementDisabled(UIElement element) {
            enabledElements.Remove(element);
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