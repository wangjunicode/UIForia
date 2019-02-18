using System.Xml.Schema;
using NUnit.Framework;
using UIForia;
using UIForia.Compilers.Style;
using UIForia.Parsing.Style;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Util;
using UnityEngine;

[TestFixture]
public class StyleSheetCompilerTests {

    [Test]
    public void CompileEmptyStyle() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        Assert.IsNotNull(styleSheet);
        Assert.AreEqual(0, styleSheet.styleGroups.Count);
    }

    [Test]
    public void CreateStyleGroupWithReferencedValue() {
        var nodes = StyleParser2.Parse(@"
            
export const color0 = rgba(255.000, 0, 0, 255);
            
style myStyle {
    BackgroundColor = @color0;
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(1, styleGroup.Count);

        Assert.AreEqual(Color.red, styleGroup[0].normal.BackgroundColor);
    }

    [Test]
    public void CreateAttributeGroupsWithMeasurements() {
        var nodes = StyleParser2.Parse(@"
            
export const m1 = 10%;
            
style myStyle {
    MarginTop = @m1;
    [hover] {
        MarginLeft = 20px;
    }
    [attr:myAttr=""val""] {
        MarginTop = 20px; 
    }
}

        ".Trim());

        StyleSheet styleSheet = StyleSheetCompiler.New().Compile(nodes);

        var styleGroup = styleSheet.styleGroups;
        Assert.AreEqual(2, styleGroup.Count);

        Assert.AreEqual(10, styleGroup[0].normal.MarginTop.value);
        Assert.AreEqual(20, styleGroup[0].hover.MarginLeft.value);
        Assert.AreEqual(20, styleGroup[1].normal.MarginTop.value);
    }

}
