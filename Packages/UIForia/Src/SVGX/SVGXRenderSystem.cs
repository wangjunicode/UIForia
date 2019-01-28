using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public class SVGXRenderSystem {

        private Mesh mesh;
        private readonly Material strokeMaterial;

        public SVGXRenderSystem(Material strokeMaterial) {
            this.strokeMaterial = strokeMaterial;
        }

        public void Render(Camera camera, ImmediateRenderContext ctx) {
            // todo -- merge draw calls
            // can use clip calls as merge bounds
            // each call can get a z index if we aren't using transparency this is awesome
            LightList<SVGXDrawCall> drawCalls = ctx.drawCalls;
            for (int i = 0; i < drawCalls.Count; i++) {
                SVGXDrawCall drawCall = drawCalls[i];
                switch (drawCall.type) {
                    case DrawCallType.StandardStroke:
                        DrawStroke(ctx, camera, drawCall);
                        break;
                }
            }
        }

        private void DrawStroke(ImmediateRenderContext ctx, Camera camera, SVGXDrawCall drawCall) {
            mesh = mesh ? mesh : new Mesh();
            mesh.MarkDynamic();
            mesh.Clear();
            int start = drawCall.shapeRange.start;
            int end = drawCall.shapeRange.end;
            StrokeVertexData strokeVertexData = new StrokeVertexData();

            // todo -- write to z-index
            for (int i = start; i < end; i++) {
                CreateSolidStrokeVertices(strokeVertexData, drawCall.style.strokeStyle, ctx.points, ctx.shapes[i].isClosed, ctx.shapes[i].pointRange);
            }

            Vector3 origin = camera.transform.position + new Vector3(0, 0, 2);
            Matrix4x4 mat = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
            // todo -- this should only issue a draw when the shader would change 
            strokeVertexData.FillMesh(mesh); // todo -- needs to handle breaking into multiple meshes if count > ushort.max
            Graphics.DrawMesh(mesh, mat, strokeMaterial, 0, camera, 0, null, false, false, false);
        }

        /// <summary>
        /// Creates vertex data for a solid stroked line
        /// </summary>
        /// <param name="vertexData"></param>
        /// <param name="style"></param>
        /// <param name="points"></param>
        /// <param name="range"></param>
        private static void CreateSolidStrokeVertices(StrokeVertexData vertexData, SVGXStrokeStyle style, LightList<Vector2> points, bool isClosed, RangeInt range) {
            Color32 color = Color.red;//style.color;

            // needs to be a while loop?
            while(range.length > 1 && points[range.start] == points[range.end - 1]) {
                range.length--;
            }

            if (range.length < 2) {
                return;
            }
            
            Vector2 dir = (points[range.start + 1] - points[range.start]).normalized;
            Vector2 prev = isClosed ? points[range.end - 1] : points[range.start] - dir;
            Vector2 curr = points[range.start];
            Vector2 next = points[range.start + 1];
            Vector2 far = points.Count == 2
                ? points[range.start + 1] + (points[range.start + 1] - points[range.start]).normalized
                : points[range.start + 2];

            // 4 vertices per segment, caps and joins done in the shader
            // 0 --- 1|4 --- 5|08 ---- 09
            // |      |       |        |
            // 2 --- 3|6 --- 7|10 ---- 11

            // todo -- if path is closed be sure to join end to start
            
            vertexData.EnsureCapacity(points.Count * 4);

            int triIdx = vertexData.triangleIndex;
            int vertexCnt = vertexData.position.Count;
            int flagCnt = vertexData.flags.Count;
            int prevNextCnt = vertexData.prevNext.Count;
            int colorCnt = vertexData.colors.Count;
            int triangleCnt = vertexData.triangles.Count;

            Vector3[] vertices = vertexData.position.Array;
            Vector4[] prevNext = vertexData.prevNext.Array;
            Vector4[] flags = vertexData.flags.Array;
            Color[] colors = vertexData.colors.Array;
            int[] triangles = vertexData.triangles.Array;

            vertices[vertexCnt++] = curr;
            vertices[vertexCnt++] = next;
            vertices[vertexCnt++] = curr;
            vertices[vertexCnt++] = next;

            // todo -- encode 0 - 3 into flags to denote which corner this belongs to
            // todo -- encode segment length, probably done in shader (or maybe just do the sqrt in the shader)
            // todo -- encode cap type / join type
            // todo -- if miter or round join / cap embed new geometry to handle that

            flags[flagCnt++] = new Vector4(1, -1, 0, 0);
            flags[flagCnt++] = new Vector4(1, 1, 0, 0);
            flags[flagCnt++] = new Vector4(-1, -1, 0, 0);
            flags[flagCnt++] = new Vector4(-1, 0, 0, 0);         

            prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
            prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
            prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
            prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

            colors[colorCnt++] = color;
            colors[colorCnt++] = color;
            colors[colorCnt++] = color;
            colors[colorCnt++] = color;

            triangles[triangleCnt++] = triIdx + 0;
            triangles[triangleCnt++] = triIdx + 1;
            triangles[triangleCnt++] = triIdx + 2;
            triangles[triangleCnt++] = triIdx + 2;
            triangles[triangleCnt++] = triIdx + 1;
            triangles[triangleCnt++] = triIdx + 3;

            triIdx += 4;

            for (int i = range.start + 1; i < range.end - 2; i++) {
                prev =  points[i - 1];
                curr = points[i];
                next = points[i + 1];
                far = points[i + 2];

                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;
                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;

                flags[flagCnt++] = new Vector4(1, -1, 0, 0);
                flags[flagCnt++] = new Vector4(1, 1, 0, 0);
                flags[flagCnt++] = new Vector4(-1, -1, 0, 0);
                flags[flagCnt++] = new Vector4(-1, 0, 0, 0);

                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

                colors[colorCnt++] = color;
                colors[colorCnt++] = color;
                colors[colorCnt++] = color;
                colors[colorCnt++] = color;

                triangles[triangleCnt++] = triIdx + 0;
                triangles[triangleCnt++] = triIdx + 1;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 1;
                triangles[triangleCnt++] = triIdx + 3;

                triIdx += 4;
            }

            if (points.Count > 2) {
                int currIdx = range.end - 2;
                prev = points[currIdx - 1];
                curr = points[currIdx];
                next = points[currIdx + 1];
                far =  isClosed ? points[range.start] : next + (next - curr);
                        
                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;
                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;

                flags[flagCnt++] = new Vector4(1, -1, 0, 0);
                flags[flagCnt++] = new Vector4(1, 1, 0, 0);
                flags[flagCnt++] = new Vector4(-1, -1, 0, 0);
                flags[flagCnt++] = new Vector4(-1, 0, 0, 0);

                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

                colors[colorCnt++] = color;
                colors[colorCnt++] = color;
                colors[colorCnt++] = color;
                colors[colorCnt++] = color;

                triangles[triangleCnt++] = triIdx + 0;
                triangles[triangleCnt++] = triIdx + 1;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 1;
                triangles[triangleCnt++] = triIdx + 3;

                triIdx += 4;

                if (isClosed) {
                    currIdx = range.end - 1;
                    prev = points[currIdx - 1];
                    curr = points[currIdx];
                    next = points[0];
                    far = points[1];
                        
                    vertices[vertexCnt++] = curr;
                    vertices[vertexCnt++] = next;
                    vertices[vertexCnt++] = curr;
                    vertices[vertexCnt++] = next;

                    flags[flagCnt++] = new Vector4(1, -1, 0, 0);
                    flags[flagCnt++] = new Vector4(1, 1, 0, 0);
                    flags[flagCnt++] = new Vector4(-1, -1, 0, 0);
                    flags[flagCnt++] = new Vector4(-1, 0, 0, 0);

                    prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                    prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
                    prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                    prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

                    colors[colorCnt++] = color;
                    colors[colorCnt++] = color;
                    colors[colorCnt++] = color;
                    colors[colorCnt++] = color;

                    triangles[triangleCnt++] = triIdx + 0;
                    triangles[triangleCnt++] = triIdx + 1;
                    triangles[triangleCnt++] = triIdx + 2;
                    triangles[triangleCnt++] = triIdx + 2;
                    triangles[triangleCnt++] = triIdx + 1;
                    triangles[triangleCnt++] = triIdx + 3;

                    triIdx += 4;
                }

            }

            vertexData.position.Count = vertexCnt;
            vertexData.triangles.Count = triangleCnt;
            vertexData.colors.Count = colorCnt;
            vertexData.flags.Count = flagCnt;
            vertexData.prevNext.Count = prevNextCnt;
            
            vertexData.triangleIndex = triIdx;
        }

       
    }
    

}