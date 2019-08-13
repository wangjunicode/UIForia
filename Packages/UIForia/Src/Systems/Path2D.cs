using System;
using SVGX;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;
using Vertigo;
using LineJoin = Vertigo.LineJoin;
using ShapeType = Vertigo.ShapeType;

namespace Src.Systems {

    public class Path2D : ShapeGenerator {

        private int version;

        internal readonly UIForiaGeometry geometry;
        internal readonly StructList<SVGXMatrix> transforms; // can remove and check for null and use identity
        internal readonly StructList<SVGXDrawCall2> drawCallList;
        internal StructList<SVGXFillStyle> fillStyles;
        internal StructList<SVGXStrokeStyle> strokeStyles;
        internal readonly StructList<Vector4> objectData;
        internal readonly StructList<Vector4> colorData;

        private Rect currentScissorRect;
        private SVGXGradient currentFillGradient;
        private Texture2D currentFillTexture;
        private Texture2D currentStrokeTexture;
        private SVGXGradient currentStrokeGradient;
        private SVGXMatrix currentMatrix;
        private SVGXStyle currentStyle;
        private SVGXFillStyle currentFillStyle;
        private SVGXStrokeStyle currentStrokeStyle;
        public bool matrixChanged;

        private static readonly float s_CircleRadii = VertigoUtil.BytesToFloat(250, 250, 250, 250);

        public Path2D() {
            this.drawCallList = new StructList<SVGXDrawCall2>();
            this.fillStyles = null;
            this.strokeStyles = null;
            this.transforms = new StructList<SVGXMatrix>();
            this.objectData = new StructList<Vector4>();
            this.colorData = new StructList<Vector4>();
            this.geometry = new UIForiaGeometry();
            this.currentMatrix = SVGXMatrix.identity;
            this.currentFillStyle = SVGXFillStyle.Default;
            this.currentStrokeStyle = SVGXStrokeStyle.Default;
            transforms.Add(currentMatrix);
        }

        public new void Clear() {
            base.Clear();
            version++;
            matrixChanged = false;
            drawCallList.QuickClear();
            fillStyles?.QuickClear();
            strokeStyles?.QuickClear();
            transforms.size = 0;
            objectData.size = 0;
            colorData.size = 0;
            geometry.Clear();
            pointList.size = 0;
            currentMatrix = SVGXMatrix.identity;
            currentFillStyle = SVGXFillStyle.Default;
            currentStrokeStyle = SVGXStrokeStyle.Default;
            currentShapeRange = default;
            transforms.Add(currentMatrix);
        }


        public void SetTexture(Texture texture) { }

        public void SetUVTransform() { }

        public void SetStroke(in Color color) {
            currentStrokeStyle.encodedColor = VertigoUtil.ColorToFloat(color);
        }

        public void SetFill(in Color color) {
            currentFillStyle.encodedColor = VertigoUtil.ColorToFloat(color);
        }

        public void SetFillOpacity(float opacity) {
            currentFillStyle.opacity = opacity;
        }

        public void SetStrokeOpacity(float opacity) {
            currentStrokeStyle.opacity = opacity;
        }

        public void SetStrokeWidth(float width) {
            currentStrokeStyle.strokeWidth = width;
        }

        public void SetShapeRounding() { }

        public void Stroke() {
            strokeStyles = strokeStyles ?? StructList<SVGXStrokeStyle>.Get();
            int styleIdx = strokeStyles.size;
            strokeStyles.Add(currentStrokeStyle);
            if (matrixChanged) {
                transforms.Add(currentMatrix); // don't add if we didnt change it
            }

            drawCallList.Add(new SVGXDrawCall2(DrawCallType.StandardStroke, styleIdx, transforms.size - 1, currentShapeRange));
            currentShapeRange = new RangeInt(currentShapeRange.end, 0);
        }

        public void Fill() {
            fillStyles = fillStyles ?? StructList<SVGXFillStyle>.Get();
            int styleIdx = fillStyles.size;
            fillStyles.Add(currentFillStyle);
            if (matrixChanged) {
                transforms.Add(currentMatrix); // don't add if we didnt change it
            }

            drawCallList.Add(new SVGXDrawCall2(DrawCallType.StandardFill, styleIdx, transforms.size - 1, currentShapeRange));
            currentShapeRange = new RangeInt(currentShapeRange.end, 0);
        }

        public void LineTo(Vector2 position) {
            LineTo(position.x, position.y);
        }

        internal void UpdateGeometry() {
            // if (!requiresGeometryUpdate) return;
            geometry.Clear(); // todo -- can be optimized for adding / updating only

            GeometryData geometryData = new GeometryData() {
                positionList = geometry.positionList,
                texCoordList0 = geometry.texCoordList0,
                texCoordList1 = geometry.texCoordList1,
                triangleList = geometry.triangleList
            };

            SVGXDrawCall2[] drawCalls = drawCallList.array;

            for (int i = 0; i < drawCallList.size; i++) {
                ref SVGXDrawCall2 drawCall = ref drawCalls[i];

                GeometryRange range = new GeometryRange(geometry.positionList.size, 0, geometry.triangleList.size, 0);

                switch (drawCall.type) {
                    case DrawCallType.StandardStroke:
                        for (int j = drawCall.shapeRange.start; j < drawCall.shapeRange.end; j++) {
                            GenerateStrokeGeometry(ref geometryData, ref shapeList.array[j], strokeStyles[drawCall.styleIdx]);
                        }
                        break;

                    case DrawCallType.StandardFill: {
                        for (int j = drawCall.shapeRange.start; j < drawCall.shapeRange.end; j++) {
                            GenerateFillGeometry(ref geometryData, ref shapeList.array[j], fillStyles[drawCall.styleIdx]);
                        }

                        break;
                    }

                    case DrawCallType.Shadow:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                range.vertexEnd = geometry.positionList.size;
                range.triangleEnd = geometry.triangleList.size;
                drawCall.geometryRange = range;
            }
        }


        private static float EncodeCornerRadii(float width, float height, Vector2 top, Vector2 bottom) {
            float min = math.min(width, height);

            if (min <= 0) min = 0.0001f;

            float halfMin = min * 0.5f;

            float cornerRadiusTopLeft = top.x;
            float cornerRadiusTopRight = top.y;
            float cornerRadiusBottomLeft = bottom.x;
            float cornerRadiusBottomRight = bottom.y;

            cornerRadiusTopLeft = math.clamp(cornerRadiusTopLeft, 0, halfMin) / min;
            cornerRadiusTopRight = math.clamp(cornerRadiusTopRight, 0, halfMin) / min;
            cornerRadiusBottomLeft = math.clamp(cornerRadiusBottomLeft, 0, halfMin) / min;
            cornerRadiusBottomRight = math.clamp(cornerRadiusBottomRight, 0, halfMin) / min;

            byte b0 = (byte) (((cornerRadiusTopLeft * 1000)) * 0.5f);
            byte b1 = (byte) (((cornerRadiusTopRight * 1000)) * 0.5f);
            byte b2 = (byte) (((cornerRadiusBottomLeft * 1000)) * 0.5f);
            byte b3 = (byte) (((cornerRadiusBottomRight * 1000)) * 0.5f);

            return VertigoUtil.BytesToFloat(b0, b1, b2, b3);
        }

        private void GenerateStrokeGeometry(ref GeometryData geometryData, ref ShapeDef shape, in SVGXStrokeStyle strokeStyle) {
            switch (shape.shapeType) {
                case ShapeType.Unset:
                    break;

                case ShapeType.Rect: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    shape.geometryRange = GeometryGenerator.StrokeRect(geometryData, position.x, position.y, size.x, size.y, strokeStyle.strokeWidth);
                    objectData.Add(new Vector4((int) ShapeType.Rect, 0, VertigoUtil.PackSizeVector(size), strokeStyle.strokeWidth));
                    colorData.Add(new Vector4(strokeStyle.encodedColor, strokeStyle.encodedTint, strokeStyle.opacity, (int) strokeStyle.paintMode));
                    break;
                }

                case ShapeType.RoundedRect: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    Vector2 radiiTop = pointList.array[shape.pointRange.start + 0].position;
                    Vector2 radiiBottom = pointList.array[shape.pointRange.start + 1].position;
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    shape.geometryRange = GeometryGenerator.FillDecoratedRect(geometryData, position, size.x, size.y, cornerDefinition);
                    objectData.Add(new Vector4((int) ShapeType.Ellipse, EncodeCornerRadii(size.x, size.y, radiiTop, radiiBottom), VertigoUtil.PackSizeVector(size), strokeStyle.strokeWidth));
                    colorData.Add(new Vector4(strokeStyle.encodedColor, strokeStyle.encodedTint, strokeStyle.opacity, (int) strokeStyle.paintMode));
                    break;
                }

                case ShapeType.Circle: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    shape.geometryRange = GeometryGenerator.FillDecoratedRect(geometryData, position, size.x, size.y, cornerDefinition);
                    objectData.Add(new Vector4((int) ShapeType.Ellipse, s_CircleRadii, VertigoUtil.PackSizeVector(size), strokeStyle.strokeWidth));
                    colorData.Add(new Vector4(strokeStyle.encodedColor, strokeStyle.encodedTint, strokeStyle.opacity, (int) strokeStyle.paintMode));
                    break;
                }

                case ShapeType.Ellipse: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    shape.geometryRange = GeometryGenerator.FillDecoratedRect(geometryData, position, size.x, size.y, cornerDefinition);
                    objectData.Add(new Vector4((int) ShapeType.Ellipse, s_CircleRadii, VertigoUtil.PackSizeVector(size), strokeStyle.strokeWidth));
                    colorData.Add(new Vector4(strokeStyle.encodedColor, strokeStyle.encodedTint, strokeStyle.opacity, (int) strokeStyle.paintMode));
                    break;
                }

                case ShapeType.Rhombus:
                    break;
                case ShapeType.Triangle:
                    break;
                case ShapeType.Polygon:
                    break;
                case ShapeType.Text:
                    break;
                case ShapeType.Path:
                    break;
                case ShapeType.ClosedPath: {
                    GeometryGenerator.RenderState renderState = new GeometryGenerator.RenderState();
                    renderState.lineCap = strokeStyle.lineCap;
                    renderState.lineJoin = strokeStyle.lineJoin;
                    renderState.miterLimit = (int)strokeStyle.miterLimit;
                    renderState.strokeWidth = strokeStyle.strokeWidth;
                    
                    GeometryGenerator.StrokeClosedPath(geometryData, pointList, shape.pointRange, renderState);
                }
                    break;
                case ShapeType.Sprite:
                    break;
                case ShapeType.Sector:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }    
        }
        
        private void GenerateFillGeometry(ref GeometryData geometryData, ref ShapeDef shape, in SVGXFillStyle fillStyle) {
            switch (shape.shapeType) {
                case ShapeType.Polygon: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    int segmentCount = (int) pointList.array[shape.pointRange.start].position.x;
                    shape.geometryRange = GeometryGenerator.FillRegularPolygon(geometryData, position, size.x, size.y, segmentCount);
                    objectData.Add(new Vector4((int) ShapeType.Polygon, 0, VertigoUtil.PackSizeVector(size), 0));
                    colorData.Add(new Vector4(fillStyle.encodedColor, fillStyle.encodedTint, fillStyle.opacity, (int) fillStyle.paintMode));
                    break;
                }

                case ShapeType.Ellipse: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    shape.geometryRange = GeometryGenerator.FillDecoratedRect(geometryData, position, size.x, size.y, cornerDefinition);
                    objectData.Add(new Vector4((int) ShapeType.Ellipse, s_CircleRadii, VertigoUtil.PackSizeVector(size), 0));
                    colorData.Add(new Vector4(fillStyle.encodedColor, fillStyle.encodedTint, fillStyle.opacity, (int) fillStyle.paintMode));
                    break;
                }

                case ShapeType.Rect: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    shape.geometryRange = GeometryGenerator.FillRect(geometryData, position.x, position.y, size.x, size.y);
                    objectData.Add(new Vector4((int) ShapeType.Rect, 0, VertigoUtil.PackSizeVector(size), 0));
                    colorData.Add(new Vector4(fillStyle.encodedColor, fillStyle.encodedTint, fillStyle.opacity, (int) fillStyle.paintMode));
                    break;
                }

                case ShapeType.ClosedPath:
                case ShapeType.Path: {
                    shape.geometryRange = GeometryGenerator.FillClosedPath(new GeometryGenerator.PathData() {
                        bounds = shape.bounds,
                        pointRange = shape.pointRange,
                        holeRange = shape.holeRange,
                        points = pointList,
                        holes = holeList
                    }, geometryData);

                    objectData.Add(new Vector4((int) ShapeType.ClosedPath, 0, 0, 0));
                    colorData.Add(new Vector4(fillStyle.encodedColor, fillStyle.encodedTint, fillStyle.opacity, (int) fillStyle.paintMode));

                    break;
                }

                case ShapeType.RoundedRect: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    Vector2 radiiTop = pointList.array[shape.pointRange.start + 0].position;
                    Vector2 radiiBottom = pointList.array[shape.pointRange.start + 1].position;
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    shape.geometryRange = GeometryGenerator.FillDecoratedRect(geometryData, position, size.x, size.y, cornerDefinition);
                    objectData.Add(new Vector4((int) ShapeType.Ellipse, EncodeCornerRadii(size.x, size.y, radiiTop, radiiBottom), VertigoUtil.PackSizeVector(size), 0));
                    colorData.Add(new Vector4(fillStyle.encodedColor, fillStyle.encodedTint, fillStyle.opacity, (int) fillStyle.paintMode));
                    break;
                }

                case ShapeType.Circle: {
                    Vector2 position = shape.bounds.position;
                    Vector2 size = shape.bounds.size;
                    float clip = Mathf.Min(size.x, size.y) * 0.25f;
                    CornerDefinition cornerDefinition = new CornerDefinition(clip);
                    shape.geometryRange = GeometryGenerator.FillDecoratedRect(geometryData, position, size.x, size.y, cornerDefinition);
                    objectData.Add(new Vector4((int) ShapeType.Circle, s_CircleRadii, VertigoUtil.PackSizeVector(size), 0));
                    colorData.Add(new Vector4(fillStyle.encodedColor, fillStyle.encodedTint, fillStyle.opacity, (int) fillStyle.paintMode));
                    break;
                }

                case ShapeType.Text:
                    break;

                case ShapeType.Sector:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetStrokeJoin(LineJoin joinType) {
            currentStrokeStyle.lineJoin = joinType;
        }

    }

}