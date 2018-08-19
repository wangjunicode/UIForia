using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Rendering;
using Src.Parsing.Style;
using Src.Style;
using UnityEngine;

namespace Src {

    public class TemplateParser {

        private string TemplateName = string.Empty;

        private readonly ParsedTemplate output = new ParsedTemplate();

        private static readonly ExpressionParser expressionParser = new ExpressionParser();

        private static readonly Dictionary<Type, ParsedTemplate> parsedTemplates =
            new Dictionary<Type, ParsedTemplate>();

        private static readonly string[] RepeatAttributes = { "list", "as", "filter", "onItemAdded", "onItemRemoved" };
        private static readonly string[] CaseAttributes = { "when" };
        private static readonly string[] PrefabAttributes = { "if", "src" };
        private static readonly string[] SwitchAttributes = { "if", "value" };
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
            string template = File.ReadAllText(Application.dataPath + processedType.GetTemplatePath());
            XDocument doc = XDocument.Parse(template);
            return new TemplateParser().ParseTemplate(processedType, doc);
        }

        public static ParsedTemplate ParseTemplateFromString<T>(string input) {
            XDocument doc = XDocument.Parse(input);
            ProcessedType processedType = TypeProcessor.GetType(typeof(T));
            return new TemplateParser().ParseTemplate(processedType, doc);
        }

        private UIStyle ParseStyleSheet(XElement styleElement) {
            XAttribute idAttr = styleElement.GetAttribute("id");

            if (idAttr == null || string.IsNullOrEmpty(idAttr.Value)) {
                throw new InvalidTemplateException(TemplateName, "Style tags require an 'id' attribute");
            }

            UIStyle styleTemplate = new UIStyle(idAttr.Value.Trim(), TemplateName);
            StyleParser.ParseStyle(styleElement, styleTemplate);
            return styleTemplate;
        }

        private ParsedTemplate ParseTemplate(ProcessedType type, XDocument doc) {
            TemplateName = type.GetTemplatePath();

            doc.MergeTextNodes();

            List<ImportDeclaration> imports = new List<ImportDeclaration>();
            List<UIStyle> styleTemplates = new List<UIStyle>();

            IEnumerable<XElement> importElements = doc.Root.GetChildren("Import");
            foreach (var xElement in importElements) {
                XAttribute pathAttr = xElement.GetAttribute("path");
                XAttribute aliasAttr = xElement.GetAttribute("as");

                if (pathAttr == null || string.IsNullOrEmpty(pathAttr.Value)) {
                    throw new InvalidTemplateException(TemplateName, "Import node without a 'path' attribute");
                }

                if (aliasAttr == null || string.IsNullOrEmpty(aliasAttr.Value)) {
                    throw new InvalidTemplateException(TemplateName, "Import node without an 'as' attribute");
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
                throw new InvalidTemplateException(TemplateName, " missing a 'Contents' section");
            }

            output.type = type.rawType;
            output.imports = imports;
            output.styles = styleTemplates;
            output.filePath = TemplateName;
            output.contextDefinition = new ContextDefinition(type.rawType);
            List<UITemplate> children = ParseNodes(contentElement.Nodes());

            output.rootElement = new UIElementTemplate();
            output.rootElement.childTemplates = children;
            output.rootElement.processedElementType = type;

            return output;
        }

        private UITemplate ParseCaseElement(XElement element) {
            EnsureAttribute(element, "when");
            EnsureOnlyAttributes(element, CaseAttributes);

            UISwitchCaseTemplate template = new UISwitchCaseTemplate();
            template.childTemplates = ParseNodes(element.Nodes());
            return template;
        }

        private UITemplate ParseDefaultElement(XElement element) {

            EnsureOnlyAttributes(element, DefaultAttributes);

            UISwitchDefaultTemplate template = new UISwitchDefaultTemplate();
            template.childTemplates = ParseNodes(element.Nodes());
            return template;
        }

        private UITemplate ParseRepeatElement(XElement element) {
            EnsureAttribute(element, "list");
            EnsureNotInsideTagName(element, "Repeat");
            EnsureOnlyAttributes(element, RepeatAttributes);

            UIRepeatTemplate template = new UIRepeatTemplate();
            template.attributes = ParseAttributes(element.Attributes());
            template.childTemplates = ParseNodes(element.Nodes());

            return template;
        }

        private UITemplate ParseGroupElement(XElement element) {
            UIGroupTemplate template = new UIGroupTemplate();
            template.attributes = ParseAttributes(element.Attributes());
            template.childTemplates = ParseNodes(element.Nodes());
            return template;
        }

        private UITemplate ParseSlotElement(XElement element) {
            Abort("<Slot> not yet implemented");
            EnsureNotInsideTagName(element, "Repeat");
            EnsureOnlyAttributes(element, ChildrenAttributes);
            return null;
        }

        private UITemplate ParseChildrenElement(XElement element) {
            EnsureEmpty(element);
            EnsureNotInsideTagName(element, "Repeat");
            EnsureOnlyAttributes(element, ChildrenAttributes);
            return new UIChildrenTemplate();
        }

        private UITemplate ParseSwitchElement(XElement element) {
            EnsureAttribute(element, "value");
            EnsureOnlyAttributes(element, SwitchAttributes);

            // can only contain <Case> and <Default>
            UISwitchTemplate template = new UISwitchTemplate();

            template.childTemplates = ParseNodes(element.Nodes());

            if (template.childTemplates.Count == 0) {
                throw Abort("<Switch> cannot be empty");
            }

            bool hasDefault = false;
            for (int i = 0; i < template.childTemplates.Count; i++) {
                Type elementType = template.childTemplates[i].ElementType;
                if (elementType == typeof(UISwitchDefaultElement)) {
                    if (hasDefault) {
                        throw Abort("<Switch> can only contain one <Default> element");
                    }

                    hasDefault = true;
                }

                if (elementType != typeof(UISwitchDefaultElement) && elementType != typeof(UISwitchCaseElement)) {
                    throw Abort("<Switch> can only contain <Case> and <Default> elements");
                }
            }

            return template;
        }

        private UITemplate ParsePrefabElement(XElement element) {
            EnsureEmpty(element);
            EnsureOnlyAttributes(element, PrefabAttributes);

            UIPrefabTemplate template = new UIPrefabTemplate();
            template.attributes = ParseAttributes(element.Attributes());

            return template;
        }

        private UITextTemplate ParseTextNode(XText node) {
            // todo split nodes based on inline {expressions}
            return new UITextTemplate(node.ToString().Trim());
        }

        private UITemplate ParseTemplateElement(XElement element) {
            UITemplate template = new UIElementTemplate();

            template.processedElementType = TypeProcessor.GetType(element.Name.LocalName, output.imports);
            template.attributes = ParseAttributes(element.Attributes());
            template.childTemplates = ParseNodes(element.Nodes());

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

            if (element.Name == "Group") {
                return ParseGroupElement(element);
            }

            return ParseTemplateElement(element);
        }

        private List<UITemplate> ParseNodes(IEnumerable<XNode> nodes) {
            List<UITemplate> retn = new List<UITemplate>();
            foreach (var node in nodes) {

                switch (node.NodeType) {
                    case XmlNodeType.Text:
                        retn.Add(ParseTextNode((XText) node));
                        continue;

                    case XmlNodeType.Element:
                        retn.Add(ParseElement((XElement) node));
                        continue;
                }

                throw new InvalidTemplateException(TemplateName, "Unable to handle node type: " + node.NodeType);
            }

            return retn;
        }

        private List<AttributeDefinition> ParseAttributes(IEnumerable<XAttribute> attributes) {
            return attributes.Select(attr => new AttributeDefinition(attr.Name.LocalName, attr.Value.Trim())).ToList();
        }

        private InvalidTemplateException Abort(string message) {
            return new InvalidTemplateException(TemplateName, message);
        }

        private void EnsureAttribute(XElement element, string attrName) {
            if (element.GetAttribute(attrName) == null) {
                throw new InvalidTemplateException(TemplateName,
                    $"<{element.Name.LocalName}> is missing required attribute '{attrName}'");
            }
        }

        private void EnsureMissingAttribute(XElement element, string attrName) {
            if (element.GetAttribute(attrName) != null) {
                throw new InvalidTemplateException(TemplateName,
                    $"<{element.Name.LocalName}> is not allowed to have attribute '{attrName}'");
            }
        }

        private void EnsureOnlyAttributes(XElement element, string[] attrs) {
            foreach (var attr in element.Attributes()) {
                if (!attrs.Contains(attr.Name.LocalName)) {
                    throw Abort($"<{element.Name.LocalName}> cannot have attribute: '{attr.Name.LocalName}");
                }
            }
        }

        private void EnsureEmpty(XElement element) {
            if (!element.IsEmpty) {
                throw new InvalidTemplateException(TemplateName,
                    $"<{element.Name.LocalName}> tags cannot have children");
            }
        }

        private void EnsureNotInsideTagName(XElement element, string tagName) {
            XElement ptr = element;

            while (ptr.Parent != null) {
                if (ptr.Parent.Name.LocalName == tagName) {
                    throw new InvalidTemplateException(TemplateName,
                        $"<{element.Name.LocalName}> cannot be inside <{tagName}>");
                }

                ptr = ptr.Parent;
            }
        }

    }

}