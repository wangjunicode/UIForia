using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Rendering;
using Src.Parsing.Style;
using Src.Style;

namespace Src {

    public class TemplateParser {

        private string templateName = string.Empty;

        private static readonly Dictionary<Type, ParsedTemplate> parsedTemplates =
            new Dictionary<Type, ParsedTemplate>();

        private static readonly string[] RepeatAttributes = { "x-if", "list", "as", "filter", "onItemAdded", "onItemRemoved" };
        private static readonly string[] CaseAttributes = { "when" };
        private static readonly string[] PrefabAttributes = { "x-if", "src" };
        private static readonly string[] SwitchAttributes = { "x-if", "value" };
        private static readonly string[] DefaultAttributes = { };
        private static readonly string[] ChildrenAttributes = { };
        private static readonly string[] TextAttributes = { };

        public static ParsedTemplate GetParsedTemplate(ProcessedType processedType, bool forceReload = false) {
            return GetParsedTemplate(processedType.rawType, forceReload);
        }

        public static ParsedTemplate GetParsedTemplate(Type elementType, bool forceReload = false) {
            if (!forceReload && parsedTemplates.ContainsKey(elementType)) {
                return parsedTemplates[elementType];
            }

            ParsedTemplate parsedTemplate = ParseTemplateFromType(elementType);
            parsedTemplates[elementType] = parsedTemplate;
            return parsedTemplate;
        }

        public ParsedTemplate GetParsedTemplate<T>(bool forceReload = false) where T : UIElement {
            return GetParsedTemplate(typeof(T), forceReload);
        }

        public static ParsedTemplate ParseTemplateFromType(Type type) {
            ProcessedType processedType = TypeProcessor.GetType(type);

            string template = processedType.GetTemplate();
            XDocument doc = XDocument.Parse(template);
            ParsedTemplate parsedTemplate = null;

            try {
                parsedTemplate = new TemplateParser().ParseTemplate(processedType, doc);
            }
            catch (InvalidTemplateException ex) {
                throw new InvalidTemplateException(template, ex.Message);
            }

            return parsedTemplate;
        }

        public static ParsedTemplate ParseTemplateFromString<T>(string input) where T : UIElement {
            XDocument doc = XDocument.Parse(input);
            ProcessedType processedType = TypeProcessor.GetType(typeof(T));
            return new TemplateParser().ParseTemplate(processedType, doc);
        }
        
        public static ParsedTemplate ParseTemplateFromString(Type rootType, string input) {
            XDocument doc = XDocument.Parse(input);
            ProcessedType processedType = TypeProcessor.GetType(rootType);
            return new TemplateParser().ParseTemplate(processedType, doc);
        }

        private UIStyle ParseStyleSheet(XElement styleElement) {
            XAttribute idAttr = styleElement.GetAttribute("id");

            if (idAttr == null || string.IsNullOrEmpty(idAttr.Value)) {
                throw new InvalidTemplateException(templateName, "Style tags require an 'id' attribute");
            }

            UIStyle styleTemplate = new UIStyle(idAttr.Value.Trim(), templateName);
            StyleParser.ParseStyle(styleElement, styleTemplate);
            return styleTemplate;
        }

        private ParsedTemplate ParseTemplate(ProcessedType type, XDocument doc) {
            templateName = type.GetTemplateName();

            doc.MergeTextNodes();

            List<ImportDeclaration> imports = new List<ImportDeclaration>();
            List<UIStyle> styleTemplates = new List<UIStyle>();

            IEnumerable<XElement> importElements = doc.Root.GetChildren("Import");
            foreach (var xElement in importElements) {
                XAttribute pathAttr = xElement.GetAttribute("path");
                XAttribute aliasAttr = xElement.GetAttribute("as");

                if (pathAttr == null || string.IsNullOrEmpty(pathAttr.Value)) {
                    throw new InvalidTemplateException(templateName, "Import node without a 'path' attribute");
                }

                if (aliasAttr == null || string.IsNullOrEmpty(aliasAttr.Value)) {
                    throw new InvalidTemplateException(templateName, "Import node without an 'as' attribute");
                }

                imports.Add(new ImportDeclaration(pathAttr.Value, aliasAttr.Value));
            }

            IEnumerable<XElement> styleElements = doc.Root.GetChildren("Style");
            foreach (var styleElement in styleElements) {
                // todo -- return a list of UIStyles and flags for their state
                UIStyle styleTemplate = ParseStyleSheet(styleElement);

                styleTemplates.Add(styleTemplate);
            }

            XElement contentElement = doc.Root.GetChild("Contents");
            if (contentElement == null) {
                throw new InvalidTemplateException(templateName, " missing a 'Contents' section");
            }

            List<UITemplate> children = ParseNodes(contentElement.Nodes());
            List<AttributeDefinition> attributes = ParseAttributes(contentElement.Attributes());

            UIElementTemplate rootTemplate = new UIElementTemplate(type.rawType, children, attributes);

            ParsedTemplate output = new ParsedTemplate(rootTemplate);
            output.imports = imports;
            output.styles = styleTemplates;
            output.filePath = templateName;
            return output;
        }

        private static UITemplate ParseCaseElement(XElement element) {
            EnsureAttribute(element, "when");
            EnsureOnlyAttributes(element, CaseAttributes);

            UISwitchCaseTemplate template = new UISwitchCaseTemplate(
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );

            return template;
        }

        private static UITemplate ParseDefaultElement(XElement element) {
            EnsureOnlyAttributes(element, DefaultAttributes);

            UISwitchDefaultTemplate template = new UISwitchDefaultTemplate(
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );
            return template;
        }

        private static UITemplate ParseRepeatElement(XElement element) {
            EnsureAttribute(element, "list");
            // this isn't actually a restriction anymore
            EnsureNotInsideTagName(element, "Repeat");
            EnsureOnlyAttributes(element, RepeatAttributes);

            UIRepeatTemplate template = new UIRepeatTemplate(
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );

            return template;
        }

        private static UITemplate ParseGroupElement(XElement element) {
            UIGroupTemplate template = new UIGroupTemplate(
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );
            return template;
        }

        private static UITemplate ParseSlotElement(XElement element) {
            Abort("<Slot> not yet implemented");
            EnsureNotInsideTagName(element, "Repeat");
            EnsureOnlyAttributes(element, ChildrenAttributes);
            return null;
        }

        private static UITemplate ParseChildrenElement(XElement element) {
            EnsureEmpty(element);
            EnsureNotInsideTagName(element, "Repeat");
            EnsureOnlyAttributes(element, ChildrenAttributes);
            return new UIChildrenTemplate();
        }

        private static UITemplate ParseSwitchElement(XElement element) {
            EnsureAttribute(element, "value");
            EnsureOnlyAttributes(element, SwitchAttributes);

            // can only contain <Case> and <Default>
            UISwitchTemplate template = new UISwitchTemplate(
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );

            if (template.childTemplates.Count == 0) {
                throw Abort("<Switch> cannot be empty");
            }

            bool hasDefault = false;
            for (int i = 0; i < template.childTemplates.Count; i++) {
                Type templateType = template.childTemplates[i].GetType();
                if (templateType == typeof(UISwitchDefaultTemplate)) {
                    if (hasDefault) {
                        throw Abort("<Switch> can only contain one <Default> element");
                    }

                    hasDefault = true;
                }

                if (templateType != typeof(UISwitchDefaultTemplate) && templateType != typeof(UISwitchCaseTemplate)) {
                    throw Abort("<Switch> can only contain <Case> and <Default> elements");
                }
            }

            return template;
        }

        private static UITemplate ParsePrefabElement(XElement element) {
            EnsureEmpty(element);
            EnsureOnlyAttributes(element, PrefabAttributes);

            UIPrefabTemplate template = new UIPrefabTemplate(ParseAttributes(element.Attributes()));

            return template;
        }

        private static UITextTemplate ParseTextNode(XText node) {
            // todo split nodes based on inline {expressions}
            return new UITextTemplate("'" + node.Value.Trim() + "'");
        }

        private static UITemplate ParseTemplateElement(XElement element) {
            UITemplate template = new UIElementTemplate(
                element.Name.LocalName,
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );

            return template;
        }

        private static UITemplate ParseTextContainerElement(XElement element) {
            UITemplate template = new UITextContainerTemplate(
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );
            return template;
        }
        
        private static UITemplate ParseGraphicElement(XElement element) {
            UITemplate template = new UIGraphicTemplate(
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );
            return template;
        }

        private static UITemplate ParseMaskElement(XElement element) {
            throw new NotImplementedException();    
        }
        
        private static UITemplate ParseShapeElement(XElement element) {
            throw new NotImplementedException();    
            UITemplate template = new UIShapeTemplate(
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );
            return template;
        }

        private static UITemplate ParseImageElement(XElement element) {
            return new UIImageTemplate(null, ParseAttributes(element.Attributes()));    
        }

        private static UITemplate ParseElement(XElement element) {
            
            if (element.Name == "Children") {
                return ParseChildrenElement(element);
            }

            if (element.Name == "Text") {
                return ParseTextContainerElement(element);
            }

            if (element.Name == "Graphic") {
                return ParseGraphicElement(element);
            }
            
//            if (element.Name == "Paragraph") { }
            
//            if (element.Name == "Heading1") { }
//            if (element.Name == "Heading2") { }
//            if (element.Name == "Heading3") { }
//            if (element.Name == "Heading4") { }
//            if (element.Name == "Heading5") { }
//            if (element.Name == "Heading6") { }
//
//            if (element.Name == "UnorderedList") { }
//            if (element.Name == "OrderedList") { }
//
//            if (element.Name == "ListItem") { }

            if (element.Name == "Mask") {
                return ParseMaskElement(element);
            }
            
            if (element.Name == "Image") {
                return ParseImageElement(element);
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

            if (element.Name == "Group") {
                return ParseGroupElement(element);
            }

            return ParseTemplateElement(element);
        }

        private static List<UITemplate> ParseNodes(IEnumerable<XNode> nodes) {
            List<UITemplate> retn = new List<UITemplate>();
            foreach (var node in nodes) {
                switch (node.NodeType) {
                    case XmlNodeType.Text:
                        retn.Add(ParseTextNode((XText) node));
                        continue;

                    case XmlNodeType.Element:
                        retn.Add(ParseElement((XElement) node));
                        continue;

                    case XmlNodeType.Comment:
                        continue;
                }

                throw new InvalidTemplateException("Unable to handle node type: " + node.NodeType);
            }

            return retn;
        }

        private static List<AttributeDefinition> ParseAttributes(IEnumerable<XAttribute> attributes) {
            return attributes.Select(attr => new AttributeDefinition(attr.Name.LocalName, attr.Value.Trim())).ToList();
        }

        private static InvalidTemplateException Abort(string message) {
            return new InvalidTemplateException(message);
        }

        private static void EnsureAttribute(XElement element, string attrName) {
            if (element.GetAttribute(attrName) == null) {
                throw new InvalidTemplateException(
                    $"<{element.Name.LocalName}> is missing required attribute '{attrName}'");
            }
        }

        private static void EnsureMissingAttribute(XElement element, string attrName) {
            if (element.GetAttribute(attrName) != null) {
                throw new InvalidTemplateException(
                    $"<{element.Name.LocalName}> is not allowed to have attribute '{attrName}'");
            }
        }

        private static void EnsureOnlyAttributes(XElement element, string[] attrs) {
            foreach (var attr in element.Attributes()) {
                if (!attrs.Contains(attr.Name.LocalName)) {
                    throw Abort($"<{element.Name.LocalName}> cannot have attribute: '{attr.Name.LocalName}");
                }
            }
        }

        private static void EnsureEmpty(XElement element) {
            if (!element.IsEmpty) {
                throw new InvalidTemplateException(
                    $"<{element.Name.LocalName}> tags cannot have children");
            }
        }

        private static void EnsureNotInsideTagName(XElement element, string tagName) {
            XElement ptr = element;

            while (ptr.Parent != null) {
                if (ptr.Parent.Name.LocalName == tagName) {
                    throw new InvalidTemplateException(
                        $"<{element.Name.LocalName}> cannot be inside <{tagName}>");
                }

                ptr = ptr.Parent;
            }
        }

    }

}