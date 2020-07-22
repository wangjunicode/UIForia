using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Text {

    internal class TextEffectAnimator {

        private LightList<TextEffect> textEffectStack;
        private TextSystem textSystem;

        public TextEffectAnimator(TextSystem textSystem) {
            this.textSystem = textSystem;
            this.textEffectStack = new LightList<TextEffect>();
        }

        private unsafe void AnimationStep(ManagedTextSpanInfo span) {

            if (span.textEffects != null && span.textEffects.size > 0) {
                textEffectStack.AddRange(span.textEffects);
            }

            int pushCount = 0;
            int charIdx = 0;
            
            // todo -- flag span as having / not having rich text effects in order to skip it

            for (int i = 0; i < span.unmanagedSpanInfo->symbolList.size; i++) {

                ref TextSymbol textSymbol = ref span.unmanagedSpanInfo->symbolList.array[i];

                if (textSymbol.type == TextSymbolType.EffectPush) {

                    if (textSymbol.effectId > 0 && textSymbol.effectId < textSystem.textEffectTable.size) {
                        textEffectStack.Add(textSystem.textEffectTable.array[textSymbol.effectId]);
                        pushCount++;
                    }

                }
                else if (textSymbol.type == TextSymbolType.EffectPop) {
                    if (pushCount > 0) {
                        pushCount--;
                        textEffectStack.size--;
                    }
                }
                else if (textSymbol.type == TextSymbolType.Character) {

                    ref BurstCharInfo burstCharInfo = ref textSymbol.charInfo;

                    if ((burstCharInfo.flags & CharacterFlags.Renderable) != 0 && textEffectStack.size > 0) {

                        CharacterInterface characterInterface = new CharacterInterface(span, burstCharInfo.position, burstCharInfo.character, charIdx, i, burstCharInfo.flags);

                        for (int evtIdx = 0; evtIdx < textEffectStack.size; evtIdx++) {
                            textEffectStack.array[evtIdx].ApplyEffect(ref characterInterface);
                        }

                    }

                    charIdx++;
                }

            }

            ManagedTextSpanInfo ptr = span.firstChild;
            
            while (ptr != null) {
                AnimationStep(ptr);             
                ptr = ptr.nextSibling;
            }
            
            textEffectStack.size -= pushCount;
            
            if (span.textEffects != null && span.textEffects.size > 0) {
                textEffectStack.size -= span.textEffects.size;
            }
            
        }

        public void Animate(ManagedTextInfo textInfo) {

            if (textInfo.textEffects != null && textInfo.textEffects.size > 0) {
                textEffectStack.AddRange(textInfo.textEffects);
            }

            ManagedTextSpanInfo ptr = textInfo.firstSpan;

            while (ptr != null) {

                AnimationStep(ptr);

                ptr = ptr.nextSibling;

            }
            
            textEffectStack.QuickClear();

        }

    }

}