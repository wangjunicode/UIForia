using System;
using UIForia.Exceptions;
using UIForia.Style;
using UIForia.Util;

namespace UIForia.Style2 {

    public struct StyleSheetParser {

        private readonly Module module;
        private readonly StyleSheet2 sheet;

        [ThreadStatic] private static StructList<ParsedStyle> s_StyleList;
        [ThreadStatic] private static ChunkedStructList<StyleBodyPart> s_Parts;

        public StyleSheetParser(Module module, StyleSheet2 sheet) {
            this.module = module;
            this.sheet = sheet;
        }

        public static StyleSheet2 ParseString(Module module, string contents) {
            char[] rawContents = contents.ToCharArray();

            s_Parts = s_Parts ?? new ChunkedStructList<StyleBodyPart>(128);
            s_StyleList = s_StyleList ?? new StructList<ParsedStyle>(64);

            s_Parts.Clear();
            s_StyleList.Clear();
            
            StyleSheet2 sheet = new StyleSheet2(module, "STRING", rawContents);

            StyleSheetParser parser = new StyleSheetParser(module, sheet);

            parser.Parse(rawContents);

            sheet.SetParts(s_Parts.ToArray());
            sheet.SetStyles(s_StyleList.ToArray());
            
            return sheet;
        }

        private void Parse(char[] contents) {
            CharStream stream = new CharStream(contents);

            try {
                while (stream.HasMoreTokens) {
                    stream.ConsumeWhiteSpace();

                    if (stream.TryMatchRange("style")) {
                        ParseStyle(ref stream);
                    }
                    else if (stream.TryMatchRange("export")) {
                        throw new NotImplementedException();
                    }
                    else if (stream.TryMatchRange("import")) {
                        throw new NotImplementedException();
                    }
                    else if (stream.TryMatchRange("const")) {
                        ParseConstant(ref stream);
                    }
                    else if (stream.TryMatchRange("animation")) {
                        throw new NotImplementedException();
                    }
                    else if (stream.TryMatchRange("spritesheet")) {
                        throw new NotImplementedException();
                    }
                    else if (stream.TryMatchRange("gradient")) {
                        throw new NotImplementedException();
                    }
                    else if (stream.TryMatchRange("sound")) {
                        throw new NotImplementedException();
                    }
                    else {
                        throw new ParseException("Unexpected end of style sheet");
                    }
                }
            }
            catch (ParseException exception) {
                exception.SetFileName(sheet.filePath);
                throw;
            }
        }

        private bool TryGetConditionId(CharSpan conditionSpan, out int id) {
            id = module.GetDisplayConditionId(conditionSpan);
            return id >= 0;
        }

        private struct ConstantBranch {

            public int rangeEnd;
            public int rangeStart;
            public int conditionId;

        }

        private unsafe void ParseConstant(ref CharStream stream) {
            if (!stream.TryParseIdentifier(out CharSpan identifier)) {
                throw new ParseException($"Expected to find an identifier after the 'const' keyword on line {stream.GetLineNumber()}");
            }

            PendingConstant constant = new PendingConstant {
                name = identifier
            };

            int branchCount = 0;

            const int k_MaxBranchCount = 32;
            // expect we do this in parallel, so to not introduce locking on StructList pools, I instead stack alloc some placeholders
            ConstantBranch* p = stackalloc ConstantBranch[k_MaxBranchCount];

            if (stream.TryParseCharacter('=')) {
                if (!stream.TryGetCharSpanTo(';', '\n', out constant.defaultValue)) {
                    throw new ParseException($"Expected constant {constant.name} to be terminated by a semi colon on line {stream.GetLineNumber()}");
                }
            }
            else if (stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                // expect #conditions and 1 defaultValue
                while (bodyStream.HasMoreTokens) {
                    bodyStream.ConsumeWhiteSpace();

                    if (!bodyStream.HasMoreTokens) {
                        break;
                    }

                    if (bodyStream.TryParseCharacter('#')) {
                        // read until '=' but stop if encountered delimiter
                        if (!bodyStream.TryGetDelimitedSubstream('=', out CharStream conditionStream, ';', '\n')) {
                            throw new ParseException($"Expected an '=' sign after the 'default' keyword while parsing style constant '{constant.name}' on line {bodyStream.GetLineNumber()}");
                        }

                        CharSpan conditionSpan = new CharSpan(conditionStream).Trim();

                        if (!bodyStream.TryGetDelimitedSubstream(';', out CharStream valueStream, ';', '\n')) {
                            throw new ParseException($"Expected a valid value definition after the '=' when parsing style constant '{constant.name}' on line {bodyStream.GetLineNumber()}");
                        }

                        CharSpan valueSpan = new CharSpan(valueStream).Trim();

                        bodyStream.ConsumeWhiteSpace();

                        if (!TryGetConditionId(conditionSpan, out int conditionId)) {
                            throw new ParseException($"Unable to find a style condition with the name '{conditionStream}'. Please be sure you registered it in your template settings");
                        }

                        if (branchCount + 1 >= k_MaxBranchCount) {
                            throw new ParseException($"Too many conditions used in constant with the name '{conditionStream}'. Please use a maximum of {k_MaxBranchCount} conditions");
                        }

                        p[branchCount++] = new ConstantBranch() {rangeStart = valueSpan.rangeStart, rangeEnd = valueSpan.rangeEnd, conditionId = conditionId};
                    }
                    else if (bodyStream.TryMatchRangeIgnoreCase("default")) {
                        if (!bodyStream.TryParseCharacter('=')) {
                            throw ExpectedEqualAfterDefault(constant, bodyStream);
                        }

                        if (!bodyStream.TryGetCharSpanTo(';', '\n', out constant.defaultValue)) {
                            throw RequireConstantDefaultValue(constant, bodyStream);
                        }
                    }
                    else {
                        throw InvalidConstBlock(stream, constant);
                    }
                }

                if (!constant.defaultValue.HasValue) {
                    throw new ParseException($"Style constant '{constant.name}' needs a default value on line {bodyStream.GetStartLineNumber()}");
                }
            }

            if (branchCount > 0) {
                constant.conditions = new PendingConstantBranch[branchCount];
                for (int i = 0; i < branchCount; i++) {
                    constant.conditions[i] = new PendingConstantBranch(p[i].conditionId, new CharSpan(stream.Data, p[i].rangeStart, p[i].rangeEnd));
                }
            }

            sheet.AddConstant(constant);
        }

        private static ParseException ExpectedEqualAfterDefault(PendingConstant constant, CharStream bodyStream) {
            return new ParseException($"Expected an '=' sign after the 'default' keyword while parsing style constant '{constant.name}' on line {bodyStream.GetLineNumber()}");
        }

        private static ParseException RequireConstantDefaultValue(PendingConstant constant, CharStream bodyStream) {
            return new ParseException($"Expected default value for constant {constant.name} to be terminated by a semicolon on line {bodyStream.GetLineNumber()}");
        }

        private static ParseException InvalidConstBlock(CharStream stream, PendingConstant constant) {
            return new ParseException($"Invalid const block for '{constant.name}'. Unexpected input on line {stream.GetLineNumber()}");
        }

        private static ref ParsedStyle AddStyle(CharSpan styleName) {
            for (int i = 0; i < s_StyleList.size; i++) {
                if (s_StyleList[i].name == styleName) {
                    throw new ParseException($"Style with name '{styleName}' was already defined on line {s_StyleList[i].name.GetLineNumber()}");
                }
            }

            s_StyleList.Add(new ParsedStyle(styleName, s_Parts.size));

            return ref s_StyleList.array[s_StyleList.size - 1];
        }

        private static void ParseStyleExtension(in CharSpan styleName, ref CharStream stream) {
            if (stream.TryParseCharacter('@')) {
                throw new NotImplementedException();
            }
            else if (stream.TryParseIdentifier(out CharSpan name)) {
                s_Parts.Add(new StyleBodyPart(BodyPartType.ExtendBaseStyle, name.ToRefless()));
            }
            else {
                throw new ParseException($"Invalid style extension for style {styleName} on line {stream.GetLineNumber()}");
            }
        }

        private void ParseStyle(ref CharStream stream) {
            if (!stream.TryParseIdentifier(out CharSpan styleName)) {
                throw new ParseException($"Expected to find an identifier after 'style' token on line {stream.GetLineNumber()}");
            }

            ref ParsedStyle style = ref AddStyle(styleName);

            // extending will build using another sheet's context. so constants & imports are used. local vars are fine
            // when validating we can resolve style id for external things
            // when extending something that uses an inline selector we need to copy that into our selector list locally

            if (stream.TryParseCharacter(':')) {
                ParseStyleExtension(styleName, ref stream);
            }

            if (stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                ParseStyleBody(styleName, ref bodyStream);

                style.partEnd = s_Parts.size;
                s_Parts.Add(new StyleBodyPart(BodyPartType.Style, style.partStart, s_Parts.size));
            }
            else {
                throw new ParseException($"Invalid style block for style {styleName} on line {stream.GetLineNumber()}");
            }
        }

        private void ParseStyleBody(CharSpan name, ref CharStream stream) {
            stream.ConsumeWhiteSpace();

            while (stream.HasMoreTokens) {
                stream.ConsumeWhiteSpace();
                
                if (!stream.HasMoreTokens) {
                    break;
                }
                
                if (stream.TryParseCharacter('[')) {
                    throw new NotImplementedException();
                }

                if (!stream.TryParseIdentifier(out CharSpan span)) {
                    throw new NotImplementedException();
                }
                
                string idName = span.ToLowerString();

                if (idName == "run") {
                    throw new NotImplementedException();
                }
                else if (idName == "selector") {
                    throw new NotImplementedException();
                }
                else if (idName == "when") {
                    throw new NotImplementedException();
                }
                else if (PropertyParsers.TryResolvePropertyId(idName, out PropertyParsers.PropertyParseEntry entry)) {
                    if (!stream.TryParseCharacter('=')) {
                        throw new ParseException($"Expected an equal sign after property name {span} on line {stream.GetLineNumber()}");
                    }

                    if (!stream.TryGetSubstreamTo(';', '\n', out CharStream propertyStream)) {
                        throw new ParseException($"Expected a property value and then a semi colon after '{span} =' on line {stream.GetLineNumber()}");
                    }

                    if (propertyStream.Contains('@') || propertyStream.Contains('$')) {
                        s_Parts.Add(new StyleBodyPart(BodyPartType.VariableProperty, entry.propertyId, new ReflessCharSpan(propertyStream)));
                        continue;
                    }

                    if (!entry.parser.TryParse(propertyStream, entry.propertyId, out StyleProperty2 property)) {
                        throw new ParseException($"Failed to parse style property {idName} in style {name} on line {propertyStream.GetLineNumber()}");
                    }

                    s_Parts.Add(new StyleBodyPart(BodyPartType.Property, property));
                    continue;
                }
                else if (PropertyParsers.TryResolvePropertyGroupId(idName)) {
                    throw new NotImplementedException();
                }

                throw new NotImplementedException();
            }
        }

    }

}