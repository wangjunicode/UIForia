using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expressions;

namespace UIForia.Templates {

    public class UISlotTemplate : UITemplate {

        public AttributeDefinition slotNameAttr;

        public UISlotTemplate(Application app, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) { }

        protected override Type elementType => typeof(UISlotDefinition);
        public string SlotName => slotNameAttr.value;

        public override void Compile(ParsedTemplate template) {
            slotNameAttr = GetAttribute("name");
            slotNameAttr.isCompiled = true;
            base.Compile(template);
        }

        public override UIElement CreateScoped(TemplateScope inputScope) {
            throw new Exception("Should never call CreateScoped on a UISlotTemplate");
        }

        public UIElement CreateWithDefault(TemplateScope inputScope) {
            UISlotDefinition definition = new UISlotDefinition(slotNameAttr.value);
            definition.templateContext = new ExpressionContext(inputScope.rootElement, definition);
            for (int i = 0; i < childTemplates.Count; i++) {
                definition.children.Add(childTemplates[i].CreateScoped(inputScope));
                definition.children[i].parent = definition;
            }

            definition.OriginTemplate = this;
            return definition;
        }

        public UIElement CreateWithContent(TemplateScope inputScope, List<UITemplate> content) {
            UISlotDefinition definition = new UISlotDefinition(slotNameAttr.value);

            TemplateScope s = new TemplateScope(inputScope.rootElement.templateContext.rootObject as UIElement);
            definition.templateContext = new ExpressionContext(s.rootElement, definition);

            for (int i = 0; i < content.Count; i++) {
                definition.children.Add(content[i].CreateScoped(s));
                definition.children[i].parent = definition;
            }

            definition.OriginTemplate = this;
            return definition;
        }

    }

}