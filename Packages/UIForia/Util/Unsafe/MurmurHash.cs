namespace Packages.UIForia.Util.Unsafe {

    public static class MurmurHash3 {

        public static unsafe int Hash(string strVal, uint seed = 0xfeedbeef) {

            fixed (char* charptr = strVal) {
                byte* bytes = (byte*) charptr;
                // length * 2 because sizeof(char) is 2 bytes, double to get length in bytes
                return Hash(bytes, strVal.Length * 2, seed);
            }

        }

        // adapted from https://ayende.com/blog/183137-C/blind-optimizations-making-murmurhash3-faster
        public static unsafe int Hash(byte* pData, int length, uint seed = 0xfeedbeef) {

            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;

            uint h1 = seed;
            uint* pInts = (uint*) pData;
            int end = (length >> 2);
            uint k1;
            for (int i = 0; i < end; i++) {
                k1 = pInts[i];
                k1 *= c1;
                k1 = (k1 << 15) | (k1 >> (32 - 15));
                k1 *= c2;

                h1 ^= k1;
                h1 = (h1 << 13) | (h1 >> (32 - 13));
                h1 = h1 * 5 + 0xe6546b64;
            }

            // handle remainder
            k1 = 0;
            int remainder = length - end * sizeof(uint);
            if (remainder > 0) {
                for (int i = 0; i < remainder; i++) {
                    // k1 = k1 << 8;
                    k1 = pData[i];
                }

                k1 *= c1;
                // k1 = Rotl32(k1, 15);
                k1 = (k1 << 15) | (k1 >> (32 - 15));
                k1 *= c2;
                h1 ^= k1;
            }

            h1 ^= (uint) length;

            // h1 = Fmix(h1);
            h1 ^= h1 >> 16;
            h1 *= 0x85ebca6b;
            h1 ^= h1 >> 13;
            h1 *= 0xc2b2ae35;
            h1 ^= h1 >> 16;

            return (int) h1;
        }

    }

}