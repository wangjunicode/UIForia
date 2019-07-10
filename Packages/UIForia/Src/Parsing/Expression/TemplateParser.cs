using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Templates;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Parsing.Expression {

    public class TemplateParser {

        private readonly Dictionary<Type, ParsedTemplate> parsedTemplates = new Dictionary<Type, ParsedTemplate>();
        public readonly Application app;

        public TemplateParser(Application app) {
            this.app = app;
        }

        public void Reset() {
            parsedTemplates.Clear();
        }

        public ParsedTemplate GetParsedTemplate(Type elementType, bool forceReload = false) {
            if (!forceReload && parsedTemplates.ContainsKey(elementType)) {
                return parsedTemplates[elementType];
            }

            try {
                ParsedTemplate parsedTemplate = ParseTemplateFromType(elementType);
                parsedTemplates[elementType] = parsedTemplate;
                return parsedTemplate;
            }
            catch (Exception e) {
                // todo -- make this a better error message
                Debug.LogError(e);
                Debug.Log($"Cannot parse file {elementType}, you might be missing a template attribute. App root path {app.TemplateRootPath}");
                throw;
            }
        }

        public ParsedTemplate ParseTemplateFromString<T>(string input) where T : UIElement {
            XDocument doc = Parse(typeof(T), input);
            return ParseTemplate(null, typeof(T), doc);
        }

        public ParsedTemplate ParseTemplateFromString(Type rootType, string input) {
            XDocument doc = Parse(rootType, input);

            try {
                return ParseTemplate(rootType.AssemblyQualifiedName, rootType, doc);
            }
            catch (TemplateParseException pe) {
                pe.SetFileName(rootType.AssemblyQualifiedName);
                throw;
            }
        }

        private ParsedTemplate ParseTemplateFromType(Type type) {
            ProcessedType processedType = TypeProcessor.GetProcessedType(type);

            string template = processedType.GetTemplate(app.TemplateRootPath);
            if (template == null) {
                throw new TemplateParseException(processedType.GetTemplatePath(), "Template not found");
            }

            XDocument doc = Parse(type, template);

            try {
                return ParseTemplate(processedType.GetTemplatePath(), processedType.rawType, doc);
            }
            catch (TemplateParseException pe) {
                pe.SetFileName(processedType.GetTemplatePath());
                throw;
            }
        }

        private XDocument Parse(Type rootType, string template) {
            try {
                return XDocument.Parse(template);
            }
            catch (Exception e) {
                throw new TemplateParseException(rootType.AssemblyQualifiedName, e.Message, e);
            }
        }

        private StyleDefinition ParseStyleSheet(string templateId, XElement styleElement) {
            XAttribute aliasAttr = styleElement.GetAttribute("alias");
            XAttribute importPathAttr = styleElement.GetAttribute("path") ?? styleElement.GetAttribute("src");

            string rawText = string.Empty;
            // styles can have either a class path or a body
            foreach (XNode node in styleElement.Nodes()) {
                switch (node.NodeType) {
                    case XmlNodeType.Text:
                        rawText += ((XText) node).Value;
                        continue;

                    case XmlNodeType.Element:
                        throw new TemplateParseException(node, "<Style> can only have text children, no elements");

                    case XmlNodeType.Comment:
                        continue;
                }

                throw new TemplateParseException(node, $"Unable to handle node type: {node.NodeType}");
            }

            string alias = StyleDefinition.k_EmptyAliasName;

            if (aliasAttr != null && !string.IsNullOrEmpty(aliasAttr.Value)) {
                alias = aliasAttr.Value.Trim();
            }

            // if we have a body, expect import path to be null
            if (!string.IsNullOrEmpty(rawText) && !string.IsNullOrWhiteSpace(rawText)) {
                if (importPathAttr != null && !string.IsNullOrEmpty(importPathAttr.Value)) {
                    throw new TemplateParseException(styleElement, "Expected 'path' or 'src' to be null when a body is provided to a style tag");
                }

                return new StyleDefinition(alias, templateId, rawText);
            }

            // if we have no body then expect path to be set
            if (importPathAttr == null || string.IsNullOrEmpty(importPathAttr.Value)) {
                throw new TemplateParseException(styleElement, "Expected 'path' or 'src' to be provided when a body is not provided in a style tag");
            }

            return new StyleDefinition(alias, importPathAttr.Value.Trim());
        }

        private readonly struct TemplateData {

            public readonly IReadOnlyList<ImportDeclaration> imports;
            public readonly IReadOnlyList<StyleDefinition> styleTemplates;
            public readonly IReadOnlyList<string> usings;

            public TemplateData(IReadOnlyList<ImportDeclaration> imports, IReadOnlyList<StyleDefinition> styleTemplates, IReadOnlyList<string> usings) {
                this.imports = imports;
                this.styleTemplates = styleTemplates;
                this.usings = usings;
            }

        }

        private ParsedTemplate ParseTemplate(string templatePath, Type type, XDocument doc) {
            doc.MergeTextNodes();

            List<ImportDeclaration> imports = new List<ImportDeclaration>();
            List<StyleDefinition> styleTemplates = new List<StyleDefinition>();
            //List<BlockDefinition> blockTemplates = new List<BlockDefinition>();

            IEnumerable<XElement> importElements = doc.Root.GetChildren("Import");
            IEnumerable<XElement> styleElements = doc.Root.GetChildren("Style");
            IEnumerable<XElement> usingElements = doc.Root.GetChildren("Using");
            // IEnumerable<XElement> blockElements = doc.Root.GetChildren("Block");
            XElement contentElement = doc.Root.GetChild("Contents");

            // todo -- we can't use any bindings on the <Content/> tag because then the binding system
            // todo    would need to hold 2 different contexts depending on where the binding came from
            // todo    (ie the parsed template or the element root itself)
            List<string> usings = ListPool<string>.Get();

            foreach (XElement xElement in importElements) {
                XAttribute valueAttr = xElement.GetAttribute("value");
                XAttribute aliasAttr = xElement.GetAttribute("as");

                if (valueAttr == null || string.IsNullOrEmpty(valueAttr.Value)) {
                    throw new TemplateParseException(templatePath, xElement, "Import node without a 'value' attribute");
                }

                if (aliasAttr == null || string.IsNullOrEmpty(aliasAttr.Value)) {
                    throw new TemplateParseException(templatePath, xElement, "Import node without an 'as' attribute");
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

            //    foreach (XElement block in blockElements) {
            //      blockTemplates.Add(ParseBlock(block));
            //  }

            TemplateData templateData = new TemplateData(imports, styleTemplates, usings);

            List<UITemplate> children = ParseNodes(contentElement.Nodes(), templateData);
            List<AttributeDefinition> attributes = ParseAttributes(contentElement);

            if (contentElement.GetAttribute("x-inherited") != null) {
                // todo -- validate base type
                ParsedTemplate baseTemplate = GetParsedTemplate(type.BaseType);
                List<UISlotContentTemplate> contentTemplates = new List<UISlotContentTemplate>();

                for (int i = 0; i < children.Count; i++) {
                    if (!(children[i] is UISlotContentTemplate)) {
                        throw new TemplateParseException(contentElement, "When using inherited templates, all children must be <SlotContent/> elements");
                    }

                    contentTemplates.Add((UISlotContentTemplate) children[i]);
                }

                return baseTemplate.CreateInherited(type, usings, contentTemplates, styleTemplates, imports);
            }

            return new ParsedTemplate(app, type, templatePath, children, attributes, usings, styleTemplates, imports);
        }

        // todo -- reenable this when we have a better way of handling expressions
        private BlockDefinition ParseBlock(XElement element) {
            throw new NotImplementedException();
//            XAttribute idAttr = element.GetAttribute("id");
//
//            if (idAttr == null) throw new TemplateParseException(element, "<Block> elements require a `id` attribute");
//
//            IEnumerable<XElement> variableElements = element.GetChildren("Variable");
//
//            LightList<ReflectionUtil.FieldDefinition> fields = LightListPool<ReflectionUtil.FieldDefinition>.Get();
//
//            foreach (var variableElement in variableElements) {
//                XAttribute typeAttr = variableElement.GetAttribute("type");
//                XAttribute nameAttr = variableElement.GetAttribute("name");
//
//                if (nameAttr == null) throw new TemplateParseException(variableElement, "<Variable> definitions need to provide a unique `name` attribute");
//                if (typeAttr == null) throw new TemplateParseException(variableElement, "<Variable> definitions need to provide a `type` attribute");
//
//                // todo -- validate that name is a legal identifier
//                // todo -- validate that no fields are duplicated
//
//                Type type = TypeProcessor.ResolveTypeName(typeAttr.Value.Trim());
//
//                if (type == null) {
//                    throw new TemplateParseException(variableElement, $"Unable to resolve type with name `{typeAttr}` in <Variable> definition");
//                }
//
//                string fieldName = nameAttr.Value.Trim();
//
//                fields.Add(new ReflectionUtil.FieldDefinition(type, fieldName));
//            }
//
//            Type generatedType = ReflectionUtil.CreateType(ReflectionUtil.GetGeneratedTypeName("GeneratedBlockElementType"), typeof(UIElement), fields);
//
//            LightListPool<ReflectionUtil.FieldDefinition>.Release(ref fields);
//
//            XElement contents = element.GetChild("Contents");
//
////            ParsedTemplate template = new ParsedTemplate(
////                app,
////                generatedType,
////                null,
////                ParseAttributes(contents.Attributes()),
////                null,
////                null
////            );
//
//            return default;
//            return new BlockDefinition(
//                idAttr.Name.LocalName,
//                null,
//                null,
//                template
//            );
        }

        private string ParseUsing(XElement element) {
            XAttribute namespaceAttr = element.GetAttribute("namespace");
            if (namespaceAttr == null) {
                throw new TemplateParseException(element, "<Using/> tags require a 'namespace' attribute");
            }

            string value = namespaceAttr.Value.Trim();
            if (string.IsNullOrEmpty(value)) {
                throw new TemplateParseException(element, "<Using/> tags require a 'namespace' attribute with a value");
            }

            return value;
        }

        private UITemplate ParseRepeatElement(XElement element, in TemplateData templateData) {
            EnsureAttribute(element, "list");

            UIRepeatTemplate template = new UIRepeatTemplate(
                app,
                ParseNodes(element.Nodes(), templateData),
                ParseAttributes(element)
            );

            return template;
        }

        private UITemplate ParseContainerElement(Type type, XElement element, in TemplateData templateData) {
            UIContainerTemplate template = new UIContainerTemplate(
                app,
                element.Name.LocalName,
                type,
                ParseNodes(element.Nodes(), templateData),
                ParseAttributes(element)
            );
            return template;
        }

        private UITemplate ParseSlotElement(XElement element, in TemplateData templateData) {
            EnsureAttribute(element, "name");
            EnsureNotInsideTagName(element, "Repeat");
            EnsureNotInsideTagName(element, "Slot");

            return new UISlotTemplate(
                app,
                ParseNodes(element.Nodes(), templateData),
                ParseAttributes(element)
            );
        }

        private UITemplate ParseSlotContentElement(XElement element, in TemplateData templateData) {
            EnsureAttribute(element, "name");
            EnsureNotDirectChildOf(element, "Repeat");
            EnsureNotDirectChildOf(element, "Slot");

            return new UISlotContentTemplate(
                app,
                ParseNodes(element.Nodes(), templateData),
                ParseAttributes(element)
            );
        }

        private UITemplate ParseChildrenElement(XElement element, in TemplateData templateData) {
            // todo -- ensure only used once in a given template
            EnsureNotInsideTagName(element, "Repeat");
            return new UIChildrenTemplate(app,
                ParseNodes(element.Nodes(), templateData),
                ParseAttributes(element)
            );
        }

        private UITextTemplate ParseTextNode(XText node, in TemplateData templateData) {
            // todo split nodes based on inline {expressions}
            return new UITextTemplate(null, "'" + node.Value.Trim() + "'");
        }

        private UITemplate ParseTemplateElement(XElement element, in TemplateData templateData) {
            string tagName = element.Name.LocalName;

            ProcessedType elementType = TypeProcessor.ResolveTagName(tagName, templateData.usings);

            if (elementType.rawType != null && typeof(UIContainerElement).IsAssignableFrom(elementType.rawType)) {
                return new UIContainerTemplate(
                    app,
                    elementType.rawType,
                    ParseNodes(element.Nodes(), templateData),
                    ParseAttributes(element)
                );
            }

            if (typeof(UITextElement).IsAssignableFrom(elementType.rawType)) {
                return ParseTextElement(elementType.rawType, element, templateData);
            }

            return new UIElementTemplate(
                app,
                element.Name.LocalName,
                ParseNodes(element.Nodes(), templateData),
                ParseAttributes(element)
            );
        }

        private UITemplate ParseTextElement(Type type, XElement element, in TemplateData templateData) {
            string rawText = string.Empty;
            foreach (XNode node in element.Nodes()) {
                switch (node.NodeType) {
                    case XmlNodeType.Text:
                        rawText += "'" + ((XText) node).Value.Trim() + "'";
                        continue;

                    case XmlNodeType.Element:
                        throw new TemplateParseException(node, "<Text> can only have text children, no elements");

                    case XmlNodeType.Comment:
                        continue;
                }

                throw new TemplateParseException(node, "Unable to handle node type: " + node.NodeType);
            }

            return new UITextTemplate(app, type, rawText, ParseAttributes(element));
        }

        private UITemplate ParseImageElement(XElement element, in TemplateData templateData) {
            return new UIImageTemplate(app, null, ParseAttributes(element));
        }

        private UITemplate ParseInputElement(XElement element, in TemplateData templateData) {
            return new UIElementTemplate(
                app,
                "InputElement--string",//typeof(InputElement<string>),
                ParseNodes(element.Nodes(), templateData),
                ParseAttributes(element)
            );
        }

        private UITemplate ParseElement(XElement element, in TemplateData templateData) {
            if (element.Name == "Children") {
                return ParseChildrenElement(element, templateData);
            }

            if (element.Name == "Image") {
                return ParseImageElement(element, templateData);
            }

            if (element.Name == "Repeat") {
                return ParseRepeatElement(element, templateData);
            }

            if (element.Name == "Slot") {
                return ParseSlotElement(element, templateData);
            }

            if (element.Name == "SlotContent") {
                return ParseSlotContentElement(element, templateData);
            }

            if (element.Name == "Input") {
                return ParseInputElement(element, templateData);
            }

            // todo -- probably don't need intrinsics anymore, just use the template tagname attribute
            
            for (int i = 0; i < IntrinsicElementTypes.Length; i++) {
                if (IntrinsicElementTypes[i].name == element.Name) {
                    if (IntrinsicElementTypes[i].isContainer) {
                        return ParseContainerElement(IntrinsicElementTypes[i].type, element, templateData);
                    }

                    return ParseTextElement(IntrinsicElementTypes[i].type, element, templateData);
                }
            }

            return ParseTemplateElement(element, templateData);
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

        private List<UITemplate> ParseNodes(IEnumerable<XNode> nodes, in TemplateData templateData) {
            List<UITemplate> retn = new List<UITemplate>();
            foreach (XNode node in nodes) {
                switch (node.NodeType) {
                    case XmlNodeType.Text:
                        retn.Add(ParseTextNode((XText) node, templateData));
                        continue;

                    case XmlNodeType.Element:
                        retn.Add(ParseElement((XElement) node, templateData));
                        continue;

                    case XmlNodeType.Comment:
                        continue;
                    
                }

                throw new TemplateParseException(node, $"Unable to handle node type: {node.NodeType}");
            }

            return retn;
        }

        private static List<AttributeDefinition> ParseAttributes(XElement element) {
            int line = ((IXmlLineInfo) element).LineNumber;
            int column = ((IXmlLineInfo) element).LinePosition;

            List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>();
            foreach (var attr in element.Attributes()) {
                attributeDefinitions.Add(new AttributeDefinition(attr.Name.LocalName, attr.Value.Trim(), line, column));
            }

            return attributeDefinitions;
        }

        private void EnsureAttribute(XElement element, string attrName) {
            if (element.GetAttribute(attrName) == null) {
                throw new TemplateParseException(element,
                    $"<{element.Name.LocalName}> is missing required attribute '{attrName}'");
            }
        }

        private void EnsureEmpty(XElement element) {
            if (!element.IsEmpty) {
                throw new TemplateParseException(element,
                    $"<{element.Name.LocalName}> tags cannot have children");
            }
        }

        private void EnsureNotInsideTagName(XElement element, string tagName) {
            XElement ptr = element;

            while (ptr.Parent != null) {
                if (ptr.Parent.Name.LocalName == tagName) {
                    throw new TemplateParseException(element,
                        $"<{element.Name.LocalName}> cannot be inside <{tagName}>");
                }

                ptr = ptr.Parent;
            }
        }

        private void EnsureNotDirectChildOf(XElement element, string tagName) {
            if (element.Parent != null) {
                if (element.Parent.Name.LocalName == tagName) {
                    throw new TemplateParseException(element,
                        $"<{element.Name.LocalName}> cannot be inside <{tagName}>");
                }
            }
        }

    }

}