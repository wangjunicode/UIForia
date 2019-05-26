using System;
using SVGX;
using TMPro;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Text {

    // only recompute text stuff when character to left or right changed

    public class TextSpan2 {

        internal int characterCount;
        internal TextGeometry[] geometry;
        internal SVGXTextStyle textStyle;
        internal CharInfo2[] charInfoList;
        internal GlyphValueRecord[] glyphAdjustments;
        internal float scaleRatioA;
        internal float scaleRatioB;
        internal float scaleRatioC;
        internal float padding;
        internal TextSpan2 parent;
        internal LightList<TextSpan2> children;

        internal TextInfo2 textInfo;
        internal int index;

        public void SetText(char[] characters) {
            if (textInfo == null) return;
            textInfo.UpdateSpanText(id, characters);
        }

    }

    // sequence of letters or whitespace w/ a type
    // also handles softhyphens
    public struct CharacterGroup {

        public int index;
        public int characterCount;
        public int lineIndex;
        public string content; // debug only
        public int start;
        public int count;
        public CharacterGroupType type;

    }

    public enum CharacterGroupType {

        Word,
        Whitespace,
        SoftHyphen

    }
    

    // use an allocator that returns spans of an array and can compress as needed

    public class TextInfo2 {

        public StructList<int> mouseHandlingGroups;

        public TextSpan2[] spans;
        public int spanCount;
        private static int s_SpanIdGenerator;

        internal StructList<CharInfo2> characterInfoList;
        internal StructList<TextGeometry> geometryList;

        public int AppendSpan(string content, in SVGXTextStyle style) {
            TextSpan2 textSpan = new TextSpan2();
            textSpan.index = spanCount;

            FontAsset fontAsset = style.fontAsset; // todo -- search if not defined

            if (spanCount + 1 >= spans.Length) {
                Array.Resize(ref spans, spanCount + 4);
            }

            characterInfoList.EnsureAdditionalCapacity(content.Length, 10);

            float ascender = fontAsset.faceInfo.Ascender;

            int charCount = characterInfoList.size;

            // shift arrays to accomodate new data

            FindGlyphs(content, fontAsset, charCount - content.Length);
            FindKerningInfo(content, fontAsset, charCount - content.Length);
            StructList<CharacterGroup> characterGroups = BreakIntoCharacterGroups(content);
            MergeCharacterGroups(insertIndex, characterGroups);
            StructList<CharacterGroup>.Release(ref characterGroups);

            spanCount++;
            return s_SpanIdGenerator++;
        }

        private StructList<CharacterGroup> BreakIntoCharacterGroups(string content) {
            // 3 group types: words, soft hyphen group, white space
            // white space gets collapsed based on a span style's white space rules
            StructList<CharacterGroup> tempWordList = StructList<CharacterGroup>.Get();
            
            bool inWhiteSpace = false;

            bool IsWhitespace(char c) {
                return c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');
            }

            int contentEnd = content.Length - 1;
            CharacterGroup group = new CharacterGroup();

            for (int i = 0; i < content.Length; i++) {
                char charCode = content[i];
                
                if (IsWhitespace(charCode)) {
                    
                    if (group.count != 0) {
                        tempWordList.Add(group);    
                    }
                    
                    int start = i;
                    while (true) {
                        i++;
                        if (i == contentEnd || !IsWhitespace(content[i])) {
                            break;
                        }
                    }

                    group = new CharacterGroup();
                    group.type = CharacterGroupType.Whitespace;
                    group.start = start;
                    group.count = i - start;
                    tempWordList.Add(group);
                    continue;
                }
                else if (charCode == '&') {
                    // look ahead for &shy; or other escape sequences (&nbsp;) etc
                }
                else {
                    
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
            
            return tempWordList;
        }
        
        // positions are not transformed by xAdvance or line position
        public void UpdateGeometry() {
            int characterCount = characterInfoList.size;
            CharInfo2[] charInfos = characterInfoList.array;

            int geometryIdx = 0;
            
            for (int i = 0; i < spanCount; i++) {
                TextSpan2 span = spans[i];
                int start = span.characterStart;
                int end = span.characterEnd;
                float padding = 0;
                float stylePadding = 0;
                float smallCapsMultiplier = 1f;
                FontAsset fontAsset = span.textStyle.fontAsset;
                float fontScale = span.textStyle.fontSize * smallCapsMultiplier / fontAsset.faceInfo.PointSize * fontAsset.faceInfo.Scale;
                float ascender = fontAsset.faceInfo.Ascender;
                int atlasWidth = fontAsset.atlas.width;
                int atlasHeight = fontAsset.atlas.height;
                
                for (int j = start; j < end; j++) {

                    TextGlyph glyph = charInfos[i].glyph;
                    GlyphValueRecord glyphAdjustments = charInfos[i].glyphAdjustment;
                    float currentElementScale = fontScale * glyph.scale;

                    TextGeometry geometry;
                    geometry.minX = (glyph.xOffset - padding - stylePadding + glyphAdjustments.xPlacement) * currentElementScale;
                    geometry.minY = (ascender - (glyph.yOffset + padding)) * currentElementScale;
                    geometry.maxX = geometry.minX + (glyph.width + padding * 2) * currentElementScale;
                    geometry.maxY = geometry.minY + (glyph.height + padding * 2 + stylePadding * 2) * currentElementScale;
                    geometry.uv0.x = (glyph.x - padding - stylePadding) / atlasWidth;
                    geometry.uv0.y = 1 - (glyph.y + padding + stylePadding + glyph.height) / atlasHeight;
                    geometryList[geometryIdx++] = geometry;
                }
                
            }
            
        }

        private void FindGlyphs(string content, FontAsset fontAsset, int start) {
            IntMap<TextGlyph> fontAssetCharacterDictionary = fontAsset.characterDictionary;
            CharInfo2[] charInfos = characterInfoList.array;
            int idx = start;
            for (int i = 0; i < content.Length; i++) {
                charInfos[idx++].glyph = fontAssetCharacterDictionary.GetOrDefault(content[i]);
            }
        }

        private void FindKerningInfo(string content, FontAsset fontAsset, int start) {
            IntMap<TextKerningPair> kerningDictionary = fontAsset.kerningDictionary;
            CharInfo2[] charInfos = characterInfoList.array;
            int idx = start;
            GlyphValueRecord glyphAdjustments = default;

            if (content.Length < 2) {
                for (int i = 0; i < content.Length; i++) {
                    charInfos[idx++].glyphAdjustment = default;
                }

                return;
            }

            glyphAdjustments = kerningDictionary.GetOrDefault(((content[1]) << 16) + content[0]).firstGlyphAdjustments;
            charInfos[idx++].glyphAdjustment = glyphAdjustments;

            for (int i = 1; i < content.Length - 1; i++) {
                int current = content[i];
                int next = content[i + 1];
                int prev = content[i - 1];

                glyphAdjustments = kerningDictionary.GetOrDefault((next << 16) + current).firstGlyphAdjustments;
                glyphAdjustments += kerningDictionary.GetOrDefault((current << 16) + prev).secondGlyphAdjustments;

                charInfos[idx++].glyphAdjustment = glyphAdjustments;
            }

            glyphAdjustments = kerningDictionary.GetOrDefault(((content[content.Length - 1]) << 16) + content[content.Length - 2]).firstGlyphAdjustments;
            charInfos[idx++].glyphAdjustment = glyphAdjustments;
        }

        private void BreakIntoWords() {
            // split into word sections
            // a word section can be soft hyphen group
            // white space sits in its own word group
            for (int i = 0; i < spanCount; i++) {
                TextSpan2 span = spans[i];
                for (int j = 0; j < span.charInfoList.Length; j++) {
                    int character = span.charInfoList[j].character;

                    if (character == 13) { }
                }
            }
        }

        private static bool IsWhitespace(char c) {
            return c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');
        }

    }

    public struct CharInfo2 {

        public Vector2 topLeft;
        public Vector2 bottomLeft;
        public int wordIndex;
        public int lineIndex;
        public float scale;
        public Vector2 shear;
        public int character;
        public TextGlyph glyph;
        public GlyphValueRecord glyphAdjustment;

    }

    public struct TextGeometry {

        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
        public Vector2 uv0;

    }

    public struct CharInfo {

        // todo -- crunch this down or at least be sure not to incur silly copy costs

        public char character;
        public Vector2 topLeft;
        public Vector2 bottomRight;
        public Vector2 layoutTopLeft;
        public Vector2 layoutBottomRight;
        public Vector2 shearValues;
        public Vector2 uv0;
        public Vector2 uv1;
        public Vector2 uv2;
        public Vector2 uv3;
        public float ascender;
        public float descender;
        public int lineIndex;
        public int wordIndex;
        public float scale;

        public float Width => bottomRight.x - topLeft.x;
        public float Height => bottomRight.y - topLeft.y;
        public Vector2 Center => topLeft + new Vector2(Width * 0.5f, Height * 0.5f);

    }

}