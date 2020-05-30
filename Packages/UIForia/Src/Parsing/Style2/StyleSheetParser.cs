using System;
using System.Collections.Generic;
using UIForia.Exceptions;
using UIForia.Style;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style2 {

    [Flags]
    internal enum ParseMode {

        Root = 0,
        Style = 1 << 0,
        Constant = 1 << 1,
        Mixin = 1 << 2,
        Selector = 1 << 3,
        StyleState = 1 << 4,
        SelectorQuery = 1 << 5,
        Import = 1 << 6

    }

    public struct StyleSheetParser {

        private readonly Module module;
        private readonly StyleSheet2 sheet;
        private int currentStyleState;
        private ParseMode currentParseMode;
        private CharSpan currentScopeName;

        [ThreadStatic] private static StructList<char> s_CharBuffer;
        [ThreadStatic] private static StructList<ParsedStyle> s_StyleList;
        [ThreadStatic] private static PagedStructList<StyleBodyPart> s_Parts;
        [ThreadStatic] private static StructList<PendingConstant> s_Constants;
        [ThreadStatic] private static StructList<StyleProperty2> s_PropertyBuffer;
        [ThreadStatic] private static StructList<ParsedMixin> s_MixinList;

        private StyleSheetParser(Module module, StyleSheet2 sheet) {
            this.module = module;
            this.sheet = sheet;
            this.currentStyleState = StyleStateIndex.Normal;
            this.currentParseMode = ParseMode.Root;
            this.currentScopeName = default;
        }

        public static StyleSheet2 ParseString(Module module, string contents) {
            char[] rawContents = contents.ToCharArray();

            s_Parts = s_Parts ?? new PagedStructList<StyleBodyPart>(128);
            s_StyleList = s_StyleList ?? new StructList<ParsedStyle>(64);
            s_Constants = s_Constants ?? new StructList<PendingConstant>(32);
            s_PropertyBuffer = s_PropertyBuffer ?? new StructList<StyleProperty2>();
            s_MixinList = s_MixinList ?? new StructList<ParsedMixin>(16);

            s_Parts.Clear();
            s_StyleList.QuickClear();
            s_Constants.QuickClear();
            s_PropertyBuffer.QuickClear();
            s_MixinList.QuickClear();

            StyleSheet2 sheet = new StyleSheet2(module, "STRING", rawContents);

            StyleSheetParser parser = new StyleSheetParser(module, sheet);

            parser.Parse(rawContents);

            return sheet;
        }

        private unsafe void Parse(char[] contents) {
            if (contents.Length >= ushort.MaxValue) {
                ParseException exception = new ParseException($"File contains {contents.Length} characters which exceeds maximum character count for style files ({ushort.MaxValue})");
                exception.SetFileName(sheet.filePath);
            }

            fixed (char* charptr = contents) {
                CharStream stream = new CharStream(charptr, 0, (uint) contents.Length);

                try {
                    while (stream.HasMoreTokens) {

                        stream.ConsumeWhiteSpaceAndComments();

                        if (stream.TryMatchRange("style")) {
                            ParseStyle(ref stream);
                        }
                        else if (stream.TryMatchRange("mixin")) {
                            ParseMixin(ref stream);
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
                            ParseAnimation(ref stream);
                        }
                        else if (stream.TryMatchRange("query")) {
                            ParseSelectorQuery(ref stream);
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
                    ValidateMixins();

                    sheet.SetParts(s_Parts.ToArray());
                    sheet.SetStyles(s_StyleList.ToArray());
                    sheet.SetConstants(s_Constants.ToArray());
                    sheet.SetMixins(s_MixinList.ToArray());

                    s_Parts.Clear();
                    s_MixinList.QuickClear();
                    s_StyleList.QuickClear();
                }
                catch (ParseException exception) {
                    exception.SetFileName(sheet.filePath);
                    throw;
                }
            }
        }

        private bool TryGetConditionId(CharSpan conditionSpan, out int id) {
            id = module.GetDisplayConditionId(conditionSpan);
            return id >= 0;
        }

        private void ParseSelectorQuery(ref CharStream stream) {
            // target could be any combination of
            // tag
            // attribute
            // style state
            // modifier
            // style name
            // query <Tag>.style from x x x x x;

            if (!stream.TryGetDelimitedSubstream(';', out CharStream queryStream)) {
                throw new ParseException($"Failed to parse selector query on line {stream.GetLineNumber()}.");
            }

            throw new NotImplementedException();
            // while (queryStream.HasMoreTokens) {
            //     if (queryStream.TryParseCharacter('.')) {
            //         var part = new SelectorTargetPart();
            //         part.type = 1;
            //         queryStream.TryParseIdentifier(out CharSpan styleName);
            //         part.name = styleName;
            //     }
            //
            //     if (queryStream.TryParseCharacter('[')) {
            //         var part = new SelectorTargetPart();
            //         part.type = 2;
            //         stream.TryGetSubStream('[', ']', out CharStream attrStream);
            //
            //         attrStream.TryParseIdentifier(out CharSpan attr);
            //             
            //         if (attrStream.TryMatchRange("=")) { }
            //         if (attrStream.TryMatchRange("!=")) { }
            //         if (attrStream.TryMatchRange("~=")) { }
            //         if (attrStream.TryMatchRange("^=")) { }
            //             
            //     }
            //
            //     if (queryStream.TryParseCharacter('<')) {
            //             
            //         if (!queryStream.TryGetSubStream('<', '>', out CharStream tagStream)) {
            //                 
            //         }
            //
            //         if (tagStream.Contains(':')) { }
            //
            //     }
            //
            //     if (queryStream.TryParseCharacter(':')) {
            //         if (queryStream.TryMatchRangeIgnoreCase("firstChild")) { }
            //         if (queryStream.TryMatchRangeIgnoreCase("lastChild")) { }
            //         if (queryStream.TryMatchRangeIgnoreCase("nthChild")) { }
            //         if (queryStream.TryMatchRangeIgnoreCase("evenChildren")) { }
            //         if (queryStream.TryMatchRangeIgnoreCase("oddChildren")) { }
            //         if (queryStream.TryMatchRangeIgnoreCase("onlyChild")) { }
            //         if (queryStream.TryMatchRangeIgnoreCase("childCountAtLeast")) { }
            //         if (queryStream.TryMatchRangeIgnoreCase("childCountAtMost")) { }
            //         if (queryStream.TryMatchRangeIgnoreCase("childCountBetween")) { }
            //     }
            //
            //     if (queryStream.TryParseCharacter('%')) {
            //             
            //         if (queryStream.TryMatchRangeIgnoreCase("hover")) { }
            //         if (queryStream.TryMatchRangeIgnoreCase("focus")) { }
            //         if (queryStream.TryMatchRangeIgnoreCase("active")) { }
            //         if (queryStream.TryMatchRangeIgnoreCase("focus-within")) { }
            //
            //     }
            // }
            //     
            // if (stream.TryMatchRangeIgnoreCase("without")) { }
            //
            // if (stream.TryMatchRangeIgnoreCase("from")) { }
            //
            // if (stream.TryMatchRange("=>")) { }

        }

        private void ParseAnimation(ref CharStream stream) {
            if (!stream.TryParseIdentifier(out CharSpan animationName)) {
                throw new ParseException("Expected a valid identifier after the 'animation' keyword on line " + stream.GetLineNumber());
            }

            if (!stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                throw new ParseException($"Expected a matching set of {{ }} after animation declaration '{animationName}' on line {animationName.GetLineNumber()}");
            }

            ParseAnimationBody(ref bodyStream);

        }

        private unsafe void ParseAnimationBody(ref CharStream stream) {

            byte* times = stackalloc byte[64];

            while (stream.HasMoreTokens) {

                // if (TryParseAnimationConditionBlock(ref stream)) {
                //     continue;
                // }

                // duration { #const = value; default = value; }

                // [keyframes] {
                //     0% { BackgroundColor = @codeColor; }
                //     20% { BackgroundColor = yellow; }
                //     80% { BackgroundColor = yellow; }
                //     100% { BackgroundColor = @codeColor;}
                // }
                // 0%, 100% { TransformPosition = 0; }
                // 10%, 30%, 50%, 70%, 90% { TransformPosition = -10px 0; }
                // 20%, 40%, 60%, 80% { TransformPosition = 10px 0; }

                // 20%,
                // 40%,
                // 60%,
                // 80%
                // { TransformPosition = 10px 0;
                //    play animation other({data}, data = x);
                // }

                // StructList<ParsedKeyFrame> keyframeList = new StructList<ParsedKeyFrame>();
                //
                // if (stream.TryGetSubStream('[', ']', out CharStream braceStream)) {
                //     
                //     if (braceStream.TryMatchRangeIgnoreCase("options")) { }
                //
                //     if (braceStream.TryMatchRangeIgnoreCase("keyframes")) {
                //         
                //         if (stream.TryGetSubStream('{', '}', out CharStream keyframes)) {
                //
                //             while (keyframes.HasMoreTokens) {
                //                 
                //                 keyframes.ConsumeWhiteSpaceAndComments();
                //
                //                 int timeCount = 0;
                //                 
                //                 // keyframe = new KeyFrame(byte[] times, range properties)
                //                 while (ParseKeyframeTime(ref keyframes, out times[timeCount++])) {
                //                     keyframes.TryParseCharacter(',');
                //                 }
                //
                //                 if (!keyframes.TryGetSubStream('{', '}', out CharStream frameBody)) {
                //                     
                //                 }
                //                 
                //                 int keyframeBodyStart = s_Parts.size;
                //                 
                //                 ParseStyleBody(ref frameBody);
                //                 
                //                 ParsedKeyFrame frame = new ParsedKeyFrame(keyframeBodyStart, s_Parts.size);
                //
                //                 for (int i = 0; i < timeCount; i++) {
                //                     animation.AddKeyFrame(times, frame);
                //                 }
                //                 
                //             }
                //
                //         }   
                //         
                //     }
                //     
                // }

                if (stream.TryParseIdentifier(out CharSpan param)) { }

                bool ParseKeyframeTime(ref CharStream frameStream, out byte time) {

                    if (frameStream.TryParseUInt(out uint value)) {

                        frameStream.TryParseCharacter('%');

                        time = (byte) Mathf.Min(value, 100);
                        return true;
                    }

                    time = 0;
                    return false;
                }
            }
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
                throw new ParseException($"Invalid style extension for style {currentScopeName} on line {stream.GetLineNumber()}");
            }
        }

        private void ParseStyle(ref CharStream stream) {
            if (!stream.TryParseIdentifier(out CharSpan styleName)) {
                throw new ParseException($"Expected to find an identifier after 'style' token on line {stream.GetLineNumber()}");
            }

            ValidateStyleName(styleName);

            currentParseMode |= ParseMode.Style;
            currentScopeName = styleName;

            // extending will build using another sheet's context. so constants & imports are used. local vars are fine
            // when validating we can resolve style id for external things
            // when extending something that uses an inline selector we need to copy that into our selector list locally

            if (stream.TryParseCharacter(':')) {
                ParseStyleExtension(ref stream);
            }

            if (stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {

                int rangeStart = s_Parts.size;

                ParseStyleBody(ref bodyStream);

                ParsedStyle style = new ParsedStyle(styleName, rangeStart, s_Parts.size);

                s_StyleList.Add(style);
                s_Parts.Add(new Part_Style(styleName, (uint) style.rangeStart));

                currentParseMode &= ~ParseMode.Style;
                currentScopeName = default;
            }
            else {
                throw new ParseException($"Invalid style block for style {styleName} on line {stream.GetLineNumber()}");
            }
        }

        private void ParseStyleBody(ref CharStream stream) {
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

                // run probably requires an [enter] or [exit] now
                // same with play, pause, whatever

                if (stream.TryMatchRangeIgnoreCase("mixin")) {
                    ApplyMixin(ref stream);
                    continue;
                }

                if (stream.TryMatchRangeIgnoreCase("run")) { }

                if (stream.TryMatchRangeIgnoreCase("selector")) { }

                if (stream.TryMatchRangeIgnoreCase("when")) { }

                if (!stream.TryParseIdentifier(out CharSpan identifier)) {
                    throw new ParseException($"Unexpected token in {currentScopeName} on line {stream.GetLineNumber()}");
                }

                if (TryParsePropertyDeclaration(identifier, ref stream)) {
                    continue;
                }

                throw new ParseException($"Unexpected token in {currentScopeName} on line {stream.GetLineNumber()}.");
            }
        }

        private void ApplyMixin(ref CharStream stream) {
            if ((currentParseMode & (ParseMode.Root | ParseMode.Constant | ParseMode.SelectorQuery | ParseMode.Import)) != 0) {
                throw new ParseException($"Applying a mixin is not valid on line {stream.GetLineNumber()}.");
            }

            if (!stream.TryGetSubStream('(', ')', out CharStream paramStream)) {
                throw new ParseException($"Expected a matched pair of ( ) after 'mixin' keyword on line {stream.GetLineNumber()}.");
            }

            bool isVariable = paramStream.TryParseCharacter('@');

            if (!paramStream.TryParseDottedIdentifier(out CharSpan identifier)) {
                throw new ParseException($"Expected a valid identifier inside the parens on line {paramStream.GetLineNumber()}.");
            }

            if (!isVariable) {
                EnsureMixin(identifier);
            }

            s_Parts.Add(new Part_ApplyMixin(identifier, isVariable));

            stream.TryParseCharacter(';'); // step over the semi colon if present. todo -- consider error if missing and not new line
            stream.ConsumeWhiteSpaceAndComments();
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

        private bool TryParseAnimationConditionBlock(ref CharStream stream) {
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

            if (!stream.TryGetSubStream('{', '}', out CharStream conditionBody)) {
                throw new ParseException($"Invalid condition block for condition {conditionName} referenced on line {stream.GetLineNumber()}. Make sure you wrap the body in matching braces.");
            }

            int start = s_Parts.size;

            ParseAnimationBody(ref conditionBody);

            s_Parts.Add(new Part_ConditionBlock(conditionName, conditionId, start));

            return true;
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

            if (!entry.parser.TryParse(propertyStream, entry.propertyId, default, out StyleProperty2 property)) {
                throw new ParseException($"Failed to parse style property {propertyName} in style {currentScopeName} on line {propertyStream.GetLineNumber()}.");
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

            if (!entry.parser.TryParse(propertyStream, default,  s_PropertyBuffer)) {
                throw new ParseException($"Failed to parse style property {propertyName} in style {currentScopeName} on line {propertyStream.GetLineNumber()}.");
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

        private void ParseMixin(ref CharStream stream) {

            if (!stream.TryParseIdentifier(out CharSpan mixinName)) {
                throw new ParseException($"Expected a valid identifier after the 'mixin' keyword on line {stream.GetLineNumber()}");
            }

            if (!stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                throw new ParseException($"Expected a matching set of {{ }} after mixin declaration '{mixinName}' on line {mixinName.GetLineNumber()}");
            }

            currentScopeName = mixinName;
            currentParseMode |= ParseMode.Mixin;
            int start = s_Parts.size;

            ParseStyleBody(ref bodyStream);

            currentParseMode &= ~ParseMode.Mixin;

            s_MixinList.Add(new ParsedMixin(mixinName, (uint) start, (uint) s_Parts.size));

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

            return default; // new CharSpan(s_CharBuffer.array, 0, s_CharBuffer.size);
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

        private static void ValidateStyleName(CharSpan styleName) {
            for (int i = 0; i < s_StyleList.size; i++) {
                if (s_StyleList[i].name == styleName) {
                    throw new ParseException($"Style with name '{styleName}' was already defined on line {s_StyleList[i].name.GetLineNumber()}");
                }
            }
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

        internal void EnsureMixin(CharSpan identifier) {

            for (int i = 0; i < s_MixinList.size; i++) {
                if (s_MixinList.array[i].name == identifier) {
                    return;
                }
            }

            s_MixinList.Add(new ParsedMixin(identifier, 0, 0));

        }

        internal void ValidateMixins() {
            for (int i = 0; i < s_MixinList.size; i++) {
                ref ParsedMixin mixin = ref s_MixinList.array[i];

                if (mixin.rangeStart == 0 && mixin.rangeEnd == 0) {
                    // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                    throw new ParseException($"Cannot find a definition for constant '{mixin.name}'. Be sure you declare it before using it on line {mixin.name.GetLineNumber()}.");
                }
            }
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