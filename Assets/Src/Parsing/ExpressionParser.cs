using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting.Messaging;

/*
* Grammar
*     LiteralStatement = Literal
*     ExpressionStatement = { Expression }
*
*     Literal = String | Boolean | Number
*     MethodExpression = Identifier ( ParameterList )
*     ValueExpression = Lookup | PropertyAccess | ArrayAccess | Literal
*     Expression = ValueExpression | UnaryExpression | OperatorExpression | ParenExpression
*     Lookup = Identifier
*     PropertyAccess = Identifier . (Identifier+)
*     ArrayAccess = Identifier [ Expression ] 
*     OperatorExpression = ValueExpression Operator ValueExpression
*     UnaryOperatorExpression = (!|-|+) Expression
*     ParenExpression = ( Expression )
*     ParameterList = Expression (, Expression)*
*/
namespace Src {

    public class ExpressionParser {

        private TokenStream tokenStream;
        private readonly Stack<ExpressionNode> expressionStack;
        private readonly Stack<IOperatorNode> operatorStack;

        private static readonly TokenStream EmptyTokenStream = new TokenStream(new List<DslToken>());

        private bool isLiteralExpression;

        public ExpressionParser() {
            tokenStream = EmptyTokenStream;
            expressionStack = new Stack<ExpressionNode>();
            operatorStack = new Stack<IOperatorNode>();
        }

        public ExpressionParser(TokenStream tokenStream) {
            this.tokenStream = tokenStream;
            expressionStack = new Stack<ExpressionNode>();
            operatorStack = new Stack<IOperatorNode>();
        }

        public ExpressionParser(string input) {
            tokenStream = new TokenStream(Tokenizer.Tokenize(input));
            expressionStack = new Stack<ExpressionNode>();
            operatorStack = new Stack<IOperatorNode>();
        }

        private int FindMatchingBraceIndex(TokenType braceOpen, TokenType braceClose) {
            if (tokenStream.Current != braceOpen) {
                return -1;
            }

            tokenStream.Save();

            int i = -1;
            int counter = 0;
            while (tokenStream.HasMoreTokens) {
                i++;

                if (tokenStream.Current == braceOpen) {
                    counter++;
                }

                if (tokenStream.Current == braceClose) {
                    counter--;
                    if (counter == 0) {
                        tokenStream.Restore();
                        return i;
                    }
                }

                tokenStream.Advance();
            }

            tokenStream.Restore();
            return -1;
        }

        public ExpressionNode Parse() {

            expressionStack.Clear();
            operatorStack.Clear();

            if (tokenStream.Current != TokenType.ExpressionOpen) {
                isLiteralExpression = true;
                return ParseLoop(ParseLiteralExpressionOperand);
            }

            tokenStream.Advance();

            if (tokenStream.Last != TokenType.ExpressionClose) {
                throw new Exception("Expected dynamic expression to be wrapped in braces { }");
            }

            tokenStream.Chop();

            return ParseLoop(ParseDynamicExpressionOperand);
        }

        public ExpressionNode Parse(string input) {
            tokenStream = new TokenStream(Tokenizer.Tokenize(input));
            return Parse();
        }

        private ExpressionNode ParseLoop(Func<ExpressionNode> parseFn) {
            while (tokenStream.HasMoreTokens) {
                ExpressionNode operand = parseFn();

                if (operand != null) {
                    expressionStack.Push(operand);
                    continue;
                }

                OperatorNode op = ParseOperatorExpression();
                if (op != null) {
                    EvaluateWhile(() => {
                        if (operatorStack.Count == 0) return false;
                        return op.precedence    <= operatorStack.Peek().Precedence;
                    });
                    operatorStack.Push(op);
                    continue;
                }

                Abort();
            }

            EvaluateWhile(() => operatorStack.Count > 0);

            if (expressionStack.Count != 1) {
                Abort();
            }

            return expressionStack.Pop();
        }

        private void Abort() {
            string additionalInfo = isLiteralExpression
                ? "This might be because you are referencing non literal values in a string not wrapped in braces."
                : string.Empty;
            throw new Exception($"Failed to parse {tokenStream}. {additionalInfo}");
        }

        private bool ParseArrayBracketExpression(ref AccessExpressionPartNode retn) {
            if (tokenStream.Current != TokenType.ArrayAccessOpen) {
                return false;
            }

            int advance = FindMatchingBraceIndex(TokenType.ArrayAccessOpen, TokenType.ArrayAccessClose);
            if (advance == -1) throw new Exception("Unmatched array bracket");
            tokenStream.Advance(); // step over the open brace

            // -1 to drop the closing array bracket token
            TokenStream stream = tokenStream.AdvanceAndReturnSubStream(advance - 1);
            // step over closing array bracket
            tokenStream.Advance();

            ExpressionParser subParser = new ExpressionParser(stream);
            subParser.isLiteralExpression = isLiteralExpression;

            if (isLiteralExpression) {
                retn = new ArrayAccessExpressionNode(subParser.ParseLoop(subParser.ParseLiteralExpressionOperand));
            }
            else {
                retn = new ArrayAccessExpressionNode(subParser.ParseLoop(subParser.ParseDynamicExpressionOperand));
            }
            return true;
        }

        private bool ParseParenExpression(ref ExpressionNode retn) {
            if (tokenStream.Current != TokenType.ParenOpen) {
                return false;
            }

            int advance = FindMatchingBraceIndex(TokenType.ParenOpen, TokenType.ParenClose);
            if (advance == -1) throw new Exception("Unmatched paren");

            tokenStream.Advance(); // step over the open brace

            // -1 to drop the closing paren token
            TokenStream stream = tokenStream.AdvanceAndReturnSubStream(advance - 1);

            // step over closing paren
            tokenStream.Advance();

            ExpressionParser subParser = new ExpressionParser(stream);
            subParser.isLiteralExpression = isLiteralExpression;

            if (isLiteralExpression) {
                retn = new ParenOperatorNode(subParser.ParseLoop(subParser.ParseLiteralExpressionOperand));
            }
            else {
                retn = new ParenOperatorNode(subParser.ParseLoop(subParser.ParseDynamicExpressionOperand));
            }
            return true;
        }

        private ExpressionNode ParseLiteralExpressionOperand() {
            ExpressionNode retn = null;
            if (ParseParenExpression(ref retn)) return retn;

            if (ParseLiteralValue(ref retn)) return retn;

            return null;
        }

        private ExpressionNode ParseDynamicExpressionOperand() {
            ExpressionNode retn = null;

            if (ParseParenExpression(ref retn)) return retn;
            if (ParseAccessExpression(ref retn)) return retn;
            if (ParseLiteralValue(ref retn)) return retn;
            if (ParseLookupValue(ref retn)) return retn;

            return null;
        }

        private bool ParseExpression(ref ExpressionNode retn) {

            if (ParseParenExpression(ref retn)) return true;
            if (ParseLookupValue(ref retn)) return true;
            if (ParseLiteralValue(ref retn)) return true;
            if (ParseUnaryExpression(ref retn)) return true;

            return false;
        }

        private OperatorNode ParseOperatorExpression() {
            tokenStream.Save();

            if ((tokenStream.Current & TokenType.Operator) == 0) {
                tokenStream.Restore();
                return null;
            }

            tokenStream.Advance();

            switch (tokenStream.Previous.tokenType) {
                case TokenType.Plus:
                    return new OperatorNode(1, OperatorType.Plus);

                case TokenType.Minus:
                    return new OperatorNode(1, OperatorType.Minus);

                case TokenType.Times:
                    return new OperatorNode(2, OperatorType.Times);

                case TokenType.Divide:
                    return new OperatorNode(2, OperatorType.Divide);

                case TokenType.Mod:
                    return new OperatorNode(1, OperatorType.Mod);

                default:
                    throw new Exception("Unknown op type");
            }
        }

        // todo make sure --, ++, and !! are not allowed
        // maybe pre-process the token stream for these
        // also disallow =, +=, -=, *=, /=, %=
        private bool ParseUnaryExpression(ref ExpressionNode retn) {
            tokenStream.Save();

            UnaryOperatorType opType = UnaryOperatorType.Not;

            // handles folding Literals like !true or -someValue
            if (ParseUnaryLiteralValue(ref retn)) {
                return true;
            }

            if ((tokenStream.Current & TokenType.UnaryOperator) != 0) {
                if (tokenStream.Current == TokenType.Not) {
                    opType = UnaryOperatorType.Not;
                }
                else if (tokenStream.Current == TokenType.Plus) {
                    opType = UnaryOperatorType.Plus;
                }
                else if (tokenStream.Current == TokenType.Minus) {
                    opType = UnaryOperatorType.Minus;
                }
                else {
                    throw new Exception("Unknown unary operator: " + tokenStream.Current.value);
                }

                tokenStream.Advance();
            }

            ExpressionNode expressionNode = null;

            if (ParseExpression(ref expressionNode)) {
                retn = new UnaryExpressionNode(expressionNode, opType);
                return true;
            }

            tokenStream.Restore();
            return false;
        }

        private bool ParseLookupValue(ref ExpressionNode retn) {
            tokenStream.Save();

            ASTNode idNode = null;

            if (!ParseIdentifier(ref idNode)) return false;

            retn = new RootContextLookupNode(((IdentifierNode) idNode));
            return true;
        }

        private bool ParseIdentifier(ref ASTNode node) {
            tokenStream.Save();

            if (tokenStream.Current != TokenType.Identifier) return false;

            node = new IdentifierNode(tokenStream.Current);
            tokenStream.Advance();
            return true;
        }

        private bool ParseAccessExpressionPart(ref AccessExpressionPartNode retn) {
            tokenStream.Save();

            if (ParseDotAccessExpression(ref retn)) return true;
            if (ParseArrayBracketExpression(ref retn)) return true;

            tokenStream.Restore();
            return false;
        }

        private bool ParseDotAccessExpression(ref AccessExpressionPartNode retn) {
            tokenStream.Save();
            if (tokenStream.Current == TokenType.PropertyAccess) {
                tokenStream.Advance();

                ASTNode idNode = null;

                if (!ParseIdentifier(ref idNode)) {
                    tokenStream.Restore();
                    return false;
                }

                retn = new PropertyAccessExpressionPartNode(((IdentifierNode) idNode).identifier);

                return true;
            }
            tokenStream.Restore();
            return false;
        }

        // identifier ((. identifier) | [ expression ] )*)
        private bool ParseAccessExpression(ref ExpressionNode retn) {
            tokenStream.Save();
            ASTNode idNode = null;

            if (!ParseIdentifier(ref idNode)) {
                tokenStream.Restore();
                return false;
            }

            if (!tokenStream.HasMoreTokens) {
                tokenStream.Restore();
                return false;
            }
            
            AccessExpressionPartNode head = null;
            if (!ParseAccessExpressionPart(ref head)) {
                tokenStream.Restore();
                return false;
            }

            List<AccessExpressionPartNode> parts = new List<AccessExpressionPartNode>();

            parts.Add(head);
            
            while (tokenStream.HasMoreTokens) {

                AccessExpressionPartNode partNode = null;
                if (ParseAccessExpressionPart(ref partNode)) {
                    parts.Add(partNode);
                }
                else {
                    break;
                }
            }

            retn = new AccessExpressionNode((IdentifierNode) idNode, parts);

            return true;
        }

        private bool ParseUnaryLiteralValue(ref ExpressionNode retn) {
            if (tokenStream.Current == TokenType.Not) {
                if (tokenStream.Next == TokenType.Boolean) {
                    bool value = bool.Parse(tokenStream.Next.value);
                    tokenStream.Advance(2);
                    retn = new BooleanLiteralNode(!value);
                    return true;
                }
            }

            if (!tokenStream.HasPrevious ||
                (tokenStream.HasPrevious && (tokenStream.Previous.tokenType & TokenType.Operator) != 0)) {

                if (tokenStream.Current == TokenType.Minus) {
                    if (tokenStream.Next == TokenType.Number) {
                        tokenStream.Advance(2);
                        retn = new NumericLiteralNode(-double.Parse(tokenStream.Previous, CultureInfo.InvariantCulture));
                        return true;
                    }
                }

                if (tokenStream.Current == TokenType.Plus) {
                    if (tokenStream.Next == TokenType.Number) {
                        tokenStream.Advance(2);
                        retn = new NumericLiteralNode(double.Parse(tokenStream.Previous, CultureInfo.InvariantCulture));
                        return true;
                    }
                }

            }

            return false;
        }

        private bool ParseLiteralValue(ref ExpressionNode retn) {
            tokenStream.Save();

            if (ParseUnaryLiteralValue(ref retn)) return true;

            if (tokenStream.Current == TokenType.Boolean) {
                tokenStream.Advance();
                retn = new BooleanLiteralNode(bool.Parse(tokenStream.Previous));
                return true;
            }

            if (tokenStream.Current == TokenType.Number) {
                tokenStream.Advance();
                retn = new NumericLiteralNode(double.Parse(tokenStream.Previous, CultureInfo.InvariantCulture));
                return true;
            }

            if (tokenStream.Current == TokenType.String) {
                string value = tokenStream.Current.value.Substring(1, tokenStream.Current.value.Length - 2);
                tokenStream.Advance();
                retn = new StringLiteralNode(value);
                return true;
            }

            tokenStream.Restore();
            return false;
        }

        private void EvaluateWhile(Func<bool> condition) {
            while (condition()) {
                expressionStack.Push(new OperatorExpressionNode(
                        expressionStack.Pop(),
                        expressionStack.Pop(),
                        operatorStack.Pop()
                    )
                );
            }
        }

    }

}