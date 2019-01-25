using System;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    internal struct SVGXDrawCall {

        public readonly DrawCallType type;
        public readonly SVGXStyle style;
        public readonly RangeInt shapeRange;
        
        public SVGXDrawCall(DrawCallType type, SVGXStyle style, RangeInt shapeRange) {
            this.type = type;
            this.style = style;
            this.shapeRange = shapeRange;
        }

    }

    internal enum SVGXShapeType {

        Rect,
        Circle,
        Ellipse,
        Path,

    }

    internal struct SVGXShape {

        public readonly int id;
        public readonly RangeInt pointRange;
        public readonly SVGXShapeType type;
        
        public bool isHole;
        public bool isClosed;

        public SVGXShape(SVGXShapeType type, RangeInt pointRange) {
            this.type = type;
            this.pointRange = pointRange;
            this.isClosed = false;
            this.isHole = false;
            this.id = 0;
        }
        
    }

    internal enum DrawCallType {

        StandardStroke,
        PatternStroke,
        PatternFill,
        StandardFill,
        Clip,
        ClipReset,

    }

    internal struct SVGXPoint {

        public float x;
        public float y;
        public SVGXPointFlag flags;

    }

    internal enum SVGXPointFlag {

        Corner = 0x1,
        Left = 0x02,
        Bevel = 0x04,
        InnerBevel = 0x08

    }
    
    public class ImmediateRenderContext {

        private readonly LightList<Vector2> points;
        private readonly LightList<SVGXStyle> styles;
        private readonly LightList<SVGXMatrix> transforms;
        private readonly LightList<SVGXDrawCall> drawCalls;
        private readonly LightList<SVGXShape> shapes;
        private readonly LightList<Vector2> shapePointBuffer;
        
        private Vector2 lastPoint;

        private SVGXMatrix currentMatrix;
        private SVGXStyle currentStyle;
        private RangeInt currentShapeRange;
        private RangeInt currentPathPointRange;
        
        private int currentShapeRangeStart;
        
        public ImmediateRenderContext() {
            points = new LightList<Vector2>(128);
            styles = new LightList<SVGXStyle>();
            transforms = new LightList<SVGXMatrix>();
            currentMatrix = SVGXMatrix.identity;
            drawCalls = new LightList<SVGXDrawCall>();
            shapes = new LightList<SVGXShape>();
        }

        public void Save() {
            transforms.Add(currentMatrix);
            styles.Add(currentStyle);
        }

        public void Restore() {
            if (transforms.Count > 0) {
                currentMatrix = transforms.RemoveLast();
            }

            if (styles.Count > 0) {
                currentStyle = styles.RemoveLast();
            }
        }

        public void EndPath() {
            // it current path has non 0 point range -> add to shapes
            currentPathPointRange = new RangeInt();
        }
        
        public void Rect(float x, float y, float width, float height) {
            EndPath();
            
            Vector2 x0y0 = currentMatrix.Transform(new Vector2(x, y));
            Vector2 x1y0 = currentMatrix.Transform(new Vector2(x + width, y));
            Vector2 x1y1 = currentMatrix.Transform(new Vector2(x + width, y + height));
            Vector2 x0y1 = currentMatrix.Transform(new Vector2(x, y + height));
            
            SVGXShape rectShape = new SVGXShape(SVGXShapeType.Rect, new RangeInt(points.Count, 4));
            
            points.EnsureAdditionalCapacity(4);
            points.AddUnchecked(x0y0);
            points.AddUnchecked(x1y0);
            points.AddUnchecked(x1y1);
            points.AddUnchecked(x0y1);
            
            shapes.Add(rectShape);
            currentShapeRange.length++;
            
        }
        
        public void BeginPath() {
            
            currentShapeRange = new RangeInt(shapes.Count, 0);
        }

        public void ClosePath() {
            
        }
        
        public void MoveTo(float x, float y) {
            Vector2 point = currentMatrix.Transform(new Vector2(x, y));
            lastPoint = point;

        }

        public void LineTo(float x, float y) {
            
            // paths are combined until a non path command interrupts them
            
            if (currentShapeRange.start == -1) {
                    
            }
            
            Vector2 point = currentMatrix.Transform(new Vector2(x, y));
            points.Add(point);
            lastPoint = point;
        }

        public void Stroke() {
            SVGXDrawCall call = new SVGXDrawCall(DrawCallType.StandardStroke, currentStyle, currentShapeRange);
            drawCalls.Add(call);
        }

        public void Render(Camera camera) {
            // todo -- merge draw calls
            // can use clip calls as merge bounds
            // each call can get a z index if we aren't using transparency this is awesome
            for (int i = 0; i < drawCalls.Count; i++) {
                SVGXDrawCall drawCall = drawCalls[i];
                switch (drawCall.type) {
                    case DrawCallType.StandardStroke:
                        DrawStroke(camera, drawCall);
                        break;
                }
            }
        }

        private Mesh mesh;
        public Material strokeMaterial;
        
        private void DrawStroke(Camera camera, SVGXDrawCall drawCall) {
            
            mesh = mesh ? mesh : new Mesh();
            
            int start = drawCall.shapeRange.start;
            int end = drawCall.shapeRange.end;
            StrokeVertexData strokeVertexData = new StrokeVertexData();
            
            // todo -- write to z-index
            for (int i = start; i < end; i++) {
                CreateSolidStrokeVertices(strokeVertexData, drawCall.style.strokeStyle, points, shapes[i].pointRange);
            }
            
            // todo -- this should only issue a draw when the shader would change 
            strokeVertexData.FillMesh(mesh); // todo -- needs to handle breaking into multiple meshes if count > ushort.max
            Graphics.DrawMesh(mesh, Matrix4x4.identity, strokeMaterial, 0, camera, 0, null, false, false, false);

        }

        /// <summary>
        /// Creates vertex data for a solid stroked line
        /// </summary>
        /// <param name="vertexData"></param>
        /// <param name="style"></param>
        /// <param name="points"></param>
        /// <param name="range"></param>
        private static void CreateSolidStrokeVertices(StrokeVertexData vertexData, SVGXStrokeStyle style, LightList<Vector2> points, RangeInt range) {
            Color32 color = style.color;
            
            Vector2 dir = (points[range.start + 1] - points[range.start]).normalized;
            Vector2 prev = points[range.start] - dir;
            Vector2 curr = points[range.start];
            Vector2 next = points[range.start + 1];
            Vector2 far = points.Count == 2 
                ? points[range.start + 1] + (points[range.start + 1] - points[range.start]).normalized 
                : points[range.start + 2];

            // 4 vertices per segment, caps and joins done in the shader
            // 0 --- 1|4 --- 5|08 ---- 09
            // |      |       |        |
            // 2 --- 3|6 --- 7|10 ---- 11
            
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
            flags[flagCnt++] = new Vector4(-1, 1, 0, 0);
            flags[flagCnt++] = new Vector4(1, -1, 0, 0);
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
                prev = points[i - 1];
                curr = points[i];
                next = points[i + 1];
                far = points[i + 2];

                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;
                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;
            
                flags[flagCnt++] = new Vector4(1, -1, 0, 0);
                flags[flagCnt++] = new Vector4(-1, 1, 0, 0);
                flags[flagCnt++] = new Vector4(1, -1, 0, 0);
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
                int currIdx = range.length - 2;
                prev = points[currIdx - 1];
                curr = points[currIdx];
                next = points[currIdx + 1];
                far = next + (next - curr);

                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;
                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;
            
                flags[flagCnt++] = new Vector4(1, -1, 0, 0);
                flags[flagCnt++] = new Vector4(-1, 1, 0, 0);
                flags[flagCnt++] = new Vector4(1, -1, 0, 0);
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

            vertexData.triangleIndex = triIdx;
        }
       
    }

}