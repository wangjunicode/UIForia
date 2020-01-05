using UIForia.Parsing;
using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateAST {

        public TemplateNode root;
        public StructList<UsingDeclaration> usings;
        public StructList<StyleDefinition> styles;
        public LightList<TemplateNode> templates;
        public string fileName;

        public TemplateNode GetTemplate(string templateId) {
            throw new System.NotImplementedException();
        }

    }

}