using System;
using SVGX;
using TMPro;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Text {

    // only recompute text stuff when character to left or right changed

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
        internal StructList<WordInfo2> wordInfoList;
        internal StructList<LineInfo> lineInfoList;

        public int AppendSpan(string content, in SVGXTextStyle style) {
            TextSpan2 textSpan = new TextSpan2(); // pool?
            textSpan.index = spanCount;

            if (spanCount + 1 >= spans.Length) {
                Array.Resize(ref spans, spanCount + 4);
            }

            characterInfoList.EnsureAdditionalCapacity(content.Length, 10);
            spans[spanCount++] = textSpan;
            textSpan.SetText(content.ToCharArray());
            return s_SpanIdGenerator++;
        }

        internal void InternalUpdateSpan(int index, StructList<CharInfo2> charInfos, StructList<WordInfo2> wordGroups) {
            if (spanCount == 1) {
                
                wordInfoList.EnsureCapacity(wordGroups.size);
                characterInfoList.EnsureCapacity(charInfos.size);
                
                Array.Copy(wordGroups.array, 0, wordInfoList.array, 0, wordGroups.size);
                Array.Copy(charInfos.array, 0, characterInfoList.array, 0, charInfos.size);
                
                wordInfoList.size = wordGroups.size;
                characterInfoList.size = charInfos.size;

            }
            
            if (index == spanCount - 1) {
                
            }
        }

        private bool requiresLayout;

        public abstract class TextLayoutPolygon {

            public abstract bool LineCast(float y, out Vector2 intersection);

            public abstract Rect GetBounds();

        }
        
        public void RunLayout(float width = float.MaxValue) {
            
            if (!requiresLayout) return;
            
            StructList<LineInfo> lines = StructList<LineInfo>.Get();
            LineInfo currentLine = new LineInfo();
            
            // cast line through shape from top bottom and center of current line
            // find right-most intersection point use that as result

            WordInfo2[] wordInfos = wordInfoList.array;
            
            TextLayoutPolygon[] polygons = new TextLayoutPolygon[0]; // sorted by max y

            TextLayoutPolygon GetNearestPolygon(float x, float y) {
                return null;
            }
            
            for (int i = 0; i < spanCount; i++) {
                TextSpan2 span = spans[i];
                FontAsset asset = span.textStyle.fontAsset;

                float scale = (span.textStyle.fontSize / asset.faceInfo.PointSize) * asset.faceInfo.Scale;
                float lh = (asset.faceInfo.Ascender - asset.faceInfo.Descender) * scale;

                int start = span.wordStart;
                int end = span.wordEnd;
                
                for (int w = start; w < end; w++) {

                    WordInfo2 wordInfo = wordInfos[w];

                    switch (wordInfo.type) {
                        
                        case WordType.Whitespace:
                            break;
                        
                        case WordType.NewLine:
                            break;
                        
                        case WordType.Normal:
                            
                            TextLayoutPolygon polygon = GetNearestPolygon(0, 0);
                            if (polygon != null) {
                                // line cast
                            }
                            
                            // GetNearestTextPolygon(x ,y);
                            // try to fit word on current line
                            if (wordInfo.width > width) {
                                // can't fit on current line, maybe break the word
                            }
                            
                            
                            
                            break;
                        case WordType.SoftHyphen:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                }  
                
            }

            void CompleteLine() {
                // compute max ascender / descender
                // do line offset
                // apply geometry offset to words (if not size only pass)
                // 
            }
            
        }
        
        // positions are not transformed by xAdvance or line position
        public void UpdateGeometry() {
            int characterCount = characterInfoList.size;
            CharInfo2[] charInfos = characterInfoList.array;

            int geometryIdx = 0;

            for (int i = 0; i < spanCount; i++) {
                TextSpan2 span = spans[i];
                int start = 0; //span.characterStart;
                int end = 0; //span.characterEnd;
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

        private static bool IsWhitespace(char c) {
            return c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');
        }

    }

    public struct CharInfo2 {

        public Vector2 topLeft;
        public Vector2 bottomRight;
        public int wordIndex;
        public int lineIndex;
        public float scale;
        public Vector2 shear;
        public int character;
        public TextGlyph glyph;
        public GlyphValueRecord glyphAdjustment;
        public int spanIndex;
        public Vector2 texCoord;

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