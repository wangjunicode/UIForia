using System;
using System.Collections.Generic;
using System.IO;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public struct UIForiaXMLNode {

        public string prefix;
        public string tagName;
        public int nextSibling;
        public int firstChild;
        public int attrStart;
        public int attrEnd;
        public int line;
        public int column;

    }

    public struct UIForiaAttributeNode {

        public string prefix;
        public string attrName;
        public int nextSibling;
        public int flags;
        public int type;
        public int value;
        public int line;
        public int column;

    }

    public class UIForiaMLParser {

        private bool hasCriticalError;
        private StructList<char> charBuffer;
        private StructStack<CharSpan> openStack;
        private TemplateFileShellBuilder builder;
        private LightList<AttributeDefinition2> attrBuffer;

        public UIForiaMLParser() {
            builder = new TemplateFileShellBuilder();
            charBuffer = new StructList<char>(128);
            openStack = new StructStack<CharSpan>();
            attrBuffer = new LightList<AttributeDefinition2>(16);
        }

        // todo -- need to verify no duplicated attributes somewhere

        private bool TryParseOuterTag(ref CharStream stream) {
            uint tagStart = stream.Ptr;
            
            if (stream.Current != '<') {
                return false;
            }

            stream.Advance();

            if (!stream.TryParseIdentifier(out CharSpan identifier)) {
                return false; //error
            }

            if (identifier == "Style") {
                // ParseStyleAttributes();
                // FindClosingStyleTagIfNotSelfClosing()
            }
            else if (identifier == "Contents") {
                stream.RewindTo(tagStart); // avoid special cases in the parser
                ParseTemplateContents(ref stream);
            }
            else if (identifier == "Using") {
                // ParseUsingAttributes();
            }
            else {
                return false;
            }

            return true;

        }

        private bool TryMakeAttribute(in CharSpan prefix, in CharSpan attrKey, in CharSpan attrValue, out AttributeDefinition2 attribute) {
            AttributeType attributeType = default;
            AttributeFlags flags = default;
            attribute = default;

            if (prefix.Length != 0) {
                if (prefix == "attr") {
                    attributeType = AttributeType.Attribute;
                }
                else if (prefix == "style") { }
                else if (prefix == "inject") { }
                else if (prefix == "property") {
                    attributeType = AttributeType.Property;
                }
                else if (prefix == "slot") {
                    attributeType = AttributeType.Slot;
                }
                else if (prefix == "mouse") {
                    attributeType = AttributeType.Mouse;
                }
                else if (prefix == "key") {
                    attributeType = AttributeType.Key;
                }
                else if (prefix == "drag") {
                    attributeType = AttributeType.Drag;
                }
                else if (prefix == "onChange") {
                    attributeType = AttributeType.ChangeHandler;
                }
                else if (prefix == "touch") {
                    attributeType = AttributeType.Touch;
                }
                else if (prefix == "controller") {
                    attributeType = AttributeType.Controller;
                }
                else if (prefix == "evt") {
                    attributeType = AttributeType.Event;
                }
                else if (prefix == "ctx") {
                    attributeType = AttributeType.Context;
                }
                else if (prefix == "var") {
                    attributeType = AttributeType.ImplicitVariable;

                    if (attrValue == "element" || attrValue == "parent" || attrValue == "root" || attrValue == "evt") {
                        LogError($"`{attrValue} is a reserved name and cannot be used as a context variable name");
                        return false;
                    }

                }
                else if (prefix == "sync") {
                    attributeType = AttributeType.Property;
                    flags |= AttributeFlags.Sync;
                }
                else if (prefix == "expose") {
                    attributeType = AttributeType.Expose;
                    if (attrValue == "element" || attrValue == "parent" || attrValue == "root" || attrValue == "evt") {
                        LogError($"`{attrValue} is a reserved name and cannot be used as a context variable name");
                        return false;
                    }
                }
                else if (prefix == "alias") {
                    attributeType = AttributeType.Alias;
                    if (attrValue == "element" || attrValue == "parent" || attrValue == "root" || attrValue == "evt") {
                        LogError($"`{attrValue} is a reserved name and cannot be used as a context variable name");
                        return false;
                    }
                }
                else {
                    LogError("Unknown attribute prefix: " + prefix);
                    return false;
                }
            }

            else {
                attributeType = AttributeType.Property;
            }

            attribute = new AttributeDefinition2() {
                flags = flags,
                type = attributeType,
                // todo -- don't need ToString() just store the template string somewhere in a buffer
                key = attrKey.ToString(),
                value = attrValue.ToString(),
                // line = lineInfo.line,
                // column = lineInfo.column
            };
            return true;
        }

        private void ParseElementAttributes(ref CharStream stream) {
            attrBuffer.size = 0;

            stream.SetCommentMode(CommentMode.None);

            while (stream.HasMoreTokens && !hasCriticalError) {

                // catch closing tag but don't step over it
                if (stream.Current == '>' || (stream.Current == '/' && stream.Next == '>')) {
                    break;
                }

                if (!stream.TryParseMultiDottedIdentifier(out CharSpan prefixOrIdentifier)) {
                    break;
                }

                CharSpan prefix = default;
                CharSpan attrKey = default;
                CharSpan attrValue = default;

                if (stream.TryParseCharacter(':')) {
                    // validate prefixOrIdentifier as prefix
                    prefix = prefixOrIdentifier;
                    // todo -- require:type=xyz;
                    if (!stream.TryParseMultiDottedIdentifier(out attrKey)) {
                        throw new Exception("bad");
                    }
                }
                else {
                    attrKey = prefixOrIdentifier;
                }

                if (stream.TryParseCharacter('=')) {
                    if (!stream.TryParseDoubleQuotedString(out attrValue)) {
                        throw new Exception("Bad");
                    }
                }

                // todo -- update compiler to allow default value attributes

                if (TryMakeAttribute(prefix, attrKey, attrValue, out AttributeDefinition2 attr)) {
                    attrBuffer.Add(attr);
                }

            }

        }

        private bool TryParseElementTag(TemplateFileShellBuilder.TemplateASTBuilder parent, ref CharStream stream) {

            uint start = stream.Ptr;
            if (!stream.TryParseCharacter('<')) {
                return false;
            }

            CharSpan tagName;
            CharSpan moduleName = default;

            if (!stream.TryParseMultiDottedIdentifier(out CharSpan prefixOrIdentifier)) {
                stream.RewindTo(start);
                return false;
            }

            TemplateNodeType nodeType = default;

            if (stream.TryParseCharacter(':')) {

                if (char.IsUpper(prefixOrIdentifier[0])) {
                    // module name
                    moduleName = prefixOrIdentifier;
                }
                else {
                    // slot or other internal specifier
                    if (prefixOrIdentifier == "define") {
                        nodeType = TemplateNodeType.SlotDefine;
                    }
                    else if (prefixOrIdentifier == "forward") {
                        nodeType = TemplateNodeType.SlotForward;
                    }
                    else if (prefixOrIdentifier == "override") {
                        nodeType = TemplateNodeType.SlotOverride;
                    }
                }

                if (!stream.TryParseMultiDottedIdentifier(out tagName)) {
                    throw new Exception("Bad");
                }

            }
            else {
                // we didnt have : specifier so the prefixOrIdentifier is actually a tag name
                tagName = prefixOrIdentifier;
            }

            ParseElementAttributes(ref stream);

            stream.ConsumeWhiteSpaceAndComments(CommentMode.None);

            if (stream.Current == '/' && stream.Next == '>') {
                stream.Advance(2);
            }
            else if (stream.Current == '>') {
                stream.Advance();
                // if was not self closing add to openStack
                if (prefixOrIdentifier.HasValue) {
                    unsafe {
                        openStack.Push( new CharSpan(stream.Data, prefixOrIdentifier.rangeStart, tagName.rangeEnd, stream.baseOffset));
                    }
                }
                else {
                    openStack.Push(tagName);
                }
            }
            else {
                throw new Exception("bad");
            }

            parent.AddElementChild(moduleName.ToString(), tagName.ToString(), default, default, "", "");

            return true;

        }

        private bool TryFindIdAttribute(out string id) {
            for (int i = 0; i < attrBuffer.size; i++) {
                if (attrBuffer.array[i].type == AttributeType.Attribute) {
                    if (attrBuffer.array[i].key == "id") {
                        id = attrBuffer.array[i].value;
                        return true;
                    }
                }
            }

            id = null;
            return false;
        }

        private void ParseTemplateContents(ref CharStream stream) {
            // attributes not yet parsed

            stream.ConsumeWhiteSpaceAndComments(CommentMode.XML);
            
            stream.TryParseCharacter('<');
            stream.TryParseIdentifier(out CharSpan contents);

            ParseElementAttributes(ref stream);

            TryFindIdAttribute(out string idValue);

            TemplateFileShellBuilder.TemplateASTBuilder rootNode = builder.CreateRootNode(idValue, attrBuffer, stream.GetLineInfo(), "generic", "require");

            stream.ConsumeWhiteSpaceAndComments(CommentMode.None);

            if (stream.Current == '>') {
                stream.Advance();
            }
            else if (stream.Current == '/' && stream.Next == '>') {
                stream.Advance(2); // if self closing, skip
                return;
            }

            openStack.Push(contents);

            // todo -- remap line numbers as post-processing step or on error
            
            while (stream.HasMoreTokens && !hasCriticalError) {

                stream.ConsumeWhiteSpaceAndComments(CommentMode.XML);

                if (TryParseElementTag(rootNode, ref stream)) {
                    continue;
                }

                if (TryParseElementClosingTag(ref stream)) {
                    continue;
                }

                if (TryParseTextContent(ref stream)) { }

            }

            // assert open stack is size = 1

        }

        private bool TryParseTextContent(ref CharStream stream) {
            return false;
        }

        private bool TryParseElementClosingTag(ref CharStream stream) {
            if (stream.Current == '<' && stream.Next == '/') {
                stream.Advance(2);
                if (!stream.TryParseIdentifier(out CharSpan tagName)) {
                    throw new Exception("Bad");
                }

                if (!stream.TryParseCharacter('>')) {
                    throw new Exception("Bad");
                }

                CharSpan span = openStack.Pop();
                if (span != tagName) {
                    throw new Exception("Bad");
                }

                return true;
            }

            return false;
        }

        public bool TryParse(string contents) {
            hasCriticalError = false;
            unsafe {

                fixed (char* ptr = contents) {
                    CharStream stream = new CharStream(ptr, 0, contents.Length);

                    stream.ConsumeWhiteSpaceAndComments(CommentMode.XML);
                    
                    if (!stream.TryParseCharacter('<') || !stream.TryParseIdentifier(out CharSpan rootTagName) || rootTagName != "UITemplate" || !stream.TryParseCharacter('>')) {
                        hasCriticalError = true;
                        LogError("Templates must begin with a <UITemplate> root");
                        return false;
                    }
                    
                    stream.ConsumeWhiteSpaceAndComments();

                    openStack.Push(rootTagName); // push the <UITemplate> tag

                    while (stream.HasMoreTokens && !hasCriticalError) {

                        if (!TryParseOuterTag(ref stream)) {
                            LogError("Expected to parse a valid outer Tag <Style> <Contents> or <Using> but hit end of file");
                            break;
                        }

                    }
                    
                    // assert open stack is empty

                }
            }

            return true;
        }

        private void LogError(string error) {
            Debug.LogError(error);
            hasCriticalError = true;
        }
    }

    class NanoXMLBase {

        protected static bool IsSpace(char c) {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        protected static void SkipSpaces(string str, ref int i) {
            while (i < str.Length) {
                if (!IsSpace(str[i])) {
                    if (str[i] == '<' && i + 4 < str.Length && str[i + 1] == '!' && str[i + 2] == '-' && str[i + 3] == '-') {
                        i += 4; // skip <!--

                        while (i + 2 < str.Length && !(str[i] == '-' && str[i + 1] == '-'))
                            i++;

                        i += 2; // skip --
                    }
                    else
                        break;
                }

                i++;
            }
        }

        protected static string GetValue(string str, ref int i, char endChar, char endChar2, bool stopOnSpace) {
            int start = i;
            while ((!stopOnSpace || !IsSpace(str[i])) && str[i] != endChar && str[i] != endChar2) i++;

            return str.Substring(start, i - start);
        }

        protected static bool IsQuote(char c) {
            return c == '"' || c == '\'';
        }

        protected static string ParseAttributes(string str, ref int i, List<NanoXMLAttribute> attributes, char endChar, char endChar2) {
            SkipSpaces(str, ref i);
            string name = GetValue(str, ref i, endChar, endChar2, true);

            SkipSpaces(str, ref i);

            while (str[i] != endChar && str[i] != endChar2) {
                string attrName = GetValue(str, ref i, '=', '\0', true);

                SkipSpaces(str, ref i);
                i++; // skip '='
                SkipSpaces(str, ref i);

                char quote = str[i];
                if (!IsQuote(quote))
                    throw new XMLParsingException("Unexpected token after " + attrName);

                i++; // skip quote
                string attrValue = GetValue(str, ref i, quote, '\0', false);
                i++; // skip quote

                attributes = attributes ?? new List<NanoXMLAttribute>();
                attributes.Add(new NanoXMLAttribute(attrName, attrValue));

                SkipSpaces(str, ref i);
            }

            return name;
        }

    }

    class NanoXMLDocument : NanoXMLBase {

        private NanoXMLNode rootNode;
        private List<NanoXMLAttribute> declarations = new List<NanoXMLAttribute>();

        /// <summary>
        /// Public constructor. Loads xml document from raw string
        /// </summary>
        /// <param name="xmlString">String with xml</param>
        public NanoXMLDocument(string xmlString) {
            int i = 0;

            while (true) {
                SkipSpaces(xmlString, ref i);

                char thing = xmlString[i];
                if (xmlString[i] != '<')
                    throw new XMLParsingException("Unexpected token");

                i++; // skip <

                if (xmlString[i] == '?') // declaration
                {
                    i++; // skip ?
                    ParseAttributes(xmlString, ref i, declarations, '?', '>');
                    i++; // skip ending ?
                    i++; // skip ending >

                    continue;
                }

                if (xmlString[i] == '!') // doctype
                {
                    while (xmlString[i] != '>') // skip doctype
                        i++;

                    i++; // skip >

                    continue;
                }

                rootNode = new NanoXMLNode(xmlString, ref i);
                break;
            }
        }

        /// <summary>
        /// Root document element
        /// </summary>
        public NanoXMLNode RootNode {
            get { return rootNode; }
        }

        /// <summary>
        /// List of XML Declarations as <see cref="NanoXMLAttribute"/>
        /// </summary>
        public IEnumerable<NanoXMLAttribute> Declarations {
            get { return declarations; }
        }

    }

    /// <summary>
    /// Element node of document
    /// </summary>
    class NanoXMLNode : NanoXMLBase {

        private string value;
        private string name;

        private List<NanoXMLNode> subNodes = new List<NanoXMLNode>();
        private List<NanoXMLAttribute> attributes = new List<NanoXMLAttribute>();

        internal NanoXMLNode(string str, ref int i) {
            name = ParseAttributes(str, ref i, attributes, '>', '/');

            if (str[i] == '/') // if this node has nothing inside
            {
                i++; // skip /
                i++; // skip >
                return;
            }

            i++; // skip >

            // temporary. to include all whitespaces into value, if any
            int tempI = i;

            SkipSpaces(str, ref tempI);

            if (str[tempI] == '<') {
                i = tempI;

                while (str[i + 1] != '/') // parse subnodes
                {
                    i++; // skip <
                    subNodes.Add(new NanoXMLNode(str, ref i));

                    SkipSpaces(str, ref i);

                    if (i >= str.Length)
                        return; // EOF

                    if (str[i] != '<')
                        throw new XMLParsingException("Unexpected token");
                }

                i++; // skip <
            }
            else // parse value
            {
                value = GetValue(str, ref i, '<', '\0', false);
                i++; // skip <

                if (str[i] != '/')
                    throw new XMLParsingException("Invalid ending on tag " + name);
            }

            i++; // skip /
            SkipSpaces(str, ref i);

            string endName = GetValue(str, ref i, '>', '\0', true);
            if (endName != name)
                throw new XMLParsingException("Start/end tag name mismatch: " + name + " and " + endName);
            SkipSpaces(str, ref i);

            if (str[i] != '>')
                throw new XMLParsingException("Invalid ending on tag " + name);

            i++; // skip >
        }

        /// <summary>
        /// Returns subelement by given name
        /// </summary>
        /// <param name="nodeName">Name of subelement to get</param>
        /// <returns>First subelement with given name or NULL if no such element</returns>
        public NanoXMLNode this[string nodeName] {
            get {
                foreach (NanoXMLNode nanoXmlNode in subNodes)
                    if (nanoXmlNode.name == nodeName)
                        return nanoXmlNode;

                return null;
            }
        }

        /// <summary>
        /// Returns attribute by given name
        /// </summary>
        /// <param name="attributeName">Attribute name to get</param>
        /// <returns><see cref="NanoXMLAttribute"/> with given name or null if no such attribute</returns>
        public NanoXMLAttribute? GetAttribute(string attributeName) {
            foreach (NanoXMLAttribute nanoXmlAttribute in attributes)
                if (nanoXmlAttribute.name == attributeName)
                    return nanoXmlAttribute;

            return null;
        }

    }

    struct NanoXMLAttribute {

        public string name;
        public string value;
        public AttributeType attributeType;
        public AttributeFlags flags;

        internal NanoXMLAttribute(string name, string value) : this() {
            this.name = name;
            this.value = value;
        }

    }

    class XMLParsingException : Exception {

        public XMLParsingException(string message) : base(message) { }

    }

}