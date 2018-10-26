using Rendering;
using Src.Util;
using UnityEngine;

namespace Src.Systems {

    public class StandardRenderer : ElementRenderer {

        private static readonly int s_ColorKey;
        private static readonly int s_ClipRectKey;
        private static readonly int s_SizeKey;
        private static readonly int s_BorderSizeKey;
        private static readonly int s_BorderRadiusKey;
        private static readonly int s_BorderColorKey;
        private static readonly Material s_BaseMaterial;

        static StandardRenderer() {
            s_BaseMaterial = Resources.Load<Material>("Materials/UIForia");
            s_ColorKey = Shader.PropertyToID("_Color");
            s_ClipRectKey = Shader.PropertyToID("_ClipRect");
            s_BorderColorKey = Shader.PropertyToID("_BorderColor");
            s_BorderSizeKey = Shader.PropertyToID("_BorderSize");
            s_BorderRadiusKey = Shader.PropertyToID("_BorderRadius");
            s_SizeKey = Shader.PropertyToID("_Size");
        }

        public override void Render(RenderData[] drawList, int start, int end, Vector3 origin, Camera camera) {
            for (int i = start; i < end; i++) {
                RenderData data = drawList[i];
                UIElement element = data.element;

                if (data.isMaterialProvider) {
                    data.material = ((IMaterialProvider) element).GetMaterial();
                }
                else if (data.material == null) {
                    data.material = new Material(s_BaseMaterial);
                }

                if (data.material == null) {
                    continue;
                }

                if (data.isMeshProvider) {
                    data.mesh = ((IMeshProvider) element).GetMesh();
                }
                else if (data.mesh == null) {
                    data.mesh = MeshUtil.CreateStandardUIMesh(element.layoutResult.actualSize, Color.white);
                }

                if (data.mesh == null) {
                    continue;
                }
                
                ComputedStyle style = data.element.ComputedStyle;
                Size size = element.layoutResult.actualSize;
                Material material = data.material;
                material.SetColor(s_ColorKey, style.BackgroundColor);
                material.SetVector(s_ClipRectKey, data.clipVector);
                material.SetVector(s_BorderSizeKey, style.ResolvedBorder);
                material.SetVector(s_BorderRadiusKey, style.ResolvedBorderRadius);
                material.SetVector(s_BorderColorKey, style.BorderColor);
                material.SetVector(s_SizeKey, new Vector4(size.width, size.height, 0, 0));
                Graphics.DrawMesh(data.mesh, origin + data.renderPosition, Quaternion.identity, data.material, 0, camera, 0, null, false, false, false);
            }
        }

    }

}