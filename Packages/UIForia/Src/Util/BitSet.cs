using System;
using System.Diagnostics;
using Unity.Mathematics;

namespace UIForia.Util {

    [DebuggerDisplay("{DebugDisplay()}")]
    public struct BitSet : IEquatable<BitSet> {

        public ulong value;

        public BitSet(ulong value) {
            this.value = value;
        }

        public void Set(int resultIndex) {
            value |= (1ul << resultIndex);
        }

        public bool Get(int resultIndex) {
            return (value & (1ul << resultIndex)) != 0;
        }

        public static bool operator ==(BitSet a, BitSet b) {
            return a.value == b.value;
        }

        public static bool operator !=(BitSet a, BitSet b) {
            return a.value != b.value;
        }

        public bool Equals(BitSet other) {
            return value == other.value;
        }

        public override bool Equals(object obj) {
            return obj is BitSet other && Equals(other);
        }

        public static explicit operator ulong(BitSet bitSet) {
            return bitSet.value;
        }

        public static explicit operator long(BitSet bitSet) {
            return (long) bitSet.value;
        }

        public override int GetHashCode() {
            return value.GetHashCode();
        }

        public int PopCount() {
            return math.countbits(value);
        }

        public bool ContainsAll(in BitSet bitSet) {
            return (value & bitSet.value) == bitSet.value;
        }

        public bool ContainsAny(in BitSet bitSet) {
            return (value & bitSet.value) != 0;
        }

        public bool this[int index] {
            get {
                ulong mask = 1ul << index;
                return (value & mask) == mask;
            }
            set {
                ulong mask = 1ul << index;
                if (value) {
                    this.value |= mask;
                }
                else {
                    this.value &= ~mask;
                }
            }
        }

        public string DebugDisplayList() {
            string retn = "";

            for (int i = 0; i < 64; i++) {
                if ((value & (1ul << i)) != 0) {
                    retn += i + ", ";
                }
            }

            return retn;
        }

        public string DebugDisplay() {
            StructList<char> charList = new StructList<char>();
            int cnt = 0;
            for (int i = 0; i < 64; i++) {
                charList.Add((value & (1ul << i)) == 0 ? '0' : '1');

                if (cnt == 3 && i != 63) {
                    cnt = 0;
                    charList.Add('_');
                    continue;
                }

                cnt++;
            }

            charList.Reverse();
            return "PopCount = " + PopCount() + " buffer = " + new string(charList.array, 0, charList.size);
        }

        public static BitSet SetRange(int start, int count) {
            return new BitSet((ulong.MaxValue >> -((start + count) - start)) << start);
        }

    }

}
