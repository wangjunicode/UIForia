using Rendering;

namespace Src.Systems {

    public class BindingSystem : ISystem {

        // todo can probaby be optimized a bit with an id-> template binding map
        // build template hierarchy and use that instead of true element hierarchy

        private SkipTree<TemplateBinding> bindingSkipTree;
//        private readonly Dictionary<int, TemplateBinding> map;

        public BindingSystem() {
            this.bindingSkipTree = new SkipTree<TemplateBinding>();
        }
        
        public void OnReset() {
            bindingSkipTree.Clear();
        }

        public void OnUpdate() {
            bindingSkipTree.TraverseRecursePreOrder();
        }

        public void OnDestroy() {
            bindingSkipTree.Clear();
        }

        public void OnElementCreated(UIElementCreationData data) {
            if (data.bindings == null || data.bindings.Length == 0) return;
            bindingSkipTree.AddItem(new TemplateBinding(data.element, data.bindings, data.context));
        }

        public void OnElementEnabled(UIElement element) {
            TemplateBinding binding = bindingSkipTree.GetItem(element);
            if (binding != null) {
                bindingSkipTree.EnableHierarchy(binding);
            }
        }

        public void OnElementDisabled(UIElement element) {
            TemplateBinding binding = bindingSkipTree.GetItem(element);
            if (binding != null) {
                bindingSkipTree.DisableHierarchy(binding);
            }
        }

        public void OnElementDestroyed(UIElement element) {
            TemplateBinding binding = bindingSkipTree.GetItem(element);
            if (binding != null) {
                bindingSkipTree.RemoveHierarchy(binding);
            }
        }

    }

}