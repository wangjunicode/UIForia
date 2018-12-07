using System;
using System.Collections.Generic;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Systems {

    public class BindingSystem : ISystem {

        private SkipTree<BindingNode>.TreeNode treeRoot;
        private readonly List<BindingNode> repeatNodes;
        private readonly SkipTree<BindingNode> bindingSkipTree;

        private readonly LightList<BindingNode> m_Nodes;

        private bool isTreeDirty;

        public BindingSystem() {
            this.isTreeDirty = true;
            this.repeatNodes = new List<BindingNode>();
            this.bindingSkipTree = new SkipTree<BindingNode>();
            this.m_Nodes = new LightList<BindingNode>();
            this.bindingSkipTree.onTreeChanged += HandleTreeChanged;
        }

        private void HandleTreeChanged(SkipTree<BindingNode>.TreeChangeType changeType) {
            isTreeDirty = true;
        }

        public void OnReset() {
            this.isTreeDirty = true;
            bindingSkipTree.Clear();
            repeatNodes.Clear();
        }

        public void OnUpdate() {
            for (int i = 0; i < m_Nodes.Count; i++) {
                m_Nodes[i].OnUpdate();
            }
        }

        public void OnDestroy() {
            bindingSkipTree.Clear();
            repeatNodes.Clear();
        }

        public void OnViewAdded(UIView view) { }

        public void OnViewRemoved(UIView view) { }

        private static void Step(UIElement current, LightList<BindingNode> nodes) {
            UITemplate template = current.OriginTemplate;

            for (int i = 0; i < template.triggeredBindings.Length; i++) {
                if (template.triggeredBindings[i].bindingType == BindingType.Constant) {
                    template.triggeredBindings[i].Execute(current, current.templateContext);
                }
            }

            if (current is UIRepeatElement repeat) {
                ReflectionUtil.TypeArray2[0] = repeat.listType;
                ReflectionUtil.TypeArray2[1] = repeat.itemType;

                ReflectionUtil.ObjectArray2[0] = repeat;
                ReflectionUtil.ObjectArray2[1] = repeat.listExpression;

                RepeatBindingNode node = (RepeatBindingNode) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(RepeatBindingNode<,>),
                    ReflectionUtil.TypeArray2,
                    ReflectionUtil.ObjectArray2
                );

                node.context = current.templateContext;
                node.element = current;
                node.template = repeat.template;
                
                nodes.Add(node); // repeat spawns w/o children
            }
            else {
                if (template.perFrameBindings.Length > 0) {
                    BindingNode node = new BindingNode();
                    node.bindings = template.perFrameBindings;
                    node.element = current;
                    node.context = current.templateContext;
                    nodes.Add(node);
                }

                if (current.children != null) {
                    for (int i = 0; i < current.children.Length; i++) {
                        Step(current.children[i], nodes);
                    }
                }
            }
        }

        // to disable, need to remove from list since we don't have tree culling
        // or while(depth != currentDepth) ptr++
        
        // assumption, bindings are always active
        // assumption, parent will always exist 
        public void OnElementCreated(UIElement element) {
            LightList<BindingNode> nodes = LightListPool<BindingNode>.Get();

            Step(element, nodes);

            // need to find where to insert the list of nodes
            // obvious if repeat node parent
            if (element.parent != null && element.parent is UIRepeatElement repeatElement) {
                RepeatBindingNode bindingNode = m_Nodes.Find(repeatElement, (e, p) => e.element == p) as RepeatBindingNode;
                if (bindingNode == null) {
                    throw new Exception("Repeat parent not found");
                }
                bindingNode.AddChildNodes(nodes);
                m_Nodes.Add(bindingNode);
            }
            else {
                UIElement ptr = element.parent;
                while (ptr != null) {
                    int idx = m_Nodes.FindIndex(ptr, (e, p) => e.element == p);
                    if (idx != -1) {
                        m_Nodes.InsertRange(idx, nodes);
                        break;
                    }
                    ptr = ptr.parent;
                }

                if (ptr == null) {
                    m_Nodes.AddRange(nodes);
                }
                LightListPool<BindingNode>.Release(ref nodes);
            }

        }

        public void OnElementDestroyed(UIElement element) {
            if (element.parent is UIRepeatElement repeatElement) {
                RepeatBindingNode bindingNode = m_Nodes.Find(repeatElement, (e, p) => e.element == p) as RepeatBindingNode;
                if (bindingNode == null) {
                    throw new Exception("Repeat parent not found");
                }

                bindingNode.RemoveNodes(element);
            }
            else {
                UIElement ptr = element.parent;
                while (ptr != null) {
                    int idx = m_Nodes.FindIndex(ptr, (e, p) => e.element == p);
                    if (idx != -1) {
                        // remove while next binding depth < current depth
                        // todo -- implement block array operations on light list instead of shifting down all the time
                        int depth = element.depth;
                        for (int i = idx + 1; i < m_Nodes.Count; i++) {
                            int d = m_Nodes[i].element.depth;
                            if (d >= depth) {
                                break;
                            }       
                            m_Nodes.RemoveAt(idx + 1);
                        }
                        m_Nodes.RemoveAt(idx);
                        break;
                    }
                    ptr = ptr.parent;
                }

//                if (ptr == null) {
//                    m_Nodes.AddRange(nodes);
//                }
            }
        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

    }

}