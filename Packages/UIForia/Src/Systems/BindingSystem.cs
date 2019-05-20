using System;
using UIForia.Bindings;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Templates;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class BindingSystem : ISystem {

        private readonly SkipTree<BindingNode> m_ReadBindingTree;

        public BindingSystem() {
            this.m_ReadBindingTree = new SkipTree<BindingNode>();
        }

        public void OnReset() {
            m_ReadBindingTree.TraversePreOrder((node) => { node.OnReset(); }, true);
            m_ReadBindingTree.Clear();
        }

        public void OnUpdate() {
            // fast iteration
            // reasonable add remove-hierarchy performance
            // low memory
            // handles adding / removing while running
            // linked list makes sense since we're traversing elements anyway

            m_ReadBindingTree.ConditionalTraversePreOrder((node) => node.OnUpdate());
        }


        public void OnDestroy() { }

        public void OnViewAdded(UIView view) { }

        public void OnViewRemoved(UIView view) { }

        public void OnElementCreated(UIElement element) {
            UITemplate template = element.OriginTemplate;

            if (template == null) return;

            for (int i = 0; i < template.triggeredBindings.Length; i++) {
                if (template.triggeredBindings[i].bindingType == BindingType.Constant) {
                    template.triggeredBindings[i].Execute(element, element.templateContext);
                }
            }

            if (element is UIRepeatElement repeat) {
                ReflectionUtil.TypeArray2[0] = repeat.listType;
                ReflectionUtil.TypeArray2[1] = repeat.itemType;

                ReflectionUtil.ObjectArray2[0] = repeat;
                ReflectionUtil.ObjectArray2[1] = repeat.listExpression;

                RepeatBindingNode node = (RepeatBindingNode) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(RepeatBindingNode<,>),
                    ReflectionUtil.TypeArray2,
                    ReflectionUtil.ObjectArray2
                );

                node.bindings = template.perFrameBindings;
                node.element = repeat;
                node.template = repeat.template;
                node.context = repeat.templateContext;
                m_ReadBindingTree.AddItem(node);
            }
            else if (element is RenderBlockElement renderBlock) {
                // find the binding for the render block id
                // if binding is constant we don't need to register a binding node unless 
                // if binding is dynamic we need to run a binding that will replace the block with what it should be
                throw new NotImplementedException("<RenderBlock> is not yet supported, need to figure out how to handle bindings");
//                RenderBlockIdBinding node = new RenderBlockIdBinding();
//                node.bindings = template.perFrameBindings;
//                node.element = renderBlock;
//                node.context = element.templateContext;
//                m_ReadBindingTree.AddItem(node);
            }
            else {
                if (template.perFrameBindings.Length > 0) {
                    BindingNode node = new BindingNode();
                    node.bindings = template.perFrameBindings;
                    node.element = element;
                    node.context = element.templateContext;
                    int enabledBindingCount = 0;
                    for (int i = 0; i < node.bindings.Length; i++) {
                        if (!(node.bindings[i] is EnabledBinding)) {
                            break;
                        }

                        enabledBindingCount++;
                    }

                    node.enableBindingCount = enabledBindingCount;
                    m_ReadBindingTree.AddItem(node);
                }

                if (template.writeBindings != null && template.writeBindings.Length > 0) {
                    Type elementType = element.GetType();
                    for (int i = 0; i < template.writeBindings.Length; i++) {
                        WriteBinding writeBinding = (WriteBinding) template.writeBindings[i];

                        WriteBindingWrapper wrapper = (WriteBindingWrapper) ReflectionUtil.CreateGenericInstanceFromOpenType(
                            typeof(WriteBindingWrapper<>),
                            writeBinding.genericArguments,
                            new ConstructorArguments(writeBinding, element)
                        );

                        // todo -- support more generic arg types
                        Delegate del = Delegate.CreateDelegate(
                            ReflectionUtil.CreateGenericType(typeof(Action<>), writeBinding.genericArguments),
                            wrapper,
                            wrapper.GetType().GetMethod("Invoke")
                        );

                        elementType.GetEvent(writeBinding.eventName).AddEventHandler(element, del);
                    }
//                    
//                    BindingNode node = new BindingNode();
//                    node.bindings = template.writeBindings;
//                    node.element = element;
//                    node.context = element.templateContext;

//                    element.GetType().GetEvent(m_WriteBindingTree.evtname).AddMethod.Invoke((values) => writeBinding.Execute(context));
                    // m_WriteBindingTree.AddItem(node);
                }
            }
        }

        private class WriteBindingWrapper {

            public readonly UIElement element;
            public readonly ExpressionContext context;
            public readonly WriteBinding writeBinding;

            public WriteBindingWrapper(WriteBinding binding, UIElement element) {
                this.writeBinding = binding;
                this.element = element;
                this.context = element.templateContext;
            }

        }

        private class WriteBindingWrapper<T> : WriteBindingWrapper {

            public WriteBindingWrapper(WriteBinding binding, UIElement element) : base(binding, element) { }

            public void Invoke(T val) {
                writeBinding.Execute(element, context);
                Debug.Log("Writing");
            }

        }

        public void OnElementDestroyed(UIElement element) {
            m_ReadBindingTree.RemoveHierarchy(element);
        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string attributeValue) { }

        internal SkipTree<BindingNode> GetReadTree() {
            return m_ReadBindingTree;
        }

        public bool UpdateReadBinding(UIElement element, string bindingId) {
            BindingNode node = m_ReadBindingTree.GetItem(element.id);
            if (node != null) {
                Binding[] bindings = node.bindings;
                for (int i = 0; i < bindings.Length; i++) {
                    if (bindings[i].bindingId == bindingId) {
                        bindings[i].Execute(element, node.context);
                        return true;
                    }
                }
            }

            return false;
        }

    }

}