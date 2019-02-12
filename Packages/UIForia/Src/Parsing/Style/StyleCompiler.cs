using UIForia.Style.Parsing;
using UIForia.Util;

namespace UIForia.Parsing.Style {
    public class StyleCompiler {

        public void CompileStyle(string style) {
            LightList<StyleASTNode> styleRootNode = StyleParser2.Parse(style);
            
            
            
        }
    }
}
