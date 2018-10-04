using System;
using Rendering;
using Src.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Elements {

    public class UIGraphicElement : UIElement, IDrawable {

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

        public event Action<IDrawable> onMeshDirty;
        public event Action<IDrawable> onMaterialDirty;
        
        public float width => mesh.bounds.extents.x;
        public float height => mesh.bounds.extents.y;

        public int Id => id;
        public bool IsMaterialDirty { get; private set; }
        public bool IsGeometryDirty { get; private set; }
        
        public override void OnCreate() {
            style.SetPreferredWidth(new UIMeasurement(0), StyleState.All);
            style.SetPreferredHeight(new UIMeasurement(0), StyleState.All);
        }

        public void RebuildGeometry() {
            rebuildGeometry?.Invoke(mesh);
            style.SetPreferredWidth(new UIMeasurement(width), StyleState.All);
            style.SetPreferredHeight(new UIMeasurement(height), StyleState.All);
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

        public void OnStylePropertyChanged(StyleProperty property) {
            
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

        public void OnAllocatedSizeChanged() {
            throw new NotImplementedException();
        }

        public void OnStylePropertyChanged(UIElement element, StyleProperty property) {
            throw new NotImplementedException();
        }

    }

}