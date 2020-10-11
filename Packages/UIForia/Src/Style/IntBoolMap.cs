using JetBrains.Annotations;
using UIForia.Util;

namespace UIForia.Style {

    public unsafe struct IntBoolMap {

        // ReSharper disable once UnassignedField.Global
        public readonly uint* map;
        public readonly int size;

        public IntBoolMap(uint* map, int bitCount) {
            this.map = map;
            this.size = bitCount >> 4;
        }

        public static bool Get(uint* ptr, int idx) {
            // then using that number, multiply by bit position to generate mask
            int mapIdx = idx >> 5; // divide by 32
            int shift = (idx - (mapIdx << 5)); // multiply by 32
            return (ptr[mapIdx] & ((1u << shift))) != 0;
        }

        public static void Set(uint* map, int idx, bool value) {
            int mapIdx = idx >> 5;
            int shift = (idx - (mapIdx << 5));
            if (value) {
                map[mapIdx] |= (1u << shift);
            }
            else {
                map[mapIdx] &= ~(1u << shift);
            }
        }

        [PublicAPI]
        public bool this[int idx] {
            get {
                // >> 5 divides by 32
                // << 5 multiplies by 32
                // want to figure out which index to use
                // index map by dividing index by 32 (integer division)
                // then using that number, multiply by bit position to generate mask
                int mapIdx = idx >> 5; // divide by 32
                int shift = (idx - (mapIdx << 5)); // multiply by 32
                return (map[mapIdx] & (1u << shift)) != 0;
            }
            set {
                int mapIdx = idx >> 5;
                int shift = (idx - (mapIdx << 5));
                if (value) {
                    map[mapIdx] |= (1u << shift);
                }
                else {
                    map[mapIdx] &= ~(1u << shift);
                }
            }
        }

        public bool this[uint idx] {
            get { return this[(int) idx]; }
            set { this[(int) idx] = value; }
        }

        public bool this[ushort idx] {
            get { return this[(int) idx]; }
            set { this[(int) idx] = value; }
        }

        // count # of set bits in all map entries
        public int Occupancy {
            get => BitUtil.CountSetBits(map, size);
        }

        public bool TrySetIndex(int index) {

            int mapIdx = index >> 5; // divide by 32
            int shift = (index - (mapIdx << 5)); // multiply by 32

            if ((map[mapIdx] & (1u << shift)) != 0) {
                return false;
            }

            map[mapIdx] |= (1u << shift);

            return true;
        }

        public void Clear() {
            for (int i = 0; i < size; i++) {
                map[i] = 0;
            }
        }

        public void Add(int idx) {
            int mapIdx = idx >> 5;
            int shift = (idx - (mapIdx << 5));
            map[mapIdx] |= (1u << shift);
        }

        public void Remove(int idx) {
            int mapIdx = idx >> 5;
            int shift = (idx - (mapIdx << 5));
            map[mapIdx] &= ~(1u << shift);
        }

        public void SetIndex(ushort idx) {
            int mapIdx = idx >> 5;
            int shift = (idx - (mapIdx << 5));
            map[mapIdx] |= (1u << shift);
        }
        
        public void UnsetIndex(ushort idx) {
            int mapIdx = idx >> 5;
            int shift = (idx - (mapIdx << 5));
            map[mapIdx] &= ~(1u << shift);
        }

    }

}