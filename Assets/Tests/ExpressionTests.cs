using System;
using NUnit.Framework;
using Src;

namespace Tests {

    [TestFixture]
    public class ExpressionTests {


        private class EmptyTarget { }

        private class SimpleRoot {

            public float value;

        }
        
        private static readonly ContextDefinition simpleRootContext = new ContextDefinition(typeof(SimpleRoot));
        private static readonly ContextDefinition nullContext = new ContextDefinition(typeof(EmptyTarget));
        
        [Test]
        public void LiteralExpression_BooleanTrue() {
            Expression expression = GetLiteralExpression("true");
            
            Assert.IsInstanceOf<BooleanLiteralExpression>(expression);
            Assert.AreEqual(true, expression.Evaluate(null));
        }
        
        [Test]
        public void LiteralExpression_BooleanFalse() {
            Expression expression = GetLiteralExpression("false");
            
            Assert.IsInstanceOf<BooleanLiteralExpression>(expression);
            Assert.AreEqual(false, expression.Evaluate(null));
        }

        [Test]
        public void LiteralExpression_Numeric() {
            Expression expression = GetLiteralExpression("114.5");
            
            Assert.IsInstanceOf<NumericLiteralExpression>(expression);
            Assert.AreEqual(114.5f, expression.Evaluate(null));
        }
        
        [Test]
        public void LiteralExpression_NegativeNumeric() {
            Expression expression = GetLiteralExpression("-114.5");
            
            Assert.IsInstanceOf<NumericLiteralExpression>(expression);
            Assert.AreEqual(-114.5f, expression.Evaluate(null));
        }
        
        [Test]
        public void LiteralExpression_String() {
            Expression expression = GetLiteralExpression("'some string here'");
            
            Assert.IsInstanceOf<StringLiteralExpression>(expression);
            Assert.AreEqual("some string here", expression.Evaluate(null));
            
        }

        [Test]
        public void UnaryBoolean_WithLiteral_True() {
            Expression expression = GetLiteralExpression("!true");
            Assert.IsInstanceOf<BooleanLiteralExpression>(expression);
            Assert.AreEqual(false, expression.Evaluate(null));
        }
        
        [Test]
        public void UnaryBoolean_WithLiteral_False() {
            Expression expression = GetLiteralExpression("!false");
            Assert.IsInstanceOf<BooleanLiteralExpression>(expression);
            Assert.AreEqual(true, expression.Evaluate(null));
        }

        [Test]
        public void LiteralOperatorExpression_Add_Integer() {
            Expression expression = GetLiteralExpression("64 + 5");
            Assert.IsInstanceOf<BooleanLiteralExpression>(expression);
            Assert.AreEqual(69, expression.Evaluate(null));
        }
        
        [Test]
        public void Expression_SimpleRootContextAccess() {
            SimpleRoot target = new SimpleRoot();
            target.value = 1234.5f;
            
            TemplateContext ctx = new TemplateContext(target);
            ExpressionParser parser = new ExpressionParser("{value}");
            ExpressionCompiler compiler = new ExpressionCompiler(simpleRootContext);
            Expression expression = compiler.Compile(parser.Parse());
            
            Assert.IsInstanceOf<RootContextSimpleAccessExpression>(expression);
            Assert.AreEqual(1234.5f, expression.Evaluate(ctx));
        }

        private static Expression GetLiteralExpression(string input) {
            ExpressionParser parser = new ExpressionParser(input);
            ExpressionCompiler compiler = new ExpressionCompiler(nullContext);
            return compiler.Compile(parser.Parse());
        }
    }

}