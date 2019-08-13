using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace Vertigo {

    public class ShapeCache : Geometry {

        public StructList<GeometryShape> shapes;

        public ShapeCache(int capacity = 8) : base(8) {
            shapes = new StructList<GeometryShape>();
        }
        
        public int shapeCount => shapes.size;

        public override void ClearGeometryData() {
            shapes.QuickClear();
        }
        // todo -- remove this
        public GeometryShape FillRect(float x, float y, float width, float height, in RenderState renderState) {
            Color color = renderState.fillColor;
            EnsureAdditionalCapacity(4, 6);

            Vector3 p0 = new Vector3(x, -y);
            Vector3 p1 = new Vector3(x + width, -y);
            Vector3 p2 = new Vector3(x + width, -(y + height));
            Vector3 p3 = new Vector3(x, -(y + height));

            Vector3 n0 = new Vector3(0, 0, -1);

            int startVert = vertexCount;
            int startTriangle = triangleCount;

            Vector3[] positions = positionList.array;
            Vector3[] normals = normalList.array;
            Color[] colors = colorList.array;
            int[] triangles = triangleList.array;

            positions[startVert + 0] = p0;
            positions[startVert + 1] = p1;
            positions[startVert + 2] = p2;
            positions[startVert + 3] = p3;

            normals[startVert + 0] = n0;
            normals[startVert + 1] = n0;
            normals[startVert + 2] = n0;
            normals[startVert + 3] = n0;

            colors[startVert + 0] = color;
            colors[startVert + 1] = color;
            colors[startVert + 2] = color;
            colors[startVert + 3] = color;

            Vector4[] uvChannel = texCoordList0.array;

            if (uvChannel != null) {
                uvChannel[startVert + 0] = new Vector4(0, 1);
                uvChannel[startVert + 1] = new Vector4(1, 1);
                uvChannel[startVert + 2] = new Vector4(1, 0);
                uvChannel[startVert + 3] = new Vector4(0, 0);
            }

            triangles[startTriangle + 0] = 0;
            triangles[startTriangle + 1] = 1;
            triangles[startTriangle + 2] = 2;
            triangles[startTriangle + 3] = 2;
            triangles[startTriangle + 4] = 3;
            triangles[startTriangle + 5] = 0;

            UpdateSizes(4, 6);

            GeometryShape retn = new GeometryShape() {
                geometryType = GeometryType.Physical,
                shapeType = ShapeType.Rect,
                vertexStart = startVert,
                vertexCount = 4,
                triangleStart = startTriangle,
                triangleCount = 6
            };

            shapes.Add(retn);

            return retn;
        }
        
        public void StrokeRectNonUniform(float x, float y, float width, float height, in RenderState renderState, in OffsetStrokeRect offsets) {
            Color color = renderState.strokeColor;
            EnsureAdditionalCapacity(8, 24);

            Vector3 p0 = new Vector3(x, -y);
            Vector3 p1 = new Vector3(x + width, -y);
            Vector3 p2 = new Vector3(x + width, -(y + height));
            Vector3 p3 = new Vector3(x, -(y + height));
            Vector3 p0Inset = p0 + new Vector3(offsets.leftSize, -offsets.topSize);
            Vector3 p1Inset = p1 + new Vector3(-offsets.rightSize, -offsets.topSize);
            Vector3 p2Inset = p2 + new Vector3(-offsets.rightSize, offsets.bottomSize);
            Vector3 p3Inset = p3 + new Vector3(offsets.leftSize, offsets.bottomSize);

            Vector3[] positions = positionList.array;
            Vector3[] normals = normalList.array;
            Color[] colors = colorList.array;
            int[] triangles = triangleList.array;

            Vector3 n0 = new Vector3(0, 0, -1);

            int startVert = vertexCount;
            int startTriangle = triangleCount;

            const int p0_index = 0;
            const int p1_index = 1;
            const int p2_index = 2;
            const int p3_index = 3;
            const int p0_inset_index = 4;
            const int p1_inset_index = 5;
            const int p2_inset_index = 6;
            const int p3_inset_index = 7;

            positions[startVert + 0] = p0;
            positions[startVert + 1] = p1;
            positions[startVert + 2] = p2;
            positions[startVert + 3] = p3;
            positions[startVert + 4] = p0Inset;
            positions[startVert + 5] = p1Inset;
            positions[startVert + 6] = p2Inset;
            positions[startVert + 7] = p3Inset;

            normals[startVert + 0] = n0;
            normals[startVert + 1] = n0;
            normals[startVert + 2] = n0;
            normals[startVert + 3] = n0;
            normals[startVert + 4] = n0;
            normals[startVert + 5] = n0;
            normals[startVert + 6] = n0;
            normals[startVert + 7] = n0;

            // todo -- color this property, will need to duplicate vertices
            colors[startVert + 0] = color;
            colors[startVert + 1] = color;
            colors[startVert + 2] = color;
            colors[startVert + 3] = color;
            colors[startVert + 4] = color;
            colors[startVert + 5] = color;
            colors[startVert + 6] = color;
            colors[startVert + 7] = color;

            Vector4[] uvChannel = GetUVChannel(renderState.texCoordChannel);

            if (uvChannel != null) {
                uvChannel[startVert + 0] = new Vector4(0, 1);
                uvChannel[startVert + 1] = new Vector4(1, 1);
                uvChannel[startVert + 2] = new Vector4(1, 0);
                uvChannel[startVert + 3] = new Vector4(0, 0);
                // todo make sure this is correct
                uvChannel[startVert + 4] = new Vector4(p0Inset.x / width, p0Inset.y / height);
                uvChannel[startVert + 5] = new Vector4(p1Inset.x / width, p1Inset.y / height);
                uvChannel[startVert + 6] = new Vector4(p2Inset.x / width, p2Inset.y / height);
                uvChannel[startVert + 7] = new Vector4(p3Inset.x / width, p3Inset.y / height);
                ComputeUVTransforms(renderState, uvChannel, new RangeInt(startVert, 8));
            }

            // todo -- dont send triangles if not needed ie top is 0
            triangles[startTriangle + 0] = p0_index;
            triangles[startTriangle + 1] = p1_index;
            triangles[startTriangle + 2] = p0_inset_index;

            triangles[startTriangle + 3] = p1_index;
            triangles[startTriangle + 4] = p1_inset_index;
            triangles[startTriangle + 5] = p0_inset_index;

            triangles[startTriangle + 6] = p1_index;
            triangles[startTriangle + 7] = p2_index;
            triangles[startTriangle + 8] = p2_inset_index;

            triangles[startTriangle + 9] = p2_inset_index;
            triangles[startTriangle + 10] = p1_inset_index;
            triangles[startTriangle + 11] = p1_index;

            triangles[startTriangle + 12] = p2_index;
            triangles[startTriangle + 13] = p3_index;
            triangles[startTriangle + 14] = p3_inset_index;

            triangles[startTriangle + 15] = p2_index;
            triangles[startTriangle + 16] = p3_inset_index;
            triangles[startTriangle + 17] = p2_inset_index;

            triangles[startTriangle + 18] = p3_index;
            triangles[startTriangle + 19] = p0_index;
            triangles[startTriangle + 20] = p0_inset_index;

            triangles[startTriangle + 21] = p0_inset_index;
            triangles[startTriangle + 22] = p3_inset_index;
            triangles[startTriangle + 23] = p3_index;

            UpdateSizes(8, 24);

//            GeometryShape retn = new GeometryShape() {
//                geometryType = GeometryType.Physical,
//                shapeType = ShapeType.Rect,
//                vertexStart = startVert,
//                vertexCount = 8,
//                triangleStart = startTriangle,
//                triangleCount = 24
//            };
//
//            shapes.Add(retn);

//            return retn;
        }

        public void StrokeRect(float x, float y, float width, float height, in RenderState renderState) {
            Color color = renderState.strokeColor;
            EnsureAdditionalCapacity(8, 24);

            Vector3 p0 = new Vector3(x, -y);
            Vector3 p1 = new Vector3(x + width, -y);
            Vector3 p2 = new Vector3(x + width, -(y + height));
            Vector3 p3 = new Vector3(x, -(y + height));
            Vector3 p0Inset = p0 + new Vector3(renderState.strokeWidth, -renderState.strokeWidth);
            Vector3 p1Inset = p1 + new Vector3(-renderState.strokeWidth, -renderState.strokeWidth);
            Vector3 p2Inset = p2 + new Vector3(-renderState.strokeWidth, renderState.strokeWidth);
            Vector3 p3Inset = p3 + new Vector3(renderState.strokeWidth, renderState.strokeWidth);

            Vector3[] positions = positionList.array;
            Vector3[] normals = normalList.array;
            Color[] colors = colorList.array;
            int[] triangles = triangleList.array;

            Vector3 n0 = new Vector3(0, 0, -1);

            int startVert = vertexCount;
            int startTriangle = triangleCount;

            const int p0_index = 0;
            const int p1_index = 1;
            const int p2_index = 2;
            const int p3_index = 3;
            const int p0_inset_index = 4;
            const int p1_inset_index = 5;
            const int p2_inset_index = 6;
            const int p3_inset_index = 7;

            positions[startVert + 0] = p0;
            positions[startVert + 1] = p1;
            positions[startVert + 2] = p2;
            positions[startVert + 3] = p3;
            positions[startVert + 4] = p0Inset;
            positions[startVert + 5] = p1Inset;
            positions[startVert + 6] = p2Inset;
            positions[startVert + 7] = p3Inset;

            normals[startVert + 0] = n0;
            normals[startVert + 1] = n0;
            normals[startVert + 2] = n0;
            normals[startVert + 3] = n0;
            normals[startVert + 4] = n0;
            normals[startVert + 5] = n0;
            normals[startVert + 6] = n0;
            normals[startVert + 7] = n0;

            colors[startVert + 0] = color;
            colors[startVert + 1] = color;
            colors[startVert + 2] = color;
            colors[startVert + 3] = color;
            colors[startVert + 4] = color;
            colors[startVert + 5] = color;
            colors[startVert + 6] = color;
            colors[startVert + 7] = color;

            Vector4[] uvChannel = GetUVChannel(renderState.texCoordChannel);

            if (uvChannel != null) {
                uvChannel[startVert + 0] = new Vector4(0, 1);
                uvChannel[startVert + 1] = new Vector4(1, 1);
                uvChannel[startVert + 2] = new Vector4(1, 0);
                uvChannel[startVert + 3] = new Vector4(0, 0);
                uvChannel[startVert + 4] = new Vector4(0, 0);
                uvChannel[startVert + 5] = new Vector4(0, 0);
                uvChannel[startVert + 6] = new Vector4(0, 0);
                uvChannel[startVert + 7] = new Vector4(0, 0);
                ComputeUVTransforms(renderState, uvChannel, new RangeInt(startVert, 4));
            }

            triangles[startTriangle + 0] = p0_index;
            triangles[startTriangle + 1] = p1_index;
            triangles[startTriangle + 2] = p0_inset_index;

            triangles[startTriangle + 3] = p1_index;
            triangles[startTriangle + 4] = p1_inset_index;
            triangles[startTriangle + 5] = p0_inset_index;

            triangles[startTriangle + 6] = p1_index;
            triangles[startTriangle + 7] = p2_index;
            triangles[startTriangle + 8] = p2_inset_index;

            triangles[startTriangle + 9] = p2_inset_index;
            triangles[startTriangle + 10] = p1_inset_index;
            triangles[startTriangle + 11] = p1_index;

            triangles[startTriangle + 12] = p2_index;
            triangles[startTriangle + 13] = p3_index;
            triangles[startTriangle + 14] = p3_inset_index;

            triangles[startTriangle + 15] = p2_index;
            triangles[startTriangle + 16] = p3_inset_index;
            triangles[startTriangle + 17] = p2_inset_index;

            triangles[startTriangle + 18] = p3_index;
            triangles[startTriangle + 19] = p0_index;
            triangles[startTriangle + 20] = p0_inset_index;

            triangles[startTriangle + 21] = p0_inset_index;
            triangles[startTriangle + 22] = p3_inset_index;
            triangles[startTriangle + 23] = p3_index;

            UpdateSizes(8, 24);

//            GeometryShape retn = new GeometryShape() {
//                geometryType = GeometryType.Physical,
//                shapeType = ShapeType.Rect,
//                vertexStart = startVert,
//                vertexCount = 8,
//                triangleStart = startTriangle,
//                triangleCount = 24
//            };
//
//            shapes.Add(retn);

//            return retn;
        }

        
        private static void ComputeUVTransforms(in RenderState renderState, Vector4[] uvs, RangeInt range) {
            float tileX = renderState.uvTiling.x;
            float tileY = renderState.uvTiling.y;
            float offsetX = renderState.uvOffset.x;
            float offsetY = renderState.uvOffset.y;

            int start = range.start;
            int end = range.end;

            if (renderState.uvRotation == 0) {
                float minX = renderState.uvRect.x;
                float minY = renderState.uvRect.y;
                float width = renderState.uvRect.width;
                float height = renderState.uvRect.height;

                for (int i = start; i < end; i++) {
                    uvs[i].x = minX + (((uvs[i].x * tileX) + offsetX) * width);
                    uvs[i].y = minY + (((uvs[i].y * tileY) + offsetY) * height);
                }
            }
            else {
                float sin = math.sin(renderState.uvRotation * Mathf.Deg2Rad);
                float cos = math.cos(renderState.uvRotation * Mathf.Deg2Rad);
                float minX = renderState.uvRect.x;
                float minY = renderState.uvRect.y;
                float width = renderState.uvRect.width;
                float height = renderState.uvRect.height;
                float pivotX = renderState.uvPivot.x * tileX;
                float pivotY = renderState.uvPivot.y * tileY;

                for (int i = start; i < end; i++) {
                    float uvX = minX + (((uvs[i].x * tileX) + offsetX) * width) - pivotX;
                    float uvY = minY + (((uvs[i].y * tileY) + offsetY) * height) - pivotY;
                    uvs[i].x = pivotX + ((cos * uvX) - (sin * uvY));
                    uvs[i].y = pivotY + ((sin * uvX) + (cos * uvY));
                }
            }
        }

        private Vector4[] GetUVChannel(VertexChannel texCoordChannel) {
            switch (texCoordChannel) {
                case VertexChannel.TextureCoord0:
                    return texCoordList0.array;
                case VertexChannel.TextureCoord1:
                    return texCoordList1.array;
                case VertexChannel.TextureCoord2:
                    return texCoordList2.array;
                case VertexChannel.TextureCoord3:
                    return texCoordList3.array;
            }

            return null;
        }
        
    }

}