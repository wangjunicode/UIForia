using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Util.Unsafe {

    public unsafe struct UntypedPagedListPage {

        internal void* data;
        public int size;

    }

    public interface IListForeach<T> where T : unmanaged {

        void Run(in T item);

    }

    public interface IListForeachWithIndex<T> where T : unmanaged {

        void Run(int idx, in T item);

    }

    public interface IListFilter<T> where T : unmanaged {

        bool Filter(in T item);

    }

    // Note: Does not support removal, intended to be add only
    [DebuggerTypeProxy(typeof(UnmanagedPagedListDebugView<>))]
    public unsafe struct PagedList<T> : IDisposable where T : unmanaged {

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

        public PagedList(PagedListState* state) {
            this.state = state;
        }

        public PagedList(int pageSize, Allocator allocator) {
            state = PagedListState.Create((uint) pageSize, allocator);
        }

        public PagedList(uint pageSize, Allocator allocator) {
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
            if (state != null) {
                Allocator allocator = state->allocator;
                state->Dispose();
                UnsafeUtility.Free(state, allocator);
            }

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

        public PagedListState* GetStatePointer() {
            return state;
        }

        public void Clear() {
            state->Clear();
        }

        public void Foreach<TForeachType>(TForeachType callback) where TForeachType : IListForeach<T> {
            for (int p = 0; p < state->pageCount; p++) {
                UntypedPagedListPage page = state->pages[p];

                T* array = (T*) page.data;

                for (int i = 0; i < page.size; i++) {
                    callback.Run(array[i]);
                }

            }
        }

        public void ForeachWithIndex<TForeachType>(TForeachType callback) where TForeachType : IListForeachWithIndex<T> {
            for (int p = 0; p < state->pageCount; p++) {
                UntypedPagedListPage page = state->pages[p];

                T* array = (T*) page.data;

                int idx = p * state->pageSizeIndex;

                for (int i = 0; i < page.size; i++) {
                    callback.Run(idx++, array[i]);
                }

            }
        }

        public int FilterToSizedBuffer<TFilterType>(T* buffer, TFilterType filter) where TFilterType : IListFilter<T> {

            int passCount = 0;
            for (int p = 0; p < state->pageCount; p++) {
                UntypedPagedListPage page = state->pages[p];

                T* array = (T*) page.data;

                for (int i = 0; i < page.size; i++) {
                    if (filter.Filter(array[i])) {
                        buffer[passCount++] = array[i];
                    }
                }

            }

            return passCount;

        }

        public T* CopyToSizedBuffer(T* ptr) {
            for (int p = 0; p < state->pageCount; p++) {
                UntypedPagedListPage page = state->pages[p];
                TypedUnsafe.MemCpy(ptr, (T*) page.data, page.size);
                ptr += page.size;
            }

            return ptr;
        }

        
        
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