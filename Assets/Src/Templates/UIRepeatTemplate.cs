using System;
using System.Collections.Generic;

namespace Src {

    public class UIRepeatTemplate : UITemplate {

        public UIRepeatTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }


        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            throw new NotImplementedException();
        }

    }

}