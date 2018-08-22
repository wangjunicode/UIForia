using System;
using System.Collections.Generic;

namespace Src {

    public class UISwitchCaseTemplate : UITemplate {

        public UISwitchCaseTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) 
            : base(childTemplates, attributes) { }
        
        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            throw new NotImplementedException();
        }


    }

}