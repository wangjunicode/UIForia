using System;
using System.Collections.Generic;

namespace UIForia {

    public class UIChildrenTemplate : UITemplate {

        public UIChildrenTemplate(List<UITemplate> childTemplates = null, List<AttributeDefinition> attributes = null) 
            : base(childTemplates, attributes) { }

        protected override Type elementType => typeof(UIChildrenElement);

        public override void Compile(ParsedTemplate template) {
            CompileStyleBindings(template);
            ResolveBaseStyles(template);
            BuildBindings();
        }

        public override UIElement CreateScoped(TemplateScope inputScope) {
            
            UIChildrenElement element = new UIChildrenElement();
            inputScope.rootElement.TranscludedChildren = element;
            element.OriginTemplate = this;
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            return element;
            
        }


    }

}