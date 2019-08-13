using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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
            this.uvTransform = uvTransform;
        }

        public void DisableUVTransform() {
            uvTransform.enabled = false;
        }

        public void FillRect(float x, float y, float width, float height) {
            GeometryGeneratorSDF.FillRect(gfx, x, y, width, height, fillColor, uvTransform);
        }
        
        
    }

    [Flags]
    public enum PaintMode {

        None = 0,
        Color = 1 << 0,
        Texture = 1 << 1,
        TextureTint = 1 << 2,
        LetterBoxTexture = 1 << 3,
        Shadow = 1 << 4,
        ShadowTint = 1 << 5

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