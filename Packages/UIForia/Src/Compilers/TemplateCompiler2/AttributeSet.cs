using System;
using UIForia.Util;

namespace UIForia.Compilers {

    public enum BindingType {

        Slot,
        Standard,
        Expanded,
        EntryPoint

    }

    public struct AttributeSet {

        public readonly BindingType bindingType;
        public readonly ReadOnlySizedArray<AttrInfo> attributes;
        public readonly ReadOnlySizedArray<TemplateContextReference> contextTypes;

        public AttributeSet(ReadOnlySizedArray<AttrInfo> attributes, BindingType bindingType, ReadOnlySizedArray<TemplateContextReference> contextTypes) {
            this.attributes = attributes;
            this.bindingType = bindingType;
            this.contextTypes = contextTypes;
        }

    }

}