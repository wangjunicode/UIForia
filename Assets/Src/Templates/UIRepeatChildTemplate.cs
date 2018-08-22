using System.Collections.Generic;

namespace Src {

    public class UIRepeatChildTemplate : UITemplate {

        public UIRepeatChildTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) :
            base(childTemplates, attributes) { }

        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            throw new System.NotImplementedException();
        }

    }

}