using UIForia.Util;
using UIForia.Util.Unsafe;
using UnityEngine;

namespace UIForia.Text {

    public struct TextSymbolStream {

        internal DataList<TextSymbol>.Shared stream;
        internal bool requiresTextTransform;

        internal TextSymbolStream(DataList<TextSymbol>.Shared stream) {
            this.stream = stream;
            this.requiresTextTransform = false;
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

    public static class TextProcessors {

        public static RichTextProcessor RichText = new RichTextProcessor();

    }

    public class RichTextProcessor : ITextProcessor {

        public bool Process(CharStream stream, ref TextSymbolStream textSymbolStream) {

            while (stream.HasMoreTokens) {

                uint start = stream.Ptr;

                if (stream != '[') {
                    textSymbolStream.AddCharacter(stream.Current);
                    stream.Advance();
                }
                else if (stream.Next == '/') {
                    stream.Advance(2);
                    if (stream.TryMatchRange("nobreak]")) {
                        textSymbolStream.PopNoBreak();
                    }
                    else if (stream.TryMatchRange("size]")) {
                        textSymbolStream.PopFontSize();
                    }
                    else if (stream.TryMatchRange("uppercase]")) {
                        textSymbolStream.PopTextTransform();
                    }
                    else if (stream.TryMatchRange("titlecase]")) {
                        textSymbolStream.PopTextTransform();
                    }
                    else if (stream.TryMatchRange("lowercase]")) {
                        textSymbolStream.PopTextTransform();
                    }
                    else {
                        stream.RewindTo(start + 1);
                        textSymbolStream.AddCharacter(stream.Current);
                    }
                }
                else {
                    stream.Advance();
                    if (stream.TryMatchRange("nobreak]")) {
                        textSymbolStream.PushNoBreak();
                    }
                    else if (stream.TryMatchRange("size") && stream.TryParseCharacter('=')) {

                        //[size=3.4em]
                        //[size=46px]

                        if (stream.TryParseFixedLength(out UIFixedLength value, true) && stream.TryParseCharacter(']')) {
                            textSymbolStream.PushFontSize(value);
                            continue;
                        }

                        stream.RewindTo(start + 1);
                        textSymbolStream.AddCharacter(stream.Current);
                    }
                    else if (stream.TryMatchRange("uppercase]")) {
                        textSymbolStream.PushTextTransform(TextTransform.UpperCase);
                    }
                    else if (stream.TryMatchRange("titlecase]")) {
                        textSymbolStream.PushTextTransform(TextTransform.TitleCase);
                    }
                    else if (stream.TryMatchRange("lowercase]")) {
                        textSymbolStream.PushTextTransform(TextTransform.LowerCase);
                    }
                    else {
                        stream.RewindTo(start + 1);
                        textSymbolStream.AddCharacter(stream.Current);
                    }
                }
            }

            return true;

        }

    }

}