using Rendering;

namespace Src.Systems {

    public class LifeCycleSystem : ISystem {

        private SkipTree<UIElement> updateTree;
        private SkipTree<UIElement> enableTree;
        private SkipTree<UIElement> disableTree;
        private SkipTree<UIElement> createTree;
        private SkipTree<UIElement> destroyTree;
        private SkipTree<UIElement> allElementTree;
        
        public LifeCycleSystem() {
            this.allElementTree = new SkipTree<UIElement>();
            this.updateTree = new SkipTree<UIElement>();
            this.enableTree = new SkipTree<UIElement>();
            this.disableTree = new SkipTree<UIElement>();
            this.createTree = new SkipTree<UIElement>();
            this.destroyTree = new SkipTree<UIElement>();
        }

        public void OnReset() {
            allElementTree.Clear();
            updateTree.Clear();
            enableTree.Clear();
            disableTree.Clear();
            createTree.Clear();
            destroyTree.Clear();
        }

        public void OnUpdate() {
            updateTree.TraversePreOrder((data) => data.OnUpdate());
        }

        public void OnDestroy() {
            allElementTree.Clear();
            destroyTree.Clear();
            createTree.Clear();
            updateTree.Clear();
            enableTree.Clear();
            disableTree.Clear();
        }

        public void OnReady() {
        }

        public void OnInitialize() {
            allElementTree.TraversePreOrder(this, (self, element) => {
                if (self.HasDisabledAncestor(element)) {
                //    element.flags |= UIElementFlags.AncestorDisabled;
                }
                if (element.isDisabled) {
                    // maybe invoke OnDisable? this will happen before other systems get a shot at it
                }
            }, true);
        }

        private bool HasDisabledAncestor(UIElement element) {
            UIElement ptr = element.parent;
            while (ptr != null) {
               // if ((ptr.flags & UIElementFlags.AncestorDisabled) != 0 || (ptr.flags & UIElementFlags.Enabled) == 0) {
               //     return true;
               // }
                ptr = ptr.parent;
            }
            return false;
        }

        public void OnElementCreated(InitData elementData) {
            UIElement element = elementData.element;
            allElementTree.AddItem(element);
            if (HasDisabledAncestor(element)) {
                //element.flags |= UIElementFlags.AncestorDisabled;
            }
            // traverse for enabled / disabled here?
            if (ReflectionUtil.IsOverride(element, nameof(UIElement.OnUpdate))) {
                updateTree.AddItem(element);
            }
            if (ReflectionUtil.IsOverride(element, nameof(UIElement.OnEnable))) {
                enableTree.AddItem(element);
            }
            if (ReflectionUtil.IsOverride(element, nameof(UIElement.OnDisable))) {
                disableTree.AddItem(element);
            }
            if (ReflectionUtil.IsOverride(element, nameof(UIElement.OnCreate))) {
                createTree.AddItem(element);
            }
            if (ReflectionUtil.IsOverride(element, nameof(UIElement.OnDestroy))) {
                destroyTree.AddItem(element);
            }
        }

        // skip tree hierarchy nodes need to enable all ancestors even 
        // if the parent is not in the tree itself
        public void OnElementEnabled(UIElement element) {
            // todo -- not quite right, need to revisit skip tree to call enable 
            // todo    on all nodes that are children of this. Right now the skip 
            // todo    just sets disabled on the node and doesn't traverse, which needs to happen
            enableTree.EnableHierarchy(element);
            updateTree.EnableHierarchy(element);
           // if ((element.flags & UIElementFlags.AncestorDisabled) == 0) {
           //     allElementTree.TraversePreOrder(element, (x) => {
           //         if (!HasDisabledAncestor(x)) {
           //             x.flags &= ~(UIElementFlags.AncestorDisabled);
           //         }
           //     }, true);
           // }
        }

        // this awkwardness of newLifeCycleData can be fixed by allowing
        // skip tree to take an item type (element in this case)
        // instead of or in addition to an item instance
        public void OnElementDisabled(UIElement element) {
            disableTree.DisableHierarchy(element);
            enableTree.DisableHierarchy(element);
        }

        public void OnElementDestroyed(UIElement element) {

            destroyTree.TraversePreOrder(element, (item) => item.OnDestroy(), true);

            createTree.RemoveHierarchy(element);
            disableTree.RemoveHierarchy(element);
            enableTree.RemoveHierarchy(element);
            updateTree.RemoveHierarchy(element);
            destroyTree.RemoveHierarchy(element);

        }

        public void OnElementShown(UIElement element) {
        }

        public void OnElementHidden(UIElement element) {
        }

    }

}