
namespace Src.Systems {

    public class BindingSystem {

        // todo can probaby be optimized a bit with an id-> template binding map
        // build template hierarchy and use that instead of true element hierarchy

        private SkipTree<TemplateBinding> bindingSkipTree;
//        private readonly Dictionary<int, TemplateBinding> map;

        public BindingSystem() {
            this.bindingSkipTree = new SkipTree<TemplateBinding>();
        }

        public void Register(UIElement element, Binding[] bindings, UITemplateContext context) {
            if (bindings == null || bindings.Length == 0) return;
            bindingSkipTree.AddItem(new TemplateBinding(element, bindings, context));
        }

        public void Update() {
            bindingSkipTree.TraverseRecursePreOrder();
        }

        public void Reset() {
            bindingSkipTree = new SkipTree<TemplateBinding>();
        }

    }

}