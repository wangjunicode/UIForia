using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    [UsedImplicitly]
    [TemplateParser("xml")]
    public class TemplateParserXML2 : TemplateParser {

        private TemplateFileShellBuilder templateFileShellBuilder;
        private List<AttributeDefinition2> attributes;

        public TemplateParserXML2() {
            attributes = new List<AttributeDefinition2>(16);
        }

        private void SetErrorContext(XElement element) {
            IXmlLineInfo info = element;
            SetErrorContext(info.LineNumber, info.LinePosition);
        }

        public override bool TryParse(string contents, TemplateFileShellBuilder templateShellBuilder) {
            this.templateFileShellBuilder = templateShellBuilder;

            XElement root;

            try {
                root = XElement.Load(new XmlTextReader(contents, XmlNodeType.Element, XMLTemplateParser.s_XmlParserContext), LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
            }
            catch (Exception exception) {
                ReportParseError(exception.Message);
                return false;
            }

            root.MergeTextNodes();

            //  ParseUsings(root);
            //  ParseStyles(root);
            ParseContents(root);

            return true;
        }

        private void ParseContents(XElement root) {

            IEnumerable<XElement> contentElements = root.GetChildren("Contents");

            foreach (XElement contentRoot in contentElements) {

                SetErrorContext(contentRoot);

                XAttribute attr = contentRoot.GetAttribute("id");

                string templateId = null;

                if (attr != null) {
                    templateId = attr.Value.Trim();
                }

                attributes.Clear();

                // maybe tag name should be root? We don't actually know what element we are parsing at this point
                ParseAttributes(contentRoot.Attributes(), attributes, out string genericTypeResolver, out string requireType);

                LineInfo lineInfo = new LineInfo(((IXmlLineInfo) contentRoot).LineNumber, ((IXmlLineInfo) contentRoot).LinePosition);

                ValidateRootAttributes(contentRoot);

                TemplateFileShellBuilder.TemplateASTBuilder retn = templateFileShellBuilder.CreateRootNode(templateId, attributes, lineInfo, genericTypeResolver, requireType);

                ParseChildren(retn, contentRoot.Nodes());
            }

        }

        private void ParseUsings(XElement root) {
            IEnumerable<XElement> usingElements = root.GetChildren("Using");

            foreach (XElement usingElement in usingElements) {
                if (TryParseUsing(usingElement, out UsingDeclaration declaration)) {
                    templateFileShellBuilder.AddUsing(declaration);
                }
            }

            // for (int i = 0; i < shell.usings.size; i++) {
            //     if (shell.usings.array[i].name != null) {
            //         shell.referencedNamespaces.Add(shell.usings.array[i].name);
            //     }
            // }
        }

        private bool TryParseUsing(XElement element, out UsingDeclaration retn) {
            SetErrorContext(element);

            XAttribute namespaceAttr = element.GetAttribute("namespace");
            XAttribute elementAttr = element.GetAttribute("element");
            XAttribute pathAttr = element.GetAttribute("path");
            retn = default;

            if (elementAttr != null || pathAttr != null) {
                if (elementAttr == null) {
                    return ReportParseError("<Using> tag requires `element` attribute if `path` is provided");
                }

                if (pathAttr == null) {
                    return ReportParseError("<Using> tag requires `path` attribute if `element` is provided");
                }

                retn = new UsingDeclaration() {
                    name = elementAttr.Value.Trim(),
                    pathName = pathAttr.Value.Trim(),
                    type = UsingDeclarationType.Element,
                    lineNumber = ((IXmlLineInfo) element).LineNumber
                };

                return true;
            }

            if (namespaceAttr == null) {
                return ReportParseError("<Using/> tags require a 'namespace' attribute");
            }

            string value = namespaceAttr.Value.Trim();
            if (string.IsNullOrEmpty(value)) {
                return ReportParseError("<Using/> tags require a 'namespace' attribute with a value");
            }

            retn = new UsingDeclaration() {
                name = value,
                type = UsingDeclarationType.Namespace,
                lineNumber = ((IXmlLineInfo) element).LineNumber
            };

            return true;
        }

        private void ValidateRootAttributes(XElement root) {

            SetErrorContext(root);

            for (int i = 0; i < attributes.Count; i++) {
                AttributeDefinition2 attr = attributes[i];

                // contents should remove `id` attr
                if (attr.type == AttributeType.Attribute && attr.key == "id") {
                    attributes.RemoveAt(i--);
                    continue;
                }

                if (attr.type == AttributeType.Conditional) {
                    ReportParseError($"<Contents> cannot contain conditional bindings. Ignoring  \"{attr.key}\"=\"{attr.value}\".");
                    attributes.RemoveAt(i--);
                    continue;
                }

                if (attr.type == AttributeType.Property) {
                    ReportParseError($"<Contents> cannot contain property bindings. Ignoring \"{attr.key}\"=\"{attr.value}\".");
                    attributes.RemoveAt(i--);
                }
            }

        }

        private StringBuilder textContent = new StringBuilder(128);
        
        private void ParseChildren(TemplateFileShellBuilder.TemplateASTBuilder parent, IEnumerable<XNode> nodes) {

            textContent.Clear();
            IXmlLineInfo xmlLineInfo = default;
            foreach (XNode node in nodes) {
                xmlLineInfo = node;
                SetErrorContext(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);

                switch (node.NodeType) {
                    case XmlNodeType.Text: {
                        XText textNode = (XText) node;

                        if (string.IsNullOrWhiteSpace(textNode.Value)) {
                            continue;
                        }

                        textContent.Append(textNode.Value);
                        
                        continue;
                    }

                    case XmlNodeType.Element: {
                        XElement element = (XElement) node;

                        if (textContent.Length > 0) {
                            IXmlLineInfo textLineInfo = element.PreviousNode;
                            parent.SetTextContent(textContent.ToString(), new LineInfo(textLineInfo.LineNumber, textLineInfo.LinePosition));
                            textContent.Clear();
                        }

                        string tagName = element.Name.LocalName;
                        string namespaceName = element.Name.NamespaceName;

                        attributes.Clear();

                        ParseAttributes(element.Attributes(), attributes, out string genericTypeResolver, out string requireType);

                        LineInfo lineInfo = new LineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);

                        TemplateFileShellBuilder.TemplateASTBuilder childNode;

                        if (namespaceName == "define") {
                            childNode = parent.AddSlotChild(tagName, attributes, lineInfo, SlotType.Define, requireType);
                        }
                        else if (namespaceName == "override") {
                            childNode = parent.AddSlotChild(tagName, attributes, lineInfo, SlotType.Override, requireType);
                        }
                        else if (namespaceName == "forward") {
                            childNode = parent.AddSlotChild(tagName, attributes, lineInfo, SlotType.Forward, requireType);
                        }
                        else if (string.IsNullOrEmpty(namespaceName)) {
                            if (string.Equals(tagName, "Repeat", StringComparison.Ordinal)) {
                                childNode = parent.AddRepeatChild(attributes, lineInfo, genericTypeResolver, requireType);
                            }
                            else if (string.Equals(tagName, "Text", StringComparison.Ordinal)) {
                                childNode = parent.AddTextChild(attributes, lineInfo);
                            }
                            else if (string.Equals(tagName, "Span", StringComparison.Ordinal)) {
                                childNode = parent.AddTextSpanChild(attributes, lineInfo);
                            }
                            else if (string.Equals(tagName, "Image", StringComparison.Ordinal)) {
                                childNode = parent.AddElementChild(namespaceName, tagName, attributes, lineInfo, genericTypeResolver, requireType);
                            }
                            else {
                                childNode = parent.AddElementChild(namespaceName, tagName, attributes, lineInfo, genericTypeResolver, requireType);
                            }
                        }
                        else {
                            childNode = parent.AddElementChild(namespaceName, tagName, attributes, lineInfo, genericTypeResolver, requireType);
                        }

                        ParseChildren(childNode, element.Nodes());

                        continue;
                    }

                    case XmlNodeType.Comment:
                        continue;

                    default:
                        ReportParseError($"Unable to handle node type: {node.NodeType}");
                        break;
                }

            }

            if (textContent.Length != 0) {
                parent.SetTextContent(textContent.ToString(), new LineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition));
                textContent.Clear();
            }

        }

        private void ParseAttributes(IEnumerable<XAttribute> xmlAttributes, List<AttributeDefinition2> attributes, out string genericTypeResolver, out string requireType) {
            genericTypeResolver = null;
            requireType = null;

            foreach (XAttribute attr in xmlAttributes) {
                string prefix = attr.Name.NamespaceName;
                string name = attr.Name.LocalName.Trim();
                int line = ((IXmlLineInfo) attr).LineNumber;
                int column = ((IXmlLineInfo) attr).LinePosition;
                SetErrorContext(line, column);

                if (name == "id" && string.IsNullOrEmpty(prefix)) {
                    prefix = "attr";
                }

                if (prefix == "generic" && name == "type") {
                    genericTypeResolver = attr.Value;
                    continue;
                }

                if (prefix == "require" && name == "type") {
                    requireType = attr.Value;
                    continue;
                }

                HandleAttribute(prefix, name, line, column, attr.Value, attributes);
            }
        }

        private void HandleAttribute(string prefix, string name, int line, int column, string value, List<AttributeDefinition2> attributes) {
            AttributeType attributeType = AttributeType.Property;
            AttributeFlags flags = 0;

            if (prefix.StartsWith("inject.")) {
                prefix = prefix.Substring("inject.".Length);
                flags |= AttributeFlags.Injected;
            }

            // once:if=""
            // enable:if=""
            // todo -- not valid everywhere
            if (name.Contains(".once") || name.Contains(".const")) {
                name = name.Replace(".once", "");
                name = name.Replace(".const", "");
                flags |= AttributeFlags.Const;
            }

            // todo -- validate this syntax
            if (name.Contains(".enable")) {
                name = name.Replace(".enable", "");
                flags |= AttributeFlags.EnableOnly;
            }

            if (name == "if") {
                attributeType = AttributeType.Conditional;
            }
            else if (prefix == string.Empty) {
                if (name == "style") {
                    attributeType = AttributeType.Style;
                    name = "style";
                }
                else if (name.StartsWith("style.")) {
                    attributeType = AttributeType.InstanceStyle;
                    name = name.Substring("style.".Length);
                }
            }
            else {
                switch (prefix) {
                    case "property":
                        break;

                    case "attr": {
                        attributeType = AttributeType.Attribute;
                        if (value[0] != '{' || value[value.Length - 1] != '}') {
                            flags |= AttributeFlags.Const;
                        }

                        break;
                    }

                    case "slot": {
                        attributeType = AttributeType.Slot;
                        break;
                    }

                    case "mouse":
                        attributeType = AttributeType.Mouse;
                        break;

                    case "key":
                        attributeType = AttributeType.Key;
                        break;

                    case "drag":
                        attributeType = AttributeType.Drag;
                        break;

                    case "onChange":
                        attributeType = AttributeType.ChangeHandler;
                        break;

                    case "touch":
                        attributeType = AttributeType.Touch;
                        break;

                    case "controller":
                        attributeType = AttributeType.Controller;
                        break;

                    case "style":
                        attributeType = AttributeType.InstanceStyle;
                        if (name.Contains(".")) {
                            if (name.StartsWith("hover.")) {
                                flags |= AttributeFlags.StyleStateHover;
                                name = name.Substring("hover.".Length);
                            }
                            else if (name.StartsWith("focus.")) {
                                flags |= AttributeFlags.StyleStateFocus;
                                name = name.Substring("focus.".Length);
                            }
                            else if (name.StartsWith("active.")) {
                                flags |= AttributeFlags.StyleStateActive;
                                name = name.Substring("active.".Length);
                            }
                            else {
                                ReportParseError($"Unable to handle style state declaration '{name.Split('.')[0]}' Expected 'active', 'focus', or 'hover'");
                                return;
                            }
                        }

                        break;

                    case "evt":
                        attributeType = AttributeType.Event;
                        break;

                    case "ctx":

                        attributeType = AttributeType.Context;

                        if (name == "element" || name == "parent" || name == "root" || name == "evt") {
                            ReportParseError($"`{name} is a reserved name and cannot be used as a context variable name");
                            return;
                        }

                        break;

                    case "var":
                        attributeType = AttributeType.ImplicitVariable;

                        if (name == "element" || name == "parent" || name == "root" || name == "evt") {
                            ReportParseError($"`{name} is a reserved name and cannot be used as a context variable name");
                            return;
                        }

                        break;

                    case "sync":
                        attributeType = AttributeType.Property;
                        flags |= AttributeFlags.Sync;
                        break;

                    case "expose":
                        attributeType = AttributeType.Expose;
                        if (name == "element" || name == "parent" || name == "root" || name == "evt") {
                            ReportParseError($"`{name} is a reserved name and cannot be used as a context variable name");
                            return;
                        }

                        break;

                    case "alias":
                        attributeType = AttributeType.Alias;
                        if (name == "element" || name == "parent" || name == "root" || name == "evt") {
                            ReportParseError($"`{name} is a reserved name and cannot be used as a context variable name");
                            return;
                        }

                        break;

                    default:
                        ReportParseError("Unknown attribute prefix: " + prefix);
                        return;
                }
            }

            attributes.Add(new AttributeDefinition2(attributeType, flags, name, value, line, column));

        }

    }

}