using System;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Src;
using UnityEngine;

namespace Tests {
    [TestFixture]
    public class BindingParserTests {

        [Test]
        public void ParseConstantValueRoot() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("5"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<NumericConstantNode>(root);
            NumericConstantNode n = (NumericConstantNode) root;
            Assert.AreEqual(5.0, n.value);

            // todo -- re think handling of negative constants
//            stream = new TokenStream(Tokenizer.Tokenize("-5.6"));
//            parser = new ExpressionParser(stream);
//            root = parser.Parse();
//            Assert.IsInstanceOf<ConstantExpressionNode>(root);
//            n = (ConstantExpressionNode) root;
//            Assert.AreEqual(-5.6, ((NumericConstantNode)n.valueNode).value);
            
            stream = new TokenStream(Tokenizer.Tokenize("'hello'"));
            parser = new ExpressionParser(stream);
            root = parser.Parse();
            Assert.IsInstanceOf<StringConstantNode>(root);
            StringConstantNode n2 = (StringConstantNode) root;
            Assert.AreEqual("hello", n2.value);
            
            stream = new TokenStream(Tokenizer.Tokenize("'hello ' "));
            parser = new ExpressionParser(stream);
            root = parser.Parse();
            Assert.IsInstanceOf<StringConstantNode>(root);
            n2 = (StringConstantNode) root;
            Assert.AreEqual("hello ", n2.value);
            
            stream = new TokenStream(Tokenizer.Tokenize("true"));
            parser = new ExpressionParser(stream);
            root = parser.Parse();
            Assert.IsInstanceOf<BooleanConstantNode>(root);
            BooleanConstantNode n3 = (BooleanConstantNode) root;
            Assert.AreEqual(true, n3.value);
            
            stream = new TokenStream(Tokenizer.Tokenize(" false"));
            parser = new ExpressionParser(stream);
            root = parser.Parse();
            Assert.IsInstanceOf<BooleanConstantNode>(root);
            n3 = (BooleanConstantNode) root;
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
            Assert.AreEqual(opEx.op.op, OperatorType.Plus);
            Assert.IsInstanceOf<ConstantValueNode>(opEx.left);
            Assert.IsInstanceOf<ConstantValueNode>(opEx.right);
        }
        
        [Test]
        public void Parse_OperatorExpression_MultipleConstants() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("5 + 6 * 7"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpression>(root);
            OperatorExpression opEx = (OperatorExpression) root;
            Assert.IsInstanceOf<OperatorNode>(opEx.op);
            Assert.AreEqual(opEx.op.op, OperatorType.Plus);
            Assert.IsInstanceOf<ConstantValueNode>(opEx.left);
            Assert.IsInstanceOf<OperatorExpression>(opEx.right);
            opEx = (OperatorExpression)opEx.right;
            Assert.AreEqual(opEx.op.op, OperatorType.Times);
            Assert.IsInstanceOf<ConstantValueNode>(opEx.left);
            Assert.IsInstanceOf<ConstantValueNode>(opEx.right);
        }

        [Test]
        public void Parse_RootContextIdentifier() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("someIdentifier"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<RootContextLookup>(root);
            stream = new TokenStream(Tokenizer.Tokenize(" someIdentifier "));
            parser = new ExpressionParser(stream);
            root = parser.Parse();
            Assert.IsInstanceOf<RootContextLookup>(root);
        }

        [Test]
        public void Parse_ContextLookupOperatorExpression() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("someIdentifier + 735.2"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpression>(root);
            OperatorExpression opEx = (OperatorExpression) root;
            Assert.AreEqual(opEx.op.op, OperatorType.Plus);
            Assert.IsInstanceOf<RootContextLookup>(opEx.left);
            Assert.IsInstanceOf<ConstantValueNode>(opEx.right);
        }

        [Test]
        public void Parse_ExpressionWithOuterParens() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("(someIdentifier + 735.2)"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpression>(root);
            OperatorExpression opEx = (OperatorExpression) root;
            Assert.AreEqual(opEx.op.op, OperatorType.Plus);
            Assert.IsInstanceOf<RootContextLookup>(opEx.left);
            Assert.IsInstanceOf<ConstantValueNode>(opEx.right);
        }
        
        [Test]
        public void Parse_ExpressionWithInnerParens() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("someIdentifier + (735.2)"));
            ExpressionParser parser = new ExpressionParser(stream);
            ASTNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpression>(root);
            OperatorExpression opEx = (OperatorExpression) root;
            Assert.AreEqual(opEx.op.op, OperatorType.Plus);
            Assert.IsInstanceOf<RootContextLookup>(opEx.left);
            Assert.IsInstanceOf<ConstantValueNode>(opEx.right);
        }

        [Test]
        public void Parse_ExpressionWithSignificantParens() {
            
        }
    }
}