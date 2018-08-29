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
            s_GUIContent.text = text;
            return Mathf.Ceil(s_GUIStyle.CalcSize(s_GUIContent).x);
        }

        public float CalcTextHeight(string text, UIStyleSet style, float width) {
            s_GUIStyle.font = style.font;
            s_GUIStyle.fontSize = style.fontSize;
            s_GUIStyle.fontStyle = style.fontStyle;
            s_GUIStyle.alignment = style.textAnchor;
//            s_GUIStyle.active = new GUIStyleState();
            s_GUIStyle.wordWrap = true;
            s_GUIContent.text = text;
            return Mathf.Ceil(s_GUIStyle.CalcHeight(s_GUIContent, width));
        }

    }

}