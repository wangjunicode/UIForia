using System.Collections.Generic;
using UIForia.Exceptions;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Parsing.Expressions.Tokenizer;
using UIForia.Util;

namespace UIForia.Parsing.Expressions {

    public class ClassBodyParser {

        private TokenStream tokenStream;
        private Stack<ASTNode> expressionStack;
        private Stack<OperatorNode> operatorStack;

        private static readonly LambdaArgument[] s_EmptySignature = { };

        public TypeBodyNode Parse(string input, string fileName, int lineStart) {

            tokenStream = new TokenStream(ExpressionTokenizer.Tokenize(input, StructList<ExpressionToken>.Get()));

            if (!tokenStream.HasMoreTokens) {
                throw new ParseException("Failed trying to parse empty expression");
            }

            TypeBodyNode retn = new TypeBodyNode();

            while (tokenStream.HasMoreTokens) {
                ExpressionToken current = tokenStream.Current;

                ASTNode node = null;

                if (ParseDeclaration(ref node)) {
                    retn.nodes.Add(node);
                    continue;
                }

                if (current == tokenStream.Current) {
                    throw new ParseException($"Failed to parse {fileName}. Got stuck on {current.value}");
                }
            }

            return retn;

        }

        private bool ParseAttribute(ref AttributeNode node) {
            if (tokenStream.Current != ExpressionTokenType.ArrayAccessOpen) {
                return false;
            }

            tokenStream.Save();
            tokenStream.Advance();
            TypeLookup typeLookup = default;
            ExpressionParser parser = new ExpressionParser(tokenStream);

            if (!parser.ParseTypePath(ref typeLookup)) {
                goto fail;
            }

            tokenStream.Set(parser.GetTokenPosition());
            parser.Release(false);

            if (tokenStream.Current == ExpressionTokenType.ArrayAccessClose) {
                tokenStream.Advance();
                node = new AttributeNode() {
                    typeLookup = typeLookup
                };
                return true;
            }

            fail:
            {
                typeLookup.Release();
                parser.Release(false);
                return false;
            }
        }

        private bool ParseDeclaration(ref ASTNode node) {

            AttributeNode attrNode = null;
            LightList<AttributeNode> attributes = LightList<AttributeNode>.Get();
            while (ParseAttribute(ref attrNode)) {
                attributes.Add(attrNode);
                if (tokenStream.Current != ExpressionTokenType.ArrayAccessOpen) {
                    break;
                }
            }

            if (attributes.size == 0) {
                LightList<AttributeNode>.Release(ref attributes);
            }

            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                return false;
            }

            // modifiers? -> returnType -> name -> signature -> openBrace * closeBrace

            tokenStream.Save();

            ExpressionParser parser = new ExpressionParser(tokenStream);
            StructList<LambdaArgument> signature = null;
            TypeLookup typeLookup = default;

            if (!parser.ParseTypePath(ref typeLookup)) {
                goto fail;
            }

            tokenStream.Set(parser.GetTokenPosition());
            parser.Release(false);

            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                goto fail;
            }

            string name = tokenStream.Current.value;

            tokenStream.Advance();

            // if semi colon then we have a field!
            if (tokenStream.Current == ExpressionTokenType.SemiColon) {
                tokenStream.Advance();
                node = new FieldNode() {
                    name = name,
                    attributes = attributes,
                    typeLookup = typeLookup
                };
                return true;
            }

            if (tokenStream.Current != ExpressionTokenType.ParenOpen) {
                goto fail;
            }

            signature = StructList<LambdaArgument>.Get();

            if (tokenStream.NextTokenIs(ExpressionTokenType.ParenClose)) {
                tokenStream.Advance(2);
            }
            else {
                int matchingIndex = tokenStream.FindMatchingIndex(ExpressionTokenType.ParenOpen, ExpressionTokenType.ParenClose);

                if (matchingIndex == -1) {
                    goto fail;
                }

                TokenStream subStream = tokenStream.AdvanceAndReturnSubStream(matchingIndex);
                subStream.Advance();
                tokenStream.Advance();
                if (!ExpressionParser.ParseSignature(subStream, signature)) {
                    goto fail;
                }

                for (int i = 0; i < signature.size; i++) {
                    if (signature.array[i].type == null) {
                        throw new ParseException($"When defining a method you must specify a type for all arguments. Found identifier {signature.array[i].identifier} but no type was given.");
                    }
                }
            }

            if (tokenStream.Current != ExpressionTokenType.ExpressionOpen) {
                goto fail;
            }

            int expressionMatch = tokenStream.FindMatchingIndex(ExpressionTokenType.ExpressionOpen, ExpressionTokenType.ExpressionClose);

            if (expressionMatch == -1) {
                goto fail;
            }

            TokenStream bodyStream = tokenStream.AdvanceAndReturnSubStream(expressionMatch);
            tokenStream.Advance(); // stop over expression close
            bodyStream.Advance(); // might be an issue with token stream advance and return
            node = new MethodNode() {
                tokens = bodyStream,
                returnTypeLookup = typeLookup,
                attributes = attributes,
                name = name,
                signatureList = signature != null ? signature.ToArray() : s_EmptySignature
            };

            StructList<LambdaArgument>.Release(ref signature);
            parser.Release(false);

            return true;

            fail:
            {
                tokenStream.Restore();
                parser.Release(false);
                typeLookup.Release();
                signature?.Release();
                return false;
            }
        }

    }

    public class TypeBodyNode : ASTNode {

        public LightList<ASTNode> nodes = new LightList<ASTNode>();

        public override void Release() {
            for (int i = 0; i < nodes.size; i++) {
                nodes[i].Release();
            }
        }

    }

    public class AttributeNode : ASTNode {

        public TypeLookup typeLookup;

        public override void Release() {
            typeLookup.Release();
        }

    }

    public abstract class DeclarationNode : ASTNode {

        public string name;
        public LightList<AttributeNode> attributes;

        public override void Release() {
            if (attributes == null) return;
            for (int i = 0; i < attributes.size; i++) {
                attributes[i].Release();
            }
        }

    }

    public class FieldNode : DeclarationNode {

        public TypeLookup typeLookup;

        public override void Release() {
            base.Release();
            typeLookup.Release();
        }

    }

    public class MethodNode : DeclarationNode {

        public TypeLookup returnTypeLookup;
        public LambdaArgument[] signatureList;
        public TokenStream tokens;

        // todo -- modifier list

        public override void Release() {
            base.Release();
            returnTypeLookup.Release();

            if (signatureList == null) return;

            for (int i = 0; i < signatureList.Length; i++) {
                signatureList[i].type?.Release();
            }
        }

    }

}