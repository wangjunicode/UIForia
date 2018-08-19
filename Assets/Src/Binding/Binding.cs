namespace Src {

    public abstract class Binding {

        public abstract void Execute(UIElement element, UITemplateContext context);

        public abstract bool IsConstant();

    }

}