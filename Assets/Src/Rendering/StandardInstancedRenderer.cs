using Src.Systems;
using Src.Util;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Src.Rendering {

    public class StandardInstancedRenderer : ElementRenderer {

        private readonly MaterialPropertyBlock block;
        private readonly LightList<float> m_Widths;
        private readonly LightList<float> m_Heights;
        
        private static readonly int s_WidthKey;
        private static readonly int s_HeightKey;
        private static readonly int s_ColorKey;
        private static readonly Mesh s_Mesh;
        private static readonly Material s_Material;
        
        static StandardInstancedRenderer() {
            s_Mesh = MeshUtil.CreateStandardUIMesh(new Size(100, 100), Color.white);
            s_Material = new Material(Resources.Load<Material>("Materials/UIForiaInstanced"));
            s_Material.enableInstancing = true;
            s_WidthKey = Shader.PropertyToID("_Widths");
            s_HeightKey = Shader.PropertyToID("_Heights");
            s_ColorKey = Shader.PropertyToID("_Color");
        }
        
        public StandardInstancedRenderer() {
            this.block = new MaterialPropertyBlock();
        }
        
        public override void Render(RenderData[] drawList, int start, int end, Vector3 origin, Camera camera) {
            float[] widths = new float[end - start];
            float[] heights = new float[end - start];
            Vector4[] colors  = new Vector4[end - start];
            Matrix4x4[] matrices = new Matrix4x4[end - start];
            int instanceId = 0;
            for (int i = start; i < end; i++) {
                RenderData data = drawList[i];
                
                matrices[instanceId] = Matrix4x4.TRS(data.renderPosition, Quaternion.identity, Vector3.one);
                widths[instanceId] = data.element.layoutResult.actualSize.width;
                heights[instanceId] = data.element.layoutResult.actualSize.height;
                colors[instanceId] = Color.red;
                instanceId++;
            }
            
            block.Clear();
            block.SetVectorArray(s_ColorKey, colors); 
            block.SetFloatArray(s_WidthKey, widths);
            block.SetFloatArray(s_HeightKey, heights);
            
            Graphics.DrawMeshInstanced(s_Mesh, 0, s_Material, matrices, end - start, block, ShadowCastingMode.Off, false, 0, camera, LightProbeUsage.Off);
        }

    }

}