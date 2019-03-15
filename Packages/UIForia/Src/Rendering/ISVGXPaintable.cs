using SVGX;

namespace UIForia.Rendering {

    public interface ISVGXPaintable {

        void Paint(ImmediateRenderContext ctx, in SVGXMatrix matrix);
        
    }

}