using NUnit.Framework;
using UIForia.Util;

[TestFixture]
public class WhitespaceProcessorTests {

    [Test]
    public void Wrap_IdentityInput() {
        string input = "nothing should change";
        string output = TextUtil.ProcessWrapString(input);
        Assert.AreEqual(input, output);
    }
    
    [Test]
    public void Wrap_CollapseSpaces() {
        string input = "spaces   should change";
        string output = TextUtil.ProcessWrapString(input);
        Assert.AreEqual("spaces should change", output);
    }
    
    [Test]
    public void Wrap_CollapseNewlines() {
        string input = "  \tspaces   \nshould change\n";
        string output = TextUtil.ProcessWrapString(input);
        Assert.AreEqual("spaces should change", output);
    }

}