using Rendering;
using Src.Util;
using UnityEngine;

namespace Src.Systems {

    public class StandardRenderer : ElementRenderer {

        private static readonly int s_ColorKey;
        private static readonly int s_ClipRectKey;
        private static readonly Material s_BaseMaterial;

        static StandardRenderer() {
            s_BaseMaterial = Resources.Load<Material>("Materials/UIForia");
            s_ColorKey = Shader.PropertyToID("_Color");
            s_ClipRectKey = Shader.PropertyToID("_ClipRect");
        }

        public override void Render(RenderData[] drawList, int start, int end, Vector3 origin, Camera camera) {
            for (int i = start; i < end; i++) {
                RenderData data = drawList[i];
                UIElement element = data.element;

                if (data.material == null) {
                    IMaterialProvider materialProvider = element as IMaterialProvider;
                    if (materialProvider != null) {
                        data.material = materialProvider.GetMaterial();
                        if (data.material == null) {
                            continue;
                        }
                    }
                    else {
                        data.material = new Material(s_BaseMaterial);
                    }
                }

                ComputedStyle style = data.element.ComputedStyle;

                // todo -- not sure if setting these every frame is an issue or not

                data.material.SetColor(s_ColorKey, style.BackgroundColor);
                data.material.SetVector(s_ClipRectKey, data.clipVector);

                if (data.mesh == null || element.layoutResult.SizeChanged) {
                    IMeshProvider meshProvider = element as IMeshProvider;
                    data.mesh = meshProvider != null ? meshProvider.GetMesh() : MeshUtil.CreateStandardUIMesh(element.layoutResult.actualSize, Color.white);
                    if (data.mesh == null) {
                        continue;
                    }
                }

                Graphics.DrawMesh(data.mesh, origin + data.renderPosition, Quaternion.identity, data.material, 0, camera, 0, null, false, false, false);
            }
        }

    }

}