//#define CENTER_ROUNDED_CAPS

using System;
using UIForia.Graphics.ShapeKit;
using UIForia.ListTypes;
using Unity.Mathematics;
using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public unsafe partial struct ShapeKit {

        private static int GetLineCapResolution(float radius, RoundingResolution roundingResolution) {

            switch (roundingResolution.resolutionType) {

                case ResolutionType.Calculated: {
                    float circumference = GeoUtils.TwoPI * radius;

                    int resolution = (int) math.ceil((circumference / roundingResolution.maxDistance * 0.5f));
                    return resolution < 2 ? 2 : resolution;
                }

                case ResolutionType.Fixed:
                    return roundingResolution.fixedResolution < 2 ? 2 : roundingResolution.fixedResolution;

                default:
                case ResolutionType.Auto: {
                    float circumference = GeoUtils.TwoPI * radius;

                    // todo -- 4 is a shitty number for small radii
                    int resolution = (int) math.ceil((circumference / 4 * 0.5f));
                    return resolution < 2 ? 2 : resolution;
                }
            }
        }

        public void AddLine(ref UIVertexHelper vh, ref List_float2 positions, LineProperties lineProperties, Color32 color, float2 positionOffset = default) {

            if (positions.array == null || positions.size < 2) {
                return;
            }

            if (scratchPointData.positionBuffer == null) {
                scratchPointData = PointsData.Create(allocator);
            }

            AddLine(ref vh, ref positions, lineProperties, color, positionOffset, ref scratchPointData);
            scratchPointData.Clear();
        }

        public void AddLine(ref UIVertexHelper vh, ref List_float2 positions, LineProperties lineProperties, Color32 color, float2 positionOffset, ref PointsData pointsData) {
            if (positions.array == null || positions.size < 2) {
                return;
            }

            pointsData.isClosed = lineProperties.closed && positions.size > 2;

            float centerDistance = GetCenterDistance(lineProperties);

            pointsData.generateRoundedCaps = lineProperties.capType == LineCapTypes.Round;

            pointsData.lineWeight = lineProperties.weight;
            float halfLineWeight = lineProperties.weight * 0.5f;

            if (pointsData.generateRoundedCaps) {
                pointsData.totalCapCount = GetLineCapResolution(lineProperties.weight * 0.5f, lineProperties.roundedCapResolution);
            }

            if (!SetLineData(ref positions, lineProperties.roundingData, ref pointsData)) {
                return;
            }

            // scale uv x for caps
            float uvXMin = 0.0f;
            float uvXLength = 1.0f;

            if (!lineProperties.closed && lineProperties.capType != LineCapTypes.Close) {
                float uvStartOffset = lineProperties.weight / pointsData.totalLength;

                uvXMin = uvStartOffset * 0.5f;
                uvXLength = 1.0f - uvXMin * 2.0f;
            }

            float innerOffset = centerDistance - (halfLineWeight + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;
            float outerOffset = centerDistance + (halfLineWeight + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;

            float capOffsetAmount = 0.0f;

            if (!lineProperties.closed && lineProperties.capType == LineCapTypes.Close) {
                capOffsetAmount = edgeGradientData.shadowOffset * (edgeGradientData.innerScale * 2.0f - 1.0f);
            }

            int numVertices = vh.currentVertCount;
            int startVertex = numVertices - 1;
            int baseIndex;

            float3 tmpPos = default;
            float4 uv = default;

            uv.x = uvXMin + pointsData.normalizedPositionDistances[0] * uvXLength;
            uv.y = 0.0f;

            float2* positionArray = positions.array;

            tmpPos.x = positionOffset.x + positionArray[0].x + pointsData.positionNormals[0].x * innerOffset + pointsData.startCapOffset.x * capOffsetAmount;
            tmpPos.y = positionOffset.y + positionArray[0].y + pointsData.positionNormals[0].y * innerOffset + pointsData.startCapOffset.y * capOffsetAmount;

            vh.AddVert(tmpPos, color, uv);

            uv.y = 1.0f;

            tmpPos.x = positionOffset.x + positionArray[0].x + pointsData.positionNormals[0].x * outerOffset + pointsData.startCapOffset.x * capOffsetAmount;
            tmpPos.y = positionOffset.y + positionArray[0].y + pointsData.positionNormals[0].y * outerOffset + pointsData.startCapOffset.y * capOffsetAmount;

            vh.AddVert(tmpPos, color, uv);

            for (int i = 1; i < pointsData.totalPositionCount - 1; i++) {
                uv.x = uvXMin + pointsData.normalizedPositionDistances[i] * uvXLength;
                uv.y = 0.0f;

                tmpPos.x = positionOffset.x + positionArray[i].x + pointsData.positionNormals[i].x * innerOffset;
                tmpPos.y = positionOffset.y + positionArray[i].y + pointsData.positionNormals[i].y * innerOffset;

                vh.AddVert(tmpPos, color, uv);

                uv.y = 1.0f;

                tmpPos.x = positionOffset.x + positionArray[i].x + pointsData.positionNormals[i].x * outerOffset;
                tmpPos.y = positionOffset.y + positionArray[i].y + pointsData.positionNormals[i].y * outerOffset;

                vh.AddVert(tmpPos, color, uv);

                baseIndex = startVertex + i * 2;
                vh.AddTriangle(baseIndex - 1, baseIndex, baseIndex + 1);
                vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
            }

            // add end vertices
            int endIndex = pointsData.totalPositionCount - 1;
            uv.x = uvXMin + pointsData.normalizedPositionDistances[endIndex] * uvXLength;
            uv.y = 0.0f;

            tmpPos.x = positionOffset.x + positionArray[endIndex].x + pointsData.positionNormals[endIndex].x * innerOffset + pointsData.endCapOffset.x * capOffsetAmount;
            tmpPos.y = positionOffset.y + positionArray[endIndex].y + pointsData.positionNormals[endIndex].y * innerOffset + pointsData.endCapOffset.y * capOffsetAmount;

            vh.AddVert(tmpPos, color, uv);

            uv.y = 1.0f;

            tmpPos.x = positionOffset.x + positionArray[endIndex].x + pointsData.positionNormals[endIndex].x * outerOffset + pointsData.endCapOffset.x * capOffsetAmount;
            tmpPos.y = positionOffset.y + positionArray[endIndex].y + pointsData.positionNormals[endIndex].y * outerOffset + pointsData.endCapOffset.y * capOffsetAmount;

            vh.AddVert(tmpPos, color, uv);

            baseIndex = startVertex + endIndex * 2;
            vh.AddTriangle(baseIndex - 1, baseIndex, baseIndex + 1);
            vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);

            if (lineProperties.closed) {
                uv.x = 1.0f;
                uv.y = 0.0f;

                tmpPos.x = positionOffset.x + positionArray[0].x + pointsData.positionNormals[0].x * innerOffset + pointsData.startCapOffset.x * capOffsetAmount;
                tmpPos.y = positionOffset.y + positionArray[0].y + pointsData.positionNormals[0].y * innerOffset + pointsData.startCapOffset.y * capOffsetAmount;

                vh.AddVert(tmpPos, color, uv);

                uv.y = 1.0f;

                tmpPos.x = positionOffset.x + positionArray[0].x + pointsData.positionNormals[0].x * outerOffset + pointsData.startCapOffset.x * capOffsetAmount;
                tmpPos.y = positionOffset.y + positionArray[0].y + pointsData.positionNormals[0].y * outerOffset + pointsData.startCapOffset.y * capOffsetAmount;

                vh.AddVert(tmpPos, color, uv);

                baseIndex = startVertex + endIndex * 2 + 2;
                vh.AddTriangle(baseIndex - 1, baseIndex, baseIndex + 1);
                vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
            }

            if (edgeGradientData.isActive) {
                byte colorAlpha = color.a;

                innerOffset = centerDistance - (halfLineWeight + edgeGradientData.shadowOffset);
                outerOffset = centerDistance + (halfLineWeight + edgeGradientData.shadowOffset);

                innerOffset -= edgeGradientData.sizeAdd;
                outerOffset += edgeGradientData.sizeAdd;

                color.a = 0;

                int outerBaseIndex = numVertices + pointsData.totalPositionCount * 2;

                if (lineProperties.closed) {
                    outerBaseIndex += 2;
                }

                uv.x = uvXMin + pointsData.normalizedPositionDistances[0] * uvXLength;
                uv.y = 0.0f;

                tmpPos.x = positionOffset.x + positionArray[0].x + pointsData.positionNormals[0].x * innerOffset;
                tmpPos.y = positionOffset.y + positionArray[0].y + pointsData.positionNormals[0].y * innerOffset;

                vh.AddVert(tmpPos, color, uv);

                uv.y = 1.0f;

                tmpPos.x = positionOffset.x + positionArray[0].x + pointsData.positionNormals[0].x * outerOffset;
                tmpPos.y = positionOffset.y + positionArray[0].y + pointsData.positionNormals[0].y * outerOffset;

                vh.AddVert(tmpPos, color, uv);

                for (int i = 1; i < pointsData.totalPositionCount; i++) {
                    uv.x = uvXMin + pointsData.normalizedPositionDistances[i] * uvXLength;
                    uv.y = 0.0f;

                    tmpPos.x = positionOffset.x + positionArray[i].x + pointsData.positionNormals[i].x * innerOffset;
                    tmpPos.y = positionOffset.y + positionArray[i].y + pointsData.positionNormals[i].y * innerOffset;

                    vh.AddVert(tmpPos, color, uv);

                    uv.y = 1.0f;

                    tmpPos.x = positionOffset.x + positionArray[i].x + pointsData.positionNormals[i].x * outerOffset;
                    tmpPos.y = positionOffset.y + positionArray[i].y + pointsData.positionNormals[i].y * outerOffset;

                    vh.AddVert(tmpPos, color, uv);

                    // inner quad
                    vh.AddTriangle(startVertex + i * 2 - 1, startVertex + i * 2 + 1, outerBaseIndex + i * 2);
                    vh.AddTriangle(startVertex + i * 2 - 1, outerBaseIndex + i * 2, outerBaseIndex + i * 2 - 2);

                    // outer quad
                    vh.AddTriangle(startVertex + i * 2, outerBaseIndex + i * 2 - 1, startVertex + i * 2 + 2);
                    vh.AddTriangle(startVertex + i * 2 + 2, outerBaseIndex + i * 2 - 1, outerBaseIndex + i * 2 + 1);
                }

                if (lineProperties.closed) {
                    int lastIndex = pointsData.totalPositionCount;

                    uv.x = 1.0f;
                    uv.y = 0.0f;

                    tmpPos.x = positionOffset.x + positionArray[0].x + pointsData.positionNormals[0].x * innerOffset;
                    tmpPos.y = positionOffset.y + positionArray[0].y + pointsData.positionNormals[0].y * innerOffset;

                    vh.AddVert(tmpPos, color, uv);

                    uv.y = 1.0f;

                    tmpPos.x = positionOffset.x + positionArray[0].x + pointsData.positionNormals[0].x * outerOffset;
                    tmpPos.y = positionOffset.y + positionArray[0].y + pointsData.positionNormals[0].y * outerOffset;

                    vh.AddVert(tmpPos, color, uv);

                    // inner quad
                    vh.AddTriangle(startVertex + lastIndex * 2 - 1, startVertex + lastIndex * 2 + 1, outerBaseIndex + lastIndex * 2);
                    vh.AddTriangle(startVertex + lastIndex * 2 - 1, outerBaseIndex + lastIndex * 2, outerBaseIndex + lastIndex * 2 - 2);

                    // outer quad
                    vh.AddTriangle(startVertex + lastIndex * 2, outerBaseIndex + lastIndex * 2 - 1, startVertex + lastIndex * 2 + 2);
                    vh.AddTriangle(startVertex + lastIndex * 2 + 2, outerBaseIndex + lastIndex * 2 - 1, outerBaseIndex + lastIndex * 2 + 1);
                }

                color.a = colorAlpha;
            }

            // close line or add caps
            if (!lineProperties.closed) {
                AddStartCap(ref vh, ref positions, lineProperties, positionOffset, color, uv, uvXMin, uvXLength, pointsData);
                AddEndCap(ref vh, ref positions, lineProperties, positionOffset, color, uv, uvXMin, uvXLength, pointsData);
            }
        }

        private void AddStartCap(ref UIVertexHelper vh, ref List_float2 positions, LineProperties lineProperties, float2 positionOffset, Color32 color, float4 uv, float uvXMin, float uvXLength, PointsData pointsData) {
            int currentVertCount = vh.currentVertCount;
            int startIndex = currentVertCount - pointsData.totalPositionCount * 2;

            if (edgeGradientData.isActive) {
                startIndex -= pointsData.totalPositionCount * 2;
            }

            float3 tmpPos2 = default;

            tmpPos2.x = positionOffset.x + positions.array[0].x;
            tmpPos2.y = positionOffset.y + positions.array[0].y;

            switch (lineProperties.capType) {
                case LineCapTypes.Close:
                    AddCloseCap(
                        ref vh,
                        startIndex,
                        tmpPos2,
                        pointsData.positionNormals[0],
                        pointsData.startCapOffset,
                        0,
                        lineProperties,
                        color,
                        uv,
                        pointsData,
                        currentVertCount
                    );

                    break;

                case LineCapTypes.Projected:
                    AddProjectedCap(
                        ref vh,
                        true,
                        startIndex,
                        tmpPos2,
                        pointsData.positionNormals[0],
                        pointsData.startCapOffset,
                        0,
                        lineProperties,
                        color,
                        pointsData.totalPositionCount,
                        currentVertCount
                    );

                    break;

                case LineCapTypes.Round:
                    AddRoundedCap(
                        ref vh,
                        true,
                        startIndex,
                        tmpPos2,
                        pointsData.positionNormals[0],
                        lineProperties,
                        color,
                        pointsData.totalPositionCount,
                        pointsData.totalCapCount,
                        pointsData.StartCapOffsets,
                        pointsData.StartCapUVs,
                        uvXMin,
                        uvXLength,
                        currentVertCount
                    );
                    break;
            }
        }

        private void AddEndCap(ref UIVertexHelper vh, ref List_float2 positions, in LineProperties lineProperties, float2 positionOffset, Color32 color, float4 uv, float uvXMin, float uvXLength, PointsData pointsData) {
            int currentVertCount = vh.currentVertCount;
            int startIndex = currentVertCount;

            if (edgeGradientData.isActive) {
                startIndex -= pointsData.totalPositionCount * 2;
            }

            int lastPositionIndex = pointsData.totalPositionCount - 1;

            float3 tmpPos2 = default;

            tmpPos2.x = positionOffset.x + positions[lastPositionIndex].x;
            tmpPos2.y = positionOffset.y + positions[lastPositionIndex].y;

            switch (lineProperties.capType) {
                case LineCapTypes.Close:

                    startIndex -= 4;

                    AddCloseCap(ref vh, startIndex, tmpPos2, pointsData.positionNormals[lastPositionIndex], pointsData.endCapOffset, 1, lineProperties, color, uv, pointsData, currentVertCount);

                    break;

                case LineCapTypes.Projected:

                    startIndex -= 6;

                    AddProjectedCap(ref vh, false, startIndex, tmpPos2, pointsData.positionNormals[lastPositionIndex], pointsData.endCapOffset, 1, lineProperties, color, pointsData.totalPositionCount, currentVertCount);

                    break;

                case LineCapTypes.Round:
#if CENTER_ROUNDED_CAPS
					startIndex -= pointsData.RoundedCapResolution + 3;
#else
                    startIndex -= pointsData.totalCapCount + 2;
#endif

                    if (edgeGradientData.isActive) {
                        startIndex -= pointsData.totalCapCount;
                    }

                    AddRoundedCap(ref vh, false, startIndex, tmpPos2, pointsData.positionNormals[lastPositionIndex], lineProperties, color, pointsData.totalPositionCount, pointsData.totalCapCount, pointsData.EndCapOffsets, pointsData.EndCapUVs, uvXMin, uvXLength, currentVertCount);

                    break;
            }
        }

        private void AddCloseCap(ref UIVertexHelper vh, int firstVertIndex, in float3 position, float2 normal, float2 capOffset, int invertIndices, in LineProperties lineProperties, Color32 color, float4 uv, PointsData pointsData, int currentVertCount) {
            if (!edgeGradientData.isActive) {
                return;
            }

            int baseIndex = currentVertCount;
            float halfLineWeight = lineProperties.weight * 0.5f;
            float centerDistance = GetCenterDistance(lineProperties);

            float innerOffset = centerDistance - (halfLineWeight + edgeGradientData.shadowOffset) - edgeGradientData.sizeAdd;
            float outerOffset = centerDistance + (halfLineWeight + edgeGradientData.shadowOffset) + edgeGradientData.sizeAdd;

            float capOffsetAmount = edgeGradientData.sizeAdd + edgeGradientData.shadowOffset;

            color.a = 0;

            uv.y = 0.0f;
            float3 tmpPos = default;
            tmpPos.x = position.x + normal.x * innerOffset + capOffset.x * capOffsetAmount;
            tmpPos.y = position.y + normal.y * innerOffset + capOffset.y * capOffsetAmount;

            vh.AddVert(tmpPos, color, uv);

            uv.y = 1.0f;

            tmpPos.x = position.x + normal.x * outerOffset + capOffset.x * capOffsetAmount;
            tmpPos.y = position.y + normal.y * outerOffset + capOffset.y * capOffsetAmount;

            vh.AddVert(tmpPos, color, uv);

            vh.AddTriangle(firstVertIndex, baseIndex + invertIndices, baseIndex + 1 - invertIndices);
            vh.AddTriangle(firstVertIndex + invertIndices, baseIndex + 1, firstVertIndex + 1 - invertIndices);

            int antiAliasedIndex = firstVertIndex + pointsData.totalPositionCount * 2;

            if (invertIndices != 0) {
                vh.AddTriangle(firstVertIndex, baseIndex, antiAliasedIndex);
                vh.AddTriangle(firstVertIndex + 1, antiAliasedIndex + 1, baseIndex + 1);
            }
            else {
                vh.AddTriangle(firstVertIndex, antiAliasedIndex, baseIndex);
                vh.AddTriangle(firstVertIndex + 1, baseIndex + 1, antiAliasedIndex + 1);
            }
        }

        private void AddProjectedCap(ref UIVertexHelper vh, bool isStart, int firstVertIndex, in float3 position, float2 normal, float2 capOffset, int invertIndices, in LineProperties lineProperties, Color32 color, int numPositions, int currentVertCount) {
            int baseIndex = currentVertCount;

            float4 uv = default;

            uv.x = isStart ? 0.0f : 1.0f;

            float3 tmpPos = default;
            float centerDistance = GetCenterDistance(lineProperties);

            float halfLineWeight = lineProperties.weight * 0.5f;

            float innerOffset = centerDistance - (halfLineWeight + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;
            float outerOffset = centerDistance + (halfLineWeight + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;

            float capOffsetAmount = edgeGradientData.shadowOffset + lineProperties.weight * 0.5f;
            capOffsetAmount *= edgeGradientData.innerScale;

            // add lineWeight to position
            tmpPos.x = position.x + normal.x * innerOffset + capOffset.x * capOffsetAmount;
            tmpPos.y = position.y + normal.y * innerOffset + capOffset.y * capOffsetAmount;

            uv.y = 0.0f;
            vh.AddVert(tmpPos, color, uv);

            tmpPos.x = position.x + normal.x * outerOffset + capOffset.x * capOffsetAmount;
            tmpPos.y = position.y + normal.y * outerOffset + capOffset.y * capOffsetAmount;

            uv.y = 1.0f;
            vh.AddVert(tmpPos, color, uv);

            vh.AddTriangle(firstVertIndex, baseIndex + invertIndices, baseIndex + 1 - invertIndices);
            vh.AddTriangle(firstVertIndex + invertIndices, baseIndex + 1, firstVertIndex + 1 - invertIndices);

            if (edgeGradientData.isActive) {
                innerOffset = centerDistance - (halfLineWeight + edgeGradientData.shadowOffset) - edgeGradientData.sizeAdd;
                outerOffset = centerDistance + (halfLineWeight + edgeGradientData.shadowOffset) + edgeGradientData.sizeAdd;

                capOffsetAmount = halfLineWeight + edgeGradientData.sizeAdd + edgeGradientData.shadowOffset;

                color.a = 0;

                tmpPos.x = position.x + normal.x * innerOffset + capOffset.x * capOffsetAmount;
                tmpPos.y = position.y + normal.y * innerOffset + capOffset.y * capOffsetAmount;

                uv.y = 0.0f;
                vh.AddVert(tmpPos, color, uv);

                tmpPos.x = position.x + normal.x * outerOffset + capOffset.x * capOffsetAmount;
                tmpPos.y = position.y + normal.y * outerOffset + capOffset.y * capOffsetAmount;

                uv.y = 1.0f;
                vh.AddVert(tmpPos, color, uv);

                int antiAliasedIndex = firstVertIndex + numPositions * 2;
                baseIndex += 2;

                if (invertIndices != 0) {
                    vh.AddTriangle(firstVertIndex, baseIndex, antiAliasedIndex);
                    vh.AddTriangle(firstVertIndex + 1, antiAliasedIndex + 1, baseIndex + 1);

                    vh.AddTriangle(baseIndex - 2, baseIndex - 1, baseIndex);
                    vh.AddTriangle(baseIndex + 1, baseIndex, baseIndex - 1);

                    vh.AddTriangle(firstVertIndex, baseIndex - 2, baseIndex);
                    vh.AddTriangle(firstVertIndex + 1, baseIndex + 1, baseIndex - 1);
                }
                else {
                    vh.AddTriangle(firstVertIndex, antiAliasedIndex, baseIndex);
                    vh.AddTriangle(firstVertIndex + 1, baseIndex + 1, antiAliasedIndex + 1);

                    vh.AddTriangle(baseIndex - 2, baseIndex, baseIndex - 1);
                    vh.AddTriangle(baseIndex + 1, baseIndex - 1, baseIndex);

                    vh.AddTriangle(firstVertIndex, baseIndex, baseIndex - 2);
                    vh.AddTriangle(firstVertIndex + 1, baseIndex - 1, baseIndex + 1);
                }
            }
        }

        private void AddRoundedCap(ref UIVertexHelper vh, bool isStart, int firstVertIndex, in float3 position, float2 normal, in LineProperties lineProperties, Color32 color, int numPositions, int capCount, float2* capOffsets, float2* uvOffsets, float uvXMin, float uvXLength, int currentVertCount) {
            int baseIndex = currentVertCount;

            float centerDistance = GetCenterDistance(lineProperties);

            float innerOffset = centerDistance;
            float capOffsetAmount = (edgeGradientData.shadowOffset + (lineProperties.weight * 0.5f)) * edgeGradientData.innerScale;

            float4 uv = default;
            if (isStart) {
                uv.x = uvXMin;
            }
            else {
                uv.x = uvXMin + uvXLength;
            }

#if CENTER_ROUNDED_CAPS
			// add center vert
			tmpPos.x = position.x;
			tmpPos.y = position.y;
			uv.y = 0.5f;

			vh.AddVert(tmpPos, color, uv, UI.GeoUtils.ZeroV2, UI.GeoUtils.UINormal, UI.GeoUtils.UITangent);
#endif

            float3 tmpPos = default;
            for (int i = 0; i < capCount; i++) {
                tmpPos.x = position.x + normal.x * innerOffset + capOffsets[i].x * capOffsetAmount;
                tmpPos.y = position.y + normal.y * innerOffset + capOffsets[i].y * capOffsetAmount;

                uv.x = isStart ? Mathf.LerpUnclamped(uvXMin, 0.0f, uvOffsets[i].x) : Mathf.LerpUnclamped(uvXMin + uvXLength, 1.0f, uvOffsets[i].x);

                uv.y = uvOffsets[i].y;

                vh.AddVert(tmpPos, color, uv);

                if (i > 0) {
#if CENTER_ROUNDED_CAPS
					vh.AddTriangle(baseIndex, baseIndex + i - 1, baseIndex + i);
#else
                    vh.AddTriangle(firstVertIndex, baseIndex + i - 1, baseIndex + i);
#endif
                }
            }

            // last fans
            if (isStart) {
#if CENTER_ROUNDED_CAPS
				// starting triangle
				vh.AddTriangle(baseIndex + 1, baseIndex, firstVertIndex);
				
				// end triangles
				vh.AddTriangle(baseIndex, baseIndex + capOffsets.Length - 1, baseIndex + capOffsets.Length);
				vh.AddTriangle(baseIndex, baseIndex + capOffsets.Length, firstVertIndex + 1);
#else
                vh.AddTriangle(baseIndex + capCount - 1, firstVertIndex + 1, firstVertIndex);
#endif
            }
            else {
#if CENTER_ROUNDED_CAPS
				// starting triangle
				vh.AddTriangle(baseIndex + 1, baseIndex, firstVertIndex + 1);

				// end triangles
				vh.AddTriangle(baseIndex, baseIndex + capOffsets.Length - 1, baseIndex + capOffsets.Length);
				vh.AddTriangle(baseIndex, baseIndex + capOffsets.Length, firstVertIndex);
#else
                vh.AddTriangle(baseIndex, firstVertIndex, firstVertIndex + 1);
#endif
            }

            if (edgeGradientData.isActive) {
                color.a = 0;

                innerOffset = centerDistance;

                capOffsetAmount = lineProperties.weight * 0.5f + edgeGradientData.sizeAdd + edgeGradientData.shadowOffset;

                int antiAliasedIndex = firstVertIndex + numPositions * 2;

                for (int i = 0; i < capCount; i++) {
                    tmpPos.x = position.x + normal.x * innerOffset + capOffsets[i].x * capOffsetAmount;
                    tmpPos.y = position.y + normal.y * innerOffset + capOffsets[i].y * capOffsetAmount;

                    if (isStart) {
                        uv.x = Mathf.LerpUnclamped(uvXMin, 0.0f, uvOffsets[i].x);
                    }
                    else {
                        uv.x = Mathf.LerpUnclamped(uvXMin + uvXLength, 1.0f, uvOffsets[i].x);
                    }

                    uv.y = uvOffsets[i].y;

                    vh.AddVert(tmpPos, color, uv);

                    if (i > 0) {
                        vh.AddTriangle(baseIndex + i - 1, baseIndex + capCount + i - 1, baseIndex + i);
                        vh.AddTriangle(baseIndex + capCount + i, baseIndex + i, baseIndex + capCount + i - 1);
                    }
                }

                if (!isStart) {
                    vh.AddTriangle(baseIndex, firstVertIndex + 1, antiAliasedIndex + 1);
                    vh.AddTriangle(antiAliasedIndex + 1, baseIndex + capCount, baseIndex);

                    vh.AddTriangle(baseIndex + capCount * 2 - 1, antiAliasedIndex, firstVertIndex);
                    vh.AddTriangle(baseIndex + capCount - 1, baseIndex + capCount * 2 - 1, firstVertIndex);
                }
                else {
                    vh.AddTriangle(firstVertIndex + 1, baseIndex + capCount - 1, baseIndex + capCount * 2 - 1);
                    vh.AddTriangle(antiAliasedIndex + 1, firstVertIndex + 1, baseIndex + capCount * 2 - 1);

                    vh.AddTriangle(antiAliasedIndex, baseIndex, firstVertIndex);
                    vh.AddTriangle(baseIndex + capCount, baseIndex, antiAliasedIndex);
                }
            }
        }

    }

}