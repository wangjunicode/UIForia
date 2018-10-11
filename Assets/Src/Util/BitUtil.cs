using System.Diagnostics;

namespace Rendering {

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

    }

}