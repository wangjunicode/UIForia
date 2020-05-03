using System;
using System.Runtime.InteropServices;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    public static unsafe class PointerListUtil {

        public static void Initialize(ushort initialCapacity, out ushort size, out ushort capacity, out void** listPointer, Allocator allocator) {
            size = 0;
            capacity = 0;
            listPointer = null;

            if (initialCapacity > 0) {
                capacity = initialCapacity;
                int listSizeInBytes = sizeof(void*) * capacity;
                listPointer = (void**) UnsafeUtility.Malloc(listSizeInBytes, UnsafeUtility.AlignOf<long>(), allocator);
            }
        }

        public static void AddPointerItem(ref ushort size, ref ushort capacity, ref void** listPointer, void* item, Allocator allocator) {
            if (size + 1 >= capacity) {

                ushort newCapacity = (ushort) (capacity * 2);

                int listSizeInBytes = sizeof(void*) * newCapacity;

                void** newListPointer = (void**) UnsafeUtility.Malloc(listSizeInBytes, UnsafeUtility.AlignOf<long>(), allocator);

                if (listPointer != null) {
                    UnsafeUtility.MemCpy(newListPointer, listPointer, sizeof(Block*) * capacity);
                    UnsafeUtility.Free(listPointer, Allocator.Persistent);
                }

                capacity = newCapacity;

                listPointer = newListPointer;
            }

            listPointer[size++] = item;
        }

        public static void Dispose(ushort size, ref void ** listPointer) {
            if (listPointer != null) {
                for (int i = 0; i < size; i++) {
                    if (listPointer[i] != null) {
                        UnsafeUtility.Free(listPointer[i], Allocator.Persistent);
                        listPointer[i] = null;
                    }
                }
            }

            listPointer = null;

        }
        
        public static void Dispose<T>(ushort size, ref void ** listPointer) where T : unmanaged, IDisposable {
            if (listPointer != null) {
                for (int i = 0; i < size; i++) {
                    if (listPointer[i] != null) {
                        T* cast = (T*) listPointer[i];
                        cast->Dispose();
                        UnsafeUtility.Free(listPointer[i], Allocator.Persistent);
                        listPointer[i] = null;
                    }
                }
            }

            listPointer = null;

        }
        
    }
    
    

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ShortPointerListHandle : IDisposable {

        public ushort size;
        public ushort capacity;
        public void** listPointer;

        public ShortPointerListHandle(ushort initialCapacity) {
            this.size = 0;
            this.capacity = 0;
            this.listPointer = null;

            if (initialCapacity > 0) {
                this.capacity = initialCapacity;
                int listSizeInBytes = sizeof(void*) * capacity;
                this.listPointer = (void**) UnsafeUtility.Malloc(listSizeInBytes, UnsafeUtility.AlignOf<long>(), Allocator.Persistent);
            }

        }

        public void Add(void* ptr) {

            if (size + 1 >= capacity) {

                ushort newCapacity = (ushort) (capacity * 2);

                int listSizeInBytes = sizeof(void*) * newCapacity;

                void** newListPointer = (void**) UnsafeUtility.Malloc(listSizeInBytes, UnsafeUtility.AlignOf<long>(), Allocator.Persistent);

                if (listPointer != null) {
                    UnsafeUtility.MemCpy(newListPointer, listPointer, sizeof(Block*) * capacity);
                    UnsafeUtility.Free(listPointer, Allocator.Persistent);
                }

                capacity = newCapacity;

                listPointer = newListPointer;
            }

            listPointer[size++] = ptr;
        }

        public void Dispose() {
            if (listPointer != null) {
                for (int i = 0; i < size; i++) {
                    if (listPointer[i] != null) {
                        UnsafeUtility.Free(listPointer[i], Allocator.Persistent);
                        listPointer[i] = null;
                    }
                }
            }

            this = default;

        }

    }

    public unsafe struct PointerListHandle : IDisposable {

        public int size;
        public int capacity;
        public void** listPointer;

        public PointerListHandle(int initialCapacity) {
            this.size = 0;
            this.capacity = 0;
            this.listPointer = null;

            if (initialCapacity > 0) {
                this.capacity = initialCapacity;
                int listSizeInBytes = sizeof(void*) * capacity;
                this.listPointer = (void**) UnsafeUtility.Malloc(listSizeInBytes, UnsafeUtility.AlignOf<long>(), Allocator.Persistent);
            }

        }

        public void Add(void* ptr) {

            if (size + 1 >= capacity) {

                int newCapacity = capacity * 2;

                int listSizeInBytes = sizeof(void*) * newCapacity;

                void** newListPointer = (void**) UnsafeUtility.Malloc(listSizeInBytes, UnsafeUtility.AlignOf<long>(), Allocator.Persistent);

                if (listPointer != null) {
                    UnsafeUtility.MemCpy(newListPointer, listPointer, sizeof(Block*) * capacity);
                    UnsafeUtility.Free(listPointer, Allocator.Persistent);
                }

                capacity = newCapacity;

                listPointer = newListPointer;
            }

            listPointer[size++] = ptr;
        }

        public void Dispose() {
            if (listPointer != null) {
                for (int i = 0; i < size; i++) {
                    if (listPointer[i] != null) {
                        UnsafeUtility.Free(listPointer[i], Allocator.Persistent);
                        listPointer[i] = null;
                    }
                }
            }

            this = default;

        }

    }

}