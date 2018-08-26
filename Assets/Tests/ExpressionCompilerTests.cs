using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using NUnit.Framework;
using Src;
using Src.Compilers.AliasSource;
using Tests;
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

        public float RetnMethod0() {
            return value;
        }

        public float RetnMethod1(float arg0) {
            return value + arg0;
        }

        public float RetnMethod2(float arg0, float arg1) {
            return value + arg0 + arg1;
        }

        public float RetnMethod3(float arg0, float arg1, float arg2) {
            return value + arg0 + arg1 + arg2;
        }

        public float RetnMethod4(float arg0, float arg1, float arg2, float arg3) {
            return value + arg0 + arg1 + arg2 + arg3;
        }
        
        [UsedImplicitly]
        public void VoidMethod0() {
            value = 0;
        }

        [UsedImplicitly]
        public void VoidMethod1(float one) {
            value = one;
        }

        [UsedImplicitly]
        public void VoidMethod2(float one, float two) {
            value = one + two;
        }

        [UsedImplicitly]
        public void VoidMethod3(float one, float two, float three) {
            value = one + two + three;
        }

        [UsedImplicitly]
        public void VoidMethod4(float one, float two, float x, float y) {
            value = one + two + x + y;
        }

        public static string staticStringValue;
        
        public static void StaticVoid0() {
            staticStringValue = "CalledStaticVoid0";
        }
        
        public static void StaticVoid1(string arg0) {
            staticStringValue = "CalledStaticVoid1_" + arg0;
        }

        public static void StaticVoid2(string arg0, string arg1) {
            staticStringValue = "CalledStaticVoid2_" + arg0 + arg1;
        }
        
        public static void StaticVoid3(string arg0, string arg1, string arg2) {
            staticStringValue = "CalledStaticVoid3_" + arg0 + arg1 + arg2;
        }
        
        public static void StaticVoid4(string arg0, string arg1, string arg2, string arg3) {
            staticStringValue = "CalledStaticVoid4_" + arg0 + arg1 + arg2 + arg3;
        }
        
        public static string StaticNonVoid0() {
            return "StaticNonVoid0";
        }
        
        public static string StaticNonVoid1(string arg0) {
            return "StaticNonVoid1" + arg0;
        }

        public static string StaticNonVoid2(string arg0, string arg1) {
            return "StaticNonVoid2" + arg0 + arg1;
        }
        
        public static string StaticNonVoid3(string arg0, string arg1, string arg2) {
            return  "StaticNonVoid3" + arg0 + arg1 + arg2;
        }
        
        public static string StaticNonVoid4(string arg0, string arg1, string arg2, string arg3) {
            return "StaticNonVoid4" + arg0 + arg1 + arg2 + arg3;
        }
        
    }

    private static ContextDefinition testContextDef = new ContextDefinition(typeof(TestRoot));
    private static ContextDefinition nullContext = new ContextDefinition(typeof(EmptyTarget));

    [SetUp]
    public void Setup() {
        testContextDef = new ContextDefinition(typeof(TestRoot));
        nullContext = new ContextDefinition(typeof(EmptyTarget));
        TestRoot.staticStringValue = string.Empty;
    }

    [Test]
    public void ConstantExpression_BoolTrue() {
        Expression expression = GetLiteralExpression("true");
        Assert.IsInstanceOf<ConstantExpression<bool>>(expression);
        Assert.AreEqual(true, expression.Evaluate(null));
    }

    [Test]
    public void ConstantExpression_BoolFalse() {
        Expression expression = GetLiteralExpression("false");

        Assert.IsInstanceOf<ConstantExpression<bool>>(expression);
        Assert.AreEqual(false, expression.Evaluate(null));
    }

    [Test]
    public void LiteralExpression_Numeric() {
        Expression expression = GetLiteralExpression("114.5");

        Assert.IsInstanceOf<ConstantExpression<double>>(expression);
        Assert.AreEqual(114.5, expression.Evaluate(null));
    }

    [Test]
    public void LiteralExpression_NegativeNumeric() {
        Expression expression = GetLiteralExpression("-114.5f");

        Assert.IsInstanceOf<ConstantExpression<float>>(expression);
        Assert.AreEqual(-114.5f, expression.Evaluate(null));
    }

    [Test]
    public void ConstantExpression_String() {
        Expression expression = GetLiteralExpression("'some string here'");

        Assert.IsInstanceOf<ConstantExpression<string>>(expression);
        Assert.AreEqual("some string here", expression.Evaluate(null));
    }

    [Test]
    public void UnaryBoolean_WithLiteral_True() {
        Expression expression = GetLiteralExpression("!true");
        Assert.IsInstanceOf<ConstantExpression<bool>>(expression);
        Assert.AreEqual(false, expression.Evaluate(null));
    }

    [Test]
    public void UnaryBoolean_WithLiteral_False() {
        Expression expression = GetLiteralExpression("!false");
        Assert.IsInstanceOf<ConstantExpression<bool>>(expression);
        Assert.AreEqual(true, expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_IntInt_Fold() {
        Expression expression = GetLiteralExpression("64 + 5");
        Assert.IsInstanceOf<ConstantExpression<int>>(expression);
        Assert.AreEqual(69, expression.Evaluate(null));
        Assert.IsInstanceOf<int>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_IntFloat_Fold() {
        Expression expression = GetLiteralExpression("64 + 5f");
        Assert.IsInstanceOf<ConstantExpression<float>>(expression);
        Assert.AreEqual(69f, expression.Evaluate(null));
        Assert.IsInstanceOf<float>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_IntDouble_Fold() {
        Expression expression = GetLiteralExpression("64 + 5.0");
        Assert.IsInstanceOf<ConstantExpression<double>>(expression);
        Assert.AreEqual(69.0, expression.Evaluate(null));
        Assert.IsInstanceOf<double>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_FloatInt_Fold() {
        Expression expression = GetLiteralExpression("64f + 5");
        Assert.IsInstanceOf<ConstantExpression<float>>(expression);
        Assert.AreEqual(69f, expression.Evaluate(null));
        Assert.IsInstanceOf<float>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_FloatFloat_Fold() {
        Expression expression = GetLiteralExpression("64f + 5.8f");
        Assert.IsInstanceOf<ConstantExpression<float>>(expression);
        Assert.AreEqual(69.8f, expression.Evaluate(null));
        Assert.IsInstanceOf<float>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_DoubleInt_Fold() {
        Expression expression = GetLiteralExpression("64.8 + 5");
        Assert.IsInstanceOf<ConstantExpression<double>>(expression);
        Assert.AreEqual(69.8, expression.Evaluate(null));
        Assert.IsInstanceOf<double>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_DoubleFloat_Fold() {
        Expression expression = GetLiteralExpression("64.8 + 5f");
        Assert.IsInstanceOf<ConstantExpression<double>>(expression);
        Assert.AreEqual(69.8, expression.Evaluate(null));
        Assert.IsInstanceOf<double>(expression.Evaluate(null));
    }

    [Test]
    public void LiteralOperatorExpression_Add_DoubleDouble_Fold() {
        Expression expression = GetLiteralExpression("64.8 + 5.0");
        Assert.IsInstanceOf<ConstantExpression<double>>(expression);
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

    [Test]
    public void MethodCallExpression_CallVoidReturnInstance() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{VoidMethod0()}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        object retn = expression.Evaluate(ctx);
        Assert.IsNull(retn);
        Assert.AreEqual(0, target.value);
    }

    [Test]
    public void MethodCallExpression_CallVoidReturnInstance_1Arg() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{VoidMethod1(1)}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        object retn = expression.Evaluate(ctx);
        Assert.IsNull(retn);
        Assert.AreEqual(1, target.value);
    }
    
    [Test]
    public void MethodCallExpression_CallVoidReturnInstance_2Arg() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{VoidMethod2(1, 2)}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        object retn = expression.Evaluate(ctx);
        Assert.IsNull(retn);
        Assert.AreEqual(3, target.value);
    }
    
    [Test]
    public void MethodCallExpression_CallVoidReturnInstance_3Arg() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{VoidMethod3(1, 2, 3)}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        object retn = expression.Evaluate(ctx);
        Assert.IsNull(retn);
        Assert.AreEqual(6, target.value);
    }
    
    [Test]
    public void MethodCallExpression_CallVoidReturnInstance_4Arg() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{VoidMethod4(1, 2, 3, 4)}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        object retn = expression.Evaluate(ctx);
        Assert.IsNull(retn);
        Assert.AreEqual(10, target.value);
    }
    
    [Test]
    public void MethodCallExpression_CallVoidReturnStatic_0Arg() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{StaticVoid0()}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        object retn = expression.Evaluate(ctx);
        Assert.IsNull(retn);
        Assert.AreEqual("CalledStaticVoid0", TestRoot.staticStringValue);
    }
    
    [Test]
    public void MethodCallExpression_CallVoidReturnStatic_1Arg() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{StaticVoid1('yes')}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        object retn = expression.Evaluate(ctx);
        Assert.IsNull(retn);
        Assert.AreEqual("CalledStaticVoid1_yes", TestRoot.staticStringValue);
    }
    
    [Test]
    public void MethodCallExpression_CallVoidReturnStatic_2Arg() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{StaticVoid2('yes','no')}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        object retn = expression.Evaluate(ctx);
        Assert.IsNull(retn);
        Assert.AreEqual("CalledStaticVoid2_yesno", TestRoot.staticStringValue);
    }
    
    [Test]
    public void MethodCallExpression_CallVoidReturnStatic_3Arg() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{StaticVoid3('yes','no', 'maybe')}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        object retn = expression.Evaluate(ctx);
        Assert.IsNull(retn);
        Assert.AreEqual("CalledStaticVoid3_yesnomaybe", TestRoot.staticStringValue);
    }

    [Test]
    public void MethodCallExpression_CallVoidReturnStatic_4Arg() {
        TestRoot target = new TestRoot();
        target.value = 124;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{StaticVoid4('yes','no', 'maybe', 'def')}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        object retn = expression.Evaluate(ctx);
        Assert.IsNull(retn);
        Assert.AreEqual("CalledStaticVoid4_yesnomaybedef", TestRoot.staticStringValue);
    }
    
    [Test]
    public void MethodCallExpression_CallNonVoidReturnInstance_0Arg() {
        TestRoot target = new TestRoot();
        target.value = 1;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{RetnMethod0()}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual(1 ,expression.Evaluate(ctx));
    }
    
    [Test]
    public void MethodCallExpression_CallNonVoidReturnInstance_1Arg() {
        TestRoot target = new TestRoot();
        target.value = 1;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{RetnMethod1(1f)}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual(2 ,expression.Evaluate(ctx));
    }
    
    [Test]
    public void MethodCallExpression_CallNonVoidReturnInstance_2Arg() {
        TestRoot target = new TestRoot();
        target.value = 1;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{RetnMethod2(1f, 2f)}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual(4 ,expression.Evaluate(ctx));
    }

    [Test]
    public void MethodCallExpression_CallNonVoidReturnInstance_3Arg() {
        TestRoot target = new TestRoot();
        target.value = 1;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{RetnMethod3(1f, 2f, 3f)}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual(7 ,expression.Evaluate(ctx));
    }
    
    [Test]
    public void MethodCallExpression_CallNonVoidReturnInstance_4Arg() {
        TestRoot target = new TestRoot();
        target.value = 1;

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{RetnMethod4(1f, 2f, 3f, 4f)}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual(11 ,expression.Evaluate(ctx));
    }
    
    [Test]
    public void MethodCallExpression_CallNonVoidReturnStatic_0Arg() {
        TestRoot target = new TestRoot();

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{StaticNonVoid0()}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual("StaticNonVoid0" ,expression.Evaluate(ctx));
    }

    [Test]
    public void MethodCallExpression_CallNonVoidReturnStatic_1Arg() {
        TestRoot target = new TestRoot();

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{StaticNonVoid1('hello')}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual("StaticNonVoid1hello" ,expression.Evaluate(ctx));
    }

    [Test]
    public void MethodCallExpression_CallNonVoidReturnStatic_2Arg() {
        TestRoot target = new TestRoot();

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{StaticNonVoid2('hi','there')}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual("StaticNonVoid2hithere" ,expression.Evaluate(ctx));
    }

    [Test]
    public void MethodCallExpression_CallNonVoidReturnStatic_3Arg() {
        TestRoot target = new TestRoot();

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{StaticNonVoid3('hi','there', 'how')}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual("StaticNonVoid3hitherehow" ,expression.Evaluate(ctx));
    }
    
    [Test]
    public void MethodCallExpression_CallNonVoidReturnStatic_4Arg() {
        TestRoot target = new TestRoot();

        ExpressionContext ctx = new ExpressionContext(target);

        ExpressionParser parser = new ExpressionParser("{StaticNonVoid4('hi','there', 'how', 'areya')}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.AreEqual("StaticNonVoid4hitherehowareya" ,expression.Evaluate(ctx));
    }
    
    [Test]
    public void ResolveConstantEnumAlias() {
        TestRoot target = new TestRoot();
        testContextDef.AddConstAliasSource(new EnumAliasSource<TestUtils.TestEnum>());
        ExpressionContext ctx = new ExpressionContext(target);
        ExpressionParser parser = new ExpressionParser("{One}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.IsInstanceOf<ConstantExpression<TestUtils.TestEnum>>(expression);
        Assert.AreEqual(TestUtils.TestEnum.One, expression.Evaluate(ctx));
    }

    [Test]
    public void ResolveNonStandardAliasType() {
        TestRoot target = new TestRoot();
        Color color = Color.red;
        testContextDef.AddConstAliasSource(new TestUtils.TestAliasSource(color));
        ExpressionContext ctx = new ExpressionContext(target);
        ExpressionParser parser = new ExpressionParser("{NonStandard}");
        ExpressionCompiler compiler = new ExpressionCompiler(testContextDef);
        Expression expression = compiler.Compile(parser.Parse());
        Assert.IsInstanceOf<ConstantExpression<Color>>(expression);
        Assert.AreEqual(Color.red, expression.Evaluate(ctx));
    }

    private static Expression GetLiteralExpression(string input) {
        ExpressionParser parser = new ExpressionParser(input);
        ExpressionCompiler compiler = new ExpressionCompiler(nullContext);
        return compiler.Compile(parser.Parse());
    }

}