using SVGX;
using UnityEngine;
using Vertigo;

namespace UIForia.Rendering {

    public interface ISVGXPaintable {

        void Paint(VertigoContext ctx, in Matrix4x4 matrix);
        void Paint(ImmediateRenderContext ctx, in SVGXMatrix matrix);

    }

}