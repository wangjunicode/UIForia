using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Src.Parsing.Style;
using Src.Style;
using UnityEngine;

namespace Src {

    public class TemplateParser {

        private string TemplateName = string.Empty;

        private readonly ParsedTemplate output = new ParsedTemplate();
        private int repeatCount;
        
        private static readonly ExpressionParser expressionParser = new ExpressionParser();

        private static readonly Dictionary<Type, ParsedTemplate> parsedTemplates =
            new Dictionary<Type, ParsedTemplate>();


        public static ParsedTemplate GetParsedTemplate(ProcessedType processedType, bool forceReload = false) {
            return GetParsedTemplate(processedType.type, forceReload);
        }
        
        public static ParsedTemplate GetParsedTemplate(Type elementType, bool forceReload = false) {
            if (!forceReload && parsedTemplates.ContainsKey(elementType)) {
                return parsedTemplates[elementType];
            }

            TemplateParser parser = new TemplateParser();
            ProcessedType type = TypeProcessor.GetType(elementType);
            string template = File.ReadAllText(Application.dataPath + type.GetTemplatePath());
            ParsedTemplate parsedTemplate = parser.ParseTemplate(type, template);
            parsedTemplates[elementType] = parsedTemplate;
            return parsedTemplate;
        }

        public ParsedTemplate GetParsedTemplate<T>(bool forceReload = false) where T : UIElement {
            return GetParsedTemplate(typeof(T), forceReload);
        }

        private StyleTemplate ParseStyleSheet(XElement root) {
            StyleTemplate styleTemplate = new StyleTemplate();
            TextStyleParser.ParseStyle(root.GetChild("Text"), styleTemplate);
            PaintStyleParser.ParseStyle(root.GetChild("Paint"), styleTemplate);
            AnimationStyleParser.ParseStyle(root.GetChild("Animations"), styleTemplate);
            SizeStyleParser.ParseStyle(root.GetChild("Size"), styleTemplate);
            LayoutStyleParser.ParseStyle(root.GetChild("Layout"), styleTemplate);
            LayoutItemStyleParser.ParseStyle(root.GetChild("LayoutItem"), styleTemplate);
            return styleTemplate;
        }

        private ParsedTemplate ParseTemplate(ProcessedType type, string template) {
            TemplateName = type.GetTemplatePath();

            XDocument doc = XDocument.Parse(template);
            doc.MergeTextNodes();

            List<ImportDeclaration> imports = new List<ImportDeclaration>();
            List<StyleTemplate> styleTemplates = new List<StyleTemplate>();

            IEnumerable<XElement> importElements = doc.Root.GetChildren("Import");
            foreach (var xElement in importElements) {
                XAttribute pathAttr = xElement.Attributes().FirstOrDefault((a) => a.Name == "path");
                XAttribute aliasAttr = xElement.Attributes().FirstOrDefault((a) => a.Name == "as");
                if (pathAttr == null || string.IsNullOrEmpty(pathAttr.Value)) {
                    throw new Exception("Import node without a 'path' attribute");
                }

                if (aliasAttr == null || string.IsNullOrEmpty(aliasAttr.Value)) {
                    throw new Exception("Import node without an 'as' attribute");
                }

                imports.Add(new ImportDeclaration(pathAttr.Value, aliasAttr.Value));
            }

            IEnumerable<XElement> styleElements = doc.Root.GetChildren("Style");
            foreach (var styleElement in styleElements) {
                XAttribute idAttr = styleElement.Attributes().FirstOrDefault((a) => a.Name == "id");
                XAttribute extendsAttr = styleElement.Attributes().FirstOrDefault((a) => a.Name == "extends");
                XAttribute fromAttr = styleElement.Attributes().FirstOrDefault((a) => a.Name == "from");

                if (idAttr == null || string.IsNullOrEmpty(idAttr.Value)) {
                    throw new Exception("Style tags require an 'id' attribute");
                }

                if (styleTemplates.Find((t) => t.id == idAttr.Value.Trim()) != null) {
                    throw new Exception("Style tags must have a unique id");
                }

                StyleTemplate styleTemplate = ParseStyleSheet(styleElement);
                styleTemplate.id = idAttr.Value.Trim();
                styleTemplate.extendsId = extendsAttr?.Value.Trim();
                styleTemplate.extendsPath = fromAttr?.Value.Trim();
                styleTemplates.Add(styleTemplate);
            }

            XElement contentElement = doc.Root.GetChild("Contents");
            if (contentElement == null) {
                throw new Exception($"Template {TemplateName} is missing a 'Contents' section");
            }

            output.type = type.type;
            output.imports = imports;
            output.styles = styleTemplates;
            output.filePath = TemplateName;
            output.contexts = new List<ContextDefinition>();
            output.rootElement = (UIElementTemplate) ParseElement(contentElement);

            return output;
        }

        private UITemplate ParseCaseElement(XElement element) {
            // must be direct child of switch
            return null;
        }

        private UITemplate ParseDefaultElement(XElement element) {
            // must be direct child of switch
            return null;
        }

        private UITemplate ParseRepeatElement(XElement element) {
            // cannot be in a repeat
            return null;
        }

        private UITemplate ParseSlotElement(XElement element) {
            // cannot be in a repeat
            return null;
        }

        private UITemplate ParseChildrenElement(XElement element) {
            // cannot be in a repeat
            return null;
        }

        private UITemplate ParseSwitchElement(XElement element) {
            // can only contain <Case> and <Default>
            return null;
        }

        private UITemplate ParsePrefabElement(XElement element) {
            return null;
        }

        private UITextTemplate ParseTextNode(XText node) {
            return null;
        }

        private UITemplate ParseTemplateElement(XElement element) {
            UITemplate template = new UIElementTemplate();

            template.processedElementType = TypeProcessor.GetType(element.Name.LocalName, output.imports);
            template.attributes = ParseAttributes(element.Attributes());
            template.childTemplates = ParseNodes(element.Nodes());

            for (int i = 0; i < template.childTemplates.Count; i++) {
                template.childTemplates[i].parent = template;
            }

            return template;
        }

        private UITemplate ParseElement(XElement element) {
            if (element.Name == "Children") {
                return ParseChildrenElement(element);
            }

            if (element.Name == "Repeat") {
                return ParseRepeatElement(element);
            }

            if (element.Name == "Slot") {
                return ParseSlotElement(element);
            }

            if (element.Name == "Switch") {
                return ParseSwitchElement(element);
            }

            if (element.Name == "Prefab") {
                return ParsePrefabElement(element);
            }

            if (element.Name == "Default") {
                return ParseDefaultElement(element);
            }

            if (element.Name == "Case") {
                return ParseCaseElement(element);
            }

            return ParseTemplateElement(element);
        }

        private List<UITemplate> ParseNodes(IEnumerable<XNode> nodes) {
            List<UITemplate> retn = new List<UITemplate>();
            foreach (var node in nodes) {
                if (node.NodeType == XmlNodeType.Text) {
                    retn.Add(ParseTextNode((XText) node));
                }
                else if (node.NodeType == XmlNodeType.Element) {
                    retn.Add(ParseElement((XElement) node));
                }

                throw new Exception("Unable to handle node type: " + node.NodeType);
            }

            return retn;
        }

        private List<AttributeDefinition> ParseAttributes(IEnumerable<XAttribute> attributes) {
            List<AttributeDefinition> retn = new List<AttributeDefinition>();
            foreach (var attr in attributes) {
                AttributeDefinition attrDef = new AttributeDefinition(attr.Name.LocalName, attr.Value.Trim());
                attrDef.bindingExpression = expressionParser.Parse(attrDef.value);
                retn.Add(attrDef);
            }

            return retn;
        }

    }

}