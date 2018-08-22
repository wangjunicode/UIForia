using System.Collections.Generic;

namespace Src {

    public class UIGroupTemplate : UITemplate {

        public UIGroupTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) 
            : base(childTemplates, attributes) { }
        
        public override UIElementCreationData CreateScoped(TemplateScope scope) {

            UIGroupElement instance = new UIGroupElement();

            UIElementCreationData instanceData = GetCreationData(instance, scope.context);

            for (int i = 0; i < childTemplates.Count; i++) {
                scope.SetParent(childTemplates[i].CreateScoped(scope), instanceData);
            }
           
            return instanceData;

        }


    }

}