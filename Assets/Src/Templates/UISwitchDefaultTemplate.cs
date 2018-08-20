using System;

namespace Src {

    public class UISwitchDefaultTemplate : UITemplate {

        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            throw new System.NotImplementedException();
        }

        public override Type ElementType => typeof(UISwitchDefaultElement);

    }

}