using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public class FillVertexData {

        public int triangleIndex;
        public readonly LightList<Vector3> position;
        public readonly LightList<Color> colors;
        public readonly LightList<int> triangles;
        public readonly LightList<Vector2> texCoords;
        public readonly LightList<Vector4> flags;
        public readonly Mesh mesh;
        
        public FillVertexData() {
            position = new LightList<Vector3>(32);
            colors = new LightList<Color>(32);
            texCoords = new LightList<Vector2>(32);
            flags = new LightList<Vector4>(32);
            triangles = new LightList<int>(32 * 3);
            mesh = new Mesh();
            mesh.MarkDynamic();
        }

        public void Clear() {
            texCoords.Clear();
            position.Clear();
            colors.Clear();
            triangles.Clear();
            flags.Clear();
            triangleIndex = 0;
        }

        // todo -- remove list allocation
        public Mesh FillMesh() {
            mesh.SetVertices(position.ToList());
            mesh.SetColors(colors.ToList());
            mesh.SetUVs(0, texCoords.ToList());
            mesh.SetUVs(1, flags.ToList());
            mesh.SetTriangles(triangles.ToList(), 0);
            return mesh;
        }

    }


}