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
            get => state->pageSizeLimit;
        }

        public PagedSplitBufferList(PagedListState* state) {
            this.state = state;
        }

        public PagedSplitBufferList(uint pageSize, Allocator allocator) {
            state = PagedListState.Create(pageSize, allocator);
        }

        public void Dispose() {
            if (state != null) {
                Allocator allocator = state->allocator;
                state->Dispose();
                UnsafeUtility.Free(state, allocator);
            }

            this = default;
        }

        // item count must be smaller than page capacity
        public RangeInt AddRange(TKey* keys, TValue* data, int itemCount) {

            throw new NotImplementedException();
            // if (keys == null || data == null) {
            //     return default;
            // }
            //
            // UntypedPagedListPage* page = PagedListState.AddRange(state, itemCount, out RangeInt retn);
            //
            // if (page == null) {
            //     return default;
            // }
            //
            // // data starts at base + keyptr * state->pageSizeLimit
            // int startIndex = page->size - itemCount;
            // TKey* keybase = (TKey*) page->data;
            // TValue* dataStart = ((TValue*) (keybase + state->pageSizeLimit));
            //
            // TypedUnsafe.MemCpy(keybase + startIndex, keys, itemCount);
            // TypedUnsafe.MemCpy(dataStart + startIndex, data, itemCount);
            //
            // return retn;
        }

        public void GetPointers(int index, out TKey* keyptr, out TValue* dataptr) {
            int pageIndex = index >> state->pageSizeIndex;
            int pageArrayIndex = index - (pageIndex * state->pageSizeLimit);

            // data starts at base + keyptr * page->capacity
            UntypedPagedListPage page = state->pages[pageIndex];
            TKey* keybase = (TKey*) page.data;
            TValue* dataStart = ((TValue*) (keybase + state->pageSizeLimit));
            keyptr = keybase + pageArrayIndex;
            dataptr = dataStart + pageArrayIndex;
        }

        public TKey GetKey(int index) {
            int pageIndex = index >> state->pageSizeIndex;
            int pageArrayIndex = index - (pageIndex * state->pageSizeLimit);
            UntypedPagedListPage page = state->pages[pageIndex];
            TKey* keybase = (TKey*) page.data;
            return *(keybase + pageArrayIndex);
        }

        public TValue GetValue(int index) {
            int pageIndex = index >> state->pageSizeIndex;
            int pageArrayIndex = index - (pageIndex * state->pageSizeLimit);
            UntypedPagedListPage page = state->pages[pageIndex];
            TKey* keybase = (TKey*) page.data;
            TValue* dataStart = ((TValue*) (keybase + state->pageSizeLimit));
            return *(dataStart + pageArrayIndex);
        }

        // this is so shitty, Unity might spawn up to 128 threads and in order to be promised some safety
        // I need to make pointer space for each thread to have its own data even though I'll likely never
        // use more than about 12 of them outside of crazy threadripper.
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

            public void ToSplitBuffer(SplitBuffer<TKey, TValue> outputList) {
                throw new NotImplementedException();
            }

        }

        public PagedListState* GetStatePointer() {
            return state;
        }

    }
    

 
}