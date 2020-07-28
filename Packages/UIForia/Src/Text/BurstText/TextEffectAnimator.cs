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
        internal BounceTextEffect bounce = new BounceTextEffect();
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

            // todo -- temp
            if (textElement.id.index == 4) {
                textEffectStack.Add(bounce);
            }

            bounce.OnPush(worldMatrix, textElement);
            
            CharacterInterface characterInterface = default;
            characterInterface.fontAssetMap = fontAssetMap.GetArrayPointer();
            characterInterface.textSystem = textSystem;
            fixed (TextInfo* textInfoPtr = &textInfo) {
                characterInterface.textInfo = textInfoPtr;
            }

            for (int i = 0; i < size; i++) {
                ref TextSymbol textSymbol = ref array[i];

                if (textSymbol.type == TextSymbolType.EffectPush) {
                    TextEffect effect = textSystem.textEffectTable.array[textSymbol.effectId];
                    effect.OnPush(worldMatrix, textElement);
                    continue;
                }

                if (textSymbol.type == TextSymbolType.EffectPop) {
                    if (textEffectStack.size > 0) {
                        TextEffect effect = textEffectStack.array[--textEffectStack.size];
                        effect.OnPop();
                    }

                    continue;
                }

                if (textEffectStack.size == 0 || textSymbol.type != TextSymbolType.Character || (textSymbol.charInfo.flags & CharacterFlags.Visible) == 0) {
                    continue;
                }

                fixed (BurstCharInfo* ptr = &textSymbol.charInfo) {

                    characterInterface.charptr = ptr;
                    characterInterface.charIndex = charIdx;
                    characterInterface.vertexPtr = null;
                    
                    // material id stored in effect data? 
                    // on the one hand thats kind of nice and encapsulated
                    // on the other hand it requires a full material data if all i want to override is styled properties
                    // could make material idx 2 ushorts, a base and an override
                    
                    for (int eidx = 0; eidx < textEffectStack.size; eidx++) {
                        textEffectStack.array[eidx].ApplyEffect(ref characterInterface);
                    }

                }

                charIdx++;

            }

        }

    }

}