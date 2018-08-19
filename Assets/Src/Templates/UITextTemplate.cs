using System;

namespace Src {

    public class UITextTemplate : UITemplate {

        private Binding[] bindings;

        public readonly string text;

        public UITextTemplate(string text) {
            this.text = text;
        }

        public override Type ElementType => typeof(UITextElement);

        public void Compile() {
            bindings = new Binding[attributes.Count];
        }

        public override RegistrationData CreateScoped(TemplateScope scope) {
            // todo -- find solution for constant expressions so they are not registered

            UITextElement instance = new UITextElement();
            ApplyConstantStyles(instance, scope);            

            return new RegistrationData(instance, bindings, scope.context);
        }

    }


}
