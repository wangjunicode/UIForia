using UIForia.Elements;
using UIForia.Parsing.StyleParser;
using UIForia.Rendering;

namespace UIForia {

    public class UIGroupElement : UIContainerElement {

        public override string GetDisplayName() {
            return "Group";
        }

    }

    public class UIPanelElement : UIContainerElement {

        public override string GetDisplayName() {
            return "Panel";
        }

    }

    public class UISectionElement : UIContainerElement {

        public override string GetDisplayName() {
            return "Section";
        }

    }

    public class UIDivElement : UIContainerElement {

        public override string GetDisplayName() {
            return "Div";
        }

    }

    public class UIHeaderElement : UIContainerElement {

        public override string GetDisplayName() {
            return "Header";
        }

    }

    public class UIFooterElement : UIContainerElement {

        public override string GetDisplayName() {
            return "Footer";
        }

    }

    public class UILabelElement : UITextElement {

        public override string GetDisplayName() {
            return "Label";
        }

    }

    public class UIParagraphElement : UITextElement {

        public override string GetDisplayName() {
            return "Paragraph";
        }

    }

    public class UIHeading1Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading1";
        }

    }

    public class UIHeading2Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading2";
        }

    }

    public class UIHeading3Element : UITextElement {

        public override void OnReady() {
//            style.AddImplicitStyleGroup(StyleParser.GetImplicitStyleGroup(""));
        }

        public override string GetDisplayName() {
            return "Heading3";
        }

    }

    public class UIHeading4Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading4";
        }

    }

    public class UIHeading5Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading5";
        }

    }

    public class UIHeading6Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading6";
        }

    }

    public static class DefaultIntrinsicStyles {

        public static readonly UIStyleGroup LabelStyle = new UIStyleGroup();
        public static readonly UIStyleGroup ParagraphStyle = new UIStyleGroup();
        public static readonly UIStyleGroup Heading1Style = new UIStyleGroup();
        public static readonly UIStyleGroup Heading2Style = new UIStyleGroup();
        public static readonly UIStyleGroup Heading3Style = new UIStyleGroup();
        public static readonly UIStyleGroup Heading4Style = new UIStyleGroup();
        public static readonly UIStyleGroup Heading5Style = new UIStyleGroup();
        public static readonly UIStyleGroup Heading6Style = new UIStyleGroup();

    }

}