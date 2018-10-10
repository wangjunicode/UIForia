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

        public UIGraphicElement() {
            flags |= UIElementFlags.Primitive;
            mesh = new Mesh();
            material = Graphic.defaultGraphicMaterial;
            IsGeometryDirty = true;
            IsMaterialDirty = true;
        }

        public event Action<IDrawable> onMeshDirty;
        public event Action<IDrawable> onMaterialDirty;
        
        public float width => mesh.bounds.extents.x;
        public float height => mesh.bounds.extents.y;

        public int Id => id;
        public bool IsMaterialDirty { get; private set; }
        public bool IsGeometryDirty { get; private set; }
        
        public Texture GetMainTexture() {
            return Texture2D.whiteTexture;
        }

        public override void OnCreate() {
            style.SetPreferredWidth(10f, StyleState.Normal);
            style.SetPreferredHeight(10f, StyleState.Normal);
        }

        public void RebuildGeometry() {
            rebuildGeometry?.Invoke(mesh);
            style.SetPreferredWidth(new UIMeasurement(width), StyleState.Normal);
            style.SetPreferredHeight(new UIMeasurement(height), StyleState.Normal);
            IsGeometryDirty = false;
        }

        public void RebuildMaterial() {
            rebuildMaterial?.Invoke(material);
            IsMaterialDirty = true;
            onMaterialDirty?.Invoke(this);
        }

        public void MarkMaterialDirty() {
            IsMaterialDirty = true;
            onMeshDirty?.Invoke(this);
        }

        public void MarkGeometryDirty() {
            IsGeometryDirty = true;
            onMaterialDirty?.Invoke(this);
        }

        public void OnStylePropertyChanged(StyleProperty property) {
            
        }

        public Mesh GetMesh() {
            if (IsGeometryDirty) {
                RebuildGeometry();
            }
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
            //throw new NotImplementedException();
        }

        public void OnStylePropertyChanged(UIElement element, StyleProperty property) {
            //throw new NotImplementedException();
        }

    }

}