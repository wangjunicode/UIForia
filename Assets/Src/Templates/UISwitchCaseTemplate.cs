using System;
using System.Collections.Generic;

namespace UIForia {

    public class UISwitchCaseTemplate : UITemplate {

        public int switchId;
        public int index;

        public UISwitchCaseTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        public override Type elementType => typeof(UISwitchCaseElement);

        public override UIElement CreateScoped(TemplateScope inputScope) {
            throw new NotImplementedException();
        }

        public override bool Compile(ParsedTemplate template) {
            AddConditionalBinding(new SwitchCaseBinding(switchId, index));
            return base.Compile(template);
        }

    }

}