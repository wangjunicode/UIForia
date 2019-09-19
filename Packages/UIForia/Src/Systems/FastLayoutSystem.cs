using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class FastLayoutSystem : ILayoutSystem {

        public readonly Application application;
        public readonly IStyleSystem styleSystem;

        private static readonly IComparer<FastLayoutBox> comparer = new FastDepthComparer();
        private readonly LightList<LayoutOwner> layoutOwners;

        public FastLayoutSystem(Application application, IStyleSystem styleSystem) {
            this.application = application;
            this.styleSystem = styleSystem;
            this.layoutOwners = new LightList<LayoutOwner>();
            this.styleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
        }

        private void HandleStylePropertyChanged(UIElement element, StructList<StyleProperty> changeList) {
            // todo -- if layout type changed pool box and set to null
            // todo -- also make pool shared between layout owners, won't work if parallel
            element.layoutBox?.OnStyleChanged(changeList);
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

            // dont via gather phase
//            element.layoutBox?.parent?.RemoveChild(element.layoutBox);

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
            element.layoutBox?.parent?.RemoveChild(element.layoutBox);
            // todo -- recycle all children boxes
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void OnElementCreated(UIElement element) { }


        public IList<UIElement> QueryPoint(Vector2 point, IList<UIElement> retn) {
            
            LightList<FastLayoutBox> boxes = LightList<FastLayoutBox>.Get();
            
            for (int i = 0; i < layoutOwners.size; i++) {
                layoutOwners.array[i].GetElementsAtPoint(point, boxes);
            }

            boxes.Sort(comparer);

            for (var i = 0; i < boxes.Count; i++) {
                retn.Add(boxes[i].element);
            }

            LightList<FastLayoutBox>.Release(ref boxes);
            
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