namespace Rendering {

    public static class BitUtil {

        public static int SetHighLowBits(int high, int low) {
            return (high << 16) | (low & 0xffff);
        }

        public static int GetHighBits(int input) {
            return (input >> 16) & (1 << 16) - 1;
        }

        public static int GetLowBits(int input) {
            return input & 0xffff;
        }

    }

}