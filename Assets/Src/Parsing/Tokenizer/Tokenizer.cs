using System;
using System.Collections.Generic;

namespace Src {

    // todo support ternary, Math, some other globals

    public static class Tokenizer {

        public static int ConsumeWhiteSpace(int start, string input) {
            int ptr = start;
            if (ptr >= input.Length) return input.Length;
            while (ptr < input.Length && char.IsWhiteSpace(input[ptr])) {
                ptr++;
            }

            return ptr;
        }

        public static int TryReadCharacters(int ptr, string input, string match, TokenType tokenType,
            List<DslToken> output) {
            if (ptr + match.Length > input.Length) return ptr;
            for (int i = 0; i < match.Length; i++) {
                if (input[ptr + i] != match[i]) {
                    return ptr;
                }
            }

            output.Add(new DslToken(tokenType, match));
            return TryConsumeWhiteSpace(ptr + match.Length, input);
        }

        public static int TryConsumeWhiteSpace(int ptr, string input) {
            return ConsumeWhiteSpace(ptr, input);
        }

        public static int TryReadDigit(int ptr, string input, List<DslToken> output) {
            bool foundDot = false;
            int startIndex = ptr;
            if (ptr >= input.Length) return input.Length;

            if (!char.IsDigit(input[ptr])) return ptr;

            // 1
            // 1.4
            // 1.4f

            while (ptr < input.Length && (char.IsDigit(input[ptr]) || (!foundDot && input[ptr] == '.'))) {
                if (input[ptr] == '.') {
                    foundDot = true;
                }

                ptr++;
            }

            if (ptr < input.Length && input[ptr] == 'f') {
                ptr++;
            }

            output.Add(new DslToken(TokenType.Number, input.Substring(startIndex, ptr - startIndex)));
            return TryConsumeWhiteSpace(ptr, input);
        }

        private static int TryReadIdentifier(int ptr, string input, List<DslToken> output) {
            int start = ptr;
            if (ptr >= input.Length) return input.Length;
            char first = input[ptr];
            if (!char.IsLetter(first) && first != '_' && first != '$') return ptr;

            while (ptr < input.Length && (char.IsLetterOrDigit(input[ptr]) || input[ptr] == '_' || input[ptr] == '$')) {
                ptr++;
            }

            output.Add(first == '$'
                ? new DslToken(TokenType.SpecialIdentifier, input.Substring(start, ptr - start))
                : new DslToken(TokenType.Identifier, input.Substring(start, ptr - start)));

            return TryConsumeWhiteSpace(ptr, input);
        }

        private static int TryReadString(int ptr, string input, List<DslToken> output) {
            int start = ptr;
            if (ptr >= input.Length) return input.Length;
            if (input[ptr] != '\'') return ptr;

            ptr++;

            while (ptr < input.Length && input[ptr] != '\'') {
                ptr++;
            }

            if (ptr >= input.Length) {
                return start;
            }

            if (input[ptr] != '\'') {
                return start;
            }

            ptr++;

            output.Add(new DslToken(TokenType.String, input.Substring(start, ptr - start)));

            return TryConsumeWhiteSpace(ptr, input);
        }

        public static List<DslToken> Tokenize(string input) {
            List<DslToken> output = new List<DslToken>();

            int ptr = TryConsumeWhiteSpace(0, input);
            while (ptr < input.Length) {
                int start = ptr;

                ptr = TryReadCharacters(ptr, input, "&&", TokenType.And, output);
                ptr = TryReadCharacters(ptr, input, "||", TokenType.Or, output);
                ptr = TryReadCharacters(ptr, input, "==", TokenType.Equals, output);
                ptr = TryReadCharacters(ptr, input, "!=", TokenType.NotEquals, output);
                ptr = TryReadCharacters(ptr, input, ">=", TokenType.GreaterThanEqualTo, output);
                ptr = TryReadCharacters(ptr, input, "<=", TokenType.LessThanEqualTo, output);
                ptr = TryReadCharacters(ptr, input, ">", TokenType.GreaterThan, output);
                ptr = TryReadCharacters(ptr, input, "<", TokenType.LessThan, output);

                ptr = TryReadCharacters(ptr, input, "!", TokenType.Not, output);
                ptr = TryReadCharacters(ptr, input, "+", TokenType.Plus, output);
                ptr = TryReadCharacters(ptr, input, "-", TokenType.Minus, output);
                ptr = TryReadCharacters(ptr, input, "/", TokenType.Divide, output);
                ptr = TryReadCharacters(ptr, input, "*", TokenType.Times, output);
                ptr = TryReadCharacters(ptr, input, "%", TokenType.Mod, output);
                ptr = TryReadCharacters(ptr, input, "?", TokenType.QuestionMark, output);
                ptr = TryReadCharacters(ptr, input, ":", TokenType.Colon, output);

                ptr = TryReadCharacters(ptr, input, ".", TokenType.Dot, output);
                ptr = TryReadCharacters(ptr, input, ",", TokenType.Comma, output);
                ptr = TryReadCharacters(ptr, input, "(", TokenType.ParenOpen, output);
                ptr = TryReadCharacters(ptr, input, ")", TokenType.ParenClose, output);
                ptr = TryReadCharacters(ptr, input, "[", TokenType.ArrayAccessOpen, output);
                ptr = TryReadCharacters(ptr, input, "]", TokenType.ArrayAccessClose, output);
                ptr = TryReadCharacters(ptr, input, "{", TokenType.ExpressionOpen, output);
                ptr = TryReadCharacters(ptr, input, "}", TokenType.ExpressionClose, output);
                ptr = TryReadCharacters(ptr, input, "true", TokenType.Boolean, output);
                ptr = TryReadCharacters(ptr, input, "false", TokenType.Boolean, output);
                ptr = TryReadCharacters(ptr, input, "@", TokenType.At, output);

                ptr = TryReadString(ptr, input, output);
                ptr = TryReadDigit(ptr, input, output);
                ptr = TryReadIdentifier(ptr, input, output);
                ptr = TryConsumeWhiteSpace(ptr, input);

                if (ptr == start && ptr < input.Length) {
                    throw new Exception("Tokenizer failed on string: " + input);
                }
            }

            return output;
        }

    }

}