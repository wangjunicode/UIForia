using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Src;
using Src.Compilers;
using Src.Compilers.AliasSource;
using UnityEngine;

[TestFixture]
public class ExpressionCompilerTests {

    private struct ValueContainer {

        public float x;
        public ValueContainer[] values;

    }

    private class EmptyTarget { }

    private class TestRoot {

        public float value;
        public ValueContainer valueContainer;
        public List<int> someArray;

        public float GetValue() {
            return value;
        }

        public float GetValue1(float multiple) {
            return value * multiple;
        }

    }

    private static ContextDefinition testContextDef = new ContextDefinition(typeof(TestRoot));
    private static ContextDefinition nullContext = new ContextDefinition(typeof(EmptyTarget));

    [SetUp]
    public void Setup() {
        testContextDef = new ContextDefinition(typeof(TestRoot));
        nullContext = new ContextDefinition(typeof(EmptyTarget));
    }

    [Test]
    public void LiteralExpression_BooleanTrue() {
        Expression expression = GetLiteralExpression("true");
        Assert.IsInstanceOf<LiteralExpression_Boolean>(expression);
        Assert.AreEqual(true, expression.Evaluate(null));
    }

    [Test]
    public void LiteralExpression_BooleanFalse() {
        Expression expression = GetLiteralExpression("false");

        Assert.IsInstanceOf<LiteralExpression_Boolean>(expression);
        Assert.AreEqual(false, expression.Evaluate(null));
    }

    [Test]
    public void LiteralExpression_Numeric() {
        Expression expression = GetLiteralExpression("114.5");

        Assert.IsInstanceOf<LiteralExpression_Double>(expression);
        Assert.AreEqual(114.5, expression.Evaluate(null));
    }

    [Test]
    public void LiteralExpression_NegativeNumeric() {
        Expression expression = GetLiteralExpression("-114.5f");

        Assert.IsInstanceOf<LiteralExpression_Float>(expression);
        Assert.AreEqual(-114.5f, expression.Evaluate(null));
    }

    [Test]
    public void LiteralExpression_String() {
        Expression expression = GetLiteralExpression("'some string here'");

        Assert.IsInstanceOf<LiteralExpression_String>(expression);
        Assert.AreEqual("some string here", expression.Evaluate(null));
    }

    [Test]
    public void UnaryBoolean_WithLiteral_True() {
        Expression expression = GetLiteralExpression("!true");
        Assert.IsInstanceOf<LiteralExpression_Boolean>(expression);
        Assert.AreEqual(false, expression.Evaluate(null));
    }

    [Test]
    public void UnaryBoolean_WithLiteral_False() {
        Expression expression = GetLiteralExpression("!false");
        Assert.IsInstanceOf<LiteralExpression_Boolean>(expression);
        Assert.AreEqual(true, expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_IntInt_Fold() {
        Expression expression = GetLiteralExpression("64 + 5");
        Assert.IsInstanceOf<LiteralExpression_Int>(expression);
        Assert.AreEqual(69, expression.Evaluate(null));
        Assert.IsInstanceOf<int>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_IntFloat_Fold() {
        Expression expression = GetLiteralExpression("64 + 5f");
        Assert.IsInstanceOf<LiteralExpression_Float>(expression);
        Assert.AreEqual(69f, expression.Evaluate(null));
        Assert.IsInstanceOf<float>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_IntDouble_Fold() {
        Expression expression = GetLiteralExpression("64 + 5.0");
        Assert.IsInstanceOf<LiteralExpression_Double>(expression);
        Assert.AreEqual(69.0, expression.Evaluate(null));
        Assert.IsInstanceOf<double>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_FloatInt_Fold() {
        Expression expression = GetLiteralExpression("64f + 5");
        Assert.IsInstanceOf<LiteralExpression_Float>(expression);
        Assert.AreEqual(69f, expression.Evaluate(null));
        Assert.IsInstanceOf<float>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_FloatFloat_Fold() {
        Expression expression = GetLiteralExpression("64f + 5.8f");
        Assert.IsInstanceOf<LiteralExpression_Float>(expression);
        Assert.AreEqual(69.8f, expression.Evaluate(null));
        Assert.IsInstanceOf<float>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_DoubleInt_Fold() {
        Expression expression = GetLiteralExpression("64.8 + 5");
        Assert.IsInstanceOf<LiteralExpression_Double>(expression);
        Assert.AreEqual(69.8, expression.Evaluate(null));
        Assert.IsInstanceOf<double>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_DoubleFloat_Fold() {
        Expression expression = GetLiteralExpression("64.8 + 5f");
        Assert.IsInstanceOf<LiteralExpression_Double>(expression);
        Assert.AreEqual(69.8, expression.Evaluate(null));
        Assert.IsInstanceOf<double>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_DoubleDouble_Fold() {
        Expression expression = GetLiteralExpression("64.8 + 5.0");
        Assert.IsInstanceOf<LiteralExpression_Double>(expression);
        Assert.AreEqual(69.8, expression.Evaluate(null));
        Assert.IsInstanceOf<double>(expression.Evaluate(null));
    }

    [Test]
    public void UnaryExpression_Minus_Literal() {
        Expression expression = GetLiteralExpression("-(64.8)");
        Assert.IsInstanceOf<UnaryExpression_Minus_Double>(expression);
        Assert.AreEqual(-64.8, expression.Evaluate(null));
        Assert.IsInstanceOf<double>(expression.Evaluate(null));
    }

    [Test]
    public void UnaryExpression_Plus_Literal() {
        Expression expression = GetLiteralExpression("+(-64.8)");
        Assert.IsInstanceOf<UnaryExpression_Plus_Double>(expression);
        Assert.AreEqual(-64.8, expression.Evaluate(null));
        Assert.IsInstanceOf<double>(expression.Evaluate(null));
    }

    [Test]
    public void AccessExpression_RootContext_Level0() {
        TestRoot target = new TestRoot();
        target.value = 1234.5f;

        ExpressionContext ctx = new ExpressionContext(target);
        ExpressionParser parser = new ExpressionParser("{value}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());

        Assert.IsInstanceOf<AccessExpression_Root<float>>(expression);
        Assert.AreEqual(1234.5f, expression.Evaluate(ctx));
    }

    [Test]
    public void AccessExpression_RootContext_Level1() {
        TestRoot target = new TestRoot();
        target.value = 1234.5f;
        target.valueContainer = new ValueContainer();
        target.valueContainer.x = 123f;

        ExpressionContext ctx = new ExpressionContext(target);
        ExpressionParser parser = new ExpressionParser("{valueContainer.x}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());

        Assert.IsInstanceOf<AccessExpression>(expression);
        Assert.AreEqual(123f, expression.Evaluate(ctx));
    }

    [Test]
    public void AccessExpression_RootContext_Level1_List() {
        TestRoot target = new TestRoot();
        target.value = 1234.5f;
        target.someArray = new List<int>();
        target.someArray.Add(1);
        target.someArray.Add(11);
        target.someArray.Add(111);

        ExpressionContext ctx = new ExpressionContext(target);
        ExpressionParser parser = new ExpressionParser("{someArray[1]}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());

        Assert.IsInstanceOf<AccessExpression>(expression);
        Assert.AreEqual(11, expression.Evaluate(ctx));
    }

    [Test]
    public void AccessExpression_RootContext_MixedArrayField() {
        TestRoot target = new TestRoot();
        target.value = 1234.5f;
        target.valueContainer = new ValueContainer();
        target.valueContainer.values = new ValueContainer[3];
        target.valueContainer.values[0].x = 12;
        target.valueContainer.values[1].x = 13;
        target.valueContainer.values[2].x = 14;
        ExpressionContext ctx = new ExpressionContext(target);
        ExpressionParser parser = new ExpressionParser("{valueContainer.values[1].x}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.IsInstanceOf<AccessExpression>(expression);
        Assert.AreEqual(13, expression.Evaluate(ctx));
    }

    [Test]
    public void AccessExpression_InnerContext_Field() {
        TestRoot target = new TestRoot();
        target.valueContainer.x = 13;

        ExpressionContext ctx = new ExpressionContext(target);

        ctx.SetObjectAlias("$item", target.valueContainer);

        testContextDef.AddRuntimeAlias("$item", typeof(ValueContainer));

        ExpressionParser parser = new ExpressionParser("{$item.x}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.IsInstanceOf<AccessExpression>(expression);
        Assert.AreEqual(13, expression.Evaluate(ctx));
    }

    [Test]
    public void AccessExpression_ArrayIndexAsAlias() {
        TestRoot target = new TestRoot();
        target.someArray = new List<int>();
        target.someArray.Add(1);
        target.someArray.Add(11);
        target.someArray.Add(111);
        ExpressionContext ctx = new ExpressionContext(target);
        testContextDef.AddRuntimeAlias("$i", typeof(int));
        ctx.SetIntAlias("$i", 2);

        ExpressionParser parser = new ExpressionParser("{someArray[$i]}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.IsInstanceOf<AccessExpression>(expression);
        Assert.AreEqual(111, expression.Evaluate(ctx));
    }

    [Test]
    public void AccessExpression_ArrayIndexAsAlias_WithOffset() {
        TestRoot target = new TestRoot();
        target.someArray = new List<int>();
        target.someArray.Add(1);
        target.someArray.Add(11);
        target.someArray.Add(111);
        ExpressionContext ctx = new ExpressionContext(target);
        testContextDef.AddRuntimeAlias("$i", typeof(int));
        ctx.SetIntAlias("$i", 2);

        ExpressionParser parser = new ExpressionParser("{someArray[$i - 1]}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.IsInstanceOf<AccessExpression>(expression);
        Assert.AreEqual(11, expression.Evaluate(ctx));
    }

    [Test]
    public void TernaryExpression_Literals() {
        TestRoot target = new TestRoot();

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{ 1 > 2 ? 5 : 6}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.IsInstanceOf<OperatorExpression_Ternary<int>>(expression);
        Assert.AreEqual(6, expression.Evaluate(ctx));
    }

    [Test]
    public void TernaryExpression_Lookup() {
        TestRoot target = new TestRoot();
        target.value = 12;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{ value > 2 ? 5 : 6}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.IsInstanceOf<OperatorExpression_Ternary<int>>(expression);
        Assert.AreEqual(5, expression.Evaluate(ctx));
    }

    [Test]
    public void StringExpression_ConcatLiterals() {
        TestRoot target = new TestRoot();
        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{ 'my' + ' ' + 'string'}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual("my string", expression.Evaluate(ctx));
    }

    [Test]
    public void MethodCallExpression_CallsMethod() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{GetValue()}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual(124, expression.Evaluate(ctx));
    }

    [Test]
    public void MethodCallExpression_CallsMethod_WithArg() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{GetValue1(5f)}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual(124 * 5, expression.Evaluate(ctx));
    }

    [Test]
    public void MethodCallExpression_CallsMethod_CallsNestedMethod_WithArg() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{GetValue1(GetValue())}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual(124 * 124, expression.Evaluate(ctx));
    }

    [Test]
    public void MethodCallExpression_AliasedMethod() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);
        MethodInfo info = typeof(Mathf).GetMethod("Max", new[] {
            typeof(float), typeof(float)
        });
        
        MethodAliasSource methodSource = new MethodAliasSource("AliasedMethod", info);
        
        testContextDef.AddConstAliasSource(methodSource);
        
        ExpressionParser parser = new ExpressionParser("{AliasedMethod(1f, 2f)}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());

        Assert.AreEqual(2, expression.Evaluate(ctx));
    }

    [Flags]
    public enum TestEnum {

        One = 1 << 0,
        Two = 1 << 1,
        Three = 1 << 2

    }


    [Test]
    public void ResolveConstantEnumAlias() {
        TestRoot target = new TestRoot();
        testContextDef.AddConstAliasSource(new EnumAliasSource<TestEnum>());
        ExpressionContext ctx = new ExpressionContext(target);
        ExpressionParser parser = new ExpressionParser("{One}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.IsInstanceOf<LiteralExpression_Enum<TestEnum>>(expression);
        Assert.AreEqual(TestEnum.One, expression.Evaluate(ctx));
    }

    private static Expression GetLiteralExpression(string input) {
        ExpressionParser parser = new ExpressionParser(input);
        ExpressionCompiler compiler = new ExpressionCompiler(nullContext);
        return compiler.Compile(parser.Parse());
    }

}