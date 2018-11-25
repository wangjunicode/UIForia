using NUnit.Framework;
using UIForia;
using static Tests.TestUtils;

    [TestFixture]
    public class ExpressionParserTests {

        [Test]
        public void ParseLiteralValueRoot() {
            ExpressionNode root = ExpressionParser.Parse("5");
            Assert.IsInstanceOf<IntLiteralNode>(root);
            IntLiteralNode n = (IntLiteralNode) root;
            Assert.AreEqual(5.0, n.value);

            root = ExpressionParser.Parse("-5.6");
            Assert.IsInstanceOf<DoubleLiteralNode>(root);
            DoubleLiteralNode doubleNode0 = (DoubleLiteralNode) root;
            Assert.AreEqual(-5.6, doubleNode0.value);

            root = ExpressionParser.Parse("'hello'");
            Assert.IsInstanceOf<StringLiteralNode>(root);
            StringLiteralNode n2 = (StringLiteralNode) root;
            Assert.AreEqual("hello", n2.value);

            root = ExpressionParser.Parse("'hello ' ");
            Assert.IsInstanceOf<StringLiteralNode>(root);
            n2 = (StringLiteralNode) root;
            Assert.AreEqual("hello ", n2.value);

            root = ExpressionParser.Parse("true");
            Assert.IsInstanceOf<BooleanLiteralNode>(root);
            BooleanLiteralNode n3 = (BooleanLiteralNode) root;
            Assert.AreEqual(true, n3.value);

            root = ExpressionParser.Parse(" false");
            Assert.IsInstanceOf<BooleanLiteralNode>(root);
            n3 = (BooleanLiteralNode) root;
            Assert.AreEqual(false, n3.value);
        }

        [Test]
        public void Parse_OperatorExpression_Literals() {
            ExpressionNode root = ExpressionParser.Parse("5 + 6");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
            Assert.IsInstanceOf<OperatorNode>(opEx.op);
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.left);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.right);
        }

        [Test]
        public void Parse_OperatorExpression_MultipleLiterals() {
            ExpressionNode root = ExpressionParser.Parse("5 + 6 * 7");
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
            ExpressionNode root = ExpressionParser.Parse("12 == 3");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.Equals, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_NotEquals() {
            ExpressionNode root = ExpressionParser.Parse("12 != 3");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.NotEquals, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_GreaterThan() {
            ExpressionNode root = ExpressionParser.Parse("12 > 3");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.GreaterThan, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_GreaterThanEqualTo() {
            ExpressionNode root = ExpressionParser.Parse("12 >= 3");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.GreaterThanEqualTo, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_LessThan() {
            ExpressionNode root = ExpressionParser.Parse("12 < 3");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.LessThan, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_LessThanEqualTo() {
            ExpressionNode root = ExpressionParser.Parse("12 <= 3");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.LessThanEqualTo, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_And() {
            ExpressionNode root = ExpressionParser.Parse("true && false");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.And, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_OperatorExpression_Or() {
            ExpressionNode root = ExpressionParser.Parse("true || false");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            Assert.AreEqual(OperatorType.Or, ((OperatorExpressionNode) root).op.OpType);
        }

        [Test]
        public void Parse_RootContextIdentifier() {
            ExpressionNode root = ExpressionParser.Parse("{someIdentifier}");
            Assert.IsInstanceOf<RootContextLookupNode>(root);
            root = ExpressionParser.Parse("{ someIdentifier }");
            Assert.IsInstanceOf<RootContextLookupNode>(root);
        }

        [Test]
        public void Parse_ContextLookupOperatorExpression() {
            ExpressionNode root = ExpressionParser.Parse("{someIdentifier + 735.2}");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<RootContextLookupNode>(opEx.left);
            Assert.IsInstanceOf<LiteralValueNode>(opEx.right);
        }

        [Test]
        public void Parse_ExpressionWithOuterParens() {
            ExpressionNode root = ExpressionParser.Parse("{ (someIdentifier + 735.2) }");
            Assert.IsInstanceOf<ParenExpressionNode>(root);
            ParenExpressionNode parenNode = (ParenExpressionNode) root;
            Assert.IsInstanceOf<OperatorExpressionNode>(parenNode.expressionNode);
            OperatorExpressionNode opNode = (OperatorExpressionNode) parenNode.expressionNode;
            Assert.IsInstanceOf<RootContextLookupNode>(opNode.left);
            Assert.IsInstanceOf<NumericLiteralNode>(opNode.right);
        }

        [Test]
        public void Parse_ExpressionWithInnerParens() {
            ExpressionNode root = ExpressionParser.Parse("{someIdentifier + (735.2)}");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<RootContextLookupNode>(opEx.left);
            Assert.IsInstanceOf<ParenExpressionNode>(opEx.right);
        }

        [Test]
        public void Parse_SingleExpressionInParens() {
            ExpressionNode root = ExpressionParser.Parse("(1)");
            Assert.IsInstanceOf<ParenExpressionNode>(root);
            ParenExpressionNode opEx = (ParenExpressionNode) root;
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.expressionNode);
        }

        [Test]
        public void Parse_OperatorLiteralExpression_AddNumbers() {
            ExpressionNode root = ExpressionParser.Parse("4 + 735.2");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.left);
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.right);
        }

        [Test]
        public void Parse_NestedParenAddition() {
            ExpressionNode root = ExpressionParser.Parse("(12 + (4 * 3)) * 2");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
            Assert.IsInstanceOf<ParenExpressionNode>(opEx.left);
            Assert.IsInstanceOf<NumericLiteralNode>(opEx.right);
        }

        [Test]
        public void Parse_AliasLookup() {
            ExpressionNode root = ExpressionParser.Parse("{$myalias}");
            Assert.IsInstanceOf<AliasExpressionNode>(root);
            AliasExpressionNode node = (AliasExpressionNode) root;
            Assert.AreEqual("$myalias", node.identifierNode.identifier);
        }
        
        [Test]
        public void Parse_AliasLookup_PropertyAccess() {
            ExpressionNode root = ExpressionParser.Parse("{$myalias.someValue}");
            Assert.IsInstanceOf<AccessExpressionNode>(root);
            AccessExpressionNode node = (AccessExpressionNode) root;
            Assert.AreEqual("$myalias", node.identifierNode.identifier);
            Assert.AreEqual(1, node.parts.Count);
            Assert.IsInstanceOf<AccessExpressionPartNode>(node.parts[0]);
        }
        
        [Test]
        public void Parse_AliasLookup_InnerExpression() {
            ExpressionNode root = ExpressionParser.Parse("{$myalias * 5}");
            Assert.IsInstanceOf<OperatorExpressionNode>(root);
            OperatorExpressionNode node = (OperatorExpressionNode) root;
            Assert.AreEqual("$myalias", ((AliasExpressionNode)node.left).identifierNode.identifier);            
        }
        
        [Test]
        public void Parse_PropertyAccess_RootContext_Level1() {
            ExpressionNode root = ExpressionParser.Parse("{rootContext.property}");
            Assert.IsInstanceOf<AccessExpressionNode>(root);
            AccessExpressionNode node = (AccessExpressionNode) root;
            Assert.AreEqual("rootContext", node.identifierNode.identifier);
            Assert.AreEqual(1, node.parts.Count);
            Assert.IsInstanceOf<AccessExpressionPartNode>(node.parts[0]);
        }

        [Test]
        public void Parse_ArrayAccess_RootContext_Level1() {
            ExpressionNode root = ExpressionParser.Parse("{rootContext[0]}");
            Assert.IsInstanceOf<AccessExpressionNode>(root);
            AccessExpressionNode node = (AccessExpressionNode) root;
            Assert.AreEqual("rootContext", node.identifierNode.identifier);
            Assert.AreEqual(1, node.parts.Count);
            Assert.IsInstanceOf<ArrayAccessExpressionNode>(node.parts[0]);
            Assert.IsInstanceOf<NumericLiteralNode>(((ArrayAccessExpressionNode) node.parts[0]).expressionNode);
        }

        [Test]
        public void Parse_ArrayAccess_RootContext_LiteralExpression() {
            ExpressionNode root = ExpressionParser.Parse("{rootContext[ 1 + 1 ]}");
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
            ExpressionNode root = ExpressionParser.Parse("{rootContext[ (1 + 1) ]}");
            Assert.IsInstanceOf<AccessExpressionNode>(root);
            AccessExpressionNode node = (AccessExpressionNode) root;
            Assert.AreEqual("rootContext", node.identifierNode.identifier);
            Assert.AreEqual(1, node.parts.Count);
            Assert.IsInstanceOf<ArrayAccessExpressionNode>(node.parts[0]);
            Assert.IsInstanceOf<ParenExpressionNode>(((ArrayAccessExpressionNode) node.parts[0]).expressionNode);

            Assert.IsInstanceOf<ParenExpressionNode>(((ArrayAccessExpressionNode) node.parts[0]).expressionNode);
            ParenExpressionNode parenNode = (ParenExpressionNode) ((ArrayAccessExpressionNode) node.parts[0]).expressionNode;
            OperatorExpressionNode opNode = (OperatorExpressionNode) parenNode.expressionNode;
            Assert.AreEqual(OperatorType.Plus, opNode.op.OpType);
            Assert.IsInstanceOf<NumericLiteralNode>(opNode.left);
            Assert.IsInstanceOf<NumericLiteralNode>(opNode.right);
        }

        [Test]
        public void Parse_ArrayAccess_NestedArrayAccess() {
            ExpressionNode root = ExpressionParser.Parse("{rootContext[ rootContext[3] ]}");
            Assert.IsInstanceOf<AccessExpressionNode>(root);
            AccessExpressionNode node = (AccessExpressionNode) root;
            Assert.AreEqual("rootContext", node.identifierNode.identifier);
            Assert.AreEqual(1, node.parts.Count);
            Assert.IsInstanceOf<ArrayAccessExpressionNode>(node.parts[0]);
            Assert.IsInstanceOf<AccessExpressionNode>(((ArrayAccessExpressionNode) node.parts[0]).expressionNode);
        }

        [Test]
        public void Parse_PropertyAccessChain() {
            ExpressionNode root = ExpressionParser.Parse("{rootContext.property0.property1.property2}");
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
            ExpressionNode root = ExpressionParser.Parse("{rootContext.list[1].property1.property2}");
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
            ExpressionNode root = ExpressionParser.Parse("{someMethod()}");
            MethodCallNode callNode = AssertInstanceOfAndReturn<MethodCallNode>(root);
            Assert.AreEqual("someMethod", callNode.identifierNode.identifier);
            Assert.AreEqual(0, callNode.signatureNode.parts.Count);
        }

        [Test]
        public void Parse_MethodSignature_OneArgument() {
            ExpressionNode root = ExpressionParser.Parse("{someMethod(1 + value)}");
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
            ExpressionNode root = ExpressionParser.Parse("{someMethod(1 + value, 4)}");
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
            ExpressionNode root = ExpressionParser.Parse("{someMethod(1 + value, 4, 5)}");
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
            ExpressionNode root = ExpressionParser.Parse("{someMethod(someOtherMethod(1, 2), 4, 5)}");
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
            ExpressionNode root = ExpressionParser.Parse("{x ? y : 1}");
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
            ExpressionNode root = ExpressionParser.Parse("{x ? y + z : 1}");
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

        [Test]
        public void Parse_UnaryExpression_MinusParen() {
            ExpressionNode root = ExpressionParser.Parse("-(64.8)");
            UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
            Assert.AreEqual(OperatorType.Minus, node.op);
            ParenExpressionNode paren = AssertInstanceOfAndReturn<ParenExpressionNode>(node.expression);
            DoubleLiteralNode d = AssertInstanceOfAndReturn<DoubleLiteralNode>(paren.expressionNode);
            Assert.AreEqual(64.8, d.value);
        }
        
        [Test]
        public void Parse_UnaryExpression_PlusParen() {
            ExpressionNode root = ExpressionParser.Parse("+(64.8)");
            UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
            Assert.AreEqual(OperatorType.Plus, node.op);
            ParenExpressionNode paren = AssertInstanceOfAndReturn<ParenExpressionNode>(node.expression);
            DoubleLiteralNode d = AssertInstanceOfAndReturn<DoubleLiteralNode>(paren.expressionNode);
            Assert.AreEqual(64.8, d.value);
        }
        
        [Test]
        public void Parse_UnaryExpression_NotParen() {
            ExpressionNode root = ExpressionParser.Parse("!(true)");
            UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
            Assert.AreEqual(OperatorType.Not, node.op);
            ParenExpressionNode paren = AssertInstanceOfAndReturn<ParenExpressionNode>(node.expression);
            BooleanLiteralNode b = AssertInstanceOfAndReturn<BooleanLiteralNode>(paren.expressionNode);
            Assert.AreEqual(true, b.value);
        }
        
        [Test]
        public void Parse_UnaryExpression_PlusParenMinus() {
            ExpressionNode root = ExpressionParser.Parse("+(-64.8)");
            UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
            Assert.AreEqual(OperatorType.Plus, node.op);
            ParenExpressionNode paren = AssertInstanceOfAndReturn<ParenExpressionNode>(node.expression);
            DoubleLiteralNode b = AssertInstanceOfAndReturn<DoubleLiteralNode>(paren.expressionNode);
            Assert.AreEqual(-64.8, b.value);
        }
        
        [Test]
        public void Parse_ChainedMethodCallExpressionNoArgs() {
            ExpressionNode root = ExpressionParser.Parse("{thing.action()}");
            AccessExpressionNode node = AssertInstanceOfAndReturn<AccessExpressionNode>(root);
            Assert.AreEqual(2, node.parts.Count);
        }
        
        [Test]
        public void Parse_ChainedMethodCallExpression1Arg() {
            ExpressionNode root = ExpressionParser.Parse("{thing.action(1)}");
            AccessExpressionNode node = AssertInstanceOfAndReturn<AccessExpressionNode>(root);
            Assert.AreEqual(2, node.parts.Count);
        }
        
        [Test]
        public void Parse_ChainedMethodCallExpression2Arg() {
            ExpressionNode root = ExpressionParser.Parse("{$thing.action(1, thing.action2())}");
            AccessExpressionNode node = AssertInstanceOfAndReturn<AccessExpressionNode>(root);
            Assert.AreEqual(2, node.parts.Count);
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

       

    }

