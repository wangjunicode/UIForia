using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UIForia.Exceptions;
using UIForia.Parsing.Expressions.Tokenizer;
using UIForia.Util;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia.Parsing.Tokenizer {

    public unsafe struct TokenizerContextUnsafe {

        public int line;
        public int column;
        public int ptr;
        public CheckedArray<char> input;

        private int savedLine;
        private int savedColumn;
        private int savedPtr;

        public TokenizerContextUnsafe(CheckedArray<char> input) : this() {
            this.line = 1;
            this.column = 1;
            this.input = input;
        }

        public TokenizerContextUnsafe(CharSpan runSpan) : this() {
            this.line = 1;
            this.column = 1;
            this.input = new CheckedArray<char>(runSpan.data + runSpan.rangeStart, runSpan.Length);
        }

        public void Save() {
            savedLine = line;
            savedColumn = column;
            savedPtr = ptr;
        }

        public void Restore() {
            line = savedLine;
            column = savedColumn;
            ptr = savedPtr;
        }

        public void Advance() {
            if (ptr >= input.size) return;
            if (input[ptr] == '\n') {
                line++;
                column = 1;
            }
            else {
                column++;
            }

            ptr++;

        }

        public void Advance(int characters) {
            while (HasMore() && characters > 0) {
                Advance();
                characters--;
            }

        }

        public bool IsConsumed() {
            return ptr >= input.size;
        }

        public bool HasMore() {
            return ptr < input.size;
        }

        public bool HasMuchMore(int howMuchMore) {
            return ptr + howMuchMore < input.size;
        }

    }

    public static unsafe class ExpressionTokenizerUnsafe {

        private static readonly char stringCharacter = '\"';

        private static void TryReadCharacterSequence(ref TokenizerContextUnsafe context, string match1, string match2, ExpressionTokenType expressionTokenType, StructList<ExpressionToken> output) {

            if (context.ptr + match1.Length > context.input.size) return;

            int ptr = context.ptr;

            for (int i = 0; i < match1.Length; i++) {
                if (context.input[ptr++] != match1[i]) {
                    return;
                }
            }

            while (ptr < context.input.size) {
                if (char.IsWhiteSpace(context.input[ptr])) {
                    ptr++;
                }
                else {
                    break;
                }
            }

            for (int i = 0; i < match2.Length; i++) {
                if (context.input[ptr++] != match2[i]) {
                    return;
                }
            }

            output.Add(new ExpressionToken(expressionTokenType, match1 + " " + match2, context.line, context.column));
            context.Advance(ptr - context.ptr);
            TryConsumeWhiteSpace(ref context);
        }

        private static void TryReadCharacters(ref TokenizerContextUnsafe context, string match, ExpressionTokenType expressionTokenType, StructList<ExpressionToken> output) {
            if (context.ptr + match.Length > context.input.size) return;
            for (int i = 0; i < match.Length; i++) {
                if (context.input[context.ptr + i] != match[i]) {
                    return;
                }
            }

            output.Add(new ExpressionToken(expressionTokenType, match, context.line, context.column));
            context.Advance(match.Length);
            TryConsumeWhiteSpace(ref context);
        }

        private static void TryConsumeWhiteSpace(ref TokenizerContextUnsafe context) {
            if (context.IsConsumed()) {
                return;
            }

            while (context.HasMore() && char.IsWhiteSpace(context.input[context.ptr])) {
                context.Advance();
            }
        }

        private static void TryConsumeComment(ref TokenizerContextUnsafe context) {
            if (context.ptr + 1 >= context.input.size) {
                return;
            }

            if (context.input[context.ptr] == '/' && context.input[context.ptr + 1] == '/') {
                while (context.HasMore()) {
                    char current = context.input[context.ptr];
                    if (current == '\n') {
                        TryConsumeWhiteSpace(ref context);
                        TryConsumeComment(ref context);
                        return;
                    }

                    context.Advance();
                }

                return;
            }

            if (context.input[context.ptr] != '/' || context.input[context.ptr + 1] != '*') {
                return;
            }

            context.Advance(2);
            while (context.HasMore()) {
                if (context.input[context.ptr] == '*' && context.input[context.ptr + 1] == '/') {
                    context.Advance(2);
                    TryConsumeWhiteSpace(ref context);
                    TryConsumeComment(ref context);
                    return;
                }

                context.Advance();
            }
        }

        private static void TryReadDigit(ref TokenizerContextUnsafe context, StructList<ExpressionToken> output) {
            if (context.IsConsumed()) return;
            bool foundDot = false;
            int startIndex = context.ptr;

            context.Save();
            if (context.input[context.ptr] == '-') {
                context.Advance();
            }

            if (!char.IsDigit(context.input[context.ptr])) {
                context.Restore();
                return;
            }

            while (context.HasMore() && (char.IsDigit(context.input[context.ptr]) || (!foundDot && context.input[context.ptr] == '.'))) {
                if (context.input[context.ptr] == '.') {
                    foundDot = true;
                }

                context.Advance();
            }

            if (context.HasMore()) {
                char next = context.input[context.ptr];
                // todo -- enable the below to making parsing numbers better in the compiler (since we already know what type to try to parse it as)
                //ExpressionTokenType type = ExpressionTokenType.Number;
//                if (next == 'f') {
//                    type = ExpressionTokenType.Number_Float;
//                }
//
//                if (next == 'd') {
//                    type = ExpressionTokenType.Number_Double;
//                }
//
//                if (next == 'l') {
//                    type = ExpressionTokenType.Number_Long;
//                }
//
//                if (next == 'u') {
//                    // todo -- check for ul here
//                    type = ExpressionTokenType.Number_UInt;
//                }
//
//                if (next == 'm') {
//                    type = ExpressionTokenType.Number_Decimal;
//                }

                if (next == 'f' || next == 'd' || next == 'l' || next == 'u' || next == 'm') {
                    if (next != '.') {
                        context.Advance();
                    }
                }
            }

//            if (context.HasMore() 
//                && context.input[context.ptr] == 'f'
//                && context.input[context.ptr - 1] != '.') {
//                context.Advance();
//            }

            int length = context.ptr - startIndex;
            string digit = new string(context.input.array + startIndex, 0, length);

            context.Restore();
            output.Add(new ExpressionToken(ExpressionTokenType.Number, digit, context.line, context.column));
            context.Advance(length);

            TryConsumeWhiteSpace(ref context);

        }

        private static void TryReadIdentifier(ref TokenizerContextUnsafe context, StructList<ExpressionToken> output) {
            if (context.IsConsumed()) return;
            int start = context.ptr;
            char first = context.input[context.ptr];

            if (!char.IsLetter(first) && first != '_' && first != '$') return;

            context.Save();

            while (context.HasMore()) {
                char character = context.input[context.ptr];

                if (!(char.IsLetterOrDigit(character) || character == '_' || character == '$')) {
                    break;
                }

                context.Advance();
            }

            int length = context.ptr - start;
            string identifier = new string(context.input.array + start, 0, length);
            context.Restore();
            output.Add(TransformIdentifierToTokenType(ref context, identifier));
            context.Advance(length);
            TryConsumeWhiteSpace(ref context);
        }

        private static ExpressionToken TransformIdentifierToTokenType(ref TokenizerContextUnsafe context, string identifier) {
            switch (identifier) {
                case "var": return new ExpressionToken(ExpressionTokenType.Var, identifier, context.line, context.column);
                case "if": return new ExpressionToken(ExpressionTokenType.If, identifier, context.line, context.column);
                case "else": return new ExpressionToken(ExpressionTokenType.Else, identifier, context.line, context.column);
                case "for": return new ExpressionToken(ExpressionTokenType.For, identifier, context.line, context.column);
                case "while": return new ExpressionToken(ExpressionTokenType.While, identifier, context.line, context.column);
                case "return": return new ExpressionToken(ExpressionTokenType.Return, identifier, context.line, context.column);
                case "null": return new ExpressionToken(ExpressionTokenType.Null, identifier, context.line, context.column);
                case "true": return new ExpressionToken(ExpressionTokenType.Boolean, identifier, context.line, context.column);
                case "false": return new ExpressionToken(ExpressionTokenType.Boolean, identifier, context.line, context.column);
                case "as": return new ExpressionToken(ExpressionTokenType.As, identifier, context.line, context.column);
                case "is": return new ExpressionToken(ExpressionTokenType.Is, identifier, context.line, context.column);
                case "new": return new ExpressionToken(ExpressionTokenType.New, identifier, context.line, context.column);
                case "typeof": return new ExpressionToken(ExpressionTokenType.TypeOf, identifier, context.line, context.column);
                case "default": return new ExpressionToken(ExpressionTokenType.Default, identifier, context.line, context.column);

                default: {
                    return new ExpressionToken(ExpressionTokenType.Identifier, identifier, context.line, context.column);
                }
            }
        }

        // todo handle {} inside of strings
        // read until end or unescaped {
        // if unescaped { found, find matching index
        // add token for string 0 to index({)
        // add + token
        // run parse loop on contents of {}

        private static void TryReadString(ref TokenizerContextUnsafe context, StructList<ExpressionToken> output) {
            if (context.IsConsumed()) return;
            if (context.input[context.ptr] != stringCharacter) return;
            int start = context.ptr;

            context.Save();
            context.Advance();

            while (context.HasMore() && context.input[context.ptr] != stringCharacter) {
                context.Advance();
            }

            if (context.IsConsumed()) {
                context.Restore();
                return;
            }

            if (context.input[context.ptr] != stringCharacter) {
                context.Restore();
                return;
            }

            context.Advance();

            // strip the quotes
            // "abc" 
            // 01234
            int length = context.ptr - start;
            string substring = new string(context.input.array + start + 1, 0, length - 2);
            if (substring.Contains("\\n")) {
                substring = substring.Replace("\\n", "\n");
            }
            // handle unicode literals in string
            if (substring.Contains("\\u")) {

                StructList<char> charBuffer = StructList<char>.Get();
                
                string[] split = s_UnicodeRegex.Split(substring);

                foreach (string s in split) {
                    try {
                        if (s.Length == 4) {
                            // the decode will throw an exception if failed, exception is caught and just treated as normal block of 4 chars in that case (no unescaped)
                            char decoded = ((char) Convert.ToUInt16(s, 16));
                            charBuffer.Add(decoded);
                        }
                        else {
                            for (int i = 0; i < s.Length; i++) {
                                charBuffer.Add(s[i]);
                            }
                        }
                    }
                    catch (Exception e) {
                        // length was 4 but not decoded to uint16
                        for (int i = 0; i < s.Length; i++) {
                            charBuffer.Add(s[i]);
                        }
                    }
                }

                fixed (char* ptr = charBuffer.array) {
                    substring = new string(ptr, 0, charBuffer.size);
                }

                charBuffer.Release();
            }

            context.Restore();
            output.Add(new ExpressionToken(ExpressionTokenType.String, substring, context.line, context.column));
            context.Advance(length);

            TryConsumeWhiteSpace(ref context);
        }

        private static readonly Regex s_UnicodeRegex = new Regex(@"\\u([a-fA-F\d]{4})");
        

        // todo take optional file / line number for error message
        public static int Tokenize(CheckedArray<char> input, StructList<ExpressionToken> output) {

            TokenizerContextUnsafe context = new TokenizerContextUnsafe(input);

            TryConsumeWhiteSpace(ref context);

            TryConsumeWhiteSpace(ref context);
            while (context.ptr < input.size) {
                int start = context.ptr;

                TryConsumeComment(ref context);

                TryReadStringInterpolation(ref context, output);

                TryReadCharacters(ref context, "@", ExpressionTokenType.At, output);
                TryReadCharacters(ref context, "&&", ExpressionTokenType.AndAlso, output);
                TryReadCharacters(ref context, "??", ExpressionTokenType.Coalesce, output);
                TryReadCharacters(ref context, "?.", ExpressionTokenType.Elvis, output);
                TryReadCharacters(ref context, "||", ExpressionTokenType.OrElse, output);
                TryReadCharacters(ref context, "=>", ExpressionTokenType.LambdaArrow, output);
                TryReadCharacters(ref context, "==", ExpressionTokenType.Equals, output);
                TryReadCharacters(ref context, "!=", ExpressionTokenType.NotEquals, output);

                TryReadCharacters(ref context, "++", ExpressionTokenType.Increment, output);
                TryReadCharacters(ref context, "--", ExpressionTokenType.Decrement, output);

                TryReadCharacters(ref context, "=", ExpressionTokenType.Assign, output);
                TryReadCharacters(ref context, "+=", ExpressionTokenType.AddAssign, output);
                TryReadCharacters(ref context, "-=", ExpressionTokenType.SubtractAssign, output);
                TryReadCharacters(ref context, "*=", ExpressionTokenType.MultiplyAssign, output);
                TryReadCharacters(ref context, "/=", ExpressionTokenType.DivideAssign, output);
                TryReadCharacters(ref context, "%=", ExpressionTokenType.ModAssign, output);
                TryReadCharacters(ref context, "&=", ExpressionTokenType.AndAssign, output);
                TryReadCharacters(ref context, "|=", ExpressionTokenType.OrAssign, output);
                TryReadCharacters(ref context, "^=", ExpressionTokenType.XorAssign, output);
                TryReadCharacters(ref context, "<<=", ExpressionTokenType.LeftShiftAssign, output);
                TryReadCharacters(ref context, ">>=", ExpressionTokenType.RightShiftAssign, output);

                TryReadCharacters(ref context, ">=", ExpressionTokenType.GreaterThanEqualTo, output);
                TryReadCharacters(ref context, "<=", ExpressionTokenType.LessThanEqualTo, output);
                TryReadCharacters(ref context, ">", ExpressionTokenType.GreaterThan, output);
                TryReadCharacters(ref context, "<", ExpressionTokenType.LessThan, output);
                TryReadCharacters(ref context, "<", ExpressionTokenType.LessThan, output);

                TryReadCharacters(ref context, "!", ExpressionTokenType.Not, output);
                TryReadCharacters(ref context, "+", ExpressionTokenType.Plus, output);
                TryReadCharacters(ref context, "-", ExpressionTokenType.Minus, output);
                TryReadCharacters(ref context, "/", ExpressionTokenType.Divide, output);
                TryReadCharacters(ref context, "*", ExpressionTokenType.Times, output);
                TryReadCharacters(ref context, "%", ExpressionTokenType.Mod, output);
                TryReadCharacters(ref context, "~", ExpressionTokenType.BinaryNot, output);
                TryReadCharacters(ref context, "|", ExpressionTokenType.BinaryOr, output);
                TryReadCharacters(ref context, "&", ExpressionTokenType.BinaryAnd, output);
                TryReadCharacters(ref context, "^", ExpressionTokenType.BinaryXor, output);
                TryReadCharacters(ref context, "?", ExpressionTokenType.QuestionMark, output);
                TryReadCharacters(ref context, ":", ExpressionTokenType.Colon, output);
                TryReadCharacters(ref context, ";", ExpressionTokenType.SemiColon, output);

                TryReadCharacters(ref context, ".", ExpressionTokenType.Dot, output);
                TryReadCharacters(ref context, ",", ExpressionTokenType.Comma, output);
                TryReadCharacters(ref context, "(", ExpressionTokenType.ParenOpen, output);
                TryReadCharacters(ref context, ")", ExpressionTokenType.ParenClose, output);
                TryReadCharacters(ref context, "[", ExpressionTokenType.ArrayAccessOpen, output);
                TryReadCharacters(ref context, "]", ExpressionTokenType.ArrayAccessClose, output);
                TryReadCharacters(ref context, "{", ExpressionTokenType.BracketOpen, output);
                TryReadCharacters(ref context, "}", ExpressionTokenType.BracketClose, output);

                TryReadCharacters(ref context, "if", ExpressionTokenType.If, output);
                TryReadCharacters(ref context, "out ", ExpressionTokenType.Out, output);
                TryReadCharacters(ref context, "ref ", ExpressionTokenType.Out, output);

                TryReadCharacterSequence(ref context, "else", "if", ExpressionTokenType.ElseIf, output);
                TryReadCharacters(ref context, "else", ExpressionTokenType.Else, output);

                TryReadDigit(ref context, output);
                TryReadString(ref context, output);
                TryReadIdentifier(ref context, output);
                TryConsumeWhiteSpace(ref context);

                if (context.ptr == start && context.ptr < input.size) {
                    int nextNewLine = 0; // input.IndexOf("\n", context.ptr + 1, input.size - context.ptr - 1, StringComparison.Ordinal);
                    nextNewLine = Mathf.Clamp(nextNewLine, context.ptr + 1, input.size - 1);
                    string errorLine = new string(input.array + context.ptr, 0, nextNewLine - context.ptr); // Substring(context.ptr, nextNewLine - context.ptr));
                    throw new ExpressionParseException($"Tokenizer failed at line {context.line}, column {context.column}.\n" +
                                                       $" Processed {new string(input.array, 0, context.ptr)}\n" +
                                                       $" ...but then got stuck on {errorLine}.\n");
                }
            }

            return output.size;
        }

        private static void TryReadStringInterpolation(ref TokenizerContextUnsafe context, StructList<ExpressionToken> output) {

            if (context.ptr + 3 >= context.input.size) {
                return;
            }

            if (context.input[context.ptr] != '@' || context.input[context.ptr + 1] != '(') {
                return;
            }

            CharStream stream = new CharStream(context.input.array, context.ptr + 1, context.input.size);
            stream.SetCommentMode(CommentMode.None);
            if (!stream.TryGetSubStream('(', ')', out CharStream contentStream, WhitespaceHandling.None)) {
                return;
            }

            context.ptr += (stream.IntPtr - context.ptr);
            output.Add(new ExpressionToken(ExpressionTokenType.StringInterpolation, contentStream.ToString(), context.line, context.column));

        }

    }

}