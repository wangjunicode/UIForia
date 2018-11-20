using System;
using System.Collections.Generic;

namespace UIForia {

    public class UIUnmatchedRouteTemplate : UITemplate {

        public UIUnmatchedRouteTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) : base(childTemplates, attributes) { }

        protected override Type elementType => typeof(UIUnmatchedRoute);
        
        public override UIElement CreateScoped(TemplateScope inputScope) {
            UIRouteElement element = new UIUnmatchedRoute();
            AssignChildrenAndTemplate(inputScope, element);
            return element;
        }

    }

    public class UIRouteTemplate : UITemplate {

        private AttributeDefinition pathAttr;
        
        public UIRouteTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) : base(childTemplates, attributes) { }

        protected override Type elementType => typeof(UIRouteElement);

        public override bool Compile(ParsedTemplate template) {
            pathAttr = GetAttribute("path");
            pathAttr.isCompiled = true;    
            return base.Compile(template);
        }

        public override UIElement CreateScoped(TemplateScope inputScope) {
            UIRouteElement element = new UIRouteElement(pathAttr.value);
            AssignChildrenAndTemplate(inputScope, element);
            return element;
        }

    }

}