using System;
using System.Runtime.InteropServices;
using UIForia.Graphics;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Text {

    public struct MaterialStack : IDisposable {

        private DataList<Entry> stack;
        public int fontSizeIdx;
        public int faceColorIdx;
        public int hInvertCount;
        public int vInvertCount;

        public float viewportWidth; // todo -- remove
        public float viewportHeight;

        private TextMaterialInfo baseMaterialInfo;
        private float baseFontSize;
        private int opacityIdx;

        public MaterialStack(Allocator allocator) : this() {
            this.stack = new DataList<Entry>(64, allocator);
        }

        public void Setup(in TextInfo textInfo) {
            baseMaterialInfo = textInfo.textMaterial;
            baseFontSize = textInfo.resolvedFontSize;
            stack.size = 0;
            fontSizeIdx = -1;
            faceColorIdx = -1;
            opacityIdx = -1;
            hInvertCount = 0;
            vInvertCount = 0;
        }

        public void PushHInversion() {
            hInvertCount++;
        }

        public bool PopHInversion() {
            hInvertCount--;
            if (hInvertCount < 0) hInvertCount = 0;
            return hInvertCount == 0;
        }

        public void PushVInversion() {
            vInvertCount++;
        }

        public bool PopVInversion() {
            vInvertCount--;
            if (vInvertCount < 0) vInvertCount = 0;
            return vInvertCount == 0;
        }

        public Color32 PushFaceColor(Color32 color) {
            stack.Add(new Entry() {
                prev = faceColorIdx,
                color = color
            });
            faceColorIdx = stack.size - 1;
            return color;
        }

        public bool TryPopFaceColor(out Color32 color) {
            if (faceColorIdx == -1) {
                color = default;
                return false;
            }

            faceColorIdx = stack[faceColorIdx].prev;
            if (faceColorIdx == -1) {
                color = baseMaterialInfo.faceColor;
            }
            else {
                color = stack[faceColorIdx].color;
            }

            return true;
        }

        public byte PushOpacity(float opacity) {
            byte opacityValue = MathUtil.Float01ToByte(opacity);
            stack.Add(new Entry() {
                prev = opacityIdx,
                byteValue = opacityValue
            });
            opacityIdx = stack.size - 1;
            return opacityValue;
        }

        public bool TryPopOpacity(out byte opacityValue) {
            if (opacityIdx == -1) {
                opacityValue = 255;
                return false;
            }

            opacityIdx = stack[opacityIdx].prev;
            if (opacityIdx == -1) {
                opacityValue = 255;
            }
            else {
                opacityValue = stack[opacityIdx].byteValue;
            }

            return true;
        }

        public bool TryPopFontSize(out float newSize) {

            if (fontSizeIdx == -1) {
                newSize = baseFontSize;
                return false;
            }

            fontSizeIdx = stack[fontSizeIdx].prev;
            if (fontSizeIdx == -1) {
                newSize = baseFontSize;
            }
            else {
                newSize = stack[fontSizeIdx].floatValue;
            }

            return true;

        }

        public float PushFontSize(UIFixedLength newFontSize) {

            float size;

            float fontSize = fontSizeIdx != -1 ? stack[fontSizeIdx].floatValue : baseFontSize;

            switch (newFontSize.unit) {

                default:
                case UIFixedUnit.Unset:
                case UIFixedUnit.Pixel:
                    size = newFontSize.value;
                    break;

                case UIFixedUnit.Percent:
                    size = newFontSize.value * fontSize * 100;
                    break;

                case UIFixedUnit.Em:
                    size = newFontSize.value * fontSize;
                    break;

                case UIFixedUnit.ViewportWidth:
                    size = viewportWidth * newFontSize.value;
                    break;

                case UIFixedUnit.ViewportHeight:
                    size = viewportHeight * newFontSize.value;
                    break;
            }

            stack.Add(new Entry() {
                prev = fontSizeIdx,
                type = TextStyleType.FontSize,
                floatValue = size
            });

            fontSizeIdx = stack.size - 1;

            return size;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Entry {

            [FieldOffset(0)]public int prev;
            [FieldOffset(4)]public TextStyleType type;
            [FieldOffset(8)] public float floatValue;
            [FieldOffset(8)] public UIFixedLength length;
            [FieldOffset(8)] public Color32 color;
            [FieldOffset(8)] public byte byteValue;

        }

        public void Dispose() {
            stack.Dispose();
        }

    }

    [BurstCompile]
    internal unsafe struct UpdateTextMaterialBuffersJob : IJob {

        public DataList<TextId> activeTextElementIds;
        public DataList<TextInfo> textInfoMap;
        private DataList<TextMaterialInfo> materialBuffer;

        public void Execute() {
            Run(0, activeTextElementIds.size);
        }

        private void Run(int start, int end) {
            materialBuffer = new DataList<TextMaterialInfo>(32, Allocator.Temp);
            MaterialStack materialStack = new MaterialStack(Allocator.Temp);

            for (int i = start; i < end; i++) {
                // if !needsMaterialUpdate, continue
                TextId textId = activeTextElementIds[i];
                ref TextInfo textInfo = ref textInfoMap[textId.textInfoId];
                UpdateMaterialBuffers(ref materialStack, ref textInfo);

            }

            materialStack.Dispose();
            materialBuffer.Dispose();
        }

        private void UpdateMaterialBuffers(ref MaterialStack materialStack, ref TextInfo textInfo) {

            // if (!textInfo.isRichText) {
            //     // todo -- also check for material data
            //     return;
            // }
            materialStack.Setup(textInfo);

            materialBuffer.size = 0;
            TextMaterialInfo textMaterial = textInfo.textMaterial;
            bool needsMaterialPush = false;

            DataList<TextMaterialInfo> textMaterialBuffer = new DataList<TextMaterialInfo>(8, Allocator.Temp);
            DataList<TextMaterialInfo> textMaterialStack = new DataList<TextMaterialInfo>(8, Allocator.Temp);

            ushort materialIdx = 0;

            byte opacityMultiplier = 255;
            textMaterialStack.Add(textMaterial);
            textMaterialBuffer.Add(textMaterial);

            // per character data
            // opacity (byte)
            // inversion flags (2bits)
            // baseline config (2 or 3 bits)
            // bold or italic (2 bits)
            // how do these work with material buffer?
            // need to know difference between default state and what was set from tags I think
            // or just say fuck it for now? if effect needs it inverted it must set it every frame, otherwise we restore previous value when rebuilding material setup

            RenderCharacterDisplayFlags displayFlags = 0;

            for (int s = 0; s < textInfo.symbolList.size; s++) {

                ref TextSymbol textSymbol = ref textInfo.symbolList.array[s];

                switch (textSymbol.type) {

                    case TextSymbolType.Character: {

                        ref BurstCharInfo charInfo = ref textSymbol.charInfo;

                        if ((charInfo.flags & CharacterFlags.Visible) == 0) {
                            continue;
                        }

                        if (needsMaterialPush) {
                            needsMaterialPush = false;
                            materialIdx++;
                            textMaterialBuffer.Add(textMaterial);
                        }

                        charInfo.materialIndex = materialIdx;
                        charInfo.baseMaterialIndex = materialIdx;
                        charInfo.opacityMultiplier = opacityMultiplier;
                        charInfo.displayFlags = displayFlags;
                        // ref RenderedCharacterInfo renderCharInfo = ref textInfo.renderedCharacters.array[charInfo.renderIdx];
                        // renderCharInfo.materialIndex = materialIdx;
                        // renderCharInfo.opacityMultiplier = opacityMultiplier;
                        // renderCharInfo.displayFlags = displayFlags;

                        break;
                    }

                    case TextSymbolType.UnderlayPush:
                        needsMaterialPush = true;
                        textMaterial.underlaySoftness = textSymbol.underlay.GetSoftness(textMaterial.underlaySoftness);
                        textMaterial.underlayDilate = 0; //textSymbol.underlay.GetDilate(textMaterial.underlayDilate);
                        textMaterial.underlayX = textSymbol.underlay.GetOffsetX(textMaterial.underlayX);
                        textMaterial.underlayY = textSymbol.underlay.GetOffsetY(textMaterial.underlayY);
                        textMaterial.underlayColor = textSymbol.underlay.GetColor(textMaterial.underlayColor);
                        textMaterialStack.Add(textMaterial);
                        break;

                    case TextSymbolType.PushHorizontalInvert:
                        displayFlags |= RenderCharacterDisplayFlags.InvertHorizontalUV;
                        materialStack.PushHInversion();
                        break;

                    case TextSymbolType.PopHorizontalInvert:
                        if (materialStack.PopHInversion()) {
                            displayFlags &= ~RenderCharacterDisplayFlags.InvertHorizontalUV;
                        }

                        break;

                    case TextSymbolType.PushVerticalInvert:
                        displayFlags |= RenderCharacterDisplayFlags.InvertVerticalUV;
                        materialStack.PushVInversion();
                        break;

                    case TextSymbolType.PopVerticalInvert:
                        if (materialStack.PopVInversion()) {
                            displayFlags &= ~RenderCharacterDisplayFlags.InvertVerticalUV;
                        }

                        break;

                    case TextSymbolType.UnderlayPop:
                        needsMaterialPush = true;
                        textMaterial = textMaterialStack[--textMaterialStack.size];
                        break;

                    case TextSymbolType.ColorPush: {
                        needsMaterialPush = true;
                        textMaterial.faceColor = materialStack.PushFaceColor(textSymbol.color);
                        break;
                    }

                    case TextSymbolType.ColorPop:
                        if (materialStack.TryPopFaceColor(out Color32 color)) {
                            needsMaterialPush = true;
                            textMaterial.faceColor = color;
                        }

                        break;

                    // font size now baked into character scale
                    // case TextSymbolType.FontSizePush:
                    //     needsMaterialPush = true;
                    //     textMaterial.fontSize = materialStack.PushFontSize(textSymbol.length);
                    //     break;
                    //
                    // case TextSymbolType.FontSizePop:
                    //    if (materialStack.TryPopFontSize(out float newCurrentFontSize)) {
                    //        needsMaterialPush = true;
                    //        textMaterial.fontSize = newCurrentFontSize;
                    //    }
                    //
                    //     break;

                    case TextSymbolType.PushOpacity:
                        opacityMultiplier = materialStack.PushOpacity(textSymbol.floatValue);
                        break;

                    case TextSymbolType.PopOpacity:
                        materialStack.TryPopOpacity(out opacityMultiplier);
                        break;

                    case TextSymbolType.Sprite:
                        // maaaaybe re-interpret as element style and push that. needs other data like textureids though, this isnt easy
                        break;

                }

            }


            textInfo.materialBuffer.size = 0;
            textInfo.materialBuffer.EnsureCapacity(textMaterialBuffer.size, Allocator.Persistent);
            textInfo.materialBuffer.AddRange(textMaterialBuffer.GetArrayPointer(), textMaterialBuffer.size);

        }

    }

}