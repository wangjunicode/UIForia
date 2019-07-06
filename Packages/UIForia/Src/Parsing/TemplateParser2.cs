using System;
using System.Xml.Linq;
using UIForia.Exceptions;
using UIForia.Parsing.Expression;

namespace UIForia.Parsing {

    public class TemplateParser2 {

        private Application app;
        
        public TemplateParser2(Application app) {
            this.app = app;
        }
        
        public void ParseTemplateFromType(Type type) {
            
            ProcessedType processedType = TypeProcessor.GetProcessedType(type);
            string template = processedType.GetTemplate(app.TemplateRootPath);
            if (template == null) {
                throw new TemplateParseException(processedType.GetTemplatePath(), "Template not found");
            }

            XDocument doc = Parse(type, template);
            
        }
        
        private XDocument Parse(Type rootType, string template) {
            try {
                return XDocument.Parse(template);
            }
            catch (Exception e) {
                throw new TemplateParseException(rootType.AssemblyQualifiedName, e.Message, e);
            }
        }

    }

}