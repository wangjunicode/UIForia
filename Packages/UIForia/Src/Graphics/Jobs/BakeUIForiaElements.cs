using System;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Graphics {

    [Flags]
    public enum MeshInfoType {

        Element = 1 << 0,
        Text = 1 << 1,
        Unity = 1 << 2,
        Geometry = 1 << 3,

        Shadow

    }

    // needs to be cache line size since I build these in parallel (avoid false sharing)
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

    [Flags]
    public enum ElementShaderFlags {

        IsNineSliceBorder = 1 << 0,
        IsNineSliceContent = 1 << 1

    }

    [BurstCompile]
    internal unsafe struct BakeUIForiaElements : IJob {

        public DataList<MeshInfo> meshInfoList;
        public DataList<DrawInfo2>.Shared drawList;
        public DataList<UIForiaVertex>.Shared vertexList;
        public DataList<float4>.Shared float4Buffer;
        public DataList<ElementMaterialInfo>.Shared materialList;

        public void Execute() {

            int drawListSize = drawList.size;
            DrawInfo2* drawInfoArray = drawList.GetArrayPointer();
            MeshInfo* meshInfoArray = meshInfoList.GetArrayPointer();

            // its only 1 entry per element, we wont have thousands, and if we do most of them are probably elements anyway

            int vertexIdx = 0;

            IntMap<int> borderColorMap = new IntMap<int>(32, Allocator.Temp);

            int maxVertexCount = 0;

            for (int i = 0; i < drawListSize; i++) {

                ref DrawInfo2 drawInfo = ref drawInfoArray[i];

                if (drawInfo.drawType != DrawType2.UIForiaElement && drawInfo.drawType != DrawType2.UIForiaShadow) {
                    continue;
                }

                ElementBatch* elementBatch = (ElementBatch*) drawInfo.shapeData;
                ref ElementDrawInfo element = ref elementBatch->elements[0];
                if (element.isSliced) {
                    maxVertexCount += 9;
                }
                else {
                    maxVertexCount++;
                }

            }

            vertexList.EnsureAdditionalCapacity(maxVertexCount);
            UIForiaVertex* vertices = vertexList.GetArrayPointer();

            for (int i = 0; i < drawListSize; i++) {

                ref DrawInfo2 drawInfo = ref drawInfoArray[i];

                if (drawInfo.drawType != DrawType2.UIForiaElement && drawInfo.drawType != DrawType2.UIForiaShadow) {
                    continue;
                }

                ref MeshInfo meshInfo = ref meshInfoArray[i];
                ElementBatch* elementBatch = (ElementBatch*) drawInfo.shapeData;

                int count = elementBatch->count;

                if (count == 1) {

                    meshInfo.type = MeshInfoType.Element;
                    meshInfo.vertexStart = vertexIdx;
                    meshInfo.vertexCount = 1;

                    int materialIdx = materialList.size;
                    // FindIdx(hash, ptr, materialList);
                    // materialList.Add();
                    // want to collect data in the material desc but then extract a lot of it to per-element data to cut down on upload 
                    // otherwise too much of the abstraction leaks out, its nice having one material per element / draw
                    // can diff this on texture but no point doing it more than that

                    ref ElementDrawInfo element = ref elementBatch->elements[0];
                    float x = element.x;
                    float y = element.y;
                    float width = element.drawDesc.width;
                    float height = element.drawDesc.height;
                    float halfWidth = width * 0.5f;
                    float halfHeight = height * 0.5f;

                    ElementMaterialInfo mat = default;

                    mat.backgroundColor = element.drawDesc.backgroundColor;
                    mat.backgroundTint = element.drawDesc.backgroundTint;

                    mat.radius0 = element.drawDesc.radiusTL;
                    mat.radius1 = element.drawDesc.radiusTR;
                    mat.radius2 = element.drawDesc.radiusBR;
                    mat.radius3 = element.drawDesc.radiusBL;

                    mat.bevelTL = element.drawDesc.bevelTL;
                    mat.bevelTR = element.drawDesc.bevelTR;
                    mat.bevelBR = element.drawDesc.bevelBR;
                    mat.bevelBL = element.drawDesc.bevelBL;

                    mat.bodyColorMode = element.drawDesc.bgColorMode;
                    mat.outlineColorMode = element.drawDesc.outlineColorMode;

                    mat.outlineWidth = element.drawDesc.outlineWidth;
                    mat.outlineColor = element.drawDesc.outlineColor;

                    mat.fillRotation = element.drawDesc.meshFillRotation;
                    mat.fillRadius = element.drawDesc.meshFillRadius;
                    mat.fillOpenAmount = element.drawDesc.meshFillOpenAmount;
                    mat.fillOffsetX = element.drawDesc.meshFillOffsetX;
                    mat.fillOffsetY = element.drawDesc.meshFillOffsetY;
                    mat.fillInvert = element.drawDesc.meshFillInvert;
                    mat.fillDirection = element.drawDesc.meshFillDirection;
                    mat.opacity = element.drawDesc.opacity;
                    mat.uvRotation = element.drawDesc.uvRotation;

                    mat.borderColorTop = element.drawDesc.borderColorTop;
                    mat.borderColorRight = element.drawDesc.borderColorRight;
                    mat.borderColorBottom = element.drawDesc.borderColorBottom;
                    mat.borderColorLeft = element.drawDesc.borderColorLeft;

                    mat.maskFlags = (uint)element.drawDesc.maskFlags;
                    mat.maskSoftness = element.drawDesc.maskSoftness;
                    mat.maskTopLeftUV = element.drawDesc.maskTopLeftUV;
                    mat.maskBottomRightUV = element.drawDesc.maskBottomRightUV;

                    if (element.drawDesc.HasBorder) {
                        mat.borderIndex = (uint) float4Buffer.size;
                        float4Buffer.Add(new float4() {
                            x = element.drawDesc.borderTop < halfHeight ? element.drawDesc.borderTop : halfHeight,
                            y = element.drawDesc.borderRight < halfWidth ? element.drawDesc.borderRight : halfWidth,
                            z = element.drawDesc.borderBottom < halfHeight ? element.drawDesc.borderBottom : halfHeight,
                            w = element.drawDesc.borderLeft < halfWidth ? element.drawDesc.borderLeft : halfWidth
                        });
                    }

                    if (element.isSliced) {
                        meshInfo.vertexCount = 9;
                        ElementShaderFlags flags = ElementShaderFlags.IsNineSliceBorder;

                        ushort uvBorderTop = element.uvBorderTop;
                        ushort uvBorderRight = element.uvBorderRight;
                        ushort uvBorderBottom = element.uvBorderBottom;
                        ushort uvBorderLeft = element.uvBorderLeft;

                        ref UIForiaVertex vertexTopLeft = ref vertices[vertexIdx++];
                        ref UIForiaVertex vertexTopCenter = ref vertices[vertexIdx++];
                        ref UIForiaVertex vertexTopRight = ref vertices[vertexIdx++];
                        ref UIForiaVertex vertexCenterLeft = ref vertices[vertexIdx++];
                        ref UIForiaVertex vertexBottomLeft = ref vertices[vertexIdx++];
                        ref UIForiaVertex vertexBottomCenter = ref vertices[vertexIdx++];
                        ref UIForiaVertex vertexBottomRight = ref vertices[vertexIdx++];
                        ref UIForiaVertex vertexCenterRight = ref vertices[vertexIdx++];

                        vertexTopLeft.position.x = x + (element.drawDesc.borderLeft * 0.5f);
                        vertexTopLeft.position.y = -(y + (element.drawDesc.borderTop * 0.5f));
                        vertexTopLeft.texCoord0.x = element.drawDesc.borderLeft;
                        vertexTopLeft.texCoord0.y = element.drawDesc.borderTop;
                        vertexTopLeft.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));

                        vertexTopLeft.indices.z = BitUtil.SetHighLowBitsUint((uint) (element.drawDesc.uvBottom - uvBorderBottom), element.drawDesc.uvLeft);
                        vertexTopLeft.indices.w = BitUtil.SetHighLowBitsUint(
                            element.drawDesc.uvBottom,
                            (uint) (element.drawDesc.uvLeft + uvBorderLeft)
                        );

                        vertexTopCenter.position.x = x + (halfWidth);
                        vertexTopCenter.position.y = -(y + (element.drawDesc.borderTop * 0.5f));
                        vertexTopCenter.texCoord0.x = element.drawDesc.width - element.drawDesc.borderLeft - element.drawDesc.borderRight;
                        vertexTopCenter.texCoord0.y = element.drawDesc.borderTop;
                        vertexTopCenter.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                        vertexTopCenter.indices.z = BitUtil.SetHighLowBitsUint((uint) (element.drawDesc.uvBottom - uvBorderBottom), (uint) (element.drawDesc.uvLeft + uvBorderLeft));
                        vertexTopCenter.indices.w = BitUtil.SetHighLowBitsUint(
                            element.drawDesc.uvBottom,
                            (uint) (element.drawDesc.uvRight - uvBorderRight)
                        );

                        vertexTopRight.position.x = x + (element.drawDesc.width - (0.5f * element.drawDesc.borderRight));
                        vertexTopRight.position.y = -(y + (element.drawDesc.borderTop * 0.5f));
                        vertexTopRight.texCoord0.x = element.drawDesc.borderRight;
                        vertexTopRight.texCoord0.y = element.drawDesc.borderTop;
                        vertexTopRight.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                        vertexTopRight.indices.z = BitUtil.SetHighLowBitsUint(
                            (uint)(element.drawDesc.uvBottom - uvBorderBottom),
                            (uint) (element.drawDesc.uvRight - uvBorderRight)
                        );
                        vertexTopRight.indices.w = BitUtil.SetHighLowBitsUint(
                            element.drawDesc.uvBottom,
                            element.drawDesc.uvRight
                        );

                        vertexCenterLeft.position.x = x + (element.drawDesc.borderLeft * 0.5f);
                        vertexCenterLeft.position.y = -(y + (element.drawDesc.height * 0.5f));
                        vertexCenterLeft.texCoord0.x = element.drawDesc.borderLeft;
                        vertexCenterLeft.texCoord0.y = element.drawDesc.height - (element.drawDesc.borderTop + element.drawDesc.borderBottom);
                        vertexCenterLeft.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                        vertexCenterLeft.indices.z = BitUtil.SetHighLowBitsUint(
                            (uint) (element.drawDesc.uvTop + uvBorderTop),
                            element.drawDesc.uvLeft
                        );
                        vertexCenterLeft.indices.w = BitUtil.SetHighLowBitsUint(
                            (uint) (element.drawDesc.uvBottom - uvBorderBottom),
                            (uint) (element.drawDesc.uvLeft + uvBorderLeft)
                        );

                        vertexBottomLeft.position.x = x + (element.drawDesc.borderLeft * 0.5f);
                        vertexBottomLeft.position.y = -(y + element.drawDesc.height - element.drawDesc.borderTop + (0.5f * element.drawDesc.borderBottom));
                        vertexBottomLeft.texCoord0.x = element.drawDesc.borderLeft;
                        vertexBottomLeft.texCoord0.y = element.drawDesc.borderBottom;
                        vertexBottomLeft.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                        vertexBottomLeft.indices.z = BitUtil.SetHighLowBitsUint(
                            element.drawDesc.uvTop,
                            element.drawDesc.uvLeft
                        );
                        vertexBottomLeft.indices.w = BitUtil.SetHighLowBitsUint(
                            (uint)(element.drawDesc.uvTop + uvBorderTop),
                            (uint) (element.drawDesc.uvLeft + uvBorderLeft)
                        );

                        vertexBottomCenter.position.x = x + halfWidth;
                        vertexBottomCenter.position.y = -(y + element.drawDesc.height - element.drawDesc.borderTop + (0.5f * element.drawDesc.borderBottom));
                        vertexBottomCenter.texCoord0.x = element.drawDesc.width - element.drawDesc.borderLeft - element.drawDesc.borderRight;
                        vertexBottomCenter.texCoord0.y = element.drawDesc.borderBottom;
                        vertexBottomCenter.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                        vertexBottomCenter.indices.z = BitUtil.SetHighLowBitsUint(
                            element.drawDesc.uvTop,
                            (uint) (element.drawDesc.uvLeft + uvBorderLeft)
                        );
                        vertexBottomCenter.indices.w = BitUtil.SetHighLowBitsUint(
                            (uint)(element.drawDesc.uvTop + uvBorderTop),
                            (uint) (element.drawDesc.uvRight - uvBorderRight)
                        );

                        vertexBottomRight.position.x = x + element.drawDesc.width - (0.5f * element.drawDesc.borderRight);
                        vertexBottomRight.position.y = -(y + element.drawDesc.height - element.drawDesc.borderTop + (0.5f * element.drawDesc.borderBottom));
                        vertexBottomRight.texCoord0.x = element.drawDesc.borderRight;
                        vertexBottomRight.texCoord0.y = element.drawDesc.borderBottom;
                        vertexBottomRight.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                        vertexBottomRight.indices.z = BitUtil.SetHighLowBitsUint(
                            element.drawDesc.uvTop,
                            (uint) (element.drawDesc.uvRight - uvBorderRight)
                        );
                        vertexBottomRight.indices.w = BitUtil.SetHighLowBitsUint(
                            (uint)(element.drawDesc.uvTop + uvBorderTop),
                            element.drawDesc.uvRight
                        );

                        vertexCenterRight.position.x = x + element.drawDesc.width - (element.drawDesc.borderRight * 0.5f);
                        vertexCenterRight.position.y = -(y + halfHeight);
                        vertexCenterRight.texCoord0.x = element.drawDesc.borderRight;
                        vertexCenterRight.texCoord0.y = element.drawDesc.height - (element.drawDesc.borderTop + element.drawDesc.borderBottom);
                        vertexCenterRight.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                        vertexCenterRight.indices.z = BitUtil.SetHighLowBitsUint(
                            (uint) (element.drawDesc.uvBottom - uvBorderBottom),
                            (uint) (element.drawDesc.uvRight - uvBorderRight)
                        );
                        vertexCenterRight.indices.w = BitUtil.SetHighLowBitsUint(
                            (uint) (element.drawDesc.uvTop + uvBorderTop),
                            element.drawDesc.uvRight
                        );

                        uint uvTransformId = 0;
                        if (element.drawDesc.uvScaleX != 1 || element.drawDesc.uvScaleY != 1 || element.drawDesc.uvOffsetX != 0 || element.drawDesc.uvOffsetY != 0) {
                            uvTransformId = (uint) float4Buffer.size;
                            float4Buffer.Add(new float4() {
                                x = element.drawDesc.uvScaleX,
                                y = element.drawDesc.uvScaleY,
                                z = element.drawDesc.uvOffsetX,
                                w = element.drawDesc.uvOffsetY
                            });
                        }

                        mat.uvTransformId = uvTransformId;
                        mat.borderIndex = (uint) float4Buffer.size;
                        materialList.Add(mat);

                        float4Buffer.Add(new float4(x + halfWidth, y + halfHeight, width, height));

                        flags = ElementShaderFlags.IsNineSliceContent;
                        ref UIForiaVertex vertex = ref vertices[vertexIdx++];
                        vertex.position.x = x + halfWidth;
                        vertex.position.y = -(y + halfHeight);
                        vertex.texCoord0.x = width - ((element.drawDesc.borderRight + element.drawDesc.borderLeft));
                        vertex.texCoord0.y = height - ((element.drawDesc.borderTop + element.drawDesc.borderBottom));
                        vertex.indices.x = 0; // set later
                        vertex.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                        vertex.indices.z = BitUtil.SetHighLowBitsUint((uint) (element.drawDesc.uvBottom - uvBorderBottom - 1), (uint) (element.drawDesc.uvLeft + uvBorderLeft + 1));
                        vertex.indices.w = BitUtil.SetHighLowBitsUint((uint) (element.drawDesc.uvTop + uvBorderTop + 2), (uint) (element.drawDesc.uvRight - uvBorderRight - 1));

                    }
                    else {
                        uint uvTransformId = 0;

                        if (element.drawDesc.uvScaleX != 1 || element.drawDesc.uvScaleY != 1 || element.drawDesc.uvOffsetX != 0 || element.drawDesc.uvOffsetY != 0) {
                            uvTransformId = (uint) float4Buffer.size;
                            float4Buffer.Add(new float4() {
                                x = element.drawDesc.uvScaleX,
                                y = element.drawDesc.uvScaleY,
                                z = element.drawDesc.uvOffsetX,
                                w = element.drawDesc.uvOffsetY
                            });
                        }

                        mat.uvTransformId = uvTransformId;

                        materialList.Add(mat);

                        // todo -- put opacity somewhere, maybe material data for elements if we arent hash caching it

                        ref UIForiaVertex vertex = ref vertices[vertexIdx++];
                        vertex.position.x = x + halfWidth;
                        vertex.position.y = -(y + halfHeight);
                        vertex.texCoord0.x = width;
                        vertex.texCoord0.y = height;
                        vertex.indices.x = 0; // set later
                        vertex.indices.y = (uint) materialIdx;
                        vertex.indices.z = BitUtil.SetHighLowBitsUint(element.drawDesc.uvTop, element.drawDesc.uvLeft);
                        vertex.indices.w = BitUtil.SetHighLowBitsUint(element.drawDesc.uvBottom, element.drawDesc.uvRight);
                    }
                }

                // for(int j = 0; j < count; j++) {
                // todo -- ensure size n stuff
                // }

                // todo -- account for uv rect, background fit, etc
                // might be able to do this in shader
                // for (int v = meshInfo.vertexStart; v < vEnd; v++) {
                //     ref UIForiaVertex vertex = ref vertices[v];
                //     float uvX = ((vertex.position.x) / width);
                //     float uvY = 1 - ((vertex.position.y) / -height);
                //     int hSign = vertex.position.x == 0 ? -1 : 1;
                //     int vSign = vertex.position.y == 0 ? 1 : -1;
                //     vertex.texCoord0.x = uvX + ((0.5f / width) * hSign);
                //     vertex.texCoord0.y = uvY + ((0.5f / height) * vSign);
                //
                //     //vertex.texCoord1.x = uvX;
                //     //vertex.texCoord1.y = uvY;
                // }

            }

            vertexList.size = vertexIdx;
            borderColorMap.Dispose();
        }

    }

}