using UIForia.Attributes;

namespace UIForia.Elements {

    [TemplateTagName("TextSpan")]
    public class TextSpanElement : UITextElement {

        public TextSpanElement(string rawText) : base(rawText) { }

        public override string GetDisplayName() {
            return "TextSpan";
        }

    }

}