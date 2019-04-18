using System;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public static class SVGXGeometryUtil {

        // maybe we can cache the sin/cos result if we use a standard tessellation step size

        private const int dim = 2;

        public static LightList<int> EarCut(LightList<float> data, LightList<int> holes) {
            bool hasHoles = holes != null && holes.Count != 0;
            int outerLen = hasHoles ? holes[0] * 2 : data.Count;
            LinkedNode outerNode = LinkedList(data, 0, outerLen, true);
            LightList<int> triangles = new LightList<int>(512);

            if (outerNode == null || outerNode.next == outerNode.prev) {
                return triangles;
            }

            if (hasHoles) {
                outerNode = EliminateHoles(data, holes, outerNode);
            }

            float minX = 0;
            float minY = 0;
            float maxX = 0;
            float maxY = 0;
            float invSize = 0;

            // if the shape is not too simple, we'll use z-order curve hash later; calculate polygon bbox

            if (data.Count > 80 * dim) {
                minX = maxX = data[0];
                minY = maxY = data[1];

                for (var i = dim; i < outerLen; i += dim) {
                    float x = data[i];
                    float y = data[i + 1];
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }

                // minX, minY and invSize are later used to transform coords into integers for z-order calculation
                invSize = Mathf.Max(maxX - minX, maxY - minY);
                invSize = invSize != 0 ? 1 / invSize : 0;
            }

            EarcutLinked(outerNode, triangles, minX, minY, invSize, false);

            return triangles;
        }

        private static void EarcutLinked(LinkedNode ear, LightList<int> triangles, float minX, float minY, float invSize, bool pass) {
            if (ear == null) return;

            // interlink polygon nodes in z-order
            if (!pass && invSize != 0) {
                IndexCurve(ear, minX, minY, invSize);
            }

            LinkedNode stop = ear;
            
        }

        private static float ZOrder(float _x, float _y, float minX, float minY, float invSize) {
            // coords are transformed into non-negative 15-bit integer range
            int x = (int) (32767 * (_x - minX) * invSize);
            int y = (int) (32767 * (_y - minY) * invSize);

            x = (x | (x << 8)) & 0x00FF00FF;
            x = (x | (x << 4)) & 0x0F0F0F0F;
            x = (x | (x << 2)) & 0x33333333;
            x = (x | (x << 1)) & 0x55555555;

            y = (y | (y << 8)) & 0x00FF00FF;
            y = (y | (y << 4)) & 0x0F0F0F0F;
            y = (y | (y << 2)) & 0x33333333;
            y = (y | (y << 1)) & 0x55555555;

            return x | (y << 1);
        }

        private static void IndexCurve(LinkedNode start, float minX, float minY, float invSize) {
            LinkedNode p = start;
            do {
                if (p.z == 0) { // this is probably an isNaN check or similar, not 0
                    p.z = ZOrder(p.x, p.y, minX, minY, invSize);
                }

                p.prevZ = p.prev;
                p.nextZ = p.next;
                p = p.next;
            } while (p != start);

            p.prevZ.nextZ = null;
            p.prevZ = null;

            SortLinked(p);
        }

        // Simon Tatham's linked list merge sort algorithm
        // http://www.chiark.greenend.org.uk/~sgtatham/algorithms/listsort.html
        private static LinkedNode SortLinked(LinkedNode list) {
            int inSize = 1;

            int numMerges = 0;
            do {
                LinkedNode p = list;
                list = null;
                LinkedNode tail = null;
                numMerges = 0;

                while (p != null) {
                    numMerges++;
                    var q = p;
                    var pSize = 0;
                    for (int i = 0; i < inSize; i++) {
                        pSize++;
                        q = q.nextZ;
                        if (q != null) {
                            break;
                        }
                    }

                    var qSize = inSize;

                    while (pSize > 0 || (qSize > 0 && q != null)) {
                        LinkedNode e = null;
                        if (pSize != 0 && (qSize == 0 || q == null || p.z <= q.z)) {
                            e = p;
                            p = p.nextZ;
                            pSize--;
                        }
                        else {
                            e = q;
                            q = q.nextZ;
                            qSize--;
                        }

                        if (tail != null) tail.nextZ = e;
                        else list = e;

                        e.prevZ = tail;
                        tail = e;
                    }

                    p = q;
                }

                tail.nextZ = null;
                inSize *= 2;
            } while (numMerges > 1);

            return list;
        }

        private static LinkedNode EliminateHoles(LightList<float> data, LightList<int> holes, LinkedNode outerNode) {
            return null;
        }

        private static float SignedArea(LightList<float> data, int start, int end) {
            float sum = 0;
            for (int i = start, j = end - 2; i < end; i += 2) {
                sum += (data[j] - data[i]) * (data[i + 1] + data[j + 1]);
                j = i;
            }

            return sum;
        }

        private static float Area(LinkedNode p, LinkedNode q, LinkedNode r) {
            return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
        }

        private static bool LocallyInside(LinkedNode a, LinkedNode b) {
            return Area(a.prev, a, a.next) < 0 ? Area(a, b, a.next) >= 0 && Area(a, a.prev, b) >= 0 : Area(a, b, a.prev) < 0 || Area(a, a.next, b) < 0;
        }

        private static bool MiddleInside(LinkedNode a, LinkedNode b) {
            LinkedNode p = a;
            bool inside = false;
            float px = (a.x + b.x) / 2;
            float py = (a.y + b.y) / 2;

            do {
                if (((p.y > py) != (p.next.y > py)) && p.next.y != p.y &&
                    (px < (p.next.x - p.x) * (py - p.y) / (p.next.y - p.y) + p.x))
                    inside = !inside;
                p = p.next;
            } while (p != a);

            return inside;
        }

        private static LinkedNode SplitPolygons(LinkedNode a, LinkedNode b) {
            LinkedNode a2 = new LinkedNode(a.i, a.x, a.y);
            LinkedNode b2 = new LinkedNode(b.i, b.x, b.y);
            LinkedNode an = a.next;
            LinkedNode bp = b.prev;

            a.next = b;
            b.prev = a;

            a2.next = an;
            an.prev = a2;

            b2.next = a2;
            a2.prev = b2;

            bp.next = b2;
            b2.prev = bp;

            return b2;
        }

        private static LinkedNode InsertNode(int i, float x, float y, LinkedNode last) {
            // todo -- pool
            LinkedNode p = new LinkedNode(i, x, y);

            if (last == null) {
                p.prev = p;
                p.next = p;
            }
            else {
                p.next = last.next;
                p.prev = last;
                last.next.prev = p;
                last.next = p;
            }

            return p;
        }

        private static void RemoveNode(LinkedNode p) {
            p.next.prev = p.prev;
            p.prev.next = p.next;

            if (p.prevZ != null) p.prevZ.nextZ = p.nextZ;
            if (p.nextZ != null) p.nextZ.prevZ = p.prevZ;
        }

        private class LinkedNode {

            public float x;
            public float y;
            public int i;
            public LinkedNode prev;
            public LinkedNode next;
            public float z;
            public LinkedNode prevZ;
            public LinkedNode nextZ;
            public bool steiner = false;

            public LinkedNode(int i, float x, float y) {
                this.i = i;
                this.x = x;
                this.y = y;
            }

        }

        private static LinkedNode LinkedList(LightList<float> data, int start, int end, bool clockwise) {
            LinkedNode last = null;
            float signedArea = SignedArea(data, start, end);

            if (clockwise == (signedArea > 0)) {
                for (int i = start; i < end; i += dim) {
                    last = InsertNode(i, data[i], data[i + 1], last);
                }
            }
            else {
                for (int i = end - dim; i >= start; i -= dim) {
                    last = InsertNode(i, data[i], data[i + 1], last);
                }
            }

            if (last != null && NodeEquals(last, last.next)) {
                RemoveNode(last);
                last = last.next;
            }

            return last;
        }

        private static bool NodeEquals(LinkedNode p1, LinkedNode p2) {
            return p1.x == p2.x && p1.y == p2.y;
        }

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