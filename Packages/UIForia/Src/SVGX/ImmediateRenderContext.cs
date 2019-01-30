using System;
using Shapes2D;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    internal struct SVGXShape {

        public SVGXShapeType type;
        public RangeInt pointRange;
        public SVGXBounds bounds;

        public bool isHole;
        public bool isClosed;

        public SVGXShape(SVGXShapeType type, RangeInt pointRange) {
            this.type = type;
            this.pointRange = pointRange;
            this.isClosed = false;
            this.isHole = false;
            this.bounds = new SVGXBounds();
        }

    }

    public struct SVGXBounds {

        public Vector2 min;
        public Vector2 max;

        public bool Intersects(SVGXBounds bounds) {
            float r1x1 = min.x;
            float r1x2 = max.x;
            float r2x1 = bounds.min.x;
            float r2x2 = bounds.max.x;
            float r1y1 = min.y;
            float r1y2 = max.y;
            float r2y1 = bounds.min.y;
            float r2y2 = bounds.max.y;
            bool noOverlap = r1x1 > r2x2 ||
                             r2x1 > r1x2 ||
                             r1y1 > r2y2 ||
                             r2y1 > r1y2;
            return !noOverlap;
        }

    }

    public struct SVGXDrawState {

        public SVGXMatrix matrix;
        public SVGXStyle style;
        public Vector2 lastPoint;

    }

    public class ImmediateRenderContext {

        internal readonly LightList<Vector2> points;
        internal readonly LightList<SVGXStyle> styles;
        internal readonly LightList<SVGXMatrix> transforms;
        internal readonly LightList<SVGXDrawCall> drawCalls;
        internal readonly LightList<SVGXShape> shapes;

        private Vector2 lastPoint;
        private SVGXMatrix currentMatrix;
        private SVGXStyle currentStyle;
        private RangeInt currentShapeRange;

        public ImmediateRenderContext() {
            points = new LightList<Vector2>(128);
            styles = new LightList<SVGXStyle>();
            transforms = new LightList<SVGXMatrix>();
            currentMatrix = SVGXMatrix.identity;
            drawCalls = new LightList<SVGXDrawCall>();
            shapes = new LightList<SVGXShape>();
            shapes.Add(new SVGXShape(SVGXShapeType.Unset, default));
        }

        public void SetStrokeColor(Color color) {
            this.currentStyle.strokeStyle.color = color;
        }

        public void MoveTo(float x, float y) {
            // todo -- if last was move to, set point and return
            lastPoint = currentMatrix.Transform(new Vector2(x, y));
            SVGXShape currentShape = shapes[shapes.Count - 1];
            if (currentShape.type != SVGXShapeType.Unset) {
                shapes.Add(new SVGXShape(SVGXShapeType.Unset, default));
                currentShapeRange.length++;
            }
        }

        public void LineTo(float x, float y) {
            SVGXShape currentShape = shapes[shapes.Count - 1];

            Vector2 point = currentMatrix.Transform(new Vector2(x, y));

            switch (currentShape.type) {
                case SVGXShapeType.Path:
                    currentShape.pointRange.length++;
                    shapes[shapes.Count - 1] = currentShape;
                    break;
                case SVGXShapeType.Unset:
                    currentShape = new SVGXShape(SVGXShapeType.Path, new RangeInt(points.Count, 2));
                    shapes[shapes.Count - 1] = currentShape;
                    currentShapeRange.length++;
                    points.Add(lastPoint);
                    break;
                default:
                    currentShape = new SVGXShape(SVGXShapeType.Path, new RangeInt(points.Count, 2));
                    shapes.Add(currentShape);
                    points.Add(lastPoint);
                    currentShapeRange.length++;
                    break;
            }

            lastPoint = point;
            points.Add(point);
        }

        public void HorizontalLineTo(float x) {
            LineTo(x, lastPoint.y);
        }

        public void VerticalLineTo(float y) {
            LineTo(lastPoint.x, y);
        }

        public void ArcTo(float rx, float ry, float angle, bool isLargeArc, bool isSweepArc, float endX, float endY) {
            Vector2 end = currentMatrix.Transform(new Vector2(endX, endY));

            int pointStart = points.Count;
            int pointCount = SVGXBezier.Arc(points, lastPoint, rx, ry, angle, isLargeArc, isSweepArc, end);
            UpdateShape(pointStart, pointCount);
            lastPoint = end;
        }

        public void ClosePath() {
            SVGXShape currentShape = shapes[shapes.Count - 1];
            if (currentShape.type != SVGXShapeType.Path) {
                return;
            }

            Vector2 startPoint = points[currentShape.pointRange.start];
            LineTo(startPoint.x, startPoint.y);
            currentShape.isClosed = true;
            shapes[shapes.Count - 1] = currentShape;
            shapes.Add(new SVGXShape(SVGXShapeType.Unset, default));
            lastPoint = startPoint;
        }

        public void CubicCurveTo(Vector2 ctrl0, Vector2 ctrl1, Vector2 end) {
            ctrl0 = currentMatrix.Transform(ctrl0);
            ctrl1 = currentMatrix.Transform(ctrl1);
            end = currentMatrix.Transform(end);

            int pointStart = points.Count;
            int pointCount = SVGXBezier.CubicCurve(points, lastPoint, ctrl0, ctrl1, end);
            UpdateShape(pointStart, pointCount);
            lastPoint = end;
        }

        public void QuadraticCurveTo(Vector2 ctrl, Vector2 end) {
            ctrl = currentMatrix.Transform(ctrl);
            end = currentMatrix.Transform(end);

            int pointStart = points.Count;
            int pointCount = SVGXBezier.QuadraticCurve(points, lastPoint, ctrl, end);
            UpdateShape(pointStart, pointCount);

            lastPoint = end;
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

        public void Clear() {
            points.Clear();
            styles.Clear();
            transforms.Clear();
            drawCalls.Clear();
            shapes.Clear();
            currentStyle = new SVGXStyle();
            currentMatrix = SVGXMatrix.identity;
            lastPoint = Vector2.zero;
            shapes.Add(new SVGXShape(SVGXShapeType.Unset, default));
            currentShapeRange = new RangeInt();
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

        public void PushClip() {
        //     clips.Add(new SVGXClipGroup(currentShapeRange))
            BeginPath();
            
        }

        public void Rect(float x, float y, float width, float height) {
            SVGXShape currentShape = shapes[shapes.Count - 1];
            SVGXShapeType lastType = currentShape.type;

            Vector2 x0y0 = currentMatrix.Transform(new Vector2(x, y));
            Vector2 x1y0 = currentMatrix.Transform(new Vector2(x + width, y));
            Vector2 x1y1 = currentMatrix.Transform(new Vector2(x + width, y + height));
            Vector2 x0y1 = currentMatrix.Transform(new Vector2(x, y + height));

            currentShape = new SVGXShape(SVGXShapeType.Rect, new RangeInt(points.Count, 4));

            points.EnsureAdditionalCapacity(4);
            points.AddUnchecked(x0y0);
            points.AddUnchecked(x1y0);
            points.AddUnchecked(x1y1);
            points.AddUnchecked(x0y1);

            currentShape.isClosed = true;

            if (lastType != SVGXShapeType.Unset) {
                shapes.Add(currentShape);
            }
            else {
                shapes[shapes.Count - 1] = currentShape;
            }

            currentShapeRange.length++;
        }

        public void Ellipse(float cx, float cy, float rx, float ry) {
            const float Kappa = 0.5522847493f;
            MoveTo(cx - rx, cy);
            CubicCurveTo(new Vector2(cx - rx, cy + ry * Kappa), new Vector2(cx - rx * Kappa, cy + ry), new Vector2(cx, cy + ry));
            CubicCurveTo(new Vector2(cx + rx * Kappa, cy + ry), new Vector2(cx + rx, cy + ry * Kappa), new Vector2(cx + rx, cy));
            CubicCurveTo(new Vector2(cx + rx, cy - ry * Kappa), new Vector2(cx + rx * Kappa, cy - ry), new Vector2(cx, cy - ry));
            CubicCurveTo(new Vector2(cx - rx * Kappa, cy - ry), new Vector2(cx - rx, cy - ry * Kappa), new Vector2(cx - rx, cy));
            shapes.Array[shapes.Count - 1].isClosed = true;
        }

        public void Circle(float x, float y, float radius) {
            Ellipse(x, y, radius, radius);
        }

        public void BeginPath() {
            SVGXShape currentShape = shapes[shapes.Count - 1];
            if (currentShape.type != SVGXShapeType.Unset) {
                shapes.Add(new SVGXShape(SVGXShapeType.Unset, default));
                currentShapeRange = new RangeInt(shapes.Count - 1, 0);
            }
        }

        public void Fill() {
            SVGXDrawCall drawCall = new SVGXDrawCall(DrawCallType.StandardFill, currentStyle, currentShapeRange);
            drawCalls.Add(drawCall);
        }

        public void Stroke() {
            SVGXDrawCall drawCall = new SVGXDrawCall(DrawCallType.StandardStroke, currentStyle, currentShapeRange);
            drawCalls.Add(drawCall);
        }

    }

}