using Src.Elements;
using Src.Rendering;

namespace Src {

    public class UIGroupElement : UIContainerElement { }

    public class UIPanelElement : UIContainerElement { }

    public class UISectionElement : UIContainerElement { }

    public class UIDivElement : UIContainerElement { }

    public class UIHeaderElement : UIContainerElement { }

    public class UIFooterElement : UIContainerElement { }

    public class UILabelElement : UITextElement { }

    public class UIParagraphElement : UITextElement { }

    public class UIHeading1Element : UITextElement { }

    public class UIHeading2Element : UITextElement { }

    public class UIHeading3Element : UITextElement { }

    public class UIHeading4Element : UITextElement { }

    public class UIHeading5Element : UITextElement { }

    public class UIHeading6Element : UITextElement { }

    public static class DefaultIntrinsicStyles {

        public static readonly UIBaseStyleGroup LabelStyle = new UIBaseStyleGroup();
        public static readonly UIBaseStyleGroup ParagraphStyle = new UIBaseStyleGroup();
        public static readonly UIBaseStyleGroup Heading1Style = new UIBaseStyleGroup();
        public static readonly UIBaseStyleGroup Heading2Style = new UIBaseStyleGroup();
        public static readonly UIBaseStyleGroup Heading3Style = new UIBaseStyleGroup();
        public static readonly UIBaseStyleGroup Heading4Style = new UIBaseStyleGroup();
        public static readonly UIBaseStyleGroup Heading5Style = new UIBaseStyleGroup();
        public static readonly UIBaseStyleGroup Heading6Style = new UIBaseStyleGroup();

    }

}