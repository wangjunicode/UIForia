using UnityEngine;

namespace Rendering {

    public enum WhitespaceMode {

        Wrap,
        NoWrap,
        Preserve,
        PreserveWrap,
        PreserveLine

    }
    
    public struct TextStyle {

        public Color color;
        public Font font;

        public int fontSize;

        public FontStyle fontStyle;
        public TextAnchor alignment;

        public HorizontalWrapMode horizontalOverflow;
        // todo -- handle these in styles
        public VerticalWrapMode verticalOverflow;

        public TextStyle(
            Color color,
            Font font,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            HorizontalWrapMode horizontalOverflow,
            VerticalWrapMode verticalOverflow
        ) { // ----------------------------
            this.color = color;
            this.font = font;
            this.fontStyle = fontStyle;
            this.alignment = alignment;
            this.fontSize = fontSize;
            this.horizontalOverflow = horizontalOverflow;
            this.verticalOverflow = verticalOverflow;
        }

        public static TextStyle Unset => new TextStyle() {
            color = ColorUtil.UnsetColorValue,
            font = null,
            fontSize = IntUtil.UnsetValue,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            horizontalOverflow = HorizontalWrapMode.Overflow,
            verticalOverflow = VerticalWrapMode.Overflow
        };

    }

}