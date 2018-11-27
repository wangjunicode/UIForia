using System;
using System.Collections.Generic;
using UIForia.Util;

namespace UIForia.Parsing {

    public struct Parser2 {

        private TokenStream tokenStream;
        private Stack<ASTNode> expressionStack;
        private Stack<OperatorNode> operatorStack;

        public static ASTNode Parse(string input) {
            return new Parser2().ParseInternal(input);
        }

        private Parser2(TokenStream stream) {
            tokenStream = stream;
            operatorStack = StackPool<OperatorNode>.Get();
            expressionStack = StackPool<ASTNode>.Get();
        }

        private void Release() {
            tokenStream.Release();
            StackPool<OperatorNode>.Release(operatorStack);
            StackPool<ASTNode>.Release(expressionStack);
        }

        private ASTNode ParseInternal(string input) {
            tokenStream = new TokenStream(Tokenizer.Tokenize(input, ListPool<DslToken>.Get()));
            expressionStack = expressionStack ?? StackPool<ASTNode>.Get();
            operatorStack = operatorStack ?? StackPool<OperatorNode>.Get();

            if (tokenStream.Current == TokenType.ExpressionOpen) {
                tokenStream.Advance();
            }

            if (!tokenStream.HasMoreTokens) {
                throw new ParseException("Failed trying to parse empty expression");
            }

            if (tokenStream.Last == TokenType.ExpressionClose) {
                tokenStream.Chop();
            }

            ASTNode retn = ParseLoop();

            Release();

            return retn;
        }

        private bool ParseIdentifier(ref ASTNode node) {
            if (tokenStream.Current != TokenType.Identifier && tokenStream.Current != TokenType.Alias) {
                return false;
            }

            node = ASTNode.IdentifierNode(tokenStream.Current);

            tokenStream.Advance();
            return true;
        }

        private bool ParseOperatorExpression(out OperatorNode operatorNode) {
            tokenStream.Save();

            if (!tokenStream.Current.IsOperator) {
                tokenStream.Restore();
                operatorNode = default;
                return false;
            }

            tokenStream.Advance();

            switch (tokenStream.Previous.tokenType) {
                case TokenType.Plus:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Plus);
                    return true;

                case TokenType.Minus:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Minus);
                    return true;

                case TokenType.Times:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Times);
                    return true;

                case TokenType.Divide:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Divide);
                    return true;

                case TokenType.Mod:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Mod);
                    return true;

                case TokenType.And:
                    operatorNode = ASTNode.OperatorNode(OperatorType.And);
                    return true;

                case TokenType.Or:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Or);
                    return true;

                case TokenType.Equals:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Equals);
                    return true;

                case TokenType.NotEquals:
                    operatorNode = ASTNode.OperatorNode(OperatorType.NotEquals);
                    return true;

                case TokenType.GreaterThan:
                    operatorNode = ASTNode.OperatorNode(OperatorType.GreaterThan);
                    return true;

                case TokenType.GreaterThanEqualTo:
                    operatorNode = ASTNode.OperatorNode(OperatorType.GreaterThanEqualTo);
                    return true;

                case TokenType.LessThan:
                    operatorNode = ASTNode.OperatorNode(OperatorType.LessThan);
                    return true;

                case TokenType.LessThanEqualTo:
                    operatorNode = ASTNode.OperatorNode(OperatorType.LessThanEqualTo);
                    return true;

                case TokenType.QuestionMark:
                    operatorNode = ASTNode.OperatorNode(OperatorType.TernaryCondition);
                    return true;

                case TokenType.Colon:
                    operatorNode = ASTNode.OperatorNode(OperatorType.TernarySelection);
                    return true;

                default:
                    throw new Exception("Unknown op type");
            }
        }

        private ASTNode ParseLoop() {
            while (tokenStream.HasMoreTokens) {
                ASTNode operand = default;
                if (ParseExpression(ref operand)) {
                    expressionStack.Push(operand);
                    continue;
                }

                OperatorNode op;
                if (!ParseOperatorExpression(out op)) {
                    Abort();
                    break;
                }

                while (operatorStack.Count != 0 && op.priority <= operatorStack.Peek().priority) {
                    OperatorNode opNode = operatorStack.Pop();
                    opNode.right = expressionStack.Pop();
                    opNode.left = expressionStack.Pop();
                    expressionStack.Push(opNode);
                }

                operatorStack.Push(op);
            }

            while (operatorStack.Count != 0) {
                OperatorNode opNode = operatorStack.Pop();
                opNode.right = expressionStack.Pop();
                opNode.left = expressionStack.Pop();
                expressionStack.Push(opNode);
            }

            if (expressionStack.Count != 1) {
                Abort();
            }

            return expressionStack.Pop();
        }

        private Parser2 CreateSubParser(int advance) {
            tokenStream.Advance(); // step over the open brace
            // -1 to drop the closing paren token from sub stream
            TokenStream stream = tokenStream.AdvanceAndReturnSubStream(advance - 1);
            // step over closing paren
            tokenStream.Advance();

            return new Parser2(stream);
        }

        // todo string concat expression "string {nested expression}"
        private bool ParseExpression(ref ASTNode retn) {
            if (ParseTypeOfExpression(ref retn)) return true;
            if (ParseAccessExpression(ref retn)) return true;
            if (ParseParenExpression(ref retn)) return true;
            if (ParseIdentifier(ref retn)) return true;
            if (ParseLiteralValue(ref retn)) return true;
            if (ParseUnaryExpression(ref retn)) return true;

            return false;
        }

        private bool ParseUnaryExpression(ref ASTNode retn) {
            tokenStream.Save();
            
            if (tokenStream.Current == TokenType.Not) {
                tokenStream.Advance();
                ASTNode expr = null;
                if (!ParseExpression(ref expr)) {
                    tokenStream.Restore();
                    return false;
                }

                retn = ASTNode.UnaryExpressionNode(ASTNodeType.UnaryNot, expr);
                return true;
            }
            
            if (tokenStream.Current == TokenType.Minus) {
                tokenStream.Advance();
                ASTNode expr = null;
                if (!ParseExpression(ref expr)) {
                    tokenStream.Restore();
                    return false;
                }
                retn = ASTNode.UnaryExpressionNode(ASTNodeType.UnaryMinus, expr);
                return true;
            }

            if (tokenStream.Current == TokenType.ParenOpen) {
                int advance = tokenStream.FindMatchingBraceIndex(TokenType.ParenOpen, TokenType.ParenClose);
                
            }
            
            tokenStream.Restore();
            return false;
        }

        private bool ParseTypePath(ref ASTNode retn) {
            string identifier = tokenStream.Current.value;
            tokenStream.Save();

            List<ASTNode> parts = ListPool<ASTNode>.Get();

            // todo finish this
            throw new NotImplementedException();
            tokenStream.Advance();
            while (tokenStream.HasMoreTokens) {
                if (tokenStream.Current == TokenType.Dot) {
                    if (tokenStream.Next != TokenType.Identifier) {
                        break;
                    }

                    tokenStream.Advance();
                    parts.Add(ASTNode.DotAccessNode(tokenStream.Current.value));
                    tokenStream.Advance();

                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }

                return false;
            }

            retn = ASTNode.TypePathNode(identifier, parts);
            return true;
        }

        private bool ParseTypeOfExpression(ref ASTNode retn) {
            if (tokenStream.Current != TokenType.TypeOf || tokenStream.Next != TokenType.ParenOpen) {
                return false;
            }

            tokenStream.Save();
            tokenStream.Advance(2);

            ASTNode node = null;

            if (!ParseIdentifier(ref node) && !ParseDotAccessExpression(ref node)) {
                tokenStream.Restore();
                return false;
            }

            if (tokenStream.Current != TokenType.ParenClose) {
                node.Release();
                tokenStream.Restore();
                return false;
            }

            retn = ASTNode.TypeOfNode(node);

            return true;
        }

        private bool ParseDotAccessExpression(ref ASTNode node) {
            return false;
        }

        private bool ParseInvokeExpression(ref InvokeNode retn) {
            return false;
        }

        private bool ParseTypeExpression(ref TypeNode retn) {
            return false;
        }

        // something.someValue
        // something[i]
        // something(*).x(*).y
        private bool ParseAccessExpression(ref ASTNode retn) {
            if (tokenStream.Current != TokenType.Identifier) {
                return false;
            }

            string identifier = tokenStream.Current.value;
            tokenStream.Save();

            List<ASTNode> parts = ListPool<ASTNode>.Get();

            tokenStream.Advance();
            while (tokenStream.HasMoreTokens) {
                if (tokenStream.Current == TokenType.Dot) {
                    if (tokenStream.Next != TokenType.Identifier) {
                        break;
                    }

                    tokenStream.Advance();
                    parts.Add(ASTNode.DotAccessNode(tokenStream.Current.value));
                    tokenStream.Advance();

                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }
                else if (tokenStream.Current == TokenType.ArrayAccessOpen) {
                    int advance = tokenStream.FindMatchingBraceIndex(TokenType.ArrayAccessOpen, TokenType.ArrayAccessClose);
                    if (advance == -1) {
                        Abort();
                        throw new Exception("Unmatched array bracket"); // todo abort
                    }

                    Parser2 subParser = CreateSubParser(advance);
                    parts.Add(ASTNode.IndexExpressionNode(subParser.ParseLoop()));
                    subParser.Release();
                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }
                else if (tokenStream.Current == TokenType.ParenOpen) {
                    List<ASTNode> parameters = null;
                    if (!ParseListExpression(ref parameters, TokenType.ParenOpen, TokenType.ParenClose)) {
                        Abort();
                    }

                    parts.Add(ASTNode.InvokeNode(parameters));

                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }

                if (parts.Count == 0) {
                    break;
                }

                retn = ASTNode.MemberAccessExpressionNode(identifier, parts);
                return true;
            }

            tokenStream.Restore();
            for (int i = 0; i < parts.Count; i++) {
                parts[i].Release();
            }

            ListPool<ASTNode>.Release(ref parts);
            return false;
        }

        private bool ParseParenExpression(ref ASTNode retn) {
            if (tokenStream.Current != TokenType.ParenOpen) {
                return false;
            }

            int advance = tokenStream.FindMatchingBraceIndex(TokenType.ParenOpen, TokenType.ParenClose);
            if (advance == -1) throw new Exception("Unmatched paren"); // todo just abort

            Parser2 subParser = CreateSubParser(advance);
            retn = subParser.ParseLoop();
            if (retn.IsCompound) {
                retn = ASTNode.ParenNode(retn);
            }

            subParser.Release();

            return true;
        }

        // new Vector3(1, 2, 3)
        // new UnityEngine.Vector3(1, 2, 3)
        private bool ParseNewExpression(ref ASTNode retn) {
            if (tokenStream.Current != TokenType.New) {
                return false;
            }

            tokenStream.Save();
            tokenStream.Advance();
            TypeNode typeNode = null;
            InvokeNode invokeNode = null;

            if (!ParseTypeExpression(ref typeNode) && !ParseInvokeExpression(ref invokeNode)) {
                typeNode?.Release();
                invokeNode?.Release();
                tokenStream.Restore();
                return false;
            }

            retn = ASTNode.NewExpressionNode(typeNode, invokeNode);
            return true;
        }

        private bool ParseValueExpression(ref ASTNode retn) {
            if (tokenStream.Current != TokenType.Identifier) {
                return false;
            }

            retn = ASTNode.IdentifierNode(tokenStream.Current.value);
            tokenStream.Advance();
            return true;
        }

        private bool ParseDirectCastExpression(ref ASTNode retn) {
            tokenStream.Save();

            if (tokenStream.Current != TokenType.ParenOpen) {
                return false;
            }

            tokenStream.Advance();

            ASTNode node = null;

            if (!ParseIdentifier(ref node) && !ParseDotAccessExpression(ref node)) {
                tokenStream.Restore();
                return false;
            }

            if (tokenStream.Current != TokenType.ParenClose) {
                node.Release();
                tokenStream.Restore();
                return false;
            }

            return true;
        }   

        private bool ParseListExpression(ref List<ASTNode> retn, TokenType openToken, TokenType closeToken) {
            if (tokenStream.Current != openToken) {
                return false;
            }

            int range = tokenStream.FindMatchingBraceIndex(openToken, closeToken);

            tokenStream.Save();
            tokenStream.Advance();

            if (range == 1) {
                tokenStream.Advance();
                retn = null;
                return true;
            }

            if (retn != null) {
                ListPool<ASTNode>.Release(ref retn);
            }

            range += tokenStream.CurrentIndex - 1;

            ASTNode expr = null;
            if (!ParseExpression(ref expr)) {
                ListPool<ASTNode>.Release(ref retn);
                tokenStream.Restore();
                return false;
            }
            else {
                retn = ListPool<ASTNode>.Get();
                retn.Add(expr);
            }

            if (tokenStream.CurrentIndex == range) {
                tokenStream.Advance();
                return true;
            }

            while (tokenStream.CurrentIndex != range) {
                tokenStream.Advance();
                if (tokenStream.Previous != TokenType.Comma || !ParseExpression(ref expr)) {
                    for (int i = 0; i < retn.Count; i++) {
                        retn[i].Release();
                    }

                    tokenStream.Restore();
                    ListPool<ASTNode>.Release(ref retn);
                    retn = null;
                    return false;
                }

                retn.Add(expr);
            }

            tokenStream.Advance();
            return true;
        }

        private bool ParseIndexExpression(ref ASTNode retn) {
            if (tokenStream.Current != TokenType.ArrayAccessOpen) {
                return false;
            }

            tokenStream.Save();
            tokenStream.Advance();

            ASTNode indexer = null;
            if (!ParseExpression(ref indexer)) {
                tokenStream.Restore();
                return false;
            }

            retn = ASTNode.IndexExpressionNode(indexer);
            return true;
        }

        private bool ParseLiteralValue(ref ASTNode retn) {
            tokenStream.Save();

            // todo if we support bitwise not, add it here

            if (tokenStream.Current == TokenType.Not && tokenStream.Next == TokenType.Boolean) {
                bool value = bool.Parse(tokenStream.Next.value);
                retn = ASTNode.BooleanLiteralNode((!value).ToString());
                tokenStream.Advance(2);
                return true;
            }

            if (tokenStream.Current == TokenType.Minus && tokenStream.Next == TokenType.Number && (tokenStream.Previous.IsOperator || !tokenStream.HasPrevious)) {
                retn = ASTNode.NumericLiteralNode("-" + tokenStream.Next.value);
                tokenStream.Advance(2);
                return true;
            }

            if (tokenStream.Current == TokenType.Plus && tokenStream.Next == TokenType.Number && (tokenStream.Previous.IsOperator || !tokenStream.HasPrevious)) {
                retn = ASTNode.NumericLiteralNode(tokenStream.Next.value);
                tokenStream.Advance(2);
                return true;
            }

            switch (tokenStream.Current.tokenType) {
                case TokenType.Null:
                    retn = ASTNode.NullLiteralNode(tokenStream.Current.value);
                    break;

                case TokenType.String:
                    retn = ASTNode.StringLiteralNode(tokenStream.Current.value);
                    break;

                case TokenType.Boolean:
                    retn = ASTNode.BooleanLiteralNode(tokenStream.Current.value);
                    break;

                case TokenType.Number:
                    retn = ASTNode.NumericLiteralNode(tokenStream.Current.value);
                    break;

                case TokenType.Default:
                    retn = ASTNode.DefaultLiteralNode(tokenStream.Current.value);
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

    public enum ASTNodeType {

        NullLiteral,
        BooleanLiteral,
        NumericLiteral,
        DefaultLiteral,
        StringLiteral,
        Operator,
        TypeOf,
        Alias,
        Identifier,
        Invalid,
        MethodCall,
        DotAccess,
        AccessExpression,
        IndexExpression,
        UnaryNot,
        UnaryMinus,
        DirectCast,

        TypePath

    }

}