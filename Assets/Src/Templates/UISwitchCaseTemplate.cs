using System;

namespace Src {

    public class UISwitchCaseTemplate : UITemplate {

        public override Type ElementType => typeof(UISwitchCaseElement);

        public override bool TypeCheck() {
            throw new System.NotImplementedException();
        }

        public override UIElement CreateScoped(TemplateScope scope) {
            throw new System.NotImplementedException();
        }

    }

}