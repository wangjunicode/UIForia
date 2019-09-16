using System;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public class UIForiaGeometry {

        public StructList<Vector3> positionList;
        public StructList<Vector4> texCoordList0;
        public StructList<Vector4> texCoordList1;
        public StructList<int> triangleList;
        
        public Vector4 packedColors;
        public Vector4 objectData;
        public Texture mainTexture;
        public Vector4 miscData;
        public Vector4 cornerData;

        public UIForiaGeometry() {
            this.positionList = new StructList<Vector3>();
            this.texCoordList0 = new StructList<Vector4>();
            this.texCoordList1 = new StructList<Vector4>();
            this.triangleList = new StructList<int>();
        }

        public void EnsureAdditionalCapacity(int vertexCount, int triangleCount) {
            positionList.EnsureAdditionalCapacity(vertexCount);
            texCoordList0.EnsureAdditionalCapacity(vertexCount);
            texCoordList1.EnsureAdditionalCapacity(vertexCount);
            triangleList.EnsureAdditionalCapacity(triangleCount);
        }

        public void Clear() {
            mainTexture = null;
            objectData = default;
            packedColors = default;
            positionList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            triangleList.size = 0;
        }

        public void UpdateSizes(int vertexCount, int triangleCount) {
            positionList.size += vertexCount;
            texCoordList0.size += vertexCount;
            texCoordList1.size += vertexCount;
            triangleList.size += triangleCount;
        }

        public void Quad(float width, float height) {
            EnsureAdditionalCapacity(4, 6);

            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            int[] triangles = triangleList.array;

            int startVert = positionList.size;
            int startTriangle = triangleList.size;

            ref Vector3 p0 = ref positions[startVert + 0];
            ref Vector3 p1 = ref positions[startVert + 1];
            ref Vector3 p2 = ref positions[startVert + 2];
            ref Vector3 p3 = ref positions[startVert + 3];

            ref Vector4 uv0 = ref texCoord0[startVert + 0];
            ref Vector4 uv1 = ref texCoord0[startVert + 1];
            ref Vector4 uv2 = ref texCoord0[startVert + 2];
            ref Vector4 uv3 = ref texCoord0[startVert + 3];

            p0.x = 0;
            p0.y = 0;
            p0.z = 0;

            p1.x = width;
            p1.y = 0;
            p1.z = 0;

            p2.x = width;
            p2.y = -height;
            p2.z = 0;

            p3.x = 0;
            p3.y = -height;
            p3.z = 0;

            uv0.x = 0;
            uv0.y = 1;

            uv1.x = 1;
            uv1.y = 1;

            uv2.x = 1;
            uv2.y = 0;

            uv3.x = 0;
            uv3.y = 0;

            triangles[startTriangle + 0] = startVert + 0;
            triangles[startTriangle + 1] = startVert + 1;
            triangles[startTriangle + 2] = startVert + 2;
            triangles[startTriangle + 3] = startVert + 2;
            triangles[startTriangle + 4] = startVert + 3;
            triangles[startTriangle + 5] = startVert + 0;

            UpdateSizes(4, 6);
        }

        public void ClipCornerRect(Size size, in CornerDefinition cornerDefinition, in Vector2 position = default) {
            EnsureAdditionalCapacity(9, 24);
            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            int[] triangles = triangleList.array;

            int startVert = positionList.size;
            int startTriangle = triangleList.size;

            float width = size.width;
            float height = size.height;

            positions[startVert + 0] = new Vector3(position.x + 0, -(position.y + cornerDefinition.topLeftY), 0);
            positions[startVert + 1] = new Vector3(position.x + cornerDefinition.topLeftX, -position.y, 0);
            positions[startVert + 2] = new Vector3(position.x + width - cornerDefinition.topRightX, -position.y, 0);
            positions[startVert + 3] = new Vector3(position.x + width, -(position.y + cornerDefinition.topRightY), 0);
            positions[startVert + 4] = new Vector3(position.x + width, -(position.y + height - cornerDefinition.bottomRightY), 0);
            positions[startVert + 5] = new Vector3(position.x + width - cornerDefinition.bottomRightX, -(position.y + height), 0);
            positions[startVert + 6] = new Vector3(position.x + cornerDefinition.bottomLeftX, -(position.y + height), 0);
            positions[startVert + 7] = new Vector3(position.x + 0, -(position.y + height - cornerDefinition.bottomLeftY), 0);
            positions[startVert + 8] = new Vector3(position.x + width * 0.5f, -(position.y + (height * 0.5f)), 0);

            triangles[startTriangle + 0] = startVert + 1;
            triangles[startTriangle + 1] = startVert + 8;
            triangles[startTriangle + 2] = startVert + 0;

            triangles[startTriangle + 3] = startVert + 2;
            triangles[startTriangle + 4] = startVert + 8;
            triangles[startTriangle + 5] = startVert + 1;

            triangles[startTriangle + 6] = startVert + 3;
            triangles[startTriangle + 7] = startVert + 8;
            triangles[startTriangle + 8] = startVert + 2;

            triangles[startTriangle + 9] = startVert + 4;
            triangles[startTriangle + 10] = startVert + 8;
            triangles[startTriangle + 11] = startVert + 3;

            triangles[startTriangle + 12] = startVert + 5;
            triangles[startTriangle + 13] = startVert + 8;
            triangles[startTriangle + 14] = startVert + 4;

            triangles[startTriangle + 15] = startVert + 6;
            triangles[startTriangle + 16] = startVert + 8;
            triangles[startTriangle + 17] = startVert + 5;

            triangles[startTriangle + 18] = startVert + 7;
            triangles[startTriangle + 19] = startVert + 8;
            triangles[startTriangle + 20] = startVert + 6;

            triangles[startTriangle + 21] = startVert + 0;
            triangles[startTriangle + 22] = startVert + 8;
            triangles[startTriangle + 23] = startVert + 7;

            for (int i = 0; i < 9; i++) {
                float x = (positions[startVert + i].x - position.x) / width;
                float y = 1 - ((positions[startVert + i].y + position.y) / -height);
                texCoord0[startVert + i] = new Vector4(x, y, x, y);
            }

            triangleList.size += 24;
            positionList.size += 9;
            texCoordList0.size += 9;
            texCoordList1.size += 9;
        }

        public void FillRect(float width, float height, in Vector2 position = default) {
            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            int[] triangles = triangleList.array;

            int startVert = positionList.size;
            int startTriangle = triangleList.size;

            ref Vector3 p0 = ref positions[startVert + 0];
            ref Vector3 p1 = ref positions[startVert + 1];
            ref Vector3 p2 = ref positions[startVert + 2];
            ref Vector3 p3 = ref positions[startVert + 3];

            ref Vector4 uv0 = ref texCoord0[startVert + 0];
            ref Vector4 uv1 = ref texCoord0[startVert + 1];
            ref Vector4 uv2 = ref texCoord0[startVert + 2];
            ref Vector4 uv3 = ref texCoord0[startVert + 3];

            p0.x = position.x + 0;
            p0.y = -position.y;
            p0.z = 0;

            p1.x = position.x + width;
            p1.y = -position.y;
            p1.z = 0;

            p2.x = position.x + width;
            p2.y = -(position.y + height);
            p2.z = 0;

            p3.x = position.x;
            p3.y = -(position.y + height);
            p3.z = 0;

            uv0.x = 0;
            uv0.y = 1;
            uv0.z = 0;
            uv0.w = 1;

            uv1.x = 1;
            uv1.y = 1;
            uv1.z = 1;
            uv1.w = 1;

            uv2.x = 1;
            uv2.y = 0;
            uv2.z = 1;
            uv2.w = 0;

            uv3.x = 0;
            uv3.y = 0;
            uv3.z = 0;
            uv3.w = 0;

            triangles[startTriangle + 0] = startVert + 0;
            triangles[startTriangle + 1] = startVert + 1;
            triangles[startTriangle + 2] = startVert + 2;
            triangles[startTriangle + 3] = startVert + 2;
            triangles[startTriangle + 4] = startVert + 3;
            triangles[startTriangle + 5] = startVert + 0;

            positionList.size += 4;
            texCoordList0.size += 4;
            texCoordList1.size += 4;
            triangleList.size += 6;
        }

        public void EnsureCapacity(int vertexCount, int triangleCount) {
            if (positionList.array.Length < vertexCount) {
                Array.Resize(ref positionList.array, vertexCount);
                Array.Resize(ref texCoordList0.array, vertexCount);
                Array.Resize(ref texCoordList1.array, vertexCount);
            }

            if (triangleList.array.Length < triangleCount) {
                Array.Resize(ref triangleList.array, triangleCount);
            }
        }

        
    }

}