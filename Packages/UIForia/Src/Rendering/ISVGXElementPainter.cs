using SVGX;
using UIForia.Elements;

namespace UIForia.Rendering {

    public interface ISVGXElementPainter {

        void Paint(UIElement element, ImmediateRenderContext ctx, SVGXMatrix matrix);

    }

}