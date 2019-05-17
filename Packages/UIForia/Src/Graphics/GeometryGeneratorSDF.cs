using Unity.Mathematics;
using UnityEngine;

namespace Vertigo {

    public static class GeometryGeneratorSDF {

        public static void FillRect(Geometry geometry, float x, float y, float width, float height, in Color color, in UVTransform uvTransform) {
            geometry.EnsureAdditionalCapacity(4, 6);

            Vector3 p0;
            Vector3 p1;
            Vector3 p2;
            Vector3 p3;
            Vector3 n0;

            n0.x = 0;
            n0.y = 0;
            n0.z = -1;
            
            p0.x = x;
            p0.y = -y;
            p0.z = 0;
            
            p1.x = x + width;
            p1.y = -y;
            p1.z = 0;
            
            p2.x = x + width;
            p2.y = -(y + height);
            p2.z = 0;
            
            p3.x = x;
            p3.y = -(y + height);
            p3.z = 0;

            int startVert = geometry.positionList.size;
            int startTriangle = geometry.triangleList.size;

            Vector3[] positions = geometry.positionList.array;
            Vector3[] normals = geometry.normalList.array;
            Color[] colors = geometry.colorList.array;
            Vector4[] uvChannel = geometry.texCoordList0.array;
            int[] triangles = geometry.triangleList.array;

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

            Vector4 uv0;
            Vector4 uv1;
            Vector4 uv2;
            Vector4 uv3;
            uv0.x = 0;
            uv0.y = 1;
            uv0.z = 0;
            uv0.w = 0;
            uv1.x = 1;
            uv1.y = 1;
            uv1.z = 0;
            uv1.w = 0;
            uv2.x = 1;
            uv2.y = 0;
            uv2.z = 0;
            uv2.w = 0;
            uv3.x = 0;
            uv3.y = 0;
            uv3.z = 0;
            uv3.w = 0;
            uvChannel[startVert + 0] = uv0;
            uvChannel[startVert + 1] = uv1;
            uvChannel[startVert + 2] = uv2;
            uvChannel[startVert + 3] = uv3;

            if (uvTransform.enabled) {
                ComputeUVTransforms(uvTransform, uvChannel, new RangeInt(startVert, 4));
            }

            triangles[startTriangle + 0] = startVert + 0;
            triangles[startTriangle + 1] = startVert + 1;
            triangles[startTriangle + 2] = startVert + 2;
            triangles[startTriangle + 3] = startVert + 2;
            triangles[startTriangle + 4] = startVert + 3;
            triangles[startTriangle + 5] = startVert + 0;

            geometry.UpdateSizes(4, 6);
        }

        public static void FillRect(ShapeCache geometry, float x, float y, float width, float height, in Color color, in UVTransform uvTransform) {
            int vertexStart = geometry.vertexCount;
            int triangleStart = geometry.triangleCount;
            FillRect((Geometry) geometry, x, y, width, height, color, uvTransform);
            geometry.shapes.Add(new GeometryShape() {
                vertexStart = vertexStart,
                vertexCount = geometry.vertexCount - vertexStart,
                triangleStart = triangleStart,
                triangleCount = geometry.triangleCount - triangleStart
            });
        }

        public static void ComputeUVTransforms(in UVTransform renderState, Vector4[] uvs, RangeInt range) {
            float tileX = renderState.tilingX;
            float tileY = renderState.tilingY;
            float offsetX = renderState.offsetX;
            float offsetY = renderState.offsetY;

            int start = range.start;
            int end = range.end;

            // todo -- rect & tiling seem to be the same 
            if (renderState.rotation == 0) {
                float minX = renderState.rect.x;
                float minY = renderState.rect.y;
                float width = renderState.rect.width;
                float height = renderState.rect.height;

                for (int i = start; i < end; i++) {
                    uvs[i].x = minX + (((uvs[i].x * tileX) + offsetX) * width);
                    uvs[i].y = minY + (((uvs[i].y * tileY) + offsetY) * height);
                }
            }
            else {
                float sin = math.sin(renderState.rotation * Mathf.Deg2Rad);
                float cos = math.cos(renderState.rotation * Mathf.Deg2Rad);
                float minX = renderState.rect.x;
                float minY = renderState.rect.y;
                float width = renderState.rect.width;
                float height = renderState.rect.height;
                float pivotX = renderState.pivotX * tileX;
                float pivotY = renderState.pivotY * tileY;

                for (int i = start; i < end; i++) {
                    float uvX = minX + (((uvs[i].x * tileX) + offsetX) * width) - pivotX;
                    float uvY = minY + (((uvs[i].y * tileY) + offsetY) * height) - pivotY;
                    uvs[i].x = pivotX + ((cos * uvX) - (sin * uvY));
                    uvs[i].y = pivotY + ((sin * uvX) + (cos * uvY));
                }
            }
        }

    }

}