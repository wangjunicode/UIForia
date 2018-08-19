using System;
using System.Collections.Generic;
using NUnit.Framework;
using Src;


    [TestFixture]
    public class TokenizingTests {

        [Test]
        public void TokenizeBasicString() {
            string input = "item.thing";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual("item", tokens[0].value);
            Assert.AreEqual(".", tokens[1].value);
            Assert.AreEqual("thing", tokens[2].value);
        }

        [Test]
        public void Tokenize_Boolean() {
            string input = "true";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("true", tokens[0].value);

            input = "false";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("false", tokens[0].value);
        }

        [Test]
        public void Tokenize_Number() {
            string input = "6264.1";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("6264.1", tokens[0].value);
            Assert.AreEqual(TokenType.Number, tokens[0].tokenType);

            input = "-6264.1";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual("-", tokens[0].value);
            Assert.AreEqual("6264.1", tokens[1].value);
            Assert.AreEqual(TokenType.Minus, tokens[0].tokenType);
            Assert.AreEqual(TokenType.Number, tokens[1].tokenType);
            
            input = "-6264";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual("-", tokens[0].value);
            Assert.AreEqual("6264", tokens[1].value);
            Assert.AreEqual(TokenType.Minus, tokens[0].tokenType);
            Assert.AreEqual(TokenType.Number, tokens[1].tokenType);
            
            input = "-6264f";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual("-", tokens[0].value);
            Assert.AreEqual("6264f", tokens[1].value);
            Assert.AreEqual(TokenType.Minus, tokens[0].tokenType);
            Assert.AreEqual(TokenType.Number, tokens[1].tokenType);
            
            input = "-6264.414f ";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual("-", tokens[0].value);
            Assert.AreEqual("6264.414f", tokens[1].value);
            Assert.AreEqual(TokenType.Minus, tokens[0].tokenType);
            
            Assert.AreEqual(TokenType.Number, tokens[1].tokenType);
            input = "6264";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("6264", tokens[0].value);
            Assert.AreEqual(TokenType.Number, tokens[0].tokenType);
        }

        [Test]
        public void Tokenize_String() {
            string input = "'some string'";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("'some string'", tokens[0].value);
        }

        [Test]
        public void Tokenize_Operators() {
            string input = "+";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Plus, tokens[0].tokenType);
            
            input = "-";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Minus, tokens[0].tokenType);
            
            input = "*";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Times, tokens[0].tokenType);
            
            input = "/";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Divide, tokens[0].tokenType);
            
            input = "%";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Mod, tokens[0].tokenType);
        }

        [Test]
        public void Tokenize_Conditionals() {
            string input = "&&";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.And, tokens[0].tokenType);
            
            input = "||";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Or, tokens[0].tokenType);
            
            input = "==";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Equals, tokens[0].tokenType);
            
            input = "!=";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.NotEquals, tokens[0].tokenType);
            
            input = ">";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.GreaterThan, tokens[0].tokenType);
            
            input = "<";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.LessThan, tokens[0].tokenType);
            
            input = ">=";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.GreaterThanEqualTo, tokens[0].tokenType);
            
            input = "<=";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.LessThanEqualTo, tokens[0].tokenType);
            
            input = "!";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Not, tokens[0].tokenType);
        }

        [Test]
        public void Tokenize_ArrayAccess() {
            string input = "[";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.ArrayAccessOpen, tokens[0].tokenType);
            
            input = "]";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.ArrayAccessClose, tokens[0].tokenType);
        }

        [Test]
        public void Tokenize_ExpressionStatement() {
            string input = "{";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.ExpressionOpen, tokens[0].tokenType);
            
            input = "}";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.ExpressionClose, tokens[0].tokenType);
        }

       
        [Test]
        public void Tokenize_CompoundOperatorExpression() {
            string input = "52 + 2.4";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(TokenType.Number, tokens[0].tokenType);
            Assert.AreEqual(TokenType.Plus, tokens[1].tokenType);
            Assert.AreEqual(TokenType.Number, tokens[2].tokenType);
            
            input = "-52 * 714";
            tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(4, tokens.Count);
            Assert.AreEqual(TokenType.Minus, tokens[0].tokenType);
            Assert.AreEqual(TokenType.Number, tokens[1].tokenType);
            Assert.AreEqual(TokenType.Times, tokens[2].tokenType);
            Assert.AreEqual(TokenType.Number, tokens[3].tokenType);
        }

        [Test]
        public void Tokenize_CompoundPropertyAccess() {
            string input = "366 + something.first.second.third";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(9, tokens.Count);
            Assert.AreEqual(TokenType.Number, tokens[0].tokenType);
            Assert.AreEqual(TokenType.Plus, tokens[1].tokenType);
            Assert.AreEqual(TokenType.Identifier, tokens[2].tokenType);
            Assert.AreEqual(TokenType.Dot, tokens[3].tokenType);
            Assert.AreEqual(TokenType.Identifier, tokens[4].tokenType);
            Assert.AreEqual(TokenType.Dot, tokens[5].tokenType);
            Assert.AreEqual(TokenType.Identifier, tokens[6].tokenType);
            Assert.AreEqual(TokenType.Dot, tokens[7].tokenType);
            Assert.AreEqual(TokenType.Identifier, tokens[8].tokenType);
        }

        [Test]
        public void Tokenize_CompoundArrayAccess() {
            string input = "366 + something[first]second.third";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            Assert.AreEqual(9, tokens.Count);
            Assert.AreEqual(TokenType.Number, tokens[0].tokenType);
            Assert.AreEqual(TokenType.Plus, tokens[1].tokenType);
            Assert.AreEqual(TokenType.Identifier, tokens[2].tokenType);
            Assert.AreEqual(TokenType.ArrayAccessOpen, tokens[3].tokenType);
            Assert.AreEqual(TokenType.Identifier, tokens[4].tokenType);
            Assert.AreEqual(TokenType.ArrayAccessClose, tokens[5].tokenType);
            Assert.AreEqual(TokenType.Identifier, tokens[6].tokenType);
            Assert.AreEqual(TokenType.Dot, tokens[7].tokenType);
            Assert.AreEqual(TokenType.Identifier, tokens[8].tokenType);
        }

       
        
        [Test]
        public void Tokenize_ComplexUnary() {
            string input = "item != 55 && !someCondition || -(11 * 4)";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            List<TokenType> types = new List<TokenType>();
            
            types.Add(TokenType.Identifier);
            types.Add(TokenType.NotEquals);
            types.Add(TokenType.Number);
            types.Add(TokenType.And);
            types.Add(TokenType.Not);
            types.Add(TokenType.Identifier);
            types.Add(TokenType.Or);
            types.Add(TokenType.Minus);
            types.Add(TokenType.ParenOpen);
            types.Add(TokenType.Number);
            types.Add(TokenType.Times);
            types.Add(TokenType.Number);
            types.Add(TokenType.ParenClose);
            
            AssertTokenTypes(types, tokens);

        }

        [Test]
        public void Tokenize_SpecialIdentifier() {
            string input = "1 + $ident";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            List<TokenType> types = new List<TokenType>();
            types.Add(TokenType.Number);
            types.Add(TokenType.Plus);
            types.Add(TokenType.SpecialIdentifier);
            
            AssertTokenTypes(types, tokens);
        }

        [Test]
        public void Tokenize_Comma() {
            string input = "method(1, 2, 3)";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            List<TokenType> types = new List<TokenType>();
            types.Add(TokenType.Identifier);
            types.Add(TokenType.ParenOpen);
            types.Add(TokenType.Number);
            types.Add(TokenType.Comma);
            types.Add(TokenType.Number);
            types.Add(TokenType.Comma);
            types.Add(TokenType.Number);
            types.Add(TokenType.ParenClose);
            
            AssertTokenTypes(types, tokens);
        }

        [Test]
        public void Tokenize_Ternary() {
            string input = "value ? 1 : 2";
            List<DslToken> tokens = Tokenizer.Tokenize(input);
            List<TokenType> types = new List<TokenType>();
            types.Add(TokenType.Identifier);
            types.Add(TokenType.QuestionMark);
            types.Add(TokenType.Number);
            types.Add(TokenType.Colon);
            types.Add(TokenType.Number);
            
            AssertTokenTypes(types, tokens);
        }
        
        [Test]
        public void FailsToTokenizeUnterminatedString() {
            Assert.Throws<Exception>(() => {
                Tokenizer.Tokenize("'havelstring");
            });
        }
        
        private static void AssertTypesAndValues(List<DslToken> expectedTokens, List<DslToken> actualTokens) {
            Assert.AreEqual(expectedTokens.Count, actualTokens.Count);
            for (int i = 0; i < actualTokens.Count; i++) {
                Assert.AreEqual(expectedTokens[i].tokenType, actualTokens[i].tokenType);
                Assert.AreEqual(expectedTokens[i].value, actualTokens[i].value);
            } 
        }
        
        private static void AssertTokenTypes(List<TokenType> types, List<DslToken> tokens) {
            Assert.AreEqual(types.Count, tokens.Count);
            for (int i = 0; i < types.Count; i++) {
                Assert.AreEqual(tokens[i].tokenType, types[i]);
            }
        }
    }

