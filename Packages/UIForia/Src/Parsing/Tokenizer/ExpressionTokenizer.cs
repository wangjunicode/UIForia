using System;
using System.Collections.Generic;

namespace UIForia {

    public static class ExpressionTokenizer {

        public static int ConsumeWhiteSpace(int start, string input) {
            int ptr = start;
            if (ptr >= input.Length) return input.Length;
            while (ptr < input.Length && char.IsWhiteSpace(input[ptr])) {
                ptr++;
            }

            return ptr;
        }

        public static int TryReadCharacters(int ptr, string input, string match, ExpressionTokenType expressionTokenType, List<ExpressionToken> output) {
            if (ptr + match.Length > input.Length) return ptr;
            for (int i = 0; i < match.Length; i++) {
                if (input[ptr + i] != match[i]) {
                    return ptr;
                }
            }
            
            output.Add(new ExpressionToken(expressionTokenType, match));
            return TryConsumeWhiteSpace(ptr + match.Length, input);
        }

        public static int TryConsumeWhiteSpace(int ptr, string input) {
            return ConsumeWhiteSpace(ptr, input);
        }

        public static int TryReadDigit(int ptr, string input, List<ExpressionToken> output) {
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

            output.Add(new ExpressionToken(ExpressionTokenType.Number, input.Substring(startIndex, ptr - startIndex)));
            return TryConsumeWhiteSpace(ptr, input);
        }

        private static int TryReadIdentifier(int ptr, string input, List<ExpressionToken> output) {
            int start = ptr;
            if (ptr >= input.Length) return input.Length;
            char first = input[ptr];
            if (!char.IsLetter(first) && first != '_' && first != '$') return ptr;

            while (ptr < input.Length && (char.IsLetterOrDigit(input[ptr]) || input[ptr] == '_' || input[ptr] == '$')) {
                ptr++;
            }
//            ptr = TryReadCharacters(ptr, input, "true", TokenType.Boolean, output, true);
//            ptr = TryReadCharacters(ptr, input, "false", TokenType.Boolean, output, true);
//            ptr = TryReadCharacters(ptr, input, "as", TokenType.As, output, true);
//            ptr = TryReadCharacters(ptr, input, "is", TokenType.Is, output, true);
//            ptr = TryReadCharacters(ptr, input, "new", TokenType.New, output, true);
//            ptr = TryReadCharacters(ptr, input, "typeof", TokenType.TypeOf, output, true);
//            ptr = TryReadCharacters(ptr, input, "default", TokenType.Default, output, true);
//            ptr = TryReadCharacters(ptr, input, "null", TokenType.Null, output, true);
            string identifier = input.Substring(start, ptr - start);
            if (identifier == "null") {
                output.Add(new ExpressionToken(ExpressionTokenType.Null, "null"));
            }
            else if (identifier == "true") {
                output.Add(new ExpressionToken(ExpressionTokenType.Boolean, "true"));
            }
            else if (identifier == "false") {
                output.Add(new ExpressionToken(ExpressionTokenType.Boolean, "false"));
            }
            else if (identifier == "as") {
                output.Add(new ExpressionToken(ExpressionTokenType.As, "as"));
            }
            else if (identifier == "is") {
                output.Add(new ExpressionToken(ExpressionTokenType.Is, "is"));
            }
            else if (identifier == "new") {
                output.Add(new ExpressionToken(ExpressionTokenType.New, "new"));
            }
            else if (identifier == "typeof") {
                output.Add(new ExpressionToken(ExpressionTokenType.TypeOf, "typeof"));
            }
            else if (identifier == "default") {
                output.Add(new ExpressionToken(ExpressionTokenType.Default, "default"));
            }
            else {
                output.Add(new ExpressionToken(ExpressionTokenType.Identifier, identifier));
            }

            return TryConsumeWhiteSpace(ptr, input);
        }

        // todo handle {} inside of strings
        // read until end or unescaped {
        // if unescaped { found, find matching index
        // add token for string 0 to index({)
        // add + token
        // run parse loop on contents of {}
        private static int TryReadString(int ptr, string input, List<ExpressionToken> output) {
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

            // strip the quotes
            output.Add(new ExpressionToken(ExpressionTokenType.String, input.Substring(start + 1, ptr - start - 2)));

            return TryConsumeWhiteSpace(ptr, input);
        }

        // todo take optional file / line number for error message
        public static List<ExpressionToken> Tokenize(string input, List<ExpressionToken> retn = null) {
            List<ExpressionToken> output = retn ?? new List<ExpressionToken>();

            int ptr = TryConsumeWhiteSpace(0, input);
            while (ptr < input.Length) {
                int start = ptr;

                ptr = TryReadCharacters(ptr, input, "@", ExpressionTokenType.At, output);
                ptr = TryReadCharacters(ptr, input, "&&", ExpressionTokenType.And, output);
                ptr = TryReadCharacters(ptr, input, "||", ExpressionTokenType.Or, output);
                ptr = TryReadCharacters(ptr, input, "==", ExpressionTokenType.Equals, output);
                ptr = TryReadCharacters(ptr, input, "!=", ExpressionTokenType.NotEquals, output);
                ptr = TryReadCharacters(ptr, input, ">=", ExpressionTokenType.GreaterThanEqualTo, output);
                ptr = TryReadCharacters(ptr, input, "<=", ExpressionTokenType.LessThanEqualTo, output);
                ptr = TryReadCharacters(ptr, input, ">", ExpressionTokenType.GreaterThan, output);
                ptr = TryReadCharacters(ptr, input, "<", ExpressionTokenType.LessThan, output);

                ptr = TryReadCharacters(ptr, input, "!", ExpressionTokenType.Not, output);
                ptr = TryReadCharacters(ptr, input, "+", ExpressionTokenType.Plus, output);
                ptr = TryReadCharacters(ptr, input, "-", ExpressionTokenType.Minus, output);
                ptr = TryReadCharacters(ptr, input, "/", ExpressionTokenType.Divide, output);
                ptr = TryReadCharacters(ptr, input, "*", ExpressionTokenType.Times, output);
                ptr = TryReadCharacters(ptr, input, "%", ExpressionTokenType.Mod, output);
                ptr = TryReadCharacters(ptr, input, "?", ExpressionTokenType.QuestionMark, output);
                ptr = TryReadCharacters(ptr, input, ":", ExpressionTokenType.Colon, output);

                ptr = TryReadCharacters(ptr, input, ".", ExpressionTokenType.Dot, output);
                ptr = TryReadCharacters(ptr, input, ",", ExpressionTokenType.Comma, output);
                ptr = TryReadCharacters(ptr, input, "(", ExpressionTokenType.ParenOpen, output);
                ptr = TryReadCharacters(ptr, input, ")", ExpressionTokenType.ParenClose, output);
                ptr = TryReadCharacters(ptr, input, "[", ExpressionTokenType.ArrayAccessOpen, output);
                ptr = TryReadCharacters(ptr, input, "]", ExpressionTokenType.ArrayAccessClose, output);
                ptr = TryReadCharacters(ptr, input, "{", ExpressionTokenType.ExpressionOpen, output);
                ptr = TryReadCharacters(ptr, input, "}", ExpressionTokenType.ExpressionClose, output);

                ptr = TryReadDigit(ptr, input, output);
                ptr = TryReadString(ptr, input, output);
                ptr = TryReadIdentifier(ptr, input, output);
                ptr = TryConsumeWhiteSpace(ptr, input);

                if (ptr == start && ptr < input.Length) {
                    throw new ParseException($"Tokenizer failed on string: {input}." +
                                        $" Processed {input.Substring(0, ptr)} as ({PrintTokenList(output)})" +
                                        $" but then got stuck on {input.Substring(ptr)}");
                }
            }

            return output;
        }

        private static string PrintTokenList(List<ExpressionToken> tokens) {
            string retn = "";
            for (int i = 0; i < tokens.Count; i++) {
                retn += tokens[i].ToString();
                if (i != tokens.Count - 1) {
                    retn += ", ";
                }
            }
            return retn;
        }

    }

}