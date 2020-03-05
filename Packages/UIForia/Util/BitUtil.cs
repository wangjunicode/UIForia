using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UIForia.Util {

    public static class BitUtil {

        [DebuggerStepThrough]
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

        public static int NextMultipleOf32(int n) {
            return (n >> 5) + 1 << 5;
        }

        // https://stackoverflow.com/questions/109023/how-to-count-the-number-of-set-bits-in-a-32-bit-integer
        // This is known as the 'Hamming Weight', 'popcount' or 'sideways addition'
        public static uint CountSetBits(uint i) {
            i -= ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
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

    }

}