using System;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Style;
using UIForia.Style2;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    [Serializable]
    public class CachedStyleSheet {

        public string[] dependencies;
        public DateTime lastWriteTime;
        public StringTableEntry[] stringTableEntries;
        public ConditionRequirement[] conditionRequirements;

        public ConditionTableEntry[] conditionTableEntries;
        // public PendingStyleProperty[] pendingStyleProperties;

    }

    [Serializable]
    public struct StringTableEntry {

        public int propertyIndex;
        public string value;

    }

    [Serializable]
    public struct ConditionRequirement {

        // via mixin / import could refer to a module that I'm not loading directly
        // in which case its best to use the location I think.
        public string moduleLocation;
        public string conditionNameInThatModule;

    }

    [Serializable]
    public struct ConditionTableEntry {

        public int propertyIndex;
        public int conditionRequirement;

    }

    [Serializable]
    public struct PropertyDefinition {

        public PropertyId propertyId;
        public ushort conditionIndex; // when adding to database this will be resolved
        public ushort conditionDepth;
        public long value;

    }

    public struct StyleParserState {

        public string filePath;
        public Module module;
        public StructList<Import> imports;
        public StructList<StyleDependency> dependencies;
        // public StructList<PendingStyleDefinition> styleDefinitions;
        public StructList<PackageDefinition> packageDefinitions;
        public StructList<MixinDefinition> mixinDefinitions;
        public StructList<PropertyDefinition> propertyDefinitions;

    }

    public struct MixinDefinition {

        public RangeInt propertyRange;
        public RangeInt selectorRange;

    }

    public struct PackageDefinition {

        public RangeInt styleRange;
        public RangeInt propertyRange;
        public RangeInt mixinRange;

    }

    public struct StyleDependency {

        public string moduleName;
        public string packageName;
        public string filepath;
        public bool isLoaded;
        public bool isValid;

    }

    public struct Import {

        public string moduleName;
        public string packageName;
        public string filepath;
        public string aliasName;

    }

    public unsafe class StyleSheetLoader {

        public Diagnostics diagnostics;
        public string filePath;
        public Module module;

        public StructStack<StyleParserState> parserStates;

        public bool TryParseFile(StyleFile styleFile, Diagnostics diagnostics) {

            filePath = styleFile.filePath;
            fixed (char* charptr = styleFile.contents) {
                CharStream stream = new CharStream(charptr, 0, (uint) styleFile.contents.Length);
                TryParseFileShell(ref stream);
            }

            return true;

        }

        // import module:package;
        // import module:package as alias;
        // import "path/to/file" as alias;

        // const constName = value;
        // const constName = {
        //    #wide-screen = value;
        //    #mobile = @alias.value;
        //    #value = module:package.value;
        //    default = @otherConst;
        // }


        // for updates -- if i know where the edit starts in characters I can just re-parse when applying, reparsing should be fast enough
        // do i dont need to know all the comment locations, i just need to know node locations
        // if theres a comment after a node Color = red; // here
        // then that might get nuked? or no probaby not since I can just get the range of the value and append
        // since this is edit time stuff i dont care about string perf here 

        // edit time options
        // add property + value
        // change value
        // maybe add style/group/ animation
        // i think this means line col info is enough for us and I can continue to ignore comments
        // updates will just do an insert into the string
        // should be ok even for largish files, commit on save or when no keystrokes after a while
        // will need to allow incomplete / invalid stuff probably 

        // original thought stands
        // 1st pass -- parse out the shell -> build all the nodes
        // 2nd pass -- resolve dependencies, convert property nodes to style ranges, apply mixins, other work no possible without dependencies
        // 2 level cache, parse cache + expand cache -- depends on if parsing ends up being slower than reading from cache file or not

       

        private bool hitCriticalFailure;
        private StyleParserState state;

        private void TryParseFileShell(ref CharStream stream) {
            while (stream.HasMoreTokens && !hitCriticalFailure) {

                stream.ConsumeWhiteSpaceAndComments();

                if (stream.TryMatchRange("style")) {
                    ParseStyleShell(ref stream);
                }
                else if (stream.TryMatchRange("mixin")) {
                    // ParseMixinShell(ref stream);
                }
                // else if (stream.TryMatchRange("import")) {
                //     if (!TryParseImport(ref stream)) {
                //         return;
                //     }
                // }
                else if (stream.TryMatchRange("const")) {
                    if (!TryParseConstant(ref stream)) {
                        return;
                    }
                }
                else {
                    ReportCriticalParseError("Unexpected end of style sheet");
                    return;
                }
            }
        }

        // private ParseResult ParseMixinShell(ref CharStream stream) {
        //     if (!stream.TryParseIdentifier(out CharSpan mixinName)) {
        //         return ReportCriticalParseError("Expected to find an identifier after 'mixin' token.", stream.GetLineNumber());
        //     }
        //
        //     if (!ValidateStyleName(mixinName)) {
        //         return ReportRecoverableParseError($"Duplicate mixin name: '{mixinName}'", mixinName.GetLineNumber());
        //     }
        //     
        //     // if (stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
        //     //     // store this and come back later on, need to sort out any dependencies first
        //     //     state.mixinDefinitions.Add(new PendingMixinDefinition() {
        //     //         name = mixinName.ToString(),
        //     //         body = new ReflessCharSpan(bodyStream)
        //     //     });
        //     //     
        //     //     return ParseResult.Success;
        //     // }
        //
        //     return ReportCriticalParseError($"Invalid style block for style {styleName}", stream.GetLineNumber(), stream.colNumber);
        // }

        private ParseResult ParseStyleShell(ref CharStream stream) {

            if (!stream.TryParseIdentifier(out CharSpan styleName)) {
                return ReportCriticalParseError("Expected to find an identifier after 'style' token.", stream.GetLineNumber());
            }

            // later validation step?
            if (!ValidateStyleName(styleName)) {
                return ReportRecoverableParseError($"Duplicate style name: '{styleName}'", styleName.GetLineNumber());
            }

            // if (stream.TryParseCharacter(':')) {
            //     ParseStyleExtension(ref stream);
            // }

            if (stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                // store this and come back later on, need to sort out any dependencies first
                return default;// ParseStyleBody(ref bodyStream);
            }

            return ReportCriticalParseError($"Invalid style block for style {styleName}", stream.GetLineNumber(), stream.colNumber);

        }

        // private ParseResult ParseStyleBody(ref CharStream stream) {
        //
        //     while (stream.HasMoreTokens) {
        //         stream.ConsumeWhiteSpaceAndComments();
        //
        //         if (!stream.HasMoreTokens) {
        //             break;
        //         }
        //
        //         if (TryParseConditionBlock(ref stream)) {
        //             continue;
        //         }
        //
        //         if (TryParseBracedBlock(ref stream)) {
        //             continue;
        //         }
        //
        //         // run probably requires an [enter] or [exit] now
        //         // same with play, pause, whatever
        //
        //         if (stream.TryMatchRangeIgnoreCase("mixin")) {
        //             // ApplyMixin(ref stream);
        //             continue;
        //         }
        //
        //         if (!stream.TryParseIdentifier(out CharSpan identifier)) {
        //             throw new ParseException($"Unexpected token in {currentScopeName} on line {stream.GetLineNumber()}");
        //         }
        //
        //         if (TryParsePropertyDeclaration(identifier, ref stream)) {
        //             continue;
        //         }
        //
        //         throw new ParseException($"Unexpected token in {currentScopeName} on line {stream.GetLineNumber()}.");
        //     }
        // }

        // private bool TryParseConditionBlock(ref CharStream stream) {
        //     if (!stream.TryParseCharacter('#')) {
        //         return false;
        //     }
        //
        //     if (!stream.TryParseIdentifier(out CharSpan conditionName)) {
        //         throw new ParseException($"Expected a valid condition identifier after '#' on line {stream.GetLineNumber()}");
        //     }
        //
        //     conditionName = conditionName.Trim();
        //
        //     int conditionId = module.GetDisplayConditionId(conditionName);
        //
        //     // todo -- consider a 'loose' condition mode that just returns false if condition not present
        //     if (conditionId == -1) {
        //         throw new ParseException($"Unknown condition {conditionName} referenced on line {stream.GetLineNumber()}. Make sure you have registered all constants in your module definition.");
        //     }
        //
        //     if ((currentParseMode & (ParseMode.Selector | ParseMode.Style | ParseMode.Mixin)) == 0) {
        //         throw new ParseException($"Conditional blocks are not valid outside selectors, styles, or mixins. See line {stream.GetLineNumber()}.");
        //     }
        //
        //     if (!stream.TryGetSubStream('{', '}', out CharStream conditionBody)) {
        //         throw new ParseException($"Invalid condition block for condition {conditionName} referenced on line {stream.GetLineNumber()}. Make sure you wrap the body in matching braces.");
        //     }
        //
        //     int start = s_Parts.size;
        //
        //     ParseStyleBody(ref conditionBody);
        //
        //     s_Parts.Add(new Part_ConditionBlock(conditionName, conditionId, start));
        //
        //     return true;
        // }
        
        private bool ValidateStyleName(CharSpan styleName) {

            // for (int i = 0; i < state.styleDefinitions.size; i++) {
            //     if (state.styleDefinitions.array[i].name == styleName) {
            //         return false;
            //     }
            // }

            return true;
        }

        private StructList<ConditionalConstant> conditionalConstants;
        private StructList<ConditionalConstantValue> conditionalConstValues;

        public struct ConditionalConstant {

            public ReflessCharSpan name;
            public ReflessCharSpan defaultValue;
            public RangeInt valueRange;

        }

        public struct ConditionalConstantValue {

            public ReflessCharSpan valueSpan;
            public int conditionId;

            public bool isDefault {
                get => conditionId < 0;
            }

        }

        /*
         * const x {
         *     #mobile = @thing.color;
         *     default = green;
         * }
         */
        private bool TryParseConstant(ref CharStream stream) {
            int valueStart = conditionalConstValues.size;

            if (ParseConstant(ref stream) == ParseResult.CriticalFailure) {
                conditionalConstValues.size = valueStart;
                return false;
            }

            return true;
        }

        private ParseResult ParseConstant(ref CharStream stream) {
            if (!stream.TryParseIdentifier(out CharSpan identifier)) {
                return ReportCriticalParseError("Expected to find an identifier after the 'const' keyword", stream.GetLineNumber());
            }

            ConditionalConstant constant = new ConditionalConstant {
                name = identifier.ToRefless(),
                valueRange = new RangeInt(conditionalConstValues.size, 0)
            };

            if (stream.TryParseCharacter('=')) {

                if (stream.TryGetCharSpanTo(';', '\n', out CharSpan valueSpan)) {
                    constant.defaultValue = valueSpan.ToRefless();
                    conditionalConstants.Add(constant);
                    return ParseResult.Success;
                }

                return ReportCriticalParseError($"Expected constant {constant.name} to be terminated by a semi colon.", stream.GetLineNumber());
            }

            if (stream.TryGetSubStream('{', '}', out CharStream bodyStream)) {
                // expect #conditions and 1 defaultValue
                while (bodyStream.HasMoreTokens) {
                    bodyStream.ConsumeWhiteSpaceAndComments();

                    if (!bodyStream.HasMoreTokens) {
                        break;
                    }

                    if (bodyStream.TryParseCharacter('#')) {
                        // read until '=' but stop if encountered delimiter
                        if (!bodyStream.TryGetDelimitedSubstream('=', out CharStream conditionStream, ';', '\n')) {
                            return ReportCriticalParseError($"Expected an '=' sign after the 'default' keyword while parsing style constant '{constant.name}'", bodyStream.GetLineNumber());
                        }

                        CharSpan conditionSpan = new CharSpan(conditionStream).Trim();

                        if (!bodyStream.TryGetDelimitedSubstream(';', out CharStream valueStream, ';', '\n')) {
                            return ReportCriticalParseError($"Expected a valid value definition after the '=' when parsing style constant '{constant.name}'.", bodyStream.GetLineNumber());
                        }

                        CharSpan valueSpan = new CharSpan(valueStream).Trim();

                        bodyStream.ConsumeWhiteSpaceAndComments();

                        if (!TryGetConditionId(conditionSpan, out int conditionId)) {
                            return ReportRecoverableParseError($"Unable to find a style condition with the name '{conditionStream}'. Please be sure you registered it in your template settings", bodyStream.GetLineNumber());
                        }

                        conditionalConstValues.Add(new ConditionalConstantValue() {
                            conditionId = conditionId,
                            valueSpan = valueSpan.ToRefless()
                        });
                        constant.valueRange.length++;
                    }
                    else if (bodyStream.TryMatchRangeIgnoreCase("default")) {

                        if (constant.defaultValue.HasValue) {
                            return ReportRecoverableParseError($"Style constant '{constant.name}' declares multiple default values.", bodyStream.GetLineNumber());
                        }

                        if (!bodyStream.TryParseCharacter('=')) {
                            return ReportCriticalParseError($"Expected an '=' sign after the 'default' keyword while parsing style constant '{constant.name}'", bodyStream.GetLineNumber());
                        }

                        if (!bodyStream.TryGetCharSpanTo(';', '\n', out CharSpan span)) {
                            return ReportCriticalParseError($"Expected default value for constant {constant.name} to be terminated by a semicolon.", bodyStream.GetLineNumber());
                        }

                        constant.defaultValue = span.ToRefless();

                    }
                    else {
                        return ReportCriticalParseError($"Invalid const block for '{constant.name}'. Unexpected input encountered.", stream.GetLineNumber());
                    }
                }

                if (!constant.defaultValue.HasValue) {
                    return ReportRecoverableParseError($"Style constant '{constant.name}' needs a default value.", bodyStream.GetLineNumber());
                }

            }

            conditionalConstants.Add(constant);

            return ParseResult.Success;
        }

        private bool TryGetConditionId(CharSpan conditionSpan, out int id) {
            id = module.GetDisplayConditionId(conditionSpan);
            return id >= 0;
        }

        private ParseResult ReportCriticalParseError(string message, int lineNumber = -1, int columnNumber = -1) {
            hitCriticalFailure = true;
            diagnostics.LogParseError(filePath, message, lineNumber, columnNumber);
            return ParseResult.CriticalFailure;
        }

        private ParseResult ReportRecoverableParseError(string message, int lineNumber = -1, int columnNumber = -1) {
            diagnostics.LogParseError(filePath, message, lineNumber, columnNumber);
            return ParseResult.RecoverableFailure;
        }

    }

}