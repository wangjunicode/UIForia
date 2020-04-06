using UIForia.Elements;

namespace UIForia.Systems {

    public class LinqBindingSystem  {
        
        
        public void NewUpdateFn(UIElement element) {
            // Debug.Log($"{new string(' ', element.hierarchyDepth * 4)}Before {element.GetDisplayName()}");

            element.bindingNode?.updateBindings?.Invoke(element.bindingNode.root, element);

            if (element.isEnabled) {
                
                for (int i = 0; i < element.children.size; i++) {
                    NewUpdateFn(element.children[i]);
                }

                // Debug.Log($"{new string(' ', element.hierarchyDepth * 4)}After {element.GetDisplayName()}");

                element.bindingNode?.lateBindings?.Invoke(element.bindingNode.root, element);
            }
        }
        
        
    }

}