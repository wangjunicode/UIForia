using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Util.Unsafe {

    public unsafe struct UntypedPagedListPage {

        internal void* data;
        public int size;

    }

    // Note: Does not support removal, intended to be add only
    [DebuggerTypeProxy(typeof(UnmanagedPagedListDebugView<>))]
    public unsafe struct UnmanagedPagedList<T> : IDisposable where T : unmanaged {

        [NativeDisableUnsafePtrRestriction] internal PagedListState* state;

        public int pageCount {
            get => state->pageCount;
        }

        public int pageSize {
            get => state->pageSizeLimit;
        }

        public int totalItemCount {
            get => state->totalItemCount;
        }
        
        public UnmanagedPagedList(PagedListState* state) {
            this.state = state;
        }

        public UnmanagedPagedList(uint pageSize, Allocator allocator) {
            state = PagedListState.Create(pageSize, allocator);
        }

        // finds the first page that can contain itemCount. checks all pages (currently dereferences pointers to do so
        public RangeInt AddRangeCompact(T* items, int itemCount) {
            int localPageSize = state->pageSizeLimit;

            for (int i = 0; i < state->pageCount - 1; i++) {

                UntypedPagedListPage* page = state->pages + i;

                if (page->size + itemCount <= localPageSize) {
                    T* ptr = ((T*) page->data) + page->size;
                    TypedUnsafe.MemCpy(ptr, items, itemCount);
                    int start = (i * state->pageSizeLimit) + page->size;
                    page->size += itemCount;
                    return new RangeInt(start, itemCount);
                }

            }

            return AddRange(items, itemCount);
        }

        /// <summary>
        /// Adds itemCount items from items to the end of the list, ensuring all items sit on the same page
        /// </summary>
        /// <param name="items"></param>
        /// <param name="itemCount"></param>
        /// <returns>Index range of where items were placed. default if input was invalid</returns>
        public RangeInt AddRange(T* items, int itemCount) {

            if (items == null || itemCount > state->pageSizeLimit) {
                return default;
            }

            if (state->pages[state->pageCount - 1].size + itemCount > state->pageSizeLimit) {
                state->AddNewPage();
            }

            UntypedPagedListPage* page = state->pages + (state->pageCount - 1);

            T* ptr = (T*) page->data + page->size;

            TypedUnsafe.MemCpy(ptr, items, itemCount);
            int start = ((state->pageCount - 1) * state->pageSizeLimit) + page->size;
            page->size += itemCount;
            return new RangeInt(start, itemCount);

        }

        public T this[int index] {
            get {
                // sizes are always power of 2 so we can compute index with shift instead of divide & mod
                int pageIndex = index >> state->pageSizeIndex;
                int pageArrayIndex = index - (pageIndex * state->pageSizeLimit);
                return UnsafeUtility.ReadArrayElement<T>(state->pages[pageIndex].data, pageArrayIndex);
            }
            set {
                // sizes are always power of 2 so we can compute index with shift instead of divide & mod
                int pageIndex = index >> state->pageSizeIndex;
                int pageArrayIndex = index - (pageIndex * state->pageSizeLimit);
                UnsafeUtility.WriteArrayElement(state->pages[pageIndex].data, pageArrayIndex, value);
            }
        }

        public void Dispose() {
            Allocator allocator = state->allocator;
            state->Dispose();
            UnsafeUtility.Free(state, allocator);
            this = default;
        }

        public PagedListPage<T> GetPage(int index) {
            return index >= 0 && index < state->pageCount
                ? new PagedListPage<T>(state->pages[index])
                : default;
        }

        public int Add(T item) {
            return AddRange(&item, 1).start;
        }

        public int AddCompactly(T item) {
            return AddRangeCompact(&item, 1).start;
        }

        public T* Reserve(int itemCount) {

            if (itemCount > state->pageSizeLimit) {
                return default;
            }

            if (state->pages[state->pageCount - 1].size + itemCount > state->pageSizeLimit) {
                state->AddNewPage();
            }

            UntypedPagedListPage* page = state->pages + (state->pageCount - 1);

            T* ptr = (T*) page->data + page->size;
            page->size += itemCount;
            return ptr;
        }

        public T* ReserveCleared(int itemCount) {
            T* ptr = Reserve(itemCount);
            TypedUnsafe.MemClear(ptr, itemCount);
            return ptr;
        }

        public ref T GetReference(int index) {
            int pageIndex = index >> state->pageSizeIndex;
            int pageArrayIndex = index - (pageIndex * state->pageSizeLimit);
            T* data = (T*) state->pages[pageIndex].data;
            return ref data[pageArrayIndex];
        }

        public T* GetPointer(int index) {
            int pageIndex = index >> state->pageSizeIndex;
            int pageArrayIndex = index - (pageIndex * state->pageSizeLimit);
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
                this.pageSize = (uint) pageSize;
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

            public void ToUnmanagedList(BufferList<T> outputList) {
                int count = CountElements();
                outputList.SetCapacity(count);
                outputList.size = 0;
                for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++) {
                    PagedListState* pagedListState = perThreadData[i];

                    if (pagedListState != null) {

                        for (int j = 0; j < pagedListState->pageCount; j++) {
                            T* dst = outputList.array + outputList.size;
                            T* src = (T*) pagedListState->pages[j].data;
                            int itemCount = pagedListState->pages[j].size;
                            TypedUnsafe.MemCpy(dst, src, itemCount);
                            outputList.size += itemCount;
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

        public PagedListState* GetStatePointer() {
            return state;
        }

        public void Clear() {
            state->Clear();
        }

        public void FlattenToList(UnmanagedList<T> output) {
            state->FlattenToList<T>(output);
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