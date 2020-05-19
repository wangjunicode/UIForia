using UIForia.Elements;

namespace UIForia.Systems {

    public class LinqBindingSystem  {
        
        // get rid binding node check by making it a struct
        // ideally we only even look at elements that have update / late update bindings
        // i think late update needs to be interleaved like this or the results get funky
        
        // possible bug: if a childs binding causes parent to reorder children, its possible the same node gets updated twice and other skipped
        
        // could it be that we need to say that if an element was disabled at the start of this run that it wont be updated?
        
        // not sure about that because of late update
        
        
        public void NewUpdateFn(UIElement element) {
            // Debug.Log($"{new string(' ', element.hierarchyDepth * 4)}Before {element.GetDisplayName()}");

            element.bindingNode?.updateBindings?.Invoke(element.bindingNode);

            if (element.isEnabled) {
                
                // if children array changes here, we fucked.
                
                // could maybe say if created or enabled during update loop
                // that element doesnt get an update call, but will get create in its place
                // update vs create
                
                // update calls property change handlers (maybe create does too!)
                // update handles sync vars
                
                // 
                
                for (int i = 0; i < element.children.size; i++) {
                    NewUpdateFn(element.children[i]);
                }

                // Debug.Log($"{new string(' ', element.hierarchyDepth * 4)}After {element.GetDisplayName()}");

                element.bindingNode?.lateBindings?.Invoke(element.bindingNode.root, element);
            }
        }
        
        
    }

}