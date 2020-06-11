using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UIForia.ListTypes;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia.Text {

    public unsafe struct TextInfoBufferSet : IDisposable {

        public string originalInput;
        public DataList<char> inputCharacterBuffer;
        public DataList<TextSymbol> inputSymbolBuffer;
        public DataList<TextSymbol> outputSymbolBuffer;
        public DataList<TextLayoutSymbol> layoutBuffer;
        public bool requiresTextTransform;

        public TextInfoBufferSet(Allocator allocator) {
            this.originalInput = null;
            this.requiresTextTransform = false;
            this.inputCharacterBuffer = new DataList<char>(128, allocator);
            this.inputSymbolBuffer = new DataList<TextSymbol>(128, allocator);
            this.outputSymbolBuffer = new DataList<TextSymbol>(128, allocator);
            this.layoutBuffer = new DataList<TextLayoutSymbol>(32, allocator);
        }

        public void SetText(char* charptr, int length, ITextProcessor processor = null) {
            inputCharacterBuffer.SetSize(length);

            inputCharacterBuffer.size = 0;
            inputSymbolBuffer.size = 0;
            outputSymbolBuffer.size = 0;
            layoutBuffer.size = 0;

            TypedUnsafe.MemCpy(inputCharacterBuffer.GetArrayPointer(), charptr, length);

            // requiresTextTransform = false;
            // bool processedStream = false;
            //
            // if (processor != null) {
            //     CharStream stream = new CharStream(charptr, 0, (uint) length);
            //     TextSymbolStream symbolStream = new TextSymbolStream(inputSymbolBuffer);
            //
            //     processedStream = processor.Process(stream, ref symbolStream);
            //
            //     requiresTextTransform = symbolStream.requiresTextTransform;
            //     inputSymbolBuffer = symbolStream.stream;
            // }
            //
            // if (!processedStream) {
            //
            //     inputSymbolBuffer.SetSize(length);
            //
            //     for (int i = 0; i < length; i++) {
            //         inputSymbolBuffer[i] = new TextSymbol() {
            //             type = TextSymbolType.Character,
            //             charInfo = new BurstCharInfo() {
            //                 character = charptr[i]
            //             }
            //         };
            //     }
            // }

        }

        public void SetText(string text, ITextProcessor processor = null) {

            fixed (char* charptr = text) {
                SetText(charptr, text.Length, processor);
            }

        }

        public void Dispose() {
            inputSymbolBuffer.Dispose();
            outputSymbolBuffer.Dispose();
            layoutBuffer.Dispose();
        }

    }

    [Flags]
    public enum TextLayoutSymbolType {

        Word = 1 << 0,
        HorizontalSpace = 1 << 1,
        LineHeightPush = 1 << 2,
        LineHeightPop = 1 << 3,
        LineIndentPush = 1 << 4,
        LineIndentPop = 1 << 5,
        MarginLeftPush = 1 << 6,
        MarginLeftPop = 1 << 7,
        MarginRightPush = 1 << 8,
        MarginRightPop = 1 << 9,
        IndentPush = 1 << 10,
        AlignPush = 1 << 11,
        IndentPop = 1 << 12,

        IsBreakable = 1 << 31

    }

    [DebuggerDisplay("TYpe = {GetDebuggerView()}")]
    [StructLayout(LayoutKind.Explicit)]
    public struct TextLayoutSymbol {

        [FieldOffset(0)] public TextLayoutSymbolType type;
        [FieldOffset(4)] public UIFixedLength space;
        [FieldOffset(4)] public WordInfo wordInfo;

        public bool isBreakable {
            get => (type & TextLayoutSymbolType.IsBreakable) != 0;
            set {
                if (value) {
                    type |= TextLayoutSymbolType.IsBreakable;
                }
                else {
                    type &= ~TextLayoutSymbolType.IsBreakable;
                }
            }
        }

        public string GetDebuggerView() {
            return type.ToString();
        }

    }

    public unsafe struct TextInfoDebugView {

        public string outputText;

        public TextLayoutSymbol[] layoutSymbols;
        public string[] layoutSymbolStrings;

        public TextInfoDebugView(BurstTextInfo target) {
            outputText = MakeOutputText(target.symbolList);

            if (target.layoutSymbolList.array != null) {
                layoutSymbols = new TextLayoutSymbol[target.layoutSymbolList.size];
                for (int i = 0; i < target.layoutSymbolList.size; i++) {
                    layoutSymbols[i] = target.layoutSymbolList[i];
                }
            }
            else {
                layoutSymbols = null;
            }

            layoutSymbolStrings = target.layoutSymbolList.size == 0 ? null : MakeLayoutSymbols(target.symbolList, target.layoutSymbolList);
        }

        private static string[] MakeLayoutSymbols(List_TextSymbol targetSymbolList, List_TextLayoutSymbol layoutSymbol) {
            string[] retn = new string[layoutSymbol.size];
            StringBuilder builder = new StringBuilder(32);
            for (int i = 0; i < layoutSymbol.size; i++) {
                switch (layoutSymbol.array[i].type & ~TextLayoutSymbolType.IsBreakable) {

                    case TextLayoutSymbolType.Word: {

                        builder.Clear();

                        ref WordInfo wordInfo = ref layoutSymbol.array[i].wordInfo;
                        for (int j = wordInfo.charStart; j < wordInfo.charEnd; j++) {

                            if (targetSymbolList[j].type == TextSymbolType.Character) {
                                builder.Append((char) targetSymbolList[j].charInfo.character);
                            }

                        }

                        retn[i] = builder.ToString();
                        if (!layoutSymbol.array[i].isBreakable) {
                            retn[i] += " (Non Breaking)";
                        }

                        break;
                    }

                    case TextLayoutSymbolType.HorizontalSpace:
                    case TextLayoutSymbolType.LineHeightPush:
                    case TextLayoutSymbolType.LineHeightPop:
                    case TextLayoutSymbolType.LineIndentPush:
                    case TextLayoutSymbolType.LineIndentPop:
                    case TextLayoutSymbolType.MarginLeftPush:
                    case TextLayoutSymbolType.MarginLeftPop:
                    case TextLayoutSymbolType.MarginRightPush:
                    case TextLayoutSymbolType.MarginRightPop:
                    case TextLayoutSymbolType.IndentPush:
                    case TextLayoutSymbolType.AlignPush:
                    case TextLayoutSymbolType.IndentPop:
                        retn[i] = layoutSymbol.array[i].type.ToString();
                        if (!layoutSymbol.array[i].isBreakable) {
                            retn[i] += " (Non Breaking)";
                        }

                        break;

                }
            }

            return retn;
        }

        private static string MakeOutputText(List_TextSymbol targetSymbolList) {
            StringBuilder builder = new StringBuilder(targetSymbolList.size);

            for (int i = 0; i < targetSymbolList.size; i++) {
                if (targetSymbolList[i].type == TextSymbolType.Character) {
                    builder.Append((char) targetSymbolList[i].charInfo.character);
                }
            }

            return builder.ToString();
        }

        public struct TextLayoutSymbolDebug { }

    }

    [StructLayout(LayoutKind.Sequential)]
    [DebuggerTypeProxy(typeof(TextInfoDebugView))]
    public struct BurstTextInfo : IDisposable {

        internal List_Char characterList;
        internal List_TextSymbol symbolList;
        internal List_TextLayoutSymbol layoutSymbolList;
        internal List_TextLineInfo lineInfoList;

        public bool requiresTextTransform;
        public TextStyle textStyle;

        // parser pushes character instructions into output stream
        // strip whitespace accordingly
        // transform text accordingly
        // convert char instructions to words 
        // measure those
        // layout knows how to handle word stream
        // renderer only handles character stream

        public string GetString() {
            TextUtil.StringBuilder.Clear();
            // todo -- if not rich text just return stringified buffer
            for (int i = 0; i < symbolList.size; i++) {
                if (symbolList[i].type == TextSymbolType.Character) {
                    TextUtil.StringBuilder.Append((char) symbolList[i].charInfo.character);
                }
            }

            return TextUtil.StringBuilder.ToString();
        }

        public void Dispose() {
            characterList.Dispose();
            symbolList.Dispose();
            layoutSymbolList.Dispose();
            lineInfoList.Dispose();
        }

    }

}