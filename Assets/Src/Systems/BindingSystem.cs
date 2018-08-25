using Rendering;
using UnityEngine;

namespace Src.Systems {

    public class BindingSystem : ISystem {

        // todo can probably be optimized a bit with an id-> template binding map
        // build template hierarchy and use that instead of true element hierarchy

        private readonly SkipTree<TemplateBinding> bindingSkipTree;
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

        public void OnInitialize() { }

        public void OnElementCreated(UIElementCreationData data) {
            if (data.constantBindings.Length != 0) {
                for (int i = 0; i < data.constantBindings.Length; i++) {
                    data.constantBindings[i].Execute(data.element, data.context);
                }
            }
            if (data.bindings.Length == 0) return;
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