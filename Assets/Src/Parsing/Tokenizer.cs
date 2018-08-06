using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Src {
    [Flags]
    public enum TokenType {
        ExpressionOpen = 1 << 0,
        ExpressionClose = 1 << 1,

        // operators
        Plus = 1 << 2,
        Minus = 1 << 3,
        Times = 1 << 4,
        Divide = 1 << 5,
        Mod = 1 << 6,

        // accessors
        PropertyAccess = 1 << 7,
        ArrayAccessOpen = 1 << 8,
        ArrayAccessClose = 1 << 9,
        FilterDelimiter = 1 << 10,

        // identifiers
        Identifier = 1 << 11,
        SpecialIdentifier = 1 << 12,

        // constants
        String = 1 << 13,
        Boolean = 1 << 14,
        Number = 1 << 15,

        // booleans
        And = 1 << 16,
        Or = 1 << 17,
        Not = 1 << 18,

        // Comparators
        Equals = 1 << 19,
        NotEquals = 1 << 20,
        GreaterThan = 1 << 21,
        LessThan = 1 << 22,
        GreaterThanEqualTo = 1 << 23,
        LessThanEqualTo = 1 << 24,

        WhiteSpace = 1 << 25,

        Operator = Plus | Minus | Times | Divide | Mod,
        Constant = String | Number | Boolean,
        Comparator = Equals | NotEquals | GreaterThan | GreaterThanEqualTo | LessThan | LessThanEqualTo,
        BooleanTest = Not | Or | And,
        AnyIdentifier = Identifier | SpecialIdentifier
    }

    public struct TokenMatch {
        public bool isMatch;
        public TokenType tokenType;
        public string value;
        public string remainingText;
    }

    public struct DslToken {
        public readonly TokenType tokenType;
        public readonly string value;

        public DslToken(TokenType tokenType) {
            this.tokenType = tokenType;
            value = string.Empty;
        }

        public DslToken(TokenType tokenType, string value) {
            this.tokenType = tokenType;
            this.value = value;
        }

        public DslToken Clone() {
            return new DslToken(tokenType, value);
        }
    }

    public static class Tokenizer {
        private static readonly TokenDefinition[] tokens = {
            new TokenDefinition(TokenType.And, "^&&"),
            new TokenDefinition(TokenType.Or, "^||"),
            new TokenDefinition(TokenType.Not, "^!"),
            new TokenDefinition(TokenType.Plus, "^\\+"),
            new TokenDefinition(TokenType.Minus, "^-"),
            new TokenDefinition(TokenType.Times, "^\\*"),
            new TokenDefinition(TokenType.Divide, "^/\\"),
            new TokenDefinition(TokenType.Mod, "^%"),

            new TokenDefinition(TokenType.SpecialIdentifier, "^\\$[_a-zA-Z][_a-zA-Z0-9]{0,30}"),
            new TokenDefinition(TokenType.Identifier, "^[_a-zA-Z][_a-zA-Z0-9]{0,30}"),

            new TokenDefinition(TokenType.PropertyAccess, "^\\."),
            new TokenDefinition(TokenType.ExpressionOpen, "^{"),
            new TokenDefinition(TokenType.ExpressionClose, "^}"),

            new TokenDefinition(TokenType.ArrayAccessOpen, "^["),
            new TokenDefinition(TokenType.ArrayAccessClose, "^}"),

            new TokenDefinition(TokenType.Equals, "^=="),
            new TokenDefinition(TokenType.NotEquals, "^!="),
            new TokenDefinition(TokenType.GreaterThan, "^>"),
            new TokenDefinition(TokenType.LessThan, "^<"),
            new TokenDefinition(TokenType.GreaterThanEqualTo, "^>="),
            new TokenDefinition(TokenType.LessThanEqualTo, "^<="),
            new TokenDefinition(TokenType.WhiteSpace, "^\\s+"),
            new TokenDefinition(TokenType.Number, "^\\d+"),
            new TokenDefinition(TokenType.FilterDelimiter, "^>>"),
            new TokenDefinition(TokenType.Boolean, "^true|^false"),
            new TokenDefinition(TokenType.String, "^'[^']*'")
        };

        // \[(.*?)\] match between brackets

        /*
         * Grammar
         *     ConstantStatement = Constant
         *     ExpressionStatement = { Expression }
         *
         *     Constant = String | Boolean | Number
         *     ValueExpression = Lookup | PropertyAccess | ArrayAccess | Constant
         *     Lookup = Identifier
         *     PropertyAccess = Identifier . (Identifier+)
         *     ArrayAccess = Identifier [ Expression ] 
         *     Operator = ValueExpression Operator ValueExpression
         *     Expression = ValueExpression ?(Operator Expression)*
         *     MethodExpression = Identifier (ParameterList)
         *     ParameterList = Expression (, Expression)*
         */

        private class ConstantStatement {
            public string type;
            public string value;
        }

        private class ExpressionStatement {
            public List<string> expressionParts;
        }

        private class Lookup {
            public string lookupName;
        }

        private class PropertyAccess {
            public string[] parts;
        }

        private class ArrayAccess {
            public string[] parts;
            public PropertyAccess propertyAccess;
            public ExpressionStatement expression;
        }

        private static TokenMatch FindMatch(string input) {
            for (int i = 0; i < tokens.Length; i++) {
                var match = tokens[i].Match(input);
                if (match.isMatch) {
                    return match;
                }
            }

            return new TokenMatch() {isMatch = false};
        }

        public static List<DslToken> Tokenize(string input) {
            List<DslToken> output = new List<DslToken>();
            string remaining = input;
            while (!string.IsNullOrWhiteSpace(remaining)) {
                var match = FindMatch(remaining);
                if (match.isMatch) {
                    output.Add(new DslToken(match.tokenType, match.value));
                    remaining = match.remainingText;
                }
                else {
                    if (Regex.IsMatch(remaining, "^\\s+")) {
                        throw new Exception("Failed to tokenize: " + input);
                    }

                    remaining = remaining.Substring(1); // super bad but probably fine for now
                }
            }

            return output;
        }
    }

    public class TokenDefinition {
        private readonly Regex regex;
        private readonly TokenType tokenType;

        public TokenDefinition(TokenType tokenType, string pattern) {
            this.tokenType = tokenType;
            regex = new Regex(pattern);
        }

        public TokenMatch Match(string inputString) {
            Match match = regex.Match(inputString);
            if (!match.Success) return new TokenMatch {isMatch = false};

            string remainingText = string.Empty;
            if (match.Length != inputString.Length)
                remainingText = inputString.Substring(match.Length);

            return new TokenMatch() {
                isMatch = true,
                remainingText = remainingText,
                tokenType = tokenType,
                value = match.Value
            };
        }
    }

    public class LexedExpressionPart { }

    public class Lexer {
        public void Lex(List<DslToken> tokens) {
            TokenStream tokenStream = new TokenStream(tokens);
            List<LexedExpressionPart> expressionParts = new List<LexedExpressionPart>();

            while (tokenStream.HasMoreTokens) {
                if (Rule_ConstantValue(tokenStream, expressionParts)) {
                    continue;
                }
            }
        }

        public static bool Rule_ConstantValue(TokenStream stream, List<LexedExpressionPart> bindingParts) {
            bool isConstant = stream.Current.tokenType == TokenType.Number
                              || stream.Current.tokenType == TokenType.String
                              || stream.Current.tokenType == TokenType.Boolean;

            if (!isConstant) return false;

            stream.Consume(1);
            bindingParts.Add(new LexedExpressionPart());
            return true;
        }

        public static bool Rule_Operator(TokenStream stream, List<ExpressionBindingPart> bindingParts) {
            bool isOperator = (stream.Current.tokenType & TokenType.Operator) != 0;
            if (!isOperator) return false;
        }

        public static void Rule_PropertyAccess(TokenStream stream) {
            bool isPropertyAccess = stream.Current.tokenType == TokenType.Identifier &&
                                    stream.Peek(1).tokenType == TokenType.PropertyAccess &&
                                    stream.Peek(2).tokenType == TokenType.Identifier;
        }

        public static void Rule_Identifier(TokenStream stream) {
            bool isIdentifier = stream.Current.tokenType == TokenType.Identifier ||
                                stream.Current.tokenType == TokenType.SpecialIdentifier;
        }
    }

    public class TokenStream {
        private int ptr;
        private int savedPtr;

        private readonly Stack<int> stack;
        private readonly List<DslToken> tokens;

        public TokenStream(List<DslToken> tokens) {
            this.tokens = tokens;
            savedPtr = 0;
            stack = new Stack<int>();
            stack.Push(0);
        }

        public DslToken Current => tokens[ptr];
        public DslToken Next => tokens[ptr + 1];
        public DslToken Previous => tokens[ptr - 1];

        public bool HasMoreTokens => (ptr + 1) < tokens.Count;

        public bool Is(TokenType tokenType) {
            return Current.tokenType == tokenType;
        }

        public DslToken Peek(int count) {
            return tokens[ptr + count];
        }

        public void Consume(int count) {
            ptr += count;
        }

        public void Save() {
            stack.Push(ptr);
        }

        public void Restore() {
            ptr = stack.Pop();
        }
    }


    public class ExpressionParser {
        private readonly TokenStream tokenStream;

        public ExpressionParser(TokenStream tokenStream) {
            this.tokenStream = tokenStream;
        }

        public ASTNode Parse(List<TokenDefinition> tokens) {
            ConstantExpressionNode node = new ConstantExpressionNode();
            ExpressionNode expressionNode = new ExpressionNode();
            if (TryParseConstantExpression(ref node)) {
                return node;
            }
            else if (TryParseExpression(ref expressionNode)) { }

            return null;
        }

        private bool TryParseExpression(ref ExpressionNode node) {
            if (TryParseValueExpression()) { }
        }

        private bool TryParseValueExpression() {
            return true;
        }

        private bool TryParseLookupExpression(ref ExpressionNode node) {
            
            if (TryParseIdentifier(ref node)) {
                
            }
            return true;
        }

        private bool TryParsePropertyAccessExpression() {
            return true;
        }

        private bool TryParseArrayAccessExpression() {
            return true;
        }

        private bool TryParseConstantExpression(ref ConstantExpressionNode node) {
            tokenStream.Save();
            if ((tokenStream.Current.tokenType & TokenType.Constant) == 0) {
                return false;
            }

            node.value = tokenStream.Current.value;
            tokenStream.Consume(1);
            return true;
        }

        private bool TryParseExpressionStatement() {
            tokenStream.Save();

            // ExpressionStatementNode node = new ExpressionStatementNode;

            if (tokenStream.Current.tokenType != TokenType.ExpressionOpen) {
                return false;
            }

            tokenStream.Consume(1);

            ConstantExpressionNode node = new ConstantExpressionNode();
            if (!TryParseConstantExpression(ref node)) {
                tokenStream.Restore();
                return false;
            }

            if (tokenStream.Current.tokenType != TokenType.ExpressionClose) {
                tokenStream.Consume(1);
                return false;
            }

            return true;
        }

        private bool TryParseIdentifier(ref IdentifierNode identifierNode) {
            tokenStream.Save();
            if (tokenStream.Current.tokenType == TokenType.AnyIdentifier &&
                tokenStream.Next.tokenType == TokenType.WhiteSpace) {
                identifierNode = new IdentifierNode(token.current.value);
                return true;
            }
            return true;
        }

        private bool TryParseConstant() {
            tokenStream.Save();
            if ((tokenStream.Current.tokenType & TokenType.Constant) == 0) {
                return false;
            }

            tokenStream.Consume(1);
            //expressionParts.Add(new ConstantExpression());
            return true;
        }

        private bool TryParseDot() {
            if (tokenStream.Current.tokenType == TokenType.PropertyAccess) {
                tokenStream.Consume(1);
                return true;
            }

            return false;
        }

        private bool TryParsePropertyAccess() {
            if (TryParseIdentifier() && TryParseDot()) {
                //tree.Add(new PropertyAccessNode() { identifier = ParseIdentifier, property = ParseIdentifier2)
            }
        }

        private bool TryParseUnaryExpression() {
            return true;
        }

        private bool TryParseOperator() {
            bool isOperator = (tokenStream.Current.tokenType & TokenType.Operator) != 0;
        }

        private bool TryParsePropertyAccessChain() {
            if (TryParseDot() && TryParseIdentifier()) {
                if (TryParsePropertyAccessChain()) { }

                return true;
            }

            return false;
        }
    }
}