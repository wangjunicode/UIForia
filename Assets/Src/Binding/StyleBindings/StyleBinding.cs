using Rendering;

namespace Src.StyleBindings {

    public abstract class StyleBinding : Binding {

        protected readonly StyleState state;
        
        protected StyleBinding(string propertyName, StyleState state) : base(propertyName + ":" + state){
            this.state = state;
        }

        public abstract void Apply(UIStyle style, UITemplateContext context);

        public abstract void Apply(UIStyleSet styleSet, UITemplateContext context);

    }

}