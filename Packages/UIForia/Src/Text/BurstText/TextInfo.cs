using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UIForia.Layout;
using UIForia.ListTypes;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;

namespace UIForia.Text {

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

    [DebuggerDisplay("Type = {GetDebuggerView()}")]
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

        public TextInfoDebugView(TextInfo target) {
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

    }

    [StructLayout(LayoutKind.Sequential)]
    [DebuggerTypeProxy(typeof(TextInfoDebugView))]
    public struct TextInfo : IDisposable {

        internal List_Char characterList;
        internal List_TextSymbol symbolList;
        internal List_TextLayoutSymbol layoutSymbolList;
        internal List_TextLineInfo lineInfoList;

        internal bool requiresTextTransform;
        internal TextStyle textStyle;
        internal bool requiresRenderProcessing;

        [ThreadStatic] internal static StructList<TextSymbol> inputSymbolBuffer;
        internal bool isRenderDecorated;
        internal float resolvedFontSize;

        public static TextInfo Create(string text, in TextStyle style, in DataList<FontAssetInfo>.Shared fontAssetMap) {

            TextInfo textInfo = new TextInfo() {
                textStyle = style
            };

            UpdateText(ref textInfo, text, null);

            unsafe {

                if (textInfo.requiresTextTransform || textInfo.textStyle.textTransform != TextTransform.None) {
                    TextUtil.TransformText(textInfo.textStyle.textTransform, textInfo.symbolList.array, textInfo.symbolList.size);
                }

                DataList<TextSymbol> symbolBuffer = new DataList<TextSymbol>(textInfo.symbolList.size, Allocator.Temp);
                DataList<TextLayoutSymbol> layoutBuffer = new DataList<TextLayoutSymbol>(textInfo.symbolList.size, Allocator.Temp);
                TextMeasureState measureState = new TextMeasureState(Allocator.Temp);

                ProcessWhitespace(ref textInfo, ref symbolBuffer);
                CreateLayoutSymbols(ref textInfo, ref layoutBuffer);
                ComputeSize(fontAssetMap, ref textInfo, 18, ref measureState);

                symbolBuffer.Dispose();
                layoutBuffer.Dispose();
                measureState.Dispose();

            }

            return textInfo;
        }

        // parser pushes character instructions into output stream
        // strip whitespace accordingly
        // transform text accordingly
        // convert char instructions to words 
        // measure those
        // layout knows how to handle word stream
        // renderer only handles character stream

        internal static unsafe void ProcessWhitespace(ref TextInfo textInfo, ref DataList<TextSymbol> symbolBuffer) {
            symbolBuffer.size = 0;

            TextUtil.ProcessWhiteSpace(textInfo.textStyle.whitespaceMode, textInfo.symbolList.array, textInfo.symbolList.size, ref symbolBuffer);

            if (symbolBuffer.size != textInfo.symbolList.size) {
                textInfo.symbolList.CopyFrom(symbolBuffer.GetArrayPointer(), symbolBuffer.size);
            }
        }

        internal static unsafe void CreateLayoutSymbols(ref TextInfo textInfo, ref DataList<TextLayoutSymbol> layoutBuffer) {
            TextUtil.CreateLayoutSymbols(textInfo.symbolList.array, textInfo.symbolList.size, ref layoutBuffer);

            if (textInfo.layoutSymbolList.array == null) {
                textInfo.layoutSymbolList = new List_TextLayoutSymbol(layoutBuffer.size, Allocator.Persistent);
            }

            textInfo.layoutSymbolList.CopyFrom(layoutBuffer.GetArrayPointer(), layoutBuffer.size);
        }

        internal static void ComputeSize(DataList<FontAssetInfo>.Shared fontAssetMap, ref TextInfo textInfo, float emSize, ref TextMeasureState measureState) {
            ComputeSizeInfo sizeInfo = new ComputeSizeInfo();
            measureState.Initialize(emSize, textInfo.textStyle, fontAssetMap[textInfo.textStyle.fontAssetId]);
            TextUtil.RecomputeFontInfo(ref measureState, ref sizeInfo);
            
            textInfo.resolvedFontSize = measureState.fontSize;
            
            for (int i = 0; i < textInfo.layoutSymbolList.size; i++) {

                ref TextLayoutSymbol layoutSymbol = ref textInfo.layoutSymbolList[i];

                TextLayoutSymbolType type = layoutSymbol.type & ~TextLayoutSymbolType.IsBreakable;

                if (type == TextLayoutSymbolType.Word) {
                    TextUtil.MeasureWord(fontAssetMap, i, ref layoutSymbol.wordInfo, textInfo.symbolList, ref sizeInfo, ref measureState);
                }
                else if (type == TextLayoutSymbolType.HorizontalSpace) {
                    // todo -- lookup the em tree for given element
                }

            }
        }

        public static unsafe void UpdateText(ref TextInfo textInfo, string text, ITextProcessor processor = null) {
            bool requiresTextTransform = false;
            bool requiresRenderProcessing = false;
            bool processedStream = false;

            if (inputSymbolBuffer == null) {
                inputSymbolBuffer = new StructList<TextSymbol>(128);
            }

            inputSymbolBuffer.size = 0;
            int length = text.Length;

            fixed (char* charptr = text) {
                if (processor != null) {
                    CharStream stream = new CharStream(charptr, 0, (uint) length);
                    TextSymbolStream symbolStream = new TextSymbolStream(inputSymbolBuffer);

                    processedStream = processor.Process(stream, ref symbolStream);

                    requiresTextTransform = symbolStream.requiresTextTransform;
                    requiresRenderProcessing = symbolStream.requiresRenderProcessing;
                    inputSymbolBuffer = symbolStream.stream;
                }

                if (!processedStream) {
                    inputSymbolBuffer.SetSize(length);

                    fixed (TextSymbol* array = inputSymbolBuffer.array) {
                        TypedUnsafe.MemClear(array, length);

                        for (int i = 0; i < length; i++) {
                            array[i].type = TextSymbolType.Character;
                            array[i].charInfo.character = charptr[i];
                        }
                    }

                }
            }

            textInfo.requiresTextTransform = requiresTextTransform;
            textInfo.requiresRenderProcessing = requiresRenderProcessing;
            textInfo.symbolList.SetSize(inputSymbolBuffer.size, Allocator.Persistent);

            fixed (TextSymbol* inputPtr = inputSymbolBuffer.array) {
                textInfo.symbolList.CopyFrom(inputPtr, inputSymbolBuffer.size);
            }

        }

        internal static unsafe void RunLayoutHorizontal_WordsOnly<T>(ref TextInfo textInfo, ref T buffer, float width) where T : IBasicList<TextLineInfo> {

            ref List_TextLayoutSymbol layoutSymbolList = ref textInfo.layoutSymbolList;

            buffer.SetSize(0);

            WhitespaceMode whitespaceMode = textInfo.textStyle.whitespaceMode;
            bool trimStart = (whitespaceMode & WhitespaceMode.TrimLineStart) != 0;

            int wordStart = 0;
            int wordCount = 0;
            float cursorX = 0;

            for (int i = 0; i < layoutSymbolList.size; i++) {
                ref TextLayoutSymbol layoutSymbol = ref layoutSymbolList.array[i];
                ref WordInfo wordInfo = ref layoutSymbol.wordInfo;
                bool isBreakable = (layoutSymbol.type & TextLayoutSymbolType.IsBreakable) != 0;

                if (!isBreakable) {
                    wordCount++;
                    cursorX += wordInfo.width;
                    continue;
                }

                switch (wordInfo.type) {

                    case WordType.Whitespace: {

                        // if whitespace overruns end of line, start a new one and add it to that line
                        if (cursorX + wordInfo.width > width) {
                            buffer.Add(new TextLineInfo(wordStart, wordCount, cursorX));
                            wordStart = i;
                            wordCount = 1;
                            cursorX = 0; //wordInfo.width; there may be cases in which we want to keep this whitespace
                        }
                        else {
                            if (wordCount != 0) {
                                wordCount++;
                                cursorX += wordInfo.width;
                            }
                            else if (trimStart && wordStart == i) {
                                wordStart++;
                            }
                        }

                        break;
                    }

                    case WordType.NewLine:
                        buffer.Add(new TextLineInfo(wordStart, wordCount, cursorX));
                        cursorX = 0;
                        wordStart = i + 1;
                        wordCount = 0;
                        break;

                    case WordType.Normal:

                        // single word is longer than the line.
                        // Finish current line,
                        // start a new one,
                        // then complete the new one
                        if (wordInfo.width > width) {

                            if (wordCount != 0) {
                                buffer.Add(new TextLineInfo(wordStart, wordCount, cursorX));
                                cursorX = 0;
                                wordStart = i;
                                wordCount = 1;
                            }
                            else {
                                buffer.Add(new TextLineInfo(wordStart, 1, width));
                                cursorX = 0;
                                wordStart = i + 1;
                                wordCount = 0;
                            }

                        }
                        // next word is too long, break it onto the next line
                        else if (cursorX + wordInfo.width >= width + 0.5f) {
                            buffer.Add(new TextLineInfo(wordStart, wordCount, cursorX));
                            wordStart = i;
                            wordCount = 1;
                            cursorX = wordInfo.width;
                        }
                        else {
                            // we fit, just add to cursor position and word count
                            cursorX += wordInfo.width;
                            wordCount++;
                        }

                        break;

                    case WordType.SoftHyphen:
                        // if word is too long for current line,
                        // split it on the hyphen
                        // if still too long, walk back and keep splitting unless there isnt a hyphen a found
                        break;

                }

            }

            if (wordCount != 0) {
                buffer.Add(new TextLineInfo(wordStart, wordCount, cursorX));
            }

        }

        internal static unsafe void RunLayoutVertical_WordsOnly(in FontAssetInfo fontAsset, float fontSize, ref TextInfo textInfo) {
            ref List_TextLayoutSymbol layoutSymbolList = ref textInfo.layoutSymbolList;

            float lineOffset = 0;

            ref List_TextLineInfo lineInfoList = ref textInfo.lineInfoList;

            float smallCapsMultiplier = (textInfo.textStyle.textTransform == TextTransform.SmallCaps) ? 0.8f : 1f;
            float fontScale = (fontSize * smallCapsMultiplier) / (fontAsset.faceInfo.pointSize * fontAsset.faceInfo.scale);

            // todo -- something is wacky with line height I think
            float lineHeight = fontAsset.faceInfo.lineHeight * fontScale;
            float lineGap = fontAsset.faceInfo.lineHeight - (fontAsset.faceInfo.lineHeight);
            // need to compute a line height for each line
            for (int i = 0; i < lineInfoList.size; i++) {
                ref TextLineInfo lineInfo = ref lineInfoList[i];
                int end = lineInfo.wordStart + lineInfo.wordCount;
                float max = 0;
                
                for (int w = lineInfo.wordStart; w < end; w++) {
                    
                    if (textInfo.layoutSymbolList.array[w].wordInfo.height > max) {
                        max = textInfo.layoutSymbolList.array[w].wordInfo.height;
                    }
                    
                }

                lineInfo.height = lineHeight;
                lineInfo.y = lineOffset;
                lineOffset += max * textInfo.textStyle.lineHeight;
                for (int w = lineInfo.wordStart; w < end; w++) {
                    layoutSymbolList.array[w].wordInfo.y = lineInfo.y;
                }
            }

        }

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

        public void Layout(DataList<FontAssetInfo>.Shared fontAssetMap, float maxWidth = float.MaxValue) {
            RunLayoutHorizontal_WordsOnly(ref this, ref lineInfoList, maxWidth);
            // RunLayoutVertical_WordsOnly(fontAssetMap[textStyle.fontAssetId], 18, ref this);
        }

    }

    public unsafe struct FontMap {

        internal int size;
        internal FontAssetInfo* map;

    }

    public class TextLayout {

        private DataList<FontAssetInfo>.Shared fontAssetMap;
        private float baseEmSize;

        public TextLayout(float baseEmSize = 18f) {
            this.baseEmSize = baseEmSize;
        }

        public void SetEmSize(float emSize) {
            this.baseEmSize = emSize;
        }

        public void Layout(ref TextInfo textInfo, float maxWidth = float.MaxValue) {
            TextInfo.RunLayoutHorizontal_WordsOnly(ref textInfo, ref textInfo.lineInfoList, maxWidth);
            // TextInfo.RunLayoutVertical_WordsOnly(fontAssetMap[textInfo.textStyle.fontAssetId], baseEmSize, ref textInfo);
        }

    }

}