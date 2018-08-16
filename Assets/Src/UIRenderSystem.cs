using System.Collections.Generic;

namespace Src {
    public class UIRenderSystem {
        private List<UIElementPrimitive> dirtyPrimitives;
        private List<UIElement> dirtyLayouts;
        private List<UITemplateContext> dirtyContexts;

        public UIRenderSystem() {
            dirtyContexts = new List<UITemplateContext>();
        }

        public void RenderChanges() {
            for (int i = 0; i < dirtyContexts.Count; i++) {
                UITemplateContext ctx = dirtyContexts[i];
                List<UIElement> dirtyElements = new List<UIElement>();

//                for (int j = 0; j < ctx.dirtyBindings.Count; j++) {
//                    string bindingName = ctx.dirtyBindings[j];
//                    List<UIElement> dependents = ctx.GetBoundElements(bindingName);
//
//                    for (int k = 0; k < dependents.Count; k++) {
//                        UIElement element = dependents[k];
//                        if (!dirtyElements.Contains(element)) {
//                            dirtyElements.Add(element);
//                        }
//                    }
//                }

                for (int j = 0; j < dirtyElements.Count; j++) {
                    UIElement element = dirtyElements[j];
                    UpdateBindings(ctx, element);
                }

                
                
                // for each dirty binding
                // add bound element to render list
                // for each dirty binding
                // for each dependent element
                //add to list if not in list already
                // for each element in list
                // update properties
                // invoke render
            }

          
        }

        private void UpdateBindings(UITemplateContext context, UIElement child) {
//            for (int i = 0; i < child.bindings.Length; i++) {
//                PropertyBinding binding = child.bindings[i];
//                //binding.Value = context.GetBinding(binding.key).Value;
//            }
        }

        /*
         * Element becomes dirty when a bound value changes
         * Then it checks all it's children for dirty
         * if an element is not dirty, its children are not dirty
         * if an element has already been dirty checked, bail
         *
         * once data changes have been applied we want to see if we need to change render properties
         * these would be things like text content, images, backgrounds, and that stuff
         *
         * after that we do a layout pass if needed
         * layout is needed when something's dimensions change
         */
    }
}