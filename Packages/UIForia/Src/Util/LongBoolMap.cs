using System.Diagnostics;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia {

    public unsafe struct PropertyMapDebugView {

        public int size;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public PropertyId[] properties;

        public PropertyMapDebugView(PropertyMap map) {
            this.size = map.size;
            int cnt = 0;
            for (int i = 0; i < size; i++) {
                cnt += math.countbits(map.map[i]);
            }

            properties = new PropertyId[cnt];
            cnt = 0;
            for (int i = 0; i < size; i++) {
                long p = (long) map.map[i];
                while (p != 0) {
                    PropertyId propertyId = (i * 64) + math.tzcnt((ulong) p);
                    properties[cnt++] = propertyId;
                    p ^= p & -p;
                }
            }
        }

    }
    
    public unsafe struct LongBoolMapEnumerator {

        private long* ptr;
        private int index;
        private int maxIndex;
        private long currentPtrValue;
        
        public int Current { get; private set; }

        public LongBoolMapEnumerator GetEnumerator() => this;

        public LongBoolMapEnumerator(ulong * map, int size) {
            index = 0;
            ptr = (long*) map;
            currentPtrValue = ptr[0];
            maxIndex = size;
            Current = default;
        }

        public bool MoveNext() {

            while (index < maxIndex) {
                if (currentPtrValue != 0) {
                    int result = (index * 64) + math.tzcnt((ulong) currentPtrValue);
                    currentPtrValue ^= currentPtrValue & -currentPtrValue;
                    Current = result;
                    return true;
                }
                index++;
                currentPtrValue = ptr[index];
            }

            return false;
        }
        
    }

    /// <summary>
    /// This is the same as <see cref="LongBoolMap"/> except it provides a debug view for PropertyIds
    /// </summary>
    [DebuggerTypeProxy(typeof(PropertyMapDebugView))]
    public unsafe struct PropertyMap {

        // ReSharper disable once UnassignedField.Global
        public ulong* map;
        public readonly int size;

        public PropertyMap(ulong* map, int size) {
            this.map = map;
            this.size = size;
        }

        public static explicit operator LongBoolMap(PropertyMap map) {
            return new LongBoolMap(map.map, map.size);
        }

        public static explicit operator PropertyMap(LongBoolMap map) {
            return new PropertyMap(map.map, map.size);
        }

        public static bool Get(ulong* ptr, int idx) {
            int mapIdx = idx >> 6; // divide by 64
            int shift = (idx - (mapIdx << 6)); // multiply by 64
            return (ptr[mapIdx] & ((1ul << shift))) != 0;
        }

        public static void Set(ulong* map, int idx) {
            int mapIdx = idx >> 6;
            int shift = (idx - (mapIdx << 6));
            map[mapIdx] |= (1ul << shift);
        }

        public bool Get(int idx) {
            int mapIdx = idx >> 6; // divide by 64
            int shift = (idx - (mapIdx << 6)); // multiply by 64
            return (map[mapIdx] & ((1ul << shift))) != 0;
        }

        public void Set(int idx) {
            int mapIdx = idx >> 6;
            int shift = (idx - (mapIdx << 6));
            map[mapIdx] |= (1ul << shift);
        }

        public void Unset(int idx) {
            int mapIdx = idx >> 6;
            int shift = (idx - (mapIdx << 6));
            map[mapIdx] &= ~(1ul << shift);
        }

        public bool this[int idx] {
            get {
                // >> 6 divides by 64
                // << 6 multiplies by 64
                // want to figure out which index to use
                // index map by dividing index by 64 (integer division)
                // then using that number, multiply by bit position to generate mask
                int mapIdx = idx >> 6; // divide by 64
                int shift = (idx - (mapIdx << 6)); // multiply by 64
                return (map[mapIdx] & (1ul << shift)) != 0;
            }
            set {
                int mapIdx = idx >> 6;
                int shift = (idx - (mapIdx << 6));
                if (value) {
                    map[mapIdx] |= (1ul << shift);
                }
                else {
                    map[mapIdx] &= ~(1ul << shift);
                }
            }
        }

        public bool TrySetIndex(int index) {

            int mapIdx = index >> 6; // divide by 64
            int shift = (index - (mapIdx << 6)); // multiply by 64

            if ((map[mapIdx] & (1ul << shift)) != 0) {
                return false;
            }

            map[mapIdx] |= (1ul << shift);

            return true;
        }

        public void Clear() {
            UnsafeUtility.MemClear(map, sizeof(long) * size);
        }

        public int PopCount() {
            int retn = 0;
            for (int i = 0; i < size; i++) {
                retn += math.countbits(map[i]);
            }

            return retn;
        }
        
        public void ExclusiveOr(PropertyMap input, ref PropertyMap result) {
            for (int b = 0; b < size; b++) {
                result.map[b] = input.map[b] ^ map[b];
            }

        }

        public void And(PropertyMap input, ref PropertyMap result) {
            for (int b = 0; b < size; b++) {
                result.map[b] = input.map[b] & map[b];
            }

        }


    }

    internal sealed class LongBoolMapDebugView {

        public int[] items;
        public int popCount;
        public int mapSize;
        public LongBoolMapDebugView(LongBoolMap longBoolMap) {
            items = longBoolMap.ToArray();
            popCount = longBoolMap.PopCount();
            mapSize = longBoolMap.size;
        }

    }

    [DebuggerDisplay("LongCount = {size}, PopCount = {PopCount()}")]
    [DebuggerTypeProxy(typeof(LongBoolMapDebugView))]
    public unsafe struct LongBoolMap {

        // ReSharper disable once UnassignedField.Global
        [NativeDisableUnsafePtrRestriction] public ulong* map;
        public readonly int size;

        public LongBoolMap(ulong* map, int size) {
            this.map = map;
            this.size = size;
        }

        public static bool Get(ulong* ptr, int idx) {
            int mapIdx = idx >> 6; // divide by 64
            int shift = (idx - (mapIdx << 6)); // multiply by 64
            return (ptr[mapIdx] & ((1ul << shift))) != 0;
        }

        public static void Set(ulong* map, int idx) {
            int mapIdx = idx >> 6;
            int shift = (idx - (mapIdx << 6));
            map[mapIdx] |= (1ul << shift);
        }

        public bool Get(int idx) {
            int mapIdx = idx >> 6; // divide by 64
            int shift = (idx - (mapIdx << 6)); // multiply by 64
            return (map[mapIdx] & ((1ul << shift))) != 0;
        }

        public void Set(int idx) {
            int mapIdx = idx >> 6;
            int shift = (idx - (mapIdx << 6));
            map[mapIdx] |= (1ul << shift);
        }

        public void Unset(int idx) {
            int mapIdx = idx >> 6;
            int shift = (idx - (mapIdx << 6));
            map[mapIdx] &= ~(1ul << shift);
        }

        public bool this[int idx] {
            get {
                // >> 6 divides by 64
                // << 6 multiplies by 64
                // want to figure out which index to use
                // index map by dividing index by 64 (integer division)
                // then using that number, multiply by bit position to generate mask
                int mapIdx = idx >> 6; // divide by 64
                int shift = (idx - (mapIdx << 6)); // multiply by 64
                return (map[mapIdx] & (1ul << shift)) != 0;
            }
            set {
                int mapIdx = idx >> 6;
                int shift = (idx - (mapIdx << 6));
                if (value) {
                    map[mapIdx] |= (1ul << shift);
                }
                else {
                    map[mapIdx] &= ~(1ul << shift);
                }
            }
        }

        public bool TrySetIndex(int index) {

            int mapIdx = index >> 6; // divide by 64
            int shift = (index - (mapIdx << 6)); // multiply by 64

            if ((map[mapIdx] & (1ul << shift)) != 0) {
                return false;
            }

            map[mapIdx] |= (1ul << shift);

            return true;
        }

        public static bool TrySetIndex(ulong* map, int index) {

            int mapIdx = index >> 6; // divide by 64
            int shift = (index - (mapIdx << 6)); // multiply by 64

            if ((map[mapIdx] & (1ul << shift)) != 0) {
                return false;
            }

            map[mapIdx] |= (1ul << shift);

            return true;
        }

        public int PopCount() {
            int retn = 0;
            for (int i = 0; i < size; i++) {
                retn += math.countbits(map[i]);
            }

            return retn;
        }

        public void Clear() {
            UnsafeUtility.MemClear(map, sizeof(long) * size);
        }

        public LongBoolMapEnumerator GetEnumerator() {
            return new LongBoolMapEnumerator(map, size);
        }

        public static int GetMapSize(int itemCount) {
            return MathUtil.Nearest64(itemCount) / 64;
        }

        public int[] ToArray() {
            int listSize = PopCount();
            int[] retn = new int[listSize];
            int idx = 0;
            for (int i = 0; i < size; i++) {
                long value = (long) map[i];
                while (value != 0) {
                    // x & -x returns an integer with only the lsb of the value set to 1
                    long t = value & -value;
                    int tzcnt = math.tzcnt((ulong) value); // counts the number of trailing 0s to lsb
                    value ^= t; // toggle the target bit off
                    retn[idx++] = (i * 64) + tzcnt;
                }
            }

            return retn;
        }

        public bool ContainsAny(LongBoolMap compareTags) {
            for (int i = 0; i < size; i++) {
                if ((map[i] & compareTags.map[i]) != 0) {
                    return true;
                }
            }

            return false;
        }

        public BumpList<int> ToBumpList(BumpAllocator* allocator) {
            int listSize = PopCount();

            if (listSize == 0) {
                return default;
            }

            BumpList<int> retn = allocator->AllocateList<int>(listSize);

            int idx = 0;
            for (int i = 0; i < size; i++) {
                long value = (long) map[i];
                while (value != 0) {
                    // x & -x returns an integer with only the lsb of the value set to 1
                    long t = value & -value;
                    int tzcnt = math.tzcnt((ulong) value); // counts the number of trailing 0s to lsb
                    value ^= t; // toggle the target bit off
                    retn.array[idx++] = (i * 64) + tzcnt;
                }
            }

            retn.size = listSize;
            return retn;
        }

        public CheckedArray<int> FillBuffer(int* usedPropertyBuffer, int popCount) {
            int idx = 0;
            for (int i = 0; i < size; i++) {
                long value = (long) map[i];
                while (value != 0) {
                    // x & -x returns an integer with only the lsb of the value set to 1
                    long t = value & -value;
                    int tzcnt = math.tzcnt((ulong) value); // counts the number of trailing 0s to lsb
                    value ^= t; // toggle the target bit off
                    usedPropertyBuffer[idx++] = (i * 64) + tzcnt;
                }
            }

            return new CheckedArray<int>(usedPropertyBuffer, popCount);
        }

    }

}