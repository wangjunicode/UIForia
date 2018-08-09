using System.Collections;
using System.Collections.Generic;
using Rendering;

namespace Src {

    public struct TemplateListInterface {

        public int length;
        public IList source;
        public int currentIndex;
        
    }
    

    public class TemplateContext {

        public UIView view;
        public readonly UIElement element;
        public TemplateListInterface currentList;
        public int currentIndex;
        
        public TemplateContext(UIElement element) {
            this.element = element;
        }

        public void SetListSource(IList source) {
            IList oldList = currentList.source;
            currentList.source = source;
            if (oldList.Count > source.Count) {
                for (int i = source.Count; i < oldList.Count; i++) {
                    element.view.DestroyElement((UIElement)oldList[i]);
                }
            }
        }
        
        public void CreateBinding(ExpressionBinding binding, UIElement element) { }

        // enable / disable bindings when things are enabled / disabled
        // add new bindings when list adds children
        // remove bindings when list removes children
        
        public void FlushChanges() {
            /*
             *
             * bindingSkipTree.Add();
             *
             * traverse()
             *     for each binding
             *         if binding.dirtyCheck(context) -> binding.onChange();
             *
             * if(binding.Evaluate(context) != binding.lastValue)
             *     binding.HandleChange(context);
             * 
             */
            // traverse tree looking for bindings
            // or register each binding and on add / remove be sure to book keep in the context
            
            
            // for each list
                // setup for list
                // for each item
                    // setup for item
                    // for each binding(dirty check)
            // for each dirty checked binding
            // binding.Update();
            // binding.target = binding.Evaluate(this);
        }

        public void RegisterBindings(UIElement uiElement, List<ExpressionBinding> generatedBindings) {
            throw new System.NotImplementedException();
        }

    }

}