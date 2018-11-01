using System.Collections.Generic;
using Src.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Util {

    public static class MeshUtil {

        public static readonly VertexHelper s_VertexHelper = new VertexHelper();
        private static readonly ObjectPool<Mesh> s_MeshPool = new ObjectPool<Mesh>(null, (m) => m.Clear());
        private static readonly Vector2 s_UVEmpty = Vector2.zero;
        private static readonly Vector4 s_TangentEmpty = new Vector4();

        public static Mesh CreateStandardUIMesh(Size size, Color32 color32) {
            Mesh mesh = new Mesh();

            Vector2 uv1 = new Vector2();
            Vector4 tangent = new Vector4();

            Vector3 normal0 = new Vector3(0, 0, 0);
            Vector3 normal1 = new Vector3(0, 1, 0);
            Vector3 normal2 = new Vector3(1, 1, 0);
            Vector3 normal3 = new Vector3(1, 0, 0);

            s_VertexHelper.AddVert(new Vector3(0, 0), color32, new Vector2(0f, 1f), uv1, normal0, tangent);
            s_VertexHelper.AddVert(new Vector3(0, -size.height), color32, new Vector2(0f, 0f), uv1, normal1, tangent);
            s_VertexHelper.AddVert(new Vector3(size.width, -size.height), color32, new Vector2(1f, 0f), uv1, normal2, tangent);
            s_VertexHelper.AddVert(new Vector3(size.width, 0), color32, new Vector2(1f, 1f), uv1, normal3, tangent);

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

        public static Mesh CreateTriangle(Size size, float offset = 0) {
            Mesh mesh = new Mesh();

            Vector3 normal0 = new Vector3(-1, 0, 0);
            Vector3 normal1 = new Vector3(0, 1, 0);
            Vector3 normal2 = new Vector3(1, 0, 0);
            Color32 color32 = Color.white;
            offset = Mathf.Clamp01(offset);
            s_VertexHelper.AddVert(new Vector3(size.width * offset, size.height, 0), color32, new Vector2(offset, 1f), s_UVEmpty, normal0, s_TangentEmpty);
            s_VertexHelper.AddVert(new Vector3(0, 0, 0), color32, new Vector2(0f, 0f), s_UVEmpty, normal1, s_TangentEmpty);
            s_VertexHelper.AddVert(new Vector3(size.width, 0, 0), color32, new Vector2(1f, 0f), s_UVEmpty, normal2, s_TangentEmpty);

            s_VertexHelper.AddTriangle(0, 1, 2);
            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();

            return mesh;
        }

        public static Mesh CreateDiamond(Size size) {
            Mesh mesh = new Mesh();

            // top, right, bottom, left

            Vector3 normal0 = new Vector3(0, 1, 0);
            Vector3 normal1 = new Vector3(1, 0, 0);
            Vector3 normal2 = new Vector3(0, -1, 0);
            Vector3 normal3 = new Vector3(-1, 0, 0);
            Color32 color32 = Color.white;

            s_VertexHelper.AddVert(new Vector3(size.width * 0.5f, 0, 0f), color32, new Vector2(0.5f, 0), s_UVEmpty, normal0, s_TangentEmpty);
            s_VertexHelper.AddVert(new Vector3(size.width, size.height * 0.5f, 0), color32, new Vector2(1f, 0.5f), s_UVEmpty, normal1, s_TangentEmpty);
            s_VertexHelper.AddVert(new Vector3(size.width * 0.5f, size.height, 0), color32, new Vector2(0.5f, 1f), s_UVEmpty, normal2, s_TangentEmpty);
            s_VertexHelper.AddVert(new Vector3(0, size.height * 0.5f, 0), color32, new Vector2(0, 0.5f), s_UVEmpty, normal3, s_TangentEmpty);

            s_VertexHelper.AddTriangle(0, 1, 2);
            s_VertexHelper.AddTriangle(2, 3, 1);
            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();

            return mesh;
        }

        public static void Release(Mesh mesh) {
            // todo -- free the arrays independently back into the respective array pools
            s_MeshPool.Release(mesh);
        }

        public static Mesh ResizeStandardUIMesh(Mesh mesh, Size size) {
            if(mesh == null) mesh = new Mesh();
           
            Vector2 uv1 = new Vector2();
            Vector4 tangent = new Vector4();

            Vector3 normal0 = new Vector3(0, 0, 0);
            Vector3 normal1 = new Vector3(0, 1, 0);
            Vector3 normal2 = new Vector3(1, 1, 0);
            Vector3 normal3 = new Vector3(1, 0, 0);

            Color32 color32 = Color.white;
            s_VertexHelper.AddVert(new Vector3(0, 0), color32, new Vector2(0f, 1f), uv1, normal0, tangent);
            s_VertexHelper.AddVert(new Vector3(0, -size.height), color32, new Vector2(0f, 0f), uv1, normal1, tangent);
            s_VertexHelper.AddVert(new Vector3(size.width, -size.height), color32, new Vector2(1f, 0f), uv1, normal2, tangent);
            s_VertexHelper.AddVert(new Vector3(size.width, 0), color32, new Vector2(1f, 1f), uv1, normal3, tangent);

            s_VertexHelper.AddTriangle(0, 1, 2);
            s_VertexHelper.AddTriangle(2, 3, 0);

            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();
            return mesh;
            
            
            
            throw new System.NotImplementedException();
        }

    }

}