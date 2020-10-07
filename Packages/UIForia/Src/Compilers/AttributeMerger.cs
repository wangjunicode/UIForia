using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

    internal static class AttributeMerger {

        public static StructList<AttributeDefinition> MergeSlotAttributes(StructList<AttributeDefinition> innerAttributes, SlotAttributeData slotAttributeData, StructList<AttributeDefinition> outerAttributes) {
            StructList<AttributeDefinition> retn = new StructList<AttributeDefinition>();

            if (innerAttributes == null && outerAttributes == null) {
                retn = new StructList<AttributeDefinition>();
            }

            if (innerAttributes != null) {
                retn.AddRange(innerAttributes);
            }

            if (outerAttributes != null) {
                for (int i = 0; i < outerAttributes.size; i++) {
                    AttributeDefinition attrCopy = outerAttributes[i];
                    attrCopy.slotAttributeData = slotAttributeData;
                    retn.Add(attrCopy);
                }
            }

            return retn;
        }

        public static StructList<AttributeDefinition> MergeModifySlotAttributes(StructList<AttributeDefinition> innerAttributes, StructList<AttributeDefinition> outerAttributes) {
            StructList<AttributeDefinition> retn = new StructList<AttributeDefinition>();

            if (innerAttributes == null && outerAttributes == null) {
                retn = new StructList<AttributeDefinition>();
            }

            if (innerAttributes != null) {
                retn.AddRange(innerAttributes);
            }

            if (outerAttributes != null) {
                for (int i = 0; i < outerAttributes.size; i++) {
                    AttributeDefinition attrCopy = outerAttributes[i];
                    retn.Add(attrCopy);
                }
            }

            return retn;
        }

        // Template Root attribute rules
        // - No if binding allowed
        // - No property bindings allowed 
        // - Dynamic attr bindings are ok 
        // - Dynamic style bindings are ok
        // - Style bindings are ok
        // - Event subscriptions are ok
        // - Input handler declarations are ok
        // - Context variables are ok
        public static void ConvertToAttrInfo(TemplateFileShell fileShell, int templateNode, StructList<AttrInfo> output) {
            ref TemplateASTNode node = ref fileShell.templateNodes[templateNode];

            output.EnsureAdditionalCapacity(node.attributeCount);

            // file name maybe?
            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                ref AttributeDefinition3 attr = ref fileShell.attributeList[i];
                ref AttrInfo attrInfo = ref output.array[output.size++];
                attrInfo.key = fileShell.GetString(attr.key);
                attrInfo.value = fileShell.GetString(attr.value);
                attrInfo.line = attr.line;
                attrInfo.column = attr.column;
                attrInfo.flags = attr.flags;
                attrInfo.isInjected = false;
                attrInfo.type = attr.type;
                attrInfo.depth = 0;
            }
        }

        public static void MergeExpandedAttributes(TemplateFileShell fileShellInner, int innerNodeId, TemplateFileShell fileShellOuter, int outerNodeId, StructList<AttrInfo> output) {

            output.size = 0;

            const AttributeType replacedType = AttributeType.Attribute | AttributeType.InstanceStyle;

            ref TemplateASTNode innerNode = ref fileShellInner.templateNodes[innerNodeId];
            ref TemplateASTNode outerNode = ref fileShellOuter.templateNodes[outerNodeId];

            output.EnsureAdditionalCapacity(innerNode.attributeCount + outerNode.attributeCount);

            // file name maybe?
            for (int i = innerNode.attributeRangeStart; i < innerNode.attributeRangeEnd; i++) {
                ref AttributeDefinition3 attr = ref fileShellInner.attributeList[i];
                ref AttrInfo attrInfo = ref output.array[output.size++];
                attrInfo.key = fileShellInner.GetString(attr.key);
                attrInfo.value = fileShellInner.GetString(attr.value);
                attrInfo.line = attr.line;
                attrInfo.column = attr.column;
                attrInfo.flags = attr.flags;
                attrInfo.isInjected = false;
                attrInfo.type = attr.type;
                attrInfo.depth = 0;
            }

            for (int i = outerNode.attributeRangeStart; i < outerNode.attributeRangeEnd; i++) {
                ref AttributeDefinition3 attr = ref fileShellOuter.attributeList[i];
                AttrInfo attrInfo = new AttrInfo();
                attrInfo.key = fileShellOuter.GetString(attr.key);
                attrInfo.value = fileShellOuter.GetString(attr.value);
                attrInfo.line = attr.line;
                attrInfo.column = attr.column;
                attrInfo.flags = attr.flags;
                attrInfo.isInjected = false;
                attrInfo.type = attr.type;
                attrInfo.depth = 1;

                int idx = ContainsAttr(attrInfo, output);
                if (idx == -1) {
                    output.array[output.size++] = attrInfo;
                }
                else if ((attr.type & replacedType) != 0) {
                    output.array[idx] = attrInfo;
                }
                else {
                    output.array[output.size++] = attrInfo;
                }
            }
        }

        public static StructList<AttributeDefinition> MergeExpandedAttributes(StructList<AttributeDefinition> innerAttributes, StructList<AttributeDefinition> outerAttributes) {
            StructList<AttributeDefinition> output = null;

            if (innerAttributes == null) {
                if (outerAttributes == null) {
                    return null;
                }

                output = new StructList<AttributeDefinition>(outerAttributes.size);
                for (int i = 0; i < outerAttributes.size; i++) {
                    AttributeDefinition attr = outerAttributes.array[i];
                    output.AddUnsafe(attr);
                }

                return output;
            }

            if (outerAttributes == null) {
                output = new StructList<AttributeDefinition>(innerAttributes.size);
                for (int i = 0; i < innerAttributes.size; i++) {
                    AttributeDefinition attr = innerAttributes.array[i];
                    attr.flags |= AttributeFlags.InnerContext;
                    output.AddUnsafe(attr);
                }

                return output;
            }

            output = new StructList<AttributeDefinition>(innerAttributes.size + outerAttributes.size);

            for (int i = 0; i < innerAttributes.size; i++) {
                AttributeDefinition attr = innerAttributes.array[i];
                attr.flags |= AttributeFlags.InnerContext;
                output.AddUnsafe(attr);
            }

            const AttributeType replacedType = AttributeType.Attribute | AttributeType.InstanceStyle;

            for (int i = 0; i < outerAttributes.size; i++) {
                ref AttributeDefinition attr = ref outerAttributes.array[i];

                int idx = ContainsAttr(attr, output);

                if (idx == -1) {
                    output.AddUnsafe(attr);
                    continue;
                }

                if ((attr.type & replacedType) != 0) {
                    output.array[idx] = attr;
                }
                else {
                    output.AddUnsafe(attr);
                }
            }

            return output;
        }

        private static int ContainsAttr(in AttrInfo a, StructList<AttrInfo> list) {
            for (int i = 0; i < list.size; i++) {
                ref AttrInfo b = ref list.array[i];
                if (a.type == b.type && a.key == b.key) {
                    return i;
                }
            }

            return -1;
        }


        private static int ContainsAttr(in AttributeDefinition a, StructList<AttributeDefinition> list) {
            for (int i = 0; i < list.size; i++) {
                ref AttributeDefinition b = ref list.array[i];
                if (a.type == b.type && a.key == b.key) {
                    return i;
                }
            }

            return -1;
        }

    }

}