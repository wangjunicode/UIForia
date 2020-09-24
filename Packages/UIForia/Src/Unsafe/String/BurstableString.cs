using Packages.UIForia.Util.Unsafe;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    public static unsafe class BurstableStringUtil {

        public static bool Compare(ushort* a, ushort* b, int length) {
            for (; length >= 12; length -= 12) {
                if (*(long*) a != *(long*) b || *(long*) (a + 4) != *(long*) (b + 4) || *(long*) (a + 8) != *(long*) (b + 8)) {
                    return false;
                }

                a += 12;
                b += 12;
            }

            for (; length > 0 && *(int*) a == *(int*) b; length -= 2) {
                a += 2;
                b += 2;
            }

            if (length == 1) {
                return *a == *b;
            }

            return length <= 0;
        }

        public static unsafe bool Compare(ushort* key, int length, string s) {
            if (length != s.Length) return false;
            fixed (char* ptr = s) {
                return Compare(key, (ushort*) ptr, s.Length);
            }
        }

        public static bool Contains(StringHandle valueHandle, StringHandle other) {
            
            if (valueHandle.length == 0 || valueHandle.data == null || other.data == null || other.length == 0) {
                return false;
            }

            int counter = 0;

            // todo -- there are probably ways to vectorize this 
            for (int i = 0; i < valueHandle.length; i++) {

                if (valueHandle.data[i] == other.data[counter]) {
                    counter++;

                    if (counter == other.length) {
                        return true;
                    }

                }
                else {
                    counter = 0;
                }

            }

            return false;
        }

        public static bool StartsWith(StringHandle valueHandle, StringHandle expectedHandle) {
            if (expectedHandle.length > valueHandle.length) return false;
            
            if (valueHandle.data == null || expectedHandle.data == null) {
                return false;
            }

            return Compare(valueHandle.data, expectedHandle.data, expectedHandle.length);
        }

        public static bool EndsWith(StringHandle valueHandle, StringHandle expectedHandle) {
            if (expectedHandle.length > valueHandle.length) return false;
            
            if (expectedHandle.length <= 0 || valueHandle.length <= 0) {
                return false;
            }
            
            if (valueHandle.data == null || expectedHandle.data == null) {
                return false;
            }

            return Compare(valueHandle.data - expectedHandle.length, expectedHandle.data, expectedHandle.length);
        }

    }

    [AssertSize(16)]
    public unsafe struct BurstableString {

        public readonly int length;
        public readonly int internSource;
        public readonly ushort* buffer;

        public BurstableString(int internSource, int length, ushort* buffer) {
            this.internSource = internSource;
            this.length = length;
            this.buffer = buffer;
        }

        public bool Contains(in BurstableString other) {

            if (length == 0 || buffer == null || other.buffer == null || other.length == 0) {
                return false;
            }

            int counter = 0;

            // todo -- there are probably ways to vectorize this 
            for (int i = 0; i < length; i++) {

                if (buffer[i] == other.buffer[counter]) {
                    counter++;

                    if (counter == other.length) {
                        return true;
                    }

                }
                else {
                    counter = 0;
                }

            }

            return false;

        }

        public bool StartsWith(in BurstableString other) {
            if (other.length > length) return false;
            if (buffer == null || other.buffer == null) {
                return false;
            }

            return UnsafeUtility.MemCmp(buffer, other.buffer, other.length * 2) == 0;
        }

        public bool EndsWith(in BurstableString other) {
            if (other.length > length) return false;
            if (buffer == null || other.buffer == null) {
                return false;
            }

            return UnsafeUtility.MemCmp(buffer + length - other.length, other.buffer, other.length * 2) == 0;
        }

        public static bool operator ==(in BurstableString a, in BurstableString b) {
            if (a.length != b.length) {
                return false;
            }

            // if interned in the same string table we can assert their offsets are the same
            // otherwise we need to do a normal strcmp 
            if (a.internSource == b.internSource && a.internSource > 0) {
                return a.buffer == b.buffer;
            }

            return UnsafeUtility.MemCmp(a.buffer, b.buffer, a.length * 2) == 0;
        }

        public static bool operator !=(in BurstableString a, in BurstableString b) {
            return !(a == b);
        }

        public bool Equals(BurstableString other) {
            return this == other;
        }

        public override bool Equals(object obj) {
            return obj is BurstableString other && Equals(other);
        }

        public override int GetHashCode() {
            return MurmurHash3.Hash((byte*) buffer, length * 2);
        }

        public override string ToString() {
            return new string((char*) buffer);
        }

    }

}