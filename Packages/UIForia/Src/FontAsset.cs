using System.Collections.Generic;
using TMPro;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public class FontAsset {

        public readonly int id;
        public readonly string name;
        public readonly float gradientScale;
        public readonly float scaleX;
        public readonly float scaleY;
        public readonly Texture2D atlas;
        public readonly float weightNormal;
        public readonly float weightBold;
        public readonly TMP_FontWeights[] weights;
        public readonly float boldSpacing;
        public readonly FaceInfo faceInfo;
        public readonly ManagedIntMap<TextKerningPair> kerningDictionary;
        public readonly ManagedIntMap<TextGlyph> characterDictionary;
        public readonly float boldStyle;
        public readonly float normalStyle;
        public readonly float normalSpacingOffset;
        public readonly byte italicStyle;
        private static FontAsset defaultAsset;
        public TMP_FontAsset textMeshProFont;
        
        public FontAsset(TMP_FontAsset tmpFontAsset) {
            if (tmpFontAsset.kerningDictionary == null) {
                tmpFontAsset.ReadFontDefinition();
            }
            this.textMeshProFont = tmpFontAsset;
            this.name = tmpFontAsset.name;
            this.id = tmpFontAsset.GetInstanceID();
            this.faceInfo = tmpFontAsset.fontInfo;
            this.atlas = tmpFontAsset.atlas;
            this.weights = tmpFontAsset.fontWeights;
            this.boldSpacing = tmpFontAsset.boldSpacing;
            this.gradientScale = tmpFontAsset.material.GetFloat(ShaderUtilities.ID_GradientScale);
            this.kerningDictionary = ConvertKerning(tmpFontAsset.kerningDictionary);
            this.characterDictionary = ConvertCharacters(tmpFontAsset.characterDictionary);
            this.boldStyle = tmpFontAsset.boldStyle;
            this.normalStyle = tmpFontAsset.normalStyle;
            this.normalSpacingOffset = tmpFontAsset.normalSpacingOffset;
            this.italicStyle = tmpFontAsset.italicStyle;
            this.weightNormal = tmpFontAsset.normalStyle;
            this.weightBold = tmpFontAsset.boldStyle;
        }

        private static ManagedIntMap<TextKerningPair> ConvertKerning(Dictionary<int, KerningPair> tmpKerning) {
            ManagedIntMap<TextKerningPair> retn = new ManagedIntMap<TextKerningPair>(tmpKerning.Count);

            foreach (KeyValuePair<int, KerningPair> pair in tmpKerning) {
                TextKerningPair tkp = new TextKerningPair();
                tkp.firstGlyph = pair.Value.firstGlyph;
                tkp.firstGlyphAdjustments = pair.Value.firstGlyphAdjustments;
                tkp.secondGlyph = pair.Value.secondGlyph;
                tkp.secondGlyphAdjustments = pair.Value.secondGlyphAdjustments;
                retn.Add(pair.Key, tkp);
            }

            return retn;
        }

        private static ManagedIntMap<TextGlyph> ConvertCharacters(Dictionary<int, TMP_Glyph> tmpGlyphs) {
            ManagedIntMap<TextGlyph> retn = new ManagedIntMap<TextGlyph>(tmpGlyphs.Count);
            foreach (KeyValuePair<int, TMP_Glyph> pair in tmpGlyphs) {
                TMP_Glyph tmpGlyph = pair.Value;
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
                defaultAsset = new FontAsset(TMP_FontAsset.defaultFontAsset);
                return defaultAsset;
            }
        }

        public bool HasCharacter(int charPoint) {
            return characterDictionary.ContainsKey(charPoint);
        }

    }

}