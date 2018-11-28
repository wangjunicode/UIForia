using System;
using NUnit.Framework;
using UIForia.Parsing;
using static Tests.TestUtils;

[TestFixture]
public class ExpressionParserTests2 {

    [Test]
    public void ParseLiteralValueRoot() {
        ASTNode root = Parser2.Parse("5");
        Assert.IsInstanceOf<LiteralNode>(root);
        LiteralNode n = (LiteralNode) root;
        Assert.AreEqual("5", n.rawValue);

        root = Parser2.Parse("-5.6");
        Assert.IsInstanceOf<LiteralNode>(root);
        LiteralNode doubleNode0 = (LiteralNode) root;
        Assert.AreEqual("-5.6", doubleNode0.rawValue);

        root = Parser2.Parse("'hello'");
        Assert.IsInstanceOf<LiteralNode>(root);
        LiteralNode n2 = (LiteralNode) root;
        Assert.AreEqual("hello", n2.rawValue);

        root = Parser2.Parse("'hello ' ");
        Assert.IsInstanceOf<LiteralNode>(root);
        n2 = (LiteralNode) root;
        Assert.AreEqual("hello ", n2.rawValue);

        root = Parser2.Parse("true");
        Assert.IsInstanceOf<LiteralNode>(root);
        LiteralNode n3 = (LiteralNode) root;
        Assert.AreEqual("true", n3.rawValue);

        root = Parser2.Parse(" false");
        Assert.IsInstanceOf<LiteralNode>(root);
        n3 = (LiteralNode) root;
        Assert.AreEqual("false", n3.rawValue);

        root = Parser2.Parse(" default");
        Assert.IsInstanceOf<LiteralNode>(root);
        n3 = (LiteralNode) root;
        Assert.AreEqual("default", n3.rawValue);

        root = Parser2.Parse(" null");
        Assert.IsInstanceOf<LiteralNode>(root);
        n3 = (LiteralNode) root;
        Assert.AreEqual("null", n3.rawValue);
    }

    [Test]
    public void Parse_OperatorExpression_Literals() {
        ASTNode root = Parser2.Parse("5 + 6");
        Assert.IsInstanceOf<OperatorNode>(root);
        OperatorNode opEx = (OperatorNode) root;
        Assert.AreEqual(OperatorType.Plus, opEx.operatorType);
        Assert.IsInstanceOf<LiteralNode>(opEx.left);
        Assert.IsInstanceOf<LiteralNode>(opEx.right);
        LiteralNode left = (LiteralNode) opEx.left;
        LiteralNode right = (LiteralNode) opEx.right;
        Assert.AreEqual("5", left.rawValue);
        Assert.AreEqual("6", right.rawValue);
    }

    [Test]
    public void Parse_OperatorExpression_NegatedLiterals() {
        ASTNode root = Parser2.Parse("5 + -6");
        Assert.IsInstanceOf<OperatorNode>(root);
        OperatorNode opEx = (OperatorNode) root;
        Assert.AreEqual(OperatorType.Plus, opEx.operatorType);
        Assert.IsInstanceOf<LiteralNode>(opEx.left);
        Assert.IsInstanceOf<LiteralNode>(opEx.right);
        LiteralNode left = (LiteralNode) opEx.left;
        LiteralNode right = (LiteralNode) opEx.right;
        Assert.AreEqual("5", left.rawValue);
        Assert.AreEqual("-6", right.rawValue);
    }

    [Test]
    public void Parse_NegatedLiterals() {
        ASTNode root = Parser2.Parse("-6");
        Assert.IsInstanceOf<LiteralNode>(root);
        LiteralNode node = (LiteralNode) root;
        Assert.AreEqual("-6", node.rawValue);
    }

    [Test]
    public void Parse_PositiveLiterals() {
        ASTNode root = Parser2.Parse("+6");
        Assert.IsInstanceOf<LiteralNode>(root);
        LiteralNode node = (LiteralNode) root;
        Assert.AreEqual("6", node.rawValue);
    }

    [Test]
    public void Parse_OperatorExpression_MultipleLiterals() {
        ASTNode root = Parser2.Parse("5 + 6 * 7");
        Assert.IsInstanceOf<OperatorNode>(root);
        OperatorNode opEx = (OperatorNode) root;
        Assert.AreEqual(OperatorType.Plus, opEx.operatorType);
        LiteralNode v0 = (LiteralNode) opEx.left;
        Assert.IsInstanceOf<OperatorNode>(opEx.right);
        opEx = (OperatorNode) opEx.right;
        Assert.AreEqual(OperatorType.Times, opEx.operatorType);
        LiteralNode v1 = (LiteralNode) opEx.left;
        LiteralNode v2 = (LiteralNode) opEx.right;
        Assert.AreEqual("5", v0.rawValue);
        Assert.AreEqual("6", v1.rawValue);
        Assert.AreEqual("7", v2.rawValue);
    }

    [Test]
    public void Parse_OperatorExpression_Equals() {
        ASTNode root = Parser2.Parse("12 == 3");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.Equals, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_NotEquals() {
        ASTNode root = Parser2.Parse("12 != 3");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.NotEquals, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_GreaterThan() {
        ASTNode root = Parser2.Parse("12 > 3");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.GreaterThan, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_GreaterThanEqualTo() {
        ASTNode root = Parser2.Parse("12 >= 3");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.GreaterThanEqualTo, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_LessThan() {
        ASTNode root = Parser2.Parse("12 < 3");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.LessThan, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_LessThanEqualTo() {
        ASTNode root = Parser2.Parse("12 <= 3");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.LessThanEqualTo, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_And() {
        ASTNode root = Parser2.Parse("true && false");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.And, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_Or() {
        ASTNode root = Parser2.Parse("true || false");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.Or, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_IdentifierValue() {
        ASTNode root = Parser2.Parse("someIdentifier");
        Assert.IsInstanceOf<IdentifierNode>(root);
        root = Parser2.Parse("{ someIdentifier }");
        Assert.IsInstanceOf<IdentifierNode>(root);
    }

    [Test]
    public void Parse_AliasValue() {
        ASTNode root = Parser2.Parse("{$someIdentifier}");
        Assert.IsInstanceOf<IdentifierNode>(root);
        root = Parser2.Parse("{ $someIdentifier }");
        Assert.IsInstanceOf<IdentifierNode>(root);
    }

    [Test]
    public void Parse_IdentifierWithOperator() {
        ASTNode v0 = Parser2.Parse("{someIdentifier + someIdentifier2}");
        ASTNode v1 = Parser2.Parse("{someIdentifier - someIdentifier2}");
        ASTNode v2 = Parser2.Parse("{someIdentifier * someIdentifier2}");
        ASTNode v3 = Parser2.Parse("{someIdentifier / someIdentifier2}");
        ASTNode v4 = Parser2.Parse("{someIdentifier % someIdentifier2}");

        void Verify(ASTNode node, OperatorType opType) {
            OperatorNode opEx = (OperatorNode) node;
            Assert.AreEqual(opType, ((OperatorNode) node).operatorType);
            Assert.IsInstanceOf<IdentifierNode>(opEx.left);
            Assert.IsInstanceOf<IdentifierNode>(opEx.right);
        }

        Verify(v0, OperatorType.Plus);
        Verify(v1, OperatorType.Minus);
        Verify(v2, OperatorType.Times);
        Verify(v3, OperatorType.Divide);
        Verify(v4, OperatorType.Mod);
    }

    [Test]
    public void Parse_ExpressionWithOuterParens_NotStripped() {
        ASTNode root = Parser2.Parse("{ (someIdentifier + 735.2) }");
        Assert.IsInstanceOf<ParenNode>(root);
        ParenNode parenNode = (ParenNode) root;
        Assert.IsInstanceOf<OperatorNode>(parenNode.expression);
        OperatorNode opNode = (OperatorNode) parenNode.expression;
        Assert.IsInstanceOf<IdentifierNode>(opNode.left);
        Assert.IsInstanceOf<LiteralNode>(opNode.right);
    }

    [Test]
    public void Parse_ExpressionWithOuterParens_Stripped() {
        ASTNode root = Parser2.Parse("{ (someIdentifier) }");
        Assert.IsInstanceOf<IdentifierNode>(root);
    }

    [Test]
    public void Parse_ExpressionWithInnerParens_Stripped() {
        ASTNode root = Parser2.Parse("{someIdentifier + (735.2)}");
        Assert.IsInstanceOf<OperatorNode>(root);
        OperatorNode opEx = (OperatorNode) root;
        Assert.IsInstanceOf<IdentifierNode>(opEx.left);
        Assert.IsInstanceOf<LiteralNode>(opEx.right);
    }

    [Test]
    public void Parse_ExpressionWithInnerParens_NotStripped() {
        ASTNode root = Parser2.Parse("{someIdentifier + (5 + 735.2)}");
        Assert.IsInstanceOf<OperatorNode>(root);
        OperatorNode opEx = (OperatorNode) root;
        Assert.IsInstanceOf<IdentifierNode>(opEx.left);
        Assert.IsInstanceOf<ParenNode>(opEx.right);
    }

    [Test]
    public void Parse_NestedParenAddition() {
        ASTNode root = Parser2.Parse("(12 + (4 * 3)) * 2");
        Assert.IsInstanceOf<OperatorNode>(root);
        OperatorNode opEx = (OperatorNode) root;
        Assert.IsInstanceOf<ParenNode>(opEx.left);
        Assert.IsInstanceOf<LiteralNode>(opEx.right);
        ParenNode parenNode = (ParenNode) opEx.left;
        Assert.IsInstanceOf<OperatorNode>(parenNode.expression);
        opEx = (OperatorNode) parenNode.expression;
        Assert.IsInstanceOf<LiteralNode>(opEx.left);
        Assert.IsInstanceOf<ParenNode>(opEx.right);
    }

    [Test]
    public void Parse_DotAccessExpression() {
        ASTNode root = Parser2.Parse("someProperty.someValue");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(1, node.parts.Count);
        DotAccessNode dotNode = node.parts[0] as DotAccessNode;
        Assert.AreEqual("someValue", dotNode.propertyName);
        Assert.AreEqual("someProperty", node.identifier);
    }
    
    [Test]
    public void Parse_DotAccessExpression_Multiple() {
        ASTNode root = Parser2.Parse("someProperty.someValue.someOtherValue.evenMore");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(3, node.parts.Count);
        Assert.AreEqual("someProperty", node.identifier);
        Assert.AreEqual("someValue", ((DotAccessNode)node.parts[0]).propertyName);
        Assert.AreEqual("someOtherValue", ((DotAccessNode)node.parts[1]).propertyName);
        Assert.AreEqual("evenMore", ((DotAccessNode)node.parts[2]).propertyName);
    }

    [Test]
    public void Parse_IndexExpression() {
        ASTNode root = Parser2.Parse("someProperty[4]");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<IndexExpressionNode>(node.parts[0]);
    }

    [Test]
    public void Parse_InvokeAccessExpression_NoArgs() {
        ASTNode root = Parser2.Parse("someProperty()");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<InvokeNode>(node.parts[0]);
    }
    
    [Test]
    public void Parse_InvokeAccessExpression_1Arg() {
        ASTNode root = Parser2.Parse("someProperty(1)");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<InvokeNode>(node.parts[0]);
        InvokeNode invokeNode = (InvokeNode) node.parts[0];
        Assert.AreEqual(1, invokeNode.parameters.Count);
        Assert.IsInstanceOf<LiteralNode>(invokeNode.parameters[0]);
    }
    
    [Test]
    public void Parse_InvokeAccessExpression_2Arg() {
        ASTNode root = Parser2.Parse("someProperty(1, something)");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<InvokeNode>(node.parts[0]);
        InvokeNode invokeNode = (InvokeNode) node.parts[0];
        Assert.AreEqual(2, invokeNode.parameters.Count);
        Assert.IsInstanceOf<LiteralNode>(invokeNode.parameters[0]);
        Assert.IsInstanceOf<IdentifierNode>(invokeNode.parameters[1]);
    }
    
    [Test]
    public void Parse_InvokeAccessExpression_2Arg_Chained() {
        ASTNode root = Parser2.Parse("someProperty(1, something)()");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(2, node.parts.Count);
        Assert.IsInstanceOf<InvokeNode>(node.parts[0]);
        Assert.IsInstanceOf<InvokeNode>(node.parts[1]);
        InvokeNode invokeNode = (InvokeNode) node.parts[0];
        Assert.AreEqual(2, invokeNode.parameters.Count);
        Assert.IsInstanceOf<LiteralNode>(invokeNode.parameters[0]);
        Assert.IsInstanceOf<IdentifierNode>(invokeNode.parameters[1]);
    }
    
    [Test]
    public void Parse_Ternary() {
        ASTNode root = Parser2.Parse("{x ? y : 1}");
        OperatorNode ternary = AssertInstanceOfAndReturn<OperatorNode>(root);
        Assert.AreEqual(OperatorType.TernaryCondition, ternary.operatorType);
        OperatorNode selection = AssertInstanceOfAndReturn<OperatorNode>(ternary.right);
        Assert.IsInstanceOf<IdentifierNode>(ternary.left);
        Assert.AreEqual(OperatorType.TernarySelection, selection.operatorType);
        Assert.IsInstanceOf<IdentifierNode>(selection.left);
        Assert.IsInstanceOf<LiteralNode>(selection.right);
    }
    
    [Test]
    public void Parse_Ternary_NestedExpression() {
        ASTNode root = Parser2.Parse("{x ? y + z : 1}");
        OperatorNode ternary = AssertInstanceOfAndReturn<OperatorNode>(root);
        Assert.AreEqual(OperatorType.TernaryCondition, ternary.operatorType);
        OperatorNode selection = AssertInstanceOfAndReturn<OperatorNode>(ternary.right);
        Assert.IsInstanceOf<IdentifierNode>(ternary.left);
        Assert.AreEqual("x", ((IdentifierNode)ternary.left).name);

        Assert.AreEqual(OperatorType.TernarySelection, selection.operatorType);

        OperatorNode nestedOp = AssertInstanceOfAndReturn<OperatorNode>(selection.left);
        Assert.AreEqual(OperatorType.Plus, nestedOp.operatorType);

        Assert.AreEqual("y", ((IdentifierNode)nestedOp.left).name);
        Assert.AreEqual("z", ((IdentifierNode)nestedOp.right).name);

        Assert.AreEqual(OperatorType.Plus, nestedOp.operatorType);

        Assert.IsInstanceOf<LiteralNode>(selection.right);
    }
    
    [Test]
    public void Parse_ArrayAccess_RootContext_LiteralExpression() {
        ASTNode root = Parser2.Parse("{rootContext[ 1 + 1 ]}");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual("rootContext", node.identifier);
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<IndexExpressionNode>(node.parts[0]);
        Assert.IsInstanceOf<OperatorNode>(((IndexExpressionNode) node.parts[0]).expression);
        OperatorNode opNode = (OperatorNode) ((IndexExpressionNode) node.parts[0]).expression;
        Assert.AreEqual(OperatorType.Plus, opNode.operatorType);
        Assert.IsInstanceOf<LiteralNode>(opNode.left);
        Assert.IsInstanceOf<LiteralNode>(opNode.right);
    }
    
    [Test]
    public void Parse_ArrayAccess_ParenExpression() {
        ASTNode root = Parser2.Parse("{rootContext[ (1 + 1) ]}");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual("rootContext", node.identifier);
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<IndexExpressionNode>(node.parts[0]);
        Assert.IsInstanceOf<ParenNode>(((IndexExpressionNode) node.parts[0]).expression);

        Assert.IsInstanceOf<ParenNode>(((IndexExpressionNode) node.parts[0]).expression);
        ParenNode parenNodeOld = (ParenNode) ((IndexExpressionNode) node.parts[0]).expression;
        OperatorNode opNode = (OperatorNode) parenNodeOld.expression;
        Assert.AreEqual(OperatorType.Plus, opNode.operatorType);
        Assert.IsInstanceOf<LiteralNode>(opNode.left);
        Assert.IsInstanceOf<LiteralNode>(opNode.right);
    }
    
    [Test]
    public void Parse_ArrayAccess_NestedArrayAccess() {
        ASTNode root = Parser2.Parse("{rootContext[ rootContext[3] ]}");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual("rootContext", node.identifier);
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<IndexExpressionNode>(node.parts[0]);
        Assert.IsInstanceOf<MemberAccessExpressionNode>(((IndexExpressionNode) node.parts[0]).expression);
    }
    
    [Test]
    public void Parse_PropertyAccessMixedArrayAccess() {
        ASTNode root = Parser2.Parse("{rootContext.list[1].property1.property2}");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual("rootContext", node.identifier);
        Assert.AreEqual(4, node.parts.Count);
        Assert.IsInstanceOf<DotAccessNode>(node.parts[0]);
        Assert.IsInstanceOf<IndexExpressionNode>(node.parts[1]);
        Assert.IsInstanceOf<DotAccessNode>(node.parts[2]);
        Assert.IsInstanceOf<DotAccessNode>(node.parts[3]);
    }
    
    [Test]
    public void Parse_PropertyAccessMixedArrayAccessWithInvoke() {
        ASTNode root = Parser2.Parse("{rootContext()[1].property1.property2}");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual("rootContext", node.identifier);
        Assert.AreEqual(4, node.parts.Count);
        Assert.IsInstanceOf<InvokeNode>(node.parts[0]);
        Assert.IsInstanceOf<IndexExpressionNode>(node.parts[1]);
        Assert.IsInstanceOf<DotAccessNode>(node.parts[2]);
        Assert.IsInstanceOf<DotAccessNode>(node.parts[3]);
    }

    [Test]
    public void Parse_UnaryExpression_Not() {
        ASTNode root = Parser2.Parse("!someValue");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.UnaryNot, node.type);
        Assert.IsInstanceOf<IdentifierNode>(node.expression);
    }
    
    [Test]
    public void Parse_UnaryExpression_Not_Complex() {
        ASTNode root = Parser2.Parse("!(someValue > 4)");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.UnaryNot, node.type);
        Assert.IsInstanceOf<ParenNode>(node.expression);
    }
    
    [Test]
    public void Parse_UnaryExpression_Minus() {
        ASTNode root = Parser2.Parse("-someValue");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.UnaryMinus, node.type);
        Assert.IsInstanceOf<IdentifierNode>(node.expression);
    }
    
    [Test]
    public void Parse_UnaryExpression_Minus_Complex() {
        ASTNode root = Parser2.Parse("-(someValue + 4)");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.UnaryMinus, node.type);
        Assert.IsInstanceOf<ParenNode>(node.expression);
    }

    [Test]
    public void Parse_UnaryExpression_DirectCast() {
        ASTNode root = Parser2.Parse("(int)5.6f");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.DirectCast, node.type);
        Assert.AreEqual("int", node.typePath.path[0]);
        
        root = Parser2.Parse("(UnityEngine.Vector3)5.6f");
        node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.DirectCast, node.type);
        Assert.AreEqual("UnityEngine", node.typePath.path[0]);
        Assert.AreEqual("Vector3", node.typePath.path[1]);
    }
    
    [Test]
    public void Parse_UnaryExpression_DirectCastGeneric() {
        ASTNode root = Parser2.Parse("(List<float, int>)someIdentifier");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.DirectCast, node.type);
        Assert.AreEqual("List", node.typePath.path[0]);
        Assert.AreEqual("float", node.typePath.genericArguments[0].path[0]);
        Assert.AreEqual("int", node.typePath.genericArguments[1].path[0]);
    }
    
    [Test]
    public void Parse_UnaryExpression_DirectCastGenericList() {
        ASTNode root = Parser2.Parse("(List<Something.Tuple<int, ValueTuple>>");//"<float, UnityEngine.Vector3>>>)someIdentifier");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual("List", node.typePath.path[0]);
        TypePath head = node.typePath;
        TypePath outerTuple = node.typePath.genericArguments[0];
        TypePath outerGen0 = outerTuple.genericArguments[0];
        TypePath outerGen1 = outerTuple.genericArguments[1];
        
        Assert.AreEqual("Something", outerTuple.path[0]);
        Assert.AreEqual("Tuple", outerTuple.path[1]);
        Assert.AreEqual("int", outerGen0.path[0]);
        Assert.AreEqual("ValueTuple", outerGen1.path[0]);
    }

//    [Test]
//    public void Parse_TypeOfExpression() { }
//
//    [Test]
//    public void Parse_NewExpression() { }
//    
//    [Test]
//    public void Parse_ArrayLiteralExpression() {}

}