using UIForia.ListTypes;
using Unity.Mathematics;
using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public static unsafe class PointsGenerator {

        public static void SetPoints(ref List_float2 positions, PointListGeneratorData data) {
            switch (data.pointListGeneratorType) {
                case PointListGeneratorType.Custom:
                    break;

                case PointListGeneratorType.Rect:
                    SetPointsRect(ref positions, data);
                    break;

                case PointListGeneratorType.Round:
                    SetPointsRound(ref positions, data);
                    break;

                case PointListGeneratorType.RadialGraph:
                    SetPointsRadialGraph(ref positions, data);
                    break;

                case PointListGeneratorType.LineGraph:
                    SetPointsLineGraph(ref positions, data);
                    break;

                case PointListGeneratorType.AngleLine:
                    SetPointsAngleLine(ref positions, data);
                    break;

                case PointListGeneratorType.Star:
                    SetPointsStar(ref positions, data);
                    break;

                case PointListGeneratorType.Gear:
                    SetPointsGear(ref positions, data);
                    break;
            }
        }

        public static void SetPointsRect(ref List_float2 positions, PointListGeneratorData data) {

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

        public static void SetPointsRound(ref List_float2 positions, in PointListGeneratorData data) {
            float absLength = math.abs(data.length);

            int numFullSteps = (int)(math.ceil(data.resolution * absLength));
            float partStepAmount = 1.0f + ((data.resolution * absLength) - numFullSteps);

            bool addPartialStep = partStepAmount >= 0.0001f;

            int resolution = numFullSteps;

            if (addPartialStep) {
                resolution++;
            }

            if (data.centerPoint) {
                resolution++;
            }

            positions.SetSize(resolution);

            if (data.centerPoint) {
                positions.array[resolution - 1].x = data.center.x;
                positions.array[resolution - 1].y = data.center.y;
            }

            float halfWidth = math.max(0.001f, data.width * 0.5f);
            float halfHeight = math.max(0.001f, data.height * 0.5f);

            float angle = data.startOffset * GeoUtils.TwoPI;
            float angleIncrement = (GeoUtils.TwoPI) / data.resolution;

            if (data.skipLastPosition) {
                angleIncrement = (GeoUtils.TwoPI) / ((float) data.resolution + 1);
            }

            angleIncrement *= math.sign(data.direction);

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

        public static unsafe void SetPointsRadialGraph(ref List_float2 positions, PointListGeneratorData data) {
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

        public static unsafe void SetPointsLineGraph(ref List_float2 positions, PointListGeneratorData data) {
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

        public static unsafe void SetPointsAngleLine(ref List_float2 positions, PointListGeneratorData data) {
            positions.SetSize(2);

            float xDir = Mathf.Sin(data.angle * GeoUtils.TwoPI);
            float yDir = Mathf.Cos(data.angle * GeoUtils.TwoPI);

            float startOffset = data.length * data.startOffset;

            positions.array[0].x = data.center.x + xDir * startOffset;
            positions.array[0].y = data.center.y + yDir * startOffset;

            positions.array[1].x = data.center.x + xDir * (data.length + startOffset);
            positions.array[1].y = data.center.y + yDir * (data.length + startOffset);
        }

        public static unsafe void SetPointsStar(ref List_float2 positions, PointListGeneratorData data) {
            int resolution = data.resolution * 2;

            positions.SetSize(resolution);

            float angle = data.startOffset * GeoUtils.TwoPI;
            float angleIncrement = (GeoUtils.TwoPI * data.length) / resolution;

            float outerRadiusX = data.width;
            float outerRadiusY = data.height;

            float innerRadiusX = data.endRadius * outerRadiusX;
            float innerRadiusY = data.endRadius * outerRadiusX;

            for (int i = 0; i < resolution; i += 2) {
                // add outer point
                positions.array[i].x = data.center.x + math.sin(angle) * outerRadiusX;
                positions.array[i].y = data.center.y + math.cos(angle) * outerRadiusY;

                angle += angleIncrement;

                // add inner point
                positions.array[i + 1].x = data.center.x + math.sin(angle) * innerRadiusX;
                positions.array[i + 1].y = data.center.y + math.cos(angle) * innerRadiusY;

                angle += angleIncrement;
            }
        }

        public static unsafe void SetPointsGear(ref List_float2 positions, PointListGeneratorData data) {
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

}