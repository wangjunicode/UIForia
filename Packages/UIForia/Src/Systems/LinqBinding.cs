using UIForia.Elements;

namespace UIForia.Systems {

    public abstract class LinqBinding {

        public abstract void Execute(UIElement templateRoot, UIElement current, TemplateContext context);


        public abstract bool CanBeShared { get; }

    }

    public class LinqContentTagBinding : LinqBinding {

        public override void Execute(UIElement templateRoot, UIElement current, TemplateContext context) {
            // bindingFn(current, current, context);
        }

        public override bool CanBeShared => false;

    }
    
}