using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

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
namespace UIForia {

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

        [DebuggerStepThrough]
        private int FindNextIndex(TokenType targetTokenType) {
            int i = 0;
            int counter = 0;
            while (tokenStream.HasTokenAt(i)) {
                TokenType token = tokenStream.Peek(i);
                if (token == TokenType.ParenOpen) {
                    counter++;
                }
                else if (token == TokenType.ParenClose) {
                    counter--;
                }
                else if (token == targetTokenType && counter == 0) {
                    return i;
                }
                i++;
            }
            return -1;
        }

        [DebuggerStepThrough]
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
            else {
                isLiteralExpression = false;
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
                    
                    while (operatorStack.Count != 0 && op.precedence <= operatorStack.Peek().Precedence) {
                        expressionStack.Push(new OperatorExpressionNode(
                                expressionStack.Pop(),
                                expressionStack.Pop(),
                                operatorStack.Pop()
                            )
                        );
                    }
                    
                    operatorStack.Push(op);
                    continue;
                }

                Abort();
            }

            while (operatorStack.Count != 0) {
                expressionStack.Push(new OperatorExpressionNode(
                        expressionStack.Pop(),
                        expressionStack.Pop(),
                        operatorStack.Pop()
                    )
                );
            }
            
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

            ExpressionParser subParser = CreateSubParser(advance);

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

            ExpressionParser subParser = CreateSubParser(advance);

            if (isLiteralExpression) {
                retn = new ParenExpressionNode(subParser.ParseLoop(subParser.ParseLiteralExpressionOperand));
            }
            else {
                retn = new ParenExpressionNode(subParser.ParseLoop(subParser.ParseDynamicExpressionOperand));
            }
            return true;
        }

        private ExpressionParser CreateSubParser(int advance) {
            tokenStream.Advance(); // step over the open brace
            // -1 to drop the closing paren token from sub stream
            TokenStream stream = tokenStream.AdvanceAndReturnSubStream(advance - 1);
            // step over closing paren
            tokenStream.Advance();
            ExpressionParser subParser = new ExpressionParser(stream);
            subParser.isLiteralExpression = isLiteralExpression;

            return subParser;
        }

        private ExpressionNode ParseLiteralExpressionOperand() {
            ExpressionNode retn = null;

            if (ParseParenExpression(ref retn)) return retn;
            if (ParseLiteralValue(ref retn)) return retn;

            return null;
        }

        private ExpressionNode ParseDynamicExpressionOperand() {
            ExpressionNode retn = null;
            return ParseExpression(ref retn) ? retn : null;
        }

        private bool ParseExpression(ref ExpressionNode retn) {

            if (ParseMethodExpression(ref retn)) return true;
            if (ParseAccessExpression(ref retn)) return true;
            if (ParseParenExpression(ref retn)) return true;
            if (ParseLookupExpression(ref retn)) return true;
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

                case TokenType.And:
                    return new OperatorNode(-1, OperatorType.And);

                case TokenType.Or:
                    return new OperatorNode(-1, OperatorType.Or);

                case TokenType.Equals:
                    return new OperatorNode(-1, OperatorType.Equals);

                case TokenType.NotEquals:
                    return new OperatorNode(-1, OperatorType.NotEquals);

                case TokenType.GreaterThan:
                    return new OperatorNode(-1, OperatorType.GreaterThan);

                case TokenType.GreaterThanEqualTo:
                    return new OperatorNode(-1, OperatorType.GreaterThanEqualTo);

                case TokenType.LessThan:
                    return new OperatorNode(-1, OperatorType.LessThan);

                case TokenType.LessThanEqualTo:
                    return new OperatorNode(-1, OperatorType.LessThanEqualTo);

                case TokenType.QuestionMark:
                    return new OperatorNode(-2, OperatorType.TernaryCondition);

                case TokenType.Colon:
                    return new OperatorNode(-1, OperatorType.TernarySelection);

                default:
                    throw new Exception("Unknown op type");
            }
        }

        private bool ParseMethodExpression(ref ExpressionNode retn) {
            // Identifier ParenOpen Expression ParenClose

            tokenStream.Save();

            IdentifierNode idNode = null;
            if (!ParseIdentifier(ref idNode)) {
                tokenStream.Restore();
                return false;
            }

            if (!tokenStream.HasMoreTokens) {
                tokenStream.Restore();
                return false;
            }

            if (tokenStream.Current != TokenType.ParenOpen) {
                tokenStream.Restore();
                return false;
            }

            if (tokenStream.Next == TokenType.ParenClose) {
                tokenStream.Advance(2);
                retn = new MethodCallNode(idNode, new MethodSignatureNode());
                return true;
            }

            int advance = FindMatchingBraceIndex(TokenType.ParenOpen, TokenType.ParenClose);
            if (advance == -1) {
                throw new Exception("Unmatched paren");
            }

            ExpressionParser subParser = CreateSubParser(advance);
            MethodSignatureNode signature = subParser.ParseMethodSignature();

            retn = new MethodCallNode(idNode, signature);
            return true;
        }

        private MethodSignatureNode ParseMethodSignature() {
            // ( Expression | ExpressionList )

            List<ExpressionNode> parts = new List<ExpressionNode>(2);

            while (tokenStream.HasMoreTokens) {
                int nextComma = FindNextIndex(TokenType.Comma);

                if (nextComma == -1) {
                    parts.Add(ParseLoop(ParseDynamicExpressionOperand));
                }
                else {
                    TokenStream subStream = tokenStream.AdvanceAndReturnSubStream(nextComma);
                    ExpressionParser subParser = new ExpressionParser(subStream);
                    parts.Add(subParser.ParseLoop(subParser.ParseDynamicExpressionOperand));
                }
                tokenStream.Advance();
            }

            return new MethodSignatureNode(parts);
        }

        // todo make sure --, ++, and !! are not allowed
        // maybe pre-process the token stream for these
        // also disallow =, +=, -=, *=, /=, %=
        private bool ParseUnaryExpression(ref ExpressionNode retn) {
            tokenStream.Save();

            // handles folding Literals like !true or -someValue
            if (ParseUnaryLiteralValue(ref retn)) {
                return true;
            }

            if ((tokenStream.Current & TokenType.UnaryOperator) != 0) {
                OperatorType opType;
                
                
                if (tokenStream.Current != TokenType.Not && tokenStream.HasPrevious && (tokenStream.Previous & TokenType.UnaryRequiresCheck) == 0) {
                    tokenStream.Restore();
                    return false;
                }

                if (tokenStream.Current == TokenType.Not) {
                    opType = OperatorType.Not;
                }
                else if (tokenStream.Current == TokenType.Plus) {
                    opType = OperatorType.Plus;
                }
                else if (tokenStream.Current == TokenType.Minus) {
                    opType = OperatorType.Minus;
                }
                else {
                    throw new Exception("Unknown unary operator: " + tokenStream.Current.value);
                }

                tokenStream.Advance();

                ExpressionNode expressionNode = null;

                if (ParseExpression(ref expressionNode)) {
                    retn = new UnaryExpressionNode(expressionNode, opType);
                    return true;
                }

            }

            tokenStream.Restore();
            return false;
        }
        
        private bool ParseLookupExpression(ref ExpressionNode retn) {

            IdentifierNode idNode = null;

            if (!ParseIdentifier(ref idNode)) {
                return false;
            }

            if (idNode is SpecialIdentifierNode) {
                retn = new AliasExpressionNode(idNode);
            }
            else {
                retn = new RootContextLookupNode(idNode);
            }
            
            return true;
        }

       
        
        [DebuggerStepThrough]
        private bool ParseIdentifier(ref IdentifierNode node) {
            tokenStream.Save();

            if (tokenStream.Current != TokenType.Identifier 
                && tokenStream.Current != TokenType.At
                && tokenStream.Current != TokenType.SpecialIdentifier) {
                tokenStream.Restore();
                return false;
            }

            if (tokenStream.Current == TokenType.SpecialIdentifier) {
                node = new SpecialIdentifierNode(tokenStream.Current);
            }
            else if(tokenStream.Current == TokenType.At) {
                tokenStream.Advance();
                node = new ExternalReferenceIdentifierNode("@" + tokenStream.Current);
            }
            else {
                node = new IdentifierNode(tokenStream.Current);
            }

            tokenStream.Advance();
            return true;
        }

        private bool ParseAccessExpressionPart(ref AccessExpressionPartNode retn) {
            tokenStream.Save();

            if (ParseDotAccessExpression(ref retn)) return true;
            if (ParseMethodAccessExpression(ref retn)) return true;
            if (ParseArrayBracketExpression(ref retn)) return true;

            tokenStream.Restore();
            return false;
        }

        private bool ParseMethodAccessExpression(ref AccessExpressionPartNode retn) {
            tokenStream.Save();
            
            if (tokenStream.Current == TokenType.ParenOpen) {
            
                if (tokenStream.Next == TokenType.ParenClose) {
                    tokenStream.Advance(2);
                    retn = new MethodAccessExpressionPartNode(new MethodSignatureNode());
                    return true;
                }
                

                int advance = FindMatchingBraceIndex(TokenType.ParenOpen, TokenType.ParenClose);
                if (advance == -1) {
                    throw new Exception("Unmatched paren");
                }

                ExpressionParser subParser = CreateSubParser(advance);
                MethodSignatureNode signature = subParser.ParseMethodSignature();

                retn = new MethodAccessExpressionPartNode(signature);
                return true;
               
            }
            tokenStream.Restore();
            return false;
        }

        private bool ParseDotAccessExpression(ref AccessExpressionPartNode retn) {
            tokenStream.Save();
            if (tokenStream.Current == TokenType.Dot) {
                tokenStream.Advance();

                IdentifierNode idNode = null;

                if (!ParseIdentifier(ref idNode)) {
                    tokenStream.Restore();
                    return false;
                }

                retn = new PropertyAccessExpressionPartNode(idNode.identifier);

                return true;
            }
            tokenStream.Restore();
            return false;
        }

        // identifier ((. identifier) | [ expression ] )*)
        private bool ParseAccessExpression(ref ExpressionNode retn) {
            tokenStream.Save();
            IdentifierNode idNode = null;

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

            retn = new AccessExpressionNode(idNode, parts);

            return true;
        }

        private bool ParseUnaryParenExpression(ref ExpressionNode retn, OperatorType operatorType) {
            tokenStream.Save();
            tokenStream.Advance();
            ExpressionNode parenExpressionNode = null;
            if (ParseParenExpression(ref parenExpressionNode)) {
                retn = new UnaryExpressionNode(parenExpressionNode, operatorType);
                return true;
            }
            tokenStream.Restore();
            return false;
        }

        private bool ParseUnaryLiteralValue(ref ExpressionNode retn) {
            
            if (!tokenStream.HasMoreTokens) return false;
            
            if (tokenStream.Current == TokenType.Not) {
                
                if (tokenStream.Next == TokenType.Boolean) {
                    bool value = bool.Parse(tokenStream.Next.value);
                    tokenStream.Advance(2);
                    retn = new BooleanLiteralNode(!value);
                    return true;
                }

                if (ParseUnaryParenExpression(ref retn, OperatorType.Not)) {
                    return true;
                }

            }

            if (!tokenStream.HasPrevious ||
                (tokenStream.HasPrevious && (tokenStream.Previous.tokenType & TokenType.ArithmeticOperator) != 0)) {

                if (tokenStream.Current == TokenType.Minus || tokenStream.Current == TokenType.Plus) {

                    OperatorType operatorType = tokenStream.Current == TokenType.Minus
                        ? OperatorType.Minus
                        : OperatorType.Plus;
                    
                    if (tokenStream.Next == TokenType.Number) {
                        bool isNegative = tokenStream.Current == TokenType.Minus;

                        tokenStream.Advance(2);
                        retn = CreateNumericLiteralNode(tokenStream.Previous.value, isNegative);
                        return true;
                    }

                    if (ParseUnaryParenExpression(ref retn, operatorType)) {
                        return true;
                    }

                }

            }

            return false;
        }

        private static NumericLiteralNode CreateNumericLiteralNode(string input, bool isNegative) {
            if (input.IndexOf('.') == -1 && input.IndexOf('f') == -1) {
                int value = int.Parse(input, CultureInfo.InvariantCulture);
                if (isNegative) value = -value;
                return new IntLiteralNode(value);
            }
            else if (input.IndexOf('f') != -1) {
                float value = float.Parse(input.Substring(0, input.Length - 1), CultureInfo.InvariantCulture);
                if (isNegative) value = -value;
                return new FloatLiteralNode(value);
            }
            else {
                double value = double.Parse(input, CultureInfo.InvariantCulture);
                if (isNegative) value = -value;
                return new DoubleLiteralNode(value);
            }
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
                retn = CreateNumericLiteralNode(tokenStream.Previous, false);
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

    }

}