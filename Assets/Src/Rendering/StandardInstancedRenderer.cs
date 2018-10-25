using Src.Systems;
using Src.Util;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Src.Rendering {

    public class StandardInstancedRenderer : ElementRenderer {

        private readonly MaterialPropertyBlock m_Block;
        private readonly LightList<float> m_Widths;
        private readonly LightList<float> m_Heights;
        private readonly LightList<Matrix4x4> m_Matrices;
        private readonly LightList<Vector4> m_Colors;
        private readonly LightList<Vector4> m_Roundness;
        
        private static readonly int s_WidthKey;
        private static readonly int s_HeightKey;
        private static readonly int s_ColorKey;
        private static readonly int s_RoundnessKey;
        private static readonly int s_BorderKey;
        private static readonly int s_OutlineKey;
        private static readonly int s_OuterBlurKey;
        private static readonly int s_InnerBlurKey;
        
        private static readonly int s_GradientTypeKey;
        private static readonly int s_GradientAxisKey;
        private static readonly int s_GradientStartKey;
        
        private static readonly int s_FillOffsetXKey;
        private static readonly int s_FillOffsetYKey;
        private static readonly int s_FillRotationKey;
        
        private static readonly Mesh s_Mesh;
        private static readonly Material s_Material;
        
        static StandardInstancedRenderer() {
            s_Mesh = MeshUtil.CreateStandardUIMesh(new Size(1, 1), Color.white);
            s_Material = new Material(Resources.Load<Material>("Materials/UIForiaInstanced"));
            s_Material.enableInstancing = true;
            s_WidthKey = Shader.PropertyToID("_Width");
            s_HeightKey = Shader.PropertyToID("_Height");
            s_ColorKey = Shader.PropertyToID("_Color");
            s_RoundnessKey = Shader.PropertyToID("_Roundness");
        }
        
        public StandardInstancedRenderer() {
            this.m_Block = new MaterialPropertyBlock();
            this.m_Widths = new LightList<float>(16);
            this.m_Heights = new LightList<float>(16);
            this.m_Colors = new LightList<Vector4>(16);
            this.m_Roundness = new LightList<Vector4>(16);
            this.m_Matrices = new LightList<Matrix4x4>(16);
        }
        
        public override void Render(RenderData[] drawList, int start, int end, Vector3 origin, Camera camera) {
         
            int count = end - start;
            m_Widths.EnsureCapacity(count);
            m_Heights.EnsureCapacity(count);
            m_Matrices.EnsureCapacity(count);
            m_Colors.EnsureCapacity(count);
            m_Roundness.EnsureCapacity(count);
            
            int instanceId = 0;
            for (int i = start; i < end; i++) {
                RenderData data = drawList[i];
                
                m_Matrices[instanceId] = Matrix4x4.TRS(origin + data.renderPosition, Quaternion.identity, Vector3.one);
                m_Widths[instanceId] = data.element.layoutResult.actualSize.width;
                m_Heights[instanceId] = data.element.layoutResult.actualSize.height;
                m_Colors[instanceId] = Color.red;//data.element.ComputedStyle.BackgroundColor;
                m_Roundness[instanceId] = data.element.ComputedStyle.ResolvedBorderRadius;
                instanceId++;
            }
            
            m_Block.Clear();
            m_Block.SetVectorArray(s_ColorKey, m_Colors.List); 
            m_Block.SetFloatArray(s_WidthKey, m_Widths.List);
            m_Block.SetFloatArray(s_HeightKey, m_Heights.List);
            m_Block.SetVectorArray(s_RoundnessKey, m_Roundness.List);
            Graphics.DrawMeshInstanced(s_Mesh, 0, s_Material, m_Matrices.List, count, m_Block, ShadowCastingMode.Off, false, 0, camera, LightProbeUsage.Off);
        }

    }

}