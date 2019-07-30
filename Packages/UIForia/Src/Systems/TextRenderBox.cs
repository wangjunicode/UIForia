using UIForia.Elements;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using TextInfo = System.Globalization.TextInfo;

namespace UIForia.Rendering {

    public class TextSpanRenderBox { }

    public class TextRenderBox : RenderBox {

        private TextInfo textInfo;
        private TextSpan textSpan;
        private int lastGeometryVersion = -1;
        private FontData fontData;

        private readonly UIForiaGeometry geometry;
        private readonly StructList<GeometryRange> ranges;
        private bool shouldUpdateMaterialProperties;

        public override Rect RenderBounds => new Rect(
            0, 0,
            element.layoutResult.actualSize.width,
            element.layoutResult.actualSize.height
        );

        public TextRenderBox() {
            this.uniqueId = "UIForia::TextRenderBox";
            this.geometry = new UIForiaGeometry();
            this.ranges = new StructList<GeometryRange>(4);
        }

        public override void OnInitialize() {
            textSpan = ((UITextElement) element).textSpan;
            lastGeometryVersion = -1;
            geometry.Clear();
            ranges.Clear();
        }

        private void UpdateGeometry() {
            // todo -- pool this
            geometry.Clear();
            ranges.size = 0;

            CharInfo2[] chars = textSpan.charInfoList.array;

            int charCount = textSpan.charInfoList.size;

            int vertIdx = 0;
            int triIndex = 0;

            int currentLineIndex = -1;
            
            GeometryRange range = new GeometryRange();

            int renderedCharCount = 0;

            for (int i = 0; i < charCount; i++) {
                ref CharInfo2 geo = ref chars[i];

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
                ref CharInfo2 geo = ref chars[i];

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

                float charX = geo.layoutX + geo.topLeft.x;
                float charY = geo.layoutY + geo.topLeft.y;

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

                uv0.x = faceTextureUVBottomRight.x;
                uv0.y = faceTextureUVBottomRight.y;
                uv1.z = geo.bottomRightUV.x;
                uv1.w = geo.bottomRightUV.y;

                uv0.x = faceTextureUVBottomRight.x;
                uv0.y = faceTextureUVTopLeft.y;
                uv2.z = geo.bottomRightUV.x;
                uv2.w = geo.topLeftUV.y;

                uv0.x = faceTextureUVTopLeft.x;
                uv0.y = faceTextureUVTopLeft.y;
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


        public override void OnStylePropertyChanged(StructList<StyleProperty> propertyList) {
            StyleProperty[] properties = propertyList.array;
            int count = propertyList.size;

            for (int i = 0; i < count; i++) {
                ref StyleProperty property = ref properties[i];
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
            FontAsset fontAsset = textSpan.fontAsset;

            fontData.fontAsset = fontAsset;
            fontData.gradientScale = textSpan.fontAsset.gradientScale;
            fontData.scaleRatioA = textSpan.scaleRatioA;
            fontData.scaleRatioB = textSpan.scaleRatioB;
            fontData.scaleRatioC = textSpan.scaleRatioC;
            fontData.textureWidth = fontAsset.atlas.width;
            fontData.textureHeight = fontAsset.atlas.height;
        }

        public override void PaintBackground(RenderContext ctx) {
            if (textSpan.geometryVersion != lastGeometryVersion) {
                lastGeometryVersion = textSpan.geometryVersion;
                UpdateGeometry();
            }

            if (textSpan.fontAsset != fontData.fontAsset) {
                UpdateFontData();
                shouldUpdateMaterialProperties = true;
            }

            if (shouldUpdateMaterialProperties) {
                shouldUpdateMaterialProperties = false;
                float underlayX = (Mathf.Clamp(textSpan.underlayX, -1, 1) + 1) * 0.5f;
                float underlayY = (Mathf.Clamp(textSpan.underlayY, -1, 1) + 1) * 0.5f;
                float underlayDilate = (Mathf.Clamp(textSpan.underlayDilate, -1, 1) + 1) * 0.5f;
                float underlaySoftness = Mathf.Clamp01(textSpan.underlaySoftness);
                // todo -- something is odd with packing, getting NaN back
                float packedUnderlay = VertigoUtil.ColorToFloat(new Color(underlayX, underlayY, underlayDilate, underlaySoftness));
                float mainColor = VertigoUtil.ColorToFloat(textSpan.textColor);
                float outlineColor = VertigoUtil.ColorToFloat(textSpan.outlineColor);
                float underlayColor = VertigoUtil.ColorToFloat(textSpan.underlayColor);
                float glowColor = VertigoUtil.ColorToFloat(textSpan.glowColor);

                float weight = 0;

                if ((textSpan.fontStyle & Text.FontStyle.Bold) != 0) {
                    weight = fontData.fontAsset.weightBold;
                }
                else {
                    weight = fontData.fontAsset.weightNormal;
                }

                weight = ((weight * 0.25f) + textSpan.faceDilate) * textSpan.scaleRatioA * 0.5f;

                float packedOutlineGlow = VertigoUtil.ColorToFloat(new Color(textSpan.outlineWidth, textSpan.outlineSoftness, textSpan.glowOuter, textSpan.glowOffset));

                geometry.objectData = new Vector4((int) ShapeType.Text, packedOutlineGlow, packedUnderlay, weight);
                geometry.packedColors = new Vector4(mainColor, outlineColor, underlayColor, glowColor);
            }


            Matrix4x4 matrix = element.layoutResult.matrix.ToMatrix4x4();
            // ctx.DrawBatchedGeometry(geometry, ranges.array[0], element.layoutResult.matrix.ToMatrix4x4());
            if (ranges.size == 1) {
                ctx.DrawBatchedText(geometry, ranges.array[0], matrix, fontData);
            }
            else {
                for (int i = 0; i < ranges.size; i++) {
                    ctx.DrawBatchedText(geometry, ranges.array[i], matrix, fontData);
                }

                // if all passed cull check, submit as one range
                // else submit individually
            }
        }

    }

}