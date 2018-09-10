using System;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Elements {

    public class UIGraphicElement : UIElement {

        public event Action<int, Mesh> onMeshUpdated;
        public event Action<int, Material> onMaterialUpdated;

        private Mesh mesh;
        private Material material;
        
        public UIGraphicElement() {
            flags |= UIElementFlags.RequiresSpecialRendering;
            flags |= UIElementFlags.Primitive;
            mesh = new Mesh();
            material = Graphic.defaultGraphicMaterial;
        }

        public float width => mesh.bounds.extents.x;
        public float height => mesh.bounds.extents.y;
        
        public Mesh GetMesh() {
            return mesh;
        }

        public void SetMesh(Mesh mesh) {
            this.mesh = mesh;
            onMeshUpdated?.Invoke(id, mesh);
        }

        public void UpdateMesh() {
            onMeshUpdated?.Invoke(id, mesh);
        }

        public Material GetMaterial() {
            return material;
        }
        
        public void SetMaterial(Material material) {
            this.material = material;
            onMaterialUpdated?.Invoke(id, material);
        }

    }

}