using System;
using System.Runtime.InteropServices;
using ThisOtherThing.UI.ShapeUtils;
using UIForia.Graphics;
using UIForia.Graphics.ShapeKit;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using FontStyle = UIForia.Text.FontStyle;

namespace UIForia.Systems {

    [BurstCompile]
    internal unsafe struct BakeShapes : IJob, IVertigoParallel {

        [NativeSetThreadIndex] public int threadIndex;

        public DataList<DrawInfo>.Shared drawList;
        public DataList<FontAssetInfo>.Shared fontAssetMap;

        public PerThread<ShapeDataBuffer> perThread_ShapeBuffer;

        public ParallelParams parallel { get; set; }

        public void Execute() {
            Run(0, drawList.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        private void GeometryFromVertexHelper(ref PagedByteAllocator byteAllocator, ref UIVertexHelper vh, ref DrawInfo drawInfo) {
            int vertexCount = vh.currentVertCount;
            int triangleCount = vh.TotalTriangleCount;

            GeometryInfo* geometryInfo = byteAllocator.AllocateCleared<GeometryInfo>();

            geometryInfo->positions = byteAllocator.Allocate<float3>(vertexCount);
            geometryInfo->texCoord0 = byteAllocator.Allocate<float4>(vertexCount);
            geometryInfo->colors = byteAllocator.Allocate<Color32>(vertexCount);
            geometryInfo->triangles = byteAllocator.Allocate<int>(triangleCount);

            TypedUnsafe.MemCpy(geometryInfo->positions, vh.positions, vertexCount);
            TypedUnsafe.MemCpy(geometryInfo->texCoord0, vh.texCoord, vertexCount);
            TypedUnsafe.MemCpy(geometryInfo->colors, vh.colors, vertexCount);
            TypedUnsafe.MemCpy(geometryInfo->triangles, vh.triangles, triangleCount);

            geometryInfo->vertexCount = vertexCount;
            geometryInfo->triangleCount = triangleCount;
            geometryInfo->vertexLayout = drawInfo.vertexLayout;
            drawInfo.geometryInfo = geometryInfo;
        }

        private void Run(int start, int end) {
            ref ShapeDataBuffer buffer = ref perThread_ShapeBuffer.GetForThread(threadIndex);
            ref UIVertexHelper vh = ref UnsafeUtilityEx.AsRef<UIVertexHelper>(buffer.vertexHelper);
            ref ShapeKit shapeKit = ref UnsafeUtilityEx.AsRef<ShapeKit>(buffer.shapeKit);

            for (int i = start; i < end; i++) {

                ref DrawInfo drawInfo = ref drawList[i];

                if ((drawInfo.type & (DrawType.SDFText | DrawType.Shape)) == 0) {
                    continue;
                }

                switch (drawInfo.type) {

                    case DrawType.SDFText: {
                        BakeSDFText(ref buffer.byteAllocator, ref drawInfo);

                        vh.Clear();

                        break;
                    }

                    case DrawType.Rect: {

                        RectData* rectData = (RectData*) drawInfo.shapeData;

                        shapeKit.SetDpiScale(1f); // remove
                        shapeKit.SetAntiAliasWidth(0); // fix

                        shapeKit.AddRect(ref vh, rectData->x, rectData->y, rectData->width, rectData->height, rectData->color);

                        GeometryFromVertexHelper(ref buffer.byteAllocator, ref vh, ref drawInfo);

                        vh.Clear();

                        drawInfo.aabb = ComputeAABB(rectData->x, rectData->y, rectData->width, rectData->height, drawInfo.matrix);

                        break;
                    }

                    case DrawType.RoundedRect: {
                        RoundedRectData* rectData = (RoundedRectData*) drawInfo.shapeData;

                        shapeKit.SetDpiScale(1f); // remove
                        shapeKit.SetAntiAliasWidth(1.25f); // fix

                        shapeKit.AddRoundedRect(ref vh, new float2(rectData->x, rectData->y), rectData->width, rectData->height, rectData->cornerProperties, rectData->color);

                        GeometryFromVertexHelper(ref buffer.byteAllocator, ref vh, ref drawInfo);

                        vh.Clear();

                        drawInfo.aabb = ComputeAABB(rectData->x, rectData->y, rectData->width, rectData->height, drawInfo.matrix);
                        break;
                    }

                    case DrawType.Arc:
                        break;

                    case DrawType.Polygon:
                        break;

                    case DrawType.Line:
                        break;

                    case DrawType.Ellipse:
                        break;

                }

            }

        }

        private void BakeSDFText(ref PagedByteAllocator byteAllocator, ref DrawInfo drawInfo) {
            ref TextInfoRenderSpan textInfo = ref UnsafeUtilityEx.AsRef<TextInfoRenderSpan>(drawInfo.shapeData);
            BakeSDFTextWithoutProcessing(ref byteAllocator, ref drawInfo, ref textInfo);
        }

        // process text by line
        // 1 line can contain multiple spans
        // each character knows its span id
        // if span id changes
        // we'll need to determine if underlines, colors, etc change
        // maybe we leave it as is, but need to know previous / next span ids to connect underlines, marks, etc

        private void BakeSDFTextWithoutProcessing(ref PagedByteAllocator byteAllocator, ref DrawInfo drawInfo, ref TextInfoRenderSpan spanInfo) {
            int quadCount = 0;

            ref TextInfoRenderData textRenderInfo = ref UnsafeUtilityEx.AsRef<TextInfoRenderData>(spanInfo.textInfo);

            // spans are split in two ways, by line and by draw call
            // this means that any call to this function handles exactly 1 line of text
            // and all the styling has already been computed

            TextSymbol* symbolArray = textRenderInfo.symbolList.array;
            TextLayoutSymbol* layoutSymbolArray = textRenderInfo.layoutSymbolList.array;

            for (int i = spanInfo.symbolStart; i < spanInfo.symbolEnd; i++) {

                ref TextSymbol symbol = ref symbolArray[i];

                // todo -- support sprites
                if ((symbol.type != TextSymbolType.Character)) {
                    continue;
                }

                if (symbol.charInfo.character == 32) {
                    continue;
                }

                quadCount++;

            }

            ref FontAssetInfo fontAssetInfo = ref fontAssetMap[spanInfo.fontAssetId];
            float3 scaleRatios = TextUtil.ComputeRatios(fontAssetInfo, spanInfo);
            float padding = TextUtil.GetPadding(fontAssetInfo.gradientScale, spanInfo, scaleRatios);

            float stylePadding;
            float weight;

            if ((spanInfo.fontStyle & Text.FontStyle.Bold) != 0) {
                weight = fontAssetInfo.weightBold;
                stylePadding = fontAssetInfo.boldStyle / 4.0f * fontAssetInfo.gradientScale * scaleRatios.x;
                if (stylePadding + padding > fontAssetInfo.gradientScale) {
                    padding = fontAssetInfo.gradientScale - stylePadding;
                }

            }
            else {
                weight = fontAssetInfo.weightNormal;
                stylePadding = fontAssetInfo.normalStyle / 4.0f * fontAssetInfo.gradientScale;
                if (stylePadding + padding > fontAssetInfo.gradientScale) {
                    padding = fontAssetInfo.gradientScale - stylePadding;
                }
            }

            float originalPadding = padding;

            padding += stylePadding;

            // should handle marks, highlights, underline, strike through here

            float smallCapsMultiplier = 1f;

            float m_fontScale = spanInfo.fontSize * smallCapsMultiplier / fontAssetInfo.faceInfo.pointSize * fontAssetInfo.faceInfo.scale;
            float elementScale = m_fontScale * 1f; //m_fontScaleMultiplier;

            padding *= elementScale;

            int vertexCount = quadCount * 4;
            int triangleCount = quadCount * 6;

            // this does NOT handle strike through / underline / highlight / mark yet, but it should

            if ((spanInfo.fontStyle & FontStyle.Underline) != 0) {
                // todo -- connect to next / prev
                vertexCount += (3 * 4);
                triangleCount += (3 * 6);
            }

            GeometryInfo* geometry = byteAllocator.AllocateCleared<GeometryInfo>();

            float3* positions = byteAllocator.Allocate<float3>(vertexCount);
            float4* texCoord0 = byteAllocator.Allocate<float4>(vertexCount);
            float4* texCoord1 = byteAllocator.Allocate<float4>(vertexCount);
            Color32* colors = byteAllocator.Allocate<Color32>(vertexCount);
            int* triangles = byteAllocator.Allocate<int>(triangleCount);

            geometry->positions = positions;
            geometry->texCoord0 = texCoord0;
            geometry->texCoord1 = (byte*) texCoord1;
            geometry->colors = colors;
            geometry->triangles = triangles;
            geometry->vertexCount = vertexCount;
            geometry->triangleCount = triangleCount;

            float4* underlayChannel = null;

            if (spanInfo.UseUnderlay) {
                underlayChannel = byteAllocator.Allocate<float4>(vertexCount);
                drawInfo.vertexLayout.texCoord2 = VertexChannelFormat.Float4;
                geometry->texCoord2 = underlayChannel;
            }

            drawInfo.vertexLayout.texCoord1 = VertexChannelFormat.Float4;

            geometry->vertexLayout = drawInfo.vertexLayout;

            drawInfo.geometryInfo = geometry;
            Color32 color = spanInfo.faceColor;

            const float sharpness = 0f; // todo -- figure this out

            float scale = math.abs(elementScale) * fontAssetInfo.gradientScale * (sharpness + 1);

            // weight is always positive, can probably map between 0 and 16 or so, definitely a ushort
            weight *= 0.25f;
            weight = (weight + spanInfo.faceDilate) * scaleRatios.x * 0.5f;

            byte outlineWidthByte = MathUtil.Float01ToByte(spanInfo.outlineWidth * scaleRatios.x);
            byte outlineSoftnessByte = MathUtil.Float01ToByte(spanInfo.outlineSoftness * scaleRatios.x);

            // between -1 and 1 since scale ratio is clamped to 0, 1
            // might be able to use a byte, experiment to see if precision matters
            float alphaClip = 1.0f - spanInfo.outlineWidth * scaleRatios.x - spanInfo.outlineSoftness * scaleRatios.x;

            // might need to only do this if glow is on
            alphaClip = math.min(alphaClip, 1.0f - spanInfo.glowOffset * scaleRatios.y - spanInfo.glowOuter * scaleRatios.y);

            int alphaClipUshort = (ushort) MathUtil.RemapRange(alphaClip, 0, 1, 0, ushort.MaxValue);

            byte glowOffsetByte = MathUtil.FloatMinus1To1ToByte(spanInfo.glowOffset * scaleRatios.y);
            byte glowInnerByte = MathUtil.Float01ToByte(spanInfo.glowInner * scaleRatios.y);
            byte glowOuterByte = MathUtil.Float01ToByte(spanInfo.glowOuter * scaleRatios.y);
            byte glowPowerByte = MathUtil.Float01ToByte(spanInfo.glowPower);

            uint packedGlow = BitUtil.SetBytes(glowOffsetByte, glowInnerByte, glowOuterByte, glowPowerByte);
            uint packedOutlineAlphaClip = BitUtil.SetBytes(outlineWidthByte, outlineSoftnessByte, 0, 0);
            packedOutlineAlphaClip = BitUtil.SetHighLowBitsUint((uint) alphaClipUshort, packedOutlineAlphaClip);

            int vertexIndex = 0;

            float widthMultiplier = 1f / fontAssetInfo.atlasWidth;
            float heightMultiplier = 1f / fontAssetInfo.atlasHeight;

            for (int i = spanInfo.symbolStart; i < spanInfo.symbolEnd; i++) {
                ref TextSymbol symbol = ref symbolArray[i];

                if (symbol.type != TextSymbolType.Character) {
                    continue;
                }

                // todo -- skip other non printing characters
                if (symbol.charInfo.character == 32 || symbol.charInfo.character == 10) {
                    continue;
                }

                ref BurstCharInfo charInfo = ref symbol.charInfo;
                ref WordInfo wordInfo = ref layoutSymbolArray[charInfo.wordIndex].wordInfo;

                ref float3 p0 = ref positions[vertexIndex + 0];
                ref float3 p1 = ref positions[vertexIndex + 1];
                ref float3 p2 = ref positions[vertexIndex + 2];
                ref float3 p3 = ref positions[vertexIndex + 3];

                ref float4 uv0_tl = ref texCoord0[vertexIndex + 0];
                ref float4 uv0_tr = ref texCoord0[vertexIndex + 1];
                ref float4 uv0_br = ref texCoord0[vertexIndex + 2];
                ref float4 uv0_bl = ref texCoord0[vertexIndex + 3];

                float charX = wordInfo.x + (charInfo.topLeft.x - padding);
                float charY = wordInfo.y + (charInfo.topLeft.y - padding);

                float charWidth = (charInfo.bottomRight.x - charInfo.topLeft.x) + (padding * 2);
                float charHeight = (charInfo.bottomRight.y - charInfo.topLeft.y) + (padding * 2);

                p0.x = charX + (charInfo.shearTop);
                p0.y = -charY;
                p0.z = 0;

                p1.x = charX + charWidth + (charInfo.shearTop);
                p1.y = -charY;
                p1.z = 0;

                p2.x = charX + charWidth + (charInfo.shearBottom);
                p2.y = -(charY + charHeight);
                p2.z = 0;

                p3.x = charX + (charInfo.shearBottom);
                p3.y = -(charY + charHeight);
                p3.z = 0;

                uv0_tl.x = (charInfo.topLeftUv.x - padding) * widthMultiplier;
                uv0_tl.y = (charInfo.bottomRightUv.y + padding) * heightMultiplier;
                uv0_tr.x = (charInfo.bottomRightUv.x + padding) * widthMultiplier;
                uv0_tr.y = uv0_tl.y;
                uv0_br.x = uv0_tr.x;
                uv0_br.y = (charInfo.topLeftUv.y - padding) * heightMultiplier;
                uv0_bl.x = uv0_tl.x;
                uv0_bl.y = uv0_br.y;

                vertexIndex += 4;

            }

            // when connecting underlays, only 1 drawInfo should control the rendering of it
            // when not connected it doesn't matter

            if ((spanInfo.fontStyle & FontStyle.Underline) != 0) {

                float subscriptOffset = 0; // baseline offset for subscript characters

                float offset = -fontAssetInfo.faceInfo.ascender * elementScale; // max ascender when mixing underlines probably (prescaled)
                float underlineThickness = fontAssetInfo.faceInfo.underlineThickness;
                float underlineBaseline = offset + (fontAssetInfo.faceInfo.underlineOffset * elementScale);
                float baselineOffset = fontAssetInfo.faceInfo.baseline - subscriptOffset;

                // the last character visually
                // find the first character 
                float3 start = positions[0];
                float3 end = positions[2];

                fontAssetInfo.TryGetGlyph(95, out UIForiaGlyph underlineGlyph);

                float segmentWidth = underlineGlyph.width * 0.5f * elementScale;

                if (end.x - start.x < underlineGlyph.width * elementScale) {
                    segmentWidth = (end.x - start.x) * 0.5f;
                }

                int index = vertexCount - 12;

                float startY = underlineBaseline;
                float h = (underlineThickness + originalPadding) * elementScale;
                float scaledPadding = originalPadding * elementScale;

                positions[index + 0] = new float3(start.x, startY - h, 0);
                positions[index + 1] = new float3(start.x + segmentWidth, startY - h, 0);
                positions[index + 2] = new float3(start.x + segmentWidth, startY + scaledPadding, 0);
                positions[index + 3] = new float3(start.x, startY + scaledPadding, 0);

                positions[index + 4] = positions[index + 1];
                positions[index + 5] = new float3(positions[1].x - segmentWidth, startY - h, 0);
                positions[index + 6] = new float3(positions[1].x - segmentWidth, startY + scaledPadding, 0);
                positions[index + 7] = positions[index + 2];

                positions[index + 8] = new float3(positions[1].x - segmentWidth, startY - h, 0);
                positions[index + 9] = new float3(positions[1].x, startY - h, 0);
                positions[index + 10] = new float3(positions[1].x, startY + scaledPadding, 0);
                positions[index + 11] = new float3(positions[1].x - segmentWidth, startY + scaledPadding, 0);

                float startPadding = originalPadding;
                float endPadding = originalPadding;

                float uvStartLeft = (underlineGlyph.uvX - startPadding) * widthMultiplier;
                float uvStartRight = (underlineGlyph.uvX - startPadding + underlineGlyph.uvWidth * 0.5f) * widthMultiplier;
                float uvTop = (underlineGlyph.uvY - originalPadding) * heightMultiplier;
                float uvBottom = (underlineGlyph.uvY + underlineGlyph.uvHeight + originalPadding) * heightMultiplier;

                float uvEndLeft = (underlineGlyph.uvX + endPadding + underlineGlyph.uvWidth * 0.5f) * widthMultiplier;
                float uvEndRight = (underlineGlyph.uvX + endPadding + underlineGlyph.uvWidth) * widthMultiplier;

                float4 uv0 = new float4(uvStartLeft, uvTop, 0, 0);
                float4 uv1 = new float4(uvStartRight, uvTop, 0, 0);
                float4 uv2 = new float4(uvStartRight, uvBottom, 0, 0);
                float4 uv3 = new float4(uvStartLeft, uvBottom, 0, 0);

                float4 uv4 = new float4(uvEndLeft, uvTop, 0, 0);
                float4 uv5 = new float4(uvEndRight, uvTop, 0, 0);
                float4 uv6 = new float4(uvEndRight, uvBottom, 0, 0);
                float4 uv7 = new float4(uvEndLeft, uvBottom, 0, 0);

                texCoord0[index + 0] = uv0;
                texCoord0[index + 1] = uv1;
                texCoord0[index + 2] = uv2;
                texCoord0[index + 3] = uv3;

                texCoord0[index + 4] = new float4(uvStartRight - uvStartRight * 0.001f, uvTop, 0, 0);
                texCoord0[index + 5] = new float4(uvStartRight + uvStartRight * 0.001f, uvTop, 0, 0);
                texCoord0[index + 6] = new float4(uvStartRight + uvStartRight * 0.001f, uvBottom, 0, 0);
                texCoord0[index + 7] = new float4(uvStartRight - uvStartRight * 0.001f, uvBottom, 0, 0);

                texCoord0[index + 8] = uv4;
                texCoord0[index + 9] = uv5;
                texCoord0[index + 10] = uv6;
                texCoord0[index + 11] = uv7;

                // todo -- quad coloring lerp appropriately
                colors[index + 0] = Color.red;
                colors[index + 1] = Color.red;
                colors[index + 2] = Color.red;
                colors[index + 3] = Color.red;

                colors[index + 4] = Color.white;
                colors[index + 5] = Color.white;
                colors[index + 6] = Color.white;
                colors[index + 7] = Color.white;

                colors[index + 8] = Color.blue;
                colors[index + 9] = Color.blue;
                colors[index + 10] = Color.blue;
                colors[index + 11] = Color.blue;

            }

            uint remappedScale = (uint) MathUtil.RemapRange(scale, 0, 32, 0, ushort.MaxValue);
            uint remappedWeight = (uint) MathUtil.RemapRange(weight, 0, 32, 0, ushort.MaxValue);

            uint packedElementScaleWeight = BitUtil.SetHighLowBitsUint(remappedScale, remappedWeight);

            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MinValue;
            float yMax = float.MaxValue;

            drawInfo.aabb = new AxisAlignedBounds2D(xMin, yMin, xMax, yMax);

            float4* tex1Float = texCoord1;
            uint4* text1Uint = (uint4*) tex1Float;
            // color indices, will need to sorted out in batcher probably?
            // outline color, face color (same as vertex color?) clipperIndex (batcher also),
            // glow info could come in through uniforms since its uncommoner and requires more expensive shader anyway
            // maybe turn this into float2 channel for default since i dont need much of it
            text1Uint->x = BitUtil.SetBytes(0, 0, 0, 0);
            tex1Float->y = BitUtil.SetBytes(0, 0, 0, 0);
            tex1Float->z = 0;
            text1Uint->w = packedGlow;
            float4 value = default;
            uint4* iVal = (uint4*) &value;
            value.x = 0;
            value.y = 0;
            value.z = 0;
            iVal->w = packedGlow;

            if (underlayChannel != null) {

                float underlayX = -(spanInfo.underlayX * scaleRatios.z) * fontAssetInfo.gradientScale / fontAssetInfo.atlasWidth;
                float underlayY = -(spanInfo.underlayY * scaleRatios.z) * fontAssetInfo.gradientScale / fontAssetInfo.atlasHeight;

                ushort underlayDilateByte = (ushort) MathUtil.RemapRange(MathUtil.Clamp01(spanInfo.underlayDilate) * scaleRatios.z, 0, 1, 0, ushort.MaxValue);
                ushort underlaySoftnessByte = (ushort) MathUtil.RemapRange(MathUtil.Clamp01(spanInfo.underlaySoftness) * scaleRatios.z, 0, 1, 0, ushort.MaxValue);

                // todo put color in and unpack accordingly
                uint packedUnderlayDilateSoftness = BitUtil.SetHighLowBitsUint(underlayDilateByte, underlaySoftnessByte);
                ref uint castColor = ref UnsafeUtilityEx.As<Color32, uint>(ref spanInfo.underlayColor);
                for (int i = 0; i < vertexCount; i++) {
                    float4* underlayFloat = underlayChannel + i;
                    uint4* underlayInt = (uint4*) underlayFloat;
                    underlayFloat->x = underlayX;
                    underlayFloat->y = underlayY;
                    underlayFloat->z = MathUtil.Clamp01(spanInfo.underlayDilate) * scaleRatios.z;
                    underlayFloat->w = MathUtil.Clamp01(spanInfo.underlaySoftness) * scaleRatios.z;
                    // underlayInt->z = packedUnderlayDilateSoftness;
                    // underlayInt->w = castColor;
                }

            }

            for (int i = 0; i < vertexCount; i++) {
                if (positions[i].x < xMin) xMin = positions[i].x;
                if (positions[i].x > xMax) xMax = positions[i].x;
                if (positions[i].y < yMin) yMin = positions[i].y;
                if (positions[i].y > yMax) yMax = positions[i].y;
            }

            for (int i = 0; i < vertexCount; i++) {
                colors[i] = color;

            }

            for (int i = 0; i < vertexCount; i++) {
                texCoord1[i] = value;
                uint4* text0Uint = (uint4*) texCoord0 + i;
                text0Uint->z = packedOutlineAlphaClip;
                text0Uint->w = packedElementScaleWeight;
            }

            int triIdx = 0;
            for (int i = 0; i < vertexCount; i += 4) {
                triangles[triIdx + 0] = i + 0;
                triangles[triIdx + 1] = i + 1;
                triangles[triIdx + 2] = i + 2;
                triangles[triIdx + 3] = i + 2;
                triangles[triIdx + 4] = i + 3;
                triangles[triIdx + 5] = i + 0;
                triIdx += 6;
            }

            ComputeTextUVMapping();

        }

        private static void ComputeTextUVMapping() {

            // for (int i = 0; i < vertexCount; i++) {
            //
            //     float4 * uv0 = 
            //     float x0 = uv0_tl.z;
            //     float y0 = uv0_tl.w;
            //     float x1 = uv0_br.z;
            //     float y1 = uv0_br.w;
            //
            //     float dx = (int) x0;
            //     float dy = (int) y0;
            //
            //     x0 -= dx;
            //     x1 -= dx;
            //     y0 -= dy;
            //     y1 -= dy;
            //
            //     // probably want to move these so they only get included if effect is on
            //     uv0_tl.z = PackUV(x0, y0);
            //     uv0_tr.z = PackUV(x0, y1);
            //     uv0_br.z = PackUV(x1, y1);
            //     uv0_bl.z = PackUV(x1, y0);
            //             switch (spanInfo.faceUVModeH) {
            //     default:
            //     case TextUVMapping.Character: {
            //         uv0_tl.z = 0;
            //         uv0_tr.z = 0;
            //         uv0_br.z = 1;
            //         uv0_bl.z = 1;
            //
            //         break;
            //     }
            //
            //     case TextUVMapping.Word: {
            //         uv0_tl.z = 0;
            //         uv0_tr.z = 0;
            //         uv0_br.z = 1;
            //         uv0_bl.z = 1;
            //         break;
            //     }
            //
            //     case TextUVMapping.Line: {
            //         uv0_tl.z = 0;
            //         uv0_tr.z = 0;
            //         uv0_br.z = 1;
            //         uv0_bl.z = 1;
            //         break;
            //     }
            //
            //     case TextUVMapping.Bounds: {
            //         uv0_tl.z = 0;
            //         uv0_tr.z = 0;
            //         uv0_br.z = 1;
            //         uv0_bl.z = 1;
            //         break;
            //     }
            // }
            //
            // switch (spanInfo.faceUVModeV) {
            //
            //     default:
            //     case TextUVMapping.Character:
            //         uv0_tl.w = 0;
            //         uv0_tr.w = 1;
            //         uv0_br.w = 1;
            //         uv0_bl.w = 0;
            //         break;
            //
            //     case TextUVMapping.Line: {
            //         uv0_tl.w = 0;
            //         uv0_tr.w = 1;
            //         uv0_br.w = 1;
            //         uv0_bl.w = 0;
            //         break;
            //     }
            //
            //     case TextUVMapping.Word: {
            //         uv0_tl.w = 0;
            //         uv0_tr.w = 1;
            //         uv0_br.w = 1;
            //         uv0_bl.w = 0;
            //         break;
            //     }
            //
            //     case TextUVMapping.Bounds: {
            //         uv0_tl.w = 0;
            //         uv0_tr.w = 1;
            //         uv0_br.w = 1;
            //         uv0_bl.w = 0;
            //         break;
            //     }
            //
            // }
            // }
        }

        private static float PackUV(float x, float y) {
            double x0 = (int) (x * 511);
            double y0 = (int) (y * 511);

            return (float) ((x0 * 4096) + y0);
        }

        private AxisAlignedBounds2D ComputeAABB(float x, float y, float w, float h, float4x4* matrix) {

            float3 p0 = math.transform(*matrix, new float3(x, y, 0));
            float3 p1 = math.transform(*matrix, new float3(x + w, y, 0));
            float3 p2 = math.transform(*matrix, new float3(x + w, y + h, 0));
            float3 p3 = math.transform(*matrix, new float3(x, y + h, 0));

            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;

            if (p0.x < xMin) xMin = p0.x;
            if (p1.x < xMin) xMin = p1.x;
            if (p2.x < xMin) xMin = p2.x;
            if (p3.x < xMin) xMin = p3.x;

            if (p0.x > xMax) xMax = p0.x;
            if (p1.x > xMax) xMax = p1.x;
            if (p2.x > xMax) xMax = p2.x;
            if (p3.x > xMax) xMax = p3.x;

            if (p0.y < yMin) yMin = p0.y;
            if (p1.y < yMin) yMin = p1.y;
            if (p2.y < yMin) yMin = p2.y;
            if (p3.y < yMin) yMin = p3.y;

            if (p0.y > yMax) yMax = p0.y;
            if (p1.y > yMax) yMax = p1.y;
            if (p2.y > yMax) yMax = p2.y;
            if (p3.y > yMax) yMax = p3.y;

            return new AxisAlignedBounds2D(xMin, yMin, xMax, yMax);

        }

    }

}