using System;
using System.Collections.Generic;

namespace UIForia {
    
    public class UIRouterTemplate : UITemplate {

        private AttributeDefinition pathAttr;

        public UIRouterTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) {
        }

        protected override Type elementType => typeof(UIRouterElement);
        
        public override bool Compile(ParsedTemplate template) {
            pathAttr = GetAttribute("path");
            pathAttr.isCompiled = true;    
            return base.Compile(template);
        }
        
        public override UIElement CreateScoped(TemplateScope inputScope) {
            UIRouterElement element = new UIRouterElement(pathAttr.value);

            AssignChildrenAndTemplate(inputScope, element);
            
            return element;
        }

    }

}