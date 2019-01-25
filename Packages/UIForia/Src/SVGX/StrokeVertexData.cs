using System.Linq;
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
            this.position = new LightList<Vector3>(1024);
            this.prevNext = new LightList<Vector4>(1024);
            this.flags = new LightList<Vector4>(1024);
            this.colors = new LightList<Color>(1024);
            this.triangles = new LightList<int>(1024 * 3);
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
            mesh.vertices = position.Array;
            mesh.colors = colors.Array;
            mesh.SetUVs(1, prevNext.Array.ToList());
            mesh.SetUVs(2, flags.Array.ToList());
            mesh.triangles = triangles.Array;
        }

    }

}