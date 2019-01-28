using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public class StrokeVertexData {

        public readonly LightList<Vector3> position;
        public readonly LightList<Vector4> prevNext;
        public readonly LightList<Vector4> flags;
        public readonly LightList<Color> colors;
        public readonly LightList<int> triangles;
        public int triangleIndex;
        
        public StrokeVertexData() {
            this.position = new LightList<Vector3>(32);
            this.prevNext = new LightList<Vector4>(32);
            this.flags = new LightList<Vector4>(32);
            this.colors = new LightList<Color>(32);
            this.triangles = new LightList<int>(32 * 3);
        }

        public void Clear() {
            position.Clear();
            prevNext.Clear();
            flags.Clear();
            colors.Clear();
            triangles.Clear();
            triangleIndex = 0;
        }

        public void EnsureCapacity(int vertexCount) {
            position.EnsureAdditionalCapacity(vertexCount);
            prevNext.EnsureCapacity(vertexCount);
            flags.EnsureCapacity(vertexCount);
            colors.EnsureCapacity(vertexCount);
            triangles.EnsureCapacity(vertexCount * 3);
        }

        public void FillMesh(Mesh mesh) {
            // todo -- the list requirement sucks :/
            mesh.SetVertices(position.ToList());
            mesh.SetColors(colors.ToList());
            mesh.SetUVs(0, new List<Vector2>());
            mesh.SetUVs(1, prevNext.ToList());
            mesh.SetUVs(2, flags.ToList());
            mesh.SetTriangles(triangles.ToList(), 0);
        }

    }

}