using SVGX;
using UIForia.Elements;
using UnityEngine;
using Vertigo;

namespace UIForia.Rendering {

    public interface ISVGXElementPainter {

        void Paint(UIElement element, VertigoContext ctx, in Matrix4x4 matrix);

    }

}