using System;
using Src.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Elements {

    public class UIGraphicElement : UIElement {

        public Action<Mesh> rebuildGeometry;

        private Mesh mesh;
        private Material material;

        internal IGraphicUpdateManager updateManager;

        public UIGraphicElement() {
            flags |= UIElementFlags.RequiresSpecialRendering;
            flags |= UIElementFlags.Primitive;
            mesh = new Mesh();
            material = Graphic.defaultGraphicMaterial;
        }

        public float width => mesh.bounds.extents.x;
        public float height => mesh.bounds.extents.y;

        public override void OnCreate() {
            style.width = new UIMeasurement(0);
            style.height = new UIMeasurement(0);
        }

        protected internal void RebuildGeometry() {
            rebuildGeometry?.Invoke(mesh);
            style.width = new UIMeasurement(width);
            style.height = new UIMeasurement(height);
        }

        public void MarkMaterialDirty() { }

        public void MarkGeometryDirty() {
            updateManager.MarkGeometryDirty(this);
        }

        public Mesh GetMesh() {
            return mesh;
        }

        public void SetMesh(Mesh mesh) {
            this.mesh = mesh;
        }


        public Material GetMaterial() {
            return material;
        }

        public void SetMaterial(Material material) {
            this.material = material;
        }

    }

}