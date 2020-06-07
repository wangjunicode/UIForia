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

        public struct TextLayoutSymbolDebug {

            

        }

    }

    [StructLayout(LayoutKind.Sequential)]
    [DebuggerTypeProxy(typeof(TextInfoDebugView))]
    public unsafe struct BurstTextInfo : IDisposable {

        // todo -- store in single larger buffer
        internal List_Char characterList;
        internal List_TextSymbol symbolList;

        internal List_TextLayoutSymbol layoutSymbolList;
        internal List_TextLineInfo lineInfoList;

        public bool requiresTextTransform;
        public TextStyle textStyle;
        public int nextFreeId;

        // parser pushes character instructions into output stream
        // strip whitespace accordingly
        // transform text accordingly
        // convert char instructions to words 
        // measure those
        // layout knows how to handle word stream
        // renderer only handles character stream

        public void SetText(string text, ref TextInfoBufferSet textInfoBufferSet, ITextProcessor processor = null) {

            textInfoBufferSet.SetText(text, processor);

            // TextUtil.ProcessWhiteSpace(textStyle.whitespaceMode,
            //     textInfoBufferSet.inputSymbolBuffer.GetArrayPointer(),
            //     textInfoBufferSet.inputSymbolBuffer.size,
            //     ref textInfoBufferSet.outputSymbolBuffer
            // );

            // if (textStyle.textTransform != TextTransform.None || textInfoBufferSet.requiresTextTransform) {
            //     TextUtil.TransformText(textStyle.textTransform, ref textInfoBufferSet.outputSymbolBuffer);
            // }

            // TextUtil.CreateLayoutSymbols(textInfoBufferSet.inputSymbolBuffer, ref textInfoBufferSet.layoutBuffer);

            // todo -- merge chars, text symbols and layout symbols into single buffer

            // if (symbolList.array == null) {
            //     symbolList = new List_TextSymbol(textInfoBufferSet.outputSymbolBuffer.size, Allocator.Persistent);
            // }
            //
            // if (layoutSymbolList.array == null) {
            //     layoutSymbolList = new List_TextLayoutSymbol(textInfoBufferSet.layoutBuffer.size, Allocator.Persistent);
            // }
            //
            // if (characterList.array == null) {
            //     characterList = new List_Char(textInfoBufferSet.originalInput.Length, Allocator.Persistent);
            // }
            //
            // symbolList.CopyFrom(textInfoBufferSet.outputSymbolBuffer.GetArrayPointer(), textInfoBufferSet.outputSymbolBuffer.size);
            //
            // layoutSymbolList.CopyFrom(textInfoBufferSet.layoutBuffer.GetArrayPointer(), textInfoBufferSet.layoutBuffer.size);
            //
            // fixed (char* charptr = text) {
            //     TypedUnsafe.MemCpy(characterList.array, charptr, text.Length);
            //     characterList.size = text.Length;
            // }

        }

        public void Dispose() { }

        // first we take the input string and find all the modifiers from it
        // this gives us a smaller buffer
        // next need to handle processing the whitespace, includes line break modifiers
        // between those modifications we'll end up with a smaller buffer and other modifications needs to have their indices updated
        // next run through and transform characters if needed, must be managed code
        // when handling whitespace, why applying whitespace modification need to go through remaining modifiers and alter their char index according to how much whitespace we collapsed in that run

        // now break into words
        // this will include no-break

        // Process whitespace -> maybe needs to be managed due to text transform
        // apply text transform -> needs managed code :(

        // here on out is all burstable
        // break into words
        // delimters 
        // resolve char infos
        // resolve kerning info
        // compute sizes

        // do i want two streams of characters here?
        // char info -> character / sprite / element
        // need a list of char info for rendering & sizing anyway
        // but if inject characters then i need to update all the modifier indices that come after that
        // actually can do that with an offset integer probably so not expensive just need to consistent

        // i need to convert my raw char stream into char infos
        // i should be able to freely edit the modifier positions here
        // i should also be able to apply white space collapse 
        // also lookup kerning info

        // i think we have two paths here, a faster one for when there are no modifications and then this one

        // this is the setup step
        // can i really create inline elements? i think its pretty cool
        // better might be to define 'blocks' and render an element into that space
        // would rather do this processing in burst job which means I cannot create an element there
        // i could post-process to create the elements though and just leave a marker in the text 
        // at process time
        // in the rare case we need it, job.Run() should be fine for measuring text

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

    }

}