using System;
using UIForia.Elements;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {

    internal static unsafe class TextUtil {
        internal static LightList<LineMapping> CreateGlyphLineMappings(in TextDataEntry entry, ref TextShapeCache shapeCache) {

            LightList<LineMapping> lines = new LightList<LineMapping>();
            if (entry.runList.size == 0) {
                return lines;
            }

            LineMapping currentLine = new LineMapping();
            currentLine.mappings = StructList<GlyphMapping>.Get();

            int runOffset = 0;
            int lastLineIndex = 0;
            float y = 0;

            for (int i = 0; i < entry.runList.size; i++) {
                ref TextCharacterRun run = ref entry.runList.array[i];
                CharacterRunType type = run.GetRunType();

                switch (type) {

                    case CharacterRunType.Characters:
                    case CharacterRunType.Whitespace: {

                        DisplayedCharacterType charType = type == CharacterRunType.Characters
                            ? DisplayedCharacterType.Character
                            : DisplayedCharacterType.Whitespace;

                        if (lastLineIndex != run.lineIndex) {
                            lastLineIndex = run.lineIndex;
                            currentLine.contentLineEnd = runOffset;
                            currentLine.endRunIndex = i;
                            lines.Add(currentLine);
                            y += currentLine.height;
                            currentLine = new LineMapping(y, StructList<GlyphMapping>.Get(), runOffset, i);
                        }

                        ShapeCacheValue shape = shapeCache.GetEntryAt(run.tagId);

                        MapGlyphsToCharacter(currentLine.mappings, run, runOffset, shape.Infos, shape.Positions, charType);
                        
                        if (run.height > currentLine.height) {
                            currentLine.height = run.height;
                        }

                        break;
                    }

                    case CharacterRunType.NewLine: {
                        currentLine.mappings.Add(new GlyphMapping() {
                            contentBufferStart = runOffset,
                            contentBufferEnd = runOffset + run.length,
                            height = run.height,
                            y = run.fontSize - run.y,
                            charType = DisplayedCharacterType.NewLine,
                            startX = 0,
                            endX = 0,
                            runGlyphStart = 0,
                            runGlyphEnd = run.length
                        });

                        currentLine.contentLineEnd = runOffset + run.length;
                        currentLine.endRunIndex = i + 1;

                        if (currentLine.height == 0) {
                            currentLine.height = run.height;
                        }

                        lastLineIndex++;
                        lines.Add(currentLine);
                        y += currentLine.height;
                        currentLine = new LineMapping(y, StructList<GlyphMapping>.Get(), currentLine.contentLineEnd, i + 1);
                        break;
                    }

                    case CharacterRunType.CursorSelection:
                    case CharacterRunType.ReserveSpace:
                    case CharacterRunType.Sprite:
                    case CharacterRunType.HorizontalSpace:
                    case CharacterRunType.VerticalSpace:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                runOffset += run.length;

            }

            currentLine.contentLineEnd = runOffset;
            currentLine.endRunIndex = entry.runList.size;
            currentLine.height = entry.runList.array[entry.runList.size - 1].height;
            lines.Add(currentLine);

            return lines;
        }

        public static void SetSelectionToPosition(LightList<LineMapping> glyphLineMappings, in TextDataEntry entry, in TextShapeCache shapeCache, float2 position, ref SelectionInfo selectionInfo) {
            SelectionData selectionData = GetCursorPosition(glyphLineMappings, position, entry, shapeCache);
            selectionInfo.selection = selectionData.index;
        }

        public static void SetCursorToPosition(LightList<LineMapping> glyphLineMappings, in TextDataEntry entry, in TextShapeCache shapeCache, float2 position, ref SelectionInfo selectionInfo) {
            
            SelectionData selectionData = GetCursorPosition(glyphLineMappings, position, entry, shapeCache);
            selectionInfo.cursor = selectionData.index;
            selectionInfo.showCursor = selectionData.index > -1;
            selectionInfo.cursorPosition = selectionData.position;
            selectionInfo.cursorHeight = selectionData.height;

        }

        public static void PreviousLine(LightList<LineMapping> glyphLineMappings, ref SelectionInfo cursorInfo, in TextDataEntry entry, in TextShapeCache shapeCache) {
            if (glyphLineMappings.size == 0) {
                cursorInfo.cursor = -1;
                cursorInfo.cursorPosition = default;
                cursorInfo.selection = -1;
                return;
            }

            if (glyphLineMappings.size == 1) {
                cursorInfo.cursor = 0;
                cursorInfo.cursorPosition = new float2(0, cursorInfo.cursorPosition.y);
                return;
            }

            bool isLastCharacterOnLastLine = cursorInfo.cursor == entry.dataLength;
            for (int index = 0; index < glyphLineMappings.size; index++) {
                LineMapping line = glyphLineMappings[index];
                // check if the cursor is within this line
                bool isLastLine = index + 1 == glyphLineMappings.size;
                if ((isLastCharacterOnLastLine && isLastLine) || line.contentLineStart <= cursorInfo.cursor && line.contentLineEnd > cursorInfo.cursor) {
                    // if the cursor is in the first line there is no line to go up to
                    if (index == 0) {
                        cursorInfo.cursor = 0;
                        cursorInfo.cursorPosition = new float2(0, cursorInfo.cursorPosition.y);
                        return;
                    }

                    LineMapping targetLine = glyphLineMappings[index - 1];
                    SelectionData selectionData = GetCursorPosition(glyphLineMappings, new float2(cursorInfo.cursorPosition.x, targetLine.y), entry, shapeCache);
                    cursorInfo.cursor = selectionData.index;
                    cursorInfo.cursorPosition = selectionData.position;
                    cursorInfo.cursorHeight = selectionData.height;
                    return;
                }
            }
        }

        public static void NextLine(LightList<LineMapping> glyphLineMappings, ref SelectionInfo cursorInfo, in TextDataEntry entry, in TextShapeCache shapeCache) {
            for (int index = 0; index < glyphLineMappings.size; index++) {
                LineMapping line = glyphLineMappings[index];
                // check if the cursor is within this line
                if (line.contentLineStart <= cursorInfo.cursor && line.contentLineEnd > cursorInfo.cursor) {
                    // if the cursor is in the last line there is no line to go down to
                    if (index == glyphLineMappings.size - 1) {
                        // right edge of the last character
                        cursorInfo.cursor = line.contentLineEnd;
                        return;
                    }

                    LineMapping targetLine = glyphLineMappings[index + 1];
                    SelectionData selectionData = GetCursorPosition(glyphLineMappings, new float2(cursorInfo.cursorPosition.x, targetLine.y), entry, shapeCache);
                    cursorInfo.cursor = selectionData.index;
                    cursorInfo.cursorPosition = selectionData.position;
                    cursorInfo.cursorHeight = selectionData.height;
                    return;
                }
            }
        }

        public static void EndOfLine(LightList<LineMapping> glyphLineMappings, ref SelectionInfo cursorInfo) {
            for (int index = 0; index < glyphLineMappings.size; index++) {
                LineMapping line = glyphLineMappings[index];
                // check if the cursor is within this line
                if (line.contentLineStart <= cursorInfo.cursor && line.contentLineEnd > cursorInfo.cursor) {
                    // if the cursor is in the last line there is no line to go down to
                    cursorInfo.cursor = index == glyphLineMappings.size - 1
                        ? line.contentLineEnd
                        : line.contentLineEnd - 1;
                    return;
                }
            }
        }

        public static void PreviousWord(LightList<LineMapping> glyphLineMappings, in TextDataEntry entry, ref SelectionInfo cursorInfo) {
            int lastRunIndex = entry.runList.size - 1;
            if (entry.dataLength == cursorInfo.cursor) {
                ref TextCharacterRun run = ref entry.runList.array[lastRunIndex];
                cursorInfo.cursor = BacktrackToPreviousWordCharacterIndex(entry.runList, ref cursorInfo, ref run, cursorInfo.cursor - run.length, lastRunIndex);
                return;
            }

            for (int index = 0; index < glyphLineMappings.size; index++) {
                LineMapping line = glyphLineMappings[index];
                // check if the cursor is within this line
                if (line.contentLineStart <= cursorInfo.cursor && line.contentLineEnd > cursorInfo.cursor) {

                    int characterOffset = AccumulateDataOffset(entry, line.startRunIndex);
                    for (int runIndex = line.startRunIndex; runIndex < line.endRunIndex; runIndex++) {
                        ref TextCharacterRun run = ref entry.runList.array[runIndex];

                        if (characterOffset <= cursorInfo.cursor && (run.length + characterOffset) >= cursorInfo.cursor) {
                            // move the cursor to the end of the previous word 
                            cursorInfo.cursor = BacktrackToPreviousWordCharacterIndex(entry.runList, ref cursorInfo, ref run, characterOffset, runIndex);
                            return;
                        }
                        // 

                        // count the characters of all runs so far since runs don't store their character ranges
                        characterOffset += run.length;
                    }
                    return;
                }
            }
        }

        public static void NextWord(LightList<LineMapping> glyphLineMappings, in TextDataEntry entry, ref SelectionInfo cursorInfo) {
            if (entry.dataLength == cursorInfo.cursor) {
                return;
            }

            for (int index = 0; index < glyphLineMappings.size; index++) {
                LineMapping line = glyphLineMappings[index];
                // check if the cursor is within this line
                if (line.contentLineStart <= cursorInfo.cursor && line.contentLineEnd > cursorInfo.cursor) {

                    int characterOffset = AccumulateDataOffset(entry, line.startRunIndex);
                    for (int runIndex = line.startRunIndex; runIndex < line.endRunIndex; runIndex++) {
                        ref TextCharacterRun run = ref entry.runList.array[runIndex];
                        // 
                        if (characterOffset <= cursorInfo.cursor && (run.length + characterOffset) >= cursorInfo.cursor) {
                            // move the cursor to the end of the previous word 
                            int offset = AccumulateDataOffset(entry, runIndex + 1);
                            if (cursorInfo.cursor == run.length + characterOffset && entry.runList.size > runIndex) {
                                offset += entry.runList.array[runIndex + 1].length;
                            }
                            cursorInfo.cursor = offset;
                            return;
                        }

                        // count the characters of all runs so far since runs don't store their character ranges
                        characterOffset += run.length;
                    }
                    return;
                }
            }
        }

        ///
        /// only backtracks the characterOffset if the run at runIndex is not a character run or if we are at the end of
        /// the data buffer.
        /// 
        private static int BacktrackToPreviousWordCharacterIndex(CheckedArray<TextCharacterRun> runList,
                                                                 ref SelectionInfo cursorInfo, 
                                                                 ref TextCharacterRun run, 
                                                                 int characterOffset,
                                                                 int runIndex) {

            if (run.IsWhiteSpace || cursorInfo.cursor == characterOffset) {
                int previousRunIndex = BacktrackToLastNonWhitespaceRun(runList, runIndex);
                // backtrack the amount of characters we're in so far in the current run
                --runIndex;
                cursorInfo.cursor -= cursorInfo.cursor - characterOffset;
                // if the current runIndex is not the run of the previous word we backtrack even more 
                for (; previousRunIndex <= runIndex; runIndex--) {
                    characterOffset -= runList.array[runIndex].length;
                }
            }

            return characterOffset;
        }

        public static void RecalcCursorPosition(ref TextDataEntry entry, TextShapeCache shapeCache, ref SelectionInfo cursor) {
            int runStart = 0;

            for (int runIndex = 0; runIndex < entry.runList.size; runIndex++) {
                ref TextCharacterRun run = ref entry.runList.array[runIndex];

                if (cursor.cursor >= runStart && cursor.cursor < runStart + run.length) {

                    if (TryProcessNewLineRun(ref entry, runIndex, ref cursor)) {
                        return;
                    }

                    int shapeResultIndex = run.shapeResultIndex;
                    ShapeCacheValue shapeCacheValue = shapeCache.GetEntryAt(shapeResultIndex);
                    CheckedArray<GlyphInfo> glyphInfos = shapeCacheValue.Infos;
                    CheckedArray<GlyphPosition> glyphPositions = shapeCacheValue.Positions;

                    StructList<GlyphMapping> glyphMappings = StructList<GlyphMapping>.Get();

                    MapGlyphsToCharacter(glyphMappings, run, runStart, glyphInfos, glyphPositions, default);

                    for (int i = 0; i < glyphMappings.size; i++) {
                        GlyphMapping mapping = glyphMappings[i];
                        if (mapping.contentBufferStart >= cursor.cursor && cursor.cursor < mapping.contentBufferEnd) {
                            // cursor should be in the middle if we don't know better
                            float y = run.fontSize - run.y;
                            if (mapping.runGlyphStart != mapping.runGlyphEnd - 1) {
                                cursor.cursorPosition = new float2(
                                    mapping.endX - (mapping.endX - mapping.startX)
                                    / (mapping.runGlyphEnd - mapping.runGlyphStart), y);
                            }
                            else {
                                cursor.cursorPosition = new float2(mapping.startX, y);
                            }

                            glyphMappings.Release();
                            return;
                        }
                    }

                    glyphMappings.Release();
                    // do actual calc

                    return;
                }

                runStart += run.length;
            }

            // everything got deleted, we're at the start of the input field again
            if (entry.runList.size == 0) {
                cursor.cursorPosition = new float2(0, 0);
                // todo find out the default font size; put it into the layout result
                // cursor.cursorHeight = CheaterSystem.GetFontSize();
            }
            // we must be at the end of the last run
            else if (!TryProcessNewLineRun(ref entry, entry.runList.size - 1, ref cursor)) {
                ref TextCharacterRun run = ref entry.runList.array[entry.runList.size - 1];
                cursor.cursorPosition = new float2(run.x + run.width, run.fontSize - run.y);
            }
        }

        private static bool TryProcessNewLineRun(ref TextDataEntry entry, int runIndex, ref SelectionInfo cursor) {
            ref var run = ref entry.runList.array[runIndex];
            if (!run.IsNewLine) {
                return false;
            }

            cursor.cursorHeight = run.height;

            if (cursor.cursor >= entry.dataLength) {
                // +1 to include this new line in the calculation, since we display the caret in the next line
                var y = BacktrackForY(ref entry, runIndex + 1);
                cursor.cursorPosition = new float2(0, -y);
                return true;
            }

            // the end of the line case
            if (runIndex - 1 >= 0 && !entry.runList.array[runIndex - 1].IsNewLine) {
                ref TextCharacterRun lastRun = ref entry.runList.array[runIndex - 1];
                cursor.cursorPosition = new float2(lastRun.x + lastRun.width, lastRun.fontSize - lastRun.y);
                cursor.cursorHeight = lastRun.height;
            }
            // multiple new line case
            else if (runIndex - 1 >= 0 && entry.runList.array[runIndex - 1].IsNewLine) {
                var y = BacktrackForY(ref entry, runIndex);
                cursor.cursorPosition = new float2(0, -y);
            }
            else {
                // first character case
                cursor.cursorPosition = new float2(0, 0);
            }

            return true;
        }

        private static float BacktrackForY(ref TextDataEntry entry, int runIndex) {
            int baseIndex = BacktrackToLastNonNewLineRun(entry.runList, runIndex);
            TextCharacterRun baseRun = entry.runList.array[baseIndex];
            float y = baseRun.IsNewLine ? baseRun.height : baseRun.y - baseRun.fontSize + baseRun.height;
            // all of these are new lines
            for (int i = baseIndex + 1; i < runIndex - 1; i++) {
                y += entry.runList.array[i].height;
            }

            return y;
        }

        private static int BacktrackToLastNonNewLineRun(CheckedArray<TextCharacterRun> runList, int idx) {
            //find last run that was a not a new line or is the first run
            int runBackIndex = idx - 1;
            for (; runBackIndex >= 1; runBackIndex--) {
                ref TextCharacterRun lastRun = ref runList.array[runBackIndex];
                if (!lastRun.IsNewLine) {
                    return runBackIndex;
                }
            }

            return 0;
        }
        
        private static int BacktrackToLastNonWhitespaceRun(CheckedArray<TextCharacterRun> runList, int idx) {
            //find last run that was a not a new line or is the first run
            int runBackIndex = idx - 1;
            for (; runBackIndex >= 1; runBackIndex--) {
                ref TextCharacterRun lastRun = ref runList.array[runBackIndex];
                if (!lastRun.IsWhiteSpace) {
                    return runBackIndex;
                }
            }

            return 0;
        }

        public static SelectionData GetCursorPosition(LightList<LineMapping> glyphLineMappings, float2 localMousePos, in TextDataEntry entry, in TextShapeCache shapeCache) {

            SelectionData selectionData = default;
            
            if (glyphLineMappings.size == 0) {
                selectionData.index = 0;
                selectionData.position = float2.zero;
                // todo this needs to be default font size instead
                selectionData.height = 18f;
                return selectionData;
            }

            int lineIndex = HitTestLineBounds(localMousePos.y, glyphLineMappings);

            LineMapping targetLine = glyphLineMappings[lineIndex];

            int runIndex = HitTestRunBounds(entry, localMousePos.x, targetLine);

            int originalDataOffset = AccumulateDataOffset(entry, runIndex);

            // note: must be a copy! do not use ref because we re-assign the run variable in some cases
            TextCharacterRun run = entry.runList.array[runIndex];

            float localX = run.x;

            switch (run.GetRunType()) {
                case CharacterRunType.NewLine: {
                    int runsOnLine = targetLine.endRunIndex - targetLine.startRunIndex;
                    if (runsOnLine > 1) {
                        TextCharacterRun lastRun = entry.runList[targetLine.endRunIndex - 2];
                        selectionData.height = lastRun.height;
                        selectionData.position = new float2(lastRun.x + lastRun.width, lastRun.fontSize - lastRun.y);
                        selectionData.index = originalDataOffset;
                    }
                    else {
                        selectionData.height = targetLine.height;
                        selectionData.position = new float2(0, -targetLine.y);
                        if (lineIndex + 1 == glyphLineMappings.size && targetLine.contentLineEnd == targetLine.contentLineStart) {
                            selectionData.index = originalDataOffset + 1;
                        }
                        else {
                            selectionData.index = originalDataOffset;
                        }

                    }

                    break;
                }

                case CharacterRunType.Whitespace:
                case CharacterRunType.Characters:
                    int shapeResultIndex = run.shapeResultIndex;
                    ShapeCacheValue shapeCacheValue = shapeCache.GetEntryAt(shapeResultIndex);
                    CheckedArray<GlyphInfo> glyphInfos = shapeCacheValue.Infos;
                    CheckedArray<GlyphPosition> glyphPositions = shapeCacheValue.Positions;
                    int nearestIndex = 0;
                    float nearestDistance = float.MaxValue;

                    for (int glyphIndex = 0; glyphIndex < glyphInfos.size; glyphIndex++) {
                        float distance = math.abs(localX - localMousePos.x);
                        if (distance < nearestDistance) {
                            nearestIndex = glyphIndex;
                            nearestDistance = distance;
                            selectionData.height = run.height;
                        }

                        localX += glyphPositions[glyphIndex].advanceX;
                    }

                    // if we clicked on the last glyph in a run...
                    // ...we check if there is a run on the same line next to us
                    if (nearestIndex == glyphInfos.size - 1) {
                        if (entry.runList.size > runIndex + 1) {
                            TextCharacterRun nextRun = entry.runList[runIndex + 1];
                            if (nextRun.lineIndex == run.lineIndex || nextRun.IsNewLine) {
                                // localX is the right edge of the current run's last character
                                float distance = math.abs(localX - localMousePos.x);
                                if (distance < nearestDistance) {
                                    originalDataOffset += run.length;
                                    nearestIndex = 0;
                                    if (nextRun.IsNewLine) {
                                        selectionData.position = new float2(localX, run.fontSize - run.y);
                                        selectionData.index = originalDataOffset;

                                        return selectionData;
                                    }

                                    run = nextRun;
                                    selectionData.height = run.height;
                                    // setup all the things again
                                    shapeResultIndex = run.shapeResultIndex;
                                    shapeCacheValue = shapeCache.GetEntryAt(shapeResultIndex);
                                    glyphInfos = shapeCacheValue.Infos;
                                    glyphPositions = shapeCacheValue.Positions;
                                }
                            }
                        }
                        else {
                            // is this the last run eva?
                            float distance = math.abs(localX - localMousePos.x);
                            if (distance < nearestDistance) {
                                selectionData.height = run.height;
                                selectionData.position = new float2(run.x + run.width, run.fontSize - run.y);
                                selectionData.index = entry.dataLength;

                                return selectionData;
                            }
                        }
                    }

                    // if clusterId repeats -> all adjacent glyphs w/ same clusterId belong to the same indivisible unit
                    // if clusterId skips -> its a ligature and id of ligature start = min of ligature members
                    // need to track input length to accurately compute mappings for clusters -> original buffer input

                    StructList<GlyphMapping> glyphMappings = StructList<GlyphMapping>.Get();

                    MapGlyphsToCharacter(glyphMappings, run, originalDataOffset, glyphInfos, glyphPositions, default);

                    for (int i = 0; i < glyphMappings.size; i++) {
                        GlyphMapping mapping = glyphMappings[i];
                        if (mapping.runGlyphStart >= nearestIndex && nearestIndex < mapping.runGlyphEnd) {
                            // cursor should be in the middle if we don't know better
                            float y = run.fontSize - run.y;
                            if (mapping.runGlyphStart != mapping.runGlyphEnd - 1) {
                                selectionData.position =
                                    new float2(
                                        mapping.endX - (mapping.endX - mapping.startX) /
                                        (mapping.runGlyphEnd - mapping.runGlyphStart), y);
                                selectionData.index = mapping.contentBufferEnd;
                            }
                            else {
                                selectionData.index = mapping.contentBufferStart;
                                selectionData.position = new float2(mapping.startX, y);
                            }

                            glyphMappings.Release();
                            return selectionData;
                        }
                    }

                    glyphMappings.Release();
                    break;

                case CharacterRunType.Sprite:
                case CharacterRunType.HorizontalSpace:
                case CharacterRunType.VerticalSpace:
                case CharacterRunType.ReserveSpace:
                    break;
            }

            return selectionData;
        }

        internal static void MapGlyphsToCharacter(StructList<GlyphMapping> glyphMappings,
            in TextCharacterRun run,
            int originalDataOffset,
            CheckedArray<GlyphInfo> glyphInfos,
            CheckedArray<GlyphPosition> glyphPositions, DisplayedCharacterType displayedCharacterType) {

            uint lastClusterId = 0;
            GlyphMapping currentGlyphMapping = new GlyphMapping();
            float currentX = run.x;
            currentGlyphMapping.startX = currentX;
            currentGlyphMapping.contentBufferStart = originalDataOffset;
            currentGlyphMapping.runGlyphStart = 0;

            float yPos = run.fontSize - run.y;
            float height = run.height;

            for (int infoIndex = 0; infoIndex < glyphInfos.size; infoIndex++) {
                if (glyphInfos.array == null) {
                    // todo let's leave this error here until we fixed the problem.
                    Debug.LogError("GlyphInfos was null, the shape cache must have evicted this one...");
                    return;
                }
                ref GlyphInfo glyph = ref glyphInfos.array[infoIndex];
                if (glyph.cluster != lastClusterId) {
                    currentGlyphMapping.contentBufferEnd = originalDataOffset + (int) glyph.cluster;
                    currentGlyphMapping.runGlyphEnd = infoIndex;
                    currentGlyphMapping.endX = currentX;
                    currentGlyphMapping.y = yPos;
                    currentGlyphMapping.height = height;
                    currentGlyphMapping.charType = displayedCharacterType;

                    glyphMappings.Add(currentGlyphMapping);

                    currentGlyphMapping = new GlyphMapping();
                    currentGlyphMapping.startX = currentX;
                    currentGlyphMapping.contentBufferStart = originalDataOffset + (int) glyph.cluster;
                    currentGlyphMapping.runGlyphStart = infoIndex;
                }

                currentX += glyphPositions[infoIndex].advanceX;

                /*  example cluster output
                         *
                         *          A accent B accent C accent fi  w e i  r  d
                         *          0 0      2 2      4 4      6   8 9 10 11 12 
                         *
                         *
                         *  A = multiple glyphs in one cluster
                         *  fi = one glyph 
                         */
            }

            currentGlyphMapping.endX = currentX;
            currentGlyphMapping.contentBufferEnd = originalDataOffset + run.length;
            currentGlyphMapping.runGlyphEnd = glyphInfos.size;
            currentGlyphMapping.y = yPos;
            currentGlyphMapping.height = height;
            currentGlyphMapping.charType = displayedCharacterType;
            glyphMappings.Add(currentGlyphMapping);
        }

        private static int AccumulateDataOffset(in TextDataEntry entry, int runEndIndex, int startIndex = 0) {
            int retn = 0;
            for (int i = startIndex; i < runEndIndex; i++) {
                retn += entry.runList.array[i].length;
            }

            return retn;
        }

        private static int HitTestRunBounds(in TextDataEntry entry, float xPoint, LineMapping targetLine) {

            for (int i = targetLine.startRunIndex; i < targetLine.endRunIndex; i++) {
                ref TextCharacterRun run = ref entry.runList.array[i];

                if (xPoint >= run.x && xPoint <= run.x + run.width) {
                    return i;
                }

            }

            return targetLine.endRunIndex - 1;

        }

        private static int HitTestLineBounds(float yPoint, LightList<LineMapping> boundsList) {

            for (int i = 0; i < boundsList.size; i++) {
                ref LineMapping bounds = ref boundsList.array[i];
                if (yPoint >= bounds.y && yPoint < bounds.y + bounds.height) {
                    return i;
                }
            }

            return boundsList.size - 1;
        }
    }

    internal struct LineMapping {

        public StructList<GlyphMapping> mappings;
        public float y;
        public float height;
        public int contentLineStart;
        public int contentLineEnd;
        // run start index
        public int startRunIndex;
        // run end index, exclusive!
        public int endRunIndex;

        public LineMapping(float y, StructList<GlyphMapping> mappings, int contentLineStart, int startRunIndex) {
            this.y = y;
            this.mappings = mappings;
            this.contentLineStart = contentLineStart;
            this.height = 0;
            this.contentLineEnd = 0;
            this.startRunIndex = startRunIndex;
            endRunIndex = 0;
        }

        public override string ToString() {
            var str = "Line -> ";
            for (int i = 0; i < mappings.size; i++) {
                str += mappings.array[i].ToString();
            }                
            return $"{nameof(mappings)}: {str}, {nameof(y)}: {y}, {nameof(height)}: {height}, {nameof(contentLineStart)}: {contentLineStart}, {nameof(contentLineEnd)}: {contentLineEnd}";
        }

    }

    internal struct LineBounds {

        public float y;
        public float height;
        public int startRunIndex;
        public int endRunIndex;

        // index into this.text
        public int contentBufferStart;
        public int contentBufferEnd;

    }

    internal struct GlyphMapping {

        // relative to the element position
        public float startX;
        public float endX;

        // index into this.text
        public int contentBufferStart;
        public int contentBufferEnd;

        // relative to each run
        public int runGlyphStart;
        public int runGlyphEnd;

        public float y;
        public float height;

        public DisplayedCharacterType charType;

        public override string ToString() {
            return $"\n\tstartX: {startX}, endX: {endX}\n\t cBufferStart: {contentBufferStart}\n\t cBufferEnd: {contentBufferEnd}\n\t runGlyphStart: {runGlyphStart}\n\t runGlyphEnd: {runGlyphEnd}\n\t y: {y}\n\t height: {height}\n\t charType: {charType}\n";
        }

    }

}