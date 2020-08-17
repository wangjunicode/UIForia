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

    internal unsafe struct TextId {

        public ElementId elementId;
        public TextInfo* textInfo;

    }

    internal unsafe struct TextChange {

        public readonly TextInfo *textInfo;
        public readonly ElementId elementId;

        public TextChange(ElementId elementId, TextInfo * textInfo) {
            this.elementId = elementId;
            this.textInfo = textInfo;
        }

    }

    public interface IRichTextRevealParser {

        bool TryParseRevealNode(CharSpan nodeName, CharStream bodyStream, bool isCloseNode, out TextSymbol textSymbol);

    }

    public class StandardTextReveal : TextReveal, IRichTextRevealParser {

        public bool TryParseRevealNode(CharSpan nodeName, CharStream bodyStream, bool isCloseNode, out TextSymbol textSymbol) {
            // textSymbol.SetSymbolData();
            textSymbol = default;
            return true;
        }

    }

    public struct TextInterface {

        public StructList<TextSymbol> symbolList;

        public ref TextSymbol GetSymbol(int idx) {
            return ref symbolList.array[idx];
        }

        public ref CharacterInterface GetCharacterAt(int idx) {
            throw new NotImplementedException();
        }

        public void SetRevealState(in CharacterInterface character, TextRevealState revealState) {
            // character.charptr->flags 
        }

    }

    [Flags]
    public enum TextRevealState : byte {

        BeginReveal = 1 << 0,
        Revealing = 1 << 1,
        RevealComplete = 1 << 2,
        Revealed = 1 << 3,
        BeginHide = 1 << 4,
        Hiding = 1 << 5,
        HideComplete = 1 << 6,
        Hidden = 1 << 7

    }

    public abstract class TextReveal {

        public virtual void OnTextChanged() { }

        public virtual void OnLayoutChanged() { }

        public virtual void OnCharacterInsert() { }

        public virtual void OnCharacterRemoved() { }

        private float elapsedTime;
        private int lastSymbolIdx;
        private float pauseStartTime;
        private float pauseEndTime;

        public virtual void Update(TextInterface textInterface) {
            for (int i = 0; i < textInterface.symbolList.size; i++) {
                if (textInterface.symbolList.array[i].type == TextSymbolType.Character) { }

                uint type = (uint) textInterface.symbolList.array[i].type;

                if (type == 255) {
                    // Pause();
                }
            }

            if (textInterface.GetSymbol(1).type == TextSymbolType.Character) {
                ref CharacterInterface characterAt = ref textInterface.GetCharacterAt(1);
                textInterface.SetRevealState(characterAt, TextRevealState.BeginReveal);
            }

            elapsedTime += Time.deltaTime;
        }

    }

    public unsafe class TextSystem : IDisposable, ITextEffectResolver {

        internal int frameId;
        internal ElementSystem elementSystem;
        private Application application;

        internal DataList<TextChange>.Shared changedElementIds;
        internal DataList<TextLayoutSymbol>.Shared layoutBuffer;
        internal LightList<TextEffect> textEffectTable;
        internal StructList<int> textEffectFreeList;
        private TextEffectAnimator textEffectAnimator;
        private LightList<UITextElement> textWithEffects;
        internal DataList<TextId> activeTextElementInfo;
        internal List_Int32 effectVertexFreeList;
        internal DataList<TextEffectInfo> textEffectVertexInfoTable;
        private LightList<TextEffectDefinition> effectDefinitions;
        private bool requireSort;

        public TextSystem(Application application, ElementSystem elementSystem, LightList<TextEffectDefinition> effectDefinitions) {
            this.application = application;
            this.elementSystem = elementSystem;
            this.activeTextElementInfo = new DataList<TextId>(32, Allocator.Persistent);
            this.changedElementIds = new DataList<TextChange>.Shared(16, Allocator.Persistent);
            this.layoutBuffer = new DataList<TextLayoutSymbol>.Shared(128, Allocator.Persistent);
            this.textEffectTable = new LightList<TextEffect>();
            this.textEffectFreeList = new StructList<int>();
            this.textEffectAnimator = new TextEffectAnimator(this);
            this.effectVertexFreeList = new List_Int32(8, Allocator.Persistent);
            this.textEffectVertexInfoTable = new DataList<TextEffectInfo>(8, Allocator.Persistent);
            this.effectDefinitions = effectDefinitions ?? new LightList<TextEffectDefinition>(0);

            this.textEffectVertexInfoTable.size++; // 0 is invalid
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
                    for (int j = 0; j < activeTextElementInfo.size; j++) {
                        if (activeTextElementInfo[j].elementId == id) {
                            requireSort = true;
                            activeTextElementInfo[j] = activeTextElementInfo[--activeTextElementInfo.size];
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
                    activeTextElementInfo.Add(new TextId() {
                        textInfo = textElement.textInfo,
                        elementId = textElement.id
                    });
                    ref TextInfo textInfo = ref textElement.textInfo[0];
                    textInfo.whitespaceMode = element.style.TextWhitespaceMode;
                    textInfo.alignment = element.style.TextAlignment;
                    textInfo.fontAssetId = element.style.TextFontAsset.id;
                    textInfo.fontStyle = element.style.TextFontStyle;
                    textInfo.textTransform = element.style.TextTransform;
                    textInfo.lineHeight = element.style.TextLineHeight;
                    textInfo.selectionColor = element.style.SelectionTextColor;

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

                    AddToChangeSet(textElement);
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

            ref TextInfo textInfo = ref textElement.textInfo[0];

            bool requiresRefresh = false;
            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref properties[i];
                switch (property.propertyId) {
                    case StylePropertyId.TextFontStyle:
                        textInfo.fontStyle = property.AsFontStyle;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextFontAsset:
                        textInfo.fontAssetId = property.AsFont.id;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextFontSize:
                        textInfo.fontSize = property.AsUIFixedLength;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextTransform:
                        textInfo.textTransform = property.AsTextTransform;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextLineHeight:
                        textInfo.lineHeight = property.AsFloat;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextWhitespaceMode:
                        textInfo.whitespaceMode = property.AsWhitespaceMode;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextAlignment:
                        textInfo.alignment = property.AsTextAlignment;
                        break;

                    case StylePropertyId.TextColor:
                        textInfo.textMaterial.faceColor = property.AsColor32;
                        break;

                    case StylePropertyId.TextGlowColor:
                        textInfo.textMaterial.glowColor = property.AsColor32;
                        break;

                    case StylePropertyId.TextGlowOffset:
                        textInfo.textMaterial.glowOffset = MathUtil.FloatMinus1To1ToUshort(property.AsFloat);
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextGlowOuter:
                        textInfo.textMaterial.glowOuter = MathUtil.Float01ToByte(property.AsFloat);
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextGlowInner:
                        textInfo.textMaterial.glowInner = MathUtil.Float01ToByte(property.AsFloat);
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextGlowPower:
                        textInfo.textMaterial.glowPower = MathUtil.Float01ToByte(property.AsFloat);
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextUnderlayX:
                        textInfo.textMaterial.underlayX = property.AsFloat;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextUnderlayY:
                        textInfo.textMaterial.underlayY = property.AsFloat;
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextUnderlayDilate:
                        textInfo.textMaterial.underlayDilate = MathUtil.Float01ToByte(property.AsFloat);
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextUnderlayColor:
                        textInfo.textMaterial.underlayColor = property.AsColor32;
                        break;

                    case StylePropertyId.TextUnderlaySoftness:
                        textInfo.textMaterial.underlaySoftness = MathUtil.Float01ToByte(property.AsFloat);
                        requiresRefresh = true;
                        break;

                    case StylePropertyId.TextFaceDilate:
                        textInfo.textMaterial.faceDilate = MathUtil.Float01ToByte(property.AsFloat);
                        requiresRefresh = true;
                        break;
                }
            }

            if (requiresRefresh) {
                AddToChangeSet(textElement);
            }
        }

        internal void CleanupFrame() {
            changedElementIds.size = 0;
        }

        internal void UpdateText(UITextElement uiTextElement) {

            ref TextInfo textInfo = ref uiTextElement.textInfo[0];

            if (uiTextElement._processor is RichTextProcessor richTextProcessor) {
                richTextProcessor.SetTextEffectResolver(this);
            }

            TextInfo.UpdateText(ref textInfo, uiTextElement.text, uiTextElement._processor, this);

            AddToChangeSet(uiTextElement);
            
        }

        private void AddToChangeSet(UITextElement uiTextElement) {
            if (uiTextElement.lastUpdateFrame != frameId) {
                uiTextElement.lastUpdateFrame = frameId;
                for (int i = 0; i < changedElementIds.size; i++) {
                    if (changedElementIds[i].elementId.id == uiTextElement.id.id) {
                        return;
                    }
                }
                changedElementIds.Add(new TextChange(uiTextElement.id, uiTextElement.textInfo));
            }
        }
   
        public void Dispose() {
            effectVertexFreeList.Dispose();
            textEffectVertexInfoTable.Dispose();
            activeTextElementInfo.Dispose();
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

            internal DataList<TextChange>.Shared changedElementIds;

            public void Execute() {
                for (int i = 0; i < changedElementIds.size; i++) {
                    ref TextInfo textInfo = ref changedElementIds[i].textInfo[0];

                    if (textInfo.requiresTextTransform || textInfo.textTransform != TextTransform.None) {
                        TextUtil.TransformText(textInfo.textTransform, textInfo.symbolList.array, textInfo.symbolList.size);
                    }
                }
            }

        }

        public void UpdateEffects() {
            // todo -- only the ones that have effects should be invoked

            textEffectAnimator.fontAssetMap = application.ResourceManager.fontAssetMap;

            Profiler.BeginSample("UIForia::TextEffectUpdate");
            TextId* arrayPointer = activeTextElementInfo.GetArrayPointer();
            int count = activeTextElementInfo.size;

            for (int i = 0; i < count; i++) {
                ref TextInfo textInfo = ref arrayPointer[i].textInfo[0];

                if (textInfo.hasEffects) {
                    textEffectAnimator.Animate(default, ref textInfo, (UITextElement) elementSystem.instanceTable[arrayPointer[i].elementId.index]);
                }
            }

            Profiler.EndSample();
        }

        internal void DeallocateEffectIndex(int idx) {
            textEffectVertexInfoTable[idx] = default;
            effectVertexFreeList.Add(idx);
        }

        internal int AllocateEffectIndex() {
            if (effectVertexFreeList.size == 0) {
                textEffectVertexInfoTable.Add(default);
                return textEffectVertexInfoTable.size - 1;
            }

            int idx = effectVertexFreeList.array[--effectVertexFreeList.size];
            ref TextEffectInfo effect = ref textEffectVertexInfoTable[idx];
            effect = default;
            return idx;
        }

        public TextEffect SpawnTextEffect(int spawnerId, out int instanceId) {
            if (spawnerId >= 0 && spawnerId < effectDefinitions.size) {
                TextEffect instance = effectDefinitions.array[spawnerId].effectSpawner.Spawn();
                if (instance != null) {
                    if (textEffectFreeList.size > 0) {
                        instanceId = textEffectFreeList.array[--textEffectFreeList.size];
                    }
                    else {
                        instanceId = textEffectTable.size;
                        textEffectTable.Add(default);
                    }

                    textEffectTable[instanceId] = instance;
                    return instance;
                }
            }

            instanceId = -1;
            return null;
        }

        public void DespawnTextEffect(TextEffect effect) { }

        public bool TryResolveTextEffect(CharSpan effectName, out TextEffectId effectId) {
            for (int i = 0; i < effectDefinitions.size; i++) {
                if (effectDefinitions.array[i].effectName == effectName) {
                    effectId = new TextEffectId(i);
                    return true;
                }
            }

            effectId = default;
            return false;
        }

        public bool TryGetFontAsset(int fontAssetId, out FontAssetInfo fontAsset) {
            DataList<FontAssetInfo>.Shared assetMap = application.ResourceManager.fontAssetMap;

            if (fontAssetId < 0 || fontAssetId >= assetMap.size) {
                fontAsset = default;
                return false;
            }

            fontAsset = assetMap[fontAssetId];
            return true;
        }

    }

    public enum TextRenderType : ushort {

        Highlight = 0,
        Characters = 1,
        Sprite = 2,
        Underline = 3,
        Image,
        Element

    }

    public unsafe struct TextRenderRange {

        public TextRenderType type;
        public ushort idx;

        public RangeInt characterRange;
        public int fontAssetId;
        public TextSymbol* symbols;
        public TextureUsage texture0;
        public TextureUsage texture1;

        public AxisAlignedBounds2D localBounds;
        // public ElementDrawDesc* drawDesc;

    }

}