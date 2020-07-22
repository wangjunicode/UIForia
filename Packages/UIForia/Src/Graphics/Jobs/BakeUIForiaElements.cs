using System;
using System.Diagnostics;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics {

    [Flags]
    public enum MeshInfoType {

        Element = 1 << 0,
        Text = 1 << 1,
        Unity = 1 << 2,
        Geometry = 1 << 3

    }

    // needs to be cache line size since I build these in parallel
    [AssertSize(64)]
    public unsafe struct MeshInfo {

        public int meshId;
        public MeshInfoType type;

        public int vertexStart;
        public int vertexCount;

        public int triangleStart;
        public int triangleCount;

        public void* geometryData;
        public int* triangleData;
        private fixed byte padding[19];

    }

    [BurstCompile]
    internal unsafe struct BakeUIForiaShapes : IJob {

        public DataList<DrawInfo2>.Shared drawList;
        public DataList<int>.Shared triangleList;
        public DataList<UIForiaVertex>.Shared vertexList;
        public DataList<MeshInfo> meshInfoList;

        public void Execute() { }

    }

    [BurstCompile]
    internal unsafe struct BakeUIForiaElements : IJob {

        // todo -- make sure geometry type is at least 1 cache line in size 
        public DataList<DrawInfo2>.Shared drawList;
        public DataList<MeshInfo> meshInfoList;

        public DataList<UIForiaVertex>.Shared vertexList;
        public DataList<int>.Shared triangleList;

        [NativeDisableUnsafePtrRestriction] private float2* scratchFloats;
        private int vertexStartIndex;

        public void Execute() {

            scratchFloats = TypedUnsafe.Malloc<float2>(4, Allocator.Temp);

            List_float2 tempVertexBuffer = new List_float2(32, Allocator.Temp);

            int drawListSize = drawList.size;
            DrawInfo2* drawInfoArray = drawList.GetArrayPointer();
            MeshInfo* meshInfoArray = meshInfoList.GetArrayPointer();
            UIForiaVertex* vertices = null;

            // todo -- can pre-allocate vertex space since we now only support quads, this is easy
            for (int i = 0; i < drawListSize; i++) {

                ref DrawInfo2 drawInfo = ref drawInfoArray[i];

                if (drawInfo.drawType != DrawType2.UIForiaElement) {
                    continue;
                }

                ref MeshInfo meshInfo = ref meshInfoArray[i];

                meshInfo.type = MeshInfoType.Element;

                meshInfo.triangleStart = triangleList.size;
                meshInfo.vertexStart = vertexList.size;

                SDFMeshDesc* desc = (SDFMeshDesc*) drawInfo.shapeData;
                vertexStartIndex = 0;

                float x = desc->x;
                float y = desc->y;
                float width = desc->width;
                float height = desc->height;

                // should never happen but avoids a div by 0 just in case
                if (width <= 0 || height <= 0) {
                    meshInfo.vertexCount = 0;
                    meshInfo.triangleCount = 0;
                    continue;
                }

                // todo -- for proper pivot I probably want to create mesh with pivot position in mind
                vertexList.EnsureAdditionalCapacity(4);
                triangleList.EnsureAdditionalCapacity(6);
                vertices = vertexList.GetArrayPointer();
                int* triangles = triangleList.GetArrayPointer();

                int vertexStart = vertexList.size;
                int triangleStart = triangleList.size;

                vertices[vertexStart + 0] = new UIForiaVertex(x, y); // tl
                vertices[vertexStart + 1] = new UIForiaVertex(x + width, y); // tr
                vertices[vertexStart + 2] = new UIForiaVertex(x + width, -(y + height)); // br
                vertices[vertexStart + 3] = new UIForiaVertex(x, -(y + height)); // bl

                triangles[triangleStart + 0] = vertexStart + 0;
                triangles[triangleStart + 1] = vertexStart + 1;
                triangles[triangleStart + 2] = vertexStart + 2;
                triangles[triangleStart + 3] = vertexStart + 2;
                triangles[triangleStart + 4] = vertexStart + 3;
                triangles[triangleStart + 5] = vertexStart + 0;

                vertexList.size += 4;
                triangleList.size += 6;

                // todo -- i dont think this is quite right yet, triangles might be off
                // GenerateFilledSprite(
                //     new float4(x, -desc->height, desc->width, y),
                //     desc->meshStyle.meshType,
                //     (int) desc->meshStyle.fillOrigin,
                //     desc->meshStyle.fillAmount,
                //     desc->meshStyle.fillDirection == MeshFillDirection.Clockwise
                // );

                meshInfo.vertexCount = vertexList.size - meshInfo.vertexStart;
                meshInfo.triangleCount = triangleList.size - meshInfo.triangleStart;
                int vEnd = meshInfo.vertexStart + meshInfo.vertexCount;

                vertices = vertexList.GetArrayPointer();

                // todo -- account for uv rect, background fit, etc
                for (int v = meshInfo.vertexStart; v < vEnd; v++) {
                    ref UIForiaVertex vertex = ref vertices[v];
                    float uvX = ((vertex.position.x) / width);
                    float uvY = 1 - ((vertex.position.y) / -height);
                    int hSign = vertex.position.x == 0 ? -1 : 1;
                    int vSign = vertex.position.y == 0 ? 1 : -1;
                    vertex.texCoord0.x = uvX + ((0.5f / width) * hSign);
                    vertex.texCoord0.y = uvY + ((0.5f / height) * vSign);

                    vertex.texCoord1.x = uvX;
                    vertex.texCoord1.y = uvY;
                }

            }

            TypedUnsafe.Dispose(scratchFloats, Allocator.Temp);
            tempVertexBuffer.Dispose();
        }

        private void GenerateFilledSprite(float4 v, MeshType fillMethod, int fillOrigin, float fillAmount, bool fillClockwise) {

            if (fillAmount < 0.001f) {
                return;
            }

            if (fillAmount >= 1) {
                fillMethod = MeshType.Simple;
            }

            if (fillOrigin > 3) fillOrigin = 0;

            switch (fillMethod) {
                default:
                case MeshType.Simple: {
                    vertexList.EnsureAdditionalCapacity(4);
                    triangleList.EnsureAdditionalCapacity(6);

                    int vertexStart = vertexList.size;

                    vertexList.AddUnchecked(new UIForiaVertex(v.x, v.y));
                    vertexList.AddUnchecked(new UIForiaVertex(v.x, v.w));
                    vertexList.AddUnchecked(new UIForiaVertex(v.z, v.w));
                    vertexList.AddUnchecked(new UIForiaVertex(v.z, v.y));

                    triangleList.AddUnchecked(vertexStart + 0);
                    triangleList.AddUnchecked(vertexStart + 1);
                    triangleList.AddUnchecked(vertexStart + 2);
                    triangleList.AddUnchecked(vertexStart + 2);
                    triangleList.AddUnchecked(vertexStart + 3);
                    triangleList.AddUnchecked(vertexStart + 0);

                    return;
                }

                case MeshType.FillRadial90: {

                    scratchFloats[0] = new float2(v.x, v.y);
                    scratchFloats[1] = new float2(v.x, v.w);
                    scratchFloats[2] = new float2(v.z, v.w);
                    scratchFloats[3] = new float2(v.z, v.y);

                    RadialCut(fillAmount, fillClockwise, fillOrigin);
                    AddQuad();
                    return;
                }

                case MeshType.FillRadial180: {
                    for (int side = 0; side < 2; side++) {
                        float fx0, fx1, fy0, fy1;
                        int even = fillOrigin > 1 ? 1 : 0;

                        if (fillOrigin == 0 || fillOrigin == 2) {
                            fy0 = 0f;
                            fy1 = 1f;
                            if (side == even) {
                                fx0 = 0f;
                                fx1 = 0.5f;
                            }
                            else {
                                fx0 = 0.5f;
                                fx1 = 1f;
                            }
                        }
                        else {
                            fx0 = 0f;
                            fx1 = 1f;
                            if (side == even) {
                                fy0 = 0.5f;
                                fy1 = 1f;
                            }
                            else {
                                fy0 = 0f;
                                fy1 = 0.5f;
                            }
                        }

                        scratchFloats[0].x = math.lerp(v.x, v.z, fx0);
                        scratchFloats[0].y = math.lerp(v.y, v.w, fy0);

                        scratchFloats[1].x = scratchFloats[0].x;
                        scratchFloats[1].y = math.lerp(v.y, v.w, fy1);

                        scratchFloats[2].x = math.lerp(v.x, v.z, fx1);
                        scratchFloats[2].y = scratchFloats[1].y;

                        scratchFloats[3].x = scratchFloats[2].x;
                        scratchFloats[3].y = scratchFloats[0].y;

                        float val = fillClockwise ? fillAmount * 2f - side : fillAmount * 2f - (1 - side);

                        RadialCut(Mathf.Clamp01(val), fillClockwise, ((side + fillOrigin + 3) % 4));
                        AddQuad();
                    }

                    return;
                }

                case MeshType.FillRadial360: {
                    for (int corner = 0; corner < 4; ++corner) {
                        float fx0, fx1, fy0, fy1;

                        if (corner < 2) {
                            fx0 = 0f;
                            fx1 = 0.5f;
                        }
                        else {
                            fx0 = 0.5f;
                            fx1 = 1f;
                        }

                        if (corner == 0 || corner == 3) {
                            fy0 = 0f;
                            fy1 = 0.5f;
                        }
                        else {
                            fy0 = 0.5f;
                            fy1 = 1f;
                        }

                        scratchFloats[0].x = math.lerp(v.x, v.z, fx0);
                        scratchFloats[0].y = math.lerp(v.y, v.w, fy0);

                        scratchFloats[1].x = scratchFloats[0].x;
                        scratchFloats[1].y = math.lerp(v.y, v.w, fy1);

                        scratchFloats[2].x = math.lerp(v.x, v.z, fx1);
                        scratchFloats[2].y = scratchFloats[1].y;

                        scratchFloats[3].x = scratchFloats[2].x;
                        scratchFloats[3].y = scratchFloats[0].y;

                        float val = fillClockwise
                            ? fillAmount * 4f - ((corner + fillOrigin) % 4)
                            : fillAmount * 4f - (3 - ((corner + fillOrigin) % 4));

                        RadialCut(Mathf.Clamp01(val), fillClockwise, ((corner + 2) % 4));
                        AddQuad();
                    }

                    return;
                }

                case MeshType.FillHorizontal: {
                    if (fillOrigin == 1) {
                        v.x = v.z - (v.z - v.x) * fillAmount;
                    }
                    else {
                        v.z = v.x + (v.z - v.x) * fillAmount;
                    }

                    scratchFloats[0] = new float2(v.x, v.y);
                    scratchFloats[1] = new float2(v.x, v.w);
                    scratchFloats[2] = new float2(v.z, v.w);
                    scratchFloats[3] = new float2(v.z, v.y);
                    AddQuad();
                    return;
                }

                case MeshType.FillVertical: {

                    if (fillOrigin == 1) {
                        v.y = v.w - (v.w - v.y) * fillAmount;
                    }
                    else {
                        v.w = v.y + (v.w - v.y) * fillAmount;
                    }

                    scratchFloats[0] = new float2(v.x, v.y);
                    scratchFloats[1] = new float2(v.x, v.w);
                    scratchFloats[2] = new float2(v.z, v.w);
                    scratchFloats[3] = new float2(v.z, v.y);
                    AddQuad();
                    return;
                }

            }

        }

        private void AddQuad() {

            vertexList.EnsureAdditionalCapacity(4);
            triangleList.EnsureAdditionalCapacity(6);

            vertexList[vertexList.size++] = new UIForiaVertex(scratchFloats[0].x, scratchFloats[0].y);
            vertexList[vertexList.size++] = new UIForiaVertex(scratchFloats[1].x, scratchFloats[1].y);
            vertexList[vertexList.size++] = new UIForiaVertex(scratchFloats[2].x, scratchFloats[2].y);
            vertexList[vertexList.size++] = new UIForiaVertex(scratchFloats[3].x, scratchFloats[3].y);

            int tri = triangleList.size;

            triangleList[tri + 0] = vertexStartIndex + 0;
            triangleList[tri + 1] = vertexStartIndex + 1;
            triangleList[tri + 2] = vertexStartIndex + 2;

            triangleList[tri + 3] = vertexStartIndex + 2;
            triangleList[tri + 4] = vertexStartIndex + 3;
            triangleList[tri + 5] = vertexStartIndex + 0;

            vertexStartIndex += 4;
            triangleList.size += 6;

        }

        /// <summary>
        /// Adjust the specified quad, making it be radially filled instead.
        /// </summary>
        private void RadialCut(float fill, bool invert, int corner) {

            // Even corners invert the fill direction
            if ((corner & 1) == 1) invert = !invert;

            // Nothing to adjust
            if (!invert && fill > 0.999f) return;

            // Convert 0-1 value into 0 to 90 degrees angle in radians
            float angle = Mathf.Clamp01(fill);
            if (invert) angle = 1f - angle;
            angle *= 90f * Mathf.Deg2Rad;

            // Calculate the effective X and Y factors
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            RadialCut(cos, sin, invert, corner);

        }

        /// <summary>
        /// Adjust the specified quad, making it be radially filled instead.
        /// </summary>
        private void RadialCut(float cos, float sin, bool invert, int corner) {
            int i0 = corner;
            int i1 = ((corner + 1) % 4);
            int i2 = ((corner + 2) % 4);
            int i3 = ((corner + 3) % 4);

            if ((corner & 1) == 1) {
                if (sin > cos) {
                    cos /= sin;
                    sin = 1f;

                    if (invert) {
                        scratchFloats[i1].x = math.lerp(scratchFloats[i0].x, scratchFloats[i2].x, cos);
                        scratchFloats[i2].x = scratchFloats[i1].x;
                    }
                }
                else if (cos > sin) {
                    sin /= cos;
                    cos = 1f;

                    if (!invert) {
                        scratchFloats[i2].y = math.lerp(scratchFloats[i0].y, scratchFloats[i2].y, sin);
                        scratchFloats[i3].y = scratchFloats[i2].y;
                    }
                }
                else {
                    cos = 1f;
                    sin = 1f;
                }

                if (!invert) scratchFloats[i3].x = math.lerp(scratchFloats[i0].x, scratchFloats[i2].x, cos);
                else scratchFloats[i1].y = math.lerp(scratchFloats[i0].y, scratchFloats[i2].y, sin);
            }
            else {
                if (cos > sin) {
                    sin /= cos;
                    cos = 1f;

                    if (!invert) {
                        scratchFloats[i1].y = math.lerp(scratchFloats[i0].y, scratchFloats[i2].y, sin);
                        scratchFloats[i2].y = scratchFloats[i1].y;
                    }
                }
                else if (sin > cos) {
                    cos /= sin;
                    sin = 1f;

                    if (invert) {
                        scratchFloats[i2].x = math.lerp(scratchFloats[i0].x, scratchFloats[i2].x, cos);
                        scratchFloats[i3].x = scratchFloats[i2].x;
                    }
                }
                else {
                    cos = 1f;
                    sin = 1f;
                }

                if (invert) scratchFloats[i3].y = math.lerp(scratchFloats[i0].y, scratchFloats[i2].y, sin);
                else scratchFloats[i1].x = math.lerp(scratchFloats[i0].x, scratchFloats[i2].x, cos);
            }
        }

    }

}