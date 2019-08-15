using System;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public static class SutherlandHodgman {

        private struct Edge {

            public Edge(Vector2 from, Vector2 to) {
                this.from = from;
                this.to = to;
            }

            public readonly Vector2 from;
            public readonly Vector2 to;

        }


        public static void GetIntersectedPolygon(StructList<Vector2> subjectPoly, StructList<Vector2> clipPoly, ref StructList<Vector2> outputList) {
                
            if (subjectPoly.size < 3 || clipPoly.size < 3) {
                throw new ArgumentException();
            }

            if (subjectPoly.size == 4 && clipPoly.size == 4) {
                // todo -- if both are rectangles then just do a rect intersect
            }

            if (outputList == null) {
                outputList = StructList<Vector2>.Get();
            }
                
            StructList<Vector2> inputList = StructList<Vector2>.Get();
            outputList.AddRange(subjectPoly);
            inputList.AddRange(outputList);
                
            //	Make sure it's clockwise
//                if (!IsClockwise(subjectPoly)) {
////                    outputList.Reverse();
//                }

            int idx = 0;
            StructList<Edge> edges = StructList<Edge>.GetMinSize(subjectPoly.size);

            for (int k = clipPoly.size - 1; k > 0; k--) {
                edges.array[idx++] = new Edge(clipPoly[k], clipPoly[k - 1]);
            }

            edges.array[idx] = new Edge(clipPoly[0], clipPoly[clipPoly.size - 1]);
            edges.size = idx;

            //	Walk around the clip polygon clockwise
            // input list = last frame output list
            for (int i = 0; i < edges.size; i++) {
                Edge clipEdge = edges[i];
                StructList<Vector2> swap = inputList;
                inputList = outputList;
                outputList = swap;
                outputList.size = 0;

                //	Sometimes when the polygons don't intersect
                if (inputList.Count == 0) {
                    break;
                }

                Vector2 S = inputList[inputList.Count - 1];

                for (int inputIdx = 0; inputIdx < inputList.Count; inputIdx++) {
                    Vector2 E = inputList[inputIdx];
                    bool sInside = IsInside(clipEdge, S);
                    if (IsInside(clipEdge, E)) {
                        if (!sInside) {
                            if (GetIntersect(S, E, clipEdge.from, clipEdge.to, out Vector2 intersect)) {
                                outputList.Add(intersect);
                            }
                            else {
                                throw new ApplicationException("Line segments don't intersect"); //	may be colinear, or may be a bug
                            }
                        }

                        outputList.Add(E);
                    }
                    else if (sInside) {
                        if (GetIntersect(S, E, clipEdge.from, clipEdge.to, out Vector2 intersect)) {
                            outputList.Add(intersect);
                        }
                        else {
                            throw new ApplicationException("Line segments don't intersect"); //	may be colinear, or may be a bug
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
            Vector2 tmp1 = edge.to - edge.from;
            Vector2 tmp2 = test - edge.to;

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