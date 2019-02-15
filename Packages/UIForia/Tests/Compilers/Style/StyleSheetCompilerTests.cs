using System.Collections;
using NUnit.Framework;
using UIForia.Compilers.Style;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Util;

[TestFixture]
public class StyleSheetCompilerTests {

    [Test]
    public void CompileEmptyStyle() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();

        StyleSheet styleSheet = new StyleSheetCompiler(new StyleSheetImporter()).Compile(nodes);
        
        Assert.IsNotNull(styleSheet);
        Assert.AreEqual(0, styleSheet.styleGroups.Count);
    }
}
