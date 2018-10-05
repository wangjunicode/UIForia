using System;
using Rendering;
using Src.Elements;
using Src.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Systems {

    public class StandardDrawable : IDrawable {

        private static readonly VertexHelper s_VertexHelper = new VertexHelper();
        private static readonly ObjectPool<Mesh> s_MeshPool = new ObjectPool<Mesh>(null, (m) => m.Clear());

        public event Action<IDrawable> onMeshDirty;
        public event Action<IDrawable> onMaterialDirty;

        protected bool isMeshDirty;
        protected bool isMaterialDirty;
        protected Mesh mesh;
        protected Material material;
        public readonly UIElement element;

        public StandardDrawable(UIElement element) {
            this.element = element;
            this.isMeshDirty = true;
            this.isMaterialDirty = true;
        }

        public int Id => element.id;
        public bool IsGeometryDirty => isMeshDirty;
        
        public Texture GetMainTexture() {
            return Texture2D.whiteTexture;
        }

        public bool IsMaterialDirty => isMaterialDirty;

        public void OnAllocatedSizeChanged() {
            isMeshDirty = true;
            onMeshDirty?.Invoke(this);
        }

        public void OnStylePropertyChanged(StyleProperty property) {
            switch (property.propertyId) {
                case StylePropertyId.BackgroundColor:
                    SetVerticesDirty();
                    break;
            }
        }

        public Mesh GetMesh() {
            if (!isMeshDirty) return mesh;
            if (mesh != null) {
                s_MeshPool.Release(mesh);
            }

            mesh = CreateStandardUIMesh(
                element.layoutResult.localPosition,
                element.layoutResult.allocatedSize,
                element.style.computedStyle.BackgroundColor
            );

            return mesh;
        }

        public Material GetMaterial() {
            return Graphic.defaultGraphicMaterial;
        }

        private void SetVerticesDirty() {
            isMeshDirty = true;
            onMeshDirty?.Invoke(this);
        }

        private static Mesh CreateStandardUIMesh(Vector2 position, Size size, Color32 color32) {
            Mesh mesh = s_MeshPool.Get();

            s_VertexHelper.AddVert(new Vector3(0, 0), color32, new Vector2(0f, 0f));
            s_VertexHelper.AddVert(new Vector3(0, position.y - size.height), color32, new Vector2(0f, 1f));
            s_VertexHelper.AddVert(new Vector3(size.width, position.y - size.height), color32, new Vector2(1f, 1f));
            s_VertexHelper.AddVert(new Vector3(size.width, 0), color32, new Vector2(1f, 0f));

            s_VertexHelper.AddTriangle(0, 1, 2);
            s_VertexHelper.AddTriangle(2, 3, 0);

            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();
            return mesh;
        }

    }

    public class RenderData : IHierarchical {

        public UIElement element;

        public VirtualScrollbar horizontalScrollbar;
        public VirtualScrollbar verticalScrollbar;
        public RectTransform horizontalScrollbarHandle;
        public RectTransform verticalScrollbarHandle;

        public IDrawable drawable;
        public GORenderSystem renderSystem;

        public RenderData(UIElement element, GORenderSystem renderSystem) {
            this.element = element;
            this.renderSystem = renderSystem;

            IDrawable graphicElement = element as IDrawable;
            if (graphicElement != null) {
                this.drawable = graphicElement;
                graphicElement.onMeshDirty += HandleMeshDirty;
                graphicElement.onMaterialDirty += HandleMaterialDirty;
            }
            else {
                this.drawable = new StandardDrawable(element);
            }
        }

        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

        public void HandleMeshDirty(IDrawable element) {
            renderSystem.MarkGeometryDirty(element);
        }

        public void HandleMaterialDirty(IDrawable element) {
            renderSystem.MarkMaterialDirty(element);
        }

    }

}