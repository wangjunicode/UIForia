using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia.Util.Unsafe {

    /// <summary>
    /// A hashmap mapping integer keys to any unmanaged struct type
    /// </summary>
    public unsafe struct UntypedIntMap : IDisposable {

        private const int k_FreeKey = 0;

        /** Keys and values */
        [NativeDisableUnsafePtrRestriction] private byte* m_data;

        /** Do we have 'free' key in the map? */
        // this is needed because we need a way to mark slots as free, but 
        // its also possible that the value we picked to mark them as free
        // is in active use as a key. When this is the case we store the 
        // corresponding value at the address of 'this' plus sizeof(this).
        // Because the keys are untyped I cannot declare the free-key value
        // in this struct, so I cheat and compute a pointer to it when it 
        // is needed.
        private bool m_hasFreeKey;

        /// <summary>
        /// Fill factor, must be between (0 and 1). the more full we let the map get the worse our
        ///  performance becomes but the better our memory usage is
        /// </summary>
        private float fillFactor;

        /// <summary>
        /// Always a power of two so I can avoid modulus and division operations
        /// </summary>
        private uint capacity;

        /// <summary>
        /// How many items are in the map 
        /// </summary>
        private int size;

        private Allocator allocator;

        /// <summary>
        /// How big an entire item is in bytes. Equal to sizeof(int) + sizeof(value)
        /// because keys and values are interleaved.
        /// </summary>
        private int itemSize;

        private static uint Hash(int x) {
            x = ((x >> 16) ^ x) * 0x45d9f3b;
            x = ((x >> 16) ^ x) * 0x45d9f3b;
            x = (x >> 16) ^ x;
            return (uint) x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize() {
            return size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetItemSize() {
            return itemSize - sizeof(int);
        }

        public static UntypedIntMap* Create<T>(int initialCapacity, float fillFactor, Allocator allocator) where T : unmanaged {
            if (initialCapacity <= 0) {
                initialCapacity = 32;
            }

            // the extra data for freeValue needs to be stored somewhere, store it right after the data for the map
            UntypedIntMap* retn = (UntypedIntMap*) UnsafeUtility.Malloc(sizeof(UntypedIntMap) + sizeof(T), 4, allocator);
            *retn = default; // clear for safety
            retn->itemSize = sizeof(int) + sizeof(T);
            retn->capacity = NextPowerOfTwo((uint) math.ceil(initialCapacity / fillFactor));
            retn->fillFactor = fillFactor;
            retn->allocator = allocator;

            long byteCount = (sizeof(int) + sizeof(T)) * retn->capacity;
            retn->m_data = (byte*) UnsafeUtility.Malloc(byteCount, UnsafeUtility.AlignOf<int>(), allocator);
            UnsafeUtility.MemClear(retn->m_data, byteCount);
            retn->size = 0;

            // free value is stored right after the retn pointer because it's size and type is unknown. 
            // (we cannot store it in the UntypedMap because it is not known at compile time and UntypedMap cannot be generic
            // because then it would not be an unmanaged type what we could get a pointer to)
            T* freeValue = (T*) (retn + 1);
            *freeValue = default;
            return retn;
        }

        public bool Contains<T>(int key) where T : unmanaged {
            return TryGetValue(key, out T _);
        }

        public T* GetOrCreate<T>(int key) where T : unmanaged {

            if (!TryGetPointer(key, out T* value)) {
                Add(key, default, out value);
            }

            return value;
        }
        
        public bool TryGetPointer<T>(int key, out T* value) where T : unmanaged {
            if (key == k_FreeKey) {

                T* pFreeValue = GetFreeValue<T>();

                if (m_hasFreeKey) {
                    value = pFreeValue;
                    return true;
                }

                value = default;
                return false;
            }

            uint idx = GetStartIndex(key);

            // pointer to a cell's key
            int* currentCell = (int*) (m_data + (idx * itemSize));

            if (*currentCell == k_FreeKey) {
                value = default;
                return false;
            }

            if (*currentCell == key) {
                // values are at offset of currentCell in bytes + sizeof(int)
                // then cast to value type and dereference.
                value = (T*) (currentCell + 1);
                return true;
            }

            while (true) {
                idx = GetNextIndex(idx);
                currentCell = (int*) (m_data + (idx * itemSize));
                if (*currentCell == k_FreeKey) {
                    value = default;
                    return false;
                }

                if (*currentCell == key) {
                    value = (T*) (currentCell + 1);
                    return true;
                }
            }
        }

        public bool TryGetValue<T>(int key, out T value) where T : unmanaged {

            // // todo -- wrap in safety if-def
            // if (sizeof(T) != itemSize - sizeof(int)) {
            //     value = default;
            //     return false;
            // }

            if (key == k_FreeKey) {

                T* pFreeValue = GetFreeValue<T>();

                if (m_hasFreeKey) {
                    value = *pFreeValue;
                    return true;
                }

                value = default;
                return false;
            }

            uint idx = GetStartIndex(key);

            // pointer to a cell's key
            int* c = (int*) (m_data + (idx * itemSize));

            if (*c == k_FreeKey) {
                value = default;
                return false;
            }

            if (*c == key) {
                // values are at offset of c in bytes + sizeof(int)
                // then cast to value type and dereference.
                value = *(T*) (c + 1);
                return true;
            }

            while (true) {
                idx = GetNextIndex(idx);
                c = (int*) (m_data + (idx * itemSize));
                if (*c == k_FreeKey) {
                    value = default;
                    return false;
                }

                if (*c == key) {
                    value = *(T*) (c + 1);
                    return true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T* GetFreeValue<T>() where T : unmanaged {
            return (T*) (((UntypedIntMap*) UnsafeUtility.AddressOf(ref this)) + 1);
        }

        public T Add<T>(int key, in T value, out T* ptr) where T : unmanaged {
            if (key == k_FreeKey) {

                T* pFreeValue = GetFreeValue<T>();

                if (m_hasFreeKey) {
                    T ret = *pFreeValue;
                    *pFreeValue = value;
                    ptr = pFreeValue;
                    return ret;
                }

                size++;
                m_hasFreeKey = true;
                *pFreeValue = value;
                ptr = pFreeValue;
                return default;

            }

            uint idx = GetStartIndex(key);
            int* c = (int*) (m_data + (idx * itemSize));

            //end of chain already
            if (*c == k_FreeKey) {
                *c = key;
                T* typedEntry = (T*) (c + 1);
                *typedEntry = value;
                ptr = typedEntry;
                uint threshold = (uint) (capacity * fillFactor);
                if (size >= threshold) {
                    Rehash<T>(); //size is set inside
                }
                else {
                    size++;
                }

                return default;
            }

            //we check FREE prior to this call
            if (*c == key) {
                T* typedEntry = (T*) (c + 1);
                T retn = *typedEntry;
                *typedEntry = value;
                ptr = typedEntry;
                return retn;
            }

            while (true) {
                idx = GetNextIndex(idx);
                c = (int*) (m_data + (idx * itemSize));
                if (*c == k_FreeKey) {
                    *c = key;
                    T* typedEntry = (T*) (c + 1);
                    *typedEntry = value;
                    ptr = typedEntry;
                    uint threshold = (uint) (capacity * fillFactor);
                    if (size >= threshold) {
                        Rehash<T>(); //size is set inside
                    }
                    else {
                        size++;
                    }

                    return default;
                }

                if (*c == key) {
                    T* typedEntry = (T*) (c + 1);
                    T retn = *typedEntry;
                    *typedEntry = value;
                    ptr = typedEntry;
                    return retn;
                }
            }
        }

        public bool TryRemove<T>(int key, out T prevValue) where T : unmanaged {
            if (key == k_FreeKey) {

                if (!m_hasFreeKey) {
                    prevValue = default;
                    return false;
                }

                m_hasFreeKey = false;
                T* pFreeValue = GetFreeValue<T>();
                prevValue = *pFreeValue;
                *pFreeValue = default;
                size--;
                return true;
            }

            uint idx = GetStartIndex(key);
            int* c = (int*) (m_data + (idx * itemSize));

            // end of chain already
            if (*c == k_FreeKey) {
                prevValue = default;
                return false;
            }

            if (*c == key) {
                size--;
                prevValue = *(T*) (c + 1);
                ShiftKeys<T>(idx);
                return true;
            }

            while (true) {
                idx = GetNextIndex(idx);
                c = (int*) (m_data + (idx * itemSize));

                if (*c == k_FreeKey) {
                    prevValue = default;
                    return false;
                }

                if (*c == key) {
                    size--;
                    prevValue = *(T*) (c + 1);
                    ShiftKeys<T>(idx);
                    return true;
                }

            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove<T>(int key) where T : unmanaged {
            return TryRemove(key, out T _);
        }

        private void ShiftKeys<T>(uint pos) where T : unmanaged {
            // Shift entries with the same hash.

            while (true) {
                uint last = pos;
                pos = (last + 1) & (capacity - 1);
                while (true) {

                    int* k = (int*) (m_data + (pos * itemSize));

                    if (*k == k_FreeKey) {
                        int* pLast = (int*) (m_data + (last * itemSize));
                        *pLast = k_FreeKey;
                        return;
                    }

                    uint slot = GetStartIndex(*k);

                    if (last <= pos ? last >= slot || slot > pos : last >= slot && slot > pos) {
                        break;
                    }

                    pos = (pos + 1) & (capacity - 1); //go to the next entry
                }

                int* pLastIndex = (int*) (m_data + (last * itemSize));
                int* pPosIndex = (int*) (m_data + (pos * itemSize));
                T* pLastValue = (T*) (pLastIndex + 1);
                T* pPosValue = (T*) (pPosIndex + 1);

                // could profile using memcpy for this but since data is small i think manual copy is faster by a big margin
                // this just shifts the key and the data down to fill the last slot
                *pLastIndex = *pPosIndex;
                *pLastValue = *pPosValue;
            }
        }

        private void Rehash<T>() where T : unmanaged {

            uint oldCapacity = capacity;

            byte* oldData = m_data;
            capacity <<= 1; // double capacity. its always a power of 2. 

            m_data = (byte*) UnsafeUtility.Malloc(capacity * itemSize, UnsafeUtility.AlignOf<int>(), allocator);

            UnsafeUtility.MemClear(m_data, capacity * itemSize);

            size = m_hasFreeKey ? 1 : 0;

            T* unused;
            for (uint i = 0; i < oldCapacity; i++) {
                int* oldKey = (int*) (oldData + (i * itemSize));
                if (*oldKey != k_FreeKey) {
                    Add(*oldKey, *(T*) (oldKey + 1), out unused); // todo -- almost certainly a better way to do this in bulk
                }
            }

            UnsafeUtility.Free(oldData, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetStartIndex(int key) {
            return Hash(key) & (capacity - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetNextIndex(uint currentIndex) {
            return (currentIndex + 1) & (capacity - 1);
        }

        private static uint NextPowerOfTwo(uint v) {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        public void Dispose() {
            if (m_data != null) {
                UnsafeUtility.Free(m_data, allocator);
            }

            this = default;
        }

    }

}