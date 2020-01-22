using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UIForia.Attributes;
using UIForia.Exceptions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class TemplateCache {

        private readonly TemplateSettings settings;
        private readonly XMLTemplateParser xmlTemplateParser;
        private readonly Dictionary<string, LightList<TemplateRootNode>> templateMap;

        public TemplateCache(TemplateSettings settings) {
            this.settings = settings;
            this.xmlTemplateParser = new XMLTemplateParser();
            this.templateMap = new Dictionary<string, LightList<TemplateRootNode>>(37);
        }

        public string ResolveDefaultFilePath(ProcessedType processedType) {
            if (settings.filePathResolver != null) {
                return settings.filePathResolver(processedType.rawType, processedType.templateAttr.templateId);
            }

            // expected is templateroot/typename/typename.xml if template id == null
            // or templateroot/typename/typename/typename.xml#templateid
            string namespaceName = processedType.rawType.Namespace;
 
            if (namespaceName != null && namespaceName.Contains(".")) {
                namespaceName = namespaceName.Replace(".", Path.PathSeparator.ToString());
            }

            string basePath = null;
            
            if (namespaceName == null) {
                basePath = Path.Combine(settings.templateRoot, processedType.rawType.Name, processedType.rawType.Name);
            }
            else {
                basePath = Path.Combine(settings.templateRoot, namespaceName, processedType.rawType.Name, processedType.rawType.Name);
            }
            
            // todo -- support more extensions
            string xmlPath = Path.GetFullPath(Path.Combine(settings.templateResolutionBasePath, basePath + ".xml"));
            
            if (File.Exists(xmlPath)) {
                basePath += ".xml";
            }
            else {
                throw new TemplateNotFoundException(processedType, xmlPath);
            }
            
            return basePath;
        }

        public TemplateRootNode GetParsedTemplate(ProcessedType processedType) {
            TemplateAttribute templateAttr = processedType.templateAttr;

            if (templateAttr.fullPathId == null && templateAttr.templateType == TemplateType.DefaultFile) {
                templateAttr.filePath = ResolveDefaultFilePath(processedType);
                templateAttr.fullPathId = templateAttr.templateId == null
                    ? templateAttr.filePath
                    : templateAttr.filePath + "#" + templateAttr.templateId;
            }

            Debug.Assert(templateAttr.fullPathId != null, "templateAttr.fullPathId != null");
            
            if (templateMap.TryGetValue(templateAttr.fullPathId, out LightList<TemplateRootNode> list)) {

                for (int i = 0; i < list.size; i++) {
                    
                    if (list.array[i].processedType.rawType == processedType.rawType) {
                        return list.array[i];
                    }     
                    
                }

                TemplateRootNode retn = list[0].Clone(processedType);
                list.Add(retn);
                return retn;

            }

            list = new LightList<TemplateRootNode>(2);
            
            templateMap[templateAttr.fullPathId] = list;

            TemplateDefinition templateDefinition = GetTemplateDefinition(processedType);

            templateAttr.source = templateDefinition.contents;
            
            TemplateShell shell = xmlTemplateParser.GetOuterTemplateShell(templateAttr);
            
            TemplateRootNode templateRootNode = new TemplateRootNode(templateAttr.templateId, shell, processedType, null, default); //, attributes, new TemplateLineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition));

            list.Add(templateRootNode);

            xmlTemplateParser.Parse(templateRootNode, processedType);

            return templateRootNode;
        }

        private string ResolveTemplateFilePath(TemplateType templateType, string filepath) {
            switch (templateType) {
                case TemplateType.DefaultFile: {
                    return settings.GetTemplatePath(filepath);
                }
                case TemplateType.Internal: {
                    return settings.GetInternalTemplatePath(filepath);
                }

                case TemplateType.File: {
                    return settings.GetTemplatePath(filepath);
                }

                default:
                    return "NONE";
            }
        }

        private TemplateDefinition GetTemplateDefinition(ProcessedType processedType) {
            TemplateAttribute templateAttr = processedType.templateAttr;

            string templatePath = ResolveTemplateFilePath(templateAttr.templateType, templateAttr.filePath);

            switch (templateAttr.templateType) {
                case TemplateType.Internal: {
                    string file = settings.TryReadFile(templatePath);

                    if (file == null) {
                        throw new TemplateParseException(settings.templateResolutionBasePath, $"Cannot find template in (internal) path {templatePath}.");
                    }

                    return new TemplateDefinition() {
                        contents = file,
                        filePath = templateAttr.templateType == TemplateType.File ? processedType.rawType.AssemblyQualifiedName : templateAttr.filePath,
                        language = TemplateLanguage.XML
                    };
                }

                case TemplateType.DefaultFile: 
                case TemplateType.File: {
                    string file = settings.TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(settings.templateResolutionBasePath, $"Cannot find template in path {templatePath}.");
                    }

                    return new TemplateDefinition() {
                        contents = file,
                        filePath = templateAttr.filePath,
                        language = TemplateLanguage.XML
                    };
                }

                default:
                    return new TemplateDefinition() {
                        contents = templateAttr.source,
                        filePath = templatePath,
                        language = TemplateLanguage.XML
                    };
            }
        }

    }

}