using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public enum AllocatorUShort : ushort {

        Invalid,
        None,
        Temp,
        TempJob,
        Persistent,
        AudioKernel,

    }

    public enum AllocatorByte : byte {

        Invalid,
        None,
        Temp,
        TempJob,
        Persistent,
        AudioKernel,

    }

    public static unsafe class TypedUnsafe {

        public static Allocator ConvertCompressedAllocator(AllocatorByte compressed) {
            switch (compressed) {

                case AllocatorByte.Invalid:
                    return Allocator.Invalid;

                case AllocatorByte.None:
                    return Allocator.None;

                case AllocatorByte.Temp:
                    return Allocator.Temp;

                case AllocatorByte.TempJob:
                    return Allocator.TempJob;

                case AllocatorByte.Persistent:
                    return Allocator.Persistent;

                case AllocatorByte.AudioKernel:
                    return Allocator.Persistent;

                default:
                    return Allocator.Invalid;
            }
        }

        public static Allocator ConvertCompressedAllocator(AllocatorUShort compressed) {
            switch (compressed) {

                case AllocatorUShort.Invalid:
                    return Allocator.Invalid;

                case AllocatorUShort.None:
                    return Allocator.None;

                case AllocatorUShort.Temp:
                    return Allocator.Temp;

                case AllocatorUShort.TempJob:
                    return Allocator.TempJob;

                case AllocatorUShort.Persistent:
                    return Allocator.Persistent;

                case AllocatorUShort.AudioKernel:
                    return Allocator.Persistent;

                default:
                    return Allocator.Invalid;
            }
        }

        public static AllocatorUShort CompressAllocatorToUShort(Allocator allocator) {
            switch (allocator) {

                case Allocator.Invalid:
                    return AllocatorUShort.Invalid;

                case Allocator.None:
                    return AllocatorUShort.None;

                case Allocator.Temp:
                    return AllocatorUShort.Temp;

                case Allocator.TempJob:
                    return AllocatorUShort.TempJob;

                case Allocator.Persistent:
                    return AllocatorUShort.Persistent;

                case Allocator.AudioKernel:
                    return AllocatorUShort.AudioKernel;

                default:
                    return AllocatorUShort.Invalid;
            }
        }

        public static bool MemCmp<T>(T* a, T* b, int count) where T : unmanaged {
            return UnsafeUtility.MemCmp(a, b, sizeof(T) * count) == 0;
        }

        public static T* Malloc<T>(int count, Allocator allocator) where T : unmanaged {
            return (T*) UnsafeUtility.Malloc(count * sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
        }

        public static T* MallocDefault<T>(Allocator allocator) where T : unmanaged {
            T* retn = (T*) UnsafeUtility.Malloc(sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(retn, sizeof(T));
            return retn;
        }

        public static T* MallocDefault<T>(uint count, Allocator allocator) where T : unmanaged {
            T* retn = (T*) UnsafeUtility.Malloc(count * sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(retn, count * sizeof(T));
            return retn;
        }

        public static T* MallocDefault<T>(int count, Allocator allocator) where T : unmanaged {
            T* retn = (T*) UnsafeUtility.Malloc(count * sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(retn, count * sizeof(T));
            return retn;
        }

        public static void Dispose(void* data, Allocator allocator) {
            if (data == null) return;
            UnsafeUtility.Free(data, allocator);
        }

        public static void MemCpy<T>(T* dest, T* source, int itemCount) where T : unmanaged {
            UnsafeUtility.MemCpy(dest, source, sizeof(T) * itemCount);
        }

        public static int MemCpyAdvance<T>(T* dest, T* source, int itemCount) where T : unmanaged {
            UnsafeUtility.MemCpy(dest, source, sizeof(T) * itemCount);
            return itemCount;
        }

        public static void Resize<T>(ref T* ptr, int oldSize, int newSize, Allocator allocator) where T : unmanaged {
            if (newSize < oldSize) return;
            if (newSize <= 0) return;

            T* newptr = Malloc<T>(newSize, allocator);
            if (ptr != null) {
                UnsafeUtility.MemCpy(newptr, ptr, sizeof(T) * oldSize);
                UnsafeUtility.Free(ptr, allocator);
            }

            ptr = newptr;
        }

        public static void ResizeCleared<T>(ref T* ptr, int oldSize, int newSize, Allocator allocator) where T : unmanaged {
            if (newSize < oldSize) return;
            if (newSize <= 0) return;

            T* newptr = Malloc<T>(newSize, allocator);
            if (ptr != null) {
                UnsafeUtility.MemCpy(newptr, ptr, sizeof(T) * oldSize);
                UnsafeUtility.Free(ptr, allocator);
            }

            UnsafeUtility.MemClear(newptr, newSize - oldSize);
            ptr = newptr;
        }

        public static void MemClear<T>(T* ptr, int itemCount) where T : unmanaged {
            if (ptr == null || itemCount <= 0) return;
            UnsafeUtility.MemClear(ptr, itemCount * sizeof(T));
        }

        public static int SizeOf<T, U, V>(int itemCount = 1) where T : unmanaged where U : unmanaged where V : unmanaged {
            return (sizeof(T) * itemCount) + (sizeof(U) * itemCount) + (sizeof(V) * itemCount);
        }

        public static int SizeOf<T, U>(int itemCount = 1) where T : unmanaged where U : unmanaged {
            return (sizeof(T) * itemCount) + (sizeof(U) * itemCount);
        }

        public static int SizeOf<T>(int itemCount) where T : unmanaged {
            return sizeof(T) * itemCount;
        }

        public static Allocator GetTemporaryAllocator<T>(int max) where T : unmanaged {
            return sizeof(T) * max <= 1024 ? Allocator.Temp : Allocator.TempJob;
        }

        public static byte* MallocSplitBuffer<T0, T1>(
            out T0* ptr0,
            out T1* ptr1,
            int itemCount,
            Allocator allocator,
            bool clearMemory = false
        )
            where T0 : unmanaged
            where T1 : unmanaged {

            long t0Size = sizeof(T0) * itemCount;
            long t1Size = sizeof(T1) * itemCount;

            long byteCount = t0Size + t1Size;
            ptr0 = (T0*) UnsafeUtility.Malloc(byteCount, 4, allocator);
            ptr1 = (T1*) (ptr0 + itemCount);

            if (clearMemory) {
                UnsafeUtility.MemClear(ptr0, byteCount);
            }

            return (byte*) ptr0;
        }

        public static byte* MallocSplitBuffer<T0, T1, T2>(
            out T0* ptr0,
            out T1* ptr1,
            out T2* ptr2,
            int itemCount,
            Allocator allocator,
            bool clearMemory = false
        )
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged {

            long t0Size = sizeof(T0) * itemCount;
            long t1Size = sizeof(T1) * itemCount;
            long t2Size = sizeof(T2) * itemCount;

            long byteCount = t0Size + t1Size + t2Size;
            ptr0 = (T0*) UnsafeUtility.Malloc(byteCount, 4, allocator);
            ptr1 = (T1*) (ptr0 + itemCount);
            ptr2 = (T2*) (ptr1 + itemCount);

            if (clearMemory) {
                UnsafeUtility.MemClear(ptr0, byteCount);
            }

            return (byte*) ptr0;
        }

        public static byte* MallocSplitBuffer<T0, T1, T2, T3>(
            out T0* ptr0,
            out T1* ptr1,
            out T2* ptr2,
            out T3* ptr3,
            int itemCount,
            Allocator allocator,
            bool clearMemory = false
        )
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged {

            long t0Size = sizeof(T0) * itemCount;
            long t1Size = sizeof(T1) * itemCount;
            long t2Size = sizeof(T2) * itemCount;
            long t3Size = sizeof(T3) * itemCount;

            long byteCount = t0Size + t1Size + t2Size + t3Size;
            ptr0 = (T0*) UnsafeUtility.Malloc(byteCount, 4, allocator);
            ptr1 = (T1*) (ptr0 + itemCount);
            ptr2 = (T2*) (ptr1 + itemCount);
            ptr3 = (T3*) (ptr2 + itemCount);

            if (clearMemory) {
                UnsafeUtility.MemClear(ptr0, byteCount);
            }

            return (byte*) ptr0;
        }

        public static byte* MallocSplitBuffer<T0, T1, T2, T3, T4>(
            out T0* ptr0,
            out T1* ptr1,
            out T2* ptr2,
            out T3* ptr3,
            out T4* ptr4,
            int itemCount,
            Allocator allocator,
            bool clearMemory = false
        )
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged {

            long t0Size = sizeof(T0) * itemCount;
            long t1Size = sizeof(T1) * itemCount;
            long t2Size = sizeof(T2) * itemCount;
            long t3Size = sizeof(T3) * itemCount;
            long t4Size = sizeof(T4) * itemCount;

            long byteCount = t0Size + t1Size + t2Size + t3Size + t4Size;
            ptr0 = (T0*) UnsafeUtility.Malloc(byteCount, 4, allocator);
            ptr1 = (T1*) (ptr0 + itemCount);
            ptr2 = (T2*) (ptr1 + itemCount);
            ptr3 = (T3*) (ptr2 + itemCount);
            ptr4 = (T4*) (ptr3 + itemCount);

            if (clearMemory) {
                UnsafeUtility.MemClear(ptr0, byteCount);
            }

            return (byte*) ptr0;
        }

        public static byte* ResizeSplitBuffer<T0, T1>(
            ref T0* ptr0,
            ref T1* ptr1,
            int oldItemCount,
            int newItemCount,
            Allocator allocator,
            bool clearNewMemory = false
        )
            where T0 : unmanaged
            where T1 : unmanaged {

            T0* oldPtr0 = ptr0;
            T1* oldPtr1 = ptr1;

            byte* retn = MallocSplitBuffer(out ptr0, out ptr1, newItemCount, allocator);

            GrowSplitBufferSection(ptr0, oldPtr0, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr1, oldPtr1, oldItemCount, newItemCount, clearNewMemory);

            if (oldPtr0 != null && oldItemCount > 0) {
                UnsafeUtility.Free(oldPtr0, allocator);
            }

            return retn;
        }

        public static byte* ResizeSplitBuffer<T0, T1, T2>(
            ref T0* ptr0,
            ref T1* ptr1,
            ref T2* ptr2,
            int oldItemCount,
            int newItemCount,
            Allocator allocator,
            bool clearNewMemory = false
        )
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged {

            T0* oldPtr0 = ptr0;
            T1* oldPtr1 = ptr1;
            T2* oldPtr2 = ptr2;

            byte* retn = MallocSplitBuffer(out ptr0, out ptr1, out ptr2, newItemCount, allocator);

            GrowSplitBufferSection(ptr0, oldPtr0, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr1, oldPtr1, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr2, oldPtr2, oldItemCount, newItemCount, clearNewMemory);

            if (oldPtr0 != null && oldItemCount > 0) {
                UnsafeUtility.Free(oldPtr0, allocator);
            }

            return retn;
        }

        public static byte* ResizeSplitBuffer<T0, T1, T2, T3>(
            ref T0* ptr0,
            ref T1* ptr1,
            ref T2* ptr2,
            ref T3* ptr3,
            int oldItemCount,
            int newItemCount,
            Allocator allocator,
            bool clearNewMemory = false
        )
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged {

            T0* oldPtr0 = ptr0;
            T1* oldPtr1 = ptr1;
            T2* oldPtr2 = ptr2;
            T3* oldPtr3 = ptr3;

            byte* retn = MallocSplitBuffer(out ptr0, out ptr1, out ptr2, out ptr3, newItemCount, allocator);

            GrowSplitBufferSection(ptr0, oldPtr0, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr1, oldPtr1, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr2, oldPtr2, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr3, oldPtr3, oldItemCount, newItemCount, clearNewMemory);

            if (oldPtr0 != null && oldItemCount > 0) {
                UnsafeUtility.Free(oldPtr0, allocator);
            }

            return retn;
        }

        public static byte* ResizeSplitBuffer<T0, T1, T2, T3, T4>(
            ref T0* ptr0,
            ref T1* ptr1,
            ref T2* ptr2,
            ref T3* ptr3,
            ref T4* ptr4,
            int oldItemCount,
            int newItemCount,
            Allocator allocator,
            bool clearNewMemory = false
        )
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged {

            T0* oldPtr0 = ptr0;
            T1* oldPtr1 = ptr1;
            T2* oldPtr2 = ptr2;
            T3* oldPtr3 = ptr3;
            T4* oldPtr4 = ptr4;

            byte* retn = MallocSplitBuffer(out ptr0, out ptr1, out ptr2, out ptr3, out ptr4, newItemCount, allocator);

            GrowSplitBufferSection(ptr0, oldPtr0, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr1, oldPtr1, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr2, oldPtr2, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr3, oldPtr3, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr4, oldPtr4, oldItemCount, newItemCount, clearNewMemory);

            if (oldPtr0 != null && oldItemCount > 0) {
                UnsafeUtility.Free(oldPtr0, allocator);
            }

            return retn;
        }

        private static void GrowSplitBufferSection<T>(T* newPtr, T* oldPtr, int oldCount, int newCount, bool clearNewMemory) where T : unmanaged {
            if (oldPtr != null && oldCount > 0) {
                MemCpy(newPtr, oldPtr, sizeof(T) * oldCount);
            }

            if (clearNewMemory) {
                MemClear(newPtr + oldCount, newCount - oldCount);
            }
        }

        public static int ByteSize<T, T1>(int count) where T : unmanaged where T1 : unmanaged {
            return count * (sizeof(T) + sizeof(T1));
        }
        
        public static int ByteSize<T, T1>(uint count) where T : unmanaged where T1 : unmanaged {
            return (int)count * (sizeof(T) + sizeof(T1));
        }
        
        public static int ByteSize<T, T1>(T * a, T1 * b, int count) where T : unmanaged where T1 : unmanaged {
            return count * (sizeof(T) + sizeof(T1));
        }

    }

}