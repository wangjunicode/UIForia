using System.Collections.Generic;
using System.Linq;
using Rendering;
using Src.Systems;
using Src.Util;
using TMPro;
using UnityEngine;
using UnityScript.Lang;

namespace Src.Layout.LayoutTypes {

    public class TextContainerLayoutBox : LayoutBox {

        protected string text;
        protected int[] processedText;
        protected TextInfo textInfo;

        public TextContainerLayoutBox(LayoutSystem2 layoutSystem, UIElement element)
            : base(layoutSystem, element) { }

        public override void RunLayout() {
            SpanInfo spanInfo = new SpanInfo();

            List<LineInfo> lineInfos = RunLayout(spanInfo, textInfo, allocatedWidth);
            textInfo.lineInfos = ArrayPool<LineInfo>.CopyFromList(lineInfos);
            textInfo.lineCount = lineInfos.Count;
            ListPool<LineInfo>.Release(lineInfos);
            
            float maxWidth = 0;
            for (int i = 0; i < lineInfos.Count; i++) {
                maxWidth = Mathf.Max(maxWidth, lineInfos[i].size.x);
            }

            actualWidth = maxWidth;
            actualHeight = lineInfos[lineInfos.Count - 1].position.y + lineInfos[lineInfos.Count - 1].Height;
        }

        protected override Size RunContentSizeLayout() {
            SpanInfo spanInfo = new SpanInfo();

            List<LineInfo> lineInfos = RunLayout(spanInfo, textInfo, allocatedWidth);
            ListPool<LineInfo>.Release(lineInfos);
            
            float maxWidth = 0;
            for (int i = 0; i < lineInfos.Count; i++) {
                maxWidth = Mathf.Max(maxWidth, lineInfos[i].size.x);
            }
            return new Size(maxWidth, lineInfos[lineInfos.Count - 1].position.y + lineInfos[lineInfos.Count - 1].Height);
        }

        // note -- all alignment / justification happens in the renderer
        private static List<LineInfo> RunLayout(SpanInfo spanInfo, TextInfo textInfo, float width) {
            float lineOffset = 0;

            LineInfo currentLine = new LineInfo();
            WordInfo[] wordInfos = textInfo.wordInfos;
            List<LineInfo> lineInfos = ListPool<LineInfo>.Get();

            TMP_FontAsset font = spanInfo.font;
            float lineGap = font.fontInfo.LineHeight - (font.fontInfo.Ascender - font.fontInfo.Descender);
            float baseScale = (spanInfo.fontSize / font.fontInfo.PointSize * font.fontInfo.Scale);

            // todo -- might want to use an optional 'lineheight' setting instead of just computing the line height
            // seems like I could also use fontInfo.lineHeight * baseScale;

            for (int w = 0; w < textInfo.wordCount; w++) {
                WordInfo currentWord = wordInfos[w];

                if (currentWord.isNewLine) {
                    lineInfos.Add(currentLine);
                    lineOffset -= (font.fontInfo.LineHeight + lineGap) * baseScale;
                    currentLine = new LineInfo();
                    currentLine.position = new Vector2(0, lineOffset);
                    currentLine.wordStart = w + 1;
                    continue;
                }

                if (currentWord.size.x > width) {
                    // we had words in this line already
                    // finish the line and start a new one
                    // line offset needs to to be bumped
                    if (currentLine.wordCount > 0) {
                        lineInfos.Add(currentLine);
                        lineOffset -= currentLine.Height + lineGap;
                        currentLine = new LineInfo();
                        currentLine.position = new Vector2(0, lineOffset);
                        currentLine.wordStart = w;
                    }

                    currentLine.wordCount = 1;
                    currentLine.maxAscender = currentWord.ascender;
                    currentLine.maxDescender = currentWord.descender;
                    currentLine.size = new Vector2(currentWord.size.x, currentLine.Height);
                    lineInfos.Add(currentLine);
                    lineOffset -= currentLine.Height + lineGap;
                    currentLine = new LineInfo();
                    currentLine.wordStart = w + 1;
                    currentLine.position = new Vector2(0, lineOffset);
                }

                else if (currentLine.size.x + currentWord.size.x > width) {
                    lineInfos.Add(currentLine);
                    lineOffset -= currentLine.Height + lineGap;
                    currentLine = new LineInfo();
                    currentLine.position = new Vector2(0, lineOffset);
                    currentLine.wordStart = w;
                    currentLine.wordCount = 1;
                    currentLine.maxAscender = currentWord.ascender;
                    currentLine.maxDescender = currentWord.descender;
                }

                else {
                    currentLine.wordCount++;
                    if (currentLine.maxAscender < currentWord.ascender) currentLine.maxAscender = currentWord.ascender;
                    if (currentLine.maxDescender > currentWord.descender) currentLine.maxDescender = currentWord.descender;
                    currentLine.size = new Vector2(currentLine.size.x + currentWord.xAdvance, currentLine.Height);
                }
            }

            lineInfos.Add(currentLine);
            return lineInfos;
        }

        // todo -- anchor adjustments and offsets need to be applied here as well
        protected static void ApplyLineAndWordOffsets(TextInfo textInfo) {
            LineInfo[] lineInfos = textInfo.lineInfos;
            WordInfo[] wordInfos = textInfo.wordInfos;
            CharInfo[] charInfos = textInfo.charInfos;

            for (int lineIdx = 0; lineIdx < textInfo.lineCount; lineIdx++) {
                float wordAdvance = 0;

                LineInfo currentLine = lineInfos[lineIdx];
                float lineOffset = currentLine.position.y;

                for (int w = currentLine.wordStart; w < currentLine.wordStart + currentLine.wordCount; w++) {
                    WordInfo currentWord = wordInfos[w];

                    for (int i = currentWord.startChar; i < currentWord.startChar + currentWord.charCount; i++) {
                        float x0 = charInfos[i].topLeft.x + wordAdvance;
                        float x1 = charInfos[i].bottomRight.x + wordAdvance;
                        float y0 = charInfos[i].topLeft.y + lineOffset;
                        float y1 = charInfos[i].bottomRight.y + lineOffset;
                        charInfos[i].topLeft = new Vector2(x0, y0);
                        charInfos[i].bottomRight = new Vector2(x1, y1);
                    }

                    wordAdvance += currentWord.xAdvance;
                }
            }
        }

      
        public void SetTextContent(string text) {
            TextSpan span = new TextSpan(text, style.FontAsset.asset);
            preferredContentSize = Size.Unset;

            processedText = ProcessText(processedText);

            for (int i = 0; i < text.Length; i++) {
                if (this.text[i] != text[i]) break;
            }

            ArrayPool<SpanInfo>.Release(textInfo.spanInfos);

            SpanInfo spanInfo = new SpanInfo();
            spanInfo.font = style.FontAsset.asset;
            spanInfo.charCount = processedText.Length;
            spanInfo.fontStyle = style.FontStyle;
            spanInfo.fontSize = style.FontSize;
            spanInfo.lineIndent = 0;
            spanInfo.pointSize = 0;
            spanInfo.whitespaceMode = WhitespaceMode.Wrap;
            textInfo.spanCount = 1;
            textInfo.spanInfos = ArrayPool<SpanInfo>.GetMinSize(1);

            TextInfo newTextInfo = TextUtil.ProcessText(span);

            //  int charCount = TextUtil.StringToCharArray(text, ref charBuffer);

            this.text = text;
        }

        private static int[] ProcessText(int[] currentText) {
            // if text matches re-use it
            // apply whitespace compression
            // look up glyphs
            return null;
        }

//        public void SetTextContent(TextSpan[] textSpans) { }

        private static TMP_FontAsset GetFontAssetForWeight(SpanInfo spanInfo, int fontWeight) {
            bool isItalic = (spanInfo.fontStyle & TextUtil.FontStyle.Italic) != 0;

            int weightIndex = fontWeight / 100;
            TMP_FontWeights weights = spanInfo.font.fontWeights[weightIndex];
            return isItalic ? weights.italicTypeface : weights.regularTypeface;
        }

        public void ApplyGlyphAdjustments(SpanInfo spanInfo, WordInfo[] wordInfos, CharInfo[] charInfos) {
            TMP_FontAsset fontAsset = spanInfo.font;
            bool isUsingAlternativeTypeface = false;
            float boldAdvanceMultiplier = 1;

            if ((spanInfo.fontStyle & TextUtil.FontStyle.Bold) != 0) {
                fontAsset = GetFontAssetForWeight(spanInfo, 700);
                isUsingAlternativeTypeface = true;
                boldAdvanceMultiplier = 1 + fontAsset.boldSpacing * 0.01f;
            }

            float xAdvance = 0;
            float yAdvance = 0;
            float monoAdvance = 0;
            float bold_xAdvance_multiplier = 1;

            float smallCapsMultiplier = (spanInfo.fontStyle & TextUtil.FontStyle.SmallCaps) != 0 ? 1.0f : 0.8f;
            float fontScale = spanInfo.fontSize * smallCapsMultiplier / fontAsset.fontInfo.PointSize * fontAsset.fontInfo.Scale;

            float minWordSize = float.MaxValue;
            float maxWordSize = float.MinValue;
            
            for (int w = spanInfo.startWord; w < spanInfo.startWord + spanInfo.wordCount; w++) {
                WordInfo currentWord = wordInfos[w];
                xAdvance = 0;
                // new lines are their own words (idea: give them an xAdvance of some huge number so they always get their own lines)

                for (int i = currentWord.startChar; i < currentWord.startChar + currentWord.charCount; i++) {
                    int current = charInfos[i].character;

                    TMP_Glyph glyph;
                    TMP_FontAsset fontForGlyph = TMP_FontUtilities.SearchForGlyph(spanInfo.font, charInfos[i].character, out glyph);

                    KerningPair adjustmentPair;
                    GlyphValueRecord glyphAdjustments = new GlyphValueRecord();

                    if (i != currentWord.startChar + currentWord.charCount - 1) {
                        int next = charInfos[i + 1].character;
                        fontAsset.kerningDictionary.TryGetValue((current << 16) + next, out adjustmentPair);
                        if (adjustmentPair != null) {
                            glyphAdjustments = adjustmentPair.firstGlyphAdjustments;
                        }
                    }

                    if (i != currentWord.startChar) {
                        int prev = charInfos[i - 1].character;
                        fontAsset.kerningDictionary.TryGetValue((prev << 16) + current, out adjustmentPair);
                        if (adjustmentPair != null) {
                            glyphAdjustments += adjustmentPair.secondGlyphAdjustments;
                        }
                    }

                    float currentElementScale = fontScale * glyph.scale;
                    float topShear = 0;
                    float bottomShear = 0;

                    if (!isUsingAlternativeTypeface && ((spanInfo.fontStyle & TextUtil.FontStyle.Italic) != 0)) {
                        float shearValue = fontAsset.italicStyle * 0.01f;
                        topShear = glyph.yOffset * shearValue;
                        bottomShear = (glyph.yOffset - glyph.height) * shearValue;
                    }

                    Vector2 topLeft;
                    Vector2 bottomRight;

                    float padding = 0.5f;
                    float style_padding = 0f;

                    // idea for auto sizing: multiply scale later on and just save base unscaled vertices

                    topLeft.x = xAdvance + (glyph.xOffset - padding + glyphAdjustments.xPlacement) * currentElementScale;
                    topLeft.y = yAdvance + (glyph.yOffset + padding + glyphAdjustments.yPlacement) * currentElementScale;
                    bottomRight.x = topLeft.x + (glyph.width + padding * 2) * currentElementScale;
                    bottomRight.y = topLeft.y - (glyph.height + padding * 2) * currentElementScale;

                    FaceInfo faceInfo = fontAsset.fontInfo;
                    Vector2 uv0;

                    uv0.x = (glyph.x - padding - style_padding) / faceInfo.AtlasWidth;
                    uv0.y = 1 - (glyph.y + padding + style_padding + glyph.height) / faceInfo.AtlasHeight;

                    Vector2 uv1;
                    uv1.x = (glyph.x + padding + style_padding + glyph.width) / faceInfo.AtlasWidth;
                    uv1.y = 1 - (glyph.y - padding - style_padding) / faceInfo.AtlasHeight;

                    charInfos[i].topLeft = topLeft;
                    charInfos[i].bottomRight = bottomRight;
                    charInfos[i].shearValues = new Vector2(topShear, bottomShear);

                    charInfos[i].uv0 = uv0;
                    charInfos[i].uv1 = uv1;

                    charInfos[i].uv2 = Vector2.one;
                    charInfos[i].uv3 = Vector2.one;

                    float elementAscender = fontAsset.fontInfo.Ascender * currentElementScale / smallCapsMultiplier;
                    float elementDescender = fontAsset.fontInfo.Descender * currentElementScale / smallCapsMultiplier;

                    currentWord.ascender = elementAscender > currentWord.ascender ? elementAscender : currentWord.ascender;
                    currentWord.descender = elementDescender < currentWord.descender ? elementDescender : currentWord.descender;

                    if ((spanInfo.fontStyle & (TextUtil.FontStyle.Superscript | TextUtil.FontStyle.Subscript)) != 0) {
                        float baseAscender = elementAscender / fontAsset.fontInfo.SubSize;
                        float baseDescender = elementDescender / fontAsset.fontInfo.SubSize;

                        currentWord.ascender = baseAscender > currentWord.ascender ? baseAscender : currentWord.ascender;
                        currentWord.descender = baseDescender < currentWord.descender ? baseDescender : currentWord.descender;
                    }

                    xAdvance += (glyph.xAdvance * bold_xAdvance_multiplier + fontAsset.normalSpacingOffset + glyphAdjustments.xAdvance) * currentElementScale;
                }

                currentWord.xAdvance = xAdvance;
                currentWord.size = new Vector2(xAdvance, currentWord.ascender - currentWord.descender);
                minWordSize = Mathf.Min(minWordSize, currentWord.size.x);
                maxWordSize = Mathf.Max(maxWordSize, currentWord.size.x);
                wordInfos[w] = currentWord;
            }
        }

    }

}

public class TextThing {

    public Mesh GenerateMesh(TextInfo textInfo) {
        Mesh retn = new Mesh();

        WordInfo[] wordInfos = textInfo.wordInfos;
        CharInfo[] charInfos = textInfo.charInfos;

        int sizeX4 = textInfo.charCount * 4;
        int sizeX6 = textInfo.charCount * 6;

        int[] triangles = new int[textInfo.charCount * 6];
        Vector3[] positions = new Vector3[textInfo.charCount * 4];
        Vector2[] uvs0 = new Vector2[sizeX4];
        Vector2[] uvs2 = new Vector2[sizeX4];
        Vector3[] normals = new Vector3[sizeX4];
        Vector4[] tangents = new Vector4[sizeX4];
        Color32[] colors = new Color32[sizeX4];

        int idx_x4 = 0;
        int idx_x6 = 0;

        for (int i = 0; i < textInfo.charCount; i++) {
            if (charInfos[i].character == ' ') continue;

            for (int j = 0; j < 4; j++) {
                normals[idx_x4 + j] = Vector3.back;
                tangents[idx_x4 + j] = new Vector4(-1f, 0, 0, 1f);
                colors[idx_x4 + j] = new Color32(255, 255, 255, 255);
            }

            Vector2 topLeft = charInfos[i].topLeft;
            Vector2 bottomRight = charInfos[i].bottomRight;

            Vector3 vTL = new Vector3(topLeft.x, topLeft.y, 0);
            Vector3 vTR = new Vector3(bottomRight.x, topLeft.y, 0);
            Vector3 vBL = new Vector3(topLeft.x, bottomRight.y, 0);
            Vector3 vBR = new Vector3(bottomRight.x, bottomRight.y);

            positions[idx_x4 + 0] = vBL;
            positions[idx_x4 + 1] = vTL;
            positions[idx_x4 + 2] = vTR;
            positions[idx_x4 + 3] = vBR;

            uvs0[idx_x4 + 0] = new Vector2(charInfos[i].uv0.x, charInfos[i].uv0.y);
            uvs0[idx_x4 + 1] = new Vector2(charInfos[i].uv0.x, charInfos[i].uv1.y);
            uvs0[idx_x4 + 2] = new Vector2(charInfos[i].uv1.x, charInfos[i].uv1.y);
            uvs0[idx_x4 + 3] = new Vector2(charInfos[i].uv1.x, charInfos[i].uv0.y);

            uvs2[idx_x4 + 0] = new Vector2(0.0f, 0.5f);
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

        retn.vertices = positions;
        retn.uv = uvs0;
        retn.uv2 = uvs2;
        retn.colors32 = colors;
        retn.normals = normals;
        retn.tangents = tangents;
        retn.triangles = triangles;

        return retn;
    }

    // new lines are their own words (idea: give them an xAdvance of some huge number so they always get their own lines)

}

public struct TextInfo {

    public int lineCount;
    public int wordCount;
    public int spanCount;
    public int charCount;
    public CharInfo[] charInfos;
    public LineInfo[] lineInfos;
    public WordInfo[] wordInfos;
    public SpanInfo[] spanInfos;

}

public struct CharInfo {

    public char character;
    public Vector2 topLeft;
    public Vector2 bottomRight;
    public Vector2 shearValues;
    public Vector2 uv0;
    public Vector2 uv1;
    public Vector2 uv2;
    public Vector2 uv3;

}

public struct WordInfo {

    public Vector2 position;
    public Vector2 size;

    public int spaceStart;
    public int charCount;
    public int startChar;
    public float ascender;
    public float descender;
    public float xAdvance;
    public int visibleCharCount => charCount - (charCount - spaceStart);
    public bool isNewLine;

}

public struct LineInfo {

    public int wordStart;
    public int wordCount;
    public float maxAscender;
    public float maxDescender;
    public Vector2 position;
    public Vector2 size;

    public float Height => maxAscender - maxDescender;

}

public struct SpanInfo {

    public int startChar;
    public int charCount;
    public TMP_FontAsset font;
    public TextUtil.FontStyle fontStyle;
    public int pointSize;
    public WhitespaceMode whitespaceMode;
    public float firstLineIndent;
    public float lineIndent;
    public float characterSpacing;
    public float wordSpacing;
    public float monoAdvance;
    public float fontSize;
    public int startWord;
    public int wordCount;

}

public struct TextSpan {

    public string text;
    public TMP_FontAsset font;

    public TextSpan(string text, TMP_FontAsset font = null) {
        this.text = text;
        this.font = font ? font : TMP_FontAsset.defaultFontAsset;
    }

}