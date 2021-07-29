using UIForia.Prototype;
using UIForia.Style;
using UIForia.Text;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Layout {

    // [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct MeasureCharacterGroups : IJob {

        // probably can be multi-threaded if we figure out how to share the tagger without lots of contention

        public CheckedArray<int> elementIdToIndex;
        public ShapeContext shapeContext;
        public TextShapeCache shapeCache;
        public StringTagger stringTagger;

        public TextDataTable textTable;

        [NativeDisableUnsafePtrRestriction] public float** emTable;
        [NativeDisableUnsafePtrRestriction] public StyleTables* styleTables;

        public void Execute() {

            // find a better way to do this allocation, auto-grow bump allocator likely a good solution  

            // need to know up front if any layout styles changed, if no style change, no content change and no rich text changes -> re-use cache values 
            // no need to split or cache in that case, but for gc purposes i think we might need to tag, or at least mark tag as used 

            // find list of text elements where enabled this frame or layout text styles changed

            TempList<TextLayoutInfo> fontInfoList = ResolveFontStyles();

            TempList<TextId> invalidatedTextList = TypedUnsafe.MallocUnsizedTempList<TextId>(textTable.activeTextIndices.size, Allocator.Temp);
            TempList<TextId> validatedTextList = TypedUnsafe.MallocUnsizedTempList<TextId>(textTable.activeTextIndices.size, Allocator.Temp);

            int contentBufferSize = 0;

            for (int i = 0; i < textTable.activeTextIndices.size; i++) {
                TextId textIndex = textTable.activeTextIndices[i];

                TextDataEntry* entry = textTable.GetEntryPointer(textIndex);
                TextLayoutInfo* infoPtr = fontInfoList.GetPointer(i);

                // if text is dirty or if content wasn't dirty but styles changed from last frame, invalidate that element 
                if (entry->explicitlyDirty || !TypedUnsafe.MemCmp(&entry->layoutInfo, infoPtr)) {
                    entry->layoutInfo = *infoPtr;
                    entry->ClearCaches();
                    invalidatedTextList.array[invalidatedTextList.size++] = textIndex;
                    contentBufferSize += textTable.GetEntry(textIndex).dataLength;
                }
                else {
                    validatedTextList.array[validatedTextList.size++] = textIndex;
                }

            }

            // We need to make sure not to garbage collect runs that were previously valid and won't be tagged again this frame
            RefreshValidTags(validatedTextList);
            RefreshValidShapes(validatedTextList);

            validatedTextList.Dispose();
            fontInfoList.Dispose();

            if (invalidatedTextList.size > 0) {

                textTable.FreeTextRunData(invalidatedTextList);

                DataList<char> contentBuffer = new DataList<char>(contentBufferSize, Allocator.TempJob);
                DataList<TextCharacterRun> runBuffer = new DataList<TextCharacterRun>(textTable.maxRunCount, Allocator.Temp);
                DataList<int> contentLengthBuffer = new DataList<int>(textTable.maxRunCount, Allocator.Temp);

                int totalRunCount = SplitText(invalidatedTextList, runBuffer, ref contentLengthBuffer, ref contentBuffer, textTable.fontTable);

                // this is over-allocated but will never resize because of overallocation, probably fine but find a better allocator scheme 
                DataList<ShapingCharacterRun> toShapeList = new DataList<ShapingCharacterRun>(totalRunCount, Allocator.TempJob);

                int estimatedGlyphCount = TagTextRuns(contentBuffer, invalidatedTextList, contentLengthBuffer, ref toShapeList);

                ShapeTextRuns(contentBuffer, toShapeList, estimatedGlyphCount);

                AssignShapeResultSizes(invalidatedTextList);

                contentBuffer.Dispose();
                runBuffer.Dispose();
                contentLengthBuffer.Dispose();
                toShapeList.Dispose();

            }

            invalidatedTextList.Dispose();

        }

        private int TagTextRuns(DataList<char> contentBuffer, TempList<TextId> invalidatedTextList, DataList<int> contentLengthBuffer, ref DataList<ShapingCharacterRun> toShapeList) {

            // contentLengthBuffer tell us how long each string is, we can compute the buffer location by accumulating content lengths for each run.
            // This saves us some storage since we can store ints instead of RangeInts

            int contentStart = 0; // offset into the content buffer
            int totalRunIndexer = 0;

            char* cbuffer = contentBuffer.GetArrayPointer();

            int estimatedGlyphCount = 0;

            for (int invalidIdx = 0; invalidIdx < invalidatedTextList.size; invalidIdx++) {

                TextId textId = invalidatedTextList[invalidIdx];

                ref TextDataEntry textEntry = ref textTable.GetEntry(textId);

                CheckedArray<TextCharacterRun> runs = textEntry.runList;

                for (int i = 0; i < runs.size; i++) {

                    ref TextCharacterRun characterRun = ref runs.array[i];

                    int contentLength = contentLengthBuffer[totalRunIndexer++];

                    // todo -- only shape rendered runs, whitespace shouldn't be shaped but can check font data for spacing instead 

                    if ((characterRun.flags & (CharacterRunFlags.Whitespace | CharacterRunFlags.Characters)) != 0) {

                        characterRun.tagId = stringTagger.TagString(cbuffer + contentStart, contentLength);

                        if (!shapeCache.TouchEntry(characterRun.GetCacheKey(), out int shapeIndex, out float advance)) {
                            toShapeList.Add(characterRun.GetShapingRunInfo(shapeIndex, contentStart, contentLength));
                            estimatedGlyphCount += contentLength;
                        }

                        characterRun.shapeResultIndex = shapeIndex;
                        characterRun.width = advance;
                    }

                    contentStart += contentLength;

                }
            }

            return estimatedGlyphCount;
        }

        private void ShapeTextRuns(DataList<char> contentBuffer, DataList<ShapingCharacterRun> toShapeList, int estimatedGlyphCount) {
            TextShapeRequestInfo requestInfo = new TextShapeRequestInfo() {
                contentBuffer = contentBuffer.GetArrayPointer(),
                shapeRequests = toShapeList.GetArrayPointer(),
                shapeRequestCount = toShapeList.size
            };

           TextShapeBuffers shapeBuffers = new TextShapeBuffers(toShapeList.size, estimatedGlyphCount, Allocator.TempJob);

            shapeContext.Shape(requestInfo, shapeBuffers);

            uint glyphOffset = 0;
  
            for (int i = 0; i < toShapeList.size; i++) {

                ref ShapingCharacterRun shapedRun = ref toShapeList.Get(i);
                ShapeResult shapeResult = shapeBuffers.shapeResults[i];

                //Debug.Log("glyphCount: "+ shapeResult.glyphCount + " width: " + shapeResult.advanceWidth);
                
                shapeCache.SetEntryAt(
                    (int) shapedRun.shapeIndex,
                    shapeResult,
                    shapeBuffers.glyphInfos + glyphOffset,
                    shapeBuffers.glyphPositions + glyphOffset
                );

                glyphOffset += shapeResult.glyphCount;
              
            }

            // for (int i = 0; i < 10; i++) {
            //     Debug.Log("glyphId[" + i + "] = " + shapeBuffers.glyphInfos[i].glyphIndex);
            // }
            //
            shapeBuffers.Dispose();
            
        }

        private void AssignShapeResultSizes(TempList<TextId> invalidatedTextList) {

            for (int invalidIdx = 0; invalidIdx < invalidatedTextList.size; invalidIdx++) {

                TextId textId = invalidatedTextList[invalidIdx];

                ref TextDataEntry textEntry = ref textTable.GetEntry(textId);

                CheckedArray<TextCharacterRun> runs = textEntry.runList;

                for (int i = 0; i < runs.size; i++) {

                    ref TextCharacterRun characterRun = ref runs.array[i];

                    if ((characterRun.flags & (CharacterRunFlags.Whitespace | CharacterRunFlags.Characters)) == 0) {
                        continue;
                    }

                    characterRun.width = shapeCache.GetEntryAt(characterRun.shapeResultIndex).advanceWidth;

                }

            }
        }

        private int SplitText(TempList<TextId> invalidatedTextList, DataList<TextCharacterRun> runBuffer, ref DataList<int> contentRangeBuffer, ref DataList<char> contentBuffer, DataList<SDFFontUnmanaged> fontTable) {
            TextSplitter.State state = TextSplitter.State.Create(Allocator.Temp);

            int totalRunCount = 0;

            for (int i = 0; i < invalidatedTextList.size; i++) {

                runBuffer.size = 0;

                TextId textId = invalidatedTextList[i];

                ref TextDataEntry textEntry = ref textTable.GetEntry(textId);

                TextSplitter.Split(
                    ref contentBuffer,
                    textEntry.GetSymbols(),
                    textEntry.GetDataBuffer(),
                    textEntry.layoutInfo,
                    ref runBuffer,
                    ref contentRangeBuffer,
                    ref state,
                    fontTable
                );

                if (runBuffer.size > textTable.maxRunCount) {
                    textTable.maxRunCount = runBuffer.size;
                }

                totalRunCount += runBuffer.size;

                textTable.AllocateTextData(ref textEntry, runBuffer);

            }

            state.Dispose();

            return totalRunCount;
        }

        private void RefreshValidShapes(TempList<TextId> validatedTextList) {
            // throw new NotImplementedException();
        }

        private void RefreshValidTags(TempList<TextId> validatedTextList) {
            for (int i = 0; i < validatedTextList.size; i++) {

                TextId index = validatedTextList[i];

                CheckedArray<TextCharacterRun> runList = textTable.GetEntry(index).runList;

                for (int runIdx = 0; runIdx < runList.size; runIdx++) {
                    ref TextCharacterRun run = ref runList.Get(runIdx);
                    if (run.tagId >= 0) {
                        stringTagger.RefreshTag(run.tagId);
                    }
                }

            }
        }

        private TempList<TextLayoutInfo> ResolveFontStyles() {

            TempList<TextLayoutInfo> fontInfoList = TypedUnsafe.MallocSizedTempList<TextLayoutInfo>(textTable.activeTextIndices.size, Allocator.Temp);

            for (int textIndex = 0; textIndex < textTable.activeTextIndices.size; textIndex++) {

                TextId textId = textTable.activeTextIndices[textIndex];
                ElementId elementId = textTable.GetEntry(textId).elementId;

                int elementIdIndex = elementId.index;
                int layoutIndex = elementIdToIndex[elementIdIndex];

                FontAssetId fontAssetId = styleTables->TextFontAsset[elementIdIndex];
                UIFontSize fontSize = styleTables->TextFontSize[elementIdIndex];

                // FontStyle fontStyle = styleTables->TextFontStyle[elementId.index];

                fontInfoList[textIndex] = new TextLayoutInfo() {
                    fontAssetId = fontAssetId.id == 0 ? new FontAssetId(1) : fontAssetId,
                    fontSize = ResolveFontSize(fontSize, (*emTable)[layoutIndex]),
                    alignment = styleTables->TextAlignment[elementIdIndex],
                    overflow = styleTables->TextOverflow[elementIdIndex],
                    whitespaceMode = styleTables->TextWhitespaceMode[elementIdIndex],
                    verticalAlignment = styleTables->TextVerticalAlignment[elementIdIndex],
                    // maxHeight = styleTables->
                    // characterSpacing = // todo add style
                };

            }

            return fontInfoList;
        }

        private static float ResolveFontSize(UIFontSize fontSize, float emSize) {
            switch (fontSize.unit) {

                default:
                case UIFontSizeUnit.Default:
                case UIFontSizeUnit.Pixel:
                case UIFontSizeUnit.Point:
                    return fontSize.value;

                case UIFontSizeUnit.Em:
                    return fontSize.value * emSize;
            }
        }

    }

}