using System;
using UIForia.Exceptions;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class SlotOverrideNode : TemplateNode2 {

        public string slotName;

        public SlotOverrideNode(RootTemplateNode root, TemplateNode2 parent, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) : base(root, parent, processedType, attributes, templateLineInfo) {
            if (attributes == null) {
                throw ParseException.UnnamedSlotOverride(root.filePath, templateLineInfo);
            }

            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Slot && string.Equals(attributes.array[i].key, "name", StringComparison.Ordinal)) {
                    slotName = attributes.array[i].value.Trim();
                    attributes.RemoveAt(i);
                    return;
                }
            }

            throw ParseException.UnnamedSlotOverride(root.filePath, templateLineInfo);
        }

        public SlotOverrideNode(RootTemplateNode root, TemplateNode2 parent, ProcessedType processedType, string slotName, in TemplateLineInfo templateLineInfo) : base(root, parent, processedType, null, templateLineInfo) {
            this.slotName = slotName;
        }

    }

}