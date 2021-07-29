using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Util {

    public static class StringUtil {

        public static readonly CharStringBuilder s_CharStringBuilder = new CharStringBuilder(128);

        public static unsafe string InlineReplace(this string target, char oldValue, char newValue) {
            if (target == null) return null;
            fixed (char* charptr = target) {
                for (int i = 0; i < target.Length; i++) {
                    if (charptr[i] == oldValue) {
                        charptr[i] = newValue;
                    }
                }
            }

            return target;
        }

        public static int FindMatchingIndex(string input, char open, char close, int startOffset = 0) {
            int start = -1;

            for (int i = startOffset; i < input.Length; i++) {
                if (input[i] == open) {
                    start = i;
                    break;
                }
            }

            if (start == -1) {
                return -1;
            }

            int counter = 0;
            int ptr = start;
            while (ptr < input.Length) {
                char current = input[ptr];
                ptr++;
                if (current == open) {
                    counter++;
                }

                if (current == close) {
                    counter--;
                    if (counter == 0) {
                        return ptr;
                    }
                }
            }

            return -1;
        }

        public static unsafe int CharCompareOrdinal(string strA, char[] chars) {
            return CharCompareOrdinal(strA, chars, 0, chars.Length);
        }

        public static unsafe int CharCompareOrdinal(string strA, char[] chars, int start, int length) {
            fixed (char* ptr = chars) {
                return CharCompareOrdinal(strA, ptr, start, length);
            }
        }

        public static unsafe int CharCompareOrdinal(string strA, char* bp, int start, int blength) {
            // adapted https://github.com/microsoft/referencesource/blob/master/mscorlib/system/string.cs to support char* comparison
            int length = strA.Length < blength ? strA.Length : blength;
            int diffOffset = -1;
            bp += start;
            fixed (char* ap = strA) {
                char* a = ap;
                char* b = bp;

                // unroll the loop
                while (length >= 10) {
                    if (*(int*) a != *(int*) b) {
                        diffOffset = 0;
                        break;
                    }

                    if (*(int*) (a + 2) != *(int*) (b + 2)) {
                        diffOffset = 2;
                        break;
                    }

                    if (*(int*) (a + 4) != *(int*) (b + 4)) {
                        diffOffset = 4;
                        break;
                    }

                    if (*(int*) (a + 6) != *(int*) (b + 6)) {
                        diffOffset = 6;
                        break;
                    }

                    if (*(int*) (a + 8) != *(int*) (b + 8)) {
                        diffOffset = 8;
                        break;
                    }

                    a += 10;
                    b += 10;
                    length -= 10;
                }

                if (diffOffset != -1) {
                    // we already see a difference in the unrolled loop above
                    a += diffOffset;
                    b += diffOffset;
                    int order;
                    if ((order = (int) *a - (int) *b) != 0) {
                        return order;
                    }

                    return ((int) *(a + 1) - (int) *(b + 1));
                }

                // now go back to slower code path and do comparison on 2 bytes one time.
                // Note: c# strings are always even byte counts (padded by runtime) but our char * is not
                // so we can't do the same compare logic c# does (int by int), we drop down to 1-1 char comparisons
                while (length > 0) {
                    if (*a != *b) {
                        break;
                    }

                    a++;
                    b++;
                    length--;
                }

                if (length > 0) {
                    int c;
                    // found a different int on above loop
                    if ((c = (int) *a - (int) *b) != 0) {
                        return c;
                    }

                    return ((int) *(a + 1) - (int) *(b + 1));
                }

                // At this point, we have compared all the characters in at least one string.
                // The longer string will be larger.
                return strA.Length - blength;
            }
        }

        public static string ListToString(string[] list, string separator = ", ") {
            if (list == null || list.Length == 0) {
                return string.Empty;
            }

            string retn = null;
            s_CharStringBuilder.Clear();

            for (int i = 0; i < list.Length; i++) {
                s_CharStringBuilder.Append(list[i]);
                if (i != list.Length - 1 && separator != null) {
                    s_CharStringBuilder.Append(separator);
                }
            }

            retn = s_CharStringBuilder.ToString();
            s_CharStringBuilder.Clear();
            return retn;
        }

        public static string ListToString(IReadOnlyList<string> list, string separator = ", ") {
            if (list == null || list.Count == 0) {
                return string.Empty;
            }

            string retn = null;
            s_CharStringBuilder.Clear();

            for (int i = 0; i < list.Count; i++) {
                s_CharStringBuilder.Append(list[i]);
                if (i != list.Count - 1 && separator != null) {
                    s_CharStringBuilder.Append(separator);
                }
            }

            retn = s_CharStringBuilder.ToString();
            s_CharStringBuilder.Clear();
            return retn;
        }

        // public static string ListToString(IList<string> list, string separator = ", ") {
        //     if (list == null || list.Count == 0) {
        //         return string.Empty;
        //     }
        //
        //     string retn = null;
        //     TextUtil.StringBuilder.Clear();
        //
        //     for (int i = 0; i < list.Count; i++) {
        //         TextUtil.StringBuilder.Append(list[i]);
        //         if (i != list.Count - 1 && separator != null) {
        //             TextUtil.StringBuilder.Append(separator);
        //         }
        //     }
        //
        //     retn = TextUtil.StringBuilder.ToString();
        //     TextUtil.StringBuilder.Clear();
        //     return retn;
        // }

        public static unsafe bool EqualsRangeUnsafe(string str, CharSpan span) {
            return EqualsRangeUnsafe(str, span.data, span.rangeStart, span.rangeEnd - span.rangeStart);
        }

        public static unsafe bool EqualsRangeUnsafe(char[] a, int aStart, string str) {
            fixed (char* strptr = str) {
                return EqualsRangeUnsafe(a, aStart, strptr, 0, str.Length);
            }
        }

        public static unsafe bool EqualsRangeUnsafe(char[] a, int aStart, char* bPtr, int bStart, int length) {
            fixed (char* aPtr = a) {
                char* chPtr1 = aPtr + aStart;
                char* chPtr2 = bPtr + bStart;

                char* chPtr3 = chPtr1;
                char* chPtr4 = chPtr2;

                // todo -- assumes 64 bit
                for (; length >= 12; length -= 12) {
                    if (*(long*) chPtr3 != *(long*) chPtr4 || *(long*) (chPtr3 + 4) != *(long*) (chPtr4 + 4) || *(long*) (chPtr3 + 8) != *(long*) (chPtr4 + 8)) {
                        return false;
                    }

                    chPtr3 += 12;
                    chPtr4 += 12;
                }

                for (; length > 0 && *(int*) chPtr3 == *(int*) chPtr4; length -= 2) {
                    chPtr3 += 2;
                    chPtr4 += 2;
                }

                if (length == 1) {
                    return *chPtr3 == *chPtr4;
                }

                return length <= 0;
            }

        }

        public static unsafe bool EqualsRangeUnsafe(char[] a, int aStart, char[] b, int bStart, int length) {
            fixed (char* aPtr = a) {
                fixed (char* bPtr = b) {
                    char* chPtr1 = aPtr + aStart;
                    char* chPtr2 = bPtr + bStart;

                    char* chPtr3 = chPtr1;
                    char* chPtr4 = chPtr2;

                    // todo -- assumes 64 bit
                    for (; length >= 12; length -= 12) {
                        if (*(long*) chPtr3 != *(long*) chPtr4 || *(long*) (chPtr3 + 4) != *(long*) (chPtr4 + 4) || *(long*) (chPtr3 + 8) != *(long*) (chPtr4 + 8)) {
                            return false;
                        }

                        chPtr3 += 12;
                        chPtr4 += 12;
                    }

                    for (; length > 0 && *(int*) chPtr3 == *(int*) chPtr4; length -= 2) {
                        chPtr3 += 2;
                        chPtr4 += 2;
                    }

                    if (length == 1) {
                        return *chPtr3 == *chPtr4;
                    }

                    return length <= 0;
                }
            }
        }

        public static unsafe bool EqualsRangeUnsafe(string str, char[] b, int bStart, int length) {
            fixed (char* charptr = b) {
                return EqualsRangeUnsafe(str, charptr, bStart, length);
            }
        }

        public static unsafe bool EqualsRangeUnsafe(string str, char* b, int bStart, int length) {

            if (str == null) {
                return length != 0;
            }

            if (str.Length != length) {
                return false;
            }

            fixed (char* strPtr = str) {
                return UnsafeUtility.MemCmp(strPtr, b + bStart, sizeof(char) * length) == 0;
            }

        }

        // [ThreadStatic] private static StringBuilder s_PerThreadStringBuilder;
        [ThreadStatic] private static StructList<PoolItem> s_PerThreadStringBuilderPool;

        private struct PoolItem {

            public StringBuilder builder;
            public bool active;

        }

        public static StringBuilder GetPerThreadStringBuilder() {
            s_PerThreadStringBuilderPool = s_PerThreadStringBuilderPool ?? new StructList<PoolItem>();
            if (s_PerThreadStringBuilderPool.Count == 0) {
                s_PerThreadStringBuilderPool.Add(new PoolItem() {
                    active = false,
                    builder = new StringBuilder(128)
                });
            }

            for (int i = 0; i < s_PerThreadStringBuilderPool.size; i++) {
                if (s_PerThreadStringBuilderPool.array[i].active == false) {
                    s_PerThreadStringBuilderPool.array[i].active = true;
                    return s_PerThreadStringBuilderPool.array[i].builder;
                }
            }

            PoolItem retn = new PoolItem() {
                active = true,
                builder = new StringBuilder(128)
            };

            s_PerThreadStringBuilderPool.Add(retn);
            return retn.builder;

        }

        public static void ReleasePerThreadStringBuilder(StringBuilder builder) {
            if (s_PerThreadStringBuilderPool == null) return;
            for (int i = 0; i < s_PerThreadStringBuilderPool.size; i++) {
                if (s_PerThreadStringBuilderPool.array[i].builder == builder) {
                    builder.Clear();
                    s_PerThreadStringBuilderPool.array[i].active = false;
                    return;
                }
            }
        }

        // public static unsafe int WhitespaceSplitRanges(string str, StructList<RangeInt> outputList) {
        //     int retn = 0;
        //     int ptr = 0;
        //     int len = str.Length;
        //
        //     RangeInt range = new RangeInt();
        //     bool inWord = false;
        //
        //     fixed (char* cbuffer = str) {
        //         while (ptr < len) {
        //             char c = cbuffer[ptr++];
        //             if (char.IsWhiteSpace(c)) {
        //                 if (inWord) {
        //                     range.length = ptr - range.start;
        //                     outputList.Add(range);
        //                     retn++;
        //                     range = default;
        //                 }
        //             }
        //             else if (!inWord) {
        //                 inWord = true;
        //                 range.start = ptr;
        //             }
        //         }
        //     }
        //     
        //     return retn;
        // }

    }

}