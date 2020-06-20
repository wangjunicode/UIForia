using UIForia.Graphics.ShapeKit;
using UIForia.ListTypes;
using Unity.Mathematics;
using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public unsafe partial struct ShapeKit {

        private static float2 GetRadius(float width, float height, EllipseFitting fitting) {
            width *= 0.5f;
            height *= 0.5f;

            switch (fitting) {
                case EllipseFitting.UniformInner:
                    float clampedMin = math.min(width, height);
                    return new float2(clampedMin, clampedMin);

                case EllipseFitting.UniformOuter:
                    float clampedMax = math.max(width, height);
                    return new float2(clampedMax, clampedMax);

                default:
                case EllipseFitting.Ellipse:
                    return new float2(width, height);
            }
        }

        public static int ResolveEllipseResolution(Vector2 radius, in RoundingResolution roundingResolution) {

            switch (roundingResolution.resolutionType) {
                default:
                case ResolutionType.Calculated:
                    float circumference;

                    float maxDistance = math.max(roundingResolution.maxDistance, 0.1f);

                    if (radius.x == radius.y) {
                        circumference = GeoUtils.TwoPI * radius.x;
                    }
                    else {
                        circumference = Mathf.PI * (
                            3.0f * (radius.x + radius.y) -
                            math.sqrt(
                                (3.0f * radius.x + radius.y) *
                                (radius.x + 3.0f * radius.y)
                            )
                        );
                    }

                    return (int) math.ceil(circumference / maxDistance);

                case ResolutionType.Fixed:
                    return math.max(roundingResolution.fixedResolution, 3);

                case ResolutionType.Auto:

                    if (radius.x == radius.y) {
                        circumference = GeoUtils.TwoPI * radius.x;
                    }
                    else {
                        circumference = Mathf.PI * (
                            3.0f * (radius.x + radius.y) -
                            math.sqrt(
                                (3.0f * radius.x + radius.y) *
                                (radius.x + 3.0f * radius.y)
                            )
                        );
                    }

                    return (int) math.ceil(circumference / 6f);
                //float minRadius = math.min(radius.x, radius.y);
                //return (int) (2 * math.PI / math.acos(-1 / (minRadius * minRadius) + 1));// / math.max(1, ((int) ((minRadius * 2) / 100)));

            }

        }

        private void SetUnitPositionData(int resolution, float baseAngle = 0.0f, float direction = 1.0f) {

            if (unitPositionBuffer.array == null) {
                unitPositionBuffer = new List_float2(resolution * 2, allocator);
            }
            else if (unitPositionBuffer.size == resolution && lastEllipseDirection == direction && lastEllipseBaseAngle == baseAngle) {
                return;
            }

            float angleIncrement = GeoUtils.TwoPI / resolution;
            angleIncrement *= direction;

            unitPositionBuffer.EnsureCapacity(resolution);
            unitPositionBuffer.size = 0;

            float2* array = unitPositionBuffer.array;

            for (int i = 0; i < resolution; i++) {
                float angle = baseAngle + (angleIncrement * i);
                math.sincos(angle, out float s, out float c);
                array[i].x = s;
                array[i].y = c;
            }

            lastEllipseBaseAngle = baseAngle;
            lastEllipseDirection = direction;
        }

        public void AddCircle(ref UIVertexHelper vh, in Rect pixelRect, Color32 color) {
            AddCircle(ref vh, pixelRect, default, color);
        }

        public void AddCircle(ref UIVertexHelper vh, in Rect pixelRect, in EllipseProperties ellipseProperties, Color32 color) {

            float2 center = pixelRect.center;
            center.y = -center.y;

            float2 radius = GetRadius(pixelRect.width, pixelRect.height, ellipseProperties.fitting);

            int resolution = ResolveEllipseResolution(radius, ellipseProperties.resolution);

            SetUnitPositionData(resolution, ellipseProperties.baseAngle);

            int numVertices = vh.currentVertCount;
            vh.EnsureAdditionalVertexCapacity(4 + (6 * resolution));
            vh.EnsureAdditionalTriangleCapacity(3 * (4 + (6 * resolution)));

            int vertexCount = numVertices;
            int vertexStart = vertexCount;

            ref float4 tmpUVPos = ref vh.texCoord[vertexCount];
            ref float3 tmpVertPos = ref vh.positions[vertexCount];

            tmpUVPos.x = 0.5f;
            tmpUVPos.y = 0.5f;

            tmpVertPos.x = center.x;
            tmpVertPos.y = center.y;

            vertexCount++;
            tmpUVPos = ref vh.texCoord[vertexCount];
            tmpVertPos = ref vh.positions[vertexCount];
            // vh.AddVert(new float3(center.x, center.y, 0), color, tmpUVPos);

            // add first circle vertex
            tmpVertPos.x = center.x + unitPositionBuffer.array[0].x * (radius.x + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;
            tmpVertPos.y = center.y + unitPositionBuffer.array[0].y * (radius.y + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;
            tmpVertPos.z = 0.0f;

            tmpUVPos.x = (unitPositionBuffer.array[0].x * edgeGradientData.innerScale + 1.0f) * 0.5f;
            tmpUVPos.y = (unitPositionBuffer.array[0].y * edgeGradientData.innerScale + 1.0f) * 0.5f;

            vertexCount++;
            tmpUVPos = ref vh.texCoord[vertexCount];
            tmpVertPos = ref vh.positions[vertexCount];
            // vh.AddVert(tmpVertPos, color, tmpUVPos);

            for (int i = 1; i < resolution; i++) {
                tmpVertPos.x = center.x + unitPositionBuffer.array[i].x * (radius.x + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;
                tmpVertPos.y = center.y + unitPositionBuffer.array[i].y * (radius.y + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;
                tmpVertPos.z = 0.0f;

                tmpUVPos.x = (unitPositionBuffer.array[i].x * edgeGradientData.innerScale + 1.0f) * 0.5f;
                tmpUVPos.y = (unitPositionBuffer.array[i].y * edgeGradientData.innerScale + 1.0f) * 0.5f;
                // vh.AddVert(tmpVertPos, color, tmpUVPos);

                vertexCount++;
                tmpUVPos = ref vh.texCoord[vertexCount];
                tmpVertPos = ref vh.positions[vertexCount];
                vh.AddTriangleUnchecked(numVertices, numVertices + i, numVertices + i + 1);
            }

            vh.AddTriangleUnchecked(numVertices, numVertices + resolution, numVertices + 1);

            int alphaStart = vertexCount;

            if (edgeGradientData.isActive) {
                radius.x += edgeGradientData.shadowOffset + edgeGradientData.sizeAdd;
                radius.y += edgeGradientData.shadowOffset + edgeGradientData.sizeAdd;

                int outerFirstIndex = numVertices + resolution;

                // color.a = 0;

                // add first point
                tmpVertPos.x = center.x + unitPositionBuffer[0].x * radius.x;
                tmpVertPos.y = center.y + unitPositionBuffer[0].y * radius.y;
                tmpVertPos.z = 0.0f;

                tmpUVPos.x = (unitPositionBuffer.array[0].x + 1.0f) * 0.5f;
                tmpUVPos.y = (unitPositionBuffer.array[0].y + 1.0f) * 0.5f;
                //vh.AddVert(tmpVertPos, color, tmpUVPos);
                vertexCount++;
                tmpUVPos = ref vh.texCoord[vertexCount];
                tmpVertPos = ref vh.positions[vertexCount];
                for (int i = 1; i < resolution; i++) {
                    tmpVertPos.x = center.x + unitPositionBuffer.array[i].x * radius.x;
                    tmpVertPos.y = center.y + unitPositionBuffer.array[i].y * radius.y;
                    tmpVertPos.z = 0.0f;

                    tmpUVPos.x = (unitPositionBuffer.array[i].x + 1.0f) * 0.5f;
                    tmpUVPos.y = (unitPositionBuffer.array[i].y + 1.0f) * 0.5f;
                    // vh.AddVert(tmpVertPos, color, tmpUVPos);

                    vertexCount++;
                    tmpUVPos = ref vh.texCoord[vertexCount];
                    tmpVertPos = ref vh.positions[vertexCount];

                    vh.AddTriangleUnchecked(numVertices + i + 1, outerFirstIndex + i, outerFirstIndex + i + 1);
                    vh.AddTriangleUnchecked(numVertices + i + 1, outerFirstIndex + i + 1, numVertices + i + 2);

                }

                vh.AddTriangleUnchecked(numVertices + 1, outerFirstIndex, outerFirstIndex + 1);
                vh.AddTriangleUnchecked(numVertices + 2, numVertices + 1, outerFirstIndex + 1);

            }

            for (int i = vertexStart; i < alphaStart; i++) {
                vh.colors[i] = color;
            }

            color.a = 0;
            for (int i = alphaStart; i < vertexCount; i++) {
                vh.colors[i] = color;
            }

            vh.AddVertexCount(vertexCount - vertexStart);

        }

        private static float GetOuterDistance(in OutlineProperties outline) {
            switch (outline.type) {
                case LineType.Inner:
                    return 0.0f;

                case LineType.Outer:
                    return outline.weight;

                default:
                case LineType.Center:
                    return outline.weight * 0.5f;
            }
        }

        internal static float GetCenterDistance(in OutlineProperties outline) {
            switch (outline.type) {
                case LineType.Inner:
                    return outline.weight * -0.5f;

                case LineType.Outer:
                    return outline.weight * 0.5f;

                default:
                case LineType.Center:
                    return 0.0f;
            }
        }

        internal static float GetCenterDistance(in LineProperties lineProperties) {
            switch (lineProperties.lineType) {
                case LineType.Inner:
                    return lineProperties.weight * -0.5f;

                case LineType.Outer:
                    return lineProperties.weight * 0.5f;

                default:
                case LineType.Center:
                    return 0.0f;
            }
        }

        public static float GetInnerDistance(in OutlineProperties outline) {
            switch (outline.type) {
                case LineType.Inner:
                    return -outline.weight;

                case LineType.Outer:
                    return 0.0f;

                default:
                case LineType.Center:
                    return outline.weight * -0.5f;
            }
        }

        public void AddRing(ref UIVertexHelper vh, in Rect pixelRect, OutlineProperties outlineProperties, Color32 color) {
            AddRing(ref vh, pixelRect, outlineProperties, new EllipseProperties(), color);
        }

        public void AddRing(ref UIVertexHelper vh, in Rect pixelRect, float strokeWidth, Color32 color) {
            AddRing(ref vh, pixelRect, new OutlineProperties(strokeWidth), new EllipseProperties(), color);
        }

        public void AddRing(ref UIVertexHelper vh, in Rect pixelRect, OutlineProperties outlineProperties, in EllipseProperties ellipseProperties, Color32 color) {

            float2 center = pixelRect.center;
            center.y = -center.y;

            float2 radius = GetRadius(pixelRect.width, pixelRect.height, ellipseProperties.fitting);

            int resolution = ResolveEllipseResolution(radius, ellipseProperties.resolution);

            SetUnitPositionData(resolution, ellipseProperties.baseAngle);

            float halfLineWeight = outlineProperties.weight * 0.5f;
            float halfLineWeightOffset = (halfLineWeight + edgeGradientData.shadowOffset) * edgeGradientData.innerScale;

            float3 tmpInnerRadius = default;
            float3 tmpOuterRadius = default;

            float centerDistance = GetCenterDistance(outlineProperties);

            tmpInnerRadius.x = radius.x + centerDistance - halfLineWeightOffset;
            tmpInnerRadius.y = radius.y + centerDistance - halfLineWeightOffset;

            tmpOuterRadius.x = radius.x + centerDistance + halfLineWeightOffset;
            tmpOuterRadius.y = radius.y + centerDistance + halfLineWeightOffset;

            int numVertices = vh.currentVertCount;
            int startVertex = numVertices - 1;

            vh.EnsureAdditionalVertexCapacity(4 + (4 * resolution));

            int baseIndex;

            float uvMaxResolution = resolution;

            int vertexCount = numVertices;
            int vertexStart = vertexCount;

            ref float3 tmpVertPos = ref vh.positions[vertexCount];
            ref float4 uv = ref vh.texCoord[vertexCount];

            for (int i = 0; i < resolution; i++) {
                float uvX = i / uvMaxResolution;
                uv.x = uvX;
                uv.y = 0.0f;

                tmpVertPos.x = center.x + unitPositionBuffer.array[i].x * tmpInnerRadius.x;
                tmpVertPos.y = center.y + unitPositionBuffer.array[i].y * tmpInnerRadius.y;
                tmpVertPos.z = 0.0f;
                //vh.AddVert(tmpVertPos, color, uv);

                vertexCount++;
                tmpVertPos = ref vh.positions[vertexCount];
                uv = ref vh.texCoord[vertexCount];

                tmpVertPos.x = center.x + unitPositionBuffer.array[i].x * tmpOuterRadius.x;
                tmpVertPos.y = center.y + unitPositionBuffer.array[i].y * tmpOuterRadius.y;
                tmpVertPos.z = 0.0f;
                uv.x = uvX;
                uv.y = 1.0f;

                vertexCount++;
                tmpVertPos = ref vh.positions[vertexCount];
                uv = ref vh.texCoord[vertexCount];
                // vh.AddVert(tmpVertPos, color, uv);

                if (i > 0) {
                    baseIndex = startVertex + i * 2;
                    vh.AddTriangle(baseIndex - 1, baseIndex, baseIndex + 1);
                    vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
                }
            }

            // add last quad
            {

                tmpVertPos.x = center.x + unitPositionBuffer.array[0].x * tmpInnerRadius.x;
                tmpVertPos.y = center.y + unitPositionBuffer.array[0].y * tmpInnerRadius.y;
                tmpVertPos.z = 0.0f;
                uv.x = 1.0f;
                uv.y = 0.0f;

                vertexCount++;
                tmpVertPos = ref vh.positions[vertexCount];
                uv = ref vh.texCoord[vertexCount];
                // vh.AddVert(tmpVertPos, color, uv);

                tmpVertPos.x = center.x + unitPositionBuffer.array[0].x * tmpOuterRadius.x;
                tmpVertPos.y = center.y + unitPositionBuffer.array[0].y * tmpOuterRadius.y;
                tmpVertPos.z = 0.0f;
                uv.x = 1.0f;
                uv.y = 1.0f;

                vertexCount++;
                tmpVertPos = ref vh.positions[vertexCount];
                uv = ref vh.texCoord[vertexCount];
                // vh.AddVert(tmpVertPos, color, uv);

                baseIndex = startVertex + resolution * 2;
                vh.AddTriangle(baseIndex - 1, baseIndex, baseIndex + 1);
                vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
            }

            int alphaStart = vertexCount;

            if (edgeGradientData.isActive) {
                halfLineWeightOffset = halfLineWeight + edgeGradientData.shadowOffset + edgeGradientData.sizeAdd;

                tmpInnerRadius.x = radius.x + centerDistance - halfLineWeightOffset;
                tmpInnerRadius.y = radius.y + centerDistance - halfLineWeightOffset;

                tmpOuterRadius.x = radius.x + centerDistance + halfLineWeightOffset;
                tmpOuterRadius.y = radius.y + centerDistance + halfLineWeightOffset;

                // color.a = 0;

                int edgesBaseIndex;
                int innerBaseIndex;

                for (int i = 0; i < resolution; i++) {
                    float uvX = i / uvMaxResolution;
                    uv.x = uvX;
                    uv.y = 0.0f;

                    tmpVertPos.x = center.x + unitPositionBuffer.array[i].x * tmpInnerRadius.x;
                    tmpVertPos.y = center.y + unitPositionBuffer.array[i].y * tmpInnerRadius.y;
                    tmpVertPos.z = 0.0f;

                    vertexCount++;
                    tmpVertPos = ref vh.positions[vertexCount];
                    uv = ref vh.texCoord[vertexCount];
                    // vh.AddVert(tmpVertPos, color, uv);

                    uv.x = uvX;
                    uv.y = 1.0f;

                    tmpVertPos.x = center.x + unitPositionBuffer.array[i].x * tmpOuterRadius.x;
                    tmpVertPos.y = center.y + unitPositionBuffer.array[i].y * tmpOuterRadius.y;
                    tmpVertPos.z = 0.0f;

                    vertexCount++;
                    tmpVertPos = ref vh.positions[vertexCount];
                    uv = ref vh.texCoord[vertexCount];

                    //vh.AddVert(tmpVertPos, color, uv);

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

                // add last quads
                {
                    uv.x = 1.0f;
                    uv.y = 0.0f;

                    tmpVertPos.x = center.x + unitPositionBuffer.array[0].x * tmpInnerRadius.x;
                    tmpVertPos.y = center.y + unitPositionBuffer.array[0].y * tmpInnerRadius.y;
                    tmpVertPos.z = 0.0f;
                    // vh.AddVert(tmpVertPos, color, uv);

                    vertexCount++;
                    tmpVertPos = ref vh.positions[vertexCount];
                    uv = ref vh.texCoord[vertexCount];

                    uv.x = 1.0f;
                    uv.y = 1.0f;
                    tmpVertPos.x = center.x + unitPositionBuffer.array[0].x * tmpOuterRadius.x;
                    tmpVertPos.y = center.y + unitPositionBuffer.array[0].y * tmpOuterRadius.y;
                    tmpVertPos.z = 0.0f;

                    // vh.AddVert(tmpVertPos, color, uv);
                    vertexCount++;
                    tmpVertPos = ref vh.positions[vertexCount];
                    uv = ref vh.texCoord[vertexCount];

                    edgesBaseIndex = baseIndex + resolution * 2;
                    innerBaseIndex = startVertex + resolution * 2;

                    // inner quad
                    vh.AddTriangle(innerBaseIndex - 1, innerBaseIndex + 1, edgesBaseIndex + 3);
                    vh.AddTriangle(edgesBaseIndex + 1, innerBaseIndex - 1, edgesBaseIndex + 3);

                    // outer quad
                    vh.AddTriangle(innerBaseIndex, edgesBaseIndex + 2, innerBaseIndex + 2);
                    vh.AddTriangle(edgesBaseIndex + 2, edgesBaseIndex + 4, innerBaseIndex + 2);
                }

            }

            for (int i = vertexStart; i < alphaStart; i++) {
                vh.colors[i] = color;
            }

            color.a = 0;

            for (int i = alphaStart; i < vertexCount; i++) {
                vh.colors[i] = color;
            }

            vh.AddVertexCount(vertexCount - startVertex);

        }

    }

    public enum EllipseFitting {

        Ellipse,
        UniformInner,
        UniformOuter

    }

}