using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Util.Unsafe {

    public unsafe struct SplitBuffer<TKey, TData> where TKey : unmanaged where TData : unmanaged {

        public SplitBuffer(RawSplitBuffer computed) { }

        // public static SplitBuffer<TKey, TData> Create(int preambleSize, int capacity) {
        //     void* rawData = UnsafeUtility.Malloc(preambleSize + (capacity * sizeof(TKey)) + (capacity * sizeof(TData)), 4, Allocator.Persistent);
        //     TKey* keyStart = (TKey*) ((long*) rawData + preambleSize);
        //     long* dataStart = (long*) (keyStart + (capacity * sizeof(TKey)));
        //
        //     RawSplitBuffer* rawSplitBuffer = (RawSplitBuffer*) (rawData + preambleSize);
        //     InstanceStyleData* data = (InstanceStyleData*) rawData;
        //     data->keys = keyStart;
        //     data->data = dataStart;
        //     data->capacity = capacity;
        //
        //     data->totalStyleCount = 0;
        //     data->usedStyleCount = 0;
        //     return new SplitBuffer<TKey, TData>();    
        // }

        public RawSplitBuffer GetRawBuffer() {
            return default;
        }

    }

    public unsafe struct RawSplitBuffer {

        public void* keys;
        public void* data;
        public int size;
        public int capacity;

    }

    public unsafe struct UntypedPagedListPage {

        internal void* data;
        public int size;

    }

    // Note: Does not support removal, intended to be add only
    [DebuggerTypeProxy(typeof(UnsafePagedListDebugView<>))]
    public unsafe struct UnmanagedPagedList<T> : IDisposable where T : unmanaged {

        [NativeDisableUnsafePtrRestriction] internal PagedListState* state;

        public int pageCount {
            get => state->pageCount;
        }

        public int pageSize {
            get => state->pageSize;
        }

        internal UnmanagedPagedList(PagedListState* state) {
            this.state = state;
        }

        public UnmanagedPagedList(uint pageSize, Allocator allocator) {
            state = PagedListState.Create(pageSize, allocator);
        }

        // finds the first page that can contain itemCount. checks all pages (currently dereferences pointers to do so
        public RangeInt AddRangeCompact(T* items, int itemCount) {
            int localPageSize = state->pageSize;

            for (int i = 0; i < state->pageCount; i++) {

                UntypedPagedListPage* page = state->pages + i;

                if (page->size + itemCount <= localPageSize) {
                    T* ptr = (T*) page->data;
                    ptr += page->size;
                    UnsafeUtility.MemCpy(ptr, items, sizeof(T) * itemCount);
                    RangeInt retn = new RangeInt(page->size, itemCount);
                    page->size += itemCount;
                    return retn;
                }

            }

            if (state->pageCount + 1 >= state->pageCapacity) {
                CreateNewPage();
            }

            UntypedPagedListPage* newPage = state->pages + state->pageCount;
            int rangeStart = state->pageCount * state->pageSize;
            state->pageCount++;
            newPage->data = (T*) UnsafeUtility.Malloc(pageSize * sizeof(T), UnsafeUtility.AlignOf<PagedListState>(), state->allocator);
            newPage->size = itemCount;
            UnsafeUtility.MemCpy(newPage->data, items, sizeof(T) * itemCount);
            return new RangeInt(rangeStart, itemCount);
        }

        public RangeInt AddRange(T* items, int itemCount) {

            if (state->pageCount == 0) {
                CreateNewPage();
            }

            if (state->pages[state->pageCount - 1].size + itemCount >= state->pageCapacity) {
                CreateNewPage();
            }

            UntypedPagedListPage* page = state->pages + (state->pageCount - 1);

            T* ptr = (T*) page->data;
            ptr += page->size;
            UnsafeUtility.MemCpy(ptr, items, sizeof(T) * itemCount);
            RangeInt retn = new RangeInt(page->size, itemCount);
            page->size += itemCount;
            return retn;

        }

        private void CreateNewPage() {
            state->pageCapacity *= 2;
            UntypedPagedListPage* ptr = (UntypedPagedListPage*) UnsafeUtility.Malloc(state->pageCapacity * sizeof(UntypedPagedListPage), UnsafeUtility.AlignOf<PagedListState>(), state->allocator);
            if (state->pages != null) {
                UnsafeUtility.MemCpy(ptr, state->pages, sizeof(UntypedPagedListPage) * state->pageCount);
                UnsafeUtility.Free(state->pages, state->allocator);
            }

            state->pages = ptr;

        }

        public T this[int index] {
            get {
                // sizes are always power of 2 so we can compute index with shift instead of divide & mod
                int pageIndex = index >> state->pageSizeIndex;
                int pageArrayIndex = index - (pageIndex * state->pageSize);
                return UnsafeUtility.ReadArrayElement<T>(state->pages[pageIndex].data, pageArrayIndex);
            }
            set {
                // sizes are always power of 2 so we can compute index with shift instead of divide & mod
                int pageIndex = index >> state->pageSizeIndex;
                int pageArrayIndex = index - (pageIndex * state->pageSize);
                UnsafeUtility.WriteArrayElement(state->pages[pageIndex].data, pageArrayIndex, value);
            }
        }

        public void Dispose() {
            state->Dispose();
            UnsafeUtility.Free(state, state->allocator);
            state = null;
        }

        public PagedListPage<T> GetPage(int index) {
            if (index >= 0 && index < state->pageCount) {
                return new PagedListPage<T>(state->pages[index]);
            }

            return default;
        }

        public int Add(T item) {

            if (state->pageCount == 0 || state->pages[state->pageCount - 1].size >= pageSize) {
                CreateNewPage();
            }

            int lastPageIndex = state->pageCount - 1;

            UntypedPagedListPage* page = state->pages + lastPageIndex;

            int index = (lastPageIndex * state->pageSize) + page->size;

            T* array = (T*) page->data;
            array[page->size] = item;
            page->size++;

            return index;

        }

        public int AddCompactly(T item) {

            int localPageSize = state->pageSize;

            int pageIdx = -1;

            for (int i = 0; i < state->pageCount; i++) {

                if ((state->pages + i)->size + 1 <= localPageSize) {
                    pageIdx = i;
                    break;
                }

            }

            if (pageIdx == -1) {
                CreateNewPage();
                pageIdx = state->pageCount - 1;
            }

            UntypedPagedListPage* page = state->pages + pageIdx;

            int index = (pageIdx * state->pageSize) + localPageSize;

            T* array = (T*) page->data;
            array[page->size] = item;
            page->size++;

            return index;

        }

        public T* Reserve(int itemCount) {
            if (state->pageCount == 0) {
                CreateNewPage();
            }

            if (state->pages[state->pageCount - 1].size + itemCount >= state->pageCapacity) {
                CreateNewPage();
            }

            UntypedPagedListPage* page = state->pages + (state->pageCount - 1);

            T* ptr = (T*) page->data;
            ptr += page->size;
            page->size += itemCount;
            return ptr;
        }

        public ref T GetReference(int index) {
            int pageIndex = index >> state->pageSizeIndex;
            int pageArrayIndex = index - (pageIndex * state->pageSize);
            T* data = (T*) state->pages[pageIndex].data;
            return ref data[pageArrayIndex];
        }

        public T* GetPointer(int index) {
            int pageIndex = index >> state->pageSizeIndex;
            int pageArrayIndex = index - (pageIndex * state->pageSize);
            T* data = (T*) state->pages[pageIndex].data;
            return data + pageArrayIndex;
        }

        // this is so shitty, Unity might spawn up to 128 threads and in order to be promised some safety I need to make pointer space for each thread to have its own data
        // even though I'll likely never use more than about 12 of them outside of crazy threadripper.
        public struct PerThread : IDisposable {

            private readonly uint pageSize;
            private readonly Allocator allocator;
            private PagedListState** perThreadData;

            public PerThread(int pageSize, Allocator allocator) {
                this.allocator = allocator;
                this.pageSize = (uint)pageSize;
                PerThreadPagedListUtil.Create(out perThreadData, allocator);
            }

            public int CountElements() {
                return PerThreadPagedListUtil.CountElements(perThreadData);
            }

            public UnmanagedPagedList<T> GetListForThread(int threadIndex) {
                return new UnmanagedPagedList<T>(PerThreadPagedListUtil.GetListForThread(perThreadData, threadIndex, allocator, pageSize));
            }

            public void Dispose() {
                PerThreadPagedListUtil.Dispose(ref perThreadData, allocator);
            }

            public void ToUnmanagedList(UnmanagedList<T> outputList) {
                int count = CountElements();
                outputList.SetCapacity(count);
                outputList.size = 0;
                for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++) {
                    PagedListState* pagedListState = perThreadData[i];

                    if (pagedListState != null) {

                        for (int j = 0; j < pagedListState->pageCount; j++) {
                            UnsafeUtility.MemCpy(outputList.array + outputList.size, pagedListState->pages[j].data, pagedListState->pages[j].size);
                            outputList.size += pagedListState->pages[j].size;
                        }

                    }
                }
            }

        }

        public struct PerThreadBatchData {

            private BatchData* data;
            private int totalBatches;
            private Allocator allocator;
            private readonly uint pageSize;

            public PerThreadBatchData(int pageSize, int maxJobs, Allocator allocator) {
                this.allocator = allocator;
                this.totalBatches = maxJobs;
                this.pageSize = (uint) pageSize;
                int size = sizeof(BatchData) * maxJobs;
                this.data = (BatchData*) UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<BatchData>(), allocator);
                UnsafeUtility.MemClear(data, size);
            }

            public PerThreadBatchData(int pageSize, Allocator allocator) : this() {
                this.allocator = allocator;
                this.pageSize = (uint) pageSize;
            }

            public void SetTotalBatchCount(int maxJobs) {
                this.totalBatches = maxJobs;
                int size = sizeof(BatchData) * maxJobs;
                this.data = (BatchData*) UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<BatchData>(), allocator);
                UnsafeUtility.MemClear(data, size);
            }

            // 1 entry per batch
            // thread search for their id in the list
            // if not found, write into batch entry 
            // this way instead of the JobUtility.MaxJobThreadCount method we allocate only 1 slot per
            // job that we positive will be used. Some indices might be unused because a thread
            // had previously processed a different batch and is now handling another, but this is
            // much less waste then before at the expense of a small search

            public UnmanagedPagedList<T> GetPerBatchData(int batchIndex, int threadId) {
                for (int i = 0; i < totalBatches; i++) {

                    if (data[i].threadId == threadId) {
                        return new UnmanagedPagedList<T>((PagedListState*) data + i);
                    }

                }

                data[batchIndex].data = PagedListState.Create(pageSize, allocator);
                data[batchIndex].threadId = threadId;
                return new UnmanagedPagedList<T>((PagedListState*) data[batchIndex].data);
            }

        }

    }

    internal unsafe struct BatchData {

        public long threadId;
        public void* data;

    }

    public unsafe struct PagedListPage<T> where T : unmanaged {

        public readonly T* array;
        public readonly int size;

        public PagedListPage(UntypedPagedListPage page) {
            this.array = (T*) page.data;
            this.size = page.size;
        }

    }

}