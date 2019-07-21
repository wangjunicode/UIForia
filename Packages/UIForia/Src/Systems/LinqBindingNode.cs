using UIForia.Bindings;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    internal class LinqBindingNode {

        internal UIElement root;
        internal UIElement element;
        internal TemplateContext ctx;
        internal int iteratorIndex;
        internal LightList<LinqBindingNode> children;
        internal LightList<LinqBinding>.ListSpan bindings;

        internal int phase;

        internal LinqBinding enabledBinding;
        internal LinqBindingSystem system;

        public LinqBindingNode() {
            this.children = new LightList<LinqBindingNode>(4);    
        }
        
        public void AddChild(LinqBindingNode childNode) {
            children.Add(childNode);
        }

        public void Update() {
            system.currentlyActive = this;

            enabledBinding?.Execute(root, element, ctx);

            if (!element.isEnabled) {
                return;
            }

            // todo -- might eventually support adding / removing bindings, will need to handle that here
            LinqBinding[] bindingArray = bindings.list.array;
            int bindingStart = bindings.start;
            int bindingsEnd = bindings.end;

            for (int i = bindingStart; i < bindingsEnd; i++) {
                bindingArray[i].Execute(root, element, ctx);
            }

            if (!element.isEnabled) {
                return;
            }

            iteratorIndex = 0;

            int activePhase = system.currentPhase;

            while (iteratorIndex < children.Count) {
                LinqBindingNode child = children.Array[iteratorIndex++];

                if (child.phase != activePhase) {
                    child.Update();
                    system.currentlyActive = this;
                }
            }

            iteratorIndex = -1;
        }

        public void InsertChild(LinqBindingNode bindingNode) {
            Debug.Assert(bindingNode != null, "bindingNode != null");
            // iterate children until index found where traversal index < element.traversal index (basically use depth comparer)
            // todo insert from correct index, not at the end
            int index = children.Count;

            bindingNode.phase = system.previousPhase;
            children.Add(bindingNode);
            if (iteratorIndex >= 0 && iteratorIndex > index) {
                iteratorIndex = index;
            }
        }

        public void InsertChildren(LightList<LinqBindingNode> childrenToAdd) {
            // children will already be in the correct order, so index is the first index
            // todo insert from correct index, not at the end
            int index = children.Count;

            for (int i = 0; i < childrenToAdd.Count; i++) {
                childrenToAdd[i].phase = system.previousPhase;
                children.Add(childrenToAdd[i]);
            }

            if (iteratorIndex >= 0 && iteratorIndex > index) {
                iteratorIndex = index;
            }
        }

        public void RemoveChild(int index) {
            children.RemoveAt(index);
            if (iteratorIndex >= 0 && iteratorIndex >= index) {
                iteratorIndex = index;
            }
        }

        public static LinqBindingNode Get(TemplateScope2 scope, UIElement element, UIElement rootElement, TemplateContext ctx, LinqBinding enabledBinding, LightList<LinqBinding>.ListSpan bindings) {
            LinqBindingNode node = new LinqBindingNode();
            node.bindings = bindings;
            node.ctx = ctx;
            node.enabledBinding = enabledBinding;
            node.root = rootElement;
            node.element = element;
            node.system = scope.application.LinqBindingSystem;
            node.phase = node.system.previousPhase;
            node.iteratorIndex = -1;
            node.bindings = default;
            return node;
        }

    }

}