using System;

namespace Src {

    public class UISwitchDefaultTemplate : UITemplate {

        public override RegistrationData CreateScoped(TemplateScope scope) {
            throw new System.NotImplementedException();
        }

        public override Type ElementType => typeof(UISwitchDefaultElement);

    }

}