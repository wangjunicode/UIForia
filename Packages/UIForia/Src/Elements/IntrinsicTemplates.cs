using UIForia.Attributes;
using UIForia.UIInput;

namespace UIForia.Elements {

    [ContainerElement]
    [TemplateTagName("Group")]
    public class UIGroupElement : UIElement {

        public override string GetDisplayName() {
            return "Group";
        }

    }

    [ContainerElement]
    [TemplateTagName("Panel")]
    public class UIPanelElement : UIElement {

        public override string GetDisplayName() {
            return "Panel";
        }

    }

    [ContainerElement]
    [TemplateTagName("Section")]
    public class UISectionElement : UIElement {

        public override string GetDisplayName() {
            return "Section";
        }

    }

    [ContainerElement]
    [TemplateTagName("Div")]
    public class UIDivElement : UIElement {

        public override string GetDisplayName() {
            return "Div";
        }

    }

    [ContainerElement]
    [TemplateTagName("Header")]
    public class UIHeaderElement : UIElement {

        public override string GetDisplayName() {
            return "Header";
        }

    }

    [ContainerElement]
    [TemplateTagName("Footer")]
    public class UIFooterElement : UIElement {

        public override string GetDisplayName() {
            return "Footer";
        }

    }

    [TemplateTagName("Label")]
    public class UILabelElement : UITextElement {

        public string forElement;

        [OnMouseClick]
        public void OnClick() {
            UIElement forEl = parent.FindById(forElement);
            if (forEl is IFocusable focusable) {
                application.InputSystem.RequestFocus(focusable);
            }
        }

        public override string GetDisplayName() {
            return "Label";
        }

    }

    [TemplateTagName("Paragraph")]
    public class UIParagraphElement : UITextElement {

        public override string GetDisplayName() {
            return "Paragraph";
        }

    }

    [TemplateTagName("Heading1")]
    public class UIHeading1Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading1";
        }

    }

    [TemplateTagName("Heading2")]
    public class UIHeading2Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading2";
        }

    }

    [TemplateTagName("Heading3")]
    public class UIHeading3Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading3";
        }

    }

    [TemplateTagName("Heading4")]
    public class UIHeading4Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading4";
        }

    }

    [TemplateTagName("Heading5")]
    public class UIHeading5Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading5";
        }

    }

    [TemplateTagName("Heading6")]
    public class UIHeading6Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading6";
        }

    }

}