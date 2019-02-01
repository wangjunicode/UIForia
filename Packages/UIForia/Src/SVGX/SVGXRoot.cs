using UIForia.Extensions;
using UnityEngine;

namespace SVGX {
    
    public class SVGXRoot : MonoBehaviour {

        public new Camera camera;
        private Mesh mesh;
        private Mesh lineMesh;

        private GFX gfx;
        
        [Header("Fill Materials")]
        public Material simpleFillOpaque;
        public Material stencilFillCutoutOpaque;
        public Material stencilFillPaintOpaque;
        public Material stencilFillClearOpaque;

        [Header("Stroke Materials")] 
        public Material simpleStrokeOpaque;
        
        private ImmediateRenderContext ctx;

        [Range(0, 360f)] public float rotation;
        public float dot;
        
        public void Start() {
            ctx = new ImmediateRenderContext();
            gfx = new GFX(camera) {
                simpleFillOpaqueMaterial = simpleFillOpaque,
                stencilFillOpaqueCutoutMaterial = stencilFillCutoutOpaque,
                stencilFillOpaquePaintMaterial = stencilFillPaintOpaque,
                stencilFillOpaqueClearMaterial = stencilFillClearOpaque,
                simpleStrokeOpaqueMaterial = simpleStrokeOpaque
            };
        }

        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            ctx.Clear();
            
//            ctx.SetFillColor(Color.red);
//            ctx.Rect(100, 100, 100, 100);
//            ctx.Fill();
//            
//            ctx.BeginPath();
//            ctx.SetFillColor(Color.white);
//            ctx.Rect(100, 300, 100, 100);
//            ctx.Ellipse(100, 100, 50, 100);
//            ctx.Fill();
//  
//            ctx.BeginPath();
//            ctx.SetFillColor(Color.blue);
//            ctx.Rect(300, 100, 100, 100);
//            ctx.Fill();
//            ctx.Stroke();

            ctx.LineTo(100, 100);
            Vector2 v = new Vector2(200, 0);
            v = v.Rotate(new Vector2(0, 0), rotation);
            ctx.LineTo(v.x, v.y);
            ctx.Stroke();
            Vector2 toCurrent = new Vector2(100, 100) - new Vector2(0, 0);
            Vector2 toNext = v - new Vector2(100, 100);

            toCurrent = toCurrent.normalized;
            toNext = toNext.normalized;
            
            dot = Vector2.Dot(toCurrent, new Vector2(-toNext.y, toNext.x));
            
            gfx.Render(ctx);
            
        }

    }

}