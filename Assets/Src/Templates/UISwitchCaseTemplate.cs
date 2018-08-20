using System;

namespace Src {

    public class UISwitchCaseTemplate : UITemplate {

        public override Type ElementType => typeof(UISwitchCaseElement);

        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            throw new System.NotImplementedException();
        }

    }

}