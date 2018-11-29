using NUnit.Framework;
using UIForia;
using UIForia.Compilers;

[TestFixture]
public class ExpressionCompilerTests2 {

    [Test]
    public void Compile() {
        ExpressionCompiler2 compiler = new ExpressionCompiler2();
        compiler.SetRoot(new {value = ""});
        Expression<string> expr = compiler.Compile<string>("value");
        expr.EvaluateTyped(new ExpressionContext());
    }

}