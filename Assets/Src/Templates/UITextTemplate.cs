namespace Src {

    public class UITextTemplate : UITemplate {

        public override bool TypeCheck() {
            throw new System.NotImplementedException();
        }

        public override UIElement CreateScoped(TemplateScope scope) {
            return new UITextElement();
        }

    }

}