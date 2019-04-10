using System;
using SVGX;
using UIForia.Util;
using UnityEngine;

namespace Packages.UIForia.Src.VectorGraphics {

    public struct State {

        public SVGXMatrix matrix;

        public static State Create() {
            return new State() {
                matrix = SVGXMatrix.identity
            };
        }

    }

    public struct VectorDrawCall {

        public VectorContext ctx;
        public DrawCallType drawCallType;
        public RangeInt shapeRange;
        public State state;

        public VectorDrawCall(DrawCallType drawCallType, VectorContext ctx, ref State state, RangeInt shapeRange) {
            this.ctx = ctx;
            this.drawCallType = drawCallType;
            this.shapeRange = shapeRange;
            this.state = state;
        }

    }

    public class VectorContext {

        private float tessTol;
        private float distTol;
        private float fringeWidth;
        private float devicePixelRatio;

        public LightList<Point> points;
        private LightList<State> states;
        internal readonly LightList<SVGXShape> shapes;
        
        private RangeInt currentShapeRange;
     //   private RangeInt currentPointRange;
        private Vector2 lastPoint;
        private GFX2 gfx;
        
        public VectorContext(GFX2 gfx) {
            this.gfx = gfx;
            points = new LightList<Point>(256);
            shapes = new LightList<SVGXShape>(16);
            states = new LightList<State>();
            shapes.Add(SVGXShape.Unset);
            states.Add(State.Create());
        }
        
        public void SetDevicePixelRatio(float ratio) {
            tessTol = 0.25f / ratio;
            distTol = 0.01f / ratio;
            fringeWidth = 1f / ratio;
            devicePixelRatio = ratio;
        }

        public void BeginPath() {
            SVGXShapeType currentShapeType = shapes.Array[shapes.Count - 1].type;
            if (currentShapeType != SVGXShapeType.Unset) {
                shapes.Add(new SVGXShape(SVGXShapeType.Unset));
                currentShapeRange = new RangeInt(shapes.Count - 1, 0);
            }
        }

        public void ClosePath() {
               
        }

        public void MoveTo(Vector2 position) {
            lastPoint = position;
            SVGXShapeType currentShapeType = shapes.Array[shapes.Count - 1].type;
            if (currentShapeType != SVGXShapeType.Unset) {
                shapes.Add(SVGXShape.Unset);
            }
        }

        public void BezierTo(Vector2 ctrl0, Vector2 ctrl1, Vector2 position) {
            SVGXMatrix matrix = states.Last.matrix;
            ctrl0 = matrix.Transform(ctrl0);
            ctrl1 = matrix.Transform(ctrl1);
            position = matrix.Transform(position);
//            int pointStart = points.Count;
//            int cnt = SVGXBezier.CubicCurve(points, lastPoint, ctrl0, ctrl1, position);
//            UpdateShape(pointStart, cnt);
        }

        public void LineTo(Vector2 position) {
            
            position = states.Last.matrix.Transform(position);
            
            SVGXShape currentShape = shapes.Last;

            if (currentShape.type == SVGXShapeType.Path) {
                currentShape.pointRange.length++;
                shapes.Last = currentShape;
            }
            else if (currentShape.type == SVGXShapeType.Unset) {
                currentShape = new SVGXShape(SVGXShapeType.Path, new RangeInt(points.Count, 2));
                currentShapeRange.length++;
                points.Add(new Point(lastPoint, PointFlags.Corner));
                shapes.Last = currentShape;
            }
            else {
                currentShape = new SVGXShape(SVGXShapeType.Path, new RangeInt(points.Count, 2));
                currentShapeRange.length++;
                points.Add(new Point(lastPoint, PointFlags.Corner));
                shapes.Add(currentShape);
            }

            lastPoint = position;
            points.Add(new Point(position, PointFlags.Corner));
            
        }
        
        private void UpdateShape(int pointStart, int pointCount) {
            SVGXShape currentShape = shapes[shapes.Count - 1];
            switch (currentShape.type) {
                case SVGXShapeType.Path:
                    currentShape.pointRange.length += pointCount;
                    shapes[shapes.Count - 1] = currentShape;
                    break;
                case SVGXShapeType.Unset:
                    currentShape = new SVGXShape(SVGXShapeType.Path, new RangeInt(pointStart, pointCount));
                    shapes[shapes.Count - 1] = currentShape;
                    currentShapeRange.length++;
                    break;
                default:
                    currentShape = new SVGXShape(SVGXShapeType.Path, new RangeInt(pointStart, pointCount));
                    shapes.Add(currentShape);
                    currentShapeRange.length++;
                    break;
            }
        }

        public void Stroke() {
            gfx.Stroke(this, states.Last, currentShapeRange);
        }      
        
        public void Fill() {
            
        }


        public void Clear() {
            shapes.QuickClear();
            points.QuickClear();
            states.QuickClear();
            lastPoint = new Vector2();
      //      currentPointRange = new RangeInt();
            currentShapeRange = new RangeInt();
            shapes.Add(SVGXShape.Unset);
            states.Add(State.Create());
        }

    }

}

//public class Path {
//
//            public LightList<Point> points;
//            public int bevelCount;
//            public bool isConvex;
//
//            public static float CurveDivs(float r, float arc, float tol) {
//                float da = Mathf.Acos(r / (r + tol)) * 2f;
//                return Mathf.Max(2, Mathf.CeilToInt(arc / da));
//            }
//            
//            public void ComputeJoins(float w, LineJoin join, float miterLimit) {
//                Point p0 = points[points.Count - 1];
//                Point p1 = points[0];
//                int leftCount = 0;
//                float iw = 0.0f;
//                if (w > 0.0f)
//                    iw = 1.0f / w;
//                
//                for (int i = 0; i < points.Count; i++) {
//                    float dlx0 = p0.deltaY;
//                    float dly0 = -p0.deltaX;
//                    float dlx1 = p1.deltaY;
//                    float dly1 = -p1.deltaX;
//
//                    p1.dmx = (dlx0 + dlx1) * 0.5f;
//                    p1.dmy = (dly0 + dly1) * 0.5f;
//
//                    float dmr2 = p1.dmx * p1.dmx + p1.dmy * p1.dmy;
//                    if (dmr2 > 0.00001f) {
//                        float scale = 1f / dmr2;
//                        if (scale > 600f) {
//                            scale = 600f;
//                        }
//
//                        p1.dmx *= scale;
//                        p1.dmy *= scale;
//                    }
//
//                    p1.flags = (p1.flags & PointFlags.Corner) != 0 ? PointFlags.Corner : 0;
//                    float cross = p1.deltaX * p0.deltaY - p0.deltaX * p1.deltaY;
//                    if (cross > 0f) {
//                        leftCount++;
//                        p1.flags |= PointFlags.Left;
//                    }
//                    
//                    float limit = Mathf.Max(1.01f, Mathf.Min(p0.length, p1.length) * iw);
//                    if (dmr2 * limit * limit < 1f) {
//                        p1.flags |= PointFlags.InnerBevel;
//                    }
//
//                    if ((p1.flags & PointFlags.Corner) != 0) {
//                        if (dmr2 * miterLimit * miterLimit < 1f || join == LineJoin.Bevel || join == LineJoin.Round) {
//                            p1.flags |= PointFlags.Bevel;
//                        }
//                    }
//
//                    if ((p1.flags & PointFlags.Bevel | PointFlags.InnerBevel) != 0) {
//                        bevelCount++;
//                    }
//                }
//
//                isConvex = leftCount == points.Count;
//            }
//            
//        }    
