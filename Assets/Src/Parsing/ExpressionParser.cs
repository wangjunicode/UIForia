using System;
using System.Collections.Generic;

/*
* Grammar
*     ConstantStatement = Constant
*     ExpressionStatement = { Expression }
*
*     Constant = String | Boolean | Number
*     MethodExpression = Identifier ( ParameterList )
*     ValueExpression = Lookup | PropertyAccess | ArrayAccess | Constant
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
        private readonly Stack<OperatorNode> operatorStack;

        private static readonly TokenStream EmptyTokenStream = new TokenStream(new List<DslToken>());
        
        public ExpressionParser() {
            tokenStream = EmptyTokenStream;
            expressionStack = new Stack<ExpressionNode>();
            operatorStack = new Stack<OperatorNode>();
        }
        
        public ExpressionParser(TokenStream tokenStream) {
            this.tokenStream = tokenStream;
            expressionStack = new Stack<ExpressionNode>();
            operatorStack = new Stack<OperatorNode>();
        }

        public ExpressionParser(string input) {
            tokenStream = new TokenStream(Tokenizer.Tokenize(input));
            expressionStack = new Stack<ExpressionNode>();
            operatorStack = new Stack<OperatorNode>();
        }

        private void EvaluateWhile(Func<bool> condition) {
            while (condition()) {
                expressionStack.Push(new OperatorExpression(
                        expressionStack.Pop(),
                        expressionStack.Pop(),
                        operatorStack.Pop()
                    )
                );
            }
        }

        public ExpressionNode Parse(string input) {
            tokenStream = new TokenStream(Tokenizer.Tokenize(input));
            return Parse();
        }
        
        public ExpressionNode Parse() {
            ConsumeWhiteSpace();

            expressionStack.Clear();
            operatorStack.Clear();

            while (tokenStream.HasMoreTokens) {
                ExpressionNode operand = ParseExpressionOperand();
                ConsumeWhiteSpace();

                if (operand != null) {
                    expressionStack.Push(operand);
                    continue;
                }

                OperatorNode op = ParseOperatorExpression();
                if (op != null) {
                    EvaluateWhile(() => {
                        if (operatorStack.Count == 0) return false;
                        if (operatorStack.Peek() is ParenOperatorNode) return false;
                        return op.precedence <= operatorStack.Peek().precedence;
                    });
                    operatorStack.Push(op);
                    continue;
                }

                if (tokenStream.Current == TokenType.ParenOpen) {
                    tokenStream.Advance();
                    ParenOperatorNode parenNode = new ParenOperatorNode(null); // todo -- ?
                    operatorStack.Push(parenNode);
                    continue;
                }

                if (tokenStream.Current == TokenType.ParenClose) {
                    tokenStream.Advance();
                    EvaluateWhile(() => {
                        if (operatorStack.Count == 0) return false;
                        return !(operatorStack.Peek() is ParenOperatorNode);
                    });
                    operatorStack.Pop();
                    continue;
                }

                throw new Exception("Failed");
            }

            EvaluateWhile(() => operatorStack.Count > 0);
            if (expressionStack.Count != 1) {
                throw new Exception("Failed");
            }

            return expressionStack.Pop();
        }

        private ExpressionNode ParseExpressionOperand() {
            ExpressionNode constant = ParseConstantExpression();
            if (constant != null) {
                return constant;
            }

            RootContextLookup lookup = ParseLookupValue();
            if (lookup != null) {
                return lookup;
            }

            return null;
        }

        private AccessExpressionPart ParseAccessExpressionPart() {
            tokenStream.Save();
            if (tokenStream.Current == TokenType.PropertyAccess) {
                tokenStream.Advance();

                IdentifierNode idNode = ParseIdentifier();
                if (idNode != null) {
                    return new PropertyAccessExpressionPart(idNode.identifier);
                }

                tokenStream.Restore();
                return null;
            }

            if (tokenStream.Current == TokenType.ArrayAccessOpen) {
                tokenStream.Advance();
                ExpressionNode expressionNode = ParseExpression();

                if (expressionNode != null) {
                    if (tokenStream.Current == TokenType.ArrayAccessClose) {
                        tokenStream.Advance();
                        return new ArrayAccessExpressionPart(expressionNode);
                    }
                }
            }
            tokenStream.Restore();
            return null;
        }

        private ExpressionNode ParseAccessExpression() {
            // identifier ((. identifier) | [ expression ] )*)
            IdentifierNode idNode = ParseIdentifier();
            if (idNode == null) return null;

            AccessExpressionPart part = ParseAccessExpressionPart();
            List<AccessExpressionPart> parts = new List<AccessExpressionPart>();

            while (part != null) {
                parts.Add(part);
                part = ParseAccessExpressionPart();
            }

            return new AccessExpressionNode(idNode.identifier, parts);
        }

        private ExpressionNode ParseExpression() {
            RootContextLookup lookup = ParseLookupValue();
            if (lookup != null) {
                return lookup;
            }

            ConstantValueNode constant = ParseConstantValue();
            if (constant != null) {
                return constant;
            }

            UnaryExpressionNode unary = ParseUnaryExpression();
            if (unary != null) {
                return unary;
            }

            //PropertyAccessExpressionNode propertyAccess = ParsePropertyAccess();

            return null;
        }

        private OperatorNode ParseOperatorExpression() {
            tokenStream.Save();
            ConsumeWhiteSpace();
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

        private UnaryExpressionNode ParseUnaryExpression() {
            tokenStream.Save();

            UnaryOperatorType opType = UnaryOperatorType.Not;

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
                    throw new Exception("Unknown unary opertor: " + tokenStream.Current.value);
                }
                tokenStream.Advance();
            }

            ExpressionNode expression = ParseExpression();

            if (expression != null) {
                return new UnaryExpressionNode(expression, opType);
            }

            tokenStream.Restore();
            return null;
        }

        private ConstantValueNode ParseConstantExpression() {
            ConstantValueNode node = ParseConstantValue();

            return node;
        }

        private RootContextLookup ParseLookupValue() {
            tokenStream.Save();
            ConsumeWhiteSpace();
            IdentifierNode idNode = ParseIdentifier();

            if (idNode == null) return null;

            return new RootContextLookup(idNode);
            ;
        }

        private IdentifierNode ParseIdentifier() {
            tokenStream.Save();
            ConsumeWhiteSpace();

            if (tokenStream.Current != TokenType.Identifier) return null;

            IdentifierNode node = new IdentifierNode(tokenStream.Current);
            tokenStream.Advance();
            return node;
        }

        private void ConsumeWhiteSpace() {
            if (tokenStream.HasMoreTokens && tokenStream.Current == TokenType.WhiteSpace) {
                tokenStream.Advance();
            }
        }

        private ConstantValueNode ParseConstantValue() {
            tokenStream.Save();
            ConsumeWhiteSpace();
            if (tokenStream.Current == TokenType.Boolean) {
                tokenStream.Advance();
                return new BooleanConstantNode(bool.Parse(tokenStream.Previous));
            }

            if (tokenStream.Current == TokenType.Number) {
                tokenStream.Advance();
                return new NumericConstantNode(double.Parse(tokenStream.Previous));
            }

            if (tokenStream.Current == TokenType.String) {
                string value = tokenStream.Current.value.Substring(1, tokenStream.Current.value.Length - 2);
                tokenStream.Advance();
                return new StringConstantNode(value);
            }

            tokenStream.Restore();
            return null;
        }

    }

}