using System.Xml.Linq;
using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Parsing {

    public enum ParsedTemplateType {

        FromCode,
        Dynamic

    }
    
    public struct RawTemplateContent {

        public string templateId;
        public XElement content;
        public XElement elementDefinition;
        public ParsedTemplateType type;
        public ProcessedType processedType;

    }

    public class TemplateShell {

        public readonly Module module;
        public readonly string filePath;
        
        public StructList<UsingDeclaration> usings;
        public StructList<StyleDefinition> styles;
        public SizedArray<TemplateRootNode> templateRootNodes;
        public LightList<string> referencedNamespaces;

        public TemplateShell(Module module, string filePath) {
            this.module = module;
            this.filePath = filePath;
            this.usings = new StructList<UsingDeclaration>(2);
            this.styles = new StructList<StyleDefinition>(2);
            this.referencedNamespaces = new LightList<string>(4);
        }

    }

}