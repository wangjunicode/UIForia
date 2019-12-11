using System;
using JetBrains.Annotations;
using UIForia.Elements;
using LinqBinding = System.Action<UIForia.Elements.UIElement, UIForia.Elements.UIElement, UIForia.Util.StructStack<UIForia.Compilers.TemplateContextWrapper>>;

namespace UIForia.Systems {

    public class ContextVariable {

        public int id;
        public string name;
        public ContextVariable next;

    }

    public class ContextVariable<T> : ContextVariable {

        public T value;

        public ContextVariable(int id, string name) {
            this.id = id;
            this.name = name;
        }

    }

    public class ContextVariableReference {

        public int id;
        public ContextVariable reference;
        public ContextVariableReference next;

    }

    public class LinqBindingNode {

        public UIElement slotRootContext;
        
        internal UIElement root;
        internal UIElement element;
        internal int lastTickedFrame;

        internal Action<UIElement, UIElement> createdBinding;
        internal Action<UIElement, UIElement> enabledBinding;
        internal Action<UIElement, UIElement> updateBindings;
        
        internal ContextVariable localVariable;
        internal ContextVariableReference resolvedVariable;
        internal LinqBindingNode parent;

        public void CreateLocalContextVariable(ContextVariable variable) {
            if (localVariable == null) {
                localVariable = variable;
                return;
            }

            ContextVariable ptr = localVariable;
            while (true) {
                if (ptr.next == null) {
                    ptr.next = variable;
                    return;
                }

                ptr = ptr.next;
            }
        }

        // todo -- optimize w/ ContextVariableReference
        public ContextVariable GetContextVariable(int id) {
            
            ContextVariable ptr = localVariable;
            while (ptr != null) {
                if (ptr.id == id) {
                    return ptr;
                }

                ptr = ptr.next;
            }

            if (parent == null) {
                    
                UIElement elemPtr = element.parent;
                while (elemPtr != null) {
                    if (elemPtr.bindingNode != null) {
                        parent = elemPtr.bindingNode;
                        break;
                    }

                    elemPtr = elemPtr.parent;
                }

            }
            
            return parent?.GetContextVariable(id);
        }

        // todo -- maybe make generic
        public ContextVariable GetLocalContextVariable(string variableName) {
            // should never be null since we only use this via generated code that is pre-validated

            ContextVariable ptr = localVariable;
            while (ptr != null) {
                if (ptr.name == variableName) {
                    return ptr;
                }

                ptr = ptr.next;
            }

            return default; // should never hit this
        }
        
        [UsedImplicitly] // called from template functions, 
        public static LinqBindingNode Get(Application application, UIElement rootElement, UIElement element, UIElement slotRootContext, int createdId, int enabledId, int updatedId) {
            LinqBindingNode node = new LinqBindingNode(); // todo -- pool
            node.root = rootElement;
            node.element = element;
            node.slotRootContext = slotRootContext;
            element.bindingNode = node;
            
            UIElement ptr = element.parent;
            while (ptr != null) {
                if (ptr.bindingNode != null) {
                    node.parent = ptr.bindingNode;
                    break;
                }

                ptr = ptr.parent;
            }

            if (createdId != -1) {
                node.createdBinding = application.templateData.bindings[createdId];
                node.createdBinding?.Invoke(rootElement, element);
            }

            if (enabledId != -1) {
                node.enabledBinding = application.templateData.bindings[enabledId];
            }

            if (updatedId != -1) {
                node.updateBindings = application.templateData.bindings[updatedId];
            }

            return node;
        }

    }

}