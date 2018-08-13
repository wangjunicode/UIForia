using System.Collections.Generic;
using Src.Layout;
using UnityEngine;

namespace Rendering {

    public class UIStyle {

        private List<UIElement> elements;
        public UILayout layout;
        public ContentBox contentBox;
        public LayoutParameters layoutParameters;
        public PaintDesc paint;
        public TextStyle textStyle = new TextStyle();

        public const int UnsetIntValue = int.MaxValue;
        public const float UnsetFloatValue = float.MaxValue;
        public static readonly Color UnsetColorValue = new Color(-1f, -1f, -1f, -1f);

        public UIStyle() {
            layout = new UILayout_Auto();
            contentBox = new ContentBox();
            paint = new PaintDesc();
            layoutParameters = new LayoutParameters();
        }
       
        public bool RequiresRendering() {
            return paint.backgroundImage != null
                   || paint.backgroundColor != UnsetColorValue
                   || paint.borderStyle != BorderStyle.Unset
                   || paint.borderColor != UnsetColorValue;
        }

    }

}