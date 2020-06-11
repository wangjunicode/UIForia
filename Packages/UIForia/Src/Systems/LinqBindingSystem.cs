using UIForia.Elements;

namespace UIForia.Systems {

    public class LinqBindingSystem {

        private UIElement currentElement;

        public void NewUpdateFn(UIElement element) {
            // Debug.Log($"{new string(' ', element.hierarchyDepth * 4)}Before {element.GetDisplayName()}");

            element.bindingNode?.updateBindings?.Invoke(element.bindingNode.root, element);

            if (element.isEnabled) {

                UIElement ptr = element.GetFirstChild();

                // todo -- this is very wrong! if children list gets updated or changed via binding then this can have bad behavior
                // i think i want to flatten this anyway
                // and when new elements are enabled out of order
                // they dont get an update until next frame
                while (ptr != null) {
                    NewUpdateFn(ptr);
                    ptr = ptr.GetNextSibling();
                }

                // Debug.Log($"{new string(' ', element.hierarchyDepth * 4)}After {element.GetDisplayName()}");

                element.bindingNode?.lateBindings?.Invoke(element.bindingNode.root, element);
            }
        }

    }

}