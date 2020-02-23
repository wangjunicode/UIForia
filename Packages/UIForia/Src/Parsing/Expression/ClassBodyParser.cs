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

        public ASTNode Parse(string input, string fileName, int lineStart) {
            
            tokenStream = new TokenStream(ExpressionTokenizer.Tokenize(input, StructList<ExpressionToken>.Get()));
            
            if (!tokenStream.HasMoreTokens) {
                throw new ParseException("Failed trying to parse empty expression");
            }

            while (tokenStream.HasMoreTokens) {
                ExpressionToken current = tokenStream.Current;

                ASTNode node = null;
                if (ParseFieldDefinition(ref node)) {
                    continue;
                }
                
                    
                if (current == tokenStream.Current) {
                    break;
                }
            }

            return null;

        }

        private bool ParseMethodDefinition(ref ASTNode node) {
            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                return false;
            }

            // modifiers? -> returnType -> name -> signature -> openBrace * closeBrace
            
            tokenStream.Save();
           
            ExpressionParser parser = new ExpressionParser(tokenStream);

            TypeLookup typeLookup = default;
            if (!parser.ParseTypePath(ref typeLookup)) {
                tokenStream.Restore();
                parser.Release(false);
                return false;
            }
            
            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                typeLookup.Release();
                tokenStream.Restore();
                return false;
            }

            string name = tokenStream.Current.value;
            
            tokenStream.Advance();

            if (!tokenStream.NextTokenIs(ExpressionTokenType.ParenOpen)) {
                typeLookup.Release();
                tokenStream.Restore();
                return false;
            }
            
            StructList<LambdaArgument> signature = StructList<LambdaArgument>.Get();

            if (!ExpressionParser.ParseSignature(tokenStream, signature)) {
                tokenStream.Restore();
                typeLookup.Release();
                signature?.Release();
                return false;
            }

            if (tokenStream.Current != ExpressionTokenType.ExpressionOpen) {
                tokenStream.Restore();
                typeLookup.Release();
                signature?.Release();
                return false;
            }

            int matchingIndex = tokenStream.FindMatchingIndex(ExpressionTokenType.ExpressionOpen, ExpressionTokenType.ExpressionClose);

            if (matchingIndex == -1) {
                tokenStream.Restore();
                typeLookup.Release();
                signature?.Release();
                return false;
            }

            TokenStream subStream = tokenStream.AdvanceAndReturnSubStream(matchingIndex);
            
            MethodNode retn = new MethodNode();
            retn.tokens = subStream;
            retn.returnTypeLookup = typeLookup;
            retn.name = name;
            node = retn;
            return true;
        }

        private bool ParseFieldDefinition(ref ASTNode node) {
            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                return false;
            }
            // todo -- support attribute parsing
            // todo -- support prefixes (require, public, private, others?)
            
            // type -> name -> semicolon
            
            tokenStream.Save();
            
            ExpressionParser parser = new ExpressionParser(tokenStream);

            TypeLookup typeLookup = default;
            if (!parser.ParseTypePath(ref typeLookup)) {
                tokenStream.Restore();
                parser.Release(false);
                return false;
            }

            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                typeLookup.Release();
                tokenStream.Restore();
                return false;
            }

            string name = tokenStream.Current.value;
            
            tokenStream.Advance();

            if (tokenStream.Current != ExpressionTokenType.SemiColon) {
                typeLookup.Release();
                tokenStream.Restore();
                return false;
            }

            FieldNode retn = new FieldNode() {
                name = name,
                typeLookup = typeLookup
            };
            
            node = retn;
            return true;

        }
        
    }

    public class FieldNode : ASTNode {

        // todo -- attribute list
        public TypeLookup typeLookup;
        public string name;
        
        public override void Release() {
            typeLookup.Release();
        }

    }
    
    public class MethodNode : ASTNode {

        public TypeLookup returnTypeLookup;
        public StructList<LambdaArgument> signatureList;
        public string name;

        public TokenStream tokens;
        
        // todo -- modifier list
        // todo -- attribute list
        
        public override void Release() {
            returnTypeLookup.Release();
            
            if (signatureList == null) return;
            
            for (int i = 0; i < signatureList.size; i++) {
                signatureList.array[i].type?.Release();
            }
        }

    }
    

}