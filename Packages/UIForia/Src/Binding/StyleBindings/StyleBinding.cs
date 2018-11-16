using UIForia.Rendering;

namespace UIForia.StyleBindings {

    public abstract class StyleBinding : Binding {

        protected readonly StyleState state;

        protected StyleBinding(string propertyName, StyleState state) : base(propertyName + ":" + state) {
            this.state = state;
        }

        public virtual bool ShouldExecute(UIElement element) {
            return element.style.IsInState(state);
        }

        public virtual void BeforeExecute() { }

        public virtual void AfterExecute() { }

        public abstract void Apply(UIStyle style, UITemplateContext context);

        public abstract void Apply(UIStyleSet styleSet, UITemplateContext context);

    }

}
