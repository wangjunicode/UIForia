using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing.Expressions;
using UIForia.Templates;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Parsing {

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

    public class XMLTemplateParser2 {

        internal readonly bool outputComments;

        private readonly XmlParserContext parserContext;
        private readonly TemplateCache templateCache;

        public XMLTemplateParser2(TemplateCache templateCache, bool outputComments = true) {
            this.templateCache = templateCache;
            this.outputComments = outputComments;
            XmlNamespaceManager nameSpaceManager = new XmlNamespaceManager(new NameTable());
            nameSpaceManager.AddNamespace("attr", "attr");
            nameSpaceManager.AddNamespace("slot", "slot");
            nameSpaceManager.AddNamespace("evt", "evt");
            nameSpaceManager.AddNamespace("style", "style");
            nameSpaceManager.AddNamespace("ctx", "ctx");
            nameSpaceManager.AddNamespace("ctxvar", "ctxvar");
            nameSpaceManager.AddNamespace("mouse", "mouse");
            nameSpaceManager.AddNamespace("key", "key");
            nameSpaceManager.AddNamespace("touch", "touch");
            nameSpaceManager.AddNamespace("touch", "touch");
            nameSpaceManager.AddNamespace("controller", "controller");
            this.parserContext = new XmlParserContext(null, nameSpaceManager, null, XmlSpace.None);
        }

        internal RootTemplateNode Parse(RootTemplateNode rootNode, string template, string filePath, ProcessedType processedType) {
            XElement root = XElement.Load(new XmlTextReader(template, XmlNodeType.Element, parserContext));
            root.MergeTextNodes();

            IEnumerable<XElement> styleElements = root.GetChildren("Style");
            IEnumerable<XElement> usingElements = root.GetChildren("Using");
            IEnumerable<XElement> contentElements = root.GetChildren("Contents");

            StructList<UsingDeclaration> usings = StructList<UsingDeclaration>.Get();
            StructList<StyleDefinition> styles = StructList<StyleDefinition>.Get();

            LightList<string> namespaces = LightList<string>.Get();

            foreach (XElement usingElement in usingElements) {
                usings.Add(ParseUsing(usingElement));
            }

            for (int i = 0; i < usings.Count; i++) {
                namespaces.Add(usings[i].namespaceName);
            }

            foreach (XElement styleElement in styleElements) {
                styles.Add(ParseStyleSheet(filePath, styleElement));
            }

            if (contentElements.Count() != 1) {
                Debug.Log(processedType.rawType + " has invalid content");
            }

            XElement contentElement = contentElements.First();
            IXmlLineInfo xmlLineInfo = contentElement;

            StructList<AttributeDefinition2> attributes = ParseAttributes(contentElement.Attributes());

            rootNode.attributes = attributes;
            rootNode.lineInfo = new TemplateLineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);

            ParseChildren(rootNode, rootNode, contentElement.Nodes(), namespaces);

            rootNode.usings = usings;
            rootNode.styles = styles;

            LightList<string>.Release(ref namespaces);

            return rootNode;
        }

        private TemplateNode2 ParseElementTag(RootTemplateNode root, TemplateNode2 parent, string namespacePath, string tagName, LightList<string> namespaces, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) {
            ProcessedType processedType = null;
            TemplateNode2 node = null;

            if (namespacePath.Length > 0) {
                throw new NotImplementedException("Element namespace resolution not yet implemented");
            }
            else {
                if (string.Equals(tagName, "Children", StringComparison.Ordinal)) {
                    processedType = TypeProcessor.GetProcessedType(typeof(UIChildrenElement));
                    node = new ChildrenNode(root, parent, processedType, attributes, templateLineInfo);
                    root.AddSlot((SlotNode) node);
                    parent.AddChild(node);
                    return node;
                }
                else if (string.Equals(tagName, "Slot", StringComparison.Ordinal)) {
                    processedType = TypeProcessor.GetProcessedType(typeof(UISlotOverride));
                                        string slotName = GetSlotName(attributes);
                    node = new SlotNode(root, parent, processedType, attributes, templateLineInfo, slotName, SlotType.Override);

                    if (!(parent is ExpandedTemplateNode expanded)) {
                        throw ParseException.InvalidSlotOverride(parent.originalString, node.originalString);
                    }

                    expanded.ValidateSlot(((SlotNode) node).slotName, templateLineInfo);

                    expanded.AddSlotOverride((SlotNode) node);
                    processedType.ValidateAttributes(attributes);
                    return node;
                }
                else if (string.Equals(tagName, "DefineSlot", StringComparison.Ordinal)) {
                    processedType = TypeProcessor.GetProcessedType(typeof(UISlotDefinition));
                    string slotName = GetSlotName(attributes);
                    node = new SlotNode(root, parent, processedType, attributes, templateLineInfo, slotName, SlotType.Default);
                    root.AddSlot((SlotNode) node);
                    parent.AddChild(node);
                    return node;
                }
                else if (string.Equals(tagName, "ExternSlot", StringComparison.Ordinal)) {
                    processedType = TypeProcessor.GetProcessedType(typeof(UISlotDefinition));

                    if (!(parent is ExpandedTemplateNode expanded)) {
                        throw ParseException.InvalidSlotOverride(parent.originalString, node.originalString);
                    }

                    // todo -- error check
                    string slotName = GetSlotName(attributes);
                    string slotAlias = GetSlotAlias(slotName, attributes);
                    // root.ValidateExternSlot(slotName, slotAlias);

                    node = new SlotNode(root, parent, processedType, attributes, templateLineInfo, slotName, SlotType.Extern);
                    root.AddSlot((SlotNode) node);
                    parent.AddChild(node);
                    return node;
                }

                processedType = TypeProcessor.ResolveTagName(tagName, null);
            }

            if (processedType == null) {
                throw new ParseException("Unresolved tag name: " + tagName);
            }

            processedType.ValidateAttributes(attributes);

//            if (node != null) {
//                parent.AddChild(node);
//                return node;
//            }

            if (typeof(UIContainerElement).IsAssignableFrom(processedType.rawType)) {
                node = new ContainerNode(root, parent, processedType, attributes, templateLineInfo);
            }
            else if (typeof(UITextElement).IsAssignableFrom(processedType.rawType)) {
                node = new TextNode(root, parent, string.Empty, processedType, attributes, templateLineInfo);
            }
            else if (typeof(UITextSpanElement).IsAssignableFrom(processedType.rawType)) {
                throw new NotImplementedException();
            }
            else if (typeof(UITerminalElement).IsAssignableFrom(processedType.rawType)) {
                node = new TerminalNode(root, parent, processedType, attributes, templateLineInfo);
            }
            else if (typeof(UIElement).IsAssignableFrom(processedType.rawType)) {
                RootTemplateNode expanded = templateCache.GetParsedTemplate(processedType);
                node = new ExpandedTemplateNode(expanded, root, parent, processedType, attributes, templateLineInfo);
            }

            if (node == null) {
                throw new ParseException("Unresolved tag name: " + tagName);
            }

            node.tagName = tagName;
            node.namespaceName = namespacePath;

            parent.AddChild(node);

            return node;
        }

        private static string GetSlotName(StructList<AttributeDefinition2> attributes) {
            if (attributes == null) return null;
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Slot && string.Equals(attributes.array[i].key, "name", StringComparison.Ordinal)) {
                    string slotName = attributes.array[i].value.Trim();
                    attributes.RemoveAt(i);
                    return slotName;
                }
            }

            return null;
        }

        private static string GetSlotAlias(string slotName, StructList<AttributeDefinition2> attributes) {
            if (attributes == null) return slotName;
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Slot && string.Equals(attributes.array[i].key, "alias", StringComparison.Ordinal)) {
                    string slotAlias = attributes.array[i].value.Trim();
                    attributes.RemoveAt(i);
                    return slotAlias;
                }
            }

            return slotName;
        }

        private static void CreateOrUpdateTextNode(RootTemplateNode root, TemplateNode2 parent, string textContent, in TemplateLineInfo templateLineInfo) {
            if (parent is TextNode textParent) {
                if (parent.ChildCount == 0) {
                    TextTemplateProcessor.ProcessTextExpressions(textContent, textParent.textExpressionList);
                }
                else {
                    TextNode node = new TextNode(root, parent, textContent, TypeProcessor.GetProcessedType(typeof(UITextElement)), null, templateLineInfo);
                    TextTemplateProcessor.ProcessTextExpressions(textContent, node.textExpressionList);
                    parent.AddChild(node);
                }
            }
            else {
                TextNode node = new TextNode(root, parent, textContent, TypeProcessor.GetProcessedType(typeof(UITextElement)), null, templateLineInfo);
                TextTemplateProcessor.ProcessTextExpressions(textContent, node.textExpressionList);
                parent.AddChild(node);
            }
        }

        private void ParseChildren(RootTemplateNode root, TemplateNode2 parent, IEnumerable<XNode> nodes, LightList<string> namespaces) {
            string textContext = string.Empty;
            foreach (XNode node in nodes) {
                switch (node.NodeType) {
                    case XmlNodeType.Text: {
                        XText textNode = (XText) node;

                        if (string.IsNullOrWhiteSpace(textNode.Value)) {
                            continue;
                        }

                        textContext += textNode.Value.Trim();

                        continue;
                    }

                    case XmlNodeType.Element: {
                        XElement element = (XElement) node;

                        if (textContext.Length > 0) {
                            IXmlLineInfo textLineInfo = element.PreviousNode;
                            CreateOrUpdateTextNode(root, parent, textContext, new TemplateLineInfo(textLineInfo.LineNumber, textLineInfo.LinePosition));
                            textContext = string.Empty;
                        }


                        string tagName = element.Name.LocalName;
                        string namespaceName = element.Name.NamespaceName;

                        StructList<AttributeDefinition2> attributes = ParseAttributes(element.Attributes());

                        IXmlLineInfo lineInfo = element;
                        TemplateNode2 p = ParseElementTag(root, parent, namespaceName, tagName, namespaces, attributes, new TemplateLineInfo(lineInfo.LineNumber, lineInfo.LinePosition));

                        ParseChildren(root, p, element.Nodes(), namespaces);

                        continue;
                    }

                    case XmlNodeType.Comment:
                        continue;
                }

                throw new TemplateParseException(node, $"Unable to handle node type: {node.NodeType}");
            }

            if (textContext.Length != 0) {
                CreateOrUpdateTextNode(root, parent, textContext, parent.lineInfo); // todo -- line info probably wrong
            }
        }

        private static StructList<AttributeDefinition2> ParseAttributes(IEnumerable<XAttribute> xmlAttributes) {
            StructList<AttributeDefinition2> attributes = StructList<AttributeDefinition2>.Get();
            foreach (XAttribute attr in xmlAttributes) {
                string prefix = attr.Name.NamespaceName;
                string name = attr.Name.LocalName.Trim();

                int line = ((IXmlLineInfo) attr).LineNumber;
                int column = ((IXmlLineInfo) attr).LinePosition;

                AttributeType attributeType = AttributeType.Property;
                AttributeFlags flags = 0;

                // todo -- not valid everywhere
                if (name.Contains(".once")) {
                    name = name.Replace(".once", "");
                    flags |= AttributeFlags.Const;
                }

                if (name == "if") {
                    attributeType = AttributeType.Conditional;
                }
                else if (prefix == string.Empty) {
                    if (attr.Name.LocalName == "style") {
                        attributeType = AttributeType.Style;
                        name = "style";
                    }
                    else if (attr.Name.LocalName.StartsWith("style.")) {
                        attributeType = AttributeType.Style;
                        name = attr.Name.LocalName.Substring("style.".Length);
                        flags |= AttributeFlags.StyleProperty;
                    }
                    else if (attr.Name.LocalName.StartsWith("x-")) {
                        attributeType = AttributeType.Attribute;
                        name = attr.Name.LocalName.Substring("x-".Length);
                        if (name[0] != '{' || name[name.Length - 1] != '}') {
                            flags |= AttributeFlags.Const;
                        }
                    }
                }
                else {
                    switch (prefix) {
                        case "attr": {
                            attributeType = AttributeType.Attribute;
                            if (attr.Value[0] != '{' || attr.Value[attr.Value.Length - 1] != '}') {
                                flags |= AttributeFlags.Const;
                            }

                            break;
                        }
                        case "slot": {
                            attributeType = AttributeType.Slot;
                            break;
                        }
                        case "slotctx": {
                            attributeType = AttributeType.SlotContext;
                            break;
                        }
                        case "mouse":
                            attributeType = AttributeType.Mouse;
                            break;
                        case "key":
                            attributeType = AttributeType.Key;
                            break;
                        case "touch":
                            attributeType = AttributeType.Touch;
                            break;
                        case "controller":
                            attributeType = AttributeType.Controller;
                            break;
                        case "style":
                            attributeType = AttributeType.Style;
                            break;
                        case "evt":
                            attributeType = AttributeType.Event;
                            break;
                        case "ctx":
                            attributeType = AttributeType.Context;
                            break;
                        case "ctxvar":
                            attributeType = AttributeType.ContextVariable;
                            break;
                        case "alias":
                            attributeType = AttributeType.Alias;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException("Unknown attribute prefix: " + prefix);
                    }
                }

                string raw = string.Empty;
                if (!string.IsNullOrEmpty(prefix)) {
                    TextUtil.StringBuilder.Append(prefix);
                    TextUtil.StringBuilder.Append(":");
                    TextUtil.StringBuilder.Append(name);
                    TextUtil.StringBuilder.Append("=\"");
                    TextUtil.StringBuilder.Append(attr.Value);
                    TextUtil.StringBuilder.Append("\"");
                    raw = TextUtil.StringBuilder.ToString();
                    TextUtil.StringBuilder.Clear();
                }
                else {
                    TextUtil.StringBuilder.Append(name);
                    TextUtil.StringBuilder.Append("=\"");
                    TextUtil.StringBuilder.Append(attr.Value);
                    TextUtil.StringBuilder.Append("\"");
                    raw = TextUtil.StringBuilder.ToString();
                    TextUtil.StringBuilder.Clear();
                }

                // todo -- set flag properly
                attributes.Add(new AttributeDefinition2(raw, attributeType, flags, name, attr.Value, line, column));
            }

            if (attributes.size == 0) {
                StructList<AttributeDefinition2>.Release(ref attributes);
            }

            return attributes;
        }

        private void BuildOriginalString(TemplateNode2 templateNode, string elementName) {
            if (!outputComments) return;

            string attrString = string.Empty;
            if (templateNode.attributes.size > 0) {
                LightList<string> str = LightList<string>.Get();
                for (int i = 0; i < templateNode.attributes.size; i++) {
                    str.Add(templateNode.attributes.array[i].rawValue);
                }

                attrString = StringUtil.ListToString((IList<string>) str, " ");
                LightList<string>.Release(ref str);
            }

            if (attrString.Length == 0) {
                templateNode.originalString = $"<{elementName} {attrString}/>";
            }

            else {
                templateNode.originalString = $"<{elementName} {attrString}/>";
            }

//            if (templateNode.textContent != null && templateNode.textContent.size > 0) {
//                templateNode.originalString += "    '";
//                for (int i = 0; i < templateNode.textContent.size; i++) {
//                    if (templateNode.textContent.array[i].isExpression) {
//                        templateNode.originalString += "{";
//                        templateNode.originalString += templateNode.textContent.array[i].text;
//                        templateNode.originalString += "}";
//                    }
//                    else {
//                        templateNode.originalString += templateNode.textContent.array[i].text;
//                    }
//                }
//
//                templateNode.originalString += "'";
//            }
        }

        private static bool Escape(string input, ref int ptr, out char result) {
// xml parser might already do this for us
            if (StringCompare(input, ref ptr, "amp;", '&', out result)) return true;
            if (StringCompare(input, ref ptr, "lt;", '<', out result)) return true;
            if (StringCompare(input, ref ptr, "amp;", '>', out result)) return true;
            if (StringCompare(input, ref ptr, "amp;", '"', out result)) return true;
            if (StringCompare(input, ref ptr, "amp;", '\'', out result)) return true;
            if (StringCompare(input, ref ptr, "obrc;", '{', out result)) return true;
            if (StringCompare(input, ref ptr, "cbrc;", '}', out result)) return true;
            return false;
        }

        private static bool StringCompare(string input, ref int ptr, string target, char match, out char result) {
            result = '\0';
            if (ptr + target.Length - 1 >= input.Length) {
                return false;
            }

            for (int i = 0;
                i < target.Length;
                i++) {
                if (target[i] != input[ptr + i]) {
                    return false;
                }
            }

            ptr += target.Length;
            result = match;
            return true;
        }


        public static void ProcessTextExpressions(string input, LightList<string> outputList) {
//input = input.Trim(); // todo -- let style handle this 
            int ptr = 0;
            int level = 0;
            StringBuilder builder = TextUtil.StringBuilder;
            builder.Clear();
            while (ptr < input.Length) {
                char current = input[ptr++];
                if (current == '&') {
                    // todo -- escape probably needs to go the other way round
                    if (Escape(input, ref ptr, out char result)) {
                        builder.Append(result);
                        continue;
                    }
                }

                if (current == '{') {
                    if (level == 0) {
                        if (builder.Length > 0) {
                            outputList.Add(builder.ToString());
                            builder.Clear();
                        }

                        level++;
                        continue;
                    }

                    level++;
                }

                if (current == '}') {
                    level--;
                    if (level == 0) {
                        if (builder.Length > 0) {
                            outputList.Add(builder.ToString());
                            builder.Clear();
                        }

                        continue;
                    }
                }

                builder.Append(current);
            }

            if (level != 0) {
                throw new Exception($"Error processing {input} into expressions. Too many unmatched braces");
            }

            if (builder.Length > 0) {
                outputList.Add(builder.ToString());
            }

            builder.Clear();
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

                return new StyleDefinition(alias, templateId + ".style", rawText);
            }

// if we have no body then expect path to be set
            if (importPathAttr == null || string.IsNullOrEmpty(importPathAttr.Value)) {
                throw new TemplateParseException(styleElement, "Expected 'path' or 'src' to be provided when a body is not provided in a style tag");
            }

            return new StyleDefinition(alias, importPathAttr.Value.Trim());
        }

    }

}