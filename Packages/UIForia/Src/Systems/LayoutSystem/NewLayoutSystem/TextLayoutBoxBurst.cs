using UIForia.Elements;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia.Layout {

    internal unsafe struct TextLayoutBoxBurst : ILayoutBox {

        public ElementId elementId;
        public int textElementInfoIndex;

        public void RunHorizontal(BurstLayoutRunner* runner) {

            ref List_TextLineInfo buffer = ref UnsafeUtilityEx.AsRef<List_TextLineInfo>(runner->lineInfoBuffer);
            ref BurstTextInfo textInfo = ref runner->GetTextInfo(textElementInfoIndex);
            ref LayoutInfo layoutInfo = ref runner->GetHorizontalLayoutInfo(elementId);

            float width = layoutInfo.finalSize - layoutInfo.paddingBorderStart - layoutInfo.paddingBorderStart;

            RunLayoutHorizontal_WordsOnly(ref textInfo, ref buffer, math.max(0, width));

            textInfo.lineInfoList.CopyFrom(buffer.array, buffer.size, Allocator.Persistent);

            // now I need to handle alignment 

            for (int i = 0; i < buffer.size; i++) {
                TextLineInfo lineInfo = buffer[i];
                float position = 0;
                for (int w = lineInfo.wordStart; w < lineInfo.wordStart + lineInfo.wordCount; w++) {
                    ref TextLayoutSymbol layoutSymbol = ref textInfo.layoutSymbolList.array[w];
                    layoutSymbol.wordInfo.x = position;
                    position += layoutSymbol.wordInfo.width;
                }
            }
        }

        public void RunVertical(BurstLayoutRunner* runner) {
            ref BurstTextInfo textInfo = ref runner->GetTextInfo(textElementInfoIndex);
            RunLayoutVertical_WordsOnly(ref UnsafeUtilityEx.AsRef<BurstLayoutRunner>(runner), ref textInfo);
        }

        public float ComputeContentWidth(ref BurstLayoutRunner runner, in BlockSize blockSize) {
            ref List_TextLineInfo buffer = ref UnsafeUtilityEx.AsRef<List_TextLineInfo>(runner.lineInfoBuffer);
            ref BurstTextInfo textInfo = ref runner.GetTextInfo(textElementInfoIndex);

            RunLayoutHorizontal_WordsOnly(ref textInfo, ref buffer, blockSize.insetSize);

            float max = 0;

            for (int i = 0; i < buffer.size; i++) {
                max = math.max(buffer.array[i].width, max);
            }

            // did the work, might as well copy result into the line list. It might change next time we compute but if it doesnt we can use the result
            // how do i know if I can use the result though? maybe i computed that result last frame?

            return max;
        }

        public float ComputeContentHeight(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            ref BurstTextInfo textInfo = ref layoutRunner.GetTextInfo(textElementInfoIndex);
            RunLayoutVertical_WordsOnly(ref  layoutRunner, ref textInfo);
            ref List_TextLineInfo lineInfoList = ref textInfo.lineInfoList;

            return lineInfoList.GetLast().y + lineInfoList.GetLast().height;
        }

        private void RunLayoutVertical_WordsOnly(ref BurstLayoutRunner runner, ref BurstTextInfo textInfo) {
            ref List_TextLayoutSymbol layoutSymbolList = ref textInfo.layoutSymbolList;

            float lineOffset = 0;

            ref List_TextLineInfo lineInfoList = ref textInfo.lineInfoList;

            int fontAssetId = textInfo.textStyle.fontAssetId;

            ref FontAssetInfo fontAsset = ref runner.GetFontAsset(fontAssetId);

            float fontSize = runner.GetResolvedFontSize(elementId);
            
            float smallCapsMultiplier = (textInfo.textStyle.textTransform == TextTransform.SmallCaps) ? 0.8f : 1f;
            float fontScale = fontSize * smallCapsMultiplier / fontAsset.faceInfo.PointSize * fontAsset.faceInfo.Scale;
            
            float lineHeight = textInfo.textStyle.lineHeight * fontAsset.faceInfo.LineHeight * fontScale;
            
            // need to compute a line height for each line
            for (int i = 0; i < lineInfoList.size; i++) {
                ref TextLineInfo lineInfo = ref lineInfoList[i];
                int end = lineInfo.wordStart + lineInfo.wordCount;
                float max = 0;
                for (int w = lineInfo.wordStart; w < end; w++) {
                    max = math.max(max, textInfo.layoutSymbolList.array[w].wordInfo.height);
                }

                lineInfo.height = max;
                lineInfo.y = lineOffset;
                lineOffset += lineHeight;
                for (int w = lineInfo.wordStart; w < end; w++) {
                    layoutSymbolList.array[w].wordInfo.y = lineInfo.y;
                }
            }

        }

        public void Dispose() { }

        public void OnInitialize(LayoutSystem layoutSystem, UIElement element) {
            UITextElement textElement = (UITextElement) element;
            this.elementId = textElement.id;
            this.textElementInfoIndex = textElement.textInfoId;
        }

        // never called
        public float ResolveAutoWidth(ref BurstLayoutRunner runner, ElementId elementId, UIMeasurement measurement, in BlockSize blockSize) {
            return 0;
        }

        // never called
        public float ResolveAutoHeight(ref BurstLayoutRunner runner, ElementId elementId, UIMeasurement measurement, in BlockSize blockSize) {
            return 0;
        }

        private void RunLayoutHorizontal_WordsOnly(ref BurstTextInfo textInfo, ref List_TextLineInfo buffer, float width) {

            ref List_TextLayoutSymbol layoutSymbolList = ref textInfo.layoutSymbolList;

            buffer.size = 0;

            WhitespaceMode whitespaceMode = textInfo.textStyle.whitespaceMode;
            bool trimStart = (whitespaceMode & WhitespaceMode.TrimLineStart) != 0;

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
                        if (cursorX + wordInfo.width > width) {
                            buffer.Add(new TextLineInfo(wordStart, wordCount, cursorX));
                            wordStart = i + 1;
                            wordCount = 1;
                            cursorX = wordInfo.width;
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

    }

}