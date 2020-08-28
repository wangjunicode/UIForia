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

                ElementDrawDesc* desc = (ElementDrawDesc*) drawInfo.shapeData;
                if (desc->isSliced) {
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
                ElementDrawDesc* elementDrawDesc = (ElementDrawDesc*) drawInfo.shapeData;

                meshInfo.type = MeshInfoType.Element;
                meshInfo.vertexStart = vertexIdx;
                meshInfo.vertexCount = 1;

                int materialIdx = materialList.size;
                // FindIdx(hash, ptr, materialList);
                // materialList.Add();
                // want to collect data in the material desc but then extract a lot of it to per-element data to cut down on upload 
                // otherwise too much of the abstraction leaks out, its nice having one material per element / draw
                // can diff this on texture but no point doing it more than that

                ref ElementDrawDesc drawDesc = ref elementDrawDesc[0];
                float x = drawInfo.localBounds.xMin;
                float y = drawInfo.localBounds.yMin;
                float width = drawInfo.localBounds.Width;
                float height = drawInfo.localBounds.Height;
                float halfWidth = width * 0.5f;
                float halfHeight = height * 0.5f;

                ElementMaterialInfo mat = default;

                mat.backgroundColor = drawDesc.backgroundColor;
                mat.backgroundTint = drawDesc.backgroundTint;

                mat.radius0 = drawDesc.radiusTL;
                mat.radius1 = drawDesc.radiusTR;
                mat.radius2 = drawDesc.radiusBR;
                mat.radius3 = drawDesc.radiusBL;

                mat.bevelTL = drawDesc.bevelTL;
                mat.bevelTR = drawDesc.bevelTR;
                mat.bevelBR = drawDesc.bevelBR;
                mat.bevelBL = drawDesc.bevelBL;

                mat.bodyColorMode = drawDesc.bgColorMode;
                mat.outlineColorMode = drawDesc.outlineColorMode;

                mat.outlineWidth = drawDesc.outlineWidth;
                mat.outlineColor = drawDesc.outlineColor;

                mat.fillRotation = drawDesc.meshFillRotation;
                mat.fillRadius = drawDesc.meshFillRadius;
                mat.fillOpenAmount = drawDesc.meshFillOpenAmount;
                mat.fillOffsetX = drawDesc.meshFillOffsetX;
                mat.fillOffsetY = drawDesc.meshFillOffsetY;
                mat.fillInvert = drawDesc.meshFillInvert;
                mat.fillDirection = drawDesc.meshFillDirection;
                mat.opacity = drawDesc.opacity;
                mat.uvRotation = drawDesc.uvRotation;

                mat.borderColorTop = drawDesc.borderColorTop;
                mat.borderColorRight = drawDesc.borderColorRight;
                mat.borderColorBottom = drawDesc.borderColorBottom;
                mat.borderColorLeft = drawDesc.borderColorLeft;

                mat.maskFlags = (uint) drawDesc.maskFlags;
                mat.maskSoftness = drawDesc.maskSoftness;
                mat.maskTopLeftUV = drawDesc.maskTopLeftUV;
                mat.maskBottomRightUV = drawDesc.maskBottomRightUV;

                if (drawDesc.HasBorder) {
                    mat.borderIndex = (uint) float4Buffer.size;
                    float4Buffer.Add(new float4() {
                        x = drawDesc.borderTop < halfHeight ? drawDesc.borderTop : halfHeight,
                        y = drawDesc.borderRight < halfWidth ? drawDesc.borderRight : halfWidth,
                        z = drawDesc.borderBottom < halfHeight ? drawDesc.borderBottom : halfHeight,
                        w = drawDesc.borderLeft < halfWidth ? drawDesc.borderLeft : halfWidth
                    });
                }

                if (drawDesc.isSliced) {
                    meshInfo.vertexCount = 9;
                    ElementShaderFlags flags = ElementShaderFlags.IsNineSliceBorder;

                    ushort uvBorderTop = drawDesc.uvBorderTop;
                    ushort uvBorderRight = drawDesc.uvBorderRight;
                    ushort uvBorderBottom = drawDesc.uvBorderBottom;
                    ushort uvBorderLeft = drawDesc.uvBorderLeft;

                    ref UIForiaVertex vertexTopLeft = ref vertices[vertexIdx++];
                    ref UIForiaVertex vertexTopCenter = ref vertices[vertexIdx++];
                    ref UIForiaVertex vertexTopRight = ref vertices[vertexIdx++];
                    ref UIForiaVertex vertexCenterLeft = ref vertices[vertexIdx++];
                    ref UIForiaVertex vertexBottomLeft = ref vertices[vertexIdx++];
                    ref UIForiaVertex vertexBottomCenter = ref vertices[vertexIdx++];
                    ref UIForiaVertex vertexBottomRight = ref vertices[vertexIdx++];
                    ref UIForiaVertex vertexCenterRight = ref vertices[vertexIdx++];

                    vertexTopLeft.position.x = x + (drawDesc.borderLeft * 0.5f);
                    vertexTopLeft.position.y = -(y + (drawDesc.borderTop * 0.5f));
                    vertexTopLeft.texCoord0.x = drawDesc.borderLeft;
                    vertexTopLeft.texCoord0.y = drawDesc.borderTop;
                    vertexTopLeft.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));

                    vertexTopLeft.indices.z = BitUtil.SetHighLowBitsUint((uint) (drawDesc.uvBottom - uvBorderBottom), drawDesc.uvLeft);
                    vertexTopLeft.indices.w = BitUtil.SetHighLowBitsUint(
                        drawDesc.uvBottom,
                        (uint) (drawDesc.uvLeft + uvBorderLeft)
                    );

                    vertexTopCenter.position.x = x + (halfWidth);
                    vertexTopCenter.position.y = -(y + (drawDesc.borderTop * 0.5f));
                    vertexTopCenter.texCoord0.x = width - drawDesc.borderLeft - drawDesc.borderRight;
                    vertexTopCenter.texCoord0.y = drawDesc.borderTop;
                    vertexTopCenter.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                    vertexTopCenter.indices.z = BitUtil.SetHighLowBitsUint((uint) (drawDesc.uvBottom - uvBorderBottom), (uint) (drawDesc.uvLeft + uvBorderLeft));
                    vertexTopCenter.indices.w = BitUtil.SetHighLowBitsUint(
                        drawDesc.uvBottom,
                        (uint) (drawDesc.uvRight - uvBorderRight)
                    );

                    vertexTopRight.position.x = x + (width - (0.5f * drawDesc.borderRight));
                    vertexTopRight.position.y = -(y + (drawDesc.borderTop * 0.5f));
                    vertexTopRight.texCoord0.x = drawDesc.borderRight;
                    vertexTopRight.texCoord0.y = drawDesc.borderTop;
                    vertexTopRight.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                    vertexTopRight.indices.z = BitUtil.SetHighLowBitsUint(
                        (uint) (drawDesc.uvBottom - uvBorderBottom),
                        (uint) (drawDesc.uvRight - uvBorderRight)
                    );
                    vertexTopRight.indices.w = BitUtil.SetHighLowBitsUint(
                        drawDesc.uvBottom,
                        drawDesc.uvRight
                    );

                    vertexCenterLeft.position.x = x + (drawDesc.borderLeft * 0.5f);
                    vertexCenterLeft.position.y = -(y + (height * 0.5f));
                    vertexCenterLeft.texCoord0.x = drawDesc.borderLeft;
                    vertexCenterLeft.texCoord0.y = height - (drawDesc.borderTop + drawDesc.borderBottom);
                    vertexCenterLeft.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                    vertexCenterLeft.indices.z = BitUtil.SetHighLowBitsUint(
                        (uint) (drawDesc.uvTop + uvBorderTop),
                        drawDesc.uvLeft
                    );
                    vertexCenterLeft.indices.w = BitUtil.SetHighLowBitsUint(
                        (uint) (drawDesc.uvBottom - uvBorderBottom),
                        (uint) (drawDesc.uvLeft + uvBorderLeft)
                    );

                    vertexBottomLeft.position.x = x + (drawDesc.borderLeft * 0.5f);
                    vertexBottomLeft.position.y = -(y + height - drawDesc.borderTop + (0.5f * drawDesc.borderBottom));
                    vertexBottomLeft.texCoord0.x = drawDesc.borderLeft;
                    vertexBottomLeft.texCoord0.y = drawDesc.borderBottom;
                    vertexBottomLeft.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                    vertexBottomLeft.indices.z = BitUtil.SetHighLowBitsUint(
                        drawDesc.uvTop,
                        drawDesc.uvLeft
                    );
                    vertexBottomLeft.indices.w = BitUtil.SetHighLowBitsUint(
                        (uint) (drawDesc.uvTop + uvBorderTop),
                        (uint) (drawDesc.uvLeft + uvBorderLeft)
                    );

                    vertexBottomCenter.position.x = x + halfWidth;
                    vertexBottomCenter.position.y = -(y + height - drawDesc.borderTop + (0.5f * drawDesc.borderBottom));
                    vertexBottomCenter.texCoord0.x = width - drawDesc.borderLeft - drawDesc.borderRight;
                    vertexBottomCenter.texCoord0.y = drawDesc.borderBottom;
                    vertexBottomCenter.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                    vertexBottomCenter.indices.z = BitUtil.SetHighLowBitsUint(
                        drawDesc.uvTop,
                        (uint) (drawDesc.uvLeft + uvBorderLeft)
                    );
                    vertexBottomCenter.indices.w = BitUtil.SetHighLowBitsUint(
                        (uint) (drawDesc.uvTop + uvBorderTop),
                        (uint) (drawDesc.uvRight - uvBorderRight)
                    );

                    vertexBottomRight.position.x = x + width - (0.5f * drawDesc.borderRight);
                    vertexBottomRight.position.y = -(y + height - drawDesc.borderTop + (0.5f * drawDesc.borderBottom));
                    vertexBottomRight.texCoord0.x = drawDesc.borderRight;
                    vertexBottomRight.texCoord0.y = drawDesc.borderBottom;
                    vertexBottomRight.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                    vertexBottomRight.indices.z = BitUtil.SetHighLowBitsUint(
                        drawDesc.uvTop,
                        (uint) (drawDesc.uvRight - uvBorderRight)
                    );
                    vertexBottomRight.indices.w = BitUtil.SetHighLowBitsUint(
                        (uint) (drawDesc.uvTop + uvBorderTop),
                        drawDesc.uvRight
                    );

                    vertexCenterRight.position.x = x + width - (drawDesc.borderRight * 0.5f);
                    vertexCenterRight.position.y = -(y + halfHeight);
                    vertexCenterRight.texCoord0.x = drawDesc.borderRight;
                    vertexCenterRight.texCoord0.y = height - (drawDesc.borderTop + drawDesc.borderBottom);
                    vertexCenterRight.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                    vertexCenterRight.indices.z = BitUtil.SetHighLowBitsUint(
                        (uint) (drawDesc.uvBottom - uvBorderBottom),
                        (uint) (drawDesc.uvRight - uvBorderRight)
                    );
                    vertexCenterRight.indices.w = BitUtil.SetHighLowBitsUint(
                        (uint) (drawDesc.uvTop + uvBorderTop),
                        drawDesc.uvRight
                    );

                    uint uvTransformId = 0;
                    if (drawDesc.uvScaleX != 1 || drawDesc.uvScaleY != 1 || drawDesc.uvOffsetX != 0 || drawDesc.uvOffsetY != 0) {
                        uvTransformId = (uint) float4Buffer.size;
                        float4Buffer.Add(new float4() {
                            x = drawDesc.uvScaleX,
                            y = drawDesc.uvScaleY,
                            z = drawDesc.uvOffsetX,
                            w = drawDesc.uvOffsetY
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
                    vertex.texCoord0.x = width - ((drawDesc.borderRight + drawDesc.borderLeft));
                    vertex.texCoord0.y = height - ((drawDesc.borderTop + drawDesc.borderBottom));
                    vertex.indices.x = 0; // set later
                    vertex.indices.y = (uint) (((int) flags << 24) | (materialIdx & 0xffffff));
                    vertex.indices.z = BitUtil.SetHighLowBitsUint((uint) (drawDesc.uvBottom - uvBorderBottom - 1), (uint) (drawDesc.uvLeft + uvBorderLeft + 1));
                    vertex.indices.w = BitUtil.SetHighLowBitsUint((uint) (drawDesc.uvTop + uvBorderTop + 2), (uint) (drawDesc.uvRight - uvBorderRight - 1));
                }
                else {
                    uint uvTransformId = 0;

                    if (drawDesc.uvScaleX != 1 || drawDesc.uvScaleY != 1 || drawDesc.uvOffsetX != 0 || drawDesc.uvOffsetY != 0) {
                        uvTransformId = (uint) float4Buffer.size;
                        float4Buffer.Add(new float4() {
                            x = drawDesc.uvScaleX,
                            y = drawDesc.uvScaleY,
                            z = drawDesc.uvOffsetX,
                            w = drawDesc.uvOffsetY
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
                    vertex.indices.z = BitUtil.SetHighLowBitsUint(drawDesc.uvTop, drawDesc.uvLeft);
                    vertex.indices.w = BitUtil.SetHighLowBitsUint(drawDesc.uvBottom, drawDesc.uvRight);
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