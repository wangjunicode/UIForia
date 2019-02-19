using System;
using System.Collections.Generic;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public class BatchedVertexData {

        public readonly Mesh mesh;

        public int triangleIndex;

        private readonly List<Vector3> positionList;
        private readonly List<Vector4> uv0List;
        private readonly List<Vector4> uv1List;
        private readonly List<Vector4> uv2List;
        private readonly List<Color> colorsList;
        private readonly List<int> trianglesList;

        public BatchedVertexData() {
            positionList = new List<Vector3>(128);
            uv0List = new List<Vector4>(128);
            uv1List = new List<Vector4>(128);
            uv2List = new List<Vector4>(128);
            colorsList = new List<Color>(128);
            trianglesList = new List<int>(128 * 3);
            mesh = new Mesh();
            mesh.MarkDynamic();
        }

        private void GenerateSegmentBodies(Vector2[] points, int count, Color color, float strokeWidth, float z) {
            const int join = 0;
            const int renderData = 0;

            for (int i = 1; i < count; i++) {
                Vector2 prev = points[i - 1];
                Vector2 curr = points[i];
                Vector2 next = points[i + 1];
                Vector2 far = points[i + 2];

                positionList.Add(new Vector3(curr.x, -curr.y, z));
                positionList.Add(new Vector3(next.x, -next.y, z));
                positionList.Add(new Vector3(curr.x, -curr.y, z));
                positionList.Add(new Vector3(next.x, -next.y, z));

                uv0List.Add(new Vector4(0, 1));
                uv0List.Add(new Vector4(1, 1));
                uv0List.Add(new Vector4(1, 0));
                uv0List.Add(new Vector4(0, 0));

                uv1List.Add(new Vector4(renderData, BitUtil.SetBytes(1, 0, join, 0), 0, strokeWidth));
                uv1List.Add(new Vector4(renderData, BitUtil.SetBytes(1, 1, join, 0), 0, strokeWidth));
                uv1List.Add(new Vector4(renderData, BitUtil.SetBytes(0, 2, join, 0), 0, strokeWidth));
                uv1List.Add(new Vector4(renderData, BitUtil.SetBytes(0, 3, join, 0), 0, strokeWidth));

                uv2List.Add(new Vector4(prev.x, -prev.y, next.x, -next.y));
                uv2List.Add(new Vector4(curr.x, -curr.y, far.x, -far.y));
                uv2List.Add(new Vector4(prev.x, -prev.y, next.x, -next.y));
                uv2List.Add(new Vector4(curr.x, -curr.y, far.x, -far.y));

                colorsList.Add(color);
                colorsList.Add(color);
                colorsList.Add(color);
                colorsList.Add(color);

                trianglesList.Add(triangleIndex + 0);
                trianglesList.Add(triangleIndex + 1);
                trianglesList.Add(triangleIndex + 2);
                trianglesList.Add(triangleIndex + 2);
                trianglesList.Add(triangleIndex + 1);
                trianglesList.Add(triangleIndex + 3);

                triangleIndex += 4;
            }
        }

        private RangeInt GenerateStrokeGeometry(LightList<Vector2> output, SVGXShapeType shapeType, SVGXMatrix matrix, Vector2[] points, RangeInt pointRange, bool isClosed) {
            RangeInt retn = new RangeInt(output.Count, 0);

            switch (shapeType) {
                case SVGXShapeType.Unset:
                    break;

                case SVGXShapeType.Rect: {
                    Vector2 p0 = matrix.Transform(points[pointRange.start + 0]);
                    Vector2 p1 = matrix.Transform(points[pointRange.start + 1]);

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
                    break;

                case SVGXShapeType.Path: {

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
                        output[retn.start] = Vector2.zero;
                        output.Add(output[output.Count - 1] + output[output.Count - 1] - output[output.Count - 2]);
                        output.Add(output[output.Count - 1] + output[output.Count - 1] - output[output.Count - 2]);
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

        private const int DrawType_Fill = 1;
        private const int DrawType_Stroke = 0;
        private const int VertexType_Near = 1;
        private const int VertexType_Far = 0;
        
        internal void CreateStrokeVertices(Vector2[] points, SVGXRenderShape renderShape, GFX.GradientData gradientData, SVGXStyle style, SVGXMatrix matrix) {
            
            // todo -- use point cache to store geometry for multiple shape lookups, index by shape id, use returned ranged from GenerateStrokeGeometry
            
            LightList<Vector2> pointCache = LightListPool<Vector2>.Get();

            RangeInt range = GenerateStrokeGeometry(pointCache, renderShape.shape.type, matrix, points, renderShape.shape.pointRange, renderShape.shape.isClosed);

            float strokeWidth = Mathf.Clamp(style.strokeWidth, 1, style.strokeWidth);
            Color color = style.strokeColor;
            color.a *= style.strokeOpacity;

            bool isClosed = renderShape.shape.isClosed;
            const int cap = 1;
            
            if (isClosed) {
                GenerateSegmentBodies(pointCache.Array, pointCache.Count - 2, color, strokeWidth, renderShape.zIndex);
            }
            else if (renderShape.shape.pointRange.length == 2) {
                GenerateSegmentBodies(pointCache.Array, pointCache.Count - 2, color, strokeWidth, renderShape.zIndex);
                uv1List[0] = new Vector4(DrawType_Stroke, BitUtil.SetBytes(1, VertexType_Near, cap, 0), 0, strokeWidth);
                uv1List[1] = new Vector4(DrawType_Stroke, BitUtil.SetBytes(1, VertexType_Far, cap, 0), 0, strokeWidth);
                uv1List[2] = new Vector4(DrawType_Stroke, BitUtil.SetBytes(0, VertexType_Near, cap, 0), 0, strokeWidth);
                uv1List[3] = new Vector4(DrawType_Stroke, BitUtil.SetBytes(0, VertexType_Far, cap, 0), 0, strokeWidth);
            }
            else {
                GenerateSegmentBodies(pointCache.Array, pointCache.Count - 2, color, strokeWidth, renderShape.zIndex);
                uv1List[0] = new Vector4(DrawType_Stroke, BitUtil.SetBytes(1, VertexType_Near, cap, 0), 0, strokeWidth);
                uv1List[2] = new Vector4(DrawType_Stroke, BitUtil.SetBytes(0, VertexType_Near, cap, 0), 0, strokeWidth);
                uv1List[uv1List.Count - 3] = new Vector4(DrawType_Stroke, BitUtil.SetBytes(1, VertexType_Far, cap, 0), 0, strokeWidth);
                uv1List[uv1List.Count - 1] = new Vector4(DrawType_Stroke, BitUtil.SetBytes(0, VertexType_Far, cap, 0), 0, strokeWidth);
            }

            LightListPool<Vector2>.Release(ref pointCache);
        }

        public Mesh FillMesh() {
            mesh.Clear(true);

            mesh.SetVertices(positionList);
            mesh.SetColors(colorsList);
            mesh.SetUVs(0, uv0List);
            mesh.SetUVs(1, uv1List);
            mesh.SetUVs(2, uv2List);
            mesh.SetTriangles(trianglesList, 0);

            positionList.Clear();
            uv0List.Clear();
            uv1List.Clear();
            uv2List.Clear();
            colorsList.Clear();

            return mesh;
        }

        internal void CreateFillVertices(Vector2[] points, SVGXRenderShape renderShape, GFX.GradientData gradientData, SVGXStyle style, SVGXMatrix matrix) {
            int start = renderShape.shape.pointRange.start;
            int end = renderShape.shape.pointRange.end;

            int triIdx = triangleIndex;

            Color color = style.fillColor;

            if (style.fillMode == FillMode.Color) {
                color = style.fillColor;
            }
            else if ((style.fillMode & FillMode.Tint) != 0) {
                color = style.fillTintColor;
            }

            int gradientId = gradientData.rowId;
            int gradientDirection = 0;

            uint fillColorModes = (uint) style.fillMode;

            if (gradientData.gradient is SVGXLinearGradient linearGradient) {
                gradientDirection = (int) linearGradient.direction;
            }

            Vector4 dimensions = renderShape.shape.Dimensions;

            float z = renderShape.zIndex;
            float opacity = style.fillOpacity;
            color.a *= opacity;

            LightList<Vector2> transformedPoints = LightListPool<Vector2>.Get();
            transformedPoints.EnsureCapacity(renderShape.shape.pointRange.length);

            Vector2[] transformedArray = transformedPoints.Array;

            for (int i = start, idx = 0; i < end; i++, idx++) {
                transformedArray[idx] = matrix.Transform(points[i]);
            }

            start = 0;
            end = renderShape.shape.pointRange.length;

            int renderData = BitUtil.SetHighLowBits((int) renderShape.shape.type, 1);

            switch (renderShape.shape.type) {
                case SVGXShapeType.Unset:
                    break;
                case SVGXShapeType.Ellipse:
                case SVGXShapeType.Circle:
                case SVGXShapeType.Rect:

                    Vector2 p0 = transformedArray[0];
                    Vector2 p1 = transformedArray[1];
                    Vector2 p2 = transformedArray[2];
                    Vector2 p3 = transformedArray[3];

                    positionList.Add(new Vector3(p0.x, -p0.y, z));
                    positionList.Add(new Vector3(p1.x, -p1.y, z));
                    positionList.Add(new Vector3(p2.x, -p2.y, z));
                    positionList.Add(new Vector3(p3.x, -p3.y, z));

                    trianglesList.Add(triIdx + 0);
                    trianglesList.Add(triIdx + 1);
                    trianglesList.Add(triIdx + 2);
                    trianglesList.Add(triIdx + 2);
                    trianglesList.Add(triIdx + 3);
                    trianglesList.Add(triIdx + 0);

                    uv0List.Add(new Vector4(0, 1));
                    uv0List.Add(new Vector4(1, 1));
                    uv0List.Add(new Vector4(1, 0));
                    uv0List.Add(new Vector4(0, 0));

//                    uv0List.Add(dimensions);
//                    uv0List.Add(dimensions);
//                    uv0List.Add(dimensions);
//                    uv0List.Add(dimensions);

                    uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));

                    uv2List.Add(new Vector4());
                    uv2List.Add(new Vector4());
                    uv2List.Add(new Vector4());
                    uv2List.Add(new Vector4());

                    colorsList.Add(color);
                    colorsList.Add(color);
                    colorsList.Add(color);
                    colorsList.Add(color);

                    triangleIndex = triIdx + 4;

                    break;

                case SVGXShapeType.Path:

                    // assume closed for now
                    // assume convex for now

                    throw new NotImplementedException();

                case SVGXShapeType.RoundedRect: // or other convex shape without holes

                    for (int i = start; i < end; i++) {
                        positionList.Add(new Vector3(transformedArray[i].x, transformedArray[i].y, z));
                        uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));
                        colorsList.Add(color);
                        uv0List.Add(dimensions);
                    }

                    int t = 0;
                    for (int i = start; i < end - 1; i++) {
                        trianglesList.Add(triIdx + 0);
                        trianglesList.Add(triIdx + t);
                        trianglesList.Add(triIdx + 1);
                        t++;
                    }

                    triangleIndex = triIdx + renderShape.shape.pointRange.length; // todo this might be off by 1

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            LightListPool<Vector2>.Release(ref transformedPoints);
        }

    }

}