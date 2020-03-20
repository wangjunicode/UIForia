using System;
using UIForia.Util;

namespace UIForia.Parsing {

    public struct TemplateParser {

        private CharStream charStream;
        private Module module;
        private string filePath;

        [ThreadStatic] private static LightStack<TemplateNode> stack;

        private void SetErrorContext() {
            // errorContext.lineNumber = ((IXmlLineInfo) element).LineNumber;
            // errorContext.colNumber = ((IXmlLineInfo) element).LinePosition;
        }

        private void SetErrorContext(int line, int column) { }

        private bool ReportParseError(string message) {
            module.ReportParseError(filePath, message, 0, 0);
            return false;
        }

        public unsafe bool TryParse(Module module, ProcessedType processedType, string contents, TemplateRootNode node) {

            charStream = new CharStream(contents);

            if (!charStream.TryMatchRange("<UITemplate>")) {
                return ReportParseError("Expected template to begin with <UITemplate>");
            }

            TemplateShell templateShell = new TemplateShell(processedType.templatePath);

            stack = stack ?? new LightStack<TemplateNode>();

            while (charStream.HasMoreTokens) {

                charStream.ConsumeWhiteSpaceAndComments();

                // if (TryReadTag(ref charStream, "Style", out CharStream styleTag)) {
                //     
                // }
                //
                // if (TryReadTag(ref charStream, "Using", out CharStream usingTag)) {
                //     
                // }
                //

                // if (TryParseContentsTag()) { }

            }

            return true;
        }

        private bool TryParseContentsTag(TemplateShell templateShell) {

            charStream.ConsumeWhiteSpaceAndComments();

            if (!charStream.TryMatchRange("<Contents")) {
                return false;
            }

            StructList<AttributeDefinition> attributes = new StructList<AttributeDefinition>(4);

            if (!charStream.TryParseCharacter('>')) {
                TryParseAttributes(attributes);
            }

            ProcessedType processedType = default;
            TemplateRootNode templateRootNode = new TemplateRootNode(
                "", templateShell, processedType, attributes,
                new TemplateLineInfo(charStream.GetLineNumber(), 0)
            );
            stack.Push(templateRootNode);
            stack.Pop();
            return true;
        }

        private bool TryParseAttribute(CharSpan key, CharSpan value, StructList<AttributeDefinition> attributes) {
            AttributeType attributeType;
            AttributeFlags flags = 0;

            CharSpan prefix = prefixOrId;
            
            
            if (prefix == "property") {
                attributeType = AttributeType.Property;
            }
            else if (prefix == "attr") {
                attributeType = AttributeType.Attribute;
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
            else if (prefix == "style") {
                attributeType = AttributeType.InstanceStyle;
            }
            else if (prefix == "evt") {
                attributeType = AttributeType.Event;
            }
            else if (prefix == "ctx") {
                attributeType = AttributeType.Context;
            }
            else if (prefix == "var") {
                attributeType = AttributeType.ImplicitVariable;
            }
            else if (prefix == "sync") {
                attributeType = AttributeType.Property;
                flags |= AttributeFlags.Sync;
            }
            else if (prefix == "expose") {
                attributeType = AttributeType.Expose;
            }
            else if (prefix == "alias") {
                attributeType = AttributeType.Alias;
            }

            return true;
        }
        
        private bool TryParseAttributes(ref CharStream stream, StructList<AttributeDefinition> attributes) {
            // identifier (.identifier)? (= ["|'|{] char* ["|'|})?

            bool hasAttributes = stream.TryGetStreamUntilWithoutWhitespace(out CharStream substream, out char end, '=', '/', '>');

            if (end == '/') {
                stream.Advance();
                // consume whitespace
                if (stream != '>') {
                }
            }
            else if (end == '>') { }

            else if (end == '=') { 
                // expect double quote, read until non escaped double quote
                // parse a single attribute, might or might not have a value at this point    
            }
            
            if (!hasAttributes) {
                return true;
            }
            
            
            if (!charStream.TryParseIdentifier(out CharSpan prefixOrId)) {
                return true;
            }

           
            return true;
        }

        // tagStack
        // push non self closing tags onto stack
        // 'complete' a tag by popping and asserting the end tag is compatible
        private LightStack<ElementTag> tagStack;

        public struct ElementTag {

            public CharSpan moduleName;
            public CharSpan tagName;
            public CharSpan attributes;

        }

        private bool TryReadTagClose(ref CharStream stream) {
            if (!stream.TryMatchRange("</")) {
                return false;
            }

            if (!stream.TryParseIdentifier(out CharSpan moduleOrTagName)) {
                // error
            }

            CharSpan moduleName = default;
            CharSpan tagName = default;

            if (charStream.TryParseCharacter(':')) {
                if (!stream.TryParseIdentifier(out tagName)) {
                    // error
                }
                moduleName = moduleOrTagName;
            }
            else {
                tagName = moduleOrTagName;
            }

            if (tagStack.Count == 0) {
                // error
            }

            ElementTag stackTop = tagStack.Peek();

            moduleName.Trim();
            tagName.Trim();

            if (stackTop.moduleName != moduleName) { }

            if (stackTop.tagName != tagName) { }

            ParseAttributes(stackTop.attributes);

            return default;

        }

        private void ParseAttributes(CharSpan stackTopAttributes) {
            throw new NotImplementedException();
        }

        private bool TryReadTag(ref CharStream stream, out CharStream output) {
            output = default;

            if (!stream.TryParseCharacter('<')) {
                return false;
            }

            if (!stream.TryParseIdentifier(out CharSpan identifier)) { }

            if (charStream.TryParseCharacter(':')) {
                if (!stream.TryParseIdentifier(out CharSpan tagName)) {
                    // error
                }
            }
            // <Tag attr={template}/>
            // <tag attr={"string"}/>
            // <Tag attr={4 > 3}>
            // <TagName .... attrs >
            // <Thing:Ello
            // </TagName>
            return true;
        }

    }

}