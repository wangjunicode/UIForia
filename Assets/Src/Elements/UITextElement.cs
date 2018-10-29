using System;
using Rendering;
using Src.Layout.LayoutTypes;
using Src.Systems;
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

    public class UITextElement : UIElement, IMeshProvider, IMaterialProvider {

        public string text;
        public TextInfo textInfo;
        private Mesh mesh;
        private Material material; // todo -- see if there is a way to do material sharing, most text can use the same materials
        private TMP_FontAsset fontAsset;

        public event Action<UITextElement, string> onTextChanged;

        private static readonly Material s_BaseTextMaterial;

        static UITextElement() {
            s_BaseTextMaterial = Resources.Load<Material>("Materials/UIForiaText");
        }

        public UITextElement(string text = "") {
            this.text = text;
            Renderer = ElementRenderer.DefaultText;
            material = new Material(s_BaseTextMaterial);
            flags = flags | UIElementFlags.TextElement
                          | UIElementFlags.Primitive;
        }

        public override void OnReady() {
            UpdateTextInfo();
        }

        public string GetText() {
            return text;
        }

        public void SetText(string newText) {
            if (this.text == newText) {
                return;
            }

            this.text = newText;

            UpdateTextInfo();

            onTextChanged?.Invoke(this, text);
        }

        private void UpdateTextInfo() {
            bool collapseSpaces = true; //style.computedStyle.TextCollapseWhiteSpace;
            bool preserveNewlines = false; //style.computedStyle.TextPreserveNewLines;

            if (textInfo.spanInfos != null) ArrayPool<SpanInfo>.Release(ref textInfo.spanInfos);
            if (textInfo.wordInfos != null) ArrayPool<WordInfo>.Release(ref textInfo.wordInfos);
            if (textInfo.lineInfos != null) ArrayPool<LineInfo>.Release(ref textInfo.lineInfos);
            if (textInfo.charInfos != null) ArrayPool<CharInfo>.Release(ref textInfo.charInfos);

            // todo release text stuff
            textInfo = TextUtil.ProcessText(text, collapseSpaces, preserveNewlines, style.computedStyle.TextTransform);
            textInfo.spanCount = 1;
            textInfo.spanInfos = ArrayPool<SpanInfo>.GetMinSize(1);
            textInfo.spanInfos[0].wordCount = textInfo.wordCount;
            textInfo.spanInfos[0].font = style.computedStyle.FontAsset;
            textInfo.spanInfos[0].charCount = textInfo.charCount;
            textInfo.spanInfos[0].fontSize = style.computedStyle.FontSize;
            textInfo.spanInfos[0].fontStyle = style.computedStyle.FontStyle;
            textInfo.spanInfos[0].alignment = style.computedStyle.TextAlignment;

            ComputeCharacterAndWordSizes(textInfo);
        }

        public SelectionRange AppendText(char character) {
            SetText(text + character);
            return new SelectionRange(textInfo.charCount - 1, TextEdge.Right);
        }

        public SelectionRange AppendText(string str) {
            SetText(text + str);
            return new SelectionRange(textInfo.charCount - 1, TextEdge.Right);
        }

        public void DeleteText(int characterIndex, int count) { }

        public void InsertText(int characterIndex, char character) { }

        public void InsertText(int characterIndex, string str) { }

        private static TMP_FontAsset GetFontAssetForWeight(SpanInfo spanInfo, int fontWeight) {
            bool isItalic = (spanInfo.fontStyle & TextUtil.FontStyle.Italic) != 0;

            int weightIndex = fontWeight / 100;
            TMP_FontWeights weights = spanInfo.font.fontWeights[weightIndex];
            return isItalic ? weights.italicTypeface : weights.regularTypeface;
        }

        private static bool IsTextProperty(StylePropertyId propertyId) {
            int intId = (int) propertyId;
            const int start = (int) StylePropertyId.__TextPropertyStart__;
            const int end = (int) StylePropertyId.__TextPropertyEnd__;
            return intId > start && intId < end;
        }

        private static void ComputeCharacterAndWordSizes(TextInfo textInfo) {
            WordInfo[] wordInfos = textInfo.wordInfos;
            CharInfo[] charInfos = textInfo.charInfos;

            for (int spanIdx = 0; spanIdx < textInfo.spanCount; spanIdx++) {
                SpanInfo spanInfo = textInfo.spanInfos[spanIdx];
                TMP_FontAsset fontAsset = spanInfo.font;
                Material fontAssetMaterial = fontAsset.material;

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
                    if (fontAssetMaterial.HasProperty(ShaderUtilities.ID_GradientScale)) {
                        float gradientScale = fontAssetMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
                        stylePadding = fontAsset.boldStyle / 4.0f * gradientScale * fontAssetMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);

                        // Clamp overall padding to Gradient Scale size.
                        if (stylePadding + padding > gradientScale) {
                            padding = gradientScale - stylePadding;
                        }
                    }

                    boldAdvanceMultiplier = 1 + fontAsset.boldSpacing * 0.01f;
                }
                else if (fontAssetMaterial.HasProperty(ShaderUtilities.ID_GradientScale)) {
                    float gradientScale = fontAssetMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
                    stylePadding = fontAsset.normalStyle / 4.0f * gradientScale * fontAssetMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);

                    // Clamp overall padding to Gradient Scale size.
                    if (stylePadding + padding > gradientScale) {
                        padding = gradientScale - stylePadding;
                    }
                }

                // todo -- handle tab
                // todo -- handle sprites

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

                        if (currentWord.startChar + currentWord.VisibleCharCount >= i) {
                            if (topLeft.y > currentWord.maxCharTop) {
                                currentWord.maxCharTop = topLeft.y;
                            }

                            if (bottomRight.y < currentWord.minCharBottom) {
                                currentWord.minCharBottom = bottomRight.y;
                            }
                        }

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
                    currentWord.size = new Vector2(xAdvance, currentWord.ascender); // was ascender - descender
                    minWordSize = Mathf.Min(minWordSize, currentWord.size.x);
                    maxWordSize = Mathf.Max(maxWordSize, currentWord.size.x);
                    wordInfos[w] = currentWord;
                }
            }
        }

        protected static void ApplyLineAndWordOffsets(TextInfo textInfo) {
            LineInfo[] lineInfos = textInfo.lineInfos;
            WordInfo[] wordInfos = textInfo.wordInfos;
            CharInfo[] charInfos = textInfo.charInfos;

            for (int lineIdx = 0; lineIdx < textInfo.lineCount; lineIdx++) {
                LineInfo currentLine = lineInfos[lineIdx];
                float lineOffset = currentLine.maxAscender - currentLine.position.y;
                float wordAdvance = currentLine.position.x;

                for (int w = currentLine.wordStart; w < currentLine.wordStart + currentLine.wordCount; w++) {
                    WordInfo currentWord = wordInfos[w];
                    currentWord.lineIndex = lineIdx;
                    currentWord.position = new Vector2(wordAdvance, currentLine.position.y);

                    for (int i = currentWord.startChar; i < currentWord.startChar + currentWord.charCount; i++) {
                        float x0 = charInfos[i].topLeft.x + wordAdvance;
                        float x1 = charInfos[i].bottomRight.x + wordAdvance;
                        float y0 = charInfos[i].topLeft.y - lineOffset;
                        float y1 = charInfos[i].bottomRight.y - lineOffset;
                        charInfos[i].wordIndex = w;
                        charInfos[i].lineIndex = lineIdx;
                        charInfos[i].topLeft = new Vector2(x0, y0);
                        charInfos[i].bottomRight = new Vector2(x1, y1);
                    }

                    wordInfos[w] = currentWord;
                    wordAdvance += currentWord.xAdvance;
                }
            }
        }

//        public override Texture GetMainTexture() {
//            return textInfo.spanInfos[0].font.material.mainTexture;
//        }
//
        public Mesh GetMesh() {
            if (mesh != null && !layoutResult.SizeChanged) {
                return mesh;
            }

            if (mesh == null) mesh = new Mesh();
            mesh.Clear();

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

            Size actualSize = layoutResult.actualSize;

            // todo -- convert this to the job system
            for (int i = 0; i < textInfo.charCount; i++) {
                if (charInfos[i].character == ' ') continue;

                for (int j = 0; j < 4; j++) {
                    normals[idx_x4 + j] = Vector3.back;
                    tangents[idx_x4 + j] = new Vector4(-1f, 0, 0, 1f);
                    colors[idx_x4 + j] = color;
                }

                Vector2 topLeft = charInfos[i].topLeft;
                Vector2 bottomRight = charInfos[i].bottomRight;

                positions[idx_x4 + 0] = new Vector3(topLeft.x, bottomRight.y, 0); // Bottom Left
                positions[idx_x4 + 1] = new Vector3(topLeft.x, topLeft.y, 0); // Top Left
                positions[idx_x4 + 2] = new Vector3(bottomRight.x, topLeft.y, 0); // Top Right
                positions[idx_x4 + 3] = new Vector3(bottomRight.x, bottomRight.y); // Bottom Right

                uvs0[idx_x4 + 0] = new Vector2(charInfos[i].uv0.x, charInfos[i].uv0.y);
                uvs0[idx_x4 + 1] = new Vector2(charInfos[i].uv0.x, charInfos[i].uv1.y);
                uvs0[idx_x4 + 2] = new Vector2(charInfos[i].uv1.x, charInfos[i].uv1.y);
                uvs0[idx_x4 + 3] = new Vector2(charInfos[i].uv1.x, charInfos[i].uv0.y);

                float leftClipX = topLeft.x / actualSize.width;
                float rightClipX = bottomRight.x / actualSize.width;
                float topClipY = topLeft.y / actualSize.height;
                float bottomClipY = bottomRight.y / actualSize.height;

                uvs2[idx_x4 + 0] = new Vector2(leftClipX, bottomClipY);
                uvs2[idx_x4 + 1] = new Vector2(leftClipX, topClipY);
                uvs2[idx_x4 + 2] = new Vector2(rightClipX, topClipY);
                uvs2[idx_x4 + 3] = new Vector2(rightClipX, bottomClipY);

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


        public Material GetMaterial() {
            ComputedStyle computedStyle = ComputedStyle;
            Material fontMaterial = computedStyle.FontAsset.material;
            
            if (fontAsset != computedStyle.FontAsset) {
                fontAsset = computedStyle.FontAsset;
                material.mainTexture = fontMaterial.mainTexture;
                material.SetFloat(ShaderUtilities.ID_GradientScale, fontMaterial.GetFloat(ShaderUtilities.ID_GradientScale));
                material.SetFloat(ShaderUtilities.ID_WeightNormal, fontMaterial.GetFloat(ShaderUtilities.ID_WeightNormal));
                material.SetFloat(ShaderUtilities.ID_WeightBold, fontMaterial.GetFloat(ShaderUtilities.ID_WeightBold));
                material.SetFloat(ShaderUtilities.ID_ScaleRatio_A, fontMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A));
                material.SetFloat(ShaderUtilities.ID_ScaleRatio_B, fontMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_B));
                material.SetFloat(ShaderUtilities.ID_ScaleRatio_C, fontMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A));
                material.SetVector("_TextureSize", new Vector4(material.mainTexture.width, material.mainTexture.height));
            }

            // todo -- text styles & keywords, try to use the same materials where possible
//            material.SetVector("_OutlineColor", computedStyle.TextOutlineColor);
//            material.SetVector("_OutlineSettings", new Vector4(computedStyle.TextOutlineWidth, computedStyle.TextOutlineSoftness));
//            
//            Vector4 glowSettings = new Vector4(
//                computedStyle.TextGlowInnerSize, 
//                computedStyle.TextGlowOuterSize,
//                computedStyle.TextGlowPower,
//                PackFloat(computedStyle.TextGlowOffset)
//            );
//            
//            material.SetVector("_GlowColor", computedStyle.TextGlowColor);
//            material.SetVector("_GlowColorSettings", glowSettings);
//
//            material.SetVector("_UnderlaySettings", underlaySettings);
//            material.SetVector("_UnderlayColor", underlayColor);
//            
//            material.SetTexture("_FaceTexture", computedStyle.TextFaceTexture);
            
            return material;
        }

        private WordInfo GetWordAtPoint(Vector2 point) {
            return FindNearestWord(FindNearestLine(point), point);
        }

        public SelectionRange DeleteRange(SelectionRange selectionRange) {
            return new SelectionRange();
        }

        public string GetSubstring(SelectionRange selectionRange) {
            if (!selectionRange.HasSelection) {
                return string.Empty;
            }

            int start = Mathf.Min(selectionRange.cursorIndex, selectionRange.selectIndex);
            int end = Mathf.Max(selectionRange.cursorIndex, selectionRange.selectIndex);

            char[] chars = new char[end - start];
            int idx = 0;
            for (int i = start; i < end; i++) {
                chars[idx++] = textInfo.charInfos[i].character;
            }

            return new string(chars);
        }

        public SelectionRange SelectAll() {
            return new SelectionRange(textInfo.charCount - 1, TextEdge.Right, 0, TextEdge.Left);
        }

        private TextEdge FindCursorEdge(int charIndex, Vector2 point) {
            CharInfo charInfo = textInfo.charInfos[charIndex];
            float width = charInfo.bottomRight.x - charInfo.topLeft.x;
            if (point.x > charInfo.topLeft.x + (width * 0.5f)) {
                return TextEdge.Right;
            }

            return TextEdge.Left;
        }

        private int FindNearestCharacterIndex(WordInfo wordInfo, Vector2 point) {
            int closestIndex = wordInfo.startChar;
            float closestDistance = float.MaxValue;
            CharInfo[] charInfos = textInfo.charInfos;

            for (int i = wordInfo.startChar; i < wordInfo.startChar + wordInfo.charCount; i++) {
                CharInfo charInfo = charInfos[i];
                float x1 = charInfo.topLeft.x;
                float x2 = charInfo.bottomRight.x;

                if (point.x >= x1 && point.x <= x2) {
                    return i;
                }

                float distToY1 = Mathf.Abs(point.x - x1);
                float distToY2 = Mathf.Abs(point.x - x2);
                if (distToY1 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToY1;
                }

                if (distToY2 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToY2;
                }
            }

            return closestIndex;
        }

        private int FindNearestCharacterIndex(Vector2 point) {
            LineInfo nearestLine = FindNearestLine(point);
            WordInfo nearestWord = FindNearestWord(nearestLine, point);
            return FindNearestCharacterIndex(nearestWord, point);
        }

        private LineInfo FindNearestLine(Vector2 point) {
            return textInfo.lineInfos[FindNearestLineIndex(point)];
        }

        private int FindNearestLineIndex(Vector2 point) {
            int closestIndex = 0;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < textInfo.lineCount; i++) {
                LineInfo line = textInfo.lineInfos[i];
                float y1 = -line.maxAscender;
                float y2 = -line.maxDescender;

                if (point.y >= y1 && point.y <= y2) {
                    return i;
                }

                float distToY1 = Mathf.Abs(point.y - y1);
                float distToY2 = Mathf.Abs(point.y - y2);
                if (distToY1 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToY1;
                }

                if (distToY2 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToY2;
                }
            }

            return closestIndex;
        }

        private WordInfo FindNearestWord(LineInfo line, Vector2 point) {
            int closestIndex = 0;
            float closestDistance = float.MaxValue;
            for (int i = line.wordStart; i < line.wordStart + line.wordCount; i++) {
                WordInfo word = textInfo.wordInfos[i];
                float x1 = word.position.x;
                float x2 = word.position.x + word.xAdvance;
                if (point.x >= x1 && point.x <= x2) {
                    return word;
                }

                float distToX1 = Mathf.Abs(point.x - x1);
                float distToX2 = Mathf.Abs(point.x - x2);
                if (distToX1 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToX1;
                }

                if (distToX2 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToX2;
                }
            }

            return textInfo.wordInfos[closestIndex];
        }

        public Mesh GetHighlightMesh(SelectionRange selectionRange) {
            Color32 selectionColor = new Color32(168, 206, 255, 192);

            int startIndex = selectionRange.selectIndex;
            int endIndex = selectionRange.cursorIndex;
            TextEdge startEdge = selectionRange.selectEdge;
            TextEdge endEdge = selectionRange.cursorEdge;

            if (selectionRange.cursorIndex < selectionRange.selectIndex) {
                startIndex = selectionRange.cursorIndex;
                endIndex = selectionRange.selectIndex;
                startEdge = selectionRange.cursorEdge;
                endEdge = selectionRange.selectEdge;
            }

            CharInfo startCharInfo = textInfo.charInfos[startIndex];
            CharInfo endCharInfo = textInfo.charInfos[endIndex];

            if (startCharInfo.lineIndex == endCharInfo.lineIndex) {
                LineInfo currentLine = textInfo.lineInfos[startCharInfo.lineIndex];
                float minX = startEdge == TextEdge.Right ? startCharInfo.bottomRight.x : startCharInfo.topLeft.x;
                float maxX = endEdge == TextEdge.Right ? endCharInfo.bottomRight.x : endCharInfo.topLeft.x;
                float minY = currentLine.position.y - layoutResult.localPosition.y;
                float maxY = currentLine.position.y - currentLine.Height - layoutResult.localPosition.y;
                Vector3 v0 = new Vector3(minX, minY);
                Vector3 v1 = new Vector3(maxX, minY);
                Vector3 v2 = new Vector3(maxX, maxY);
                Vector3 v3 = new Vector3(minX, maxY);

                MeshUtil.s_VertexHelper.AddVert(v0, selectionColor, new Vector2(0, 1));
                MeshUtil.s_VertexHelper.AddVert(v1, selectionColor, new Vector2(0, 0));
                MeshUtil.s_VertexHelper.AddVert(v2, selectionColor, new Vector2(1, 0));
                MeshUtil.s_VertexHelper.AddVert(v3, selectionColor, new Vector2(1, 1));

                MeshUtil.s_VertexHelper.AddTriangle(0, 1, 2);
                MeshUtil.s_VertexHelper.AddTriangle(2, 3, 0);

                Mesh highlightMesh = new Mesh();

                MeshUtil.s_VertexHelper.FillMesh(highlightMesh);
                MeshUtil.s_VertexHelper.Clear();

                return highlightMesh;
            }

            return null;
        }

        public SelectionRange GetSelectionAtPoint(Vector2 point) {
            int charIndex = FindNearestCharacterIndex(point);
            return new SelectionRange(
                FindNearestCharacterIndex(point),
                FindCursorEdge(charIndex, point)
            );
        }

        public SelectionRange SelectWordAtPoint(Vector2 point) {
            WordInfo wordInfo = GetWordAtPoint(point);
            return new SelectionRange(
                wordInfo.startChar + wordInfo.VisibleCharCount - 1,
                TextEdge.Right,
                wordInfo.startChar
            );
        }

        public SelectionRange ValidateSelectionRange(SelectionRange range) {
            int cursorIdx = (range.cursorIndex < textInfo.charCount) ? range.cursorIndex : textInfo.charCount - 1;
            int selectIdx = (range.selectIndex < textInfo.charCount) ? range.selectIndex : textInfo.charCount - 1;
            return new SelectionRange(cursorIdx, range.cursorEdge, selectIdx, range.selectEdge);
        }

        public SelectionRange SelectToPoint(SelectionRange range, Vector2 point) {
            int charIndex = FindNearestCharacterIndex(point);
            return new SelectionRange(
                FindNearestCharacterIndex(point),
                FindCursorEdge(charIndex, point),
                range.selectIndex,
                range.selectEdge
            );
        }

        public enum TextEdge {

            Right,
            Left

        }

        public struct SelectionRange {

            public readonly int cursorIndex;
            public readonly int selectIndex;
            public readonly TextEdge cursorEdge;
            public readonly TextEdge selectEdge;

            public SelectionRange(int cursorIndex, TextEdge cursorEdge, int selectIndex = -1, TextEdge selectEdge = TextEdge.Left) {
                this.cursorIndex = cursorIndex;
                this.cursorEdge = cursorEdge;
                this.selectIndex = selectIndex;
                this.selectEdge = selectEdge;
            }

            public bool HasSelection => selectIndex != -1;

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                return obj is SelectionRange && Equals((SelectionRange) obj);
            }

            public override int GetHashCode() {
                unchecked {
                    int hashCode = cursorIndex;
                    hashCode = (hashCode * 397) ^ selectIndex;
                    hashCode = (hashCode * 397) ^ (int) cursorEdge;
                    hashCode = (hashCode * 397) ^ (int) selectEdge;
                    return hashCode;
                }
            }

            public bool Equals(SelectionRange previousSelectionRange) {
                return cursorIndex != previousSelectionRange.cursorIndex
                       || cursorEdge != previousSelectionRange.cursorEdge
                       || selectEdge != previousSelectionRange.selectEdge
                       || selectIndex != previousSelectionRange.selectIndex;
            }

            public static bool operator ==(SelectionRange a, SelectionRange b) {
                return a.cursorIndex == b.cursorIndex
                       && a.cursorEdge == b.cursorEdge
                       && a.selectEdge == b.selectEdge
                       && a.selectIndex == b.selectIndex;
            }

            public static bool operator !=(SelectionRange a, SelectionRange b) {
                return !(a == b);
            }

        }

        public Vector2 GetCursorPosition(SelectionRange selectionRange) {
            CharInfo charInfo = textInfo.charInfos[selectionRange.cursorIndex];
            LineInfo lineInfo = textInfo.lineInfos[charInfo.lineIndex];
            return new Vector2(selectionRange.cursorEdge == TextEdge.Right
                    ? charInfo.bottomRight.x
                    : charInfo.topLeft.x,
                -lineInfo.position.y
            );
        }

        public SelectionRange BeginSelection(Vector2 point) {
            int selectIdx = FindNearestCharacterIndex(point);
            TextEdge selectEdge = FindCursorEdge(selectIdx, point);
            return new SelectionRange(selectIdx, selectEdge, selectIdx, selectEdge);
        }

        public float GetLineHeightAtCursor(SelectionRange selectionRange) {
            return textInfo.lineInfos[textInfo.charInfos[selectionRange.cursorIndex].lineIndex].Height;
        }

    }

}