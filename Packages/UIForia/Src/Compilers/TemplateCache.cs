using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Exceptions;
using UIForia.Parsing;

namespace UIForia.Compilers {

    public class TemplateCache {

        private readonly TemplateSettings settings;
        private readonly XMLTemplateParser2 xmlTemplateParser;
        private readonly Dictionary<string, RootTemplateNode> templateMap;

        public TemplateCache(TemplateSettings settings) {
            this.settings = settings;
            this.xmlTemplateParser = new XMLTemplateParser2(this);
            this.templateMap = new Dictionary<string, RootTemplateNode>(37);
        }

        public RootTemplateNode GetParsedTemplate(ProcessedType processedType) {
            RootTemplateNode retn = null;

            string filePath = GetTemplateFilePath(processedType);

            if (templateMap.TryGetValue(filePath, out retn)) {
                return retn;
            }

            TemplateDefinition templateDefinition = GetTemplateSource(processedType);

            RootTemplateNode rootNode = new RootTemplateNode(filePath, processedType, null, default);

            retn = xmlTemplateParser.Parse(rootNode, templateDefinition.contents, filePath, processedType);

            templateMap.Add(templateDefinition.filePath, retn);

            return retn;
        }

        public string GetTemplateFilePath(ProcessedType processedType) {
            TemplateAttribute templateAttr = processedType.templateAttr;
            switch (templateAttr.templateType) {

                case TemplateType.Internal: {
                    return settings.GetInternalTemplatePath(templateAttr.template);
                }

                case TemplateType.File: {
                    return settings.GetTemplatePath(templateAttr.template);
                }

                default:
                    return "NONE";
            }
        }

        private TemplateDefinition GetTemplateSource(ProcessedType processedType) {
            TemplateAttribute templateAttr = processedType.templateAttr;

            string templatePath = GetTemplateFilePath(processedType);

            switch (templateAttr.templateType) {
                case TemplateType.Internal: {
                    string file = settings.TryReadFile(templatePath);

                    if (file == null) {
                        throw new TemplateParseException(settings.templateResolutionBasePath, $"Cannot find template in (internal) path {templatePath}.");
                    }

                    return new TemplateDefinition() {
                        contents = file,
                        filePath = templateAttr.templateType == TemplateType.File ? processedType.rawType.AssemblyQualifiedName : templateAttr.template,
                        language = TemplateLanguage.XML
                    };
                }

                case TemplateType.File: {
                    string file = settings.TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(settings.templateResolutionBasePath, $"Cannot find template in path {templatePath}.");
                    }

                    return new TemplateDefinition() {
                        contents = file,
                        filePath = templateAttr.template,
                        language = TemplateLanguage.XML
                    };
                }

                default:
                    return new TemplateDefinition() {
                        contents = templateAttr.template,
                        filePath = templatePath,
                        language = TemplateLanguage.XML
                    };
            }
        }

    }

}