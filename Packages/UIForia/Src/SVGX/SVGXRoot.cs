using UnityEngine;

namespace SVGX {

    public class SVGXRoot : MonoBehaviour {

        public new Camera camera;
        private Mesh mesh;
        private Mesh lineMesh;

        private GFX gfx;

        private ImmediateRenderContext ctx;

        [Range(0, 360f)] public float rotation;
        public float dot;

        public void Start() {
            ctx = new ImmediateRenderContext();
            gfx = new GFX(camera);
        }

        public Texture2D texture;

        public Color tintColor;
        [Range(0, 1)] public float strokeOpacity;
        [Range(0, 1)] public float fillOpacity;

        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            ctx.Clear();

            ctx.MoveTo(-Screen.width * 0.5f, 0);
            ctx.LineTo(Screen.width, 0);
            ctx.SetStrokeColor(Color.blue);
            ctx.SetStrokeWidth(2f);
            ctx.SetStrokeOpacity(strokeOpacity);

            ctx.Stroke();
            
            ctx.BeginPath();
            
//            SVGXMatrix mat = SVGXMatrix.identity;
//            mat = mat.Translate(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
//            mat = mat.SkewX(45f);
//
//            ctx.SetTransform(mat);
            
            ctx.SetStrokeColor(Color.red);
            ctx.SetStrokeWidth(10f);
            ctx.SetStrokeOpacity(strokeOpacity);
            ctx.SetFillOpacity(fillOpacity);
            
            ctx.RoundedRect(new Rect(-150, -50, 300, 100), 20, 20, 20, 20);
                        
            ctx.Fill();
//            ctx.Stroke();


            gfx.Render(ctx);
        }

    }

}