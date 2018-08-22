using System;
using System.Collections.Generic;

namespace Src {

    public class UISwitchDefaultTemplate : UITemplate {

        public UISwitchDefaultTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }
        
        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            throw new NotImplementedException();
        }


    }

}