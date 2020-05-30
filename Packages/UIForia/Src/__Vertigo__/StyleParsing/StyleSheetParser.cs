using System;
using UIForia.Style;
using UIForia.Style2;
using UIForia.Util;

namespace UIForia.NewStyleParsing {

    public unsafe struct StyleSheetParser3 {

        private StyleState2 currentStyleState;
        private ParseMode currentParseMode;
        private CharSpan currentScopeName;
        private bool hitCriticalFailure;
        private bool hitFailure;
        private Diagnostics diagnostics;
        private string filePath;
        private StyleFileBuilder builder;

        [ThreadStatic] private static StructList<char> s_CharBuffer;

        public ParseResult TryParseFile(StyleFile styleFile, Diagnostics diagnostics, out ParsedStyleFile parsedStyleFile) {
            this.builder = builder ?? new StyleFileBuilder();
            this.diagnostics = diagnostics;
            this.filePath = styleFile.filePath;

            fixed (char* charptr = styleFile.contents) {
                CharStream stream = new CharStream(charptr, 0, (uint) styleFile.contents.Length);

                NodeRef nodeRef = builder.CreateRoot();

                while (stream.HasMoreTokens && !hitCriticalFailure) {

                    stream.ConsumeWhiteSpaceAndComments();

                    if (stream.TryMatchRange("style")) {
                        ParseStyleShell(nodeRef, ref stream);
                    }
                    else {
                        parsedStyleFile = default;
                        return ReportCriticalParseError("Unexpected end of style sheet");
                    }
                }
            }

            if (hitCriticalFailure) {
                parsedStyleFile = default;
                return ParseResult.CriticalFailure;
            }

            if (hitFailure) {
                parsedStyleFile = default;
                return ParseResult.RecoverableFailure;
            }

            parsedStyleFile = builder.Build();
            return ParseResult.Success;
        }

        private ParseResult ParseStyleShell(NodeRef nodeRef, ref CharStream stream) {

            if (!stream.TryParseIdentifier(out CharSpan styleName)) {
                return ReportCriticalParseError("Expected to find an identifier after 'style' token.", stream.GetLineInfo());
            }

            // later validation step?
            // if (!ValidateStyleName(styleName)) {
            //     return ReportRecoverableParseError($"Duplicate style name: '{styleName}'", styleName.GetLineNumber());
            // }

            // if (stream.TryParseCharacter(':')) {
            //     ParseStyleExtension(ref stream);
            // }

            if (!stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                return ReportCriticalParseError($"Invalid style block for style {styleName}", stream.GetLineInfo());
            }

            return ParseStyleBody(nodeRef.AddStyleNode(styleName), ref bodyStream);

        }

        private ParseResult ParseStyleBody(NodeRef nodeRef, ref CharStream stream) {

            while (stream.HasMoreTokens && !hitCriticalFailure) {
                stream.ConsumeWhiteSpaceAndComments();

                if (!stream.HasMoreTokens) {
                    break;
                }

                // if (TryParseConditionBlock(ref stream)) {
                //     continue;
                // }

                // [hover] { }
                // [enter] ...
                if (TryReadBlockHeader(ref stream, out CharSpan blockName)) {

                    if (IsStateBlockHeader(blockName, out StyleState2 state)) {

                        if (!stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                            return ReportRecoverableParseError($"Expected a valid {{ }} grouping after [{blockName}].", stream.GetLineInfo());
                        }

                        ParseResult result = ParseStyleState(nodeRef, ref bodyStream, state);

                        if (result != ParseResult.Success) {
                            return result;
                        }

                        continue;
                    }

                    // if (IsEventHeader(blockName, out string eventName)) {
                    // continue;
                    // }

                    return ReportRecoverableParseError($"Encountered unexpected block name '{blockName}'. Supported values are 'active', 'focus', 'hover', 'normal'", stream.GetLineInfo());

                }

                //if (stream.TryMatchRangeIgnoreCase("mixin")) {
                //  ApplyMixin(ref stream);
                //     continue;
                // }

                if (!stream.TryParseIdentifier(out CharSpan identifier)) {
                    return ReportRecoverableParseError($"Expected a style property identifier in {currentScopeName}.", stream.GetLineInfo());
                }

                if (IsPropertyIdentifier(identifier, out PropertyId propertyId, out string loweredName)) {

                    if (!stream.TryParseCharacter('=')) {
                        return ReportRecoverableParseError($"Expected an '=' after property identifier '{identifier}'", stream.GetLineInfo());
                    }

                    if (!stream.TryGetCharSpanTo(';', out CharSpan valueSpan)) {
                        return ReportRecoverableParseError($"Unable to read property value encountering identifier '{identifier}'", stream.GetLineInfo());
                    }

                    nodeRef.AddPropertyNode(new PropertyParseInfo() {
                        identifierSpan = identifier,
                        propertyId = propertyId,
                        propertyName = loweredName,
                        valueSpan = valueSpan
                    });
                    continue;

                }

                return ReportRecoverableParseError("Unexpected token in {currentScopeName}.", stream.GetLineInfo());
            }

            return ParseResult.Success;
        }

        private static bool IsPropertyIdentifier(in CharSpan identifier, out PropertyId propertyId, out string propertyName) {
            GetLoweredSpan(identifier, ref s_CharBuffer);
            propertyId = default;

            fixed (char* ptr = s_CharBuffer.array) {

                CharSpan loweredPropertyName = new CharSpan(ptr, 0, s_CharBuffer.size, identifier.baseOffset);

                if (PropertyParsers.TryResolvePropertyId(loweredPropertyName, out PropertyParseEntry entry)) {
                    propertyName = entry.loweredName;
                    propertyId = entry.propertyId;
                    return true;
                }

                if (PropertyParsers.TryResolveShorthand(loweredPropertyName, out ShorthandEntry shorthand)) {
                    propertyName = shorthand.loweredName;
                    return true;
                }

            }

            propertyName = null;
            return false;
        }
        
        private static bool IsStateBlockHeader(CharSpan blockName, out StyleState2 styleState) {
            if (blockName.EqualsIgnoreCase("active")) {
                styleState = StyleState2.Active;
                return true;
            }

            if (blockName.EqualsIgnoreCase("focus")) {
                styleState = StyleState2.Focused;
                return true;
            }

            if (blockName.EqualsIgnoreCase("hover")) {
                styleState = StyleState2.Hover;
                return true;
            }

            if (blockName.EqualsIgnoreCase("normal")) {
                styleState = StyleState2.Normal;
                return true;
            }

            styleState = default;
            return false;
        }

        private static bool TryReadBlockHeader(ref CharStream stream, out CharSpan blockName) {
            if (stream.TryGetSubStream('[', ']', out CharStream blockStream)) {
                blockName = new CharSpan(blockStream);
                blockName.Trim();
                return true;
            }

            blockName = default;
            return false;
        }
        

        private ParseResult ParseStyleState(NodeRef nodeRef, ref CharStream stream, StyleState2 state) {

            if ((currentParseMode & ParseMode.Style) == 0) {
                return ReportRecoverableParseError($"You cannot declare [{state}] blocks outside of a style body.", stream.GetLineInfo());
            }

            if (currentStyleState == state && currentStyleState != StyleStateIndex.Normal) {
                return ReportRecoverableParseError($"You cannot nest [{state}] blocks.", stream.GetLineInfo());
            }

            currentParseMode |= ParseMode.StyleState;

            ParseResult retn = ParseStyleBody(nodeRef.AddStyleStateNode(state), ref stream);

            currentParseMode &= ~ParseMode.StyleState;

            return retn;
        }

        private ParseResult ReportCriticalParseError(string message, LineInfo lineInfo = default) {
            hitCriticalFailure = true;
            diagnostics.LogError(message, filePath, lineInfo.line, lineInfo.column);
            return ParseResult.CriticalFailure;
        }

        private ParseResult ReportRecoverableParseError(string message, LineInfo lineInfo) {
            diagnostics.LogError(message,filePath, lineInfo.line, lineInfo.column);
            return ParseResult.RecoverableFailure;
        }

        private static void GetLoweredSpan(CharSpan span, ref StructList<char> buffer) {
            buffer = buffer ?? new StructList<char>(64 > span.Length ? 64 : span.Length * 2);
            buffer.size = 0;
            buffer.EnsureCapacity(span.Length);
            for (uint j = span.rangeStart; j < span.rangeEnd; j++) {
                buffer.array[buffer.size++] = char.ToLower(span.data[j]);
            }
        }

    }

}