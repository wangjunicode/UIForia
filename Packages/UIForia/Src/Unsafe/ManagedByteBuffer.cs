using System;
using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Util {

    public unsafe struct ManagedByteBuffer {

        public byte[] array;
        public int ptr;

        public ManagedByteBuffer(byte[] bytes) {
            this.array = bytes;
            this.ptr = 0;
        }

        public void Write(string[] values) {
            Write(values.Length);
            for (int i = 0; i < values.Length; i++) {
                Write(values[i]);
            }
        }

        public void Read(out string[] values) {
            Read(out int length);
            values = new string[length];
            for (int i = 0; i < length; i++) {
                Read(out values[i]);
            }
        }

        public void Read(out uint value) {
            Read(out int i);
            value = (uint) i;
        }

        public void Write(string value) {
            int length = 0;
            if (value != null) {
                length = value.Length;
            }

            if (array == null) {
                array = new byte[Mathf.Max(512, length * 2)];
            }

            int advance = ptr + sizeof(int) + (length * 2);
            if (advance > array.Length) {
                int required = advance;
                int doubled = array.Length * 2;
                Array.Resize(ref array, required > doubled ? required : doubled);
            }

            if (value == null) length = -1;

            fixed (byte* p = array) {
                int* offset = (int*) (p + ptr);
                *offset = length;
            }

            ptr += sizeof(int);

            if (value != null) {
                fixed (char* charptr = value)
                fixed (byte* p = array) {
                    byte* offset = p + ptr;
                    UnsafeUtility.MemCpy(offset, charptr, value.Length * 2);
                }

                ptr += value.Length * 2;
            }
        }

        public void Write(int value) {
            if (array == null) {
                array = new byte[512];
            }

            if (ptr + sizeof(int) > array.Length) {
                Array.Resize(ref array, array.Length * 2);
            }

            fixed (byte* p = array) {
                byte* offset = p + ptr;
                *((int*) offset) = value;
            }

            ptr += sizeof(int);
        }

        public void Write(long value) {
            if (array == null) {
                array = new byte[512];
            }

            if (ptr + sizeof(long) > array.Length) {
                Array.Resize(ref array, array.Length * 2);
            }

            fixed (byte* p = array) {
                long* offset = (long*) (p + ptr);
                *offset = value;
            }

            ptr += sizeof(long);
        }

        public void Write(DateTime value) {
            Write(value.Ticks);
        }

        public void Write(ushort value) {
            if (array == null) {
                array = new byte[512];
            }

            if (ptr + sizeof(ushort) > array.Length) {
                Array.Resize(ref array, array.Length * 2);
            }

            fixed (byte* p = array) {
                byte* offset = p + ptr;
                *((ushort*) offset) = value;
            }

            ptr += sizeof(ushort);
        }

        public void Write<T>(T[] itemArray) where T : unmanaged {
            if (itemArray == null) {
                Write(0);
                return;
            }

            Write(itemArray.Length);
            if (ptr + (sizeof(T) * itemArray.Length) > array.Length) {
                int advance = ptr + (sizeof(T) * itemArray.Length);
                int doubled = array.Length * 2;
                Array.Resize(ref array, advance > doubled ? advance : doubled);
            }

            fixed (T* itemPtr = itemArray)
            fixed (byte* p = array) {
                UnsafeUtility.MemCpy(p + ptr, itemPtr, sizeof(T) * itemArray.Length);
            }

            ptr += sizeof(T) * itemArray.Length;
        }

        public RangeInt WriteWithRange<T>(T item) where T : unmanaged {

            if (ptr + (sizeof(T)) > array.Length) {
                int advance = ptr + (sizeof(T));
                int doubled = array.Length * 2;
                Array.Resize(ref array, advance > doubled ? advance : doubled);
            }

            fixed (byte* p = array) {
                T* dest = (T*) (p + ptr);
                *dest = item;
            }

            RangeInt retn = new RangeInt(ptr, sizeof(T));
            ptr += sizeof(T);
            return retn;
        }

        public void Write<T>(T item) where T : unmanaged {

            if (ptr + (sizeof(T)) > array.Length) {
                int advance = ptr + (sizeof(T));
                int doubled = array.Length * 2;
                Array.Resize(ref array, advance > doubled ? advance : doubled);
            }

            fixed (byte* p = array) {
                T* dest = (T*) (p + ptr);
                *dest = item;
            }

            ptr += sizeof(T);
        }

        public void Write<T>(T* items, int count) where T : unmanaged {
            if (array == null) {
                array = new byte[sizeof(T) * count * 2];
            }

            // count is expected to have been written already, or otherwise known ahead of time
            
            if (ptr + (sizeof(T) * count) > array.Length) {
                int advance = ptr + (sizeof(T) * count);
                int doubled = array.Length * 2;
                Array.Resize(ref array, advance > doubled ? advance : doubled);
            }

            fixed (byte* p = array) {
                UnsafeUtility.MemCpy(p + ptr, items, sizeof(T) * count);
            }

            ptr += sizeof(T) * count;
        }

        public void Read<T>(out T[] outputArray) where T : unmanaged {
            Read(out int size);
            outputArray = new T[size];
            Read(outputArray);
        }

        public void Read<T>(T[] outputArray) where T : unmanaged {
            fixed (T* output = outputArray)
            fixed (byte* p = array) {
                T* items = (T*) (p + ptr);
                UnsafeUtility.MemCpy(output, items, sizeof(T) * outputArray.Length);
                ptr += sizeof(T) * outputArray.Length;
            }
        }

        public void Read(out DateTime target) {
            Read(out long longVal);
            target = new DateTime(longVal);
        }

        public void Read(out long target) {
            fixed (byte* p = array) {
                long* i = (long*) (p + ptr);
                target = *i;
            }

            ptr += sizeof(long);
        }

        public void Read(out int target) {
            fixed (byte* p = array) {
                int* i = (int*) (p + ptr);
                target = *i;
            }

            ptr += sizeof(int);
        }
        
        public void Read(out float target) {
            fixed (byte* p = array) {
                float* i = (float*) (p + ptr);
                target = *i;
            }

            ptr += sizeof(float);
        }
        
        public void Read(out ushort target) {
            fixed (byte* p = array) {
                ushort* i = (ushort*) (p + ptr);
                target = *i;
            }

            ptr += sizeof(ushort);
        }

        public void Read<T>(T* output, int itemCount) where T : unmanaged {
            fixed (byte* p = array) {
                T* items = (T*) (p + ptr);
                UnsafeUtility.MemCpy(output, items, sizeof(T) * itemCount);
                ptr += sizeof(T) * itemCount;
            }
        }

        public void Read(out string target) {
            fixed (byte* p = array) {
                int* i = (int*) (p + ptr);
                int length = *i;
                ptr += sizeof(int);
                if (length == 0) {
                    target = string.Empty;
                }
                else if (length < 0) {
                    target = null;
                }
                else {
                    char* charptr = (char*) (p + ptr);
                    target = new string(charptr, 0, length);
                    ptr += sizeof(char) * length;
                }
            }
        }

        public void Read(out Guid guid) {
            Guid g = default;
            Read(&g, 1);
            guid = g;
        }

        public void Write(Guid guid) {
            Write(&guid, 1);
        }

        public void Write<T>(DataList<T> list) where T : unmanaged {
            Write(list.size);
            Write(list.array, list.size);
        }

        public void Read<T>(ref DataList<T> list, Allocator allocator) where T : unmanaged {
            Read(out int size);
            if (allocator != Allocator.None) {
                list = new DataList<T>(size, allocator);
            }

            list.SetSize(size);
            fixed (byte* p = array) {
                TypedUnsafe.MemCpy(list.array, (T*) (p + ptr), size);
            }

            ptr += sizeof(T) * size;

        }

        public byte[] Clone() {
            byte[] retn = new byte[ptr];
            Array.Copy(array, retn, ptr);
            return retn;

        }

    }

}