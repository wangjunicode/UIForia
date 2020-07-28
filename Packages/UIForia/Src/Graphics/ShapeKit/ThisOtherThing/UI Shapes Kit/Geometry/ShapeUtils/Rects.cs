using UIForia.Graphics.ShapeKit;
using Unity.Mathematics;
using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    internal partial struct ShapeKit {

        public void AddRect(ref UIVertexHelper vh, Rect rect, Color32 color) {
            AddRect(ref vh, rect.x, rect.y, rect.width, rect.height, color);
        }

        public void AddRect(ref UIVertexHelper vh, float2 position, float width, float height, Color32 color) {
            AddRect(ref vh, position.x, position.y, width, height, color);
        }

        public void AddQuad(ref UIVertexHelper vh, float x, float y, float width, float height, Color32 color) {
            width += edgeGradientData.shadowOffset * 2.0f;
            height += edgeGradientData.shadowOffset * 2.0f;

            float2 center = default;
            center.x = x + (width * 0.5f);
            center.y = -(y + (height * 0.5f));

            float innerOffset = (width < height ? width : height) * (1.0f - edgeGradientData.innerScale);

            AddRectVertRing(ref vh, center, width - innerOffset, height - innerOffset, color, width, height);

            AddRectQuadIndices(ref vh);
        }

        public void AddRect(ref UIVertexHelper vh, float x, float y, float width, float height, Color32 color) {

            width += edgeGradientData.shadowOffset * 2.0f;
            height += edgeGradientData.shadowOffset * 2.0f;

            float2 center = default;
            center.x = x + (width * 0.5f);
            center.y = -(y + (height * 0.5f));

            float innerOffset = (width < height ? width : height) * (1.0f - edgeGradientData.innerScale);

            AddRectVertRing(ref vh, center, width - innerOffset, height - innerOffset, color, width, height);

            AddRectQuadIndices(ref vh);

            if (edgeGradientData.isActive) {
                color.a = 0;

                width += edgeGradientData.sizeAdd * 2.0f;
                height += edgeGradientData.sizeAdd * 2.0f;

                AddRectVertRing(ref vh, center, width, height, color, width - edgeGradientData.sizeAdd * 2.0f, height - edgeGradientData.sizeAdd * 2.0f, true);

            }
        }

        private void AddRectRing(ref UIVertexHelper vh, OutlineProperties outlineProperties, float2 center, float width, float height, Color32 color) {
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

        private unsafe static void AddRectVertRing(ref UIVertexHelper vh, float2 center, float width, float height, Color32 color, float totalWidth, float totalHeight, bool addRingIndices = false) {
            float uvXInset = 0.5f - width / totalWidth * 0.5f;
            float uvYInset = 0.5f - height / totalHeight * 0.5f;

            float3 tmpPos = default;
            float4 tmpUVPos = default;

            // TL
            tmpPos.x = center.x - width * 0.5f;
            tmpPos.y = center.y + height * 0.5f;
            tmpUVPos.x = uvXInset;
            tmpUVPos.y = 1.0f - uvYInset;

            vh.EnsureAdditionalVertexCapacity(4);
            
            // int vCount = vh.currentVertCount;
            //vh.positions[vCount] = tmpPos;
            //vh.texCoord[vCount] = tmpUVPos;
            //vh.colors[vCount] = color;
            //
            vh.AddVert(tmpPos, color, tmpUVPos);

            // TR
            tmpPos.x += width;
            tmpUVPos.x = 1.0f - uvXInset;
            vh.AddVert(tmpPos, color, tmpUVPos);
            // vh.positions[vCount + 1] = tmpPos;
            // vh.texCoord[vCount + 1] = tmpUVPos;
            // vh.colors[vCount + 1] = color;
            
            // BR
            tmpPos.y -= height;
            tmpUVPos.y = uvYInset;
            vh.AddVert(tmpPos, color, tmpUVPos);
            //vh.positions[vCount + 2] = tmpPos;
            //vh.texCoord[vCount + 2] = tmpUVPos;
            //vh.colors[vCount + 2] = color;
            
            // BL
            tmpPos.x -= width;
            tmpUVPos.x = uvXInset;
             vh.AddVert(tmpPos, color, tmpUVPos);
            //vh.positions[vCount + 3] = tmpPos;
            //vh.texCoord[vCount + 3] = tmpUVPos;
            //vh.colors[vCount + 3] = color;
            
            //vh.AddVertexCount(4);
            
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

        public void AddVerticalTwoColorRect(ref UIVertexHelper vh, float3 topLeft, float height, float width, Color32 topColor, Color32 bottomColor, float2 uv) {
            int numVertices = vh.currentVertCount;

            vh.AddVert(topLeft, topColor, uv); // TL
            vh.AddVert(topLeft + GeoUtils.RightV3 * width, topColor, uv); // TR
            vh.AddVert(topLeft + GeoUtils.DownV3 * height, bottomColor, uv); // BL
            vh.AddVert(topLeft + GeoUtils.RightV3 * width + GeoUtils.DownV3 * height, bottomColor, uv); // BR

            vh.AddTriangle(numVertices, numVertices + 1, numVertices + 2);
            vh.AddTriangle(numVertices + 2, numVertices + 1, numVertices + 3);
        }

        public void AddHorizontalTwoColorRect(ref UIVertexHelper vh, float3 topLeft, float height, float width, Color32 leftColor, Color32 rightColor, float2 uv) {
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