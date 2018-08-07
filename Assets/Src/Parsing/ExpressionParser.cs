using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Src {
    public class BindingGenerator { }

    public class StringGenerator { }

    public class CodeGenerator {
        public CodeGenerator(ASTNode root) { }


        public void Visit(ConstantValueNode node) {
            // bindings.Add(new ConstantValueBinding(node.value));
        }

        public void Visit(RootContextLookup node) {
            // bindings.Add(new RootContextLookupBinding(node.identifier);
        }

        public void Visit(ExpressionNode node) {
            // if node.left is ParenExpression
            // if node.left is ConstantExpression
            // if node.right is op expression
            // new OperatorBinding(node.left, node.right.op, node.right.expression)
            // if operatorbinding.Optimize()
            // operatorbinding.right == constant
        }

        public void Visit() {
            // CompoundExpressionBinding()
            // left = exp, op = op
            // expression.Add(new OperatorExpression(op, Visit(left), Visit(right));
        }
    }

    public class ExpressionParser {
        private readonly TokenStream tokenStream;
        private readonly Stack<ExpressionNode> expressionStack;
        private readonly Stack<OperatorNode> operatorStack;

        public ExpressionParser(TokenStream tokenStream) {
            this.tokenStream = tokenStream;
            expressionStack = new Stack<ExpressionNode>();
            operatorStack = new Stack<OperatorNode>();
        }

        private void EvaluateWhile(Func<bool> condition) {
            while (condition()) {
                ExpressionNode right = expressionStack.Pop();
                ExpressionNode left = expressionStack.Pop();
                OperatorExpression expression = new OperatorExpression();
                expression.right = right;
                expression.left = left;
                expression.op = operatorStack.Pop();
                expressionStack.Push(expression);
            }
        }

        public ASTNode Parse() {
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
                    ParenOperatorNode parenNode = new ParenOperatorNode();
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

        private ExpressionNode ParseExpressionStatement() {
            tokenStream.Save();
            if (tokenStream.Current != TokenType.ExpressionOpen) {
                tokenStream.Restore();
                return null;
            }

            tokenStream.Advance();

            ExpressionNode expression = ParseExpression();
            if (expression == null) {
                // error -- expected expression here
            }

            if (tokenStream.Current != TokenType.ExpressionClose) {
                tokenStream.Restore();
                return null;
            }

            tokenStream.Advance();

            return null;
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

            OperatorNode opNode = new OperatorNode();
            switch (tokenStream.Current.tokenType) {
                case TokenType.Plus:
                    opNode.precedence = 1;
                    opNode.op = OperatorType.Plus;
                    break;
                case TokenType.Minus:
                    opNode.precedence = 1;
                    opNode.op = OperatorType.Minus;
                    break;
                case TokenType.Times:
                    opNode.precedence = 2;
                    opNode.op = OperatorType.Times;
                    break;
                case TokenType.Divide:
                    opNode.precedence = 2;
                    opNode.op = OperatorType.Divide;
                    break;
                case TokenType.Mod:
                    opNode.precedence = 1;
                    opNode.op = OperatorType.Mod;
                    break;
                default:
                    throw new Exception("Unknown op type");
            }

            tokenStream.Advance();
            return opNode;
        }

        private UnaryExpressionNode ParseUnaryExpression() {
            tokenStream.Save();

            string op = string.Empty;
            if ((tokenStream.Current & TokenType.UnaryOperator) != 0) {
                op = tokenStream.Current;
                tokenStream.Advance();
            }

            ExpressionNode expression = ParseExpression();

            if (expression != null) {
                UnaryExpressionNode retn = new UnaryExpressionNode();
                retn.expression = expression;
                retn.op = op;
                return retn;
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

            RootContextLookup lookup = new RootContextLookup();
            lookup.idNode = idNode;
            return lookup;
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
                BooleanConstantNode constantNode = new BooleanConstantNode();
                constantNode.value = bool.Parse(tokenStream.Current);
                tokenStream.Advance();
                return constantNode;
            }

            if (tokenStream.Current == TokenType.Number) {
                NumericConstantNode constantNode = new NumericConstantNode();
                constantNode.value = double.Parse(tokenStream.Current);
                tokenStream.Advance();
                return constantNode;
            }

            if (tokenStream.Current == TokenType.String) {
                StringConstantNode constantNode = new StringConstantNode();
                constantNode.value = tokenStream.Current.value.Substring(1, tokenStream.Current.value.Length - 2);
                tokenStream.Advance();
                return constantNode;
            }

            tokenStream.Restore();
            return null;
        }
    }
}