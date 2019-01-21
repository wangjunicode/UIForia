using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UIForia.Style;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia {

    public class TemplateParser {

        private static readonly Dictionary<Type, ParsedTemplate> parsedTemplates = new Dictionary<Type, ParsedTemplate>();

        private static readonly string[] RepeatAttributes = {"if", "style", "x-id", "list", "as", "filter", "onItemAdded", "onItemRemoved"};
        private static readonly string[] CaseAttributes = {"when"};
        private static readonly string[] SwitchAttributes = {"if", "value"};
        private static readonly string[] DefaultAttributes = { };
        private static readonly string[] ChildrenAttributes = {"style"};
        private static readonly string[] TextAttributes = { };

        public static void Reset() {
            parsedTemplates.Clear();
        }

        public static ParsedTemplate GetParsedTemplate(Type elementType, bool forceReload = false) {
            if (!forceReload && parsedTemplates.ContainsKey(elementType)) {
                return parsedTemplates[elementType];
            }

            ParsedTemplate parsedTemplate = ParseTemplateFromType(elementType);
            parsedTemplates[elementType] = parsedTemplate;
            return parsedTemplate;
        }

        public static ParsedTemplate ParseTemplateFromString<T>(string input) where T : UIElement {
            XDocument doc = XDocument.Parse(input);
            return ParseTemplate(null, typeof(T), doc);
        }

        public static ParsedTemplate ParseTemplateFromString(Type rootType, string input) {
            XDocument doc = XDocument.Parse(input);
            return ParseTemplate(rootType.AssemblyQualifiedName, rootType, doc);
        }

        private static ParsedTemplate ParseTemplateFromType(Type type) {
            ProcessedType processedType = TypeProcessor.GetType(type);

            string template = processedType.GetTemplate();
            XDocument doc = XDocument.Parse(template);
            ParsedTemplate parsedTemplate;

            try {
                parsedTemplate = ParseTemplate(processedType.GetTemplatePath(), processedType.rawType, doc);
            }
            catch (InvalidTemplateException ex) {
                if (ex.templatePath == null) {
                    ex.templatePath = processedType.GetTemplatePath();
                }
                throw;
            }
            catch (ParseException pe) {
                throw new InvalidTemplateException(processedType.GetTemplatePath(), pe);
            }

            return parsedTemplate;
        }

        private static StyleDefinition ParseStyleSheet(string templateId, XElement styleElement) {
            XAttribute aliasAttr = styleElement.GetAttribute("alias");
            XAttribute importPathAttr = styleElement.GetAttribute("path");

            string rawText = string.Empty;
            // styles can have either a class path or a body
            foreach (XNode node in styleElement.Nodes()) {
                switch (node.NodeType) {
                    case XmlNodeType.Text:
                        rawText += ((XText) node).Value;
                        continue;

                    case XmlNodeType.Element:
                        throw new Exception("<Style> can only have text children, no elements");

                    case XmlNodeType.Comment:
                        continue;
                }

                throw new InvalidTemplateException("Unable to handle node type: " + node.NodeType);
            }

            string alias = StyleDefinition.k_EmptyAliasName;

            if (aliasAttr != null && !string.IsNullOrEmpty(aliasAttr.Value)) {
                alias = aliasAttr.Value.Trim();
            }

            // if we have a body, expect import path to be null
            if (!string.IsNullOrEmpty(rawText) && !string.IsNullOrWhiteSpace(rawText)) {
                if (importPathAttr != null && !string.IsNullOrEmpty(importPathAttr.Value)) {
                    throw new UIForia.ParseException("Expected 'path' to be null when a body is provided to a style tag");
                }

                return new StyleDefinition(alias, templateId, rawText);
            }

            // if we have no body then expect path to be set
            if (importPathAttr == null || string.IsNullOrEmpty(importPathAttr.Value)) {
                throw new UIForia.ParseException("Expected 'path' to be provided when a body is not provided in a style tag");
            }

            return new StyleDefinition(alias, importPathAttr.Value.Trim());
        }

        private static ParsedTemplate ParseTemplate(string templatePath, Type type, XDocument doc) {
            doc.MergeTextNodes();

            List<ImportDeclaration> imports = new List<ImportDeclaration>();
            List<StyleDefinition> styleTemplates = new List<StyleDefinition>();

            IEnumerable<XElement> importElements = doc.Root.GetChildren("Import");
            IEnumerable<XElement> styleElements = doc.Root.GetChildren("Style");
            IEnumerable<XElement> usingElements = doc.Root.GetChildren("Using");
            XElement contentElement = doc.Root.GetChild("Contents");

            // todo -- we can't use any bindings on the <Content/> tag because then the binding system
            // todo    would need to hold 2 different contexts depending on where the binding came from
            // todo    (ie the parsed template or the element root itself)
            List<string> usings = ListPool<string>.Get();

            foreach (XElement xElement in importElements) {
                XAttribute valueAttr = xElement.GetAttribute("value");
                XAttribute aliasAttr = xElement.GetAttribute("as");

                if (valueAttr == null || string.IsNullOrEmpty(valueAttr.Value)) {
                    throw new InvalidTemplateException(templatePath, "Import node without a 'value' attribute");
                }

                if (aliasAttr == null || string.IsNullOrEmpty(aliasAttr.Value)) {
                    throw new InvalidTemplateException(templatePath, "Import node without an 'as' attribute");
                }

                string alias = aliasAttr.Value;
                if (alias[0] != '@') alias = "@" + alias;

                imports.Add(new ImportDeclaration(valueAttr.Value, alias));
            }

            foreach (XElement usingElement in usingElements) {
                usings.Add(ParseUsing(usingElement));
            }

            foreach (XElement styleElement in styleElements) {
                styleTemplates.Add(ParseStyleSheet(templatePath, styleElement));
            }

            List<UITemplate> children = ParseNodes(contentElement.Nodes());
            List<AttributeDefinition> attributes = ParseAttributes(contentElement.Attributes());

            if (contentElement.GetAttribute("x-inherited") != null) {
                // todo -- validate base type
                ParsedTemplate baseTemplate = GetParsedTemplate(type.BaseType);
                List<UISlotContentTemplate> contentTemplates = new List<UISlotContentTemplate>();

                for (int i = 0; i < children.Count; i++) {
                    if (!(children[i] is UISlotContentTemplate)) {
                        throw new ParseException("When using inherited templates, all children must be <SlotContent/> elements");
                    }

                    contentTemplates.Add((UISlotContentTemplate) children[i]);
                }

                return baseTemplate.CreateInherited(type, usings, contentTemplates, styleTemplates, imports);
            }

//            UIElementTemplate rootTemplate = new UIElementTemplate(type, children, attributes);
            return new ParsedTemplate(type, children, attributes, usings, styleTemplates, imports);
        }

        private static string ParseUsing(XElement element) {
            var namespaceAttr = element.GetAttribute("namespace");
            if (namespaceAttr == null) {
                throw new ParseException("<Using/> tags require a 'namespace' attribute");
            }

            string value = namespaceAttr.Value.Trim();
            if (string.IsNullOrEmpty(value)) {
                throw new ParseException("<Using/> tags require a 'namespace' attribute with a value");
            }

            return value;
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
//            EnsureOnlyAttributes(element, RepeatAttributes);

            UIRepeatTemplate template = new UIRepeatTemplate(
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );

            return template;
        }

        private static UITemplate ParseContainerElement(Type type, XElement element) {
            UIContainerTemplate template = new UIContainerTemplate(
                type,
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );
            return template;
        }

        private static UITemplate ParseSlotElement(XElement element) {
            EnsureAttribute(element, "name");
            EnsureNotInsideTagName(element, "Repeat");
            EnsureNotInsideTagName(element, "Slot");

            return new UISlotTemplate(
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );
        }

        private static UITemplate ParseSlotContentElement(XElement element) {
            EnsureAttribute(element, "name");
            EnsureNotInsideTagName(element, "Repeat");
            EnsureNotInsideTagName(element, "Slot");

            return new UISlotContentTemplate(
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );
        }

        private static UITemplate ParseChildrenElement(XElement element) {
            EnsureEmpty(element);
            EnsureNotInsideTagName(element, "Repeat");
            return new UIChildrenTemplate(null, ParseAttributes(element.Attributes()));
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

        private static UITextTemplate ParseTextNode(XText node) {
            // todo split nodes based on inline {expressions}
            return new UITextTemplate(null, "'" + node.Value.Trim() + "'");
        }

        private static UITemplate ParseTemplateElement(XElement element) {
            ProcessedType elementType = TypeProcessor.GetTemplateType(element.Name.LocalName);
            if (typeof(UIContainerElement).IsAssignableFrom(elementType.rawType)) {
                return new UIContainerTemplate(
                    elementType.rawType,
                    ParseNodes(element.Nodes()),
                    ParseAttributes(element.Attributes())
                );
            }

            if (typeof(UITextElement).IsAssignableFrom(elementType.rawType)) {
                return ParseTextElement(elementType.rawType, element);
            }

//            if (typeof(UIPrimitiveElement).IsAssignableFrom(elementType.rawType)) {
//                return ParsePrimitiveElement(elementType.rawType, element);
//            }

            UITemplate template = new UIElementTemplate(
                element.Name.LocalName,
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );

            return template;
        }

        private static UITemplate ParseTextElement(Type type, XElement element) {
            string rawText = string.Empty;
            foreach (XNode node in element.Nodes()) {
                switch (node.NodeType) {
                    case XmlNodeType.Text:
                        rawText += "'" + ((XText) node).Value.Trim() + "'";
                        continue;

                    case XmlNodeType.Element:
                        throw new Exception("<Text> can only have text children, no elements");

                    case XmlNodeType.Comment:
                        continue;
                }

                throw new InvalidTemplateException("Unable to handle node type: " + node.NodeType);
            }

            return new UITextTemplate(type, rawText, ParseAttributes(element.Attributes()));
        }

        private static UITemplate ParseGraphicElement(XElement element) {
            UITemplate template = new UIGraphicTemplate(
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );
            return template;
        }

        private static UITemplate ParseImageElement(XElement element) {
            return new UIImageTemplate(null, ParseAttributes(element.Attributes()));
        }

        private static UITemplate ParseInputElement(XElement element) {
            return new UIElementTemplate(
                typeof(UIInputFieldElement),
                ParseNodes(element.Nodes()),
                ParseAttributes(element.Attributes())
            );
        }

        private static UITemplate ParseElement(XElement element) {
            if (element.Name == "Children") {
                return ParseChildrenElement(element);
            }

            if (element.Name == "Graphic") {
                return ParseGraphicElement(element);
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

            if (element.Name == "SlotContent") {
                return ParseSlotContentElement(element);
            }

            if (element.Name == "Switch") {
                return ParseSwitchElement(element);
            }

            if (element.Name == "Default") {
                return ParseDefaultElement(element);
            }

            if (element.Name == "Case") {
                return ParseCaseElement(element);
            }

            if (element.Name == "Input") {
                return ParseInputElement(element);
            }

            for (int i = 0; i < IntrinsicElementTypes.Length; i++) {
                if (IntrinsicElementTypes[i].name == element.Name) {
                    if (IntrinsicElementTypes[i].isContainer) {
                        return ParseContainerElement(IntrinsicElementTypes[i].type, element);
                    }
                    else {
                        return ParseTextElement(IntrinsicElementTypes[i].type, element);
                    }
                }
            }

            return ParseTemplateElement(element);
        }

        public struct IntrinsicElementType {

            public readonly string name;
            public readonly Type type;
            public readonly bool isContainer;

            public IntrinsicElementType(string name, Type type, bool isContainer) {
                this.name = name;
                this.type = type;
                this.isContainer = isContainer;
            }

        }

        public static readonly IntrinsicElementType[] IntrinsicElementTypes = {
            new IntrinsicElementType("Group", typeof(UIGroupElement), true),
            new IntrinsicElementType("Panel", typeof(UIPanelElement), true),
            new IntrinsicElementType("Section", typeof(UISectionElement), true),
            new IntrinsicElementType("Div", typeof(UIDivElement), true),
            new IntrinsicElementType("Header", typeof(UIHeaderElement), true),
            new IntrinsicElementType("Footer", typeof(UIFooterElement), true),

            new IntrinsicElementType("Text", typeof(UITextElement), false),
            new IntrinsicElementType("Label", typeof(UILabelElement), false),
            new IntrinsicElementType("Paragraph", typeof(UIParagraphElement), false),
            new IntrinsicElementType("Heading1", typeof(UIHeading1Element), false),
            new IntrinsicElementType("Heading2", typeof(UIHeading2Element), false),
            new IntrinsicElementType("Heading3", typeof(UIHeading3Element), false),
            new IntrinsicElementType("Heading4", typeof(UIHeading4Element), false),
            new IntrinsicElementType("Heading5", typeof(UIHeading5Element), false),
            new IntrinsicElementType("Heading6", typeof(UIHeading6Element), false),
        };

        private static List<UITemplate> ParseNodes(IEnumerable<XNode> nodes) {
            List<UITemplate> retn = new List<UITemplate>();
            foreach (XNode node in nodes) {
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
            foreach (XAttribute attr in element.Attributes()) {
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