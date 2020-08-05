using UIForia.Util;
using UnityEngine;

namespace UIForia.Text {

    public struct TextSymbolStream {

        internal LightList<PendingTextEffectSymbolData> textEffects;
        internal StructList<TextSymbol> stream;
        internal bool requiresTextTransform;
        internal bool requiresRichTextLayout;

        internal TextSymbolStream(LightList<PendingTextEffectSymbolData> textEffects, StructList<TextSymbol> stream) {
            this.stream = stream;
            this.textEffects = textEffects;
            this.requiresTextTransform = false;
            this.requiresRichTextLayout = false;
        }

        public void AddCharacter(char character) {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.Character,
                charInfo = new BurstCharInfo() {
                    character = character
                }
            });
        }

        public void PushCharacterSpacing(UIFixedLength characterSpacing) {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.CharSpacingPush,
                length = characterSpacing
            });
        }

        public void PopCharacterSpacing() {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.CharSpacingPop,
            });
        }

        public void PushTextEffect(TextEffectId effectId, CharStream bodyStream) {
            textEffects.Add(new PendingTextEffectSymbolData() {
                bodyStream = bodyStream,
                symbolIndex = stream.size,
                effectId = effectId
            });
            stream.Add(new TextSymbol() {
                type = TextSymbolType.EffectPush,
            });
        }

        public void PopTextEffect(TextEffectId effectId) {
            if (textEffects.size > 0) {
                stream.Add(new TextSymbol() {
                    type = TextSymbolType.EffectPop,
                    effectId = effectId
                });
            }
        }

        public void PushTextTransform(TextTransform textTransform) {
            if (textTransform == TextTransform.None) {
                return;
            }

            requiresTextTransform = true;
            stream.Add(new TextSymbol() {
                type = TextSymbolType.TextTransformPush,
                textTransform = textTransform
            });
        }

        public void PopTextTransform() {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.TextTransformPop,
            });
        }

        public void PushNoBreak() {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.NoBreakPush,
            });
        }

        public void PopNoBreak() {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.NoBreakPop,
            });
        }

        public void PushColor(Color color) {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.ColorPush,
                color = color
            });
        }

        public void PopColor() {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.ColorPop,
            });
        }

        public void PushFontSize(UIFixedLength fontSize) {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.FontSizePush,
                length = fontSize
            });
        }

        public void PopFontSize() {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.FontSizePop,
            });
        }

        public void HorizontalSpace(UIFixedLength fixedLength) {
            requiresRichTextLayout = true;
            stream.Add(new TextSymbol() {
                type = TextSymbolType.HorizontalSpace,
                length = fixedLength
            });
        }

        public void PushHorizontalInvert() {
            stream.Add(new TextSymbol() {type = TextSymbolType.PushHorizontalInvert});
        }

        public void PopHorizontalInvert() {
            stream.Add(new TextSymbol() {type = TextSymbolType.PopHorizontalInvert});
        }

        public void PushVerticalInvert() {
            stream.Add(new TextSymbol() {type = TextSymbolType.PushVerticalInvert});
        }

        public void PopVerticalInvert() {
            stream.Add(new TextSymbol() {type = TextSymbolType.PopVerticalInvert});
        }

        public void PushOpacity(float value) {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.PushOpacity,
                floatValue = value
            });
        }

        public void PopOpacity() {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.PopOpacity
            });
        }

    }

}