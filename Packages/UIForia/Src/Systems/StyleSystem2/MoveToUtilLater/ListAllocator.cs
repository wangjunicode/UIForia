using System;

namespace UIForia.Util.Unsafe {

    public unsafe struct ListAllocator<T> : IDisposable where T : unmanaged {

        private Pow2AllocatorSet allocatorSet;
        private bool shouldDispose;

        public static ListAllocator<T> Create(FixedAllocatorDesc* allocators, int count) {
            return new ListAllocator<T>() {
                shouldDispose = true,
                allocatorSet = new Pow2AllocatorSet(allocators, count)
            };
        }

        public static ListAllocator<T> Create(Pow2AllocatorSet allocatorSet) {
            return new ListAllocator<T>() {
                shouldDispose = false,
                allocatorSet = allocatorSet
            };
        }

        public void Add<TListType>(ref TListType rawList, in T item) where TListType : unmanaged, IListInterface {
            ref ListInterface list = ref rawList.GetListInterface();

            // todo -- diagnostic
            // if (rawList.ItemSize != sizeof(T)) {
            //     return;
            // }

            if (list.size + 1 > list.capacity) {
                int byteCapacity = allocatorSet.Allocate((list.size + 1) * sizeof(T), out void* ptr);

                if (list.array != null) {
                    TypedUnsafe.MemCpy((T*) ptr, (T*) list.array, list.size);
                    allocatorSet.Free(list.array, list.capacity * sizeof(T));
                }

                list.array = ptr;
                list.capacity = byteCapacity / sizeof(T);
            }

            ((T*) list.array)[list.size++] = item;
        }

        public void EnsureAdditionalCapacity<TListType>(ref TListType rawList, int count) where TListType : unmanaged, IListInterface {
            ref ListInterface list = ref rawList.GetListInterface();

            // todo -- diagnostic
            // if (rawList.ItemSize != sizeof(T)) {
            //     return;
            // }

            if (list.size + count > list.capacity) {
                int byteCapacity = allocatorSet.Allocate((list.size + count) * sizeof(T), out void* ptr);

                if (list.array != null) {
                    TypedUnsafe.MemCpy((T*) ptr, (T*) list.array, list.size);
                    allocatorSet.Free(list.array, list.capacity * sizeof(T));
                }

                list.array = ptr;
                list.capacity = byteCapacity / sizeof(T);
            }

        }

        public void Free<TListType>(ref TListType toFree) where TListType : unmanaged, IListInterface {
            ref ListInterface list = ref toFree.GetListInterface();

            // todo -- diagnostic
            // if (toFree.ItemSize != sizeof(T)) {
            //     return;
            // }

            if (list.array != null) {
                int byteCapacity = list.capacity * sizeof(T);
                allocatorSet.Free(list.array, byteCapacity);
            }

            list = default;
        }

        public void Dispose() {
            if (shouldDispose) {
                allocatorSet.Dispose();
                allocatorSet = default;
            }
        }

        public void Remove<TListType>(ref TListType rawList, int index) where TListType : unmanaged, IListInterface {
            ref ListInterface list = ref rawList.GetListInterface();
            T* typedArray = (T*) list.array;
            typedArray[index] = typedArray[--list.size];

            if (list.size == 0) {
                allocatorSet.Free(list.array, list.capacity * sizeof(T*));
                rawList = default;
            }
        }

        public void Remove<TListType>(TListType* rawList, int index) where TListType : unmanaged, IListInterface {
            ref ListInterface list = ref rawList->GetListInterface();
            T* typedArray = (T*) list.array;
            typedArray[index] = typedArray[--list.size];

            if (list.size == 0) {
                allocatorSet.Free(list.array, list.capacity * sizeof(T*));
                *rawList = default;
            }
        }

        public void Replace<TListType>(ref TListType rawList, T* items, int itemCount) where TListType : unmanaged, IListInterface {
            ref ListInterface list = ref rawList.GetListInterface();
            if (list.capacity < itemCount) {
                
                if (list.array != null) {
                    allocatorSet.Free(list.array, list.capacity * sizeof(T));
                }
                
                int byteCapacity = allocatorSet.Allocate(itemCount * sizeof(T), out void* ptr);
                list.array = ptr;
                list.capacity = byteCapacity / sizeof(T);
            }

            list.size = itemCount;
            TypedUnsafe.MemCpy((T*)list.array, items, itemCount);
        }

        public void Clear() {
            throw new NotImplementedException();
        }

    }

}