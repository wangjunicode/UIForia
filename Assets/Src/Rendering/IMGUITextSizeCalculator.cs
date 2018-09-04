using UnityEngine;

namespace Rendering {

    public class IMGUITextSizeCalculator : ITextSizeCalculator {

        private static readonly GUIStyle s_GUIStyle = new GUIStyle();
        private static readonly GUIContent s_GUIContent = new GUIContent();

        public float CalcTextWidth(string text, UIStyleSet style) {
            s_GUIStyle.font = style.font;
            s_GUIStyle.fontSize = style.fontSize;
            s_GUIStyle.fontStyle = style.fontStyle;
            s_GUIStyle.alignment = style.textAnchor;
            s_GUIStyle.wordWrap = true;
            s_GUIContent.text = text;
            return Mathf.Ceil(s_GUIStyle.CalcSize(s_GUIContent).x + 1);
        }

        public float CalcTextHeight(string text, UIStyleSet style, float width) {
            s_GUIStyle.font = style.font;
            s_GUIStyle.fontSize = style.fontSize;
            s_GUIStyle.fontStyle = style.fontStyle;
            s_GUIStyle.alignment = style.textAnchor;
            s_GUIStyle.wordWrap = true;
            s_GUIContent.text = text;
            return Mathf.Ceil(s_GUIStyle.CalcHeight(s_GUIContent, width));
        }

        public static float S_CalcTextWidth(string text, UIStyleSet style) {
            s_GUIStyle.font = style.font;
            s_GUIStyle.fontSize = style.fontSize;
            s_GUIStyle.fontStyle = style.fontStyle;
            s_GUIStyle.alignment = style.textAnchor;
            s_GUIStyle.padding = new RectOffset();
            s_GUIStyle.margin = new RectOffset();
            s_GUIStyle.wordWrap = true;
            s_GUIStyle.stretchWidth = false;
            s_GUIStyle.richText = false;
            s_GUIContent.text = text;
            s_GUIStyle.imagePosition = ImagePosition.TextOnly;
            return Mathf.Ceil(s_GUIStyle.CalcSize(s_GUIContent).x + 1);
        }

        public static float S_CalcTextHeight(string text, UIStyleSet style, float width) {
            s_GUIStyle.font = style.font;
            s_GUIStyle.fontSize = style.fontSize;
            s_GUIStyle.fontStyle = style.fontStyle;
            s_GUIStyle.alignment = style.textAnchor;
            s_GUIStyle.wordWrap = true;
            s_GUIStyle.richText = false;
            s_GUIStyle.imagePosition = ImagePosition.TextOnly;
            s_GUIContent.text = text;
            return Mathf.Ceil(s_GUIStyle.CalcHeight(s_GUIContent, width));
        }
    }

}