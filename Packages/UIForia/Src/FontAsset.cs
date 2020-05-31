using System;
using System.Collections.Generic;
using System.Text;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    [Serializable]
    public struct FaceInfo {

        // public string Name;
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
    public class DEPRECATE_TMP_Glyph {

        public int id;
        public float x;
        public float y;
        public float width;
        public float height;
        public float xOffset;
        public float yOffset;
        public float xAdvance;
        public float scale;
        
        public static DEPRECATE_TMP_Glyph Clone(DEPRECATE_TMP_Glyph source)
        {
            DEPRECATE_TMP_Glyph copy = new DEPRECATE_TMP_Glyph();

            copy.id = source.id;
            copy.x = source.x;
            copy.y = source.y;
            copy.width = source.width;
            copy.height = source.height;
            copy.xOffset = source.xOffset;
            copy.yOffset = source.yOffset;
            copy.xAdvance = source.xAdvance;
            copy.scale = source.scale;

            return copy;
        }
    }

    public class FontAsset : ScriptableObject {

        public string name;
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
        private static FontAsset defaultAsset;

        public IntMap<TextKerningPair> kerningDictionary;
        public IntMap<TextGlyph> characterDictionary;

        public TextKerningPair[] kerningPairs; // todo -- kill
        public DEPRECATE_TMP_Glyph[] textGlyphList;

        public byte tabSize = 10;

        public void OnEnable() {
            // if (tmpFontAsset.kerningDictionary == null) {
            //     tmpFontAsset.ReadFontDefinition();
            // }
            // this.textMeshProFont = tmpFontAsset;
            // this.name = tmpFontAsset.name;
            // this.id = tmpFontAsset.GetInstanceID();
            // this.faceInfo = tmpFontAsset.fontInfo;
            // this.atlas = tmpFontAsset.atlas;
            // this.weights = tmpFontAsset.fontWeights;
            // this.boldSpacing = tmpFontAsset.boldSpacing;
            // this.gradientScale = tmpFontAsset.material.GetFloat(ShaderUtilities.ID_GradientScale);
            // this.kerningDictionary = ConvertKerning(tmpFontAsset.kerningDictionary);
            // this.characterDictionary = ConvertCharacters(tmpFontAsset.characterDictionary);
            // this.boldStyle = tmpFontAsset.boldStyle;
            // this.normalStyle = tmpFontAsset.normalStyle;
            // this.normalSpacingOffset = tmpFontAsset.normalSpacingOffset;
            // this.italicStyle = tmpFontAsset.italicStyle;
            // this.weightNormal = tmpFontAsset.normalStyle;
            // this.weightBold = tmpFontAsset.boldStyle;

            BuildCharacterDictionary();

        }

        private void BuildCharacterDictionary() {

            Dictionary<int, DEPRECATE_TMP_Glyph> dictionary = new Dictionary<int, DEPRECATE_TMP_Glyph>();

            for (int i = 0; i < textGlyphList.Length; i++) {
                DEPRECATE_TMP_Glyph glyph = textGlyphList[i];

                if (!dictionary.ContainsKey(glyph.id)) {
                    dictionary.Add(glyph.id, glyph);
                }

            }

            DEPRECATE_TMP_Glyph temp_charInfo = null;

            if (dictionary.ContainsKey(32)) {
                dictionary[32].width = dictionary[32].xAdvance; // m_fontInfo.Ascender / 5;
                dictionary[32].height = faceInfo.Ascender - faceInfo.Descender;
                dictionary[32].yOffset = faceInfo.Ascender;
                dictionary[32].scale = 1;
            }
            else {
                //Debug.Log("Adding Character 32 (Space) to Dictionary for Font (" + faceInfo.Name + ").");
                temp_charInfo = new DEPRECATE_TMP_Glyph();
                temp_charInfo.id = 32;
                temp_charInfo.x = 0;
                temp_charInfo.y = 0;
                temp_charInfo.width = faceInfo.Ascender / 5;
                temp_charInfo.height = faceInfo.Ascender - faceInfo.Descender;
                temp_charInfo.xOffset = 0;
                temp_charInfo.yOffset = faceInfo.Ascender;
                temp_charInfo.xAdvance = faceInfo.PointSize / 4;
                temp_charInfo.scale = 1;
                dictionary.Add(32, temp_charInfo);
            }

            // Add Non-Breaking Space (160)
            if (!dictionary.ContainsKey(160)) {
                temp_charInfo = DEPRECATE_TMP_Glyph.Clone(dictionary[32]);
                dictionary.Add(160, temp_charInfo);
            }

            // Add Zero Width Space (8203)
            if (!dictionary.ContainsKey(8203)) {
                temp_charInfo = DEPRECATE_TMP_Glyph.Clone(dictionary[32]);
                temp_charInfo.width = 0;
                temp_charInfo.xAdvance = 0;
                dictionary.Add(8203, temp_charInfo);
            }

            //Add Zero Width no-break space (8288)
            if (!dictionary.ContainsKey(8288)) {
                temp_charInfo = DEPRECATE_TMP_Glyph.Clone(dictionary[32]);
                temp_charInfo.width = 0;
                temp_charInfo.xAdvance = 0;
                dictionary.Add(8288, temp_charInfo);
            }

            // Add Linefeed (10)
            if (dictionary.ContainsKey(10) == false) {
                //Debug.Log("Adding Character 10 (Linefeed) to Dictionary for Font (" + m_fontInfo.Name + ").");

                temp_charInfo = new DEPRECATE_TMP_Glyph();
                temp_charInfo.id = 10;
                temp_charInfo.x = 0; // dictionary[32].x;
                temp_charInfo.y = 0; // dictionary[32].y;
                temp_charInfo.width = 10; // dictionary[32].width;
                temp_charInfo.height = dictionary[32].height;
                temp_charInfo.xOffset = 0; // dictionary[32].xOffset;
                temp_charInfo.yOffset = dictionary[32].yOffset;
                temp_charInfo.xAdvance = 0;
                temp_charInfo.scale = 1;
                dictionary.Add(10, temp_charInfo);

                if (!dictionary.ContainsKey(13))
                    dictionary.Add(13, temp_charInfo);
            }

            // Add Tab Character to Dictionary. Tab is Tab Size * Space Character Width.
            if (dictionary.ContainsKey(9) == false) {
                //Debug.Log("Adding Character 9 (Tab) to Dictionary for Font (" + m_fontInfo.Name + ").");

                temp_charInfo = new DEPRECATE_TMP_Glyph();
                temp_charInfo.id = 9;
                temp_charInfo.x = dictionary[32].x;
                temp_charInfo.y = dictionary[32].y;
                temp_charInfo.width = dictionary[32].width * tabSize + (dictionary[32].xAdvance - dictionary[32].width) * (tabSize - 1);
                temp_charInfo.height = dictionary[32].height;
                temp_charInfo.xOffset = dictionary[32].xOffset;
                temp_charInfo.yOffset = dictionary[32].yOffset;
                temp_charInfo.xAdvance = dictionary[32].xAdvance * tabSize;
                temp_charInfo.scale = 1;
                dictionary.Add(9, temp_charInfo);
            }

            // Tab Width is using the same xAdvance as space (32).
            faceInfo.TabWidth = dictionary[9].xAdvance;

            // Set Cap Height
            if (faceInfo.CapHeight == 0 && dictionary.ContainsKey(72)) {
                faceInfo.CapHeight = dictionary[72].yOffset;
            }

            // Adjust Font Scale for compatibility reasons
            if (faceInfo.Scale == 0) {
                faceInfo.Scale = 1.0f;
            }

            // Set Strikethrough Offset (if needed)
            if (faceInfo.strikethrough == 0) {
                faceInfo.strikethrough = faceInfo.CapHeight / 2.5f;
            }

            kerningDictionary = new IntMap<TextKerningPair>(kerningPairs?.Length ?? 0);

            if (kerningPairs != null) {

                for (int i = 0; i < kerningPairs.Length; i++) {
                    kerningDictionary[(int) ((kerningPairs[i].firstGlyph << 16) + kerningPairs[i].secondGlyph)] = kerningPairs[i];
                }

            }

            characterDictionary = ConvertCharacters(dictionary);

        }
        //
        // private static IntMap<TextKerningPair> ConvertKerning(Dictionary<int, KerningPair> tmpKerning) {
        //     IntMap<TextKerningPair> retn = new IntMap<TextKerningPair>(tmpKerning.Count);
        //
        //     foreach (KeyValuePair<int, KerningPair> pair in tmpKerning) {
        //         TextKerningPair tkp = new TextKerningPair();
        //         tkp.firstGlyph = pair.Value.firstGlyph;
        //         tkp.firstGlyphAdjustments = pair.Value.firstGlyphAdjustments;
        //         tkp.secondGlyph = pair.Value.secondGlyph;
        //         tkp.secondGlyphAdjustments = pair.Value.secondGlyphAdjustments;
        //         retn.Add(pair.Key, tkp);
        //     }
        //
        //     return retn;
        // }

        public static IntMap<TextGlyph> ConvertCharacters(Dictionary<int, DEPRECATE_TMP_Glyph> tmpGlyphs) {
            IntMap<TextGlyph> retn = new IntMap<TextGlyph>(tmpGlyphs.Count);
            foreach (KeyValuePair<int, DEPRECATE_TMP_Glyph> pair in tmpGlyphs) {
                DEPRECATE_TMP_Glyph tmpGlyph = pair.Value;
                TextGlyph glyph = new TextGlyph();

                glyph.id = tmpGlyph.id;
                glyph.height = tmpGlyph.height;
                glyph.width = tmpGlyph.width;
                glyph.x = tmpGlyph.x;
                glyph.y = tmpGlyph.y;
                glyph.scale = tmpGlyph.scale;
                glyph.xAdvance = tmpGlyph.xAdvance;
                glyph.xOffset = tmpGlyph.xOffset;
                glyph.yOffset = tmpGlyph.yOffset;
                retn.Add(pair.Key, glyph);
            }

            return retn;
        }

        public static FontAsset defaultFontAsset {
            get {
                if (defaultAsset != null) return defaultAsset;
                defaultAsset = Resources.Load<FontAsset>("Fonts/UIForiaDefaultFont SDF");
                return defaultAsset;
            }
        }

        public bool HasCharacter(int charPoint) {
            return characterDictionary.ContainsKey(charPoint);
        }

        public static class TMPConversionUtil {

            /// <summary>
            /// Function which returns an array that contains all the characters from a font asset.
            /// </summary>
            /// <param name="fontAsset"></param>
            /// <returns></returns>
            public static int[] GetCharactersArray(FontAsset fontAsset) {
                int[] characters = new int[fontAsset.textGlyphList.Length];

                for (int i = 0; i < fontAsset.textGlyphList.Length; i++) {
                    characters[i] = fontAsset.textGlyphList[i].id;
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
                    builder.Append((char) fontAsset.textGlyphList[i].id);
                }

                return builder.ToString();
            }

        }

    }

}