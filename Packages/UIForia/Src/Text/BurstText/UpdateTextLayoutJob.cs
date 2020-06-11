using System;
using UIForia.Layout;
using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Text {

    [BurstCompile]
    public unsafe struct UpdateTextLayoutJob : IJob, IVertigoParallel {

        public float viewportWidth;
        public float viewportHeight;

        internal ElementTable<EmValue> emTable;
        internal DataList<TextChange>.Shared textChanges;
        internal DataList<BurstTextInfo>.Shared textInfoMap;
        internal DataList<FontAssetInfo>.Shared fontAssetMap;

        public ParallelParams parallel { get; set; }

        public void Execute() {
            Run(0, textChanges.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        private void Run(int start, int end) {

            DataList<TextSymbol> symbolBuffer = new DataList<TextSymbol>(128, Allocator.Temp);
            DataList<TextLayoutSymbol> layoutBuffer = new DataList<TextLayoutSymbol>(128, Allocator.Temp);
            TextMeasureState measureState = new TextMeasureState(Allocator.Temp);

            for (int i = start; i < end; i++) {

                ref BurstTextInfo textInfo = ref textInfoMap[textChanges[i].textInfoId];

                symbolBuffer.size = 0;

                TextUtil.ProcessWhiteSpace(textInfo.textStyle.whitespaceMode, textInfo.symbolList.array, textInfo.symbolList.size, ref symbolBuffer);

                if (symbolBuffer.size != textInfo.symbolList.size) {
                    textInfo.symbolList.CopyFrom(symbolBuffer.GetArrayPointer(), symbolBuffer.size);
                }

            }

            for (int i = start; i < end; i++) {

                ref BurstTextInfo textInfo = ref textInfoMap[textChanges[i].textInfoId];

                layoutBuffer.size = 0;
                TextUtil.CreateLayoutSymbols(textInfo.symbolList.array, textInfo.symbolList.size, ref layoutBuffer);

                if (textInfo.layoutSymbolList.array == null) {
                    textInfo.layoutSymbolList = new List_TextLayoutSymbol(layoutBuffer.size, Allocator.Persistent);
                }

                textInfo.layoutSymbolList.CopyFrom(layoutBuffer.GetArrayPointer(), layoutBuffer.size);

            }

            for (int changeIndex = start; changeIndex < end; changeIndex++) {

                ElementId elementId = textChanges[changeIndex].elementId;
                BurstTextInfo textInfo = textInfoMap[textChanges[changeIndex].textInfoId];

                ComputeSizeInfo sizeInfo = new ComputeSizeInfo();

                measureState.Initialize(emTable[elementId].resolvedValue, textInfo.textStyle, fontAssetMap[textInfo.textStyle.fontAssetId]);

                RecomputeFontInfo(elementId, ref measureState, ref sizeInfo);

                for (int i = 0; i < textInfo.layoutSymbolList.size; i++) {

                    ref TextLayoutSymbol layoutSymbol = ref textInfo.layoutSymbolList[i];

                    TextLayoutSymbolType type = layoutSymbol.type & ~TextLayoutSymbolType.IsBreakable;

                    if (type == TextLayoutSymbolType.Word) {
                        MeasureWord(elementId, ref layoutSymbol.wordInfo, textInfo.symbolList, ref sizeInfo, ref measureState);
                    }
                    else if (type == TextLayoutSymbolType.HorizontalSpace) {
                        // todo -- lookup the em tree for given element
                    }

                }
            }

            symbolBuffer.Dispose();
            layoutBuffer.Dispose();
            measureState.Dispose();
        }

        private void MeasureWord(ElementId elementId, ref WordInfo wordInfo, List_TextSymbol symbolList, ref ComputeSizeInfo sizeInfo, ref TextMeasureState measureState) {
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
                                   + kerningAdvance) * currentElementScale;

                        xAdvance += w;

                        maxHeight = math.max(maxHeight, textSymbol.charInfo.bottomRight.y - textSymbol.charInfo.topLeft.y);

                        break;
                    }

                    // todo -- loop until we hit a character again before recomputing
                    case TextSymbolType.FontPush: {
                        if (TryResolveFontId(textSymbol.fontId, out FontAssetInfo fontAssetInfo)) {
                            measureState.PushFont(fontAssetInfo);
                            RecomputeFontInfo(elementId, ref measureState, ref sizeInfo);
                        }

                        break;
                    }

                    case TextSymbolType.FontPop:
                        measureState.PopFont();
                        RecomputeFontInfo(elementId, ref measureState, ref sizeInfo);
                        break;

                    case TextSymbolType.FontSizePush:
                        measureState.PushFontSize(textSymbol.length, viewportWidth, viewportHeight);
                        RecomputeFontInfo(elementId, ref measureState, ref sizeInfo);
                        break;

                    case TextSymbolType.FontSizePop:
                        measureState.PopFontSize();
                        RecomputeFontInfo(elementId, ref measureState, ref sizeInfo);
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

        private float ComputeFontSize(ElementId elementId, UIFixedLength inputSize) {
            switch (inputSize.unit) {

                default:
                case UIFixedUnit.Unset:
                case UIFixedUnit.Pixel:
                    return inputSize.value;

                case UIFixedUnit.Percent:
                    return emTable[elementId].resolvedValue * (100 * inputSize.value);

                case UIFixedUnit.Em:
                    return emTable[elementId].resolvedValue * inputSize.value;

                case UIFixedUnit.ViewportWidth:
                    return viewportWidth * inputSize.value;

                case UIFixedUnit.ViewportHeight:
                    return viewportHeight * inputSize.value;
            }
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

        private static float3 ComputeRatios(in FontAssetInfo fontAsset, in TextMeasureState textStyle) {
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

        private void RecomputeFontInfo(ElementId elementId, ref TextMeasureState state, ref ComputeSizeInfo sizeInfo) {

            ref FontAssetInfo fontAsset = ref state.fontAssetInfo;

            float fontSize = state.fontSize;

            float smallCapsMultiplier = (state.textTransform == TextTransform.SmallCaps) ? 0.8f : 1f;
            float fontScale = fontSize * smallCapsMultiplier / fontAsset.faceInfo.PointSize * fontAsset.faceInfo.Scale;

            float3 sdfRatios = ComputeRatios(state.fontAssetInfo, state);

            float padding = TextUtil.GetPadding(fontAsset.gradientScale, state, sdfRatios);
            float gradientScale = fontAsset.gradientScale;
            float boldAdvanceMultiplier = 1; // todo -- this shouldn't be 1
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
            sizeInfo.fontAscender = fontAsset.faceInfo.Ascender;
            sizeInfo.fontDescender = 0;
            sizeInfo.characterSpacing = 0;
            sizeInfo.fontScale = fontScale;
            sizeInfo.boldAdvanceMultiplier = boldAdvanceMultiplier;
            sizeInfo.fontScaleMultiplier = fontScaleMultiplier;
            sizeInfo.fontBaseLineOffset = fontBaseLineOffset;

        }

    }

}