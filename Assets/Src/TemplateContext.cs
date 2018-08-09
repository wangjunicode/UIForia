using System.Collections;
using System.Collections.Generic;

namespace Src {

    public class TemplateContext {

        public readonly UIElement element;
        public readonly List<string> dirtyBindings;

        private readonly List<object> contextProviders;
        private readonly Dictionary<string, List<UIElement>> bindingMap;

        public IList currentList;
        
        public TemplateContext(UIElement element) {
            this.element = element;
            bindingMap = new Dictionary<string, List<UIElement>>();
            dirtyBindings = new List<string>();
            contextProviders.Add(element);
        }

        public void CreateBinding(ExpressionBinding binding, UIElement element) { }

        // enable / disable bindings when things are enabled / disabled
        // add new bindings when list adds children
        // remove bindings when list removes children
        
        public void FlushChanges() {
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