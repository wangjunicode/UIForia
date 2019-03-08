using System;
using System.Collections.Generic;
using Packages.UIForia.Src.VectorGraphics;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public class BatchedVertexData {

        internal const int RenderTypeFill = 0;
        internal const int RenderTypeText = 1;
        internal const int RenderTypeStroke = 2;
        internal const int RenderTypeStrokeShape = 3;
        internal const int RenderTypeShadow = 4;

        public readonly Mesh mesh;

        public int triangleIndex;

        private readonly List<Vector3> positionList;
        private readonly List<Vector4> uv0List;
        private readonly List<Vector4> uv1List;
        private readonly List<Vector4> uv2List;
        private readonly List<Vector4> uv3List;
        private readonly List<Vector4> uv4List;
        private readonly List<Color> colorsList;
        private readonly List<int> trianglesList;

        // todo -- figure out bit packing and remove uv3List and uv4List
        // todo -- when unity allows better mesh api, remove lists in favor of array
        // todo -- verify that mesh.Clear doesn't cause a re-allocation

        public BatchedVertexData() {
            positionList = new List<Vector3>(128);
            uv0List = new List<Vector4>(128);
            uv1List = new List<Vector4>(128);
            uv2List = new List<Vector4>(128);
            uv3List = new List<Vector4>(128);
            uv4List = new List<Vector4>(128);
            colorsList = new List<Color>(128);
            trianglesList = new List<int>(128 * 3);
            mesh = new Mesh();
            mesh.MarkDynamic();
        }

        private const int DrawType_Fill = 1;
        private const int DrawType_Stroke = 0;
        private const int VertexType_Near = 1;
        private const int VertexType_Far = 0;

        public Mesh FillMesh() {
            mesh.Clear(true);

            mesh.SetVertices(positionList);
            mesh.SetColors(colorsList);
            mesh.SetUVs(0, uv0List);
            mesh.SetUVs(1, uv1List);
            mesh.SetUVs(2, uv2List);
            mesh.SetUVs(3, uv3List);
            mesh.SetUVs(4, uv4List);
            mesh.SetTriangles(trianglesList, 0);

            positionList.Clear();
            uv0List.Clear();
            uv1List.Clear();
            uv2List.Clear();
            uv3List.Clear();
            uv4List.Clear();
            colorsList.Clear();
            trianglesList.Clear();
            triangleIndex = 0;

            return mesh;
        }

        public static float EncodeColor(Color color) {
            return Vector4.Dot(color, new Vector4(1f, 1 / 255f, 1 / 65025.0f, 1 / 16581375.0f));
        }

        private void GenerateCapStart(Vector2[] points, int count, LineCap cap, float strokeWidth, float z) {
            Vector2 p0 = points[0];
            Vector2 p1 = points[1];

            Vector3 v0 = new Vector3();
            Vector3 v1 = new Vector3();
            Vector3 v2 = new Vector3();
            Vector3 v3 = new Vector3();

            Vector2 toP1 = (p1 - p0).normalized;
            Vector2 toP1Perp = new Vector2(-toP1.y, toP1.x);

            switch (cap) {
                case LineCap.Butt:
                    v0 = toP1Perp * strokeWidth * 0.5f;
                    v1 = -toP1Perp * strokeWidth * 0.5f;
                    break;
                case LineCap.Square:
                    break;
                case LineCap.Round:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cap), cap, null);
            }

            positionList.Add(v0);
            positionList.Add(v1);
            positionList.Add(v2);
            positionList.Add(v3);
        }

        private void GenerateJoinedPath(Vector2[] points, int count, Color color, float strokeWidth, float z) {
            //  GenerateCapStart();
            //  GenerateCapEnd();
        }

        private void GenerateSegmentBodies(Vector2[] points, int count, Color color, float strokeWidth, float z) {
            const int join = 0;

            int renderData = BitUtil.SetHighLowBits((int) SVGXShapeType.Path, RenderTypeStroke);

            uint flags0 = BitUtil.SetBytes(1, VertexType_Near, join, 0);
            uint flags1 = BitUtil.SetBytes(1, VertexType_Far, join, 0);
            uint flags2 = BitUtil.SetBytes(0, VertexType_Near, join, 0);
            uint flags3 = BitUtil.SetBytes(0, VertexType_Far, join, 0);

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

                uv1List.Add(new Vector4(renderData, flags0, 1, strokeWidth));
                uv1List.Add(new Vector4(renderData, flags1, 1, strokeWidth));
                uv1List.Add(new Vector4(renderData, flags2, -1, strokeWidth));
                uv1List.Add(new Vector4(renderData, flags3, -1, strokeWidth));

                uv2List.Add(new Vector4(prev.x, -prev.y, next.x, -next.y));
                uv2List.Add(new Vector4(curr.x, -curr.y, far.x, -far.y));
                uv2List.Add(new Vector4(prev.x, -prev.y, next.x, -next.y));
                uv2List.Add(new Vector4(curr.x, -curr.y, far.x, -far.y));

                uv3List.Add(new Vector4(1, 1, 0, 0));
                uv3List.Add(new Vector4(1, 0, 0, 0));
                uv3List.Add(new Vector4(-1, 1, 0, 0));
                uv3List.Add(new Vector4(-1, 0, 0, 0));

                uv4List.Add(new Vector4());
                uv4List.Add(new Vector4());
                uv4List.Add(new Vector4());
                uv4List.Add(new Vector4());

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

        internal void CreateStrokeVertices(Vector2[] points, SVGXRenderShape renderShape, GFX.GradientData gradientData, SVGXStyle style, SVGXMatrix matrix) {
            int start = renderShape.shape.pointRange.start;
            int triIdx = triangleIndex;
            float strokeWidth = Mathf.Clamp(style.strokeWidth, 1f, style.strokeWidth);
            float z = renderShape.zIndex;
            Color color = style.strokeColor;
            color.a *= style.strokeOpacity;

            switch (renderShape.shape.type) {
                case SVGXShapeType.Path:
                    break;

                case SVGXShapeType.Unset:
                    return;

                case SVGXShapeType.RoundedRect: {
                    int renderData = BitUtil.SetHighLowBits((int) renderShape.shape.type, RenderTypeStrokeShape);

                    Vector2 pos = points[start + 0];
                    Vector2 wh = points[start + 1];
                    Vector2 borderRadiusXY = points[start + 2];
                    Vector2 borderRadiusZW = points[start + 3];
                    strokeWidth += 0.1f; // not sure why but stroke isn't right here if 

                    int gradientId = gradientData.rowId;
                    int gradientDirection = 0;

                    uint strokeColorMode = (uint) style.strokeColorMode;

                    if (gradientData.gradient is SVGXLinearGradient linearGradient) {
                        gradientDirection = (int) linearGradient.direction;
                    }

                    // if we use negatives then we can bend the outline without bending the box

                    if (style.strokePlacement == StrokePlacement.Outside) {
                        pos -= new Vector2(strokeWidth - 0.5f, strokeWidth - 0.5f);
                        wh += new Vector2(strokeWidth * 2f, strokeWidth * 2f);
                        borderRadiusXY.x += strokeWidth;
                        borderRadiusXY.y += strokeWidth;
                        borderRadiusZW.x += strokeWidth;
                        borderRadiusZW.y += strokeWidth;
                        wh.x -= 1f;
                        wh.y -= 1;
                    }
                    else if (style.strokePlacement == StrokePlacement.Center) {
                        strokeWidth += 0.1f;
                        pos -= new Vector2(strokeWidth * 0.5f, strokeWidth * 0.5f);
                        wh += new Vector2(strokeWidth, strokeWidth);
                        borderRadiusXY.x += strokeWidth * 0.5f;
                        borderRadiusXY.y += strokeWidth * 0.5f;
                        borderRadiusZW.x += strokeWidth * 0.5f;
                        borderRadiusZW.y += strokeWidth * 0.5f;
                    }
                    else {
                        pos.x -= 1f;
                        pos.y -= 1f;
                        wh.x += 1f;
                        wh.y += 1f;
                    }

                    Vector2 p0 = matrix.Transform(new Vector2(pos.x, pos.y));
                    Vector2 p1 = matrix.Transform(new Vector2(pos.x + wh.x, pos.y));
                    Vector2 p2 = matrix.Transform(new Vector2(pos.x + wh.x, pos.y + wh.y));
                    Vector2 p3 = matrix.Transform(new Vector2(pos.x, pos.y + wh.y));

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

                    uv0List.Add(new Vector4(0, 1, wh.x, wh.y));
                    uv0List.Add(new Vector4(1, 1, wh.x, wh.y));
                    uv0List.Add(new Vector4(1, 0, wh.x, wh.y));
                    uv0List.Add(new Vector4(0, 0, wh.x, wh.y));

                    uv1List.Add(new Vector4(renderData, strokeColorMode, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, strokeColorMode, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, strokeColorMode, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, strokeColorMode, gradientId, gradientDirection));

                    uv2List.Add(new Vector4(borderRadiusXY.x, borderRadiusXY.y, borderRadiusZW.x, borderRadiusZW.y));
                    uv2List.Add(new Vector4(borderRadiusXY.x, borderRadiusXY.y, borderRadiusZW.x, borderRadiusZW.y));
                    uv2List.Add(new Vector4(borderRadiusXY.x, borderRadiusXY.y, borderRadiusZW.x, borderRadiusZW.y));
                    uv2List.Add(new Vector4(borderRadiusXY.x, borderRadiusXY.y, borderRadiusZW.x, borderRadiusZW.y));

                    uv3List.Add(new Vector4(strokeWidth, 0, 0, 0));
                    uv3List.Add(new Vector4(strokeWidth, 0, 0, 0));
                    uv3List.Add(new Vector4(strokeWidth, 0, 0, 0));
                    uv3List.Add(new Vector4(strokeWidth, 0, 0, 0));

                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());

                    colorsList.Add(color);
                    colorsList.Add(color);
                    colorsList.Add(color);
                    colorsList.Add(color);

                    triangleIndex = triIdx + 4;
                    return;
                }


                case SVGXShapeType.Rect:
                case SVGXShapeType.Circle:
                case SVGXShapeType.Ellipse: {
                    int gradientId = gradientData.rowId;
                    int gradientDirection = 0;

                    uint strokeColorMode = (uint) style.strokeColorMode;

                    if (gradientData.gradient is SVGXLinearGradient linearGradient) {
                        gradientDirection = (int) linearGradient.direction;
                    }

                    // todo for strokes cutout the shape of the center, ie build a mesh that produces less discard ops
                    int renderData = BitUtil.SetHighLowBits((int) renderShape.shape.type, RenderTypeStrokeShape);

                    Vector2 pos = points[start + 0];
                    Vector2 wh = points[start + 1];
                    Vector2 borderRadiusXY = wh;
                    Vector2 borderRadiusZW = wh;

                    if (renderShape.shape.type == SVGXShapeType.Rect) {
                        borderRadiusXY = Vector2.zero;
                        borderRadiusZW = Vector2.zero;
                    }

                    if (style.strokePlacement == StrokePlacement.Outside) {
                        pos -= new Vector2(strokeWidth - 0.5f, strokeWidth - 0.5f);
                        wh += new Vector2(strokeWidth * 2f, strokeWidth * 2f);
                    }
                    else if (style.strokePlacement == StrokePlacement.Center) {
                        pos -= new Vector2(strokeWidth * 0.5f, strokeWidth * 0.5f);
                        wh += new Vector2(strokeWidth, strokeWidth);
                    }
                    else {
                        pos.x -= 1;
                        pos.y -= 1;
                        wh.x += 2;
                        wh.y += 2;
                    }

                    Vector2 p0 = matrix.Transform(new Vector2(pos.x, pos.y));
                    Vector2 p1 = matrix.Transform(new Vector2(pos.x + wh.x, pos.y));
                    Vector2 p2 = matrix.Transform(new Vector2(pos.x + wh.x, pos.y + wh.y));
                    Vector2 p3 = matrix.Transform(new Vector2(pos.x, pos.y + wh.y));

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

                    uv0List.Add(new Vector4(0, 1, wh.x, wh.y));
                    uv0List.Add(new Vector4(1, 1, wh.x, wh.y));
                    uv0List.Add(new Vector4(1, 0, wh.x, wh.y));
                    uv0List.Add(new Vector4(0, 0, wh.x, wh.y));

                    uv1List.Add(new Vector4(renderData, strokeColorMode, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, strokeColorMode, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, strokeColorMode, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, strokeColorMode, gradientId, gradientDirection));

                    uv2List.Add(new Vector4(borderRadiusXY.x, borderRadiusXY.y, borderRadiusZW.x, borderRadiusZW.y));
                    uv2List.Add(new Vector4(borderRadiusXY.x, borderRadiusXY.y, borderRadiusZW.x, borderRadiusZW.y));
                    uv2List.Add(new Vector4(borderRadiusXY.x, borderRadiusXY.y, borderRadiusZW.x, borderRadiusZW.y));
                    uv2List.Add(new Vector4(borderRadiusXY.x, borderRadiusXY.y, borderRadiusZW.x, borderRadiusZW.y));

                    uv3List.Add(new Vector4(strokeWidth, 0, 0, 0));
                    uv3List.Add(new Vector4(strokeWidth, 0, 0, 0));
                    uv3List.Add(new Vector4(strokeWidth, 0, 0, 0));
                    uv3List.Add(new Vector4(strokeWidth, 0, 0, 0));

                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());

                    colorsList.Add(color);
                    colorsList.Add(color);
                    colorsList.Add(color);
                    colorsList.Add(color);

                    triangleIndex = triIdx + 4;
                    return;
                }
                case SVGXShapeType.Text:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            LightList<Vector2> pointCache = LightListPool<Vector2>.Get();
            SVGXGeometryUtil.GenerateStrokeGeometry(pointCache, style.strokePlacement, style.strokeWidth, renderShape.shape.type, matrix, points, renderShape.shape.pointRange, renderShape.shape.isClosed);

            bool isClosed = renderShape.shape.isClosed;
            const int cap = 1;
            if (isClosed) {
                GenerateSegmentBodies(pointCache.Array, pointCache.Count - 2, color, strokeWidth, renderShape.zIndex);
            }
            else if (renderShape.shape.pointRange.length == 2) {
                int renderData = BitUtil.SetHighLowBits(DrawType_Stroke, RenderTypeStroke);
                int rangeStart = uv1List.Count;
                GenerateSegmentBodies(pointCache.Array, pointCache.Count - 2, color, strokeWidth, renderShape.zIndex);
                uv1List[rangeStart + 0] = new Vector4(renderData, BitUtil.SetBytes(1, VertexType_Near, cap, 0), 0, strokeWidth);
                uv1List[rangeStart + 1] = new Vector4(renderData, BitUtil.SetBytes(1, VertexType_Far, cap, 0), 0, strokeWidth);
                uv1List[rangeStart + 2] = new Vector4(renderData, BitUtil.SetBytes(0, VertexType_Near, cap, 0), 0, strokeWidth);
                uv1List[rangeStart + 3] = new Vector4(renderData, BitUtil.SetBytes(0, VertexType_Far, cap, 0), 0, strokeWidth);
            }
            else {
                int rangeStart = uv3List.Count;
                GenerateSegmentBodies(pointCache.Array, pointCache.Count - 2, color, strokeWidth, renderShape.zIndex);
                uv3List[rangeStart + 0] = new Vector4(1, VertexType_Near, cap, 0);
                uv3List[rangeStart + 2] = new Vector4(-1, VertexType_Near, cap, 0);
                uv3List[uv3List.Count - 3] = new Vector4(uv3List[uv3List.Count - 3].x, uv3List[uv3List.Count - 3].y, cap, 0);
                uv3List[uv3List.Count - 1] = new Vector4(uv3List[uv3List.Count - 1].x, uv3List[uv3List.Count - 1].y, cap, 0);
            }

            LightListPool<Vector2>.Release(ref pointCache);
        }

        internal void CreateFillVertices(Vector2[] points, SVGXRenderShape renderShape, GFX.GradientData gradientData, SVGXStyle style, SVGXMatrix matrix) {
            int start = renderShape.shape.pointRange.start;
            int end = renderShape.shape.pointRange.end;

            int triIdx = triangleIndex;

            Color color = style.fillColor;

            if (style.fillColorMode == ColorMode.Color) {
                color = style.fillColor;
            }
            else if ((style.fillColorMode & ColorMode.Tint) != 0) {
                color = style.fillTintColor;
            }

            int gradientId = gradientData.rowId;
            int gradientDirection = 0;

            uint fillColorModes = (uint) style.fillColorMode;

            if (gradientData.gradient is SVGXLinearGradient linearGradient) {
                gradientDirection = (int) linearGradient.direction;
            }

            float z = renderShape.zIndex;
            float opacity = style.fillOpacity;
            color.a *= opacity;

            int renderData = BitUtil.SetHighLowBits((int) renderShape.shape.type, RenderTypeFill);

            switch (renderShape.shape.type) {
                case SVGXShapeType.Unset:
                    break;
                case SVGXShapeType.Text: {
                    Vector2 p0 = matrix.Transform(points[start]);
                    renderData = BitUtil.SetHighLowBits((int) renderShape.shape.type, RenderTypeText);

                    TextInfo textInfo = renderShape.textInfo;

                    CharInfo[] charInfos = textInfo.charInfos;
                    int charCount = textInfo.charCount;

                    SVGXTextStyle textStyle = textInfo.spanInfos[0].textStyle;


                    float outlineWidth = Mathf.Clamp01(textStyle.outlineWidth);
                    float outlineSoftness = textStyle.outlineSoftness;

                    Color32 underlayColor = Color.white;
                    float underlayOffsetX = 0;
                    float underlayOffsetY = 0;
                    float underlayDilate = 0;

                    Color32 glowColor = Color.green;
                    float glowOuter = textStyle.glowOuter;
                    float glowOffset = textStyle.glowOffset;

                    Vector4 glowAndRenderData = new Vector4(renderData, new StyleColor(glowColor).rgba, glowOuter, glowOffset);

                    int isStroke = 0;

                    Vector4 outline = new Vector4(outlineWidth, outlineSoftness, 0, 0);

                    Color textColor = textStyle.color;
                    Color outlineColor = textStyle.outlineColor;

                    for (int i = 0; i < charCount; i++) {
                        if (charInfos[i].character == ' ') continue;
                        Vector2 topLeft = charInfos[i].layoutTopLeft;
                        Vector2 bottomRight = charInfos[i].layoutBottomRight;
//                        topLeft.x = topLeft.x - 2.5f;
//                        bottomRight.x = bottomRight.x + 2.5f;
//                        topLeft.y = topLeft.y - 2.5f;
//                        bottomRight.y = bottomRight.y + 2.5f;

                        positionList.Add(new Vector3(p0.x + topLeft.x, -p0.y + -bottomRight.y, z)); // Bottom Left
                        positionList.Add(new Vector3(p0.x + topLeft.x, -p0.y + -topLeft.y, z)); // Top Left
                        positionList.Add(new Vector3(p0.x + bottomRight.x, -p0.y + -topLeft.y, z)); // Top Right
                        positionList.Add(new Vector3(p0.x + bottomRight.x, -p0.y + -bottomRight.y, z)); // Bottom Right

                        float x = charInfos[i].uv0.x; // - (5 / 1024f);
                        float y = charInfos[i].uv0.y; // - (5 / 1024f);
                        float x1 = charInfos[i].uv1.x; // + (5 / 1024f);
                        float y1 = charInfos[i].uv1.y; // + (5 / 1024f);

                        uv0List.Add(new Vector4(x, y, charInfos[i].uv2.x, isStroke));
                        uv0List.Add(new Vector4(x, y1, charInfos[i].uv2.x, isStroke));
                        uv0List.Add(new Vector4(x1, y1, charInfos[i].uv2.x, isStroke));
                        uv0List.Add(new Vector4(x1, y, charInfos[i].uv2.x, isStroke));

                        uv1List.Add(glowAndRenderData);
                        uv1List.Add(glowAndRenderData);
                        uv1List.Add(glowAndRenderData);
                        uv1List.Add(glowAndRenderData);

                        uv2List.Add(outline);
                        uv2List.Add(outline);
                        uv2List.Add(outline);
                        uv2List.Add(outline);

                        uv3List.Add(outlineColor);
                        uv3List.Add(outlineColor);
                        uv3List.Add(outlineColor);
                        uv3List.Add(outlineColor);

                        uv4List.Add(new Vector4());
                        uv4List.Add(new Vector4());
                        uv4List.Add(new Vector4());
                        uv4List.Add(new Vector4());

                        colorsList.Add(textColor);
                        colorsList.Add(textColor);
                        colorsList.Add(textColor);
                        colorsList.Add(textColor);

                        trianglesList.Add(triangleIndex + 0);
                        trianglesList.Add(triangleIndex + 1);
                        trianglesList.Add(triangleIndex + 2);
                        trianglesList.Add(triangleIndex + 2);
                        trianglesList.Add(triangleIndex + 3);
                        trianglesList.Add(triangleIndex + 0);

                        triangleIndex += 4;
                    }

                    break;
                }
                case SVGXShapeType.Ellipse:
                case SVGXShapeType.Circle:
                case SVGXShapeType.Rect: {
                    Vector2 pos = points[start + 0];
                    Vector2 wh = points[start + 1];

                    pos.x -= 1f;
                    pos.y -= 1f;
                    wh.x += 2f;
                    wh.y += 2f;
                    Vector2 p0 = matrix.Transform(new Vector2(pos.x, pos.y));
                    Vector2 p1 = matrix.Transform(new Vector2(pos.x + wh.x, pos.y));
                    Vector2 p2 = matrix.Transform(new Vector2(pos.x + wh.x, pos.y + wh.y));
                    Vector2 p3 = matrix.Transform(new Vector2(pos.x, pos.y + wh.y));

                    // we probably want to buffer all these sizes so our sdf doesn't cut off

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

                    uv0List.Add(new Vector4(0, 1, wh.x, wh.y));
                    uv0List.Add(new Vector4(1, 1, wh.x, wh.y));
                    uv0List.Add(new Vector4(1, 0, wh.x, wh.y));
                    uv0List.Add(new Vector4(0, 0, wh.x, wh.y));

                    uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));

                    uv2List.Add(new Vector4());
                    uv2List.Add(new Vector4());
                    uv2List.Add(new Vector4());
                    uv2List.Add(new Vector4());

                    uv3List.Add(new Vector4());
                    uv3List.Add(new Vector4());
                    uv3List.Add(new Vector4());
                    uv3List.Add(new Vector4());

                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());

                    colorsList.Add(color);
                    colorsList.Add(color);
                    colorsList.Add(color);
                    colorsList.Add(color);

                    triangleIndex = triIdx + 4;

                    break;
                }
                case SVGXShapeType.Path: {
                    // assume closed for now
                    // assume convex for now

                    throw new NotImplementedException();
                }
                case SVGXShapeType.RoundedRect: { // or other convex shape without holes

                    Vector2 pos = points[start + 0];
                    Vector2 wh = points[start + 1];

                    Vector2 borderRadiusXY = points[start + 2];
                    Vector2 borderRadiusZW = points[start + 3];

                    Vector2 p0 = matrix.Transform(new Vector2(pos.x, pos.y));
                    Vector2 p1 = matrix.Transform(new Vector2(pos.x + wh.x, pos.y));
                    Vector2 p2 = matrix.Transform(new Vector2(pos.x + wh.x, pos.y + wh.y));
                    Vector2 p3 = matrix.Transform(new Vector2(pos.x, pos.y + wh.y));

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

                    uv0List.Add(new Vector4(0, 1, wh.x, wh.y));
                    uv0List.Add(new Vector4(1, 1, wh.x, wh.y));
                    uv0List.Add(new Vector4(1, 0, wh.x, wh.y));
                    uv0List.Add(new Vector4(0, 0, wh.x, wh.y));

                    uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));
                    uv1List.Add(new Vector4(renderData, fillColorModes, gradientId, gradientDirection));

                    uv2List.Add(new Vector4(borderRadiusXY.x, borderRadiusXY.y, borderRadiusZW.x, borderRadiusZW.y));
                    uv2List.Add(new Vector4(borderRadiusXY.x, borderRadiusXY.y, borderRadiusZW.x, borderRadiusZW.y));
                    uv2List.Add(new Vector4(borderRadiusXY.x, borderRadiusXY.y, borderRadiusZW.x, borderRadiusZW.y));
                    uv2List.Add(new Vector4(borderRadiusXY.x, borderRadiusXY.y, borderRadiusZW.x, borderRadiusZW.y));

                    uv3List.Add(new Vector4());
                    uv3List.Add(new Vector4());
                    uv3List.Add(new Vector4());
                    uv3List.Add(new Vector4());

                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());

                    colorsList.Add(color);
                    colorsList.Add(color);
                    colorsList.Add(color);
                    colorsList.Add(color);

                    triangleIndex = triIdx + 4;

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal void CreateShadowVertices(Vector2[] points, SVGXRenderShape renderShape, GFX.GradientData gradientData, SVGXStyle style, SVGXMatrix matrix) {
            int start = renderShape.shape.pointRange.start;
            int end = renderShape.shape.pointRange.end;

            int triIdx = triangleIndex;
            float z = renderShape.zIndex;

            int renderData = BitUtil.SetHighLowBits((int) renderShape.shape.type, RenderTypeShadow);

            Color color = style.shadowColor;
            Color shadowTint = style.shadowTint;
            float shadowSoftnessX = style.shadowSoftnessX;
            float shadowSoftnessY = style.shadowSoftnessY;
            float shadowIntensity = style.shadowIntensity;

            switch (renderShape.shape.type) {
                case SVGXShapeType.Unset:
                    break;
                case SVGXShapeType.Rect:
                case SVGXShapeType.Ellipse:
                case SVGXShapeType.Circle:

                    Vector2 pos = points[start + 0];
                    Vector2 wh = points[start + 1];

                    pos.x += style.shadowOffsetX;
                    pos.y += style.shadowOffsetY;
                    pos -= wh * 0.2f;
                    wh += wh * 0.2f; // shader wants a buffer to blur in, user defines a fixed size.
                    // making the size 20% larger fixes expectations of user and shader

                    Vector2 p0 = matrix.Transform(new Vector2(pos.x, pos.y));
                    Vector2 p1 = matrix.Transform(new Vector2(pos.x + wh.x, pos.y));
                    Vector2 p2 = matrix.Transform(new Vector2(pos.x + wh.x, pos.y + wh.y));
                    Vector2 p3 = matrix.Transform(new Vector2(pos.x, pos.y + wh.y));

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

                    uv0List.Add(new Vector4(0, 1, wh.x, wh.y));
                    uv0List.Add(new Vector4(1, 1, wh.x, wh.y));
                    uv0List.Add(new Vector4(1, 0, wh.x, wh.y));
                    uv0List.Add(new Vector4(0, 0, wh.x, wh.y));

                    uv1List.Add(new Vector4(renderData, 0, 0, 0));
                    uv1List.Add(new Vector4(renderData, 0, 0, 0));
                    uv1List.Add(new Vector4(renderData, 0, 0, 0));
                    uv1List.Add(new Vector4(renderData, 0, 0, 0));

                    uv2List.Add(new Vector4(shadowSoftnessX, shadowSoftnessY, shadowIntensity, 0));
                    uv2List.Add(new Vector4(shadowSoftnessX, shadowSoftnessY, shadowIntensity, 0));
                    uv2List.Add(new Vector4(shadowSoftnessX, shadowSoftnessY, shadowIntensity, 0));
                    uv2List.Add(new Vector4(shadowSoftnessX, shadowSoftnessY, shadowIntensity, 0));

                    uv3List.Add(shadowTint);
                    uv3List.Add(shadowTint);
                    uv3List.Add(shadowTint);
                    uv3List.Add(shadowTint);

                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());
                    uv4List.Add(new Vector4());

                    colorsList.Add(color);
                    colorsList.Add(color);
                    colorsList.Add(color);
                    colorsList.Add(color);

                    triangleIndex = triIdx + 4;
                    break;
                case SVGXShapeType.RoundedRect:
                    break;
                case SVGXShapeType.Path:
                    break;
                case SVGXShapeType.Text:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool IsLeft(Vector2 a, Vector2 b, Vector2 c) {
            return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0;
        }


        public void GenerateStartCap2() { }

        // joins always work on vertex 1 and 3 from s0 and 0 and 2 from s1
        // this means we need to 'complete' the last segment geometry before creating
        // the join geometry and starting the next one
        // caps should handle the start / end of the first and last segments respectively

        public void GenerateBevelJoin(Segment s0, Segment s1, float strokeWidth) {
            float offset = strokeWidth * 0.5f;

            Vector2 p0 = s0.p0;
            Vector2 p1 = s0.p1;
            Vector2 p2 = s1.p0;
            Vector2 p3 = s1.p1;

            Vector2 toSegment1End = (p1 - p0).normalized;
            Vector2 toSegment2End = (p3 - p2).normalized;

            Vector2 segment1Perp = new Vector2(-toSegment1End.y, toSegment1End.x);
            Vector2 segment2Perp = new Vector2(-toSegment2End.y, toSegment2End.x);

            Vector2 v0 = p0 - (segment1Perp * offset);
            Vector2 v1 = p1 - (segment1Perp * offset);
            Vector2 v2 = p0 - (segment1Perp * offset);
            Vector2 v3 = p1 + (segment1Perp * offset);

            Vector2 v4 = p2 - (segment2Perp * offset);
            Vector2 v5 = p3 + (segment2Perp * offset);
            Vector2 v6 = p2 - (segment2Perp * offset);
            Vector2 v7 = p3 + (segment2Perp * offset);

            Vector2 prev = p0;
            Vector2 curr = p1;
            Vector2 next = p2;
            
            Vector2 miter = (segment1Perp + segment2Perp).normalized;
            float miterLength = offset / Vector2.Dot(miter, segment2Perp);
            
            // first we finish the geometry from start segment

            // need to compute the bevel point, first figure out if we are going left or right

            if (IsLeft(p0, p1, p3)) {
                v3 = p1 + (miter * miterLength);
                v6 = v3;
                
                Vector2 bevel0 = v1;
                Vector2 bevel1 = v3;
                Vector2 bevel2 = v4;
                
                AddVertex(v3, Color.blue, 3);
                AddVertex(v1, Color.blue, 1);
                
                CompleteQuad();

                AddVertex(bevel0, Color.yellow, -1);
                AddVertex(bevel1, Color.yellow, -1);
                AddVertex(bevel2, Color.yellow, -1);
                
                CompleteTriangle();
                
                AddVertex(v4, Color.red, 0);
                AddVertex(v6, Color.red, 2);
            }
            else {
                
                // v1 = normal offset position
                // v3 = miter position

                // v4 = normal offset position
                // v6 = v3 (miter position)
                // todo -- figure out this case
                v3 = p1 + (miter * miterLength);
                //v3 = v6;
                
                Vector2 bevel0 = v3;
                Vector2 bevel1 = v1;
                Vector2 bevel2 = v6;
                
                AddVertex(v3, Color.blue, 1);
                AddVertex(v1, Color.white, 3);
                
                CompleteQuad();

//                AddVertex(bevel0, Color.yellow, -1);
//                AddVertex(bevel1, Color.yellow, -1);
//                AddVertex(bevel2, Color.yellow, -1);
//                
//                CompleteTriangle();
                
                AddVertex(v3, Color.red, 0);
                AddVertex(v1, Color.red, 2);
                
            }

        }

        public struct Segment {

            public Vector2 p0;
            public Vector2 p1;

            public Segment(Vector2 p0, Vector2 p1) {
                this.p0 = p0;
                this.p1 = p1;
            }

        }

        public LightList<Segment> CreateSegments(LightList<Point> points, RangeInt range) {
            LightList<Segment> segments = new LightList<Segment>(range.length);
            for (int i = range.start; i < range.end - 1; i++) {
                segments.Add(new Segment(
                        points.Array[i + 0].position,
                        points.Array[i + 1].position
                    )
                );
            }

            return segments;
        }

        private void AddVertex(Vector2 position, Color color, int idx) {
            switch (idx) {
                case 0:
                    uv0List.Add(new Vector4(0, 1, 0, 0));
                    break;
                case 1:
                    uv0List.Add(new Vector4(1, 1, 0, 0));
                    break;
                case 2:
                    uv0List.Add(new Vector4(1, 0, 0, 0));
                    break;
                case 3: 
                    uv0List.Add(new Vector4(0, 0, 0, 0));
                    break;
                default:
                    uv0List.Add(new Vector4(0, 0, 0, 0));
                    break;
            }
            positionList.Add(new Vector3(position.x, -position.y, 500)); // todo -- set z
            colorsList.Add(color);
            uv1List.Add(new Vector4());
            uv2List.Add(new Vector4());
            uv3List.Add(new Vector4());
            uv4List.Add(new Vector4());
        }

        private void CompleteQuad() {

            // assume vertex 0 and 2 are added first
            // then vertex 1 and 3
            
            int triIdx = triangleIndex;

            trianglesList.Add(triIdx + 0);
            trianglesList.Add(triIdx + 1);
            trianglesList.Add(triIdx + 2);
            trianglesList.Add(triIdx + 2);
            trianglesList.Add(triIdx + 1);
            trianglesList.Add(triIdx + 3);

            triangleIndex = triIdx + 4;
            
        }
        
        private void CompleteTriangle() {

            int triIdx = triangleIndex;

            trianglesList.Add(triIdx + 0);
            trianglesList.Add(triIdx + 1);
            trianglesList.Add(triIdx + 2);

            triangleIndex = triIdx + 3;
            
        }
        
        public void GenerateStrokeBody(LightList<Point> points, SVGXShape shape, float strokeWidth, LineJoin join) {
            int start = shape.pointRange.start;
            int end = shape.pointRange.end;


            Point[] pointArray = points.Array;

            LightList<Segment> segments = CreateSegments(points, shape.pointRange);

            for (int i = 0; i < segments.Count - 1; i++) {
                GenerateBevelJoin(segments[i], segments[i + 1], strokeWidth);
            }

//            for (int i = start; i < end - 1; i++) {
//                Vector2 p0 = pointArray[i + 0].position;
//                Vector2 p1 = pointArray[i + 1].position;
//
//                continue;
//
//                Vector2 toNext = (p1 - p0).normalized;
//                Vector2 toNextPerp = new Vector2(-toNext.y, toNext.x);
//
//                float dist = (p1 - p0).magnitude;
//                float offset = strokeWidth * 0.5f;
//
//                Vector2 v0 = p0; // + (toNextPerp * (-offset));
//                Vector2 v1 = p1; //0 + (toNext * dist);
//                Vector2 v2 = p0; // + (toNextPerp * (offset));
//                Vector2 v3 = p1; //0 + (toNext * dist) + (toNextPerp * (offset));
//
//                if (i == start) {
//                    Vector2 curr = p1;
//                    Vector2 next = pointArray[i + 2].position;
//                    Vector2 __toNext = (next - curr).normalized;
//                    Vector2 __toCurr = (p1 - p0).normalized;
//                    Vector2 __toCurrPerp = new Vector2(-__toCurr.y, __toCurr.x);
//                    Vector2 __toNextPerp = new Vector2(-__toNext.y, __toNext.x);
//                    Vector2 miter = (__toCurrPerp + __toNextPerp).normalized;
//
//                    float miterLength = (strokeWidth * 0.5f) / Vector2.Dot(miter, toNextPerp);
//
//                    v0 -= (toNextPerp * offset);
//                    v1 -= (miter * miterLength);
//                    v2 += (toNextPerp * offset);
//                    v3 += (miter * miterLength);
//
//                    // walk miter back along itself to find the proper bevel coordinate
//                    if (miterLength > 30) {
//                        Vector2 toV0 = (v0 - v1).normalized;
//                        v1 += (toV0 * (miterLength - 30));
//                    }
//
//                    // if miter line intersects [v0, v2] we need to limit v3
//                }
//                else if (i == end - 1) { }
//                else {
//                    Vector2 prev = pointArray[i - 1].position;
//                    Vector2 curr = p0;
//                    Vector2 next = p1;
//                    Vector2 toCurr = (curr - prev).normalized;
//                    Vector2 miter = (toCurr + toNext).normalized;
//                    Vector2 n1 = toNextPerp;
//                    Vector2 n0 = new Vector2(-toCurr.y, toCurr.x);
//                    float miterLength = (strokeWidth * 0.5f) / Vector2.Dot(miter, n1);
//                    if (IsLeft(p0, p1, pointArray[i + 2].position)) { }
//                    else { }
//
//                    //v0 += (miter * miterLength);
//                    //v1 += (miter * miterLength);
////                    v2 += (miter * miterLength);
////                    v3 += (miter * miterLength);
//                }
//
//                positionList.Add(new Vector3(v0.x, -v0.y, 65534));
//                positionList.Add(new Vector3(v1.x, -v1.y, 65534));
//                positionList.Add(new Vector3(v2.x, -v2.y, 65534));
//                positionList.Add(new Vector3(v3.x, -v3.y, 65534));
//
//                uv0List.Add(new Vector4(0, 1, 0, 0));
//                uv0List.Add(new Vector4(1, 1, 0, 0));
//                uv0List.Add(new Vector4(1, 0, 0, 0));
//                uv0List.Add(new Vector4(0, 0, 0, 0));
//
//                uv1List.Add(new Vector4(0, 0, 0, 0));
//                uv1List.Add(new Vector4(0, 0, 0, 0));
//                uv1List.Add(new Vector4(0, 0, 0, 0));
//                uv1List.Add(new Vector4(0, 0, 0, 0));
//
//                uv2List.Add(new Vector4(0, 0, 0, 0));
//                uv2List.Add(new Vector4(0, 0, 0, 0));
//                uv2List.Add(new Vector4(0, 0, 0, 0));
//                uv2List.Add(new Vector4(0, 0, 0, 0));
//
//                uv3List.Add(new Vector4());
//                uv3List.Add(new Vector4());
//                uv3List.Add(new Vector4());
//                uv3List.Add(new Vector4());
//
//                uv4List.Add(new Vector4());
//                uv4List.Add(new Vector4());
//                uv4List.Add(new Vector4());
//                uv4List.Add(new Vector4());
//
//                if (i != start) {
//                    colorsList.Add(Color.clear);
//                    colorsList.Add(Color.clear);
//                    colorsList.Add(Color.clear);
//                    colorsList.Add(Color.clear);
//                }
//
//                else {
//                    colorsList.Add(Color.blue);
//                    colorsList.Add(Color.white);
//                    colorsList.Add(Color.red);
//                    colorsList.Add(Color.black);
//                }
//
//                int triIdx = triangleIndex;
//
//                trianglesList.Add(triIdx + 0);
//                trianglesList.Add(triIdx + 1);
//                trianglesList.Add(triIdx + 2);
//                trianglesList.Add(triIdx + 2);
//                trianglesList.Add(triIdx + 1);
//                trianglesList.Add(triIdx + 3);
//
//                triangleIndex = triIdx + 4;
//            }
        }

        public void GenerateStartCap(LightList<Point> points, SVGXShape shape, float strokeWidth, LineCap cap) {

            Vector2 p0 = points.Array[shape.pointRange.start + 0].position;
            Vector2 p1 = points.Array[shape.pointRange.start + 1].position;

            Vector2 toNext = (p1 - p0).normalized;
            Vector2 toNextPerp = new Vector2(-toNext.y, toNext.x);

            float dist = strokeWidth * 0.5f;

            if (cap == LineCap.Butt) {
                dist = 2f;
            }

            Vector2 v0 = p0 + (toNextPerp * (dist));
            Vector2 v1 = p0 + (toNextPerp * (-dist));
            Vector2 v2 = p0 - (toNext * dist) + (toNextPerp * (-dist));
            Vector2 v3 = p0 - (toNext * dist) + (toNextPerp * (dist));

            positionList.Add(new Vector3(v0.x, -v0.y, 65534));
            positionList.Add(new Vector3(v1.x, -v1.y, 65534));
            positionList.Add(new Vector3(v2.x, -v2.y, 65534));
            positionList.Add(new Vector3(v3.x, -v3.y, 65534));

            uv0List.Add(new Vector4(0, 1, 0, 0));
            uv0List.Add(new Vector4(1, 1, 0, 0));
            uv0List.Add(new Vector4(1, 0, 0, 0));
            uv0List.Add(new Vector4(0, 0, 0, 0));

            uv1List.Add(new Vector4(1, (int) cap, 0, 0));
            uv1List.Add(new Vector4(1, (int) cap, 0, 0));
            uv1List.Add(new Vector4(1, (int) cap, 0, 0));
            uv1List.Add(new Vector4(1, (int) cap, 0, 0));

            uv2List.Add(new Vector4(0, 0, 0, 0));
            uv2List.Add(new Vector4(0, 0, 0, 0));
            uv2List.Add(new Vector4(0, 0, 0, 0));
            uv2List.Add(new Vector4(0, 0, 0, 0));

            uv3List.Add(new Vector4());
            uv3List.Add(new Vector4());
            uv3List.Add(new Vector4());
            uv3List.Add(new Vector4());

            uv4List.Add(new Vector4());
            uv4List.Add(new Vector4());
            uv4List.Add(new Vector4());
            uv4List.Add(new Vector4());

            colorsList.Add(Color.green);
            colorsList.Add(Color.green);
            colorsList.Add(Color.green);
            colorsList.Add(Color.green);

            int triIdx = triangleIndex;

            trianglesList.Add(triIdx + 0);
            trianglesList.Add(triIdx + 1);
            trianglesList.Add(triIdx + 2);
            trianglesList.Add(triIdx + 2);
            trianglesList.Add(triIdx + 3);
            trianglesList.Add(triIdx + 0);

            triangleIndex = triIdx + 4;
            
            AddVertex(v0, Color.red, 0);
            AddVertex(v1, Color.red, 2);
            
        }

        public void GenerateEndCap(LightList<Point> points, SVGXShape shape, float strokeWidth, LineCap cap) {

            Vector2 p0 = points.Array[shape.pointRange.end - 2].position;
            Vector2 p1 = points.Array[shape.pointRange.end - 1].position;

            Vector2 toPrev = (p0 - p1).normalized;
            Vector2 toPrevPerp = new Vector2(-toPrev.y, toPrev.x);

            float dist = strokeWidth * 0.5f;

            if (cap == LineCap.Butt) {
                dist = 2f;
            }

            Vector2 v0 = p1 + (toPrevPerp * (dist));
            Vector2 v1 = p1 + (toPrevPerp * (-dist));
            Vector2 v2 = p1 - (toPrev * dist) + (toPrevPerp * (-dist));
            Vector2 v3 = p1 - (toPrev * dist) + (toPrevPerp * (dist));

            AddVertex(v3, Color.blue, 3);
            AddVertex(v1, Color.blue, 1);
            
            CompleteQuad();
            
            positionList.Add(new Vector3(v0.x, -v0.y, 65534));
            positionList.Add(new Vector3(v1.x, -v1.y, 65534));
            positionList.Add(new Vector3(v2.x, -v2.y, 65534));
            positionList.Add(new Vector3(v3.x, -v3.y, 65534));

            uv0List.Add(new Vector4(0, 1, 0, 0));
            uv0List.Add(new Vector4(1, 1, 0, 0));
            uv0List.Add(new Vector4(1, 0, 0, 0));
            uv0List.Add(new Vector4(0, 0, 0, 0));

            uv1List.Add(new Vector4(1, (int) cap, 0, 0));
            uv1List.Add(new Vector4(1, (int) cap, 0, 0));
            uv1List.Add(new Vector4(1, (int) cap, 0, 0));
            uv1List.Add(new Vector4(1, (int) cap, 0, 0));

            uv2List.Add(new Vector4(0, 0, 0, 0));
            uv2List.Add(new Vector4(0, 0, 0, 0));
            uv2List.Add(new Vector4(0, 0, 0, 0));
            uv2List.Add(new Vector4(0, 0, 0, 0));

            uv3List.Add(new Vector4());
            uv3List.Add(new Vector4());
            uv3List.Add(new Vector4());
            uv3List.Add(new Vector4());

            uv4List.Add(new Vector4());
            uv4List.Add(new Vector4());
            uv4List.Add(new Vector4());
            uv4List.Add(new Vector4());

            colorsList.Add(Color.green);
            colorsList.Add(Color.green);
            colorsList.Add(Color.green);
            colorsList.Add(Color.green);

            int triIdx = triangleIndex;

            trianglesList.Add(triIdx + 0);
            trianglesList.Add(triIdx + 1);
            trianglesList.Add(triIdx + 2);
            trianglesList.Add(triIdx + 2);
            trianglesList.Add(triIdx + 3);
            trianglesList.Add(triIdx + 0);

            triangleIndex = triIdx + 4;
        }

    }

}