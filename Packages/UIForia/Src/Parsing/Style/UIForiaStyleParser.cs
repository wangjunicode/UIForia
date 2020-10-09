using System;
using UIForia.Style;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public class UIForiaStyleParser {

        private bool hasCriticalError;
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
                LogError(ErrorMessages.ExpectedStyleName(), stream.GetLineInfo());
                return false;
            }

            if (!stream.TryGetSubStream('{', '}', out CharStream styleContents)) {
                LogError(ErrorMessages.UnmatchedBraces("style " + styleName), stream.GetLineInfo());
                return false;
            }

            StyleFileShellBuilder.StyleASTBuilder styleNode = builder.AddStyleNode(styleName);

            return TryParseStyleContents(styleNode, ref styleContents);
        }

        private void ParsePropertyBlock(ref CharStream stream) { }

        private bool allowStateBlock;
        private bool allowAttribute;
        private bool allowCondition;
        private bool allowSelectors;
        
        private bool TryParseStyleContents(StyleFileShellBuilder.StyleASTBuilder styleNode, ref CharStream styleContents) {
            // variable block
            // state header
            // attribute header
            // condition
            // selector
            // when

            StructStack<int> stack = StructStack<int>.Get();

            while (!hasCriticalError && styleContents.HasMoreTokens) {
                if (styleContents.TryGetSubStream('[', ']', out CharStream blockContents)) {
                    // parse block and set type 

                    if (blockContents.TryParseIdentifier(out CharSpan span)) {
                        if (IsStateBlockHeader(span, out StyleState2 state)) {
                            if (!allowStateBlock) {
                                // diagnostics.LogWarning();
                            }
                            switch (state) {
                                case StyleState2.Normal:
                                    styleTarget = styleNode.normal;
                                    break;
                                case StyleState2.Hover:
                                    break;
                                case StyleState2.Focused:
                                    break;
                                case StyleState2.Active:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            ParseStyleBody(ref );
                            
                        }
                        else { }
                    }

                    if (blockContents.TryMatchRange("normal")) { }
                    else if (blockContents.TryMatchRange("hover")) { }
                    else if (blockContents.TryMatchRange("focus")) { }
                    else if (blockContents.TryMatchRange("active")) { }

                    else if (blockContents.TryMatchRange("attr") && blockContents.TryParseCharacter(':')) { }
                }

                // look for keywords
                // fall back to style names

                if (styleContents.TryParseCharacter('#')) { }

                if (styleContents.TryMatchRange("when")) { }

                if (styleContents.TryMatchRangeIgnoreCase("selector")) { }

                if (styleContents.TryParseIdentifier(out CharSpan propertyName)) {
                    
                    if (styleContents.TryParseCharacter(':')) { }

                    if (styleContents.TryParseCharacter('=')) {
                        
                        styleContents.TryGetCharSpanTo(';', out CharSpan propertyValue);

                        propertyListStack.Peek().AddProperty(new PropertyNode() {
                            keyRange = default,
                        });

                    }
                    
                    LogError("invalid symbol");
                    
                }
                
            }

            return true;
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

        private void LogError(string error) {
            if (diagnostics != null) {
                diagnostics.LogError(error);
            }
            else {
                Debug.Log(error);
            }

            hasCriticalError = true;
        }

        private void LogError(string error, LineInfo lineInfo) {
            if (diagnostics != null) {
                diagnostics.LogError(error, filePath, lineInfo.line, lineInfo.column);
            }
            else {
                Debug.Log(error);
            }

            hasCriticalError = true;
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