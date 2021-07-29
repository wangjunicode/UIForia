using System.Diagnostics;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia.Style {

    /// <summary>
    /// This is the same as <see cref="LongBoolMap"/> except it provides a debug view for ElementIds (without generation info)
    /// </summary>
    [DebuggerTypeProxy(typeof(ElementMapDebugView))]
    internal unsafe struct ElementMap {

        [NativeDisableUnsafePtrRestriction] public ulong* map;
        public int longCount;

        public ElementMap(ulong* map, int longCount) {
            this.map = map;
            this.longCount = longCount;
        }

        public bool Get(ElementId elementId) {
            int idx = elementId.index;
            int mapIdx = idx >> 6; // divide by 64
            int shift = (idx - (mapIdx << 6)); // multiply by 64
            return (map[mapIdx] & ((1ul << shift))) != 0;
        }

        public int GetValue(ElementId elementId) {
            int idx = elementId.index;
            int mapIdx = idx >> 6; // divide by 64
            int shift = (idx - (mapIdx << 6)); // multiply by 64
            return (map[mapIdx] & ((1ul << shift))) == 0 ? 0 : 1;
        }

        public void Set(ElementId elementId) {
            int idx = elementId.index;
            int mapIdx = idx >> 6;
            int shift = (idx - (mapIdx << 6));
            map[mapIdx] |= (1ul << shift);
        }

        public void Set(int elementId) {
            int idx = elementId;
            int mapIdx = idx >> 6;
            int shift = (idx - (mapIdx << 6));
            map[mapIdx] |= (1ul << shift);
        }

        public void Unset(ElementId elementId) {
            int idx = elementId.index;
            int mapIdx = idx >> 6;
            int shift = (idx - (mapIdx << 6));
            map[mapIdx] &= ~(1ul << shift);
        }

        public bool TrySet(ElementId elementId) {
            int idx = elementId.index;
            int mapIdx = idx >> 6;
            int shift = (idx - (mapIdx << 6));

            if ((map[mapIdx] & (1ul << shift)) == 0) {
                map[mapIdx] |= (1ul << shift);
                return true;
            }

            return false;
        }

        public static explicit operator LongBoolMap(ElementMap map) {
            return new LongBoolMap(map.map, map.longCount);
        }

        public static explicit operator ElementMap(LongBoolMap map) {
            return new ElementMap(map.map, map.size);
        }

        public int PopCount() {
            int retn = 0;
            for (int i = 0; i < longCount; i++) {
                retn += math.countbits(map[i]);
            }

            return retn;
        }

        public TempList<ElementId> ToTempList(Allocator allocator) {
            int listSize = PopCount();

            if (listSize == 0) {
                return default;
            }

            TempList<ElementId> retn = TypedUnsafe.MallocUnsizedTempList<ElementId>(listSize, allocator);
            int idx = 0;
            for (int i = 0; i < longCount; i++) {
                long value = (long) map[i];
                while (value != 0) {
                    // x & -x returns an integer with only the lsb of the value set to 1
                    long t = value & -value;
                    int tzcnt = math.tzcnt((ulong) value); // counts the number of trailing 0s to lsb
                    value ^= t; // toggle the target bit off
                    retn.array[idx++] = new ElementId((i * 64) + tzcnt);
                }
            }

            retn.size = listSize;
            return retn;
        }

        public BumpList<ElementId> ToBumpList(BumpAllocator* allocator) {
            int listSize = PopCount();

            if (listSize == 0) {
                return default;
            }

            BumpList<ElementId> retn = allocator->AllocateList<ElementId>(listSize);

            int idx = 0;
            for (int i = 0; i < longCount; i++) {
                long value = (long) map[i];
                while (value != 0) {
                    // x & -x returns an integer with only the lsb of the value set to 1
                    long t = value & -value;
                    int tzcnt = math.tzcnt((ulong) value); // counts the number of trailing 0s to lsb
                    value ^= t; // toggle the target bit off
                    retn.array[idx++] = new ElementId((i * 64) + tzcnt);
                }
            }

            retn.size = listSize;
            return retn;
        }

        public TempList<int> ToTempListInt(Allocator allocator) {
            int listSize = PopCount();

            if (listSize == 0) {
                return default;
            }

            TempList<int> retn = TypedUnsafe.MallocUnsizedTempList<int>(listSize, allocator);
            int idx = 0;
            for (int i = 0; i < longCount; i++) {
                long value = (long) map[i];
                while (value != 0) {
                    // x & -x returns an integer with only the lsb of the value set to 1
                    long t = value & -value;
                    int tzcnt = math.tzcnt((ulong) value); // counts the number of trailing 0s to lsb
                    value ^= t; // toggle the target bit off
                    int elementId = (i * 64) + tzcnt;
                    retn.array[idx++] = elementId;
                }
            }

            retn.size = listSize;
            return retn;
        }

        public void CombineThreadSafe(ElementMap combine) {
            // CompareExchange only works on longs
            long* castMap = (long*) map;
            for (int i = 0; i < longCount; i++) {
                long initialValue;
                long computedValue;
                do {
                    initialValue = castMap[i];
                    computedValue = (long) (map[i] | combine.map[i]);
                } while (System.Threading.Interlocked.CompareExchange(ref castMap[i], computedValue, initialValue) != initialValue);
            }
        }

        public void Combine(ElementMap other) {
            for (int i = 0; i < longCount; i++) {
                map[i] |= other.map[i];
            }
        }

        public ElementMap Copy(ElementMap other) {
            for (int i = 0; i < longCount; i++) {
                map[i] = other.map[i];
            }

            return this;
        }

        public ElementMap Invert() {
            for (int i = 0; i < longCount; i++) {
                map[i] = ~map[i];
            }

            return this;
        }

        public ElementMap Not(ElementMap other) {
            for (int i = 0; i < longCount; i++) {
                map[i] &= ~other.map[i];
            }

            return this;
        }

        public ElementMap And(ElementMap other) {
            for (int i = 0; i < longCount; i++) {
                map[i] &= other.map[i];
            }

            return this;
        }

        public ElementMap Or(ElementMap other) {
            for (int i = 0; i < longCount; i++) {
                map[i] |= other.map[i];
            }

            return this;
        }

        public void Clear() {
            UnsafeUtility.MemClear(map, sizeof(ulong) * longCount);
        }

    }

    internal unsafe struct ElementMapDebugView {

        public int size;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public ElementId[] properties;

        public ElementMapDebugView(ElementMap map) {
            this.size = map.longCount;
            int cnt = 0;
            for (int i = 0; i < size; i++) {
                cnt += math.countbits(map.map[i]);
            }

            properties = new ElementId[cnt];
            cnt = 0;
            for (int i = 0; i < size; i++) {
                long p = (long) map.map[i];
                while (p != 0) {
                    // generation info is lost
                    ElementId elementId = new ElementId((i * 64) + math.tzcnt((ulong) p));
                    properties[cnt++] = elementId;
                    p ^= p & -p;
                }
            }
        }

    }

}