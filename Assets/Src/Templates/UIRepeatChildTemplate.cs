using System.Collections.Generic;
using System.Linq;

namespace Src {

    public class UIRepeatChildTemplate : UITemplate {

        public UIRepeatChildTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) :
            base(childTemplates, attributes) {
            // clone so parent can clear
            this.childTemplates = new List<UITemplate>(childTemplates);
        }

        public override UIElementCreationData CreateScoped(TemplateScope scope) {

            UIRepeatChild instance = new UIRepeatChild();

            return GetCreationData(instance, scope.context);

        }

    }

}