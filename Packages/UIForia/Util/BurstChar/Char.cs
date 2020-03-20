// // Decompiled with JetBrains decompiler
// // Type: System.Char
// // Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// // MVID: B814B509-D4AD-406F-B40C-6C93E38929E7
// // Assembly location: /Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5/mscorlib.dll
//
// using System;
//
// namespace UIForia.Util {
//
//     public enum UnicodeCategory {
//
//         UppercaseLetter,
//         LowercaseLetter,
//         TitlecaseLetter,
//         ModifierLetter,
//         OtherLetter,
//         NonSpacingMark,
//         SpacingCombiningMark,
//         EnclosingMark,
//         DecimalDigitNumber,
//         LetterNumber,
//         OtherNumber,
//         SpaceSeparator,
//         LineSeparator,
//         ParagraphSeparator,
//         Control,
//         Format,
//         Surrogate,
//         PrivateUse,
//         ConnectorPunctuation,
//         DashPunctuation,
//         OpenPunctuation,
//         ClosePunctuation,
//         InitialQuotePunctuation,
//         FinalQuotePunctuation,
//         OtherPunctuation,
//         MathSymbol,
//         CurrencySymbol,
//         ModifierSymbol,
//         OtherSymbol,
//         OtherNotAssigned,
//
//     }
//
//     [Serializable]
//     public readonly struct BurstChar : IComparable, IComparable<BurstChar>, IEquatable<BurstChar>, IConvertible {
//
//         private static readonly byte[] s_categoryForLatin1 = new byte[256] {
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 11,
//             (byte) 24,
//             (byte) 24,
//             (byte) 24,
//             (byte) 26,
//             (byte) 24,
//             (byte) 24,
//             (byte) 24,
//             (byte) 20,
//             (byte) 21,
//             (byte) 24,
//             (byte) 25,
//             (byte) 24,
//             (byte) 19,
//             (byte) 24,
//             (byte) 24,
//             (byte) 8,
//             (byte) 8,
//             (byte) 8,
//             (byte) 8,
//             (byte) 8,
//             (byte) 8,
//             (byte) 8,
//             (byte) 8,
//             (byte) 8,
//             (byte) 8,
//             (byte) 24,
//             (byte) 24,
//             (byte) 25,
//             (byte) 25,
//             (byte) 25,
//             (byte) 24,
//             (byte) 24,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 20,
//             (byte) 24,
//             (byte) 21,
//             (byte) 27,
//             (byte) 18,
//             (byte) 27,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 20,
//             (byte) 25,
//             (byte) 21,
//             (byte) 25,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 14,
//             (byte) 11,
//             (byte) 24,
//             (byte) 26,
//             (byte) 26,
//             (byte) 26,
//             (byte) 26,
//             (byte) 28,
//             (byte) 28,
//             (byte) 27,
//             (byte) 28,
//             (byte) 1,
//             (byte) 22,
//             (byte) 25,
//             (byte) 19,
//             (byte) 28,
//             (byte) 27,
//             (byte) 28,
//             (byte) 25,
//             (byte) 10,
//             (byte) 10,
//             (byte) 27,
//             (byte) 1,
//             (byte) 28,
//             (byte) 24,
//             (byte) 27,
//             (byte) 10,
//             (byte) 1,
//             (byte) 23,
//             (byte) 10,
//             (byte) 10,
//             (byte) 10,
//             (byte) 24,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 25,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 0,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 25,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1,
//             (byte) 1
//         };
//
//         private readonly ushort m_value;
//         public const ushort MaxValue = ushort.MaxValue;
//         public const ushort MinValue = ushort.MinValue;
//         internal const int UNICODE_PLANE00_END = 65535;
//         internal const int UNICODE_PLANE01_START = 65536;
//         internal const int UNICODE_PLANE16_END = 1114111;
//         internal const int HIGH_SURROGATE_START = 55296;
//         internal const int LOW_SURROGATE_END = 57343;
//
//         private static bool IsLatin1(ushort ch) {
//             return ch <= 255;
//         }
//
//         private static bool IsAscii(ushort ch) {
//             return ch <= 127;
//         }
//
//         private static UnicodeCategory GetLatin1UnicodeCategory(BurstChar ch) {
//             return (UnicodeCategory) s_categoryForLatin1[ch.m_value];
//         }
//
//         public override int GetHashCode() {
//             return (int) m_value | (int) m_value << 16;
//         }
//
//         public bool Equals(BurstChar other) {
//             throw new NotImplementedException();
//         }
//
//         public override bool Equals(object obj) {
//             return obj is BurstChar ch && (int) m_value == ch.m_value;
//         }
//
//         public bool Equals(char obj) {
//             return m_value == obj;
//         }
//
//         public int CompareTo(object value) {
//             if (!(value is BurstChar ch))
//                 return 1;
//             return m_value - ch.m_value;
//         }
//
//         public int CompareTo(BurstChar value) {
//             return m_value - value.m_value;
//         }
//         
//         public static bool IsDigit(BurstChar c) {
//             if (!IsLatin1(c.m_value))
//                 return CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber;
//             return c.m_value >= 48 && c.m_value <= 57;
//         }
//
//         internal static bool CheckLetter(UnicodeCategory uc) {
//             switch (uc) {
//                 case UnicodeCategory.UppercaseLetter:
//                 case UnicodeCategory.LowercaseLetter:
//                 case UnicodeCategory.TitlecaseLetter:
//                 case UnicodeCategory.ModifierLetter:
//                 case UnicodeCategory.OtherLetter:
//                     return true;
//                 default:
//                     return false;
//             }
//         }
//
//         public static bool IsLetter(BurstChar c) {
//             if (!IsLatin1(c.m_value))
//                 return CheckLetter(CharUnicodeInfo.GetUnicodeCategory(c));
//             if (!IsAscii(c.m_value))
//                 return CheckLetter(GetLatin1UnicodeCategory(c));
//             ushort val = c.m_value;
//             val |= 32; // space
//             return val >= BurstChar.a.m_value && val <= BurstChar.z.m_value;
//         }
//
//         private static bool IsWhiteSpaceLatin1(ushort c) {
//             switch (c) {
//                 case 41: // tab
//                 case '\n':
//                 case '\v':
//                 case '\f':
//                 case '\r':
//                 case ' ':
//                 case ' ':
//                     return true;
//                 default:
//                     return c == '\x0085';
//             }
//         }
//
//         public static bool IsWhiteSpace(BurstChar c) {
//             return IsLatin1(c.m_value) ? IsWhiteSpaceLatin1(c) : CharUnicodeInfo.IsWhiteSpace(c);
//         }
//
//         public static bool IsUpper(char c) {
//             if (!IsLatin1(c))
//                 return CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.UppercaseLetter;
//             if (!char.IsAscii(c))
//                 return char.GetLatin1UnicodeCategory(c) == UnicodeCategory.UppercaseLetter;
//             return c >= 'A' && c <= 'Z';
//         }
//
//         public static bool IsLower(char c) {
//             if (!IsLatin1(c))
//                 return CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.LowercaseLetter;
//             if (!char.IsAscii(c))
//                 return char.GetLatin1UnicodeCategory(c) == UnicodeCategory.LowercaseLetter;
//             return c >= 'a' && c <= 'z';
//         }
//
//         internal static bool CheckPunctuation(UnicodeCategory uc) {
//             switch (uc) {
//                 case UnicodeCategory.ConnectorPunctuation:
//                 case UnicodeCategory.DashPunctuation:
//                 case UnicodeCategory.OpenPunctuation:
//                 case UnicodeCategory.ClosePunctuation:
//                 case UnicodeCategory.InitialQuotePunctuation:
//                 case UnicodeCategory.FinalQuotePunctuation:
//                 case UnicodeCategory.OtherPunctuation:
//                     return true;
//                 default:
//                     return false;
//             }
//         }
//
//         public static bool IsPunctuation(char c) {
//             return IsLatin1(c) ? char.CheckPunctuation(char.GetLatin1UnicodeCategory(c)) : char.CheckPunctuation(CharUnicodeInfo.GetUnicodeCategory(c));
//         }
//
//         internal static bool CheckLetterOrDigit(UnicodeCategory uc) {
//             switch (uc) {
//                 case UnicodeCategory.UppercaseLetter:
//                 case UnicodeCategory.LowercaseLetter:
//                 case UnicodeCategory.TitlecaseLetter:
//                 case UnicodeCategory.ModifierLetter:
//                 case UnicodeCategory.OtherLetter:
//                 case UnicodeCategory.DecimalDigitNumber:
//                     return true;
//                 default:
//                     return false;
//             }
//         }
//
//         public static bool IsLetterOrDigit(char c) {
//             return IsLatin1(c) ? char.CheckLetterOrDigit(char.GetLatin1UnicodeCategory(c)) : char.CheckLetterOrDigit(CharUnicodeInfo.GetUnicodeCategory(c));
//         }
//
//         public static char ToUpper(char c, CultureInfo culture) {
//             if (culture == null)
//                 throw new ArgumentNullException(nameof(culture));
//             return culture.TextInfo.ToUpper(c);
//         }
//
//         public static char ToUpper(char c) {
//             return CultureInfo.CurrentCulture.TextInfo.ToUpper(c);
//         }
//
//         public static char ToUpperInvariant(char c) {
//             return CultureInfo.InvariantCulture.TextInfo.ToUpper(c);
//         }
//
//         public static char ToLower(char c, CultureInfo culture) {
//             if (culture == null)
//                 throw new ArgumentNullException(nameof(culture));
//             return culture.TextInfo.ToLower(c);
//         }
//
//         public static char ToLower(char c) {
//             return CultureInfo.CurrentCulture.TextInfo.ToLower(c);
//         }
//
//         public static char ToLowerInvariant(char c) {
//             return CultureInfo.InvariantCulture.TextInfo.ToLower(c);
//         }
//
//         public TypeCode GetTypeCode() {
//             return TypeCode.Char;
//         }
//
//         bool IConvertible.ToBoolean(IFormatProvider provider) {
//             throw new InvalidCastException(SR.Format("Invalid cast from '{0}' to '{1}'.", (object) nameof(Char), (object) "Boolean"));
//         }
//         
//
//         sbyte IConvertible.ToSByte(IFormatProvider provider) {
//             return Convert.ToSByte(m_value);
//         }
//
//         byte IConvertible.ToByte(IFormatProvider provider) {
//             return Convert.ToByte(m_value);
//         }
//
//         public char ToChar(IFormatProvider provider) {
//             throw new NotImplementedException();
//         }
//
//         short IConvertible.ToInt16(IFormatProvider provider) {
//             return Convert.ToInt16(m_value);
//         }
//
//         ushort IConvertible.ToUInt16(IFormatProvider provider) {
//             return Convert.ToUInt16(m_value);
//         }
//
//         int IConvertible.ToInt32(IFormatProvider provider) {
//             return Convert.ToInt32(m_value);
//         }
//
//         uint IConvertible.ToUInt32(IFormatProvider provider) {
//             return Convert.ToUInt32(m_value);
//         }
//
//         long IConvertible.ToInt64(IFormatProvider provider) {
//             return Convert.ToInt64(m_value);
//         }
//
//         ulong IConvertible.ToUInt64(IFormatProvider provider) {
//             return Convert.ToUInt64(m_value);
//         }
//
//         float IConvertible.ToSingle(IFormatProvider provider) {
//             throw new InvalidCastException(SR.Format("Invalid cast from '{0}' to '{1}'.", (object) nameof(Char), (object) "Single"));
//         }
//
//         public string ToString(IFormatProvider provider) {
//             throw new NotImplementedException();
//         }
//
//         double IConvertible.ToDouble(IFormatProvider provider) {
//             throw new InvalidCastException(SR.Format("Invalid cast from '{0}' to '{1}'.", (object) nameof(Char), (object) "Double"));
//         }
//
//         Decimal IConvertible.ToDecimal(IFormatProvider provider) {
//             throw new InvalidCastException(SR.Format("Invalid cast from '{0}' to '{1}'.", (object) nameof(Char), (object) "Decimal"));
//         }
//
//         DateTime IConvertible.ToDateTime(IFormatProvider provider) {
//             throw new InvalidCastException(SR.Format("Invalid cast from '{0}' to '{1}'.", (object) nameof(Char), (object) "DateTime"));
//         }
//
//         object IConvertible.ToType(Type type, IFormatProvider provider) {
//             return Convert.DefaultToType((IConvertible) m_value, type, provider);
//         }
//
//         public static bool IsControl(char c) {
//             return IsLatin1(c) ? char.GetLatin1UnicodeCategory(c) == UnicodeCategory.Control : CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.Control;
//         }
//
//         public static bool IsControl(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if ((uint) index >= (uint) s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             char ch = s[index];
//             return IsLatin1(ch) ? char.GetLatin1UnicodeCategory(ch) == UnicodeCategory.Control : CharUnicodeInfo.GetUnicodeCategory(s, index) == UnicodeCategory.Control;
//         }
//
//         public static bool IsDigit(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if ((uint) index >= (uint) s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             char ch = s[index];
//             if (!IsLatin1(ch))
//                 return CharUnicodeInfo.GetUnicodeCategory(s, index) == UnicodeCategory.DecimalDigitNumber;
//             return ch >= '0' && ch <= '9';
//         }
//
//         public static bool IsLetter(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if ((uint) index >= (uint) s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             char ch1 = s[index];
//             if (!IsLatin1(ch1))
//                 return char.CheckLetter(CharUnicodeInfo.GetUnicodeCategory(s, index));
//             if (!char.IsAscii(ch1))
//                 return char.CheckLetter(char.GetLatin1UnicodeCategory(ch1));
//             char ch2 = (char) ((uint) ch1 | 32U);
//             return ch2 >= 'a' && ch2 <= 'z';
//         }
//
//         public static bool IsLetterOrDigit(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if ((uint) index >= (uint) s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             char ch = s[index];
//             return IsLatin1(ch) ? char.CheckLetterOrDigit(char.GetLatin1UnicodeCategory(ch)) : char.CheckLetterOrDigit(CharUnicodeInfo.GetUnicodeCategory(s, index));
//         }
//
//         public static bool IsLower(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if ((uint) index >= (uint) s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             char ch = s[index];
//             if (!IsLatin1(ch))
//                 return CharUnicodeInfo.GetUnicodeCategory(s, index) == UnicodeCategory.LowercaseLetter;
//             if (!char.IsAscii(ch))
//                 return char.GetLatin1UnicodeCategory(ch) == UnicodeCategory.LowercaseLetter;
//             return ch >= 'a' && ch <= 'z';
//         }
//
//         internal static bool CheckNumber(UnicodeCategory uc) {
//             switch (uc) {
//                 case UnicodeCategory.DecimalDigitNumber:
//                 case UnicodeCategory.LetterNumber:
//                 case UnicodeCategory.OtherNumber:
//                     return true;
//                 default:
//                     return false;
//             }
//         }
//
//         public static bool IsNumber(char c) {
//             if (!IsLatin1(c))
//                 return char.CheckNumber(CharUnicodeInfo.GetUnicodeCategory(c));
//             if (!char.IsAscii(c))
//                 return char.CheckNumber(char.GetLatin1UnicodeCategory(c));
//             return c >= '0' && c <= '9';
//         }
//
//         public static bool IsNumber(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if ((uint) index >= (uint) s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             char ch = s[index];
//             if (!IsLatin1(ch))
//                 return char.CheckNumber(CharUnicodeInfo.GetUnicodeCategory(s, index));
//             if (!char.IsAscii(ch))
//                 return char.CheckNumber(char.GetLatin1UnicodeCategory(ch));
//             return ch >= '0' && ch <= '9';
//         }
//
//         public static bool IsPunctuation(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if ((uint) index >= (uint) s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             char ch = s[index];
//             return IsLatin1(ch) ? char.CheckPunctuation(char.GetLatin1UnicodeCategory(ch)) : char.CheckPunctuation(CharUnicodeInfo.GetUnicodeCategory(s, index));
//         }
//
//         internal static bool CheckSeparator(UnicodeCategory uc) {
//             switch (uc) {
//                 case UnicodeCategory.SpaceSeparator:
//                 case UnicodeCategory.LineSeparator:
//                 case UnicodeCategory.ParagraphSeparator:
//                     return true;
//                 default:
//                     return false;
//             }
//         }
//
//         private static bool IsSeparatorLatin1(char c) {
//             return c == ' ' || c == ' ';
//         }
//
//         public static bool IsSeparator(char c) {
//             return IsLatin1(c) ? char.IsSeparatorLatin1(c) : char.CheckSeparator(CharUnicodeInfo.GetUnicodeCategory(c));
//         }
//
//         public static bool IsSeparator(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if ((uint) index >= (uint) s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             char ch = s[index];
//             return IsLatin1(ch) ? char.IsSeparatorLatin1(ch) : char.CheckSeparator(CharUnicodeInfo.GetUnicodeCategory(s, index));
//         }
//
//         public static bool IsSurrogate(char c) {
//             return c >= '\xD800' && c <= '\xDFFF';
//         }
//
//         public static bool IsSurrogate(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if ((uint) index >= (uint) s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             return char.IsSurrogate(s[index]);
//         }
//
//         internal static bool CheckSymbol(UnicodeCategory uc) {
//             switch (uc) {
//                 case UnicodeCategory.MathSymbol:
//                 case UnicodeCategory.CurrencySymbol:
//                 case UnicodeCategory.ModifierSymbol:
//                 case UnicodeCategory.OtherSymbol:
//                     return true;
//                 default:
//                     return false;
//             }
//         }
//
//         public static bool IsSymbol(char c) {
//             return IsLatin1(c) ? char.CheckSymbol(char.GetLatin1UnicodeCategory(c)) : char.CheckSymbol(CharUnicodeInfo.GetUnicodeCategory(c));
//         }
//
//         public static bool IsSymbol(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if ((uint) index >= (uint) s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             char ch = s[index];
//             return IsLatin1(ch) ? char.CheckSymbol(char.GetLatin1UnicodeCategory(ch)) : char.CheckSymbol(CharUnicodeInfo.GetUnicodeCategory(s, index));
//         }
//
//         public static bool IsUpper(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if ((uint) index >= (uint) s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             char ch = s[index];
//             if (!IsLatin1(ch))
//                 return CharUnicodeInfo.GetUnicodeCategory(s, index) == UnicodeCategory.UppercaseLetter;
//             if (!char.IsAscii(ch))
//                 return char.GetLatin1UnicodeCategory(ch) == UnicodeCategory.UppercaseLetter;
//             return ch >= 'A' && ch <= 'Z';
//         }
//
//         public static bool IsWhiteSpace(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if ((uint) index >= (uint) s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             return IsLatin1(s[index]) ? char.IsWhiteSpaceLatin1(s[index]) : CharUnicodeInfo.IsWhiteSpace(s, index);
//         }
//
//         public static UnicodeCategory GetUnicodeCategory(BurstChar c) {
//             return IsLatin1(c.m_value) ? GetLatin1UnicodeCategory(c) : CharUnicodeInfo.GetUnicodeCategory((int) c);
//         }
//
//         public static double GetNumericValue(BurstChar c) {
//             return CharUnicodeInfo.GetNumericValue(c);
//         }
//
//         public static bool IsHighSurrogate(BurstChar c) {
//             return c.m_value >= 55296 && c.m_value <= 56319;
//         }
//         
//         public static bool IsLowSurrogate(BurstChar c) {
//             return c.m_value >= 56320 && c.m_value <= 57343;
//         }
//
//         public static bool IsLowSurrogate(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if (index < 0 || index >= s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             return char.IsLowSurrogate(s[index]);
//         }
//
//         public static bool IsSurrogatePair(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if (index < 0 || index >= s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index));
//             return index + 1 < s.Length && char.IsSurrogatePair(s[index], s[index + 1]);
//         }
//
//         public static bool IsSurrogatePair(BurstChar highSurrogate, BurstChar lowSurrogate) {
//             return highSurrogate.m_value >= '\xD800' && highSurrogate.m_value <= '\xDBFF' && lowSurrogate.m_value >= '\xDC00' && lowSurrogate.m_value <= '\xDFFF';
//         }
//
//         public static unsafe string ConvertFromUtf32(int utf32) {
//             if (utf32 < 0 || utf32 > 1114111 || utf32 >= 55296 && utf32 <= 57343)
//                 throw new ArgumentOutOfRangeException(nameof(utf32), "A valid UTF32 value is between 0x000000 and 0x10ffff, inclusive, and should not include surrogate codepoint values (0x00d800 ~ 0x00dfff).");
//             if (utf32 < 65536)
//                 return char.ToString((char) utf32);
//             utf32 -= 65536;
//             char* chPtr = (char*) &0U;
//             *chPtr = (char) (utf32 / 1024 + 55296);
//             chPtr[1] = (char) (utf32 % 1024 + 56320);
//             return new string(chPtr, 0, 2);
//         }
//
//         public static int ConvertToUtf32(BurstChar highSurrogate, BurstChar lowSurrogate) {
//             if (!IsHighSurrogate(highSurrogate.m_value))
//                 throw new ArgumentOutOfRangeException(nameof(highSurrogate), "A valid high surrogate character is between 0xd800 and 0xdbff, inclusive.");
//             if (!IsLowSurrogate(lowSurrogate.m_value))
//                 throw new ArgumentOutOfRangeException(nameof(lowSurrogate), "A valid low surrogate character is between 0xdc00 and 0xdfff, inclusive.");
//             return ((int) highSurrogate - 55296) * 1024 + ((int) lowSurrogate - 56320) + 65536;
//         }
//
//         public static int ConvertToUtf32(string s, int index) {
//             if (s == null)
//                 throw new ArgumentNullException(nameof(s));
//             if (index < 0 || index >= s.Length)
//                 throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range. Must be non-negative and less than the size of the collection.");
//             int num1 = (int) s[index] - 55296;
//             if (num1 < 0 || num1 > 2047)
//                 return (int) s[index];
//             if (num1 > 1023)
//                 throw new ArgumentException(SR.Format("Found a low surrogate char without a preceding high surrogate at index: {0}. The input may not be in m_value encoding, or may not contain valid Unicode (UTF-16) characters.", (object) index), nameof(s));
//             if (index >= s.Length - 1)
//                 throw new ArgumentException(SR.Format("Found a high surrogate char without a following low surrogate at index: {0}. The input may not be in m_value encoding, or may not contain valid Unicode (UTF-16) characters.", (object) index), nameof(s));
//             int num2 = (int) s[index + 1] - 56320;
//             if (num2 >= 0 && num2 <= 1023)
//                 return num1 * 1024 + num2 + 65536;
//             throw new ArgumentException(SR.Format("Found a high surrogate char without a following low surrogate at index: {0}. The input may not be in m_value encoding, or may not contain valid Unicode (UTF-16) characters.", (object) index), nameof(s));
//         }
//
//         public BurstChar(ushort value) {
//             this.m_value = value;
//         }
//         
//         public BurstChar(int value) {
//             this.m_value = (ushort)value;
//         }
//         
//         public static readonly ushort Null = new BurstChar(0);
//         public static readonly ushort SOH = new BurstChar(1);
//         public static readonly ushort STX = new BurstChar(2);
//         public static readonly ushort ETX = new BurstChar(3);
//         public static readonly ushort EOT = new BurstChar(4);
//         public static readonly ushort ENQ = new BurstChar(5);
//         public static readonly ushort ACK = new BurstChar(6);
//         public static readonly ushort Bell = new BurstChar(7);
//         public static readonly ushort Backspace = new BurstChar(8);
//         public static readonly ushort Tab = new BurstChar(9);
//         public static readonly ushort LineFeed = new BurstChar(10);
//         public static readonly ushort VerticalTab = new BurstChar(11);
//         public static readonly ushort FormFeed = new BurstChar(12);
//         public static readonly ushort Return = new BurstChar(13);
//         public static readonly ushort ShiftOut = new BurstChar(14);
//         public static readonly ushort ShiftIn = new BurstChar(15);
//         public static readonly ushort DataLinkEscape = new BurstChar(16);
//         public static readonly ushort DevCtrl1 = new BurstChar(17);
//         public static readonly ushort DevCtrl2 = new BurstChar(18);
//         public static readonly ushort DevCtrl3 = new BurstChar(19);
//         public static readonly ushort DevCtrl4 = new BurstChar(20);
//         public static readonly ushort NegativeAck = new BurstChar(21);
//         public static readonly ushort SynchronousIdle = new BurstChar(22);
//         public static readonly ushort ETB = new BurstChar(23);
//         public static readonly ushort Cancel = new BurstChar(24);
//         public static readonly ushort EM = new BurstChar(25);
//         public static readonly ushort Substitute = new BurstChar(26);
//         public static readonly ushort Escape = new BurstChar(27);
//         public static readonly ushort FileSeparator = new BurstChar(28);
//         public static readonly ushort GroupSeparator = new BurstChar(29);
//         public static readonly ushort RecordSeparator = new BurstChar(30);
//         public static readonly ushort UnitSeparator = new BurstChar(31);
//         
//         public static readonly BurstChar Space = new BurstChar(32);
//         public static readonly BurstChar Exclaim = new BurstChar(33);
//         public static readonly BurstChar DoulbeQuote = new BurstChar(34);
//         public static readonly BurstChar Pound = new BurstChar(35);
//         public static readonly BurstChar Dollar = new BurstChar(36);
//         public static readonly BurstChar Percent = new BurstChar(37);
//         public static readonly BurstChar Amp = new BurstChar(38);
//         public static readonly BurstChar SingleQuote = new BurstChar(39);
//         public static readonly BurstChar OpenParen = new BurstChar(40);
//         public static readonly BurstChar CloseParen = new BurstChar(41);
//         public static readonly BurstChar Asterix = new BurstChar(42);
//         public static readonly BurstChar Plus = new BurstChar(43);
//         public static readonly BurstChar Comma = new BurstChar(44);
//         public static readonly BurstChar Minus = new BurstChar(45);
//         public static readonly BurstChar Dot = new BurstChar(46);
//         public static readonly BurstChar Backslash = new BurstChar(47);
//         public static readonly BurstChar Zero = new BurstChar(48);
//         public static readonly BurstChar One = new BurstChar(49);
//         public static readonly BurstChar Two = new BurstChar(50);
//         public static readonly BurstChar Three = new BurstChar(51);
//         public static readonly BurstChar Four = new BurstChar(52);
//         public static readonly BurstChar Five = new BurstChar(53);
//         public static readonly BurstChar Six = new BurstChar(54);
//         public static readonly BurstChar Seven = new BurstChar(55);
//         public static readonly BurstChar Eight = new BurstChar(56);
//         public static readonly BurstChar Nine = new BurstChar(57);
//         public static readonly BurstChar Colon = new BurstChar(58);
//         public static readonly BurstChar SemiColon = new BurstChar(59);
//         public static readonly BurstChar LessThan = new BurstChar(60);
//         public static readonly BurstChar Equal = new BurstChar(61);
//         public static readonly BurstChar GreaterThan = new BurstChar(62);
//         public static readonly BurstChar QuestionMark = new BurstChar(63);
//         
//         public static readonly BurstChar At = new BurstChar(64);
//         public static readonly BurstChar A = new BurstChar(65);
//         public static readonly BurstChar B = new BurstChar(66);
//         public static readonly BurstChar C = new BurstChar(67);
//         public static readonly BurstChar D = new BurstChar(68);
//         public static readonly BurstChar E = new BurstChar(69);
//         public static readonly BurstChar F = new BurstChar(70);
//         public static readonly BurstChar G = new BurstChar(71);
//         public static readonly BurstChar H = new BurstChar(72);
//         public static readonly BurstChar I = new BurstChar(73);
//         public static readonly BurstChar J = new BurstChar(74);
//         public static readonly BurstChar K = new BurstChar(75);
//         public static readonly BurstChar L = new BurstChar(76);
//         public static readonly BurstChar M = new BurstChar(77);
//         public static readonly BurstChar N = new BurstChar(78);
//         public static readonly BurstChar O = new BurstChar(79);
//         public static readonly BurstChar P = new BurstChar(80);
//         public static readonly BurstChar Q = new BurstChar(81);
//         public static readonly BurstChar R = new BurstChar(82);
//         public static readonly BurstChar S = new BurstChar(83);
//         public static readonly BurstChar T = new BurstChar(84);
//         public static readonly BurstChar U = new BurstChar(85);
//         public static readonly BurstChar V = new BurstChar(86);
//         public static readonly BurstChar W = new BurstChar(87);
//         public static readonly BurstChar X = new BurstChar(88);
//         public static readonly BurstChar Y = new BurstChar(89);
//         public static readonly BurstChar Z = new BurstChar(90);
//         public static readonly BurstChar OpenBrace = new BurstChar(91);
//         public static readonly BurstChar ForwardSlash = new BurstChar(92);
//         public static readonly BurstChar CloseBrace = new BurstChar(93);
//         public static readonly BurstChar Caret = new BurstChar(94);
//         public static readonly BurstChar Underscore = new BurstChar(95);
//         
//         public static readonly BurstChar BackTick = new BurstChar(96);
//         public static readonly BurstChar a = new BurstChar(97);
//         public static readonly BurstChar b = new BurstChar(98);
//         public static readonly BurstChar c = new BurstChar(99);
//         public static readonly BurstChar d = new BurstChar(100);
//         public static readonly BurstChar e = new BurstChar(101);
//         public static readonly BurstChar f = new BurstChar(102);
//         public static readonly BurstChar g = new BurstChar(103);
//         public static readonly BurstChar h = new BurstChar(104);
//         public static readonly BurstChar i = new BurstChar(105);
//         public static readonly BurstChar j = new BurstChar(106);
//         public static readonly BurstChar k = new BurstChar(107);
//         public static readonly BurstChar l = new BurstChar(108);
//         public static readonly BurstChar m = new BurstChar(109);
//         public static readonly BurstChar n = new BurstChar(110);
//         public static readonly BurstChar o = new BurstChar(111);
//         public static readonly BurstChar p = new BurstChar(112);
//         public static readonly BurstChar q = new BurstChar(113);
//         public static readonly BurstChar r = new BurstChar(114);
//         public static readonly BurstChar s = new BurstChar(115);
//         public static readonly BurstChar t = new BurstChar(116);
//         public static readonly BurstChar u = new BurstChar(117);
//         public static readonly BurstChar v = new BurstChar(118);
//         public static readonly BurstChar w = new BurstChar(119);
//         public static readonly BurstChar x = new BurstChar(120);
//         public static readonly BurstChar y = new BurstChar(121);
//         public static readonly BurstChar z = new BurstChar(122);
//         public static readonly BurstChar OpenBracket = new BurstChar(123);
//         public static readonly BurstChar Or = new BurstChar(124);
//         public static readonly BurstChar CloseBracket = new BurstChar(125);
//         public static readonly BurstChar Grave = new BurstChar(126);
//         public static readonly BurstChar Delete = new BurstChar(127);
//         
//         
//         
//         
//  //  0  NUL (null)                      32  SPACE     64  @         96  `
//  //  1  SOH (start of heading)          33  !         65  A         97  a
//  //  2  STX (start of text)             34  "         66  B         98  b
//  //  3  ETX (end of text)               35  #         67  C         99  c
//  //  4  EOT (end of transmission)       36  $         68  D        100  d
//  //  5  ENQ (enquiry)                   37  %         69  E        101  e
//  //  6  ACK (acknowledge)               38  &         70  F        102  f
//  //  7  BEL (bell)                      39  '         71  G        103  g
//  //  8  BS  (backspace)                 40  (         72  H        104  h
//  //  9  TAB (horizontal tab)            41  )         73  I        105  i
//  // 10  LF  (NL line feed, new line)    42  *         74  J        106  j
//  // 11  VT  (vertical tab)              43  +         75  K        107  k
//  // 12  FF  (NP form feed, new page)    44  ,         76  L        108  l
//  // 13  CR  (carriage return)           45  -         77  M        109  m
//  // 14  SO  (shift out)                 46  .         78  N        110  n
//  // 15  SI  (shift in)                  47  /         79  O        111  o
//  // 16  DLE (data link escape)          48  0         80  P        112  p
//  // 17  DC1 (device control 1)          49  1         81  Q        113  q
//  // 18  DC2 (device control 2)          50  2         82  R        114  r
//  // 19  DC3 (device control 3)          51  3         83  S        115  s
//  // 20  DC4 (device control 4)          52  4         84  T        116  t
//  // 21  NAK (negative acknowledge)      53  5         85  U        117  u
//  // 22  SYN (synchronous idle)          54  6         86  V        118  v
//  // 23  ETB (end of trans. block)       55  7         87  W        119  w
//  // 24  CAN (cancel)                    56  8         88  X        120  x
//  // 25  EM  (end of medium)             57  9         89  Y        121  y
//  // 26  SUB (substitute)                58  :         90  Z        122  z
//  // 27  ESC (escape)                    59  ;         91  [        123  {
//  // 28  FS  (file separator)            60  <         92  \        124  |
//  // 29  GS  (group separator)           61  =         93  ]        125  }
//  // 30  RS  (record separator)          62  >         94  ^        126  ~
//  // 31  US  (unit separator)            63  ?         95  _        127  DEL
//
//     }
//
// }