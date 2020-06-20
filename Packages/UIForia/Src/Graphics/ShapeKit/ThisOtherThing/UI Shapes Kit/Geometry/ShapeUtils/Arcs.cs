using System;
using UIForia.Graphics.ShapeKit;
using Unity.Mathematics;
using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public unsafe partial struct ShapeKit {

        public void AddArc(ref UIVertexHelper vh, float width, float height, in ArcProperties arcProperties) {

            float baseAngle = arcProperties.baseAngle;
            float2 radius = GetRadius(width, height, EllipseFitting.Ellipse);

            PointListGeneratorData generatorData = new PointListGeneratorData();
            // GetDirectionAndBaseAngle(arcProperties, ref baseAngle, out float direction);
            // generatorData.width = width;
            // generatorData.height = height;
            // generatorData.direction = GetArcDirection();
            // generatorData.startOffset;

        }

        private static float GetArcDirection(ArcDirection direction) {
            switch (direction) {

                case ArcDirection.Backward:
                    return -1f;

                case ArcDirection.Centered:
                    return -1f;

                default:
                case ArcDirection.Forward:
                    return 1f;
            }
        }

        public void AddSector(ref UIVertexHelper vh, float2 center, float2 radius, ArcProperties arcProperties, Color32 color) {
            if (arcProperties.length <= 0.0f) {
                return;
            }

            float2 r = GetRadius(radius.x, radius.y, EllipseFitting.Ellipse);

            float baseAngle = arcProperties.baseAngle;

            GetDirectionAndBaseAngle(arcProperties, ref baseAngle, out float direction);

            float adjustedBaseAngle = baseAngle * Mathf.PI;

            int resolution = ResolveEllipseResolution(r, arcProperties.resolution);
            SetUnitPositionData(resolution, adjustedBaseAngle, direction);

            resolution = (int) (math.ceil(resolution * arcProperties.length));

            bool isReversed = arcProperties.direction == ArcDirection.Backward;

            int reversedZeroForwardMinus = isReversed ? 0 : -1;
            int reversedMinusForwardZero = isReversed ? -1 : 0;

            int reversed1Forward2 = isReversed ? 1 : 2;
            int reversed2Forward1 = isReversed ? 2 : 1;

            int reversed1Forward0 = isReversed ? 1 : 0;
            int reversed0Forward1 = isReversed ? 0 : 1;

            float endSegmentAngle = adjustedBaseAngle + ((Mathf.PI * 2.0f) * arcProperties.length) * direction;

            float2 endSegmentUnitPosition = new float2(
                math.sin(endSegmentAngle),
                math.cos(endSegmentAngle)
            );

            float2 endTangent = new float2(
                endSegmentUnitPosition.y * direction,
                endSegmentUnitPosition.x * -direction
            );
            
            float2 startTangent = new float2(
                math.cos(adjustedBaseAngle) * -direction,
                math.sin(adjustedBaseAngle) * direction
            );

            float centerAngle = adjustedBaseAngle + (Mathf.PI * arcProperties.length) * direction;
            float lengthScaler = 1.0f / math.sin(Mathf.PI * arcProperties.length);
            lengthScaler = Mathf.Min(4.0f, lengthScaler);

            float3 centerNormal = new float3(
                -math.sin(centerAngle) * lengthScaler,
                -math.cos(centerAngle) * lengthScaler,
                0
            );

            int numVertices = vh.currentVertCount;
            float2 tmpOuterRadius = default;
            float3 tmpOffsetCenter = default;

            tmpOuterRadius.x = (radius.x + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;
            tmpOuterRadius.y = (radius.y + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;

            float capsExtensionLength = edgeGradientData.shadowOffset * edgeGradientData.innerScale;

            tmpOffsetCenter.x = center.x + centerNormal.x * radius.x * (edgeGradientData.innerScale - 1.0f) * 0.2f;
            tmpOffsetCenter.y = center.y + centerNormal.y * radius.y * (edgeGradientData.innerScale - 1.0f) * 0.2f;
            tmpOffsetCenter.z = 0.0f;

            if (arcProperties.length >= 1.0f) {
                capsExtensionLength = 0.0f;
                tmpOffsetCenter.x = center.x;
                tmpOffsetCenter.y = center.y;
            }

            float4 uv = default; // todo -- need to map these according to positions

            vh.AddVert(tmpOffsetCenter + centerNormal * capsExtensionLength, color, uv);

            Vector3 tmpPosition = default;

            tmpPosition.x = tmpOffsetCenter.x + unitPositionBuffer.array[0].x * tmpOuterRadius.x + startTangent.x * capsExtensionLength;
            tmpPosition.y = tmpOffsetCenter.y + unitPositionBuffer.array[0].y * tmpOuterRadius.y + startTangent.y * capsExtensionLength;
            tmpPosition.z = tmpOffsetCenter.z;

            vh.AddVert(tmpPosition, color, uv);
            for (int i = 1; i < resolution; i++) {
                tmpPosition.x = tmpOffsetCenter.x + unitPositionBuffer.array[i].x * tmpOuterRadius.x;
                tmpPosition.y = tmpOffsetCenter.y + unitPositionBuffer.array[i].y * tmpOuterRadius.y;
                tmpPosition.z = tmpOffsetCenter.z;

                vh.AddVert(tmpPosition, color, uv);

                vh.AddTriangle(numVertices, numVertices + i + reversedZeroForwardMinus, numVertices + i + reversedMinusForwardZero);

            }

            int lastFullIndex = numVertices + resolution;

            // add last partial segment
            tmpPosition.x = tmpOffsetCenter.x + endSegmentUnitPosition.x * tmpOuterRadius.x + endTangent.x * capsExtensionLength;
            tmpPosition.y = tmpOffsetCenter.y + endSegmentUnitPosition.y * tmpOuterRadius.y + endTangent.y * capsExtensionLength;
            
            vh.AddVert(tmpPosition, color, uv);

            if (isReversed) {
                vh.AddTriangle(numVertices, lastFullIndex, lastFullIndex - 1);
                vh.AddTriangle(numVertices, lastFullIndex + 1, lastFullIndex);
            }
            else {
                vh.AddTriangle(numVertices, lastFullIndex - 1, lastFullIndex);
                vh.AddTriangle(numVertices, lastFullIndex, lastFullIndex + 1);
            }

            if (edgeGradientData.isActive) {
                radius.x += edgeGradientData.sizeAdd + edgeGradientData.shadowOffset;
                radius.y += edgeGradientData.sizeAdd + edgeGradientData.shadowOffset;
                color.a = 0;

                tmpPosition.x = center.x + unitPositionBuffer.array[0].x * radius.x;
                tmpPosition.y = center.y + unitPositionBuffer.array[0].y * radius.y;
                tmpPosition.z = 0.0f;
                
                vh.AddVert(tmpPosition, color, uv);
                
                for (int i = 1; i <= resolution; i++) {
                    if (i < resolution) {
                        tmpPosition.x = center.x + unitPositionBuffer.array[i].x * radius.x;
                        tmpPosition.y = center.y + unitPositionBuffer.array[i].y * radius.y;
                        tmpPosition.z = 0.0f;
                        vh.AddVert(tmpPosition, color, uv);
                    }
                    else {
                        tmpPosition.x = center.x + endSegmentUnitPosition.x * radius.x;
                        tmpPosition.y = center.y + endSegmentUnitPosition.y * radius.y;
                        tmpPosition.z = 0.0f;
                        vh.AddVert(tmpPosition, color, uv);
                    }

                    int innerBase = numVertices + i;
                    int outerBase = innerBase + resolution;

                    vh.AddTriangle(outerBase + 2, innerBase + reversed0Forward1, innerBase + reversed1Forward0);
                    vh.AddTriangle(innerBase, outerBase + reversed2Forward1, outerBase + reversed1Forward2);
                }

                if (arcProperties.length >= 1.0f) {
                    return;
                }

                tmpOuterRadius.x = edgeGradientData.sizeAdd + edgeGradientData.shadowOffset;
                tmpOuterRadius.y = tmpOuterRadius.x;

                // add start outer vertex
                tmpPosition.x = center.x + unitPositionBuffer.array[0].x * radius.x + startTangent.x * tmpOuterRadius.x;
                tmpPosition.y = center.y + unitPositionBuffer.array[0].y * radius.y + startTangent.y * tmpOuterRadius.y;
                tmpPosition.z = 0.0f;
                vh.AddVert(tmpPosition, color, uv);

                // add end outer vertex
                tmpPosition.x = center.x + endSegmentUnitPosition.x * radius.x + endTangent.x * tmpOuterRadius.x;
                tmpPosition.y = center.y + endSegmentUnitPosition.y * radius.y + endTangent.y * tmpOuterRadius.y;
                tmpPosition.z = 0.0f;

                vh.AddVert(tmpPosition, color, uv);

                radius.x -= edgeGradientData.sizeAdd;
                radius.y -= edgeGradientData.sizeAdd;

                // add start inner vertex
                tmpPosition.x = center.x + unitPositionBuffer.array[0].x * radius.x + startTangent.x * tmpOuterRadius.x;
                tmpPosition.y = center.y + unitPositionBuffer.array[0].y * radius.y + startTangent.y * tmpOuterRadius.y;
                tmpPosition.z = 0.0f;

                vh.AddVert(tmpPosition, color, uv);

                // add end inner vertex
                tmpPosition.x = center.x + endSegmentUnitPosition.x * radius.x + endTangent.x * tmpOuterRadius.x;
                tmpPosition.y = center.y + endSegmentUnitPosition.y * radius.y + endTangent.y * tmpOuterRadius.y;
                tmpPosition.z = 0.0f;
                vh.AddVert(tmpPosition, color, uv);

                // add center extruded vertex
                tmpPosition.x = center.x + centerNormal.x * tmpOuterRadius.x;
                tmpPosition.y = center.y + centerNormal.y * tmpOuterRadius.y;
                tmpPosition.z = 0.0f;
                vh.AddVert(tmpPosition, color, uv);

                int baseCornersIndex = vh.currentVertCount - 5;
                int baseOuterIndex = numVertices + resolution;
                int secondOuterIndex = numVertices + resolution * 2;

                if (isReversed) {
                    // start corner
                    vh.AddTriangle(baseCornersIndex, baseCornersIndex + 2, numVertices + 1);
                    vh.AddTriangle(baseCornersIndex, numVertices + 1, baseOuterIndex + 2);

                    // end corner
                    vh.AddTriangle(baseCornersIndex + 1, baseOuterIndex + 1, baseCornersIndex + 3);
                    vh.AddTriangle(baseCornersIndex + 1, secondOuterIndex + 2, baseOuterIndex + 1);

                    // start corner to center
                    vh.AddTriangle(baseCornersIndex + 2, numVertices, numVertices + 1);
                    vh.AddTriangle(baseCornersIndex + 2, baseCornersIndex + 4, numVertices);

                    // end corner to center
                    vh.AddTriangle(baseCornersIndex + 3, baseOuterIndex + 1, baseCornersIndex + 4);
                    vh.AddTriangle(baseCornersIndex + 4, baseOuterIndex + 1, numVertices);
                }
                else {
                    // start corner
                    vh.AddTriangle(baseCornersIndex, numVertices + 1, baseCornersIndex + 2);
                    vh.AddTriangle(baseCornersIndex, baseOuterIndex + 2, numVertices + 1);

                    // end corner
                    vh.AddTriangle(baseCornersIndex + 1, baseCornersIndex + 3, baseOuterIndex + 1);
                    vh.AddTriangle(baseCornersIndex + 1, baseOuterIndex + 1, secondOuterIndex + 2);

                    // start corner to center
                    vh.AddTriangle(baseCornersIndex + 2, numVertices + 1, numVertices);
                    vh.AddTriangle(baseCornersIndex + 2, numVertices, baseCornersIndex + 4);

                    // end corner to center
                    vh.AddTriangle(baseCornersIndex + 3, baseCornersIndex + 4, baseOuterIndex + 1);
                    vh.AddTriangle(baseCornersIndex + 4, numVertices, baseOuterIndex + 1);
                }
            }
        }

        public unsafe void AddSectorOutline(ref UIVertexHelper vh, Vector2 center, Vector2 radius, ArcProperties arcProperties, OutlineProperties outlineProperties, Color32 color) {
            if (arcProperties.length <= 0.0f) {
                return;
            }

            float centerDistance = GetCenterDistance(outlineProperties);
            radius.x += centerDistance;
            radius.y += centerDistance;

            float2 r = GetRadius(radius.x, radius.y, EllipseFitting.Ellipse);

            float baseAngle = arcProperties.baseAngle;

            GetDirectionAndBaseAngle(arcProperties, ref baseAngle, out float direction);

            float adjustedBaseAngle = baseAngle * Mathf.PI;

            int resolution = ResolveEllipseResolution(r, arcProperties.resolution);
            SetUnitPositionData(resolution, adjustedBaseAngle, direction);

            resolution = (int) (math.ceil(resolution * arcProperties.length));

            float endSegmentAngle = adjustedBaseAngle + ((Mathf.PI * 2.0f) * arcProperties.length) * direction;

            float4 uv = default;

            Vector3 endSegmentUnitPosition = new Vector3(
                math.sin(endSegmentAngle),
                math.cos(endSegmentAngle)
            );

            Vector3 endTangent = new Vector3(
                endSegmentUnitPosition.y * direction,
                endSegmentUnitPosition.x * -direction
            );
            Vector3 startTangent = new Vector3(
                math.cos(adjustedBaseAngle) * -direction,
                math.sin(adjustedBaseAngle) * direction
            );

            float centerAngle = adjustedBaseAngle + (Mathf.PI * arcProperties.length) * direction;
            float lengthScaler = 1.0f / math.sin(Mathf.PI * arcProperties.length);
            lengthScaler = Mathf.Min(4.0f, lengthScaler);

            Vector3 centerNormal = new Vector3(
                -math.sin(centerAngle) * lengthScaler,
                -math.cos(centerAngle) * lengthScaler
            );

            float halfLineWeight = outlineProperties.weight * 0.5f;
            float halfLineWeightOffset = (halfLineWeight + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;
            Vector3 tmpInnerRadius = default;
            Vector3 tmpOuterRadius = default;
            Vector3 tmpPosition = default;
            Vector3 tmpArcInnerRadius = default;
            Vector3 tmpArcOuterRadius = default;

            if (arcProperties.direction == ArcDirection.Backward) {
                tmpInnerRadius.x = radius.x + halfLineWeightOffset;
                tmpInnerRadius.y = radius.y + halfLineWeightOffset;

                tmpOuterRadius.x = radius.x - halfLineWeightOffset;
                tmpOuterRadius.y = radius.y - halfLineWeightOffset;
            }
            else {
                tmpInnerRadius.x = radius.x - halfLineWeightOffset;
                tmpInnerRadius.y = radius.y - halfLineWeightOffset;

                tmpOuterRadius.x = radius.x + halfLineWeightOffset;
                tmpOuterRadius.y = radius.y + halfLineWeightOffset;
            }

            float capsExtensionLength = edgeGradientData.shadowOffset * edgeGradientData.innerScale;

            if (arcProperties.length >= 1.0f) {
                capsExtensionLength = 0.0f;
            }

            int numVertices = vh.currentVertCount;
            int startVertex = numVertices - 1;

            int baseIndex;
            tmpPosition.x = center.x + unitPositionBuffer.array[0].x * tmpInnerRadius.x + startTangent.x * capsExtensionLength;
            tmpPosition.y = center.y + unitPositionBuffer.array[0].y * tmpInnerRadius.y + startTangent.y * capsExtensionLength;
            tmpPosition.z = 0.0f;
            vh.AddVert(tmpPosition, color, uv);

            tmpPosition.x = center.x + unitPositionBuffer.array[0].x * tmpOuterRadius.x + startTangent.x * capsExtensionLength;
            tmpPosition.y = center.y + unitPositionBuffer.array[0].y * tmpOuterRadius.y + startTangent.y * capsExtensionLength;
            tmpPosition.z = 0.0f;
            vh.AddVert(tmpPosition, color, uv);

            for (int i = 1; i < resolution; i++) {
                tmpPosition.x = center.x + unitPositionBuffer.array[i].x * tmpInnerRadius.x;
                tmpPosition.y = center.y + unitPositionBuffer.array[i].y * tmpInnerRadius.y;
                tmpPosition.z = 0.0f;

                vh.AddVert(tmpPosition, color, uv);

                tmpPosition.x = center.x + unitPositionBuffer.array[i].x * tmpOuterRadius.x;
                tmpPosition.y = center.y + unitPositionBuffer.array[i].y * tmpOuterRadius.y;
                tmpPosition.z = 0.0f;
                vh.AddVert(tmpPosition, color, uv);

                baseIndex = startVertex + i * 2;
                vh.AddTriangle(baseIndex - 1, baseIndex, baseIndex + 1);
                vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
            }

            // add last partial segment
            tmpPosition.x = center.x + endSegmentUnitPosition.x * tmpInnerRadius.x + endTangent.x * capsExtensionLength;
            tmpPosition.y = center.y + endSegmentUnitPosition.y * tmpInnerRadius.y + endTangent.y * capsExtensionLength;
            tmpPosition.z = 0.0f;
            vh.AddVert(tmpPosition, color, uv);

            tmpPosition.x = center.x + endSegmentUnitPosition.x * tmpOuterRadius.x + endTangent.x * capsExtensionLength;
            tmpPosition.y = center.y + endSegmentUnitPosition.y * tmpOuterRadius.y + endTangent.y * capsExtensionLength;
            tmpPosition.z = 0.0f;
            vh.AddVert(tmpPosition, color, uv);

            baseIndex = startVertex + resolution * 2;
            vh.AddTriangle(baseIndex - 1, baseIndex, baseIndex + 1);
            vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);

            if (edgeGradientData.isActive) {
                halfLineWeightOffset = halfLineWeight + edgeGradientData.shadowOffset + edgeGradientData.sizeAdd;

                if (arcProperties.direction == ArcDirection.Backward) {
                    tmpOuterRadius.x = radius.x - halfLineWeightOffset;
                    tmpOuterRadius.y = radius.y - halfLineWeightOffset;

                    tmpInnerRadius.x = radius.x + halfLineWeightOffset;
                    tmpInnerRadius.y = radius.y + halfLineWeightOffset;
                }
                else {
                    tmpOuterRadius.x = radius.x + halfLineWeightOffset;
                    tmpOuterRadius.y = radius.y + halfLineWeightOffset;

                    tmpInnerRadius.x = radius.x - halfLineWeightOffset;
                    tmpInnerRadius.y = radius.y - halfLineWeightOffset;
                }

                color.a = 0;

                int edgesBaseIndex;
                int innerBaseIndex;

                // ensure inner vertices don't overlap
                tmpArcInnerRadius.x = Mathf.Max(0.0f, tmpInnerRadius.x);
                tmpArcInnerRadius.y = Mathf.Max(0.0f, tmpInnerRadius.y);

                tmpArcOuterRadius.x = Mathf.Max(0.0f, tmpOuterRadius.x);
                tmpArcOuterRadius.y = Mathf.Max(0.0f, tmpOuterRadius.y);

                float3 noOverlapInnerOffset = default;
                float3 noOverlapOuterOffset = default;
                noOverlapInnerOffset.x = centerNormal.x * -Mathf.Min(0.0f, tmpInnerRadius.x);
                noOverlapInnerOffset.y = centerNormal.y * -Mathf.Min(0.0f, tmpInnerRadius.y);
                noOverlapInnerOffset.z = 0.0f;

                noOverlapOuterOffset.x = centerNormal.x * -Mathf.Min(0.0f, tmpOuterRadius.x);
                noOverlapOuterOffset.y = centerNormal.y * -Mathf.Min(0.0f, tmpOuterRadius.y);
                noOverlapOuterOffset.z = 0.0f;

                if (arcProperties.length >= 1.0f) {
                    noOverlapInnerOffset.x = 0.0f;
                    noOverlapInnerOffset.y = 0.0f;
                    noOverlapInnerOffset.z = 0.0f;

                    noOverlapOuterOffset.x = 0.0f;
                    noOverlapOuterOffset.y = 0.0f;
                    noOverlapOuterOffset.z = 0.0f;
                }

                for (int i = 0; i < resolution; i++) {
                    tmpPosition.x = center.x + unitPositionBuffer.array[i].x * tmpArcInnerRadius.x + noOverlapInnerOffset.x;
                    tmpPosition.y = center.y + unitPositionBuffer.array[i].y * tmpArcInnerRadius.y + noOverlapInnerOffset.y;
                    tmpPosition.z = noOverlapInnerOffset.z;

                    vh.AddVert(tmpPosition, color, uv);

                    tmpPosition.x = center.x + unitPositionBuffer.array[i].x * tmpArcOuterRadius.x + noOverlapOuterOffset.x;
                    tmpPosition.y = center.y + unitPositionBuffer.array[i].y * tmpArcOuterRadius.y + noOverlapOuterOffset.y;
                    tmpPosition.z = noOverlapOuterOffset.z;
                    vh.AddVert(tmpPosition, color, uv);

                    edgesBaseIndex = baseIndex + i * 2;
                    innerBaseIndex = startVertex + i * 2;

                    if (i > 0) {
                        // inner quad
                        vh.AddTriangle(innerBaseIndex - 1, innerBaseIndex + 1, edgesBaseIndex + 3);
                        vh.AddTriangle(edgesBaseIndex + 1, innerBaseIndex - 1, edgesBaseIndex + 3);

                        // outer quad
                        vh.AddTriangle(innerBaseIndex, edgesBaseIndex + 2, innerBaseIndex + 2);
                        vh.AddTriangle(edgesBaseIndex + 2, edgesBaseIndex + 4, innerBaseIndex + 2);
                    }
                }

                // add partial segment antiAliasing
                tmpPosition.x = center.x + endSegmentUnitPosition.x * tmpArcInnerRadius.x + noOverlapInnerOffset.x;
                tmpPosition.y = center.y + endSegmentUnitPosition.y * tmpArcInnerRadius.y + noOverlapInnerOffset.y;
                tmpPosition.z = noOverlapInnerOffset.z;
                vh.AddVert(tmpPosition, color, uv);

                tmpPosition.x = center.x + endSegmentUnitPosition.x * tmpArcOuterRadius.x + noOverlapOuterOffset.x;
                tmpPosition.y = center.y + endSegmentUnitPosition.y * tmpArcOuterRadius.y + noOverlapOuterOffset.y;
                tmpPosition.z = noOverlapOuterOffset.z;
                vh.AddVert(tmpPosition, color, uv);

                edgesBaseIndex = baseIndex + resolution * 2;
                innerBaseIndex = startVertex + resolution * 2;

                // inner quad
                vh.AddTriangle(innerBaseIndex - 1, innerBaseIndex + 1, edgesBaseIndex + 3);
                vh.AddTriangle(edgesBaseIndex + 1, innerBaseIndex - 1, edgesBaseIndex + 3);

                // outer quad
                vh.AddTriangle(innerBaseIndex, edgesBaseIndex + 2, innerBaseIndex + 2);
                vh.AddTriangle(edgesBaseIndex + 2, edgesBaseIndex + 4, innerBaseIndex + 2);

                // skip end antiAliasing if full ring is being generated
                if (arcProperties.length >= 1.0f) {
                    return;
                }

                capsExtensionLength = edgeGradientData.sizeAdd + edgeGradientData.shadowOffset;

                // add start outer antiAliasing
                tmpPosition.x = center.x + unitPositionBuffer.array[0].x * tmpInnerRadius.x + startTangent.x * capsExtensionLength;
                tmpPosition.y = center.y + unitPositionBuffer.array[0].y * tmpInnerRadius.y + startTangent.y * capsExtensionLength;
                tmpPosition.z = startTangent.z * capsExtensionLength;
                vh.AddVert(tmpPosition, color, uv);

                tmpPosition.x = center.x + unitPositionBuffer.array[0].x * tmpOuterRadius.x + startTangent.x * capsExtensionLength;
                tmpPosition.y = center.y + unitPositionBuffer.array[0].y * tmpOuterRadius.y + startTangent.y * capsExtensionLength;
                tmpPosition.z = 0.0f;
                vh.AddVert(tmpPosition, color, uv);

                // add end outer antiAliasing
                tmpPosition.x = center.x + endSegmentUnitPosition.x * tmpInnerRadius.x + endTangent.x * capsExtensionLength;
                tmpPosition.y = center.y + endSegmentUnitPosition.y * tmpInnerRadius.y + endTangent.y * capsExtensionLength;
                tmpPosition.z = endTangent.z * capsExtensionLength;
                vh.AddVert(tmpPosition, color, uv);

                tmpPosition.x = center.x + endSegmentUnitPosition.x * tmpOuterRadius.x + endTangent.x * capsExtensionLength;
                tmpPosition.y = center.y + endSegmentUnitPosition.y * tmpOuterRadius.y + endTangent.y * capsExtensionLength;
                tmpPosition.z = endTangent.z * capsExtensionLength;
                vh.AddVert(tmpPosition, color, uv);

                if (arcProperties.direction == ArcDirection.Backward) {
                    tmpOuterRadius.x += edgeGradientData.sizeAdd;
                    tmpOuterRadius.y += edgeGradientData.sizeAdd;

                    tmpInnerRadius.x -= edgeGradientData.sizeAdd;
                    tmpInnerRadius.y -= edgeGradientData.sizeAdd;
                }
                else {
                    tmpOuterRadius.x -= edgeGradientData.sizeAdd;
                    tmpOuterRadius.y -= edgeGradientData.sizeAdd;

                    tmpInnerRadius.x += edgeGradientData.sizeAdd;
                    tmpInnerRadius.y += edgeGradientData.sizeAdd;
                }

                // add start inner antiAliasing
                tmpPosition.x = center.x + unitPositionBuffer.array[0].x * tmpInnerRadius.x + startTangent.x * capsExtensionLength;
                tmpPosition.y = center.y + unitPositionBuffer.array[0].y * tmpInnerRadius.y + startTangent.y * capsExtensionLength;
                tmpPosition.z = startTangent.z * capsExtensionLength;
                vh.AddVert(tmpPosition, color, uv);

                tmpPosition.x = center.x + unitPositionBuffer.array[0].x * tmpOuterRadius.x + startTangent.x * capsExtensionLength;
                tmpPosition.y = center.y + unitPositionBuffer.array[0].y * tmpOuterRadius.y + startTangent.y * capsExtensionLength;
                tmpPosition.z = startTangent.z * capsExtensionLength;
                vh.AddVert(tmpPosition, color, uv);

                // add end inner antiAliasing
                tmpPosition.x = center.x + endSegmentUnitPosition.x * tmpInnerRadius.x + endTangent.x * capsExtensionLength;
                tmpPosition.y = center.y + endSegmentUnitPosition.y * tmpInnerRadius.y + endTangent.y * capsExtensionLength;
                tmpPosition.z = endTangent.z * capsExtensionLength;
                vh.AddVert(tmpPosition, color, uv);

                tmpPosition.x = center.x + endSegmentUnitPosition.x * tmpOuterRadius.x + endTangent.x * capsExtensionLength;
                tmpPosition.y = center.y + endSegmentUnitPosition.y * tmpOuterRadius.y + endTangent.y * capsExtensionLength;
                tmpPosition.z = endTangent.z * capsExtensionLength;
                vh.AddVert(tmpPosition, color, uv);

                int currentVertCount = vh.currentVertCount;

                // add end antiAliasing triangles

                // center
                vh.AddTriangle(currentVertCount - 1, currentVertCount - 2, innerBaseIndex + 1);
                vh.AddTriangle(currentVertCount - 1, innerBaseIndex + 1, innerBaseIndex + 2);

                // inner
                vh.AddTriangle(edgesBaseIndex + 3, innerBaseIndex + 1, currentVertCount - 6);
                vh.AddTriangle(currentVertCount - 6, innerBaseIndex + 1, currentVertCount - 2);

                // outer
                vh.AddTriangle(edgesBaseIndex + 4, currentVertCount - 5, innerBaseIndex + 2);
                vh.AddTriangle(currentVertCount - 5, currentVertCount - 1, innerBaseIndex + 2);

                // add start antiAliasing triangles

                // center
                vh.AddTriangle(currentVertCount - 3, numVertices, currentVertCount - 4);
                vh.AddTriangle(currentVertCount - 3, numVertices + 1, numVertices);

                // inner
                vh.AddTriangle(currentVertCount - 4, numVertices, currentVertCount - 8);
                vh.AddTriangle(innerBaseIndex + 3, currentVertCount - 8, numVertices);

                // outer
                vh.AddTriangle(currentVertCount - 7, innerBaseIndex + 4, numVertices + 1);
                vh.AddTriangle(currentVertCount - 7, numVertices + 1, currentVertCount - 3);
            }
        }

        private static void GetDirectionAndBaseAngle(in ArcProperties arcProperties, ref float baseAngle, out float direction) {
            switch (arcProperties.direction) {
                case ArcDirection.Backward:
                    direction = -1f;
                    break;

                default:
                case ArcDirection.Centered:
                    direction = 1.0f;
                    baseAngle -= arcProperties.length;
                    break;

                case ArcDirection.Forward:
                    direction = 1f;
                    break;
            }
        }

    }

}