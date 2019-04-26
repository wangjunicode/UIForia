using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expression;
using UIForia.Util;

namespace UIForia.Templates {

    public class UISlotTemplate : UITemplate {

        public AttributeDefinition slotNameAttr;

        public UISlotTemplate(Application app, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(app, childTemplates, attributes) { }

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
            element.children =  LightListPool<UIElement>.Get();
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            for (int i = 0; i < childTemplates.Count; i++) {
                element.children.Add(childTemplates[i].CreateScoped(inputScope));
                element.children[i].parent = element;
            }

            element.OriginTemplate = this;
            return element;
        }

        public UIElement CreateWithContent(TemplateScope inputScope, List<UITemplate> content) {
            UISlotElement element = new UISlotElement(slotNameAttr.value);

            element.children = new LightList<UIElement>(content.Count);
            TemplateScope s = new TemplateScope(inputScope.rootElement.templateContext.rootObject as UIElement);
            element.templateContext = new ExpressionContext(s.rootElement, element);

            for (int i = 0; i < content.Count; i++) {
                element.children.Add(content[i].CreateScoped(s));
                element.children[i].parent = element;
            }

            element.OriginTemplate = this;
            return element;
        }

    }

}