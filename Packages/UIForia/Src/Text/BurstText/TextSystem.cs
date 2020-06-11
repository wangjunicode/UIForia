using System;
using System.Diagnostics;
using UIForia.Elements;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Text {

    internal struct TextChange {

        public readonly int textInfoId;
        public readonly ElementId elementId;

        public TextChange(ElementId elementId, int textInfoId) {
            this.elementId = elementId;
            this.textInfoId = textInfoId;
        }

    }

    public unsafe class TextSystem : IDisposable {

        internal int freeList;
        internal int frameId;
        internal ElementSystem elementSystem;

        internal DataList<TextChange>.Shared changedElementIds;
        internal DataList<TextSymbol>.Shared inputSymbolBuffer;
        internal DataList<BurstTextInfo>.Shared textInfoMap;
        internal DataList<TextLayoutSymbol>.Shared layoutBuffer;

        public TextSystem(ElementSystem elementSystem) {
            this.elementSystem = elementSystem;
            this.textInfoMap = new DataList<BurstTextInfo>.Shared(128, Allocator.Persistent);
            this.changedElementIds = new DataList<TextChange>.Shared(32, Allocator.Persistent);
            this.layoutBuffer = new DataList<TextLayoutSymbol>.Shared(128, Allocator.Persistent);
            this.inputSymbolBuffer = new DataList<TextSymbol>.Shared(128, Allocator.Persistent);
            textInfoMap.size++;
        }

        public void HandleElementDisabled(DataList<ElementId>.Shared disabledElements) { }

        public void HandleElementEnabled(DataList<ElementId>.Shared enabledElements) {

            for (int i = 0; i < enabledElements.size; i++) {
                UIElement element = elementSystem.instanceTable[enabledElements[i].index];
                if (element is UITextElement textElement) {
                    ref BurstTextInfo textInfo = ref textInfoMap[textElement.textInfoId];
                    textInfo.textStyle = new TextStyle() {
                        alignment = element.style.TextAlignment,
                        faceDilate = element.style.TextFaceDilate,
                        fontSize = element.style.TextFontSize,
                        fontAssetId = element.style.TextFontAsset.id,
                        fontStyle = element.style.TextFontStyle,
                        glowColor = element.style.TextGlowColor,
                        glowOffset = element.style.TextGlowOffset,
                        glowOuter = element.style.TextGlowOuter,
                        outlineColor = element.style.TextOutlineColor,
                        outlineSoftness = element.style.TextOutlineSoftness,
                        outlineWidth = element.style.TextOutlineWidth,
                        textColor = element.style.TextColor,
                        textTransform = element.style.TextTransform,
                        underlayColor = element.style.TextUnderlayColor,
                        underlayDilate = element.style.TextUnderlayDilate,
                        underlaySoftness = element.style.TextUnderlaySoftness,
                        underlayX = element.style.TextUnderlayX,
                        underlayY = element.style.TextUnderlayY,
                        whitespaceMode = element.style.TextWhitespaceMode,
                        lineHeight = element.style.TextLineHeight
                    };

                    if (textElement.lastUpdateFrame != frameId) {
                        textElement.lastUpdateFrame = frameId;
                        changedElementIds.Add(new TextChange(textElement.id, textElement.textInfoId));
                    }
                }

            }

        }

        // need to re-process text if whitespace changed, if text transform changed, if style changed
        // font size, any sdf property

        // will be bursted eventually
        public void HandleStyleChanged(UIElement element, StyleProperty[] properties, int propertyCount) {

            // todo -- flag check MIGHT be better
            if (!(element is UITextElement textElement)) {
                return;
            }

            ref BurstTextInfo textInfo = ref textInfoMap[textElement.textInfoId];

            bool requiresRefresh = false;
            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref properties[i];
                switch (property.propertyId) {
                    case StylePropertyId.TextFontStyle:
                        textInfo.textStyle.fontStyle = property.AsFontStyle;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextFontAsset:
                        textInfo.textStyle.fontAssetId = property.AsFont.id;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextFontSize:
                        textInfo.textStyle.fontSize = property.AsUIFixedLength;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextTransform:
                        textInfo.textStyle.textTransform = property.AsTextTransform;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextLineHeight:
                        textInfo.textStyle.lineHeight = property.AsFloat;
                        requiresRefresh = true;
                        break;
                    
                    case StylePropertyId.TextWhitespaceMode:
                        textInfo.textStyle.whitespaceMode = property.AsWhitespaceMode;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextAlignment:
                        textInfo.textStyle.alignment = property.AsTextAlignment;
                        break;

                    case StylePropertyId.TextColor:
                        textInfo.textStyle.textColor = property.AsColor32;
                        break;

                    case StylePropertyId.TextGlowColor:
                        textInfo.textStyle.glowColor = property.AsColor32;
                        break;

                    case StylePropertyId.TextGlowOffset:
                        textInfo.textStyle.glowOffset = property.AsFloat;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextGlowOuter:
                        textInfo.textStyle.glowOuter = property.AsFloat;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextUnderlayX:
                        textInfo.textStyle.underlayX = property.AsFloat;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextUnderlayY:
                        textInfo.textStyle.underlayY = property.AsFloat;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextUnderlayDilate:
                        textInfo.textStyle.underlayDilate = property.AsFloat;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextUnderlayColor:
                        textInfo.textStyle.underlayColor = property.AsColor32;
                        break;

                    case StylePropertyId.TextUnderlaySoftness:
                        textInfo.textStyle.underlaySoftness = property.AsFloat;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextFaceDilate:
                        textInfo.textStyle.faceDilate = property.AsFloat;
                        requiresRefresh = true;
                        break;

                }
                
                
                
            }

            if (requiresRefresh && textElement.lastUpdateFrame != frameId) {
                textElement.lastUpdateFrame = frameId;
                changedElementIds.Add(new TextChange(textElement.id, textElement.textInfoId));
            }

        }

        public void CleanupFrame() {

            // new RemoveDeadElementsJob() {
            //     metaTable = elementSystem.metaTable,
            //     textChanges = changedElementIds
            // }.Run();
            //
            // for (int i = 0; i < changedElementIds.size; i++) {
            //
            //     ref BurstTextInfo textInfo = ref textInfoMap[changedElementIds[i].textInfoId];
            //
            //     if (textInfo.requiresTextTransform || textInfo.textStyle.textTransform != TextTransform.None) {
            //         TextUtil.TransformText(textInfo.textStyle.textTransform, textInfo.symbolList.array, textInfo.symbolList.size);
            //     }
            //
            // }
            //
            // // from here on out its all burst
            //
            // new ProcessWhitespaceJob() {
            //     buffer = inputSymbolBuffer,
            //     textChanges = changedElementIds,
            //     textInfoMap = textInfoMap
            // }.Run();
            //
            // new CreateLayoutSymbols() {
            //     buffer = layoutBuffer,
            //     textChanges = changedElementIds,
            //     textInfoMap = textInfoMap
            // }.Run();
            //
            // new ComputeWordSizes() {
            //     changes = changedElementIds,
            //     textInfoMap = textInfoMap,
            //     fontAssetMap = resourceManager.fontAssetMap
            // }.Execute();

            changedElementIds.size = 0;

        }

        //Not burst job because we need access to the 'char' class for transformation
        public struct UpdateTextTransformJob : IJob {

            internal DataList<BurstTextInfo>.Shared textInfoMap;
            internal DataList<TextChange>.Shared changedElementIds;

            public void Execute() {
                for (int i = 0; i < changedElementIds.size; i++) {

                    ref BurstTextInfo textInfo = ref textInfoMap[changedElementIds[i].textInfoId];

                   // if (textInfo.requiresTextTransform || textInfo.textStyle.textTransform != TextTransform.None) {
                        TextUtil.TransformText(textInfo.textStyle.textTransform, textInfo.symbolList.array, textInfo.symbolList.size);
                   // }

                }

            }

        }

        public void UpdateText(UITextElement uiTextElement) {

            if (uiTextElement.textInfoId == 0) {
                uiTextElement.textInfoId = GetNextTextId();
            }

            bool requiresTextTransform = false;
            bool processedStream = false;

            int length = uiTextElement.text.Length;

            fixed (char* charptr = uiTextElement.text) {
                if (uiTextElement._processor != null) {
                    CharStream stream = new CharStream(charptr, 0, (uint) length);
                    TextSymbolStream symbolStream = new TextSymbolStream(inputSymbolBuffer);

                    processedStream = uiTextElement._processor.Process(stream, ref symbolStream);

                    requiresTextTransform = symbolStream.requiresTextTransform;
                    inputSymbolBuffer = symbolStream.stream;
                }

                if (!processedStream) {
                    inputSymbolBuffer.SetSize(length);

                    TextSymbol* array = inputSymbolBuffer.GetArrayPointer();
                    TypedUnsafe.MemClear(array, length);

                    for (int i = 0; i < length; i++) {
                        array[i].type = TextSymbolType.Character;
                        array[i].charInfo.character = charptr[i];
                    }

                }
            }

            ref BurstTextInfo textInfo = ref textInfoMap[uiTextElement.textInfoId];
            textInfo.requiresTextTransform = requiresTextTransform;

            if (textInfo.symbolList.array == null) {
                textInfo.symbolList = new List_TextSymbol(inputSymbolBuffer.size, Allocator.Persistent);
            }
            else {
                textInfo.symbolList.EnsureCapacity(inputSymbolBuffer.size);
            }

            textInfo.symbolList.CopyFrom(inputSymbolBuffer.GetArrayPointer(), inputSymbolBuffer.size);

            inputSymbolBuffer.size = 0;

            if (uiTextElement.lastUpdateFrame != frameId) {
                uiTextElement.lastUpdateFrame = frameId;
                changedElementIds.Add(new TextChange(uiTextElement.id, uiTextElement.textInfoId));
            }

        }

        // todo -- make this real, need to use free list or grow
        private int GetNextTextId() {
            int id = textInfoMap.size;
            textInfoMap.Add(default);
            return id;
        }

        public void Dispose() {
            textInfoMap.Dispose();
            changedElementIds.Dispose();
            inputSymbolBuffer.Dispose();
            layoutBuffer.Dispose();
        }

        internal void GetTextChangesForView(ElementId viewRootId, DataList<TextChange>.Shared textChangeBuffer) {
            textChangeBuffer.size = 0;

            ElementTraversalInfo info = elementSystem.traversalTable[viewRootId];

            for (int i = 0; i < changedElementIds.size; i++) {

                if (info.IsAncestorOf(elementSystem.traversalTable[changedElementIds[i].elementId])) {
                    textChangeBuffer.Add(changedElementIds[i]);
                }

            }

        }

    }

}