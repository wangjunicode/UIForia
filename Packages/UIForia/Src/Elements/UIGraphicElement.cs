using System;
using JetBrains.Annotations;
using UIForia.Rendering;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Elements {

    public class UIGraphicElement : UIElement, IMeshProvider {

        public Action<Mesh> rebuildGeometry;

        private Mesh mesh;

        public UIGraphicElement() {
            flags |= UIElementFlags.Primitive;
            mesh = new Mesh();
            IsGeometryDirty = true;
        }

        public float width => (mesh != null) ? mesh.bounds.extents.x : 0;
        public float height => (mesh != null) ? mesh.bounds.extents.y : 0;

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

        public override string GetDisplayName() {
            return "Graphic";
        }

    }

}