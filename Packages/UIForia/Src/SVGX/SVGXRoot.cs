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

        public float strokeWidth = 5;
        public Color tintColor;
        [Range(0, 1)] public float strokeOpacity;
        [Range(0, 1)] public float fillOpacity;

        public ColorStop[] gradientStops;

        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            ctx.Clear();

//            SVGXGradient gradient = new SVGXLinearGradient(GradientDirection.Vertical, gradientStops);

//            ctx.Circle(0, 0, 100);
//            ctx.PushClip();

//            ctx.SetFill(texture);
//            ctx.FillRect(new Rect(-50, -50, 300, 300));

            ctx.BeginPath();
            ctx.SetStrokeColor(tintColor);
            ctx.SetStrokeWidth(strokeWidth);
            ctx.SetStrokeOpacity(strokeOpacity);
//            ctx.Circle(0, 0, 300);
            ctx.LineTo(400, 400);
            ctx.LineTo(400, 0);
//            ctx.Rect(0, 0, 300, 300);
            ctx.Stroke();

//            ctx.SetFill(new Color(1, 0, 0, 0.5f));
//
//            ctx.FillRect(0, 0, 100, 100);
//
//            ctx.SetFill(new Color(1, 1, 0, 0.5f));
//            ctx.FillRect(25, 25, 100, 100);

            gfx.Render(ctx);
        }

    }

}