using System;
using System.Collections.Generic;

namespace Src {

    public class UIChildrenTemplate : UITemplate {

        public UIChildrenTemplate(List<UITemplate> childTemplates = null, List<AttributeDefinition> attributes = null) 
            : base(childTemplates, attributes) { }
        
        public override Type elementType => typeof(UIChildrenElement);

        public override UIElement CreateScoped(TemplateScope inputScope) {
            
            UIChildrenElement element = new UIChildrenElement();
            inputScope.rootElement.transcludedChildren = element;
            element.templateRef = this;
            return element;
            
        }


    }

}