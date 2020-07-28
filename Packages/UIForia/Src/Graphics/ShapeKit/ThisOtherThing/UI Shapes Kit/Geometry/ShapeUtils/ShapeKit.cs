using System;
using UIForia.Graphics.ShapeKit;
using UIForia.ListTypes;
using UIForia.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public enum DrawMode {

        Normal,
        Shadow

    }

    public struct ShadowData {

        public float size;
        public float softness;

    }

    internal unsafe partial struct ShapeKit {

        private List_float2 unitPositionBuffer;
        private List_float2 cornerBuffer;
        private List_float2 positionBuffer;
        private PointsData scratchPointData;

        private float lastEllipseBaseAngle;
        private float lastEllipseDirection;
        private EdgeGradientData edgeGradientData;
        private float adjustedAA;
        private float baseAA;
        private RangeInt topLeftCornerRange;
        private RangeInt topRightCornerRange;
        private RangeInt bottomLeftCornerRange;
        private RangeInt bottomRightCornerRange;
        private Allocator allocator;
        private DrawMode drawMode;
        private float dpiScale;
        private ShadowData shadowData;

        public ShapeKit(Allocator allocator) : this() {
            this.dpiScale = 1f;
            this.baseAA = 1.25f;
            this.drawMode = DrawMode.Normal;
            this.allocator = allocator;
            this.edgeGradientData = default;
            SetAntiAliasWidth(baseAA);
        }

        public void SetDpiScale(float dpiScale) {
            if (dpiScale <= 0) {
                dpiScale = 1f;
            }

            this.dpiScale = dpiScale;
            SetAntiAliasWidth(baseAA);
        }

        public void SetAntiAliasWidth(float aa) {
            aa = aa < 0 ? 0 : aa;
            baseAA = aa;
            if (dpiScale > 0) {
                adjustedAA = aa * (1.0f / dpiScale);
            }
            else {
                adjustedAA = aa;
            }

            UpdateEdgeGradient();
        }

        public void SetShadow(ShadowData shadowData) {
            this.shadowData = shadowData;
            if (drawMode == DrawMode.Shadow) {
                UpdateEdgeGradient();
            }
        }

        public void SetDrawMode(DrawMode drawMode) {
            if (this.drawMode == drawMode) {
                return;
            }

            this.drawMode = drawMode;
            UpdateEdgeGradient();
        }

        private void UpdateEdgeGradient() {
            if (drawMode == DrawMode.Normal) {
                if (adjustedAA > 0.0f) {
                    edgeGradientData.SetActiveData(1.0f, 0.0f, adjustedAA);
                }
                else {
                    edgeGradientData.Reset();
                }
            }
            else {
                edgeGradientData.SetActiveData(1.0f - shadowData.softness, shadowData.size, adjustedAA);
            }
        }

        public void Dispose() {
            cornerBuffer.Dispose();
            unitPositionBuffer.Dispose();
            positionBuffer.Dispose();
            scratchPointData.Dispose();
        }

        private void AddDecoratedText(int quadCount, ref UIVertexHelper vh, ref TextInfo textInfo, float2 position) { }

        private void AddUndecoratedText(int quadCount, ref UIVertexHelper vh, ref TextInfo textInfo, float2 position) {
            // this does NOT handle strike through / underline / highlight / mark
            // int triangleCount = quadCount * 6;
            // int vertexCount = quadCount * 4;
            //
            // int vertexIndex = 0;
            //
            // vh.EnsureAdditionalVertexCapacity(vertexCount);
            // vh.EnsureAdditionalTriangleCapacity(triangleCount);
            //
            // int currentVertexCount = vh.currentVertCount;
            //
            // Color32 color = textInfo.textStyle.faceColor;
            // float3* positions = vh.positions + currentVertexCount;
            // float4* texCoords = vh.texCoord + currentVertexCount;
            // Color32* colors = vh.colors + currentVertexCount;
            // int* triangles = vh.triangles + vh.TotalTriangleCount;
            //
            // for (int i = 0; i < textInfo.layoutSymbolList.size; i++) {
            //
            //     ref WordInfo wordInfo = ref textInfo.layoutSymbolList[i].wordInfo;
            //     int charStart = wordInfo.charStart;
            //     int charEnd = wordInfo.charEnd;
            //
            //     for (int j = charStart; j < charEnd; j++) {
            //
            //         ref BurstCharInfo charInfo = ref textInfo.symbolList.array[j].charInfo;
            //
            //         ref float3 p0 = ref positions[vertexIndex + 0];
            //         ref float3 p1 = ref positions[vertexIndex + 1];
            //         ref float3 p2 = ref positions[vertexIndex + 2];
            //         ref float3 p3 = ref positions[vertexIndex + 3];
            //
            //         ref float4 uv0 = ref texCoords[vertexIndex + 0];
            //         ref float4 uv1 = ref texCoords[vertexIndex + 1];
            //         ref float4 uv2 = ref texCoords[vertexIndex + 2];
            //         ref float4 uv3 = ref texCoords[vertexIndex + 3];
            //
            //         float charX = position.x + wordInfo.x + charInfo.topLeft.x;
            //         float charY = position.y + wordInfo.y + charInfo.topLeft.y;
            //
            //         float charWidth = charInfo.bottomRight.x - charInfo.topLeft.x;
            //         float charHeight = charInfo.bottomRight.y - charInfo.topLeft.y;
            //
            //         p0.x = charX + charInfo.shearTop;
            //         p0.y = -charY;
            //         p0.z = 0;
            //
            //         p1.x = charX + charWidth + charInfo.shearTop;
            //         p1.y = -charY;
            //         p1.z = 0;
            //
            //         p2.x = charX + charWidth + charInfo.shearBottom;
            //         p2.y = -(charY + charHeight);
            //         p2.z = 0;
            //
            //         p3.x = charX + charInfo.shearBottom;
            //         p3.y = -(charY + charHeight);
            //         p3.z = 0;
            //
            //         uv0.x = charInfo.topLeftUv.x;
            //         uv0.y = charInfo.bottomRightUv.y;
            //         uv0.z = charInfo.topLeftUv.x;
            //         uv0.w = charInfo.bottomRightUv.y;
            //
            //         uv1.x = charInfo.bottomRightUv.x;
            //         uv1.y = charInfo.bottomRightUv.y;
            //         uv1.z = charInfo.bottomRightUv.x;
            //         uv1.w = charInfo.bottomRightUv.y;
            //
            //         uv2.x = charInfo.bottomRightUv.x;
            //         uv2.y = charInfo.topLeftUv.y;
            //         uv2.z = charInfo.bottomRightUv.x;
            //         uv2.w = charInfo.topLeftUv.y;
            //
            //         uv3.x = charInfo.topLeftUv.x;
            //         uv3.y = charInfo.topLeftUv.y;
            //         uv3.z = charInfo.topLeftUv.x;
            //         uv3.w = charInfo.topLeftUv.y;
            //
            //         vertexIndex += 4;
            //     }
            //
            // }
            //
            // // should vectorize
            // for (int i = 0; i < vertexCount; i++) {
            //     colors[i] = color;
            // }
            //
            // vh.AddVertexCount(vertexCount);
            //
            // int startVertex = vh.currentVertCount;
            //
            // int triIdx = 0;
            // for (int i = 0; i < vertexCount; i += 4) {
            //     triangles[triIdx + 0] = startVertex + i + 0;
            //     triangles[triIdx + 1] = startVertex + i + 1;
            //     triangles[triIdx + 2] = startVertex + i + 2;
            //     triangles[triIdx + 3] = startVertex + i + 2;
            //     triangles[triIdx + 4] = startVertex + i + 3;
            //     triangles[triIdx + 5] = startVertex + i + 0;
            //     triIdx += 6;
            // }
            //
            // vh.AddTriangleCount(triangleCount);
        }

        // ShapeKit.ApplyVertexModifier(ref T modifier) {}
        // geometryInfo.ApplyVertexModifier(ref T modifier) {
        // 
        // }

        // public void ApplyVertexModifier(shapeId, ref T modifier) {
        // }

        public void AddTextBackground(ref UIVertexHelper vh, ref TextInfo textInfo, float2 position = default) { }

        public void AddTextForeground(ref UIVertexHelper vh, ref TextInfo textInfo, float2 position = default) { }

        public unsafe void AddTextGlyphs(ref UIVertexHelper vh, ref TextInfo textInfo, float2 position) {
            int quadCount = 0;

            for (int i = 0; i < textInfo.layoutSymbolList.size; i++) {
                ref TextLayoutSymbol symbol = ref textInfo.layoutSymbolList[i];

                if ((symbol.type & TextLayoutSymbolType.Word) == 0 || symbol.wordInfo.type != WordType.Normal) {
                    continue;
                }

                int start = symbol.wordInfo.charStart;
                int end = symbol.wordInfo.charEnd;

                for (int j = start; j < end; j++) {

                    if (textInfo.symbolList[j].type == TextSymbolType.Character || textInfo.symbolList[j].type == TextSymbolType.Sprite) {

                        quadCount++;

                    }
                }

            }

            if (textInfo.isRenderDecorated) {
                AddDecoratedText(quadCount, ref vh, ref textInfo, position);
            }
            else {
                AddUndecoratedText(quadCount, ref vh, ref textInfo, position);
            }

        }

    }

}