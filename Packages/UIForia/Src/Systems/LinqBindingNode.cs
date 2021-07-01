using System;
using System.Diagnostics;
using JetBrains.Annotations;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Systems {

    public abstract class ContextVariable {

        public int id;
        public string name;
        public ContextVariable next;
        public ContextVariable reference;

        public bool IsReference => reference != null;

        public abstract ContextVariable CreateReference();
        public abstract void Release();

    }

    [DebuggerDisplay("{name} {id}")]
    public class ContextVariable<T> : ContextVariable {
        
        private static readonly ObjectPool<ContextVariable<T>> s_Pool = new ObjectPool<ContextVariable<T>>(null, null, 4096);

        public static ContextVariable<T> Create(int id, string name, T value) {
            ContextVariable<T> var = s_Pool.Get();
            var.Initialize(id, name, value);
            return var;
        }

        public T value;
        
        [UsedImplicitly]
        public ContextVariable() {
        }

        public void Initialize(int id, string name, T value) {
            this.id = id;
            this.name = name;
            this.value = value;
            this.next = null;
            this.reference = null;
        }

        public override ContextVariable CreateReference() {
            ContextVariable<T> copyFrom = (ContextVariable<T>)reference ?? this;
            ContextVariable<T> var = s_Pool.Get();
            var.Initialize(copyFrom.id, copyFrom.name, copyFrom.value);
            var.reference = copyFrom;
            return var;
        }

        public override void Release() {
            this.next?.Release();
            s_Pool.Release(this);
            this.next = null;
        }
    }

    public class LinqBindingNode {

        public UIElement root;
        public UIElement element;
        public UIElement innerContext;

        internal uint lastBeforeUpdateFrame;
        internal uint lastAfterUpdateFrame;

        internal Action<UIElement, UIElement> createdBinding;
        internal Action<UIElement, UIElement> enabledBinding;
        internal Action<UIElement, UIElement> updateBindings;
        internal Action<UIElement, UIElement> lateBindings;

        internal ContextVariable localVariable;
        internal LinqBindingNode parent;
        public UIElement[] referencedContexts;
        
        private static readonly ObjectPool<LinqBindingNode> s_NodePool = new ObjectPool<LinqBindingNode>(null, null, 4096);
        
        private void Initialize(UIElement root, UIElement element, UIElement innerContext) {
            this.root = root;
            this.element = element;
            this.innerContext = innerContext;
            element.bindingNode = this;
            
            this.lastBeforeUpdateFrame = default;
            this.lastAfterUpdateFrame = default;
            this.createdBinding = default;
            this.enabledBinding = default;
            this.updateBindings = default;
            this.lateBindings = default;
            this.localVariable = default;
            this.parent = default;
            this.referencedContexts = default;
        }

        public void InitializeContextArray(string slotName, TemplateScope templateScope, int size) {
            referencedContexts = new UIElement[size + 1];

            if (templateScope.slotInputs != null) {
                int idx = 1;
                for (int i = templateScope.slotInputs.size - 1; i >= 0; i--) {
                    if (templateScope.slotInputs.array[i].slotName == slotName) {
                        referencedContexts[idx++] = templateScope.slotInputs.array[i].context;
                    }
                }
            }

            referencedContexts[0] = templateScope.innerSlotContext;
        }

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

        public static LinqBindingNode GetSlotNode(Application application, UIElement rootElement, UIElement element, UIElement innerContext, int createdId, int enabledId, int updatedId, int lateId, string slotName, TemplateScope templateScope, int slotContextSize) {
            LinqBindingNode node = s_NodePool.Get();
            node.Initialize(rootElement, element, innerContext);

            // todo -- profile this against skip tree
            UIElement ptr = element.parent;
            while (ptr != null) {
                if (ptr.bindingNode != null) {
                    node.parent = ptr.bindingNode;
                    break;
                }

                ptr = ptr.parent;
            }

            node.InitializeContextArray(slotName, templateScope, slotContextSize);

            node.SetBindings(application, rootElement, createdId, enabledId, updatedId, lateId);

            return node;
        }

        public static LinqBindingNode GetSlotModifyNode(Application application, UIElement rootElement, UIElement element, UIElement innerContext, int createdId, int enabledId, int updatedId, int lateId) {
            LinqBindingNode node = s_NodePool.Get();
            node.Initialize(rootElement, element, innerContext);

            // todo -- profile this against skip tree
            UIElement ptr = element.parent;
            while (ptr != null) {
                if (ptr.bindingNode != null) {
                    node.parent = ptr.bindingNode;
                    break;
                }

                ptr = ptr.parent;
            }

            node.referencedContexts = new UIElement[1];
            node.referencedContexts[0] = innerContext;

            node.SetBindings(application, rootElement, createdId, enabledId, updatedId, lateId);

            return node;
        }

        private void SetBindings(Application application, UIElement rootElement, int createdId, int enabledId, int updatedId, int lateId) {
            if (createdId != -1) {
                try {
                    createdBinding = application.templateData.bindings[createdId];
                    createdBinding?.Invoke(rootElement, element);
                }
                catch (Exception e) {
                    UnityEngine.Debug.LogWarning(e);
                }
            }

            if (enabledId != -1) {
                enabledBinding = application.templateData.bindings[enabledId];
            }

            if (updatedId != -1) {
                updateBindings = application.templateData.bindings[updatedId];
            }

            if (lateId != -1) {
                lateBindings = application.templateData.bindings[lateId];
            }
        }

        [UsedImplicitly] // called from template functions, 
        public static LinqBindingNode Get(Application application, UIElement rootElement, UIElement element, UIElement innerContext, int createdId, int enabledId, int updatedId, int lateId) {
            LinqBindingNode node = s_NodePool.Get();
            node.Initialize(rootElement, element, innerContext);

            // todo -- profile this against skip tree
            UIElement ptr = element.parent;
            while (ptr != null) {
                if (ptr.bindingNode != null) {
                    node.parent = ptr.bindingNode;
                    break;
                }

                ptr = ptr.parent;
            }

            node.SetBindings(application, rootElement, createdId, enabledId, updatedId, lateId);

            return node;
        }

        public static void Release(LinqBindingNode node) {
            s_NodePool.Release(node);
            node.localVariable?.Release();
        }

        public ContextVariable<T> GetRepeatItem<T>(int id) {
            return (ContextVariable<T>) GetContextVariable(id);
        }

    }

}