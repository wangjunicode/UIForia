namespace Src {

    public abstract class Binding {

        public bool isEnabled;
        public readonly string bindingId;
        
        protected Binding(string bindingId) {
            this.bindingId = bindingId;
            this.isEnabled = true;
        }
        
        public abstract void Execute(UIElement element, UITemplateContext context);

        public abstract bool IsConstant();

        public static readonly Binding[] EmptyArray = new Binding[0];

    }

}