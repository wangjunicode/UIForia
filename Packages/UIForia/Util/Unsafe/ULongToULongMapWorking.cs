using UIForia.Util;
using UnityEngine;

namespace Tests {

    public class ULongToULongMapWorking {

        private static ulong FREE_KEY = 0;

        public static ulong NO_VALUE = 0;

        /** Keys and values */
        public ulong[] m_data;

        /** Do we have 'free' key in the map? */
        private bool m_hasFreeKey;

        /** Value of 'free' key */
        private ulong m_freeValue;

        /** Fill factor, must be between (0 and 1) */
        private float m_fillFactor;

        /** We will resize a map once it reaches this size */
        private ulong m_threshold;

        /** Current map size */
        private ulong m_size;

        /** Mask to calculate the original position */
        private ulong m_mask;

        private ulong m_mask2;

        private static ulong Hash(ulong x) {
            x = (x ^ (x >> 30)) * 0xbf58476d1ce4e5b9;
            x = (x ^ (x >> 27)) * 0x94d049bb133111eb;
            x ^= (x >> 31);
            return x;
        }

        public static uint ArraySize(int expected, float f) {
            uint ceil = (uint) Mathf.Ceil(expected / f);
            uint powerOfTwo = BitUtil.NextPowerOfTwo(ceil);
            return powerOfTwo;
        }

        public ULongToULongMapWorking(int size, float fillFactor) {
            if (fillFactor <= 0 || fillFactor >= 1)
                fillFactor = 0.75f;
            if (size <= 0) {
                size = 32;
            }

            ulong capacity = ArraySize(size, fillFactor);
            m_mask = capacity - 1;
            m_mask2 = capacity * 2 - 1;
            m_fillFactor = fillFactor;

            m_data = new ulong[capacity * 2];
            m_threshold = (ulong) (capacity * fillFactor);
        }

        public bool TryGetValue(ulong key, out ulong value) {
            ulong ptr = (Hash(key) & m_mask) << 1;

            if (key == FREE_KEY) {
                if (m_hasFreeKey) {
                    value = m_freeValue;
                    return true;
                }

                value = default;
                return false;
            }

            ulong k = m_data[ptr];

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

            while (true) {
                ptr = (ptr + 2) & m_mask2; //that's next index
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

        public ulong Add(ulong key, ulong value) {
            if (key == FREE_KEY) {
                ulong ret = m_freeValue;
                if (!m_hasFreeKey) {
                    ++m_size;
                }

                m_hasFreeKey = true;
                m_freeValue = value;
                return ret;
            }

            ulong ptr = (Hash(key) & m_mask) << 1;
            ulong k = m_data[ptr];
            //end of chain already
            if (k == FREE_KEY) {
                m_data[ptr] = key;
                m_data[ptr + 1] = value;
                if (m_size >= m_threshold) {
                    rehash((ulong) m_data.LongLength * 2); //size is set inside
                }
                else {
                    ++m_size;
                }

                return NO_VALUE;
            }

            //we check FREE prior to this call
            if (k == key) {
                ulong ret = m_data[ptr + 1];
                m_data[ptr + 1] = value;
                return ret;
            }

            while (true) {
                ptr = (ptr + 2) & m_mask2; //that's next index calculation
                k = m_data[ptr];
                if (k == FREE_KEY) {
                    m_data[ptr] = key;
                    m_data[ptr + 1] = value;
                    if (m_size >= m_threshold)
                        rehash((ulong) m_data.LongLength * 2); //size is set inside
                    else
                        ++m_size;
                    return NO_VALUE;
                }

                if (k == key) {
                    ulong ret = m_data[ptr + 1];
                    m_data[ptr + 1] = value;
                    return ret;
                }
            }
        }

        public ulong Remove(ulong key) {
            if (key == FREE_KEY) {
                if (!m_hasFreeKey)
                    return NO_VALUE;
                m_hasFreeKey = false;
                --m_size;
                return m_freeValue; //value is not cleaned
            }

            ulong ptr = (Hash(key) & m_mask) << 1;
            ulong k = m_data[ptr];
            if (k == key) //we check FREE prior to this call
            {
                ulong res = m_data[ptr + 1];
                shiftKeys(ptr);
                --m_size;
                return res;
            }
            else if (k == FREE_KEY)
                return NO_VALUE; //end of chain already

            while (true) {
                ptr = (ptr + 2) & m_mask2; //that's next index calculation
                k = m_data[ptr];
                if (k == key) {
                    ulong res = m_data[ptr + 1];
                    shiftKeys(ptr);
                    --m_size;
                    return res;
                }
                else if (k == FREE_KEY)
                    return NO_VALUE;
            }
        }

        private ulong shiftKeys(ulong pos) {
            // Shift entries with the same hash.
            ulong last, slot;
            ulong k;
            ulong[] data = this.m_data;
            while (true) {
                pos = ((last = pos) + 2) & m_mask2;
                while (true) {
                    if ((k = data[pos]) == FREE_KEY) {
                        data[last] = FREE_KEY;
                        return last;
                    }

                    slot = (Hash(k) & m_mask) << 1; //calculate the starting slot for the current key
                    if (last <= pos ? last >= slot || slot > pos : last >= slot && slot > pos) break;
                    pos = (pos + 2) & m_mask2; //go to the next entry
                }

                data[last] = k;
                data[last + 1] = data[pos + 1];
            }
        }

        public int size() {
            return (int) m_size;
        }

        private void rehash(ulong newCapacity) {
            m_threshold = (ulong) ((newCapacity / 2) * m_fillFactor);
            m_mask = newCapacity / 2 - 1;
            m_mask2 = newCapacity - 1;

            ulong oldCapacity = (ulong) m_data.LongLength;
            ulong[] oldData = m_data;

            m_data = new ulong[newCapacity];
            m_size = m_hasFreeKey ? 1ul : 0;

            for (ulong i = 0; i < oldCapacity; i += 2) {
                ulong oldKey = oldData[i];
                if (oldKey != FREE_KEY)
                    Add(oldKey, oldData[i + 1]);
            }
        }

    }

}