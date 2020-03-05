using JetBrains.Annotations;

namespace UIForia.Util {

    public unsafe struct IntBoolMap {

        // ReSharper disable once UnassignedField.Global
        public readonly uint* map;
        public readonly int size;
        
        public IntBoolMap(uint* map, int size) {
            this.map = map;
            this.size = size;
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
                if (mapIdx >= size) return false;
                int shift = (idx - (mapIdx << 5)); // multiply by 32
                return (map[mapIdx] & ((uint) (1 << shift))) != 0;
            }
            set {
                int mapIdx = idx >> 5;
                if (mapIdx >= size) return;
                int shift = (idx - (mapIdx << 5));
                if (value) {
                    map[mapIdx] |= (uint) (1 << shift);
                }
                else {
                    map[mapIdx] &= (uint) ~(1 << shift);
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
            get {
                uint count = 0;
                for (int x = 0; x < size; x++) {
                    uint i = map[x];

                    i -= ((i >> 1) & 0x55555555);
                    i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
                    count += (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
                }

                return (int)count;
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
                return (map[mapIdx] & ((uint) (1 << shift))) != 0;
            }
            set {
                int mapIdx = idx >> 5;
                int shift = (idx - (mapIdx << 5));
                if (value) {
                    map[mapIdx] |= (uint) (1 << shift);
                }
                else {
                    map[mapIdx] &= (uint) ~(1 << shift);
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