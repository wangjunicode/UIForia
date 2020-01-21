using System;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Systems {

    public class LinqBindingSystem : ISystem {

        private readonly LightList<UIElement> rootNodes;
        private UIElement currentElement;
        private int iteratorIndex;
        private uint iterationId;
        private uint currentFrameId;

        public LinqBindingSystem() {
            this.rootNodes = new LightList<UIElement>();
        }

        public void OnReset() { }

        public void OnUpdate_ElementStack() {
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
                if ((currentElement.flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet) {
                    continue;
                }

                iteratorIndex = 0;

                while (iteratorIndex != currentElement.children.size) {
                    UIElement child = currentElement.children.array[iteratorIndex];
                    if (child.bindingNode != null && child.bindingNode.lastBeforeUpdateFrame != currentFrameId) {
                        child.bindingNode.lastBeforeUpdateFrame = currentFrameId;

                        // if ((child.flags & pendingInput) != 0 {
                        //
                        // }

                        child.bindingNode.updateBindings?.Invoke(child.bindingNode.root, child);

                        // if ((child.flags & animating) != 0) {
                        //     // child.Animator.Update();
                        // }
                        //
                        // if ((child.flags & styleUpdates) != 0 && handlesStyleUpdates) {
                        //    child.OnStylePropertiesChanged();
                        // }

                        // child.bindingNode.syncBinding?.Invoke(child.bindingNode.root, child);
                    }

                    iteratorIndex++;
                }

                int childCount = currentElement.children.size;

                if (stack.size + childCount >= stack.array.Length) {
                    Array.Resize(ref stack.array, stack.size + childCount + 16);
                }

                for (int i = childCount - 1; i >= 0; i--) {
                    stack.array[stack.size++] = currentElement.children.array[i];
                }
            }

            LightStack<UIElement>.Release(ref stack);
        }

        private ElemRef[] elemRefStack = new ElemRef[16];

        public void OnUpdate_ElementRefStack() {
            currentFrameId++;
            // update can cause add, remove, move, enable, destroy, disable of children
            // need to be resilient of these changes so a child doesn't get update skipped and doesn't get multiple updates
            // whenever child is effected, if currently iterating element's children, restart, save state on binding nodes as to last frame they were ticked

            int size = 0;

            if (rootNodes.size >= elemRefStack.Length) {
                elemRefStack = new ElemRef[rootNodes.size + 16];
            }

            for (int i = rootNodes.size - 1; i >= 0; i--) {
                elemRefStack[size++].element = rootNodes.array[i];
            }

            ElemRef[] stack = elemRefStack;

            while (size != 0) {
                currentElement = stack[--size].element;

                // if current element is destroyed or disabled, bail out
                if ((currentElement.flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet) {
                    continue;
                }

                iteratorIndex = 0;

                // todo -- these might need to be fields
                int target = currentElement.children.size;
                UIElement[] elementChildren = currentElement.children.array;

                while (iteratorIndex != target) {
                    UIElement child = elementChildren[iteratorIndex];
                    LinqBindingNode bindingNode = child.bindingNode;

                    if (bindingNode != null && bindingNode.lastBeforeUpdateFrame != currentFrameId) {
                        bindingNode.lastBeforeUpdateFrame = currentFrameId;
                        bindingNode.updateBindings?.Invoke(bindingNode.root, child);
                    }

                    iteratorIndex++;
                }

                int childCount = target; //currentElement.children.size;

                if (size + childCount >= stack.Length) {
                    Array.Resize(ref elemRefStack, size + childCount + 16);
                    stack = elemRefStack;
                }

                for (int i = childCount - 1; i >= 0; i--) {
                    stack[size++].element = elementChildren[i];
                }
            }
        }

        public void OnUpdate() {
            OnUpdate_ElementRefStack();
        }

        public void OnUpdate2(LightList<UIElement> activeBuffer) {
            OnUpdate_ElementRefStack();
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
            if (element.parent == currentElement) {
                iteratorIndex = 0;
            }
        }

        public void OnElementEnabled(UIElement element) {
            // if enabling something higher in the tree than current, need to reset
            if (element.parent == currentElement) {
                iteratorIndex = 0;
            }
        }

        public void OnElementDisabled(UIElement element) {
            // if disabling current or ancestor of current need to bail out
            if (element.parent == currentElement) {
                iteratorIndex = 0;
            }
        }

        public void OnElementDestroyed(UIElement element) {
            // if destroying current or ancestor of current need to bail out
            iteratorIndex = 0;
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void OnLateUpdate() { }

        public void OnFrameCompleted() { }

        public void OnFrameStarted() { }

        public void BeforeUpdate(LightList<UIElement> activeBuffer) {
            int size = 0;
            iterationId++;

            if (activeBuffer.size >= elemRefStack.Length) {
                elemRefStack = new ElemRef[activeBuffer.size + 16];
            }

            for (int i = activeBuffer.size - 1; i >= 0; i--) {
                elemRefStack[size++].element = activeBuffer.array[i];
            }

            ElemRef[] stack = elemRefStack;

            while (size != 0) {
                currentElement = stack[--size].element;

                // if current element is destroyed or disabled, bail out
                if ((currentElement.flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet) {
                    continue;
                }

                iteratorIndex = 0;

                while (iteratorIndex != currentElement.children.size) {
                    UIElement child = currentElement.children.array[iteratorIndex];
                    LinqBindingNode bindingNode = child.bindingNode;

                    // if was enabled in this iteration, skip it for now
                    if (bindingNode != null && bindingNode.lastBeforeUpdateFrame != currentFrameId) {
                        bindingNode.lastBeforeUpdateFrame = currentFrameId;
                        bindingNode.updateBindings?.Invoke(bindingNode.root, child);
                    }

                    iteratorIndex++;
                }

                int childCount = currentElement.children.size;

                if (size + childCount >= stack.Length) {
                    Array.Resize(ref elemRefStack, size + childCount + 16);
                    stack = elemRefStack;
                }

                for (int i = childCount - 1; i >= 0; i--) {
                    stack[size++].element = currentElement.children.array[i];
                }
            }
        }

        public void AfterUpdate(LightList<UIElement> activeBuffer) {
            int size = 0;

            if (activeBuffer.size >= elemRefStack.Length) {
                elemRefStack = new ElemRef[activeBuffer.size + 16];
            }

            for (int i = activeBuffer.size - 1; i >= 0; i--) {
                elemRefStack[size++].element = activeBuffer.array[i];
            }

            ElemRef[] stack = elemRefStack;

            while (size != 0) {
                currentElement = stack[--size].element;

                // if current element is destroyed or disabled, bail out
                if ((currentElement.flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet) {
                    continue;
                }

                iteratorIndex = 0;

                while (iteratorIndex != currentElement.children.size) {
                    UIElement child = currentElement.children.array[iteratorIndex];
                    LinqBindingNode bindingNode = child.bindingNode;

                    // if was enabled in this iteration, skip it for now
                    if (bindingNode != null && bindingNode.lastAfterUpdateFrame != currentFrameId) {
                        bindingNode.lastAfterUpdateFrame = currentFrameId;
                        bindingNode.lateBindings?.Invoke(bindingNode.root, child);
                    }

                    iteratorIndex++;
                }

                int childCount = currentElement.children.size;

                if (size + childCount >= stack.Length) {
                    Array.Resize(ref elemRefStack, size + childCount + 16);
                    stack = elemRefStack;
                }

                for (int i = childCount - 1; i >= 0; i--) {
                    stack[size++].element = currentElement.children.array[i];
                }
            }
        }

        public void BeginFrame() {
            iteratorIndex = 0;
            currentFrameId++;
        }

    }

}