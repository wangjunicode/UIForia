using UIForia.Graphics.ShapeKit;
using Unity.Mathematics;
using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public partial struct ShapeKit {

        public void AddRect(ref UIVertexHelper vh, Rect rect, Color32 color) {
            float width = rect.width;
            float height = rect.height;
            float2 center = rect.center;
            center.y = -center.y;
            AddRectVertRing(ref vh, center, width, height, color, width, height);
            AddRectQuadIndices(ref vh);
        }
        
        public void AddRect(ref UIVertexHelper vh, Vector2 center, float width, float height, Color32 color) {
            AddRectVertRing(ref vh, center, width, height, color, width, height);
            AddRectQuadIndices(ref vh);
        }

        public void AddRect(ref UIVertexHelper vh, Vector2 center, float width, float height, Color32 color, EdgeGradientData edgeGradientData) {

            width += edgeGradientData.shadowOffset * 2.0f;
            height += edgeGradientData.shadowOffset * 2.0f;

            float innerOffset = Mathf.Min(width, height) * (1.0f - edgeGradientData.innerScale);

            AddRectVertRing(ref vh, center, width - innerOffset, height - innerOffset, color, width, height);

            AddRectQuadIndices(ref vh);

            if (edgeGradientData.isActive) {
                color.a = 0;

                GeoUtils.AddOffset(ref width, ref height, edgeGradientData.sizeAdd);

                AddRectVertRing(ref vh, center, width, height, color, width - edgeGradientData.sizeAdd * 2.0f, height - edgeGradientData.sizeAdd * 2.0f, true);

            }
        }

        private static void AddRectRing(ref UIVertexHelper vh, OutlineProperties outlineProperties, Vector2 center, float width, float height, Color32 color, EdgeGradientData edgeGradientData) {
            byte alpha = color.a;
            float outerDistance = GetOuterDistance(outlineProperties);
            float centerDistance = GetCenterDistance(outlineProperties);

            float fullWidth = width + outerDistance * 2.0f;
            float fullHeight = height + outerDistance * 2.0f;

            width += centerDistance * 2.0f;
            height += centerDistance * 2.0f;
            float halfLineWidth = outlineProperties.weight * 0.5f;

            float halfLineWeightOffset = halfLineWidth * 2.0f + edgeGradientData.shadowOffset;
            float halfLineWeightInnerOffset = halfLineWeightOffset * edgeGradientData.innerScale;

            if (edgeGradientData.isActive) {
                color.a = 0;

                AddRectVertRing(ref vh, center, width - halfLineWeightOffset - edgeGradientData.sizeAdd, height - halfLineWeightOffset - edgeGradientData.sizeAdd, color, fullWidth, fullHeight);

                color.a = alpha;
            }

            AddRectVertRing(ref vh, center, width - halfLineWeightInnerOffset, height - halfLineWeightInnerOffset, color, fullWidth, fullHeight, edgeGradientData.isActive);

            AddRectVertRing(ref vh, center, width + halfLineWeightInnerOffset, height + halfLineWeightInnerOffset, color, fullWidth, fullHeight, true);

            if (edgeGradientData.isActive) {
                color.a = 0;
                AddRectVertRing(ref vh, center, width + halfLineWeightOffset + edgeGradientData.sizeAdd, height + halfLineWeightOffset + edgeGradientData.sizeAdd, color, fullWidth, fullHeight, true);
            }
        }

        private static void AddRectVertRing(ref UIVertexHelper vh, Vector2 center, float width, float height, Color32 color, float totalWidth, float totalHeight, bool addRingIndices = false) {
            float uvXInset = 0.5f - width / totalWidth * 0.5f;
            float uvYInset = 0.5f - height / totalHeight * 0.5f;

            Vector3 tmpPos = default;
            Vector2 tmpUVPos = default;

            // TL
            tmpPos.x = center.x - width * 0.5f;
            tmpPos.y = center.y + height * 0.5f;
            tmpUVPos.x = uvXInset;
            tmpUVPos.y = 1.0f - uvYInset;
            vh.AddVert(tmpPos, color, tmpUVPos);

            // TR
            tmpPos.x += width;
            tmpUVPos.x = 1.0f - uvXInset;
            vh.AddVert(tmpPos, color, tmpUVPos);

            // BR
            tmpPos.y -= height;
            tmpUVPos.y = uvYInset;
            vh.AddVert(tmpPos, color, tmpUVPos);

            // BL
            tmpPos.x -= width;
            tmpUVPos.x = uvXInset;
            vh.AddVert(tmpPos, color, tmpUVPos);

            if (addRingIndices) {
                int baseIndex = vh.currentVertCount - 8;

                vh.AddTriangle(baseIndex + 4, baseIndex + 5, baseIndex);
                vh.AddTriangle(baseIndex, baseIndex + 5, baseIndex + 1);

                vh.AddTriangle(baseIndex + 1, baseIndex + 5, baseIndex + 6);
                vh.AddTriangle(baseIndex + 1, baseIndex + 6, baseIndex + 2);

                vh.AddTriangle(baseIndex + 2, baseIndex + 6, baseIndex + 7);
                vh.AddTriangle(baseIndex + 7, baseIndex + 3, baseIndex + 2);

                vh.AddTriangle(baseIndex + 4, baseIndex + 3, baseIndex + 7);
                vh.AddTriangle(baseIndex + 4, baseIndex, baseIndex + 3);
            }
        }

        private static void AddRectQuadIndices(ref UIVertexHelper vh) {
            int baseIndex = vh.currentVertCount - 4;

            vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 3);
            vh.AddTriangle(baseIndex + 3, baseIndex + 1, baseIndex + 2);
        }

        public void AddVerticalTwoColorRect(ref UIVertexHelper vh, Vector3 topLeft, float height, float width, Color32 topColor, Color32 bottomColor, Vector2 uv) {
            int numVertices = vh.currentVertCount;

            vh.AddVert(topLeft, topColor, uv); // TL
            vh.AddVert(topLeft + GeoUtils.RightV3 * width, topColor, uv); // TR
            vh.AddVert(topLeft + GeoUtils.DownV3 * height, bottomColor, uv); // BL
            vh.AddVert(topLeft + GeoUtils.RightV3 * width + GeoUtils.DownV3 * height, bottomColor, uv); // BR

            vh.AddTriangle(numVertices, numVertices + 1, numVertices + 2);
            vh.AddTriangle(numVertices + 2, numVertices + 1, numVertices + 3);
        }

        public void AddHorizontalTwoColorRect(ref UIVertexHelper vh, Vector3 topLeft, float height, float width, Color32 leftColor, Color32 rightColor, Vector2 uv) {
            int numVertices = vh.currentVertCount;

            vh.AddVert(topLeft, leftColor, uv); // TL
            vh.AddVert(topLeft + GeoUtils.RightV3 * width, rightColor, uv); // TR
            vh.AddVert(topLeft + GeoUtils.DownV3 * height, leftColor, uv); // BL
            vh.AddVert(topLeft + GeoUtils.RightV3 * width + GeoUtils.DownV3 * height, rightColor, uv); // BR

            vh.AddTriangle(numVertices, numVertices + 1, numVertices + 2);
            vh.AddTriangle(numVertices + 2, numVertices + 1, numVertices + 3);
        }

    }

}