using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Text;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;
using Vertigo;

namespace UIForia.Rendering {

    public class TextRenderBox : RenderBox {

        private TextInfoOld textInfoOld;
        private int lastGeometryVersion = -1;
        private FontData fontData;

        private readonly UIForiaGeometry geometry;
        private readonly StructList<GeometryRange> ranges;
        private bool shouldUpdateMaterialProperties;

        public TextRenderBox() {
            this.uniqueId = "UIForia::TextRenderBox";
            this.geometry = new UIForiaGeometry();
            this.ranges = new StructList<GeometryRange>(4);
        }

        public override void OnInitialize() {
            base.OnInitialize();
            // textSpan = ((UITextElement) element).textSpan;
            lastGeometryVersion = -1;
            geometry.Clear();
            ranges.Clear();
        }

        private void UpdateGeometry() {
            // todo -- pool this
            geometry.Clear();
            ranges.size = 0;

            CharInfo[] chars = default; //textSpan.charInfoList.array;

            int charCount = default; //textSpan.charInfoList.size;

            int vertIdx = 0;
            int triIndex = 0;

            int currentLineIndex = -1;

            GeometryRange range = new GeometryRange();

            int renderedCharCount = 0;

            for (int i = 0; i < charCount; i++) {
                ref CharInfo geo = ref chars[i];

                if (geo.visible) {
                    if (currentLineIndex == -1) {
                        currentLineIndex = geo.lineIndex;
                    }

                    renderedCharCount++;
                }
            }

            if (currentLineIndex == -1) {
                // nothing to draw
                return;
            }

            geometry.EnsureCapacity(renderedCharCount * 4, renderedCharCount * 6);

            Vector3[] positions = geometry.positionList.array;
            Vector4[] texCoord0 = geometry.texCoordList0.array;
            Vector4[] texCoord1 = geometry.texCoordList1.array;
            int[] triangles = geometry.triangleList.array;

            Vector2 faceTextureUVTopLeft = new Vector2(0, 1);
            Vector2 faceTextureUVBottomRight = new Vector2(1, 0);

            for (int i = 0; i < charCount; i++) {
                ref CharInfo geo = ref chars[i];

                if (!geo.visible) continue;

                if (geo.lineIndex != currentLineIndex) {
                    range.vertexEnd = vertIdx;
                    range.triangleEnd = triIndex;
                    ranges.Add(range);
                    range = new GeometryRange();
                    range.vertexStart = vertIdx;
                    range.triangleStart = triIndex;
                    currentLineIndex = geo.lineIndex;
                }

                ref Vector3 p0 = ref positions[vertIdx + 0];
                ref Vector3 p1 = ref positions[vertIdx + 1];
                ref Vector3 p2 = ref positions[vertIdx + 2];
                ref Vector3 p3 = ref positions[vertIdx + 3];

                ref Vector4 uv0 = ref texCoord0[vertIdx + 0];
                ref Vector4 uv1 = ref texCoord0[vertIdx + 1];
                ref Vector4 uv2 = ref texCoord0[vertIdx + 2];
                ref Vector4 uv3 = ref texCoord0[vertIdx + 3];

                float charX = geo.wordLayoutX + geo.topLeft.x;
                float charY = geo.wordLayoutY + geo.topLeft.y;

                float charWidth = geo.bottomRight.x - geo.topLeft.x;
                float charHeight = geo.bottomRight.y - geo.topLeft.y;

                p0.x = charX + geo.topShear;
                p0.y = -charY;
                p0.z = 0;

                p1.x = charX + charWidth + geo.topShear;
                p1.y = -charY;
                p1.z = 0;

                p2.x = charX + charWidth + geo.bottomShear;
                p2.y = -(charY + charHeight);
                p2.z = 0;

                p3.x = charX + geo.bottomShear;
                p3.y = -(charY + charHeight);
                p3.z = 0;

                uv0.x = faceTextureUVTopLeft.x;
                uv0.y = faceTextureUVBottomRight.y;
                uv0.z = geo.topLeftUV.x;
                uv0.w = geo.bottomRightUV.y;

                uv1.x = faceTextureUVBottomRight.x;
                uv1.y = faceTextureUVBottomRight.y;
                uv1.z = geo.bottomRightUV.x;
                uv1.w = geo.bottomRightUV.y;

                uv2.x = faceTextureUVBottomRight.x;
                uv2.y = faceTextureUVTopLeft.y;
                uv2.z = geo.bottomRightUV.x;
                uv2.w = geo.topLeftUV.y;

                uv2.x = faceTextureUVTopLeft.x;
                uv2.y = faceTextureUVTopLeft.y;
                uv3.z = geo.topLeftUV.x;
                uv3.w = geo.topLeftUV.y;

                uv0 = ref texCoord1[vertIdx + 0];
                uv1 = ref texCoord1[vertIdx + 1];
                uv2 = ref texCoord1[vertIdx + 2];
                uv3 = ref texCoord1[vertIdx + 3];

                uv0.x = geo.scale;
                uv1.x = geo.scale;
                uv2.x = geo.scale;
                uv3.x = geo.scale;

                triangles[triIndex + 0] = vertIdx + 0;
                triangles[triIndex + 1] = vertIdx + 1;
                triangles[triIndex + 2] = vertIdx + 2;
                triangles[triIndex + 3] = vertIdx + 2;
                triangles[triIndex + 4] = vertIdx + 3;
                triangles[triIndex + 5] = vertIdx + 0;

                vertIdx += 4;
                triIndex += 6;
            }

            range.vertexEnd = vertIdx;
            range.triangleEnd = triIndex;
            ranges.Add(range);
            geometry.positionList.size = vertIdx;
            geometry.texCoordList0.size = vertIdx;
            geometry.texCoordList1.size = vertIdx;
            geometry.triangleList.size = triIndex;
        }

        public override void OnStylePropertyChanged(StyleProperty[] propertyList, int propertyCount) {
            base.OnStylePropertyChanged(propertyList, propertyCount);

            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref propertyList[i];
                switch (property.propertyId) {
                    case StylePropertyId.TextFontAsset:
                    case StylePropertyId.Opacity:
                    case StylePropertyId.TextColor:
                    case StylePropertyId.TextFaceDilate:
                    case StylePropertyId.TextUnderlayColor:
                    case StylePropertyId.TextUnderlayX:
                    case StylePropertyId.TextUnderlayY:
                    case StylePropertyId.TextUnderlaySoftness:
                    case StylePropertyId.TextUnderlayDilate:
                    case StylePropertyId.TextFontStyle:
                    case StylePropertyId.TextGlowColor:
                    case StylePropertyId.TextGlowInner:
                    case StylePropertyId.TextGlowOuter:
                    case StylePropertyId.TextGlowOffset:
                        shouldUpdateMaterialProperties = true;
                        return;
                }
            }
        }

        private void UpdateFontData() {
            FontAsset fontAsset = FontAsset.defaultFontAsset;
            fontData.fontAsset = fontAsset;
            float weight = (fontAsset.weightNormal > fontAsset.weightBold ? fontAsset.weightNormal : fontAsset.weightBold) / 4f;
            float ratioA_t = math.max(1, weight); // + faceDilate + outlineThickness + outlineSoftness);
            float ratioA = (fontAsset.gradientScale - 1f) / (fontAsset.gradientScale * ratioA_t);
            fontData.gradientScale = fontAsset.gradientScale;
            fontData.scaleRatioA = ratioA;
            fontData.scaleRatioB = 0;
            fontData.scaleRatioC = 0;
            fontData.textureWidth = fontAsset.atlas.width;
            fontData.textureHeight = fontAsset.atlas.height;
        }

    }

}