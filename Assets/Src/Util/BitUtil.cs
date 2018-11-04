using System.Diagnostics;

namespace Src.Rendering {

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

    }

}