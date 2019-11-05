using System;
using JetBrains.Annotations;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;
using LinqBinding = System.Action<UIForia.Elements.UIElement, UIForia.Elements.UIElement, UIForia.Util.StructStack<UIForia.Compilers.TemplateContextWrapper>>;

namespace UIForia.Systems {

    public class ContextVariable {

        public int id;
        public string name;
        public ContextVariable next;

    }

    public class ContextVariable<T> : ContextVariable {

        public T value;

    }

    public class LinqBindingNode {

        internal UIElement root;
        internal UIElement element;
        internal LightList<LinqBindingNode> children;

        internal int lastTickedFrame;

        internal Action<UIElement, UIElement> createdBinding;
        internal Action<UIElement, UIElement> enabledBinding;
        internal Action<UIElement, UIElement> updateBindings;

        internal TemplateContextWrapper contextWrapper;

        internal ContextVariable localVariable;
        private LinqBindingNode parent;

        public void CreateLocalContextVariable(ContextVariable variable) { }

        public ContextVariable GetContextVariable(int id) {
            return null;
        }

        public ContextVariable GetLocalContextVariable(string variableName) {
//            if (localVariable == null) {
//                return default; // should never hit this since we only use this via generated code that is pre-validated
//            }
//
//            ContextVariable ptr = localVariable;
//            while (ptr != null) {
//                if (ptr.name == variableName) {
//                    return (T) ptr;
//                }
//                ptr = ptr.next;
//            }

            return default; // should never hit this
        }
        
        public void Update() {
            updateBindings?.Invoke(root, element);
//
//            if (!element.isEnabled) {
//                return;
//            }
//
//            iteratorIndex = 0;
//
//            int activePhase = system.currentPhase;
//
//            // todo if doing an out of order binding run we need to be sure the context stack is saved / restored 
//
//            if (contextWrapper.context != null) {
//                contextStack.Push(contextWrapper);
//            }
//
//            while (iteratorIndex < children.size) {
//                LinqBindingNode child = children.array[iteratorIndex++];
//
//                if (child.phase != activePhase) {
//                    child.Update(contextStack);
//                    system.currentlyActive = this;
//                }
//            }
//
//            if (contextWrapper.context != null) {
//                contextStack.Pop();
//            }
//
//            iteratorIndex = -1;
        }

        [UsedImplicitly] // called from template functions, 
        public static LinqBindingNode Get(Application application, UIElement rootElement, UIElement element, int createdId, int enabledId, int updatedId) {
            LinqBindingNode node = new LinqBindingNode(); // todo -- pool
            node.root = rootElement;
            node.element = element;
            
            if (createdId != -1) {
                node.createdBinding = application.templateData.bindings[createdId];
            }

            if (enabledId != -1) {
                node.enabledBinding = application.templateData.bindings[enabledId];
            }

            if (updatedId != -1) {
                node.updateBindings = application.templateData.bindings[updatedId];
            }

            UIElement ptr = element.parent;
            while (ptr != null) {
                if (ptr.bindingNode != null) {
                    node.parent = ptr.bindingNode;
                    break;
                }
                ptr = ptr.parent;
            }
            
            return node;
        }

    }

}