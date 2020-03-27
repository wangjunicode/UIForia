using UIForia.Rendering;
using UnityEngine;

namespace Src.Systems {

    public class RootRenderBox : RenderBox {

        public override RenderBounds RenderBounds => new RenderBounds(0, 0, element.application.Width, element.application.Height);

        public override void PaintBackground(RenderContext ctx) { }

    }

}