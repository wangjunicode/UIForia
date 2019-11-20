using UIForia;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.DebugTools {

    [CustomPainter("ClipBounds")]
    public class PaintClipBounds : StandardRenderBox {

        public override void OnInitialize() {
            base.OnInitialize();
            hasForeground = true;
        }

        public override void PaintForeground(RenderContext ctx) {
            LightList<ClipData> clippers = element.Application.LayoutSystem.GetLayoutRunner(null).clipperList;
            Path2D path = new Path2D();
            ClipData clipper = clippers[2];
            if (clipper == null) return;

            Color color = Color.red;
            if (clipper.ContainsPoint(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) {
                color = Color.green;
            }

            path.BeginPath();
            path.MoveTo(clipper.orientedBounds.p0);
            path.LineTo(clipper.orientedBounds.p1);
            path.LineTo(clipper.orientedBounds.p2);
            path.LineTo(clipper.orientedBounds.p3);
            path.ClosePath();
            path.SetStrokeWidth(3f);
            path.SetStroke(color);
            path.Stroke();
            ctx.DrawPath(path);
        }

    }

}