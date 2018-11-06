using System;
using System.Collections.Generic;

namespace Src {

    public class UIChildrenTemplate : UITemplate {

        public UIChildrenTemplate(List<UITemplate> childTemplates = null, List<AttributeDefinition> attributes = null) 
            : base(childTemplates, attributes) { }
        
        public override Type elementType => typeof(UIChildrenElement);

        public override MetaData CreateScoped(TemplateScope inputScope) {
            
            UIChildrenElement element = new UIChildrenElement();
            element.ownChildren = new UIElement[inputScope.inputChildren.Count];
            return null;
        }


    }

}