using System;
using UIForia.Layout;
using UIForia.ListTypes;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Text {

    [BurstCompile]
    internal struct ComputeWordSizes : IJob {

        public DataList<TextChange>.Shared changes;
        public DataList<BurstTextInfo>.Shared textInfoMap;
        public DataList<FontAssetInfo>.Shared fontAssetMap;

        public void Execute() {

            TextMeasureState measureState = new TextMeasureState(Allocator.Temp);

            for (int changeIndex = 0; changeIndex < changes.size; changeIndex++) {
                
                BurstTextInfo textInfo = textInfoMap[changes[changeIndex].textInfoId];
                
                ComputeSizeInfo sizeInfo = new ComputeSizeInfo();

                measureState.Initialize(textInfo.textStyle, fontAssetMap[textInfo.textStyle.fontAssetId]);
                
                RecomputeFontInfo(ref measureState, ref sizeInfo);
                
                for (int i = 0; i < textInfo.layoutSymbolList.size; i++) {

                    ref TextLayoutSymbol layoutSymbol = ref textInfo.layoutSymbolList[i];

                    TextLayoutSymbolType type = layoutSymbol.type & ~TextLayoutSymbolType.IsBreakable;

                    if (type == TextLayoutSymbolType.Word) {
                        MeasureWord(ref layoutSymbol.wordInfo, textInfo.symbolList, ref sizeInfo, ref measureState);
                    }
                    else if (type == TextLayoutSymbolType.HorizontalSpace) {
                        // todo -- lookup the em tree for given element
                    }

                }
            }

        }

        private void MeasureWord(ref WordInfo wordInfo, List_TextSymbol symbolList, ref ComputeSizeInfo sizeInfo, ref TextMeasureState measureState) {
            float xAdvance = 0;
            float maxHeight = 0;
            int start = wordInfo.charStart;
            int end = wordInfo.charEnd;
            for (int charIndex = start; charIndex < end; charIndex++) {

                ref TextSymbol textSymbol = ref symbolList[charIndex];

                switch (textSymbol.type) {

                    case TextSymbolType.Character: {

                        if (!fontAssetMap[sizeInfo.fontAssetId].TryGetGlyph(textSymbol.charInfo.character, out UIForiaGlyph glyph)) {
                            continue; // todo -- handle missing glyphs somehow
                        }

                        float currentElementScale = sizeInfo.fontScale * sizeInfo.fontScaleMultiplier * glyph.scale;

                        // if (sizeInfo.monospacing != 0) {
                        //     xAdvance += (sizeInfo.monospacing - monoAdvance + ((sizeInfo.characterSpacing + sizeInfo.normalSpacingOffset) * currentElementScale));
                        // }


                        float kerningAdvance = 0;
                        if (charIndex + 1 < symbolList.size) {
                            kerningAdvance = fontAssetMap[sizeInfo.fontAssetId].GetKerning(textSymbol.charInfo.character, symbolList[charIndex + 1].charInfo.character);
                        }

                        float padding = sizeInfo.padding;
                        float stylePadding = sizeInfo.stylePadding;

                        textSymbol.charInfo.topLeft.x = xAdvance + (glyph.xOffset - padding - stylePadding) * currentElementScale;
                        // i think ascender usage here is wrong
                        textSymbol.charInfo.topLeft.y = sizeInfo.fontBaseLineOffset + (sizeInfo.fontAscender - (glyph.yOffset + padding)) * currentElementScale;

                        textSymbol.charInfo.bottomRight.x = textSymbol.charInfo.topLeft.x + (glyph.width + padding * 2 + stylePadding * 2) * currentElementScale;
                        textSymbol.charInfo.bottomRight.y = textSymbol.charInfo.topLeft.y + (glyph.height + padding * 2) * currentElementScale;

                        textSymbol.charInfo.topLeftUv.x = (glyph.x - padding - stylePadding) / sizeInfo.atlasWidth;
                        textSymbol.charInfo.topLeftUv.y = 1 - (glyph.y + padding + stylePadding + glyph.height) / sizeInfo.atlasHeight;

                        textSymbol.charInfo.bottomRightUv.x = (glyph.x + padding + stylePadding + glyph.width) / sizeInfo.atlasWidth;
                        textSymbol.charInfo.bottomRightUv.y = 1 - (glyph.y - padding - stylePadding) / sizeInfo.atlasHeight;

                        textSymbol.charInfo.shearTop = sizeInfo.shear * ((glyph.yOffset + padding + stylePadding) * currentElementScale);
                        textSymbol.charInfo.shearBottom = sizeInfo.shear * ((glyph.yOffset + padding + stylePadding) * currentElementScale);

                        // todo -- im not sure kerning is correct here, it has very small values and I think it might be an x position adjustment or need to be scaled 
                        
                        float w = (glyph.xAdvance
                                  * sizeInfo.boldAdvanceMultiplier
                                  + sizeInfo.characterSpacing
                                  + sizeInfo.normalSpacingOffset
                                  + kerningAdvance) *currentElementScale;
                        
                        xAdvance += w;

                        maxHeight = math.max(maxHeight, textSymbol.charInfo.bottomRight.y - textSymbol.charInfo.topLeft.y);

                        break;
                    }

                    // todo -- loop until we hit a character again before recomputing
                    case TextSymbolType.FontPush: {
                        if (TryResolveFontId(textSymbol.fontId, out FontAssetInfo fontAssetInfo)) {
                            measureState.PushFont(fontAssetInfo);
                            RecomputeFontInfo(ref measureState, ref sizeInfo);
                        }

                        break;
                    }

                    case TextSymbolType.FontPop:
                        measureState.PopFont();
                        RecomputeFontInfo(ref measureState, ref sizeInfo);
                        break;

                    case TextSymbolType.FontSizePush:
                        measureState.PushFontSize(textSymbol.fontSize);
                        RecomputeFontInfo(ref measureState, ref sizeInfo);
                        break;

                    case TextSymbolType.FontSizePop:
                        measureState.PopFontSize();
                        RecomputeFontInfo(ref measureState, ref sizeInfo);
                        break;

                    case TextSymbolType.NoBreakPush:
                    case TextSymbolType.NoBreakPop:
                    case TextSymbolType.HorizontalSpace: {
                        break;
                    }

                    case TextSymbolType.ColorPush:
                    case TextSymbolType.ColorPop:
                    case TextSymbolType.TextTransformPush:
                    case TextSymbolType.TextTransformPop:
                    default:
                        break;
                }

            }

            wordInfo.width = xAdvance;
            wordInfo.height = maxHeight;
        }

        private int GetNextCharIndex(BurstTextInfo textInfo, int charIndex, int end) {
            throw new NotImplementedException();
        }

        private bool TryResolveFontId(int fontId, out FontAssetInfo fontAssetInfo) {
            if (fontId >= 1 && fontId < fontAssetMap.size) {
                fontAssetInfo = fontAssetMap[fontId];
                return true;
            }

            fontAssetInfo = default;
            return false;
        }

        internal struct ComputeSizeInfo {

            public int fontAssetId;
            public float fontScale;
            public float fontBaseLineOffset;
            public float padding;
            public float stylePadding;
            public float fontAscender;
            public float fontDescender;
            public float fontScaleMultiplier;
            public float boldAdvanceMultiplier;
            public float atlasWidth;
            public float atlasHeight;
            public float shear;
            public float monospacing;
            public float characterSpacing;
            public float normalSpacingOffset;

        }

        public static float3 ComputeRatios(in FontAssetInfo fontAsset, in TextMeasureState textStyle) {
            float gradientScale = fontAsset.gradientScale;
            float faceDilate = textStyle.faceDilate;
            float outlineThickness = textStyle.outlineWidth;
            float outlineSoftness = textStyle.outlineSoftness;
            float weight = (fontAsset.weightNormal > fontAsset.weightBold ? fontAsset.weightNormal : fontAsset.weightBold) / 4f;
            float ratioA_t = math.max(1, weight + faceDilate + outlineThickness + outlineSoftness);
            float ratioA = (gradientScale - 1f) / (gradientScale * ratioA_t);

            float glowOffset = textStyle.glowOffset;
            float glowOuter = textStyle.glowOuter;
            float ratioBRange = (weight + faceDilate) * (gradientScale - 1f);

            float ratioB_t = math.max(1, glowOffset + glowOuter);
            float ratioB = math.max(0, gradientScale - 1 - ratioBRange) / (gradientScale * ratioB_t);
            float underlayOffsetX = textStyle.underlayX;
            float underlayOffsetY = textStyle.underlayY;
            float underlayDilate = textStyle.underlayDilate;
            float underlaySoftness = textStyle.underlaySoftness;

            float ratioCRange = (weight + faceDilate) * (gradientScale - 1);
            float ratioC_t = math.max(1, math.max(math.abs(underlayOffsetX), math.abs(underlayOffsetY)) + underlayDilate + underlaySoftness);

            float ratioC = math.max(0, gradientScale - 1f - ratioCRange) / (gradientScale * ratioC_t);

            return new float3(ratioA, ratioB, ratioC);
        }

        private void RecomputeFontInfo(ref TextMeasureState state, ref ComputeSizeInfo sizeInfo) {

            ref FontAssetInfo fontAsset = ref state.fontAssetInfo;

            float fontSize = state.fontSize.value; // todo -- support em etc
            
            float smallCapsMultiplier = (state.textTransform == TextTransform.SmallCaps) ? 0.8f : 1f;
            float fontScale = fontSize * smallCapsMultiplier / fontAsset.faceInfo.PointSize * fontAsset.faceInfo.Scale;

            float3 sdfRatios = ComputeRatios(state.fontAssetInfo, state);

            float padding = TextUtil.GetPadding(fontAsset.gradientScale, state, sdfRatios);
            float gradientScale = fontAsset.gradientScale;
            float boldAdvanceMultiplier = 1;
            float stylePadding;

            if ((state.fontStyle & FontStyle.Bold) != 0) {
                stylePadding = state.fontAssetInfo.boldStyle / 4.0f * gradientScale * sdfRatios.x;
                if (stylePadding + padding > gradientScale) {
                    padding = gradientScale - stylePadding;
                }

                boldAdvanceMultiplier = 1 + state.fontAssetInfo.boldSpacing * 0.01f;
            }
            else {
                stylePadding = fontAsset.normalStyle / 4.0f * gradientScale;
                if (stylePadding + padding > gradientScale) {
                    padding = gradientScale - stylePadding;
                }
            }

            float fontScaleMultiplier = state.scriptStyle == TextScript.SuperScript || state.scriptStyle == TextScript.SubScript
                ? fontAsset.faceInfo.SubSize
                : 1;
            
            float fontBaseLineOffset = fontAsset.faceInfo.Baseline * fontScale * fontScaleMultiplier * fontAsset.faceInfo.Scale;

            sizeInfo.padding = padding;
            sizeInfo.stylePadding = stylePadding;
            sizeInfo.monospacing = 0;
            sizeInfo.shear = 0;
            sizeInfo.atlasWidth = fontAsset.atlasWidth;
            sizeInfo.atlasHeight = fontAsset.atlasHeight;
            sizeInfo.fontAscender =  fontAsset.faceInfo.Ascender;
            sizeInfo.fontDescender = 0;
            sizeInfo.characterSpacing = 0;
            sizeInfo.fontScale = fontScale;
            sizeInfo.boldAdvanceMultiplier = boldAdvanceMultiplier;
            sizeInfo.fontScaleMultiplier = fontScaleMultiplier;
            sizeInfo.fontBaseLineOffset = fontBaseLineOffset;

        }

    }

}