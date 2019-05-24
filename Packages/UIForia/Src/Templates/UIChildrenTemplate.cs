using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expression;

namespace UIForia.Templates {

    public class UIChildrenTemplate : UITemplate {

        private bool isTemplate;

        public UITemplate defaultContent;
        
        public UIChildrenTemplate(Application app, List<UITemplate> childTemplates = null, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) {
            if (childTemplates != null) {
                if (childTemplates.Count > 1) {
                    defaultContent = new MultiChildContainerTemplate(app, childTemplates);
                }
                else {
                    defaultContent = childTemplates[0];
                }
            }
        }

        protected override Type elementType => typeof(UIChildrenElement);

        public override void Compile(ParsedTemplate template) {
            AttributeDefinition typeAttr = GetAttribute("x-type");
            CompileInputBindings(template, false);
            CompileStyleBindings(template);
            ResolveBaseStyles(template);
            CompilePropertyBindings(template);
            BuildBindings();
            
            if (typeAttr != null && typeAttr.value == "template") {
                isTemplate = true;
            }
            
        }

        public override UIElement CreateScoped(TemplateScope inputScope) {
            UIChildrenElement element = new UIChildrenElement();
            inputScope.rootElement.TranscludedChildren = element;
            element.OriginTemplate = this;
            element.templateScope = inputScope;
            element.template = isTemplate ? childTemplates[0] : null;
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            
            if (isTemplate && defaultContent != null) {
                    
            }
            
            return element;
            
        }


    }

}