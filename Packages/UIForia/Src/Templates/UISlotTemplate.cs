using System;
using System.Collections.Generic;

namespace UIForia {

    public class UISlotTemplate : UITemplate {

        public AttributeDefinition slotNameAttr;

        public UISlotTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        protected override Type elementType => typeof(UISlotElement);
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
            UISlotElement element = new UISlotElement(slotNameAttr.value);
            element.children = new UIElement[childTemplates.Count];
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            for (int i = 0; i < childTemplates.Count; i++) {
                element.children[i] = childTemplates[i].CreateScoped(inputScope);
                element.children[i].parent = element;
                element.children[i].templateParent = element;
            }

            element.OriginTemplate = this;
            return element;
        }

        public UIElement CreateWithContent(TemplateScope inputScope, List<UITemplate> content) {
            UISlotElement element = new UISlotElement(slotNameAttr.value);

            element.children = new UIElement[content.Count];
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);

            for (int i = 0; i < content.Count; i++) {
                element.children[i] = content[i].CreateScoped(inputScope);
                element.children[i].parent = element;
                element.children[i].templateParent = element;
            }

            element.OriginTemplate = this;
            return element;
        }

    }

}