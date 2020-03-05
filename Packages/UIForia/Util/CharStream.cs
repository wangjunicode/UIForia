using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UIForia.Util {

    public unsafe struct CharStream {

        private char* data;
        private uint dataStart;
        private uint dataEnd;
        private uint ptr;

        [ThreadStatic] private static string s_ScratchBuffer;
        [ThreadStatic] private static List<EnumNameEntry> s_EnumNameEntryList;

        public CharStream(char[] data, uint dataStart, uint dataEnd) {
            fixed (char* dataptr = data) {
                this.data = dataptr;
            }

            this.dataStart = dataStart;
            this.dataEnd = dataEnd;
            this.ptr = dataStart;

        }

        public CharStream(char[] data) {

            fixed (char* dataptr = data) {
                this.data = dataptr;
            }

            this.dataStart = 0;
            this.dataEnd = (uint) data.Length;
            this.ptr = dataStart;
        }

        public CharStream(char[] source, ReflessCharSpan span) {
            fixed (char* dataptr = source) {
                this.data = dataptr;
            }

            this.dataStart = (uint) span.rangeStart;
            this.dataEnd = (uint) span.rangeEnd;
            this.ptr = dataStart;
        }

        public CharStream(CharSpan span) {
            this.data = span.data;
            this.dataStart = (uint) span.rangeStart;
            this.dataEnd = (uint) span.rangeEnd;
            this.ptr = dataStart;
        }

        private CharStream(char* source, uint start, uint end) {
            this.data = source;
            this.dataStart = start;
            this.dataEnd = end;
            this.ptr = dataStart;

        }

        // copy from current pointer position
        public CharStream(CharStream propertyStream) {
            this.data = propertyStream.data;
            this.dataStart = propertyStream.ptr;
            this.dataEnd = propertyStream.dataEnd;
            this.ptr = propertyStream.ptr;
        }

        public bool HasMoreTokens => ptr < dataEnd;
        public uint Size => dataEnd - dataStart;
        public char* Data => data;
        public uint Ptr => ptr;
        public uint End => dataEnd;
        public int IntPtr => (int) ptr;

        public char this[uint idx] {
            get => data[idx];
        }

        public void Advance(uint advance = 1) {
            ptr += advance;
            if (ptr >= dataEnd) {
                ptr = dataEnd;
            }
        }

        public void AdvanceTo(uint target) {
            if (target < ptr) return;
            ptr = target;
            if (ptr >= dataEnd) {
                ptr = dataEnd;
            }
        }

        public void AdvanceTo(int target) {
            if (target < ptr) return;
            ptr = (uint) target;
            if (ptr >= dataEnd) {
                ptr = dataEnd;
            }
        }

        public void AdvanceSkipWhitespace(uint advance = 1) {
            ptr += advance;
            if (ptr >= dataEnd) {
                ptr = dataEnd;
            }

            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }
        }

        public bool TryMatchRange(string str) {
            if (ptr + str.Length >= dataEnd) {
                return false;
            }

            fixed (char* s = str) {
                for (int i = 0; i < str.Length; i++) {
                    if (data[ptr + i] != s[i]) {
                        return false;
                    }
                }
            }

            Advance((uint) str.Length);
            return true;
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

        public bool TryMatchRange(string str, char optional, out bool usedOptional, out uint advance) {
            if (TryMatchRange(str, out advance)) {
                if (data[ptr] == optional) {
                    usedOptional = true;
                    advance += 1;
                }
            }

            usedOptional = false;
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

        public bool TryGetSubStream(char open, char close, out CharStream charStream, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            uint start = ptr;
            if ((whitespaceHandling & WhitespaceHandling.ConsumeBefore) != 0) {
                ConsumeWhiteSpace();
            }

            if (data[ptr] != open) {
                charStream = default;
                RewindTo(start);
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
                        ptr = i + 1; // step over close
                        if ((whitespaceHandling & WhitespaceHandling.ConsumeAfter) != 0) {
                            ConsumeWhiteSpace();
                        }

                        return true;
                    }
                }
            }

            RewindTo(start);
            charStream = default;
            return false;
        }

        public void RewindTo(uint start) {
            ptr = start < dataStart ? dataStart : start;
        }

        public void ConsumeWhiteSpace() {
            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }
        }

        public bool TryGetSubstreamTo(char c0, char c1, out CharStream stream) {
            uint i = ptr;
            while (i < dataEnd) {
                char c = data[i];
                if (c == c0 || c == c1) {
                    stream = new CharStream(data, ptr, i);
                    Advance(i - ptr + 1);
                    return true;
                }

                i++;
            }

            stream = default;
            return false;
        }

        public bool TryGetCharSpanTo(char c0, char c1, out CharSpan span) {
            uint i = ptr;
            while (i < dataEnd) {
                char c = data[i];
                if (c == c0 || c == c1) {
                    span = new CharSpan(data, (int) ptr, (int) i);
                    Advance(i - ptr + 1);
                    return true;
                }

                i++;
            }

            span = default;
            return false;
        }

        public bool TryGetSubstreamTo(char c0, out CharStream stream) {
            uint i = ptr;
            while (i < dataEnd) {
                char c = data[i];
                if (c == c0) {
                    stream = new CharStream(data, ptr, i);
                    Advance(i - ptr + 1);
                    return true;
                }

                i++;
            }

            stream = default;
            return false;
        }

        public bool TryGetDelimitedSubstream(char target, out CharStream stream, char c1 = '\0', char c2 = '\0', char c3 = '\0') {
            uint i = ptr;
            while (i < dataEnd) {
                char c = data[i];
                if (c == target) {
                    stream = new CharStream(data, ptr, i);
                    Advance(i - ptr + 1);
                    return true;
                }
                else if (c == c1 || c == c2 || c == c3) {
                    stream = default;
                    return false;
                }

                i++;
            }

            stream = default;
            return false;
        }

        public int NextIndexOf(char c) {
            uint i = ptr;
            while (i < dataEnd) {
                if (data[i] == c) {
                    return (int) i;
                }

                i++;
            }

            return -1;
        }

        public bool Contains(char c) {
            return NextIndexOf(c) != -1;
        }

        public int GetLineNumber() {
            int line = 0;
            for (int i = 0; i < ptr; i++) {
                if (data[i] == '\n') line++;
            }

            return line;
        }

        public int GetStartLineNumber() {
            int line = 0;
            for (int i = 0; i < dataStart; i++) {
                if (data[i] == '\n') line++;
            }

            return line;
        }

        public int GetEndLineNumber() {
            int line = 0;
            for (int i = 0; i < dataEnd; i++) {
                if (data[i] == '\n') line++;
            }

            return line;
        }

        private const int s_ScratchBufferLength = 128;

        private struct EnumNameEntry {

            public Type type;
            public string[] names;
            public int[] values;

        }

        public bool TryParseIdentifier(out CharSpan span, bool allowMinus = true, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            uint start = ptr;

            if ((whitespaceHandling & WhitespaceHandling.ConsumeBefore) != 0) {
                ConsumeWhiteSpace();
            }

            if (TryParseIdentifier(out int rangeStart, out int rangeEnd, allowMinus)) {
                span = new CharSpan(data, rangeStart, rangeEnd);
                if ((whitespaceHandling & WhitespaceHandling.ConsumeAfter) != 0) {
                    ConsumeWhiteSpace();
                }

                return true;
            }

            RewindTo(start);
            span = default;
            return false;
        }

        public bool TryParseIdentifier(out int rangeStart, out int rangeEnd, bool allowMinus = true) {
            char first = data[ptr];

            if (!char.IsLetter(first) && first != '_') {
                rangeStart = -1;
                rangeEnd = -1;
                return false;
            }

            uint ptr2 = ptr;
            while (ptr2 < End) {
                char c = data[ptr2];
                if (!char.IsLetterOrDigit(c) && c != '_' && (allowMinus && c != '-')) {
                    break;
                }

                ptr2++;
            }

            uint length = ptr2 - ptr;
            if (length > 0) {
                rangeStart = (int) ptr;
                rangeEnd = (int) ptr2;
                Advance(length);
                return true;
            }

            rangeStart = -1;
            rangeEnd = -1;
            return false;
        }

        public bool TryParseByte(out byte value) {
            if (TryParseUInt(out uint val) && val <= byte.MaxValue) {
                value = (byte) val;
                return true;
            }

            value = 0;
            return false;
        }

        public bool TryParseUShort(out ushort value) {
            if (TryParseUInt(out uint val) && val <= ushort.MaxValue) {
                value = (ushort) val;
                return true;
            }

            value = 0;
            return false;
        }

        public bool TryParseFloat(out float value) {
            if (s_ScratchBuffer == null) {
                s_ScratchBuffer = new string('\0', s_ScratchBufferLength);
            }

            uint start = ptr;

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
                    if (data[ptr] == '-') {
                        charptr[cnt++] = '-';
                        idx++;
                    }

                    // read until end or whitespace
                    while (idx < dataEnd && cnt < s_ScratchBufferLength) {
                        char c = data[idx];
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

                    bool retn = float.TryParse(s_ScratchBuffer, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);

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
                        Advance((uint) cnt);
                    }

                    return retn;
                }
            }
        }

        public bool TryParseUInt(out uint intVal) {
            uint i = ptr;

            // read until end or whitespace
            while (i < dataEnd) {
                char c = data[i];
                if (c < '0' || c > '9') {
                    break;
                }

                i++;
            }

            if (i == ptr) {
                intVal = default;
                return false;
            }

            uint length = i - ptr;
            Advance(length);

            int number = 0;
            int multiplier = 1;

            while (length-- != 0) {
                number += (data[length] - '0') * multiplier;
                multiplier *= 10;
            }

            intVal = (uint) number;

            return true;
        }

        public bool TryParseInt(out int intVal) {
            int sign = 1;
            uint minusSize = 0;
            uint i = ptr;

            if (data[ptr] == '-') {
                sign = -1;
                minusSize = 1;
                i++;
            }

            // read until end or whitespace
            while (i < dataEnd) {
                char c = data[i];
                if (c < '0' || c > '9') {
                    break;
                }

                i++;
            }

            if (i == ptr) {
                intVal = default;
                return false;
            }

            uint length = i - ptr;
            Advance(length);

            int number = 0;
            int mult = 1;

            while (length-- != minusSize) {
                number += (data[length] - '0') * mult;
                mult *= 10;
            }

            intVal = number * sign;

            return true;
        }

        // Cannot cast T to int to T without boxing, so we return the integer and expect the caller to cast to enum type
        public bool TryParseEnum<T>(out int enumValue) where T : Enum {
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
                values = (int[]) Enum.GetValues(typeof(T));
                s_EnumNameEntryList.Add(new EnumNameEntry() {
                    type = typeof(T),
                    names = names,
                    values = values
                });
            }

            if (!TryParseIdentifier(out int rangeStart, out int rangeEnd)) {
                enumValue = default;
                return false;
            }

            for (int i = 0; i < names.Length; i++) {
                string name = names[i];
                if (StringUtil.EqualsRangeUnsafe(name, data, rangeStart, rangeEnd - rangeStart)) {
                    enumValue = values[i];
                    return true;
                }
            }

            enumValue = default;
            return false;
        }

        public bool TryParseCharacter(char character, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            uint save = ptr;

            if ((whitespaceHandling & WhitespaceHandling.ConsumeBefore) != 0) {
                ConsumeWhiteSpace();
            }

            if (data[ptr] == character) {
                Advance();

                if ((whitespaceHandling & WhitespaceHandling.ConsumeAfter) != 0) {
                    ConsumeWhiteSpace();
                }

                return true;
            }

            RewindTo(save);

            return false;
        }

        public bool TryParseColorProperty(out Color32 color) {
            uint start = ptr;

            ConsumeWhiteSpace();

            if (TryMatchRange("rgb", 'a', out bool usedOptional, out uint advance)) {
                AdvanceSkipWhitespace(advance);

                if (TryGetSubStream('(', ')', out CharStream signature)) {
                    Advance(signature.Size + 1);

                    byte a = 255;

                    // expect four comma separated floats or integers

                    if (!signature.TryParseByte(out byte r)) {
                        goto fail;
                    }

                    if (!signature.TryParseCharacter(',')) {
                        goto fail;
                    }

                    if (!signature.TryParseByte(out byte g)) {
                        goto fail;
                    }

                    if (!signature.TryParseCharacter(',')) {
                        goto fail;
                    }

                    if (!signature.TryParseByte(out byte b)) {
                        goto fail;
                    }

                    if (usedOptional && (!signature.TryParseCharacter(',') || !signature.TryParseByte(out a))) {
                        goto fail;
                    }

                    color = new Color32(r, g, b, a);
                }
            }
            else if (data[ptr] == '#') {
                // read 6 or 8 characters
                // https://www.includehelp.com/code-snippets/convert-hexadecimal-string-to-integer-in-c-programming.aspx

                unsafe {
                    char* buffer = stackalloc char[8];

                    int i = 0;
                    bool valid = true;
                    for (i = 0; i < 8; i++) {
                        if (ptr + i <= dataEnd) {
                            break;
                        }

                        char c = data[ptr + i];

                        if (c >= '0' && c <= '9') {
                            buffer[i] = c;
                        }
                        else {
                            switch (c) {
                                case 'A':
                                case 'a':
                                case 'B':
                                case 'b':
                                case 'C':
                                case 'c':
                                case 'D':
                                case 'd':
                                case 'E':
                                case 'e':
                                case 'F':
                                case 'f':
                                    buffer[i] = c;
                                    break;

                                default: {
                                    valid = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (!valid || (i != 6 && i != 8)) {
                        color = default;
                        return false;
                    }

                    if (i == 6) {
                        buffer[6] = 'F';
                        buffer[7] = 'F';
                    }

                    int digit = 0;
                    int intValue = 0;

                    for (int x = 7, p = 0; x >= 0; x--, p++) {
                        char c = buffer[x];
                        if (c >= '0' || c <= '9') {
                            digit = c - 0x30;
                        }
                        else {
                            switch (c) {
                                case 'A':
                                case 'a':
                                    digit = 10;
                                    break;

                                case 'B':
                                case 'b':
                                    digit = 11;
                                    break;

                                case 'C':
                                case 'c':
                                    digit = 12;
                                    break;

                                case 'D':
                                case 'd':
                                    digit = 13;
                                    break;

                                case 'E':
                                case 'e':
                                    digit = 14;
                                    break;

                                case 'F':
                                case 'f':
                                    digit = 15;
                                    break;
                            }
                        }

                        intValue = digit * (int) Mathf.Pow(16, p);
                    }

                    color = ColorUtil.ColorFromInt(intValue);
                    return true;
                }
            }

            else if (ColorUtil.TryParseColorName(new CharSpan(data, (int) ptr, (int) dataEnd), out color, out int nameLength)) {
                Advance((uint) nameLength);
                return true;
            }

            fail:
            RewindTo(start);
            color = default;
            return false;
        }

        public override string ToString() {
            return new string(data, (int) ptr, (int) (dataEnd - ptr));
        }

        public bool TryMatchRangeIgnoreCase(string str, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            uint start = ptr;
            if ((whitespaceHandling & WhitespaceHandling.ConsumeBefore) != 0) {
                ConsumeWhiteSpace();
            }

            if (str.Length == 1 && ptr <= dataEnd && str[0] == data[ptr]) {
                ptr++;
                if ((whitespaceHandling & WhitespaceHandling.ConsumeAfter) != 0) {
                    ConsumeWhiteSpace();
                }

                return true;
            }

            if (ptr + str.Length >= dataEnd) {
                ptr = start;
                return false;
            }

            fixed (char* s = str) {
                for (int i = 0; i < str.Length; i++) {
                    char strChar = s[i];
                    char dataChar = data[ptr + i];

                    char c1 = strChar >= 'a' && strChar <= 'z' ? char.ToLower(strChar) : strChar;
                    char c2 = dataChar >= 'a' && dataChar <= 'z' ? char.ToLower(dataChar) : dataChar;

                    if (c1 != c2) {
                        ptr = start;
                        return false;
                    }
                }
            }

            Advance((uint) str.Length);

            if ((whitespaceHandling & WhitespaceHandling.ConsumeAfter) != 0) {
                ConsumeWhiteSpace();
            }

            return true;

        }

        public void CopyRangeTo(char* charBuffer, object bufferStart, uint streamPtr, int idx) {
            throw new NotImplementedException();
        }

        public void Rewind(uint i) {
            ptr -= i;
            if (ptr < dataStart) ptr = dataStart;
        }

    }

    [Flags]
    public enum WhitespaceHandling {

        None = 0,
        ConsumeBefore = 1 << 0,
        ConsumeAfter = 1 << 1,
        ConsumeAll = ConsumeBefore | ConsumeAfter

    }

    public struct ReflessCharSpan {

        public readonly int rangeStart;
        public readonly int rangeEnd;

        public int Length => rangeEnd - rangeStart;

        public ReflessCharSpan(int rangeStart, int rangeEnd) {
            this.rangeStart = rangeStart;
            this.rangeEnd = rangeEnd;
        }

        public ReflessCharSpan(CharSpan span) {
            this.rangeStart = span.rangeStart;
            this.rangeEnd = span.rangeEnd;
        }

        public ReflessCharSpan(CharStream stream) {
            this.rangeStart = (int) stream.Ptr;
            this.rangeEnd = (int) stream.End;
        }

        public static string MakeString(in ReflessCharSpan span, char[] data) {
            return new string(data, span.rangeStart, span.rangeEnd - span.rangeStart);
        }

        public string MakeLowerString(char[] data) {
            int length = rangeEnd - rangeStart;

            unsafe {
                char* buffer = stackalloc char[length + 1];
                int idx = 0;
                for (int i = rangeStart; i < rangeEnd; i++) {
                    buffer[idx++] = char.ToLower(data[i]);
                }

                buffer[length] = '\0';
                return new string(buffer);
            }
        }

    }

    public unsafe struct CharSpan : IEquatable<CharSpan> {

        public readonly int rangeStart;
        public readonly int rangeEnd;

        public char* data { get; }

        public bool HasValue => Length > 0;

        public int Length => data != null ? rangeEnd - rangeStart : 0;

        public CharSpan(char[] data, int rangeStart, int rangeEnd) {
            fixed (char* charptr = data) {
                this.data = charptr;
            }

            this.rangeStart = rangeStart;
            this.rangeEnd = rangeEnd;
        }

        public CharSpan(char* data, int rangeStart, int rangeEnd) {
            this.data = data;
            this.rangeStart = rangeStart;
            this.rangeEnd = rangeEnd;
        }

        public CharSpan(CharStream stream) {
            this.data = stream.Data;
            this.rangeStart = (int) stream.Ptr;
            this.rangeEnd = (int) stream.End;
        }

        public CharSpan(string data) {
            fixed (char* ptr = data) {
                this.data = ptr;
                this.rangeStart = 0;
                this.rangeEnd = data.Length;
            }
        }

        public static bool operator ==(CharSpan a, string b) {
            return StringUtil.EqualsRangeUnsafe(b, a);
        }

        public static bool operator !=(CharSpan a, string b) {
            return !StringUtil.EqualsRangeUnsafe(b, a);
        }

        public static bool operator !=(string b, CharSpan a) {
            return !StringUtil.EqualsRangeUnsafe(b, a);
        }

        public static bool operator ==(string b, CharSpan a) {
            return !(b != a);
        }

        public static bool operator ==(CharSpan a, CharSpan b) {
            // this is a value comparison
            int aLen = a.rangeEnd - a.rangeStart;
            int bLen = b.rangeEnd - b.rangeStart;
            if (aLen != bLen) return false;
            int ptrA = a.rangeStart;
            int ptrB = b.rangeStart;
            for (int i = 0; i < aLen; i++) {
                if (a.data[i + ptrA] != b.data[i + ptrB]) {
                    return false;
                }

                ptrA++;
                ptrB++;
            }

            return true;
        }

        public static bool operator !=(CharSpan a, CharSpan b) {
            return !(a == b);
        }

        public bool Equals(CharSpan other) {
            return this == other;
        }

        public override bool Equals(object obj) {
            return obj is CharSpan other && Equals(other);
        }

        public override int GetHashCode() {

#if WIN32
            int hash1 = (5381<<16) + 5381;
#else
            int hash1 = 5381;
#endif
            int hash2 = hash1;

#if WIN32
                    // 32 bit machines.
                    int* pint = (int *)src;
                    int len = this.Length;
                    while (len > 2)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                        hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                        pint += 2;
                        len -= 4;
                    }
 
                    if (len > 0)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                    }
#else
            int c;
            char* s = &data[rangeStart];
            while ((c = s[0]) != 0) {
                hash1 = ((hash1 << 5) + hash1) ^ c;
                c = s[1];
                if (c == 0)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ c;
                s += 2;
            }
#endif
            return hash1 + (hash2 * 1566083941);

        }

        public override string ToString() {
            return new string(data, rangeStart, rangeEnd - rangeStart);
        }

        public string ToLowerString() {
            int length = rangeEnd - rangeStart;

            unsafe {
                char* buffer = stackalloc char[length + 1];
                int idx = 0;
                for (int i = rangeStart; i < rangeEnd; i++) {
                    buffer[idx++] = char.ToLower(data[i]);
                }

                buffer[length] = '\0';
                return new string(buffer);
            }
        }

        public CharSpan Trim() {
            int start = rangeStart;
            int end = rangeEnd;
            for (int i = rangeStart; i < rangeEnd; i++) {
                if (char.IsWhiteSpace(data[i])) {
                    start++;
                }
                else {
                    break;
                }
            }

            for (int i = rangeEnd - 1; i >= start; i--) {
                if (char.IsWhiteSpace(data[i])) {
                    end--;
                }
                else {
                    break;
                }
            }

            return new CharSpan(data, start, end);
        }

        public int GetLineNumber() {
            int line = 0;
            for (int i = 0; i < rangeStart; i++) {
                if (data[i] == '\n') line++;
            }

            return line;
        }

        public int GetEndLineNumber() {
            int line = 0;
            for (int i = 0; i < rangeEnd; i++) {
                if (data[i] == '\n') line++;
            }

            return line;
        }

        public ReflessCharSpan ToRefless() {
            return new ReflessCharSpan(this);
        }

    }

}