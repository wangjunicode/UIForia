using SVGX;
using UIForia.Rendering;
using UnityEngine;
using Vertigo;

namespace UIForia.Elements {

    public enum ArrowRotation {
        Left, Right, Up, Down   
    }

    /// <summary>
    /// Renders an arrowhead that can be used as an arrow-like border or a chevron. Uses BackgroundColor as
    /// arrow color and respects margins.
    /// </summary>
    public class Arrow : UIContainerElement, ISVGXPaintable {

        /// <summary>
        /// Sets the direction in which the arrow points. Defaults to left.
        /// </summary>
        public ArrowRotation Rotation;

        public override void OnCreate() {
            style.SetPainter("self", StyleState.Normal);
        }

        public void Paint(VertigoContext ctx, in Matrix4x4 matrix) {
            throw new System.NotImplementedException();
        }

        public void Paint(ImmediateRenderContext ctx, in SVGXMatrix matrix) {
            ctx.SetTransform(matrix);

            ctx.BeginPath();

            float xMax = (layoutResult.ActualWidth - layoutResult.margin.left - layoutResult.margin.right);
            float yMax = (layoutResult.ActualHeight - layoutResult.margin.top - layoutResult.margin.bottom);
            switch (Rotation) {
                case ArrowRotation.Down: 
                    ctx.MoveTo(0, 0);
                    ctx.LineTo(xMax / 2, yMax);
                    ctx.LineTo(layoutResult.ActualWidth, 0);
                    break;
                case ArrowRotation.Up:
                    ctx.MoveTo(0, yMax);
                    ctx.LineTo(xMax / 2, 0);
                    ctx.LineTo(layoutResult.ActualWidth, yMax);
                    break;
                case ArrowRotation.Right: 
                    ctx.MoveTo(0, 2);
                    ctx.LineTo(xMax, yMax / 2);
                    ctx.LineTo(0, layoutResult.ActualHeight);
                    break;
                case ArrowRotation.Left:
                    ctx.MoveTo(xMax, 2);
                    ctx.LineTo(0, yMax / 2);
                    ctx.LineTo(xMax, layoutResult.ActualHeight);
                    break;
            }

            ctx.SetStrokeWidth(2f);
            ctx.SetStroke(style.BackgroundColor.r == -1 ? Color.grey : style.BackgroundColor);
            ctx.Stroke();
        }
    }
}
