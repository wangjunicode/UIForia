using System;
using Rendering;
using Src.Systems;
using UnityEngine;

namespace Src.Elements {

    public class UIGraphicElement : UIElement, IMeshProvider {

        public Action<Mesh> rebuildGeometry;

        private Mesh mesh;

        public UIGraphicElement() {
            flags |= UIElementFlags.Primitive;
            mesh = new Mesh();
            IsGeometryDirty = true;
        }      
        
        public float width => mesh.bounds.extents.x;
        public float height => mesh.bounds.extents.y;

        public bool IsGeometryDirty { get; private set; }
       
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

        public void MarkGeometryDirty() {
            IsGeometryDirty = true;
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

    }

}