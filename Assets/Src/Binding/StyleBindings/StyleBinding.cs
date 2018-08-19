using Rendering;

namespace Src.StyleBindings {

    public abstract class StyleBinding : Binding {

        public readonly StyleStateType state;
        
        protected StyleBinding(StyleStateType state) {
            this.state = state;
        }

        public abstract void Apply(UIStyle style, UITemplateContext context);

        public abstract void Apply(UIStyleSet styleSet, UITemplateContext context);

    }

}