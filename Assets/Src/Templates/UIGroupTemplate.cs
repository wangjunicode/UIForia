using System;
using System.Collections.Generic;
using System.Linq;

namespace Src {

    public class UIGroupTemplate : UITemplate {

        public UIGroupTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        public override Type elementType => typeof(UIGroupElement);

        public override InitData CreateScoped(TemplateScope inputScope) {
            UIGroupElement instance = new UIGroupElement();

            InitData instanceData = GetCreationData(instance, inputScope.context);

            for (int i = 0; i < childTemplates.Count; i++) {
                instanceData.AddChild(childTemplates[i].CreateScoped(inputScope));
            }

            if (instanceData.children.Count != 0) {
                instanceData.element.templateChildren = instanceData.children.Select(c => c.element).ToArray();
                instanceData.element.ownChildren = instanceData.element.templateChildren;
            }

            return instanceData;
        }

    }

}