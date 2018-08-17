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
            opEx = (OperatorExpressionNode) opEx.right;
            Assert.AreEqual(opEx.op.OpType, OperatorType.Times);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.left);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.right);
        }

        [Test]
        public void Parse_OperatorExpression_Equals() {
            ExpressionParser parser = new ExpressionParser("12 == 3");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.Equals, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_NotEquals() {
            ExpressionParser parser = new ExpressionParser("12 != 3");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.NotEquals, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_GreaterThan() {
            ExpressionParser parser = new ExpressionParser("12 > 3");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.GreaterThan, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_GreaterThanEqualTo() {
            ExpressionParser parser = new ExpressionParser("12 >= 3");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.GreaterThanEqualTo, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_LessThan() {
            ExpressionParser parser = new ExpressionParser("12 < 3");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.LessThan, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_LessThanEqualTo() {
            ExpressionParser parser = new ExpressionParser("12 <= 3");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.LessThanEqualTo, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_And() {
            ExpressionParser parser = new ExpressionParser("true && false");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.And, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_Or() {
            ExpressionParser parser = new ExpressionParser("true || false");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.Or, ((OperatorExpressionNode) root).op.OpType);
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
            Assert.IsInstanceOf<NumericLiteralNode>(((ArrayAccessExpressionNode) node.parts[0]).expressionNode);
        }

        [Test]
        public void Parse_ArrayAccess_RootContext_LiteralExpression() {
            ExpressionParser parser = new ExpressionParser("{rootContext[ 1 + 1 ]}");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<AccessExpressionNode>(root);
            AccessExpressionNode node = (AccessExpressionNode) root;
            Assert.AreEqual("rootContext", node.identifierNode.identifier);
            Assert.AreEqual(1, node.parts.Count);
            Assert.IsInstanceOf<ArrayAccessExpressionNode>(node.parts[0]);
            Assert.IsInstanceOf<OperatorExpressionNode>(((ArrayAccessExpressionNode) node.parts[0]).expressionNode);
            OperatorExpressionNode opNode = (OperatorExpressionNode) ((ArrayAccessExpressionNode) node.parts[0]).expressionNode;
            Assert.AreEqual(OperatorType.Plus, opNode.op.OpType);
            Assert.IsInstanceOf<NumericLiteralNode>(opNode.left);
            Assert.IsInstanceOf<NumericLiteralNode>(opNode.right);
        }

        [Test]
        public void Parse_ArrayAccess_ParenExpression() {
            ExpressionParser parser = new ExpressionParser("{rootContext[ (1 + 1) ]}");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<AccessExpressionNode>(root);
            AccessExpressionNode node = (AccessExpressionNode) root;
            Assert.AreEqual("rootContext", node.identifierNode.identifier);
            Assert.AreEqual(1, node.parts.Count);
            Assert.IsInstanceOf<ArrayAccessExpressionNode>(node.parts[0]);
            Assert.IsInstanceOf<ParenOperatorNode>(((ArrayAccessExpressionNode) node.parts[0]).expressionNode);

            Assert.IsInstanceOf<ParenOperatorNode>(((ArrayAccessExpressionNode) node.parts[0]).expressionNode);
            ParenOperatorNode parenNode = (ParenOperatorNode) ((ArrayAccessExpressionNode) node.parts[0]).expressionNode;
            OperatorExpressionNode opNode = (OperatorExpressionNode) parenNode.expression;
            Assert.AreEqual(OperatorType.Plus, opNode.op.OpType);
            Assert.IsInstanceOf<NumericLiteralNode>(opNode.left);
            Assert.IsInstanceOf<NumericLiteralNode>(opNode.right);
        }

        [Test]
        public void Parse_ArrayAccess_NestedArrayAccess() {
            ExpressionParser parser = new ExpressionParser("{rootContext[ rootContext[3] ]}");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<AccessExpressionNode>(root);
            AccessExpressionNode node = (AccessExpressionNode) root;
            Assert.AreEqual("rootContext", node.identifierNode.identifier);
            Assert.AreEqual(1, node.parts.Count);
            Assert.IsInstanceOf<ArrayAccessExpressionNode>(node.parts[0]);
            Assert.IsInstanceOf<AccessExpressionNode>(((ArrayAccessExpressionNode) node.parts[0]).expressionNode);
        }

        [Test]
        public void Parse_PropertyAccessChain() {
            ExpressionParser parser = new ExpressionParser("{rootContext.property0.property1.property2}");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<AccessExpressionNode>(root);
            AccessExpressionNode node = (AccessExpressionNode) root;
            Assert.AreEqual("rootContext", node.identifierNode.identifier);
            Assert.AreEqual(3, node.parts.Count);
            Assert.AreEqual("property0", ((PropertyAccessExpressionPartNode) node.parts[0]).fieldName);
            Assert.AreEqual("property1", ((PropertyAccessExpressionPartNode) node.parts[1]).fieldName);
            Assert.AreEqual("property2", ((PropertyAccessExpressionPartNode) node.parts[2]).fieldName);
        }

        [Test]
        public void Parse_PropertyAccessMixedArrayAccess() {
            ExpressionParser parser = new ExpressionParser("{rootContext.list[1].property1.property2}");
            ExpressionNode root = parser.Parse();
            Assert.IsInstanceOf<AccessExpressionNode>(root);
            AccessExpressionNode node = (AccessExpressionNode) root;
            Assert.AreEqual("rootContext", node.identifierNode.identifier);
            Assert.AreEqual(4, node.parts.Count);
            Assert.IsInstanceOf<PropertyAccessExpressionPartNode>(node.parts[0]);
            Assert.IsInstanceOf<ArrayAccessExpressionNode>(node.parts[1]);
            Assert.IsInstanceOf<PropertyAccessExpressionPartNode>(node.parts[2]);
            Assert.IsInstanceOf<PropertyAccessExpressionPartNode>(node.parts[3]);
        }

        [Test]
        public void Parse_MethodSignature_Empty() {
            ExpressionParser parser = new ExpressionParser("{someMethod()}");
            ExpressionNode root = parser.Parse();
            MethodCallNode callNode = AssertInstanceOfAndReturn<MethodCallNode>(root);
            Assert.AreEqual("someMethod", callNode.identifierNode.identifier);
            Assert.AreEqual(0, callNode.signatureNode.parts.Count);
        }

        [Test]
        public void Parse_MethodSignature_OneArgument() {
            ExpressionParser parser = new ExpressionParser("{someMethod(1 + value)}");
            ExpressionNode root = parser.Parse();
            MethodCallNode callNode = AssertInstanceOfAndReturn<MethodCallNode>(root);
            Assert.AreEqual("someMethod", callNode.identifierNode.identifier);
            Assert.AreEqual(1, callNode.signatureNode.parts.Count);
            OperatorExpressionNode opEx = AssertInstanceOfAndReturn<OperatorExpressionNode>(callNode.signatureNode.parts[0]);
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.left);
            Assert.IsInstanceOf<RootContextLookupNode>(opEx.right);
            Assert.AreEqual(OperatorType.Plus, opEx.op.OpType);
        }

        [Test]
        public void Parse_MethodSignature_TwoArguments() {
            ExpressionParser parser = new ExpressionParser("{someMethod(1 + value, 4)}");
            ExpressionNode root = parser.Parse();
            MethodCallNode callNode = AssertInstanceOfAndReturn<MethodCallNode>(root);
            Assert.AreEqual("someMethod", callNode.identifierNode.identifier);
            Assert.AreEqual(2, callNode.signatureNode.parts.Count);
            OperatorExpressionNode opEx = AssertInstanceOfAndReturn<OperatorExpressionNode>(callNode.signatureNode.parts[0]);
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.left);
            Assert.IsInstanceOf<RootContextLookupNode>(opEx.right);
            Assert.AreEqual(OperatorType.Plus, opEx.op.OpType);
            Assert.IsInstanceOf<NumericLiteralNode>(callNode.signatureNode.parts[1]);
        }

        [Test]
        public void Parse_MethodSignature_ThreeArguments() {
            ExpressionParser parser = new ExpressionParser("{someMethod(1 + value, 4, 5)}");
            ExpressionNode root = parser.Parse();
            MethodCallNode callNode = AssertInstanceOfAndReturn<MethodCallNode>(root);
            Assert.AreEqual("someMethod", callNode.identifierNode.identifier);
            Assert.AreEqual(3, callNode.signatureNode.parts.Count);
            OperatorExpressionNode opEx = AssertInstanceOfAndReturn<OperatorExpressionNode>(callNode.signatureNode.parts[0]);
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.left);
            Assert.IsInstanceOf<RootContextLookupNode>(opEx.right);
            Assert.AreEqual(OperatorType.Plus, opEx.op.OpType);
            Assert.IsInstanceOf<NumericLiteralNode>(callNode.signatureNode.parts[1]);
            Assert.IsInstanceOf<NumericLiteralNode>(callNode.signatureNode.parts[2]);
        }

        [Test]
        public void Parse_MethodSignature_NestedMethodCall() {
            ExpressionParser parser = new ExpressionParser("{someMethod(someOtherMethod(1, 2), 4, 5)}");
            ExpressionNode root = parser.Parse();
            MethodCallNode callNode = AssertInstanceOfAndReturn<MethodCallNode>(root);
            Assert.AreEqual("someMethod", callNode.identifierNode.identifier);
            Assert.AreEqual(3, callNode.signatureNode.parts.Count);
            MethodCallNode nestedMethod = AssertInstanceOfAndReturn<MethodCallNode>(callNode.signatureNode.parts[0]);
            Assert.IsInstanceOf<NumericLiteralNode>(nestedMethod.signatureNode.parts[0]);
            Assert.IsInstanceOf<NumericLiteralNode>(nestedMethod.signatureNode.parts[1]);
            Assert.IsInstanceOf<NumericLiteralNode>(callNode.signatureNode.parts[1]);
            Assert.IsInstanceOf<NumericLiteralNode>(callNode.signatureNode.parts[2]);
        }

        [Test]
        public void Parse_Ternary() {
            ExpressionParser parser = new ExpressionParser("{x ? y : 1}");
            ExpressionNode root = parser.Parse();
            OperatorExpressionNode ternary = AssertInstanceOfAndReturn<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.TernaryCondition, ternary.OpType);
            OperatorExpressionNode selection = AssertInstanceOfAndReturn<OperatorExpressionNode>(ternary.right);
            Assert.IsInstanceOf<RootContextLookupNode>(ternary.left);
            Assert.AreEqual(OperatorType.TernarySelection, selection.op.OpType);
            Assert.IsInstanceOf<RootContextLookupNode>(selection.left);
            Assert.IsInstanceOf<NumericLiteralNode>(selection.right);
        }

        [Test]
        public void Parse_Ternary_NestedExpression() {
            ExpressionParser parser = new ExpressionParser("{x ? y + z : 1}");
            ExpressionNode root = parser.Parse();
            OperatorExpressionNode ternary = AssertInstanceOfAndReturn<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.TernaryCondition, ternary.OpType);
            OperatorExpressionNode selection = AssertInstanceOfAndReturn<OperatorExpressionNode>(ternary.right);
            Assert.IsInstanceOf<RootContextLookupNode>(ternary.left);
            Assert.AreEqual("x", GetIdentifierName(ternary.left));

            Assert.AreEqual(OperatorType.TernarySelection, selection.op.OpType);

            OperatorExpressionNode nestedOp = AssertInstanceOfAndReturn<OperatorExpressionNode>(selection.left);
            Assert.AreEqual(OperatorType.Plus, nestedOp.OpType);

            Assert.AreEqual("y", GetIdentifierName(nestedOp.left));
            Assert.AreEqual("z", GetIdentifierName(nestedOp.right));

            Assert.AreEqual(OperatorType.Plus, nestedOp.OpType);

            Assert.IsInstanceOf<NumericLiteralNode>(selection.right);
        }

        public static string GetIdentifierName(ASTNode node) {
            if (node is IdentifierNode) {
                return ((IdentifierNode) node).identifier;
            }
            else if(node is RootContextLookupNode) {
                return ((RootContextLookupNode) node).idNode.identifier;
            }
            return null;
        }

        public static T AssertInstanceOfAndReturn<T>(object target) {
            Assert.IsInstanceOf<T>(target);
            return (T) target;
        }

    }

}