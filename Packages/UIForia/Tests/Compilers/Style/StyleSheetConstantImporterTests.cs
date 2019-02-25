using NUnit.Framework;
using UIForia.Compilers.Style;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Util;

[TestFixture]
public class StyleSheetConstantImporterTests {

    [Test]
    public void CreateContextWithMultipleConstants() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();
        nodes.Add(StyleASTNodeFactory.ExportNode(StyleASTNodeFactory.ConstNode("col0", StyleASTNodeFactory.ColorNode("red"))));
        nodes.Add(StyleASTNodeFactory.ExportNode(StyleASTNodeFactory.ConstNode("thing0", StyleASTNodeFactory.StringLiteralNode("someVal"))));
        nodes.Add(StyleASTNodeFactory.ExportNode(StyleASTNodeFactory.ConstNode("number", StyleASTNodeFactory.NumericLiteralNode("1"))));

        var context = new StyleSheetConstantImporter(new StyleSheetImporter(null)).CreateContext(nodes);

        Assert.AreEqual(3, context.constants.Count);
        Assert.AreEqual("col0", context.constants[0].name);
        Assert.True(context.constants[0].exported);
        
        Assert.AreEqual("thing0", context.constants[1].name);
        Assert.True(context.constants[1].exported);
        
        Assert.AreEqual("number", context.constants[2].name);
        Assert.True(context.constants[2].exported);
    }

    [Test]
    public void CreateContextWithReferences() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();
        nodes.Add(StyleASTNodeFactory.ExportNode(StyleASTNodeFactory.ConstNode("x", StyleASTNodeFactory.ReferenceNode("y"))));
        nodes.Add(StyleASTNodeFactory.ExportNode(StyleASTNodeFactory.ConstNode("y", StyleASTNodeFactory.ReferenceNode("z"))));
        var stringValue = StyleASTNodeFactory.StringLiteralNode("you win!");
        nodes.Add(StyleASTNodeFactory.ExportNode(StyleASTNodeFactory.ConstNode("z", stringValue)));

        var context = new StyleSheetConstantImporter(new StyleSheetImporter(null)).CreateContext(nodes);

        Assert.AreEqual(3, context.constants.Count);
        
        Assert.AreEqual("x", context.constants[2].name);
        Assert.AreEqual(stringValue, context.constants[2].value);
        Assert.True(context.constants[2].exported);
        
        Assert.AreEqual("y", context.constants[1].name);
        Assert.AreEqual(stringValue, context.constants[1].value);
        Assert.True(context.constants[1].exported);
        
        Assert.AreEqual("z", context.constants[0].name);
        Assert.AreEqual(stringValue, context.constants[0].value);
        Assert.True(context.constants[0].exported);
        
        Assert.AreEqual(0, context.constantsWithReferences.Count, "There should be no unresolved const left.");
    }
}
