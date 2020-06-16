using UIForia.ListTypes;
using Unity.Mathematics;
using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public static unsafe class PointsGenerator {

        public static void SetPoints(ref List_float2 positions, PointListGeneratorData data) {
            switch (data.pointListGeneratorType) {
                case PointListGeneratorType.Custom:
                    break;

                // case PointListGeneratorType.Rect:
                //     SetPointsRect(ref positions, data);
                //     break;

                // case PointListGeneratorType.Round:
                //     SetPointsRound(ref positions, data);
                //     break;

                case PointListGeneratorType.RadialGraph:
                    SetPointsRadialGraph(ref positions, data);
                    break;

                case PointListGeneratorType.LineGraph:
                    SetPointsLineGraph(ref positions, data);
                    break;

                // case PointListGeneratorType.AngleLine:
                //     SetPointsAngleLine(ref positions, data);
                //     break;

                // case PointListGeneratorType.Star:
                //     SetPointsStar(ref positions, data);
                //     break;

                case PointListGeneratorType.Gear:
                    SetPointsGear(ref positions, data);
                    break;
            }
        }

        public static void SetPointsRect(ref List_float2 positions, RectPointListData data) {

            positions.SetSize(4);

            float halfWidth = data.width * 0.5f;
            float halfHeight = data.height * 0.5f;

            int offset = (int) (data.startOffset) % 4;

            offset = 4 + offset;
            offset %= 4;

            for (int i = 0; i < 4; i++) {
                int index = i + offset;
                index %= 4;

                switch (index) {
                    case 0:
                        positions.array[i].x = data.center.x - halfWidth;
                        positions.array[i].y = data.center.y + halfHeight;
                        break;

                    case 1:
                        positions.array[i].x = data.center.x + halfWidth;
                        positions.array[i].y = data.center.y + halfHeight;
                        break;

                    case 2:
                        positions.array[i].x = data.center.x + halfWidth;
                        positions.array[i].y = data.center.y - halfHeight;
                        break;

                    case 3:
                        positions.array[i].x = data.center.x - halfWidth;
                        positions.array[i].y = data.center.y - halfHeight;
                        break;
                }
            }
        }

        public static void SetPointsRound(ref List_float2 positions, in RoundPointListData data) {
            if (data.length <= 0) return;

            float absLength = math.abs(data.length);

            int startResolution = ShapeKit.ResolveEllipseResolution(new float2(data.width * 0.5f, data.height * 0.5f), data.resolution); // * 2;

            int numFullSteps = (int) (math.ceil(startResolution * absLength));
            float partStepAmount = 1.0f + ((startResolution * absLength) - numFullSteps);

            bool addPartialStep = partStepAmount >= 0.0001f;

            int resolution = numFullSteps;

            if (addPartialStep) {
                resolution++;
            }

            if (data.useCenterPoint) {
                resolution++;
            }

            positions.SetSize(resolution);

            if (data.useCenterPoint) {
                positions.array[resolution - 1].x = data.center.x;
                positions.array[resolution - 1].y = data.center.y;
            }

            float halfWidth = math.max(0.001f, data.width * 0.5f);
            float halfHeight = math.max(0.001f, data.height * 0.5f);

            float angle = data.startOffset * GeoUtils.TwoPI;
            float angleIncrement = (GeoUtils.TwoPI) / startResolution;

            if (data.skipLastPoint) {
                angleIncrement = (GeoUtils.TwoPI) / ((float) startResolution + 1);
            }

            int direction = -1;
            if (data.direction == ArcDirection.Forward) {
                direction = 1;
            }

            angleIncrement *= direction;

            float relCompletion;

            for (int i = 0; i < numFullSteps; i++) {
                relCompletion = (float) i / resolution;

                positions.array[i].x = data.center.x + math.sin(angle) * (halfWidth + (halfWidth * data.endRadius * relCompletion));
                positions.array[i].y = data.center.y + math.cos(angle) * (halfHeight + (halfHeight * data.endRadius * relCompletion));

                angle += angleIncrement;
            }

            // add last point
            if (addPartialStep) {
                relCompletion = (numFullSteps + partStepAmount) / resolution;
//				angle -= angleIncrement * (1.0f - partStepAmount);
                positions.array[numFullSteps].x = data.center.x + math.sin(angle) * (halfWidth + (halfWidth * data.endRadius * relCompletion));
                positions.array[numFullSteps].y = data.center.y + math.cos(angle) * (halfHeight + (halfHeight * data.endRadius * relCompletion));

                int prevStep = math.max(numFullSteps - 1, 0);

                // lerp back to partial position
                positions.array[numFullSteps].x = Mathf.LerpUnclamped(positions.array[prevStep].x, positions.array[numFullSteps].x, partStepAmount);
                positions.array[numFullSteps].y = Mathf.LerpUnclamped(positions.array[prevStep].y, positions.array[numFullSteps].y, partStepAmount);
            }
        }

        public static void SetPointsRadialGraph(ref List_float2 positions, PointListGeneratorData data) {
            if (data.length <= 0) return;

            int resolution = data.FloatValues.Length;

            if (data.FloatValues.Length < 3)
                return;

            positions.SetSize(resolution);

            float angle = data.startOffset * GeoUtils.TwoPI;
            float angleIncrement = GeoUtils.TwoPI / resolution;

            for (int i = 0; i < resolution; i++) {

                float value = Mathf.InverseLerp(data.minValue, data.maxValue, data.FloatValues[i]);

                value *= data.radius;

                positions.array[i].x = data.center.x + Mathf.Sin(angle) * value;
                positions.array[i].y = data.center.y + Mathf.Cos(angle) * value;

                angle += angleIncrement;
            }
        }

        public static void SetPointsLineGraph(ref List_float2 positions, PointListGeneratorData data) {
            if (data.length <= 0) return;

            int resolution = data.FloatValues.Length;

            if (data.FloatValues.Length < 2) {
                return;
            }

            if (data.centerPoint) {
                resolution += 2;
            }

            positions.SetSize(resolution);

            float xPos = data.center.x + data.width * -0.5f;

            float xStep = data.width / (data.FloatValues.Length - 1.0f);

            for (int i = 0; i < data.FloatValues.Length; i++) {
                float value = Mathf.InverseLerp(data.minValue, data.maxValue, data.FloatValues[i]);

                value -= 0.5f;

                value *= data.height;

                positions.array[i].x = xPos;
                positions.array[i].y = data.center.y + value;

                xPos += xStep;
            }

            if (data.centerPoint) {
                positions.array[data.FloatValues.Length].x = data.center.x + data.width * 0.5f;
                positions.array[data.FloatValues.Length].y = data.center.y - data.height * 0.5f;

                positions.array[data.FloatValues.Length + 1].x = data.center.x + data.width * -0.5f;
                positions.array[data.FloatValues.Length + 1].y = positions.array[data.FloatValues.Length].y;
            }
        }

        public static void SetPointsAngleLine(ref List_float2 positions, PointListAngleLineData data) {
            if (data.length <= 0) return;

            positions.SetSize(2);

            math.sincos(data.angle * GeoUtils.TwoPI, out float xDir, out float yDir);

            float startOffset = data.length * data.startOffset;

            positions.array[0].x = data.center.x + xDir * startOffset;
            positions.array[0].y = data.center.y + yDir * startOffset;

            positions.array[1].x = data.center.x + xDir * (data.length + startOffset);
            positions.array[1].y = data.center.y + yDir * (data.length + startOffset);
        }

        public static void SetPointsStar(ref List_float2 positions, in PointListStarData data) {

            if (data.length <= 0) return;
            
            int resolution = (data.resolution < 2 ? 2 : data.resolution) * 2;

            positions.SetSize(resolution);

            float angle = data.startOffset * GeoUtils.TwoPI;
            float angleIncrement = (GeoUtils.TwoPI * data.length) / resolution;

            float outerRadiusX = data.width;
            float outerRadiusY = data.height;

            float spikeAmount = data.spikeAmount <= 0 ? 0.5f : data.spikeAmount;

            float innerRadiusX = spikeAmount * outerRadiusX;
            float innerRadiusY = spikeAmount * outerRadiusX;

            for (int i = 0; i < resolution; i += 2) {
                // add outer point
                math.sincos(angle, out float s, out float c);
                positions.array[i].x = data.center.x + s * outerRadiusX;
                positions.array[i].y = data.center.y + c * outerRadiusY;

                angle += angleIncrement;
                math.sincos(angle, out s, out c);

                // add inner point
                positions.array[i + 1].x = data.center.x + s * innerRadiusX;
                positions.array[i + 1].y = data.center.y + c * innerRadiusY;

                angle += angleIncrement;
            }
        }

        public static unsafe void SetPointsGear(ref List_float2 positions, PointListGeneratorData data) {
            if (data.length <= 0) return;

            int resolution = data.resolution * 4;

            positions.SetSize(resolution);

            float angle = data.startOffset * GeoUtils.TwoPI;
            float angleIncrement = GeoUtils.TwoPI / data.resolution;

            float outerRadiusX = data.width;
            float outerRadiusY = data.height;

            float innerRadiusX = data.endRadius * outerRadiusX;
            float innerRadiusY = data.endRadius * outerRadiusY;

            float bottomAngleOffset = angleIncrement * 0.49f * data.innerScale;
            float topAngleOffset = angleIncrement * 0.49f * data.outerScale;

            for (int i = 0; i < data.resolution; i++) {
                int index = i * 4;

                // add first inner point
                positions.array[index].x = data.center.x + Mathf.Sin(angle - bottomAngleOffset) * innerRadiusX;
                positions.array[index].y = data.center.y + Mathf.Cos(angle - bottomAngleOffset) * innerRadiusY;

                // add first outer point
                positions.array[index + 1].x = data.center.x + Mathf.Sin(angle - topAngleOffset) * outerRadiusX;
                positions.array[index + 1].y = data.center.y + Mathf.Cos(angle - topAngleOffset) * outerRadiusY;

                // add secont outer point
                positions.array[index + 2].x = data.center.x + Mathf.Sin(angle + topAngleOffset) * outerRadiusX;
                positions.array[index + 2].y = data.center.y + Mathf.Cos(angle + topAngleOffset) * outerRadiusY;

                // add second inner point
                positions.array[index + 3].x = data.center.x + Mathf.Sin(angle + bottomAngleOffset) * innerRadiusX;
                positions.array[index + 3].y = data.center.y + Mathf.Cos(angle + bottomAngleOffset) * innerRadiusY;

                angle += angleIncrement;
            }
        }

    }

    public struct PointListStarData {

        public int resolution;
        public float startOffset;
        public float length;
        public float width;
        public float height;
        public float spikeAmount;
        public float2 center;

    }

    public struct PointListAngleLineData {

        public float angle;
        public float length;
        public float startOffset;
        public float2 center;

    }

    public struct RectPointListData {

        public float width;
        public float height;
        public float startOffset;
        public float2 center;

    }

    public struct RoundPointListData {

        public float length;
        public RoundingResolution resolution;
        public bool useCenterPoint;
        public bool skipLastPoint;
        public float2 center;
        public float width;
        public float height;
        public ArcDirection direction;
        public float startOffset;
        public float endRadius;

    }

}