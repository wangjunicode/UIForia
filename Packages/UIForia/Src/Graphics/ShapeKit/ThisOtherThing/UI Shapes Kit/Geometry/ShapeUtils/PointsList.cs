using UnityEngine;
using UIForia.ListTypes;
using Unity.Mathematics;

namespace ThisOtherThing.UI.ShapeUtils {

    public unsafe partial struct ShapeKit {

        private void SetPositions(ref List_float2 positions, in PointListProperties pointListProperties, ref PointsData lineData) {

            positionBuffer.SetSize(positions.size, allocator);

            CheckMinPointDistances(ref positions, ref positionBuffer, lineData.lineWeight * 0.5f, lineData.isClosed);

            positions.size = 0;

            int inputNumPositions = positionBuffer.size;

            positions.EnsureCapacity(inputNumPositions + 1);
            float2* positionArray = positionBuffer.array;

            // add first position
            if (lineData.isClosed) {
                InterpolatePoints(ref positions, positionArray[inputNumPositions - 1], positionArray[0], positionArray[1], pointListProperties);
            }
            else {
                positions.Add(positionArray[0]);
            }

            for (int i = 1; i < inputNumPositions - 1; i++) {
                InterpolatePoints(ref positions, positionArray[i - 1], positionArray[i], positionArray[i + 1], pointListProperties);
            }

            // add end point
            if (lineData.isClosed) {
                InterpolatePoints(ref positions, positionArray[inputNumPositions - 2], positionArray[inputNumPositions - 1], positionArray[0], pointListProperties);
            }
            else {
                positions.Add(positionArray[inputNumPositions - 1]);
            }

            lineData.NumPositions = positions.size;
        }

        private static void CheckMinPointDistances(ref List_float2 inPositions, ref List_float2 outPositions, float minDistance, bool isClosed) {
            outPositions.size = 0;

            float minSqrDistance = minDistance * minDistance;
            float sqrDistance;

            float2* positionArray = inPositions.array;

            int outputSize = 0;
            float2* outputArray = outPositions.array;

            outputArray[outputSize++] = positionArray[0];

            float2 tmpPos = default;

            for (int i = 2; i < inPositions.size; i++) {
                tmpPos.x = positionArray[i].x - positionArray[i - 1].x;
                tmpPos.y = positionArray[i].y - positionArray[i - 1].y;

                sqrDistance = tmpPos.x * tmpPos.x + tmpPos.y * tmpPos.y;

                if (sqrDistance < minSqrDistance) {
                    tmpPos.x *= 0.5f;
                    tmpPos.x += positionArray[i - 1].x;

                    tmpPos.y *= 0.5f;
                    tmpPos.y += positionArray[i - 1].y;

                    outputArray[outputSize++] = tmpPos;

                    i++;
                }
                else {
                    outputArray[outputSize++] = positionArray[i - 1];
                }
            }

            if (!isClosed) {
                outputArray[outputSize++] = positionArray[inPositions.size - 1];
            }
            else {
                tmpPos.x = positionArray[inPositions.size - 1].x - positionArray[0].x;
                tmpPos.y = positionArray[inPositions.size - 1].y - positionArray[0].y;

                sqrDistance = tmpPos.x * tmpPos.x + tmpPos.y * tmpPos.y;

                if (sqrDistance < minSqrDistance) {
                    tmpPos.x *= 0.5f;
                    tmpPos.x += positionArray[0].x;

                    tmpPos.y *= 0.5f;
                    tmpPos.y += positionArray[0].y;

                    outPositions[0] = tmpPos;
                }
                else {
                    outputArray[outputSize++] = positionArray[inPositions.size - 1];
                }
            }

            outPositions.size = outputSize;
        }

        private static void InterpolatePoints(ref List_float2 positions, float2 prevPosition, float2 position, float2 nextPosition, in PointListProperties pointListProperties) {

            float2 tmpBackV = default;
            float2 tmpBackNormV = default;

            float2 tmpForwV = default;
            float2 tmpForwNormV = default;

            tmpBackV.x = prevPosition.x - position.x;
            tmpBackV.y = prevPosition.y - position.y;
            float backLength = math.sqrt(tmpBackV.x * tmpBackV.x + tmpBackV.y * tmpBackV.y);
            tmpBackNormV.x = tmpBackV.x / backLength;
            tmpBackNormV.y = tmpBackV.y / backLength;

            tmpForwV.x = nextPosition.x - position.x;
            tmpForwV.y = nextPosition.y - position.y;
            float forwLength = math.sqrt(tmpForwV.x * tmpForwV.x + tmpForwV.y * tmpForwV.y);
            tmpForwNormV.x = tmpForwV.x / forwLength;
            tmpForwNormV.y = tmpForwV.y / forwLength;

            float cos = (tmpBackNormV.x * tmpForwNormV.x + tmpBackNormV.y * tmpForwNormV.y);
            float angle = Mathf.Acos(cos);

            // ignore points along straight line
            if (cos <= -0.9999f) {
                return;
            }

            if (pointListProperties.roundingDistance > 0.0f) {
                AddRoundedPoints(ref positions, tmpBackNormV, position, tmpForwNormV, pointListProperties, angle, Mathf.Min(backLength, forwLength) * 0.49f);
            }
            else {
                float maxAngle = pointListProperties.maxAngle; // > 0.2f ? pointListProperties.maxAngle : 0.2f;
                if (angle < maxAngle) {
                    positions.Add(position + tmpBackNormV * 0.5f);
                    positions.Add(position + tmpForwNormV * 0.5f);
                }
                else {
                    positions.Add(position);
                }
            }
        }

        private static int ComputeRoundingResolution(float radius, RoundingResolution roundingResolution, int numCorners) {
            switch (roundingResolution.resolutionType) {
                default:
                case ResolutionType.Auto: {
                    float circumference = GeoUtils.TwoPI * radius;
                    float maxDistance = roundingResolution.maxDistance;
                    if (maxDistance <= 0.1f) {
                        maxDistance = 4f;
                    }

                    int resolution = Mathf.CeilToInt(circumference / maxDistance / numCorners);
                    return resolution < 2 ? 2 : resolution;
                }

                case ResolutionType.Calculated: {
                    float circumference = GeoUtils.TwoPI * radius;
                    float maxDistance = roundingResolution.maxDistance;
                    if (maxDistance <= 0.1f) {
                        maxDistance = 4f;
                    }

                    int resolution = Mathf.CeilToInt(circumference / maxDistance / numCorners);
                    return resolution < 2 ? 2 : resolution;
                }

                case ResolutionType.Fixed:
                    return roundingResolution.fixedResolution;
            }
        }

        private static void AddRoundedPoints(ref List_float2 positions, float2 backNormV, float2 position, float2 forwNormV, PointListProperties pointListProperties, float angle, float maxDistance) {
            float roundingDistance = Mathf.Min(maxDistance, pointListProperties.roundingDistance);

            float2 tmpBackPos = default;
            float2 tmpForwPos = default;
            float2 tmpPos = default;

            tmpBackPos.x = position.x + backNormV.x * roundingDistance;
            tmpBackPos.y = position.y + backNormV.y * roundingDistance;

            tmpForwPos.x = position.x + forwNormV.x * roundingDistance;
            tmpForwPos.y = position.y + forwNormV.y * roundingDistance;

            // pointListProperties.CornerRounding.UpdateAdjusted(roundingDistance / 4.0f, 0.0f, (GeoUtils.TwoPI - angle) / Mathf.PI);

            int resolution = ComputeRoundingResolution(roundingDistance * 0.25f, pointListProperties.rounding, (int) ((GeoUtils.TwoPI - angle) / Mathf.PI));
            float resolutionF = resolution - 1.0f;

            positions.EnsureAdditionalCapacity(resolution);

            for (int i = 0; i < resolution; i++) {
                float interpolator = i / resolutionF;

                tmpPos.x = Mathf.LerpUnclamped(
                    Mathf.LerpUnclamped(tmpBackPos.x, position.x, interpolator),
                    Mathf.LerpUnclamped(position.x, tmpForwPos.x, interpolator),
                    interpolator
                );

                tmpPos.y = Mathf.LerpUnclamped(
                    Mathf.LerpUnclamped(tmpBackPos.y, position.y, interpolator),
                    Mathf.LerpUnclamped(position.y, tmpForwPos.y, interpolator),
                    interpolator
                );

                positions.Add(tmpPos);
            }
        }

        private bool SetLineData(ref List_float2 positions, PointListProperties pointListProperties, ref PointsData lineData) {
            if (positions.array == null) {
                return false;
            }

            SetPositions(ref positions, pointListProperties, ref lineData);

            int numPositions = lineData.NumPositions;

            lineData.EnsureCapacity(numPositions);

            int numPositionsMinusOne = numPositions - 1;

            lineData.totalLength = 0.0f;

            float distance;
            float2 lastUnitTangent = default;
            float2 currentUnitTangent = default;

            float2* positionArray = positions.array;

            // set data for first point
            if (!lineData.isClosed) {
                lineData.positionTangents[0].x = positionArray[0].x - positionArray[1].x;
                lineData.positionTangents[0].y = positionArray[0].y - positionArray[1].y;

                distance = math.sqrt(
                    lineData.positionTangents[0].x * lineData.positionTangents[0].x +
                    lineData.positionTangents[0].y * lineData.positionTangents[0].y
                );

                lineData.positionDistances[0] = distance;
                lineData.totalLength += distance;

                lineData.positionNormals[0].x = lineData.positionTangents[0].y / distance;
                lineData.positionNormals[0].y = -lineData.positionTangents[0].x / distance;

                lastUnitTangent.x = -lineData.positionTangents[0].x / distance;
                lastUnitTangent.y = -lineData.positionTangents[0].y / distance;

                lineData.startCapOffset.x = -lastUnitTangent.x;
                lineData.startCapOffset.y = -lastUnitTangent.y;
            }
            else {
                lastUnitTangent.x = positionArray[0].x - positionArray[numPositionsMinusOne].x;
                lastUnitTangent.y = positionArray[0].y - positionArray[numPositionsMinusOne].y;

                distance = math.sqrt(
                    lastUnitTangent.x * lastUnitTangent.x +
                    lastUnitTangent.y * lastUnitTangent.y
                );

                lastUnitTangent.x /= distance;
                lastUnitTangent.y /= distance;

                SetPointData(
                    positionArray[0],
                    positionArray[1],
                    ref currentUnitTangent,
                    ref lineData.positionTangents[0],
                    ref lineData.positionNormals[0],
                    ref lastUnitTangent,
                    out lineData.positionDistances[0]
                );

                lineData.totalLength += lineData.positionDistances[0];
            }

            for (int i = 1; i < numPositionsMinusOne; i++) {
                SetPointData(
                    positionArray[i],
                    positionArray[i + 1],
                    ref currentUnitTangent,
                    ref lineData.positionTangents[i],
                    ref lineData.positionNormals[i],
                    ref lastUnitTangent,
                    out lineData.positionDistances[i]
                );

                lineData.totalLength += lineData.positionDistances[i];
            }

            // set data for last point
            if (!lineData.isClosed) {
                lineData.positionTangents[numPositionsMinusOne].x = positionArray[numPositionsMinusOne].x - positionArray[numPositionsMinusOne - 1].x;
                lineData.positionTangents[numPositionsMinusOne].y = positionArray[numPositionsMinusOne].y - positionArray[numPositionsMinusOne - 1].y;

                distance = Mathf.Sqrt(
                    lineData.positionTangents[numPositionsMinusOne].x * lineData.positionTangents[numPositionsMinusOne].x +
                    lineData.positionTangents[numPositionsMinusOne].y * lineData.positionTangents[numPositionsMinusOne].y
                );

                lineData.endCapOffset.x = lineData.positionTangents[numPositionsMinusOne].x / distance;
                lineData.endCapOffset.y = lineData.positionTangents[numPositionsMinusOne].y / distance;

                lineData.positionNormals[numPositionsMinusOne].x = -lineData.positionTangents[numPositionsMinusOne].y / distance;
                lineData.positionNormals[numPositionsMinusOne].y = lineData.positionTangents[numPositionsMinusOne].x / distance;
            }
            else {
                SetPointData(
                    positionArray[numPositionsMinusOne],
                    positionArray[0],
                    ref currentUnitTangent,
                    ref lineData.positionTangents[numPositionsMinusOne],
                    ref lineData.positionNormals[numPositionsMinusOne],
                    ref lastUnitTangent,
                    out lineData.positionDistances[numPositionsMinusOne]
                );

                lineData.totalLength += lineData.positionDistances[numPositionsMinusOne];
            }

            if (lineData.generateRoundedCaps) {
                SetRoundedCapPointData(
                    Mathf.Atan2(-lineData.positionNormals[0].x, -lineData.positionNormals[0].y),
                    ref lineData.StartCapOffsets,
                    ref lineData.StartCapUVs,
                    lineData.roundedCapResolution,
                    true
                );

                SetRoundedCapPointData(
                    Mathf.Atan2(lineData.positionNormals[numPositionsMinusOne].x, lineData.positionNormals[numPositionsMinusOne].y),
                    ref lineData.EndCapOffsets,
                    ref lineData.EndCapUVs,
                    lineData.roundedCapResolution,
                    false
                );
            }

            float accumulatedLength = 0.0f;
            for (int i = 0; i < lineData.totalPositionCount; i++) {
                lineData.normalizedPositionDistances[i] = accumulatedLength / lineData.totalLength;
                accumulatedLength += lineData.positionDistances[i];
            }

            return true;
        }

        private static void SetRoundedCapPointData(float centerAngle, ref List_float2 offsets, ref List_float2 uvs, int resolution, bool isStart) {
            float angleIncrement = Mathf.PI / (resolution + 1);
            float baseAngle = centerAngle;

            offsets.SetSize(resolution);
            uvs.SetSize(resolution);
            // if (offsets == null || offsets.Length != resolution) {
                // offsets = new float2[resolution];
                // uvs = new float2[resolution];
            // }

            baseAngle += angleIncrement;

            for (int i = 0; i < resolution; i++) {
                float angle = baseAngle + (angleIncrement * i);

                offsets[i].x = math.sin(angle);
                offsets[i].y = math.cos(angle);

                // set angle for uvs
                angle = angleIncrement * i + math.PI * 0.14f;

                if (isStart) {
                    angle += math.PI;
                }

                uvs[i].x = math.abs(math.sin(angle));
                uvs[i].y = math.cos(angle) * 0.5f + 0.5f;
            }
        }

        private static void SetPointData(float2 currentPoint, float2 nextPoint, ref float2 currentUnitTangent, ref float2 positionTangent, ref float2 positionNormal, ref float2 lastUnitTangent, out float distance) {
            positionTangent.x = currentPoint.x - nextPoint.x;
            positionTangent.y = currentPoint.y - nextPoint.y;

            distance = math.sqrt(
                positionTangent.x * positionTangent.x +
                positionTangent.y * positionTangent.y
            );

            currentUnitTangent.x = positionTangent.x / distance;
            currentUnitTangent.y = positionTangent.y / distance;

            positionNormal.x = -(lastUnitTangent.x + currentUnitTangent.x);
            positionNormal.y = -(lastUnitTangent.y + currentUnitTangent.y);

            if (positionNormal.x == 0.0f && positionNormal.y == 0.0f) {
                positionNormal.x = -lastUnitTangent.y;
                positionNormal.y = lastUnitTangent.x;
            }

            // normalize line normal
            float normalMag = math.sqrt(
                positionNormal.x * positionNormal.x +
                positionNormal.y * positionNormal.y
            );
            positionNormal.x /= normalMag;
            positionNormal.y /= normalMag;

            float inBetweenAngle = math.acos(math.dot(lastUnitTangent, currentUnitTangent)) * 0.5f;
            float angleAdjustedLength = 1.0f / math.sin(inBetweenAngle);

            if (currentUnitTangent.x * positionNormal.y - currentUnitTangent.y * positionNormal.x > 0.0f) {
                angleAdjustedLength *= -1.0f;
            }

            positionNormal.x *= angleAdjustedLength;
            positionNormal.y *= angleAdjustedLength;

            lastUnitTangent.x = -currentUnitTangent.x;
            lastUnitTangent.y = -currentUnitTangent.y;
        }

    }

}