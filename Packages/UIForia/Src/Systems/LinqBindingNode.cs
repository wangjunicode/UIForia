using UIForia.Bindings;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class LinqBindingNode {

        public UIElement root;
        public UIElement element;
        public TemplateContext ctx;
        private int iteratorIndex;
        public LightList<LinqBindingNode> children;
        public LightList<LinqBinding>.ListSpan bindings;
            
        public int phase;
            
        public LinqBinding enabledBinding;
        public LinqBindingSystem system;

        internal void Initialize(LinqBindingSystem system, UIElement root, UIElement element, TemplateContext ctx, LinqBinding enabledBinding) {
            this.system = system;
            this.phase = system.previousPhase;
            this.root = root;
            this.element = element;
            this.enabledBinding = enabledBinding;
            this.ctx = ctx;
            this.children = LightList<LinqBindingNode>.Get();
            //this.bindings = LightList<LinqBinding>.Get();
            this.iteratorIndex = -1;
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
            
            for(int i = bindingStart; i < bindingsEnd; i++) {
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
            
        public static LinqBindingNode Get(int count) {
            return new LinqBindingNode();
        }
            
            

    }

}