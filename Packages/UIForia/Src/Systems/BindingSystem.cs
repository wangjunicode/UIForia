﻿using System;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Systems {

    public class BindingSystem : ISystem {

        private readonly SkipTree<BindingNode> m_BindingTree;
        
        public BindingSystem() {
            this.m_BindingTree = new SkipTree<BindingNode>();
        }

        public void OnReset() {
            m_BindingTree.Clear();
        }

        public void OnUpdate() {
            m_BindingTree.TraversePreOrder((node) => {
                node.OnUpdate();
            });
        }

        public void OnDestroy() {}

        public void OnViewAdded(UIView view) { }

        public void OnViewRemoved(UIView view) { }

        public void OnElementCreated(UIElement element) {

            UITemplate template = element.OriginTemplate;

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
                m_BindingTree.AddItem(node);
            }
            else {
                
                if (template.perFrameBindings.Length > 0) {
                    BindingNode node = new BindingNode();
                    node.bindings = template.perFrameBindings;
                    node.element = element;
                    node.context = element.templateContext;
                    m_BindingTree.AddItem(node);
                }

                if (element.children != null) {
                    for (int i = 0; i < element.children.Count; i++) {
                       OnElementCreated(element.children[i]);
                    }
                }
            }

        }

        public void OnElementDestroyed(UIElement element) {
            m_BindingTree.RemoveHierarchy(element);
        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

    }

}