using System;
using UIForia.Util;

namespace UIForia.Compilers {

    public enum ElementBindingType {

        Slot,
        Standard,
        Expanded,
        EntryPoint

    }

    public struct AttributeSet {

        public readonly ElementBindingType elementBindingType;
        public readonly ReadOnlySizedArray<AttrInfo> attributes;
        public readonly ReadOnlySizedArray<TemplateContextReference> contextTypes;

        public AttributeSet(ReadOnlySizedArray<AttrInfo> attributes, ElementBindingType elementBindingType, ReadOnlySizedArray<TemplateContextReference> contextTypes) {
            this.attributes = attributes;
            this.elementBindingType = elementBindingType;
            this.contextTypes = contextTypes;
        }

    }

}