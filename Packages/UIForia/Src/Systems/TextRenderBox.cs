using UnityEngine;

namespace UIForia.Rendering {

    public class TextRenderBox : RenderBox {

        public override Rect RenderBounds => new Rect(
            0, 0,
            element.layoutResult.actualSize.width,
            element.layoutResult.actualSize.height
        );

        public TextRenderBox() {
            this.uniqueId = "UIForia::TextRenderBox";
        }

        public override void PaintBackground(RenderContext ctx) { }

    }

}