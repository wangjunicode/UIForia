using System;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Rendering.ElementRendering {

    public class StandardTextRenderer : ElementRenderer {

        private static int s_FaceColorKey;
        private readonly MaterialPropertyBlock m_PropertyBlock;

        public StandardTextRenderer() {
            s_FaceColorKey = Shader.PropertyToID("_FaceColor");
            m_PropertyBlock = new MaterialPropertyBlock();
        }
        
        public override void Render(RenderData[] drawList, int start, int end, Vector3 origin, Camera camera) {
            
            for (int i = start; i < end; i++) {
                RenderData data = drawList[i];
                UITextElement textElement = (UITextElement) data.element;
                Mesh mesh = textElement.GetMesh();
                Material material = textElement.GetMaterial();
                m_PropertyBlock.SetVector(s_FaceColorKey, textElement.ComputedStyle.TextColor);
                Quaternion rotation = Quaternion.AngleAxis(data.element.ComputedStyle.TransformRotation, Vector3.forward);
                material.color = Color.white;
                
                Graphics.DrawMesh(mesh, origin + data.renderPosition, rotation, material, 0, camera, 0, m_PropertyBlock, false, false, false);
            }
            
        }

    }

}