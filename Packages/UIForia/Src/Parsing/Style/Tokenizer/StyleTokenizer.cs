using System.Collections.Generic;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Parsing.Style.Tokenizer {

    public static class StyleTokenizer {

        private static readonly string[] s_AcceptedTypes = {
            nameof(LayoutDirection),
            nameof(Color),
            "int",
            "float",
            "string",
            "bool",
        };

        public static int TryReadCharacters(int ptr, string input, string match, StyleTokenType styleTokenType, List<StyleToken> output) {
            if (ptr + match.Length > input.Length) return ptr;
            for (int i = 0; i < match.Length; i++) {
                if (input[ptr + i] != match[i]) {
                    return ptr;
                }
            }

            output.Add(new StyleToken(styleTokenType, match));
            return TryConsumeWhiteSpace(ptr + match.Length, input);
        }

        public static int TryConsumeWhiteSpace(int start, string input) {
            int ptr = start;
            if (ptr >= input.Length) return input.Length;
            while (ptr < input.Length && char.IsWhiteSpace(input[ptr])) {
                ptr++;
            }

            return ptr;
        }

        public static int TryConsumeComment(int ptr, string input) {
            if (ptr + 1 >= input.Length) {
                return ptr;
            }

            if (!(input[ptr] == '/' && input[ptr + 1] == '/')) {
                return ptr;
            }

            while (ptr < input.Length) {
                char current = input[ptr];
                if (current == '\n') {
                    ptr++;
                    return TryConsumeWhiteSpace(ptr, input);
                }

                ptr++;
            }

            return ptr;
        }

        public static int TryReadHashColor(int ptr, string input, List<StyleToken> output) {
            if (ptr >= input.Length) return input.Length;
            if (input[ptr] != '#') return ptr;

            int start = ptr;
            while (ptr < input.Length && input[ptr] != ';' && !char.IsWhiteSpace(input[ptr])) {
                ptr++;
            }

            output.Add(new StyleToken(StyleTokenType.HashColor, input.Substring(start, ptr - start)));

            return TryConsumeWhiteSpace(ptr, input);
        }

        public static int TryReadDigit(int ptr, string input, List<StyleToken> output) {
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

            output.Add(new StyleToken(StyleTokenType.Number, input.Substring(startIndex, ptr - startIndex)));
            return TryConsumeWhiteSpace(ptr, input);
        }

        private static int TryReadIdentifier(int ptr, string input, List<StyleToken> output) {
            int start = ptr;
            if (ptr >= input.Length) return input.Length;
            char first = input[ptr];
            if (!char.IsLetter(first) && first != '_' && first != '$') return ptr;

            while (ptr < input.Length && (char.IsLetterOrDigit(input[ptr]) || input[ptr] == '_' || input[ptr] == '$')) {
                ptr++;
            }

            string identifier = input.Substring(start, ptr - start);
            output.Add(TransformIdentifierToTokenType(identifier));
            return TryConsumeWhiteSpace(ptr, input);
        }

        private static StyleToken TransformIdentifierToTokenType(string identifier) {
            string identifierLowerCase = identifier.ToLower();
            switch (identifierLowerCase) {
                case "use": return new StyleToken(StyleTokenType.Use, identifierLowerCase);
                case "and": return new StyleToken(StyleTokenType.And, identifierLowerCase);
                case "not": return new StyleToken(StyleTokenType.Not, identifierLowerCase);
                case "style": return new StyleToken(StyleTokenType.Style, identifierLowerCase);
                case "animation": return new StyleToken(StyleTokenType.Animation, identifierLowerCase);
                case "texture": return new StyleToken(StyleTokenType.Texture, identifierLowerCase);
                case "audio": return new StyleToken(StyleTokenType.Audio, identifierLowerCase);
                case "cursor": return new StyleToken(StyleTokenType.Cursor, identifierLowerCase);
                case "export": return new StyleToken(StyleTokenType.Export, identifierLowerCase);
                case "const": return new StyleToken(StyleTokenType.Const, identifierLowerCase);
                case "import": return new StyleToken(StyleTokenType.Import, identifierLowerCase);
                case "attr": return new StyleToken(StyleTokenType.AttributeSpecifier, identifierLowerCase);
                case "true": return new StyleToken(StyleTokenType.Boolean, identifierLowerCase);
                case "false": return new StyleToken(StyleTokenType.Boolean, identifierLowerCase);
                case "from": return new StyleToken(StyleTokenType.From, identifierLowerCase);
                case "as": return new StyleToken(StyleTokenType.As, identifierLowerCase);
                case "rgba": return new StyleToken(StyleTokenType.Rgba, identifierLowerCase);
                case "rgb": return new StyleToken(StyleTokenType.Rgb, identifierLowerCase);
                case "url": return new StyleToken(StyleTokenType.Url, identifierLowerCase);
                default: {
                    for (int index = 0; index < s_AcceptedTypes.Length; index++) {
                        if (identifier == s_AcceptedTypes[index]) {
                            return new StyleToken(StyleTokenType.VariableType, identifier);
                        }
                    }

                    return new StyleToken(StyleTokenType.Identifier, identifier);
                }
            }
        }

        private static int TryReadString(int ptr, string input, List<StyleToken> output) {
            int start = ptr;
            if (ptr >= input.Length) return input.Length;
            if (input[ptr] != '"') return ptr;

            ptr++;

            while (ptr < input.Length && input[ptr] != '"') {
                ptr++;
            }

            if (ptr >= input.Length) {
                return start;
            }

            if (input[ptr] != '"') {
                return start;
            }

            ptr++;

            // strip the quotes
            output.Add(new StyleToken(StyleTokenType.String, input.Substring(start + 1, ptr - start - 2)));

            return TryConsumeWhiteSpace(ptr, input);
        }

        private static int TryReadValue(int ptr, string input, List<StyleToken> output) {
            int originalPosition = ptr;
            if (ptr >= input.Length) return input.Length;
            if (input[ptr] != '=') return ptr;

            ptr++;
            int start = TryConsumeWhiteSpace(ptr, input);

            while (ptr < input.Length && input[ptr] != ';' && input[ptr] != '\n') {
                if (input[ptr] == '/' && ptr + 1 < input.Length && input[ptr + 1] == '/') {
                    break;
                }

                ptr++;
            }

            if (ptr >= input.Length) {
                return originalPosition;
            }

            output.Add(new StyleToken(StyleTokenType.Value, input.Substring(start, ptr - start)));

            return TryConsumeWhiteSpace(ptr, input);
        }

        private static int TryReadAttribute(int ptr, string input, List<StyleToken> output) {
            if (ptr >= input.Length) return input.Length;

            int start = ptr;
            ptr = TryReadCharacters(ptr, input, "attr", StyleTokenType.AttributeSpecifier, output);

            if (start == ptr) {
                return ptr;
            }

            ptr = TryReadCharacters(ptr, input, ":", StyleTokenType.Colon, output);
            ptr = TryReadIdentifier(ptr, input, output);

            if (input[ptr] != '=') return ptr;
            ptr++;

            return TryReadString(ptr, input, output);
        }

        // todo take optional file / line number for error message
        public static List<StyleToken> Tokenize(string input, List<StyleToken> retn = null) {
            List<StyleToken> output = retn ?? new List<StyleToken>();

            int ptr = TryConsumeWhiteSpace(0, input);
            while (ptr < input.Length) {
                int start = ptr;

                ptr = TryConsumeComment(ptr, input);

                ptr = TryReadCharacters(ptr, input, "@", StyleTokenType.At, output);
                ptr = TryReadCharacters(ptr, input, ":", StyleTokenType.Colon, output);
                ptr = TryReadCharacters(ptr, input, "==", StyleTokenType.Equals, output);
                ptr = TryReadCharacters(ptr, input, "!=", StyleTokenType.NotEquals, output);
                ptr = TryReadCharacters(ptr, input, "!", StyleTokenType.BooleanNot, output);
                ptr = TryReadCharacters(ptr, input, "=", StyleTokenType.Equal, output);
                ptr = TryReadCharacters(ptr, input, ">", StyleTokenType.GreaterThan, output);
                ptr = TryReadCharacters(ptr, input, "<", StyleTokenType.LessThan, output);
                ptr = TryReadCharacters(ptr, input, "&&", StyleTokenType.BooleanAnd, output);
                ptr = TryReadCharacters(ptr, input, "||", StyleTokenType.BooleanOr, output);
                ptr = TryReadCharacters(ptr, input, "$", StyleTokenType.Dollar, output);
                ptr = TryReadCharacters(ptr, input, "+", StyleTokenType.Plus, output);
                ptr = TryReadCharacters(ptr, input, "-", StyleTokenType.Minus, output);
                ptr = TryReadCharacters(ptr, input, "/", StyleTokenType.Divide, output);
                ptr = TryReadCharacters(ptr, input, "*", StyleTokenType.Times, output);
                ptr = TryReadCharacters(ptr, input, "%", StyleTokenType.Mod, output);
                ptr = TryReadCharacters(ptr, input, "?", StyleTokenType.QuestionMark, output);

                ptr = TryReadCharacters(ptr, input, ".", StyleTokenType.Dot, output);
                ptr = TryReadCharacters(ptr, input, ",", StyleTokenType.Comma, output);
                ptr = TryReadCharacters(ptr, input, "(", StyleTokenType.ParenOpen, output);
                ptr = TryReadCharacters(ptr, input, ")", StyleTokenType.ParenClose, output);
                ptr = TryReadCharacters(ptr, input, "[", StyleTokenType.BracketOpen, output);
                ptr = TryReadCharacters(ptr, input, "]", StyleTokenType.BracketClose, output);
                ptr = TryReadCharacters(ptr, input, "{", StyleTokenType.BracesOpen, output);
                ptr = TryReadCharacters(ptr, input, "}", StyleTokenType.BracesClose, output);

                ptr = TryReadHashColor(ptr, input, output);
                ptr = TryReadDigit(ptr, input, output);
                ptr = TryReadString(ptr, input, output);
                ptr = TryReadIdentifier(ptr, input, output);
                ptr = TryConsumeWhiteSpace(ptr, input);

                ptr = TryReadCharacters(ptr, input, ";\n", StyleTokenType.EndStatement, output);
                ptr = TryReadCharacters(ptr, input, ";", StyleTokenType.EndStatement, output);
                ptr = TryReadCharacters(ptr, input, "\n", StyleTokenType.EndStatement, output);

                if (ptr == start && ptr < input.Length) {
                    throw new ParseException($"Tokenizer failed on string: {input}." +
                                             $" Processed {input.Substring(0, ptr)} as ({PrintTokenList(output)})" +
                                             $" but then got stuck on {input.Substring(ptr)}");
                }
            }

            return output;
        }

        private static string PrintTokenList(List<StyleToken> tokens) {
            string retn = "\n";
            for (int i = 0; i < tokens.Count; i++) {
                retn += tokens[i].ToString();
                if (i != tokens.Count - 1) {
                    retn += ", \n";
                }
            }

            return retn;
        }

    }

}
