using System;
using System.Collections.Generic;

namespace Src {

    public class UISwitchDefaultTemplate : UITemplate {

        public int switchId;

        public UISwitchDefaultTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        public override Type elementType => typeof(UISwitchDefaultElement);

        public override MetaData CreateScoped(TemplateScope inputScope) {
            UISwitchDefaultElement instance = new UISwitchDefaultElement();
            MetaData data = GetCreationData(instance, inputScope.context);
            
            for (int i = 0; i < childTemplates.Count; i++) {
                data.AddChild(childTemplates[i].CreateScoped(inputScope));
            }

            return data;
        }

        public override bool Compile(ParsedTemplate template) {
            conditionalBindings = new Binding[] {
                new SwitchCaseBinding(switchId, -1)
            };
            return true;
        }


    }

}