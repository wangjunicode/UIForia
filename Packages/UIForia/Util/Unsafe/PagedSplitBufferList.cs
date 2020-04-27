using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Util.Unsafe {

    public unsafe struct PagedSplitBufferList<TKey, TValue> : IDisposable where TKey : unmanaged where TValue : unmanaged {

        [NativeDisableUnsafePtrRestriction] internal PagedListState* state;

        public int pageCount {
            get => state->pageCount;
        }

        public int pageSize {
            get => state->pageSize;
        }

        public PagedSplitBufferList(PagedListState* state) {
            this.state = state;
        }

        public PagedSplitBufferList(uint pageSize, Allocator allocator) {
            state = PagedListState.Create(pageSize, allocator);
        }

        public void Dispose() {
            state->Dispose();
            UnsafeUtility.Free(state, state->allocator);
            state = null;
        }

        // item count must be smaller than page capacity
        public RangeInt AddRange(TKey* keys, TValue* data, int itemCount) {

            if (state->pageSize > itemCount) {
                return default;
            }

            if (state->pageCount == 0) {
                state->CreateNewPage();
            }

            if (state->pages[state->pageCount - 1].size + itemCount >= state->pageCapacity) {
                state->CreateNewPage();
            }

            UntypedPagedListPage* page = state->pages + (state->pageCount - 1);

            TKey* keyptr = (TKey*) page->data + page->size;
            TValue* dataptr = (TValue*) page->data + (state->pageCapacity * sizeof(TKey) + (page->size * sizeof(TValue)));

            // data starts at base + keyptr * page->capacity

            keyptr += page->size;
            UnsafeUtility.MemCpy(keyptr, keys, sizeof(TKey) * itemCount);
            UnsafeUtility.MemCpy(dataptr, data, sizeof(TValue) * itemCount);
            RangeInt retn = new RangeInt(page->size, itemCount);
            page->size += itemCount;
            return retn;

        }

        public void GetPointers(int index, out TKey* keyptr, out TValue* dataptr) {
            int pageIndex = index >> state->pageSizeIndex;
            int pageArrayIndex = index - (pageIndex * state->pageSize);
            // data starts at base + keyptr * page->capacity
            keyptr = (TKey*) (state->pages + pageIndex);
            dataptr = (TValue*) (keyptr + (state->pageCapacity * sizeof(TKey)));
            keyptr += pageArrayIndex;
            dataptr += pageArrayIndex;
        }

        public TKey GetKey(int index) {
            int pageIndex = index >> state->pageSizeIndex;
            int pageArrayIndex = index - (pageIndex * state->pageSize);
            return *((TKey*) (state->pages + pageIndex) + pageArrayIndex);
        }

        public TValue GetValue(int index) {
            int pageIndex = index >> state->pageSizeIndex;
            int pageArrayIndex = index - (pageIndex * state->pageSize);
            TValue* dataptr = (TValue*) ((state->pages + pageIndex) + (state->pageCapacity * sizeof(TKey)));
            return dataptr[pageArrayIndex];
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
                this.perThreadData = default;
                PerThreadPagedListUtil.Create(out perThreadData, allocator);
            }

            public int CountElements() {
                return PerThreadPagedListUtil.CountElements(perThreadData);
            }

            public PagedSplitBufferList<TKey, TValue> GetListForThread(int threadIndex) {
                return new PagedSplitBufferList<TKey, TValue>(PerThreadPagedListUtil.GetListForThread(perThreadData, threadIndex, allocator, pageSize));
            }

            public void Dispose() {
                PerThreadPagedListUtil.Dispose(ref perThreadData, allocator);
            }

        }

        public PagedListState* GetRawPointer() {
            return state;
        }

    }

}