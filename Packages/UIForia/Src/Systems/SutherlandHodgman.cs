using System;
using UIForia.Util;
using UnityEngine;


namespace UIForia.Rendering {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;

    namespace Sutherland {

        public static class SutherlandHodgman {

            #region Class: Edge

            /// <summary>
            /// This represents a line segment
            /// </summary>
            private class Edge {

                public Edge(Vector2 from, Vector2 to) {
                    this.From = from;
                    this.To = to;
                }

                public readonly Vector2 From;
                public readonly Vector2 To;

            }

            #endregion

            /// <summary>
            /// This clips the subject polygon against the clip polygon (gets the intersection of the two polygons)
            /// </summary>
            /// <remarks>
            /// Based on the psuedocode from:
            /// http://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman
            /// </remarks>
            /// <param name="subjectPoly">Can be concave or convex</param>
            /// <param name="clipPoly">Must be convex</param>
            /// <returns>The intersection of the two polygons (or null)</returns>
            public static Vector2[] GetIntersectedPolygon(Vector2[] subjectPoly, Vector2[] clipPoly) {
                if (subjectPoly.Length < 3 || clipPoly.Length < 3) {
                    throw new ArgumentException(string.Format("The polygons passed in must have at least 3 points: subject={0}, clip={1}", subjectPoly.Length.ToString(), clipPoly.Length.ToString()));
                }

                List<Vector2> outputList = subjectPoly.ToList();

                //	Make sure it's clockwise
                if (!IsClockwise(subjectPoly)) {
                    outputList.Reverse();
                }

                //	Walk around the clip polygon clockwise
                foreach (Edge clipEdge in IterateEdgesClockwise(clipPoly)) {
                    List<Vector2> inputList = outputList.ToList(); //	clone it
                    outputList.Clear();

                    if (inputList.Count == 0) {
                        //	Sometimes when the polygons don't intersect, this list goes to zero.  Jump out to avoid an index out of range exception
                        break;
                    }

                    Vector2 S = inputList[inputList.Count - 1];

                    foreach (Vector2 E in inputList) {
                        if (IsInside(clipEdge, E)) {
                            if (!IsInside(clipEdge, S)) {
                                Vector2? point = GetIntersect(S, E, clipEdge.From, clipEdge.To);
                                if (point == null) {
                                    throw new ApplicationException("Line segments don't intersect"); //	may be colinear, or may be a bug
                                }
                                else {
                                    outputList.Add(point.Value);
                                }
                            }

                            outputList.Add(E);
                        }
                        else if (IsInside(clipEdge, S)) {
                            Vector2? point = GetIntersect(S, E, clipEdge.From, clipEdge.To);
                            if (point == null) {
                                throw new ApplicationException("Line segments don't intersect"); //	may be colinear, or may be a bug
                            }
                            else {
                                outputList.Add(point.Value);
                            }
                        }

                        S = E;
                    }
                }

                //	Exit Function
                return outputList.ToArray();
            }

            #region Private Methods

            /// <summary>
            /// This iterates through the edges of the polygon, always clockwise
            /// </summary>
            private static IEnumerable<Edge> IterateEdgesClockwise(Vector2[] polygon) {
                if (IsClockwise(polygon)) {
                    #region Already clockwise

                    for (int cntr = 0; cntr < polygon.Length - 1; cntr++) {
                        yield return new Edge(polygon[cntr], polygon[cntr + 1]);
                    }

                    yield return new Edge(polygon[polygon.Length - 1], polygon[0]);

                    #endregion
                }
                else {
                    #region Reverse

                    for (int cntr = polygon.Length - 1; cntr > 0; cntr--) {
                        yield return new Edge(polygon[cntr], polygon[cntr - 1]);
                    }

                    yield return new Edge(polygon[0], polygon[polygon.Length - 1]);

                    #endregion
                }
            }

            /// <summary>
            /// Returns the intersection of the two lines (line segments are passed in, but they are treated like infinite lines)
            /// </summary>
            /// <remarks>
            /// Got this here:
            /// http://stackoverflow.com/questions/14480124/how-do-i-detect-triangle-and-rectangle-intersection
            /// </remarks>
            private static Vector2? GetIntersect(Vector2 line1From, Vector2 line1To, Vector2 line2From, Vector2 line2To) {
                Vector2 direction1 = line1To - line1From;
                Vector2 direction2 = line2To - line2From;
                float dotPerp = (direction1.x * direction2.y) - (direction1.y * direction2.x);

                // If it's 0, it means the lines are parallel so have infinite intersection points
                if (IsNearZero(dotPerp)) {
                    return null;
                }

                Vector2 c = line2From - line1From;
                float t = (c.x * direction2.y - c.y * direction2.x) / dotPerp;
                //if (t < 0 || t > 1)
                //{
                //    return null;		//	lies outside the line segment
                //}

                //double u = (c.X * direction1.Y - c.Y * direction1.X) / dotPerp;
                //if (u < 0 || u > 1)
                //{
                //    return null;		//	lies outside the line segment
                //}

                //	Return the intersection point
                return line1From + (direction1 * t);
            }

            private static bool IsInside(Edge edge, Vector2 test) {
                bool? isLeft = IsLeftOf(edge, test);
                if (isLeft == null) {
                    //	Colinear points should be considered inside
                    return true;
                }

                return !isLeft.Value;
            }

            private static bool IsClockwise(Vector2[] polygon) {
                for (int cntr = 2; cntr < polygon.Length; cntr++) {
                    bool? isLeft = IsLeftOf(new Edge(polygon[0], polygon[1]), polygon[cntr]);
                    if (isLeft != null) //	some of the points may be colinear.  That's ok as long as the overall is a polygon
                    {
                        return !isLeft.Value;
                    }
                }

                throw new ArgumentException("All the points in the polygon are colinear");
            }

            /// <summary>
            /// Tells if the test point lies on the left side of the edge line
            /// </summary>
            private static bool? IsLeftOf(Edge edge, Vector2 test) {
                Vector2 tmp1 = edge.To - edge.From;
                Vector2 tmp2 = test - edge.To;

                double x = (tmp1.x * tmp2.y) - (tmp1.y * tmp2.x); //	dot product of perpendicular?

                if (x < 0) {
                    return false;
                }
                else if (x > 0) {
                    return true;
                }
                else {
                    //	Colinear points;
                    return null;
                }
            }

            private static bool IsNearZero(double testValue) {
                return Math.Abs(testValue) <= .000000001d;
            }

            #endregion

        }

    }

    public static class SutherlandHodgman {

        private struct Edge {

            public Edge(in Vector2 from, in Vector2 to) {
                this.from = from;
                this.to = to;
            }

            public readonly Vector2 from;
            public readonly Vector2 to;

        }

        [ThreadStatic] private static StructList<Edge> s_Edges;

        public static void GetIntersectedPolygon(StructList<Vector2> subjectPoly, StructList<Vector2> clipPolyList, ref StructList<Vector2> outputList) {
            //public static Vector2[] GetIntersectedPolygon(Vector2[] subjectPoly, Vector2[] clipPoly)

            if (outputList == null) {
                outputList = StructList<Vector2>.Get();
            }

            outputList.AddRange(Sutherland.SutherlandHodgman.GetIntersectedPolygon(subjectPoly.ToArray(), clipPolyList.ToArray()));

            if(Time.deltaTime > 0) return;
            
            s_Edges = s_Edges ?? new StructList<Edge>();

            if (subjectPoly.size < 3 || clipPolyList.size < 3) {
                throw new ArgumentException();
            }

            if (subjectPoly.size == 4 && clipPolyList.size == 4) {
                // todo -- if both are rectangles then just do a rect intersect
            }

            if (outputList == null) {
                outputList = StructList<Vector2>.Get();
            }

            StructList<Vector2> inputList = StructList<Vector2>.Get();
            outputList.AddRange(subjectPoly);
            inputList.AddRange(outputList);

            //	Make sure it's clockwise
            // if (!IsClockwise(subjectPoly)) {
            //    outputList.Reverse();
            // }

            int idx = 0;
            s_Edges.size = 0;
            StructList<Edge> edgeList = s_Edges;

            int clipPolyCount = clipPolyList.size;
            Vector2[] clipPoly = clipPolyList.array;
            for (int k = clipPolyCount - 1; k > 0; k--) {
                edgeList.array[idx++] = new Edge(clipPoly[k], clipPoly[k - 1]);
            }

            edgeList.array[idx++] = new Edge(clipPoly[0], clipPoly[clipPolyCount - 1]);
            edgeList.size = idx;

            //	Walk around the clip polygon clockwise
            // input list = last frame output list
            for (int i = 0; i < edgeList.size; i++) {
                ref Edge clipEdge = ref edgeList.array[i];
                StructList<Vector2> swap = inputList;
                inputList = outputList;
                outputList = swap;
                outputList.size = 0;

                //	Sometimes when the polygons don't intersect
                if (inputList.size == 0) {
                    break;
                }

                Vector2 S = inputList.array[inputList.size - 1];

                for (int inputIdx = 0; inputIdx < inputList.size; inputIdx++) {
                    ref Vector2 E = ref inputList.array[inputIdx];
                    bool sInside = IsInside(clipEdge, S);
                    if (IsInside(clipEdge, E)) {
                        if (!sInside) {
                            if (GetIntersect(S, E, clipEdge.from, clipEdge.to, out Vector2 intersect)) {
                                outputList.Add(intersect);
                            }
                            else {
                                throw new ApplicationException("Line segments don't intersect");
                            }
                        }

                        outputList.Add(E);
                    }
                    else if (sInside) {
                        if (GetIntersect(S, E, clipEdge.from, clipEdge.to, out Vector2 intersect)) {
                            outputList.Add(intersect);
                        }
                        else {
                            throw new ApplicationException("Line segments don't intersect");
                        }
                    }

                    S = E;
                }
            }

            StructList<Vector2>.Release(ref inputList);
        }

        private static bool GetIntersect(in Vector2 line1From, in Vector2 line1To, in Vector2 line2From, in Vector2 line2To, out Vector2 intersect) {
            // inlining subtraction so we don't call methods on `in` structs resulting in copies
            Vector2 direction1;
            Vector2 direction2;
            direction1.x = line1To.x - line1From.x;
            direction1.y = line1To.y - line1From.y;
            direction2.x = line2To.x - line2From.x;
            direction2.y = line2To.y - line2From.y;
            float dotPerp = (direction1.x * direction2.y) - (direction1.y * direction2.x);

            // If it's 0, it means the lines are parallel so have infinite intersection points
            if (dotPerp <= 0.0000001f && dotPerp >= -0.000001f) {
                intersect = default;
                return false;
            }

            Vector2 c;
            c.x = line2From.x - line1From.x;
            c.y = line2From.y - line1From.y;
            float t = (c.x * direction2.y - c.y * direction2.x) / dotPerp;

            intersect.x = line1From.x + (t * direction1.x);
            intersect.y = line1From.y + (t * direction1.y);
            return true;
        }

        private static bool IsInside(Edge edge, Vector2 test) {
            Vector2 tmp1 = default;
            Vector2 tmp2 = default;

            tmp1.x = edge.to.x - edge.from.x;
            tmp1.x = edge.to.y - edge.from.y;
            tmp2.x = test.x - edge.to.x;
            tmp2.y = test.y - edge.to.y;

            float x = (tmp1.x * tmp2.y) - (tmp1.y * tmp2.x);

            if (x < 0) {
                return true;
            }
            else if (x > 0) {
                return false;
            }
            else {
                return true;
            }
        }

        private static bool IsClockwise(StructList<Vector2> polygon) {
            for (int cntr = 2; cntr < polygon.size; cntr++) {
                bool? isLeft = IsLeftOf(new Edge(polygon.array[0], polygon.array[1]), polygon.array[cntr]);
                if (isLeft != null) //	some of the points may be colinear.  That's ok as long as the overall is a polygon
                {
                    return !isLeft.Value;
                }
            }

            throw new ArgumentException("All the points in the polygon are colinear");
        }

        private static bool? IsLeftOf(Edge edge, Vector2 test) {
            Vector2 tmp1 = edge.to - edge.from;
            Vector2 tmp2 = test - edge.to;

            float x = (tmp1.x * tmp2.y) - (tmp1.y * tmp2.x); //	dot product of perpendicular

            if (x < 0) {
                return false;
            }
            else if (x > 0) {
                return true;
            }
            else {
                //	Colinear points;
                return null;
            }
        }

    }

}