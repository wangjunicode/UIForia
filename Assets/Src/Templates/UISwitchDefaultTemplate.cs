using System;

namespace Src {

    public class UISwitchDefaultTemplate : UITemplate {

        public override bool TypeCheck() {
            throw new System.NotImplementedException();
        }

        public override UIElement CreateScoped(TemplateScope scope) {
            throw new System.NotImplementedException();
        }

        public override Type ElementType => typeof(UISwitchDefaultElement);

    }

}