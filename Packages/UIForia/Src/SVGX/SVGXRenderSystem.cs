using System;
using UIForia.Util;
using Unity.Jobs;
using UnityEngine;

namespace SVGX {

    public class SVGXRenderSystem {

        private readonly Material strokeMaterial;
        private readonly Material fillMaterial;
        private readonly Material clipMaterial;
        private readonly Material clipClearMaterial;
        private readonly Material stencilFillMaterial;
        private readonly Material stencilCutoutMaterial;

        private readonly ObjectPool<StrokeVertexData> strokePool;
        private readonly ObjectPool<FillVertexData> fillPool;
        private readonly LightList<StrokeVertexData> strokesToRecycle;
        private readonly LightList<FillVertexData> fillsToRecycle;

        public SVGXRenderSystem(Material strokeMaterial, Material fillMaterial, Material stencilFillMaterial, Material stencilCutoutMaterial, Material clipMaterial) {
            this.strokeMaterial = strokeMaterial;
            this.strokePool = new ObjectPool<StrokeVertexData>(null, (a) => a.Clear());
            this.fillPool = new ObjectPool<FillVertexData>(null, (a) => a.Clear());
            this.strokesToRecycle = new LightList<StrokeVertexData>();
            this.fillsToRecycle = new LightList<FillVertexData>();
            this.fillMaterial = fillMaterial;
            this.stencilFillMaterial = stencilFillMaterial;
            this.stencilCutoutMaterial = stencilCutoutMaterial;
            this.clipMaterial = clipMaterial;
            this.clipClearMaterial = null;
        }

        public void Render(Camera camera, ImmediateRenderContext ctx) {
            for (int i = 0; i < strokesToRecycle.Count; i++) {
                strokePool.Release(strokesToRecycle[i]);
            }

            for (int i = 0; i < fillsToRecycle.Count; i++) {
                fillPool.Release(fillsToRecycle[i]);
            }

            fillsToRecycle.Clear();
            strokesToRecycle.Clear();

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

                    case DrawCallType.StandardFill:
                        DrawFill(ctx, camera, drawCall);
                        break;
                }
            }
        }

        private static bool IsPolygonSelfIntersecting(LightList<Vector2> points, SVGXShape shape) {
            return true;
        }

        private void DrawFill(ImmediateRenderContext ctx, Camera camera, SVGXDrawCall drawCall) {
//            FillVertexData fillVertexData = fillPool.Get();
//
//            int start = drawCall.shapeRange.start;
//            int end = drawCall.shapeRange.end;
//            LightList<SVGXShape> withHoles = LightListPool<SVGXShape>.Get();
//            Vector3 origin = camera.transform.position + new Vector3(0, 0, 2);
//            
//            if (drawCall.shapeRange.length == 1) {
//                CreateFillVertices(fillVertexData, ctx.points, drawCall.style, ctx.shapes[drawCall.shapeRange.start]);
//                if (ctx.shapes[drawCall.shapeRange.start].type != SVGXShapeType.Path) {
//                    Matrix4x4 matrix = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
//                    Graphics.DrawMesh(fillVertexData.FillMesh(), matrix, fillMaterial, 0, camera, 0, null, false, false, false);
//                }
//                else {
//                    Mesh fillMesh = fillVertexData.FillMesh();
//                    Matrix4x4 matrix = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
//                    Graphics.DrawMesh(fillMesh, matrix, stencilCutoutMaterial, 0, camera, 0, null, false, false, false);
//                    Graphics.DrawMesh(fillMesh, matrix, stencilFillMaterial, 0, camera, 0, null, false, false, false);
////                    Graphics.DrawMesh(fillMesh, matrix, stencilResetMaterial, 0, camera, 0, null, false, false, false);
//                }
//            }
//
//            fillsToRecycle.Add(fillVertexData);

//            for (int i = start; i < end; i++) {
//                if (ctx.shapes[i].isHole) {
//                    withHoles.Add(ctx.shapes[i]);
//                }
//                else {
//                    CreateFillVertices(fillVertexData, ctx.points, drawCall.style, ctx.shapes[i]);
//                }
//            }
//
//
//            // if path contains only lines and # of lines < 25 -> see if any two lines intersect use stencil if they do
//            // if path contains curves -> stencil automatically
//
//            // for each item to be stenciled
//            // if that item intersects any other thing to be stencilled
//            // put it in its own call
//
//            // best way to do this is probably globally, between all fill calls
//
//            // while list has more items
//            // take first item
//            // for every other item still in the list
//            // if item does not overlap / intersect bounds of any item in current batch
//            //     add that item to the batch
//
//            LightList<LightList<SVGXShape>> batches = new LightList<LightList<SVGXShape>>();
//            LightList<SVGXShape> currentBatch = new LightList<SVGXShape>();
//
//            while (withHoles.Count > 1) {
//                int cnt = withHoles.Count;
//                for (int i = 1; i < withHoles.Count; i++) {
//                    SVGXShape currentShape = withHoles[i];
//                    SVGXBounds currentBounds = currentShape.bounds;
//
//                    bool shouldAdd = true;
//                    for (int j = 0; j < currentBatch.Count; j++) {
//                        if (currentBounds.Intersects(currentBatch[i].bounds)) {
//                            shouldAdd = false;
//                            break;
//                        }
//                    }
//
//                    if (shouldAdd) {
//                        currentBatch.Add(currentShape);
//                        withHoles.RemoveAt(i--);
//                    }
//                }
//
//                if (currentBatch.Count > 0 && cnt == withHoles.Count) {
//                    batches.Add(currentBatch);
//                }
//            }
//
//            LightListPool<SVGXShape>.Release(ref withHoles);
        }

        internal static void CreateShapeStrokeVertices(StrokeVertexData vertexData, LightList<Vector2> points, SVGXMatrix matrix, SVGXStyle style, SVGXShape shape) {
            switch (shape.type) {
                case SVGXShapeType.Rect:
                    //     CreateRectStrokeVertices();
                    break;
            }
        }

        internal static void CreateRectStrokeVertices(StrokeVertexData vertexData, LightList<Vector2> points, SVGXMatrix matrix, SVGXStyle style, SVGXShape shape) {
            int triIdx = vertexData.triangleIndex;
            int vertexCnt = vertexData.position.Count;
            int flagCnt = vertexData.flags.Count;
            int prevNextCnt = vertexData.prevNext.Count;
            int colorCnt = vertexData.colors.Count;
            int texCoordCnt = vertexData.texCoords.Count;
            int triangleCnt = vertexData.triangles.Count;

            Vector3[] vertices = vertexData.position.Array;
            Vector4[] prevNext = vertexData.prevNext.Array;
            Vector4[] flags = vertexData.flags.Array;
            Vector2[] texCoords = vertexData.texCoords.Array;
            Color[] colors = vertexData.colors.Array;
            int[] triangles = vertexData.triangles.Array;

            switch (style.lineJoin) {
                case LineJoin.Bevel:


                    break;
            }
        }

        internal static void CreateFillVertices(FillVertexData vertexData, LightList<Vector2> points, SVGXMatrix matrix, SVGXStyle style, SVGXShape shape) {
            int triIdx = vertexData.triangleIndex;
            int vertexCnt = vertexData.position.Count;
            int colorCnt = vertexData.colors.Count;
            int texCoordCnt = vertexData.texCoords.Count;
            int flagsCnt = vertexData.flags.Count;
            int triangleCnt = vertexData.triangles.Count;

            Vector3[] vertices = vertexData.position.Array;
            Vector2[] texCoords = vertexData.texCoords.Array;
            Vector4[] flags = vertexData.flags.Array;
            Color[] colors = vertexData.colors.Array;
            int[] triangles = vertexData.triangles.Array;

            int start = shape.pointRange.start;
            Color color = style.fillColor;

            // Matrix 3x3 fillTransform -> matrix defining rotation / offset / scale for fill 
            // would need a way to index into a constant buffer probably, or pass even more vertex data
            // float rotation, vec2 scale, vec2, position -> pack into 2 floats?

            // probably want to pre-process the points and apply the matrix transform to them in a job
            // that way we can do each point only once even if they get re-used

            switch (shape.type) {
                case SVGXShapeType.Unset:
                    break;
                case SVGXShapeType.Ellipse:
                case SVGXShapeType.Circle:
                case SVGXShapeType.Rect:

                    int fillMode = (int) style.fillMode;
//                    int gradientId = 
                    if (style.fillMode == FillMode.Gradient) {
                            
                    }
                    
                    vertices[vertexCnt++] = matrix.Transform(points[start++]);
                    vertices[vertexCnt++] = matrix.Transform(points[start++]);
                    vertices[vertexCnt++] = matrix.Transform(points[start++]);
                    vertices[vertexCnt++] = matrix.Transform(points[start]);

                    triangles[triangleCnt++] = triIdx + 0;
                    triangles[triangleCnt++] = triIdx + 1;
                    triangles[triangleCnt++] = triIdx + 2;
                    triangles[triangleCnt++] = triIdx + 2;
                    triangles[triangleCnt++] = triIdx + 3;
                    triangles[triangleCnt++] = triIdx + 0;

                    texCoords[texCoordCnt++] = new Vector2(0, 1);
                    texCoords[texCoordCnt++] = new Vector2(1, 1);
                    texCoords[texCoordCnt++] = new Vector2(1, 0);
                    texCoords[texCoordCnt++] = new Vector2(0, 0);

                    flags[flagsCnt++] = new Vector4((int) shape.type, 0, 0, 0);
                    flags[flagsCnt++] = new Vector4((int) shape.type, 0, 0, 0);
                    flags[flagsCnt++] = new Vector4((int) shape.type, 0, 0, 0);
                    flags[flagsCnt++] = new Vector4((int) shape.type, 0, 0, 0);

                    colors[colorCnt++] = color;
                    colors[colorCnt++] = color;
                    colors[colorCnt++] = color;
                    colors[colorCnt++] = color;

                    vertexData.position.Count = vertexCnt;
                    vertexData.triangles.Count = triangleCnt;
                    vertexData.colors.Count = colorCnt;
                    vertexData.texCoords.Count = texCoordCnt;
                    vertexData.flags.Count = flagsCnt;

                    vertexData.triangleIndex = triIdx + 4;

                    break;

                case SVGXShapeType.Path:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawStroke(ImmediateRenderContext ctx, Camera camera, SVGXDrawCall drawCall) {
//            int start = drawCall.shapeRange.start;
//            int end = drawCall.shapeRange.end;
//            StrokeVertexData strokeVertexData = strokePool.Get();
//
//            // todo -- write to z-index
//            for (int i = start; i < end; i++) {
//                CreateSolidStrokeVertices(strokeVertexData, drawCall.style.strokeStyle, ctx.points, ctx.shapes[i].isClosed, ctx.shapes[i].pointRange);
//            }
//
//            Vector3 origin = camera.transform.position + new Vector3(0, 0, 2);
//            Matrix4x4 mat = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
//            // todo -- this should only issue a draw when the shader would change 
//            ; // todo -- needs to handle breaking into multiple meshes if count > ushort.max
//            Graphics.DrawMesh(strokeVertexData.FillMesh(), mat, strokeMaterial, 0, camera, 0, null, false, false, false);
//            strokesToRecycle.Add(strokeVertexData);
        }

        internal static void CreateStrokeVertices(StrokeVertexData vertexData, LightList<Vector2> points, SVGXMatrix matrix, SVGXStyle style, SVGXShape shape) {
            RangeInt range = shape.pointRange;
            bool isClosed = shape.isClosed;

            // todo -- interesting optimization for later: when stroking rects / ellipses / circles, see if it is faster to generate less geometry but use a discard to clip out the middle

            while (range.length > 1 && points[range.start] == points[range.end - 1]) {
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
            
            vertexData.EnsureCapacity(points.Count * 4);

            int triIdx = vertexData.triangleIndex;
            int vertexCnt = vertexData.position.Count;
            int flagCnt = vertexData.flags.Count;
            int prevNextCnt = vertexData.prevNext.Count;
            int colorCnt = vertexData.colors.Count;
            int texCoordCnt = vertexData.texCoords.Count;
            int triangleCnt = vertexData.triangles.Count;

            Vector3[] vertices = vertexData.position.Array;
            Vector4[] prevNext = vertexData.prevNext.Array;
            Vector4[] flags = vertexData.flags.Array;
            Vector2[] texCoords = vertexData.texCoords.Array;
            Color[] colors = vertexData.colors.Array;
            int[] triangles = vertexData.triangles.Array;

            
            
        }

        internal static void CreateSolidStrokeVertices(StrokeVertexData vertexData, LightList<Vector2> points, SVGXMatrix matrix, SVGXStyle style, SVGXShape shape) {
            Color32 color = Color.red; //style.color;
            float strokeWidth = 5f; //style.width;
            float miterLimit = style.miterLimit;

            RangeInt range = shape.pointRange;
            bool isClosed = shape.isClosed;

            // todo -- interesting optimization for later: when stroking rects / ellipses / circles, see if it is faster to generate less geometry but use a discard to clip out the middle

            while (range.length > 1 && points[range.start] == points[range.end - 1]) {
                range.length--;
            }

            if (range.length < 2) {
                return;
            }

            const int top = 1;
            const int btm = -1;

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
            int texCoordCnt = vertexData.texCoords.Count;
            int triangleCnt = vertexData.triangles.Count;

            Vector3[] vertices = vertexData.position.Array;
            Vector4[] prevNext = vertexData.prevNext.Array;
            Vector4[] flags = vertexData.flags.Array;
            Vector2[] texCoords = vertexData.texCoords.Array;
            Color[] colors = vertexData.colors.Array;
            int[] triangles = vertexData.triangles.Array;

            vertices[vertexCnt++] = curr;
            vertices[vertexCnt++] = next;
            vertices[vertexCnt++] = curr;
            vertices[vertexCnt++] = next;

            // color, opacity, texcoords, texture id
            // join type, cap type, miter limit, 
            // stroke width,

            // todo -- encode segment length, probably done in shader (or maybe just do the sqrt in the shader)
            // todo -- encode cap type / join type
            // todo -- if miter or round join / cap embed new geometry to handle that

//            flags[flagCnt++] = new Vector4(top, 0, miterLimit, strokeWidth);
//            flags[flagCnt++] = new Vector4(top, 1, miterLimit, strokeWidth);
//            flags[flagCnt++] = new Vector4(btm, 2, miterLimit, strokeWidth);
//            flags[flagCnt++] = new Vector4(btm, 3, miterLimit, strokeWidth);

            int cap = 1;
            int join = 2;
            
            flags[flagCnt++] = new Vector4(top, 0, cap, strokeWidth);
            flags[flagCnt++] = new Vector4(top, 1, join, strokeWidth);
            flags[flagCnt++] = new Vector4(btm, 2, cap, strokeWidth);
            flags[flagCnt++] = new Vector4(btm, 3, join, strokeWidth);
            
            prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
            prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
            prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
            prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

            texCoords[texCoordCnt++] = new Vector2(0, 1);
            texCoords[texCoordCnt++] = new Vector2(1, 1);
            texCoords[texCoordCnt++] = new Vector2(1, 0);
            texCoords[texCoordCnt++] = new Vector2(0, 0);

            colors[colorCnt++] = Color.blue;//color;
            colors[colorCnt++] = Color.blue;//color;
            colors[colorCnt++] = Color.blue;//color;
            colors[colorCnt++] = Color.blue;//color;

            triangles[triangleCnt++] = triIdx + 0;
            triangles[triangleCnt++] = triIdx + 1;
            triangles[triangleCnt++] = triIdx + 2;
            triangles[triangleCnt++] = triIdx + 2;
            triangles[triangleCnt++] = triIdx + 1;
            triangles[triangleCnt++] = triIdx + 3;

            triIdx += 4;

            for (int i = range.start + 1; i < range.end - 2; i++) {
                prev = points[i - 1];
                curr = points[i];
                next = points[i + 1];
                far = points[i + 2];

                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;
                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;

                flags[flagCnt++] = new Vector4(top, 0, miterLimit, strokeWidth);
                flags[flagCnt++] = new Vector4(top, 1, miterLimit, strokeWidth);
                flags[flagCnt++] = new Vector4(btm, 2, miterLimit, strokeWidth);
                flags[flagCnt++] = new Vector4(btm, 3, miterLimit, strokeWidth);

                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

                texCoords[texCoordCnt++] = new Vector2(0, 1);
                texCoords[texCoordCnt++] = new Vector2(1, 1);
                texCoords[texCoordCnt++] = new Vector2(1, 0);
                texCoords[texCoordCnt++] = new Vector2(0, 0);

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
                far = isClosed ? points[range.start] : next + (next - curr);

                // start TEMP BEVEL / ROUND GEOMETRY
//                vertices[vertexCnt++] = curr;
//                vertices[vertexCnt++] = curr;
//                vertices[vertexCnt++] = curr;
//
//                flags[flagCnt++] = new Vector4(top, 4, join, strokeWidth);
//                flags[flagCnt++] = new Vector4(top, 5, join, strokeWidth);
//                flags[flagCnt++] = new Vector4(btm, 6, join, strokeWidth);
//                
//                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
//                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
//                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
//
//                texCoords[texCoordCnt++] = new Vector2(0, 1);
//                texCoords[texCoordCnt++] = new Vector2(1, 1);
//                texCoords[texCoordCnt++] = new Vector2(1, 0);
//
//                colors[colorCnt++] = Color.yellow;
//                colors[colorCnt++] = Color.yellow;
//                colors[colorCnt++] = Color.yellow;
//
//                triangles[triangleCnt++] = triIdx + 0;
//                triangles[triangleCnt++] = triIdx + 1;
//                triangles[triangleCnt++] = triIdx + 2;
//
//                triIdx += 3;
//                
//                // START TEMP ROUND JOIN GEOMETRY
//                vertices[vertexCnt++] = curr;
//                vertices[vertexCnt++] = curr;
//                vertices[vertexCnt++] = curr;
//
//                flags[flagCnt++] = new Vector4(top, 7, join, strokeWidth);
//                flags[flagCnt++] = new Vector4(top, 8, join, strokeWidth);
//                flags[flagCnt++] = new Vector4(btm, 9, join, strokeWidth);
//                
//                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
//                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
//                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
//
//                texCoords[texCoordCnt++] = new Vector2(0, 1);
//                texCoords[texCoordCnt++] = new Vector2(1, 1);
//                texCoords[texCoordCnt++] = new Vector2(1, 0);
//
//                colors[colorCnt++] = Color.magenta;
//                colors[colorCnt++] = Color.magenta;
//                colors[colorCnt++] = Color.magenta;
//
//                triangles[triangleCnt++] = triIdx + 0;
//                triangles[triangleCnt++] = triIdx + 1;
//                triangles[triangleCnt++] = triIdx + 2;
//
//                triIdx += 3;
//                // END TEMP ROUND JOIN GEOMETRY

                // end TEMP join segment
                
                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;
                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;

//                flags[flagCnt++] = new Vector4(top, 0, miterLimit, strokeWidth);
//                flags[flagCnt++] = new Vector4(top, 1, miterLimit, strokeWidth);
//                flags[flagCnt++] = new Vector4(btm, 2, miterLimit, strokeWidth);
//                flags[flagCnt++] = new Vector4(btm, 3, miterLimit, strokeWidth);

                flags[flagCnt++] = new Vector4(top, 0, join, strokeWidth);
                flags[flagCnt++] = new Vector4(top, 1, cap, strokeWidth);
                flags[flagCnt++] = new Vector4(btm, 2, join, strokeWidth);
                flags[flagCnt++] = new Vector4(btm, 3, cap, strokeWidth);
                
                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

                texCoords[texCoordCnt++] = new Vector2(0, 1);
                texCoords[texCoordCnt++] = new Vector2(1, 1);
                texCoords[texCoordCnt++] = new Vector2(1, 0);
                texCoords[texCoordCnt++] = new Vector2(0, 0);

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

                    flags[flagCnt++] = new Vector4(top, 0, miterLimit, strokeWidth);
                    flags[flagCnt++] = new Vector4(top, 1, miterLimit, strokeWidth);
                    flags[flagCnt++] = new Vector4(btm, 2, miterLimit, strokeWidth);
                    flags[flagCnt++] = new Vector4(btm, 3, miterLimit, strokeWidth);

                    prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                    prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
                    prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                    prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

                    texCoords[texCoordCnt++] = new Vector2(0, 1);
                    texCoords[texCoordCnt++] = new Vector2(1, 1);
                    texCoords[texCoordCnt++] = new Vector2(1, 0);
                    texCoords[texCoordCnt++] = new Vector2(0, 0);

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
            vertexData.texCoords.Count = texCoordCnt;

            vertexData.triangleIndex = triIdx;
        }

    }

}