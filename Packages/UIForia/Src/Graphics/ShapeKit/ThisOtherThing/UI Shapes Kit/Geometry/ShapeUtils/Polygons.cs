using UIForia.Graphics.ShapeKit;
using UIForia.ListTypes;
using Unity.Mathematics;
using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public unsafe partial struct ShapeKit {

        public void AddPolygon(ref UIVertexHelper vh, ref List_float2 positions, PolygonProperties polygonProperties, PointListProperties pointListProperties, float2 positionOffset, Color32 color) {
            if (scratchPointData.positionBuffer == null) {
                scratchPointData = PointsData.Create(allocator);
            }

            AddPolygon(ref vh, ref positions, polygonProperties, pointListProperties, positionOffset, color, ref scratchPointData);
            scratchPointData.Clear();
        }

        public void AddPolygon(ref UIVertexHelper vh, ref List_float2 positions, PolygonProperties polygonProperties, PointListProperties pointListProperties, float2 positionOffset, Color32 color, ref PointsData pointsData) {

            float4 uv = default;
            float2 adjustedCenter = default;

            if (polygonProperties.polygonCenterType == PolygonCenterTypes.CustomPosition) {
                adjustedCenter.x = polygonProperties.centerOffset.x;
                adjustedCenter.y = polygonProperties.centerOffset.y;
            }
            else {
                for (int i = 0; i < positions.size; i++) {
                    adjustedCenter.x += positions.array[i].x;
                    adjustedCenter.y += positions.array[i].y;
                }

                adjustedCenter.x /= positions.size;
                adjustedCenter.y /= positions.size;
            }

            // if (polygonProperties.polygonCenterType == PolygonCenterTypes.Cutout) {
            //     float safeRotationOffset = polygonProperties.cutoutProperties.RotationOffset;
            //
            //     if (safeRotationOffset < 0.0f) {
            //         safeRotationOffset = GeoUtils.TwoPI + safeRotationOffset;
            //     }
            //
            //     float step = (GeoUtils.TwoPI / polygonProperties.cutoutProperties.Resolution);
            //
            //     safeRotationOffset %= step;
            //     safeRotationOffset -= step * 0.5f;
            //
            //     GeoUtils.SetUnitPositionData(ref CutoutProperties.UnitPositionData, CutoutProperties.Resolution, safeRotationOffset);
            // }

            if (polygonProperties.polygonCenterType == PolygonCenterTypes.Offset || polygonProperties.polygonCenterType == PolygonCenterTypes.Cutout) {
                adjustedCenter.x += polygonProperties.centerOffset.x;
                adjustedCenter.y += polygonProperties.centerOffset.y;
            }

            SetLineData(ref positions, pointListProperties, ref pointsData);

            int numVertices = vh.currentVertCount;
            int firstOuterVertex = vh.currentVertCount + polygonProperties.cutoutProperties.Resolution - 1;

            bool usesCutout = polygonProperties.polygonCenterType == PolygonCenterTypes.Cutout;

            float3 tmpPos = default;

            if (usesCutout) {
                float cutoutOffsetDistance = polygonProperties.cutoutProperties.Radius - edgeGradientData.shadowOffset;
                cutoutOffsetDistance += Mathf.LerpUnclamped(
                    math.length(pointsData.positionNormals[0]) * edgeGradientData.shadowOffset * 3.0f,
                    0.0f,
                    edgeGradientData.innerScale
                );

                for (int i = 0; i < polygonProperties.cutoutProperties.Resolution; i++) {
                    tmpPos.x =
                        adjustedCenter.x +
                        positionOffset.x +
                        polygonProperties.cutoutProperties.UnitPositionData.UnitPositions[i].x * cutoutOffsetDistance;
                    tmpPos.y =
                        adjustedCenter.y +
                        positionOffset.y +
                        polygonProperties.cutoutProperties.UnitPositionData.UnitPositions[i].y * cutoutOffsetDistance;

                    vh.AddVert(tmpPos, color, uv);
                }
            }
            else {
                // add center
                tmpPos.x = adjustedCenter.x + positionOffset.x;
                tmpPos.y = adjustedCenter.y + positionOffset.y;

                vh.AddVert(tmpPos, color, uv);
            }

            // add first position
            tmpPos.x = positionOffset.x + Mathf.LerpUnclamped(
                adjustedCenter.x,
                positions[0].x + pointsData.positionNormals[0].x * edgeGradientData.shadowOffset,
                edgeGradientData.innerScale
            );
            tmpPos.y = positionOffset.y + Mathf.LerpUnclamped(
                adjustedCenter.y,
                positions[0].y + pointsData.positionNormals[0].y * edgeGradientData.shadowOffset,
                edgeGradientData.innerScale
            );
            tmpPos.z = 0.0f;

            vh.AddVert(tmpPos, color, uv);

            for (int i = 1; i < pointsData.totalPositionCount; i++) {
                tmpPos.x = positionOffset.x + Mathf.LerpUnclamped(
                    adjustedCenter.x,
                    positions.array[i].x + pointsData.positionNormals[i].x * edgeGradientData.shadowOffset,
                    edgeGradientData.innerScale
                );
                tmpPos.y = positionOffset.y + Mathf.LerpUnclamped(
                    adjustedCenter.y,
                    positions.array[i].y + pointsData.positionNormals[i].y * edgeGradientData.shadowOffset,
                    edgeGradientData.innerScale
                );

                vh.AddVert(tmpPos, color, uv);

                if (!usesCutout) {
                    vh.AddTriangle(numVertices, numVertices + i, numVertices + i + 1);
                }
            }

            // add cutout indices
            if (usesCutout) {
                for (int i = 1; i < pointsData.totalPositionCount; i++) {
                    vh.AddTriangle(
                        numVertices + GeoUtils.SimpleMap(i, pointsData.totalPositionCount, polygonProperties.cutoutProperties.Resolution),
                        firstOuterVertex + i,
                        firstOuterVertex + i + 1
                    );
                }

                for (int i = 1; i < polygonProperties.cutoutProperties.Resolution; i++) {
                    vh.AddTriangle(
                        numVertices + i,
                        numVertices + i - 1,
                        firstOuterVertex + Mathf.CeilToInt(GeoUtils.SimpleMap(i, polygonProperties.cutoutProperties.Resolution, (float) pointsData.totalPositionCount))
                    );
                }
            }

            // add last triangle
            if (usesCutout) {
                vh.AddTriangle(numVertices, firstOuterVertex + pointsData.totalPositionCount, firstOuterVertex + 1);
                vh.AddTriangle(numVertices, firstOuterVertex, firstOuterVertex + pointsData.totalPositionCount);
            }
            else {
                vh.AddTriangle(numVertices, numVertices + pointsData.totalPositionCount, numVertices + 1);
            }

            if (edgeGradientData.isActive) {
                color.a = 0;

                int outerFirstIndex = numVertices + pointsData.totalPositionCount;

                if (usesCutout) {
                    outerFirstIndex = firstOuterVertex + pointsData.totalPositionCount;
                }
                else {
                    firstOuterVertex = numVertices;
                }

                float offset = edgeGradientData.sizeAdd + edgeGradientData.shadowOffset;

                vh.AddVert(tmpPos, color, uv);

                for (int i = 1; i < pointsData.totalPositionCount; i++) {

                    float2 p = positionOffset + positions[i] + pointsData.positionNormals[i] * offset;
                    tmpPos.x = p.x;
                    tmpPos.y = p.y;
                    vh.AddVert(tmpPos, color, uv);

                    vh.AddTriangle(firstOuterVertex + i + 1, outerFirstIndex + i, outerFirstIndex + i + 1);
                    vh.AddTriangle(firstOuterVertex + i + 1, outerFirstIndex + i + 1, firstOuterVertex + i + 2);
                }

                // fill last outer quad
                vh.AddTriangle(firstOuterVertex + 1, outerFirstIndex, outerFirstIndex + 1);
                vh.AddTriangle(firstOuterVertex + 2, firstOuterVertex + 1, outerFirstIndex + 1);

                if (usesCutout) {
                    float radius = (polygonProperties.cutoutProperties.Radius - offset);
                    for (int i = 0; i < polygonProperties.cutoutProperties.Resolution; i++) {
                        tmpPos.x =
                            adjustedCenter.x +
                            positionOffset.x +
                            polygonProperties.cutoutProperties.UnitPositionData.UnitPositions[i].x * radius;
                        tmpPos.y =
                            adjustedCenter.y +
                            positionOffset.y +
                            polygonProperties.cutoutProperties.UnitPositionData.UnitPositions[i].y * radius;

                        vh.AddVert(tmpPos, color, uv);
                    }

                    for (int i = 1; i < polygonProperties.cutoutProperties.Resolution; i++) {
                        vh.AddTriangle(numVertices + i - 1, numVertices + i, outerFirstIndex + pointsData.totalPositionCount + i);

                        vh.AddTriangle(numVertices + i, outerFirstIndex + pointsData.totalPositionCount + i + 1, outerFirstIndex + pointsData.totalPositionCount + i);
                    }

                    vh.AddTriangle(firstOuterVertex, numVertices, outerFirstIndex + pointsData.totalPositionCount + polygonProperties.cutoutProperties.Resolution);

                    vh.AddTriangle(numVertices, outerFirstIndex + pointsData.totalPositionCount + 1, outerFirstIndex + pointsData.totalPositionCount + polygonProperties.cutoutProperties.Resolution);
                }
            }
        }

    }

}