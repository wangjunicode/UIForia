using System;
using System.Collections.Generic;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public class UIForiaGeometry {

        public Color packedColors;
        public Vector4 objectData;
        public StructList<Vector3> positionList;
        public StructList<Vector4> texCoordList0;
        public StructList<Vector4> texCoordList1;
        public StructList<int> triangleList;
        public Texture mainTexture;


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

        public struct CornerDef {

            public float topLeftX;
            public float topLeftY;
            public float topRightX;
            public float topRightY;
            public float bottomLeftX;
            public float bottomLeftY;
            public float bottomRightX;
            public float bottomRightY;
            
        }

        public void ClipCornerRect(Size size, in CornerDef cornerDef) {
            EnsureAdditionalCapacity(9, 24);
            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            Vector4[] texCoord1 = texCoordList1.array;
            int[] triangles = triangleList.array;

            int startVert = positionList.size;
            int startTriangle = triangleList.size;

            float width = size.width;
            float height = size.height;
            
            positions[startVert + 0] = new Vector3(0, -cornerDef.topLeftY, 0);
            positions[startVert + 1] = new Vector3(cornerDef.topLeftX, 0, 0);
            positions[startVert + 2] = new Vector3(width - cornerDef.topRightX, 0, 0);
            positions[startVert + 3] = new Vector3(width, -cornerDef.topRightY, 0);
            positions[startVert + 4] = new Vector3(width, -(height - cornerDef.bottomRightY), 0);
            positions[startVert + 5] = new Vector3(width - cornerDef.bottomRightX, -height, 0);
            positions[startVert + 6] = new Vector3(cornerDef.bottomLeftX, -height, 0);
            positions[startVert + 7] = new Vector3(0, -(height - cornerDef.bottomLeftY), 0);
            positions[startVert + 8] = new Vector3(width * 0.5f, -height * 0.5f, 0);

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
                float x = positions[startVert + i].x / width;
                float y = 1 - (positions[startVert + i].y / -height);
                texCoord0[startVert + i] = new Vector4(x, y, x, y);
            }
            
            // x = border color 0
            // y = border color 1
            // z = distance to edge (0 in all but center)
            // w = object index
           
            for (int i = 0; i < 8; i++) {
                texCoord1[startVert + i].z = 0;// = new Vector4(x, y, width, height);
            }
            
            triangleList.size += 24;
            positionList.size += 9;
            texCoordList0.size += 9;
            texCoordList1.size += 9;
        }
        
        public void ClipCornerRectWithFill(Vector2 fillOrigin, float angle, Size size, in CornerDef cornerDef) {
            EnsureAdditionalCapacity(9, 24);
            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            Vector4[] texCoord1 = texCoordList1.array;
            int[] triangles = triangleList.array;

            int startVert = positionList.size;
            int startTriangle = triangleList.size;

            float width = size.width;
            float height = size.height;
            
            positions[startVert + 0] = new Vector3(0, -cornerDef.topLeftY, 0);
            positions[startVert + 1] = new Vector3(cornerDef.topLeftX, 0, 0);
            positions[startVert + 2] = new Vector3(width - cornerDef.topRightX, 0, 0);
            positions[startVert + 3] = new Vector3(width, -cornerDef.topRightY, 0);
            positions[startVert + 4] = new Vector3(width, -(height - cornerDef.bottomRightY), 0);
            positions[startVert + 5] = new Vector3(width - cornerDef.bottomRightX, -height, 0);
            positions[startVert + 6] = new Vector3(cornerDef.bottomLeftX, -height, 0);
            positions[startVert + 7] = new Vector3(0, -(height - cornerDef.bottomLeftY), 0);
            positions[startVert + 8] = new Vector3(width * 0.5f, -height * 0.5f, 0);
            
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
                float x = positions[startVert + i].x / width;
                float y = positions[startVert + i].y / height;
                texCoord0[startVert + i] = new Vector4(x, y, width, height);
            }
            
            triangleList.size += 24;
            positionList.size += 9;
            texCoordList0.size += 9;
            texCoordList1.size += 9;
        }

        public void FillRectUniformBorder_MixedCorners(float width, float height) {
            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            Vector4[] texCoord1 = texCoordList1.array;
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

            ref Vector4 data0 = ref texCoord1[startVert + 0];
            ref Vector4 data1 = ref texCoord1[startVert + 1];
            ref Vector4 data2 = ref texCoord1[startVert + 2];
            ref Vector4 data3 = ref texCoord1[startVert + 3];

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

        public void FillRectUniformBorder_Miter(float width, float height) {
            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            Vector4[] texCoord1 = texCoordList1.array;
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

            ref Vector4 data0 = ref texCoord1[startVert + 0];
            ref Vector4 data1 = ref texCoord1[startVert + 1];
            ref Vector4 data2 = ref texCoord1[startVert + 2];
            ref Vector4 data3 = ref texCoord1[startVert + 3];

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

        public static Mesh CreateQuadMesh(float width, float height) {
            Vector3[] positions = new Vector3[4];
            Vector4[] texCoord0 = new Vector4[4];
            int[] triangles = new int[6];

            int startVert = 0;
            int startTriangle = 0;

            ref Vector3 p0 = ref positions[0];
            ref Vector3 p1 = ref positions[1];
            ref Vector3 p2 = ref positions[2];
            ref Vector3 p3 = ref positions[3];

            ref Vector4 uv0 = ref texCoord0[0];
            ref Vector4 uv1 = ref texCoord0[1];
            ref Vector4 uv2 = ref texCoord0[2];
            ref Vector4 uv3 = ref texCoord0[3];

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

            Mesh retn = new Mesh();
            retn.vertices = positions;
            retn.SetUVs(0, new List<Vector4>(texCoord0));

            return retn;
        }

    }

}