using System;
using System.Collections.Generic;

namespace UIForia {

    public class UISlotContentTemplate : UITemplate {

        public AttributeDefinition slotNameAttr;

        public UISlotContentTemplate(List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null)
            : base(childTemplates, attributes) { }

        protected override Type elementType => null;
        public string SlotName => GetAttribute("name").value;

        public override void Compile(ParsedTemplate template) {
            slotNameAttr = GetAttribute("name");
        }

        public override UIElement CreateScoped(TemplateScope inputScope) {
            throw new Exception("Should never call CreateScoped on a UISlotContentTemplate. This might be being called because you have a <SlotContent/> element at the root level of a template");
        }

    }

}