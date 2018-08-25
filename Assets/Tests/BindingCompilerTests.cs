using NUnit.Framework;
using Src;
using Src.Compilers;
using Tests;

[TestFixture]
public class BindingCompilerTests {

    [Test]
    public void CreatesBinding_FieldSetter() {
        ContextDefinition context = new ContextDefinition(typeof(TestUtils.TestUIElementType));
        BindExpressionCompiler compiler = new BindExpressionCompiler(context);
        Binding binding = compiler.Compile(typeof(TestUtils.TestUIElementType), "intValue", "{1 + 1}");
        Assert.IsNotNull(binding);
    }

}