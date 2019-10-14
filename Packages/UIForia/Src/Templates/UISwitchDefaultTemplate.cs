using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Parsing.Expressions;

namespace UIForia.Templates {

    public class UISwitchDefaultTemplate : UITemplate {

        public UISwitchDefaultTemplate(Application app, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) { }

        protected override Type elementType => typeof(UISwitchDefaultElement);

        public override UIElement CreateScoped(TemplateScope inputScope) {
          throw new NotImplementedException();
        }

    }

}