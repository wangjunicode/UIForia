using UIForia.Elements;
using UIForia.Expressions;

namespace UIForia.Bindings {

    public abstract class Binding {

        public readonly string bindingId;
        public BindingType bindingType;
        
        protected Binding(string bindingId) {
            this.bindingId = bindingId;
            this.bindingType = BindingType.Read;
        }

        public bool IsOnEnable => bindingType == BindingType.OnEnable;
        public bool IsWrite => bindingType == BindingType.Write;
        public bool IsRead => bindingType == BindingType.Read;

        public abstract void Execute(UIElement element, ExpressionContext context);

        public abstract bool IsConstant();

        public static readonly Binding[] EmptyArray = new Binding[0];

    }

}