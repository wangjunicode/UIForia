using System;
using System.Collections.Generic;
using UIForia.Style;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public struct StyleSheet2 { }

    public class UIForiaStyleCompiler {

        private Dictionary<StyleFileShell, StyleSheet2> styleSheetMap;

        public bool Compile(StyleFileShell shell) {
            if (styleSheetMap.TryGetValue(shell, out StyleSheet2 retn)) {
                return true;
            }

            
            
            return true;
        }

    }

    public class UIForiaStyleParser {

        public const int Version = 1;

        private bool hasCriticalError;
        private bool hasErrors;

        private Diagnostics diagnostics;
        private string filePath;

        private StyleFileShellBuilder builder;

        public UIForiaStyleParser(Diagnostics diagnostics = null) {
            this.diagnostics = diagnostics;
            this.builder = new StyleFileShellBuilder();
        }

        public bool TryParse(string filePath, string contents, out StyleFileShell result) {
            this.filePath = filePath;
            this.hasCriticalError = false;
            this.hasErrors = false;
            this.builder.Clear();
            result = default;

            unsafe {
                fixed (char* ptr = contents) {
                    CharStream stream = new CharStream(ptr, 0, contents.Length);

                    while (!hasCriticalError && stream.HasMoreTokens) {
                        stream.ConsumeWhiteSpaceAndComments();

                        if (stream.TryMatchRange("import")) {
                            ParseImport(ref stream);
                            continue;
                        }

                        if (stream.TryMatchRange("style")) {
                            ParseStyleDefinition(ref stream);

                            continue;
                        }

                        if (stream.TryMatchRange("animation")) { }

                        if (stream.TryMatchRange("mixin")) { }

                        if (stream.TryMatchRange("material")) { }

                        if (stream.TryMatchRange("spritesheet")) { }

                        if (stream.TryMatchRange("texture")) { }

                        if (stream.TryMatchRange("sound")) { }

                        if (stream.TryMatchRange("cursor")) { }

                        if (stream.TryMatchRange("const")) { }

                        if (stream.TryMatchRange("export")) { }
                    }
                }
            }

            if (hasCriticalError) {
                return false;
            }

            return true;
        }

        private void ParseImport(ref CharStream stream) {
            throw new NotImplementedException("Import not implemented");
        }

        private bool ParseStyleDefinition(ref CharStream stream) {
            if (!stream.TryParseIdentifier(out CharSpan styleName)) {
                LogCriticalError(ErrorMessages.ExpectedStyleName(), stream.GetLineInfo());
                return false;
            }

            if (!stream.TryGetSubStream('{', '}', out CharStream styleContents)) {
                LogCriticalError(ErrorMessages.UnmatchedBraces("style " + styleName), stream.GetLineInfo());
                return false;
            }

            StyleFileShellBuilder.StyleASTBuilder styleNode = builder.AddStyleNode(styleName);

            return true; //TryParseStyleContents(styleNode, ref styleContents);
        }

        private void ParsePropertyBlock(ref CharStream stream) { }

        private bool allowStateBlock;
        private bool allowAttribute;
        private bool allowCondition;
        private bool allowSelectors;

        private StructStack<StyleFileShellBuilder.StyleASTBuilder> blockStack;

        private void ParseStyleBlock(ref CharStream stream) {

            while (!hasCriticalError && stream.HasMoreTokens) {
                uint start = stream.Ptr;
                stream.ConsumeWhiteSpaceAndComments(CommentMode.DoubleSlash);

                char c = stream.Current;
                if (c == '[' && stream.TryGetSubStream('[', ']', out CharStream headerStream)) {

                    if (IsStateBlockHeader(headerStream, out StyleState2 state)) { }

                    else if (IsAttributeBlock(headerStream, out CharSpan attrKey, out CharSpan attrValue)) {

                        if (!allowAttribute) {
                            LogNonCriticalError("attribute not allowed here", attrKey.GetLineInfo());
                        }

                        blockStack.Push(blockStack.Peek().AddAttributeBlock(attrKey, attrValue));

                        stream.ConsumeWhiteSpaceAndComments(CommentMode.DoubleSlash);

                        if (!stream.TryGetSubStream('{', '}', out CharStream attributeBlock)) {
                            LogCriticalError("");
                        }

                        ParseStyleBlock(ref stream);
                        blockStack.Pop();

                        continue;
                    }
                    else if (IsEventBlock(headerStream, out bool isEnter)) {
                        // might or might not include a { } with styles,
                        // could be decorating a run command or selector
                    }

                    // else if (IsTransitionBlock(headerStream)) { }
                    // else if (IsVariableBlock(headerStream)) { }
                }
                else if (c == '#') {
                    // ParseCondition();
                    continue;
                }
                else if (c == 's' && stream.TryMatchRangeIgnoreCase("select")) { }
                else if (c == 'w') { }

                if (stream.Ptr == start) {
                    hasCriticalError = true;
                    break;
                }
            }

        }

        private bool IsEventBlock(CharStream headerStream, out bool isEnter) {

            if (headerStream.TryMatchRangeIgnoreCase("enter")) {
                isEnter = true;
                return !headerStream.HasMoreTokens;
            }

            if (headerStream.TryMatchRangeIgnoreCase("exit")) {
                isEnter = false;
                return !headerStream.HasMoreTokens;
            }

            isEnter = default;
            return false;
        }

        private bool IsAttributeBlock(CharStream headerStream, out CharSpan attrKey, out CharSpan attrValue) {
            headerStream.SetCommentMode(CommentMode.None);

            attrKey = default;
            attrValue = default;

            if (!headerStream.TryMatchRangeIgnoreCase("attr")) {
                return false;
            }
            
            if (!headerStream.TryParseCharacter(':')) {
                LogNonCriticalError("Expected a `:` after `attr`", headerStream.GetLineInfo());
            }
            else {

                if (!headerStream.TryGetDelimitedSubstream('=', out CharStream keyStream)) {
                    LogNonCriticalError($"Expected an attribute name after `attr:` {headerStream}", headerStream.GetLineInfo());
                }
                else {
                    attrKey = new CharSpan(keyStream);
                    if (!headerStream.TryParseCharacter('=')) {
                        LogNonCriticalError($"Expected an `=` after {keyStream}", keyStream.GetLineInfo());
                    }
                    else {
                        headerStream.TryParseDoubleQuotedString(out attrValue);
                    }
                }
            }

            return true;

        }

        private bool IsStateBlockHeader(CharStream blockName, out StyleState2 styleState) {
            if (blockName.TryMatchRangeIgnoreCase("active")) {
                styleState = StyleState2.Active;
                if (blockName.HasMoreTokens) {
                    LogNonCriticalError("Expected end of state header name", blockName.GetLineInfo());
                }

                return true;
            }

            if (blockName.TryMatchRangeIgnoreCase("focus")) {
                styleState = StyleState2.Focused;
                if (blockName.HasMoreTokens) {
                    LogNonCriticalError("Expected end of state header name", blockName.GetLineInfo());
                }

                return true;
            }

            if (blockName.TryMatchRangeIgnoreCase("hover")) {
                styleState = StyleState2.Hover;
                if (blockName.HasMoreTokens) {
                    LogNonCriticalError("Expected end of state header name", blockName.GetLineInfo());
                }

                return true;
            }

            if (blockName.TryMatchRangeIgnoreCase("normal")) {
                styleState = StyleState2.Normal;
                if (blockName.HasMoreTokens) {
                    LogNonCriticalError("Expected end of state header name", blockName.GetLineInfo());
                }

                return true;
            }

            styleState = default;
            return false;
        }

        private void LogCriticalError(string error) {
            if (diagnostics != null) {
                diagnostics.LogError(error);
            }
            else {
                Debug.Log(error);
            }

            hasCriticalError = true;
        }

        private void LogCriticalError(string error, LineInfo lineInfo) {
            if (diagnostics != null) {
                diagnostics.LogError(error, filePath, lineInfo.line, lineInfo.column);
            }
            else {
                Debug.Log(error);
            }

            hasCriticalError = true;
        }

        private void LogNonCriticalError(string error, LineInfo lineInfo) {
            if (diagnostics != null) {
                diagnostics.LogError(error, filePath, lineInfo.line, lineInfo.column);
            }
            else {
                Debug.Log(error);
            }
        }

        internal static class ErrorMessages {

            public static string ExpectedStyleName() {
                return "Expected a valid style name after `style` keyword.";
            }

            public static string UnmatchedBraces(string context) {
                return "Expected a matching set of `{` and `}` after `" + context + "`";
            }

        }

    }

}