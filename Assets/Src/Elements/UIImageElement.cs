using System;
using Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Src {

    public class UIImageElement : UIElement, IDrawable, IPropertyChangedHandler {

        public event Action<IDrawable> onMeshDirty;
        public event Action<IDrawable> onMaterialDirty;

        private Mesh mesh;
        private Texture2DAssetReference src;

        public bool useNativeSize;
        public bool preserveAspectRatio;
        public bool scaleToFitContainer;

        public UIImageElement() {
            flags |= UIElementFlags.Image;
            flags |= UIElementFlags.Primitive;
            IsMaterialDirty = true;
            IsGeometryDirty = true;
        }

        public Texture2D Asset => src.asset;

        public void SetTexture(Texture2D texture) {
            src = new Texture2DAssetReference(texture);
            IsMaterialDirty = true;
            onMaterialDirty?.Invoke(this);
        }

        public void SetPreserveAspectRatio(bool preserve) {
            if (preserve != preserveAspectRatio) {
                preserveAspectRatio = preserve;
                // todo -- need to trigger layout somehow
                OnAllocatedSizeChanged();
            }
        }

        public void OnPropertyChanged(string propertyName, object oldValue) {
            if (propertyName == nameof(src)) {
                IsMaterialDirty = true;
                onMaterialDirty?.Invoke(this);
            }
        }

        public void OnAllocatedSizeChanged() {
            IsGeometryDirty = true;
            onMeshDirty?.Invoke(this);
        }

        public void OnStylePropertyChanged(StyleProperty property) { }

        public Mesh GetMesh() {
            if (!IsGeometryDirty) return mesh;

            Color32 color = style.computedStyle.BackgroundColor;
            if (!ColorUtil.IsDefined(style.computedStyle.BackgroundColor)) {
                color = Color.white;
            }

            mesh = MeshUtil.CreateStandardUIMesh(layoutResult.contentOffset,
                layoutResult.actualSize, color);

            return mesh;
        }

        public Material GetMaterial() {
            return Graphic.defaultGraphicMaterial;
        }

        public int Id => id;
        public bool IsMaterialDirty { get; private set; }
        public bool IsGeometryDirty { get; private set; }

        public Texture GetMainTexture() {
            return src.asset;
        }

    }

}