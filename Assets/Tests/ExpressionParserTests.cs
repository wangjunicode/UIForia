using NUnit.Framework;
using Src;

namespace Tests {
    [TestFixture]
    public class ExpressionParserTests {

        [Test]
        public void ParseLiteralValueRoot() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("5"));
            ExpressionParser parser = new ExpressionParser(stream);
            ExpressionNode root = parser.Parse();
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
        public void Parse_OperatorExpression_Literals() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("5 + 6"));
            ExpressionParser parser = new ExpressionParser(stream);
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
            Assert.IsInstanceOf<OperatorNode>(opEx.op);
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.left);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.right);
        }
        
        [Test]
        public void Parse_OperatorExpression_MultipleLiterals() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("5 + 6 * 7"));
            ExpressionParser parser = new ExpressionParser(stream);
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
            Assert.IsInstanceOf<OperatorNode>(opEx.op);
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.left);
            Assert.IsInstanceOf<OperatorExpressionNode>(opEx.right);
            opEx = (OperatorExpressionNode)opEx.right;
            Assert.AreEqual(opEx.op.OpType, OperatorType.Times);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.left);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.right);
        }

        [Test]
        public void Parse_RootContextIdentifier() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("{someIdentifier}"));
            ExpressionParser parser = new ExpressionParser(stream);
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<RootContextLookupNode>(root);
            stream = new TokenStream(Tokenizer.Tokenize("{ someIdentifier }"));
            parser = new ExpressionParser(stream);
            root = parser.Parse();
            Assert.IsInstanceOf<RootContextLookupNode>(root);
        }

        [Test]
        public void Parse_ContextLookupOperatorExpression() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("{someIdentifier + 735.2}"));
            ExpressionParser parser = new ExpressionParser(stream);
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<RootContextLookupNode>(opEx.left);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.right);
        }

        [Test]
        public void Parse_ExpressionWithOuterParens() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("{ (someIdentifier + 735.2) }"));
            ExpressionParser parser = new ExpressionParser(stream);
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<ParenOperatorNode>(root);
            ParenOperatorNode parenNode = (ParenOperatorNode) root;
            Assert.IsInstanceOf<OperatorExpressionNode>(parenNode.expression);
            OperatorExpressionNode opNode = (OperatorExpressionNode) parenNode.expression;
            Assert.IsInstanceOf<RootContextLookupNode>(opNode.left);
            Assert.IsInstanceOf<NumericLiteralNode>(opNode.right);
        }    
        
        [Test]
        public void Parse_ExpressionWithInnerParens() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("{someIdentifier + (735.2)}"));
            ExpressionParser parser = new ExpressionParser(stream);
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<RootContextLookupNode>(opEx.left);
            Assert.IsInstanceOf<ParenOperatorNode>(opEx.right);
        }

        [Test]
        public void Parse_SingleExpressionInParens() {
            TokenStream stream = new TokenStream(Tokenizer.Tokenize("(1)"));
            ExpressionParser parser = new ExpressionParser(stream);
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<ParenOperatorNode>(root);
            ParenOperatorNode opEx = (ParenOperatorNode) root;
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.expression);
        }
        
        [Test]
        public void Parse_OperatorLiteralExpression_AddNumbers() {
            ExpressionParser parser = new ExpressionParser("4 + 735.2");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.left);
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.right);
        }

        [Test]
        public void Parse_NestedParenAddition() {
            ExpressionParser parser = new ExpressionParser("(12 + (4 * 3)) * 2");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
            Assert.IsInstanceOf<ParenOperatorNode>(opEx.left);
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.right);
        }

        [Test]
        public void Parse_PropertyAccess_RootContext_Level1() {
            ExpressionParser parser = new ExpressionParser("{rootContext.property}");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<AccessExpressionNode>(root);
            AccessExpressionNode node = (AccessExpressionNode) root;
            Assert.AreEqual("rootContext", node.identifierNode.identifier);
            Assert.AreEqual(1, node.parts.Count);
            Assert.IsInstanceOf<AccessExpressionPartNode>(node.parts[0]);
        }

        [Test]
        public void Parse_ArrayAccess_RootContext_Level1() {
            ExpressionParser parser = new ExpressionParser("{rootContext[0]}");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<AccessExpressionNode>(root);
            AccessExpressionNode node = (AccessExpressionNode) root;
            Assert.AreEqual("rootContext", node.identifierNode.identifier);
            Assert.AreEqual(1, node.parts.Count);
            Assert.IsInstanceOf<ArrayAccessExpressionNode>(node.parts[0]);
            Assert.IsInstanceOf<NumericLiteralNode>(((ArrayAccessExpressionNode)node.parts[0]).expressionNode);
        }

    }
}