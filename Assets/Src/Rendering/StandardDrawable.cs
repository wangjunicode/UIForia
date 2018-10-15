using System;
using Rendering;
using Src.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Systems {

    public class StandardDrawable : IDrawable {

        private static readonly VertexHelper s_VertexHelper = new VertexHelper();
        private static readonly ObjectPool<Mesh> s_MeshPool = new ObjectPool<Mesh>(null, (m) => m.Clear());

        public event Action<IDrawable> onMeshDirty;
        public event Action<IDrawable> onMaterialDirty;
        
        public readonly UIElement element;

        protected bool isMeshDirty;
        protected bool isMaterialDirty;
        protected Mesh mesh;
        private Material borderRadiusMaterial;

        public StandardDrawable(UIElement element) {
            this.element = element;
            this.isMeshDirty = true;
            this.isMaterialDirty = true;
        }

        public int Id => element.id;
        public bool IsGeometryDirty => isMeshDirty;

        public bool IsMaterialDirty => isMaterialDirty;

        public void OnAllocatedSizeChanged() {
            isMeshDirty = true;
            onMeshDirty?.Invoke(this);
        }

        public void OnStylePropertyChanged(StyleProperty property) {
            switch (property.propertyId) {
                case StylePropertyId.BackgroundColor:
                    SetVerticesDirty();
                    break;
                case StylePropertyId.BorderRadiusTopLeft:
                case StylePropertyId.BorderRadiusTopRight:
                case StylePropertyId.BorderRadiusBottomLeft:
                case StylePropertyId.BorderRadiusBottomRight:
                    SetVerticesDirty();
                    SetMaterialDirty();
                    break;
            }
        }

        public Mesh GetMesh() {
            if (!isMeshDirty) return mesh;
            if (mesh != null) {
                s_MeshPool.Release(mesh);
            }

            Color32 color = element.style.computedStyle.BackgroundColor;
            if (element.style.computedStyle.BackgroundImage.asset != null) {
                if (!ColorUtil.IsDefined(element.style.computedStyle.BackgroundColor) || element.style.computedStyle.HasBorderRadius) {
                    color = Color.white;
                }
            }
            
            mesh = MeshUtil.CreateStandardUIMesh(element.layoutResult.allocatedSize, color);

            return mesh;
        }

        public Texture GetMainTexture() {
            if (element.style.computedStyle.BackgroundImage.asset != null) {
                return element.style.computedStyle.BackgroundImage.asset;
            }

            return Texture2D.whiteTexture;
        }
        
        public Material GetMaterial() {

            ComputedStyle computed = element.style.computedStyle;
            if (computed.HasBorderRadius) {
                if (borderRadiusMaterial == null) {
                    borderRadiusMaterial = new Material(GetBorderRadiusMaterial());
                }

                float tl = computed.RareData.BorderRadiusTopLeft.IsDefined() ? computed.RareData.BorderRadiusTopLeft.value : 0f;
                float tr = computed.RareData.BorderRadiusTopRight.IsDefined() ? computed.RareData.BorderRadiusTopRight.value : 0f;
                float bl = computed.RareData.BorderRadiusBottomLeft.IsDefined() ? computed.RareData.BorderRadiusBottomLeft.value : 0f;
                float br = computed.RareData.BorderRadiusBottomRight.IsDefined() ? computed.RareData.BorderRadiusBottomRight.value : 0f;
                borderRadiusMaterial.SetVector("_Roundness", new Vector4(tl, tr, bl, br) * 100f); // roundness expects 0 - 50
                borderRadiusMaterial.EnableKeyword("FILL_SOLID_COLOR");
                borderRadiusMaterial.SetInt("_Shapes2D_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                borderRadiusMaterial.SetInt("_Shapes2D_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                borderRadiusMaterial.SetInt("_Shapes2D_SrcAlpha", (int) UnityEngine.Rendering.BlendMode.One);
                borderRadiusMaterial.SetInt("_Shapes2D_DstAlpha", (int) UnityEngine.Rendering.BlendMode.One);
                borderRadiusMaterial.SetInt("_PreMultiplyAlpha", 1);
                borderRadiusMaterial.SetFloat("_PixelSize", 1f);
                borderRadiusMaterial.SetFloat("_XScale", 100f);
                borderRadiusMaterial.SetFloat("_YScale", 100f);
                borderRadiusMaterial.SetFloat("_OutlineSize", computed.BorderTop.value);
                borderRadiusMaterial.SetColor("_OutlineColor", computed.BorderColor);
                borderRadiusMaterial.SetColor("_FillColor", computed.BackgroundColor);
                return borderRadiusMaterial;
            }
            
            return Graphic.defaultGraphicMaterial;
        }

        private void SetVerticesDirty() {
            isMeshDirty = true;
            onMeshDirty?.Invoke(this);
        }

        private void SetMaterialDirty() {
            isMaterialDirty = true;
            onMaterialDirty?.Invoke(this);
        }

        private static Material s_BorderRadiusMaterial;

        private static Material GetBorderRadiusMaterial() {
            if (s_BorderRadiusMaterial == null) {
                s_BorderRadiusMaterial = (Material) Resources.Load("MattUISimpleBorderRadius");
            }

            return s_BorderRadiusMaterial;
        }

    }

}