using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia.Util {

    public static class BitUtil {

// #define BitVal(data,y) ( (data>>y) & 1)      /** Return Data.Y value   **/
// #define SetBit(data,y)    data |= (1 << y)    /** Set Data.Y   to 1    **/
// #define ClearBit(data,y)  data &= ~(1 << y)   /** Clear Data.Y to 0    **/
// #define ToggleBit(data,y)     (data ^=BitVal(y))     /** Toggle Data.Y  value  **/
// #define Toggle(data)   (data =~data )         /** Toggle Data value     **/

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetHighLowBits(int high, int low) {
            return (high << 16) | (low & 0xffff);
        }

        [DebuggerStepThrough]
        public static int GetHighBits(int input) {
            return (input >> 16) & (1 << 16) - 1;
        }

        [DebuggerStepThrough]
        public static int GetLowBits(int input) {
            return input & 0xffff;
        }

        public static int ExtractBits(int number, int bitCount, int offset) {
            return (((1 << bitCount) - 1) & (number >> (offset - 1)));
        }

        public static long IntsToLong(int a1, int a2) {
            long b = a2;
            b = b << 32;
            b = b | (uint) a1;
            return b;
        }

        public static void LongToInts(long longVal, out int int0, out int int1) {
            int0 = (int) (longVal & uint.MaxValue);
            int1 = (int) (longVal >> 32);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct BitSetter {

            [FieldOffset(0)] public uint intVal;
            [FieldOffset(0)] public byte byte0;
            [FieldOffset(1)] public byte byte1;
            [FieldOffset(2)] public byte byte2;
            [FieldOffset(3)] public byte byte3;

            public BitSetter(uint value) {
                byte0 = 0;
                byte1 = 0;
                byte2 = 0;
                byte3 = 0;
                intVal = value;
            }

        }

        public static uint NextPowerOfTwo(uint v) {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        public static int NextMultipleOf128(int n) {
            return (n >> 7) + 1 << 7;
        }

        public static int NextMultipleOf64(int n) {
            return (n >> 6) + 1 << 6;
        }

        public static int NextMultipleOf32(int n) {
            return (n >> 5) + 1 << 5;
        }

        public static int NextMultipleOf16(int n) {
            return (n >> 4) + 1 << 4;
        }

        public static int NextMultipleOf8(int n) {
            return (n >> 3) + 1 << 3;
        }

        public static int NextMultipleOf4(int n) {
            return (n >> 2) + 1 << 2;
        }

        public static bool IsPowerOfTwo(uint x) {
            return ((x != 0) && ((x & (~x + 1)) == x));
        }

        // https://stackoverflow.com/questions/109023/how-to-count-the-number-of-set-bits-in-a-32-bit-integer
        // This is known as the 'Hamming Weight', 'popcount' or 'sideways addition'
        public static uint CountSetBits(uint i) {
            i -= ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        public static unsafe int CountSetBits(uint* map, int size) {
            uint count = 0;
            for (int x = 0; x < size; x++) {
                uint i = map[x];

                i -= ((i >> 1) & 0x55555555);
                i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
                count += (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
            }

            return (int) count;
        }

        public static uint SetByte0(uint value, int i) {
            BitSetter b = new BitSetter(value);
            b.byte0 = (byte) i;
            return b.intVal;
        }

        public static uint SetByte1(uint value, int i) {
            BitSetter b = new BitSetter(value);
            b.byte1 = (byte) i;
            return b.intVal;
        }

        public static uint SetByte2(uint value, int i) {
            BitSetter b = new BitSetter(value);
            b.byte2 = (byte) i;
            return b.intVal;
        }

        public static uint SetByte3(uint value, int i) {
            BitSetter b = new BitSetter(value);
            b.byte3 = (byte) i;
            return b.intVal;
        }

        public static byte GetByteN(uint value, int n) {
            return (byte) ((value >> 8 * n) & 0xff);
        }

        public static uint SetBytes(int byte0, int byte1, int byte2, int byte3) {
            BitSetter b = new BitSetter(0);
            b.byte0 = (byte) byte0;
            b.byte1 = (byte) byte1;
            b.byte2 = (byte) byte2;
            b.byte3 = (byte) byte3;
            return b.intVal;
        }

        public static uint EnsurePowerOfTwo(uint size) {
            if (IsPowerOfTwo(size)) {
                return size;
            }

            return NextPowerOfTwo(size);
        }

        public static ulong MergeUint3ToLong(uint input1, uint input2, uint input3) {
            ulong l = 0;
            l |= (input1 & 0xfffff);
            l |= (ulong) (input2 & 0xfffff) << 21;
            l |= (ulong) (input3 & 0xfffff) << 42;
            return l;
        }

        public static int EnsurePowerOfTwo(int size) {
            uint uintSize = (uint) size;
            if (IsPowerOfTwo(uintSize)) {
                return (int) uintSize;
            }

            return (int) NextPowerOfTwo(uintSize);
        }

        public static int GetPowerOfTwoBitIndex(uint value) {
            switch (value) {
                case 0: return 0;
                case 2: return 1;
                case 4: return 2;
                case 8: return 3;
                case 16: return 4;
                case 32: return 5;
                case 64: return 6;
                case 128: return 7;
                case 256: return 8;
                case 512: return 9;
                case 1024: return 10;
                case 2048: return 11;
                case 4096: return 12;
                case 8192: return 13;
                case 16384: return 14;
                case 32768: return 15;
                case 65536: return 16;
                case 131072: return 17;
                case 262144: return 18;
                case 524288: return 19;
                case 1048576: return 20;
                case 2097152: return 21;
                case 4194304: return 22;
                case 8388608: return 23;
                case 16777216: return 24;
                case 33554432: return 25;
                case 67108864: return 26;
                case 134217728: return 27;
                case 268435456: return 28;
                case 536870912: return 29;
                case 1073741824: return 30;
                case 2147483648: return 31;
                // case 4294967296: return 32; 
            }

            return -1;
        }

     
    }

}