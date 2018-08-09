namespace Src {

    public class UISwitchTemplate : UITemplate {

        public override UIElement CreateScoped(TemplateScope scope) {

            // somehow register binding
            NonRenderedElement retn = new NonRenderedElement();
            for (int i = 0; i < childTemplates.Count; i++) {
                // todo -- process children 
            }

            return retn;
        }
        
        public override bool TypeCheck() {
            throw new System.NotImplementedException();
        }

    }

}