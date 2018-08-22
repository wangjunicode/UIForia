using Rendering;

namespace Src.StyleBindings {

    public abstract class StyleBinding : Binding {

        protected readonly StyleState state;
        
        protected StyleBinding(StyleState state) {
            this.state = state;
        }

        public abstract void Apply(UIStyle style, UITemplateContext context);

        public abstract void Apply(UIStyleSet styleSet, UITemplateContext context);

    }

}