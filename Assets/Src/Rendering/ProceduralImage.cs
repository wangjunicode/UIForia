using UnityEngine;
using UnityEngine.UI;

namespace Rendering {

    [AddComponentMenu("UI/Procedural Image")]
    public class ProceduralImage : Image {

        [SerializeField] private float borderWidth;
        private Material materialInstance;
        [SerializeField] private float falloffDistance = 1;

        private static Sprite instance;


        public float BorderWidth {
            get { return borderWidth; }
            set {
                borderWidth = value;
                this.SetMaterialDirty();
            }
        }

        public float FalloffDistance {
            get { return falloffDistance; }
            set {
                falloffDistance = value;
                this.SetMaterialDirty();
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            this.Init();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init() {
            if (instance == null) {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                instance = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
            }

            if (materialInstance == null) {
                materialInstance = new Material(Shader.Find("UI/Procedural UI Image"));
            }

            this.material = materialInstance;
        }

        public void Update() {
            this.UpdateMaterial();
        }

        /// <summary>
        /// Prevents radius to get bigger than rect size
        /// </summary>
        /// <returns>The fixed radius.</returns>
        /// <param name="vec">border-radius as Vector4 (starting upper-left, clockwise)</param>
        private Vector4 FixRadius(Vector4 vec) {
            Rect r = this.rectTransform.rect;
            vec = new Vector4(Mathf.Max(vec.x, 0), Mathf.Max(vec.y, 0), Mathf.Max(vec.z, 0), Mathf.Max(vec.w, 0));
            //float maxRadiusSums = Mathf.Max (vec.x,vec.z) + Mathf.Max (vec.y,vec.w);
            float scaleFactor = Mathf.Min(r.width / (vec.x + vec.y), r.width / (vec.z + vec.w),
                r.height / (vec.x + vec.w), r.height / (vec.z + vec.y), 1);
            return vec * scaleFactor;
        }

        protected override void OnPopulateMesh(VertexHelper toFill) {
            //note: Sliced and Tiled have no effect to this currently.

            if (overrideSprite == null) {
                base.OnPopulateMesh(toFill);
                return;
            }

            switch (type) {
                case Type.Simple:
                    GenerateSimpleSprite(toFill);
                    break;
                case Type.Sliced:
                    GenerateSimpleSprite(toFill);
                    break;
                case Type.Tiled:
                    GenerateSimpleSprite(toFill);
                    break;
                case Type.Filled:
                    base.OnPopulateMesh(toFill);
                    break;
            }
        }
#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();
            OnEnable();
        }
#endif
        /// <summary>
        /// Generates the Verticies needed.
        /// </summary>
        /// <param name="vh">vertex helper</param>
        void GenerateSimpleSprite(VertexHelper vh) {
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
            var uv = new Vector4(0, 0, 1, 1);
            float aa = falloffDistance / 2f;
            var color32 = this.color;
            vh.Clear();
            vh.AddVert(new Vector3(v.x - aa, v.y - aa), color32, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(v.x - aa, v.w + aa), color32, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(v.z + aa, v.w + aa), color32, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(v.z + aa, v.y - aa), color32, new Vector2(uv.z, uv.y));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        /// <summary>
        /// Sets the material values of shader.
        /// Implementation of IMaterialModifier
        /// </summary>
        public override Material GetModifiedMaterial(Material baseMaterial) {
            Rect rect = this.GetComponent<RectTransform>().rect;
            //get world-space corners of rect
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float pixelSize = Vector3.Distance(corners[1], corners[2]) / rect.width;
            pixelSize = pixelSize / falloffDistance;

            Vector4 radius = FixRadius(CalculateRadius(rect));

            return SetMaterialValues(new ProceduralImageMaterialInfo(
                    rect.width + falloffDistance,
                    rect.height + falloffDistance,
                    Mathf.Max(pixelSize, 0),
                    radius,
                    Mathf.Max(borderWidth, 0)),
                base.GetModifiedMaterial(baseMaterial)
            );
        }

        public enum EdgeType {

            Free,
            Round,
            Uniform

        }

        public EdgeType edgeType;
        public Vector4 edges;

        private Vector4 CalculateRadius(Rect rect) {
            switch (edgeType) {
                case EdgeType.Free:
                    return edges;

                case EdgeType.Round:
                    float r = Mathf.Min(rect.width, rect.height) * 0.5f;
                    return new Vector4(r, r, r, r);

                case EdgeType.Uniform:
                    return new Vector4(edges.x, edges.x, edges.x, edges.x);
            }

            return Vector4.zero;
        }

        private static Material SetMaterialValues(ProceduralImageMaterialInfo info, Material baseMaterial) {
            if (baseMaterial == null) {
                throw new System.ArgumentNullException(nameof(baseMaterial));
            }

            if (baseMaterial.shader.name != "UI/Procedural UI Image") {
                Debug.LogWarning(
                    "Parameter 'baseMaterial' does not use shader 'UI/Procedural UI Image'. Method returns baseMaterial.");
                return baseMaterial;
            }

            baseMaterial.SetFloat("_Width", info.width);
            baseMaterial.SetFloat("_Height", info.height);
            baseMaterial.SetFloat("_PixelWorldScale", info.pixelWorldScale);
            baseMaterial.SetVector("_Radius", info.radius);
            baseMaterial.SetFloat("_LineWeight", info.borderWidth);
            return baseMaterial;
        }

        private struct ProceduralImageMaterialInfo {

            public readonly float width;
            public readonly float height;
            public readonly float pixelWorldScale;
            public readonly Vector4 radius;
            public readonly float borderWidth;

            public ProceduralImageMaterialInfo(float width,
                float height,
                float pixelWorldScale,
                Vector4 radius,
                float borderWidth) {
                this.width = width;
                this.height = height;
                this.pixelWorldScale = pixelWorldScale;
                this.radius = radius;
                this.borderWidth = borderWidth;
            }

            public override string ToString() {
                return $"width:{width},height:{height},pws:{pixelWorldScale},radius:{radius},bw:{borderWidth}";
            }

        }
    }   

}