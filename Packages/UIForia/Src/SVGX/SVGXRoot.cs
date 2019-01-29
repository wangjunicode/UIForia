using System.Collections.Generic;
using UnityEngine;

namespace SVGX {

    public class SVGXClipElement {

        public SVGXClipElement parentClipElement;
        public List<SVGXRenderElement> shapes;

    }

    public struct SVGXRenderElement {

        public List<Vector2> strokeAA;
        public List<Vector2> stroke;
        public List<Vector2> fill;
        public SVGXClipElement clip;
        public SVGXRenderElementType type;
        public SVGXStyle style;
        public SVGXMatrix transform;

    }

    public class SVGXRoot : MonoBehaviour {

        public Material paint;
        public Material stencil;
        public new Camera camera;
        private Mesh mesh;
        private Mesh lineMesh;

        private SVGXRenderSystem renderSystem;

        public Material clip;
        public Material stencilClear;
        public Material strokeMaterial;
        public Material fillMaterial;

        private ImmediateRenderContext ctx;

        public void Start() {
            renderSystem = new SVGXRenderSystem(strokeMaterial, fillMaterial, null, null, null);
            ctx = new ImmediateRenderContext();
        }

        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            ctx.Clear();
            ctx.Rect(100, 100, 100, 100);
            ctx.Fill();
            
            renderSystem.Render(camera, ctx);

        }

    }

}