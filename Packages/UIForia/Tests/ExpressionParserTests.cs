//using NUnit.Framework;
//using UIForia;
//using static Tests.TestUtils;
//
//    [TestFixture]
//    public class ExpressionParserTests {
//
//        [Test]
//        public void ParseLiteralValueRoot() {
//            ExpressionNodeOld root = ExpressionParser.Parse("5");
//            Assert.IsInstanceOf<IntLiteralNodeOld>(root);
//            IntLiteralNodeOld n = (IntLiteralNodeOld) root;
//            Assert.AreEqual(5.0, n.value);
//
//            root = ExpressionParser.Parse("-5.6");
//            Assert.IsInstanceOf<DoubleLiteralNodeOld>(root);
//            DoubleLiteralNodeOld doubleNode0 = (DoubleLiteralNodeOld) root;
//            Assert.AreEqual(-5.6, doubleNode0.value);
//
//            root = ExpressionParser.Parse("'hello'");
//            Assert.IsInstanceOf<StringLiteralNodeOld>(root);
//            StringLiteralNodeOld n2 = (StringLiteralNodeOld) root;
//            Assert.AreEqual("hello", n2.value);
//
//            root = ExpressionParser.Parse("'hello ' ");
//            Assert.IsInstanceOf<StringLiteralNodeOld>(root);
//            n2 = (StringLiteralNodeOld) root;
//            Assert.AreEqual("hello ", n2.value);
//
//            root = ExpressionParser.Parse("true");
//            Assert.IsInstanceOf<BooleanLiteralNodeOld>(root);
//            BooleanLiteralNodeOld n3 = (BooleanLiteralNodeOld) root;
//            Assert.AreEqual(true, n3.value);
//
//            root = ExpressionParser.Parse(" false");
//            Assert.IsInstanceOf<BooleanLiteralNodeOld>(root);
//            n3 = (BooleanLiteralNodeOld) root;
//            Assert.AreEqual(false, n3.value);
//        }
//
//        [Test]
//        public void Parse_OperatorExpression_Literals() {
////            ExpressionNode root = ExpressionParser.Parse("5 + 6");
////            Assert.IsInstanceOf<OperatorExpressionNode>(root);
////            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
////            Assert.IsInstanceOf<OperatorNode>(opEx.op);
////            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
////            Assert.IsInstanceOf<LiteralValueNode>(opEx.left);
////            Assert.IsInstanceOf<LiteralValueNode>(opEx.right);
//        }
//
//        [Test]
//        public void Parse_OperatorExpression_MultipleLiterals() {
////            ExpressionNode root = ExpressionParser.Parse("5 + 6 * 7");
////            Assert.IsInstanceOf<OperatorExpressionNode>(root);
////            OperatorExpressionNode opEx = (OperatorExpressionNode) root;
////            Assert.IsInstanceOf<OperatorNode>(opEx.op);
////            Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
////            Assert.IsInstanceOf<LiteralValueNode>(opEx.left);
////            Assert.IsInstanceOf<OperatorExpressionNode>(opEx.right);
////            opEx = (OperatorExpressionNode) opEx.right;
////            Assert.AreEqual(opEx.op.OpType, OperatorType.Times);
////            Assert.IsInstanceOf<LiteralValueNode>(opEx.left);
////            Assert.IsInstanceOf<LiteralValueNode>(opEx.right);
//        }
//
////        [Test]
////        public void Parse_OperatorExpression_Equals() {
////            ExpressionNodeOld root = ExpressionParser.Parse("12 == 3");
////            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
////            Assert.AreEqual(OperatorType.Equals, ((OperatorExpressionNodeOld) root).op.OpType);
////        }
////
////        [Test]
////        public void Parse_OperatorExpression_NotEquals() {
////            ExpressionNodeOld root = ExpressionParser.Parse("12 != 3");
////            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
////            Assert.AreEqual(OperatorType.NotEquals, ((OperatorExpressionNodeOld) root).op.OpType);
////        }
////
////        [Test]
////        public void Parse_OperatorExpression_GreaterThan() {
////            ExpressionNodeOld root = ExpressionParser.Parse("12 > 3");
////            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
////            Assert.AreEqual(OperatorType.GreaterThan, ((OperatorExpressionNodeOld) root).op.OpType);
////        }
////
////        [Test]
////        public void Parse_OperatorExpression_GreaterThanEqualTo() {
////            ExpressionNodeOld root = ExpressionParser.Parse("12 >= 3");
////            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
////            Assert.AreEqual(OperatorType.GreaterThanEqualTo, ((OperatorExpressionNodeOld) root).op.OpType);
////        }
////
////        [Test]
////        public void Parse_OperatorExpression_LessThan() {
////            ExpressionNodeOld root = ExpressionParser.Parse("12 < 3");
////            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
////            Assert.AreEqual(OperatorType.LessThan, ((OperatorExpressionNodeOld) root).op.OpType);
////        }
////
////        [Test]
////        public void Parse_OperatorExpression_LessThanEqualTo() {
////            ExpressionNodeOld root = ExpressionParser.Parse("12 <= 3");
////            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
////            Assert.AreEqual(OperatorType.LessThanEqualTo, ((OperatorExpressionNodeOld) root).op.OpType);
////        }
////
////        [Test]
////        public void Parse_OperatorExpression_And() {
////            ExpressionNodeOld root = ExpressionParser.Parse("true && false");
////            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
////            Assert.AreEqual(OperatorType.And, ((OperatorExpressionNodeOld) root).op.OpType);
////        }
////
////        [Test]
////        public void Parse_OperatorExpression_Or() {
////            ExpressionNodeOld root = ExpressionParser.Parse("true || false");
////            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
////            Assert.AreEqual(OperatorType.Or, ((OperatorExpressionNodeOld) root).op.OpType);
////        }
//
//        [Test]
//        public void Parse_RootContextIdentifier() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{someIdentifier}");
//            Assert.IsInstanceOf<RootContextLookupNodeOld>(root);
//            root = ExpressionParser.Parse("{ someIdentifier }");
//            Assert.IsInstanceOf<RootContextLookupNodeOld>(root);
//        }
//
//        [Test]
//        public void Parse_ContextLookupOperatorExpression() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{someIdentifier + 735.2}");
//            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
//            OperatorExpressionNodeOld opEx = (OperatorExpressionNodeOld) root;
//        //    Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
//            Assert.IsInstanceOf<RootContextLookupNodeOld>(opEx.left);
//            Assert.IsInstanceOf<LiteralValueNodeOld>(opEx.right);
//        }
//
//        [Test]
//        public void Parse_ExpressionWithOuterParens() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{ (someIdentifier + 735.2) }");
//            Assert.IsInstanceOf<ParenExpressionNodeOld>(root);
//            ParenExpressionNodeOld parenNodeOld = (ParenExpressionNodeOld) root;
//            Assert.IsInstanceOf<OperatorExpressionNodeOld>(parenNodeOld.expressionNodeOld);
//            OperatorExpressionNodeOld opNodeOld = (OperatorExpressionNodeOld) parenNodeOld.expressionNodeOld;
//            Assert.IsInstanceOf<RootContextLookupNodeOld>(opNodeOld.left);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(opNodeOld.right);
//        }
//
//        [Test]
//        public void Parse_ExpressionWithInnerParens() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{someIdentifier + (735.2)}");
//            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
//            OperatorExpressionNodeOld opEx = (OperatorExpressionNodeOld) root;
//         //   Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
//            Assert.IsInstanceOf<RootContextLookupNodeOld>(opEx.left);
//            Assert.IsInstanceOf<ParenExpressionNodeOld>(opEx.right);
//        }
//
//        [Test]
//        public void Parse_SingleExpressionInParens() {
//            ExpressionNodeOld root = ExpressionParser.Parse("(1)");
//            Assert.IsInstanceOf<ParenExpressionNodeOld>(root);
//            ParenExpressionNodeOld opEx = (ParenExpressionNodeOld) root;
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(opEx.expressionNodeOld);
//        }
//
//        [Test]
//        public void Parse_OperatorLiteralExpression_AddNumbers() {
//            ExpressionNodeOld root = ExpressionParser.Parse("4 + 735.2");
//            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
//            OperatorExpressionNodeOld opEx = (OperatorExpressionNodeOld) root;
//           // Assert.AreEqual(opEx.op.OpType, OperatorType.Plus);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(opEx.left);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(opEx.right);
//        }
//
//        [Test]
//        public void Parse_NestedParenAddition() {
//            ExpressionNodeOld root = ExpressionParser.Parse("(12 + (4 * 3)) * 2");
//            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
//            OperatorExpressionNodeOld opEx = (OperatorExpressionNodeOld) root;
//            Assert.IsInstanceOf<ParenExpressionNodeOld>(opEx.left);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(opEx.right);
//        }
//
//        [Test]
//        public void Parse_AliasLookup() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{$myalias}");
//            Assert.IsInstanceOf<AliasExpressionNodeOld>(root);
//            AliasExpressionNodeOld nodeOld = (AliasExpressionNodeOld) root;
//            Assert.AreEqual("$myalias", nodeOld.identifierNodeOld.identifier);
//        }
//        
//        [Test]
//        public void Parse_AliasLookup_PropertyAccess() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{$myalias.someValue}");
//            Assert.IsInstanceOf<AccessExpressionNodeOld>(root);
//            AccessExpressionNodeOld nodeOld = (AccessExpressionNodeOld) root;
//            Assert.AreEqual("$myalias", nodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(1, nodeOld.parts.Count);
//            Assert.IsInstanceOf<AccessExpressionPartNodeOld>(nodeOld.parts[0]);
//        }
//        
//        [Test]
//        public void Parse_AliasLookup_InnerExpression() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{$myalias * 5}");
//            Assert.IsInstanceOf<OperatorExpressionNodeOld>(root);
//            OperatorExpressionNodeOld nodeOld = (OperatorExpressionNodeOld) root;
//            Assert.AreEqual("$myalias", ((AliasExpressionNodeOld)nodeOld.left).identifierNodeOld.identifier);            
//        }
//        
//        [Test]
//        public void Parse_PropertyAccess_RootContext_Level1() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{rootContext.property}");
//            Assert.IsInstanceOf<AccessExpressionNodeOld>(root);
//            AccessExpressionNodeOld nodeOld = (AccessExpressionNodeOld) root;
//            Assert.AreEqual("rootContext", nodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(1, nodeOld.parts.Count);
//            Assert.IsInstanceOf<AccessExpressionPartNodeOld>(nodeOld.parts[0]);
//        }
//
//        [Test]
//        public void Parse_ArrayAccess_RootContext_Level1() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{rootContext[0]}");
//            Assert.IsInstanceOf<AccessExpressionNodeOld>(root);
//            AccessExpressionNodeOld nodeOld = (AccessExpressionNodeOld) root;
//            Assert.AreEqual("rootContext", nodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(1, nodeOld.parts.Count);
//            Assert.IsInstanceOf<ArrayAccessExpressionNodeOld>(nodeOld.parts[0]);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(((ArrayAccessExpressionNodeOld) nodeOld.parts[0]).expressionNodeOld);
//        }
//
//        [Test]
//        public void Parse_ArrayAccess_RootContext_LiteralExpression() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{rootContext[ 1 + 1 ]}");
//            Assert.IsInstanceOf<AccessExpressionNodeOld>(root);
//            AccessExpressionNodeOld nodeOld = (AccessExpressionNodeOld) root;
//            Assert.AreEqual("rootContext", nodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(1, nodeOld.parts.Count);
//            Assert.IsInstanceOf<ArrayAccessExpressionNodeOld>(nodeOld.parts[0]);
//            Assert.IsInstanceOf<OperatorExpressionNodeOld>(((ArrayAccessExpressionNodeOld) nodeOld.parts[0]).expressionNodeOld);
//            OperatorExpressionNodeOld opNodeOld = (OperatorExpressionNodeOld) ((ArrayAccessExpressionNodeOld) nodeOld.parts[0]).expressionNodeOld;
//          //  Assert.AreEqual(OperatorType.Plus, opNodeOld.op.OpType);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(opNodeOld.left);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(opNodeOld.right);
//        }
//
//        [Test]
//        public void Parse_ArrayAccess_ParenExpression() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{rootContext[ (1 + 1) ]}");
//            Assert.IsInstanceOf<AccessExpressionNodeOld>(root);
//            AccessExpressionNodeOld nodeOld = (AccessExpressionNodeOld) root;
//            Assert.AreEqual("rootContext", nodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(1, nodeOld.parts.Count);
//            Assert.IsInstanceOf<ArrayAccessExpressionNodeOld>(nodeOld.parts[0]);
//            Assert.IsInstanceOf<ParenExpressionNodeOld>(((ArrayAccessExpressionNodeOld) nodeOld.parts[0]).expressionNodeOld);
//
//            Assert.IsInstanceOf<ParenExpressionNodeOld>(((ArrayAccessExpressionNodeOld) nodeOld.parts[0]).expressionNodeOld);
//            ParenExpressionNodeOld parenNodeOld = (ParenExpressionNodeOld) ((ArrayAccessExpressionNodeOld) nodeOld.parts[0]).expressionNodeOld;
//            OperatorExpressionNodeOld opNodeOld = (OperatorExpressionNodeOld) parenNodeOld.expressionNodeOld;
//          //  Assert.AreEqual(OperatorType.Plus, opNodeOld.op.OpType);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(opNodeOld.left);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(opNodeOld.right);
//        }
//
//        [Test]
//        public void Parse_ArrayAccess_NestedArrayAccess() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{rootContext[ rootContext[3] ]}");
//            Assert.IsInstanceOf<AccessExpressionNodeOld>(root);
//            AccessExpressionNodeOld nodeOld = (AccessExpressionNodeOld) root;
//            Assert.AreEqual("rootContext", nodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(1, nodeOld.parts.Count);
//            Assert.IsInstanceOf<ArrayAccessExpressionNodeOld>(nodeOld.parts[0]);
//            Assert.IsInstanceOf<AccessExpressionNodeOld>(((ArrayAccessExpressionNodeOld) nodeOld.parts[0]).expressionNodeOld);
//        }
//
//        [Test]
//        public void Parse_PropertyAccessChain() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{rootContext.property0.property1.property2}");
//            Assert.IsInstanceOf<AccessExpressionNodeOld>(root);
//            AccessExpressionNodeOld nodeOld = (AccessExpressionNodeOld) root;
//            Assert.AreEqual("rootContext", nodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(3, nodeOld.parts.Count);
//            Assert.AreEqual("property0", ((PropertyAccessExpressionPartNodeOld) nodeOld.parts[0]).fieldName);
//            Assert.AreEqual("property1", ((PropertyAccessExpressionPartNodeOld) nodeOld.parts[1]).fieldName);
//            Assert.AreEqual("property2", ((PropertyAccessExpressionPartNodeOld) nodeOld.parts[2]).fieldName);
//        }
//
//        [Test]
//        public void Parse_PropertyAccessMixedArrayAccess() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{rootContext.list[1].property1.property2}");
//            Assert.IsInstanceOf<AccessExpressionNodeOld>(root);
//            AccessExpressionNodeOld nodeOld = (AccessExpressionNodeOld) root;
//            Assert.AreEqual("rootContext", nodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(4, nodeOld.parts.Count);
//            Assert.IsInstanceOf<PropertyAccessExpressionPartNodeOld>(nodeOld.parts[0]);
//            Assert.IsInstanceOf<ArrayAccessExpressionNodeOld>(nodeOld.parts[1]);
//            Assert.IsInstanceOf<PropertyAccessExpressionPartNodeOld>(nodeOld.parts[2]);
//            Assert.IsInstanceOf<PropertyAccessExpressionPartNodeOld>(nodeOld.parts[3]);
//        }
//        
//        [Test]
//        public void Parse_MethodSignature_Empty() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{someMethod()}");
//            MethodCallNodeOld callNodeOld = AssertInstanceOfAndReturn<MethodCallNodeOld>(root);
//            Assert.AreEqual("someMethod", callNodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(0, callNodeOld.signatureNodeOld.parts.Count);
//        }
//
//        [Test]
//        public void Parse_MethodSignature_OneArgument() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{someMethod(1 + value)}");
//            MethodCallNodeOld callNodeOld = AssertInstanceOfAndReturn<MethodCallNodeOld>(root);
//            Assert.AreEqual("someMethod", callNodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(1, callNodeOld.signatureNodeOld.parts.Count);
//            OperatorExpressionNodeOld opEx = AssertInstanceOfAndReturn<OperatorExpressionNodeOld>(callNodeOld.signatureNodeOld.parts[0]);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(opEx.left);
//            Assert.IsInstanceOf<RootContextLookupNodeOld>(opEx.right);
//         //   Assert.AreEqual(OperatorType.Plus, opEx.op.OpType);
//        }
//
//        [Test]
//        public void Parse_MethodSignature_TwoArguments() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{someMethod(1 + value, 4)}");
//            MethodCallNodeOld callNodeOld = AssertInstanceOfAndReturn<MethodCallNodeOld>(root);
//            Assert.AreEqual("someMethod", callNodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(2, callNodeOld.signatureNodeOld.parts.Count);
//            OperatorExpressionNodeOld opEx = AssertInstanceOfAndReturn<OperatorExpressionNodeOld>(callNodeOld.signatureNodeOld.parts[0]);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(opEx.left);
//            Assert.IsInstanceOf<RootContextLookupNodeOld>(opEx.right);
//         //   Assert.AreEqual(OperatorType.Plus, opEx.op.OpType);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(callNodeOld.signatureNodeOld.parts[1]);
//        }
//
//        [Test]
//        public void Parse_MethodSignature_ThreeArguments() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{someMethod(1 + value, 4, 5)}");
//            MethodCallNodeOld callNodeOld = AssertInstanceOfAndReturn<MethodCallNodeOld>(root);
//            Assert.AreEqual("someMethod", callNodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(3, callNodeOld.signatureNodeOld.parts.Count);
//            OperatorExpressionNodeOld opEx = AssertInstanceOfAndReturn<OperatorExpressionNodeOld>(callNodeOld.signatureNodeOld.parts[0]);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(opEx.left);
//            Assert.IsInstanceOf<RootContextLookupNodeOld>(opEx.right);
//          //  Assert.AreEqual(OperatorType.Plus, opEx.op.OpType);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(callNodeOld.signatureNodeOld.parts[1]);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(callNodeOld.signatureNodeOld.parts[2]);
//        }
//
//        [Test]
//        public void Parse_MethodSignature_NestedMethodCall() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{someMethod(someOtherMethod(1, 2), 4, 5)}");
//            MethodCallNodeOld callNodeOld = AssertInstanceOfAndReturn<MethodCallNodeOld>(root);
//            Assert.AreEqual("someMethod", callNodeOld.identifierNodeOld.identifier);
//            Assert.AreEqual(3, callNodeOld.signatureNodeOld.parts.Count);
//            MethodCallNodeOld nestedMethod = AssertInstanceOfAndReturn<MethodCallNodeOld>(callNodeOld.signatureNodeOld.parts[0]);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(nestedMethod.signatureNodeOld.parts[0]);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(nestedMethod.signatureNodeOld.parts[1]);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(callNodeOld.signatureNodeOld.parts[1]);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(callNodeOld.signatureNodeOld.parts[2]);
//        }
//
//        [Test]
//        public void Parse_Ternary() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{x ? y : 1}");
//            OperatorExpressionNodeOld ternary = AssertInstanceOfAndReturn<OperatorExpressionNodeOld>(root);
//            Assert.AreEqual(OperatorType.TernaryCondition, ternary.OpType);
//            OperatorExpressionNodeOld selection = AssertInstanceOfAndReturn<OperatorExpressionNodeOld>(ternary.right);
//            Assert.IsInstanceOf<RootContextLookupNodeOld>(ternary.left);
//          //  Assert.AreEqual(OperatorType.TernarySelection, selection.op.OpType);
//            Assert.IsInstanceOf<RootContextLookupNodeOld>(selection.left);
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(selection.right);
//        }
//
//        [Test]
//        public void Parse_Ternary_NestedExpression() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{x ? y + z : 1}");
//            OperatorExpressionNodeOld ternary = AssertInstanceOfAndReturn<OperatorExpressionNodeOld>(root);
//            Assert.AreEqual(OperatorType.TernaryCondition, ternary.OpType);
//            OperatorExpressionNodeOld selection = AssertInstanceOfAndReturn<OperatorExpressionNodeOld>(ternary.right);
//            Assert.IsInstanceOf<RootContextLookupNodeOld>(ternary.left);
//            Assert.AreEqual("x", GetIdentifierName(ternary.left));
//
//         //   Assert.AreEqual(OperatorType.TernarySelection, selection.op.OpType);
//
//            OperatorExpressionNodeOld nestedOp = AssertInstanceOfAndReturn<OperatorExpressionNodeOld>(selection.left);
//            Assert.AreEqual(OperatorType.Plus, nestedOp.OpType);
//
//            Assert.AreEqual("y", GetIdentifierName(nestedOp.left));
//            Assert.AreEqual("z", GetIdentifierName(nestedOp.right));
//
//            Assert.AreEqual(OperatorType.Plus, nestedOp.OpType);
//
//            Assert.IsInstanceOf<NumericLiteralNodeOld>(selection.right);
//        }
//
//        [Test]
//        public void Parse_UnaryExpression_MinusParen() {
//            ExpressionNodeOld root = ExpressionParser.Parse("-(64.8)");
//            UnaryExpressionNodeOld nodeOld = AssertInstanceOfAndReturn<UnaryExpressionNodeOld>(root);
//            Assert.AreEqual(OperatorType.Minus, nodeOld.op);
//            ParenExpressionNodeOld paren = AssertInstanceOfAndReturn<ParenExpressionNodeOld>(nodeOld.expression);
//            DoubleLiteralNodeOld d = AssertInstanceOfAndReturn<DoubleLiteralNodeOld>(paren.expressionNodeOld);
//            Assert.AreEqual(64.8, d.value);
//        }
//        
//        [Test]
//        public void Parse_UnaryExpression_PlusParen() {
//            ExpressionNodeOld root = ExpressionParser.Parse("+(64.8)");
//            UnaryExpressionNodeOld nodeOld = AssertInstanceOfAndReturn<UnaryExpressionNodeOld>(root);
//            Assert.AreEqual(OperatorType.Plus, nodeOld.op);
//            ParenExpressionNodeOld paren = AssertInstanceOfAndReturn<ParenExpressionNodeOld>(nodeOld.expression);
//            DoubleLiteralNodeOld d = AssertInstanceOfAndReturn<DoubleLiteralNodeOld>(paren.expressionNodeOld);
//            Assert.AreEqual(64.8, d.value);
//        }
//        
//        [Test]
//        public void Parse_UnaryExpression_NotParen() {
//            ExpressionNodeOld root = ExpressionParser.Parse("!(true)");
//            UnaryExpressionNodeOld nodeOld = AssertInstanceOfAndReturn<UnaryExpressionNodeOld>(root);
//            Assert.AreEqual(OperatorType.Not, nodeOld.op);
//            ParenExpressionNodeOld paren = AssertInstanceOfAndReturn<ParenExpressionNodeOld>(nodeOld.expression);
//            BooleanLiteralNodeOld b = AssertInstanceOfAndReturn<BooleanLiteralNodeOld>(paren.expressionNodeOld);
//            Assert.AreEqual(true, b.value);
//        }
//        
//        [Test]
//        public void Parse_UnaryExpression_PlusParenMinus() {
//            ExpressionNodeOld root = ExpressionParser.Parse("+(-64.8)");
//            UnaryExpressionNodeOld nodeOld = AssertInstanceOfAndReturn<UnaryExpressionNodeOld>(root);
//            Assert.AreEqual(OperatorType.Plus, nodeOld.op);
//            ParenExpressionNodeOld paren = AssertInstanceOfAndReturn<ParenExpressionNodeOld>(nodeOld.expression);
//            DoubleLiteralNodeOld b = AssertInstanceOfAndReturn<DoubleLiteralNodeOld>(paren.expressionNodeOld);
//            Assert.AreEqual(-64.8, b.value);
//        }
//        
//        [Test]
//        public void Parse_ChainedMethodCallExpressionNoArgs() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{thing.action()}");
//            AccessExpressionNodeOld nodeOld = AssertInstanceOfAndReturn<AccessExpressionNodeOld>(root);
//            Assert.AreEqual(2, nodeOld.parts.Count);
//        }
//        
//        [Test]
//        public void Parse_ChainedMethodCallExpression1Arg() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{thing.action(1)}");
//            AccessExpressionNodeOld nodeOld = AssertInstanceOfAndReturn<AccessExpressionNodeOld>(root);
//            Assert.AreEqual(2, nodeOld.parts.Count);
//        }
//        
//        [Test]
//        public void Parse_ChainedMethodCallExpression2Arg() {
//            ExpressionNodeOld root = ExpressionParser.Parse("{$thing.action(1, thing.action2())}");
//            AccessExpressionNodeOld nodeOld = AssertInstanceOfAndReturn<AccessExpressionNodeOld>(root);
//            Assert.AreEqual(2, nodeOld.parts.Count);
//        }
//        
//        public static string GetIdentifierName(ASTNode_Old nodeOld) {
//            if (nodeOld is IdentifierNodeOld) {
//                return ((IdentifierNodeOld) nodeOld).identifier;
//            }
//            else if(nodeOld is RootContextLookupNodeOld) {
//                return ((RootContextLookupNodeOld) nodeOld).idNodeOld.identifier;
//            }
//            return null;
//        }
//
//       
//
//    }
//
