using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia.Systems {

    public class LinqBindingSystem : ISystem {

        internal int currentPhase;
        internal int previousPhase;
        internal LinqBindingNode currentlyActive;
        internal StructStack<TemplateContextWrapper> contextStack;

        private readonly LightList<LinqBindingNode> rootNodes;

        public LinqBindingSystem() {
            this.contextStack = new StructStack<TemplateContextWrapper>();
            this.currentPhase = 0;
            this.previousPhase = -1;
            this.rootNodes = new LightList<LinqBindingNode>(4);
        }

        public void OnReset() { }

        public void OnUpdate() {
            for (int i = 0; i < rootNodes.size; i++) {
                contextStack.Clear(); // just being safe
                rootNodes.array[i].Update(contextStack);
            }
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
//            // todo -- need keep this sorted
//            view.rootElement.bindingNode = new LinqBindingNode();
//            view.rootElement.bindingNode.system = this;
//            rootNodes.Add(view.rootElement.bindingNode);
        }

        public void OnViewRemoved(UIView view) { }

        public void OnElementCreated(UIElement element) { }

        private LinqBindingNode GetBindingNode(UIElement element) {
            if (element.bindingNode != null) {
                return element.bindingNode;
            }

            UIElement ptr = element.parent;

            while (ptr != null) {
                if (ptr.bindingNode != null) {
                    return ptr.bindingNode;
                }

                ptr = ptr.parent;
            }

            return null;
        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void AddElementHierarchy(UIElement element) {
            // element binding node won't be null if adding via hierarchy
//            Assert.IsNotNull(element.bindingNode, "element.bindingNode != null");
//            // binding node might be empty. if it is, add the children instead
//            if (element.bindingNode.bindings.list != null) {
//                LinqBindingNode parentNode = GetBindingNode(element.parent);
//                if (parentNode == null) {
//                    rootNodes.Add(element.bindingNode);
//                }
//                else {
//                    parentNode.InsertChild(element.bindingNode);
//                }
//            }
//            else {
//                for (int i = 0; i < element.children.size; i++) {
//                    AddElementHierarchy(element.children[i]);
//                }
//            }
        }

    }

}