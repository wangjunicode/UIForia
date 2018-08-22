using UnityEngine;

namespace Src {

    public class UITextElement : UIElement {

        private string text;
        private Vector2 size;
        
        public delegate void TextChanged(UITextElement thisElement, string text);
        public delegate void DimensionsChanged(UITextElement thisElement, Vector2 size);

        public event TextChanged onTextChanged;
        public event DimensionsChanged onSizeChanged;

        public UITextElement(string text = "") {
            this.text = text;
            flags |= UIElementFlags.TextElement;
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

        public Vector2 Size => size;
        
        public void SetDimensions(Vector2 size) {
            if (this.size == size) {
                return;
            }

            this.size = size;
            onSizeChanged?.Invoke(this, size);
        }

    }

}