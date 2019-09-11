using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace Demo {

    public class DateIndicator : UIContainerElement, IElementBackgroundPainter {

        public Path2D path = new Path2D();

        public override void OnCreate() {
            style.SetPainter("self", StyleState.Normal);    
        }
        
        public void PaintBackground(RenderContext ctx) {
            path.SetTransform(layoutResult.matrix.ToMatrix4x4());
            path.Clear();
            path.BeginPath();
            path.Circle(0, 0, Mathf.Min(layoutResult.actualSize.width, layoutResult.actualSize.height) * 0.5f);
            path.SetFill(Color.black);
            path.Fill();
            path.BeginPath();
            
            path.SetFill(Color.green);
            path.Sector(0, 0, 50, 0f, 90f, 15f);
            path.Fill();
            
            ctx.DrawPath(path);
        }

    }

}