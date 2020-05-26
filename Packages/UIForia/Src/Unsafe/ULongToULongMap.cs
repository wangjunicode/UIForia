using System;
using UIForia.Util;
using UnityEditor;
using UnityEngine;

namespace Tests {

    public class ULongToULongMap {

        private static long FREE_KEY = 0;


        /** Keys and values */
        public long[] m_data;

        /** Do we have 'free' key in the map? */
        private bool m_hasFreeKey;

        /** Value of 'free' key */
        private long m_freeValue;

        /** Fill factor, must be between (0 and 1) */
        private float m_fillFactor;

        /** We will resize a map once it reaches this size */
        private uint m_threshold;

        private uint capacity;

        /** Current map size */
        private int m_size;

        private static ulong Hash(long z) {
            ulong x = (ulong) z;
            x = (x ^ (x >> 30)) * 0xbf58476d1ce4e5b9;
            x = (x ^ (x >> 27)) * 0x94d049bb133111eb;
            x ^= (x >> 31);
            return x;
        }

        public static uint ArraySize(int expected, float f) {
            uint ceil = (uint) Mathf.Ceil(expected / f);
            return BitUtil.NextPowerOfTwo(ceil);
        }

        public ULongToULongMap(int initialCapacity, float fillFactor) {
            if (fillFactor <= 0 || fillFactor >= 1) {
                fillFactor = 0.75f;
            }

            if (initialCapacity <= 0) {
                initialCapacity = 32;
            }

            capacity = ArraySize(initialCapacity, fillFactor);
            // m_mask = capacity - 1;
            // m_mask2 = capacity * 2 - 1;
            m_fillFactor = fillFactor;

            m_data = new long[capacity * 2];
            m_threshold = (uint) (capacity * fillFactor);
        }

        public bool TryGetValue(long key, out long value) {
            ulong mask = capacity - 1ul;

            ulong ptr = (Hash(key) & mask) << 1;

            if (key == FREE_KEY) {
                if (m_hasFreeKey) {
                    value = m_freeValue;
                    return true;
                }

                value = default;
                return false;
            }

            long k = m_data[ptr];

            if (k == FREE_KEY) {
                //end of chain already
                value = default;
                return false;
            }

            //we check FREE prior to this call
            if (k == key) {
                value = m_data[ptr + 1];
                return true;
            }

            ulong m_mask2 = (capacity << 1) - 1;

            while (true) {
                ptr = (ptr + 2ul) & m_mask2; //that's next index
                k = m_data[ptr];
                if (k == FREE_KEY) {
                    value = default;
                    return false;
                }

                if (k == key) {
                    value = m_data[ptr + 1];
                    return true;
                }
            }
        }

        public long Add(long key, long value) {
            if (key == FREE_KEY) {
                long ret = m_freeValue;
                if (!m_hasFreeKey) {
                    ++m_size;
                }

                m_hasFreeKey = true;
                m_freeValue = value;
                return ret;
            }

            ulong mask = capacity - 1;
            ulong ptr = (Hash(key) & mask) << 1;

            long k = m_data[ptr];
            //end of chain already
            if (k == FREE_KEY) {
                m_data[ptr] = key;
                m_data[ptr + 1] = value;
                if (m_size >= m_threshold) {
                    Rehash(); //m_data.LongLength * 2); //size is set inside
                }
                else {
                    ++m_size;
                }

                return default;
            }

            //we check FREE prior to this call
            if (k == key) {
                long ret = m_data[ptr + 1];
                m_data[ptr + 1] = value;
                return ret;
            }

            ulong m_mask2 = (capacity << 1) - 1;

            while (true) {
                ptr = (ptr + 2) & m_mask2; //that's next index calculation
                k = m_data[ptr];
                if (k == FREE_KEY) {
                    m_data[ptr] = key;
                    m_data[ptr + 1] = value;
                    if (m_size >= m_threshold) {
                        Rehash(); //size is set inside
                    }
                    else {
                        ++m_size;
                    }

                    return default;
                }

                if (k == key) {
                    long ret = m_data[ptr + 1];
                    m_data[ptr + 1] = value;
                    return ret;
                }
            }
        }

        public long Remove(long key) {
            if (key == FREE_KEY) {
                if (!m_hasFreeKey) {
                    return default;
                }

                m_hasFreeKey = false;
                --m_size;
                return m_freeValue; //value is not cleaned
            }

            ulong mask = capacity - 1;
            ulong mask2 = (capacity << 1) - 1;

            ulong ptr = ((Hash(key) & mask) << 1);
            long k = m_data[ptr];
            //we check FREE prior to this call
            if (k == key) {
                long res = m_data[ptr + 1];
                ShiftKeys(ptr);
                --m_size;
                return res;
            }
            
            if (k == FREE_KEY) {
                return default; //end of chain already
            }

            while (true) {
                ptr = (ptr + 2) & mask2; //that's next index calculation
                k = m_data[ptr];
                if (k == key) {
                    long res = m_data[ptr + 1];
                    ShiftKeys(ptr);
                    --m_size;
                    return res;
                }

                if (k == FREE_KEY) {
                    return default;
                }
            }
        }

        private void ShiftKeys(ulong pos) {
            // Shift entries with the same hash.
            ulong last, slot;
            long k;

            long[] data = this.m_data;

            ulong mask = capacity - 1;
            ulong mask2 = (capacity << 1) - 1;

            while (true) {
                pos = ((last = pos) + 2) & mask2;
                while (true) {
                    if ((k = data[pos]) == FREE_KEY) {
                        data[last] = FREE_KEY;
                        return;
                    }

                    slot = ((Hash(k) & mask) << 1); //calculate the starting slot for the current key

                    if (last <= pos ? last >= slot || slot > pos : last >= slot && slot > pos) {
                        break;
                    }

                    pos = (pos + 2) & mask2; //go to the next entry
                }

                data[last] = k;
                data[last + 1] = data[pos + 1];
            }
        }

        private void Rehash() { 
            uint oldCapacity = capacity;
            capacity <<= 1; // double capacity
            m_threshold = (uint) ((oldCapacity) * m_fillFactor);

            long[] oldData = m_data;

            m_data = new long[capacity * 2]; // array size is always 2s capacity
            m_size = m_hasFreeKey ? 1 : 0;

            for (long i = 0; i < oldCapacity; i += 2) {
                long oldKey = oldData[i];
                if (oldKey != FREE_KEY) {
                    Add(oldKey, oldData[i + 1]);
                }
            }
        }

    }

}