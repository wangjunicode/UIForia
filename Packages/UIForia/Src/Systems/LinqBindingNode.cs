using System;
using System.Diagnostics;
using JetBrains.Annotations;
using UIForia.Elements;
using UIForia.Util;
using LinqBinding = System.Action<UIForia.Elements.UIElement, UIForia.Elements.UIElement, UIForia.Util.StructStack<UIForia.Compilers.TemplateContextWrapper>>;

namespace UIForia.Systems {

    [DebuggerTypeProxy(typeof(LinqBindingNode))]
    internal class LinqBindingNodeDebugView  {

        private readonly LinqBindingNode node;

        public LightList<ContextVariable> localVars;
        
        public LinqBindingNodeDebugView(LinqBindingNode node) {
            this.node = node;
            this.localVars = new LightList<ContextVariable>();
            ContextVariable ptr = node.localVariable;
            while (ptr != null) {
                localVars.Add(ptr);
                ptr = ptr.next;
            }
        }

    }
    
    public abstract class ContextVariable {

        public int id;
        public string name;
        public ContextVariable next;
        public ContextVariable reference;

        public bool IsReference => reference != null;
        
        public abstract ContextVariable CreateReference();

    }
    
    [DebuggerDisplay("{name} {id}")]
    public class ContextVariable<T> : ContextVariable {

        public T value;
        
        private ContextVariable(ContextVariable<T> original) {
            this.id = original.id;
            this.name = original.name;
            this.reference = original;
            this.value = original.value;
        }
        
        public ContextVariable(int id, string name, T value) {
            this.id = id;
            this.name = name;
            this.value = value;
        }

        public override ContextVariable CreateReference() {
            return new ContextVariable<T>((ContextVariable<T>)reference ?? this);
        }

    }

    public class LinqBindingNode {

        public UIElement root;
        public UIElement element;
        public UIElement innerContext;

        internal int lastTickedFrame;
        
        internal Action<UIElement, UIElement> createdBinding;
        internal Action<UIElement, UIElement> enabledBinding;
        internal Action<UIElement, UIElement> updateBindings;
        internal Action<UIElement, UIElement> lateBindings;
        
        internal ContextVariable localVariable;
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

        public ContextVariable GetContextVariable(int id) {
            
            ContextVariable ptr = localVariable;
            while (ptr != null) {
                
                if (ptr.id == id) {
                    return ptr.reference ?? ptr;
                }

                ptr = ptr.next;
            }

            // if didnt find a local variable, start a search upwards
            
            // if found, reference locally. should only hit this once

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
            
            ContextVariable value = parent?.GetContextVariable(id);
            
            // it is technically an error if we can't find the context variable, something went wrong with compilation
            Debug.Assert(value != null, nameof(value) + " != null");
            
            value = value.CreateReference();
            
            CreateLocalContextVariable(value);

            return value;
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
        public static LinqBindingNode Get(Application application, UIElement rootElement, UIElement element, UIElement innerContext, int createdId, int enabledId, int updatedId, int lateId) {
            LinqBindingNode node = new LinqBindingNode(); // todo -- pool
            node.root = rootElement;
            node.element = element;
            node.innerContext = innerContext;
            element.bindingNode = node;
            
            // todo -- profile this against skip tree
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
            
            if (lateId != -1) {
                node.lateBindings = application.templateData.bindings[lateId];
            }

            return node;
        }

        public ContextVariable<T> GetRepeatItem<T>(int id) {
            ContextVariable ptr = localVariable; 
            while (ptr != null) {
                if (ptr.id == id) {
                    return (ContextVariable<T>) ptr;
                }

                ptr = ptr.next;
            }

            return null;
        }
        
    }

}