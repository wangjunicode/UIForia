using System;
using System.Collections.Generic;
using System.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;

namespace UIForia {

    [Serializable]
    public struct FaceInfo {

        public float PointSize;
        public float Scale;
        public int CharacterCount;
        public float LineHeight;
        public float Baseline;
        public float Ascender;
        public float CapHeight;
        public float Descender;
        public float CenterLine;
        public float SuperscriptOffset;
        public float SubscriptOffset;
        public float SubSize;
        public float Underline;
        public float UnderlineThickness;
        public float strikethrough;
        public float strikethroughThickness;
        public float TabWidth;
        public float Padding;
        public float AtlasWidth;
        public float AtlasHeight;

    }

    [Serializable]
    public struct UIForiaGlyph {

        public int codepoint;
        public float x;
        public float y;
        public float width;
        public float height;
        public float xOffset;
        public float yOffset;
        public float xAdvance;
        public float scale;

    }

    // [Serializable]
    // public class DEPRECATE_TMP_Glyph {
    //
    //     public int id;
    //     public float x;
    //     public float y;
    //     public float width;
    //     public float height;
    //     public float xOffset;
    //     public float yOffset;
    //     public float xAdvance;
    //     public float scale;
    //
    //     public static DEPRECATE_TMP_Glyph Clone(DEPRECATE_TMP_Glyph source) {
    //         DEPRECATE_TMP_Glyph copy = new DEPRECATE_TMP_Glyph();
    //
    //         copy.id = source.id;
    //         copy.x = source.x;
    //         copy.y = source.y;
    //         copy.width = source.width;
    //         copy.height = source.height;
    //         copy.xOffset = source.xOffset;
    //         copy.yOffset = source.yOffset;
    //         copy.xAdvance = source.xAdvance;
    //         copy.scale = source.scale;
    //
    //         return copy;
    //     }
    //
    // }

    public unsafe struct FontAssetInfo {

        public FaceInfo faceInfo;
        public float gradientScale;
        public UntypedIntMap* glyphMapState; // holds index to glyphs list
        public UntypedIntMap* kerningMapState; // holds actual kerning value
        public int atlasWidth;
        public int atlasHeight;

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

    public class FontAsset : ScriptableObject {

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

        public void OnEnable() {
            BuildCharacterDictionary();
        }

        public unsafe FontAssetInfo GetFontInfo() {
            return new FontAssetInfo() {
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
                temp_charInfo.height = faceInfo.Ascender - faceInfo.Descender;
                temp_charInfo.yOffset = faceInfo.Ascender;
                temp_charInfo.scale = 1;
            }
            else {
                temp_charInfo = new UIForiaGlyph();
                temp_charInfo.codepoint = 32;
                temp_charInfo.x = 0;
                temp_charInfo.y = 0;
                temp_charInfo.width = faceInfo.Ascender / 5;
                temp_charInfo.height = faceInfo.Ascender - faceInfo.Descender;
                temp_charInfo.xOffset = 0;
                temp_charInfo.yOffset = faceInfo.Ascender;
                temp_charInfo.xAdvance = faceInfo.PointSize / 4;
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
                lineFeed.x = 0;
                lineFeed.y = 0;
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
                tab.x = copy.x;
                tab.y = copy.y;
                tab.width = copy.width * tabSize + (copy.xAdvance - copy.width) * (tabSize - 1);
                tab.height = copy.height;
                tab.xOffset = copy.xOffset;
                tab.yOffset = copy.yOffset;
                tab.xAdvance = copy.xAdvance * tabSize;
                tab.scale = 1;
            }

            // Tab Width is using the same xAdvance as space (32).
            faceInfo.TabWidth = characterDictionary.GetOrDefault(9).xAdvance;

            // Set Cap Height
            if (faceInfo.CapHeight == 0 && characterDictionary.ContainsKey(72)) {
                faceInfo.CapHeight = characterDictionary.GetOrDefault(72).yOffset;
            }

            // Adjust Font Scale for compatibility reasons
            if (faceInfo.Scale == 0) {
                faceInfo.Scale = 1.0f;
            }

            // Set Strikethrough Offset (if needed)
            if (faceInfo.strikethrough == 0) {
                faceInfo.strikethrough = faceInfo.CapHeight / 2.5f;
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
                defaultAsset = Resources.Load<FontAsset>("UIForiaDefaultFont SDF");
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