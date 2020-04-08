using System;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public struct AttrInfo {

        public string key;
        public string value;
        public string rawValue;
        public int depth;
        public int line;
        public int column;
        public AttributeType type;
        public AttributeFlags flags;

        public AttrInfo(int depth, in AttributeDefinition attr) {
            this.depth = depth;
            this.key = attr.key;
            this.value = attr.value;
            this.line = attr.line;
            this.column = attr.column;
            this.type = attr.type;
            this.flags = attr.flags;
            this.rawValue = attr.rawValue;
        }

    }

    public enum AttributeSetType {

        Slot,
        Standard,
        Expanded,
        EntryPoint

    }

    public struct AttributeSet {

        public readonly AttributeSetType attributeSetType;
        public readonly ReadOnlySizedArray<AttrInfo> attributes;
        public readonly ReadOnlySizedArray<TemplateContextReference> contextTypes;

        public AttributeSet(ReadOnlySizedArray<AttrInfo> attributes, AttributeSetType attributeSetType, ReadOnlySizedArray<TemplateContextReference> contextTypes) {
            this.attributes = attributes;
            this.attributeSetType = attributeSetType;
            this.contextTypes = contextTypes;
        }

    }

}