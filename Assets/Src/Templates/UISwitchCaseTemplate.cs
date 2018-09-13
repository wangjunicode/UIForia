using System;
using System.Collections.Generic;

namespace Src {

    public class UISwitchCaseTemplate : UITemplate {

        public int switchId;
        public int index;

        public UISwitchCaseTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        public override Type elementType => typeof(UISwitchCaseElement);

        public override MetaData CreateScoped(TemplateScope inputScope) {
            UISwitchCaseElement instance = new UISwitchCaseElement();
            MetaData data = GetCreationData(instance, inputScope.context);
            
            for (int i = 0; i < childTemplates.Count; i++) {
                data.AddChild(childTemplates[i].CreateScoped(inputScope));
            }
            
            return data;
        }

        public override bool Compile(ParsedTemplate template) {
            AddConditionalBinding(new SwitchCaseBinding(switchId, index));
            return base.Compile(template);
        }

    }

}