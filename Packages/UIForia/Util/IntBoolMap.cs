using System;
using JetBrains.Annotations;

namespace UIForia.Util {

    public unsafe struct BitBuffer512 {

        public fixed uint data[16];

        public uint* ptr {
            get {
                fixed (uint* p = data) {
                    return p;
                }
            }
        }

        public void Clear() {
            this = default;
        }

        public int CountSetBits() {
            fixed (uint* map = data) {
                return BitUtil.CountSetBits(map, 16);
            }
        }

    }

    public unsafe struct BitBuffer256 {

        public fixed uint data[8];

        public uint* ptr {
            get {
                fixed (uint* p = data) {
                    return p;
                }
            }
        }

        public void Clear() {
            this = default;
        }

        public int CountSetBits() {
            fixed (uint* map = data) {
                return BitUtil.CountSetBits(map, 8);
            }
        }

    }

    public unsafe struct BitBuffer128 {

        public fixed uint data[4];

        public uint* ptr {
            get {
                fixed (uint* p = data) {
                    return p;
                }
            }
        }

        public void Clear() {
            this = default;
        }

        public int CountSetBits() {
            fixed (uint* map = data) {
                return BitUtil.CountSetBits(map, 4);
            }
        }
        
    }

    public unsafe struct BitBuffer64 {

        public fixed uint data[2];

        public uint* ptr {
            get {
                fixed (uint* p = data) {
                    return p;
                }
            }
        }

        public void Clear() {
            this = default;
        }
        
        public int CountSetBits() {
            fixed (uint* map = data) {
                return BitUtil.CountSetBits(map, 2);
            }
        }

    }

    public unsafe struct IntBoolMap {

        // ReSharper disable once UnassignedField.Global
        public readonly uint* map;
        public readonly int size;

        public IntBoolMap(uint* map, int size) {
            this.map = map;
            this.size = size;
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

        [PublicAPI]
        public bool this[uint idx] {
            get { return this[(int) idx]; }
            set { this[(int) idx] = value; }
        }

        [PublicAPI]
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

    }

    public unsafe struct IntBoolMap512 {

        public fixed uint map[512];

        public bool this[int idx] {
            get {
                fixed (uint* ptr = map) {
                    return IntBoolMap.Get(ptr, idx);
                }
            }
            set {
                fixed (uint* ptr = map) {
                    IntBoolMap.Set(ptr, idx, value);
                }
            }
        }

    }

    public unsafe struct IntBoolMap128 {

        // ReSharper disable once UnassignedField.Global
        public fixed uint map[4];

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
                return (map[mapIdx] & ((1u << shift))) != 0;
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

        [PublicAPI]
        public bool this[uint idx] {
            get { return this[(int) idx]; }
            set { this[(int) idx] = value; }
        }

        [PublicAPI]
        public bool this[ushort idx] {
            get { return this[(int) idx]; }
            set { this[(int) idx] = value; }
        }

        // count # of set bits in all map entries
        public uint Occupancy {
            get {
                uint count = 0;
                uint i = map[0];

                i -= ((i >> 1) & 0x55555555);
                i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
                count += (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;

                i = map[1];
                i -= ((i >> 1) & 0x55555555);
                i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
                count += (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;

                i = map[2];
                i -= ((i >> 1) & 0x55555555);
                i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
                count += (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;

                i = map[3];
                i -= ((i >> 1) & 0x55555555);
                i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
                count += (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
                return count;
            }
        }

        public bool TrySetIndex(int index) {
            if (this[index]) {
                return false;
            }

            this[index] = true;

            return true;
        }

    }

}