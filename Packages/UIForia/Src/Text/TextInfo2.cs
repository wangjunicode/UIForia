using System;
using SVGX;
using TMPro;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Text {

    public struct SpanInfo2 {

        public int charStart;
        public int charEnd;
        public int wordStart;
        public int wordEnd;
        public SVGXTextStyle textStyle;

        public SpanInfo2(SVGXTextStyle textStyle) {
            this.textStyle = textStyle;
            this.charStart = 0;
            this.charEnd = 0;
            this.wordStart = 0;
            this.wordEnd = 0;
        }

        public int CharCount => charEnd - charStart;
        public int WordCount => wordEnd - wordStart;

    }

    public class TextInfo2 {

        public LightList<SpanInfo2> spanList;
        public LightList<LineInfo> lineInfoList;
        public LightList<WordInfo> wordInfoList;
        public LightList<CharInfo> charInfoList;
        public LightList<char> characterList;

        private Size metrics;
        private bool metricsDirty;
        private bool layoutDirty;

        public static TMP_FontAsset DefaultFont => TMP_FontAsset.defaultFontAsset;

        public TextInfo2(TextSpan span) {
            spanList = new LightList<SpanInfo2>(2);
            lineInfoList = new LightList<LineInfo>(2);
            wordInfoList = new LightList<WordInfo>();
            charInfoList = new LightList<CharInfo>();
            characterList = new LightList<char>();
            AppendSpan(span);
        }

        public TextInfo2(params TextSpan[] spans) {
            spanList = new LightList<SpanInfo2>(spans.Length);
            lineInfoList = new LightList<LineInfo>(2);
            wordInfoList = new LightList<WordInfo>();
            charInfoList = new LightList<CharInfo>();
            characterList = new LightList<char>();
            for (int i = 0; i < spans.Length; i++) {
                AppendSpan(spans[i]);
            }
        }

        private static SpanInfo2 CreateSpanInfo(TextSpan span) {
            SpanInfo2 spanInfo = new SpanInfo2(span.style);
            if (spanInfo.textStyle.font == null) {
                spanInfo.textStyle.font = DefaultFont;
            }

            if (spanInfo.textStyle.fontSize <= 0) {
                spanInfo.textStyle.fontSize = 24;
            }

            if (!((Color) spanInfo.textStyle.color).IsDefined()) {
                spanInfo.textStyle.color = new Color32(0, 0, 0, 255);
            }

            return spanInfo;
        }

        public void AppendSpan(TextSpan span) {
            int spanIdx = spanList.Count;
            int previousCount = characterList.Count;
            spanList.Add(CreateSpanInfo(span));

            char[] buffer = null;

            int bufferSize = TextUtil.ProcessWrap(span.text, span.CollapseWhiteSpace, span.PreserveNewlines, ref buffer);
            TextUtil.ApplyTextTransform(buffer, bufferSize, span.style.textTransform);

            characterList.EnsureAdditionalCapacity(bufferSize);
            charInfoList.EnsureAdditionalCapacity(bufferSize);
            char[] characters = characterList.Array;

            Array.Copy(buffer, 0, characters, characterList.Count, bufferSize);
            ArrayPool<char>.Release(ref buffer);

            int wordStart = spanList.Count == 1 ? 0 : spanList[spanIdx - 1].wordEnd;

            characterList.Count += bufferSize;
            charInfoList.Count += bufferSize;

            LightList<WordInfo> tempWordList = ProcessText(previousCount, previousCount + bufferSize);

            ComputeCharacterAndWordSizes(spanIdx);

            spanList.Array[spanIdx].charStart = previousCount;
            spanList.Array[spanIdx].charEnd = previousCount + bufferSize;
            spanList.Array[spanIdx].wordStart = wordStart;
            spanList.Array[spanIdx].wordEnd = wordStart + tempWordList.Count;

            int w = wordInfoList.Count;
            wordInfoList.AddRange(tempWordList);

            for (int i = w; i < w + tempWordList.Count; i++) {
                    
            }
            
            LightListPool<WordInfo>.Release(ref tempWordList);
        }

        public void UpdateSpan(int spanIdx, TextSpan span) {
            
            if (spanIdx >= spanList.Count) {
                AppendSpan(span);
                return;
            }
            
            SpanInfo2 old = spanList.Array[spanIdx];

            spanList.Array[spanIdx] = CreateSpanInfo(span);

            char[] buffer = null;

            int bufferSize = TextUtil.ProcessWrap(span.text, span.CollapseWhiteSpace, span.PreserveNewlines, ref buffer);
            TextUtil.ApplyTextTransform(buffer, bufferSize, span.style.textTransform);

            if (bufferSize > old.CharCount) {
                characterList.ShiftRight(old.charEnd, bufferSize - old.CharCount);
                charInfoList.ShiftRight(old.charEnd, bufferSize - old.CharCount);
                char[] characters = characterList.Array;
                Array.Copy(buffer, 0, characters, old.charStart, bufferSize);
            }
            else if (bufferSize < old.CharCount) { }
            else {
                Array.Copy(buffer, 0, characterList.Array, old.charStart, bufferSize);
            }

            LightList<WordInfo> tempWordList = ProcessText(old.charStart, bufferSize);

            int wordStart = old.wordStart;

            int charDiff = bufferSize - old.CharCount;
            int wordDiff = tempWordList.Count - old.WordCount;
            SpanInfo2[] spans = spanList.Array;

            if (wordDiff > 0) {
                wordInfoList.ShiftRight(old.wordEnd, wordDiff);
                WordInfo[] wordInfos = wordInfoList.Array;
                Array.Copy(tempWordList.Array, 0, wordInfos, old.wordStart, tempWordList.Count);
                for (int i = spanIdx + 1; i < spanList.Count; i++) {
                    spans[i].charStart += charDiff;
                    spans[i].charEnd += charDiff;
                    spans[i].wordStart += wordDiff;
                    spans[i].wordEnd += wordDiff;
                    int ws = spans[i].wordStart;
                    int we = spans[i].wordEnd;
                    for (int j = ws; j < we; j++) {
                        wordInfos[j].startChar += charDiff;
                    }
                }
            }
            
            spans[spanIdx].charStart = old.charStart;
            spans[spanIdx].charEnd = old.charStart + bufferSize;
            spans[spanIdx].wordStart = wordStart;
            spans[spanIdx].wordEnd = wordStart + tempWordList.Count;

            ComputeCharacterAndWordSizes(spanIdx);

            LightListPool<WordInfo>.Release(ref tempWordList);
        }

        public void RemoveSpan(int idx) { }

        private LightList<WordInfo> ProcessText(int characterStart, int characterEnd) {
            LightList<WordInfo> tempWordList = LightListPool<WordInfo>.Get();

            WordInfo currentWord = new WordInfo();
            currentWord.startChar = characterStart;
            
            bool inWhiteSpace = false;
            CharInfo[] charInfos = charInfoList.Array;
            char[] buffer = characterList.Array;
            for (int i = characterStart; i < characterEnd; i++) {
                int charCode = buffer[i];
                charInfos[i].character = (char) charCode;

                if ((char) charCode == '\n') {
                    if (currentWord.charCount > 0) {
                        tempWordList.Add(currentWord);
                        currentWord = new WordInfo();
                        currentWord.startChar = i;
                    }

                    currentWord.charCount = 1;
                    currentWord.spaceStart = 0;
                    currentWord.isNewLine = true;
                    tempWordList.Add(currentWord);
                    currentWord = new WordInfo();
                    currentWord.startChar = i + 1;
                }

                if (!char.IsWhiteSpace((char) charCode)) {
                    if (inWhiteSpace) {
                        // new word starts
                        tempWordList.Add(currentWord);
                        currentWord = new WordInfo();
                        currentWord.startChar = i;
                        inWhiteSpace = false;
                    }

                    currentWord.charCount++;
                }
                else {
                    if (!inWhiteSpace) {
                        inWhiteSpace = true;
                        currentWord.spaceStart = currentWord.charCount;
                    }

                    currentWord.charCount++;
                }
            }

            if (!inWhiteSpace) {
                currentWord.spaceStart = currentWord.charCount;
            }

            tempWordList.Add(currentWord);

            return tempWordList;
        }

        public float ComputeWidth() {
            return 0;
        }

        public float ComputeHeight(float width) {
            return 0;
        }

        public Size ComputeMetrics(float width = -1) {
            return new Size();
        }

        public Size Layout(float width = float.MaxValue) {
            return new Size();
        }

        private static TMP_FontAsset GetFontAssetForWeight(FontStyle fontStyle, TMP_FontAsset font, int fontWeight) {
            bool isItalic = (fontStyle & FontStyle.Italic) != 0;

            int weightIndex = fontWeight / 100;
            TMP_FontWeights weights = font.fontWeights[weightIndex];
            return isItalic ? weights.italicTypeface : weights.regularTypeface;
        }

        private void ComputeCharacterAndWordSizes(int spanIdx) {
            WordInfo[] wordInfos = wordInfoList.Array;
            CharInfo[] charInfos = charInfoList.Array;

            SpanInfo2 spanInfo = spanList[spanIdx];
            TMP_FontAsset fontAsset = spanInfo.textStyle.font;
            Material fontAssetMaterial = fontAsset.material;

            bool isUsingAltTypeface = false;
            float boldAdvanceMultiplier = 1;

            if ((spanInfo.textStyle.fontStyle & FontStyle.Bold) != 0) {
                fontAsset = GetFontAssetForWeight(spanInfo.textStyle.fontStyle, spanInfo.textStyle.font, 700);
                isUsingAltTypeface = true;
                boldAdvanceMultiplier = 1 + fontAsset.boldSpacing * 0.01f;
            }

            float smallCapsMultiplier = (spanInfo.textStyle.fontStyle & FontStyle.SmallCaps) == 0 ? 1.0f : 0.8f;
            float fontScale = spanInfo.textStyle.fontSize * smallCapsMultiplier / fontAsset.fontInfo.PointSize * fontAsset.fontInfo.Scale;

            //float yAdvance = fontAsset.fontInfo.Baseline * fontScale * fontAsset.fontInfo.Scale;
            //float monoAdvance = 0;

            float minWordSize = float.MaxValue;
            float maxWordSize = float.MinValue;

            float padding = ShaderUtilities.GetPadding(fontAsset.material, enableExtraPadding: false, isBold: false);
            float stylePadding = 0;

            if (!isUsingAltTypeface && (spanInfo.textStyle.fontStyle & FontStyle.Bold) == FontStyle.Bold) {
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
                stylePadding = fontAsset.normalStyle / 4.0f * gradientScale *
                               fontAssetMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);

                // Clamp overall padding to Gradient Scale size.
                if (stylePadding + padding > gradientScale) {
                    padding = gradientScale - stylePadding;
                }
            }

            // todo -- handle tab
            // todo -- handle sprites

            int charCount = charInfoList.Count;

            for (int w = spanInfo.wordStart; w < spanInfo.wordEnd; w++) {
                WordInfo currentWord = wordInfos[w];
                float xAdvance = 0;
                // new lines are their own words (idea: give them an xAdvance of some huge number so they always get their own lines)

                for (int i = currentWord.startChar; i < currentWord.startChar + currentWord.charCount; i++) {
                    int current = charInfos[i].character;

                    TMP_Glyph glyph;
                    TMP_FontAsset fontForGlyph = TMP_FontUtilities.SearchForGlyph(spanInfo.textStyle.font, charInfos[i].character, out glyph);

                    KerningPair adjustmentPair;
                    GlyphValueRecord glyphAdjustments = new GlyphValueRecord();

                    // todo -- if we end up doing character wrapping we probably want to ignore prev x kerning for line start
                    if (i != charCount - 1) {
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

                    if (!isUsingAltTypeface && ((spanInfo.textStyle.fontStyle & FontStyle.Italic) != 0)) {
                        float shearValue = fontAsset.italicStyle * 0.01f;
                        topShear = glyph.yOffset * shearValue;
                        bottomShear = (glyph.yOffset - glyph.height) * shearValue;
                    }

                    Vector2 topLeft;
                    Vector2 bottomRight;

                    // idea for auto sizing: multiply scale later on and just save base unscaled vertices
//                        topLeft.x = xAdvance + (glyph.xOffset - padding - stylePadding + glyphAdjustments.xPlacement) * currentElementScale;
//                        topLeft.y = yAdvance + (glyph.yOffset + padding + glyphAdjustments.yPlacement) * currentElementScale;
//                        bottomRight.x = topLeft.x + (glyph.width + padding * 2) * currentElementScale;
//                        bottomRight.y = topLeft.y - (glyph.height + padding * 2 + stylePadding * 2) * currentElementScale;

                    topLeft.x = xAdvance + (glyph.xOffset - padding - stylePadding + glyphAdjustments.xPlacement) * currentElementScale;
                    topLeft.y = ((fontAsset.fontInfo.Ascender) - (glyph.yOffset + padding)) * currentElementScale;
                    bottomRight.x = topLeft.x + (glyph.width + padding * 2) * currentElementScale;
                    bottomRight.y = topLeft.y + (glyph.height + padding * 2 + stylePadding * 2) * currentElementScale;

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

                    charInfos[i].uv2 = new Vector2(currentElementScale, 0); // todo -- compute uv2s
                    charInfos[i].uv3 = Vector2.one;

                    float elementAscender = fontAsset.fontInfo.Ascender * currentElementScale / smallCapsMultiplier;
                    float elementDescender = fontAsset.fontInfo.Descender * currentElementScale / smallCapsMultiplier;

                    charInfos[i].ascender = elementAscender;
                    charInfos[i].descender = elementDescender;

                    currentWord.ascender = elementAscender > currentWord.ascender
                        ? elementAscender
                        : currentWord.ascender;
                    currentWord.descender = elementDescender < currentWord.descender
                        ? elementDescender
                        : currentWord.descender;

                    if ((spanInfo.textStyle.fontStyle & (FontStyle.Superscript | FontStyle.Subscript)) != 0) {
                        float baseAscender = elementAscender / fontAsset.fontInfo.SubSize;
                        float baseDescender = elementDescender / fontAsset.fontInfo.SubSize;

                        currentWord.ascender = baseAscender > currentWord.ascender
                            ? baseAscender
                            : currentWord.ascender;
                        currentWord.descender = baseDescender < currentWord.descender
                            ? baseDescender
                            : currentWord.descender;
                    }

                    if (i < currentWord.startChar + currentWord.spaceStart) {
                        currentWord.characterSize = charInfos[i].bottomRight.x;
                    }

                    xAdvance += (glyph.xAdvance * boldAdvanceMultiplier + fontAsset.normalSpacingOffset +
                                 glyphAdjustments.xAdvance) * currentElementScale;
                }

                currentWord.xAdvance = xAdvance;
                currentWord.size = new Vector2(xAdvance, currentWord.ascender); // was ascender - descender
                minWordSize = Mathf.Min(minWordSize, currentWord.size.x);
                maxWordSize = Mathf.Max(maxWordSize, currentWord.size.x);
                wordInfos[w] = currentWord;
            }
        }

    }

    public enum WhitespaceMode {

        CollapseWhitespace = 1 << 0,
        PreserveNewLines = 1 << 1

    }

    public struct TextSpan {

        public string text;

        public SVGXTextStyle style;
//        public SpanFlowType flowType;
//        public WhitespaceMode whitespaceMode;

        public TextSpan(string text, SVGXTextStyle style = default) {
            this.text = text;
            this.style = style;
        }

        public bool CollapseWhiteSpace => (style.whitespaceMode & WhitespaceMode.CollapseWhitespace) != 0;
        public bool PreserveNewlines => (style.whitespaceMode & WhitespaceMode.PreserveNewLines) != 0;

    }

    public enum SpanFlowType {

        Inline = 0,
        Left = 1,
        Right = 2,
        Nonblocking = 3

    }

}