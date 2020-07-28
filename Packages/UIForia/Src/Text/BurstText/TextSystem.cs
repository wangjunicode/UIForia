using System;
using UIForia.Elements;
using UIForia.Graphics;
using UIForia.Layout;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace UIForia.Text {

    internal struct TextId {

        public ElementId elementId;
        public int textInfoId;

    }

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

        internal DataList<TextInfo> textInfoMap;
        internal DataList<TextChange>.Shared changedElementIds;
        internal DataList<TextLayoutSymbol>.Shared layoutBuffer;
        internal LightList<TextEffect> textEffectTable;
        internal StructList<int> textEffectFreeList;
        private TextEffectAnimator textEffectAnimator;
        private LightList<UITextElement> textWithEffects;
        internal DataList<TextId> activeTextElementIds;
        internal List_Int32 effectVertexFreeList;
        internal DataList<TextEffectInfo> textEffectVertexInfoTable;
        private bool requireSort;
        private Application application;
        
        public TextSystem(Application application, ElementSystem elementSystem) {
            this.application = application;
            this.elementSystem = elementSystem;
            this.activeTextElementIds = new DataList<TextId>(32, Allocator.Persistent);
            this.textInfoMap = new DataList<TextInfo>(32, Allocator.Persistent, NativeArrayOptions.ClearMemory); // clear memory is very important here!
            this.changedElementIds = new DataList<TextChange>.Shared(32, Allocator.Persistent);
            this.layoutBuffer = new DataList<TextLayoutSymbol>.Shared(128, Allocator.Persistent);
            this.textEffectTable = new LightList<TextEffect>();
            this.textEffectFreeList = new StructList<int>();
            this.textEffectAnimator = new TextEffectAnimator(this);
            this.effectVertexFreeList = new List_Int32(8, Allocator.Persistent);
            this.textEffectVertexInfoTable = new DataList<TextEffectInfo>(8, Allocator.Persistent);
            textEffectVertexInfoTable.size++; // 0 is invalid
            textInfoMap.size++; // 0 is invalid
        }

        internal int RegisterTextEffect(TextEffect effect) {
            if (textEffectFreeList.size > 0) {
                int idx = textEffectFreeList.array[--textEffectTable.size];
                textEffectTable[idx] = effect;
                return idx;
            }

            textEffectTable.Add(effect);
            return textEffectTable.size - 1;
        }

        internal void DeregisterTextEffect(int effectId) {
            if (effectId > 0 && effectId < textEffectTable.size) {
                textEffectTable[effectId] = null;
                textEffectFreeList.Add(effectId);
            }
        }

        [BurstCompile]
        internal struct SortTextElements : IJob {

            public DataList<ElementId> textElementIds;
            public ElementTable<ElementTraversalInfo> traversalTable;

            public void Execute() {
                NativeSortExtension.Sort(textElementIds.GetArrayPointer(), textElementIds.size, new ElementFTBHierarchySort(traversalTable));
            }

        }

        internal void AnimateText() {

            if (requireSort) {
                new SortTextElements().Run();
                requireSort = false;
            }

            // for each enabled element. if text -> add it. 
            // for each disabled element. if text -> remove it
            // does execution order matter? probably not a bad thing to retain just in case
            // enable/disable in here is nasty as fuck. another reason to handle deferred enable/disable

            // crawl all text
            // apply reveal, then apply effects 

            // also need to build material buffers n shit

            // 

            // for (int i = 0; i < textWithEffects.size; i++) {
            //
            //     UITextElement textElement = textWithEffects.array[i];
            //     
            //     if (textElement.isEnabled) {
            //         textEffectAnimator.Animate(textElement.layoutResult.GetWorldMatrix(), textElement.textInfoManaged);
            //     }
            //
            // }

        }

        internal void HandleElementDisabled(DataList<ElementId>.Shared disabledElements) {

            // todo -- need to handle destroying text infos!
            // todo -- could be bursted

            for (int i = 0; i < disabledElements.size; i++) {
                ElementId id = disabledElements[i];
                UIElement element = elementSystem.instanceTable[id.index];
                if (element is UITextElement textElement) {
                    for (int j = 0; j < activeTextElementIds.size; j++) {
                        if (activeTextElementIds[j].elementId == id) {
                            requireSort = true;
                            activeTextElementIds[j] = activeTextElementIds[--activeTextElementIds.size];
                            break;
                        }
                    }
                }
            }

        }

        internal void HandleElementEnabled(DataList<ElementId>.Shared enabledElements) {

            for (int i = 0; i < enabledElements.size; i++) {
                UIElement element = elementSystem.instanceTable[enabledElements[i].index];
                if (element is UITextElement textElement) {
                    requireSort = true;
                    activeTextElementIds.Add(new TextId() {
                        textInfoId = textElement.textInfoId,
                        elementId = textElement.id
                    });
                    ref TextInfo textInfo = ref textInfoMap[textElement.textInfoId];
                    textInfo.textMaterial = new TextMaterialInfo() {
                        // opacity = element.style.Opacity, // todo -- should be multiplied appropriately
                        faceColor = element.style.TextColor,
                        faceDilate = MathUtil.FloatMinus1To1ToUshort(element.style.TextFaceDilate),
                        // fontSize = element.style.TextFontSize.value, // todo -- need to resolve this somehow later on
                        glowColor = element.style.TextGlowColor,
                        glowInner = MathUtil.Float01ToByte(element.style.TextGlowInner),
                        glowOffset = MathUtil.FloatMinus1To1ToUshort(element.style.TextGlowOffset),
                        glowOuter = MathUtil.Float01ToByte(element.style.TextGlowOuter),
                        glowPower = MathUtil.Float01ToByte(element.style.TextGlowPower),
                        outlineColor = element.style.TextOutlineColor,
                        outlineSoftness = MathUtil.Float01ToByte(element.style.TextOutlineSoftness),
                        outlineWidth = MathUtil.Float01ToByte(element.style.TextOutlineWidth),
                        underlayColor = element.style.TextUnderlayColor,
                        underlayDilate = MathUtil.FloatMinus1To1ToUshort(element.style.TextUnderlayDilate),
                        underlaySoftness = MathUtil.Float01ToByte(element.style.TextUnderlaySoftness),
                        underlayX = element.style.TextUnderlayX,
                        underlayY = element.style.TextUnderlayY,
                    };

                    textInfo.textStyle = new TextStyle() {
                        alignment = element.style.TextAlignment,
                        faceDilate = element.style.TextFaceDilate,
                        fontSize = element.style.TextFontSize,
                        fontAssetId = element.style.TextFontAsset.id,
                        fontStyle = element.style.TextFontStyle,
                        glowColor = element.style.TextGlowColor,
                        glowOffset = element.style.TextGlowOffset,
                        glowInner = element.style.TextGlowInner,
                        glowPower = element.style.TextGlowPower,
                        glowOuter = element.style.TextGlowOuter,
                        outlineColor = element.style.TextOutlineColor,
                        outlineSoftness = element.style.TextOutlineSoftness,
                        outlineWidth = element.style.TextOutlineWidth,
                        faceColor = element.style.TextColor,
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
        internal void HandleStyleChanged(UIElement element, StyleProperty[] properties, int propertyCount) {

            // todo -- flag check MIGHT be better
            if (!(element is UITextElement textElement)) {
                return;
            }

            ref TextInfo textInfo = ref textInfoMap[textElement.textInfoId];

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
                        textInfo.textStyle.faceColor = property.AsColor32;
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

                    case StylePropertyId.TextGlowInner:
                        textInfo.textStyle.glowInner = property.AsFloat;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextGlowPower:
                        textInfo.textStyle.glowPower = property.AsFloat;
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

        internal void CleanupFrame() {
            changedElementIds.size = 0;
        }

        internal void UpdateText(UITextElement uiTextElement) {

            if (uiTextElement.textInfoId == 0) {
                uiTextElement.textInfoId = GetNextTextId();
                if (uiTextElement.isEnabled) {
                    activeTextElementIds.Add(new TextId() {
                        textInfoId = uiTextElement.textInfoId,
                        elementId = uiTextElement.id
                    });
                }
            }

            ref TextInfo textInfo = ref textInfoMap[uiTextElement.textInfoId];

            TextInfo.UpdateText(ref textInfo, uiTextElement.text, uiTextElement._processor);

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

            // for (int i = 0; i < textInfoMap.size; i++) {
            //     textInfoMap[i].Dispose();
            // }

            // effectVertexFreeList.Dispose();
            // textEffectVertexInfoTable.Dispose();
            activeTextElementIds.Dispose();
            textInfoMap.Dispose();
            changedElementIds.Dispose();
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

        //Not burst job because we need access to the 'char' class for transformation
        internal struct UpdateTextTransformJob : IJob {

            internal DataList<TextInfo> textInfoMap;
            internal DataList<TextChange>.Shared changedElementIds;

            public void Execute() {
                for (int i = 0; i < changedElementIds.size; i++) {

                    ref TextInfo textInfo = ref textInfoMap[changedElementIds[i].textInfoId];

                    if (textInfo.requiresTextTransform || textInfo.textStyle.textTransform != TextTransform.None) {
                        TextUtil.TransformText(textInfo.textStyle.textTransform, textInfo.symbolList.array, textInfo.symbolList.size);
                    }

                }

            }

        }

        public void UpdateEffects() {

            // todo -- only the ones that have effects should be invoked

            textEffectAnimator.fontAssetMap = application.ResourceManager.fontAssetMap;
            
            Profiler.BeginSample("UIForia::TextEffectUpdate");
            for (int i = 0; i < activeTextElementIds.size; i++) {

                ref TextInfo textInfo = ref textInfoMap[activeTextElementIds[i].textInfoId];

                textEffectAnimator.Animate(default, ref textInfo, (UITextElement) elementSystem.instanceTable[activeTextElementIds[i].elementId.index]);

                // todo -- running on all right now

            }
            Profiler.EndSample();
        }

        internal void DeallocateEffectIndex(int idx) {
            textEffectVertexInfoTable[idx] = default;
            effectVertexFreeList.Add(idx);
        }

        internal int AllocateEffectIndex() {
            Color32 white = new Color32(255, 255, 255, 255);
            if (effectVertexFreeList.size == 0) {
                textEffectVertexInfoTable.Add(default);
                return textEffectVertexInfoTable.size - 1;
            }
            else {
                int idx = effectVertexFreeList.array[--effectVertexFreeList.size];
                ref TextEffectInfo effect = ref textEffectVertexInfoTable[idx];
                effect = default;
                return idx;
            }
        }

    }

    public enum TextRenderType {

        Characters,
        Underline,
        Sprite,
        Image,
        Element

    }

    public unsafe struct TextRenderRange {

        public TextRenderType type;
        public RangeInt characterRange;
        public int fontAssetId;
        public TextSymbol* symbols;
        public TextureUsage texture0;
        public TextureUsage texture1;
        public AxisAlignedBounds2D localBounds;

    }

}