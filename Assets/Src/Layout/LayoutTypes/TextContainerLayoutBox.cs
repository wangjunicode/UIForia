using System.Collections.Generic;
using Rendering;
using Src.Systems;
using Src.Util;
using TMPro;
using UnityEngine;
using UnityScript.Lang;

namespace Src.Layout.LayoutTypes {

    public enum TextElementType {

        Character,
        Sprite

    };

    public struct CharacterInfo {

        public char character; // Should be changed to an int to handle UTF 32

        /// <summary>
        /// Index of the character in the raw string.
        /// </summary>
        public int index; // Index of the character in the input string.

        public TMP_TextElementType elementType;

        public TMP_TextElement textElement;
        public TMP_FontAsset fontAsset;
        public TMP_SpriteAsset spriteAsset;
        public int spriteIndex;
        public bool isUsingAlternateTypeface;

        public float pointSize;

        public int lineNumber;
        public int pageNumber;

        public int vertexIndex;
        public TMP_Vertex vertex_TL;
        public TMP_Vertex vertex_BL;
        public TMP_Vertex vertex_TR;
        public TMP_Vertex vertex_BR;

        public Vector3 topLeft;
        public Vector3 bottomLeft;
        public Vector3 topRight;
        public Vector3 bottomRight;
        public float origin;
        public float ascender;
        public float baseLine;
        public float descender;

        public float xAdvance;
        public float aspectRatio;
        public float scale;
        public FontStyles style;

        public bool isVisible;
        //public bool isIgnoringAlignment;

    }

    public class TextContainerLayoutBox : LayoutBox {

        protected ITextSizeCalculator textSizeCalculator;

        public TextContainerLayoutBox(ITextSizeCalculator textSizeCalculator, LayoutSystem2 layoutSystem, UIElement element)
            : base(layoutSystem, element) {
            this.textSizeCalculator = textSizeCalculator;
        }

        public override void RunLayout() {
            // no-op I think, maybe just apply padding / indent / whatever to text child
            // or maybe this evolves into handling inline things in text.
        }

        public void SetTextContent(string text) { }

        protected override float GetMinRequiredHeightForWidth(float width) {
            return textSizeCalculator.CalcTextHeight(element.style.textContent, element.style, width);
        }

        public int m_totalCharacterCount;
        public float m_fontScale;
        public float m_fontSize;
        public TMP_FontAsset m_fontAsset;

        public float m_fontScaleMultiplier;
        public float m_currentFontSize;
        protected TMP_XmlTagStack<float> m_sizeStack = new TMP_XmlTagStack<float>(new float[16]);

        public struct TextInfo {

            public int lineCount;
            public int wordCount;
            public int spanCount;
            public int charCount;

        }

        protected TMP_FontAsset GetFontAssetForWeight(FontStyle m_style, int fontWeight) {
            bool isItalic = (m_style & FontStyle.Italic) == FontStyle.Italic || (m_fontStyle & FontStyle.Italic) == FontStyle.Italic;
            TMP_FontAsset fontAsset;

            int weightIndex = fontWeight / 100;

            if (isItalic) {
                fontAsset = m_currentFontAsset.fontWeights[weightIndex].italicTypeface;
            }
            else {
                fontAsset = m_currentFontAsset.fontWeights[weightIndex].regularTypeface;
            }

            return fontAsset;
        }

        public void Run() {
            string text = "text";
            char[] buffer = new char[128];
            TMP_Glyph glyph;
            TMP_FontAsset tempFontAsset;
        }

        // create
        // insert 
        // append
        // delete
        // measure
        // word wrap
        // line count
        // max word size

        public struct CharInfo {

            public char character;
            public TMP_Glyph glyph;
            public Vector2 position;
            public Vector2 size;

        }

        public struct WordInfo {

            public Vector2 position;
            public Vector2 size;

            public int spaceStart;
            public int spaceCount;
            public int charStart;
            public int charCount;

        }

        public struct LineInfo {

            public int wordStart;
            public int wordCount;
            public Vector2 position;
            public Vector2 size;

        }

        public struct SpanInfo {

            public int startChar;
            public int charCount;
            public TMP_FontAsset font;
            public FontStyle fontStyle;
            public int pointSize;
            public WhitespaceMode whitespaceMode;
            public float firstLineIndent;
            public float lineIndent;

        }
//         m_stylePadding = extra padding to accomodate bold
//        m_fontScale = (m_fontSize / m_fontAsset.fontInfo.PointSize * m_fontAsset.fontInfo.Scale * (m_isOrthographic ? 1 : 0.1f))
//        currentElementScale = m_fontScale * m_fontScaleMultiplier * m_cached_TextElement.scale;
// top left x = wordLocalxAdvance + ((glyph.xOffset - paddingFromShader + glyphAdjustment.xPlacement) * currentElementScale);
// top left y = fontBaseLineOffset + (glyph.yOffset + paddingFromShader + glyphAdjustment.yPlacement) * currentElementScale; (maybe - lineOffset + baseLineOffset)
// bottom right x = topLeft.x + glyph.width + padding * 2 * currentElementScale   
        
//        top_left.x = m_xAdvance + ((glyph.xOffset - padding - style_padding + glyphAdjustments.xPlacement) * currentElementScale * (1 - m_charWidthAdjDelta));
//        top_left.y = fontBaseLineOffset + (glyph.yOffset + padding + glyphAdjustments.yPlacement) * currentElementScale - m_lineOffset + m_baselineOffset;
//        bottom_right.x = bottom_left.x + ((glyph.width + padding * 2 + style_padding * 2) * currentElementScale * (1 - m_charWidthAdjDelta));
//        bottom_right.y = top_left.y - ((glyph.height + padding * 2) * currentElementScale);

        public class Glyph : TMP_TextElement { }

        public void SetText(string text) {
            TextInfo textInfo = ParseText(text);
        }

        private TextInfo ParseText(string text) {
            int[] buffer = null;
            int bufferLength = TextUtil.StringToCharArray(text, ref buffer);

            SpanInfo spanInfo = new SpanInfo();
            spanInfo.startChar = 0;
            spanInfo.charCount = bufferLength;
            spanInfo.font = TMP_FontAsset.defaultFontAsset;

            List<WordInfo> s_WordInfoList = new List<WordInfo>(128);
            WordInfo currentWord = new WordInfo();
            bool inWhiteSpace = false;
            int wordCharCount = 0;
            int wordFirstVisible = -1;

            CharInfo[] charInfos = new CharInfo[bufferLength];

            WhitespaceMode whitespaceMode = WhitespaceMode.Wrap;

            int charCount = bufferLength;

            // for each span
            // for each word in span
            // for each char in word
            //    apply kerning
            //    compute local xAdvance
            //    find min / max asecendor descendor
            //    apply glyph adjustment y placement
            //    apply currentElementScale
            //    idea is to position characters in local space relative to the word they belong to
            //
            for (int i = 0; i < bufferLength; i++) {
                // will need to run this through a white-space processor
                int charCode = buffer[i];
                // probably a 2nd pass to avoid excessive if checks and ignore if kerning disabled or monospace enabled
                // HandleKerning(spanInfo.font, buffer[i - 1], buffer[i], buffer[i + 1]);

                if (char.IsWhiteSpace((char) charCode)) {
                    if (inWhiteSpace) {
                        wordCharCount++;
                        charCount--; // reduce char count if skipping
                    }
                    else {
                        // end the current word
                        inWhiteSpace = true;
                        wordCharCount++;
                        // consume whitespace

                        currentWord.charCount = wordCharCount;
                        currentWord.charStart = wordFirstVisible;
                        currentWord.spaceStart = i;
                        currentWord.spaceCount = wordFirstVisible - i;

                        wordCharCount = 0;
                        wordFirstVisible = -1;

                        s_WordInfoList.Add(currentWord);
                    }
                }
                else {
                    if (inWhiteSpace) {
                        inWhiteSpace = false;
                        wordFirstVisible = wordCharCount;
                    }
                }
            }
        }

        public void AppendText(string text) { }

        public void InsertText(int index, string text) { }

        public void RemoveText(int index, int count) { }

        protected void Generate(string text) {
            TMP_TextInfo textInfo;
            TMP_FontUtilities.SearchForGlyph(m_currentFontAsset, c, out glyph);
            if (text.Length == 0) {
                return;
            }

            TMP_FontAsset m_currentFontAsset = null;
            int totalCharacterCount = m_totalCharacterCount;

            int[] charBuffer = null;
            TMP_FontAsset fontAsset = null;

            TextUtil.StringToCharArray(text, ref charBuffer);

            CharacterInfo[] charInfos = new CharacterInfo[charBuffer.Length];

            for (int i = 0; i < charBuffer.Length && charBuffer[i] != 0; i++) {
                int charCode = charBuffer[i];

                TMP_Glyph glyph;

                TMP_FontAsset useFont = TMP_FontUtilities.SearchForGlyph(fontAsset, charCode, out glyph);

                charInfos[i].character = (char) charCode;
                charInfos[i].textElement = glyph;
            }
        }

        public struct TextSpan {

            public int startIndex;
            public int charCount;
            public TMP_FontAsset fontAsset;
            public FontStyle fontStyle;

        }

        public void CalculateVertexPositions() {
//            float fontBaseLineOffset = m_currentFontAsset.fontInfo.Baseline * m_fontScale * m_fontScaleMultiplier * m_currentFontAsset.fontInfo.Scale;
//            Vector3 top_left;
//            top_left.x = m_xAdvance + ((m_cached_TextElement.xOffset - padding - style_padding + glyphAdjustments.xPlacement) * currentElementScale * (1 - m_charWidthAdjDelta));
//            top_left.y = fontBaseLineOffset + (m_cached_TextElement.yOffset + padding + glyphAdjustments.yPlacement) * currentElementScale - m_lineOffset + m_baselineOffset;
//            top_left.z = 0;
//
//            Vector3 bottom_left;
//            bottom_left.x = top_left.x;
//            bottom_left.y = top_left.y - ((m_cached_TextElement.height + padding * 2) * currentElementScale);
//            bottom_left.z = 0;
//
//            Vector3 top_right;
//            top_right.x = bottom_left.x + ((m_cached_TextElement.width + padding * 2 + style_padding * 2) * currentElementScale * (1 - m_charWidthAdjDelta));
//            top_right.y = top_left.y;
//            top_right.z = 0;
//
//            Vector3 bottom_right;
//            bottom_right.x = top_right.x;
//            bottom_right.y = bottom_left.y;
//            bottom_right.z = 0;
        }

        public void ApplyFakeBold() {
//            if (m_textElementType == TMP_TextElementType.Character && !isUsingAltTypeface && ((m_style & FontStyles.Bold) == FontStyles.Bold || (m_fontStyle & FontStyles.Bold) == FontStyles.Bold)) // Checks for any combination of Bold Style.
//            {
//                if (m_currentMaterial.HasProperty(ShaderUtilities.ID_GradientScale))
//                {
//                    float gradientScale = m_currentMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
//                    style_padding = m_currentFontAsset.boldStyle / 4.0f * gradientScale * m_currentMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);
//
//                    // Clamp overall padding to Gradient Scale size.
//                    if (style_padding + padding > gradientScale)
//                        padding = gradientScale - style_padding;
//                }
//                else
//                    style_padding = 0;
//
//                bold_xAdvance_multiplier = 1 + m_currentFontAsset.boldSpacing * 0.01f;
//            }
//            else
//            {
//                if (m_currentMaterial.HasProperty(ShaderUtilities.ID_GradientScale))
//                {
//                    float gradientScale = m_currentMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
//                    style_padding = m_currentFontAsset.normalStyle / 4.0f * gradientScale * m_currentMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);
//
//                    // Clamp overall padding to Gradient Scale size.
//                    if (style_padding + padding > gradientScale)
//                        padding = gradientScale - style_padding;
//                }
//                else
//                    style_padding = 0;
//
//                bold_xAdvance_multiplier = 1.0f;
//            }
        }

        public void ApplyFakeItalics() {
//            if (m_textElementType == TMP_TextElementType.Character && !isUsingAltTypeface && ((m_style & FontStyles.Italic) == FontStyles.Italic || (m_fontStyle & FontStyles.Italic) == FontStyles.Italic))
//            {
//                // Shift Top vertices forward by half (Shear Value * height of character) and Bottom vertices back by same amount. 
//                float shear_value = m_currentFontAsset.italicStyle * 0.01f;
//                Vector3 topShear = new Vector3(shear_value * ((m_cached_TextElement.yOffset + padding + style_padding) * currentElementScale), 0, 0);
//                Vector3 bottomShear = new Vector3(shear_value * (((m_cached_TextElement.yOffset - m_cached_TextElement.height - padding - style_padding)) * currentElementScale), 0, 0);
//
//                top_left = top_left + topShear;
//                bottom_left = bottom_left + bottomShear;
//                top_right = top_right + topShear;
//                bottom_right = bottom_right + bottomShear;
//            }
        }

        public int GetGlyphs(string text, ref Glyph[] glyphs) { }

        public GlyphValueRecord HandleKerning(TMP_FontAsset fontAsset, char prev, char current, char next) {
            KerningPair adjustmentPair;
            GlyphValueRecord glyphAdjustments = new GlyphValueRecord();

            int key0 = (current << 16) + next;
            int key1 = (prev << 16) + current;

            fontAsset.kerningDictionary.TryGetValue(key0, out adjustmentPair);
            if (adjustmentPair != null) {
                glyphAdjustments = adjustmentPair.firstGlyphAdjustments;
            }

            fontAsset.kerningDictionary.TryGetValue(key1, out adjustmentPair);
            if (adjustmentPair != null) {
                glyphAdjustments += adjustmentPair.secondGlyphAdjustments;
            }

            return glyphAdjustments;
        }

    }

}