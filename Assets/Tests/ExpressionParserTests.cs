using System;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Src;
using UnityEngine;

namespace Tests {
    [TestFixture]
    public class ExpressionParserTests {

        [Test]
        public void ParseConstantValueRoot() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("5"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<NumericLiteralNode>(root);
            NumericLiteralNode n = (NumericLiteralNode) root;
            Assert.AreEqual(5.0, n.value);

            stream = new TokenStream(Tokenizer.Tokenize("-5.6"));
            parser = new ExpressionParser(stream);
            root = parser.Parse();
            Assert.IsInstanceOf<NumericLiteralNode>(root);
            n = (NumericLiteralNode) root;
            Assert.AreEqual(-5.6, n.value);
            
            stream = new TokenStream(Tokenizer.Tokenize("'hello'"));
            parser = new ExpressionParser(stream);
            root = parser.Parse();
            Assert.IsInstanceOf<StringLiteralNode>(root);
            StringLiteralNode n2 = (StringLiteralNode) root;
            Assert.AreEqual("hello", n2.value);
            
            stream = new TokenStream(Tokenizer.Tokenize("'hello ' "));
            parser = new ExpressionParser(stream);
            root = parser.Parse();
            Assert.IsInstanceOf<StringLiteralNode>(root);
            n2 = (StringLiteralNode) root;
            Assert.AreEqual("hello ", n2.value);
            
            stream = new TokenStream(Tokenizer.Tokenize("true"));
            parser = new ExpressionParser(stream);
            root = parser.Parse();
            Assert.IsInstanceOf<BooleanLiteralNode>(root);
            BooleanLiteralNode n3 = (BooleanLiteralNode) root;
            Assert.AreEqual(true, n3.value);
            
            stream = new TokenStream(Tokenizer.Tokenize(" false"));
            parser = new ExpressionParser(stream);
            root = parser.Parse();
            Assert.IsInstanceOf<BooleanLiteralNode>(root);
            n3 = (BooleanLiteralNode) root;
            Assert.AreEqual(false, n3.value);
        }

        [Test]
        public void Parse_OperatorExpression_Constants() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("5 + 6"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpression>(root);
            OperatorExpression opEx = (OperatorExpression) root;
            Assert.IsInstanceOf<OperatorNode>(opEx.op);
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.left);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.right);
        }
        
        [Test]
        public void Parse_OperatorExpression_MultipleConstants() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("5 + 6 * 7"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpression>(root);
            OperatorExpression opEx = (OperatorExpression) root;
            Assert.IsInstanceOf<OperatorNode>(opEx.op);
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.left);
            Assert.IsInstanceOf<OperatorExpression>(opEx.right);
            opEx = (OperatorExpression)opEx.right;
            Assert.AreEqual(opEx.op.OpType, OperatorType.Times);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.left);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.right);
        }

        [Test]
        public void Parse_RootContextIdentifier() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("{someIdentifier}"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<RootContextLookup>(root);
            stream = new TokenStream(Tokenizer.Tokenize("{ someIdentifier }"));
            parser = new ExpressionParser(stream);
            root = parser.Parse();
            Assert.IsInstanceOf<RootContextLookup>(root);
        }

        [Test]
        public void Parse_ContextLookupOperatorExpression() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("{someIdentifier + 735.2}"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpression>(root);
            OperatorExpression opEx = (OperatorExpression) root;
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<RootContextLookup>(opEx.left);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.right);
        }

        [Test]
        public void Parse_ExpressionWithOuterParens() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("{ (someIdentifier + 735.2) }"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<ParenOperatorNode>(root);
            ParenOperatorNode parenNode = (ParenOperatorNode) root;
            Assert.IsInstanceOf<OperatorExpression>(parenNode.expression);
            OperatorExpression opNode = (OperatorExpression) parenNode.expression;
            Assert.IsInstanceOf<RootContextLookup>(opNode.left);
            Assert.IsInstanceOf<NumericLiteralNode>(opNode.right);
        }    
        
        [Test]
        public void Parse_ExpressionWithInnerParens() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("{someIdentifier + (735.2)}"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpression>(root);
            OperatorExpression opEx = (OperatorExpression) root;
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<RootContextLookup>(opEx.left);
            Assert.IsInstanceOf<ParenOperatorNode>(opEx.right);
        }

        [Test]
        public void Parse_OperatorLiteralExpression_AddNumbers() {
            ExpressionParser parser = new ExpressionParser("4 + 735.2");
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpression>(root);
            OperatorExpression opEx = (OperatorExpression) root;
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.left);
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.right);
        }

        [Test]
        public void Parse_NestedParenAddition() {
            ExpressionParser parser = new ExpressionParser("(12 + (4 * 3)) * 2");
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpression>(root);
            OperatorExpression opEx = (OperatorExpression) root;
            Assert.IsInstanceOf<ParenOperatorNode>(opEx.left);
            Assert.IsInstanceOf<NumericLiteralExpression>(opEx.right);
        }
        
//        [Test]
//        public void Parse_ExpressionWithSignificantParens() {
//            
//        }
    }
}