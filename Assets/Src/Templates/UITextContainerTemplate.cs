using System;
using System.Collections.Generic;

namespace Src {

    public class UITextContainerTemplate : UITemplate {

        public UITextContainerTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) : base(childTemplates, attributes) { }

        public override Type elementType => typeof(UITextContainerElement);
        
        public override InitData CreateScoped(TemplateScope inputScope) {
            return GetCreationData(new UITextContainerElement(), inputScope.context);
        }

    }

}