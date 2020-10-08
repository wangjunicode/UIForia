using System;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public class UIForiaMLParser {

        private bool hasCriticalError;
        private StructStack<CharSpan> openStack;
        private StructStack<TemplateFileShellBuilder.TemplateASTBuilder> elementStack;
        private TemplateFileShellBuilder builder;
        private LightList<AttributeDefinition3> attrBuffer;
        private StructList<char> textBuffer;
        private Diagnostics diagnostics;
        private string filePath;

        public UIForiaMLParser(Diagnostics diagnostics = null) {
            this.diagnostics = diagnostics;
            this.builder = new TemplateFileShellBuilder();
            this.textBuffer = new StructList<char>(1024);
            this.openStack = new StructStack<CharSpan>();
            this.attrBuffer = new LightList<AttributeDefinition3>(16);
            this.elementStack = new StructStack<TemplateFileShellBuilder.TemplateASTBuilder>(16);
        }

        public const int Version = 2;

        public static bool IsProbablyTemplate(string contents) {
            unsafe {
                fixed (char* ptr = contents) {
                    CharStream stream = new CharStream(ptr, 0, contents.Length);
                    stream.ConsumeWhiteSpaceAndComments(CommentMode.XML);
                    if (stream.TryMatchRange("<?")) {
                        stream.ConsumeUntilFound("?>", out CharSpan _);
                    }

                    stream.ConsumeWhiteSpaceAndComments(CommentMode.XML);
                    return stream.HasMoreTokens && stream.TryMatchRange("<UITemplate");
                }
            }
        }

        public bool TryParse(string filePath, string contents, out TemplateFileShell result) {
            this.filePath = filePath;
            hasCriticalError = false;
            result = default;
            openStack.size = 0;
            attrBuffer.size = 0;
            elementStack.size = 0;
            textBuffer.size = 0;
            unsafe {
                fixed (char* ptr = contents) {
                    CharStream stream = new CharStream(ptr, 0, contents.Length);

                    stream.ConsumeWhiteSpaceAndComments(CommentMode.XML);

                    if (stream.TryMatchRange("<?")) {
                        stream.ConsumeUntilFound("?>", out CharSpan _);
                    }

                    if (!stream.TryParseCharacter('<') || !stream.TryParseIdentifier(out CharSpan rootTagName) || rootTagName != "UITemplate" || !stream.TryParseCharacter('>')) {
                        hasCriticalError = true;
                        LogError("Templates must begin with a <UITemplate> root");
                        return false;
                    }

                    stream.ConsumeWhiteSpaceAndComments();

                    openStack.Push(rootTagName); // push the <UITemplate> tag

                    while (stream.HasMoreTokens && !hasCriticalError) {
                        uint p = stream.Ptr;
                        stream.ConsumeWhiteSpaceAndComments(CommentMode.XML);

                        if (stream.TryMatchRange("</UITemplate")) {
                            stream.ConsumeWhiteSpaceAndComments(CommentMode.None);
                            if (stream.Current == '>') {
                                openStack.Pop();
                            }

                            break;
                        }

                        if (!TryParseOuterTag(ref stream)) {
                            break;
                        }

                        if (!hasCriticalError && p == stream.Ptr) {
                            break;
                        }
                    }

                    // assert open stack is empty except for the <UITemplate> root node
                    if (!hasCriticalError && openStack.size != 0) {
                        LogError("Pushed more tags than popped");
                        return false;
                    }
                }
            }

            if (hasCriticalError) {
                return false;
            }

            result = builder.Build(filePath);
            return true;
        }

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
                if (!TryParseStyle(ref stream)) {
                    return false;
                }
            }
            else if (identifier == "Contents") {
                stream.RewindTo(tagStart); // avoid special cases in the parser
                ParseTemplateContents(ref stream);
            }
            else if (identifier == "Using") {
                if (!ParseUsingAttributes(identifier, ref stream)) {
                    return false;
                }
            }
            else {
                return false;
            }

            return true;
        }

        private const string s_Style = "</Style";

        private bool TryParseStyle(ref CharStream stream) {
            ParseElementAttributes(ref stream);

            stream.ConsumeWhiteSpaceAndComments(CommentMode.None);

            if (stream.Current == '>') {
                stream.Advance();

                if (attrBuffer.size != 0) {
                    LogError("A <Style> node that is not self-closing cannot take attributes.", stream.GetLineInfo());
                    return false;
                }

                unsafe {
                    int length = s_Style.Length;
                    fixed (char* styleTag = s_Style) {
                        // Note -- this will not consume comments which means we'll need to remove xml comments before actually parsing the style 
                        if (!stream.ConsumeUntilFound(styleTag, length, out CharSpan styleSource)) {
                            LogError("Expected to find a terminating </Style> tag but hit the end of the file without finding one.");
                            return false;
                        }

                        stream.ConsumeWhiteSpaceAndComments(CommentMode.None);

                        if (stream.Current != '>') {
                            LogError("Expected a terminating `>` after </Style", stream.GetLineInfo());
                            return false;
                        }

                        stream.Advance();

                        builder.AddStyleSource(styleSource);
                    }
                }
            }
            else if (stream.Current == '/' && stream.Next == '>') {
                stream.Advance(2);
                AttributeDefinition3? srcAttr = GetAttribute(AttributeType.Property, "src");
                AttributeDefinition3? aliasAttr = GetAttribute(AttributeType.Property, "alias");

                if (srcAttr == null) {
                    // non critical error, style will just be ignored
                    return true;
                }

                if (builder.StyleSourceIsReferenced(srcAttr.Value.value)) {
                    LogError($"Unable to add <Style> node because another style node referencing `{builder.GetCharSpan(srcAttr.Value.value)}` was already declared", stream.GetLineInfo());
                    return false;
                }

                if (aliasAttr != null && builder.StyleAliasIsDeclared(aliasAttr.Value.value)) {
                    LogError($"Unable to add <Style> node because another style node already declared an alias `{builder.GetCharSpan(aliasAttr.Value.value)}`", stream.GetLineInfo());
                    return false;
                }

                builder.AddStyleReference(srcAttr.Value.value, aliasAttr?.value ?? default);
            }
            else {
                // invalid
                LogError("Expected the <Style tag to be closed but didn't find `>` or `/>`", stream.GetLineInfo());
                return false;
            }

            return true;
        }

        private bool ParseUsingAttributes(CharSpan usingSpan, ref CharStream stream) {
            ParseElementAttributes(ref stream);

            // ensure the <Using> is self closing
            stream.ConsumeWhiteSpaceAndComments(CommentMode.None);

            if (stream.Current != '/' || stream.Next != '>') {
                LogError("Expected <Using> to be self closing (<Using/>)", usingSpan.GetLineInfo());
                return false;
            }

            stream.Advance(2); // step over closing />

            AttributeDefinition3? namespaceAttr = GetAttribute(AttributeType.Property, "namespace");

            if (namespaceAttr == null) {
                LogError("<Using/> tags require a `namespace` attribute", usingSpan.GetLineInfo());
                return false;
            }

            builder.AddUsing(new UsingDeclaration() {
                namespaceRange = namespaceAttr.Value.value,
            });

            return true;
        }

        private AttributeDefinition3? GetAttribute(AttributeType type, string attrKey) {
            for (int i = 0; i < attrBuffer.size; i++) {
                if (attrBuffer.array[i].type == type) {
                    CharSpan span = builder.GetCharSpan(attrBuffer.array[i].key);
                    if (span == attrKey) {
                        return attrBuffer.array[i];
                    }
                }
            }

            return null;
        }

        private bool TryMakeAttribute(in CharSpan prefix, CharSpan attrKey, in CharSpan attrValue, out AttributeDefinition3 attribute) {
            AttributeType attributeType = default;
            AttributeFlags flags = default;
            attribute = default;

            if (prefix.Length != 0) {
                if (prefix == "attr") {
                    attributeType = AttributeType.Attribute;
                    if (attrValue.HasValue) {
                        if (!attrValue.StartsWith('{') && !attrValue.EndsWith('}')) {
                            flags |= AttributeFlags.Const;
                        }
                    }
                }
                else if (prefix == "require") {
                    attributeType = AttributeType.RequireType;
                }
                else if (prefix == "generic") {
                    if (attrKey == "type") {
                        attributeType = AttributeType.GenericType;
                    }
                    else {
                        return false; // error message?
                    }
                }
                else if (prefix == "style") {
                    attributeType = AttributeType.InstanceStyle;
                    if (attrKey.Contains(".")) {
                        if (attrKey.StartsWith("hover.")) {
                            flags |= AttributeFlags.StyleStateHover;
                            attrKey = attrKey.Substring("hover.".Length);
                        }
                        else if (attrKey.StartsWith("focus.")) {
                            flags |= AttributeFlags.StyleStateFocus;
                            attrKey = attrKey.Substring("focus.".Length);
                        }
                        else if (attrKey.StartsWith("active.")) {
                            flags |= AttributeFlags.StyleStateActive;
                            attrKey = attrKey.Substring("active.".Length);
                        }
                        else {
                            diagnostics.LogWarning("Invalid style property");
                        }
                    }
                }
                else if (prefix == "mixin") {
                    attributeType = AttributeType.MixinDefinition;
                }
                
                else if (prefix == "inject") {
                    throw new NotImplementedException("Inject attributes not implemented");
                }
                
                else if (prefix == "create") {

                    if (attrKey == "disabled") {
                        attributeType = AttributeType.CreateDisabled;
                    }
                    else if (attrKey == "lazy") {
                        attributeType = AttributeType.CreateLazy;
                    }
                    else {
                        return false; // error message?
                    }
                }
                else if (prefix == "disable") {
                    if (attrKey == "destroy") {
                        attributeType = AttributeType.DestroyChildrenOnDisable;
                    }
                    else {
                        return false; // error message?
                    }
                }
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
                else if (prefix == "painter") {
                    attributeType = AttributeType.PainterVar;
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
                    LogError("Unknown attribute prefix: " + prefix); // todo -- for the mixin case we need to prefix appropriately and this should not be an error. should handle this as part of validation
                    return false;
                }
            }
            else {
                attributeType = AttributeType.Property;
            }

            // todo -- probably kinda slow
            LineInfo lineInfo = attrKey.GetLineInfo();

            for (int i = 0; i < attrBuffer.size; i++) {
                // todo -- de-duplicate attributes here
                // make sure keys are different
            }

            attribute = new AttributeDefinition3() {
                flags = flags,
                type = attributeType,
                key = builder.AddString(attrKey, true),
                value = builder.AddString(attrValue),
                line = lineInfo.line,
                column = lineInfo.column
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

                if (!stream.TryParseMultiDottedIdentifier(out CharSpan prefixOrIdentifier, WhitespaceHandling.ConsumeAll, true)) {
                    break;
                }

                CharSpan attrKey;
                CharSpan prefix = default;
                CharSpan attrValue = default;

                if (stream.TryParseCharacter(':')) {
                    // validate prefixOrIdentifier as prefix
                    prefix = prefixOrIdentifier;
                    if (!stream.TryParseMultiDottedIdentifier(out attrKey, WhitespaceHandling.ConsumeAll, true)) {
                        LogError($"Expected a valid attribute name after `:` for attribute `{prefix}`", prefix.GetLineInfo());
                        return;
                    }
                }
                else {
                    attrKey = prefixOrIdentifier;
                }

                if (stream.TryParseCharacter('=')) {
                    if (!stream.TryParseDoubleQuotedString(out attrValue) && !stream.TryParseSingleQuotedString(out attrValue)) {
                        LogError($"Expected a quoted attribute value or expression after the `=` for attribute `{attrKey}`", attrKey.GetLineInfo());
                        return;
                    }
                }

                // todo -- update compiler to allow default value attributes

                if (TryMakeAttribute(prefix, attrKey, attrValue, out AttributeDefinition3 attr)) {
                    attrBuffer.Add(attr);
                }
            }
        }

        private unsafe CharSpan GetFullTagRange(in CharSpan prefix, in CharSpan tagName) {
            return prefix.HasValue ? new CharSpan(tagName.data, prefix.rangeStart, tagName.rangeEnd, tagName.baseOffset) : tagName;
        }

        private bool TryParseElementTag(ref CharStream stream) {
            if (!TryParseTag(ref stream, false, out CharSpan prefix, out CharSpan tagName)) {
                return false;
            }

            ParseElementAttributes(ref stream);

            stream.ConsumeWhiteSpaceAndComments(CommentMode.None);

            CharSpan fullTagRange = GetFullTagRange(prefix, tagName);

            TemplateFileShellBuilder.TemplateASTBuilder element;
            LineInfo lineInfo = tagName.GetLineInfo(); // todo -- line info remapping

            if (!prefix.HasValue || char.IsUpper(prefix[0])) {
                element = elementStack.Peek().AddElementChild(prefix, tagName, attrBuffer, lineInfo);
            }
            else if (prefix == "define") {
                element = elementStack.Peek().AddSlotChild(tagName, attrBuffer, lineInfo, SlotType.Define);
            }
            else if (prefix == "forward") {
                element = elementStack.Peek().AddSlotChild(tagName, attrBuffer, lineInfo, SlotType.Forward);
            }
            else if (prefix == "override") {
                element = elementStack.Peek().AddSlotChild(tagName, attrBuffer, lineInfo, SlotType.Override);
            }
            else {
                LogError("Unknown directive `" + prefix + "`. Valid directives are (define, forward, or override). If you intended to reference a module you must uppercase the prefix", prefix.GetLineInfo());
                return false;
            }

            if (stream.Current == '/' && stream.Next == '>') {
                // if was self closing we have no children and do not add it to the open or element stacks
                stream.Advance(2);
            }
            else if (stream.Current == '>') {
                // if was not self closing add to the open and element stacks
                stream.Advance();
                openStack.Push(fullTagRange);
                elementStack.Push(element);
            }
            else {
                LogError("Expected a closing '>' or '/>' after opening the tag <" + fullTagRange + "> but was not found", fullTagRange.GetLineInfo());
                return false;
            }

            return true;
        }

        private bool TryFindIdAttribute(out RangeInt id) {
            for (int i = 0; i < attrBuffer.size; i++) {
                if (attrBuffer.array[i].type == AttributeType.Attribute) {
                    CharSpan span = builder.GetCharSpan(attrBuffer.array[i].key);

                    if (span == "id") {
                        id = attrBuffer.array[i].value;
                        return true;
                    }
                }
            }

            id = default;
            return false;
        }

        private void ParseTemplateContents(ref CharStream stream) {
            // attributes not yet parsed

            stream.ConsumeWhiteSpaceAndComments(CommentMode.XML);

            stream.TryParseCharacter('<');
            stream.TryParseIdentifier(out CharSpan contents);

            ParseElementAttributes(ref stream);

            TryFindIdAttribute(out RangeInt idValue);

            stream.ConsumeWhiteSpaceAndComments(CommentMode.None);

            if (stream.Current == '>') {
                stream.Advance();
            }
            else if (stream.Current == '/' && stream.Next == '>') {
                stream.Advance(2); // if self closing, skip

                builder.CreateRootNode(idValue, attrBuffer, contents.GetLineInfo());

                return;
            }

            openStack.Push(contents);

            //todo assert id for template is not already taken in this file
            elementStack.Push(builder.CreateRootNode(idValue, attrBuffer, contents.GetLineInfo()));

            // todo -- remap line numbers as post-processing step or on error

            while (elementStack.size > 0 && stream.HasMoreTokens && !hasCriticalError) {
                if (TryParseTextContent(ref stream)) {
                    TemplateFileShellBuilder.TemplateASTBuilder parent = elementStack.Peek();

                    if (parent.GetNodeType() == TemplateNodeType.Text) {
                        // todo -- line numbers are not correct
                        parent.SetTextContent(textBuffer, default);
                    }
                    else {
                        // todo -- line numbers are not correct
                        TemplateFileShellBuilder.TemplateASTBuilder textNode = parent.AddTextChild(null, stream.GetLineInfo());
                        textNode.SetTextContent(textBuffer, default);
                    }

                    continue;
                }

                stream.ConsumeWhiteSpaceAndComments(CommentMode.XML);

                if (TryParseElementTag(ref stream)) {
                    continue;
                }

                TryParseElementClosingTag(ref stream);
            }

            openStack.Pop();
        }

        private bool TryParseTextContent(ref CharStream stream) {
            textBuffer.size = 0;

            bool hasNonWhitespace = false;
            while (stream.HasMoreTokens) {
                char current = stream.Current;

                switch (current) {
                    // see if its a comment
                    case '<' when stream.Next == '!':
                        // looks like a comment, try to consume it
                        stream.ConsumeXMLComment();
                        break;

                    case '<':
                        // stop
                        return hasNonWhitespace;

                    case '&': {
                        // handle escaping stuff
                        hasNonWhitespace = true;
                        if (stream.TryMatchRange("&amp;")) {
                            textBuffer.Add('&');
                        }
                        else if (stream.TryMatchRange("&lt;")) {
                            textBuffer.Add('<');
                        }
                        else if (stream.TryMatchRange("&gt;")) {
                            textBuffer.Add('>');
                        }
                        else {
                            textBuffer.Add('&');
                        }

                        break;
                    }

                    default: {
                        if (!hasNonWhitespace && !(current == ' ' || current == '\t' || current == '\n' || current == '\r')) {
                            hasNonWhitespace = true;
                        }

                        textBuffer.Add(current);
                        break;
                    }
                }

                stream.Advance();
            }

            return false;
        }

        private bool TryParseTag(ref CharStream stream, bool closing, out CharSpan prefix, out CharSpan tagName) {
            prefix = default;
            tagName = default;

            uint start = stream.Ptr;

            if (!stream.TryParseCharacter('<', WhitespaceHandling.None)) {
                stream.RewindTo(start);
                return false;
            }

            if (closing && !stream.TryParseCharacter('/', WhitespaceHandling.None)) {
                stream.RewindTo(start);
                return false;
            }

            if (!stream.TryParseMultiDottedIdentifier(out CharSpan prefixOrIdentifier, WhitespaceHandling.ConsumeAll, true)) {
                stream.RewindTo(start);
                return false;
            }

            if (stream.TryParseCharacter(':')) {
                if (!stream.TryParseMultiDottedIdentifier(out tagName, WhitespaceHandling.ConsumeAll, true)) {
                    LogError("Expected to find a valid tag name after the ':' while parsing tag `<" + prefixOrIdentifier + ":` ", prefixOrIdentifier.GetLineInfo());
                    return false;
                }

                prefix = prefixOrIdentifier;
            }
            else {
                // we didnt have : specifier so the prefixOrIdentifier is actually a tag name
                tagName = prefixOrIdentifier;
            }

            if (!closing) return true;

            stream.ConsumeWhiteSpaceAndComments();

            if (!stream.TryParseCharacter('>')) {
                CharSpan fullTagRange = GetFullTagRange(prefix, tagName);
                LogError("Expected closing tag for `<" + fullTagRange + ">` to terminated with '>' but it was not", fullTagRange.GetLineInfo());
                return false;
            }

            return true;
        }

        private unsafe bool TryParseElementClosingTag(ref CharStream stream) {
            if (!TryParseTag(ref stream, true, out CharSpan prefix, out CharSpan tagName)) {
                return false;
            }

            CharSpan fullTagRange;
            if (prefix.HasValue) {
                fullTagRange = new CharSpan(stream.Data, prefix.rangeStart, tagName.rangeEnd, stream.baseOffset);
            }
            else {
                fullTagRange = tagName;
            }

            CharSpan span = openStack.Pop();
            if (span != fullTagRange) {
                LogError("Expected to find closing tag for <" + span + "> but instead found </" + fullTagRange + ">", fullTagRange.GetLineInfo());
                return false;
            }

            if (elementStack.size != 0) {
                elementStack.Pop();
            }

            return true;
        }

        private void LogError(string error, LineInfo lineInfo) {
            error = "[UIForia::ParseError]@" + filePath + "|" + error;
            if (diagnostics != null) {
                diagnostics.LogError(error, filePath, lineInfo.line, lineInfo.column);
            }
            else {
                Debug.LogError(error + "(" + lineInfo + ")");
            }

            hasCriticalError = true;
        }

        private void LogError(string error) {
            if (diagnostics != null) {
                diagnostics.LogError(error);
            }
            else {
                Debug.Log(error);
            }

            hasCriticalError = true;
        }

    }

}