using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Templates;
using UIForia.Util;
using Unity.Jobs;

namespace UIForia {

    public struct TemplateJobParser {

        public Module module;
        public string filePath;
        public TemplateShell shell;
        public string source;

        private ErrorContext errorContext;
        private StructList<AttributeDefinition> attributes;
        private StructList<AttributeDefinition> injectedAttributes;
        public TemplateJobParser(Module.TemplateParseInfo info) {
            this.module = info.module;
            this.filePath = info.path;
            this.source = info.source;
            this.shell = new TemplateShell(info.path);
            this.errorContext = default;
            this.attributes = new StructList<AttributeDefinition>();
            this.injectedAttributes = new StructList<AttributeDefinition>();
        }

        internal struct ErrorContext {

            public int lineNumber;
            public int colNumber;

        }

        public void ParseTemplate() {
            XElement root;
            try {
               root = XElement.Load(new XmlTextReader(source, XmlNodeType.Element, XMLTemplateParser.s_XmlParserContext), LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
            }

            catch (Exception exception) {
                ReportParseError(exception.Message);
                return;
            }

            if (root.Name.LocalName != "UITemplate") {
                return;
            }

            root.MergeTextNodes();

            // IEnumerable<XElement> elementDefElements = root.GetChildren("Element");

            ParseUsings(root);
            ParseStyles(root);
            ParseContents(root);

        }

        private void ParseContents(XElement root) {
            IEnumerable<XElement> contentElements = root.GetChildren("Contents");

            foreach (XElement contentElement in contentElements) {
                if (TryParseContents(contentElement, out TemplateRootNode retn)) {
                    // todo -- here
                }
            }
        }

        private bool TryParseContents(XElement contentRoot, out TemplateRootNode retn) {
            retn = default;

            SetErrorContext(contentRoot);

            XAttribute attr = contentRoot.GetAttribute("id");

            string templateId = null;

            if (attr != null) {
                templateId = attr.Value.Trim();
            }

            if (shell.HasContentNode(templateId)) {
                return ReportParseError("Multiple templates found with id: " + templateId);
            }

            // maybe tag name should be root? We don't actually know what element we are parsing at this point
            ParseAttributes("Contents", contentRoot.Attributes(), attributes, injectedAttributes, out string genericTypeResolver, out string requireType);

            retn = new TemplateRootNode(templateId ?? filePath, shell, null, null, default);

            retn.attributes = ValidateRootAttributes(contentRoot, attributes);
            retn.lineInfo = new TemplateLineInfo(((IXmlLineInfo) contentRoot).LineNumber, ((IXmlLineInfo) contentRoot).LinePosition);
            retn.genericTypeResolver = genericTypeResolver;
            retn.requireType = requireType; // always null I think

            ParseChildren(retn, retn, contentRoot.Nodes());

            return true;
        }

        private void ParseChildren(TemplateRootNode templateRoot, TemplateNode parent, IEnumerable<XNode> nodes) {
            string textContext = string.Empty;
            foreach (XNode node in nodes) {
                IXmlLineInfo lineInfo2 = node;
                SetErrorContext(lineInfo2.LineNumber, lineInfo2.LinePosition);

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
                            CreateOrUpdateTextNode(templateRoot, parent, textContext, new TemplateLineInfo(textLineInfo.LineNumber, textLineInfo.LinePosition));
                            textContext = string.Empty;
                        }

                        string tagName = element.Name.LocalName;
                        string namespaceName = element.Name.NamespaceName;

                        attributes.QuickClear();
                        injectedAttributes.QuickClear();

                        ParseAttributes(tagName, element.Attributes(), attributes, injectedAttributes, out string genericTypeResolver, out string requireType);

                        IXmlLineInfo lineInfo = element;

                        if (!TryParseElementTag(templateRoot, parent, namespaceName, tagName, attributes.Clone(), new TemplateLineInfo(lineInfo.LineNumber, lineInfo.LinePosition), out TemplateNode p)) {
                            continue;
                        }

                        p.genericTypeResolver = genericTypeResolver;
                        p.requireType = requireType;

                        if (p is SlotNode slotNode) {
                            slotNode.injectedAttributes = injectedAttributes.Clone();
                        }
                        else if (injectedAttributes.size != 0) {
                            ReportParseError("Only slot nodes can have injected attributes");
                            injectedAttributes.QuickClear();
                        }

                        ParseChildren(templateRoot, p, element.Nodes());

                        continue;
                    }

                    case XmlNodeType.Comment:
                        continue;
                }

                ReportParseError($"Unable to handle node type: {node.NodeType}");
            }

            if (textContext.Length != 0) {
                CreateOrUpdateTextNode(templateRoot, parent, textContext, parent.lineInfo); // todo -- line info probably wrong
            }
        }

        private bool TryParseElementTag(TemplateRootNode templateRoot, TemplateNode parent, string namespacePath, string tagName, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo, out TemplateNode retn) {
            ProcessedType processedType;
            retn = null;

            string lowerNamespace = namespacePath.ToLower();

            if (lowerNamespace == "define") {
                processedType = TypeProcessor.GetProcessedType(typeof(UISlotDefinition));
                retn = new SlotNode(templateRoot, parent, processedType, attributes, templateLineInfo, tagName, SlotType.Define);
                templateRoot.AddSlot((SlotNode) retn);
                parent.AddChild(retn);
                return true;
            }

            if (lowerNamespace == "override") {
                processedType = TypeProcessor.GetProcessedType(typeof(UISlotOverride));
                if (!(parent is ExpandedTemplateNode expanded)) {
                    return ReportParseError(InvalidSlotOverride("override", parent.TemplateNodeDebugData, tagName));
                }

                retn = new SlotNode(templateRoot, parent, processedType, attributes, templateLineInfo, tagName, SlotType.Override);
                expanded.AddSlotOverride((SlotNode) retn);
                return true;
            }

            if (lowerNamespace == "forward") {
                processedType = TypeProcessor.GetProcessedType(typeof(UISlotForward));
                if (!(parent is ExpandedTemplateNode)) {
                    return ReportParseError(InvalidSlotOverride("forward", parent.TemplateNodeDebugData, tagName));
                }

                retn = new SlotNode(templateRoot, parent, processedType, attributes, templateLineInfo, tagName, SlotType.Forward);
                templateRoot.AddSlot((SlotNode) retn);
                parent.AddChild(retn);
                return true;
            }

            if (string.Equals(tagName, "Repeat", StringComparison.Ordinal)) {
                retn = new RepeatNode(templateRoot, parent, null, attributes, templateLineInfo);
                parent.AddChild(retn);
                return true;
            }

            if (string.IsNullOrEmpty(lowerNamespace) && string.Equals(tagName, "Children", StringComparison.Ordinal)) {
                return ReportParseError("<Children> tag is not supported. Please use an appropriate prefix `forward`, `override`, or `define`");
            }

            processedType = module.ResolveTagName(namespacePath, tagName, shell.usings);

            if (processedType == null) {
                return ReportParseError($"Unable to resolve tag name: <{tagName}>");
            }

            processedType.ValidateAttributes(attributes);

            if (processedType.IsContainerElement) {
                retn = new ContainerNode(templateRoot, parent, processedType, attributes, templateLineInfo);
            }
            else if (typeof(UITextElement).IsAssignableFrom(processedType.rawType)) {
                retn = new TextNode(templateRoot, parent, string.Empty, processedType, attributes, templateLineInfo);
            }
            else if (typeof(UITextSpanElement).IsAssignableFrom(processedType.rawType)) {
                throw new NotImplementedException();
            }
            else if (typeof(UITerminalElement).IsAssignableFrom(processedType.rawType)) {
                retn = new TerminalNode(templateRoot, parent, processedType, attributes, templateLineInfo);
            }
            else if (typeof(UIElement).IsAssignableFrom(processedType.rawType)) {
                retn = new ExpandedTemplateNode(templateRoot, parent, processedType, attributes, templateLineInfo);
            }

            if (retn == null) {
                return ReportParseError($"Unable to resolve tag name: <{tagName}>");
            }

            retn.tagName = tagName;
            retn.namespaceName = namespacePath;

            parent.AddChild(retn);

            return true;
        }

        private ProcessedType ResolveTagName(string tagName, string namespacePath, TemplateShell templateShell) {
            return null;
            // ProcessedType retn = GetDynamicElementType(templateShell, tagName);
            //
            // if (retn != null) {
            //     return retn;
            // }
            //
            // for (int i = 0; i < templateShell.usings.size; i++) {
            //     UsingDeclaration usingDef = templateShell.usings.array[i];
            //     if (usingDef.type == UsingDeclarationType.Element && usingDef.name == tagName) {
            //         int index = usingDef.pathName.IndexOf("#", StringComparison.Ordinal);
            //         if (index != -1) {
            //             string path = usingDef.pathName.Substring(0, index);
            //             string id = usingDef.pathName.Substring(index + 1);
            //             TemplateShell shell = GetOuterTemplateShell(path, null);
            //             if (shell == null) {
            //                 throw new ParseException($"Error in file {templateShell.filePath} line {usingDef.lineNumber}. Unable to find template file at path `{path}`");
            //             }
            //
            //             return GetDynamicElementType(shell, id);
            //         }
            //         else {
            //             TemplateShell shell = GetOuterTemplateShell(usingDef.pathName, null);
            //             if (shell == null) {
            //                 throw new ParseException($"Error in file {templateShell.filePath} line {usingDef.lineNumber}. Unable to find template file at path `{usingDef.pathName}`");
            //             }
            //
            //             return GetDynamicElementType(shell, tagName);
            //         }
            //     }
            // }

            // return TypeProcessor.ResolveTagName(tagName, namespacePath, templateShell.referencedNamespaces);
        }

        private static void CreateOrUpdateTextNode(TemplateRootNode templateRootRoot, TemplateNode parent, string textContent, in TemplateLineInfo templateLineInfo) {
            if (parent is TextNode textParent) {
                if (parent.ChildCount == 0) {
                    TextTemplateProcessor.ProcessTextExpressions(textContent, textParent.textExpressionList);
                }
                else {
                    TextNode node = new TextNode(templateRootRoot, parent, textContent, TypeProcessor.GetProcessedType(typeof(UITextElement)), null, templateLineInfo);
                    TextTemplateProcessor.ProcessTextExpressions(textContent, node.textExpressionList);
                    parent.AddChild(node);
                }
            }
            else {
                TextNode node = new TextNode(templateRootRoot, parent, textContent, TypeProcessor.GetProcessedType(typeof(UITextElement)), null, templateLineInfo);
                TextTemplateProcessor.ProcessTextExpressions(textContent, node.textExpressionList);
                parent.AddChild(node);
            }
        }

        private StructList<AttributeDefinition> ValidateRootAttributes(XElement root, StructList<AttributeDefinition> attributes) {
            if (attributes == null) return null;

            SetErrorContext(root);

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                // contents should remove `id` attr
                if (attr.type == AttributeType.Attribute && attr.key == "id") {
                    attributes.RemoveAt(i--);
                    continue;
                }

                if (attr.type == AttributeType.Conditional) {
                    ReportParseError($"<Contents> cannot contain conditional bindings. Ignoring {attr.rawValue}.");
                    attributes.RemoveAt(i--);
                    continue;
                }

                if (attr.type == AttributeType.Property) {
                    ReportParseError($"<Contents> cannot contain property bindings. Ignoring {attr.rawValue}.");
                    attributes.RemoveAt(i--);
                }
            }

            return attributes;
        }

        private void ParseAttributes(string tagName, IEnumerable<XAttribute> xmlAttributes, StructList<AttributeDefinition> attributes, StructList<AttributeDefinition> injectedAttributes, out string genericTypeResolver, out string requireType) {
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

                if (prefix.StartsWith("inject.")) {
                    prefix = prefix.Substring("inject.".Length);
                    HandleAttribute(prefix, name, line, column, attr.Value, injectedAttributes);
                    continue;
                }

                HandleAttribute(prefix, name, line, column, attr.Value, attributes);
            }
        }

        private void HandleAttribute(string prefix, string name, int line, int column, string value, StructList<AttributeDefinition> attributes) {
            AttributeType attributeType = AttributeType.Property;
            AttributeFlags flags = 0;

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

            string raw = string.Empty;

            // todo -- not threadsafe atm -- should use character ranges anyway
            // if (!string.IsNullOrEmpty(prefix)) {
            //     threadSafeStringBuilder.Append(prefix);
            //     threadSafeStringBuilder.Append(":");
            //     threadSafeStringBuilder.Append(name);
            //     threadSafeStringBuilder.Append("=\"");
            //     threadSafeStringBuilder.Append(value);
            //     threadSafeStringBuilder.Append("\"");
            //     raw = threadSafeStringBuilder.ToString();
            //     threadSafeStringBuilder.Clear();
            // }
            // else {
            //     threadSafeStringBuilder.Append(name);
            //     threadSafeStringBuilder.Append("=\"");
            //     threadSafeStringBuilder.Append(value);
            //     threadSafeStringBuilder.Append("\"");
            //     raw = threadSafeStringBuilder.ToString();
            //     threadSafeStringBuilder.Clear();
            // }

            attributes.Add(new AttributeDefinition(raw, attributeType, flags, name, value, shell, line, column));
        }

        private void ParseStyles(XElement root) {
            IEnumerable<XElement> styleElements = root.GetChildren("Style");

            foreach (XElement styleElement in styleElements) {
                if (TryParseStyle(styleElement, out StyleDefinition styleDefinition)) {
                    shell.styles.Add(styleDefinition);
                }
            }
        }

        private bool TryParseStyle(XElement styleElement, out StyleDefinition retn) {
            XAttribute aliasAttr = styleElement.GetAttribute("as");
            XAttribute importPathAttr = styleElement.GetAttribute("path") ?? styleElement.GetAttribute("src");

            SetErrorContext(styleElement);
            retn = default;

            string rawText = string.Empty;
            // styles can have either a class path or a body
            foreach (XNode node in styleElement.Nodes()) {
                switch (node.NodeType) {
                    case XmlNodeType.Text:
                        rawText += ((XText) node).Value;
                        continue;

                    case XmlNodeType.Element:
                        return ReportParseError("<Style> can only have text children, no elements");

                    case XmlNodeType.Comment:
                        continue;
                }

                return ReportParseError($"Unable to handle node type: {node.NodeType}");
            }

            string alias = StyleDefinition.k_EmptyAliasName;
            if (aliasAttr != null && !string.IsNullOrEmpty(aliasAttr.Value)) {
                alias = aliasAttr.Value.Trim();
            }

            // if we have a body, expect import path to be null
            if (!string.IsNullOrEmpty(rawText) && !string.IsNullOrWhiteSpace(rawText)) {
                if (importPathAttr != null && !string.IsNullOrEmpty(importPathAttr.Value)) {
                    return ReportParseError("Expected 'path' or 'src' to be null when a body is provided to a style tag");
                }

                retn = new StyleDefinition(alias, filePath + ".style", rawText);
                return true;
            }

            // if we have no body then expect path to be set
            if (importPathAttr == null || string.IsNullOrEmpty(importPathAttr.Value)) {
                return ReportParseError("Expected 'path' or 'src' to be provided when a body is not provided in a style tag");
            }

            retn = new StyleDefinition(alias, importPathAttr.Value.Trim());
            return true;
        }

        private void ParseUsings(XElement root) {
            IEnumerable<XElement> usingElements = root.GetChildren("Using");

            foreach (XElement usingElement in usingElements) {
                if (TryParseUsing(usingElement, out UsingDeclaration declaration)) {
                    shell.usings.Add(declaration);
                }
            }

            for (int i = 0; i < shell.usings.size; i++) {
                if (shell.usings.array[i].name != null) {
                    shell.referencedNamespaces.Add(shell.usings.array[i].name);
                }
            }
        }

        private void SetErrorContext(XElement element) {
            errorContext.lineNumber = ((IXmlLineInfo) element).LineNumber;
            errorContext.colNumber = ((IXmlLineInfo) element).LinePosition;
        }

        private void SetErrorContext(int line, int column) {
            errorContext.lineNumber = line;
            errorContext.colNumber = column;
        }

        private bool ReportParseError(string message) {
            module.ReportParseError(filePath, message, errorContext.lineNumber, errorContext.colNumber);
            return false;
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

        public static string InvalidSlotOverride(string verb, TemplateNodeDebugData parentData, string childTagName) {
            return $"Slot overrides can only be defined as a direct child of an expanded template. <{parentData.tagName}> is not an expanded template and cannot support slot {verb} <{childTagName}>";
        }

    }

    // public struct TemplateParseJob : IJob {

    public struct TemplateParseJob : IJobParallelFor, IJob {

        public GCHandle handle;

        public void Execute(int index) {
            List<Module.TemplateParseInfo> list = (List<Module.TemplateParseInfo>) handle.Target;

            Module.TemplateParseInfo info = list[index];

            if (info.path == null || !info.path.EndsWith(".xml", StringComparison.Ordinal)) {
                return;
            }

            new TemplateJobParser(info).ParseTemplate();
        }

        public void Execute() {
            List<Module.TemplateParseInfo> list = (List<Module.TemplateParseInfo>) handle.Target;

            for (int i = 0; i < list.Count; i++) {
                int index = i;
                Module.TemplateParseInfo info = list[index];

                if (info.path == null || !info.path.EndsWith(".xml", StringComparison.Ordinal)) {
                    continue;
                }

                new TemplateJobParser(info).ParseTemplate();
            }
        }

    }

}