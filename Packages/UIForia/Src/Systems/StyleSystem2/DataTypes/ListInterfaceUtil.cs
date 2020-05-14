using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    public unsafe struct ListInterfaceUtil<T> where T : unmanaged {

        public static void Create<TListType>(ref TListType toCreate, int initialCapacity, Allocator allocator) where TListType : unmanaged, IListInterface {
            ref ListInterface list = ref toCreate.GetListInterface();
            int capacity = initialCapacity < 8 ? 8 : initialCapacity;
            list.size = 0;
            list.capacity = capacity;
            list.array = TypedUnsafe.Malloc<T>(capacity, allocator);
        }
        
        public static void Add<TListType>(ref TListType rawList, T item, Allocator allocator) where TListType : unmanaged, IListInterface {
            ref ListInterface list = ref rawList.GetListInterface();

            if (list.size + 1 > list.capacity) {

                int capacity = (list.capacity < 8 ? 8 : list.capacity) * 2;

                T* ptr = TypedUnsafe.Malloc<T>(capacity, allocator);

                if (list.array != null && list.size > 0) {
                    TypedUnsafe.MemCpy(ptr, (T*) list.array, list.size);
                    TypedUnsafe.Dispose(list.array, allocator);
                }

                list.capacity = capacity;
                list.array = ptr;
            }

            ((T*) list.array)[list.size++] = item;
        }

        
        public static void Dispose<TListType>(ref TListType toDispose, Allocator allocator) where TListType : unmanaged, IListInterface {
            ref ListInterface list = ref toDispose.GetListInterface();
            TypedUnsafe.Dispose(list.array, allocator);
            list = default;
        }

    }

}