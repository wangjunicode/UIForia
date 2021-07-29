using System;
using UnityEngine;

namespace UIForia {
    
    public partial class StringMemoizer {
    
        public string MemoizeStringString(string a, string b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeStringFloat(string a, float b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeStringInt(string a, int b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeStringByte(string a, byte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeStringSByte(string a, sbyte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeStringUInt(string a, uint b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeStringShort(string a, short b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeStringUShort(string a, ushort b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeStringChar(string a, char b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeStringBool(string a, bool b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeStringLong(string a, long b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeStringULong(string a, ulong b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeStringBuffer<T>(string a, T b) where T : IToStringBuffer {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatString(float a, string b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatFloat(float a, float b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatInt(float a, int b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatByte(float a, byte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatSByte(float a, sbyte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatUInt(float a, uint b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatShort(float a, short b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatUShort(float a, ushort b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatChar(float a, char b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatBool(float a, bool b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatLong(float a, long b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatULong(float a, ulong b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeFloatBuffer<T>(float a, T b) where T : IToStringBuffer {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntString(int a, string b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntFloat(int a, float b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntInt(int a, int b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntByte(int a, byte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntSByte(int a, sbyte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntUInt(int a, uint b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntShort(int a, short b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntUShort(int a, ushort b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntChar(int a, char b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntBool(int a, bool b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntLong(int a, long b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntULong(int a, ulong b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeIntBuffer<T>(int a, T b) where T : IToStringBuffer {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteString(byte a, string b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteFloat(byte a, float b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteInt(byte a, int b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteByte(byte a, byte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteSByte(byte a, sbyte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteUInt(byte a, uint b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteShort(byte a, short b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteUShort(byte a, ushort b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteChar(byte a, char b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteBool(byte a, bool b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteLong(byte a, long b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteULong(byte a, ulong b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeByteBuffer<T>(byte a, T b) where T : IToStringBuffer {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteString(sbyte a, string b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteFloat(sbyte a, float b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteInt(sbyte a, int b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteByte(sbyte a, byte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteSByte(sbyte a, sbyte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteUInt(sbyte a, uint b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteShort(sbyte a, short b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteUShort(sbyte a, ushort b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteChar(sbyte a, char b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteBool(sbyte a, bool b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteLong(sbyte a, long b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteULong(sbyte a, ulong b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeSByteBuffer<T>(sbyte a, T b) where T : IToStringBuffer {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntString(uint a, string b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntFloat(uint a, float b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntInt(uint a, int b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntByte(uint a, byte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntSByte(uint a, sbyte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntUInt(uint a, uint b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntShort(uint a, short b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntUShort(uint a, ushort b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntChar(uint a, char b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntBool(uint a, bool b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntLong(uint a, long b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntULong(uint a, ulong b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUIntBuffer<T>(uint a, T b) where T : IToStringBuffer {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortString(short a, string b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortFloat(short a, float b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortInt(short a, int b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortByte(short a, byte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortSByte(short a, sbyte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortUInt(short a, uint b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortShort(short a, short b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortUShort(short a, ushort b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortChar(short a, char b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortBool(short a, bool b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortLong(short a, long b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortULong(short a, ulong b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeShortBuffer<T>(short a, T b) where T : IToStringBuffer {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortString(ushort a, string b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortFloat(ushort a, float b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortInt(ushort a, int b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortByte(ushort a, byte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortSByte(ushort a, sbyte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortUInt(ushort a, uint b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortShort(ushort a, short b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortUShort(ushort a, ushort b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortChar(ushort a, char b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortBool(ushort a, bool b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortLong(ushort a, long b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortULong(ushort a, ulong b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeUShortBuffer<T>(ushort a, T b) where T : IToStringBuffer {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharString(char a, string b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharFloat(char a, float b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharInt(char a, int b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharByte(char a, byte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharSByte(char a, sbyte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharUInt(char a, uint b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharShort(char a, short b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharUShort(char a, ushort b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharChar(char a, char b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharBool(char a, bool b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharLong(char a, long b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharULong(char a, ulong b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeCharBuffer<T>(char a, T b) where T : IToStringBuffer {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolString(bool a, string b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolFloat(bool a, float b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolInt(bool a, int b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolByte(bool a, byte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolSByte(bool a, sbyte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolUInt(bool a, uint b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolShort(bool a, short b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolUShort(bool a, ushort b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolChar(bool a, char b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolBool(bool a, bool b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolLong(bool a, long b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolULong(bool a, ulong b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeBoolBuffer<T>(bool a, T b) where T : IToStringBuffer {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongString(long a, string b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongFloat(long a, float b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongInt(long a, int b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongByte(long a, byte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongSByte(long a, sbyte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongUInt(long a, uint b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongShort(long a, short b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongUShort(long a, ushort b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongChar(long a, char b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongBool(long a, bool b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongLong(long a, long b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongULong(long a, ulong b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeLongBuffer<T>(long a, T b) where T : IToStringBuffer {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongString(ulong a, string b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongFloat(ulong a, float b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongInt(ulong a, int b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongByte(ulong a, byte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongSByte(ulong a, sbyte b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongUInt(ulong a, uint b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongShort(ulong a, short b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongUShort(ulong a, ushort b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongChar(ulong a, char b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongBool(ulong a, bool b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongLong(ulong a, long b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongULong(ulong a, ulong b) {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }

        public string MemoizeULongBuffer<T>(ulong a, T b) where T : IToStringBuffer {
            buffer.size = 0;
            buffer.Append(a);
            buffer.Append(b);
            return Memoize();
        }



    }

}