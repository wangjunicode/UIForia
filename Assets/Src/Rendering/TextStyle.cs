using UnityEngine;

namespace Rendering {

    public enum WhitespaceMode {

        Unset,
        Wrap,
        NoWrap,
        Preserve,
        PreserveWrap,
        PreserveLine // might be nice to have this be 'pre' but use whatever offset the first line is set on as a uniform space
    }
    
    public struct TextStyle {

        public Color color;
        public Font font;

        public int fontSize;

        public FontStyle fontStyle;
        public TextAnchor alignment;

        public WhitespaceMode whiteSpace;
        public HorizontalWrapMode horizontalOverflow;
        // todo -- handle these in styles
        public VerticalWrapMode verticalOverflow;

        public TextStyle(
            Color color,
            Font font,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            WhitespaceMode whiteSpace,
            HorizontalWrapMode horizontalOverflow,
            VerticalWrapMode verticalOverflow
        ) { // ----------------------------
            this.color = color;
            this.font = font;
            this.fontStyle = fontStyle;
            this.alignment = alignment;
            this.fontSize = fontSize;
            this.whiteSpace = whiteSpace;
            this.horizontalOverflow = horizontalOverflow;
            this.verticalOverflow = verticalOverflow;
        }

        public static TextStyle Unset => new TextStyle() {
            color = ColorUtil.UnsetColorValue,
            font = null,
            fontSize = IntUtil.UnsetValue,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.UpperLeft,
            whiteSpace =  WhitespaceMode.Unset,
            horizontalOverflow = HorizontalWrapMode.Overflow,
            verticalOverflow = VerticalWrapMode.Overflow
        };

    }

}