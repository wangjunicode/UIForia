using System;
using Rendering;
using Src.Layout.LayoutTypes;
using Src.Text;
using Src.Util;
using TMPro;
using UnityEngine;

namespace Src {

    // <Paragraph/>
    // <Header1/>
    // <Label/>

    // while parent is text span
    // if parent is something else -> layout injects text container parent
    // 1 text contains many spans
    // span created by default if not created from template
    // text span elements do not lay themselves out or render themselves
    // they contain scoped font properties
    // text span can only contain text (and maybe a shape / sprite)
    // text container can contain non text things, text will be treated as blocks
    // text flow container can handle layouting text to flow around other contents
    // 

    public sealed class UITextSpanElement : UIElement {

        public int startCharacter;
        public int characterCount;

    }

    public class UITextElement : CustomDrawableElement {

        private string text;
        public TextInfo textInfo;
        protected int[] processedText;

        public event Action<UITextElement, string> onTextChanged;

        public UITextElement(string text = "") {
            this.text = text;
            flags = flags | UIElementFlags.TextElement
                          | UIElementFlags.Primitive;
        }

        public string GetText() {
            return text;
        }

        public void SetText(string newText) {
            if (this.text == newText) {
                return;
            }

            this.text = newText;

            SetVerticesDirty();
            bool collapseSpaces = true; //style.computedStyle.TextCollapseWhiteSpace;
            bool preserveNewlines = false; //style.computedStyle.TextPreserveNewLines;


            if (textInfo.spanInfos != null) ArrayPool<SpanInfo>.Release(textInfo.spanInfos);
            if (textInfo.wordInfos != null) ArrayPool<WordInfo>.Release(textInfo.wordInfos);
            if (textInfo.lineInfos != null) ArrayPool<LineInfo>.Release(textInfo.lineInfos);
            if (textInfo.charInfos != null) ArrayPool<CharInfo>.Release(textInfo.charInfos);

            // todo release text stuff
            textInfo = TextUtil.ProcessText(text, collapseSpaces, preserveNewlines);
            textInfo.spanCount = 1;
            textInfo.spanInfos = ArrayPool<SpanInfo>.GetMinSize(1);
            textInfo.spanInfos[0].wordCount = textInfo.wordCount;
            textInfo.spanInfos[0].font = TMP_FontAsset.defaultFontAsset;
            textInfo.spanInfos[0].charCount = textInfo.charCount;
            textInfo.spanInfos[0].fontSize = style.computedStyle.FontSize;
            textInfo.spanInfos[0].fontStyle = style.computedStyle.FontStyle;

            ComputeCharacterAndWordSizes(textInfo);

            onTextChanged?.Invoke(this, text);
        }

        public void AppendText() { }

        public void InsertText() { }

        public override void OnAllocatedSizeChanged() {
            SetVerticesDirty();
        }

        private static TMP_FontAsset GetFontAssetForWeight(SpanInfo spanInfo, int fontWeight) {
            bool isItalic = (spanInfo.fontStyle & TextUtil.FontStyle.Italic) != 0;

            int weightIndex = fontWeight / 100;
            TMP_FontWeights weights = spanInfo.font.fontWeights[weightIndex];
            return isItalic ? weights.italicTypeface : weights.regularTypeface;
        }

        public override void OnStylePropertyChanged(StyleProperty property) {
            if (IsTextProperty(property.propertyId)) {
                switch (property.propertyId) {
                    case StylePropertyId.TextAnchor:
                        SetVerticesDirty();
                        break;
                    case StylePropertyId.TextColor:
                        SetVerticesDirty();
                        break;
                    case StylePropertyId.TextAutoSize:
                        break;
                    case StylePropertyId.TextFontAsset:
                        break;
                    case StylePropertyId.TextFontSize:
                        SetVerticesDirty();
                        break;
                    case StylePropertyId.TextFontStyle:
                        SetVerticesDirty();
                        break;
                    case StylePropertyId.TextHorizontalOverflow:
                        break;
                    case StylePropertyId.TextVerticalOverflow:
                        break;
                    case StylePropertyId.TextWhitespaceMode:
                        break;
                }
            }
        }

        private static bool IsTextProperty(StylePropertyId propertyId) {
            int intId = (int) propertyId;
            const int start = (int) StylePropertyId.__TextPropertyStart__;
            const int end = (int) StylePropertyId.__TextPropertyEnd__;
            return intId > start && intId < end;
        }

        protected static void ApplyLineAndWordOffsets(TextInfo textInfo) {
            LineInfo[] lineInfos = textInfo.lineInfos;
            WordInfo[] wordInfos = textInfo.wordInfos;
            CharInfo[] charInfos = textInfo.charInfos;

            for (int lineIdx = 0; lineIdx < textInfo.lineCount; lineIdx++) {
                float wordAdvance = 0;

                LineInfo currentLine = lineInfos[lineIdx];
                float lineOffset = currentLine.Height - currentLine.position.y;

                for (int w = currentLine.wordStart; w < currentLine.wordStart + currentLine.wordCount; w++) {
                    WordInfo currentWord = wordInfos[w];

                    for (int i = currentWord.startChar; i < currentWord.startChar + currentWord.charCount; i++) {
                        float x0 = charInfos[i].topLeft.x + wordAdvance;
                        float x1 = charInfos[i].bottomRight.x + wordAdvance;
                        float y0 = charInfos[i].topLeft.y - lineOffset;
                        float y1 = charInfos[i].bottomRight.y - lineOffset;
                        charInfos[i].topLeft = new Vector2(x0, y0);
                        charInfos[i].bottomRight = new Vector2(x1, y1);
                    }

                    wordAdvance += currentWord.xAdvance;
                }
            }
        }

        private static void ComputeCharacterAndWordSizes(TextInfo textInfo) {
            WordInfo[] wordInfos = textInfo.wordInfos;
            CharInfo[] charInfos = textInfo.charInfos;

            for (int spanIdx = 0; spanIdx < textInfo.spanCount; spanIdx++) {
                
                SpanInfo spanInfo = textInfo.spanInfos[spanIdx];
                TMP_FontAsset fontAsset = spanInfo.font;
                Material material = fontAsset.material;
                
                bool isUsingAltTypeface = false;
                float boldAdvanceMultiplier = 1;

                if ((spanInfo.fontStyle & TextUtil.FontStyle.Bold) != 0) {
                    fontAsset = GetFontAssetForWeight(spanInfo, 700);
                    isUsingAltTypeface = true;
                    boldAdvanceMultiplier = 1 + fontAsset.boldSpacing * 0.01f;
                }

                float smallCapsMultiplier = (spanInfo.fontStyle & TextUtil.FontStyle.SmallCaps) == 0 ? 1.0f : 0.8f;
                float fontScale = spanInfo.fontSize * smallCapsMultiplier / fontAsset.fontInfo.PointSize * fontAsset.fontInfo.Scale;

                float yAdvance = fontAsset.fontInfo.Baseline * fontScale * fontAsset.fontInfo.Scale;
                float monoAdvance = 0;

                float minWordSize = float.MaxValue;
                float maxWordSize = float.MinValue;

                float padding = ShaderUtilities.GetPadding(fontAsset.material, enableExtraPadding: false, isBold: false);
                float stylePadding = 0;

                if (!isUsingAltTypeface && (spanInfo.fontStyle & TextUtil.FontStyle.Bold) == TextUtil.FontStyle.Bold) {
                    if (material.HasProperty(ShaderUtilities.ID_GradientScale)) {
                        float gradientScale = material.GetFloat(ShaderUtilities.ID_GradientScale);
                        stylePadding = fontAsset.boldStyle / 4.0f * gradientScale * material.GetFloat(ShaderUtilities.ID_ScaleRatio_A);

                        // Clamp overall padding to Gradient Scale size.
                        if (stylePadding + padding > gradientScale) {
                            padding = gradientScale - stylePadding;
                        }
                    }

                    boldAdvanceMultiplier = 1 + fontAsset.boldSpacing * 0.01f;
                }
                else if (material.HasProperty(ShaderUtilities.ID_GradientScale)) {
                    float gradientScale = material.GetFloat(ShaderUtilities.ID_GradientScale);
                    stylePadding = fontAsset.normalStyle / 4.0f * gradientScale * material.GetFloat(ShaderUtilities.ID_ScaleRatio_A);

                    // Clamp overall padding to Gradient Scale size.
                    if (stylePadding + padding > gradientScale) {
                        padding = gradientScale - stylePadding;
                    }
                }

                // todo -- handle tab
                // todo -- handle sprites
                // todo -- handle alignment / justification
                
                for (int w = spanInfo.startWord; w < spanInfo.startWord + spanInfo.wordCount; w++) {
                    WordInfo currentWord = wordInfos[w];
                    float xAdvance = 0;
                    // new lines are their own words (idea: give them an xAdvance of some huge number so they always get their own lines)

                    for (int i = currentWord.startChar; i < currentWord.startChar + currentWord.charCount; i++) {
                        int current = charInfos[i].character;

                        TMP_Glyph glyph;
                        TMP_FontAsset fontForGlyph = TMP_FontUtilities.SearchForGlyph(spanInfo.font, charInfos[i].character, out glyph);

                        KerningPair adjustmentPair;
                        GlyphValueRecord glyphAdjustments = new GlyphValueRecord();

                        // todo -- if we end up doing character wrapping we probably want to ignore prev x kerning for line start
                        if (i != textInfo.charCount - 1) {
                            int next = charInfos[i + 1].character;
                            fontAsset.kerningDictionary.TryGetValue((next << 16) + current, out adjustmentPair);
                            if (adjustmentPair != null) {
                                glyphAdjustments = adjustmentPair.firstGlyphAdjustments;
                            }
                        }

                        if (i != 0) {
                            int prev = charInfos[i - 1].character;
                            fontAsset.kerningDictionary.TryGetValue((current << 16) + prev, out adjustmentPair);
                            if (adjustmentPair != null) {
                                glyphAdjustments += adjustmentPair.secondGlyphAdjustments;
                            }
                        }

                        float currentElementScale = fontScale * glyph.scale;
                        float topShear = 0;
                        float bottomShear = 0;

                        if (!isUsingAltTypeface && ((spanInfo.fontStyle & TextUtil.FontStyle.Italic) != 0)) {
                            float shearValue = fontAsset.italicStyle * 0.01f;
                            topShear = glyph.yOffset * shearValue;
                            bottomShear = (glyph.yOffset - glyph.height) * shearValue;
                        }

                        Vector2 topLeft;
                        Vector2 bottomRight;

                        // idea for auto sizing: multiply scale later on and just save base unscaled vertices

                        topLeft.x = xAdvance + (glyph.xOffset - padding - stylePadding + glyphAdjustments.xPlacement) * currentElementScale;
                        topLeft.y = yAdvance + (glyph.yOffset + padding + glyphAdjustments.yPlacement) * currentElementScale;
                        bottomRight.x = topLeft.x + (glyph.width + padding * 2) * currentElementScale;
                        bottomRight.y = topLeft.y - (glyph.height + padding * 2 + stylePadding * 2) * currentElementScale;

                        FaceInfo faceInfo = fontAsset.fontInfo;
                        Vector2 uv0;

                        uv0.x = (glyph.x - padding - stylePadding) / faceInfo.AtlasWidth;
                        uv0.y = 1 - (glyph.y + padding + stylePadding + glyph.height) / faceInfo.AtlasHeight;

                        Vector2 uv1;
                        uv1.x = (glyph.x + padding + stylePadding + glyph.width) / faceInfo.AtlasWidth;
                        uv1.y = 1 - (glyph.y - padding - stylePadding) / faceInfo.AtlasHeight;

                        charInfos[i].topLeft = topLeft;
                        charInfos[i].bottomRight = bottomRight;
                        charInfos[i].shearValues = new Vector2(topShear, bottomShear);

                        charInfos[i].uv0 = uv0;
                        charInfos[i].uv1 = uv1;

                        charInfos[i].uv2 = Vector2.one; // todo -- compute uv2s
                        charInfos[i].uv3 = Vector2.one;

                        float elementAscender = fontAsset.fontInfo.Ascender * currentElementScale / smallCapsMultiplier;
                        float elementDescender = fontAsset.fontInfo.Descender * currentElementScale / smallCapsMultiplier;

                        charInfos[i].ascender = elementAscender;
                        charInfos[i].descender = elementDescender;

                        currentWord.ascender = elementAscender > currentWord.ascender ? elementAscender : currentWord.ascender;
                        currentWord.descender = elementDescender < currentWord.descender ? elementDescender : currentWord.descender;

                        if ((spanInfo.fontStyle & (TextUtil.FontStyle.Superscript | TextUtil.FontStyle.Subscript)) != 0) {
                            float baseAscender = elementAscender / fontAsset.fontInfo.SubSize;
                            float baseDescender = elementDescender / fontAsset.fontInfo.SubSize;

                            currentWord.ascender = baseAscender > currentWord.ascender ? baseAscender : currentWord.ascender;
                            currentWord.descender = baseDescender < currentWord.descender ? baseDescender : currentWord.descender;
                        }

                        if (i < currentWord.startChar + currentWord.spaceStart) {
                            currentWord.characterSize = charInfos[i].bottomRight.x;
                        }

                        xAdvance += (glyph.xAdvance * boldAdvanceMultiplier + fontAsset.normalSpacingOffset + glyphAdjustments.xAdvance) * currentElementScale;
                    }

                    currentWord.xAdvance = xAdvance;
                    currentWord.size = new Vector2(xAdvance, currentWord.ascender - currentWord.descender);
                    minWordSize = Mathf.Min(minWordSize, currentWord.size.x);
                    maxWordSize = Mathf.Max(maxWordSize, currentWord.size.x);
                    wordInfos[w] = currentWord;
                }
            }
        }

        public override Texture GetMainTexture() {
            return textInfo.spanInfos[0].font.material.mainTexture;
        }

        public override Mesh GetMesh() {
            if (mesh != null && !IsGeometryDirty) {
                return mesh;
            }

            isMeshDirty = false;
            if (mesh == null) mesh = new Mesh();
            mesh.Clear();

            // todo adjust for anchoring / alignment 

            ApplyLineAndWordOffsets(textInfo);

            CharInfo[] charInfos = textInfo.charInfos;

            int sizeX4 = textInfo.charCount * 4;

            int[] triangles = new int[textInfo.charCount * 6];
            Vector3[] positions = new Vector3[textInfo.charCount * 4];
            Vector2[] uvs0 = new Vector2[sizeX4];
            Vector2[] uvs2 = new Vector2[sizeX4];
            Vector3[] normals = new Vector3[sizeX4];
            Vector4[] tangents = new Vector4[sizeX4];
            Color32[] colors = new Color32[sizeX4];

            Color32 color = style.computedStyle.TextColor;
            
            int idx_x4 = 0;
            int idx_x6 = 0;

            for (int i = 0; i < textInfo.charCount; i++) {
                if (charInfos[i].character == ' ') continue;

                for (int j = 0; j < 4; j++) {
                    normals[idx_x4 + j] = Vector3.back;
                    tangents[idx_x4 + j] = new Vector4(-1f, 0, 0, 1f);
                    colors[idx_x4 + j] = color;
                }

                Vector2 topLeft = charInfos[i].topLeft;
                Vector2 bottomRight = charInfos[i].bottomRight;

//                topLeft.y -= 11.4f;
//                bottomRight.y -= 11.4f;

                positions[idx_x4 + 0] = new Vector3(topLeft.x, bottomRight.y, 0); // Bottom Left
                positions[idx_x4 + 1] = new Vector3(topLeft.x, topLeft.y, 0); // Top Left
                positions[idx_x4 + 2] = new Vector3(bottomRight.x, topLeft.y, 0); // Top Right
                positions[idx_x4 + 3] = new Vector3(bottomRight.x, bottomRight.y); // Bottom Right

                uvs0[idx_x4 + 0] = new Vector2(charInfos[i].uv0.x, charInfos[i].uv0.y);
                uvs0[idx_x4 + 1] = new Vector2(charInfos[i].uv0.x, charInfos[i].uv1.y);
                uvs0[idx_x4 + 2] = new Vector2(charInfos[i].uv1.x, charInfos[i].uv1.y);
                uvs0[idx_x4 + 3] = new Vector2(charInfos[i].uv1.x, charInfos[i].uv0.y);

                uvs2[idx_x4 + 0] = new Vector2(0.0f, 0.5f); // todo -- need to compute these
                uvs2[idx_x4 + 1] = new Vector2(511f, 0.5f);
                uvs2[idx_x4 + 2] = new Vector2(2093056, 0.5f);
                uvs2[idx_x4 + 3] = new Vector2(2093056, 0.5f);

                triangles[idx_x6 + 0] = idx_x4 + 0;
                triangles[idx_x6 + 1] = idx_x4 + 1;
                triangles[idx_x6 + 2] = idx_x4 + 2;
                triangles[idx_x6 + 3] = idx_x4 + 2;
                triangles[idx_x6 + 4] = idx_x4 + 3;
                triangles[idx_x6 + 5] = idx_x4 + 0;

                idx_x4 += 4;
                idx_x6 += 6;
            }

            mesh.vertices = positions;
            mesh.uv = uvs0;
            mesh.uv2 = uvs2;
            mesh.colors32 = colors;
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.triangles = triangles;

            return mesh;
        }

        public override Material GetMaterial() {
            return TMP_FontAsset.defaultFontAsset.material;
        }

    }

}