using UIForia.Util;
using UIForia.Util.Unsafe;
using UnityEngine;

namespace UIForia.Text {

    public struct TextSymbolStream {

        internal StructList<TextSymbol> stream;
        internal bool requiresTextTransform;
        internal bool requiresRenderProcessing;

        internal TextSymbolStream(StructList<TextSymbol> stream) {
            this.stream = stream;
            this.requiresTextTransform = false;
            this.requiresRenderProcessing = false;
        }

        public void AddCharacter(char character) {
            stream.Add(new TextSymbol() {
                type = TextSymbolType.Character,
                charInfo = new BurstCharInfo() {
                    character = character
                }
            });
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

    }

}