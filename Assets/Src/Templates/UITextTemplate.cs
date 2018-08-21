using System;

namespace Src {

    public class UITextTemplate : UITemplate {

        private Binding[] bindings;

        public readonly string text;

        public UITextTemplate(string text) {
            this.text = text;
        }

       public override Type ElementType => typeof(string);

        public void Compile() {
            bindings = new Binding[attributes.Count];
        }

        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            throw new NotImplementedException();//
//          //  UITextElement instance = new UITextElement();
//            ApplyConstantStyles(instance, scope);            
//
//            return new UIElementCreationData(instance, bindings, scope.context);
        }

    }


}
