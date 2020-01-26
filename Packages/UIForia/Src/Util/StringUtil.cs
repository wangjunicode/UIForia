using System;
using System.Collections.Generic;
using UIForia.Text;

namespace UIForia.Util {

    public static class StringUtil {

        public static CharStringBuilder s_CharStringBuilder = new CharStringBuilder(128);

        
        public static int FindMatchingIndex(string input, char open, char close) {

            int start = -1;
            
            for (int i = 0; i < input.Length; i++) {
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
            int num1 = Math.Min(strA.Length, chars.Length);
            int num2 = -1;
            fixed (char* chPtr1 = strA) {
                fixed (char* chPtr2 = chars) {
                    char* chPtr3 = chPtr1;
                    char* chPtr4 = chPtr2;
                    for (; num1 >= 10; num1 -= 10) {
                        if (*(int*) chPtr3 != *(int*) chPtr4) {
                            num2 = 0;
                            break;
                        }

                        if (*(int*) (chPtr3 + 2) != *(int*) (chPtr4 + 2)) {
                            num2 = 2;
                            break;
                        }

                        if (*(int*) (chPtr3 + 4) != *(int*) (chPtr4 + 4)) {
                            num2 = 4;
                            break;
                        }

                        if (*(int*) (chPtr3 + 6) != *(int*) (chPtr4 + 6)) {
                            num2 = 6;
                            break;
                        }

                        if (*(int*) (chPtr3 + 8) != *(int*) (chPtr4 + 8)) {
                            num2 = 8;
                            break;
                        }

                        chPtr3 += 10;
                        chPtr4 += 10;
                    }

                    if (num2 != -1) {
                        char* chPtr5 = chPtr3 + num2;
                        char* chPtr6 = chPtr4 + num2;
                        int num3;
                        return (num3 = (int) *chPtr5 - (int) *chPtr6) != 0 ? num3 : (int) chPtr5[1] - (int) chPtr6[1];
                    }

                    for (; num1 > 0 && *(int*) chPtr3 == *(int*) chPtr4; num1 -= 2) {
                        chPtr3 += 2;
                        chPtr4 += 2;
                    }

                    if (num1 <= 0)
                        return strA.Length - chars.Length;
                    int num4;
                    return (num4 = (int) *chPtr3 - (int) *chPtr4) != 0 ? num4 : chPtr3[1] - chPtr4[1];
                }
            }
        }

        public static string ListToString(IReadOnlyList<string> list, string separator = ", ") {
            if (list == null || list.Count == 0) {
                return string.Empty;
            }

            string retn = null;
            TextUtil.StringBuilder.Clear();

            for (int i = 0; i < list.Count; i++) {
                TextUtil.StringBuilder.Append(list[i]);
                if (i != list.Count - 1 && separator != null) {
                    TextUtil.StringBuilder.Append(separator);
                }
            }

            retn = TextUtil.StringBuilder.ToString();
            TextUtil.StringBuilder.Clear();
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

    }

}