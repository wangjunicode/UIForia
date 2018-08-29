using System;
using System.Collections.Generic;

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
           
            return instanceData;

        }


    }

}