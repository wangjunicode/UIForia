using System.Collections.Generic;

namespace Src {

    public class UISwitchDefaultTemplate : UITemplate {

        public int switchId;

        public UISwitchDefaultTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }
        
        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            UISwitchDefaultElement instance = new UISwitchDefaultElement();
            UIElementCreationData data = GetCreationData(instance, scope.context);
            
            for (int i = 0; i < childTemplates.Count; i++) {
                scope.SetParent(childTemplates[i].CreateScoped(scope), data);
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