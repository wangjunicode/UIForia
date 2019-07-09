using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing.Expression;
using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Compilers {

    // namespaced elements
    // using declarations

    // <Using namespace=""/>
    // <Slot:Name>
    // <SlotContent:Name>
    // <Layout:Element>
    // <Transclude:
    // <Slot:Children>
    // <Dynamic:ElementType type="" data="">
    // <Repeat:
    // <LazyLoad:
    // <Virtual:
    // <NameSpace.Whatever.Element>
    // <Const
    // <RecursiveConst: 
    // <ConstTree
    // <Shadow:
    // elements can start & end with : so we can have anonymous elements


    public class XMLTemplateParser {

        public Application application;

        private readonly XmlParserContext parserContext;

        private readonly string[] s_Directives = {
            "Slot",
            "SlotContent",
            "LazyLoad",
            "Dynamic",
            "Virtual",
            "Const",
            "ConstRecursive",
            "Shadow",
            "Repeat",
        };

        public XMLTemplateParser(Application application) {
            this.application = application;
            XmlNamespaceManager nameSpaceManager = new XmlNamespaceManager(new NameTable());
            nameSpaceManager.AddNamespace("attr", "attr");
            nameSpaceManager.AddNamespace("evt", "evt");
            nameSpaceManager.AddNamespace("style", "style");
            for (int i = 0; i < s_Directives.Length; i++) {
                nameSpaceManager.AddNamespace(s_Directives[i], s_Directives[i]);
            }

            this.parserContext = new XmlParserContext(null, nameSpaceManager, null, XmlSpace.None);
        }

        internal TemplateAST Parse(string template, string fileName = null) {
            XElement root = XElement.Load(new XmlTextReader(template, XmlNodeType.Element, parserContext));

            root.MergeTextNodes();
            
            IEnumerable<XElement> styleElements = root.GetChildren("Style");
            IEnumerable<XElement> usingElements = root.GetChildren("Using");
            IEnumerable<XElement> contentElements = root.GetChildren("Content");

            StructList<UsingDeclaration> usings = StructList<UsingDeclaration>.Get();
            StructList<StyleDefinition> styles = StructList<StyleDefinition>.Get();

            foreach (XElement usingElement in usingElements) {
                usings.Add(ParseUsing(usingElement));
            }

            foreach (XElement styleElement in styleElements) {
                styles.Add(ParseStyleSheet("someId", styleElement));
            }

            if (contentElements.Count() != 1) { }

            XElement contentElement = contentElements.First();
            
            TemplateNode rootNode = TemplateNode.Get();

            ParseChildren(rootNode, contentElement.Nodes());

            TemplateAST retn = new TemplateAST();

            retn.fileName = fileName;
            retn.root = rootNode;
            retn.usings = usings;
            retn.styles = styles;
            //retn.extends = contentElement.GetAttribute("x-inherited") != null || contentElement.GetAttribute("attr:inherited") != null;
            return retn;
        }

        internal TemplateAST Parse(Type type) {
            ProcessedType processedType = TypeProcessor.GetProcessedType(type);

            string template = processedType.GetTemplate(application.TemplateRootPath);
            TemplateAST retn = Parse(template);
            retn.root.typeLookup = new TypeLookup(type);
            return retn;
        }

        private static readonly TypeLookup s_TextTypeLookup = new TypeLookup() {
            typeName = typeof(UITextElement).Name,
            namespaceName = typeof(UITextElement).Namespace
        };

        private static readonly char[] s_DotArray = {'.'};

        private static void ParseElementTag(TemplateNode templateNode, XElement element) {
            string directives = element.Name.Namespace.NamespaceName;
            string tagName = element.Name.LocalName;

            if (directives.Contains('.')) {
                string[] directiveList = directives.Split(s_DotArray, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < directiveList.Length; i++) {
                    templateNode.directives.Add(new DirectiveDefinition(directiveList[i]));
                }
            }
            else if (!string.IsNullOrWhiteSpace(directives) && !string.IsNullOrEmpty(directives)) {
                templateNode.directives.Add(new DirectiveDefinition(directives));
            }

            ref TypeLookup typeLookup = ref templateNode.typeLookup;

            int lastIdx = tagName.LastIndexOf('.');

            if (lastIdx > 0) {
                typeLookup.namespaceName = tagName.Substring(0, lastIdx);
                typeLookup.typeName = tagName.Substring(lastIdx);
            }
            else {
                typeLookup.typeName = tagName;
            }
        }

        private static void ParseChildren(TemplateNode parent, IEnumerable<XNode> nodes) {
            foreach (XNode node in nodes) {
                switch (node.NodeType) {
                    case XmlNodeType.Text: {
                        XText textNode = (XText) node;
                        if (string.IsNullOrWhiteSpace(textNode.Value)) {
                            continue;
                        }
                        TemplateNode templateNode = TemplateNode.Get();
                        templateNode.parent = parent;
                        templateNode.typeLookup = s_TextTypeLookup;
                        templateNode.textContent = "'" + textNode.Value.Trim() + "'";
                        parent.children.Add(templateNode);
                        templateNode = TemplateNode.Get();
                        continue;
                    }

                    case XmlNodeType.Element: {
                        XElement element = (XElement) node;
                        TemplateNode templateNode = TemplateNode.Get();

                        ParseElementTag(templateNode, element);

                        foreach (XAttribute attr in element.Attributes()) {
                            string prefix = attr.Name.NamespaceName;
                            string name = attr.Name.LocalName.Trim();

                            int line = ((IXmlLineInfo) attr).LineNumber;
                            int column = ((IXmlLineInfo) attr).LinePosition;
                            
                            AttributeType attributeType = AttributeType.Property;
                            
                            if (prefix == string.Empty) {
                             
                                if (attr.Name.LocalName.StartsWith("style.")) {
                                    attributeType = AttributeType.Style;
                                    name = attr.Name.LocalName.Substring("style.".Length);
                                }

                                if (attr.Name.LocalName.StartsWith("x-")) {
                                    attributeType = AttributeType.Attribute;
                                    name = attr.Name.LocalName.Substring("x-.".Length);
                                }
                                
                            }
                            else {
                                switch (prefix) {
                                    case "attr":
                                        attributeType = AttributeType.Attribute;
                                        break;
                                    case "style":
                                        attributeType = AttributeType.Style;
                                        break;
                                    case "evt":
                                        attributeType = AttributeType.Event;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException("Unknown attribute prefix: " + prefix);
                                }
                            }

                            // todo -- set flag properly
                            templateNode.attributes.Add(new AttributeDefinition2(attributeType, 0, name, attr.Value.Trim(), line, column));
                        }
                        
                        templateNode.parent = parent;
                        parent.children.Add(templateNode);

                        ParseChildren(templateNode, element.Nodes());

                        templateNode = TemplateNode.Get();
                        continue;
                    }

                    case XmlNodeType.Comment:
                        continue;
                }

                throw new TemplateParseException(node, $"Unable to handle node type: {node.NodeType}");
            }
        }

        private UsingDeclaration ParseUsing(XElement element) {
            XAttribute namespaceAttr = element.GetAttribute("namespace");
            if (namespaceAttr == null) {
                throw new TemplateParseException(element, "<Using/> tags require a 'namespace' attribute");
            }

            string value = namespaceAttr.Value.Trim();
            if (string.IsNullOrEmpty(value)) {
                throw new TemplateParseException(element, "<Using/> tags require a 'namespace' attribute with a value");
            }

            return new UsingDeclaration() {
                namespaceName = value
            };
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
        

    }

}