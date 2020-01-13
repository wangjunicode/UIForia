using System;
using System.Collections.Generic;
using System.Text;
using UIForia.Text;
using ZFormat;

namespace UIForia.Util {

    public class CharStringBuilder {

        public int size;
        public char[] characters;

        private static char[] s_Scratch = new char[256];

        public CharStringBuilder(int capacity = 32) {
            this.size = 0;
            this.characters = new char[Math.Max(8, capacity)];
        }

        public void Clear() {
            size = 0;
        }

        public CharStringBuilder Append(string str) {
            int strLength = str.Length;

            if (size + strLength >= characters.Length) {
                Array.Resize(ref characters, (size + strLength) * 2);
            }

            unsafe {
                fixed (char* smem = str) {
                    fixed (char* dmem = characters) {
                        byte* d = (byte*) (dmem + size);
                        Buffer.MemoryCopy((byte*) smem, d, strLength * 2, strLength * 2);
                    }
                }

                size += strLength;
            }

            return this;
        }
        
        public CharStringBuilder Append(short val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(int val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(long val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(ushort val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(uint val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(ulong val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(float val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(double val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(decimal val) {
            Append(val.ToString());
            return this;
        }


        public CharStringBuilder Append(byte val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(sbyte val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(bool val) {
            return Append(val ? "true" : "false");
        }

        public CharStringBuilder Append(char str) {
            if (size + 1 >= characters.Length) {
                Array.Resize(ref characters, (size + 1) * 2);
            }

            characters[size] = str;
            size++;
            return this;
        }

        public CharStringBuilder Append(char[] str) {
            int strLength = str.Length;

            if (size + strLength >= characters.Length) {
                Array.Resize(ref characters, (size + strLength) * 2);
            }

            unsafe {
                fixed (char* smem = str) {
                    fixed (char* dmem = characters) {
                        byte* d = (byte*) (dmem + size);
                        Buffer.MemoryCopy((byte*) smem, d, strLength * 2, strLength * 2);
                    }
                }

                size += strLength;
            }

            return this;
        }

        public CharStringBuilder Append(char[] str, int count) {
            return Append(str, 0, count);
        }

        public CharStringBuilder Append(char[] str, int start, int end) {
            int strLength = end - start;

            if (size + strLength >= characters.Length) {
                Array.Resize(ref characters, (size + strLength) * 2);
            }

            unsafe {
                fixed (char* smem = str) {
                    fixed (char* dmem = characters) {
                        byte* d = (byte*) (dmem + size);
                        byte* s = (byte*) (smem + start);
                        Buffer.MemoryCopy((byte*) s, d, strLength * 2, strLength * 2);
                    }
                }

                size += strLength;
            }

            return this;
        }

        public override string ToString() {
            return new string(characters, 0, size);
        }

        public unsafe bool EqualsString(string str) {
            if (str == null || size != str.Length) {
                return false;
            }

            int length = size;

            fixed (char* chPtr1 = characters) {
                fixed (char* chPtr2 = str) {
                    char* chPtr3 = chPtr1;
                    char* chPtr4 = chPtr2;
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

                    return length <= 0;
                }
            }
        }

        public unsafe bool EqualsString(char[] str) {
            if (size != str.Length) {
                return false;
            }

            int length = size;

            fixed (char* chPtr1 = characters) {
                fixed (char* chPtr2 = str) {
                    char* chPtr3 = chPtr1;
                    char* chPtr4 = chPtr2;
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

                    return length <= 0;
                }
            }
        }

        public static unsafe bool CompareStringBuilder_String(StringBuilder builder, string str) {
            if (builder.Length != str.Length) {
                return false;
            }

            if (builder.Length > s_Scratch.Length) {
                s_Scratch = new char[builder.Length];
            }

            builder.CopyTo(0, s_Scratch, 0, builder.Length);
            int length = builder.Length;

            fixed (char* chPtr1 = s_Scratch) {
                fixed (char* chPtr2 = str) {
                    char* chPtr3 = chPtr1;
                    char* chPtr4 = chPtr2;
                    for (; length >= 12; length -= 12) {
                        if (*(long*) chPtr3 != *(long*) chPtr4 || *(long*) (chPtr3 + 4) != *(long*) (chPtr4 + 4) || *(long*) (chPtr3 + 8) != *(long*) (chPtr4 + 8))
                            return false;
                        chPtr3 += 12;
                        chPtr4 += 12;
                    }

                    for (; length > 0 && *(int*) chPtr3 == *(int*) chPtr4; length -= 2) {
                        chPtr3 += 2;
                        chPtr4 += 2;
                    }

                    return length <= 0;
                }
            }
        }

    }

    public static class StringUtil {

        public static CharStringBuilder s_CharStringBuilder = new CharStringBuilder(128);


        public static unsafe bool EqualsHelper(string strA, string strB) {
            int length = strA.Length;
            fixed (char* chPtr1 = strA) {
                fixed (char* chPtr2 = strB) {
                    char* chPtr3 = chPtr1;
                    char* chPtr4 = chPtr2;
                    for (; length >= 12; length -= 12) {
                        if (*(long*) chPtr3 != *(long*) chPtr4 || *(long*) (chPtr3 + 4) != *(long*) (chPtr4 + 4) || *(long*) (chPtr3 + 8) != *(long*) (chPtr4 + 8))
                            return false;
                        chPtr3 += 12;
                        chPtr4 += 12;
                    }

                    for (; length > 0 && *(int*) chPtr3 == *(int*) chPtr4; length -= 2) {
                        chPtr3 += 2;
                        chPtr4 += 2;
                    }

                    return length <= 0;
                }
            }
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

        public static string ListToString(IList<string> list, string separator = ", ") {
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

    }

}