using UIForia.Style;

namespace UIForia {

    internal struct TemplateInfo {

        public int templateHostId;
        public int templateOriginId;
        public TagId tagNameId;
        public ElementTypeClass typeClass;

    }

    public enum ElementTypeClass {

        Container,
        Template,
        Text,
        Image,
        ScrollView

    }

}