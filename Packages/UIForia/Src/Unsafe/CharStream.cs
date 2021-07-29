using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia.Util {

    public enum CommentMode {

        None,
        DoubleSlash,
        XML

    }

    public unsafe struct CharStream {

        public bool Equals(CharStream other) {
            return data == other.data && dataStart == other.dataStart && dataEnd == other.dataEnd && ptr == other.ptr && baseOffset == other.baseOffset && commentMode == other.commentMode;
        }

        public override bool Equals(object obj) {
            return obj is CharStream other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = unchecked((int) (long) data);
                hashCode = (hashCode * 397) ^ (int) dataStart;
                hashCode = (hashCode * 397) ^ (int) dataEnd;
                hashCode = (hashCode * 397) ^ (int) ptr;
                hashCode = (hashCode * 397) ^ baseOffset;
                hashCode = (hashCode * 397) ^ (int) commentMode;
                return hashCode;
            }
        }

        private char* data;
        private uint dataStart;
        private uint dataEnd;
        private uint ptr;
        public int baseOffset;
        public CommentMode commentMode;

        [ThreadStatic] private static string s_ScratchBuffer;
        [ThreadStatic] private static List<EnumInfo> s_EnumNameEntryList;

        public CharStream(CharSpan span) : this() {
            this.data = span.data;
            this.dataStart = span.rangeStart;
            this.dataEnd = span.rangeEnd;
            this.ptr = dataStart;
            this.commentMode = CommentMode.DoubleSlash;
        }

        public CharStream(char* source, int start, int end) : this() {
            this.data = source;
            this.dataStart = (uint) start;
            this.dataEnd = (uint) end;
            this.ptr = dataStart;
            this.commentMode = CommentMode.DoubleSlash;
        }

        public CharStream(char* source, uint start, uint end) : this() {
            this.data = source;
            this.dataStart = start;
            this.dataEnd = end;
            this.ptr = dataStart;
            this.commentMode = CommentMode.DoubleSlash;
        }

        // copy from current pointer position
        public CharStream(CharStream propertyStream) : this() {
            this.data = propertyStream.data;
            this.dataStart = propertyStream.ptr;
            this.dataEnd = propertyStream.dataEnd;
            this.ptr = propertyStream.ptr;
            this.commentMode = CommentMode.DoubleSlash;
        }

        public bool HasMoreTokens => ptr < dataEnd;
        public uint Size => dataEnd - dataStart;
        public char* Data => data;
        public uint Ptr => ptr;
        public uint End => dataEnd;
        public int IntPtr => (int) ptr;
        public char Last => data[dataEnd - 1];

        public char Current {
            get => data[ptr];
        }

        public char Next {
            get => ptr + 1 < dataEnd ? data[ptr + 1] : '\0';
        }

        public char Previous {
            get {
                int i = (int) ptr - 1;
                int ds = (int) dataStart;
                return i >= ds ? data[i] : '\0';
            }
        }

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
            if (ptr + str.Length > dataEnd) {
                return false;
            }

            fixed (char* s = str) {
                if (UnsafeUtility.MemCmp(s, data + ptr, str.Length * 2) != 0) {
                    return false;
                }

                Advance((uint) str.Length);
                return true;
            }
        }

        public bool TryMatchRange(in CharSpan charSpan) {
            if (ptr + charSpan.Length > dataEnd) {
                return false;
            }

            for (int i = 0; i < charSpan.Length; i++) {
                if (data[ptr + i] != charSpan[i]) {
                    return false;
                }
            }

            Advance((uint) charSpan.Length);
            return true;
        }

        public bool TryMatchRange(string str, out uint advance) {
            if (ptr + str.Length > dataEnd) {
                advance = 0;
                return false;
            }

            fixed (char* s = str) {
                for (int i = 0; i < str.Length; i++) {
                    if (data[ptr + i] != s[i]) {
                        advance = 0;
                        return false;
                    }
                }
            }

            advance = (uint) str.Length;
            return true;
        }

        public bool TryMatchRange(string str, char optional, out bool usedOptional, out uint advance) {
            if (TryMatchRange(str, out advance)) {
                if (data[ptr + advance] == optional) {
                    usedOptional = true;
                    advance++;
                    return true;
                }

                usedOptional = false;
                return true;
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

        // finds the next target char while descending into sub blocks of () {} [], strings, and comments 
        public CharStream GetNextTraversedStream(char target, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            uint i = ptr;
            uint start = ptr;

            while (i < dataEnd) {

                char c = data[i];
                i++;

                if (c == target) {
                    ptr = i;
                    return new CharStream(data, start, i - 1);
                }

                if (c == '(') {
                    ptr = i - 1;
                    TryGetSubStream('(', ')', out CharStream _, whitespaceHandling);
                    i = ptr;
                }
                else if (c == '{') {
                    ptr = i - 1;
                    TryGetSubStream('{', '}', out CharStream _, whitespaceHandling);
                    i = ptr;
                }
                else if (c == '[') {
                    ptr = i - 1;
                    TryGetSubStream('[', ']', out CharStream _, whitespaceHandling);
                    i = ptr;
                }
                else if (c == '"') {
                    while (i < dataEnd && data[i] != '"') {
                        i++;
                    }

                    i++; // step over last "
                }
                else if (c == '/' && i + 1 < dataEnd && data[i + 1] == '/') {
                    ptr = i;
                    ConsumeWhiteSpaceAndComments(CommentMode.DoubleSlash);
                    i = ptr;
                }
            }

            ptr = dataEnd;
            return new CharStream(data, start, dataEnd);

        }

        public bool TryGetSubStream(char open, char close, out CharStream charStream, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            uint start = ptr;
            if ((whitespaceHandling & WhitespaceHandling.ConsumeBefore) != 0) {
                ConsumeWhiteSpaceAndComments();
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
                            ConsumeWhiteSpaceAndComments();
                        }

                        return true;
                    }
                }
            }

            RewindTo(start);
            charStream = default;
            return false;
        }

        public bool TryGetSafeSubStream(char open, char close, out CharStream charStream, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            if ((whitespaceHandling & WhitespaceHandling.ConsumeBefore) != 0) {
                ConsumeWhiteSpaceAndComments();
            }

            uint start = ptr;
            if (data[ptr] != open) {
                charStream = default;
                RewindTo(start);
                return false;
            }

            uint i = ptr;
            int counter = 1;

            while (i < dataEnd) {
                i++;

                bool hasMore = i + 1 < dataEnd;

                if (data[i] == open) {
                    if (open == '{' && hasMore && data[i + 1] == open) {
                        i++;
                        continue;
                    }

                    counter++;
                }
                else if (data[i] == '/' && hasMore && data[i + 1] == '/') {
                    ptr = i;
                    ConsumeWhiteSpaceAndComments(CommentMode.DoubleSlash);
                    i = ptr - 1; // need to step back because loop starts with ++ 
                }
                else if (data[i] == '"') {
                    i++;

                    while (i < dataEnd && data[i] != '"') {
                        i++;
                    }
                    
                }
                else if (data[i] == close) {

                    if (close == '}' && hasMore && data[i + 1] == close) {
                        i++;
                        continue;
                    }

                    counter--;
                    if (counter == 0) {
                        charStream = new CharStream(data, start + 1 , i);
                        ptr = i + 1; // step over close
                        if ((whitespaceHandling & WhitespaceHandling.ConsumeAfter) != 0) {
                            ConsumeWhiteSpaceAndComments();
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

        public void ConsumeWhiteSpaceAndComments(CommentMode commentMode) {
            CommentMode lastMode = this.commentMode;
            this.commentMode = commentMode;
            ConsumeWhiteSpaceAndComments();
            this.commentMode = lastMode;
        }

        private static bool IsSpace(char c) {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        public void ConsumeWhiteSpace() {
            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }
        }

        public void ConsumeWhiteSpaceAndComments() {
            while (true) {
                while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                    ptr++;
                }

                if (commentMode == CommentMode.DoubleSlash && ptr < dataEnd - 2) {
                    if (data[ptr + 0] == '/' && data[ptr + 1] == '/') {
                        ptr += 2;
                        while (ptr < dataEnd) {
                            if (data[ptr] == '\n') {
                                break;
                            }

                            ptr++;
                        }

                        continue;
                    }
                }
                else if (commentMode == CommentMode.XML) {
                    ConsumeXMLComment();
                }

                break;
            }
        }

        public void ConsumeXMLComment() {
            if (data[ptr] == '<' && ptr + 4 < dataEnd && data[ptr + 1] == '!' && data[ptr + 2] == '-' && data[ptr + 3] == '-') {
                ptr += 4;

                while (ptr + 3 < dataEnd) {
                    if (data[ptr] == '-' && data[ptr + 1] == '-' && data[ptr + 2] == '>') {
                        ptr += 3;
                        return;
                    }

                    ptr++;
                }
            }

            // while (ptr < dataEnd) {
            //     char current = data[ptr];
            //     if (!(current == ' ' || current == '\t' || current == '\n' || current == '\r')) {
            //         if (data[ptr] == '<' && ptr + 4 < dataEnd && data[ptr + 1] == '!' && data[ptr + 2] == '-' && data[ptr + 3] == '-') {
            //             ptr += 4; // step over <!--
            //
            //             while (ptr + 2 < dataEnd && !(data[ptr] == '-' && data[ptr + 1] == '-')) {
            //                 ptr++;
            //             }
            //
            //             ptr += 2; // step over --
            //         }
            //         else {
            //             break;
            //         }
            //     }
            //
            //     ptr++;
            // }
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

        /// <summary>
        /// Creates a CharSpan that starts after the last occurrence of the optional from-character
        /// and ends at the end of the stream. If the from-character is not found the CharSpan
        /// is equal to the whole stream.
        /// </summary>
        /// <param name="optionalChar"></param>
        /// <param name="span"></param>
        public void GetCharSpanAfterLast(char optionalChar, out CharSpan span) {
            uint i = ptr;
            int startIndex = (int) ptr;
            while (i < dataEnd) {
                if (data[i] == optionalChar) {
                    startIndex = (int) i + 1;
                }

                i++;
            }

            span = new CharSpan(data, startIndex, (int) dataEnd);
        }

        public CharSpan GetCharSpanToDelimiterOrEnd(char c0) {
            uint i = ptr;
            uint start = ptr;
            while (i < dataEnd) {
                char c = data[i];
                if (c == c0) {
                    CharSpan retn = new CharSpan(data, (int) ptr, (int) i);
                    ptr += i - ptr + 1;
                    return retn;
                }

                i++;
            }

            ptr = dataEnd;
            return new CharSpan(data, (int) start, (int) dataEnd);
        }

        public bool TryGetCharSpanTo(char c0, out CharSpan span) {
            uint i = ptr;
            while (i < dataEnd) {
                char c = data[i];
                if (c == c0) {
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

                if (c == c1 || c == c2 || c == c3) {
                    stream = default;
                    return false;
                }

                i++;
            }

            stream = default;
            return false;
        }

        public bool TryGetStreamUntil(out CharStream stream, out char end, char c1, char c2 = '\0', char c3 = '\0') {
            uint i = ptr;
            while (i < dataEnd) {
                char c = data[i];
                if (c == c1 || c == c2 || c == c3) {
                    end = c;
                    stream = new CharStream(data, ptr, i);
                    Advance(i - ptr + 1);
                    return true;
                }

                i++;
            }

            end = default;
            stream = default;
            return false;
        }

        public bool TryGetStreamUntil(char terminator, out CharStream span, char escape) {
            int i = (int) ptr;
            char prev = '\0';
            if (i - 1 >= dataStart) {
                prev = data[i - 1];
            }

            while (i < dataEnd) {
                char c = data[i];
                if (c == terminator && prev != escape) {
                    span = new CharStream(data, (ushort) ptr, (ushort) i);
                    ptr = (uint) i;
                    return true;
                }

                prev = c;
                i++;
            }

            span = default;
            return false;
        }

        public bool TryGetStreamUntil(char terminator, out CharStream span) {
            uint i = ptr;
            while (i < dataEnd) {
                char c = data[i];
                if (c == terminator) {
                    span = new CharStream(data, (ushort) ptr, (ushort) i);
                    ptr = i;
                    return true;
                }

                i++;
            }

            span = default;
            return false;
        }

        public bool TryGetStreamUntil(out CharSpan span, char c1, char c2 = '\0', char c3 = '\0') {
            uint i = ptr;
            while (i < dataEnd) {
                char c = data[i];
                if (c == c1 || c == c2 || c == c3 || char.IsWhiteSpace(c)) {
                    span = new CharSpan(data, (ushort) ptr, (ushort) i);
                    ptr = i;
                    return true;
                }

                i++;
            }

            span = default;
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

        private struct EnumInfo {

            public Type type;
            public string[] names;
            public int[] values;

        }

        public bool TryParseMultiDottedIdentifier(out CharSpan retn, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll, bool allowMinus = false) {
            uint start = ptr;

            if ((whitespaceHandling & WhitespaceHandling.ConsumeBefore) != 0) {
                ConsumeWhiteSpaceAndComments();
            }

            if (!TryParseIdentifier(out CharSpan identifier, allowMinus, WhitespaceHandling.ConsumeBefore)) {
                retn = default;
                return false;
            }

            int end = identifier.rangeEnd;

            while (TryParseCharacter('.', WhitespaceHandling.None)) {
                if (!TryParseIdentifier(out CharSpan endIdent, allowMinus, WhitespaceHandling.None)) {
                    retn = default;
                    ptr = start;
                    return false;
                }

                end = endIdent.rangeEnd;
            }

            retn = new CharSpan(identifier.data, identifier.rangeStart, end);
            if ((whitespaceHandling & WhitespaceHandling.ConsumeAfter) != 0) {
                ConsumeWhiteSpaceAndComments();
            }

            return true;
        }

        public bool TryParseDottedIdentifier(out CharSpan retn, out bool wasDotted, bool allowMinus = true) {
            if (!TryParseIdentifier(out CharSpan identifier, allowMinus, WhitespaceHandling.ConsumeBefore)) {
                retn = default;
                wasDotted = false;
                return false;
            }

            if (!TryParseCharacter('.', WhitespaceHandling.None)) {
                retn = identifier;
                wasDotted = false;
                return true;
            }

            if (!TryParseIdentifier(out CharSpan endIdent, allowMinus, WhitespaceHandling.ConsumeAfter)) {
                ConsumeWhiteSpaceAndComments();
                wasDotted = false;
                retn = identifier;
                return true;
            }

            wasDotted = true;
            retn = new CharSpan(data, identifier.rangeStart, endIdent.rangeEnd);
            return true;
        }

        public bool TryParseDottedIdentifier(out CharSpan retn, out int dotIdx, bool allowMinus = true) {
            if (!TryParseIdentifier(out CharSpan identifier, allowMinus, WhitespaceHandling.ConsumeBefore)) {
                retn = default;
                dotIdx = -1;
                return false;
            }

            dotIdx = (int) ptr;
            if (!TryParseCharacter('.', WhitespaceHandling.None)) {
                retn = identifier;
                dotIdx = -1;
                return true;
            }

            if (!TryParseIdentifier(out CharSpan endIdent, allowMinus, WhitespaceHandling.ConsumeAfter)) {
                ConsumeWhiteSpaceAndComments();
                dotIdx = -1;
                retn = identifier;
                return true;
            }

            retn = new CharSpan(data, identifier.rangeStart, endIdent.rangeEnd);
            return true;
        }

        public bool TryParseDottedIdentifier(out CharSpan retn) {
            return TryParseDottedIdentifier(out retn, out bool _);
        }

        public bool TryParseIdentifier(out CharSpan span, bool allowMinus = true, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            uint start = ptr;

            if ((whitespaceHandling & WhitespaceHandling.ConsumeBefore) != 0) {
                ConsumeWhiteSpaceAndComments();
            }

            if (TryParseIdentifier(out int rangeStart, out int rangeEnd, allowMinus)) {
                span = new CharSpan(data, rangeStart, rangeEnd);
                if ((whitespaceHandling & WhitespaceHandling.ConsumeAfter) != 0) {
                    ConsumeWhiteSpaceAndComments();
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
                if (!char.IsLetterOrDigit(c)) {
                    if (c == '_' || (allowMinus && c == '-')) {
                        ptr2++;
                        continue;
                    }

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

        public bool TryParseHalf(out half value, bool consumeTrailingF = false) {
            if (TryParseFloat(out float result, consumeTrailingF)) {
                value = (half) result;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryParseFloat(out float value, bool consumeTrailingF = false) {
            if (s_ScratchBuffer == null) {
                s_ScratchBuffer = new string('\0', s_ScratchBufferLength);
            }

            uint start = ptr;

            // oh, you thought C# strings were immutable? :)

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
                    if (consumeTrailingF) {
                        TryParseCharacter('f');
                    }
                }

                return retn;
            }
        }

        public bool TryParseUInt(out uint intVal) {
            uint i = ptr;
            uint startPointer = ptr;

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
                number += (data[startPointer + length] - '0') * multiplier;
                multiplier *= 10;
            }

            intVal = (uint) number;

            return true;
        }

        public bool TryParseInt(out int intVal) {
            int sign = 1;
            uint minusSize = 0;
            uint i = ptr;

            uint startPointer = ptr;

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

            int length = (int) i - (int) ptr;
            Advance((uint) length);

            int number = 0;
            int mult = 1;

            while (length-- != minusSize) {
                number += (data[startPointer + length] - '0') * mult;
                mult *= 10;
            }

            intVal = number * sign;

            return true;
        }

        private static EnumInfo GetEnumData(Type enumType) {
            if (s_EnumNameEntryList == null) {
                s_EnumNameEntryList = new List<EnumInfo>();
            }

            for (int i = 0; i < s_EnumNameEntryList.Count; i++) {
                EnumInfo info = s_EnumNameEntryList[i];
                if (enumType == info.type) {
                    return info;
                }
            }

            string[] names = Enum.GetNames(enumType);

            for (int i = 0; i < names.Length; i++) {
                names[i] = names[i].ToLower();
            }

            Array vals = Enum.GetValues(enumType);

            int[] values = new int[vals.Length];

            for (int i = 0; i < values.Length; i++) {
                values[i] = Convert.ToInt32((object) vals.GetValue(i));
            }

            s_EnumNameEntryList.Add(new EnumInfo() {
                type = enumType,
                names = names,
                values = values
            });

            return s_EnumNameEntryList[s_EnumNameEntryList.Count - 1];

        }

        ///<summary>
        /// Cannot cast T to int to T without boxing, so we return the integer and expect the caller to cast to enum type
        /// </summary> 
        public bool TryParseEnum(Type enumType, out int enumValue) {
            if (!TryParseIdentifier(out CharSpan span)) {
                enumValue = default;
                return false;
            }
            
            EnumInfo enumInfo = GetEnumData(enumType);

            char* buffer = stackalloc char[span.Length];
            span.ToLower(buffer);
            CharSpan lowered = new CharSpan(buffer, 0, span.Length);

            for (int i = 0; i < enumInfo.names.Length; i++) {
                string name = enumInfo.names[i];
                if (lowered == name) {
                    enumValue = enumInfo.values[i];
                    return true;
                }
            }

            enumValue = default;
            return false;
        }

        ///<summary>
        /// Cannot cast T to int to T without boxing, so we return the integer and expect the caller to cast to enum type
        /// </summary> 
        public bool TryParseEnum<T>(out int enumValue) where T : Enum {

            if (!TryParseIdentifier(out CharSpan span)) {
                enumValue = default;
                return false;

            }

            EnumInfo enumInfo = GetEnumData(typeof(T));

            char* buffer = stackalloc char[span.Length];
            span.ToLower(buffer);
            CharSpan lowered = new CharSpan(buffer, 0, span.Length);

            for (int i = 0; i < enumInfo.names.Length; i++) {
                string name = enumInfo.names[i];
                if (lowered == name) {
                    enumValue = enumInfo.values[i];
                    return true;
                }
            }

            enumValue = default;
            return false;
        }

        public bool TryParseCharacter(char character, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            uint save = ptr;

            if ((whitespaceHandling & WhitespaceHandling.ConsumeBefore) != 0) {
                ConsumeWhiteSpaceAndComments();
            }

            if (data[ptr] == character) {
                Advance();

                if ((whitespaceHandling & WhitespaceHandling.ConsumeAfter) != 0) {
                    ConsumeWhiteSpaceAndComments();
                }

                return true;
            }

            RewindTo(save);

            return false;
        }

        public bool TryParseColorProperty(out Color32 color) {
            uint start = ptr;

            ConsumeWhiteSpaceAndComments();

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
                    return true;

                }
            }
            else if (data[ptr] == '#') {
                // read 6 or 8 characters
                // https://www.includehelp.com/code-snippets/convert-hexadecimal-string-to-integer-in-c-programming.aspx

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

        public bool TryParseXMLClose(out CharSpan identifier) {
            if (ptr + 4 < dataEnd) {
                identifier = default;
                return false;
            }

            if (data[ptr] != '<' || data[ptr + 1] != '/') {
                identifier = default;
                return false;
            }

            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }

            if (!TryParseIdentifier(out identifier, true, WhitespaceHandling.ConsumeAfter)) {
                return false;
            }

            return TryParseCharacter('>');
        }

        public bool TryParseXMLAttribute(out CharSpan key, out CharSpan value, bool requireQuotes = false) {
            uint start = ptr;

            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }

            if (!TryParseIdentifier(out key)) {
                value = default;
                ptr = start;
                return false;
            }

            if (!TryParseCharacter('=')) {
                value = default;
                ptr = start;
                return false;
            }

            if (requireQuotes) {
                if (!TryParseCharacter('"')) {
                    value = default;
                    ptr = start;
                    return false;
                }

                if (TryGetCharSpanTo('"', out value)) {
                    return true;
                }
            }
            // read until we hit a space, > or end of input
            else if (TryGetCharSpanTo(' ', '>', out value)) {
                return true;
            }

            ptr = start;
            return false;
        }

        public bool TryParseXMLOpenTagIdentifier(out CharSpan identifier) {
            if (data[ptr] != '<') {
                identifier = default;
                return false;
            }

            ptr++;
            if (TryParseIdentifier(out identifier, true, WhitespaceHandling.ConsumeAfter)) {
                return true;
            }

            return false;
        }

        public bool TryMatchRangeIgnoreCase(string str, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            uint start = ptr;
            if ((whitespaceHandling & WhitespaceHandling.ConsumeBefore) != 0) {
                ConsumeWhiteSpaceAndComments();
            }

            if (str.Length == 1 && ptr <= dataEnd && str[0] == data[ptr]) {
                ptr++;
                if ((whitespaceHandling & WhitespaceHandling.ConsumeAfter) != 0) {
                    ConsumeWhiteSpaceAndComments();
                }

                return true;
            }

            if (ptr + str.Length > dataEnd) {
                ptr = start;
                return false;
            }

            fixed (char* s = str) {
                for (int i = 0; i < str.Length; i++) {
                    char strChar = s[i];
                    char dataChar = data[ptr + i];

                    char c1 = char.ToLower(strChar);
                    char c2 = char.ToLower(dataChar);

                    if (c1 != c2) {
                        ptr = start;
                        return false;
                    }
                }
            }

            Advance((uint) str.Length);

            if ((whitespaceHandling & WhitespaceHandling.ConsumeAfter) != 0) {
                ConsumeWhiteSpaceAndComments();
            }

            return true;
        }

        public void Rewind(uint i) {
            ptr -= i;
            if (ptr < dataStart) ptr = dataStart;
        }

        public void RemoveLast() {
            if (dataEnd <= dataStart) {
                return;
            }

            dataEnd--;
        }

        public LineInfo GetLineInfo() {
            int line = 1; // start counting at line 1
            int x = 0;

            for (int i = 0; i < ptr; i++) {
                if (data[i] == '\n') {
                    line++;
                    x = i + 1;
                }
            }

            int col = 1;
            if (ptr != 0) {
                for (int i = x; i < ptr - 1; i++) {
                    col++;
                }
            }

            return new LineInfo(line, col);
        }

        public bool TryParseOffsetMeasurement(out UIOffset offset, bool allowUnitless) {
            uint start = ptr;
            if (TryParseFloat(out float value)) {

                bool gotUnit = TryParseOffsetMeasurementUnit(out UIOffsetUnit unit);

                if (gotUnit) {
                    offset = new UIOffset(value, unit);
                    return true;
                }

                if (allowUnitless) { // do we need to make sure there is no unit and not that we failed to parse it?
                    offset = new UIOffset(value);
                    return true;
                }

            }

            ptr = start;
            offset = default;
            return false;
        }

        public bool TryParseGridCellSize(out GridCellDefinition cell) {
            uint start = ptr;

            // 1s 30px_1s 12mx;

            ConsumeWhiteSpace();

            if (TryParseFloat(out float value)) {

                if (!HasMoreTokens || (HasMoreTokens && char.IsWhiteSpace(data[ptr]))) {
                    cell = new GridCellDefinition(value);
                    return true;
                }

                if (!TryParseGridCellUnit(out GridTemplateUnit unit)) {
                    ptr = start;
                    cell = default;
                    return false;
                }

                if (unit == GridTemplateUnit.Stretch) {
                    unit = GridTemplateUnit.Pixel;
                    cell = new GridCellDefinition(0, unit, (ushort) (int) value);
                    return true;
                }

                if (HasMoreTokens && data[ptr] == '_') {

                    ptr++;
                    if (!TryParseUInt(out uint stretch) || data[ptr] != 's') {
                        cell = default;
                        return false;
                    }

                    ptr++;
                    cell = new GridCellDefinition(value, unit, (ushort) stretch);
                    return true;

                }

                cell = new GridCellDefinition(value, unit);
                return true;

            }

            ptr = start;
            cell = default;
            return false;
        }

        public bool TryParseFixedLength(out UIFixedLength fixedLength, bool allowUnitless) {
            uint start = ptr;
            if (TryParseFloat(out float value)) {

                bool gotUnit = TryParseFixedLengthUnit(out UIFixedUnit unit);

                if (gotUnit) {
                    fixedLength = new UIFixedLength(value, unit);
                    return true;
                }

                if (allowUnitless) { // do we need to make sure there is no unit and not that we failed to parse it?
                    fixedLength = new UIFixedLength(value, UIFixedUnit.Pixel);
                    return true;
                }

            }

            ptr = start;
            fixedLength = default;
            return false;
        }

        public bool TryParseSpaceSize(out UISpaceSize spaceSize, bool allowUnitless) {
            uint start = ptr;

            // 12px;
            // 3s;
            // 10px_3s;

            if (TryParseFloat(out float value)) {

                bool gotUnit = TryParseSpaceSizeUnit(out UISpaceSizeUnit unit);

                if (gotUnit) {
                    if (unit == UISpaceSizeUnit.Stretch) {
                        unit = UISpaceSizeUnit.Pixel;
                        spaceSize = new UISpaceSize(0, unit, (ushort) (int) value);
                    }
                    else if (HasMoreTokens && data[ptr] == '_') {

                        ptr++;
                        if (!TryParseUInt(out uint stretch) || data[ptr] != 's') {
                            spaceSize = default;
                            return false;
                        }

                        ptr++;
                        spaceSize = new UISpaceSize(value, unit, (ushort) stretch);

                    }
                    else {
                        spaceSize = new UISpaceSize(value, unit);
                    }

                    return true;
                }

                if (allowUnitless) { // do we need to make sure there is no unit and not that we failed to parse it?
                    spaceSize = new UISpaceSize(value, UISpaceSizeUnit.Pixel);
                    return true;
                }

            }

            ptr = start;
            spaceSize = default;
            return false;
        }

        public bool TryParseSizeConstraint(out UISizeConstraint uiSizeConstraint, bool allowUnitless) {
            uint start = ptr;
            if (TryParseFloat(out float value)) {

                bool gotValidSize = TryParseSizeConstraintUnit(out UISizeConstraintUnit unit, allowUnitless);
                uiSizeConstraint = new UISizeConstraint(value, unit);
                return gotValidSize;
            }

            ptr = start;
            uiSizeConstraint = default;
            return false;
        }

        public bool TryParseFontSize(out UIFontSize fontSize, bool allowUnitless) {
            uint start = ptr;
            if (TryParseFloat(out float value)) {

                bool gotUnit = TryParseFontSizeUnit(out UIFontSizeUnit unit);

                if (gotUnit) {
                    fontSize = new UIFontSize(value, unit);
                    return true;
                }

                if (allowUnitless) { // do we need to make sure there is no unit and not that we failed to parse it?
                    fontSize = new UIFontSize(value, UIFontSizeUnit.Pixel);
                    return true;
                }

            }

            ptr = start;
            fontSize = default;
            return false;
        }

        public bool TryParseTime(out UITimeMeasurement time, bool allowUnitless = true) {

            ConsumeWhiteSpace();

            uint start = ptr;
            if (TryParseFloat(out float value)) {
                bool gotUnit = TryParseTimeUnit(out UITimeMeasurementUnit unit);

                if (gotUnit) {
                    switch (unit) {
                        case UITimeMeasurementUnit.Percentage:
                            time = new UITimeMeasurement(value / 100f, unit);
                            break;
                        case UITimeMeasurementUnit.Milliseconds:
                            time = new UITimeMeasurement((int) value, unit);
                            break;
                        default:
                            time = new UITimeMeasurement(value, unit);
                            break;
                    }
                    return true;
                }

                if (allowUnitless) {
                    time = new UITimeMeasurement((int) value);
                    return true;
                }

            }

            ptr = start;
            time = default;
            return false;
        }

        public bool TryParseMeasurement(out UIMeasurement measurement, bool allowUnitless) {
            ConsumeWhiteSpace();
            uint start = ptr;
            if (TryParseFloat(out float value)) {

                bool gotUnit = TryParseMeasurementUnit(out UIMeasurementUnit unit);
                if (gotUnit || allowUnitless) {
                    measurement = new UIMeasurement(value, unit);
                    return true;
                }
            }

            ptr = start;
            measurement = default;
            return false;
        }

        public bool TryParseAngle(out UIAngle angle, bool allowUnitless) {
            uint start = ptr;
            if (TryParseFloat(out float value)) {

                bool gotUnit = TryParseAngleUnit(out UIAngleUnit unit);

                if (gotUnit) {
                    angle = new UIAngle(value, unit);
                    return true;
                }

                if (allowUnitless) { // do we need to make sure there is no unit and not that we failed to parse it?
                    angle = new UIAngle(value, UIAngleUnit.Degrees);
                    return true;
                }

            }

            ptr = start;
            angle = default;
            return false;
        }

        public bool TryParseAngleUnit(out UIAngleUnit unit) {
            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }

            if (ptr >= dataEnd) {
                unit = default;
                return false;
            }

            if (data[ptr] == '%') {
                unit = UIAngleUnit.Percent;
                ptr++;
                return true;
            }

            if (TryMatchRange("deg")) {
                unit = UIAngleUnit.Percent;
                return true;
            }

            if (TryMatchRange("rad")) {
                unit = UIAngleUnit.Radians;
                return true;
            }

            unit = default;
            return false;
        }

        public bool TryParseSizeConstraintUnit(out UISizeConstraintUnit unit, bool allowUnitless = true) {
            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }

            if (ptr >= dataEnd) {
                unit = UISizeConstraintUnit.Pixel;
                return allowUnitless;
            }

            if (data[ptr] == '%') {
                ptr++;
                unit = UISizeConstraintUnit.Percent;
                return true;
            }

            if (TryParseCharacters('l', 'h')) {
                unit = UISizeConstraintUnit.LineHeight;
                return true;
            }

            if (TryParseCharacters('e', 'm')) {
                unit = UISizeConstraintUnit.Em;
                return true;
            }

            if (TryParseCharacters('p', 'x')) {
                unit = UISizeConstraintUnit.Pixel;
                return true;
            }

            if (TryParseCharacters('c', 'n', 't')) {
                unit = UISizeConstraintUnit.Content;
                return true;
            }

            if (TryParseCharacters('m', 'x')) {
                unit = UISizeConstraintUnit.MaxChild;
                return true;
            }

            if (TryParseCharacters('m', 'n')) {
                unit = UISizeConstraintUnit.MinChild;
                return true;
            }

            if (TryParseCharacters('b', 'w')) {
                unit = UISizeConstraintUnit.BackgroundImageWidth;
                return true;
            }

            if (TryParseCharacters('b', 'h')) {
                unit = UISizeConstraintUnit.BackgroundImageHeight;
                return true;
            }

            if (TryParseCharacters('s', 'w')) {
                unit = UISizeConstraintUnit.ApplicationWidth;
                return true;
            }

            if (TryParseCharacters('s', 'h')) {
                unit = UISizeConstraintUnit.ApplicationHeight;
                return true;
            }

            if (TryParseCharacters('v', 'w')) {
                unit = UISizeConstraintUnit.ViewportWidth;
                return true;
            }

            if (TryParseCharacters('v', 'h')) {
                unit = UISizeConstraintUnit.ViewportHeight;
                return true;
            }

            if (TryParseCharacters('p', 's', 'z')) {
                unit = UISizeConstraintUnit.ParentSize;
                return true;
            }

            unit = default;
            return false;
        }

        public bool TryParseFontSizeUnit(out UIFontSizeUnit unit) {
            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }

            if (ptr >= dataEnd) {
                unit = default;
                return false;
            }

            if (TryParseCharacters('e', 'm')) {
                unit = UIFontSizeUnit.Em;
                return true;
            }

            if (TryParseCharacters('p', 'x')) {
                unit = UIFontSizeUnit.Pixel;
                return true;
            }

            if (TryParseCharacters('p', 't')) {
                unit = UIFontSizeUnit.Point;
                return true;
            }

            unit = default;
            return false;

        }

        public bool TryParseTimeUnit(out UITimeMeasurementUnit unit) {
            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }

            if (ptr >= dataEnd) {
                unit = default;
                return false;
            }

            if (data[ptr] == '%') {
                unit = UITimeMeasurementUnit.Percentage;
                ptr++;
                return true;
            }

            if (TryParseCharacters('m', 's')) {
                unit = UITimeMeasurementUnit.Milliseconds;
                return true;
            }

            if (TryParseCharacters('s', 'e', 'c')) {
                unit = UITimeMeasurementUnit.Seconds;
                return true;
            }

            unit = default;
            return false;
        }

        private bool TryParseCharactersUnchecked(char c0, char c1) {
            bool result = (data[ptr] == c0 && data[ptr + 1] == c1);
            if (result) {
                ptr += 2;
            }

            return result;
        }

        private bool TryParseCharactersUnchecked(char c0, char c1, char c2) {
            bool result = (data[ptr] == c0 && data[ptr + 1] == c1 && data[ptr + 2] == c2);
            if (result) {
                ptr += 3;
            }

            return result;
        }

        private bool TryParseCharactersUnchecked(char c0, char c1, char c2, char c3) {
            bool result = (data[ptr] == c0 && data[ptr + 1] == c1 && data[ptr + 2] == c2 && data[ptr + 3] == c3);
            if (result) {
                ptr += 4;
            }

            return result;
        }

        private bool TryParseCharacters(char c0, char c1) {
            if (ptr + 1 >= dataEnd) return false;
            bool result = (data[ptr] == c0 && data[ptr + 1] == c1);
            if (result) {
                ptr += 2;
            }

            return result;
        }

        private bool TryParseCharacters(char c0, char c1, char c2) {
            if (ptr + 2 >= dataEnd) return false;

            bool result = (data[ptr] == c0 && data[ptr + 1] == c1 && data[ptr + 2] == c2);
            if (result) {
                ptr += 3;
            }

            return result;
        }

        private bool TryParseCharacters(char c0, char c1, char c2, char c3) {
            if (ptr + 3 >= dataEnd) return false;

            bool result = (data[ptr] == c0 && data[ptr + 1] == c1 && data[ptr + 2] == c2 && data[ptr + 3] == c3);
            if (result) {
                ptr += 4;
            }

            return result;
        }

        private bool TryParseCharacters(char c0, char c1, char c2, char c3, char c4) {
            if (ptr + 4 >= dataEnd) return false;

            bool result = (data[ptr] == c0 && data[ptr + 1] == c1 && data[ptr + 2] == c2 && data[ptr + 3] == c3 && data[ptr + 4] == c4);
            if (result) {
                ptr += 5;
            }

            return result;
        }

        public bool TryParseMeasurementUnit(out UIMeasurementUnit unit) {
            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }

            if (ptr >= dataEnd) {
                unit = default;
                return false;
            }

            if (TryMatchRange("strcnt")) {
                unit = UIMeasurementUnit.StretchContent;
                return true;
            }

            if (TryParseCharacters('f', 'c')) {
                unit = UIMeasurementUnit.FitContent;
                return true;
            }

            if (TryParseCharacters('f', 'r')) {
                unit = UIMeasurementUnit.FillRemaining;
                return true;
            }
            
            if (TryParseCharacters('l', 'h')) {
                unit = UIMeasurementUnit.LineHeight;
                return true;
            }
            
            if (data[ptr] == 's') {
                unit = UIMeasurementUnit.Stretch;
                ptr++;
                return true;
            }

            if (data[ptr] == '%') {
                unit = UIMeasurementUnit.Percent;
                ptr++;
                return true;
            }

            if (ptr + 1 >= dataEnd) {
                unit = default;
                ptr += 2;
                return false;
            }

            if (data[ptr] == 'e' && data[ptr + 1] == 'm') {
                unit = UIMeasurementUnit.Em;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'm' && data[ptr + 1] == 'x') {
                unit = UIMeasurementUnit.MaxChild;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'm' && data[ptr + 1] == 'n') {
                unit = UIMeasurementUnit.MinChild;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'p' && data[ptr + 1] == 'x') {
                unit = UIMeasurementUnit.Pixel;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'b' && data[ptr + 1] == 'w') {
                unit = UIMeasurementUnit.Pixel;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'b' && data[ptr + 1] == 'h') {
                unit = UIMeasurementUnit.Pixel;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'v' && data[ptr + 1] == 'w') {
                unit = UIMeasurementUnit.ViewportWidth;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'v' && data[ptr + 1] == 'h') {
                unit = UIMeasurementUnit.ViewportHeight;
                ptr += 2;
                return true;
            }

            if (ptr + 2 >= dataEnd) {
                unit = default;
                ptr += 2;
                return false;
            }

            if (data[ptr] == 'c' && data[ptr + 1] == 'n' && data[ptr + 2] == 't') {
                unit = UIMeasurementUnit.Content;
                ptr += 3;
                return true;
            }

            // if (data[ptr] == 'p' && data[ptr + 1] == 'c' && data[ptr + 2] == 'a') {
            //     unit = UIMeasurementUnit.ParentContentArea;
            //     ptr += 3;
            //     return true;
            // }
            //
            // if (data[ptr] == 'p' && data[ptr + 1] == 's' && data[ptr + 2] == 'z') {
            //     unit = UIMeasurementUnit.BlockSize;
            //     ptr += 3;
            //     return true;
            // }
            //
            // if (TryMatchRangeIgnoreCase("auto")) {
            //     unit = UIMeasurementUnit.Auto;
            //     return true;
            // }

            unit = default;
            return false;
        }

        public bool TryParseOffsetMeasurementUnit(out UIOffsetUnit unit) {
            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }

            if (ptr >= dataEnd) {
                unit = default;
                return false;
            }

            if (data[ptr] == 'w') {
                unit = UIOffsetUnit.Width;
                ptr += 1;
                return true;
            }

            if (data[ptr] == 'h') {
                unit = UIOffsetUnit.Height;
                ptr += 1;
                return true;
            }

            // if (data[ptr] == '%') {
            //     unit = UIOffsetUnit.Percent;
            //     ptr += 1;
            //     return true;
            // }

            if (ptr + 1 >= dataEnd) {
                unit = default;
                return false;
            }

            if (data[ptr] == 'p' && data[ptr + 1] == 'x') {
                unit = UIOffsetUnit.Pixel;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'e' && data[ptr + 1] == 'm') {
                unit = UIOffsetUnit.Em;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'c' && data[ptr + 1] == 'w') {
                unit = UIOffsetUnit.ContentWidth;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'c' && data[ptr + 1] == 'h') {
                unit = UIOffsetUnit.ContentHeight;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'v' && data[ptr + 1] == 'w') {
                unit = UIOffsetUnit.ViewportWidth;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'v' && data[ptr + 1] == 'h') {
                unit = UIOffsetUnit.ViewportHeight;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'p' && data[ptr + 1] == 'w') {
                unit = UIOffsetUnit.ParentWidth;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'p' && data[ptr + 1] == 'h') {
                unit = UIOffsetUnit.ParentHeight;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 's' && data[ptr + 1] == 'w') {
                unit = UIOffsetUnit.ScreenWidth;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 's' && data[ptr + 1] == 'h') {
                unit = UIOffsetUnit.ScreenHeight;
                ptr += 2;
                return true;
            }

            if (ptr + 2 >= dataEnd) {
                unit = default;
                return false;
            }

            // if (data[ptr] == 'a' && data[ptr + 1] == 'l' && data[ptr + 2] == 'w') {
            //     unit = UIOffsetUnit.AllocatedWidth;
            //     ptr += 3;
            //     return true;
            // }
            //
            // if (data[ptr] == 'a' && data[ptr + 1] == 'l' && data[ptr + 2] == 'h') {
            //     unit = UIOffsetUnit.AllocatedHeight;
            //     ptr += 3;
            //     return true;
            // }

            if (data[ptr] == 'c' && data[ptr + 1] == 'a' && data[ptr + 2] == 'w') {
                unit = UIOffsetUnit.ContentAreaWidth;
                ptr += 3;
                return true;
            }

            if (data[ptr] == 'c' && data[ptr + 1] == 'a' && data[ptr + 2] == 'h') {
                unit = UIOffsetUnit.ContentAreaHeight;
                ptr += 3;
                return true;
            }

            if (ptr + 3 >= dataEnd) {
                unit = default;
                return false;
            }

            // if (data[ptr] == 'p' && data[ptr + 1] == 'c' && data[ptr + 2] == 'a' && data[ptr + 3] == 'w') {
            //     unit = UIOffsetUnit.ParentContentAreaWidth;
            //     ptr += 4;
            //     return true;
            // }
            //
            // if (data[ptr] == 'p' && data[ptr + 1] == 'c' && data[ptr + 2] == 'a' && data[ptr + 3] == 'h') {
            //     unit = UIOffsetUnit.ParentContentAreaHeight;
            //     ptr += 4;
            //     return true;
            // }

            unit = default;
            return false;
        }

        public bool TryParseSpaceSizeUnit(out UISpaceSizeUnit unit) {

            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }

            if (ptr >= dataEnd) {
                unit = default;
                return false;
            }

            if (data[ptr] == 's') {
                unit = UISpaceSizeUnit.Stretch;
                return true;
            }

            if (TryParseCharacters('p', 'x')) {
                unit = UISpaceSizeUnit.Pixel;
                return true;
            }

            if (TryParseCharacters('e', 'm')) {
                unit = UISpaceSizeUnit.Em;
                return true;
            }

            if (TryParseCharacters('v', 'w')) {
                unit = UISpaceSizeUnit.ViewportWidth;
                return true;
            }

            if (TryParseCharacters('v', 'h')) {
                unit = UISpaceSizeUnit.ViewportHeight;
                return true;
            }

            if (TryParseCharacters('s', 'w')) {
                unit = UISpaceSizeUnit.ApplicationWidth;
                return true;
            }

            if (TryParseCharacters('s', 'h')) {
                unit = UISpaceSizeUnit.ApplicationHeight;
                return true;
            }

            unit = default;
            return false;
        }

        private static bool IsUnitValid<T>(Unit unit) where T : Enum {

            EnumInfo enumInfo = GetEnumData(typeof(T));

            int unitVal = (int) unit;

            for (int i = 0; i < enumInfo.values.Length; i++) {
                if (enumInfo.values[i] == unitVal) {
                    return true;
                }
            }

            return false;
        }

        private bool TryParseUntypedUnit(out Unit unit) {

            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }

            if (ptr >= dataEnd) {
                unit = default;
                return false;
            }

            if (data[ptr] == '%') {
                unit = Unit.Percent;
                ptr++;
                return true;
            }

            if (TryParseCharacters('s', 'c')) {
                unit = Unit.StretchContent;
                return true;
            }

            if (TryParseCharacters('f', 'c')) {
                unit = Unit.FitContent;
                return true;
            }

            if (data[ptr] == 's') {
                unit = Unit.Stretch;
                ptr++;
                return true;
            }

            if (TryParseCharacters('p', 'x')) {
                unit = Unit.Pixel;
                return true;
            }

            if (TryParseCharacters('e', 'm')) {
                unit = Unit.Em;
                return true;
            }

            if (TryParseCharacters('s', 'w')) {
                unit = Unit.ScreenWidth;
                return true;
            }

            if (TryParseCharacters('s', 'h')) {
                unit = Unit.ScreenHeight;
                return true;
            }

            if (TryParseCharacters('v', 'w')) {
                unit = Unit.ViewportWidth;
                return true;
            }

            if (TryParseCharacters('v', 'h')) {
                unit = Unit.ViewportHeight;
                return true;
            }

            if (TryParseCharacters('b', 'w')) {
                unit = Unit.BackgroundImageWidth;
                return true;
            }

            if (TryParseCharacters('b', 'h')) {
                unit = Unit.BackgroundImageHeight;
                return true;
            }

            if (data[ptr] == 'w') {
                unit = Unit.Width;
                ptr++;
                return true;
            }

            if (data[ptr] == 'h') {
                unit = Unit.Height;
                ptr++;
                return true;
            }

            if (TryParseCharacters('a', 'h')) {
                unit = Unit.Height;
                return true;
            }

            if (TryParseCharacters('c', 'n', 't')) {
                unit = Unit.Content;
                return true;
            }

            if (TryParseCharacters('m', 'x', 'c', 'n', 't')) {
                unit = Unit.MaxContent;
                return true;
            }

            if (TryParseCharacters('m', 'n', 'c', 'n', 't')) {
                unit = Unit.MinContent;
                return true;
            }

            if (TryParseCharacters('c', 'w')) {
                unit = Unit.ContentWidth;
                return true;
            }

            if (TryParseCharacters('c', 'h')) {
                unit = Unit.ContentHeight;
                return true;
            }

            if (TryParseCharacters('m', 'x')) {
                unit = Unit.MaxChild;
                return true;
            }

            if (TryParseCharacters('m', 'n')) {
                unit = Unit.MinChild;
                return true;
            }

            if (TryParseCharacters('m', 'n')) {
                unit = Unit.MinChild;
                return true;
            }

            if (TryParseCharacters('s', 'e', 'c')) {
                unit = Unit.Seconds;
                return true;
            }

            if (TryParseCharacters('m', 's')) {
                unit = Unit.Milliseconds;
                return true;
            }

            if (TryParseCharacters('d', 'e', 'g')) {
                unit = Unit.Degrees;
                return true;
            }

            if (TryParseCharacters('r', 'a', 'd')) {
                unit = Unit.Radians;
                return true;
            }

            unit = default;
            return false;
        }

        public bool TryParseGridCellUnit(out GridTemplateUnit unit) {

            if (TryParseUntypedUnit(out Unit untypedUnit) && IsUnitValid<GridTemplateUnit>(untypedUnit)) {
                unit = (GridTemplateUnit) (ushort) (untypedUnit);
                return true;
            }

            unit = default;
            return false;

        }

        public bool TryParseFixedLengthUnit(out UIFixedUnit fixedUnit) {
            while (ptr < dataEnd && char.IsWhiteSpace(data[ptr])) {
                ptr++;
            }

            if (ptr >= dataEnd) {
                fixedUnit = default;
                return false;
            }

            if (data[ptr] == '%') {
                fixedUnit = UIFixedUnit.Percent;
                ptr++;
                return true;
            }

            if (ptr + 1 >= dataEnd) {
                fixedUnit = default;
                ptr += 2;
                return false;
            }

            if (data[ptr] == 'e' && data[ptr + 1] == 'm') {
                fixedUnit = UIFixedUnit.Em;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'p' && data[ptr + 1] == 'x') {
                fixedUnit = UIFixedUnit.Pixel;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'v' && data[ptr + 1] == 'w') {
                fixedUnit = UIFixedUnit.ViewportWidth;
                ptr += 2;
                return true;
            }

            if (data[ptr] == 'v' && data[ptr + 1] == 'h') {
                fixedUnit = UIFixedUnit.ViewportHeight;
                ptr += 2;
                return true;
            }

            fixedUnit = default;
            return false;
        }

        private bool TryParseQuotedString(char quote, out CharSpan span, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            uint start = ptr;

            if ((whitespaceHandling & WhitespaceHandling.ConsumeBefore) != 0) {
                ConsumeWhiteSpaceAndComments();
            }

            if (!HasMoreTokens || data[ptr] != quote) {
                ptr = start;
                span = default;
                return false;
            }

            ptr++;

            uint rangeStart = ptr;
            for (uint i = ptr; i < dataEnd; i++) {
                if (data[i] == quote && data[i - 1] != '\\') {
                    span = new CharSpan(data, (int) rangeStart, (int) i);
                    ptr = i + 1; // step over quote;
                    if ((whitespaceHandling & WhitespaceHandling.ConsumeAfter) != 0) {
                        ConsumeWhiteSpaceAndComments();
                    }

                    return true;
                }
            }

            ptr = start;
            span = default;
            return false;
        }

        public bool TryParseSingleQuotedString(out CharSpan span, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            return TryParseQuotedString('\'', out span, whitespaceHandling);
        }

        public bool TryParseDoubleQuotedString(out CharSpan span, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            return TryParseQuotedString('"', out span, whitespaceHandling);
        }

        public bool TryParseAnyQuotedString(out CharSpan span, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            return TryParseQuotedString('\'', out span, whitespaceHandling) || TryParseQuotedString('"', out span, whitespaceHandling);
        }

        public void ParseFloatAttr(string key, out float floatValue, float defaultValue) {
            if (!TryParseFloatAttr(key, out floatValue)) {
                floatValue = defaultValue;
            }
        }

        /// <summary>
        /// key=value
        /// </summary>
        public bool TryParseFloatAttr(string key, out float floatValue) {
            uint i = ptr;
            uint save = ptr;
            char startChar = key[0];

            while (i != dataEnd) {
                if (data[i] == startChar) {
                    ptr = i;
                    if (Previous != ' ') {
                        i++;
                        continue;
                    }

                    if (TryMatchRange(key) && TryParseCharacter('=')) {
                        if (ptr + 1 >= dataEnd) {
                            i++;
                            continue;
                        }

                        char openQuote = data[ptr];
                        ptr++;
                        if (openQuote != '\'' && openQuote != '"') {
                            openQuote = '\0';
                        }

                        if (!TryParseFloat(out floatValue)) {
                            i++;
                            continue;
                        }

                        if (openQuote != '\0' && !TryParseCharacter(openQuote)) {
                            i++;
                            continue;
                        }

                        ptr = save; // this method doesnt consume the stream
                        return true;
                    }
                }

                i++;
            }

            ptr = save;
            floatValue = 0;
            return false;
        }

        public bool TryParseColorAttr(string key, out Color32 colorValue, char quote = '\0') {
            uint i = ptr;
            uint save = ptr;
            char startChar = key[0];

            while (i != dataEnd) {
                if (data[i] == startChar) {
                    ptr = i;
                    if (Previous != ' ') {
                        i++;
                        continue;
                    }

                    if (TryMatchRange(key) && TryParseCharacter('=')) {
                        char openQuote = data[ptr];

                        if (openQuote != '\'' && openQuote != '"') {
                            openQuote = '\0';
                        }

                        if (!TryParseColorProperty(out colorValue)) {
                            i++;
                            continue;
                        }

                        if (openQuote != '\0' && !TryParseCharacter(openQuote)) {
                            i++;
                            continue;
                        }

                        ptr = save; // this method doesnt consume the stream
                        return true;
                    }
                }

                i++;
            }

            ptr = save;
            colorValue = default;
            return false;
        }

        public void SetCommentMode(CommentMode commentMode) {
            this.commentMode = commentMode;
        }

        public bool ConsumeUntilFound(string str, out CharSpan result) {
            fixed (char* buffer = str) {
                return ConsumeUntilFound(buffer, str.Length, out result);
            }
        }

        public bool ConsumeUntilFound(char* str, int length, out CharSpan result) {
            if (length == 0 || str == null) {
                result = default;
                return false;
            }

            uint max = (uint) (dataEnd - length);
            uint idx = ptr;
            while (idx < max) {
                while (idx < max && data[idx] != str[0]) {
                    idx++;
                }

                if (length == 1) {
                    result = new CharSpan(data, (int) ptr, (int) idx + 1, baseOffset);
                    ptr = (uint) (idx + length);
                    return true;
                }

                bool found = true;

                // todo -- memcmp might be faster, depends on how big length is
                for (int i = 1; i < length; i++) {
                    if (data[idx + i] != str[i]) {
                        found = false;
                        break;
                    }
                }

                if (found) {
                    result = new CharSpan(data, (int) ptr, (int) idx, baseOffset);
                    ptr = idx + (uint) length;
                    return true;
                }

                idx++;
            }

            result = default;
            return false;
        }

        public bool TryMatchRangeFast(char* str, int length) {
            if (ptr + length < dataEnd) {
                bool found = false;
                for (int i = 0; i < length; i++) {
                    if (data[ptr + i] != str[i]) {
                        break;
                    }
                }

                return true;
            }

            return false;
        }

        public bool TryGetBraceStream(out CharStream charStream, WhitespaceHandling whitespaceHandling = WhitespaceHandling.ConsumeAll) {
            return TryGetSubStream('{', '}', out charStream, whitespaceHandling);
        }

        public bool TryGetParenStream(out CharStream charStream) {
            return TryGetSubStream('(', ')', out charStream);
        }

        public bool TryGetSquareBracketStream(out CharStream charStream) {
            return TryGetSubStream('[', ']', out charStream);
        }

        public CheckedArray<char> ToCheckedArray() {
            return new CheckedArray<char>(data + ptr, (int) (dataEnd - ptr));
        }

        public bool TryGetSubstreamTo(string target, out CharStream stream) {
            uint i = ptr;
            stream = default;
            uint start = ptr;
            if (target == null || target.Length == 0) {
                return false;
            }
            
            fixed (char* cbuffer = target) {
                while (i < dataEnd) {
                    char c = data[i];
                    if (c == cbuffer[0]) {
                        if (i + target.Length >= dataEnd) {
                            return false;
                        }

                        bool match = true;
                        for (int k = 1; k < target.Length; k++) {
                            if (cbuffer[k] != data[i + k]) {
                                match = false;
                                break;
                            }
                        }

                        if (match) {
                            stream = new CharStream(data, start, i - 1);
                            ptr = i + (uint) target.Length;
                            return true;
                        }
                        
                    }
                }
            }

            return false;
        }

        public CharStream Trim() {
            uint start = ptr;
            uint end = dataEnd;
            
            for (uint i = start; i < dataEnd; i++) {
                if (char.IsWhiteSpace(data[i])) {
                    start++;
                }
                else {
                    break;
                }
            }

            for (uint i = dataEnd - 1; i >= start; i--) {
                if (char.IsWhiteSpace(data[i])) {
                    end--;
                }
                else {
                    break;
                }
            }

            return new CharStream(data, start, end);
        }

        public void CommaSplit(StructList<CharSpan> spanList) {
            while (HasMoreTokens) {

                uint start = ptr;
                ConsumeWhiteSpace();
                
                if (TryGetSubstreamTo(',', out CharStream stream)) {
                    spanList.Add(new CharSpan(stream).Trim());
                }
                else {
                    spanList.Add(new CharSpan(this).Trim());
                    ptr = dataEnd;
                    return;
                }

                if (ptr == start) {
                    throw new Exception("Parse loop");
                }
                
            }
            
            ConsumeWhiteSpace();
        }

        public void WhitespaceSplit(StructList<CharSpan> spanList) {
            while (HasMoreTokens) {

                uint start = ptr;
                ConsumeWhiteSpace();
                
                if (TryGetSubstreamTo(' ', out CharStream stream)) {
                    spanList.Add(new CharSpan(stream).Trim());
                }
                else {
                    spanList.Add(new CharSpan(this).Trim());
                    ptr = dataEnd;
                    return;
                }

                if (ptr == start) {
                    throw new Exception("Parse loop");
                }
                
            }
            
            ConsumeWhiteSpace();

        }

        public bool TrySplitOnLastIndexOf(string target, out CharStream stream) {
            uint i = (uint)(dataEnd - target.Length);
            stream = default;
            fixed (char* cbuffer = target) {
            
                while (i >= ptr + target.Length - 1) {

                    bool allEqual = true;
                    for (uint c = 0; c < target.Length; c++) {
                        if (data[i + c] != cbuffer[c]) {
                            allEqual = false;
                            break;
                        }
                    }

                    if (allEqual) {
                        stream = new CharStream(data, (uint)(i + target.Length), dataEnd);
                        dataEnd = i;
                        return true;
                    }
                    i--;
                }
            }

            stream = default;
            return false;
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

        public readonly ushort rangeStart;
        public readonly ushort rangeEnd;

        public int Length => rangeEnd - rangeStart;
        public bool HasValue => Length > 0;

        public ReflessCharSpan(int rangeStart, int rangeEnd) {
            this.rangeStart = (ushort) rangeStart;
            this.rangeEnd = (ushort) rangeEnd;
        }

        public ReflessCharSpan(CharSpan span) {
            this.rangeStart = span.rangeStart;
            this.rangeEnd = span.rangeEnd;
        }

        public ReflessCharSpan(CharStream stream) {
            this.rangeStart = (ushort) stream.Ptr;
            this.rangeEnd = (ushort) stream.End;
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

        public readonly ushort rangeStart;
        public readonly ushort rangeEnd;
        public readonly int baseOffset;
        public char* data { get; }

        public bool HasValue => Length > 0;

        public int Length => data != null ? rangeEnd - rangeStart : 0;

        // public CharSpan(char[] data, int rangeStart, int rangeEnd) {
        //     fixed (char* charptr = data) {
        //         this.data = charptr;
        //     }
        //
        //     this.rangeStart = (ushort) rangeStart;
        //     this.rangeEnd = (ushort) rangeEnd;
        // }

        public CharSpan(char* data, int rangeStart, int rangeEnd, int baseOffset = 0) {
            this.data = data;
            this.rangeStart = (ushort) rangeStart;
            this.rangeEnd = (ushort) rangeEnd;
            this.baseOffset = baseOffset;
        }

        public CharSpan(CharStream stream) {
            this.data = stream.Data;
            this.rangeStart = (ushort) stream.Ptr;
            this.rangeEnd = (ushort) stream.End;
            this.baseOffset = stream.baseOffset;
        }

        public CharSpan(char* data, RangeInt range) : this(data, range.start, range.end) { }

        public static bool operator ==(CharSpan a, string b) {
            return StringUtil.EqualsRangeUnsafe(b, a.data, a.rangeStart, a.rangeEnd - a.rangeStart);
        }

        public static bool operator !=(CharSpan a, string b) {
            return !StringUtil.EqualsRangeUnsafe(b, a.data, a.rangeStart, a.rangeEnd - a.rangeStart);
        }

        public static bool operator !=(string b, CharSpan a) {
            return !StringUtil.EqualsRangeUnsafe(b, a.data, a.rangeStart, a.rangeEnd - a.rangeStart);
        }

        public static bool operator ==(string b, CharSpan a) {
            return StringUtil.EqualsRangeUnsafe(b, a.data, a.rangeStart, a.rangeEnd - a.rangeStart);
        }

        public static bool operator ==(CharSpan a, CharSpan b) {
            // this is a value comparison
            int aLen = a.rangeEnd - a.rangeStart;
            int bLen = b.rangeEnd - b.rangeStart;
            if (aLen != bLen) return false;
            for (int i = 0; i < aLen; i++) {
                if (a.data[a.rangeStart + i] != b.data[b.rangeStart + i]) {
                    return false;
                }
            }

            return true;
        }

        public bool EqualsIgnoreCase(string str) {
            if (rangeEnd - rangeStart != str.Length) return false;

            fixed (char* s = str) {
                int idx = 0;
                for (int i = rangeStart; i < rangeEnd; i++) {
                    char strChar = s[idx++];
                    char dataChar = data[i];

                    char c1 = strChar >= 'a' && strChar <= 'z' ? char.ToLower(strChar) : strChar;
                    char c2 = dataChar >= 'a' && dataChar <= 'z' ? char.ToLower(dataChar) : dataChar;

                    if (c1 != c2) {
                        return false;
                    }
                }
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

            char* buffer = stackalloc char[length + 1];
            int idx = 0;
            for (int i = rangeStart; i < rangeEnd; i++) {
                buffer[idx++] = char.ToLower(data[i]);
            }

            buffer[length] = '\0';
            return new string(buffer);
        }

        public void ToLower(char[] buffer) {
            int idx = 0;

            for (int i = rangeStart; i < rangeEnd; i++) {
                buffer[idx++] = char.ToLower(data[i]);
            }
        }

        public void ToLower(char* buffer) {
            int idx = 0;

            for (int i = rangeStart; i < rangeEnd; i++) {
                buffer[idx++] = char.ToLower(data[i]);
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

        public char this[int i] => data[i];

        public int IndexOf(char c) {
            for (int i = rangeStart; i < rangeEnd; i++) {
                if (data[i] == c) {
                    return i;
                }
            }

            return -1;
        }

        public int LastIndexOf(char c) {
            for (int i = rangeEnd - 1; i >= rangeStart; i--) {
                if (data[i] == c) {
                    return i;
                }
            }

            return -1;
        }

        public RangeInt GetContentRange() {
            return new RangeInt(rangeStart, rangeEnd - rangeStart);
        }

        public bool TryParseColor(out Color32 color) {
            return new CharStream(this).TryParseColorProperty(out color);
        }

        public LineInfo GetLineInfo() {
            int line = 1; // start counting at line 1
            int x = 0;

            for (int i = 0; i < rangeStart; i++) {
                if (data[i] == '\n') {
                    line++;
                    x = i + 1;
                }
            }

            int col = 1;
            for (int i = x; i < rangeStart - 1; i++) {
                col++;
            }

            return new LineInfo(line, col);
        }

        public bool Contains(char c) {
            for (int i = rangeStart; i < rangeEnd; i++) {
                char test = data[i];
                if (test == c) {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(char c, out int idx) {
            for (int i = rangeStart; i < rangeEnd; i++) {
                char test = data[i];
                if (test == c) {
                    idx = i;
                    return true;
                }
            }

            idx = -1;
            return false;
        }

        public bool Contains(string str) {
            char c = str[0];
            for (int i = rangeStart; i < rangeEnd; i++) {
                char test = data[i];
                if (test == c) {
                    if (StringUtil.EqualsRangeUnsafe(str, data, i, str.Length)) {
                        return true;
                    }
                }

                if (i + str.Length >= rangeEnd) {
                    return false;
                }
            }

            return false;
        }

        public bool StartsWith(string str) {
            if (HasValue || Length < str.Length) {
                return false;
            }

            return StringUtil.EqualsRangeUnsafe(str, data, rangeStart, str.Length);
        }

        public bool StartsWith(char c, bool trim = true) {
            if (!trim) {
                return HasValue && data[rangeStart] == c;
            }

            for (int i = rangeStart; i < rangeEnd; i++) {
                char test = data[i];

                if (char.IsWhiteSpace(test)) {
                    continue;
                }

                if (test == c) {
                    return true;
                }

                return false;
            }

            return false;
        }

        public bool EndsWith(char c, bool trim = true) {
            if (!trim) {
                return HasValue && data[rangeEnd - 1] == c;
            }

            for (int i = rangeEnd - 1; i != rangeStart; i--) {
                char test = data[i];

                if (char.IsWhiteSpace(test)) {
                    continue;
                }

                if (test == c) {
                    return true;
                }

                return false;
            }

            return false;
        }

        public CharSpan Substring(int length) {
            return new CharSpan(data, rangeStart + length, rangeEnd);
        }

        public bool ContainsCharacter(char c, out int idx) {
            for (int i = rangeStart; i < rangeEnd; i++) {
                char test = data[i];

                if (test == c) {
                    idx = i;
                    return true;
                }

            }

            idx = -1;
            return false;
        }

        public CharSpan GetSubSpanTo(int dotIdx) {
            if (dotIdx < rangeStart) dotIdx = rangeStart;
            return new CharSpan(data, rangeStart, dotIdx);
        }

        public CharSpan GetSubSpanFrom(int dotIdx) {
            if (dotIdx < rangeStart) dotIdx = rangeStart - 1; // -1 account for + 1 on next line
            return new CharSpan(data, dotIdx + 1, rangeEnd);
        }


    }

}