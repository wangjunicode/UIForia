using System;
using System.Collections.Generic;
using UIForia.Util;

namespace UIForia.Src.Parsing {

    public class ExpressionTree {

        private LightList<AstNode> m_NodeMap;

        public int AddAstNode(AstNode node) {
            node.id = m_NodeMap.Count;
            m_NodeMap.Add(node);
            return node.id;
        }

    }

    public struct Parser2 {

        private LightList<AstNode> expressionTree;
        private TokenStream tokenStream;
        private Stack<AstNode> expressionStack;
        private Stack<AstNode> operatorStack;

        private Parser2(TokenStream stream) {
            tokenStream = stream;
            expressionTree = new LightList<AstNode>();
            operatorStack = StackPool<AstNode>.Get();
            expressionStack = StackPool<AstNode>.Get();
        }

        private void Release() {
            tokenStream.Release();
            StackPool<AstNode>.Release(operatorStack);
            StackPool<AstNode>.Release(expressionStack);
        }

        private AstNode ParseInternal(string input) {
            tokenStream = new TokenStream(Tokenizer.Tokenize(input, ListPool<DslToken>.Get()));
            expressionStack = expressionStack ?? StackPool<AstNode>.Get();
            operatorStack = operatorStack ?? StackPool<AstNode>.Get();

            if (tokenStream.Current == TokenType.ExpressionOpen) {
                tokenStream.Advance();
            }

            if (!tokenStream.HasMoreTokens) {
                throw new ParseException("Failed trying to parse empty expression");
            }

            if (tokenStream.Last == TokenType.ExpressionClose) {
                tokenStream.Chop();
            }

            AstNode retn = ParseLoop();

            Release();

            return retn;
        }

        private AstNode ParseLoop() {
            while (tokenStream.HasMoreTokens) {
                int count = expressionTree.Count;

                AstNode operand = default;
                if (ParseExpression(ref operand)) {
                    expressionStack.Push(operand);
                    continue;
                }

                OperatorNode op = ParseOperatorExpression();
                if (op != null) {
                    while (operatorStack.Count != 0 && op.precedence <= operatorStack.Peek().precedence) {
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

        private bool ParseExpression(ref AstNode retn) {
            int count = expressionTree.Count;
//            if (ParseStringLiteralExpression(ref retn)) return true;
//            if (ParseMethodExpression(ref retn)) return true;
//            if (ParseAccessExpression(ref retn)) return true;
//            if (ParseParenExpression(ref retn)) return true;
//            if (ParseLookupExpression(ref retn)) return true;
            if (ParseLiteralValue(ref retn)) return true;
            expressionTree.RemovePast(count);
            
//            if (ParseUnaryExpression(ref retn)) return true;

            return false;
        }

        private bool ParseLiteralValue(ref AstNode retn) {
            tokenStream.Save();

            switch (tokenStream.Current.tokenType) {
                case TokenType.Null:
                    retn = new AstNode();
                    retn.type = ASTNodeType.NullLiteral;
                    break;

                case TokenType.String:
                    retn = new AstNode();
                    retn.type = ASTNodeType.StringLiteral;
                    break;

                case TokenType.Boolean:
                    retn = new AstNode();
                    retn.type = tokenStream.Current.value == "true" ? ASTNodeType.TrueLiteral : ASTNodeType.FalseLiteral;
                    break;

                case TokenType.Number:
                    retn = new AstNode();
                    retn.type = ASTNodeType.NumericLiteral;
                    retn.value = tokenStream.Current.value;
                    break;

                case TokenType.Default:
                    retn = new AstNode();
                    retn.type = ASTNodeType.DefaultLiteral;
                    break;

                default:
                    return false;
            }

            tokenStream.Advance();
            return true;
        }

        private void Abort() {
//            string additionalInfo = isLiteralExpression
//                ? "This might be because you are referencing non literal values in a string not wrapped in braces."
//                : string.Empty;
            throw new Exception($"Failed to parse {tokenStream}");
        }

    }

    public struct AstNode {

        public int id;
        public string value;
        public int firstChildId;
        public int nextSiblingId;
        public ASTNodeType type;
        public int precedence;

    }

    public enum ASTNodeType {

        NullLiteral,
        TrueLiteral,
        FalseLiteral,
        NumericLiteral,
        DefaultLiteral,
        StringLiteral,
        Operator_Plus,
        Operator_Minus,
        Operator_Times,
        Operator_Divide,
        Operator_Mod

    }

}