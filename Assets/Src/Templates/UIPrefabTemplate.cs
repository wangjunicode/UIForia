using System.Collections.Generic;

namespace Src {

    public class UIPrefabTemplate : UITemplate {

        public UIPrefabTemplate(List<AttributeDefinition> attributes)
            : base(null, attributes) { }

        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            throw new System.NotImplementedException();
        }

    }

}