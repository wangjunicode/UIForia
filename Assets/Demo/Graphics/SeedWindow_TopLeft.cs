using UIForia;
using UIForia.Rendering;
using UnityEngine;

namespace Demo.Graphics {

    [CustomPainter("SeedWindow:TopLeft")]
    public class SeedWindow_TopLeft : RenderBox {

        private Path2D path;

        public SeedWindow_TopLeft() {
            this.path = new Path2D();
        }

        public override void OnInitialize() {
            path.Clear();
        }

        public override void PaintBackground(RenderContext ctx) {
            path.Clear();
            path.SetTransform(element.layoutResult.matrix.ToMatrix4x4());
            path.BeginPath();
            path.SetFill(element.style.BackgroundColor);
            path.RoundedRect(0, 0, element.layoutResult.actualSize.width, element.layoutResult.actualSize.height, 0, 0, 0, 0);
            path.SetShadowColor(new Color32(96, 96, 96, 255));
            path.SetShadowOpacity(0.6f);
            path.SetShadowIntensity(20);
            path.Fill(FillMode.Shadow);
            path.BeginPath();
            path.Rect(0, 0, element.layoutResult.actualSize.width, element.layoutResult.actualSize.height);
            path.Fill();
            ctx.DrawPath(path);
        }

    }

}