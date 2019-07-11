using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateAST {

        public TemplateNode root;
        public StructList<UsingDeclaration> usings;
        public StructList<StyleDefinition> styles;
        public string fileName;
        public bool extends;

    }

}