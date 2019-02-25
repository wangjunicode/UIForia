using System;
using System.Collections.Generic;

namespace UIForia {

    public class UISwitchDefaultTemplate : UITemplate {

        public UISwitchDefaultTemplate(Application app, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) { }

        protected override Type elementType => typeof(UISwitchDefaultElement);

        public override UIElement CreateScoped(TemplateScope inputScope) {
          throw new NotImplementedException();
        }

    }

}