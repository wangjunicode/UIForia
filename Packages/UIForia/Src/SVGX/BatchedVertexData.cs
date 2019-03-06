using System;
using System.Collections.Generic;
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

        private void GenerateSegmentBodies(Vector2[] points, int count, Color color, float strokeWidth, float z) {
            const int join = 0;

            int renderData = BitUtil.SetHighLowBits((int)SVGXShapeType.Path, RenderTypeStroke);

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

                        positionList.Add(new Vector3(p0.x + topLeft.x, -p0.y + -bottomRight.y, z)); // Bottom Left
                        positionList.Add(new Vector3(p0.x + topLeft.x, -p0.y + -topLeft.y, z)); // Top Left
                        positionList.Add(new Vector3(p0.x + bottomRight.x, -p0.y + -topLeft.y, z)); // Top Right
                        positionList.Add(new Vector3(p0.x + bottomRight.x, -p0.y + -bottomRight.y, z)); // Bottom Right

                        uv0List.Add(new Vector4(charInfos[i].uv0.x, charInfos[i].uv0.y, charInfos[i].uv2.x, isStroke));
                        uv0List.Add(new Vector4(charInfos[i].uv0.x, charInfos[i].uv1.y, charInfos[i].uv2.x, isStroke));
                        uv0List.Add(new Vector4(charInfos[i].uv1.x, charInfos[i].uv1.y, charInfos[i].uv2.x, isStroke));
                        uv0List.Add(new Vector4(charInfos[i].uv1.x, charInfos[i].uv0.y, charInfos[i].uv2.x, isStroke));

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

    }

}