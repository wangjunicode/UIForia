using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {

    internal class TextEffectAnimator {

        internal LightList<TextEffect> textEffectStack;
        internal TextSystem textSystem;
        internal DataList<FontAssetInfo>.Shared fontAssetMap;

        public TextEffectAnimator(TextSystem textSystem) {
            this.textSystem = textSystem;
            this.textEffectStack = new LightList<TextEffect>();
        }

        public unsafe void Animate(in float4x4 worldMatrix, ref TextInfo textInfo, UITextElement textElement) {

            //if (textInfo.textEffects != null && textInfo.textEffects.size > 0) {
            //    textEffectStack.AddRange(textInfo.textEffects);
            //}

            textEffectStack.size = 0;
            int size = textInfo.symbolList.size;
            TextSymbol* array = textInfo.symbolList.array;

            int charIdx = 0;

            CharacterInterface characterInterface = default;
            characterInterface.fontAssetMap = fontAssetMap.GetArrayPointer();
            characterInterface.textSystem = textSystem;

            fixed (TextInfo* textInfoPtr = &textInfo) {
                characterInterface.textInfo = textInfoPtr;
            }

            int depthCount = 0;

            for (int i = 0; i < size; i++) {

                ref TextSymbol currentSymbol = ref array[i];

                if (currentSymbol.type == TextSymbolType.Character && (currentSymbol.charInfo.flags & CharacterFlags.Visible) != 0) {
                    charIdx++;
                }
                
                if (currentSymbol.type != TextSymbolType.EffectPush) {
                    continue;
                }

                if (currentSymbol.effectInfo.instanceId == -1) {
                    continue;
                }

                TextEffect effect = textSystem.textEffectTable.array[currentSymbol.effectInfo.instanceId];

                if (effect == null || !effect.isActive) {
                    continue;
                }

                int rangeStart = i + 1;
                int rangeEnd = size;

                for (int j = i + 1; j < size; j++) {
                    ref TextSymbol s = ref array[j];

                    if (s.type == TextSymbolType.EffectPush && s.effectInfo.spawnerId == currentSymbol.effectInfo.spawnerId) {
                        depthCount++;
                    }
                    else if (s.type == TextSymbolType.EffectPop && s.effectInfo.spawnerId == currentSymbol.effectInfo.spawnerId) {

                        if (depthCount == 0) {
                            rangeEnd = j;
                            break;
                        }

                        depthCount--;
                    }

                }

                effect.OnPush(worldMatrix, textElement);

                int charIdxSave = charIdx;
                for (int j = rangeStart; j < rangeEnd; j++) {
                    ref TextSymbol textSymbol = ref array[j];

                    if (textSymbol.type != TextSymbolType.Character || (textSymbol.charInfo.flags & CharacterFlags.Visible) == 0) {
                        continue;
                    }

                    fixed (BurstCharInfo* ptr = &textSymbol.charInfo) {

                        characterInterface.charptr = ptr;
                        characterInterface.charIndex = charIdx;
                        characterInterface.vertexPtr = null;

                        effect.ApplyEffect(ref characterInterface);
                        // material id stored in effect data? 
                        // on the one hand thats kind of nice and encapsulated
                        // on the other hand it requires a full material data if all i want to override is styled properties
                        // could make material idx 2 ushorts, a base and an override

                        // for (int eidx = 0; eidx < textEffectStack.size; eidx++) {
                        //     textEffectStack.array[eidx].ApplyEffect(ref characterInterface);
                        // }

                    }

                    charIdx++; 
                }

                effect.OnPop();
                charIdx = charIdxSave;
            }

            // // I think its better to traverse char ranges once per effect and treat pop as the end of range
            // if (textSymbol.type == TextSymbolType.EffectPop) {
            //     if (textEffectStack.size > 0) {
            //         for (int j = textEffectStack.size - 1; j >= 0; j--) {
            //             TextEffect effect = textEffectStack.array[j];
            //             // if (effect.effectTypeId == textSymbol.effectInfo.spawnerId) {
            //             //     effect.OnPop();
            //             //     textEffectStack.RemoveAt(j);
            //             //     break;
            //             // }
            //         }
            //     }
            //
            //     continue;
            // }
            //
            // if (textEffectStack.size == 0 || textSymbol.type != TextSymbolType.Character || (textSymbol.charInfo.flags & CharacterFlags.Visible) == 0) {
            //     continue;
            // }
            //
            // fixed (BurstCharInfo* ptr = &textSymbol.charInfo) {
            //
            //     characterInterface.charptr = ptr;
            //     characterInterface.charIndex = charIdx;
            //     characterInterface.vertexPtr = null;
            //
            //     // material id stored in effect data? 
            //     // on the one hand thats kind of nice and encapsulated
            //     // on the other hand it requires a full material data if all i want to override is styled properties
            //     // could make material idx 2 ushorts, a base and an override
            //
            //     for (int eidx = 0; eidx < textEffectStack.size; eidx++) {
            //         textEffectStack.array[eidx].ApplyEffect(ref characterInterface);
            //     }
            //
            // }
            //
            // charIdx++;

        }

    }

}