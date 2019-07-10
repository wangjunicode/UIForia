using SVGX;
using UIForia.Attributes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using UnityEngine;

namespace UIForia.Elements {

    [TemplateTagName("Text")]
    public class UITextElement : UIElement, IStyleChangeHandler, IStylePropertiesWillChangeHandler, IStylePropertiesDidChangeHandler {

        internal string text;
        internal TextInfo textInfo;
        private bool shouldUpdateSpanStyle;
        private SVGXTextStyle spanStyle;
        public UITextElement(string text = "") {
            this.text = text ?? string.Empty;
            this.textInfo = new TextInfo(new TextSpan(string.Empty));
            this.flags = flags | UIElementFlags.TextElement
                               | UIElementFlags.BuiltIn
                               | UIElementFlags.Primitive;
        }

        internal TextInfo TextInfo => textInfo;

        public override void OnCreate() {
            if (children != null) {
                for (int i = 0; i < children.Count; i++) {
                    TextSpanElement childSpan = (TextSpanElement) children[i];
                    childSpan.Initialize(textInfo);
                }
            }

            textInfo.UpdateSpan(0, new TextSpan(text, style.GetTextStyle()));
        }

        public override void OnEnable() {
            textInfo.UpdateSpan(0, new TextSpan(text, style.GetTextStyle())); 
        }

        public string GetText() {
            return text;
        }

        public void SetText(string newText) {
            if (this.text == newText) {
                return;
            }
            this.text = newText;
            textInfo.UpdateSpan(0, text, style.GetTextStyle());
        }

        public void SetText(int spanIndex, string text) {
            textInfo.UpdateSpan(spanIndex, text);
        }

        public override string GetDisplayName() {
            return "Text";
        }

        public string GetSubstring(SelectionRange selectionRange) {
            if (!selectionRange.HasSelection) {
                return string.Empty;
            }

            int start = Mathf.Min(selectionRange.cursorIndex, selectionRange.selectIndex);
            int end = Mathf.Max(selectionRange.cursorIndex, selectionRange.selectIndex);

            char[] chars = new char[end - start];
//            int idx = 0;
//            for (int i = start; i < end; i++) {
//                chars[idx++] = textInfo.charInfos[i].character;
//            }

            return new string(chars);
        }

        // size, font, style, whitespace, transform, alignment
        public void OnStylePropertyChanged(in StyleProperty property) {
            switch (property.propertyId) {
                case StylePropertyId.TextFontSize:
                    shouldUpdateSpanStyle = true;
                    spanStyle.fontSize = property.AsInt;
                    break;
                case StylePropertyId.TextFontStyle:
                    shouldUpdateSpanStyle = true;
                    spanStyle.fontStyle = property.AsFontStyle;
                    break;
                case StylePropertyId.TextAlignment:
                    shouldUpdateSpanStyle = true;
                    spanStyle.alignment = property.AsTextAlignment;
                    break;
                case StylePropertyId.TextFontAsset:
                    shouldUpdateSpanStyle = true;
                    spanStyle.fontAsset = property.AsFont;
                    break;
                case StylePropertyId.TextTransform:
                    shouldUpdateSpanStyle = true;
                    spanStyle.textTransform = property.AsTextTransform;
                    break;
                // todo -- support this
                // case StylePropertyId.WhiteSpaceMode:
            }
        }

        public void OnStylePropertiesWillChange() {
            shouldUpdateSpanStyle = false;
        }

        public void OnStylePropertiesDidChange() {
            if (shouldUpdateSpanStyle) {
                textInfo.UpdateSpanStyle(0, style.GetTextStyle());
                shouldUpdateSpanStyle = false;
            }
        }

    }

}