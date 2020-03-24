using System;
using UIForia.Util;
using WhitespaceHandling = UIForia.Util.WhitespaceHandling;

namespace UIForia.Parsing {

    public struct TemplateParser_XML_Bad {

        private CharStream charStream;
        private Module module;
        private string filePath;

        [ThreadStatic] private static LightStack<TemplateNode> stack;

        private bool ReportParseError(string message, int line = -1) {
            // module.ReportParseError(filePath, message, line < 0 ? charStream.GetLineNumber() : line, 0);
            return false;
        }

        public bool TryParse(string contents) {

            charStream = new CharStream(contents);
            if (!charStream.TryMatchRange("<UITemplate>")) {
                return ReportParseError("Expected template to begin with <UITemplate>");
            }

            stack = stack ?? new LightStack<TemplateNode>();

            while (charStream.HasMoreTokens) {

                if (!TryParseTag(ref charStream, out TagData tagData)) {
                    return false;
                }

                if (!tagData.isSelfClosing) {
                    TryParseTagEnd(ref charStream, tagData);
                }

            }

            return true;
        }

        private bool TryParseTagEnd(ref CharStream charStream, in TagData tagData) {

            if (tagData.moduleName == default(CharSpan)) {
                while (charStream.HasMoreTokens) {
                    if (charStream.TryMatchRange("</")) {
                        if (charStream.TryMatchRange(tagData.tagName) && charStream.TryParseCharacter('>')) { }
                    }
                }
            }
            else { }

            return true;
        }

        private unsafe bool TryParseTemplateContents(ref CharStream stream, out TagData tagData, out CharStream contentStream) {

            contentStream = default;

            if (!TryParseTag(ref stream, out tagData) || !(tagData.moduleName == default(CharSpan) && tagData.tagName == "Contents")) {
                return false;
            }

            contentStream = new CharStream(stream);
            while (contentStream.HasMoreTokens) {

                int idx = contentStream.NextIndexOf('<');

                if (idx == -1) {
                    return ReportParseError("Unable to find matching </Contents> for <Contents> tag.", stream.GetLineNumber());
                }

                contentStream.AdvanceTo(idx + 1);

                if (stream.TryMatchRange("/Contents>")) {
                    contentStream = new CharStream(stream.Data, stream.Ptr, contentStream.Ptr);
                    return true;
                }

            }

            return ReportParseError("Unable to find matching </Contents> for <Contents> tag.", stream.GetLineNumber());

        }

        internal struct AttributeData {

            public CharSpan key;
            public CharSpan value;

        }

        internal struct TagData {

            public bool isSelfClosing;
            public CharSpan moduleName;
            public CharSpan tagName;
            public AttributeData[] attributes;

        }

        // todo decide what to do with attributes, can probably all live in a single array per template and get indexed.
        internal unsafe bool TryParseTag(ref CharStream stream, out TagData tagData) {

            tagData = default;

            if (!stream.TryParseCharacter('<')) {
                return false;
            }

            CharSpan tagName;

            if (!stream.TryParseIdentifier(out CharSpan moduleName)) {
                return false;
            }

            if (stream.TryParseCharacter(':')) {
                if (!stream.TryParseIdentifier(out tagName, false, WhitespaceHandling.ConsumeAfter)) {
                    return ReportParseError("Expected a valid element name after reading <" + moduleName + ":");
                }
            }
            else {
                tagName = moduleName;
                moduleName = default;
            }

            tagData.tagName = tagName;
            tagData.moduleName = moduleName;

            int attrCount = 0;
            AttributeData* attributes = stackalloc AttributeData[32];

            while (stream.HasMoreTokens) {

                if (!stream.TryGetStreamUntil(out CharSpan keySpan, '/', '>', '=')) {
                    return ReportParseError("Unexpected end of element attributes", stream.GetLineNumber());
                }

                if (stream.TryParseCharacter('=')) {

                    if (!stream.TryParseCharacter('"')) {
                        return ReportParseError("Unexpected end of element attributes", stream.GetLineNumber());
                    }

                    // todo -- escape quotes? how would this get handled? expression parser needs to know about it I think.

                    int idx = stream.NextIndexOf('"');

                    if (idx == -1) {
                        return ReportParseError("Unterminated attribute: " + keySpan, stream.GetLineNumber());
                    }

                    CharSpan valueSpan = new CharSpan(stream.Data, stream.IntPtr, idx);

                    stream.AdvanceTo(idx);

                    attributes[attrCount] = new AttributeData() {
                        key = keySpan,
                        value = valueSpan
                    };

                    attrCount++;

                }
                else if (stream.TryMatchRange("/>")) {
                    tagData.isSelfClosing = true;
                    break;
                }
                else if (stream.TryParseCharacter('>')) {
                    break;
                }
                else {
                    return ReportParseError("Unexpected end of element attributes", stream.GetLineNumber());
                }

            }

            if (attrCount != 0) {
                tagData.attributes = new AttributeData[attrCount];
                for (int i = 0; i < attrCount; i++) {
                    tagData.attributes[i] = attributes[i];
                }
            }

            return true;
        }

        private bool TryParseAttribute(CharSpan key, CharSpan value, StructList<AttributeDefinition> attributes) {
            AttributeType attributeType;
            AttributeFlags flags = 0;

            CharSpan prefix = key;

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