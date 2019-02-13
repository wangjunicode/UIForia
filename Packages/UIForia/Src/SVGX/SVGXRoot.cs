using UnityEngine;

namespace SVGX {

    public class SVGXRoot : MonoBehaviour {

        public new Camera camera;
        private Mesh mesh;
        private Mesh lineMesh;

        private GFX gfx;

        [Header("Fill Materials")] public Material simpleFillOpaque;
        public Material stencilFillCutoutOpaque;
        public Material stencilFillPaintOpaque;
        public Material stencilFillClearOpaque;

        [Header("Stroke Materials")] public Material simpleStrokeOpaque;

        private ImmediateRenderContext ctx;

        [Range(0, 360f)] public float rotation;
        public float dot;

        public Texture2D texture;
        
        public void Start() {
            ctx = new ImmediateRenderContext();
            gfx = new GFX(camera);
        }

        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            ctx.Clear();
            
//            ctx.SetFill(Color.blue);
//            ctx.Circle(0, 0, 100, 100);
//            ctx.Fill();

            ctx.BeginPath();
            
            ctx.Circle(0, 0, 50);
            
            ctx.PushClip();
            
            ctx.SetFill(Color.red);
            ctx.FillRect(new Rect(0, 0, 100, 100));
            
            ctx.SetFill(Color.green);
            ctx.FillRect(new Rect(50, 50, 100, 100));
            
            ctx.PopClip();
            
            ctx.SetFill(Color.blue);
            ctx.FillRect(new Rect(75, 75, 200, 200));
            
            gfx.Render(ctx);
            
//
//
//            Vector2 v = new Vector2(200, 0);
//            v = v.Rotate(new Vector2(100, 100), rotation);
//
//            ctx.MoveTo(20, 20);
//            ctx.LineTo(100, 20);
//            ctx.LineTo(v.x, v.y);
//            ctx.Stroke();
//
//
//            gfx.Render(ctx);
//            
//            Vector2 start = new Vector2(20, 20);
//            Vector2 mid = new Vector2(100, 20);
//            Vector2 toV = (v - mid).normalized;
//            
//            gfx.DrawDebugLine(mid, v + toV * 100, Color.red, 2f);
//            gfx.DrawDebugLine(new Vector2(-120, 20), new Vector2(400, 20), Color.blue, 2f);
//
////            Vector2 toCurrent = new Vector2(100, 20) - v;
////            Vector2 
////            toCurrent = toCurrent.normalized;
////            toNext = toNext.normalized;
//            dot = Vector2.Dot(toV, (mid - start).normalized);
        }

    }

}