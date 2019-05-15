using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Util;

namespace UIForia.Bindings {

    public class BindingNode : IHierarchical {

        public Binding[] bindings;
        public UIElement element;
        public ExpressionContext context;
        public int enableBindingCount;

        public virtual bool OnUpdate() {
            
            for (int i = 0; i < enableBindingCount; i++) {
                bindings[i].Execute(element, context);
            }
            
            if (element.isDisabled) return false;
            
            for (int i = enableBindingCount; i < bindings.Length; i++) {
                bindings[i].Execute(element, context);
            }

            return element.isEnabled;
        }

        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

        public virtual void OnReset() {}
    }
}
