using System;
using System.Runtime.InteropServices;
using UIForia.Graphics.ShapeKit;
using UIForia.ListTypes;
using Unity.Mathematics;
using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public unsafe partial struct ShapeKit : IDisposable {

        private static readonly float k_Sqrt2 = Mathf.Sqrt(2.0f);

        private static int GetCornerResolution(in Corner corner) {
            if (corner.cornerType == CornerType.Bevel) {
                return corner.bevelSteps > 2 ? corner.bevelSteps : 2;
            }

            float circumference = GeoUtils.TwoPI * corner.radius;

            int resolution = (int)math.ceil(circumference / corner.roundDistance * 0.25f);
            return resolution > 2 ? resolution : 2;
        }

        private void SetCornerUnitPositions(in CornerProperties corners) {
            // todo -- probably worth caching these results
            int tl = GetCornerResolution(corners.topLeft);
            int tr = GetCornerResolution(corners.topRight);
            int br = GetCornerResolution(corners.bottomRight);
            int bl = GetCornerResolution(corners.bottomLeft);

            if (cornerBuffer.array == null) {
                cornerBuffer = new List_float2(tl + tr + br + bl, allocator);
            }
            else {
                cornerBuffer.EnsureCapacity(tl + tr + br + bl);
            }

            cornerBuffer.size = 0;

            topLeftCornerRange = SetUnitPositions(corners.topLeft.radius, tl, GeoUtils.HalfPI + Mathf.PI);
            topRightCornerRange = SetUnitPositions(corners.topRight.radius, tr, 0.0f);
            bottomRightCornerRange = SetUnitPositions(corners.bottomRight.radius, br, GeoUtils.HalfPI);
            bottomLeftCornerRange = SetUnitPositions(corners.bottomLeft.radius, bl, Mathf.PI);

        }

        private RangeInt SetUnitPositions(float radius, int resolution, float baseAngle) {

            float angleIncrement = GeoUtils.HalfPI / (resolution - 1.0f);
            float angle;

            int size = cornerBuffer.size;
            int startSize = size;

            if (radius < 0.1f) {
                angle = baseAngle + GeoUtils.HalfPI * 0.5f;
                float length = k_Sqrt2;
                math.sincos(angle, out float s, out float c);

                for (int i = 0; i < resolution; i++) {
                    cornerBuffer[startSize + i] = new float2(s * length, c * length);
                }
            }
            else {
                for (int i = 0; i < resolution; i++) {
                    angle = baseAngle + angleIncrement * i;
                    math.sincos(angle, out float s, out float c);
                    cornerBuffer[startSize + i] = new float2(s, c);
                }
            }

            cornerBuffer.size += resolution;
            return new RangeInt(startSize, resolution);
        }

        public void AddRoundedRect(ref UIVertexHelper vh, Vector2 position, float width, float height, in Corner corner, Color32 color) {
            CornerProperties cornerProperties = new CornerProperties() {
                topLeft = corner,
                topRight = corner,
                bottomLeft = corner,
                bottomRight = corner
            };
            AddRoundedRect(ref vh, new Vector2(position.x + width * 0.5f, position.y + height * 0.5f), width, height, cornerProperties, color);
        }

        public void AddRoundedRect(ref UIVertexHelper vh, in Rect rect, in Corner corner, Color32 color) {
            CornerProperties cornerProperties = new CornerProperties() {
                topLeft = corner,
                topRight = corner,
                bottomLeft = corner,
                bottomRight = corner
            };
            AddRoundedRect(ref vh, rect.center, rect.width, rect.height, cornerProperties, color);
        }

        public void AddRoundedRect(ref UIVertexHelper vh, Vector2 center, float width, float height, in CornerProperties cornerProperties, Color32 color) {

            center.y = -center.y;

            int numVertices = vh.currentVertCount;

            float constraint = math.min(width, height) * 0.5f;

            SetCornerUnitPositions(cornerProperties);

            float topLeft = math.clamp(cornerProperties.topLeft.radius, 0, constraint);
            float topRight = math.clamp(cornerProperties.topRight.radius, 0, constraint);
            float bottomRight = math.clamp(cornerProperties.bottomRight.radius, 0, constraint);
            float bottomLeft = math.clamp(cornerProperties.bottomLeft.radius, 0, constraint);

            vh.AddVert(center, color, new Vector2(0.5f, 0.5f));

            float sizeSub = math.min(height, width);
            sizeSub *= 1.0f - edgeGradientData.innerScale;

            AddRoundedRectVerticesRing(
                ref vh,
                center,
                width - sizeSub,
                height - sizeSub,
                width - sizeSub,
                height - sizeSub,
                topLeft * edgeGradientData.innerScale,
                (topLeft + edgeGradientData.shadowOffset) * edgeGradientData.innerScale,
                topRight * edgeGradientData.innerScale,
                (topRight + edgeGradientData.shadowOffset) * edgeGradientData.innerScale,
                bottomRight * edgeGradientData.innerScale,
                (bottomRight + edgeGradientData.shadowOffset) * edgeGradientData.innerScale,
                bottomLeft * edgeGradientData.innerScale,
                (bottomLeft + edgeGradientData.shadowOffset) * edgeGradientData.innerScale,
                color,
                false
            );

            // set indices
            int numNewVertices = vh.currentVertCount - numVertices;
            for (int i = 0; i < numNewVertices - 1; i++) {
                vh.AddTriangle(numVertices, numVertices + i, numVertices + i + 1);
            }

            // set last triangle
            vh.AddTriangle(numVertices, vh.currentVertCount - 1, numVertices + 1);

            if (edgeGradientData.isActive) {

                float outerRadiusMod = 0.0f;
                outerRadiusMod += edgeGradientData.shadowOffset;
                outerRadiusMod += edgeGradientData.sizeAdd;

                color.a = 0;

                AddRoundedRectVerticesRing(
                    ref vh,
                    center,
                    width,
                    height,
                    width,
                    height,
                    topLeft,
                    topLeft + outerRadiusMod,
                    topRight,
                    topRight + outerRadiusMod,
                    bottomRight,
                    bottomRight + outerRadiusMod,
                    bottomLeft,
                    bottomLeft + outerRadiusMod,
                    color,
                    true
                );
            }

        }

        public void AddRoundedRectOutline(ref UIVertexHelper vh, Vector2 position, float width, float height, OutlineProperties outlineProperties, in Corner corner, Color32 color) {
            CornerProperties cornerProperties = new CornerProperties() {
                topLeft = corner,
                bottomLeft = corner,
                bottomRight = corner,
                topRight = corner
            };
            AddRoundedRectOutline(ref vh, new Vector2(position.x + width * 0.5f, position.y + height * 0.5f), width, height, outlineProperties, cornerProperties, color);
        }

        public void AddRoundedRectOutline(ref UIVertexHelper vh, Vector2 position, float width, float height, OutlineProperties outlineProperties, in CornerProperties cornerProperties, Color32 color) {

            float outerDistance = GetOuterDistance(outlineProperties);
            float centerDistance = GetCenterDistance(outlineProperties);

            float fullWidth = width + outerDistance * 2.0f;
            float fullHeight = height + outerDistance * 2.0f;

            float constraint = math.min(width, height) * 0.5f;

            SetCornerUnitPositions(cornerProperties);

            float topLeft = math.clamp(cornerProperties.topLeft.radius, 0, constraint);
            float topRight = math.clamp(cornerProperties.topRight.radius, 0, constraint);
            float bottomRight = math.clamp(cornerProperties.bottomRight.radius, 0, constraint);
            float bottomLeft = math.clamp(cornerProperties.bottomLeft.radius, 0, constraint);

            float outerRadiusMod;
            float halfLineWidth = outlineProperties.weight * 0.5f;
            position.y = -position.y;
            
            byte alpha = color.a;

            if (edgeGradientData.isActive) {
                color.a = 0;

                outerRadiusMod = centerDistance - halfLineWidth - edgeGradientData.shadowOffset;
                outerRadiusMod -= edgeGradientData.sizeAdd;

                AddRoundedRectVerticesRing(
                    ref vh,
                    position,
                    width,
                    height,
                    fullWidth,
                    fullHeight,
                    topLeft,
                    topLeft + outerRadiusMod,
                    topRight,
                    topRight + outerRadiusMod,
                    bottomRight,
                    bottomRight + outerRadiusMod,
                    bottomLeft,
                    bottomLeft + outerRadiusMod,
                    color,
                    false
                );

                color.a = alpha;
            }

            outerRadiusMod = Mathf.LerpUnclamped(centerDistance, centerDistance - halfLineWidth - edgeGradientData.shadowOffset, edgeGradientData.innerScale);

            AddRoundedRectVerticesRing(
                ref vh,
                position,
                width,
                height,
                fullWidth,
                fullHeight,
                topLeft,
                topLeft + outerRadiusMod,
                topRight,
                topRight + outerRadiusMod,
                bottomRight,
                bottomRight + outerRadiusMod,
                bottomLeft,
                bottomLeft + outerRadiusMod,
                color,
                edgeGradientData.isActive
            );

            outerRadiusMod = centerDistance + (halfLineWidth + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;

            AddRoundedRectVerticesRing(
                ref vh,
                position,
                width,
                height,
                fullWidth,
                fullHeight,
                topLeft,
                topLeft + outerRadiusMod,
                topRight,
                topRight + outerRadiusMod,
                bottomRight,
                bottomRight + outerRadiusMod,
                bottomLeft,
                bottomLeft + outerRadiusMod,
                color,
                true
            );

            if (edgeGradientData.isActive) {

                outerRadiusMod = centerDistance + halfLineWidth + edgeGradientData.shadowOffset;
                outerRadiusMod += edgeGradientData.sizeAdd;

                color.a = 0;

                AddRoundedRectVerticesRing(
                    ref vh,
                    position,
                    width,
                    height,
                    fullWidth,
                    fullHeight,
                    topLeft,
                    topLeft + outerRadiusMod,
                    topRight,
                    topRight + outerRadiusMod,
                    bottomRight,
                    bottomRight + outerRadiusMod,
                    bottomLeft,
                    bottomLeft + outerRadiusMod,
                    color,
                    true
                );
            }
        }
        
        private void AddRoundedRectVerticesRing(
            ref UIVertexHelper vh,
            Vector2 center,
            float width,
            float height,
            float fullWidth,
            float fullHeight,
            float tlRadius,
            float tlOuterRadius,
            float trRadius,
            float trOuterRadius,
            float brRadius,
            float brOuterRadius,
            float blRadius,
            float blOuterRadius,
            Color32 color,
            bool addIndices
        ) {
            float xMin = center.x - width * 0.5f;
            float yMin = center.y - height * 0.5f;

            float xMax = center.x + width * 0.5f;
            float yMax = center.y + height * 0.5f;

            float xMinUV = center.x - fullWidth * 0.5f;
            float yMinUV = center.y - fullHeight * 0.5f;

            // TR
            Vector3 tmpV3 = default;
            Vector3 tmpPos = default;
            Vector2 tmpUV = default;

            tmpV3.x = xMax - trRadius;
            tmpV3.y = yMax - trRadius;

            if (trOuterRadius < 0.0f) {
                tmpV3.x += trOuterRadius;
                tmpV3.y += trOuterRadius;

                trOuterRadius = 0.0f;
            }

            int start = topRightCornerRange.start;
            int end = topRightCornerRange.end;
            for (int i = start; i < end; i++) {
                tmpPos.x = tmpV3.x + cornerBuffer[i].x * trOuterRadius;
                tmpPos.y = tmpV3.y + cornerBuffer[i].y * trOuterRadius;
                tmpPos.z = tmpV3.z;

                tmpUV.x = (tmpPos.x - xMinUV) / fullWidth;
                tmpUV.y = (tmpPos.y - yMinUV) / fullHeight;

                vh.AddVert(tmpPos, color, tmpUV);
            }

            // BR
            tmpV3.x = xMax - brRadius;
            tmpV3.y = yMin + brRadius;

            if (brOuterRadius < 0.0f) {
                tmpV3.x += brOuterRadius;
                tmpV3.y -= brOuterRadius;

                brOuterRadius = 0.0f;
            }

            start = bottomRightCornerRange.start;
            end = bottomRightCornerRange.end;
            for (int i = start; i < end; i++) {
                tmpPos.x = tmpV3.x + cornerBuffer[i].x * brOuterRadius;
                tmpPos.y = tmpV3.y + cornerBuffer[i].y * brOuterRadius;
                tmpPos.z = tmpV3.z;

                tmpUV.x = (tmpPos.x - xMinUV) / fullWidth;
                tmpUV.y = (tmpPos.y - yMinUV) / fullHeight;

                vh.AddVert(tmpPos, color, tmpUV);
            }

            // BL
            tmpV3.x = xMin + blRadius;
            tmpV3.y = yMin + blRadius;

            if (blOuterRadius < 0.0f) {
                tmpV3.x -= blOuterRadius;
                tmpV3.y -= blOuterRadius;

                blOuterRadius = 0.0f;
            }

            start = bottomLeftCornerRange.start;
            end = bottomLeftCornerRange.end;
            for (int i = start; i < end; i++) {
                tmpPos.x = tmpV3.x + cornerBuffer[i].x * blOuterRadius;
                tmpPos.y = tmpV3.y + cornerBuffer[i].y * blOuterRadius;
                tmpPos.z = tmpV3.z;

                tmpUV.x = (tmpPos.x - xMinUV) / fullWidth;
                tmpUV.y = (tmpPos.y - yMinUV) / fullHeight;

                vh.AddVert(tmpPos, color, tmpUV);
            }

            // TL
            tmpV3.x = xMin + tlRadius;
            tmpV3.y = yMax - tlRadius;

            if (tlOuterRadius < 0.0f) {
                tmpV3.x -= tlOuterRadius;
                tmpV3.y += tlOuterRadius;

                tlOuterRadius = 0.0f;
            }

            start = topLeftCornerRange.start;
            end = topLeftCornerRange.end;
            for (int i = start; i < end; i++) {
                tmpPos.x = tmpV3.x + cornerBuffer[i].x * tlOuterRadius;
                tmpPos.y = tmpV3.y + cornerBuffer[i].y * tlOuterRadius;
                tmpPos.z = tmpV3.z;

                tmpUV.x = (tmpPos.x - xMinUV) / fullWidth;
                tmpUV.y = (tmpPos.y - yMinUV) / fullHeight;

                vh.AddVert(tmpPos, color, tmpUV);
            }

            // add last circle vertex
            tmpPos.x = tmpV3.x + cornerBuffer[topRightCornerRange.start].x * tlOuterRadius;
            tmpPos.y = tmpV3.y + cornerBuffer[topRightCornerRange.start].y * tlOuterRadius;
            tmpPos.z = tmpV3.z;

            tmpUV.x = (tmpPos.x - xMinUV) / fullWidth;
            tmpUV.y = (tmpPos.y - yMinUV) / fullHeight;

            vh.AddVert(tmpPos, color, tmpUV);

            if (addIndices) {
                AddRoundedRingIndices(ref vh, topLeftCornerRange.length + topLeftCornerRange.length + bottomRightCornerRange.length + bottomLeftCornerRange.length);
            }
        }

        private static void AddRoundedRingIndices(ref UIVertexHelper vh, int totalResolution) {

            int numNewVertices = totalResolution + 1;

            int innerStartIndex = vh.currentVertCount - numNewVertices - numNewVertices - 1;
            int outerStartIndex = vh.currentVertCount - numNewVertices;

            for (int i = 0; i < totalResolution; i++) {
                vh.AddTriangle(innerStartIndex + i + 1, outerStartIndex + i, outerStartIndex + i + 1);
                vh.AddTriangle(innerStartIndex + i + 1, outerStartIndex + i + 1, innerStartIndex + i + 2);
            }

            vh.AddTriangle(innerStartIndex + 1, outerStartIndex + totalResolution, outerStartIndex);
            vh.AddTriangle(innerStartIndex + 1, outerStartIndex - 1, outerStartIndex + totalResolution);
        }

        private static void AddRoundedRingIndices(ref UIVertexHelper vh, RoundedCornerUnitPositionData cornerUnitPositions) {
            int totalResolution =
                cornerUnitPositions.TLUnitPositions.Length +
                cornerUnitPositions.TRUnitPositions.Length +
                cornerUnitPositions.BRUnitPositions.Length +
                cornerUnitPositions.BLUnitPositions.Length;

            int numNewVertices = totalResolution + 1;

            int innerStartIndex = vh.currentVertCount - numNewVertices - numNewVertices - 1;
            int outerStartIndex = vh.currentVertCount - numNewVertices;

            for (int i = 0; i < totalResolution; i++) {
                vh.AddTriangle(innerStartIndex + i + 1, outerStartIndex + i, outerStartIndex + i + 1);
                vh.AddTriangle(innerStartIndex + i + 1, outerStartIndex + i + 1, innerStartIndex + i + 2);
            }

            vh.AddTriangle(innerStartIndex + 1, outerStartIndex + totalResolution, outerStartIndex);
            vh.AddTriangle(innerStartIndex + 1, outerStartIndex - 1, outerStartIndex + totalResolution);
        }

    }

    public enum CornerType {

        Round,
        Bevel

    }

    public struct CornerProperties {

        public Corner topRight;
        public Corner topLeft;
        public Corner bottomRight;
        public Corner bottomLeft;

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Corner {

        [FieldOffset(0)] internal CornerType cornerType;
        [FieldOffset(4)] internal float radius;
        [FieldOffset(8)] internal int bevelSteps;
        [FieldOffset(8)] internal float roundDistance;

        public static Corner Bevel(float radius, int bevelSteps = 2) {
            return new Corner() {
                radius = radius,
                cornerType = CornerType.Bevel,
                bevelSteps = bevelSteps
            };
        }

        public static Corner Round(float radius, float roundDistance = 4f) {
            return new Corner() {
                radius = radius,
                cornerType = CornerType.Round,
                roundDistance = roundDistance
            };
        }

    }

}