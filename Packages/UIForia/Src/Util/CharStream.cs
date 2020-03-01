using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIForia.Util {

    public struct CharStream {

        private char[] data;
        private uint dataStart;
        private uint dataEnd;
        private uint ptr;

        public CharStream(char[] data, uint dataStart, uint dataEnd) {
            this.data = data;
            this.dataStart = dataStart;
            this.dataEnd = dataEnd;
            this.ptr = dataStart;
        }

        public CharStream(char[] data) {
            this.data = data;
            this.dataStart = 0;
            this.dataEnd = (uint) data.Length;
            this.ptr = dataStart;
        }

        public bool HasMoreTokens => ptr < dataEnd;
        public uint Size => dataEnd - dataStart;
        public char[] Data => data;
        public uint Ptr => ptr;
        public uint End => dataEnd;

        public char this[uint idx] {
            get => data[idx];
        }

        public void Advance(uint advance = 1) {
            ptr += advance;
        }

        public void AdvanceSkipWhitespace(uint advance = 1) {
            ptr += advance;
            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }
        }

        public bool TryMatchRange(string str, out uint advance) {
            if (ptr + str.Length >= dataEnd) {
                advance = 0;
                return false;
            }

            unsafe {

                fixed (char* s = str) {
                    for (int i = 0; i < str.Length; i++) {
                        if (data[ptr + i] != *s) {
                            advance = 0;
                            return false;
                        }
                    }
                }

            }

            advance = (uint) str.Length;
            return true;

        }

        public bool TryMatchRange(string str, char optional, out uint advance) {
            if (TryMatchRange(str, out advance)) {
                if (data[ptr] == optional) {
                    advance += 1;
                }
            }

            advance = 0;
            return false;
        }

        public static bool operator ==(CharStream stream, char character) {
            if (stream.ptr >= stream.dataEnd) return false;
            return stream.data[stream.ptr] == character;
        }

        public static bool operator !=(CharStream stream, char character) {
            return !(stream == character);
        }

        public bool TryGetSubStream(char open, char close, out CharStream charStream) {
            if (data[ptr] != open) {
                charStream = default;
                return false;
            }

            uint i = ptr;
            int counter = 1;
            while (i < dataEnd) {
                i++;

                if (data[i] == open) {
                    counter++;
                }
                else if (data[i] == close) {
                    counter--;
                    if (counter == 0) {
                        charStream = new CharStream(data, ptr + 1, i);
                        return true;
                    }
                }
            }

            charStream = default;
            return false;
        }

        public void RewindTo(uint start) {
            ptr = start < dataStart ? start : dataStart;
        }

    }

    public static class CharacterStreamExtensions {

        [ThreadStatic] private static string s_ScratchBuffer;
        [ThreadStatic] private static List<EnumNameEntry> s_EnumNameEntryList;

        private const int s_ScratchBufferLength = 128;

        private struct EnumNameEntry {

            public Type type;
            public string[] names;
            public int[] values;

        }

        private static bool TryParseIdentifier(this CharStream stream, out int rangeStart, out int rangeEnd) {

            char first = stream.Data[stream.Ptr];

            if (!char.IsLetter(first) && first != '_') {
                rangeStart = -1;
                rangeEnd = -1;
                return false;
            }

            uint ptr = stream.Ptr;
            while (ptr < stream.End) {
                char c = stream[ptr];
                if (!char.IsLetterOrDigit(c) && c != '_') {
                    break;
                }

                ptr++;
            }

            uint length = ptr - stream.Ptr;
            if (length > 0) {
                rangeStart = (int) stream.Ptr;
                rangeEnd = (int) ptr;
                stream.Advance(ptr);
                return true;
            }

            rangeStart = -1;
            rangeEnd = -1;
            return false;
        }

        public static bool TryParseByte(this CharStream stream, out byte value) {
            if (TryParseUInt(stream, out uint val) && val <= byte.MaxValue) {
                value = (byte) val;
                return true;
            }

            value = 0;
            return false;
        }

        public static bool TryParseUShort(this CharStream stream, out ushort value) {
            if (TryParseUInt(stream, out uint val) && val <= ushort.MaxValue) {
                value = (ushort) val;
                return true;
            }

            value = 0;
            return false;
        }

        public static bool TryParseFloat(this CharStream stream, out float value) {

            if (s_ScratchBuffer == null) {
                s_ScratchBuffer = new string('\0', s_ScratchBufferLength);
            }

            uint start = stream.Ptr;

            unsafe {

                // oh, you thought C# strings were immutable? How cute :)

                // writing a float.Parse function is hard and error prone so I want to the use the C# built in one.
                // Somebody at Microsoft thought it would be a good idea to only support parsing float from strings though
                // and didn't consider the character range use case that we need. So I take a string, set its contents
                // to my data, pass that string to float.TryParse, and use the result. 
                int cnt = 0;
                fixed (char* charptr = s_ScratchBuffer) {

                    uint idx = start;
                    int dotIndex = -1;
                    if (stream == '-') {
                        charptr[cnt++] = '-';
                        idx++;
                    }

                    // read until end or whitespace
                    while (idx < stream.End && cnt < s_ScratchBufferLength) {
                        char c = stream[idx];
                        if (c < '0' || c > '9') {

                            if (c == '.' && dotIndex == -1) {
                                dotIndex = (int) idx;
                                charptr[cnt++] = c;
                                idx++;
                                continue;
                            }

                            break;
                        }

                        charptr[cnt++] = c;
                        idx++;

                    }

                    bool retn = float.TryParse(s_ScratchBuffer, out value);

                    // reset the scratch buffer so the next call has valid state.
                    // treat our charptr as long * so that we can clear it in fewer operations (char = 16 bits, long = 64)
                    // avoiding divide, also avoiding Math.Ceil call
                    // instead just clamp to bufferSize / sizeof(long) which happens to be 16
                    int longCnt = (int) (cnt * 0.25f) + 1;
                    long* longptr = (long*) charptr;

                    if (longCnt > 16) longCnt = 16;

                    for (int i = 0; i < longCnt; i++) {
                        longptr[i] = 0;
                    }

                    if (retn) {
                        stream.Advance((uint) cnt);
                    }

                    return retn;
                }

            }
        }

        public static bool TryParseUInt(this CharStream stream, out uint intVal) {
            uint i = stream.Ptr;

            // read until end or whitespace
            while (i < stream.End) {
                char c = stream[i];
                if (c < '0' || c > '9') {
                    break;
                }

                i++;
            }

            if (i == stream.Ptr) {
                intVal = default;
                return false;
            }

            uint length = i - stream.Ptr;
            stream.Advance(length);

            int number = 0;
            int mult = 1;

            while (length-- != 0) {
                number += (stream.Data[length] - '0') * mult;
                mult *= 10;
            }

            intVal = (uint) number;

            return true;
        }

        public static bool TryParseInt(this CharStream stream, out int intVal) {
            int sign = 1;
            uint minusSize = 0;
            uint i = stream.Ptr;

            if (stream == '-') {
                sign = -1;
                minusSize = 1;
                i++;
            }

            // read until end or whitespace
            while (i < stream.End) {
                char c = stream[i];
                if (c < '0' || c > '9') {
                    break;
                }

                i++;
            }

            if (i == stream.Ptr) {
                intVal = default;
                return false;
            }

            uint length = i - stream.Ptr;
            stream.Advance(length);

            int number = 0;
            int mult = 1;

            while (length-- != minusSize) {
                number += (stream.Data[length] - '0') * mult;
                mult *= 10;
            }

            intVal = number * sign;

            return true;
        }

        public static bool TryParseEnum<T>(this CharStream stream, out int enumValue) where T : Enum {

            if (s_EnumNameEntryList == null) {
                s_EnumNameEntryList = new List<EnumNameEntry>();
            }

            string[] names = null;
            int[] values = null;
            
            for (int i = 0; i < s_EnumNameEntryList.Count; i++) {
                EnumNameEntry nameEntry = s_EnumNameEntryList[i];
                if (typeof(T) == nameEntry.type) {
                    names = nameEntry.names;
                    values = nameEntry.values;
                    break;
                }
            }

            if (names == null) {
                names = Enum.GetNames(typeof(T));
                values = (int[])Enum.GetValues(typeof(T));
                s_EnumNameEntryList.Add(new EnumNameEntry() {
                    type = typeof(T),
                    names = names
                });
            }

            if (!stream.TryParseIdentifier(out int rangeStart, out int rangeEnd)) {
                enumValue = default;
                return false;
            }

            for (int i = 0; i < names.Length; i++) {
                string name = names[i];
                if (StringUtil.EqualsRangeUnsafe(name, stream.Data, rangeStart, rangeEnd - rangeStart)) {
                    enumValue = values[i];
                    return true;
                }
            }

            enumValue = default;
            return false;
        }

        public static bool TryParseColorProperty(this CharStream stream, out Color32 color) {
            if (stream.TryMatchRange("rgb", 'a', out uint advance)) {
                stream.AdvanceSkipWhitespace(advance);

                if (stream.TryGetSubStream('(', ')', out CharStream signature)) {
                    stream.Advance(signature.Size + 1);
                    // expect four comma separated floats or integers

                    if (!TryParseInt(stream, out int r)) { }

                    if (!TryParseInt(stream, out int g)) { }

                }

            }

            // else if (ColorUtil.TryParseColorName(stream.Data, stream.Ptr, stream.End, out color)) {
            //     return true;
            // }

            color = default;
            return false;

        }

    }

}