using System;
using System.Diagnostics;
using System.Text;
using UIForia.Exceptions;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Parsing.Expressions.Tokenizer;
using UIForia.Parsing.Tokenizer;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Parsing.Expressions {

    public struct ExpressionParser {

        private TokenStream tokenStream;
        private LightStack<ASTNode> expressionStack;
        private LightStack<OperatorNode> operatorStack;

        [DebuggerStepThrough]
        public static ASTNode Parse(string input) {
            return new ExpressionParser().ParseInternal(input);
        }

        public ExpressionParser(TokenStream stream) {
            tokenStream = stream;
            operatorStack = LightStack<OperatorNode>.Get();
            expressionStack = LightStack<ASTNode>.Get();
        }

        public void Release(bool releaseTokenStream) {
            if (releaseTokenStream) {
                tokenStream.Release();
            }

            LightStack<OperatorNode>.Release(ref operatorStack);
            LightStack<ASTNode>.Release(ref expressionStack);
        }

        public static ASTNode Parse(TokenStream tokenStream) {
            ExpressionParser parser = new ExpressionParser(tokenStream);
            ASTNode retn = parser.ParseLoop();
            parser.Release(false);
            return retn;
        }

        public static ASTNode Parse(StructList<ExpressionToken> tokenList) {
            ExpressionParser parser = new ExpressionParser(new TokenStream(tokenList));
            ASTNode retn = parser.ParseLoop();
            parser.Release(false);
            return retn;
        }

        private ASTNode ParseInternal(string input) {
            tokenStream = new TokenStream(ExpressionTokenizer.Tokenize(input, StructList<ExpressionToken>.Get()));
            expressionStack ??= LightStack<ASTNode>.Get();
            operatorStack ??= LightStack<OperatorNode>.Get();

            // if (tokenStream.Current == ExpressionTokenType.BracketOpen) {
            //     tokenStream.Advance();
            // }

            if (!tokenStream.HasMoreTokens) {
                throw new ExpressionParseException("Failed trying to parse empty expression");
            }

            // if (tokenStream.Last == ExpressionTokenType.BracketClose) {
            // tokenStream.Chop();
            // }

            ASTNode retn = ParseLoop();

            Release(true);

            return retn;
        }

        // consider replacing with access expression since this will always be a root property access
        private bool ParseIdentifier(ref ASTNode node) {
            if (tokenStream.Current != ExpressionTokenType.Identifier) {

                if (tokenStream.Current == ExpressionTokenType.At && tokenStream.Next == ExpressionTokenType.Identifier) {
                    string name = tokenStream.Next.value;
                    tokenStream.Advance(2);
                    if (tokenStream.Current == ExpressionTokenType.Dot && tokenStream.Next == ExpressionTokenType.Identifier) {
                        node = ASTNode.StyleIdNode(name, tokenStream.Next);
                        tokenStream.Advance(2);
                    }
                    else {
                        node = ASTNode.StyleIdNode(name);
                    }

                    return true;
                }

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
                case ExpressionTokenType.Assign:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Assign);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.AddAssign:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Assign | OperatorType.Plus);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.SubtractAssign:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Assign | OperatorType.Minus);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.MultiplyAssign:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Assign | OperatorType.Times);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.DivideAssign:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Assign | OperatorType.Divide);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.ModAssign:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Assign | OperatorType.Mod);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.AndAssign:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Assign | OperatorType.And);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.OrAssign:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Assign | OperatorType.Or);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.XorAssign:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Assign | OperatorType.BinaryXor);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.LeftShiftAssign:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Assign | OperatorType.ShiftLeft);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.RightShiftAssign:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Assign | OperatorType.ShiftRight);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

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

                case ExpressionTokenType.BinaryAnd:
                    operatorNode = ASTNode.OperatorNode(OperatorType.BinaryAnd);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.BinaryOr:
                    operatorNode = ASTNode.OperatorNode(OperatorType.BinaryOr);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.BinaryXor:
                    operatorNode = ASTNode.OperatorNode(OperatorType.BinaryXor);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.AndAlso:
                    operatorNode = ASTNode.OperatorNode(OperatorType.And);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.OrElse:
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
                    // don't make this a token type since generic paths use the << in their syntax
                    // two greater thans next to each other are the same as a shift right. this since whitespace is ignored this means > > is actually a shift operator
                    if (tokenStream.Current.expressionTokenType == ExpressionTokenType.GreaterThan) {
                        tokenStream.Advance(); // step over the 2nd one
                        operatorNode = ASTNode.OperatorNode(OperatorType.ShiftRight);
                        operatorNode.WithLocation(tokenStream.Previous);
                        return true;
                    }

                    operatorNode = ASTNode.OperatorNode(OperatorType.GreaterThan);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.GreaterThanEqualTo:
                    operatorNode = ASTNode.OperatorNode(OperatorType.GreaterThanEqualTo);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.LessThan:
                    // two less thans next to each other are the same as a shift left. this since whitespace is ignored this means < < is actually a shift operator
                    if (tokenStream.Current.expressionTokenType == ExpressionTokenType.LessThan) {
                        tokenStream.Advance(); // step over the 2nd one
                        operatorNode = ASTNode.OperatorNode(OperatorType.ShiftLeft);
                        operatorNode.WithLocation(tokenStream.Previous);
                        return true;
                    }

                    operatorNode = ASTNode.OperatorNode(OperatorType.LessThan);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.LessThanEqualTo:
                    operatorNode = ASTNode.OperatorNode(OperatorType.LessThanEqualTo);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.QuestionMark: {
                    throw new Exception("Should not hit this");
                }

                case ExpressionTokenType.Colon:
                    operatorNode = ASTNode.OperatorNode(OperatorType.TernarySelection);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.Coalesce:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Coalesce);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.Elvis:
                    operatorNode = ASTNode.OperatorNode(OperatorType.Elvis);
                    operatorNode.WithLocation(tokenStream.Previous);
                    return true;

                case ExpressionTokenType.As: {
                    operatorNode = ASTNode.OperatorNode(OperatorType.As);
                    operatorNode.WithLocation(tokenStream.Previous);
                    TypeLookup typeLookup = default;
                    if (!ParseTypePath(ref typeLookup)) {
                        Abort();
                    }

                    // todo -- figure out why we are directly pushing this into the stack
                    expressionStack.Push(ASTNode.TypeOfNode(typeLookup));
                    return true;
                }

                case ExpressionTokenType.Is: {
                    // todo -- support variable name syntax ie val is float f
                    operatorNode = ASTNode.OperatorNode(OperatorType.Is);
                    operatorNode.WithLocation(tokenStream.Previous);
                    TypeLookup typeLookup = default;
                    if (!ParseTypePath(ref typeLookup)) {
                        Abort();
                    }

                    // todo -- figure out why we are directly pushing this into the stack
                    expressionStack.Push(ASTNode.TypeOfNode(typeLookup));
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
                    if (tokenStream.Current == ExpressionTokenType.Increment) {
                        tokenStream.Advance();
                        expressionStack.Push(ASTNode.UnaryExpressionNode(ASTNodeType.UnaryPostIncrement, operand));
                    }
                    else if (tokenStream.Current == ExpressionTokenType.Decrement) {
                        tokenStream.Advance();
                        expressionStack.Push(ASTNode.UnaryExpressionNode(ASTNodeType.UnaryPostDecrement, operand));
                    }
                    else {
                        expressionStack.Push(operand);
                    }

                    continue;
                }

                if (expressionStack.Count == 0) {
                    Abort();
                }

                if (tokenStream.Current == ExpressionTokenType.QuestionMark && !tokenStream.NextTokenIs(ExpressionTokenType.QuestionMark)) {
                    while (operatorStack.Count != 0) {
                        OperatorNode opNode = operatorStack.Pop();
                        opNode.right = expressionStack.Pop();
                        opNode.left = expressionStack.Pop();
                        expressionStack.Push(opNode);
                    }

                    OperatorNode condition = ASTNode.OperatorNode(OperatorType.TernaryCondition);
                    OperatorNode selection = ASTNode.OperatorNode(OperatorType.TernarySelection);

                    condition.WithLocation(tokenStream.Previous);
                    tokenStream.Advance();
                    int idx = tokenStream.FindMatchingTernaryColon();

                    if (idx != -1) {
                        TokenStream stream = tokenStream.AdvanceAndReturnSubStream(idx);

                        // parse the left side of the : operator
                        ExpressionParser parser = new ExpressionParser(stream);
                        ASTNode leftNode = parser.ParseLoop();
                        parser.Release(true);

                        tokenStream.Advance(); // step over colon

                        ExpressionParser parserRight = new ExpressionParser(tokenStream);
                        ASTNode rightNode = parserRight.ParseLoop();
                        tokenStream.Set(parserRight.tokenStream.CurrentIndex);
                        parserRight.Release(false);

                        selection.left = leftNode;
                        selection.right = rightNode;

                        condition.left = expressionStack.Pop();
                        condition.right = selection;

                        expressionStack.Push(condition);
                    }
                    else {
                        // read to end use implicit default value for left hand side

                        ExpressionParser parserLeft = new ExpressionParser(tokenStream);
                        ASTNode leftNode = parserLeft.ParseLoop();
                        tokenStream.Set(parserLeft.tokenStream.CurrentIndex);
                        parserLeft.Release(false);

                        selection.left = leftNode;
                        selection.right = ASTNode.DefaultLiteralNode("default");

                        condition.left = expressionStack.Pop();
                        condition.right = selection;

                        expressionStack.Push(condition);
                    }

                    continue;
                }

                if (!ParseOperatorExpression(out OperatorNode op)) {
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

        // todo -- in theory we don't need a full substream, just need constrain the indices for the ptr
        private ExpressionParser CreateSubParser(int advance) {
            tokenStream.Advance(); // step over the open brace
            // -1 to drop the closing paren token from sub stream
            TokenStream stream = tokenStream.AdvanceAndReturnSubStream(advance - 1);
            // step over closing paren
            tokenStream.Advance();

            return new ExpressionParser(stream);
        }

        private bool ParseExpression(ref ASTNode retn) {
            if (ParseNewExpression(ref retn)) return true;
            if (ParseOutExpression(ref retn)) return true;
            if (ParseStringInterpolation(ref retn)) return true;
            if (ParseLambdaExpression(ref retn)) return true;
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

        private unsafe bool ParseStringInterpolation(ref ASTNode node) {

            if (tokenStream.Current != ExpressionTokenType.StringInterpolation) {
                return false;
            }

            // find contents, everything until { is a string

            StructList<StringInterpolationPart> parts = StructList<StringInterpolationPart>.Get();
            string rawContent = tokenStream.Current.value;
            fixed (char* cbuffer = rawContent) {

                CharStream stream = new CharStream(cbuffer, 0, rawContent.Length);
                stream.SetCommentMode(CommentMode.None);

                RangeInt partRange = new RangeInt();

                while (stream.HasMoreTokens) {

                    if (stream.TryGetBraceStream(out CharStream expressionStream, WhitespaceHandling.None)) {
                        if (partRange.length > 0) {
                            parts.Add(new StringInterpolationPart() {
                                isExpression = false,
                                contentRange = partRange,
                                rawContent = rawContent
                            });
                        }

                        parts.Add(new StringInterpolationPart() {
                            isExpression = true,
                            contentRange = new CharSpan(expressionStream).GetContentRange(),
                            rawContent = rawContent
                        });
                        partRange = new RangeInt(stream.IntPtr, 0);
                    }
                    else {
                        partRange.length++;
                        stream.Advance();
                    }

                }

                if (partRange.length > 0) {
                    parts.Add(new StringInterpolationPart() {
                        isExpression = false,
                        contentRange = partRange,
                        rawContent = rawContent
                    });
                }
            }

            node = ASTNode.StringInterpolationNode(rawContent, parts);
            tokenStream.Advance();

            return true;

        }

        private ASTNode ParseBlockExpression() {

            if (tokenStream.Current != ExpressionTokenType.BracketOpen) {
                return null;
            }

            int advance = tokenStream.FindMatchingIndex(ExpressionTokenType.BracketOpen, ExpressionTokenType.BracketClose);

            if (advance < 0) {
                return null;
            }

            // split block into sections 
            // section terminators = ; if else switch while
            LightList<RangeInt> ranges = LightList<RangeInt>.Get();

            tokenStream.Advance(); // step over open bracket 

            int currentIndex = tokenStream.CurrentIndex;

            int last = currentIndex;

            for (int i = currentIndex; i < currentIndex + advance - 1; i++) {

                // todo -- will need to support if, else, switch, loops probably 

                if (tokenStream.tokens[i] == ExpressionTokenType.SemiColon) {
                    ranges.Add(new RangeInt(last, i - last));
                    last = i + 1;
                }

            }

            // should handle the single statement case that is non terminated
            if (ranges.size == 0) {
                ranges.Add(new RangeInt(currentIndex, advance - 1));
            }

            LightList<ASTNode> blockExpressions = new LightList<ASTNode>(ranges.size);

            for (int i = 0; i < ranges.size; i++) {
                blockExpressions.Add(ExpressionParser.Parse(tokenStream.Slice(ranges[i])));
            }

            BlockNode node = new BlockNode(blockExpressions);

            tokenStream.Advance(advance);

            ranges.Release();

            return node;
        }

        private bool ParseLambdaExpression(ref ASTNode node) {
            if (tokenStream.Current != ExpressionTokenType.ParenOpen) {
                return false;
            }

            int advance = tokenStream.FindMatchingIndex(ExpressionTokenType.ParenOpen, ExpressionTokenType.ParenClose);

            if (advance < 0) {
                return false;
            }

            if (!tokenStream.HasTokenAt(advance + 1)) {
                return false;
            }

            if (tokenStream.Peek(advance + 1) != ExpressionTokenType.LambdaArrow) {
                return false;
            }

            StructList<LambdaArgument> signatureList = StructList<LambdaArgument>.Get();

            if (advance != 1) {
                ExpressionParser subParser = CreateSubParser(advance);

                if (!subParser.ParseLambdaSignature(ref signatureList)) {
                    return false;
                }

                tokenStream.Advance(); // step over => 
            }
            else {
                tokenStream.Advance(3); // 1 = ( 2 = ) 3 = => 
            }

            ASTNode body = ParseBlockExpression() ?? ParseLoop();

            node = ASTNode.LambdaExpressionNode(signatureList, body);

            return true;
        }

        public static bool ParseSignature(TokenStream tokenStream, StructList<LambdaArgument> signature) {
            ExpressionParser parser = new ExpressionParser(tokenStream);
            bool retn = parser.ParseLambdaSignature(ref signature);

            parser.Release(true);

            if (!retn) {
                signature.Clear();
            }

            return retn;
        }

        public bool ParseLambdaSignature(ref StructList<LambdaArgument> signature) {
            tokenStream.Save();

            while (tokenStream.HasMoreTokens) {
                // can be either just names or type specifier and then names. can probably also be aliases
                TypeLookup typeLookup = default;
                LambdaArgument argument = default;

                // should parse a path even if only an identifier is present
                if (ParseTypePath(ref typeLookup)) {
                    argument.type = typeLookup;
                }
                else {
                    tokenStream.Restore();
                    return false;
                }

                if (!tokenStream.HasMoreTokens) {
                    if (typeLookup.generics.array != null || !string.IsNullOrEmpty(typeLookup.namespaceName)) {
                        tokenStream.Restore();
                        return false;
                    }

                    argument.identifier = typeLookup.typeName;
                    argument.type = null;
                    signature.Add(argument);
                    return true;
                }

                if (tokenStream.Current == ExpressionTokenType.Identifier) {
                    argument.identifier = tokenStream.Current;
                    tokenStream.Advance();
                    signature.Add(argument);
                }
                else if (tokenStream.Current == ExpressionTokenType.Comma) {
                    argument.identifier = typeLookup.typeName;
                    argument.type = null;
                    signature.Add(argument);
                }

                if (!tokenStream.HasMoreTokens) {
                    return true;
                }

                // better be a comma or end if have more tokens
                if (tokenStream.Current != ExpressionTokenType.Comma) {
                    signature.Clear();
                    Abort($"Failed parse LambdaExpression signature because we expected a comma and we hit {tokenStream.Current} instead.");
                    return false;
                }

                tokenStream.Advance();
            }

            return true;
        }

        private bool ParseArrayLiteralExpression(ref ASTNode retn) {
            if (tokenStream.Current != ExpressionTokenType.ArrayAccessOpen) {
                return false;
            }

            LightList<ASTNode> list = null;
            bool valid = ParseListExpression(ref list, ExpressionTokenType.ArrayAccessOpen, ExpressionTokenType.ArrayAccessClose);

            if (!valid) {
                return false;
            }

            retn = ASTNode.ListInitializerNode(list);

            return true;
        }

        private bool ParseUnaryExpression(ref ASTNode retn) {
            if (tokenStream.Current != ExpressionTokenType.Not && tokenStream.Current != ExpressionTokenType.BinaryNot && tokenStream.HasPrevious && !tokenStream.Previous.UnaryRequiresCheck) {
                return false;
            }

            tokenStream.Save();

            if (tokenStream.Current == ExpressionTokenType.Increment) {
                tokenStream.Advance();
                ASTNode expr = null;
                if (!ParseExpression(ref expr)) {
                    tokenStream.Restore();
                    return false;
                }

                retn = ASTNode.UnaryExpressionNode(ASTNodeType.UnaryPreIncrement, expr);
                return true;
            }

            if (tokenStream.Current == ExpressionTokenType.Decrement) {
                tokenStream.Advance();
                ASTNode expr = null;
                if (!ParseExpression(ref expr)) {
                    tokenStream.Restore();
                    return false;
                }

                retn = ASTNode.UnaryExpressionNode(ASTNodeType.UnaryPreDecrement, expr);
                return true;
            }

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

            if (tokenStream.Current == ExpressionTokenType.BinaryNot) {
                tokenStream.Advance();
                ASTNode expr = null;
                if (!ParseExpression(ref expr)) {
                    tokenStream.Restore();
                    return false;
                }

                retn = ASTNode.UnaryExpressionNode(ASTNodeType.UnaryBitwiseNot, expr);
                return true;
            }

            tokenStream.Restore();
            return false;
        }

        private bool ParseTypePathGenerics(ref TypeLookup retn) {
            if (tokenStream.Current != ExpressionTokenType.LessThan) {
                return false;
            }

            int advance = tokenStream.FindMatchingIndex(ExpressionTokenType.LessThan, ExpressionTokenType.GreaterThan);
            if (advance == -1) {
                //  Abort();
                return false;
            }

            tokenStream.Save();

            ExpressionParser subParser = CreateSubParser(advance);
            bool valid = subParser.ParseTypePathGenericStep(ref retn);
            subParser.Release(true);

            if (!valid) {
                Abort();
            }

            //tokenStream.Advance();
            return true;
        }

        private bool ParseTypePathGenericStep(ref TypeLookup retn) {
            TypeLookup arg = default;

            while (tokenStream.HasMoreTokens) {
                if (tokenStream.Current == ExpressionTokenType.Identifier) {
                    if (!ParseTypePathHead(ref arg)) {
                        tokenStream.Restore();
                        return false;
                    }

                    arg.generics = default;

                    continue;
                }

                if (tokenStream.Current == ExpressionTokenType.Comma) {
                    retn.generics = retn.generics.array != default ? retn.generics : new SizedArray<TypeLookup>(4);
                    retn.generics.Add(arg);
                    tokenStream.Advance();
                    continue;
                }

                if (tokenStream.Current == ExpressionTokenType.LessThan) {
                    if (ParseTypePathGenerics(ref arg)) {
                        continue;
                    }
                }

                tokenStream.Restore();
                retn.Release();
                return false;
            }

            retn.generics = retn.generics.array != default ? retn.generics : new SizedArray<TypeLookup>(4);
            retn.generics.Add(arg);

            return true;
        }

        private bool ParseTypePathHead(ref TypeLookup retn) {
            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                return false;
            }

            string identifier = tokenStream.Current.value;

            tokenStream.Save();
            tokenStream.Advance();

            string lastString = identifier;

            StringBuilder builder = StringUtil.GetPerThreadStringBuilder();

            while (tokenStream.Current == ExpressionTokenType.Dot) {
                tokenStream.Advance();

                if (tokenStream.Current != ExpressionTokenType.Identifier) {
                    retn.Release();
                    retn = default;
                    tokenStream.Restore();
                    break;
                }

                builder.Append(lastString);

                if (tokenStream.NextTokenIs(ExpressionTokenType.Dot)) {
                    builder.Append(".");
                }

                lastString = tokenStream.Current.value;

                tokenStream.Advance();
            }

            // if (builder.Length > 1) {
            // builder.Remove(builder.Length - 1, 1);
            // }

            retn.namespaceName = builder.ToString();
            retn.typeName = lastString;
            StringUtil.ReleasePerThreadStringBuilder(builder);
            return true;
        }

        public bool ParseTypePath(ref TypeLookup retn) {
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

            if (tokenStream.Current == ExpressionTokenType.ArrayAccessOpen && tokenStream.HasMoreTokens && tokenStream.Next == ExpressionTokenType.ArrayAccessClose) {
                retn.isArray = true;
                tokenStream.Advance(2);
            }

            return true;
        }

        public static unsafe bool TryParseTypeName(string typeName, out TypeLookup typeLookup) {
            fixed (char* cbuffer = typeName) {
                StructList<ExpressionToken> list = StructList<ExpressionToken>.Get();
                CheckedArray<char> str = new CheckedArray<char>(cbuffer, typeName.Length);
                ExpressionTokenizerUnsafe.Tokenize(str, list);
                ExpressionParser parser = new ExpressionParser(new TokenStream(list));
                typeLookup = default;
                bool valid = parser.ParseTypePath(ref typeLookup);

                parser.Release(true); // list released here 
                return valid;
            }
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
            TypeLookup typeLookup = new TypeLookup();
            bool valid = subParser.ParseTypePath(ref typeLookup);
            subParser.Release(true);

            if (!valid) {
                Abort(); // hard fail since typeof token has no other paths to go 
                tokenStream.Restore();
                return false;
            }

            retn = ASTNode.TypeOfNode(typeLookup);
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
            LightList<ASTNode> parts = LightList<ASTNode>.Get();
            tokenStream.Advance();
            while (tokenStream.HasMoreTokens) {
                if (tokenStream.Current == ExpressionTokenType.Dot || tokenStream.Current == ExpressionTokenType.Elvis) {
                    if (tokenStream.Next != ExpressionTokenType.Identifier) {
                        break;
                    }

                    tokenStream.Advance();
                    parts.Add(ASTNode.DotAccessNode(tokenStream.Current.value, tokenStream.Previous == ExpressionTokenType.Elvis));
                    tokenStream.Advance();
                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }
                else if (tokenStream.Current == ExpressionTokenType.ArrayAccessOpen || tokenStream.Current == ExpressionTokenType.QuestionMark && tokenStream.NextTokenIs(ExpressionTokenType.ArrayAccessOpen)) {
                    bool isElvis = false;
                    if (tokenStream.Current == ExpressionTokenType.QuestionMark) {
                        isElvis = true;
                        tokenStream.Advance();
                    }

                    int advance = tokenStream.FindMatchingIndex(ExpressionTokenType.ArrayAccessOpen, ExpressionTokenType.ArrayAccessClose);
                    if (advance == -1) {
                        Abort("Unmatched array bracket");
                    }

                    ExpressionParser subParser = CreateSubParser(advance);
                    parts.Add(ASTNode.IndexExpressionNode(subParser.ParseLoop(), isElvis));
                    subParser.Release(true);
                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }
                else if (tokenStream.Current == ExpressionTokenType.ParenOpen) {
                    LightList<ASTNode> parameters = null;

                    if (!ParseListExpression(ref parameters, ExpressionTokenType.ParenOpen, ExpressionTokenType.ParenClose)) {
                        Abort();
                    }

                    parts.Add(ASTNode.InvokeNode(parameters));
                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }

                else if (tokenStream.Current == ExpressionTokenType.LessThan) {
                    // shortcut the << operator since we can't have a << in a generic type node. List<<string>> is invalid for example
                    if (tokenStream.HasMoreTokens && tokenStream.Next == ExpressionTokenType.LessThan) {
                        tokenStream.Restore();
                        LightList<ASTNode>.Release(ref parts);
                        return false;
                    }

                    TokenStream stream = tokenStream.AdvanceAndGetSubStreamBetween(ExpressionTokenType.LessThan, ExpressionTokenType.GreaterThan);
                    ExpressionParser subParser = new ExpressionParser(stream);
                    
                    TypeLookup typePath = new TypeLookup();
                    if (!subParser.ParseTypePath(ref typePath)) {
                        tokenStream.Restore();
                        LightList<ASTNode>.Release(ref parts);
                        return false;
                    }


                    parts.Add(ASTNode.GenericTypePath(typePath));
                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }

                if (parts.Count == 0) {
                    tokenStream.Restore();
                    LightList<ASTNode>.Release(ref parts);
                    return false;
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

            ASTNode access = null;

            if (ParseParenAccessExpression(ref access)) {
                ParenNode parenNode = (ParenNode) retn;
                parenNode.accessExpression = (MemberAccessExpressionNode) access;
            }

            subParser.Release(true);
            return true;
        }

        private bool ParseParenAccessExpression(ref ASTNode retn) {
            // string identifier = tokenStream.Current.value;
            // tokenStream.Save();
            // tokenStream.Advance();

            LightList<ASTNode> parts = LightList<ASTNode>.Get();
            tokenStream.Save();

            while (tokenStream.HasMoreTokens) {
                if (tokenStream.Current == ExpressionTokenType.Dot || tokenStream.Current == ExpressionTokenType.Elvis) {
                    if (tokenStream.Next != ExpressionTokenType.Identifier) {
                        break;
                    }

                    tokenStream.Advance();
                    parts.Add(ASTNode.DotAccessNode(tokenStream.Current.value, tokenStream.Previous == ExpressionTokenType.Elvis));
                    tokenStream.Advance();
                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }
                else if (tokenStream.Current == ExpressionTokenType.ArrayAccessOpen || tokenStream.Current == ExpressionTokenType.QuestionMark && tokenStream.NextTokenIs(ExpressionTokenType.ArrayAccessOpen)) {
                    bool isElvis = false;
                    if (tokenStream.Current == ExpressionTokenType.QuestionMark) {
                        isElvis = true;
                        tokenStream.Advance();
                    }

                    int advance = tokenStream.FindMatchingIndex(ExpressionTokenType.ArrayAccessOpen, ExpressionTokenType.ArrayAccessClose);
                    if (advance == -1) {
                        Abort("Unmatched array bracket");
                    }

                    ExpressionParser subParser = CreateSubParser(advance);
                    parts.Add(ASTNode.IndexExpressionNode(subParser.ParseLoop(), isElvis));
                    subParser.Release(true);
                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }
                else if (tokenStream.Current == ExpressionTokenType.ParenOpen) {
                    LightList<ASTNode> parameters = null;

                    if (!ParseListExpression(ref parameters, ExpressionTokenType.ParenOpen, ExpressionTokenType.ParenClose)) {
                        Abort();
                    }

                    parts.Add(ASTNode.InvokeNode(parameters));
                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }

                else if (tokenStream.Current == ExpressionTokenType.LessThan) {
                    // shortcut the << operator since we can't have a << in a generic type node. List<<string>> is invalid for example
                    if (tokenStream.HasMoreTokens && tokenStream.Next == ExpressionTokenType.LessThan) {
                        tokenStream.Restore();
                        LightList<ASTNode>.Release(ref parts);
                        return false;
                    }

                    TypeLookup typePath = new TypeLookup();

                    if (!(ParseTypePathGenerics(ref typePath))) {
                        tokenStream.Restore();
                        LightList<ASTNode>.Release(ref parts);
                        return false;
                    }

                    parts.Add(ASTNode.GenericTypePath(typePath));
                    if (tokenStream.HasMoreTokens) {
                        continue;
                    }
                }

                if (parts.Count == 0) {
                    tokenStream.Restore();
                    LightList<ASTNode>.Release(ref parts);
                    return false;
                }

                retn = ASTNode.MemberAccessExpressionNode("__parens__", parts).WithLocation(tokenStream.Peek());
                return true;
            }

            ReleaseList(parts);
            tokenStream.Restore();
            return false;
        }

        private bool ParseNewExpression(ref ASTNode retn) {
            if (tokenStream.Current != ExpressionTokenType.New) {
                return false;
            }

            tokenStream.Save();
            tokenStream.Advance();
            TypeLookup typeLookup = new TypeLookup(); // todo -- allocates a list :(
            bool valid = ParseTypePath(ref typeLookup);

            if (!valid || tokenStream.Current != ExpressionTokenType.ParenOpen) {
                typeLookup.Release();
                tokenStream.Restore();
                return false;
            }

            LightList<ASTNode> parameters = null;

            if (!ParseListExpression(ref parameters, ExpressionTokenType.ParenOpen, ExpressionTokenType.ParenClose)) {
                Abort();
            }

            retn = ASTNode.NewExpressionNode(typeLookup, parameters);

            return true;
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
            TypeLookup typeLookup = new TypeLookup();
            bool valid = subParser.ParseTypePath(ref typeLookup);
            subParser.Release(true);

            if (!valid) {
                tokenStream.Restore();
                return false;
            }

            if (!ParseExpression(ref expression)) {
                typeLookup.Release();
                tokenStream.Restore();
                tokenStream.Restore();
                return false;
            }

            retn = ASTNode.DirectCastNode(typeLookup, expression);
            return true;
        }

        private bool ParseOutExpression(ref ASTNode retn) {

            if (tokenStream.Current != ExpressionTokenType.Out) {
                return false;
            }

            tokenStream.Save();
            tokenStream.Advance();

            ASTNode expression = ParseLoop();
            if (expression != null) {
                retn = new OutNode(expression);
                return true;
            }

            tokenStream.Restore();
            return false;
        }

        private bool ParseListExpressionStep(ref LightList<ASTNode> retn) {
            while (true) {
                int commaIndex = tokenStream.FindNextIndexAtSameLevel(ExpressionTokenType.Comma);
                if (commaIndex != -1) {
                    ExpressionParser parser = CreateUndelimitedSubParser(commaIndex);
                    tokenStream.Advance();
                    bool valid = parser.ParseListExpressionStep(ref retn);
                    parser.Release(true);
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

        private bool ParseListExpression(ref LightList<ASTNode> retn, ExpressionTokenType openExpressionToken, ExpressionTokenType closeExpressionToken) {
            if (tokenStream.Current != openExpressionToken) {
                return false;
            }

            int range = tokenStream.FindMatchingIndex(openExpressionToken, closeExpressionToken);
            tokenStream.Save();

            if (range == 1) {
                tokenStream.Advance(2);
                retn = LightList<ASTNode>.Get();
                return true;
            }

            if (retn != null) {
                LightList<ASTNode>.Release(ref retn);
            }

            retn = LightList<ASTNode>.Get();
            //todo find next comma at same level (meaning not inside [ or ( or <

            ExpressionParser parser = CreateSubParser(range);
            bool valid = parser.ParseListExpressionStep(ref retn);
            parser.Release(false);

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
                    // todo -- allow a type expression a-la default(List<float>);
                    retn = ASTNode.DefaultLiteralNode(tokenStream.Current.value).WithLocation(tokenStream.Current);
                    break;

                default:
                    return false;
            }

            tokenStream.Advance();
            return true;
        }

        private void Abort(string info = null) {
            if (info != null) {
                throw new ExpressionParseException($"Failed to parse expression: {tokenStream.PrintTokens()}. {info}");
            }
            else {
                throw new ExpressionParseException($"Failed to parse expression: {tokenStream.PrintTokens()}");
            }
        }

        private static void ReleaseList(LightList<ASTNode> list) {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++) {
                list[i].Release();
            }

            LightList<ASTNode>.Release(ref list);
        }

        public int GetTokenPosition() {
            return tokenStream.CurrentIndex;
        }

    }

    internal class BlockNode : ASTNode {

        public LightList<ASTNode> blockExpressions;

        public BlockNode(LightList<ASTNode> blockExpressions) {
            this.blockExpressions = blockExpressions;
        }

        public override void Release() {
            for (int i = 0; i < blockExpressions.size; i++) {
                blockExpressions[i].Release();
            }
        }

    }

}