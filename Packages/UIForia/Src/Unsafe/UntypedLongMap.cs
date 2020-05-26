using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia.Util.Unsafe {

    public unsafe struct UnmanagedLongMapDebugView<TValue> where TValue : unmanaged {

        public int size;
        public int capacity;
        public KeyValuePair<long, TValue>[] data;
        
        public UnmanagedLongMapDebugView(UnmanagedLongMap<TValue> target) {
            if (target.mapState == null) {
                size = default;
                capacity = default;
                data = default;
                return;
            }

            size = target.mapState->GetSize();
            capacity = target.mapState->GetCapacity();
            data = new KeyValuePair<long, TValue>[size];
            target.mapState->CopyKeyValuePairs<TValue>(data);
        }

    }

    [DebuggerTypeProxy(typeof(UnmanagedLongMapDebugView<>))]
    public unsafe struct UnmanagedLongMap<TValue> : IDisposable where TValue : unmanaged {

        internal readonly UntypedLongMap* mapState;

        public UnmanagedLongMap(UntypedLongMap* mapState) {
            this.mapState = mapState->GetItemSize() == sizeof(TValue)
                ? mapState
                : default;
        }

        public UnmanagedLongMap(int initialCapacity, Allocator allocator, float fillFactor = 0.75f) {
            this.mapState = UntypedLongMap.Create<TValue>(initialCapacity, fillFactor, allocator);
        }

        public void Dispose() {
            if (mapState != null) {
                mapState->Dispose();
            }

            this = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(long key, in TValue value) {
            mapState->Add<TValue>(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(long key, out TValue value) {
            return mapState->TryGetValue<TValue>(key, out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(long key) {
            return mapState->Remove<TValue>(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(long key, out TValue oldValue) {
            return mapState->TryRemove<TValue>(key, out oldValue);
        }

        public int size {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => mapState->GetSize();
        }

        public bool TryAddValue(long key, TValue value) {
            if (mapState->TryGetValue<TValue>(key, out TValue _)) {
                return false;
            }

            mapState->Add<TValue>(key, value);
            return true;
        }

        [BurstDiscard]
        public void CopyKeyValuePairs(KeyValuePair<long, TValue>[] data) {
            mapState->CopyKeyValuePairs<TValue>(data);
        }

        public void Clear() {
            mapState->Clear();
        }

    }

    public unsafe struct UntypedLongMap : IDisposable {

        private const long k_FreeKey = 0;

        /** Keys and values */
        [NativeDisableUnsafePtrRestriction] private byte* m_data;

        /** Do we have 'free' key in the map? */
        private bool m_hasFreeKey;

        /** We will resize a map once it reaches this size */
        /** Fill factor, must be between (0 and 1) */
        private float fillFactor;

        private uint capacity;
        private uint size;

        private Allocator allocator;

        private int itemSize; // sizeof(long) + sizeof(value)

        private static ulong Hash(long z) {
            ulong x = (ulong) z;
            x = (x ^ (x >> 30)) * 0xbf58476d1ce4e5b9;
            x = (x ^ (x >> 27)) * 0x94d049bb133111eb;
            x ^= (x >> 31);
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize() {
            return (int) size;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCapacity() {
            return (int) capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetItemSize() {
            return itemSize - sizeof(int);
        }

        public static UntypedLongMap* Create<T>(int initialCapacity, float fillFactor, Allocator allocator) where T : unmanaged {
            if (initialCapacity <= 0) {
                initialCapacity = 32;
            }

            // the extra data for freeValue needs to be stored somewhere, store it right after the data for the map
            UntypedLongMap* retn = (UntypedLongMap*) UnsafeUtility.Malloc(sizeof(UntypedLongMap) + sizeof(T), 4, allocator);
            *retn = default; // clear for safety
            retn->itemSize = sizeof(long) + sizeof(T);
            retn->capacity = BitUtil.NextPowerOfTwo((uint) math.ceil(initialCapacity / fillFactor));
            retn->fillFactor = fillFactor;
            retn->allocator = allocator;

            // the extra data for freeValue needs to be stored somewhere, tack it to start of the data array
            long byteCount = (sizeof(long) + sizeof(T)) * retn->capacity;
            retn->m_data = (byte*) UnsafeUtility.Malloc(byteCount, UnsafeUtility.AlignOf<long>(), allocator);
            UnsafeUtility.MemClear(retn->m_data, byteCount);
            retn->size = 0;

            // free value is stored right after the retn pointer because it's size and type is unknown. 
            // (we cannot store it in the UntypedMap because it is not known at compile time and UntypedMap cannot be generic
            // because then it would not be an unmanaged type what we could get a pointer to)
            
            // this does mean we cannot have UntypedLongMap without making it a pointer. todo -- improve this, better to tack it onto the data pointer
            T* freeValue = (T*) (retn + 1);
            *freeValue = default;
            return retn;
        }

        public void Clear() {
            m_hasFreeKey = false;
            size = 0;
            UnsafeUtility.MemClear(m_data, itemSize * capacity);
        }

        public bool Contains<T>(long key) where T : unmanaged {
            return TryGetValue(key, out T _);
        }

        public bool TryGetValue<T>(long key, out T value) where T : unmanaged {

            // // todo -- wrap in safety if-def
            // if (sizeof(T) != itemSize - sizeof(long)) {
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

            long* c = (long*) (m_data + (idx * itemSize));

            if (*c == k_FreeKey) {
                value = default;
                return false;
            }

            if (*c == key) {
                value = *(T*) (c + 1);
                return true;
            }

            while (true) {
                idx = GetNextIndex(idx);
                c = (long*) (m_data + (idx * itemSize));
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
            return (T*) (((UntypedLongMap*) UnsafeUtility.AddressOf(ref this)) + 1);
        }

        [BurstDiscard]
        public int CopyKeyValuePairs<T>(KeyValuePair<long, T>[] kvpList) where T: unmanaged {

            int idx = 0;
            for (int i = 0; i < capacity; i++) {
                long* c = (long*) (m_data + (i * itemSize));
                if (*c == k_FreeKey) {
                    continue;
                }
                kvpList[idx++] = new KeyValuePair<long, T>(*c, *(T*)(c + 1)); 
            }

            if (m_hasFreeKey) {
                kvpList[idx++] = new KeyValuePair<long, T>(k_FreeKey, *GetFreeValue<T>()); 
            }

            return idx;
        }

        public T Add<T>(long key, in T value) where T : unmanaged {
            if (key == k_FreeKey) {

                T* pFreeValue = GetFreeValue<T>();

                if (m_hasFreeKey) {
                    T ret = *pFreeValue;
                    *pFreeValue = value;
                    return ret;
                }

                size++;
                m_hasFreeKey = true;
                *pFreeValue = value;
                return default;

            }

            uint idx = GetStartIndex(key);
            long* c = (long*) (m_data + (idx * itemSize));

            //end of chain already
            if (*c == k_FreeKey) {
                *c = key;
                T* typedEntry = (T*) (c + 1);
                *typedEntry = value;
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
                return retn;
            }

            while (true) {
                idx = GetNextIndex(idx);
                c = (long*) (m_data + (idx * itemSize));
                if (*c == k_FreeKey) {
                    *c = key;
                    T* typedEntry = (T*) (c + 1);
                    *typedEntry = value;
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
                    return retn;
                }
            }
        }

        public bool TryRemove<T>(long key, out T prevValue) where T : unmanaged {
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
            long* c = (long*) (m_data + (idx * itemSize));

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
                c = (long*) (m_data + (idx * itemSize));

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
        public bool Remove<T>(long key) where T : unmanaged {
            return TryRemove(key, out T _);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove<T>(long key, out T value) where T : unmanaged {
            return TryRemove(key, out value);
        }

        private void ShiftKeys<T>(uint pos) where T : unmanaged {
            // Shift entries with the same hash.

            while (true) {
                uint last = pos;
                pos = (last + 1) & (capacity - 1);
                while (true) {

                    long* k = (long*) (m_data + (pos * itemSize));

                    if (*k == k_FreeKey) {
                        long* pLast = (long*) (m_data + (last * itemSize));
                        *pLast = k_FreeKey;
                        return;
                    }

                    uint slot = GetStartIndex(*k);

                    if (last <= pos ? last >= slot || slot > pos : last >= slot && slot > pos) {
                        break;
                    }

                    pos = (pos + 1) & (capacity - 1); //go to the next entry
                }

                long* pLastIndex = (long*) (m_data + (last * itemSize));
                long* pPosIndex = (long*) (m_data + (pos * itemSize));
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

            m_data = (byte*) UnsafeUtility.Malloc(capacity * itemSize, UnsafeUtility.AlignOf<long>(), allocator);

            UnsafeUtility.MemClear(m_data, capacity * itemSize);

            size = m_hasFreeKey ? 1u : 0;

            for (uint i = 0; i < oldCapacity; i++) {
                long* oldKey = (long*) (oldData + (i * itemSize));
                if (*oldKey != k_FreeKey) {
                    Add(*oldKey, *(T*) (oldKey + 1)); // todo -- almost certainly a better way to do this in bulk
                }
            }

            UnsafeUtility.Free(oldData, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetStartIndex(long key) {
            return (uint) (Hash(key) & (capacity - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetNextIndex(uint currentIndex) {
            return (currentIndex + 1) & (capacity - 1);
        }

        public void Dispose() {
            if (m_data != null) {
                UnsafeUtility.Free(m_data, allocator);
            }

            this = default;
        }

    }

}