using System;
using System.Collections.Generic;

namespace UIForia {

    public class UISlotTemplate : UITemplate {

        private AttributeDefinition slotIdAttr;

        public UISlotTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        protected override Type elementType => typeof(UISlotElement);

        public override bool Compile(ParsedTemplate template) {
            slotIdAttr = GetAttribute("slotId");
            slotIdAttr.isCompiled = true;
            return base.Compile(template);
        }

        public override UIElement CreateScoped(TemplateScope inputScope) {
            throw new NotImplementedException();
//            UISlotElement element = new UISlotElement(slotIdAttr.value);
//            inputScope.rootElement.SetSlotContent(slotIdAttr, element);
//            AssignChildrenAndTemplate(inputScope, element);
//            return element;
        }

    }

}