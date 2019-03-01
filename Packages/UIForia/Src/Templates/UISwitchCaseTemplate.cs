using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Parsing.Expression;

namespace UIForia.Templates {

    public class UISwitchCaseTemplate : UITemplate {

        public int switchId;
        public int index;

        public UISwitchCaseTemplate(Application app, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) { }

        protected override Type elementType => typeof(UISwitchCaseElement);

        public override UIElement CreateScoped(TemplateScope inputScope) {
            throw new NotImplementedException();
        }


    }

}