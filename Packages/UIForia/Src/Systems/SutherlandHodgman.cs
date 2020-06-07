using UIForia.Util.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Rendering {

    public unsafe static class SutherlandHodgman {

        public struct Edge {

            public float2 from;
            public float2 to;

        }

        /// <summary>
        /// This clips the subject polygon against the clip polygon (gets the intersection of the two polygons)
        /// </summary>
        public static void GetIntersectedPolygon(DataList<float2> subjectPoly, float2 * clipPoly, int clipPolySize, ref DataList<float2> outputList, ref DataList<float2> inputBuffer, ref DataList<Edge> edgeBuffer) {

            if (subjectPoly.size < 3 || clipPolySize < 3) {
                return;
            }

            inputBuffer.size = 0;
            edgeBuffer.size = 0;
            
            outputList.size = 0;
            outputList.AddRange(subjectPoly.GetArrayPointer(), subjectPoly.size);

            //	Make sure it's clockwise
            if (!IsClockwise(subjectPoly.GetArrayPointer(), subjectPoly.size, out bool invalid)) {
                outputList.Reverse();
            }

            if (invalid) {
                return;
            }

            IterateEdgesClockwise(clipPoly, clipPolySize, ref edgeBuffer);

            //	Walk around the clip polygon clockwise
            for (int i = 0; i < edgeBuffer.size; i++) {
                ref Edge clipEdge = ref edgeBuffer[i];
                inputBuffer.size = 0;
                inputBuffer.AddRange(outputList.GetArrayPointer(), outputList.size);
                outputList.size = 0;

                //	Sometimes when the polygons don't intersect, this list goes to zero.  Jump out to avoid an index out of range exception
                if (inputBuffer.size == 0) {
                    break;
                }

                float2 S = inputBuffer[inputBuffer.size - 1];

                for (int index = 0; index < inputBuffer.size; index++) {
                    ref float2 E = ref inputBuffer[index];
                    if (IsInside(clipEdge, E)) {
                        if (!IsInside(clipEdge, S)) {
                            float2? v = GetIntersect(S, E, clipEdge.from, clipEdge.to);
                            if (v == null) {
                                // throw new ApplicationException("Line segments don't intersect"); //	may be colinear, or may be a bug
                                outputList.size = 0;
                                return;
                            }

                            outputList.Add(v.Value);
                        }

                        outputList.Add(E);
                    }
                    else if (IsInside(clipEdge, S)) {
                        float2? float2 = GetIntersect(S, E, clipEdge.from, clipEdge.to);
                        if (float2 == null) {
                            outputList.size = 0;
                            return;
                            // throw new ApplicationException("Line segments don't intersect"); //	may be colinear, or may be a bug
                        }

                        outputList.Add(float2.Value);
                    }

                    S = E;
                }
            }
        }

        private static void IterateEdgesClockwise(float2 * polygon, int polygonSize, ref DataList<Edge> output) {
            output.EnsureCapacity(polygonSize);
            output.size = 0;

            if (IsClockwise(polygon, polygonSize, out bool _)) {
                for (int i = 0; i < polygonSize - 1; i++) {
                    ref Edge edge = ref output[output.size++];
                    edge.from = polygon[i];
                    edge.to = polygon[i + 1];
                }

                ref Edge last = ref output[output.size++];
                last.from = polygon[polygonSize - 1];
                last.to = polygon[0];
            }
            else {
                for (int i = polygonSize - 1; i > 0; i--) {
                    ref Edge edge = ref output[output.size++];
                    edge.from = polygon[i];
                    edge.to = polygon[i - 1];
                }

                ref Edge last = ref output[output.size++];
                last.from = polygon[0];
                last.to = polygon[polygonSize - 1];
            }
        }

        /// <summary>
        /// Returns the intersection of the two lines (line segments are passed in, but they are treated like infinite lines)
        /// </summary>
        /// <remarks>
        /// Got this here:
        /// http://stackoverflow.com/questions/14480124/how-do-i-detect-triangle-and-rectangle-intersection
        /// </remarks>
        private static float2? GetIntersect(in float2 line1From, in float2 line1To, in float2 line2From, in float2 line2To) {
            float2 direction1 = line1To - line1From;
            float2 direction2 = line2To - line2From;
            float dotPerp = (direction1.x * direction2.y) - (direction1.y * direction2.x);

            // 0 means the lines are parallel so have infinite intersection float2s
            if (Mathf.Abs(dotPerp) <= 0.000000001f) {
                return null;
            }

            float2 c = line2From - line1From;
            float t = (c.x * direction2.y - c.y * direction2.x) / dotPerp;
            return line1From + (t * direction1);
        }

        private static bool IsInside(in Edge edge, in float2 test) {
            float tmp1X = edge.to.x - edge.from.x;
            float tmp1Y = edge.to.y - edge.from.y;
            float tmp2X = test.x - edge.to.x;
            float tmp2Y = test.y - edge.to.y;

            //	co-linear values should be considered inside so test with <= instead of <
            return (tmp1X * tmp2Y) - (tmp1Y * tmp2X) <= 0;
        }

        private static bool IsClockwise(float2 * polygon, int polygonSize, out bool invalid) {
            for (int i = 2; i < polygonSize; i++) {
                bool? isLeft = IsLeftOf(polygon[0], polygon[1], polygon[i]);
                //	some of the points may be co-linear.  That's ok as long as the overall is a polygon
                if (isLeft != null) {
                    invalid = false;
                    return !isLeft.Value;
                }
            }

            invalid = true;
            return false;
        }

        /// <summary>
        /// Tells if the test float2 lies on the left side of the edge line
        /// </summary>
        private static bool? IsLeftOf(in float2 from, in float2 to, in float2 test) {
            float tmp1X = to.x - from.x;
            float tmp1Y = to.y - from.y;
            float tmp2X = test.x - to.x;
            float tmp2Y = test.y - to.y;

            float x = (tmp1X * tmp2Y) - (tmp1Y * tmp2X);

            if (x < 0) {
                return false;
            }

            if (x > 0) {
                return true;
            }

            return null;
        }

    }

}