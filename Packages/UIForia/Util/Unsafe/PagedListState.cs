using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Util.Unsafe {

    [AssertSize(32)]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PagedListState : IDisposable {

        internal UntypedPagedListPage* pages;
        internal int pageCapacity;
        internal int pageCount;
        internal int pageSizeLimit;
        internal int pageSizeIndex;
        public int totalItemCount;
        internal Allocator allocator;

        public static PagedListState* Create(uint pageSize, Allocator allocator) {
            pageSize = BitUtil.EnsurePowerOfTwo(pageSize < 4 ? 4 : pageSize);
            PagedListState* state = TypedUnsafe.MallocDefault<PagedListState>(allocator);
            state->pageCapacity = 4;
            state->pageSizeLimit = (int) pageSize;
            state->pageCount = 0;
            state->pageSizeIndex = BitUtil.GetPowerOfTwoBitIndex(pageSize);
            state->allocator = allocator;
            state->pages = TypedUnsafe.MallocDefault<UntypedPagedListPage>(state->pageCapacity, allocator);
            state->AddNewPage();
            return state;
        }

        public void AddNewPage() {
            if (pageCount + 1 > pageCapacity) {
                GrowPageList();
            }

            pages[pageCount].size = 0;
            pages[pageCount].data = TypedUnsafe.Malloc<UntypedPagedListPage>(pageSizeLimit, allocator);

            pageCount++;
        }

        public int CountElements() {
            int count = 0;

            for (int i = 0; i < pageCount; i++) {
                count += pages[i].size;
            }

            return count;

        }

        public T* AddItem<T>(T item) where T : unmanaged {
            return AddItem(item, out int _);
        }

        public T* AddItem<T>(T item, out int index) where T : unmanaged {
            if (pages[pageCount - 1].size + 1 > pageSizeLimit) {
                AddNewPage();
            }

            UntypedPagedListPage* page = pages + (pageCount - 1);

            T* ptr = (T*) page->data + page->size;
            index = ((pageCount - 1) * pageSizeLimit) + page->size;

            *ptr = item;
            page->size++;
            totalItemCount++;
            return ptr;
        }

        public T* AddRange<T>(T* items, int itemCount) where T : unmanaged {
            return AddRange(items, itemCount, out RangeInt _);
        }

        public T* AddRange<T>(T* items, int itemCount, out RangeInt range) where T : unmanaged {
            if (items == null || itemCount > pageSizeLimit) {
                range = default;
                return default;
            }

            if (pages[pageCount - 1].size + itemCount > pageSizeLimit) {
                AddNewPage();
            }

            UntypedPagedListPage* page = pages + (pageCount - 1);

            T* ptr = (T*) page->data + page->size;

            TypedUnsafe.MemCpy(ptr, items, itemCount);
            int start = ((pageCount - 1) * pageSizeLimit) + page->size;
            page->size += itemCount;
            totalItemCount += itemCount;
            range = new RangeInt(start, itemCount);
            return ptr;
        }

        public void GrowPageList() {
            TypedUnsafe.ResizeCleared(ref pages, pageCapacity, pageCapacity * 2, allocator);
            pageCapacity *= 2;
        }

        public void Dispose() {
            for (int i = 0; i < pageCount; i++) {
                UnsafeUtility.Free(pages[i].data, allocator);
            }

            if (pageCount > 0) {
                UnsafeUtility.Free(pages, allocator);
            }

            this = default;
        }

        public void FlattenToList<T>(UnmanagedList<T> output) where T : unmanaged {
            output.EnsureAdditionalCapacity(totalItemCount);
            for (int i = 0; i < pageCount; i++) {
                output.AddRange((T*) pages[i].data, pages[i].size);
            }
        }

        // public static UntypedPagedListPage* AddRange(PagedListState* state, int itemCount, out RangeInt range) {
        //     if (itemCount > state->pageSizeLimit) {
        //         range = default;
        //         return default;
        //     }
        //
        //     if (state->pages[state->pageCount - 1].size + itemCount > state->pageSizeLimit) {
        //         state->AddNewPage();
        //     }
        //
        //     UntypedPagedListPage* page = state->pages + (state->pageCount - 1);
        //     int start = ((state->pageCount - 1) * state->pageSizeLimit) + page->size;
        //     range = new RangeInt(start, itemCount);
        //     return page;
        // }

        public void Clear() {
            totalItemCount = 0;
            for (int i = 0; i < pageCount; i++) {
                pages[i].size = 0;
            }
        }

    }

}