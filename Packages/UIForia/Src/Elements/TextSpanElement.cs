using UIForia.Text;

namespace UIForia.Elements {

    public class TextSpanElement : UIElement {

        private string text;
        private int spanIndex;
        private TextInfo2 textInfo;
        
        public TextSpanElement(string text = "") {
            this.text = text ?? string.Empty;
            this.spanIndex = -1;
            flags = flags | UIElementFlags.TextElement
                          | UIElementFlags.BuiltIn
                          | UIElementFlags.Primitive;
        }

        internal void Initialize(TextInfo2 textInfo) {
            this.textInfo = textInfo;
            spanIndex = textInfo.spanList.Count;
            textInfo.AppendSpan(new TextSpan(text));
            if (children != null) {
                for (int i = 0; i < children.Count; i++) {
                    TextSpanElement childSpan = (TextSpanElement) children[i];
                    childSpan.Initialize(textInfo);
                }
            }
        }

        public void SetText(string text) {
            textInfo.UpdateSpan(spanIndex, new TextSpan(text));
        }
        
        public override string GetDisplayName() {
            return "TextSpan";
        }
        
        // maybe just mark for update, or have an option to disable a span 
        public override void OnEnable() {
            textInfo.UpdateSpan(spanIndex, new TextSpan(text));
        }
        
        // maybe just mark for update, or have an option to disable a span 
        public override void OnDisable() {
            textInfo.UpdateSpan(spanIndex, new TextSpan(string.Empty));
        }
        
    }

}