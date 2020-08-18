using System;
using System.Text;
using UIForia.Graphics;
using UIForia.Layout;
using UIForia.ListTypes;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {

    public static class TextUtil {

        public static StringBuilder StringBuilder = new StringBuilder(1024);

        private const int k_Space = ' ';
        private const int k_Tab = '\t';
        private const int k_CarriageReturn = '\r';
        private const int k_Ellipsis = '\x0085';
        private const int k_NewLine = '\n';

        // whitespace processing needs to happen in two phases. the first is where we collapse whitespace and handle new lines
        // the second is what to do with trailing space and wrapping.
        public static int ProcessWhitespace(WhitespaceMode whitespaceMode, ref char[] buffer, char[] input, int inputSize = -1) {
            if (inputSize < 0) inputSize = input.Length;

            if (inputSize == 0) {
                return 0;
            }

            bool collapseSpaceAndTab = (whitespaceMode & WhitespaceMode.CollapseWhitespace) != 0;
            bool preserveNewLine = (whitespaceMode & WhitespaceMode.PreserveNewLines) != 0;
            bool trimStart = (whitespaceMode & WhitespaceMode.TrimStart) != 0;
            bool trimEnd = (whitespaceMode & WhitespaceMode.TrimEnd) != 0;

            bool collapsing = false;

            if (buffer == null) {
                buffer = ArrayPool<char>.GetMinSize(inputSize);
            }

            if (buffer.Length < inputSize) {
                ArrayPool<char>.Resize(ref buffer, inputSize);
            }

            int writeIndex = 0;
            int start = 0;
            int end = inputSize;

            if (trimStart) {
                for (int i = 0; i < end; i++) {
                    char c = input[i];

                    bool isWhiteSpace = c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');

                    if (!isWhiteSpace) {
                        start = i;
                        break;
                    }
                }
            }

            if (trimEnd) {
                for (int i = end - 1; i >= start; i--) {
                    char c = input[i];

                    bool isWhiteSpace = c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');

                    if (!isWhiteSpace) {
                        end = i + 1;
                        break;
                    }
                }
            }

            for (int i = start; i < end; i++) {
                char c = input[i];

                if (c == '\n' && !preserveNewLine) {
                    continue;
                }

                bool isWhiteSpace = c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');

                if (c == '\n') {
                    if (preserveNewLine) {
                        buffer[writeIndex++] = c;
                        continue;
                    }
                }

                if (collapsing) {
                    if (!isWhiteSpace) {
                        buffer[writeIndex++] = c;
                        collapsing = false;
                    }
                }
                else if (isWhiteSpace) {
                    collapsing = collapseSpaceAndTab;
                    buffer[writeIndex++] = ' ';
                }
                else {
                    buffer[writeIndex++] = c;
                }
            }

            return writeIndex;
        }

        public static StructList<WordInfo> BreakIntoWords(char[] buffer, int bufferSize = -1) {
            return BreakIntoWords(StructList<WordInfo>.Get(), buffer, bufferSize);
        }

        public static StructList<WordInfo> BreakIntoWords(StructList<WordInfo> retn, char[] buffer, int bufferSize = -1) {
            if (retn == null) {
                retn = new StructList<WordInfo>();
            }

            if (bufferSize < 0) bufferSize = buffer.Length;
            retn.size = 0;

            if (bufferSize == 0) {
                return retn;
            }

            WordInfo currentWord = new WordInfo();
            WordType previousType = WordType.Normal;

            char c = buffer[0];

            if (c == '\n') {
                previousType = WordType.NewLine;
            }
            else if (c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085')) {
                previousType = WordType.Whitespace;
            }
            else if (c == 0xAD) {
                previousType = WordType.SoftHyphen;
            }
            else {
                previousType = WordType.Normal;
            }

            currentWord.type = previousType;
            currentWord.charStart = 0;
            currentWord.charEnd = 1;

            for (int i = 1; i < bufferSize; i++) {
                c = buffer[i];

                WordType type = WordType.Normal;

                if (c == '\n') {
                    type = WordType.NewLine;
                }
                else if (c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085')) {
                    type = WordType.Whitespace;
                }
                else if (c == 0xAD) {
                    type = WordType.SoftHyphen;
                }

                if (type == previousType) {
                    if (type == WordType.NewLine) {
                        retn.Add(currentWord);
                        currentWord.type = type;
                        currentWord.charStart = i;
                        currentWord.charEnd = i + 1;
                    }
                    else {
                        currentWord.charEnd++;
                    }
                }
                else {
                    retn.Add(currentWord);
                    currentWord.type = type;
                    currentWord.charStart = i;
                    currentWord.charEnd = i + 1;
                }

                previousType = type;
            }

            if (currentWord.charEnd > 0) {
                retn.Add(currentWord);
            }

            return retn;
        }

        public static void TransformText(TextTransform transform, char[] buffer, int count = -1) {
            if (count < 0) count = buffer.Length;

            switch (transform) {
                case TextTransform.UpperCase:
                case TextTransform.SmallCaps:
                    for (int i = 0; i < count; i++) {
                        buffer[i] = char.ToUpper(buffer[i]);
                    }

                    break;

                case TextTransform.LowerCase:
                    for (int i = 0; i < count; i++) {
                        buffer[i] = char.ToLower(buffer[i]);
                    }

                    break;

                case TextTransform.TitleCase:
                    for (int i = 0; i < count - 1; i++) {
                        if (char.IsLetter(buffer[i]) && char.IsWhiteSpace(buffer[i - 1])) {
                            buffer[i] = char.ToUpper(buffer[i]);
                        }
                    }

                    break;
            }
        }

        internal static float GetPadding(float gradientScale, in TextInfoRenderSpan textStyle, in float3 ratios) {
            float4 padding = default;

            float scaleRatio_A = ratios.x;
            float scaleRatio_B = ratios.y;
            float scaleRatio_C = ratios.z;

            float faceDilate = textStyle.faceDilate * scaleRatio_A;
            float faceSoftness = textStyle.outlineSoftness * scaleRatio_A;
            float outlineThickness = textStyle.outlineWidth * scaleRatio_A;

            float uniformPadding = outlineThickness + faceSoftness + faceDilate;

            float glowOffset = textStyle.glowOffset * scaleRatio_B;
            float glowOuter = textStyle.glowOuter * scaleRatio_B;

            float dilateOffsetGlow = faceDilate + glowOffset + glowOuter;
            uniformPadding = uniformPadding > dilateOffsetGlow ? uniformPadding : dilateOffsetGlow;

            float offsetX = textStyle.underlayX * scaleRatio_C;
            float offsetY = textStyle.underlayY * scaleRatio_C;
            float dilate = textStyle.underlayDilate * scaleRatio_C;
            float softness = textStyle.underlaySoftness * scaleRatio_C;

            // tmp does a max check here with 0, I don't think we need it though
            padding.x = faceDilate + dilate + softness - offsetX;
            padding.y = faceDilate + dilate + softness - offsetY;
            padding.z = faceDilate + dilate + softness + offsetX;
            padding.w = faceDilate + dilate + softness + offsetY;

            padding = math.max(padding, uniformPadding);

            padding.x = padding.x < 1 ? padding.x : 1;
            padding.y = padding.y < 1 ? padding.y : 1;
            padding.z = padding.z < 1 ? padding.z : 1;
            padding.w = padding.w < 1 ? padding.w : 1;

            padding *= gradientScale;

            // Set UniformPadding to the maximum value of any of its components.
            uniformPadding = padding.x > padding.y ? padding.x : padding.y;
            uniformPadding = padding.z > uniformPadding ? padding.z : uniformPadding;
            uniformPadding = padding.w > uniformPadding ? padding.w : uniformPadding;

            return uniformPadding + 1.25f;
        }

        internal static float GetPadding(float gradientScale, in TextMeasureState textStyle, in float3 ratios) {
            // not using cpu side padding anymore, gpu will apply proper padding.

            // float4 padding = default;
            //
            // float scaleRatio_A = ratios.x;
            // float scaleRatio_B = ratios.y;
            // float scaleRatio_C = ratios.z;
            //
            // float faceDilate = textStyle.faceDilate * scaleRatio_A;
            // float faceSoftness = textStyle.outlineSoftness * scaleRatio_A;
            // float outlineThickness = textStyle.outlineWidth * scaleRatio_A;
            //
            // float uniformPadding = outlineThickness + faceSoftness + faceDilate;
            //
            // float glowOffset = textStyle.glowOffset * scaleRatio_B;
            // float glowOuter = textStyle.glowOuter * scaleRatio_B;
            //
            // uniformPadding = math.max(uniformPadding, faceDilate + glowOffset + glowOuter);
            //
            // float offsetX = textStyle.underlayX * scaleRatio_C;
            // float offsetY = textStyle.underlayY * scaleRatio_C;
            // float dilate = textStyle.underlayDilate * scaleRatio_C;
            // float softness = textStyle.underlaySoftness * scaleRatio_C;
            //
            // // tmp does a max check here with 0, I don't think we need it though
            // padding.x = faceDilate + dilate + softness - offsetX;
            // padding.y = faceDilate + dilate + softness - offsetY;
            // padding.z = faceDilate + dilate + softness + offsetX;
            // padding.w = faceDilate + dilate + softness + offsetY;
            //
            // padding = math.max(padding, uniformPadding);
            //
            // padding = math.min(padding, 1);
            // padding *= gradientScale;
            //
            // // Set UniformPadding to the maximum value of any of its components.
            // uniformPadding = math.max(padding.x, padding.y);
            // uniformPadding = math.max(padding.z, uniformPadding);
            // uniformPadding = math.max(padding.w, uniformPadding);
            //
            // return uniformPadding + 1.25f;
            return 0;
        }

        public static float GetPadding(in TextDisplayData textStyle, in Vector3 ratios) {
            float4 padding = Vector4.zero;

            float scaleRatio_A = ratios.x;
            float scaleRatio_B = ratios.y;
            float scaleRatio_C = ratios.z;

            float faceDilate = textStyle.faceDilate * scaleRatio_A;
            float faceSoftness = textStyle.outlineSoftness * scaleRatio_A;
            float outlineThickness = textStyle.outlineWidth * scaleRatio_A;

            float uniformPadding = outlineThickness + faceSoftness + faceDilate;

            float glowOffset = textStyle.glowOffset * scaleRatio_B;
            float glowOuter = textStyle.glowOuter * scaleRatio_B;

            uniformPadding = math.max(uniformPadding, faceDilate + glowOffset + glowOuter);

            float offsetX = textStyle.underlayX * scaleRatio_C;
            float offsetY = textStyle.underlayY * scaleRatio_C;
            float dilate = textStyle.underlayDilate * scaleRatio_C;
            float softness = textStyle.underlaySoftness * scaleRatio_C;

            // tmp does a max check here with 0, I don't think we need it though
            padding.x = faceDilate + dilate + softness - offsetX;
            padding.y = faceDilate + dilate + softness - offsetY;
            padding.z = faceDilate + dilate + softness + offsetX;
            padding.w = faceDilate + dilate + softness + offsetY;

            padding = math.max(padding, uniformPadding);
            padding = math.min(padding, 1);
            padding *= textStyle.fontAsset.gradientScale;

            // Set UniformPadding to the maximum value of any of its components.
            uniformPadding = math.max(padding.x, padding.y);
            uniformPadding = math.max(padding.z, uniformPadding);
            uniformPadding = math.max(padding.w, uniformPadding);

            return uniformPadding + 0.5f;
        }

        public static unsafe void ProcessWhiteSpace(WhitespaceMode whitespaceMode, TextSymbol* symbols, int inputSize, ref DataList<TextSymbol> buffer) {
            // changing whitespace is very rare, not worth caching pre-transform 
            bool collapseSpaceAndTab = (whitespaceMode & WhitespaceMode.CollapseWhitespace) != 0;
            bool preserveNewLine = (whitespaceMode & WhitespaceMode.PreserveNewLines) != 0;
            bool trimStart = (whitespaceMode & WhitespaceMode.TrimStart) != 0;
            bool trimEnd = (whitespaceMode & WhitespaceMode.TrimEnd) != 0;

            bool collapsing = false;

            int start = 0;
            int end = inputSize;

            buffer.EnsureAdditionalCapacity(inputSize);

            if (trimStart) {
                for (int i = 0; i < end; i++) {
                    ref TextSymbol symbol = ref symbols[i];

                    if (symbol.type != TextSymbolType.Character) {
                        buffer.AddUnchecked(symbol);
                        continue;
                    }

                    char c = (char) symbol.charInfo.character;

                    bool isWhiteSpace = c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');

                    if (!isWhiteSpace) {
                        start = i;
                        break;
                    }
                }
            }

            bool didTrimEnd = false;
            // needs to be done later since I don't know the index to write to at this point, or buffer array
            if (trimEnd) {
                for (int i = end - 1; i >= start; i--) {
                    ref TextSymbol symbol = ref symbols[i];

                    if (symbol.type != TextSymbolType.Character) {
                        continue;
                    }

                    char c = (char) symbol.charInfo.character;

                    bool isWhiteSpace = c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');

                    if (!isWhiteSpace) {
                        end = i + 1;
                        didTrimEnd = true;
                        break;
                    }
                }
            }

            for (int i = start; i < end; i++) {
                ref TextSymbol symbol = ref symbols[i];

                if (symbol.type != TextSymbolType.Character) {
                    buffer.AddUnchecked(symbol);
                    continue;
                }

                char c = (char) symbol.charInfo.character;

                if (c == '\n' && !preserveNewLine) {
                    continue;
                }

                bool isWhiteSpace = c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');

                if (c == '\n') {
                    if (preserveNewLine) {
                        buffer.AddUnchecked(symbol);
                        continue;
                    }
                }

                if (collapsing) {
                    if (!isWhiteSpace) {
                        symbol.charInfo.flags |= CharacterFlags.Visible;
                        buffer.AddUnchecked(symbol);
                        collapsing = false;
                    }
                }
                else if (isWhiteSpace) {
                    collapsing = collapseSpaceAndTab;
                    buffer.AddUnchecked(symbol);
                }
                else {
                    symbol.charInfo.flags |= CharacterFlags.Visible;
                    buffer.AddUnchecked(symbol);
                }
            }

            if (trimEnd && didTrimEnd) {
                for (int i = end; i < inputSize; i++) {
                    ref TextSymbol symbol = ref symbols[i];

                    if (symbol.type != TextSymbolType.Character) {
                        buffer.AddUnchecked(symbol);
                    }
                }
            }
        }

        [ThreadStatic] private static TextTransform[] ts_TransformStack;

        // This must be done in managed code because char.ToUpper requires some virtual calls for locale
        public static unsafe void TransformText(TextTransform transform, TextSymbol* buffer, int count) {
            int stackSize = 0;

            if (ts_TransformStack == null) {
                ts_TransformStack = new TextTransform[8];
            }

            ts_TransformStack[stackSize++] = transform;

            int prevCharacter = 0;

            for (int i = 0; i < count; i++) {
                ref TextSymbol symbol = ref buffer[i];

                if (symbol.type == TextSymbolType.Character) {
                    ref int character = ref symbol.charInfo.character;

                    switch (transform) {
                        case TextTransform.UpperCase:
                        case TextTransform.SmallCaps:
                            character = char.ToUpper((char) character);
                            break;

                        case TextTransform.LowerCase:
                            character = char.ToLower((char) character);
                            break;

                        case TextTransform.TitleCase:

                            if (char.IsLetter((char) character) && (prevCharacter == -1 || char.IsWhiteSpace((char) buffer[prevCharacter].charInfo.character))) {
                                character = char.ToUpper((char) character);
                            }

                            prevCharacter = i;
                            break;
                    }
                }
                else if (symbol.type == TextSymbolType.TextTransformPush) {
                    if (stackSize + 1 >= ts_TransformStack.Length) {
                        Array.Resize(ref ts_TransformStack, ts_TransformStack.Length * 2);
                    }

                    ts_TransformStack[stackSize++] = symbol.textTransform;
                    transform = symbol.textTransform;
                }
                else if (symbol.type == TextSymbolType.TextTransformPop) {
                    if (stackSize > 0) {
                        stackSize--;
                        transform = ts_TransformStack[stackSize - 1];
                    }
                    else {
                        transform = TextTransform.None;
                    }
                }
            }
        }

        public static unsafe void CreateLayoutSymbols(TextSymbol* symbolList, int symbolListSize, ref DataList<TextLayoutSymbol> layoutBuffer) {
            WordInfo currentWord = new WordInfo();
            WordType previousType = WordType.Normal;

            layoutBuffer.EnsureCapacity(symbolListSize);
            TypedUnsafe.MemClear(layoutBuffer.GetArrayPointer(), symbolListSize);

            // find the first character
            for (int i = 0; i < symbolListSize; i++) {
                ref TextSymbol symbol = ref symbolList[i];

                // todo -- maybe also for the horizontal space type?
                if (symbol.type != TextSymbolType.Character) {
                    continue;
                }

                int c = symbol.charInfo.character;

                if (c == k_NewLine) {
                    previousType = WordType.NewLine;
                }
                else if (c == k_Space || (c >= k_Tab && c <= k_CarriageReturn) || c == k_Ellipsis) {
                    previousType = WordType.Whitespace;
                }
                else if (c == 0xAD) {
                    previousType = WordType.SoftHyphen;
                }
                else {
                    previousType = WordType.Normal;
                }

                break;
            }

            currentWord.type = previousType;
            currentWord.charStart = 0;
            currentWord.charEnd = 0;

            bool isBreakable = true; //TextLayoutSymbolType.IsBreakable;

            for (int i = 0; i < symbolListSize; i++) {
                ref TextSymbol symbol = ref symbolList[i];

                switch (symbol.type) {
                    case TextSymbolType.Character: {
                        int c = symbol.charInfo.character;

                        WordType type = WordType.Normal;

                        if (c == k_NewLine) {
                            type = WordType.NewLine;
                        }
                        else if (c == k_Space || (c >= k_Tab && c <= k_CarriageReturn) || c == k_Ellipsis) {
                            type = WordType.Whitespace;
                        }
                        else if (c == 0xAD) {
                            type = WordType.SoftHyphen;
                        }

                        if (type == previousType) {
                            if (type == WordType.NewLine) {
                                layoutBuffer.AddUnchecked(new TextLayoutSymbol() {
                                    type = TextLayoutSymbolType.Word,
                                    isBreakable = isBreakable,
                                    wordInfo = currentWord
                                });

                                currentWord.type = type;
                                currentWord.charStart = i;
                                currentWord.charEnd = i + 1;
                            }
                            else {
                                currentWord.charEnd++;
                            }
                        }
                        else {
                            layoutBuffer.AddUnchecked(new TextLayoutSymbol() {
                                type = TextLayoutSymbolType.Word,
                                isBreakable = isBreakable,
                                wordInfo = currentWord
                            });
                            currentWord.type = type;
                            currentWord.charStart = i;
                            currentWord.charEnd = i + 1;
                        }

                        previousType = type;
                        break;
                    }

                    case TextSymbolType.NoBreakPush:
                        isBreakable = true; //0;
                        break;

                    case TextSymbolType.NoBreakPop:
                        isBreakable = false; //TextLayoutSymbolType.IsBreakable;
                        break;

                    default:
                        if (symbol.ConvertToLayoutSymbol(out TextLayoutSymbol layoutSymbol)) {
                            layoutSymbol.isBreakable = isBreakable;
                            layoutBuffer.AddUnchecked(layoutSymbol);
                        }
                        else {
                            currentWord.charEnd++;
                        }

                        continue;
                }
            }

            if (currentWord.charEnd > 0) {
                layoutBuffer.AddUnchecked(new TextLayoutSymbol() {
                    type = TextLayoutSymbolType.Word,
                    isBreakable = isBreakable,
                    wordInfo = currentWord
                });
            }
        }

        internal static void RecomputeFontInfo(ref TextMeasureState state, ref ComputeSizeInfo sizeInfo) {
            ref FontAssetInfo fontAsset = ref state.fontAssetInfo;

            float fontSize = state.fontSize;

            float smallCapsMultiplier = (state.textTransform == TextTransform.SmallCaps) ? 0.8f : 1f;
            float fontScale = fontSize * smallCapsMultiplier / fontAsset.faceInfo.pointSize * fontAsset.faceInfo.scale;

            // todo -- I think i need to bring back the ratio computation here... if I do then I can't support per-character material overrides without a layout

            //float3 sdfRatios = ComputeRatios(state.fontAssetInfo, state);

            // float padding = GetPadding(fontAsset.gradientScale, state, sdfRatios);
            //  float gradientScale = fontAsset.gradientScale;
            float boldAdvanceMultiplier = 1;

            CharacterDisplayFlags displayFlags = 0;
            // dont need style padding here
            if ((state.fontStyle & FontStyle.Bold) != 0) {
                displayFlags |= CharacterDisplayFlags.Bold;
                // stylePadding = state.fontAssetInfo.boldStyle / 4.0f * gradientScale * sdfRatios.x;
                // if (stylePadding + padding > gradientScale) {
                //     padding = gradientScale - stylePadding;
                // }

                boldAdvanceMultiplier = 1 + state.fontAssetInfo.boldSpacing * 0.01f;
            }
            else {
                displayFlags &= ~CharacterDisplayFlags.Bold;
            }
            // else {
            //     stylePadding = fontAsset.normalStyle / 4.0f * gradientScale;
            //     if (stylePadding + padding > gradientScale) {
            //         padding = gradientScale - stylePadding;
            //     }
            // }

            float fontScaleMultiplier = state.scriptStyle == TextScript.SuperScript || state.scriptStyle == TextScript.SubScript
                ? fontAsset.faceInfo.SubSize
                : 1;

            float fontBaseLineOffset = fontAsset.faceInfo.baseline * fontScale * fontScaleMultiplier * fontAsset.faceInfo.scale;

            // sizeInfo.padding = padding;
            //sizeInfo.stylePadding = stylePadding;
            sizeInfo.monospacing = 0;

            // will need to check for a real italic font, probably I just want to ensure italic fonts have an italic style of 0
            if ((state.fontStyle & FontStyle.Italic) != 0) {
                sizeInfo.shear = fontAsset.italicStyle * 0.01f;
                displayFlags |= CharacterDisplayFlags.Italic;
            }
            else {
                sizeInfo.shear = 0;
                displayFlags &= ~CharacterDisplayFlags.Italic;
            }

            sizeInfo.fontAssetId = fontAsset.id;
            sizeInfo.displayFlags = displayFlags;
            sizeInfo.atlasWidth = fontAsset.atlasWidth;
            sizeInfo.atlasHeight = fontAsset.atlasHeight;
            sizeInfo.fontAscender = fontAsset.faceInfo.ascender;
            sizeInfo.fontDescender = fontAsset.faceInfo.descender;
            sizeInfo.fontScale = fontScale;
            sizeInfo.boldAdvanceMultiplier = boldAdvanceMultiplier;
            sizeInfo.fontScaleMultiplier = fontScaleMultiplier;
            sizeInfo.fontBaseLineOffset = fontBaseLineOffset;
        }

        public struct TextRatioData {

            public float outlineWidth;
            public float outlineSoftness;
            public float faceDilate;
            public float glowOffset;
            public float glowOuter;
            public float underlayX;
            public float underlayY;
            public float underlaySoftness;
            public float underlayDilate;

        }

        internal static float3 ComputeRatios(in FontAssetInfo fontAsset, in TextRatioData textStyle) {
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

            float ratioB_t = glowOffset + glowOuter > 1 ? glowOffset + glowOuter : 1;
            float ratioB = math.max(0, gradientScale - 1 - ratioBRange) / (gradientScale * ratioB_t);
            if (ratioB < 0) ratioB = 0;
            float underlayOffsetX = textStyle.underlayX;
            float underlayOffsetY = textStyle.underlayY;
            float underlayDilate = textStyle.underlayDilate;
            float underlaySoftness = textStyle.underlaySoftness;

            float ratioCRange = (weight + faceDilate) * (gradientScale - 1);
            float ratioC_t = math.max(1, math.max(math.abs(underlayOffsetX), math.abs(underlayOffsetY)) + underlayDilate + underlaySoftness);

            float ratioC = math.max(0, gradientScale - 1f - ratioCRange) / (gradientScale * ratioC_t);

            return new float3(ratioA, ratioB, ratioC);
        }

        internal static float3 ComputeRatios(in FontAssetInfo fontAsset, in TextInfoRenderSpan textStyle) {
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

            float ratioB_t = glowOffset + glowOuter > 1 ? glowOffset + glowOuter : 1;
            float ratioB = math.max(0, gradientScale - 1 - ratioBRange) / (gradientScale * ratioB_t);
            if (ratioB < 0) ratioB = 0;
            float underlayOffsetX = textStyle.underlayX;
            float underlayOffsetY = textStyle.underlayY;
            float underlayDilate = textStyle.underlayDilate;
            float underlaySoftness = textStyle.underlaySoftness;

            float ratioCRange = (weight + faceDilate) * (gradientScale - 1);
            float ratioC_t = math.max(1, math.max(math.abs(underlayOffsetX), math.abs(underlayOffsetY)) + underlayDilate + underlaySoftness);

            float ratioC = math.max(0, gradientScale - 1f - ratioCRange) / (gradientScale * ratioC_t);

            return new float3(ratioA, ratioB, ratioC);
        }


        internal static void MeasureWord(in DataList<FontAssetInfo>.Shared fontAssetMap, int wordIndex, ref WordInfo wordInfo, in List_TextSymbol symbolList, ref ComputeSizeInfo sizeInfo, ref TextMeasureState measureState) {
            float xAdvance = 0;
            float maxHeight = 0;
            int start = wordInfo.charStart;
            int end = wordInfo.charEnd;

            wordInfo.maxAscender = 0;

            float currentElementScale = sizeInfo.fontScale * sizeInfo.fontScaleMultiplier;

            for (int charIndex = start; charIndex < end; charIndex++) {
                ref TextSymbol textSymbol = ref symbolList[charIndex];

                switch (textSymbol.type) {
                    case TextSymbolType.Character: {
                        if (!fontAssetMap[sizeInfo.fontAssetId].TryGetGlyphIndex(textSymbol.charInfo.character, out int glyphIdx)) {
                            continue; // todo -- handle missing glyphs somehow
                        }

                        ref UIForiaGlyph glyph = ref fontAssetMap[sizeInfo.fontAssetId].GetGlyphRef(glyphIdx);

                        // if (sizeInfo.monospacing != 0) {
                        //     xAdvance += (sizeInfo.monospacing - monoAdvance + ((sizeInfo.characterSpacing + sizeInfo.normalSpacingOffset) * currentElementScale));
                        // }

                        float kerningAdvance = 0;
                        if (charIndex + 1 < symbolList.size) {
                            kerningAdvance = fontAssetMap[sizeInfo.fontAssetId].GetKerning(textSymbol.charInfo.character, symbolList[charIndex + 1].charInfo.character);
                        }

                        float ascender = sizeInfo.fontAscender * currentElementScale;
                        textSymbol.charInfo.position.x = xAdvance + (charIndex == start ? 0 : (glyph.xOffset * currentElementScale));
                        textSymbol.charInfo.position.y = sizeInfo.fontBaseLineOffset + (sizeInfo.fontAscender - glyph.yOffset) * currentElementScale;
                        textSymbol.charInfo.scale = currentElementScale;
                        textSymbol.charInfo.fontAssetId = (ushort) sizeInfo.fontAssetId;
                        textSymbol.charInfo.glyphIndex = (ushort) glyphIdx;
                        textSymbol.charInfo.wordIndex = (ushort) wordIndex;
                        textSymbol.charInfo.displayFlags |= sizeInfo.displayFlags;

                        //(sizeInfo.fontAscender * currentElementScale); 
                        // Debug.Log(((char)textSymbol.charInfo.character) + " asc: "+ (sizeInfo.fontAscender * currentElementScale));
                        // + ((sizeInfo.fontAscender - glyph.yOffset) * currentElementScale);
                        float bottom = textSymbol.charInfo.position.y + (glyph.height * currentElementScale);
                        wordInfo.maxAscender = wordInfo.maxAscender > ascender ? wordInfo.maxAscender : ascender;
                        // textSymbol.charInfo.bottomRight.x = textSymbol.charInfo.topLeft.x + (glyph.width * currentElementScale);
                        // textSymbol.charInfo.bottomRight.y = textSymbol.charInfo.topLeft.y + (glyph.height * currentElementScale);

                        // float topShear = sizeInfo.shear * (glyph.yOffset * currentElementScale);
                        // float bottomShear = sizeInfo.shear * ((glyph.yOffset - glyph.height) * currentElementScale);

                        // todo -- im not sure kerning is correct here, it has very small values and I think it might be an x position adjustment or need to be scaled 

                        float w = (glyph.xAdvance
                                   * sizeInfo.boldAdvanceMultiplier
                                   + sizeInfo.normalSpacingOffset
                                   + kerningAdvance) * currentElementScale + sizeInfo.characterSpacing;

                        xAdvance += w;
                        if (charIndex == start) {
                            xAdvance -= (glyph.xOffset * currentElementScale);
                        }

                        maxHeight = math.max(maxHeight, bottom - textSymbol.charInfo.position.y);

                        break;
                    }

                    // todo -- loop until we hit a character again before recomputing
                    case TextSymbolType.FontPush: {
                        // this feels really wrong! beware
                        if (textSymbol.fontId >= 1 && textSymbol.fontId < fontAssetMap.size) {
                            measureState.PushFont(fontAssetMap[textSymbol.fontId]);
                            RecomputeFontInfo(ref measureState, ref sizeInfo);
                        }

                        break;
                    }

                    case TextSymbolType.FontPop:
                        measureState.PopFont();
                        RecomputeFontInfo(ref measureState, ref sizeInfo);
                        break;

                    case TextSymbolType.FontSizePush:
                        measureState.PushFontSize(textSymbol.length);
                        RecomputeFontInfo(ref measureState, ref sizeInfo);
                        currentElementScale = sizeInfo.fontScale * sizeInfo.fontScaleMultiplier;
                        break;

                    case TextSymbolType.FontSizePop:
                        measureState.PopFontSize();
                        RecomputeFontInfo(ref measureState, ref sizeInfo);
                        currentElementScale = sizeInfo.fontScale * sizeInfo.fontScaleMultiplier;
                        break;

                    case TextSymbolType.HorizontalSpace: {
                        // I think this does nothing, spacing should have been handled in word creation
                        // todo -- I need to resolve the space size here
                        break;
                    }

                    case TextSymbolType.CharSpacingPush: {
                        sizeInfo.characterSpacing = measureState.PushCharSpacing(textSymbol.length);
                        break;
                    }

                    case TextSymbolType.CharSpacingPop: {
                        measureState.TryPopCharSpacing(out sizeInfo.characterSpacing);
                        break;
                    }
                }
            }

            wordInfo.width = xAdvance;
            wordInfo.height = maxHeight;
        }

        internal static float3 ComputeRatios(in FontAssetInfo fontAsset, in TextMeasureState textStyle) {
            // A = outline, face dilate, weight
            // B = glow 
            // C = underlay

            // float gradientScale = fontAsset.gradientScale;
            // float faceDilate = textStyle.faceDilate;
            //
            // float outlineThickness = textStyle.outlineWidth;
            // float outlineSoftness = textStyle.outlineSoftness;
            // float weight = math.max(fontAsset.weightNormal, fontAsset.weightBold) / 4f;
            // float ratioA_t = math.max(1, weight + faceDilate + outlineThickness + outlineSoftness);
            // float ratioA = (gradientScale - 1f) / (gradientScale * ratioA_t);
            //
            // float glowOffset = textStyle.glowOffset;
            // float glowOuter = textStyle.glowOuter;
            // float ratioBRange = (weight + faceDilate) * (gradientScale - 1f);
            //
            // float ratioB_t = math.max(1, glowOffset + glowOuter);
            // float ratioB = math.max(0, gradientScale - 1 - ratioBRange) / (gradientScale * ratioB_t);
            //
            // float underlayOffsetX = textStyle.underlayX;
            // float underlayOffsetY = textStyle.underlayY;
            // float underlayDilate = textStyle.underlayDilate;
            // float underlaySoftness = textStyle.underlaySoftness;
            //
            // float ratioCRange = (weight + faceDilate) * (gradientScale - 1);
            // float ratioC_t = math.max(1, math.max(math.abs(underlayOffsetX), math.abs(underlayOffsetY)) + underlayDilate + underlaySoftness);
            //
            // float ratioC = math.max(0, gradientScale - 1f - ratioCRange) / (gradientScale * ratioC_t);

            // return new float3(ratioA, ratioB, ratioC);
            return new float3(1, 1, 1);
        }

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
        public float monospacing;
        public float characterSpacing;
        public float normalSpacingOffset;
        public CharacterDisplayFlags displayFlags;
        public float shear;

    }

    public struct TextDisplayData {

        public float faceDilate;
        public FontAsset fontAsset;
        public float outlineWidth;
        public float outlineSoftness;
        public float glowOuter;
        public float glowOffset;
        public float underlayX;
        public float underlayY;
        public float underlayDilate;
        public float underlaySoftness;

    }

}