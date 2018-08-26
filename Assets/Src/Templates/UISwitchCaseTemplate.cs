using System.Collections.Generic;

namespace Src {

    public class UISwitchCaseTemplate : UITemplate {

        public int switchId;
        public int index;

        public UISwitchCaseTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        public override UIElementCreationData CreateScoped(TemplateScope scope) {
            UISwitchCaseElement instance = new UISwitchCaseElement();
            UIElementCreationData data = GetCreationData(instance, scope.context);
            
            for (int i = 0; i < childTemplates.Count; i++) {
                scope.SetParent(childTemplates[i].CreateScoped(scope), data);
            }

            return data;
        }

        public override bool Compile(ParsedTemplate template) {
            AddConditionalBinding(new SwitchCaseBinding(switchId, index));
            return base.Compile(template);
        }

    }

}