using UIForia;
using UIForia.Rendering;
using UnityEngine;

namespace SeedLib {

    [CustomPainter("SeedLib::Slider")]
    public class SeedLibSliderPainter : StandardRenderBox {

        private Path2D path2D;

        public override void OnInitialize() {
            path2D = new Path2D();
        }

        public override void PaintBackground(RenderContext ctx) {
            base.PaintBackground(ctx);
            Slider slider = element as Slider;
            float stepSize = slider.stepSize;
            if (!slider.showStepTicks || stepSize <= 0) {
                return;
            }

            float rangeStart = slider.rangeStart;
            float rangeEnd = slider.rangeEnd;

            float width = element.layoutResult.ContentAreaWidth;

            int steps = (int) ((rangeEnd - rangeStart) / stepSize);

            path2D.Clear();

            path2D.SetTransform(element.layoutResult.matrix.ToMatrix4x4());
            path2D.SetFill(Color.black);
            steps++;
            float inset = element.layoutResult.HorizontalPaddingBorderStart - 1;
            float widthStep = (width - 2) / (steps - 1);
            for (int i = 0; i < steps; i++) {
                path2D.BeginPath();
                path2D.Circle(inset + (i * widthStep), 24, 2);
                path2D.Fill();
            }

            ctx.DrawPath(path2D);

        }

    }

}