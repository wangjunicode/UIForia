using System;
using UIForia.Layout;
using UIForia.Prototype;
using UIForia.Unsafe;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Text {

    internal struct TextId {

        public int index;

        public TextId(int index) {
            this.index = index;
        }

    }

    internal unsafe struct MainThreadTextEntry {

        public char* dataBuffer;
        public TextSymbol* symbolPtr;

        public int dataLength;
        public int symbolLength;
        public int dataCapacity;
        public ElementId elementId;
        public int cursorIndex;
        public int selectionIndex;

        // override symbol length to also mean next free index
        public int NextFreeIndex {
            get => dataBuffer == null ? dataLength : 0;
            set => dataLength = value; // note : dangerous re-use here 
        }

    }

    internal unsafe struct TextDataTable : IDisposable {

        public int maxRunCount;

        public LongBoolMap mainThreadDirtyElements;
        public DataList<ulong> dirtyElementStorage;
        public DataList<TextDataEntry> workerEntries;
        public DataList<MainThreadTextEntry> mainThreadEntries;
        public DataList<TextId> activeTextIndices;

        // todo -- combine these and use a spin lock around them, should be very low contention (generally 0) 
        public ListAllocatorSized mainThreadListData;
        public ListAllocatorSized workerThreadListData;

        // todo -- remove one of these 
        public UnsafeHashMap<int, TextId> boxIndexToTextId;
        public UnsafeHashMap<ElementId, TextId> elementIdToTextId;

        private TextLayoutStore store;
        private DataList<TextLineInfoNew> lineBuffer;

        private int freeListIndex;

        public DataList<SDFFontUnmanaged> fontTable; // todo -- needs to wrapped in a mutex somehow to disallow access while reading from layout job

        public static TextDataTable Create() {
            return new TextDataTable() {
                maxRunCount = 32,
                workerEntries = new DataList<TextDataEntry>(32, Allocator.Persistent) {size = 1},
                mainThreadEntries = new DataList<MainThreadTextEntry>(32, Allocator.Persistent) {size = 1},
                activeTextIndices = new DataList<TextId>(32, Allocator.Persistent),
                // todo -- single allocator w/ lock? probably very very little contention 
                mainThreadListData = ListAllocatorSized.CreateAllocator(32, TypedUnsafe.Kilobytes(16), TypedUnsafe.Kilobytes(128)),
                workerThreadListData = ListAllocatorSized.CreateAllocator(32, TypedUnsafe.Kilobytes(16), TypedUnsafe.Kilobytes(128)),
                dirtyElementStorage = new DataList<ulong>(8, Allocator.Persistent),
                mainThreadDirtyElements = new LongBoolMap(TypedUnsafe.MallocCleared<ulong>(64, Allocator.Persistent), 64),
                boxIndexToTextId = new UnsafeHashMap<int, TextId>(32, Allocator.Persistent),
                elementIdToTextId = new UnsafeHashMap<ElementId, TextId>(32, Allocator.Persistent),
                fontTable = new DataList<SDFFontUnmanaged>(16, Allocator.Persistent),
                store = TextLayoutStore.Create(),
                lineBuffer = new DataList<TextLineInfoNew>(64, Allocator.Persistent),
                freeListIndex = 0
            };
        }
        
        public void SetupLayout() {
            lineBuffer.size = 0;
        }

        public void Dispose() {
            lineBuffer.Dispose();
            store.Dispose();
            fontTable.Dispose();
            dirtyElementStorage.Dispose();
            workerEntries.Dispose();
            mainThreadEntries.Dispose();
            mainThreadListData.Dispose();
            workerThreadListData.Dispose();
            activeTextIndices.Dispose();
            elementIdToTextId.Dispose();
            boxIndexToTextId.Dispose();
            this = default;
        }

        // todo -- the text system should be able to share worker & main thread data for hit testing & selection because we'll run input from main thread and never while layout is running so no need to copy back  
        // same theory applies to rendering, safe to access worker thread data at any point we know for sure we aren't dealing with concurrency across styling, shaping, layout 

        public TextId AllocateTextId(ElementId elementId) {
            // use the same meta scheme with element ids to detect dead ids? perf hit & totally managed by system so maybe not

            TextId textId;

            if (freeListIndex != 0) {
                textId = new TextId(freeListIndex);
                freeListIndex = mainThreadEntries[freeListIndex].NextFreeIndex;
            }
            else {
                textId = new TextId(mainThreadEntries.size);
                mainThreadEntries.Add(default);

                // make sure our dirty element map is big enough to hold the new id, dont mark dirty yet 
                if (textId.index >= dirtyElementStorage.size * 64) {
                    dirtyElementStorage.Add(default);
                    mainThreadDirtyElements = new LongBoolMap(dirtyElementStorage.GetArrayPointer(), dirtyElementStorage.size);
                }

            }

            ref MainThreadTextEntry entry = ref mainThreadEntries[textId.index];

            entry = new MainThreadTextEntry() {
                elementId = elementId,
                dataLength = 0,
                symbolLength = 0,
                cursorIndex = -1,
                selectionIndex = -1
            };

            return textId;
        }

        public void FreeTextId(TextId textId) {

            if (freeListIndex != 0) {
                ref MainThreadTextEntry entry = ref mainThreadEntries[textId.index];
                entry.NextFreeIndex = freeListIndex;
            }

            freeListIndex = textId.index;

        }

        public int GetMainThreadContent(TextId textId, out char* cbuffer) {
            ref MainThreadTextEntry entry = ref mainThreadEntries[textId.index];
            cbuffer = entry.dataBuffer;
            return entry.dataLength;
        }

        public bool SetMainThreadTextData(TextId textId, char* cbuffer, int size) {

            ref MainThreadTextEntry entry = ref mainThreadEntries[textId.index];

            if (entry.dataLength == size && UnsafeUtility.MemCmp(entry.dataBuffer, cbuffer, size * sizeof(char)) == 0) {
                return false;
            }

            mainThreadDirtyElements.Set(textId.index);

            if (entry.dataCapacity < size) {

                if (entry.dataBuffer != null) {
                    mainThreadListData.Free(entry.dataBuffer, entry.dataCapacity);
                }

                AllocatedList<char> alloc = mainThreadListData.Allocate<char>(size);
                entry.dataBuffer = alloc.array;
                entry.dataCapacity = alloc.capacity;
            }

            entry.dataLength = size;
            TypedUnsafe.MemCpy(entry.dataBuffer, cbuffer, size);

            return true;

        }

        public void SetCursor(TextId textId, SelectionInfo selectionInfo) {

            ref MainThreadTextEntry entry = ref mainThreadEntries[textId.index];
            entry.cursorIndex = selectionInfo.cursor;
            entry.selectionIndex = selectionInfo.selection;
            mainThreadDirtyElements.Set(textId.index);

        }

        public ref TextDataEntry GetEntry(TextId textId) {
            return ref workerEntries.Get(textId.index);
        }

        public void AllocateTextData(ref TextDataEntry entry, DataList<TextCharacterRun> runBuffer) {
            AllocatedList<TextCharacterRun> alloc = workerThreadListData.Allocate<TextCharacterRun>(runBuffer.size);
            entry.runList = new CheckedArray<TextCharacterRun>(alloc.array, runBuffer.size);
            TypedUnsafe.MemCpy(entry.runList.array, runBuffer.GetArrayPointer(), runBuffer.size);
        }

        public TextDataEntry* GetEntryPointer(TextId textIndex) {
            return workerEntries.GetPointer(textIndex.index);
        }

        public void FreeTextRunData(TempList<TextId> textIds) {

            for (int i = 0; i < textIds.size; i++) {

                ref TextDataEntry entry = ref workerEntries.Get(textIds[i].index);

                if (entry.runList.array != null) {
                    int capacity = workerThreadListData.GetCapacityFromSize<TextCharacterRun>(entry.runList.size);
                    workerThreadListData.Free(entry.runList.array, capacity);
                    entry.runList = default;
                }

            }
        }

        public float ResolveContentWidth(TextId id) {

            ref TextDataEntry textEntry = ref workerEntries[id.index];

            if (textEntry.cachedContentWidth >= 0) {
                return textEntry.cachedContentWidth;
            }

            RangeInt lineRange = TextLayout.FlowCharacterRuns(
                float.MaxValue,
                ref store,
                ref lineBuffer,
                textEntry.GetDataBuffer(),
                textEntry.GetSymbols(),
                textEntry.runList,
                textEntry.layoutInfo
            );

            CheckedArray<TextLineInfoNew> lineSlice = lineBuffer.Slice(lineRange);

            float contentWidth = TextLayout.GetWidth(lineSlice);
            float contentHeight = TextLayout.GetHeight(fontTable, textEntry.runList, lineSlice);

            textEntry.cachedContentWidth = contentWidth;
            textEntry.cachedContentHeight = contentHeight;

            return contentWidth;
        }

        public float GetHeight(TextId id, float targetWidth) {
            ref TextDataEntry textData = ref workerEntries[id.index];

            if (textData.cachedContentWidth >= 0) {
                if (targetWidth >= textData.cachedContentWidth) {
                    return textData.cachedContentHeight;
                }
            }

            if (targetWidth == textData.sizeCache0Width) {
                return textData.sizeCache0Height;
            }
            
            if (targetWidth == textData.sizeCache1Width) {
                return textData.sizeCache1Height;
            }

            RangeInt lineRange = TextLayout.FlowCharacterRuns(
                targetWidth,
                ref store,
                ref lineBuffer,
                textData.GetDataBuffer(),
                textData.GetSymbols(),
                textData.runList,
                textData.layoutInfo
            );

            float height = TextLayout.GetHeight(fontTable, textData.runList, lineBuffer.Slice(lineRange));

            switch (textData.nextSizeCacheIdx) {
                case 0:
                    textData.sizeCache0Width = targetWidth;
                    textData.sizeCache0Height = height;
                    textData.nextSizeCacheIdx = 1;
                    break;
                default:
                    textData.sizeCache1Width = targetWidth;
                    textData.sizeCache1Height = height;
                    textData.nextSizeCacheIdx = 0; // only have 2 size cache entries atm, could expand to more if needed 
                    break;
            }

            return height;

        }

        public void FinalizeLayout(TextId textId, float outputWidth, float outputHeight, in OffsetRect padding) {

            ref TextDataEntry entry = ref workerEntries.Get(textId.index);

            // if our output is already setup for a text flow at the target width (ie it didnt change) then just keep our result from last frame 
            if (entry.currentFlowWidth != outputWidth) {
                
                // todo -- there are cases where we can flow text for measuring and then save the flow result instead of re-flowing here
                
                entry.currentFlowWidth = outputWidth;
                
                RangeInt lineRange = TextLayout.FlowCharacterRuns(
                    outputWidth,
                    ref store,
                    ref lineBuffer,
                    entry.GetDataBuffer(),
                    entry.GetSymbols(),
                    entry.runList,
                    entry.layoutInfo
                );

                entry.layoutOutput = TextLayout.LayoutLines(outputWidth, padding, fontTable, entry.layoutInfo, entry.runList, lineBuffer.Slice(lineRange));
            }

        }

    }

}