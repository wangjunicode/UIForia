using System;
using Src.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Elements {

    public interface IGraphicElement {

        int Id { get; }
        void RebuildGeometry();
        void RebuildMaterial();

        void MarkGeometryDirty();
        void MarkMaterialDirty();

        bool IsMaterialDirty { get; }
        bool IsGeometryDirty { get; }

        Mesh GetMesh();
        Material GetMaterial();

    }

    public class UIGraphicElement : UIElement, IGraphicElement {

        public Action<Mesh> rebuildGeometry;
        public Action<Material> rebuildMaterial;

        private Mesh mesh;
        private Material material;

        internal IGraphicUpdateManager updateManager;

        public UIGraphicElement() {
            flags |= UIElementFlags.RequiresSpecialRendering;
            flags |= UIElementFlags.Primitive;
            mesh = new Mesh();
            material = Graphic.defaultGraphicMaterial;
        }

        public int Id => id;
        
        public float width => mesh.bounds.extents.x;
        public float height => mesh.bounds.extents.y;
        
        public bool IsMaterialDirty { get; private set; }
        public bool IsGeometryDirty { get; private set; }
        
        public override void OnCreate() {
            style.width = new UIMeasurement(0);
            style.height = new UIMeasurement(0);
        }

        public void RebuildGeometry() {
            rebuildGeometry?.Invoke(mesh);
            style.width = new UIMeasurement(width);
            style.height = new UIMeasurement(height);
            IsGeometryDirty = false;
        }

        public void RebuildMaterial() {
            rebuildMaterial?.Invoke(material);
            IsMaterialDirty = true;
        }

        public void MarkMaterialDirty() {
            IsMaterialDirty = true;
            updateManager.MarkMaterialDirty(this);
        }

        public void MarkGeometryDirty() {
            IsGeometryDirty = true;
            updateManager.MarkGeometryDirty(this);
        }

        public Mesh GetMesh() {
            return mesh;
        }

        public void SetMesh(Mesh mesh) {
            this.mesh = mesh;
            MarkGeometryDirty();
        }

        public Material GetMaterial() {
            return material;
        }

        public void SetMaterial(Material material) {
            this.material = material;
            MarkMaterialDirty();
        }

    }

}