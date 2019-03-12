using System;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public static class SVGXGeometryUtil {

        // maybe we can cache the sin/cos result if we use a standard tessellation step size
        
        public static Vector2 PointOnCircle(Vector2 origin, float radianStep, float radius) {
            return new Vector2(
                origin.x + radius * Mathf.Cos(radianStep),
                origin.y + radius * Mathf.Sin(radianStep)
            );
        }
        
        public static Vector2 PointOnEllipse(Vector2 origin, float radianStep, float width, float height) {
            return new Vector2(
                origin.x + (width * 0.5f) * Mathf.Cos(radianStep),
                origin.y + (height * 0.5f) * Mathf.Sin(radianStep)
            );
        }
        
        public static bool IsWindingClockWise(LightList<Vector2> points) {
            if (points == null || points.Count < 2) {
                return false;
            }
            
            int pointsCount = points.Count;
            Vector2 lastPoint = points[0];
            Vector2[] array = points.Array;
            float sum = 0f;
            
            for (int i = 1; i < pointsCount; i++) {
                sum += (array[i].x - lastPoint.x) * (array[i].y + lastPoint.y);
                lastPoint = array[i];
            }

            return sum >= 0;
        }

        public static bool IsWindingClockWise(Vector2[] points) {
            
            if (points == null || points.Length < 2) {
                return false;
            }
            int pointsCount = points.Length;
            Vector2 lastPoint = points[0];
            float sum = 0f;
            for (int i = 1; i < pointsCount; i++) {
                sum += (points[i].x - lastPoint.x) * (points[i].y + lastPoint.y);
                lastPoint = points[i];
            }

            return sum >= 0;
        }

        internal static void GenerateStrokePathGeometry(LightList<Vector2> output, LightList<float> strokeWidths, SVGXMatrix matrix, Vector2[] points, RangeInt pointRange, bool isClosed) {
            //https://github.com/memononen/nanovg/blob/master/src/nanovg.c#L1650
            //https://github.com/memononen/nanovg/blob/master/src/nanovg.c#L1720
        }

        internal static RangeInt GenerateStrokeGeometry(LightList<Vector2> output, StrokePlacement strokePlacement, float strokeWidth, SVGXShapeType shapeType, SVGXMatrix matrix, Vector2[] points, RangeInt pointRange, bool isClosed) {
            RangeInt retn = new RangeInt(output.Count, 0);

            switch (shapeType) {
                case SVGXShapeType.Unset:
                    break;

                case SVGXShapeType.Rect: {
                    // consider turning off AA for rect strokes that are not transformed

                    Vector2 p0 = matrix.Transform(points[pointRange.start + 0]);
                    Vector2 p1 = matrix.Transform(points[pointRange.start + 1]);

                    if (strokePlacement == StrokePlacement.Inside) {
                        p0.x += strokeWidth * 0.5f;
                        p1.x -= strokeWidth * 0.5f;
                        p0.y += strokeWidth * 0.5f;
                        p1.y -= strokeWidth * 0.5f;
                    }
                    else if (strokePlacement == StrokePlacement.Outside) {
                        p0.x -= strokeWidth * 0.5f;
                        p1.x += strokeWidth * 0.5f;
                        p0.y -= strokeWidth * 0.5f;
                        p1.y += strokeWidth * 0.5f;
                    }

                    output.Add(new Vector2(p0.x, p1.y));
                    output.Add(p0);
                    output.Add(new Vector2(p1.x, p0.y));
                    output.Add(p1);
                    output.Add(new Vector2(p0.x, p1.y));
                    output.Add(p0);
                    output.Add(new Vector2(p1.x, p0.y));

                    break;
                }

                case SVGXShapeType.RoundedRect:

                    output.Add(Vector2.zero);

                    CreateShapeGeometry(output, new Vector2(-strokeWidth * 0.5f, -strokeWidth * 0.5f), shapeType, pointRange, points, matrix);

                    output[retn.start] = output[output.Count - 1];
                    output.Add(output[retn.start + 2]);
                    output.Add(output[retn.start + 3]);

                    break;

                case SVGXShapeType.Path: {
                    if (pointRange.length == 2) {
                        Vector2 p0 = matrix.Transform(points[pointRange.start + 0]);
                        Vector2 p1 = matrix.Transform(points[pointRange.start + 1]);
                        output.Add(p0 - (p1 - p0));
                        output.Add(p0);
                        output.Add(p1);
                        output.Add(p1 + (p1 - p0));
                        break;
                    }

                    if (isClosed) {
                        output.Add(Vector2.zero);
                    }
                    else {
                        output.Add(Vector2.zero);
                    }

                    for (int i = pointRange.start; i < pointRange.end; i++) {
                        output.Add(matrix.Transform(points[i]));
                    }

                    if (isClosed) {
                        output[retn.start] = output[output.Count - 1];
                        output.Add(output[retn.start + 2]);
                        output.Add(output[retn.start + 3]);
                    }
                    else {
                        output[retn.start] = output[1];
                        output.Add(output[output.Count - 1]);
                    }

                    break;
                }
                case SVGXShapeType.Ellipse:
                case SVGXShapeType.Circle: {
                    Vector2 p0 = matrix.Transform(points[pointRange.start + 0]);
                    Vector2 p1 = matrix.Transform(points[pointRange.start + 1]);

                    float rx = (p1.x - p0.x) * 0.5f;
                    float ry = (p1.y - p0.y) * 0.5f;
                    float cx = p0.x + rx;
                    float cy = p0.y + ry;

                    output.Add(Vector2.zero);
                    output.Add(p0 + new Vector2(0, ry));

                    const float Kappa90 = 0.5522847493f;
                    SVGXBezier.CubicCurve(output,
                        output[output.Count - 1],
                        new Vector2(cx - rx, cy + ry * Kappa90),
                        new Vector2(cx - rx * Kappa90, cy + ry),
                        new Vector2(cx, cy + ry)
                    );
                    SVGXBezier.CubicCurve(
                        output,
                        output[output.Count - 1],
                        new Vector2(cx + rx * Kappa90, cy + ry),
                        new Vector2(cx + rx, cy + ry * Kappa90),
                        new Vector2(cx + rx, cy)
                    );
                    SVGXBezier.CubicCurve(
                        output,
                        output[output.Count - 1],
                        new Vector2(cx + rx, cy - ry * Kappa90),
                        new Vector2(cx + rx * Kappa90, cy - ry),
                        new Vector2(cx, cy - ry)
                    );
                    SVGXBezier.CubicCurve(
                        output,
                        output[output.Count - 1],
                        new Vector2(cx - rx * Kappa90, cy - ry),
                        new Vector2(cx - rx, cy - ry * Kappa90),
                        new Vector2(cx - rx, cy)
                    );

                    output[retn.start] = output[output.Count - 1];
                    output.Add(output[retn.start + 2]);
                    output.Add(output[retn.start + 3]);

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(shapeType), shapeType, null);
            }

            retn.length = output.Count - retn.start;

            return retn;
        }
        
        internal static RangeInt CreateShapeGeometry(LightList<Vector2> output, Vector2 offset, SVGXShapeType shapeType, RangeInt pointRange, Vector2[] points, SVGXMatrix matrix) {
            int start = pointRange.start;
            RangeInt retn = new RangeInt(output.Count, 0);

            switch (shapeType) {
                case SVGXShapeType.Circle:
                case SVGXShapeType.Ellipse:

                    Vector2 p0 = offset + matrix.Transform(points[start]);
                    Vector2 p1 = points[start + 1]; // width, height -> no need to transform

                    float rx = (p1.x - p0.x) * 0.5f;
                    float ry = (p1.y - p0.y) * 0.5f;
                    float cx = p0.x + rx;
                    float cy = p0.y + ry;

                    output.Add(Vector2.zero);
                    output.Add(p0 + new Vector2(0, ry));

                    const float Kappa90 = 0.5522847493f;
                    SVGXBezier.CubicCurve(output,
                        output[output.Count - 1],
                        new Vector2(cx - rx, cy + ry * Kappa90),
                        new Vector2(cx - rx * Kappa90, cy + ry),
                        new Vector2(cx, cy + ry)
                    );
                    SVGXBezier.CubicCurve(
                        output,
                        output[output.Count - 1],
                        new Vector2(cx + rx * Kappa90, cy + ry),
                        new Vector2(cx + rx, cy + ry * Kappa90),
                        new Vector2(cx + rx, cy)
                    );
                    SVGXBezier.CubicCurve(
                        output,
                        output[output.Count - 1],
                        new Vector2(cx + rx, cy - ry * Kappa90),
                        new Vector2(cx + rx * Kappa90, cy - ry),
                        new Vector2(cx, cy - ry)
                    );
                    SVGXBezier.CubicCurve(
                        output,
                        output[output.Count - 1],
                        new Vector2(cx - rx * Kappa90, cy - ry),
                        new Vector2(cx - rx, cy - ry * Kappa90),
                        new Vector2(cx - rx, cy)
                    );

                    break;

                case SVGXShapeType.RoundedRect:
                    // todo -- using a large stroke with rounded rect causes vertices to 'spider web'. Not related to distance tolerance
                    Vector2 position = matrix.Transform(points[start]);
                    float x = position.x;
                    float y = position.y;
                    float w = points[start + 1].x;
                    float h = points[start + 1].y;

                    float rtl = points[start + 2].x;
                    float rtr = points[start + 2].y;
                    float rbl = points[start + 3].x;
                    float rbr = points[start + 3].y;

                    float halfW = w * 0.5f;
                    float halfH = h * 0.5f;
                    float rxBL = rbl < halfW ? rbl : halfW;
                    float ryBL = rbl < halfH ? rbl : halfH;
                    float rxBR = rbr < halfW ? rbr : halfW;
                    float ryBR = rbr < halfH ? rbr : halfH;
                    float rxTL = rtl < halfW ? rtl : halfW;
                    float ryTL = rtl < halfH ? rtl : halfH;
                    float rxTR = rtr < halfW ? rtr : halfW;
                    float ryTR = rtr < halfH ? rtr : halfH;

                    const float OneMinusKappa90 = 0.4477152f;

                    output.Add(new Vector2(x, y + ryTL)); // move to
                    Vector2 last = new Vector2(x, y + h - ryBL);

                    output.Add(last); // line to

                    const float distanceTolerance = 0.15f;

                    SVGXBezier.CubicCurve(
                        output,
                        last,
                        new Vector2(x, y + h - ryBL * OneMinusKappa90),
                        new Vector2(x + rxBL * OneMinusKappa90, y + h),
                        new Vector2(x + rxBL, y + h),
                        distanceTolerance
                    );

                    last = new Vector2(x + w - rxBR, y + h); // line to
                    output.Add(last);

                    SVGXBezier.CubicCurve(
                        output,
                        last,
                        new Vector2(x + w - rxBR * OneMinusKappa90, y + h),
                        new Vector2(x + w, y + h - ryBR * OneMinusKappa90),
                        new Vector2(x + w, y + h - ryBR),
                        distanceTolerance
                    );

                    last = new Vector2(x + w, y + ryTR); // line to
                    output.Add(last);

                    SVGXBezier.CubicCurve(
                        output,
                        last,
                        new Vector2(x + w, y + ryTR * OneMinusKappa90),
                        new Vector2(x + w - rxTR * OneMinusKappa90, y),
                        new Vector2(x + w - rxTR, y),
                        distanceTolerance
                    );

                    last = new Vector2(x + rxTL, y); // line to
                    output.Add(last);

                    SVGXBezier.CubicCurve(
                        output,
                        last,
                        new Vector2(x + rxTL * OneMinusKappa90, y),
                        new Vector2(x, y + ryTL * OneMinusKappa90),
                        new Vector2(x, y + ryTL),
                        distanceTolerance
                    );

                    break;
            }

            retn.length = output.Count - retn.start;
            return retn;
        }
    }

}