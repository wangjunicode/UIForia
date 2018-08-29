using NUnit.Framework;
using Src;
using Src.Compilers;
using Tests;

[TestFixture]
public class BindingCompilerTests {

    [Test]
    public void CreatesBinding_FieldSetter() {
        ContextDefinition context = new ContextDefinition(typeof(TestUtils.TestUIElementType));
        PropertyBindingCompiler compiler = new PropertyBindingCompiler(context);
        Binding binding = compiler.CompileAttribute(typeof(TestUtils.TestUIElementType), new AttributeDefinition("intValue", "{1 + 1}"));
        Assert.IsNotNull(binding);
    }

}