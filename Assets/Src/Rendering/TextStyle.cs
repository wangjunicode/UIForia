using UnityEngine;

namespace Rendering {

    public class TextStyle {

        public Color? color;
        public Font font;
        public int fontSize = -1;
        public FontStyle? fontStyle;
        public TextAnchor? alignment;
        public HorizontalWrapMode horizontalOverflow;
        public VerticalWrapMode verticalOverflow;

    }

}