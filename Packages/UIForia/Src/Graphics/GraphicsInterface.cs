using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Src.Systems;
using UIForia.Util;
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
            
            byte b0 = (byte) (((info.borderRadius.x * 1000)) * 0.5f);
            byte b1 = (byte) (((info.borderRadius.y * 1000)) * 0.5f);
            byte b2 = (byte) (((info.borderRadius.z * 1000)) * 0.5f);
            byte b3 = (byte) (((info.borderRadius.w * 1000)) * 0.5f);
            
            float borderRadii = VertigoUtil.BytesToFloat(b0, b1, b2, b3);
            float borderColorTop = VertigoUtil.ColorToFloat(info.borderColorTop);
            float borderColorRight = VertigoUtil.ColorToFloat(info.borderColorRight);
            float borderColorBottom = VertigoUtil.ColorToFloat(info.borderColorBottom);
            float borderColorLeft = VertigoUtil.ColorToFloat(info.borderColorLeft);

            Color color = info.backgroundColor;
            Color tint = info.backgroundTint;
            float packedBackgroundColor = VertigoUtil.ColorToFloat(color);
            float packedBackgroundTint = VertigoUtil.ColorToFloat(tint);
            
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

            float encodedColorMode = FloatUtil.DecodeToFloat((int) (PaintMode.Texture));
            
            // texcoord 0 xy = uv zw = size
            // texcoord 1 x = border radius, y = border sizes z = (fill color mode | shape type)
            // texcoord 2 clip rect , mask or not, mask softness, mask uvs
           
            gfx.EnsureAdditionalCapacity(4, 6);

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

            colors[startVert + 0].r = packedBackgroundColor;
            colors[startVert + 0].g = packedBackgroundTint;
            colors[startVert + 0].b = encodedColorMode;
            
            colors[startVert + 1].r = packedBackgroundColor;
            colors[startVert + 1].g = packedBackgroundTint;
            colors[startVert + 1].b = encodedColorMode;
            
            colors[startVert + 2].r = packedBackgroundColor;
            colors[startVert + 2].g = packedBackgroundTint;
            colors[startVert + 2].b = encodedColorMode;
            
            colors[startVert + 3].r = packedBackgroundColor;
            colors[startVert + 3].g = packedBackgroundTint;
            colors[startVert + 3].b = encodedColorMode;
            

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
            
            texCoord0[startVert + 0] = uv0;
            texCoord0[startVert + 1] = uv1;
            texCoord0[startVert + 2] = uv2;
            texCoord0[startVert + 3] = uv3;

            uv0.x = borderRadii;
            uv0.y = encodedColorMode;
            uv1.x = borderRadii;
            uv1.y = encodedColorMode;
            uv2.x = borderRadii;
            uv2.y = encodedColorMode;
            uv3.x = borderRadii;
            uv3.y = encodedColorMode;
            
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

        public virtual void SetTexture(int key, Texture value) {
            gfx.SetTexture(key, value);
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