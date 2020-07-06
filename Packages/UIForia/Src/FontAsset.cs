using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;

namespace UIForia {

    [Serializable]
    public struct FaceInfo {

        public float pointSize;
        public float scale;
        public int CharacterCount;
        public float lineHeight;
        public float baseline;
        public float ascender;
        public float capHeight;
        public float descender;
        public float centerLine;
        public float SuperscriptOffset;
        public float SubscriptOffset;
        public float SubSize;
        public float underlineOffset;
        public float underlineThickness;
        public float strikethroughOffset;
        public float strikethroughThickness;
        public float tabWidth;
        public float padding;
        public float atlasWidth;
        public float atlasHeight;

    }

    [Serializable]
    public struct UIForiaGlyph {

        public int codepoint;
        public float uvX;
        public float uvY;
        public int uvWidth;
        public int uvHeight;
        public float width;
        public float height;
        public float xOffset;
        public float yOffset;
        public float xAdvance;
        public float scale;

    }

    public unsafe struct FontAssetInfo {

        public FaceInfo faceInfo;
        public float gradientScale;
        public UntypedIntMap* glyphMapState; // holds index to glyphs list
        public UntypedIntMap* kerningMapState; // holds actual kerning value
        public int atlasWidth;
        public int atlasHeight;
        public int atlasTextureId;

        public float boldStyle;
        public float normalStyle;
        public float italicStyle;
        public float boldSpacing;
        public float normalSpacingOffset;
        public float weightNormal;
        public float weightBold;

        public float GetKerning(int char0, int char1) {
            if (kerningMapState->TryGetValue(BitUtil.SetHighLowBits(char0, char1), out float value)) {
                return value;
            }

            return 0;
        }

        public bool TryGetGlyph(int charcode, out UIForiaGlyph glyph) {
            if (glyphMapState->TryGetValue(charcode, out glyph)) {
                return true;
            }

            glyph = default;
            return false;
        }

    }

    [CreateAssetMenu(fileName = "Font", menuName = "UIForia/FontAsset", order = 1)]
    public class FontAsset : ScriptableObject {

        public TMP_FontAsset convertFrom;

        internal int id;
        public float gradientScale; // sdf padding + 1
        public Texture2D atlas;
        public float weightNormal;
        public float weightBold;
        public float boldSpacing = 7f;
        public FaceInfo faceInfo;

        public float boldStyle = 0.75f;
        public float normalStyle = 0;
        public float normalSpacingOffset = 0;

        public byte italicStyle = 35;
        public byte tabSize = 10;

        private static FontAsset defaultAsset;

        [NonSerialized] public IntMap<float> kerningDictionary;
        [NonSerialized] public IntMap<UIForiaGlyph> characterDictionary;

        public TextKerningPair[] kerningPairs;
        public UIForiaGlyph[] textGlyphList;
        private static readonly int s_WeightBold = Shader.PropertyToID("_WeightBold");
        private static readonly int s_WeightNormal = Shader.PropertyToID("_WeightNormal");
        private static readonly int s_GradientScale = Shader.PropertyToID("_GradientScale");

        public void ConvertFromTMP() {
            atlas = (Texture2D)convertFrom.material.mainTexture;
            boldSpacing = convertFrom.boldSpacing;
            boldStyle = convertFrom.boldStyle;
            weightBold = convertFrom.material.GetFloat(s_WeightBold);
            weightNormal = convertFrom.material.GetFloat(s_WeightNormal);
            normalStyle = convertFrom.normalStyle;
            normalSpacingOffset = convertFrom.normalSpacingOffset;
            gradientScale = convertFrom.material.GetFloat(s_GradientScale);
            italicStyle = convertFrom.italicStyle;
            tabSize = convertFrom.tabSize;
            
            faceInfo = new FaceInfo() {
                pointSize = convertFrom.faceInfo.pointSize,
                lineHeight = convertFrom.faceInfo.lineHeight,
                strikethroughOffset = convertFrom.faceInfo.strikethroughOffset,
                ascender = convertFrom.faceInfo.ascentLine,
                descender = convertFrom.faceInfo.descentLine,
                baseline = convertFrom.faceInfo.baseline,
                padding = convertFrom.atlasPadding,
                scale = convertFrom.faceInfo.scale,
                strikethroughThickness = convertFrom.faceInfo.strikethroughThickness,
                underlineOffset = convertFrom.faceInfo.underlineOffset,
                underlineThickness = convertFrom.faceInfo.underlineThickness,
                atlasWidth = atlas.width,
                atlasHeight = atlas.height,
                capHeight = convertFrom.faceInfo.capLine,
                centerLine = convertFrom.faceInfo.meanLine,
                tabWidth = convertFrom.faceInfo.tabWidth
            };
            List<TMP_Character> table = convertFrom.characterTable;
            textGlyphList = new UIForiaGlyph[table.Count];
            for (int i = 0; i < table.Count; i++) {
                TMP_Character g = table[i];
                textGlyphList[i] = new UIForiaGlyph() {
                    codepoint = (int) g.unicode,
                    width = g.glyph.metrics.width,
                    height = g.glyph.metrics.height,
                    uvX = g.glyph.glyphRect.x,
                    uvY = g.glyph.glyphRect.y,
                    uvWidth = g.glyph.glyphRect.width,
                    uvHeight = g.glyph.glyphRect.height,
                    xAdvance = g.glyph.metrics.horizontalAdvance,
                    xOffset = g.glyph.metrics.horizontalBearingX,
                    yOffset = g.glyph.metrics.horizontalBearingY,
                    scale = 1,
                };
            }
        }
        
        public void OnEnable() {
            if (convertFrom != null) {
                ConvertFromTMP();
            }
            BuildCharacterDictionary();
        }

        public unsafe FontAssetInfo GetFontInfo() {
            return new FontAssetInfo() {
                atlasTextureId = atlas.GetHashCode(),
                glyphMapState = characterDictionary.GetState(),
                kerningMapState = kerningDictionary.GetState(),
                atlasWidth = atlas.width,
                atlasHeight = atlas.height,
                boldSpacing = boldSpacing,
                boldStyle = boldStyle,
                faceInfo = faceInfo,
                gradientScale = gradientScale,
                italicStyle = italicStyle,
                normalStyle = normalStyle,
                weightBold = weightBold,
                weightNormal = weightNormal,
                normalSpacingOffset = normalSpacingOffset,
            };
        }

        private void BuildCharacterDictionary() {

            if (textGlyphList == null) return;

            characterDictionary = new IntMap<UIForiaGlyph>(textGlyphList.Length + 10, Allocator.Persistent);

            for (int i = 0; i < textGlyphList.Length; i++) {
                UIForiaGlyph glyph = textGlyphList[i];
                characterDictionary.Add(glyph.codepoint, glyph);
            }

            UIForiaGlyph temp_charInfo = default;

            // make sure we have a space character
            if (characterDictionary.TryGetReference(32, ref temp_charInfo)) {
                temp_charInfo.width = temp_charInfo.xAdvance;
                temp_charInfo.height = faceInfo.ascender - faceInfo.descender;
                temp_charInfo.yOffset = faceInfo.ascender;
                temp_charInfo.scale = 1;
            }
            else {
                temp_charInfo = new UIForiaGlyph();
                temp_charInfo.codepoint = 32;
                temp_charInfo.uvX = 0;
                temp_charInfo.uvY = 0;
                temp_charInfo.width = faceInfo.ascender / 5;
                temp_charInfo.height = faceInfo.ascender - faceInfo.descender;
                temp_charInfo.xOffset = 0;
                temp_charInfo.yOffset = faceInfo.ascender;
                temp_charInfo.xAdvance = faceInfo.pointSize / 4;
                temp_charInfo.scale = 1;
                characterDictionary.Add(32, temp_charInfo);
            }

            // Add Non-Breaking Space (160)
            if (!characterDictionary.TryGetValue(160, out temp_charInfo)) {
                ref UIForiaGlyph g = ref characterDictionary.GetOrCreateReference(32);
                g.codepoint = 160;
            }

            // Add Zero Width Space (8203)
            if (!characterDictionary.TryGetReference(8203, ref temp_charInfo)) {
                characterDictionary.TryGetValue(32, out temp_charInfo);
                temp_charInfo.codepoint = 8203;
                temp_charInfo.width = 0;
                temp_charInfo.xAdvance = 0;
                characterDictionary.Add(8203, temp_charInfo);
            }

            //Add Zero Width no-break space (8288)
            if (!characterDictionary.ContainsKey(8288)) {
                ref UIForiaGlyph zwnbsp = ref characterDictionary.GetOrCreateReference(8288);
                zwnbsp.codepoint = 8288;
                zwnbsp.width = 0;
                zwnbsp.xAdvance = 0;
            }

            // Add Linefeed (10)
            if (characterDictionary.ContainsKey(10) == false) {

                ref UIForiaGlyph lineFeed = ref characterDictionary.GetOrCreateReference(10);
                lineFeed.codepoint = 10;
                lineFeed.uvX = 0;
                lineFeed.uvY = 0;
                lineFeed.width = 10;
                lineFeed.height = characterDictionary.GetOrDefault(32).height;
                lineFeed.xOffset = 0;
                lineFeed.yOffset = characterDictionary.GetOrDefault(32).yOffset;
                lineFeed.xAdvance = 0;
                lineFeed.scale = 1;

                if (!characterDictionary.ContainsKey(13)) {
                    lineFeed.codepoint = 13;
                    characterDictionary.Add(13, lineFeed);
                }
            }

            // Add characterDictionary Character to Dictionary. Tab is Tab Size * Space Character Width.
            if (characterDictionary.ContainsKey(9) == false) {

                UIForiaGlyph copy = characterDictionary.GetOrDefault(32);
                ref UIForiaGlyph tab = ref characterDictionary.GetOrCreateReference(9);

                tab = new UIForiaGlyph();
                tab.codepoint = 9;
                tab.uvX = copy.uvX;
                tab.uvY = copy.uvY;
                tab.width = copy.width * tabSize + (copy.xAdvance - copy.width) * (tabSize - 1);
                tab.height = copy.height;
                tab.xOffset = copy.xOffset;
                tab.yOffset = copy.yOffset;
                tab.xAdvance = copy.xAdvance * tabSize;
                tab.scale = 1;
            }

            // Tab Width is using the same xAdvance as space (32).
            faceInfo.tabWidth = characterDictionary.GetOrDefault(9).xAdvance;

            // Set Cap Height
            if (faceInfo.capHeight == 0 && characterDictionary.ContainsKey(72)) {
                faceInfo.capHeight = characterDictionary.GetOrDefault(72).yOffset;
            }

            // Adjust Font Scale for compatibility reasons
            if (faceInfo.scale == 0) {
                faceInfo.scale = 1.0f;
            }

            // Set Strikethrough Offset (if needed)
            if (faceInfo.strikethroughOffset == 0) {
                faceInfo.strikethroughOffset = faceInfo.capHeight / 2.5f;
            }

            kerningDictionary = new IntMap<float>(kerningPairs?.Length ?? 0, Allocator.Persistent);

            if (kerningPairs != null) {

                for (int i = 0; i < kerningPairs.Length; i++) {
                    kerningDictionary.Add(BitUtil.SetHighLowBits(kerningPairs[i].firstGlyph, kerningPairs[i].secondGlyph), kerningPairs[i].advance);
                }

            }

        }

        public static FontAsset defaultFontAsset {
            get {
                if (defaultAsset != null) return defaultAsset;
                defaultAsset = Resources.Load<FontAsset>("Fonts/VarelaRound-Regular SDF"); // todo -- improve this
                return defaultAsset;
            }
        }

        // public bool HasCharacter(int charPoint) {
        //     return characterDictionary.ContainsKey(charPoint);
        // }

        public static class TMPConversionUtil {

            /// <summary>
            /// Function which returns an array that contains all the characters from a font asset.
            /// </summary>
            /// <param name="fontAsset"></param>
            /// <returns></returns>
            public static int[] GetCharactersArray(FontAsset fontAsset) {
                int[] characters = new int[fontAsset.textGlyphList.Length];

                for (int i = 0; i < fontAsset.textGlyphList.Length; i++) {
                    characters[i] = fontAsset.textGlyphList[i].codepoint;
                }

                return characters;
            }

            /// <summary>
            /// Function to extract all the characters from a font asset.
            /// </summary>
            /// <param name="fontAsset"></param>
            /// <returns></returns>
            public static string GetCharacters(FontAsset fontAsset) {
                string characters = string.Empty;
                StringBuilder builder = new StringBuilder(fontAsset.textGlyphList.Length);
                for (int i = 0; i < fontAsset.textGlyphList.Length; i++) {
                    builder.Append((char) fontAsset.textGlyphList[i].codepoint);
                }

                return builder.ToString();
            }

        }

    }

}