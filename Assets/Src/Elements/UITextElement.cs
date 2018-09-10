namespace Src {

    public class UITextElement : UIElement {

        private string text;

        public delegate void TextChanged(UITextElement thisElement, string text);

        public event TextChanged onTextChanged;

        public UITextElement(string text = "") {
            this.text = text;
            flags = flags | UIElementFlags.TextElement
                          | UIElementFlags.Primitive;
        }

        public string GetText() {
            return text;
        }

        public void SetText(string newText) {
            if (this.text == newText) {
                return;
            }

            this.text = newText;
            onTextChanged?.Invoke(this, text);
        }

    }

}