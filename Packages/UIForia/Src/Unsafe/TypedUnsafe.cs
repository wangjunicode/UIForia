using System;
using System.Diagnostics;
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

    [DebuggerTypeProxy(typeof(ScopedListDebugView<>))]
    public unsafe struct ScopedList<T> : IDisposable where T : unmanaged {

        public readonly Allocator allocator;
        public readonly T* array;
        public int size;
        public int capacity;

        internal ScopedList(T* array, int capacity, Allocator allocator) {
            this.allocator = allocator;
            this.array = array;
            this.size = 0;
            this.capacity = capacity;
        }

        public void Dispose() {
            if (array != null) {
                UnsafeUtility.Free(array, allocator);
            }

            this = default;
        }

        public T this[int index] {
            get {
                TypedUnsafe.CheckRange(index, size);
                return array[index];
            }
            set {
                TypedUnsafe.CheckRange(index, size);
                array[index] = value;
            }
        }

        [DebuggerStepThrough]
        public T[] ToArray() {
            T[] dst = new T[size];
            for (int i = 0; i < size; i++) {
                dst[i] = array[i];
            }

            return dst;
        }

    }

    public unsafe struct ScopedPtr<T> : IDisposable where T : unmanaged {

        public readonly Allocator allocator;
        public readonly T* ptr;

        internal ScopedPtr(T* ptr, Allocator allocator) {
            this.allocator = allocator;
            this.ptr = ptr;
        }

        public static implicit operator T*(ScopedPtr<T> ptr) {
            return ptr.ptr;
        }

        public void Dispose() {
            if (ptr != null) {
                UnsafeUtility.Free(ptr, allocator);
            }

            this = default;
        }

    }

    internal sealed class ScopedListDebugView<T> where T : unmanaged {

        private ScopedList<T> list;

        public ScopedListDebugView(ScopedList<T> array) => this.list = array;

        public T[] Items => this.list.ToArray();

    }

    internal sealed class TempListDebugView<T> where T : unmanaged {

        private TempList<T> list;

        public TempListDebugView(TempList<T> array) => this.list = array;

        public T[] Items => this.list.ToArray();

    }

    [DebuggerTypeProxy(typeof(TempListDebugView<>))]
    public unsafe struct TempList<T> : IDisposable where T : unmanaged {

        [NativeDisableUnsafePtrRestriction] public T* array;
        public int size;
        public Allocator allocator;

        [DebuggerStepThrough]
        public TempList(T* array, int size = 0) {
            this.array = array;
            this.size = size;
            this.allocator = Allocator.None;
        }

        public void Dispose() {
            TypedUnsafe.Dispose(array, allocator);
            this = default;
        }

        [DebuggerStepThrough]
        public ref T Get(int index) {
            TypedUnsafe.CheckRange(index, size);
            return ref array[index];
        }

        public T this[int index] {
            [DebuggerStepThrough]
            get {
                TypedUnsafe.CheckRange(index, size);
                return array[index];
            }
            [DebuggerStepThrough]
            set {
                TypedUnsafe.CheckRange(index, size);
                array[index] = value;
            }
        }

        [DebuggerStepThrough]
        public T[] ToArray() {
            T[] dst = new T[size];
            for (int i = 0; i < size; i++) {
                dst[i] = array[i];
            }

            return dst;
        }
        
        [DebuggerStepThrough]
        public void MemClear() {
            TypedUnsafe.MemClear(array, size);
        }

        [DebuggerStepThrough]
        public CheckedArray<T> ToCheckedArray() {
            return new CheckedArray<T>(array, size);
        }

        [DebuggerStepThrough]
        public T* GetPointer(int i) {
            return array + i;
        }

        [DebuggerStepThrough]
        public CheckedArray<T> Slice(int start, int length) {
            return new CheckedArray<T>(array + start, length);
        }

    }

    public static unsafe class TypedUnsafe {

        public static void MemCpy<T>(T* dst, T[] array, int count) where T : unmanaged {
            if (count > array.Length) {
                throw new Exception("Cannot copy, count is larger than array size");
            }

            fixed (T* src = array) {
                MemCpy(dst, src, count);
            }
        }

        public static void MemCpy<T>(T[] dest, CheckedArray<T> source) where T : unmanaged {

            if (source.size > dest.Length) {
                throw new Exception("Cannot copy, source is larger than destination.");
            }

            fixed (T* destPointer = dest) {
                MemCpy(destPointer, source.array, source.size);
            }
        }

        public static ScopedList<T> ScopedMallocList<T>(int count, Allocator allocator) where T : unmanaged {
            return new ScopedList<T>(Malloc<T>(count, allocator), count, allocator);
        }

        public static ScopedList<T> ScopedMallocListCleared<T>(int count, Allocator allocator) where T : unmanaged {
            ScopedList<T> retn = new ScopedList<T>(Malloc<T>(count, allocator), count, allocator);
            MemClear(retn.array, count);
            return retn;
        }

        public static TempList<T> MallocClearedTempList<T>(int count, Allocator allocator) where T : unmanaged {
            TempList<T> retn = MallocUnsizedTempList<T>(count, allocator);
            MemClear(retn.array, count);
            return retn;
        }

        public static TempList<T> MallocUnsizedTempList<T>(int count, Allocator allocator) where T : unmanaged {
            return new TempList<T>() {
                size = 0,
                allocator = allocator,
                array = Malloc<T>(count, allocator)
            };
        }

        public static TempList<T> MallocSizedTempList<T>(int count, Allocator allocator) where T : unmanaged {
            return new TempList<T>() {
                size = count,
                allocator = allocator,
                array = Malloc<T>(count, allocator)
            };
        }

        public static ScopedPtr<T> ScopedMallocCleared<T>(int count, Allocator allocator) where T : unmanaged {
            ScopedPtr<T> retn = new ScopedPtr<T>(Malloc<T>(count, allocator), allocator);
            MemClear(retn.ptr, count);
            return retn;
        }

        public static ScopedPtr<T> ScopedMalloc<T>(int count, Allocator allocator) where T : unmanaged {
            return new ScopedPtr<T>(Malloc<T>(count, allocator), allocator);
        }

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

        public static bool MemCmp<T>(T a, T b, int count) where T : unmanaged {
            return UnsafeUtility.MemCmp(&a, &b, sizeof(T) * count) == 0;
        }

        public static bool MemCmp<T>(T* a, T* b, int count) where T : unmanaged {
            return UnsafeUtility.MemCmp(a, b, sizeof(T) * count) == 0;
        }

        public static bool MemCmp<T>(T* a, T* b) where T : unmanaged {
            return UnsafeUtility.MemCmp(a, b, sizeof(T)) == 0;
        }

        public static T* Malloc<T>(int count, Allocator allocator) where T : unmanaged {
            return (T*) UnsafeUtility.Malloc(count * sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
        }

        public static T* MallocCleared<T>(int count, Allocator allocator) where T : unmanaged {
            T* ptr = (T*) UnsafeUtility.Malloc(count * sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(ptr, count * sizeof(T));
            return ptr;
        }

        public static T* Malloc<T>(in T instance, Allocator allocator) where T : unmanaged {
            T* retn = (T*) UnsafeUtility.Malloc(sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
            *retn = instance;
            return retn;
        }

        public static T* Malloc<T>(Allocator allocator) where T : unmanaged {
            return (T*) UnsafeUtility.Malloc(sizeof(T), UnsafeUtility.AlignOf<T>(), allocator);
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
            if (data == null || allocator == Allocator.Invalid || allocator == Allocator.None) {
                return;
            }
            UnsafeUtility.Free(data, allocator);
        }

        public static void MemCpy<T>(T* dest, T* source, int itemCount) where T : unmanaged {
            UnsafeUtility.MemCpy(dest, source, sizeof(T) * itemCount);
        }

        public static int MemCpyAdvance<T>(T* dest, T* source, int itemCount) where T : unmanaged {
            UnsafeUtility.MemCpy(dest, source, sizeof(T) * itemCount);
            return itemCount;
        }

        public static int Megabytes(int count) {
            return count * 1024 * 1024;
        }

        public static int Kilobytes(int count) {
            return count * 1024;
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

            if (ptr != null && oldSize > 0) {
                UnsafeUtility.MemCpy(newptr, ptr, sizeof(T) * oldSize);
            }

            if (ptr != null) {
                UnsafeUtility.Free(ptr, allocator);
            }

            UnsafeUtility.MemClear(newptr + oldSize, (newSize - oldSize) * sizeof(T));

            ptr = newptr;
        }

        public static void MemClear<T>(T* ptr, int itemCount) where T : unmanaged {
            if (ptr == null || itemCount <= 0) return;
            UnsafeUtility.MemClear(ptr, itemCount * sizeof(T));
        }

        public static void MemSet<T>(T* ptr, int itemCount, byte value) where T : unmanaged {
            if (ptr == null || itemCount <= 0) return;
            UnsafeUtility.MemSet(ptr, value, itemCount * sizeof(T));
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

        public static Allocator GetTemporaryAllocatorLabel<T>(int max) where T : unmanaged {
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

        public static byte* MallocSplitBuffer<T0, T1, T2, T3, T4, T5>(
            out T0* ptr0,
            out T1* ptr1,
            out T2* ptr2,
            out T3* ptr3,
            out T4* ptr4,
            out T5* ptr5,
            int itemCount,
            Allocator allocator,
            bool clearMemory = false
        )
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged {

            long t0Size = sizeof(T0) * itemCount;
            long t1Size = sizeof(T1) * itemCount;
            long t2Size = sizeof(T2) * itemCount;
            long t3Size = sizeof(T3) * itemCount;
            long t4Size = sizeof(T4) * itemCount;
            long t5Size = sizeof(T5) * itemCount;

            long byteCount = t0Size + t1Size + t2Size + t3Size + t4Size + t5Size;
            ptr0 = (T0*) UnsafeUtility.Malloc(byteCount, 4, allocator);
            ptr1 = (T1*) (ptr0 + itemCount);
            ptr2 = (T2*) (ptr1 + itemCount);
            ptr3 = (T3*) (ptr2 + itemCount);
            ptr4 = (T4*) (ptr3 + itemCount);
            ptr5 = (T5*) (ptr4 + itemCount);

            if (clearMemory) {
                UnsafeUtility.MemClear(ptr0, byteCount);
            }

            return (byte*) ptr0;
        }

        public static byte* MallocSplitBuffer<T0, T1, T2, T3, T4, T5, T6>(
            out T0* ptr0,
            out T1* ptr1,
            out T2* ptr2,
            out T3* ptr3,
            out T4* ptr4,
            out T5* ptr5,
            out T6* ptr6,
            int itemCount,
            Allocator allocator,
            bool clearMemory = false
        )
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
            where T6 : unmanaged {

            long t0Size = sizeof(T0) * itemCount;
            long t1Size = sizeof(T1) * itemCount;
            long t2Size = sizeof(T2) * itemCount;
            long t3Size = sizeof(T3) * itemCount;
            long t4Size = sizeof(T4) * itemCount;
            long t5Size = sizeof(T5) * itemCount;
            long t6Size = sizeof(T6) * itemCount;

            long byteCount = t0Size + t1Size + t2Size + t3Size + t4Size + t5Size + t6Size;
            ptr0 = (T0*) UnsafeUtility.Malloc(byteCount, 4, allocator);
            ptr1 = (T1*) (ptr0 + itemCount);
            ptr2 = (T2*) (ptr1 + itemCount);
            ptr3 = (T3*) (ptr2 + itemCount);
            ptr4 = (T4*) (ptr3 + itemCount);
            ptr5 = (T5*) (ptr4 + itemCount);
            ptr6 = (T6*) (ptr5 + itemCount);

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

        public static byte* ResizeSplitBuffer<T0, T1, T2, T3, T4, T5>(
            ref T0* ptr0,
            ref T1* ptr1,
            ref T2* ptr2,
            ref T3* ptr3,
            ref T4* ptr4,
            ref T5* ptr5,
            int oldItemCount,
            int newItemCount,
            Allocator allocator,
            bool clearNewMemory = false
        )
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged {

            T0* oldPtr0 = ptr0;
            T1* oldPtr1 = ptr1;
            T2* oldPtr2 = ptr2;
            T3* oldPtr3 = ptr3;
            T4* oldPtr4 = ptr4;
            T5* oldPtr5 = ptr5;

            byte* retn = MallocSplitBuffer(out ptr0, out ptr1, out ptr2, out ptr3, out ptr4, out ptr5, newItemCount, allocator);

            GrowSplitBufferSection(ptr0, oldPtr0, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr1, oldPtr1, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr2, oldPtr2, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr3, oldPtr3, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr4, oldPtr4, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr5, oldPtr5, oldItemCount, newItemCount, clearNewMemory);

            if (oldPtr0 != null && oldItemCount > 0) {
                UnsafeUtility.Free(oldPtr0, allocator);
            }

            return retn;
        }

        public static byte* ResizeSplitBuffer<T0, T1, T2, T3, T4, T5, T6>(
            ref T0* ptr0,
            ref T1* ptr1,
            ref T2* ptr2,
            ref T3* ptr3,
            ref T4* ptr4,
            ref T5* ptr5,
            ref T6* ptr6,
            int oldItemCount,
            int newItemCount,
            Allocator allocator,
            bool clearNewMemory = false
        )
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
            where T6 : unmanaged {

            T0* oldPtr0 = ptr0;
            T1* oldPtr1 = ptr1;
            T2* oldPtr2 = ptr2;
            T3* oldPtr3 = ptr3;
            T4* oldPtr4 = ptr4;
            T5* oldPtr5 = ptr5;
            T6* oldPtr6 = ptr6;

            byte* retn = MallocSplitBuffer(out ptr0, out ptr1, out ptr2, out ptr3, out ptr4, out ptr5, out ptr6, newItemCount, allocator);

            GrowSplitBufferSection(ptr0, oldPtr0, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr1, oldPtr1, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr2, oldPtr2, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr3, oldPtr3, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr4, oldPtr4, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr5, oldPtr5, oldItemCount, newItemCount, clearNewMemory);
            GrowSplitBufferSection(ptr6, oldPtr6, oldItemCount, newItemCount, clearNewMemory);

            if (oldPtr0 != null && oldItemCount > 0) {
                UnsafeUtility.Free(oldPtr0, allocator);
            }

            return retn;
        }

        private static void GrowSplitBufferSection<T>(T* newPtr, T* oldPtr, int oldCount, int newCount, bool clearNewMemory) where T : unmanaged {
            if (oldPtr != null && oldCount > 0) {
                MemCpy(newPtr, oldPtr, oldCount);
            }

            if (clearNewMemory) {
                MemClear(newPtr + oldCount, newCount - oldCount);
            }
        }

        public static int ByteSize<T, T1>(int count) where T : unmanaged where T1 : unmanaged {
            return count * (sizeof(T) + sizeof(T1));
        }

        public static int ByteSize<T, T1>(uint count) where T : unmanaged where T1 : unmanaged {
            return (int) count * (sizeof(T) + sizeof(T1));
        }

        public static int ByteSize<T, T1>(T* a, T1* b, int count) where T : unmanaged where T1 : unmanaged {
            return count * (sizeof(T) + sizeof(T1));
        }

        public static ref T AsRef<T>(T* target) where T : unmanaged {
            return ref UnsafeUtility.AsRef<T>(target);
        }

        public static T* MallocCopy<T>(T[] buffer, int size, Allocator allocator = Allocator.Persistent) where T : unmanaged {
            T* retn = Malloc<T>(size, allocator);
            fixed (T* src = buffer) {
                MemCpy(retn, src, size);
            }

            return retn;
        }

        public static T* GetPointer<T>(NativeArray<T> nativeArray) where T : unmanaged {
            return (T*) nativeArray.GetUnsafePtr();
        }

        public static void MemCpyResize<T>(ref NativeArray<T> dst, T* src, int oldCount, int count) where T : unmanaged {
            throw new NotImplementedException();
        }

        public static void MemCpyResize<T>(ref T* dst, T* src, int oldCount, int count) where T : unmanaged {
            throw new NotImplementedException();
        }

        public static void AddToList<T>(ref T* listPtr, ref int size, ref int capacity, in T value) where T : unmanaged {
            if (capacity == 0) {
                capacity = 4;
                listPtr = Malloc<T>(capacity, Allocator.Persistent);
                listPtr[0] = value;
                size = 1;
            }
            else if (size + 1 > capacity) {
                T* alloc = Malloc<T>(capacity * 2, Allocator.Persistent);
                MemCpy(alloc, listPtr, size);
                Dispose(listPtr, Allocator.Persistent);
                listPtr = alloc;
                capacity *= 2;
                listPtr[size] = value;
                size++;
            }
            else {
                listPtr[size] = value;
                size++;
            }
        }

        public static T[] ToArray<T>(T* array, int size) where T : unmanaged {
            T[] retn = new T[size];
            for (int i = 0; i < size; i++) {
                retn[i] = array[i];
            }

            return retn;
        }

        public static T[] ToArray<T>(T* array, int start, int count) where T : unmanaged {
            T[] retn = new T[count];
            for (int i = 0; i < count; i++) {
                retn[i] = array[start + i];
            }

            return retn;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal static void CheckRange(int index, int size) {
            if ((uint) index >= size) {
                throw new IndexOutOfRangeException($"Index {(object) index} is out of range of '{(object) size}' size.");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal static void CheckRange(uint index, int size) {
            if ((uint) index >= size) {
                throw new IndexOutOfRangeException($"Index {(object) index} is out of range of '{(object) size}' size.");
            }
        }
    }

}