using System;
using Rendering;
using Src.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Src {

    public abstract class CustomDrawableElement : UIElement, IDrawable {

        protected bool isMeshDirty;
        protected bool isMaterialDirty;
        protected Mesh mesh;

        protected CustomDrawableElement() {
            this.isMeshDirty = true;
            this.isMaterialDirty = true;
        }
        
        public virtual void OnAllocatedSizeChanged() { }

        public virtual void OnStylePropertyChanged(StyleProperty property) { }

        public abstract Mesh GetMesh();

        public virtual Material GetMaterial() {
            return Graphic.defaultGraphicMaterial;
        }

        protected void SetVerticesDirty() {
            isMeshDirty = true;
            onMeshDirty?.Invoke(this);
        }

        protected void SetMaterialDirty() {
            isMaterialDirty = true;
            onMaterialDirty?.Invoke(this);
        }

        public event Action<IDrawable> onMeshDirty;
        public event Action<IDrawable> onMaterialDirty;
        public int Id => id;
        public bool IsMaterialDirty => isMaterialDirty;
        public bool IsGeometryDirty => isMeshDirty;
        
        public virtual Texture GetMainTexture() {
            return Texture2D.whiteTexture;
        }

    }

}