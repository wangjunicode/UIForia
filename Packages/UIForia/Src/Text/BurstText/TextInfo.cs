using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UIForia.Elements;
using UIForia.Graphics;
using UIForia.Layout;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia.Text {

    [Flags]
    public enum TextLayoutSymbolType : ushort {

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

        //IsBreakable = 1 << 31

    }

    [DebuggerDisplay("Type = {GetDebuggerView()}")]
    [StructLayout(LayoutKind.Explicit)]
    public struct TextLayoutSymbol {

        [FieldOffset(0)] public TextLayoutSymbolType type;
        [FieldOffset(2)] public bool isBreakable;
        [FieldOffset(4)] public WordInfo wordInfo;
        [FieldOffset(4)] public UIFixedLength space;
        [FieldOffset(8)] public float width;
        [FieldOffset(12)] public float height;

        // public bool isBreakable {
        //     get => (type & TextLayoutSymbolType.IsBreakable) != 0;
        //     set {
        //         if (value) {
        //             type |= TextLayoutSymbolType.IsBreakable;
        //         }
        //         else {
        //             type &= ~TextLayoutSymbolType.IsBreakable;
        //         }
        //     }
        // }

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
                switch (layoutSymbol.array[i].type) {
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

    [Flags]
    public enum TextInfoFlags {

        HasRichText = 1 << 0,
        RequiresRichTextLayout = 1 << 1

    }

    [StructLayout(LayoutKind.Sequential)]
    [DebuggerTypeProxy(typeof(TextInfoDebugView))]
    public unsafe struct TextInfo : IDisposable {

        internal List_TextSymbol symbolList;
        internal List_TextLayoutSymbol layoutSymbolList;
        internal List_TextLineInfo lineInfoList; // theres a chance I dont need to store this
        internal List_TextRenderRange renderRangeList;

        internal List_TextMaterialInfo materialBuffer;
        
        internal int renderingCharacterCount;

        // todo -- flags
        internal bool isRichText;
        internal bool requiresRichLayout;
        internal bool requiresTextTransform;
        internal bool requiresRenderProcessing;
        internal bool isRenderDecorated;

        [ThreadStatic] internal static StructList<TextSymbol> inputSymbolBuffer;
        internal float resolvedFontSize;
        public TextMaterialInfo textMaterial;
        private TextInfoFlags flags;
        internal bool hasEffects;
        public bool requiresRenderRangeUpdate;
        public RangeInt selectionRange;

        internal UIFixedLength fontSize;
        internal TextAlignment alignment;
        internal int fontAssetId;
        internal TextTransform textTransform;
        internal WhitespaceMode whitespaceMode;
        internal FontStyle fontStyle;
        internal float lineHeight;

        // parser pushes character instructions into output stream
        // strip whitespace accordingly
        // transform text accordingly
        // convert char instructions to words 
        // measure those
        // layout knows how to handle word stream
        // renderer only handles character stream
        public int GetIndexAtPoint(in DataList<FontAssetInfo>.Shared fontMap, Vector2 point) {
            if (lineInfoList.size == 0) {
                return 0;
            }

            int lineIndex = FindNearestLine(point);

            if (lineIndex < 0) return -1;

            int wordStart = lineInfoList[lineIndex].wordStart;
            int wordEnd = wordStart + lineInfoList[lineIndex].wordCount;
            float closestDistance = float.MaxValue;
            int closestIndex = -1;
            
            for (int i = wordStart; i < wordEnd; i++) {
                ref TextLayoutSymbol layoutSymbol = ref layoutSymbolList[i];

                if (layoutSymbol.type != TextLayoutSymbolType.Word) {
                    continue;
                }

                ref WordInfo wordInfo = ref layoutSymbol.wordInfo;


                for (int charIndex = wordInfo.charStart; charIndex < wordInfo.charEnd; charIndex++) {
                    ref TextSymbol charSymbol = ref symbolList.array[charIndex];
                    if (charSymbol.type != TextSymbolType.Character) {
                        continue;
                    }

                    ref UIForiaGlyph glyph = ref fontMap[charSymbol.charInfo.fontAssetId].glyphList[charSymbol.charInfo.glyphIndex];

                    float midpoint = wordInfo.x + charSymbol.charInfo.position.x + (glyph.width * charSymbol.charInfo.scale * 0.5f);

                    float dist = Mathf.Abs(midpoint - point.x);
                    
                    if (dist < closestDistance) {
                        closestDistance = dist;
                        closestIndex = midpoint <= point.x ? charIndex + 1 : charIndex;
                    }
                }
            }

            return closestIndex;
        }

        public Rect GetCursorRect(in DataList<FontAssetInfo>.Shared fontAssetMap, int cursorIndex, float width = 1) {
            
            if (cursorIndex < 0) return default;
            int symbolIdx = GetCursorSymbolIndex(cursorIndex);
            if (symbolIdx < 0) return default;

            ref BurstCharInfo charInfo = ref symbolList.array[symbolIdx].charInfo;

            if (charInfo.wordIndex >= layoutSymbolList.size) {
                return default;
            }

            ref WordInfo wordInfo = ref layoutSymbolList.array[charInfo.wordIndex].wordInfo;
            float2 position = charInfo.position + new float2(wordInfo.x, wordInfo.y);
            position.x -= fontAssetMap[charInfo.fontAssetId].glyphList[charInfo.glyphIndex].xOffset * charInfo.scale;
            return new Rect(position.x, lineInfoList.array[charInfo.lineIndex].y, width, lineInfoList.array[charInfo.lineIndex].height);
        }

        private int GetCursorSymbolIndex(int cursorIndex) {
            int charIdx = 0;
            
            for (int i = 0; i < symbolList.size; i++) {
                ref TextSymbol symbol = ref symbolList.array[i];
                if (symbol.type != TextSymbolType.Character) {
                    continue;
                }

                if (charIdx == cursorIndex) {
                    return i;
                }

                charIdx++;
            }

            return -1;
        }

        private int FindNearestLine(Vector2 point) {
            if (lineInfoList.size == 0) {
                return -1;
            }

            if (point.y <= lineInfoList[0].y) {
                return 0;
            }

            if (point.y >= lineInfoList[lineInfoList.size - 1].y) {
                return lineInfoList.size - 1;
            }

            float closestDistance = float.MaxValue;
            int closestIndex = 0;
            for (int i = 0; i < lineInfoList.size; i++) {
                ref TextLineInfo line = ref lineInfoList.array[i];
                float y1 = line.y;
                float y2 = y1 + line.height;

                if (point.y >= y1 && point.y <= y2) {
                    return i;
                }

                float distToY1 = Mathf.Abs(point.y - y1);
                float distToY2 = Mathf.Abs(point.y - y2);
                if (distToY1 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToY1;
                }

                if (distToY2 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToY2;
                }
            }

            return closestIndex;
        }

        internal static void ProcessWhitespace(ref TextInfo textInfo, ref DataList<TextSymbol> symbolBuffer) {
            symbolBuffer.size = 0;

            TextUtil.ProcessWhiteSpace(textInfo.whitespaceMode, textInfo.symbolList.array, textInfo.symbolList.size, ref symbolBuffer);

            if (symbolBuffer.size != textInfo.symbolList.size) {
                textInfo.symbolList.CopyFrom(symbolBuffer.GetArrayPointer(), symbolBuffer.size);
            }
        }

        internal static void CreateLayoutSymbols(ref TextInfo textInfo, ref DataList<TextLayoutSymbol> layoutBuffer) {
            TextUtil.CreateLayoutSymbols(textInfo.symbolList.array, textInfo.symbolList.size, ref layoutBuffer);

            if (textInfo.layoutSymbolList.array == null) {
                textInfo.layoutSymbolList = new List_TextLayoutSymbol(layoutBuffer.size, Allocator.Persistent);
            }

            textInfo.layoutSymbolList.CopyFrom(layoutBuffer.GetArrayPointer(), layoutBuffer.size);
        }

        internal static void CountRenderedCharacters(ref TextInfo textInfo) {
            int cnt = 0;

            for (int s = 0; s < textInfo.symbolList.size; s++) {
                // todo -- also handle disabled characters
                if (textInfo.symbolList.array[s].type == TextSymbolType.Character) {
                    if ((textInfo.symbolList.array[s].charInfo.flags & CharacterFlags.Visible) != 0) {
                        cnt++;
                    }
                }
            }

            textInfo.renderingCharacterCount = cnt;
        }

        internal static void ComputeSize(DataList<FontAssetInfo>.Shared fontAssetMap, ref TextInfo textInfo, float emSize, ref TextMeasureState measureState) {
            ComputeSizeInfo sizeInfo = new ComputeSizeInfo();
            measureState.Initialize(emSize, textInfo, fontAssetMap[textInfo.fontAssetId]);
            TextUtil.RecomputeFontInfo(ref measureState, ref sizeInfo);

            textInfo.resolvedFontSize = measureState.fontSize;

            for (int i = 0; i < textInfo.layoutSymbolList.size; i++) {
                ref TextLayoutSymbol layoutSymbol = ref textInfo.layoutSymbolList[i];

                TextLayoutSymbolType type = layoutSymbol.type;

                if (type == TextLayoutSymbolType.Word) {
                    TextUtil.MeasureWord(fontAssetMap, i, ref layoutSymbol.wordInfo, textInfo.symbolList, ref sizeInfo, ref measureState);
                }
                else if (type == TextLayoutSymbolType.HorizontalSpace) {
                    // todo -- lookup the em tree for given element
                }
            }
        }

        public static void UpdateText(ref TextInfo textInfo, string text, ITextProcessor processor, TextSystem textSystem) {
            bool requiresTextTransform = false;
            bool requiresRichTextLayout = false;
            bool processedStream = false;

            if (inputSymbolBuffer == null) {
                inputSymbolBuffer = new StructList<TextSymbol>(128);
            }

            inputSymbolBuffer.size = 0;
            int length = text.Length;

            textInfo.hasEffects = false;
            textInfo.requiresRenderProcessing = true; // always re-process material buffer when text updates
            textInfo.requiresRenderRangeUpdate = true; // always re-update ranges when changing text
            fixed (char* charptr = text) {
                if (processor != null) {
                    CharStream stream = new CharStream(charptr, 0, (uint) length);
                    LightList<PendingTextEffectSymbolData> textEffects = LightList<PendingTextEffectSymbolData>.Get();
                    TextSymbolStream symbolStream = new TextSymbolStream(textEffects, inputSymbolBuffer);

                    processedStream = processor.Process(stream, ref symbolStream);

                    requiresTextTransform = symbolStream.requiresTextTransform;
                    requiresRichTextLayout = symbolStream.requiresRichTextLayout;
                    inputSymbolBuffer = symbolStream.stream;

                    // todo -- diff with previous stream if there was one, but only if using effects or a type writer

                    if (symbolStream.textEffects.size > 0) {
                        textInfo.hasEffects = true;
                        bool replacing = true;

                        for (int i = 0; i < textEffects.size; i++) {
                            PendingTextEffectSymbolData effectData = textEffects.array[i];

                            ref TextSymbol symbol = ref symbolStream.stream.array[effectData.symbolIndex];

                            Assert.IsTrue(symbol.type == TextSymbolType.EffectPush);

                            // if replacing we spawn a new one, otherwise ask the effect to re-parse 
                            TextEffect effect = textSystem.SpawnTextEffect(effectData.effectId.id, out int instanceId);
                            symbol.effectInfo.instanceId = instanceId;
                            symbol.effectInfo.spawnerId = effectData.effectId.id;

                            effect.isActive = true;

                            if (effect is IUIForiaRichTextEffect richTextEffect) {
                                effect.isActive = richTextEffect.TryParseRichTextAttributes(effectData.bodyStream);
                            }
                        }
                    }

                    textEffects.Release();
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
            if (requiresRichTextLayout) {
                textInfo.flags |= TextInfoFlags.RequiresRichTextLayout;
            }

            textInfo.symbolList.SetSize(inputSymbolBuffer.size, Allocator.Persistent);

            fixed (TextSymbol* inputPtr = inputSymbolBuffer.array) {
                textInfo.symbolList.CopyFrom(inputPtr, inputSymbolBuffer.size);
            }
        }

        internal static void RunLayoutHorizontal_RichText<T>(ref TextInfo textInfo, ref T buffer, float width) where T : IBasicList<TextLineInfo> {
            ref List_TextLayoutSymbol layoutSymbolList = ref textInfo.layoutSymbolList;

            buffer.SetSize(0);

            WhitespaceMode whitespaceMode = textInfo.whitespaceMode;
            bool trimStart = (whitespaceMode & WhitespaceMode.TrimLineStart) != 0;

            int wordStart = 0;
            int wordCount = 0;
            float cursorX = 0;

            for (int i = 0; i < layoutSymbolList.size; i++) {
                ref TextLayoutSymbol layoutSymbol = ref layoutSymbolList.array[i];
                TextLayoutSymbolType type = layoutSymbol.type;
                //& ~TextLayoutSymbolType.IsBreakable);

                switch (type) {
                    case TextLayoutSymbolType.Word: {
                        ref WordInfo wordInfo = ref layoutSymbol.wordInfo;

                        if (!layoutSymbol.isBreakable) {
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

                        break;
                    }

                    case TextLayoutSymbolType.HorizontalSpace:
                        cursorX += layoutSymbol.space.value;
                        layoutSymbol.width = layoutSymbol.space.value; // todo -- use resolved size
                        wordCount++;
                        break;

                    case TextLayoutSymbolType.LineHeightPush:
                        break;

                    case TextLayoutSymbolType.LineHeightPop:
                        break;

                    case TextLayoutSymbolType.LineIndentPush:
                        break;

                    case TextLayoutSymbolType.LineIndentPop:
                        break;

                    case TextLayoutSymbolType.MarginLeftPush:
                        break;

                    case TextLayoutSymbolType.MarginLeftPop:
                        break;

                    case TextLayoutSymbolType.MarginRightPush:
                        break;

                    case TextLayoutSymbolType.MarginRightPop:
                        break;

                    case TextLayoutSymbolType.IndentPush:
                        break;

                    case TextLayoutSymbolType.AlignPush:
                        break;

                    case TextLayoutSymbolType.IndentPop:
                        break;
                }
            }

            if (wordCount != 0) {
                buffer.Add(new TextLineInfo(wordStart, wordCount, cursorX));
            }
        }

        internal static void RunLayoutHorizontal_WordsOnly<T>(ref TextInfo textInfo, ref T buffer, float width) where T : IBasicList<TextLineInfo> {
            ref List_TextLayoutSymbol layoutSymbolList = ref textInfo.layoutSymbolList;

            buffer.SetSize(0);

            WhitespaceMode whitespaceMode = textInfo.whitespaceMode;
            bool trimStart = (whitespaceMode & WhitespaceMode.TrimLineStart) != 0;
            bool allowWrapping = (whitespaceMode & WhitespaceMode.NoWrap) == 0;

            int wordStart = 0;
            int wordCount = 0;
            float cursorX = 0;

            for (int i = 0; i < layoutSymbolList.size; i++) {
                ref TextLayoutSymbol layoutSymbol = ref layoutSymbolList.array[i];
                ref WordInfo wordInfo = ref layoutSymbol.wordInfo;
                bool isBreakable = layoutSymbol.isBreakable;

                if (!isBreakable) {
                    wordCount++;
                    cursorX += wordInfo.width;
                    continue;
                }

                switch (wordInfo.type) { 
                    case WordType.Whitespace: {
                        // if whitespace overruns end of line, start a new one and add it to that line
                        if (allowWrapping && cursorX + wordInfo.width > width) {
                            buffer.Add(new TextLineInfo(wordStart, wordCount, cursorX));
                            wordStart = i;
                            wordCount = 1;
                            cursorX = trimStart ? 0 : wordInfo.width;
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
                        if (allowWrapping && wordInfo.width > width) {
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
                        else if (allowWrapping && cursorX + wordInfo.width >= width + 0.5f) {
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

        internal static unsafe void RunLayoutVertical_RichText(in FontAssetInfo fontAsset, float fontSize, ref TextInfo textInfo) {
            // for (int i = 0; i < layoutSymbolList.size; i++) {
            //     ref TextLayoutSymbol layoutSymbol = ref layoutSymbolList.array[i];
            //     TextLayoutSymbolType type = (layoutSymbol.type & ~TextLayoutSymbolType.IsBreakable);
            //
            //     switch (type) {
            //
            //         case TextLayoutSymbolType.Word: {
            //
            //             break;
            //         }
            //
            //         case TextLayoutSymbolType.HorizontalSpace:
            //
            //             break;
            //
            //         case TextLayoutSymbolType.LineHeightPush:
            //             break;
            //
            //         case TextLayoutSymbolType.LineHeightPop:
            //             break;
            //
            //         case TextLayoutSymbolType.LineIndentPush:
            //             break;
            //
            //         case TextLayoutSymbolType.LineIndentPop:
            //             break;
            //
            //         case TextLayoutSymbolType.MarginLeftPush:
            //             break;
            //
            //         case TextLayoutSymbolType.MarginLeftPop:
            //             break;
            //
            //         case TextLayoutSymbolType.MarginRightPush:
            //             break;
            //
            //         case TextLayoutSymbolType.MarginRightPop:
            //             break;
            //
            //         case TextLayoutSymbolType.IndentPush:
            //             break;
            //
            //         case TextLayoutSymbolType.AlignPush:
            //             break;
            //
            //         case TextLayoutSymbolType.IndentPop:
            //             break;
            //
            //         case TextLayoutSymbolType.IsBreakable:
            //             break;
            //
            //     }
            //
            // }
        }

        internal static unsafe void RunLayoutVertical_WordsOnly(in FontAssetInfo fontAsset, float fontSize, ref TextInfo textInfo) {
            ref List_TextLayoutSymbol layoutSymbolList = ref textInfo.layoutSymbolList;

            float lineOffset = 0;

            ref List_TextLineInfo lineInfoList = ref textInfo.lineInfoList;

            float smallCapsMultiplier = (textInfo.textTransform == TextTransform.SmallCaps) ? 0.8f : 1f;
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
                    ref TextLayoutSymbol layoutSymbol = ref textInfo.layoutSymbolList.array[w];
                    ref WordInfo wordInfo = ref textInfo.layoutSymbolList.array[w].wordInfo;
                    if (wordInfo.height > max) {
                        max = textInfo.layoutSymbolList.array[w].wordInfo.height;
                    }

                    ushort lineIdx = (ushort) i;
                    // assign line index to character symbols
                    for (int c = wordInfo.charStart; c < wordInfo.charEnd; c++) {
                        textInfo.symbolList.array[c].charInfo.lineIndex = lineIdx;
                    }
                }

                lineInfo.height = lineHeight;
                lineInfo.y = lineOffset;
                lineOffset += max * textInfo.lineHeight;
                for (int w = lineInfo.wordStart; w < end; w++) {
                    layoutSymbolList.array[w].wordInfo.y = lineInfo.y;
                }

                // baseline defined by tallest symbol on line's lower position? doesnt handle 
                // 
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
            symbolList.Dispose();
            layoutSymbolList.Dispose();
            lineInfoList.Dispose();
            renderRangeList.Dispose();
            materialBuffer.Dispose();
        }

        // internal static unsafe void ApplyTextAlignment(ref TextInfo textInfo, float totalWidth) {
        //     TextAlignment alignment = textInfo.textStyle.alignment;
        //     for (int i = 0; i < textInfo.lineInfoList.size; i++) {
        //         ref TextLineInfo lineInfo = ref textInfo.lineInfoList.array[i];
        //         float lineOffsetX = lineInfo.x;
        //
        //         switch (alignment) {
        //
        //             default:
        //             case TextAlignment.Unset:
        //             case TextAlignment.Left:
        //                 break;
        //
        //             case TextAlignment.Right:
        //                 lineOffsetX = totalWidth - lineInfo.width;
        //                 break;
        //
        //             case TextAlignment.Center:
        //                 lineOffsetX = (totalWidth - lineInfo.width) * 0.5f;
        //                 break;
        //         }
        //
        //         int lsEnd = lineInfo.wordStart + lineInfo.wordCount;
        //         for (int ls = lineInfo.wordStart; ls < lsEnd; ls++) {
        //             ref TextLayoutSymbol symbol = ref textInfo.layoutSymbolList.array[ls];
        //             if (symbol.type == TextLayoutSymbolType.Word) {
        //                 symbol.wordInfo.x += lineOffsetX;
        //             }
        //         }
        //     }
        // }

    }

    public struct PendingTextEffectSymbolData {

        public int symbolIndex;
        public CharStream bodyStream;
        public TextEffectId effectId;

    }

    public struct TextEffectId {

        public readonly int id;

        public TextEffectId(int id) {
            this.id = id;
        }

    }

    public struct TextVertexOverride {

        public float2 topLeft;
        public float2 topRight;
        public float2 bottomRight;
        public float2 bottomLeft;

    }

    public interface IUIForiaRichTextEffect {

        bool TryParseRichTextAttributes(CharStream stream);

    }

    public abstract class TextEffect<T> : TextEffect {

        public abstract void SetParameters(T parameters);

    }

    public abstract class TextEffect {

        public bool isActive;

        internal UITextElement textElement;

        public virtual void OnTextChanged() { }

        public virtual void OnTextLayout() { }

        public virtual void OnVisualsChanged() { }

        public virtual void OnPush(float4x4 worldMatrix, UITextElement element) { }

        public virtual void OnPop() { }

        public virtual void OnApplyEffect(ref CharacterInterface characterInterface) { }

        public virtual void ApplyEffect(ref CharacterInterface characterInterface) {
//            characterInterface.isRevealed;
//            characterInterface.isRevealing;
//            
//            characterInterface.RotateVertices(45, characterInterface.GetTopLine());
//            characterInterface.SetMaterial();
//            characterInterface.SetGlowColor();
//            
//            characterInterface.span.Insert();
//            
//            // layout start = lock
//            // render end = unlock
//            
//            characterInterface.SetDisplayedGlyph('a');

            // span.GetCharacterAt(i).SetEffect(somecomputed);
            // [underline color=x]
        }

    }

}