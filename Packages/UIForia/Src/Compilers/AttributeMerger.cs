using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public static class AttributeMerger {

        public static SizedArray<AttributeDefinition> MergeSlotAttributes(ReadOnlySizedArray<AttributeDefinition> innerAttributes, SlotAttributeData slotAttributeData, ReadOnlySizedArray<AttributeDefinition> outerAttributes) {
            SizedArray<AttributeDefinition> retn = new SizedArray<AttributeDefinition>();

            if (innerAttributes.array == null && outerAttributes.array == null) {
                retn = new SizedArray<AttributeDefinition>(0);
            }

            if (innerAttributes.array != null) {
                retn.AddRange(innerAttributes);
            }

            if (outerAttributes.array != null) {
                for (int i = 0; i < outerAttributes.size; i++) {
                    AttributeDefinition attrCopy = outerAttributes.array[i];
                    attrCopy.slotAttributeData = slotAttributeData;
                    retn.Add(attrCopy);
                }
            }

            return retn;
        }

        public static SizedArray<AttributeDefinition> MergeModifySlotAttributes(ReadOnlySizedArray<AttributeDefinition> innerAttributes, ReadOnlySizedArray<AttributeDefinition> outerAttributes) {

            SizedArray<AttributeDefinition> retn = new SizedArray<AttributeDefinition>(innerAttributes.size + outerAttributes.size);

            if (innerAttributes.size != 0) {
                retn.AddRange(innerAttributes);
            }

            for (int i = 0; i < outerAttributes.size; i++) {
                AttributeDefinition attrCopy = outerAttributes.array[i];
                retn.Add(attrCopy);
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

        public static void ConvertAttributeDefinitions(ReadOnlySizedArray<AttributeDefinition> attrDefs, ref SizedArray<AttrInfo> output) {
            output.size = 0;
            output.EnsureCapacity(attrDefs.size);

            for (int i = 0; i < attrDefs.size; i++) {
                output.array[i] = new AttrInfo(0, attrDefs.array[i]);
            }

            output.size = attrDefs.size;

        }

        public static void MergeExpandedAttributes2(ReadOnlySizedArray<AttributeDefinition> outerAttributes, ReadOnlySizedArray<AttributeDefinition> innerAttributes, ref SizedArray<AttrInfo> output) {

            const AttributeType replacedType = AttributeType.Attribute | AttributeType.InstanceStyle;

            output.EnsureCapacity(innerAttributes.size + outerAttributes.size);

            int attrIdx = 0;

            for (int i = 0; i < innerAttributes.size; i++) {
                output.array[attrIdx++] = new AttrInfo(1, innerAttributes.array[i]);
            }

            for (int i = 0; i < outerAttributes.size; i++) {
                ref AttributeDefinition attr = ref outerAttributes.array[i];

                int idx = ContainsAttr(attr, output);

                if (idx == -1) {
                    output.array[attrIdx++] = new AttrInfo(0, attr);
                }
                else if ((attr.type & replacedType) != 0) {
                    output.array[idx] = new AttrInfo(0, attr);
                }
                else {
                    output.array[attrIdx++] = new AttrInfo(1, attr);
                }
            }

            output.size = attrIdx;

        }

        public static SizedArray<AttributeDefinition> MergeExpandedAttributes(ReadOnlySizedArray<AttributeDefinition> innerAttributes, ReadOnlySizedArray<AttributeDefinition> outerAttributes) {
            SizedArray<AttributeDefinition> output = default;

            if (innerAttributes.size == 0) {
                if (outerAttributes.size == 0) {
                    return default;
                }

                output = new SizedArray<AttributeDefinition>(outerAttributes.size);
                for (int i = 0; i < outerAttributes.size; i++) {
                    AttributeDefinition attr = outerAttributes.array[i];
                    output.array[i] = attr;
                }

                output.size = outerAttributes.size;

                return output;
            }

            if (outerAttributes.size == 0) {
                output = new SizedArray<AttributeDefinition>(innerAttributes.size);
                for (int i = 0; i < innerAttributes.size; i++) {
                    AttributeDefinition attr = innerAttributes.array[i];
                    attr.flags |= AttributeFlags.InnerContext;
                    output.array[i] = attr;
                }

                output.size = innerAttributes.size;

                return output;
            }

            output = new SizedArray<AttributeDefinition>(innerAttributes.size + outerAttributes.size);

            for (int i = 0; i < innerAttributes.size; i++) {
                AttributeDefinition attr = innerAttributes.array[i];
                attr.flags |= AttributeFlags.InnerContext;
                output.array[i] = attr;
            }

            output.size = innerAttributes.size;

            const AttributeType replacedType = AttributeType.Attribute | AttributeType.InstanceStyle;

            for (int i = 0; i < outerAttributes.size; i++) {
                ref AttributeDefinition attr = ref outerAttributes.array[i];

                int idx = ContainsAttr(attr, output);

                if (idx == -1) {
                    output.Add(attr);
                    continue;
                }

                if ((attr.type & replacedType) != 0) {
                    output.array[idx] = attr;
                }
                else {
                    output.Add(attr);
                }
            }

            return output;
        }

        private static int ContainsAttr(in AttributeDefinition a, ReadOnlySizedArray<AttrInfo> list) {
            for (int i = 0; i < list.size; i++) {
                ref AttrInfo b = ref list.array[i];
                if (a.type == b.type && a.key == b.key) {
                    return i;
                }
            }

            return -1;
        }

        private static int ContainsAttr(in AttributeDefinition a, ReadOnlySizedArray<AttributeDefinition> list) {
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