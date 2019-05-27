using System;
using SVGX;
using TMPro;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {

    public class TextSpan2 {

        internal int characterCount;
        internal SVGXTextStyle textStyle;
        internal float scaleRatioA;
        internal float scaleRatioB;
        internal float scaleRatioC;

        internal TextSpan2 parent;

        internal LightList<TextSpan2> children;

        internal int wordStart;
        internal int wordEnd;
        internal int charStart;
        internal int charEnd;

        internal TextInfo2 textInfo;
        internal int index;

        internal char[] rawContent;
        internal int rawContentSize;

        public void SetText(char[] characters) {
            if (textInfo == null) return;

            if (characters.Length > rawContent.Length) {
                Array.Resize(ref rawContent, characters.Length * 2);
            }

            Array.Copy(characters, 0, rawContent, 0, characters.Length);
            StructList<CharInfo2> charInfoList = StructList<CharInfo2>.Get();

            rawContentSize = characters.Length;

            char[] buffer = null;

            // todo -- handle this later
            for (int i = 0; i < characters.Length; i++) {
                if (characters[i] == '&') {
                    // c# provides System.Net.WebUtility.HtmlDecode("&eacute;");
                    // returns a string not a character and allocates a lot, better not to use it
                    int advance = TryParseSymbol(characters, i);
                    if (advance != 0) {
                        i += advance;
                        // adjust buffer size? not sure how to handle these yet
                    }
                }
            }

            int bufferSize = ProcessWhitespace(rawContent, textStyle.whitespaceMode, ref buffer);


            // handle all kerning within a span as if there were no other spans. text info will handle 'gluing' spans together
            charInfoList.size = bufferSize;


            TextUtil.TransformText(textStyle.textTransform, buffer, bufferSize);

            CharInfo2[] charInfos = charInfoList.array;
            charInfoList.size = bufferSize;

            for (int i = 0; i < bufferSize; i++) {
                charInfos[i].character = buffer[i];
                charInfos[i].spanIndex = index;
            }

            StructList<WordInfo2> wordInfoList = TextUtil.BreakIntoWords(buffer, bufferSize);

            FindGlyphs(textStyle.fontAsset, charInfos, bufferSize);
            FindKerningInfo(textStyle.fontAsset, charInfos, bufferSize);
            ComputeWordAndCharacterSizes(charInfoList, wordInfoList);

            textInfo.InternalUpdateSpan(index, charInfoList, wordInfoList);

            StructList<CharInfo2>.Release(ref charInfoList);
            ArrayPool<char>.Release(ref buffer);
        }

        private void ComputeWordAndCharacterSizes(StructList<CharInfo2> characterList, StructList<WordInfo2> wordInfoList) {
            FontAsset fontAsset = textStyle.fontAsset;
            float smallCapsMultiplier = (textStyle.fontStyle & FontStyle.SmallCaps) == 0 ? 1.0f : 0.8f;
            float fontScale = textStyle.fontSize * smallCapsMultiplier / fontAsset.faceInfo.PointSize * fontAsset.faceInfo.Scale;
            Vector3 ratios = TextUtil.ComputeRatios(textStyle);
            float padding = TextUtil.GetPadding(textStyle, ratios);
            scaleRatioA = ratios.x;
            scaleRatioB = ratios.y;
            scaleRatioC = ratios.z;
            float gradientScale = textStyle.fontAsset.gradientScale;
            float boldAdvanceMultiplier = 1;
            float stylePadding = 0;

            int atlasWidth = textStyle.fontAsset.atlas.width;
            int atlasHeight = textStyle.fontAsset.atlas.height;

            float fontAscender = fontAsset.faceInfo.Ascender;

            // todo -- italic
            
            if ((textStyle.fontStyle & FontStyle.Bold) != 0) {
                stylePadding = fontAsset.boldStyle / 4.0f * gradientScale * ratios.x;
                if (stylePadding + padding > gradientScale) {
                    padding = gradientScale - stylePadding;
                }

                boldAdvanceMultiplier = 1 + fontAsset.boldSpacing * 0.01f;
            }
            else {
                stylePadding = stylePadding = fontAsset.normalStyle / 4.0f * gradientScale;
                if (stylePadding + padding > gradientScale) {
                    padding = gradientScale - stylePadding;
                }
            }

            CharInfo2[] characters = characterList.array;
            WordInfo2[] words = wordInfoList.array;
            int wordCount = wordInfoList.size;

            for (int w = 0; w < wordCount; w++) {

                WordInfo2 group = words[w];
                int start = group.charStart;
                int end = group.charEnd;
                float xAdvance = 0;

                for (int c = start; c < end; c++) {

                    Vector2 topLeft;
                    Vector2 bottomRight;
                    Vector2 uv0;

                    TextGlyph glyph = characters[c].glyph;
                    GlyphValueRecord glyphAdjustments = characters[c].glyphAdjustment;

                    float currentElementScale = fontScale * glyph.scale;

                    topLeft.x = xAdvance + (glyph.xOffset - padding - stylePadding + glyphAdjustments.xPlacement) * currentElementScale;
                    topLeft.y = (fontAscender - (glyph.yOffset + padding)) * currentElementScale;
                    bottomRight.x = topLeft.x + (glyph.width + padding * 2) * currentElementScale;
                    bottomRight.y = topLeft.y + (glyph.height + padding * 2 + stylePadding * 2) * currentElementScale;

                    uv0.x = (glyph.x - padding - stylePadding) / atlasWidth;
                    uv0.y = 1 - (glyph.y + padding + stylePadding + glyph.height) / atlasHeight;

                    // maybe store geometry elsewhere
                    characters[c].topLeft = topLeft;
                    characters[c].bottomRight = bottomRight;
                    characters[c].texCoord = uv0;
                    characters[c].scale = currentElementScale;
                    
                    // maybe just store x advance per character since we need a word pass on xadvance any
                    
                    xAdvance += (glyph.xAdvance
                                 * boldAdvanceMultiplier
                                 + fontAsset.normalSpacingOffset
                                 + glyphAdjustments.xAdvance) * currentElementScale;
                }
                
            }
            
        }

        private static int TryParseSymbol(char[] characters, int index) {
            return 0;
        }

        private static readonly string[] s_Symbols = {
            "&shy;",
            "&nbsp;"
        };

        private static void FindGlyphs(FontAsset fontAsset, CharInfo2[] charInfos, int count) {
            IntMap<TextGlyph> fontAssetCharacterDictionary = fontAsset.characterDictionary;
            for (int i = 0; i < count; i++) {
                charInfos[i].glyph = fontAssetCharacterDictionary.GetOrDefault(charInfos[i].character);
            }
        }

        private static void FindKerningInfo(FontAsset fontAsset, CharInfo2[] charInfos, int count) {
            IntMap<TextKerningPair> kerningDictionary = fontAsset.kerningDictionary;
            if (count < 2) {
                return;
            }

            GlyphValueRecord glyphAdjustments = default;
            int idx = 0;

            glyphAdjustments = kerningDictionary.GetOrDefault(((charInfos[1].character) << 16) + charInfos[0].character).firstGlyphAdjustments;
            charInfos[idx++].glyphAdjustment = glyphAdjustments;

            for (int i = 1; i < count - 1; i++) {
                int current = charInfos[i].character;
                int next = charInfos[i + 1].character;
                int prev = charInfos[i - 1].character;

                glyphAdjustments = kerningDictionary.GetOrDefault((next << 16) + current).firstGlyphAdjustments;
                glyphAdjustments += kerningDictionary.GetOrDefault((current << 16) + prev).secondGlyphAdjustments;

                charInfos[idx++].glyphAdjustment = glyphAdjustments;
            }

            glyphAdjustments = kerningDictionary.GetOrDefault(((charInfos[count - 1].character) << 16) + charInfos[count - 2].character).firstGlyphAdjustments;
            charInfos[idx++].glyphAdjustment = glyphAdjustments;
        }

        // whitespace between spans?
        private static int ProcessWhitespace(char[] input, WhitespaceMode whitespaceMode, ref char[] buffer) {
            bool collapseSpaceAndTab = (whitespaceMode & WhitespaceMode.CollapseWhitespace) != 0;
            bool preserveNewLine = (whitespaceMode & WhitespaceMode.PreserveNewLines) != 0;

            bool collapsing = collapseSpaceAndTab;

            if (buffer == null) {
                buffer = ArrayPool<char>.GetMinSize(input.Length);
            }

            if (buffer.Length < input.Length) {
                ArrayPool<char>.Resize(ref buffer, input.Length);
            }

            int writeIndex = 0;

            for (int i = 0; i < input.Length; i++) {
                char current = input[i];

                if (current == '\n' && !preserveNewLine) {
                    continue;
                }

                bool isWhiteSpace = current == '\t' || current == ' ';

                if (collapsing) {
                    if (!isWhiteSpace) {
                        buffer[writeIndex++] = current;
                        collapsing = false;
                    }
                }
                else if (isWhiteSpace) {
                    collapsing = collapseSpaceAndTab;
                    buffer[writeIndex++] = ' ';
                }
                else {
                    buffer[writeIndex++] = current;
                }
            }

            return writeIndex;
        }

    }

}