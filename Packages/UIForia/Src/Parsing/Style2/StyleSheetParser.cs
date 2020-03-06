using System;
using UIForia.Exceptions;
using UIForia.Style;
using UIForia.Util;

namespace UIForia.Style2 {

    [Flags]
    internal enum ParseMode {

        Root = 0,
        Style = 1 << 0,
        Constant = 1 << 1,
        Mixin = 1 << 2,
        Selector = 1 << 3,
        StyleState = 1 << 4

    }

    public struct StyleSheetParser {

        private readonly Module module;
        private readonly StyleSheet2 sheet;
        private int currentStyleState;
        private ParseMode currentParseMode;
        private CharSpan currentStyleName;

        [ThreadStatic] private static StructList<char> s_CharBuffer;
        [ThreadStatic] private static StructList<ParsedStyle> s_StyleList;
        [ThreadStatic] private static ChunkedStructList<StyleBodyPart> s_Parts;
        [ThreadStatic] private static StructList<PendingConstant> s_Constants;
        [ThreadStatic] private static StructList<StyleProperty2> s_PropertyBuffer;

        public StyleSheetParser(Module module, StyleSheet2 sheet) {
            this.module = module;
            this.sheet = sheet;
            this.currentStyleState = StyleStateIndex.Normal;
            this.currentParseMode = ParseMode.Root;
            this.currentStyleName = default;
        }

        public static StyleSheet2 ParseString(Module module, string contents) {
            char[] rawContents = contents.ToCharArray();

            s_Parts = s_Parts ?? new ChunkedStructList<StyleBodyPart>(128);
            s_StyleList = s_StyleList ?? new StructList<ParsedStyle>(64);
            s_Constants = s_Constants ?? new StructList<PendingConstant>(32);
            s_PropertyBuffer = s_PropertyBuffer ?? new StructList<StyleProperty2>();

            s_Parts.Clear();
            s_StyleList.QuickClear();
            s_Constants.QuickClear();
            s_PropertyBuffer.QuickClear();

            StyleSheet2 sheet = new StyleSheet2(module, "STRING", rawContents);

            StyleSheetParser parser = new StyleSheetParser(module, sheet);

            parser.Parse(rawContents);


            return sheet;
        }

        private void Parse(char[] contents) {
            if (contents.Length >= ushort.MaxValue) {
                ParseException exception = new ParseException($"File contains {contents.Length} characters which exceeds maximum character count for style files ({ushort.MaxValue})");
                exception.SetFileName(sheet.filePath);
            }

            CharStream stream = new CharStream(contents);

            try {
                while (stream.HasMoreTokens) {
                    stream.ConsumeWhiteSpaceAndComments();

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

                ValidateLocalConstants();

                sheet.SetParts(s_Parts.ToArray());
                sheet.SetStyles(s_StyleList.ToArray());
                sheet.SetConstants(s_Constants.ToArray());
                s_Parts.Clear();
                s_StyleList.QuickClear();
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

        private void ParseConstant(ref CharStream stream) {
            if (!stream.TryParseIdentifier(out CharSpan identifier)) {
                throw new ParseException($"Expected to find an identifier after the 'const' keyword on line {stream.GetLineNumber()}");
            }

            PendingConstant constant = new PendingConstant {
                name = identifier
            };

            int constantPartRangeStart = s_Parts.size;

            if (stream.TryParseCharacter('=')) {
                if (!stream.TryGetCharSpanTo(';', '\n', out constant.defaultValue)) {
                    throw new ParseException($"Expected constant {constant.name} to be terminated by a semi colon on line {stream.GetLineNumber()}");
                }
            }
            else if (stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                // expect #conditions and 1 defaultValue
                while (bodyStream.HasMoreTokens) {
                    bodyStream.ConsumeWhiteSpaceAndComments();

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

                        bodyStream.ConsumeWhiteSpaceAndComments();

                        if (!TryGetConditionId(conditionSpan, out int conditionId)) {
                            throw new ParseException($"Unable to find a style condition with the name '{conditionStream}'. Please be sure you registered it in your template settings");
                        }

                        s_Parts.Add(new Part_ConstantBranch(conditionId, valueSpan));
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

            constant.partRange = new Range16(constantPartRangeStart, s_Parts.size);
            constant.sourceId = sheet.id;
            AddLocalConstant(constant);
        }


        private void ParseStyleExtension(ref CharStream stream) {
            if (stream.TryParseCharacter('@')) {
                throw new NotImplementedException();
            }
            else if (stream.TryParseIdentifier(out CharSpan name)) {
                s_Parts.Add(new Part_ExtendStyle(name, sheet.id, 0));
            }
            else {
                throw new ParseException($"Invalid style extension for style {currentStyleName} on line {stream.GetLineNumber()}");
            }
        }

        private void ParseStyle(ref CharStream stream) {
            if (!stream.TryParseIdentifier(out CharSpan styleName)) {
                throw new ParseException($"Expected to find an identifier after 'style' token on line {stream.GetLineNumber()}");
            }

            currentParseMode |= ParseMode.Style;
            currentStyleName = styleName;

            ref ParsedStyle style = ref AddStyle(styleName);

            // extending will build using another sheet's context. so constants & imports are used. local vars are fine
            // when validating we can resolve style id for external things
            // when extending something that uses an inline selector we need to copy that into our selector list locally

            if (stream.TryParseCharacter(':')) {
                ParseStyleExtension(ref stream);
            }

            if (stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                ParseStyleBody(ref bodyStream);

                style.partEnd = s_Parts.size;
                s_Parts.Add(new Part_Style(styleName, style.partStart));
                currentParseMode &= ~ParseMode.Style;
                currentStyleName = default;
            }
            else {
                throw new ParseException($"Invalid style block for style {styleName} on line {stream.GetLineNumber()}");
            }
        }

        private unsafe void ParseStyleBody(ref CharStream stream) {
            stream.ConsumeWhiteSpaceAndComments();

            while (stream.HasMoreTokens) {
                stream.ConsumeWhiteSpaceAndComments();

                if (!stream.HasMoreTokens) {
                    break;
                }

                if (TryParseConditionBlock(ref stream)) {
                    continue;
                }

                if (TryParseBracedBlock(ref stream)) {
                    continue;
                }

                if (!stream.TryParseIdentifier(out CharSpan identifier)) {
                    throw new ParseException($"Unexpected token in {currentStyleName} on line {stream.GetLineNumber()}");
                }

                // run probably requires an [enter] or [exit] now
                // same with play, pause, whatever

                if (stream.TryMatchRangeIgnoreCase("run")) { }

                if (stream.TryMatchRangeIgnoreCase("selector")) { }

                if (stream.TryMatchRangeIgnoreCase("when")) { }

                if (TryParsePropertyDeclaration(identifier, ref stream)) {
                    continue;
                }

                throw new ParseException($"Unexpected token in {currentStyleName} on line {stream.GetLineNumber()}");
            }
        }

        private bool TryParsePropertyDeclaration(CharSpan propertyName, ref CharStream stream) {
            // be careful with this, this is referencing s_CharBuffer as data, which MIGHT change elsewhere, invalidating our identifier
            CharSpan loweredProperty = GetLoweredSpan(propertyName);

            if (PropertyParsers.TryResolvePropertyId(loweredProperty, out PropertyParseEntry entry)) {
                ParsePropertyValue(ref stream, propertyName, entry);
                return true;
            }

            // loweredProperty should still be valid here since we never went elsewhere in code before checking again
            if (PropertyParsers.TryResolveShorthand(loweredProperty, out ShorthandEntry shorthand)) {
                ParseShorthandValue(ref stream, propertyName, shorthand);
                return true;
            }

            return false;
        }

        private bool TryParseConditionBlock(ref CharStream stream) {
            if (!stream.TryParseCharacter('#')) {
                return false;
            }

            if (!stream.TryParseIdentifier(out CharSpan conditionName)) {
                throw new ParseException($"Expected a valid condition identifier after '#' on line {stream.GetLineNumber()}");
            }

            conditionName = conditionName.Trim();

            int conditionId = module.GetDisplayConditionId(conditionName);

            // todo -- consider a 'loose' condition mode that just returns false if condition not present
            if (conditionId == -1) {
                throw new ParseException($"Unknown condition {conditionName} referenced on line {stream.GetLineNumber()}. Make sure you have registered all constants in your module definition.");
            }

            if ((currentParseMode & (ParseMode.Selector | ParseMode.Style | ParseMode.Mixin)) == 0) {
                throw new ParseException($"Conditional blocks are not valid outside selectors, styles, or mixins. See line {stream.GetLineNumber()}.");
            }

            if (!stream.TryGetSubStream('{', '}', out CharStream conditionBody)) {
                throw new ParseException($"Invalid condition block for condition {conditionName} referenced on line {stream.GetLineNumber()}. Make sure you wrap the body in matching braces.");
            }

            int start = s_Parts.size;

            ParseStyleBody(ref conditionBody);

            s_Parts.Add(new Part_ConditionBlock(conditionName, conditionId, start));

            return true;
        }

        private bool TryParseBracedBlock(ref CharStream stream) {
            if (stream.TryGetSubStream('[', ']', out CharStream braceStream)) {
                ParseBracedBlock(ref stream, ref braceStream);
                return true;
            }

            return false;
        }

        private void ParsePropertyValue(ref CharStream stream, CharSpan propertyName, in PropertyParseEntry entry) {
            if (!stream.TryParseCharacter('=')) {
                throw new ParseException($"Expected an equal sign after property name {propertyName} on line {stream.GetLineNumber()}");
            }

            if (!stream.TryGetSubstreamTo(';', '\n', out CharStream propertyStream)) {
                throw new ParseException($"Expected a property value and then a semi colon after '{propertyName} =' on line {stream.GetLineNumber()}");
            }

            if (propertyStream.Contains('@') || propertyStream.Contains('$')) {
                ParseConstantIdentifiers(propertyStream);

                ParseVariableIdentifiers(propertyStream);

                s_Parts.Add(new Part_VariableProperty(entry.propertyId, new CharSpan(propertyStream)));

                return;
            }

            if (!entry.parser.TryParse(propertyStream, entry.propertyId, out StyleProperty2 property)) {
                throw new ParseException($"Failed to parse style property {propertyName} in style {currentStyleName} on line {propertyStream.GetLineNumber()}.");
            }

            s_Parts.Add(new Part_Property(property));
        }

        private void ParseShorthandValue(ref CharStream stream, CharSpan propertyName, in ShorthandEntry entry) {
            if (!stream.TryParseCharacter('=')) {
                throw new ParseException($"Expected an equal sign after shorthand name {propertyName} on line {stream.GetLineNumber()}");
            }

            if (!stream.TryGetSubstreamTo(';', '\n', out CharStream propertyStream)) {
                throw new ParseException($"Expected a property value and then a semi colon after '{propertyName} =' on line {stream.GetLineNumber()}");
            }

            if (propertyStream.Contains('@') || propertyStream.Contains('$')) {
                ParseConstantIdentifiers(propertyStream);

                ParseVariableIdentifiers(propertyStream);

                s_Parts.Add(new Part_VariablePropertyShorthand(entry.index, new CharSpan(propertyStream)));

                return;
            }

            s_PropertyBuffer = s_PropertyBuffer ?? new StructList<StyleProperty2>();
            s_PropertyBuffer.size = 0;

            if (!entry.parser.TryParse(propertyStream, s_PropertyBuffer)) {
                throw new ParseException($"Failed to parse style property {propertyName} in style {currentStyleName} on line {propertyStream.GetLineNumber()}.");
            }

            for (int i = 0; i < s_PropertyBuffer.size; i++) {
                s_Parts.Add(new Part_Property(s_PropertyBuffer.array[i]));
            }

            s_PropertyBuffer.Clear();
        }

        private bool TryParseStyleStateBlock(string stateName, int stateIndex, ref CharStream stream, ref CharStream braceStream) {
            if (!braceStream.TryMatchRangeIgnoreCase(stateName)) {
                return false;
            }

            if ((currentParseMode & ParseMode.Style) == 0) {
                throw new ParseException($"You cannot declare [{stateName}] blocks outside of a style body. See line {stream.GetLineNumber()}.");
            }

            if (currentStyleState == stateIndex && currentStyleState != StyleStateIndex.Normal) {
                throw new ParseException($"You cannot nest [{stateName}] blocks. See line {stream.GetLineNumber()}.");
            }

            currentStyleState = stateIndex;

            if (!stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                throw new ParseException($"Expected a valid {{ }} grouping after [{stateName}] on line {braceStream.GetLineNumber()}.");
            }

            ParseStyleState(ref bodyStream, stateIndex);
            return true;
        }

        private void ParseBracedBlock(ref CharStream stream, ref CharStream braceStream) {
            // could be an [enter] or [exit] or [state]
            // remove [attr] implementation in favor of selectors / when selectors

            if (TryParseStyleStateBlock("hover", StyleStateIndex.Hover, ref stream, ref braceStream)) {
                return;
            }

            if (TryParseStyleStateBlock("focus", StyleStateIndex.Focus, ref stream, ref braceStream)) {
                return;
            }

            if (TryParseStyleStateBlock("active", StyleStateIndex.Active, ref stream, ref braceStream)) {
                return;
            }

            if (TryParseStyleStateBlock("normal", StyleStateIndex.Normal, ref stream, ref braceStream)) {
                return;
            }

            if (braceStream.TryMatchRangeIgnoreCase("enter")) {
                return;
            }

            if (braceStream.TryMatchRangeIgnoreCase("exit")) {
                return;
            }

            throw new ParseException("Invalid input");
        }

        private void ParseStyleState(ref CharStream stream, int stateIndex) {
            int partRangeStart = s_Parts.size;

            currentParseMode |= ParseMode.StyleState;

            ParseStyleBody(ref stream);

            currentParseMode &= ~ParseMode.StyleState;

            s_Parts.Add(new Part_EnterState(stateIndex, partRangeStart));
        }

        private static void ParseVariableIdentifiers(CharStream propertyStream) {
            CharStream duplicate = new CharStream(propertyStream);

            while (duplicate.HasMoreTokens) {
                int idx = duplicate.NextIndexOf('$');

                if (idx == -1) break;

                duplicate.AdvanceTo(idx + 1);

                if (!duplicate.TryParseIdentifier(out CharSpan identifier)) {
                    throw new ParseException($"Expected to find a valid identifier after the '$' on line {duplicate.GetLineNumber()}");
                }

                // todo -- not sure what to do with variables yet
                throw new NotImplementedException("Style variables are not yet implemented");
            }
        }

        private void ParseConstantIdentifiers(CharStream propertyStream) {
            CharStream duplicate = new CharStream(propertyStream);

            while (duplicate.HasMoreTokens) {
                int idx = duplicate.NextIndexOf('@');

                if (idx == -1) break;

                duplicate.AdvanceTo(idx + 1);

                ParseConstantIdentifier(ref duplicate, out CharSpan identifier);

                EnsureConstant(identifier);

                duplicate.ConsumeWhiteSpaceAndComments();
            }
        }

        private static unsafe void ParseConstantIdentifier(ref CharStream stream, out CharSpan identifier) {
            if (!stream.TryParseIdentifier(out identifier)) {
                throw new ParseException($"Expected to find a valid identifier after the @ sign on line {stream.GetLineNumber()}");
            }

            if (stream.TryParseCharacter('.')) {
                if (!stream.TryParseIdentifier(out CharSpan dotIdentifier)) {
                    throw new ParseException($"Expected to find a valid identifier after constant reference on line {stream.GetLineNumber()}");
                }

                identifier = new CharSpan(identifier.data, identifier.rangeStart, dotIdentifier.rangeEnd);
            }
        }

        private static unsafe CharSpan GetLoweredSpan(CharSpan span) {
            s_CharBuffer = s_CharBuffer ?? new StructList<char>(64);
            s_CharBuffer.size = 0;
            s_CharBuffer.EnsureAdditionalCapacity(span.Length);
            for (uint j = span.rangeStart; j < span.rangeEnd; j++) {
                s_CharBuffer.array[s_CharBuffer.size++] = char.ToLower(span.data[j]);
            }

            return new CharSpan(s_CharBuffer.array, 0, s_CharBuffer.size);
        }

        private static unsafe void AppendToCharBuffer(in char* data, uint start, uint length) {
            s_CharBuffer.EnsureAdditionalCapacity((int) length);
            uint end = start + length;
            for (uint j = start; j < end; j++) {
                s_CharBuffer.array[s_CharBuffer.size++] = data[j];
            }
        }

        private void AddLocalConstant(in PendingConstant constant) {
            s_Constants = s_Constants ?? new StructList<PendingConstant>();
            for (int i = 0; i < s_Constants.size; i++) {
                if (s_Constants.array[i].name != constant.name) {
                    continue;
                }

                if (s_Constants.array[i].HasDefinition) {
                    // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                    throw new ParseException($"Cannot redefine constant {constant.name} on line {constant.name.GetLineNumber()}");
                }

                s_Constants.array[i] = constant;
                return;
            }

            s_Constants.Add(constant);
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

        internal void ValidateLocalConstants() {
            if (s_Constants == null) return;
            for (int i = 0; i < s_Constants.size; i++) {
                ref PendingConstant constant = ref s_Constants.array[i];

                constant.resolvedValue = default;

                if (!constant.HasDefinition) {
                    throw new ParseException($"Cannot find a definition for constant '{constant.name}'. Be sure you declare it before using it on line {constant.name.GetLineNumber()}.");
                }
            }
        }


        internal void EnsureConstant(CharSpan identifier) {
            if (s_Constants == null) {
                s_Constants = new StructList<PendingConstant>();
            }

            for (int i = 0; i < s_Constants.size; i++) {
                if (s_Constants.array[i].name == identifier) {
                    return;
                }
            }

            s_Constants.Add(new PendingConstant() {
                name = identifier
            });
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

    }

}