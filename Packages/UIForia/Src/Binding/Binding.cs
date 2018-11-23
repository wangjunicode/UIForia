namespace UIForia {

    public abstract class Binding {

        public readonly string bindingId;
        public BindingType bindingType;
        public bool useRootContext;
        
        protected Binding(string bindingId) {
            this.bindingId = bindingId;
            this.bindingType = BindingType.Normal;
        }

        public bool IsOnce => bindingType == BindingType.Once;
        public bool IsOnEnable => bindingType == BindingType.OnEnable;
        public bool IsNormal => bindingType == BindingType.Normal;
        public bool IsTriggered => bindingType != BindingType.Normal;
        
        public abstract void Execute(UIElement element, UITemplateContext context);

        public abstract bool IsConstant();

        public static readonly Binding[] EmptyArray = new Binding[0];

    }

}