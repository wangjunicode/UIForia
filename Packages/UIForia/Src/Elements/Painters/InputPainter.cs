using SVGX;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Elements.Painters {
    
    public class InputPainter : ISVGXElementPainter {

        public void Paint(UIElement element, ImmediateRenderContext ctx, SVGXMatrix matrix) {
            ctx.SetTransform(matrix);
            LayoutResult layoutResult = element.layoutResult;
            ctx.Rect(0, 0, layoutResult.actualSize.width, layoutResult.actualSize.height);
            ctx.SetFill(Color.red);
            ctx.Fill();
        }

    }

}