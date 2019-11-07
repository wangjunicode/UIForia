using System;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Systems {

    public class LinqBindingSystem : ISystem {

        private readonly LightList<UIElement> rootNodes;
        private UIElement currentElement;
        private int iteratorIndex;
        private int currentFrameId;

        public LinqBindingSystem() {
            this.rootNodes = new LightList<UIElement>();
        }

        public void OnReset() { }

        public void OnUpdate() {
            currentFrameId++;
            // update can cause add, remove, move, enable, destroy, disable of children
            // need to be resilient of these changes so a child doesn't get update skipped and doesn't get multiple updates
            // whenever child is effected, if currently iterating element's children, restart, save state on binding nodes as to last frame they were ticked

            LightStack<UIElement> stack = LightStack<UIElement>.Get();

            for (int i = rootNodes.size - 1; i >= 0; i--) {
                stack.Push(rootNodes.array[i]);
            }

            while (stack.size != 0) {
                currentElement = stack.array[--stack.size];

                // if current element is destroyed or disabled, bail out
                if (!currentElement.isEnabled) {
                    continue;
                }

                iteratorIndex = 0;

                while (iteratorIndex != currentElement.children.size) {
                    UIElement child = currentElement.children.array[iteratorIndex];
                    if (child.bindingNode != null && child.bindingNode.lastTickedFrame != currentFrameId) {
                        child.bindingNode.lastTickedFrame = currentFrameId;
                        child.bindingNode.updateBindings?.Invoke(child.bindingNode.root, child.bindingNode.element);
                    }

                    iteratorIndex++;
                }

                stack.EnsureAdditionalCapacity(currentElement.children.size);
                int childCount = currentElement.children.size;
                
                if (stack.size + childCount >= stack.array.Length) {
                    Array.Resize(ref stack.array, stack.size + childCount + 16);
                }
                
                for (int i = childCount - 1; i >= 0; i--) {
                    stack.array[stack.size++] = currentElement.children.array[i];
                }
            }
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
//            // todo -- need keep this sorted or just iterate application views
            rootNodes.Add(view.dummyRoot);
        }

        public void OnViewRemoved(UIView view) {
            rootNodes.Remove(view.dummyRoot);
            // todo -- if currently iterating this view, need to bail out
        }

        public void OnElementCreated(UIElement element) {
            // if creating something higher in the tree than current, need to reset
            iteratorIndex = 0;
        }

        public void OnElementEnabled(UIElement element) {
            // if enabling something higher in the tree than current, need to reset
            iteratorIndex = 0;
        }

        public void OnElementDisabled(UIElement element) {
            // if disabling current or ancestor of current need to bail out
            iteratorIndex = 0;
        }

        public void OnElementDestroyed(UIElement element) {
            // if destroying current or ancestor of current need to bail out
            iteratorIndex = 0;
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

    }

}