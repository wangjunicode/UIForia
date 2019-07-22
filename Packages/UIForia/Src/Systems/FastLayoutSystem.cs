using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Util;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace UIForia.Systems {

    public class FastFlexLayoutBox : FastLayoutBox {

        private StructList<Item> itemList;

        public FastFlexLayoutBox(UIElement element) : base(element) {
            this.itemList = new StructList<Item>();
        }


        public override void PerformLayout() {
            Item[] items = itemList.array;
            FastLayoutBox child = firstChild;
            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];

                if (item.needsUpdate) { }

//                items[i].baseSize = child.GetSize(new BoxConstraint() {
//                    minWidth = 0,
//                    maxWidth = float.PositiveInfinity,
//                    minHeight = 0,
//                    maxHeight = float.PositiveInfinity
//                });

                child = child.nextSibling;
            }
        }

        public struct Item {

            public bool needsUpdate;
            public float mainAxisStart;
            public float crossAxisStart;
            public float mainSize;
            public float crossSize;
            public float minSize;
            public float maxSize;
            public int growFactor;
            public int shrinkFactor;
            public OffsetRect margin;
            public CrossAxisAlignment crossAxisAlignment;

        }

    }

    public class TranscludeLayoutBox : FastLayoutBox {

        public TranscludeLayoutBox(UIElement element) : base(element) { }

        public override void PerformLayout() {
            throw new NotImplementedException("Should never call layout on a transcluded layout box");
        }

    }

    [Flags]
    public enum LayoutRenderFlag {

        NeedsLayout = 1 << 0

    }

    public struct OffsetBox { }

    public struct MeasurementSet {

        public float minValue;
        public float maxValue;
        public float prefValue;

        public UIMeasurementUnit minUnit;
        public UIMeasurementUnit maxUnit;
        public UIMeasurementUnit prefUnit;

    }

    public struct ContainingBlock {

        public float width;
        public float height;

    }


    public class FastLayoutSystem : ILayoutSystem {

        public readonly LightList<FastLayoutBox> nodesNeedingLayout;
        private readonly Dictionary<int, LayoutBoxPool> layoutBoxPoolMap;

        public const int TextLayoutPoolKey = 100;
        public const int ImageLayoutPoolKey = 200;
        public readonly Application application;
        public readonly IStyleSystem styleSystem;

        private readonly LightList<UIElement> enabledElements;

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
            
            for (int i = 0; i < nodesNeedingLayout.size; i++) {
                if ((toLayout[i].flags & LayoutRenderFlag.NeedsLayout) != 0) {
                    toLayout[i].PerformLayout();
                    toLayout[i].flags &= ~LayoutRenderFlag.NeedsLayout;
                }
            }

            nodesNeedingLayout.QuickClear();
        }

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