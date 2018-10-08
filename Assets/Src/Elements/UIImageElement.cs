using System;
using Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = System.Numerics.Vector2;

namespace Src {

    public class UIImageElement : UIElement, IDrawable {

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
//            useNativeSize = true; // hack for now
        }

        public Texture2D Asset => src.asset;

//        public Texture2DAssetReference src {
//            get { return _src; }
//            set {
//                _src = value;
//                if (_src.asset != null) {
//                    style.SetPreferredWidth(_src.asset.width, StyleState.Normal);
//                    style.SetPreferredHeight(_src.asset.height, StyleState.Normal);
//                }
//
//                IsGeometryDirty = true;
//                onMeshDirty?.Invoke(this);
//            }
//        }

        public void OnAllocatedSizeChanged() {
            if (scaleToFitContainer) { }
        }

        public void OnStylePropertyChanged(StyleProperty property) { }

        public Mesh GetMesh() {
            if (!IsGeometryDirty) return mesh;
//            if (mesh != null) {
//                MeshUtil.Release(mesh);
//            }

            Color32 color = style.computedStyle.BackgroundColor;
            if (!ColorUtil.IsDefined(style.computedStyle.BackgroundColor)) {
                color = Color.white;
            }

//            Size size = layoutResult.allocatedSize;

//            float width = size.width
//                          - style.computedStyle.PaddingLeft.value
//                          - style.computedStyle.PaddingRight.value
//                          - style.computedStyle.BorderLeft.value
//                          - style.computedStyle.BorderRight.value;
//
//            float height = size.height
//                           - style.computedStyle.PaddingTop.value
//                           - style.computedStyle.PaddingBottom.value
//                           - style.computedStyle.BorderTop.value
//                           - style.computedStyle.BorderBottom.value;
//
//            float offsetX = style.computedStyle.PaddingLeft.value + style.computedStyle.BorderLeft.value;
//            float offsetY = style.computedStyle.PaddingTop.value + style.computedStyle.BorderTop.value;
            
            mesh = MeshUtil.CreateStandardUIMesh(layoutResult.allocatedSize, color);

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