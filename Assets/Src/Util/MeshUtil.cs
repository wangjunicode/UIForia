using Src.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Util {

    public static class MeshUtil {

        public static readonly VertexHelper s_VertexHelper = new VertexHelper();
        public static readonly ObjectPool<Mesh> s_MeshPool = new ObjectPool<Mesh>(null, (m) => m.Clear());

        public static Mesh CreateStandardUIMesh(Size size, Color32 color32) {
            Mesh mesh = new Mesh();

            s_VertexHelper.AddVert(new Vector3(0, 0), color32, new Vector2(0f, 1f));
            s_VertexHelper.AddVert(new Vector3(0, -size.height), color32, new Vector2(0f, 0f));
            s_VertexHelper.AddVert(new Vector3(size.width, -size.height), color32, new Vector2(1f, 0f));
            s_VertexHelper.AddVert(new Vector3(size.width, 0), color32, new Vector2(1f, 1f));

            s_VertexHelper.AddTriangle(0, 1, 2);
            s_VertexHelper.AddTriangle(2, 3, 0);

            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();
            return mesh;
        }
        
        public static Mesh CreateStandardUIMesh(Vector2 offset, Size size, Color32 color32) {
            Mesh mesh = new Mesh();

            s_VertexHelper.AddVert(new Vector3(offset.x + 0, offset.y + 0), color32, new Vector2(0f, 1f));
            s_VertexHelper.AddVert(new Vector3(offset.x + 0, offset.y + -size.height), color32, new Vector2(0f, 0f));
            s_VertexHelper.AddVert(new Vector3(offset.x + size.width, offset.y + -size.height), color32, new Vector2(1f, 0f));
            s_VertexHelper.AddVert(new Vector3(offset.x + size.width, offset.y + 0), color32, new Vector2(1f, 1f));

            s_VertexHelper.AddTriangle(0, 1, 2);
            s_VertexHelper.AddTriangle(2, 3, 0);

            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();
            return mesh;
        }


        public static void Release(Mesh mesh) {
            // todo -- free the arrays independently back into the respective array pools
            s_MeshPool.Release(mesh);
        }

    }

}