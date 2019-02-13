//using System;
//using JetBrains.Annotations;
//using NUnit.Framework;
//using UIForia;
//using UnityEngine;
//
//[TestFixture]
//public class TypeCheckTests {
//
//    private class EmptyTarget { }
//
//    private class SimpleRoot {
//
//        public float floatValue;
//        public string stringValue;
//        public bool boolValue;
//        public int[] intArrayValue;
//        public Vector3[] vectors;
//
//        [UsedImplicitly]
//        public void SimpleMethod() { }
//
//        public void SimpleMethod2(int i) { }
//
//    }
//
//    private static readonly ContextDefinition simpleRootContext = new ContextDefinition(typeof(SimpleRoot));
//    private static readonly ContextDefinition nullContext = new ContextDefinition(typeof(EmptyTarget));
//
//    [Test]
//    public void CheckType_Literals() {
//        ExpressionParser parser = new ExpressionParser();
//        Assert.AreEqual(typeof(double), parser.Parse("1.5").GetYieldedType(nullContext));
//        Assert.AreEqual(typeof(double), parser.Parse("-1.5").GetYieldedType(nullContext));
//        Assert.AreEqual(typeof(double), parser.Parse("+1.5").GetYieldedType(nullContext));
//        Assert.AreEqual(typeof(string), parser.Parse("'string here'").GetYieldedType(nullContext));
//        Assert.AreEqual(typeof(bool), parser.Parse("true").GetYieldedType(nullContext));
//        Assert.AreEqual(typeof(bool), parser.Parse("false").GetYieldedType(nullContext));
//    }
//
//    [Test]
//    public void CheckType_RootContextLookup() {
//        ExpressionParser parser = new ExpressionParser();
//        Assert.AreEqual(typeof(float), parser.Parse("{floatValue}").GetYieldedType(simpleRootContext));
//        Assert.AreEqual(typeof(string), parser.Parse("{stringValue}").GetYieldedType(simpleRootContext));
//        Assert.AreEqual(typeof(bool), parser.Parse("{boolValue}").GetYieldedType(simpleRootContext));
//    }
//
//    [Test]
//    public void CheckType_RootContext_Array() {
//        ExpressionParser parser = new ExpressionParser();
//        Assert.AreEqual(typeof(int), parser.Parse("{intArrayValue[1]}").GetYieldedType(simpleRootContext));
//        Assert.AreEqual(typeof(Vector3), parser.Parse("{vectors[1]}").GetYieldedType(simpleRootContext));
//    }
//
//    [Test]
//    public void CheckType_UnaryBoolean_Literal() {
//        ExpressionParser parser = new ExpressionParser();
//        Assert.AreEqual(typeof(bool), parser.Parse("!true").GetYieldedType(nullContext));
//        Assert.AreEqual(typeof(bool), parser.Parse("!false").GetYieldedType(nullContext));
//    }
//
//    [Test]
//    public void CheckType_UnaryBoolean_Expression() {
//        ExpressionParser parser = new ExpressionParser();
//        Assert.AreEqual(typeof(bool), parser.Parse("!(true)").GetYieldedType(nullContext));
//        Assert.AreEqual(typeof(bool), parser.Parse("!(false)").GetYieldedType(nullContext));
//    }
//
//    [Test]
//    public void CheckType_ParenExpression_Literal() {
//        ExpressionParser parser = new ExpressionParser();
//        Assert.AreEqual(typeof(bool), parser.Parse("(true)").GetYieldedType(nullContext));
//    }
//
//    [Test]
//    public void CheckType_MethodCall_NoParameters() {
//        ExpressionParser parser = new ExpressionParser();
//        Assert.AreEqual(typeof(void), parser.Parse("{SimpleMethod()}").GetYieldedType(simpleRootContext));
//    }
//
//    [Test]
//    public void CheckType_MethodCall_1Argument() {
//        ExpressionParser parser = new ExpressionParser();
//        Assert.AreEqual(typeof(void), parser.Parse("{SimpleMethod2(1)}").GetYieldedType(simpleRootContext));
//    }
//
//    [Test]
//    public void CheckType_TernaryArguments() {
//        var error = Assert.Throws<Exception>(() => {
//            ExpressionCompiler compiler = new ExpressionCompiler(nullContext);
//            compiler.Compile("{ true ? true : 1f }");
//        });
//        Assert.IsTrue(error.Message.StartsWith("Types in ternary don't match"));
//    }
//
//}