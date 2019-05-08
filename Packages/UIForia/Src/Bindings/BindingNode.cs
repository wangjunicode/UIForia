using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Util;

namespace UIForia.Bindings {

    public class BindingNode : IHierarchical {

        public Binding[] bindings;
        public UIElement element;
        public ExpressionContext context;

        public virtual void OnUpdate() {
            for (int i = 0; i < bindings.Length; i++) {
                bindings[i].Execute(element, context);
            }
        }

        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

        public virtual void OnReset() {}
    }
}
