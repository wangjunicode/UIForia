using System;
using System.Collections.Generic;
using UIForia.Exceptions;
using UIForia.Parsing.Expression.AstNodes;
using UIForia.Parsing.Expression.Tokenizer;
using UIForia.Util;

namespace UIForia.Parsing.Expression {

    public struct ExpressionParser {

        private TokenStream tokenStream;
        private Stack<ASTNode> expressionStack;
        private Stack<OperatorNode> operatorStack;

        public static ASTNode Parse(string input) {
            return new ExpressionParser().ParseInternal(input);
        }

        public static TypePath ParseTypePath(string input) {
            TypePath typePath = new TypePath();
            ExpressionParser parser = new ExpressionParser();
            bool valid = parser.ParseTypePath(ref typePath);
            parser.Release();
            if (!valid) {
                typePath.Release();
                return default;
            }

            return typePath;
        }

        private ExpressionParser(TokenStream stream) {
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
            tokenStream = new TokenStream(ExpressionTokenizer.Tokenize(input, ListPool<ExpressionToken>.Get()));
            expressionStack = expressionStack ?? StackPool<ASTNode>.Get();
            operatorStack = operatorStack ?? StackPool<OperatorNode>.Get();

            if (tokenStream.Current == ExpressionTokenType.ExpressionOpen) {
                tokenStream.Advance();
            }

            if (!tokenStream.HasMoreTokens) {
                throw new ParseException("Failed trying to parse empty expression");
            }

            if (tokenStream.Last == ExpressionTokenType.ExpressionClose) {
                tokenStream.Chop();
            }

            ASTNode retn = ParseLoop();

            Release();

            return retn;
        }

        // consider replacing with access expression since this will always be a root property access
        private bool ParseIdentifier(ref ASTNode node) {
            if (tokenStream.Current != ExpressionTokenType.Identifier) {
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

            switch (tokenStream.Previous.expressionTokenType) {
                case ExpressionTokenType.Plus:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Plus);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.Minus:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Minus);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.Times:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Times);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.Divide:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Divide);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.Mod:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Mod);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.And:
                    operatorNode = ASTNode.OperatorNode(OperatorType.And);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.Or:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Or);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.Equals:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Equals);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.NotEquals:
                    operatorNode = ASTNode.OperatorNode(OperatorType.NotEquals);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.GreaterThan:
                    operatorNode = ASTNode.OperatorNode(OperatorType.GreaterThan);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.GreaterThanEqualTo:
                    operatorNode = ASTNode.OperatorNode(OperatorType.GreaterThanEqualTo);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.LessThan:
                    operatorNode = ASTNode.OperatorNode(OperatorType.LessThan);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.LessThanEqualTo:
                    operatorNode = ASTNode.OperatorNode(OperatorType.LessThanEqualTo);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.QuestionMark:
                    operatorNode = ASTNode.OperatorNode(OperatorType.TernaryCondition);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.Colon:
                    operatorNode = ASTNode.OperatorNode(OperatorType.TernarySelection);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.As: {
                    operatorNode = ASTNode.OperatorNode(OperatorType.As);
                    operatorNode.WithLocation(tokenStream.Previous);
                    TypePath typePath = default;
                    if (!ParseTypePath(ref typePath)) {
                        Abort();
                    }

                    // being lazy :(
                    expressionStack.Push(ASTNode.TypeOfNode(typePath));
                    return true;
                }

                case ExpressionTokenType.Is: {
                    operatorNode = ASTNode.OperatorNode(OperatorType.Is);
                    operatorNode.WithLocation(tokenStream.Previous);
                    TypePath typePath = default;
                    if (!ParseTypePath(ref typePath)) {
                        Abort();
                    }

                    // being lazy :(
                    expressionStack.Push(ASTNode.TypeOfNode(typePath));
                    return true;
                }

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

                if (expressionStack.Count == 0) {
                    Abort();
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

        private ExpressionParser CreateUndelimitedSubParser(int advance) {
            return new ExpressionParser(tokenStream.AdvanceAndReturnSubStream(advance));
        }

        private ExpressionParser CreateSubParser(int advance) {
            tokenStream.Advance(); // step over the open brace
            // -1 to drop the closing paren token from sub stream
            TokenStream stream = tokenStream.AdvanceAndReturnSubStream(advance - 1);
            // step over closing paren
            tokenStream.Advance();

            return new ExpressionParser(stream);
        }

        // todo string concat expression "string {nested expression}"
        private bool ParseExpression(ref ASTNode retn) {
            
            if (ParseDirectCastExpression(ref retn)) return true;
            if (ParseTypeOfExpression(ref retn)) return true;
            if (ParseArrayLiteralExpression(ref retn)) return true;
            if (ParseAccessExpression(ref retn)) return true;
            if (ParseParenExpression(ref retn)) return true;
            if (ParseIdentifier(ref retn)) return true;
            if (ParseLiteralValue(ref retn)) return true;
            if (ParseUnaryExpression(ref retn)) return true;

            return false;
        }
        
        private bool ParseArrayLiteralExpression(ref ASTNode retn) {
            if (tokenStream.Current != ExpressionTokenType.ArrayAccessOpen) {
                return false;
            }

            List<ASTNode> list = null;
            bool valid = ParseListExpression(ref list, ExpressionTokenType.ArrayAccessOpen, ExpressionTokenType.ArrayAccessClose);

            if (!valid) {
                return false;
            }

            retn = ASTNode.ListInitializerNode(list);

            return true;
        }

        private bool ParseUnaryExpression(ref ASTNode retn) {
            if (tokenStream.Current != ExpressionTokenType.Not && tokenStream.HasPrevious && !tokenStream.Previous.UnaryRequiresCheck) {
                return false;
            }

            tokenStream.Save();

            if (tokenStream.Current == ExpressionTokenType.Not) {
                tokenStream.Advance();
                ASTNode expr = null;
                if (!ParseExpression(ref expr)) {
                    tokenStream.Restore();
                    return false;
                }

                retn = ASTNode.UnaryExpressionNode(ASTNodeType.UnaryNot, expr);
                return true;
            }

            if (tokenStream.Current == ExpressionTokenType.Minus) {
                tokenStream.Advance();
                ASTNode expr = null;
                if (!ParseExpression(ref expr)) {
                    tokenStream.Restore();
                    return false;
                }

                retn = ASTNode.UnaryExpressionNode(ASTNodeType.UnaryMinus, expr);
                return true;
            }

            tokenStream.Restore();
            return false;
        }


        private bool ParseTypePathGenerics(ref TypePath retn) {
            if (tokenStream.Current != ExpressionTokenType.LessThan) {
                return false;
            }

            int advance = tokenStream.FindMatchingIndex(ExpressionTokenType.LessThan, ExpressionTokenType.GreaterThan);
            if (advance == -1) {
                Abort();
                return false;
            }


            tokenStream.Save();

            ExpressionParser subParser = CreateSubParser(advance);
            bool valid = subParser.ParseTypePathGenericStep(ref retn);
            subParser.Release();

            if (!valid) {
                Abort();
            }

            //tokenStream.Advance();
            return true;
        }

        private bool ParseTypePathGenericStep(ref TypePath retn) {
            TypePath arg = default;

            while (tokenStream.HasMoreTokens) {
                if (tokenStream.Current == ExpressionTokenType.Identifier) {
                    if (!ParseTypePathHead(ref arg)) {
                        tokenStream.Restore();
                        return false;
                    }

                    retn.genericArguments.Add(arg);
                    continue;
                }

                if (tokenStream.Current == ExpressionTokenType.Comma) {
                    tokenStream.Advance();
                    continue;
                }

                if (tokenStream.Current == ExpressionTokenType.LessThan) {
                    if (ParseTypePathGenerics(ref arg)) {
                        continue;
                    }
                }

                tokenStream.Restore();
                retn.ReleaseGenerics();
                return false;
            }

            return true;
        }

        private bool ParseTypePathHead(ref TypePath retn) {
            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                return false;
            }

            string identifier = tokenStream.Current.value;

            tokenStream.Save();
            tokenStream.Advance();

            retn.path = ListPool<string>.Get();
            retn.genericArguments = ListPool<TypePath>.Get();

            retn.path.Add(identifier);

            while (tokenStream.Current == ExpressionTokenType.Dot) {
                tokenStream.Advance();
                if (tokenStream.Current != ExpressionTokenType.Identifier) {
                    retn.Release();
                    retn = default;
                    tokenStream.Restore();
                    break;
                }

                retn.path.Add(tokenStream.Current.value);
                tokenStream.Advance();
            }

            return true;
        }

        private bool ParseTypePath(ref TypePath retn) {
            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                return false;
            }

            tokenStream.Save();

            if (!ParseTypePathHead(ref retn)) {
                tokenStream.Restore();
                retn.Release();
                return false;
            }

            if (!tokenStream.HasMoreTokens) {
                return true;
            }

            if (tokenStream.Current == ExpressionTokenType.LessThan && !ParseTypePathGenerics(ref retn)) {
                tokenStream.Restore();
                retn.Release();
                return false;
            }

            return true;
        }

        private bool ParseTypeOfExpression(ref ASTNode retn) {
            if (tokenStream.Current != ExpressionTokenType.TypeOf || tokenStream.Next != ExpressionTokenType.ParenOpen) {
                return false;
            }

            tokenStream.Advance();

            int advance = tokenStream.FindMatchingIndex(ExpressionTokenType.ParenOpen, ExpressionTokenType.ParenClose);
            if (advance == -1) {
                Abort();
                return false;
            }

            tokenStream.Save();

            ExpressionParser subParser = CreateSubParser(advance);
            TypePath typePath = new TypePath();
            bool valid = subParser.ParseTypePath(ref typePath);
            subParser.Release();

            if (!valid) {
                Abort(); // hard fail since typeof token has no other paths to go 
                tokenStream.Restore();
                return false;
            }

            retn = ASTNode.TypeOfNode(typePath);
            return true;
        }

        // something.someValue
        // something[i]
        // something(*).x(*).y
        private bool ParseAccessExpression(ref ASTNode retn) {
            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                return false;
            }

            string identifier = tokenStream.Current.value;
            tokenStream.Save();
            List<ASTNode> parts = ListPool<ASTNode>.Get();
            tokenStream.Advance();
            while (tokenStream.HasMoreTokens) {
                if (tokenStream.Current == ExpressionTokenType.Dot) {
                    if (tokenStream.Next != ExpressionTokenType.Identifier) {
                        break;
                    }

                    tokenStream.Advance();
                    parts.Add(ASTNode.DotAccessNode(tokenStream.Current.value));
                    tokenStream.Advance();
                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }
                else if (tokenStream.Current == ExpressionTokenType.ArrayAccessOpen) {
                    int advance = tokenStream.FindMatchingIndex(ExpressionTokenType.ArrayAccessOpen, ExpressionTokenType.ArrayAccessClose);
                    if (advance == -1) {
                        Abort();
                        throw new Exception("Unmatched array bracket"); // todo abort
                    }

                    ExpressionParser subParser = CreateSubParser(advance);
                    parts.Add(ASTNode.IndexExpressionNode(subParser.ParseLoop()));
                    subParser.Release();
                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }
                else if (tokenStream.Current == ExpressionTokenType.ParenOpen) {
                    List<ASTNode> parameters = null;
                    if (!ParseListExpression(ref parameters, ExpressionTokenType.ParenOpen, ExpressionTokenType.ParenClose)) {
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

                retn = ASTNode.MemberAccessExpressionNode(identifier, parts).WithLocation(tokenStream.Peek());
                return true;
            }

            ReleaseList(parts);
            tokenStream.Restore();
            return false;
        }

        private bool ParseParenExpression(ref ASTNode retn) {
            if (tokenStream.Current != ExpressionTokenType.ParenOpen) {
                return false;
            }

            int advance = tokenStream.FindMatchingIndex(ExpressionTokenType.ParenOpen, ExpressionTokenType.ParenClose);
            if (advance == -1) throw new Exception("Unmatched paren"); // todo just abort
            ExpressionParser subParser = CreateSubParser(advance);
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
            if (tokenStream.Current != ExpressionTokenType.New) {
                return false;
            }

            throw new NotImplementedException();
        }

        // (int)something
        private bool ParseDirectCastExpression(ref ASTNode retn) {
            if (tokenStream.Current != ExpressionTokenType.ParenOpen) {
                return false;
            }

            ASTNode expression = null;

            int advance = tokenStream.FindMatchingIndex(ExpressionTokenType.ParenOpen, ExpressionTokenType.ParenClose);
            if (advance == -1) {
                Abort();
                return false;
            }

            tokenStream.Save();

            ExpressionParser subParser = CreateSubParser(advance);
            TypePath typePath = new TypePath();
            bool valid = subParser.ParseTypePath(ref typePath);
            subParser.Release();

            if (!valid) {
                tokenStream.Restore();
                return false;
            }

            if (!ParseExpression(ref expression)) {
                typePath.Release();
                tokenStream.Restore();
                tokenStream.Restore();
                return false;
            }

            retn = ASTNode.DirectCastNode(typePath, expression);
            return true;
        }

        private bool ParseListExpressionStep(ref List<ASTNode> retn) {
            while (true) {
                int commaIndex = tokenStream.FindNextIndexAtSameLevel(ExpressionTokenType.Comma);
                if (commaIndex != -1) {
                    ExpressionParser parser = CreateUndelimitedSubParser(commaIndex);
                    tokenStream.Advance();
                    bool valid = parser.ParseListExpressionStep(ref retn);
                    parser.Release();
                    if (!valid) {
                        ReleaseList(retn);
                        return false;
                    }
                    
                }
                else {
                    ASTNode node = ParseLoop();
                    if (node == null) {
                        return false;
                    }

                    retn.Add(node);
                    return true;
                }
            }

        }

        private bool ParseListExpression(ref List<ASTNode> retn, ExpressionTokenType openExpressionToken, ExpressionTokenType closeExpressionToken) {
            if (tokenStream.Current != openExpressionToken) {
                return false;
            }

            int range = tokenStream.FindMatchingIndex(openExpressionToken, closeExpressionToken);
            tokenStream.Save();

            if (range == 1) {
                tokenStream.Advance(2);
                retn = ListPool<ASTNode>.Get();
                return true;
            }

            if (retn != null) {
                ListPool<ASTNode>.Release(ref retn);
            }

            retn = ListPool<ASTNode>.Get();
            //todo find next comma at same level (meaning not inside [ or ( or <

            ExpressionParser parser = CreateSubParser(range);
            bool valid = parser.ParseListExpressionStep(ref retn);
            parser.Release();

            if (!valid) {
                tokenStream.Restore();
                ReleaseList(retn);
                return false;
            }

            return true;
        }

        private bool ParseLiteralValue(ref ASTNode retn) {
            tokenStream.Save();

            // todo if we support bitwise not, add it here
            if (tokenStream.Current == ExpressionTokenType.Not && tokenStream.Next == ExpressionTokenType.Boolean) {
                bool value = bool.Parse(tokenStream.Next.value);
                retn = ASTNode.BooleanLiteralNode((!value).ToString()).WithLocation(tokenStream.Current);
                tokenStream.Advance(2);
                return true;
            }

            if (tokenStream.Current == ExpressionTokenType.Minus && tokenStream.Next == ExpressionTokenType.Number && (tokenStream.Previous.IsOperator || !tokenStream.HasPrevious)) {
                retn = ASTNode.NumericLiteralNode("-" + tokenStream.Next.value).WithLocation(tokenStream.Current);
                tokenStream.Advance(2);
                return true;
            }

            if (tokenStream.Current == ExpressionTokenType.Plus && tokenStream.Next == ExpressionTokenType.Number && (tokenStream.Previous.IsOperator || !tokenStream.HasPrevious)) {
                retn = ASTNode.NumericLiteralNode(tokenStream.Next.value).WithLocation(tokenStream.Current);
                tokenStream.Advance(2);
                return true;
            }

            switch (tokenStream.Current.expressionTokenType) {
                case ExpressionTokenType.Null:
                    retn = ASTNode.NullLiteralNode(tokenStream.Current.value).WithLocation(tokenStream.Current);
                    break;
                case ExpressionTokenType.String:
                    retn = ASTNode.StringLiteralNode(tokenStream.Current.value).WithLocation(tokenStream.Current);
                    break;
                case ExpressionTokenType.Boolean:
                    retn = ASTNode.BooleanLiteralNode(tokenStream.Current.value).WithLocation(tokenStream.Current);
                    break;
                case ExpressionTokenType.Number:
                    retn = ASTNode.NumericLiteralNode(tokenStream.Current.value).WithLocation(tokenStream.Current);
                    break;
                case ExpressionTokenType.Default:
                    retn = ASTNode.DefaultLiteralNode(tokenStream.Current.value).WithLocation(tokenStream.Current);
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

        private static void ReleaseList(List<ASTNode> list) {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++) {
                list[i].Release();
            }

            ListPool<ASTNode>.Release(ref list);
        }

    }

    public enum ASTNodeType {

        None,
        NullLiteral,
        BooleanLiteral,
        NumericLiteral,
        DefaultLiteral,
        StringLiteral,
        Operator,
        TypeOf,
        Identifier,
        Invalid,
        DotAccess,
        AccessExpression,
        IndexExpression,
        UnaryNot,
        UnaryMinus,
        DirectCast,
        ListInitializer,
        New,
        Paren

    }

}