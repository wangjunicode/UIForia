using System;
using UIForia.Layout;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Prototype {

    internal unsafe struct TextLayout {

        // todo --
        // image / reserved box
        // word spacing
        // char spacing
        // click / events / hittesting
        // selection
        // decorations
        // uv transforms? or part of effects system? probably effects 
        // [won't fix]case transform
        // layout needs to provide xy positions and a line index per character run. and a bounding box for the whole thing (lines can overflow this box) 

        // horizontal cursor position and vertical offsets will be handled via render system instead of layout
        
        internal static RangeInt FlowCharacterRuns(float maxWidth, ref TextLayoutStore store, ref DataList<TextLineInfoNew> lineList, CheckedArray<char> textData, CheckedArray<TextSymbol> symbolBuffer, CheckedArray<TextCharacterRun> runList, in TextLayoutInfo layoutInfo) {

            int lineListStartIndex = lineList.size;

            int runCountOnLine = 0;
            int runStart = 0;
            int dataOffset = 0;
            int lastRunIndex = 0;

            TextAlignment alignment = layoutInfo.alignment;
            WhitespaceMode whitespaceMode = layoutInfo.whitespaceMode;
            VerticalAlignment verticalAlignment = layoutInfo.verticalAlignment;

            store.Reset();

            // could reduce these to single array with reverse search for push pop & a count to early out of search

            ref DataList<TextAlignment> alignmentStack = ref store.alignmentStack;
            ref DataList<WhitespaceMode> whitespaceModeStack = ref store.whitespaceModeStack;
            ref DataList<LineInset> insetStack = ref store.insetStack;
            ref DataList<VerticalAlignment> verticalAlignStack = ref store.verticalAlignStack;

            verticalAlignStack.Add(verticalAlignment);
            alignmentStack.Add(alignment);
            whitespaceModeStack.Add(whitespaceMode);
            insetStack.Add(new LineInset(layoutInfo.lineStartInset, layoutInfo.lineEndInset));

            float lineStart = layoutInfo.lineStartInset; // edited by margins and indents

            float cursorX = lineStart;
            float lineWidth = maxWidth - layoutInfo.lineEndInset;

            bool trimTextStart = (whitespaceMode & WhitespaceMode.TrimStart) != 0;
            bool trimTextEnd = (whitespaceMode & WhitespaceMode.TrimEnd) != 0;

            const int k_FloatSizeInChars = 2;
            bool didLastCharacterRunWrap = false;

            for (int i = 0; i < symbolBuffer.size; i++) {

                TextSymbol symbol = symbolBuffer[i];

                switch (symbol.symbolType) {

                    case SymbolType.PushLineInsets: {
                        float insetStart = *(float*) textData.GetPointer(dataOffset);
                        float insetEnd = *(float*) textData.GetPointer(dataOffset + k_FloatSizeInChars);
                        insetStack.Add(new LineInset(insetStart, insetEnd));

                        lineWidth -= insetEnd;
                        lineStart += insetStart;

                        break;
                    }

                    case SymbolType.PopLineInsets: {
                        if (insetStack.size == 1) {
                            break;
                        }

                        lineStart -= insetStack.GetLast().insetStart;
                        lineWidth += insetStack.GetLast().insetEnd;

                        insetStack.Pop();

                        break;
                    }

                    case SymbolType.PushWhitespaceMode: {
                        whitespaceMode = (WhitespaceMode) (*(ushort*) textData.GetPointer(dataOffset));
                        whitespaceModeStack.Add(whitespaceMode);
                        break;
                    }

                    case SymbolType.PopWhitespaceMode: {
                        if (whitespaceModeStack.size != 1) {
                            whitespaceModeStack.Pop();
                            whitespaceMode = whitespaceModeStack.GetLast();
                        }

                        break;
                    }

                    case SymbolType.PushAlignment: {
                        alignment = (TextAlignment) (*(ushort*) textData.GetPointer(dataOffset));
                        alignmentStack.Add(alignment);
                        break;
                    }

                    case SymbolType.PopAlignment: {
                        if (alignmentStack.size != 1) {
                            alignmentStack.Pop();
                            alignment = alignmentStack.GetLast();
                        }

                        break;
                    }

                    // treated the same as whitespace 
                    case SymbolType.HorizontalSpace: {

                        float advance = runList.Get(lastRunIndex).width;
                        bool canWrap = (whitespaceMode & WhitespaceMode.NoWrap) == 0;

                        if (canWrap && cursorX + advance > lineWidth) {
                            lineList.Add(new TextLineInfoNew(runStart, runCountOnLine, cursorX, lineStart, alignment, whitespaceMode, lineWidth));
                            runStart = lastRunIndex;
                            runCountOnLine = 1;
                            cursorX = lineStart; //run.advance; there may be cases in which we want to keep this whitespace
                        }
                        else {
                            runCountOnLine++;
                            cursorX += advance;
                        }

                        lastRunIndex++;
                        break;
                    }

                    case SymbolType.InlineSpace: {

                        Size size = *(Size*) textData.GetPointer(dataOffset);
                        float advance = size.width;

                        bool canWrap = (whitespaceMode & WhitespaceMode.NoWrap) == 0;

                        if (canWrap && cursorX + advance > lineWidth) {
                            lineList.Add(new TextLineInfoNew(runStart, runCountOnLine, cursorX, lineStart, alignment, whitespaceMode, lineWidth));
                            runStart = lastRunIndex;
                            runCountOnLine = 1;
                            cursorX = lineStart; //run.advance; there may be cases in which we want to keep this whitespace
                        }
                        else {
                            runCountOnLine++;
                            cursorX += advance;
                        }

                        break;
                    }

                    case SymbolType.CharacterGroup: {

                        // metaInfo will be -1 of no characters are in this group. This can happen when merging rich text symbosl around a non layout symbol like [Color]
                        // example: he[color]llo
                        // where we shape as though the color symbol was not there but still have a symbol in our buffer 
                        if (symbol.metaInfo < 0) break;

                        for (int r = lastRunIndex; r < runList.size; r++) {

                            ref TextCharacterRun run = ref runList.Get(r);

                            float advance = run.width;

                            bool canWrap = (whitespaceMode & WhitespaceMode.NoWrap) == 0;

                            switch (run.GetRunType()) {

                                case CharacterRunType.Characters: {
                                    if (canWrap && advance > lineWidth) {
                                        didLastCharacterRunWrap = true;
                                        if (runCountOnLine != 0) {
                                            lineList.Add(new TextLineInfoNew(runStart, runCountOnLine, cursorX, lineStart, alignment, whitespaceMode, lineWidth));
                                            cursorX = lineStart;
                                            runStart = r;
                                            runCountOnLine = 1;
                                        }
                                        else {
                                            lineList.Add(new TextLineInfoNew(runStart, 1, advance, lineStart, alignment, whitespaceMode, lineWidth));
                                            cursorX = lineStart;
                                            runStart = r + 1;
                                            runCountOnLine = 0;
                                        }
                                    }
                                    // next word is too long, break it onto the next line
                                    else if (canWrap && cursorX + advance >= lineWidth + 0.5f) {
                                        didLastCharacterRunWrap = true;
                                        lineList.Add(new TextLineInfoNew(runStart, runCountOnLine, cursorX, lineStart, alignment, whitespaceMode, lineWidth));
                                        runStart = r;
                                        runCountOnLine = 1;
                                        cursorX = lineStart + advance;
                                    }
                                    else {
                                        didLastCharacterRunWrap = false;
                                        // we fit, just add to cursor position and word count
                                        cursorX += advance;
                                        runCountOnLine++;
                                    }

                                    break;
                                }

                                case CharacterRunType.Whitespace: {
                                    // if (runCountOnLine == 0) {
                                    //     runStart++;
                                    // }
                                    // else
                                    if (canWrap && cursorX + advance > lineWidth) {
                                        didLastCharacterRunWrap = true;
                                        lineList.Add(new TextLineInfoNew(runStart, runCountOnLine, cursorX, lineStart, alignment, whitespaceMode, lineWidth));
                                        runStart = r + 1;
                                        runCountOnLine = 0;
                                        cursorX = lineStart; //run.advance; there may be cases in which we want to keep this whitespace
                                    }
                                    else {
                                        didLastCharacterRunWrap = false;
                                        runCountOnLine++;
                                        cursorX += advance;
                                    }

                                    break;
                                }

                                case CharacterRunType.NewLine: {
                                    int reportedRuns = runCountOnLine;
                                    if (reportedRuns == 0) reportedRuns = 1;
                                    lineList.Add(new TextLineInfoNew(runStart, reportedRuns, cursorX, lineStart, alignment, whitespaceMode, lineWidth));
                                    cursorX = lineStart;
                                    runStart += runCountOnLine + 1; // skip the new line itself
                                    runCountOnLine = 0;
                                    break;
                                }

                                // never hits, handled at symbol type
                                case CharacterRunType.Sprite:
                                case CharacterRunType.HorizontalSpace:
                                case CharacterRunType.VerticalSpace:
                                case CharacterRunType.ReserveSpace:
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            if (run.IsLastInGroup) {
                                lastRunIndex = r + 1;
                                break;
                            }

                        }

                        break;
                    }
                }

                dataOffset += symbol.dataLength;

            }

            // if we have leftover content after loop body, register it on a line struct
            if (runCountOnLine != 0) {
                lineList.Add(new TextLineInfoNew(runStart, runCountOnLine, cursorX, lineStart, alignment, whitespaceMode, lineWidth));
            }

            if (!didLastCharacterRunWrap && runList.size > 0 && runList[runList.size - 1].IsNewLine) {
                lineList.Add(new TextLineInfoNew(runList.size - 1, 1, cursorX, lineStart, alignment, whitespaceMode, lineWidth));
            }

            ClampMaxLines(layoutInfo, ref lineList);

            RangeInt lineRange = new RangeInt(lineListStartIndex, lineList.size - lineListStartIndex);

            TrimLines(lineRange, ref lineList, trimTextStart, trimTextEnd, runList);

            return lineRange;
        }

        private static void TrimLines(RangeInt lineRange, ref DataList<TextLineInfoNew> lineList, bool trimTextStart, bool trimTextEnd, CheckedArray<TextCharacterRun> runList) {
            if (lineList.size == 0) return;
            
            if (trimTextStart) {
                lineList[lineRange.start].whitespaceMode |= WhitespaceMode.TrimLineStart;
            }

            if (trimTextEnd) {
                lineList[lineRange.end - 1].whitespaceMode |= WhitespaceMode.TrimLineEnd;
            }

            for (int i = lineRange.start; i < lineRange.end; i++) {

                ref TextLineInfoNew line = ref lineList.Get(i);

                if (line.runCount <= 1) {
                    continue;
                }

                if ((line.whitespaceMode & WhitespaceMode.TrimLineStart) != 0 && runList[line.runStart].IsWhiteSpace) {

                    line.width -= runList[line.runStart].width;
                    line.runStart++;
                    line.runCount--;

                    if (line.runCount <= 1) {
                        continue;
                    }

                }

                int endIndex = line.runStart + line.runCount - 1;

                if ((line.whitespaceMode & WhitespaceMode.TrimLineEnd) != 0 && runList[endIndex].IsWhiteSpace) {
                    line.width -= runList[endIndex].width;
                    line.runCount--;
                }

            }
        }

        public static float GetHeight(DataList<SDFFontUnmanaged> fontTable, CheckedArray<TextCharacterRun> runList, CheckedArray<TextLineInfoNew> lineList) {

            float lineY = 0;

            for (int lineIndex = 0; lineIndex < lineList.size; lineIndex++) {

                ref TextLineInfoNew info = ref lineList.array[lineIndex];

                int end = info.runStart + info.runCount;

                float maxAscent = 0;
                float maxDescent = 0;

                for (int rIdx = info.runStart; rIdx < end; rIdx++) {

                    ref TextCharacterRun run = ref runList.Get(rIdx);
                    ref SDFFontUnmanaged font = ref fontTable.Get(run.fontId);

                    float ascent = run.fontSize;
                    float descent = font.descent * run.fontSize;

                    if (maxAscent < ascent) {
                        maxAscent = ascent;
                    }

                    if (-maxDescent < -descent) {
                        maxDescent = descent;
                    }

                }

                float lineHeight = maxAscent - maxDescent;
                // float halfLeading = -maxDescent * 0.5f;
                // float baseline = maxAscent;
                // lineY += lineHeight; // halfLeading + baseline;
                // lineY += (lineHeight + halfLeading) * info.lineCount;
                lineY += lineHeight * info.lineCount;
                // if (lineIndex + 1 == lineList.size && runList.Get(info.runStart + (info.runCount - 1)).IsNewLine) {
                //     lineY += lineHeight;
                // }
            }

            return lineY;
        }

        internal static TextLayoutOutput LayoutLines(float maxWidth, in OffsetRect padding, DataList<SDFFontUnmanaged> fontTable, in TextLayoutInfo layoutInfo, CheckedArray<TextCharacterRun> runList, CheckedArray<TextLineInfoNew> lineList) {

            TextLayoutOutput output = new TextLayoutOutput() {
                yOffset = 0,
                ellipsisRunIndex = -1
            };

            float penY = padding.top;

            for (int lineIndex = 0; lineIndex < lineList.size; lineIndex++) {

                ref TextLineInfoNew info = ref lineList.Get(lineIndex);

                int end = info.runStart + info.runCount;

                int stretchParts = 0;

                float lineStart = padding.left + info.x; // todo -- only 50% sure this is right, need to figure out right align inset
                float placementX = lineStart;

                float remaining = info.maxWidth;

                float maxAscent = 0;
                float maxDescent = 0;

                for (int rIdx = info.runStart; rIdx < end; rIdx++) {

                    ref TextCharacterRun run = ref runList.Get(rIdx);
                    ref SDFFontUnmanaged font = ref fontTable.Get(run.fontId);

                    if ((run.flags & CharacterRunFlags.HorizontalSpace) != 0) {
                        stretchParts += run.StretchParts;
                    }

                    run.x = placementX;
                    run.lineIndex = (ushort) lineIndex;

                    placementX += run.width;
                    remaining -= run.width;

                    float ascent = run.fontSize;
                    float descent = font.descent * run.fontSize;

                    if (maxAscent < ascent) {
                        maxAscent = ascent;
                    }

                    if (-maxDescent < -descent) {
                        maxDescent = descent;
                    }

                }

                float lineHeight = maxAscent - maxDescent;
                float halfLeading = -maxDescent * 0.5f;

                float baseline = maxAscent;
                // penY += halfLeading + baseline;
                penY += baseline;
                
                for (int rIdx = info.runStart; rIdx < end; rIdx++) {
                    ref TextCharacterRun run = ref runList.Get(rIdx);
                   
                    switch (run.verticalAlignment) {

                        default:
                        case VerticalAlignment.Baseline:
                            run.y = penY;
                            break;

                        case VerticalAlignment.Top:
                            run.y = penY - (baseline - (runList[rIdx].fontSize));
                            break;

                        case VerticalAlignment.Bottom: {
                            ref SDFFontUnmanaged font = ref fontTable.Get(run.fontId);
                            run.y = penY + (font.descent * run.fontSize - maxDescent);
                            break;
                        }

                        case VerticalAlignment.CenterTemp: {
                            run.y = penY;

                            // baseline = center of line 
                            // float diff = baseline - runList[rIdx].fontSize;
                            // ref CharacterRun run = ref runList.Get(rIdx);
                            //
                            // if (diff == 0) {
                            //     result.y = lineY;
                            //     break;
                            // }
                            //
                            // float baseAscent = (lineHeight / 2);
                            // baseAscent -= run.fontSize / 2;
                            // result.y = lineY - baseline - baseAscent;
                            break;
                        }

                        case VerticalAlignment.Center: {
                            float diff = baseline - runList[rIdx].fontSize;
                            run.y = penY - diff / 2;
                            break;
                        }
                    }
                }

                penY += -maxDescent;
                // penY += (lineHeight * (info.lineCount - 1));

                if (stretchParts > 0 && remaining > 0 && info.maxWidth != float.MaxValue) {
                    StretchLine(info, runList, remaining, stretchParts);
                }

                if (lineIndex == 0) {
                    output.yOffset = -penY;
                }

                switch (info.alignment) {

                    default:
                    case TextAlignment.Left:
                        // no work here, this is the default already 
                        break;

                    case TextAlignment.Right: {

                        float shift = info.maxWidth - info.width;
                        for (int i = info.runStart; i < info.runStart + info.runCount; i++) {
                            runList.Get(i).x += shift;
                        }

                        break;
                    }

                    case TextAlignment.Center: {
                        float shift = (info.maxWidth - info.width) * 0.5f;
                        for (int i = info.runStart; i < info.runStart + info.runCount; i++) {
                            runList.Get(i).x += shift;
                        }

                        break;
                    }

                }
            }

            output.ellipsisRunIndex = HandleEllipsisOverflow(maxWidth, layoutInfo, lineList, runList);

            return output;
        }

        private static int HandleEllipsisOverflow(float maxWidth, in TextLayoutInfo layoutInfo, CheckedArray<TextLineInfoNew> lineList, CheckedArray<TextCharacterRun> runList) {
            int firstNoFit = -1;

            if (layoutInfo.overflow == TextOverflow.Ellipsis) {

                ref TextLineInfoNew lastLine = ref lineList.Get(lineList.size - 1);

                if (lastLine.width < maxWidth) {
                    return -1;
                }

                // find first character run that overflows our line bounds 
                // record index of that run 
                // font or font size could change on line and ellipsis width != expected width 

                for (int runIdx = lastLine.runStart; runIdx < lastLine.runStart + lastLine.runCount; runIdx++) {

                    float extent = runList[runIdx].x + runList[runIdx].width;

                    if (extent > maxWidth) {
                        firstNoFit = runIdx;
                        break;
                    }

                }

            }

            return firstNoFit;
        }

        private static void ClampMaxLines(in TextLayoutInfo layoutInfo, ref DataList<TextLineInfoNew> lineList) {

            if (layoutInfo.maxLineCount <= 0 || lineList.size <= layoutInfo.maxLineCount) {
                return;
            }

            lineList.size = layoutInfo.maxLineCount;

        }

        private static void StretchLine(TextLineInfoNew info, CheckedArray<TextCharacterRun> runList, float remaining, int totalStretch) {
            int end = info.runStart + info.runCount;

            // todo -- reuse this?
            TempList<StretchInfo> stretchList = TypedUnsafe.MallocSizedTempList<StretchInfo>(info.runCount, Allocator.Temp);

            int stretchIdx = 0;
            for (int i = info.runStart; i < end; i++) {
                ref StretchInfo stretchInfo = ref stretchList.Get(stretchIdx++);
                ref TextCharacterRun run = ref runList.Get(i);
                stretchInfo.minSize = run.width;
                stretchInfo.maxSize = float.MaxValue;
                stretchInfo.stretchParts = 0;
                stretchInfo.stretchParts = (run.flags & CharacterRunFlags.HorizontalSpace) != 0 ? run.StretchParts : 0;
            }

            float pieceSize = remaining / totalStretch;

            for (int i = 0; i < stretchList.size; i++) {
                ref StretchInfo item = ref stretchList.array[i];
                item.minSize += item.stretchParts * pieceSize;
            }

            float xOffset = 0;
            stretchIdx = 0;
            for (int i = info.runStart; i < end; i++) {
                ref StretchInfo item = ref stretchList.array[stretchIdx++];
                ref TextCharacterRun run = ref runList.Get(i);
                run.x = xOffset;
                xOffset += item.minSize;
            }

            stretchList.Dispose();

        }

        public static float GetWidth(CheckedArray<TextLineInfoNew> lines) {

            if (lines.size == 0) return 0;
            
            float maxWidth = lines[0].width;

            for (int i = 1; i < lines.size; i++) {
                if (maxWidth < lines[i].width) {
                    maxWidth = lines[i].width;
                }
            }

            return maxWidth == 0 ? 0 : maxWidth + 1; // account for sdf padding but this is cheating and bad
        }

    }

    internal struct LineInset {

        public float insetStart;
        public float insetEnd;

        public LineInset(float insetStart, float insetEnd) {
            this.insetStart = insetStart;
            this.insetEnd = insetEnd;
        }

    }

}