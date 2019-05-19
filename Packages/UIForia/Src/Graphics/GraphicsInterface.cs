using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Src.Systems;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Vertigo {

    [UsedImplicitly]
    public class UIForiaGraphicsInterface : GraphicsInterface {

        // todo -- see if there is a face but nice way to use GraphicsContext directly & avoid the extra geometry copy
        private Geometry geometry = new Geometry();
        private UVTransform uvTransform;
        public Color fillColor;
        public Color strokeColor;
        
        public UIForiaGraphicsInterface(GraphicsContext gfx) : base(gfx) { }

        public void Save() { }

        public void Restore() { }

        public override void BeginFrame() {
            geometry.ClearGeometryData();
        }

        public override void SetTextureProperty(int key, Texture texture) {
            if (ReferenceEquals(texture, null)) {
                return;
            }
            gfx.SetTextureProperty(key, texture);
        }
        
        public void EnableUVTransform(in UVTransform uvTransform) {
            this.uvTransform = this.uvTransform;
        }

        public void DisableUVTransform() {
            uvTransform.enabled = false;
        }

        public void FillRect(float x, float y, float width, float height) {
            GeometryGeneratorSDF.FillRect(gfx, x, y, width, height, fillColor, uvTransform);
        }
        
        public void FillRect(float x, float y, float width, float height, in RenderInfo info) {
            
            if (info.backgroundImage != null) {
                UVTransform uvTransform;
                uvTransform.enabled = true;
                uvTransform.rotation = info.backgroundRotation;
                uvTransform.tilingX = info.backgroundScale.x;
                uvTransform.tilingY = info.backgroundScale.y;
                uvTransform.offsetX = info.uvOffset.x;
                uvTransform.offsetX = info.uvOffset.y;
                uvTransform.rect = new Rect(0, 0, 1, 1); // todo -- add style property
            }
            
            // resolve to percentage of width / height, input will be resolved pixel value
            float min = math.min(width, height);
            if (min <= 0) min = 0.0001f;
            float halfMin = min * 0.5f;

            float r0 = math.clamp(info.borderRadius.topLeft, 0, halfMin) / min;
            float r1 = math.clamp(info.borderRadius.topRight, 0, halfMin) / min;
            float r2 = math.clamp(info.borderRadius.bottomLeft, 0, halfMin) / min;
            float r3 = math.clamp(info.borderRadius.bottomRight, 0, halfMin) / min;
            
            byte b0 = (byte) (((r0 * 1000)) * 0.5f);
            byte b1 = (byte) (((r1 * 1000)) * 0.5f);
            byte b2 = (byte) (((r2 * 1000)) * 0.5f);
            byte b3 = (byte) (((r3 * 1000)) * 0.5f);
            
            float borderRadii = VertigoUtil.BytesToFloat(b0, b1, b2, b3);
            float borderColorTop = VertigoUtil.ColorToFloat(info.borderColorTop);
            float borderColorRight = VertigoUtil.ColorToFloat(info.borderColorRight);
            float borderColorBottom = VertigoUtil.ColorToFloat(info.borderColorBottom);
            float borderColorLeft = VertigoUtil.ColorToFloat(info.borderColorLeft);

            float packedBackgroundColor = VertigoUtil.ColorToFloat(info.backgroundColor);
            float packedBackgroundTint = VertigoUtil.ColorToFloat(info.backgroundTint);
            
            PaintMode colorMode = PaintMode.None;
            
            if (info.backgroundImage != null) {
                colorMode |= PaintMode.Texture;
            }
            
            if (info.backgroundTint.a > 0) {
                colorMode |= PaintMode.TextureTint;
            }
            
            if (info.backgroundColor.a > 0) {
                colorMode |= PaintMode.Color;
            }

            Color color = new Color(packedBackgroundColor, packedBackgroundTint, (int) (colorMode), 0);

            
            // texcoord 0 xy = uv zw = size
            // texcoord 1 x = border radius, y = border sizes z = (fill color mode | shape type)
            // texcoord 2 clip rect , mask or not, mask softness, mask uvs
           
            gfx.EnsureAdditionalCapacity(4, 6);

            Vector4 uv0;
            Vector4 uv1;
            Vector4 uv2;
            Vector4 uv3;
            
            uv0.x = 0;
            uv0.y = 1;
            uv0.z = width;
            uv0.w = height;
            
            uv1.x = 1;
            uv1.y = 1;
            uv1.z = width;
            uv1.w = height;
            
            uv2.x = 1;
            uv2.y = 0;
            uv2.z = width;
            uv2.w = height;
            
            uv3.x = 0;
            uv3.y = 0;
            uv3.z = width;
            uv3.w = height;
            
            float packedSize = VertigoUtil.PackSizeVector(new Vector2(width, height));
            float packedUV0 = VertigoUtil.Vector2ToFloat(uv0);
            float packedUV1 = VertigoUtil.Vector2ToFloat(uv1);
            float packedUV2 = VertigoUtil.Vector2ToFloat(uv2);
            float packedUV3 = VertigoUtil.Vector2ToFloat(uv3);
            
            Vector3 p0;
            Vector3 p1;
            Vector3 p2;
            Vector3 p3;
            Vector3 n0;

            n0.x = 0;
            n0.y = 0;
            n0.z = -1;
            
            p0.x = x;
            p0.y = -y;
            p0.z = 0;
            
            p1.x = x + width;
            p1.y = -y;
            p1.z = 0;
            
            p2.x = x + width;
            p2.y = -(y + height);
            p2.z = 0;
            
            p3.x = x;
            p3.y = -(y + height);
            p3.z = 0;

            int startVert = gfx.positionList.size;
            int startTriangle = gfx.triangleList.size;

            Vector3[] positions = gfx.positionList.array;
            Vector3[] normals = gfx.normalList.array;
            Color[] colors = gfx.colorList.array;
            Vector4[] texCoord0 = gfx.texCoordList0.array;
            Vector4[] texCoord1 = gfx.texCoordList1.array;
            int[] triangles = gfx.triangleList.array;
            
            positions[startVert + 0] = p0;
            positions[startVert + 1] = p1;
            positions[startVert + 2] = p2;
            positions[startVert + 3] = p3;

            normals[startVert + 0] = n0;
            normals[startVert + 1] = n0;
            normals[startVert + 2] = n0;
            normals[startVert + 3] = n0;

            colors[startVert + 0] = color;
            colors[startVert + 1] = color;
            colors[startVert + 2] = color;
            colors[startVert + 3] = color;
            
            texCoord0[startVert + 0] = uv0;
            texCoord0[startVert + 1] = uv1;
            texCoord0[startVert + 2] = uv2;
            texCoord0[startVert + 3] = uv3;

            uv0.x = borderRadii;
            uv0.y = 0;
            uv0.z = packedSize;
            uv0.w = packedUV0;
            
            uv1.x = borderRadii;
            uv1.y = 0;
            uv1.z = packedSize;
            uv1.w = packedUV1;
            
            uv2.x = borderRadii;
            uv2.y = 0;
            uv2.z = packedSize;
            uv2.w = packedUV2;
            
            uv3.x = borderRadii;
            uv3.y = 0;
            uv3.z = packedSize;
            uv3.w = packedUV3;
            
            texCoord1[startVert + 0] = uv0;
            texCoord1[startVert + 1] = uv1;
            texCoord1[startVert + 2] = uv2;
            texCoord1[startVert + 3] = uv3;

            triangles[startTriangle + 0] = startVert + 0;
            triangles[startTriangle + 1] = startVert + 1;
            triangles[startTriangle + 2] = startVert + 2;
            triangles[startTriangle + 3] = startVert + 2;
            triangles[startTriangle + 4] = startVert + 3;
            triangles[startTriangle + 5] = startVert + 0;

            gfx.UpdateSizes(4, 6);
        }

        public void FillMixedBorderRect(float x, float y, float width, float height, in RenderInfo renderInfo) {
            Vector3 p0 = new Vector3(x, -y);
            Vector3 p1 = new Vector3(x + width, -y);
            Vector3 p2 = new Vector3(x + width, -(y + height));
            Vector3 p3 = new Vector3(x, -(y + height));
            
            float leftSize = renderInfo.borderSize.left;
            float rightSize = renderInfo.borderSize.right;
            float topSize = renderInfo.borderSize.top;
            float bottomSize = renderInfo.borderSize.bottom;
            
            Vector3 p0Inset = p0 + new Vector3(leftSize, -topSize);
            Vector3 p1Inset = p1 + new Vector3(-rightSize, -topSize);
            Vector3 p2Inset = p2 + new Vector3(-rightSize, bottomSize);
            Vector3 p3Inset = p3 + new Vector3(leftSize, bottomSize);

            if (topSize > 0) {
                
                if (leftSize > 0) {
                    
                }

                if (rightSize > 0) {
                    
                }
                
            }
            
        }

    }

    [Flags]
    public enum PaintMode : byte {

        None = 0,
        Color = 1 << 0,
        Texture = 1 << 1,
        TextureTint = 1 << 2
                      
    }

    public abstract class GraphicsInterface {

        protected GraphicsContext gfx;

        internal GraphicsInterface(GraphicsContext gfx) {
            this.gfx = gfx;
        }

        public virtual void BeginFrame() { }

        public virtual void EndFrame() { }

        public virtual void SetTextureProperty(int key, Texture value) {
            gfx.SetTextureProperty(key, value);
        }

        public virtual void SetFloatProperty(int key, float value) {
            gfx.SetFloatProperty(key, value);
        }

        public void SetIntProperty(int key, int value) {
            gfx.SetIntProperty(key, value);
        }

        public void SetVectorProperty(int key, in Vector4 value) {
            gfx.SetVectorProperty(key, value);
        }

        public void SetColorProperty(int key, in Color value) {
            gfx.SetColorProperty(key, value);
        }

        public virtual void Draw(Geometry geometry) {
            gfx.Draw(geometry);
        }

        public virtual void Draw(ShapeCache geometry, int index) {
            gfx.Draw(geometry, index);
        }

        public virtual void Draw(ShapeCache geometry, IList<int> toDraw) {
            gfx.Draw(geometry, toDraw);
        }

        public virtual void SetStencilState(in StencilState state) { }

        public virtual void SetStencilRef(int stencilRef) { }

        public virtual void SetBlendState(in RenderTargetBlendState state) { }

        public void Render() {
            gfx.Render();
        }

        public void PushRenderTexture(int width, int height) {
            gfx.PushRenderTexture(width, height);
        }

        public void PopRenderTarget() {
            gfx.PopRenderTexture();
        }

    }

}