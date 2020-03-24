using System;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Parsing {

    public enum SlotType {

        Define,
        Forward,
        Override,
        Template

    }

    public class SlotNode : TemplateNode {

        public readonly string slotName;
        public readonly SlotType slotType;
        
        public StructList<AttributeDefinition> injectedAttributes;

        public SlotNode(string slotName, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo, SlotType slotType)
            : base(attributes, templateLineInfo) {
            this.slotName = slotName;
            this.slotType = slotType;
            
            switch (slotType) {
                case SlotType.Define:
                    processedType = TypeProcessor.GetProcessedType(typeof(UISlotDefinition));
                    break;

                case SlotType.Forward:
                    processedType = TypeProcessor.GetProcessedType(typeof(UISlotForward));
                    break;

                case SlotType.Override:
                    processedType = TypeProcessor.GetProcessedType(typeof(UISlotOverride));
                    break;

                case SlotType.Template:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(slotType), slotType, null);
            }
        }

        public AttributeDefinition[] GetAttributes(AttributeType expose) {
            if (attributes == null) {
                return null;
            }

            int cnt = 0;
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == expose) {
                    cnt++;
                }
            }

            if (cnt == 0) return null;
            int idx = 0;
            AttributeDefinition[] retn = new AttributeDefinition[cnt];
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == expose) {
                    retn[idx++] = attributes.array[i];
                }
            }

            return retn;
        }

        public override string GetTagName() {
            switch (slotType) {
                case SlotType.Define:
                    return "define:" + slotName;
                case SlotType.Forward:
                    return "forward:" + slotName;
                case SlotType.Override:
                    return "override:" + slotName;
                case SlotType.Template:
                    return "template:" + slotName;
            }

            return "slot:" + slotName;
        }

    }

}