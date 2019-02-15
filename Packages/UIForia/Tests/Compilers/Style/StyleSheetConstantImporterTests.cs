using System;
using NUnit.Framework;
using UIForia;
using UIForia.Compilers.Style;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Parsing.Style.Tokenizer;
using UIForia.Util;
using UnityEngine;

[TestFixture]
public class StyleSheetConstantImporterTests {

    [Test]
    public void CreateContextWithMultipleConstants() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();
        nodes.Add(StyleASTNode.ExportNode(StyleASTNode.ConstNode("col0", "Color", StyleASTNode.ColorNode("red"))));
        nodes.Add(StyleASTNode.ExportNode(StyleASTNode.ConstNode("thing0", "string", StyleASTNode.StringLiteralNode("someVal"))));
        nodes.Add(StyleASTNode.ExportNode(StyleASTNode.ConstNode("number", "int", StyleASTNode.NumericLiteralNode("1"))));

        var context = new StyleSheetConstantImporter(new StyleSheetImporter()).CreateContext(nodes);

        Assert.AreEqual(3, context.constants.Count);
        Assert.AreEqual("col0", context.constants[0].name);
        Assert.AreEqual(typeof(Color), context.constants[0].type);
        Assert.True(context.constants[0].exported);
        
        Assert.AreEqual("thing0", context.constants[1].name);
        Assert.AreEqual(typeof(string), context.constants[1].type);
        Assert.True(context.constants[1].exported);
        
        Assert.AreEqual("number", context.constants[2].name);
        Assert.AreEqual(typeof(int), context.constants[2].type);
        Assert.True(context.constants[2].exported);
    }

    [Test]
    public void FailContextWithWrongType() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();
        var constNode = StyleASTNode.ConstNode("col0", "Wrong", StyleASTNode.ColorNode("red"));
        constNode.WithLocation(new StyleToken(StyleTokenType.Const, 1, 10));
        var exportNode = StyleASTNode.ExportNode(constNode);
        exportNode.WithLocation(new StyleToken(StyleTokenType.Export, 0, 5));
        nodes.Add(exportNode);

        try {
            new StyleSheetConstantImporter(new StyleSheetImporter()).CreateContext(nodes);
            Assert.Fail("Should have thrown a CompileException");
        }
        catch (CompileException e) {
            Console.WriteLine(e);
            Assert.IsTrue(e.Message.Contains("line 1, column 10"));
        }
        catch (Exception e) {
            Console.WriteLine(e);
            Assert.Fail($"Expected a CompileException and all I got was this lousy {e}");
        }
    }

    [Test]
    public void CreateContextWithReferences() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();
        nodes.Add(StyleASTNode.ExportNode(StyleASTNode.ConstNode("x", "string", StyleASTNode.ReferenceNode("y"))));
        nodes.Add(StyleASTNode.ExportNode(StyleASTNode.ConstNode("y", "string", StyleASTNode.ReferenceNode("z"))));
        var stringValue = StyleASTNode.StringLiteralNode("you win!");
        nodes.Add(StyleASTNode.ExportNode(StyleASTNode.ConstNode("z", "string", stringValue)));

        var context = new StyleSheetConstantImporter(new StyleSheetImporter()).CreateContext(nodes);

        Assert.AreEqual(3, context.constants.Count);
        
        Assert.AreEqual("x", context.constants[2].name);
        Assert.AreEqual(stringValue, context.constants[2].value);
        Assert.AreEqual(typeof(string), context.constants[2].type);
        Assert.True(context.constants[2].exported);
        
        Assert.AreEqual("y", context.constants[1].name);
        Assert.AreEqual(stringValue, context.constants[1].value);
        Assert.AreEqual(typeof(string), context.constants[1].type);
        Assert.True(context.constants[1].exported);
        
        Assert.AreEqual("z", context.constants[0].name);
        Assert.AreEqual(stringValue, context.constants[0].value);
        Assert.AreEqual(typeof(string), context.constants[0].type);
        Assert.True(context.constants[0].exported);
    }
}
