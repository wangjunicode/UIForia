using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.ListTypes;
using UIForia.Parsing;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;

namespace UIForia.Compilers {

    internal class UIForiaStyleCompiler : IDisposable {

        // todo -- if this was threadsafe we could compile styles multi-threaded
        // problem then is that we include too many styles?
        // maybe we just want a way to do a dry run when compiling templates to gather
        // all the templates that will be used. also useful for knowing type hierarchies for injection tools or stats

        private Diagnostics diagnostics;

        private StyleDatabase styleDatabase;
        private StructList<StyleFileDesc> registeredFiles;
        private StructList<CompiledProperty> shorthandPropertyBuffer;

        private Dictionary<string, StyleFileShell> fileMap;
        private LightStack<StyleCompilationContext> compilationStack;
        private List_Char tmpCharBuffer;

        private UIForiaStyleParser parser;

        private StructList<CharStream> slotBuffer;
        
        private ObjectPool<CompiledStyleBlock> blockPool = new ObjectPool<CompiledStyleBlock>(null, (a) => {
            a.firstChild = null;
            a.nextSibling = null;
            a.propertyStart = 0;
            a.parseBlockId = default;
            a.transitionStart = 0;
            a.animationActionCount = default;
            a.animationActionStart = default;
        });

        public UIForiaStyleCompiler(StyleDatabase styleDatabase, Diagnostics diagnostics) {
            this.styleDatabase = styleDatabase;
            this.diagnostics = diagnostics;
            this.fileMap = new Dictionary<string, StyleFileShell>();
            this.compilationStack = new LightStack<StyleCompilationContext>();
            this.tmpCharBuffer = new List_Char(1024, Allocator.Persistent);
            this.registeredFiles = new StructList<StyleFileDesc>();
            this.shorthandPropertyBuffer = new StructList<CompiledProperty>();
            this.slotBuffer = new StructList<CharStream>();
        }

        ~UIForiaStyleCompiler() {
            tmpCharBuffer.Dispose();
        }

        public void Dispose() {
            tmpCharBuffer.Dispose();
        }

        internal void Register(StyleFileDesc styleFileDesc) {

            for (int i = 0; i < registeredFiles.size; i++) {
                if (registeredFiles.array[i].filePath == styleFileDesc.filePath) {
                    registeredFiles.array[i] = styleFileDesc;
                    return;
                }
            }

            registeredFiles.Add(styleFileDesc);
        }

        private bool TryResolveStyleFile(string filePath, out StyleFileShell retn) {

            if (fileMap.TryGetValue(filePath, out retn)) {
                return !retn.hasParseErrors;
            }

            return false;
        }

        public bool Compile(StyleFileDesc fileDesc) {

            // todo -- figure out where the parse cache lives or if it is in passed in?

            parser ??= new UIForiaStyleParser(diagnostics);

            if (!parser.TryParse(fileDesc.filePath, fileDesc.contents, out StyleFileShell shell)) {
                return false;
            }

            return Compile(shell);

        }

        public bool Compile(StyleFileShell shell) {

            if (!shell.isValid) return false;

            if (fileMap.TryGetValue(shell.filePath, out StyleFileShell _)) {
                return true;
            }

            for (int i = 0; i < compilationStack.size; i++) {
                if (compilationStack.array[i].shell.filePath == shell.filePath) {
                    List<string> strList = new List<string>();
                    for (int j = 0; j < compilationStack.size; j++) {
                        strList.Add(compilationStack.array[j].shell.filePath);
                    }

                    string output = StringUtil.ListToString(strList, "\n -> ");
                    diagnostics.LogError($"Failed to compile {shell.filePath} because of a recursive import. {output}");
                    return false;
                }

            }

            CustomPropertyDefinition[] customProperties = shell.customProperties;

            if (customProperties.Length != 0) {

                PropertyParseContext parseContext = new PropertyParseContext() {
                    diagnostics = diagnostics, // maybe dont expose this directly but expose via method api
                    fileName = shell.filePath,
                    variableTagger = styleDatabase.variableTagger,
                };

                for (int i = 0; i < customProperties.Length; i++) {
                    unsafe {
                        CustomPropertyDefinition property = customProperties[i];

                        string propertyName = shell.GetString(property.name);
                        string defaultValue = shell.GetString(property.defaultValue);

                        fixed (char* defaultValuePtr = defaultValue) {
                            parseContext.charStream = new CharStream(defaultValuePtr, 0, defaultValue.Length);

                            if (property.enumTypeName.length != 0) {
                                string enumTypeName = shell.GetString(property.enumTypeName);
                                TypeParser.TryParseTypeName(enumTypeName, out TypeLookup lookup);
                                Type type = TypeResolver.ResolveType(lookup);

                                if (EnumParser.TryParse(ref parseContext, type, out int value, out EnumParser enumParser)) {
                                    if (!styleDatabase.TryRegisterCustomProperty_Enum(shell.module.moduleName, propertyName, value, enumParser)) {
                                        throw new Exception();
                                    }
                                }
                                else {
                                    throw new Exception();
                                }

                            }
                            else if (!styleDatabase.TryRegisterCustomProperty(shell.module.moduleName, propertyName, property.type, ref parseContext)) {
                                throw new Exception();
                            }

                        }
                    }

                }
            }

            compilationStack.Push(new StyleCompilationContext() {
                shell = shell
            });

            // make sure referenced sheets are compiled first, also ensure we don't have recursive imports

            // imports and implicit references come first
            StructList<StyleSheetReference> toResolveList = StructList<StyleSheetReference>.Get();

            // for (int i = 0; i < shell.module.implicitStyleSheetReferences.Length; i++) {
            //     ctx.referenceList.Add(shell.module.implicitStyleSheetReferences[i]);
            // }

            bool importFailed = false;

            shell.resolvedImports = shell.imports == null || shell.imports.Length == 0
                ? ArrayPool<ResolvedImport>.Get(0)
                : new ResolvedImport[shell.imports.Length];

            for (int i = 0; i < shell.resolvedImports.Length; i++) {

                ImportNode import = shell.imports[i];

                string filePath = shell.GetString(import.filePath);
                string alias = shell.GetString(import.alias);

                StyleLocation location = new StyleLocation(filePath);

                StyleFileShell importedShell = ResolveImport(location);

                if (importedShell == null) {
                    importFailed = true;
                    diagnostics.LogError($"Import failed from expression `import \"{filePath}\" as {alias}`. Unable to resolve style at {location.filePath}.", shell.filePath, import.lineInfo.line, import.lineInfo.column);
                    continue;
                }

                if (!Compile(importedShell)) {
                    importFailed = true;
                    diagnostics.LogError($"Import failed from expression `import \"{filePath}\" as {alias}`. Failed to compile style at {location.filePath}.", shell.filePath, import.lineInfo.line, import.lineInfo.column);
                    continue;
                }

                shell.resolvedImports[i] = new ResolvedImport() {
                    alias = import.alias,
                    shell = importedShell
                };

            }

            // warn about conflicting references

            if (importFailed) {
                return false;
            }

            StyleCompilationContext ctx = new StyleCompilationContext() {
                shell = shell,
                compiledStyles = StructList<CompiledStyle>.Get(),
                propertyBuffer = StructList<CompiledProperty>.Get(),
                transitionBuffer = StructList<CompiledTransition>.Get(),
                animationActionBuffer = StructList<AnimationActionData>.Get(),
                compiledAnimations = StructList<CompiledAnimation>.Get(),
                propertyValueBuffer = new ManagedByteBuffer(new byte[TypedUnsafe.Kilobytes(1)]) // todo maybe pool these
            };

            ctx.propertyBuffer.size = 1; // 0 is invalid
            ctx.transitionBuffer.size = 1; // 0 is invalid
            ctx.animationActionBuffer.size = 1; // 0 is invalid

            CompileStyleFile(ref ctx);

            compilationStack.Pop();

            styleDatabase.AddStylesFromCompilation(ctx);

            LightStack<CompiledStyleBlock> stack = LightStack<CompiledStyleBlock>.Get();
            
            for (int i = 0; i < ctx.compiledStyles.size; i++) {
                
                CompiledStyle style = ctx.compiledStyles.array[i];
                stack.Push(style.rootBlock);
                
                while (stack.size != 0) {
                    CompiledStyleBlock element = stack.Pop();
                    CompiledStyleBlock ptr = element.firstChild;

                    while (ptr != null) {
                        stack.Push(ptr);
                        ptr = ptr.nextSibling;
                    }
                    
                    blockPool.Release(element);
                }

            }
            LightStack<CompiledStyleBlock>.Release(ref stack);

            ctx.Release();

            toResolveList.Release();

            fileMap[shell.filePath] = shell;

            return true;
        }

        private StyleFileShell ResolveImport(StyleLocation location) {

            if (fileMap.TryGetValue(location.filePath, out StyleFileShell shell)) {
                return shell;
            }

            // todo -- sort if needed & binary search or use dictionary 
            for (int i = 0; i < registeredFiles.size; i++) {

                ref StyleFileDesc styleFileDesc = ref registeredFiles.array[i];

                if (styleFileDesc.filePath == location.filePath) {

                    // todo -- handle registered but had parse issues...probably just fail the compilation?
                    if (styleFileDesc.styleFile != null && styleFileDesc.styleFile.isValid) {
                        return styleFileDesc.styleFile;
                    }

                    // todo -- parse or get from parse cache
                    if (parser.TryParse(styleFileDesc.filePath, styleFileDesc.contents, out shell)) {
                        // add to parse cache if present
                        return shell;
                    }

                }

            }

            return null;
        }

        private void CompileStyleFile(ref StyleCompilationContext ctx) {
            StyleFileShell shell = ctx.shell;

            for (int animationNodeIndex = 0; animationNodeIndex < shell.animations.Length; animationNodeIndex++) {

                StyleContext styleContext = new StyleContext() {
                    shell = ctx.shell,
                    propertyBuffer = ctx.propertyBuffer,
                    transitionList = ctx.transitionBuffer,
                    animationActionBuffer = ctx.animationActionBuffer,
                    propertyValueBuffer = ctx.propertyValueBuffer,
                };

                ctx.compiledAnimations.Add(CompileAnimation(ref styleContext, shell.animations[animationNodeIndex]));

                // todo this pattern sucks

                ctx.propertyValueBuffer = styleContext.propertyValueBuffer; // dont lose data from struct manipulation 
            }

            for (int i = 0; i < shell.styles.Length; i++) {
                CompileStyleNode(ref ctx, shell.styles[i]);
            }
        }

        private void CompileStyleNode(ref StyleCompilationContext ctx, StyleNode styleNode) {

            StyleContext styleContext = new StyleContext() {
                shell = ctx.shell,
                //blockBuffer = ctx.blockBuffer,
                propertyBuffer = ctx.propertyBuffer,
                transitionList = ctx.transitionBuffer,
                animationActionBuffer = ctx.animationActionBuffer,
                propertyValueBuffer = ctx.propertyValueBuffer,
            };

            string styleName = ctx.shell.GetString(styleNode.nameRange);
            ctx.compiledStyles.Add(new CompiledStyle() {
                styleName = styleName,
                rootBlock = CompileStyleBlock(ref styleContext, styleNode.rootBlockIndex, styleName)
            });

            ctx.propertyValueBuffer = styleContext.propertyValueBuffer; // dont lose data from struct manipulation 

        }


        private CompiledStyleBlock CompileStyleBlock(ref StyleContext styleContext, ParseBlockId blockIdx, string styleName) {

            ref ParseBlockNode parseBlockNode = ref styleContext.shell.blocks[blockIdx.id];

            // need to track where the last property was added to know where this block starts
            int propertyStart = styleContext.propertyBuffer.size;
            int transitionStart = styleContext.transitionList.size;
            int actionStart = styleContext.animationActionBuffer.size;

            int lastPropertyIdx = -1;
            int lastTransitionIdx = -1;

            PropertyResult result = default;

            CompiledStyleBlock compiledStyleBlock = blockPool.Get();

            BuildProperties(ref result, ref styleContext, compiledStyleBlock, parseBlockNode, ref lastPropertyIdx, ref lastTransitionIdx);

            // if we didnt have any transitions or properties added, set the start values to invalid (0)
            compiledStyleBlock.propertyStart = result.hasProperties ? propertyStart : 0;
            compiledStyleBlock.transitionStart = result.hasTransitions ? transitionStart : 0;
            compiledStyleBlock.animationActionStart = actionStart;
            compiledStyleBlock.animationActionCount = (styleContext.animationActionBuffer.size - actionStart);
            compiledStyleBlock.parseBlockId = blockIdx;

            // once all properties have been parsed we can move from individual lists to the main buffer. This allows us to support mixin expansion 

            ParseBlockId ptr = parseBlockNode.firstChild;

            if (ptr.id != 0) {

                compiledStyleBlock.firstChild = CompileStyleBlock(ref styleContext, ptr, styleName);

                CompiledStyleBlock last = compiledStyleBlock.firstChild;
                ptr = styleContext.shell.blocks[ptr.id].nextSibling;

                while (ptr.id != 0) {
                
                    last.nextSibling = CompileStyleBlock(ref styleContext, ptr, styleName);
                    last = last.nextSibling;
                    
                    ptr = styleContext.shell.blocks[ptr.id].nextSibling;
                
                }
            }

            return compiledStyleBlock;
        }

        private struct PropertyResult {

            public bool hasProperties;
            public bool hasTransitions;

        }

        private unsafe void BuildProperties(ref PropertyResult result, ref StyleContext styleContext, CompiledStyleBlock parentBlockId, in ParseBlockNode parseBlockNode, ref int lastAddedIdx, ref int lastTransitionIdx) {

            PropertyParseContext parseContext = new PropertyParseContext() {
                diagnostics = diagnostics, // maybe dont expose this directly but expose via method api
                fileName = styleContext.shell.filePath,
                variableTagger = styleDatabase.variableTagger,
            };

            int declPtr = parseBlockNode.lastDeclaration.id;

            while (declPtr != 0) {

                ref StyleDeclaration decl = ref styleContext.shell.propertyDefinitions[declPtr];

                switch (decl.declType) {

                    // todo -- a lot of these can be pre-parsed if they do not contain variables or const references from remote files
                    case StyleDeclarationType.Property: {

                        parseContext.propertyId = decl.declarationData.propertyDeclarationData.propertyId;
                        PropertyKeyInfoFlag flags = decl.declarationData.propertyDeclarationData.flags;
                        int propertyId = decl.declarationData.propertyDeclarationData.propertyId;
                        RangeInt valueRange = decl.declarationData.propertyDeclarationData.valueRange;

                        parseContext.rawValueSpan = new CharSpan(styleContext.shell.charBuffer, valueRange.start, valueRange.end);
                        parseContext.charStream = new CharStream(parseContext.rawValueSpan);

                        if ((flags & PropertyKeyInfoFlag.ContainsMixinVariable) != 0) {
                            ReplaceMixinVariableUsages(ref tmpCharBuffer, ref styleContext, ref parseContext.rawValueSpan);
                            parseContext.charStream = new CharStream(tmpCharBuffer.array, 0, tmpCharBuffer.size);
                        }

                        if ((flags & PropertyKeyInfoFlag.ContainsConst) != 0) {
                            ReplaceConstUsages(ref tmpCharBuffer, ref styleContext, ref parseContext.rawValueSpan);
                            parseContext.charStream = new CharStream(tmpCharBuffer.array, 0, tmpCharBuffer.size);
                        }

                        // todo -- if contains variables then we'll need to store an unparsed version.
                        if ((flags & PropertyKeyInfoFlag.ContainsVariable) != 0) {

                            if (parseContext.charStream.TryMatchRange("var")) {

                                if (!parseContext.charStream.TryGetParenStream(out CharStream parenStream)) {
                                    throw new MissingDiagnosticException("Variable usage must follow the pattern `var($variableName, defaultValue)`");
                                    break;
                                }

                                if (!parenStream.TryGetCharSpanTo(',', out CharSpan variableNameSpan)) {
                                    throw new MissingDiagnosticException("Variable usage must follow the pattern `var($variableName, defaultValue)`");
                                    break;
                                }

                                variableNameSpan = variableNameSpan.Trim();

                                if (!variableNameSpan.StartsWith('$')) {
                                    throw new MissingDiagnosticException("Variable names must begin with a `$`");
                                }

                                // todo -- maybe restrict variables to 32k or 16k and use some other bits to encode explicit inherit 

                                int variableId = styleDatabase.variableTagger.TagString(variableNameSpan.data, new RangeInt(variableNameSpan.rangeStart, variableNameSpan.Length));
                                ushort variableNameId = (ushort) variableId; // todo -- ensure we don't overflow this 
                                parseContext.charStream = parenStream;

                                if ((flags & PropertyKeyInfoFlag.AnimationCurrent) != 0) {
                                    // todo -- rangeInt isn't a great way to handle this
                                    // right now we're just setting the range length to 0 if we should be treating this as animation, thats pretty shitty
                                    AddProperty(ref result, styleContext.propertyBuffer, ref lastAddedIdx, new CompiledProperty(propertyId, new RangeInt(0, 0)));
                                }
                                else if (PropertyParsers.s_parseEntries[propertyId].parser.TryParseFromStyleSheet(ref parseContext, ref styleContext.propertyValueBuffer, out RangeInt propertyRange)) {
                                    AddProperty(ref result, styleContext.propertyBuffer, ref lastAddedIdx, new CompiledProperty(propertyId, propertyRange, variableNameId));
                                }
                                else {
                                    string propertyName = PropertyParsers.s_PropertyNames[propertyId];
                                    diagnostics.LogError($"Failed to parse {propertyName} from {parseContext.charStream.ToString()}");
                                }
                            }

                        }

                        else if ((flags & PropertyKeyInfoFlag.AnimationCurrent) != 0) {
                            // todo -- rangeInt isn't a great way to handle this
                            // right now we're just setting the range length to 0 if we should be treating this as animation, thats pretty shitty
                            AddProperty(ref result, styleContext.propertyBuffer, ref lastAddedIdx, new CompiledProperty(propertyId, new RangeInt(0, 0)));
                        }

                        else if (PropertyParsers.s_parseEntries[propertyId].parser.TryParseFromStyleSheet(ref parseContext, ref styleContext.propertyValueBuffer, out RangeInt propertyRange)) {
                            AddProperty(ref result, styleContext.propertyBuffer, ref lastAddedIdx, new CompiledProperty(propertyId, propertyRange));
                        }
                        else {
                            string propertyName = PropertyParsers.s_PropertyNames[propertyId];
                            diagnostics.LogError($"Failed to parse {propertyName} from {parseContext.charStream.ToString()}");
                        }

                        break;
                    }

                    case StyleDeclarationType.ShortHand: {

                        RangeInt valueRange = decl.declarationData.propertyDeclarationData.valueRange;

                        parseContext.rawValueSpan = new CharSpan(styleContext.shell.charBuffer, valueRange.start, valueRange.end);
                        parseContext.charStream = new CharStream(parseContext.rawValueSpan).Trim();

                        PropertyKeyInfoFlag flags = decl.declarationData.propertyDeclarationData.flags;

                        slotBuffer.size = 0;
                        shorthandPropertyBuffer.size = 0;

                        while (parseContext.charStream.HasMoreTokens) {
                            CharStream slotStream = parseContext.charStream.GetNextTraversedStream(' ', WhitespaceHandling.None);
                            parseContext.charStream.ConsumeWhiteSpace();

                            if ((flags & PropertyKeyInfoFlag.ContainsMixinVariable) != 0) {
                                CharSpan rawValueSpan = new CharSpan(slotStream);
                                ReplaceMixinVariableUsages(ref tmpCharBuffer, ref styleContext, ref rawValueSpan);
                                slotStream = new CharStream(tmpCharBuffer.array, 0, tmpCharBuffer.size);
                            }

                            if ((flags & PropertyKeyInfoFlag.ContainsConst) != 0) {
                                CharSpan rawValueSpan = new CharSpan(slotStream);
                                ReplaceConstUsages(ref tmpCharBuffer, ref styleContext, ref rawValueSpan);
                                slotStream = new CharStream(tmpCharBuffer.array, 0, tmpCharBuffer.size);
                            }

                            slotBuffer.Add(slotStream);
                        }

                        IStyleShorthandParser shorthandParser = PropertyParsers.s_ShorthandEntries[decl.declarationData.propertyDeclarationData.propertyId].parser;
                        shorthandParser.TryParseFromStyleSheet(ref parseContext, slotBuffer, ref styleContext.propertyValueBuffer, shorthandPropertyBuffer);

                        for (int bufferIndex = 0; bufferIndex < shorthandPropertyBuffer.size; bufferIndex++) {
                            AddProperty(ref result, styleContext.propertyBuffer, ref lastAddedIdx, shorthandPropertyBuffer[bufferIndex]);
                        }

                        break;
                    }

                    case StyleDeclarationType.MaterialVar:
                        break;

                    case StyleDeclarationType.PainterVar:
                        break;

                    case StyleDeclarationType.MixinProperty:
                        break;

                    case StyleDeclarationType.MixinUsage: {

                        ApplyMixinUsage(ref result, ref styleContext, parentBlockId, ref lastAddedIdx, ref lastTransitionIdx, decl);

                        break;
                    }

                    case StyleDeclarationType.Transition: {
                        TransitionDeclarationData transitionData = decl.declarationData.transitionDeclarationData;

                        if (transitionData.propertyId == -1) {

                            string rawPropertyKey = styleContext.shell.GetString(transitionData.customPropertyRange);
                            string moduleName;
                            string propertyName;

                            if (rawPropertyKey.StartsWith("::")) {
                                moduleName = styleContext.shell.module.moduleName;
                                propertyName = rawPropertyKey.Substring(2);
                            }
                            else {
                                string[] split = rawPropertyKey.Split(new char[] {':', ':'}, StringSplitOptions.RemoveEmptyEntries);
                                moduleName = split[0];
                                propertyName = split[1];
                            }

                            if (!styleDatabase.TryGetCustomPropertyId(moduleName, propertyName, out PropertyId propertyId)) {
                                throw new Exception($"Custom property not found `{moduleName}::{propertyName}`");
                            }

                            TransitionDeclaration transition = styleContext.shell.transitions[transitionData.transitionId];

                            AddTransition(ref result, styleContext, ref lastTransitionIdx, new CompiledTransition() {
                                propertyId = propertyId,
                                definition = new TransitionDefinition() {
                                    delay = transition.delay,
                                    duration = transition.duration,
                                    bezier = transition.bezier,
                                    easing = transition.easing
                                },
                            });
                        }

                        else {
                            // todo -- if the id is a shorthand -> resolve all the properties in the shorthand and add 1 transition for each one

                            TransitionDeclaration transition = styleContext.shell.transitions[transitionData.transitionId];

                            AddTransition(ref result, styleContext, ref lastTransitionIdx, new CompiledTransition() {
                                propertyId = transitionData.propertyId,
                                definition = new TransitionDefinition() {
                                    delay = transition.delay,
                                    duration = transition.duration,
                                    bezier = transition.bezier,
                                    easing = transition.easing
                                },
                            });
                        }

                        break;
                    }

                    case StyleDeclarationType.Action: {
                        // todo -- action might have various types, but assuming its an animation for the moment
                        // todo -- resolve the module and get the animation from it 
                        styleContext.animationActionBuffer.Add(decl.declarationData.animationActionData);
                        break;
                    }

                    case StyleDeclarationType.CustomProperty: {

                        PropertyKeyInfoFlag flags = decl.declarationData.propertyDeclarationData.flags;
                        string rawPropertyKey = styleContext.shell.GetString(decl.declarationData.propertyDeclarationData.keyRange);
                        string moduleName;
                        string propertyName;

                        if (rawPropertyKey.StartsWith("::")) {
                            moduleName = styleContext.shell.module.moduleName;
                            propertyName = rawPropertyKey.Substring(2);
                        }
                        else {
                            string[] split = rawPropertyKey.Split(new char[] {':', ':'}, StringSplitOptions.RemoveEmptyEntries);
                            moduleName = split[0];
                            propertyName = split[1];
                        }

                        if (!styleDatabase.TryGetCustomPropertyId(moduleName, propertyName, out PropertyId propertyId)) {
                            throw new Exception($"Custom property not found `{moduleName}::{propertyName}`");
                        }

                        RangeInt valueRange = decl.declarationData.propertyDeclarationData.valueRange;

                        parseContext.rawValueSpan = new CharSpan(styleContext.shell.charBuffer, valueRange.start, valueRange.end);
                        parseContext.charStream = new CharStream(parseContext.rawValueSpan);

                        if ((flags & PropertyKeyInfoFlag.ContainsMixinVariable) != 0) {
                            ReplaceMixinVariableUsages(ref tmpCharBuffer, ref styleContext, ref parseContext.rawValueSpan);
                            parseContext.charStream = new CharStream(tmpCharBuffer.array, 0, tmpCharBuffer.size);
                        }

                        if ((flags & PropertyKeyInfoFlag.ContainsConst) != 0) {
                            ReplaceConstUsages(ref tmpCharBuffer, ref styleContext, ref parseContext.rawValueSpan);
                            parseContext.charStream = new CharStream(tmpCharBuffer.array, 0, tmpCharBuffer.size);
                        }

                        // todo -- if contains variables then we'll need to store an unparsed version.
                        if ((flags & PropertyKeyInfoFlag.ContainsVariable) != 0) {

                            if (parseContext.charStream.TryMatchRange("var")) {

                                if (!parseContext.charStream.TryGetParenStream(out CharStream parenStream)) {
                                    throw new MissingDiagnosticException("Variable usage must follow the pattern `var($variableName, defaultValue)`");
                                    break;
                                }

                                if (!parenStream.TryGetCharSpanTo(',', out CharSpan variableNameSpan)) {
                                    throw new MissingDiagnosticException("Variable usage must follow the pattern `var($variableName, defaultValue)`");
                                    break;
                                }

                                variableNameSpan = variableNameSpan.Trim();

                                if (!variableNameSpan.StartsWith('$')) {
                                    throw new MissingDiagnosticException("Variable names must begin with a `$`");
                                }

                                // todo -- maybe restrict variables to 32k or 16k and use some other bits to encode explicit inherit 

                                int variableId = styleDatabase.variableTagger.TagString(variableNameSpan.data, new RangeInt(variableNameSpan.rangeStart, variableNameSpan.Length));
                                ushort variableNameId = (ushort) variableId; // todo -- ensure we don't overflow this 
                                parseContext.charStream = parenStream;

                                if ((flags & PropertyKeyInfoFlag.AnimationCurrent) != 0) {
                                    // todo -- rangeInt isn't a great way to handle this
                                    // right now we're just setting the range length to 0 if we should be treating this as animation, thats pretty shitty
                                    AddProperty(ref result, styleContext.propertyBuffer, ref lastAddedIdx, new CompiledProperty(propertyId, new RangeInt(0, 0)));
                                }
                                else if (PropertyParsers.s_parseEntries[propertyId].parser.TryParseFromStyleSheet(ref parseContext, ref styleContext.propertyValueBuffer, out RangeInt propertyRange)) {
                                    AddProperty(ref result, styleContext.propertyBuffer, ref lastAddedIdx, new CompiledProperty(propertyId, propertyRange, variableNameId));
                                }
                                else {
                                    diagnostics.LogError($"Failed to parse {propertyName} from {parseContext.charStream.ToString()}");
                                }
                            }

                        }

                        else if ((flags & PropertyKeyInfoFlag.AnimationCurrent) != 0) {
                            // todo -- rangeInt isn't a great way to handle this
                            // right now we're just setting the range length to 0 if we should be treating this as animation, thats pretty shitty
                            AddProperty(ref result, styleContext.propertyBuffer, ref lastAddedIdx, new CompiledProperty(propertyId, new RangeInt(0, 0)));
                        }

                        else if (styleDatabase.GetCustomPropertyParser(propertyId).TryParseFromStyleSheet(ref parseContext, ref styleContext.propertyValueBuffer, out RangeInt propertyRange)) {
                            AddProperty(ref result, styleContext.propertyBuffer, ref lastAddedIdx, new CompiledProperty(propertyId, propertyRange));
                        }
                        else {
                            diagnostics.LogError($"Failed to parse {propertyName} from {parseContext.charStream.ToString()}");
                        }

                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                declPtr = decl.prevSibling.id;
            }

        }

        private CompiledAnimation CompileAnimation(ref StyleContext styleContext, AnimationNode animationNode) {

            StructList<CompiledPropertyKeyFrames> keyFrameValues = new StructList<CompiledPropertyKeyFrames>(); // todo -- make this not a struct list 

            ParseBlockId keyFramePtr = animationNode.firstKeyFrame;
            while (keyFramePtr.id != 0) {

                ParseBlockNode keyFrameBlock = styleContext.shell.blocks[keyFramePtr.id];

                // CompiledBlockId blockId = new CompiledBlockId(styleContext.blockBuffer.size); // local to a style file, will be offset later

                // need to track where the last property was added to know where this block starts
                int propertyStart = styleContext.propertyBuffer.size;

                int lastPropertyIdx = -1;
                int lastTransitionIdx = -1;

                PropertyResult result = default;
                
                BuildProperties(ref result, ref styleContext, null, keyFrameBlock, ref lastPropertyIdx, ref lastTransitionIdx);

                for (int i = propertyStart; i < styleContext.propertyBuffer.size; i++) {

                    // <time, propertyId, valueRange>
                    keyFrameValues.Add(new CompiledPropertyKeyFrames() {
                        key = styleContext.propertyBuffer[i].propertyKey,
                        valueRange = styleContext.propertyBuffer[i].valueRange,
                        time = keyFrameBlock.blockData.keyFrameTime
                    });
                }

                keyFramePtr = keyFrameBlock.nextSibling;
            }

            // assert that we don't have mixed units
            bool isPercentageBased = false;
            for (int i = 0; i < keyFrameValues.size; i++) {
                if (isPercentageBased && keyFrameValues[i].time.unit != UITimeMeasurementUnit.Percentage) {
                    diagnostics.LogError($"Failed to compile {styleContext.shell.GetString(animationNode.nameRange)} in {styleContext.shell}. " +
                                         $"Either all keyframes must be percentage based or none at all.");
                    return default;
                }

                if (keyFrameValues[i].time.unit == UITimeMeasurementUnit.Percentage) {
                    isPercentageBased = true;
                }
            }

            // group by property id, secondary sort by time
            keyFrameValues.Sort((a, b) => {
                if (a.key.index == b.key.index) {
                    // sort by time
                    if (a.time.unit == UITimeMeasurementUnit.Percentage) {
                        return a.time.value < b.time.value ? -1 : 1;
                    }

                    return a.time.AsMilliseconds < b.time.AsMilliseconds ? -1 : 1;
                }

                return a.key.index < b.key.index ? -1 : 1;
            });

            return new CompiledAnimation() {
                options = animationNode.options,
                animationName = animationNode.nameRange,
                propertyKeyFrames = keyFrameValues
            };
        }

        private void AddProperty(ref PropertyResult propertyResult, StructList<CompiledProperty> propertyList, ref int lastPropertyIdx, CompiledProperty property) {
            propertyResult.hasProperties = true;
            if (lastPropertyIdx != -1) {
                propertyList.array[lastPropertyIdx].next = propertyList.size;
            }

            property.next = 0;
            propertyList.Add(property);
            lastPropertyIdx = propertyList.size - 1;
        }

        private void AddTransition(ref PropertyResult propertyResult, in StyleContext styleContext, ref int lastTransitionIdx, CompiledTransition transition) {
            propertyResult.hasTransitions = true;
            if (lastTransitionIdx != -1) {
                styleContext.transitionList.array[lastTransitionIdx].next = styleContext.transitionList.size;
            }

            transition.next = 0;
            styleContext.transitionList.Add(transition);
            lastTransitionIdx = styleContext.transitionList.size - 1;
        }

        private unsafe void ApplyMixinUsage(ref PropertyResult result, ref StyleContext styleContext, CompiledStyleBlock parentBlock, ref int lastPropertyIdx, ref int lastTransitionIdx, StyleDeclaration decl) {

            CharSpan mixinName = styleContext.shell.GetCharSpan(decl.declarationData.mixinDeclarationData.keyRange);
            CharSpan toResolve = mixinName;
            StyleFileShell shell = ResolveDottedAlias(ref styleContext, ref toResolve);

            if (shell == null) {
                throw new NotImplementedException("Error");
            }

            if (!shell.TryGetMixin(toResolve, out MixinNode mixinNode)) {
                throw new NotImplementedException("Error");
            }

            // we exploit the stack for storing previous state
            StyleFileShell prevShell = styleContext.shell;
            MixinVariableId prevMixinStart = styleContext.mixinVarStart;
            int prevMixinCount = styleContext.mixinVarCount;
            StructList<MixinVariableUsage> prevMixinList = styleContext.mixinList;

            // setup the new state
            styleContext.shell = shell;
            styleContext.mixinVarStart = mixinNode.variableIndex;
            styleContext.mixinVarCount = mixinNode.variableCount;

            int offset = styleContext.mixinVarStart.id; // offset used to find the index of our variable range in the shellarra

            MixinVariableId overrideOffset = decl.declarationData.mixinDeclarationData.mixinVariableStart;
            int overrideCount = decl.declarationData.mixinDeclarationData.mixinVariableCount;

            // errors for unmatched mixin vars? probably a warning 
            styleContext.mixinList = StructList<MixinVariableUsage>.Get();

            // we want to process the values of override keys and values in the variables. We might need to replace the contents
            // with a new char span. The problem is that we need char* storage for this so we can build charspan's out of them.
            // this is our temporary storage buffer. 
            List_Char overrideBuffer = new List_Char(512, Allocator.TempJob);

            // once we handle replacing keys & values from the variables this is where they get stored
            StructList<MixinVariableUsage> resolvedOverrides = StructList<MixinVariableUsage>.Get();

            // because our overrideBuffer might resize while we are adding to it, we dont actually create the
            // char spans until we process all overrides. This is a temp buffer to store the ranges 
            StructList<RangeInt> rangeBuffer = StructList<RangeInt>.Get();

            // for each mixin var declared by the applied mixin, resolve its values into a MixinVariableUsage and add to the new mixin list
            for (int j = 0; j < styleContext.mixinVarCount; j++) {
                MixinVariable mixinVar = styleContext.shell.mixinVariables[offset + j];

                styleContext.mixinList.Add(new MixinVariableUsage() {
                    key = styleContext.shell.GetCharSpan(mixinVar.key),
                    value = styleContext.shell.GetCharSpan(mixinVar.value)
                });

            }

            // for each override we need to interpolate an %{whatever} values. This buffers the interpolation results
            for (int k = 0; k < overrideCount; k++) {
                ref MixinVariable overrideCandidate = ref prevShell.mixinVariables[overrideOffset.id + k];
                CharSpan key = prevShell.GetCharSpan(overrideCandidate.key);
                CharSpan value = prevShell.GetCharSpan(overrideCandidate.value);

                RangeInt keyRange = new RangeInt(overrideBuffer.size, 0);
                ReplaceMixinVariableUsages(ref tmpCharBuffer, prevMixinList, key);
                keyRange.length = tmpCharBuffer.size;
                overrideBuffer.AddRange(tmpCharBuffer.array, tmpCharBuffer.size);

                RangeInt valueRange = new RangeInt(overrideBuffer.size, 0);
                ReplaceMixinVariableUsages(ref tmpCharBuffer, prevMixinList, value);
                valueRange.length = tmpCharBuffer.size;
                overrideBuffer.AddRange(tmpCharBuffer.array, tmpCharBuffer.size);

                rangeBuffer.Add(keyRange);
                rangeBuffer.Add(valueRange);

            }

            // take the ranges we computed and create MixinVariableUsages from them
            for (int k = 0; k < overrideCount * 2; k += 2) {
                RangeInt keyRange = rangeBuffer.array[k];
                RangeInt valueRange = rangeBuffer.array[k + 1];
                resolvedOverrides.Add(new MixinVariableUsage(
                    new CharSpan(overrideBuffer.array, keyRange),
                    new CharSpan(overrideBuffer.array, valueRange)
                ));
            }

            rangeBuffer.Release();

            // match overrides to declarations
            for (int j = 0; j < styleContext.mixinList.size; j++) {

                for (int k = 0; k < overrideCount; k++) {
                    MixinVariableUsage overrideCandidate = resolvedOverrides.array[k];

                    if (styleContext.mixinList.array[j].key == overrideCandidate.key) {
                        styleContext.mixinList.array[j].value = overrideCandidate.value;
                        break;
                    }

                }

            }

            // assert all assignment slots are true
            for (int j = 0; j < styleContext.mixinList.size; j++) {
                if (styleContext.mixinList.array[j].value.Length == 0) {
                    throw new MissingDiagnosticException();
                }
            }

            CompileMixin(ref result, ref styleContext, mixinNode, parentBlock, ref lastPropertyIdx, ref lastTransitionIdx);

            styleContext.mixinList.Release();
            styleContext.shell = prevShell;
            styleContext.mixinVarStart = prevMixinStart;
            styleContext.mixinVarCount = prevMixinCount;
            styleContext.mixinList = prevMixinList;
            overrideBuffer.Dispose();
            resolvedOverrides.Release();

        }

        internal struct MixinVariableUsage {

            public CharSpan key;
            public CharSpan value;

            public MixinVariableUsage(CharSpan key, CharSpan value) {
                this.key = key;
                this.value = value;
            }

        }

        private static unsafe StyleFileShell ResolveDottedAlias(ref StyleContext styleContext, ref CharSpan value) {

            int idx = value.IndexOf('.');

            if (idx < 0) {
                return styleContext.shell;
            }

            CharSpan aliasSpan = new CharSpan(value.data, value.rangeStart, idx);

            if (styleContext.shell.TryResolveAlias(aliasSpan, out StyleFileShell alias)) {
                value = new CharSpan(value.data, idx + 1, value.rangeEnd);
                return alias;
            }

            return null;

        }

        private struct ResolvedMixinVariable {

            public CharSpan key;
            public CharSpan value;

            public ResolvedMixinVariable(CharSpan key, CharSpan value) {
                this.key = key;
                this.value = value;
            }

        }

        private static unsafe bool ReplaceMixinVariableUsages(ref List_Char buffer, StructList<MixinVariableUsage> variables, CharSpan span) {

            buffer.size = 0;
            buffer.EnsureCapacity(4 * span.Length);

            char* data = span.data;
            int start = span.rangeStart;
            int end = span.rangeEnd;

            if (variables == null || variables.size == 0) {
                buffer.AddRange(data + start, span.Length);
                return false;
            }

            bool replaced = false;

            for (int i = start; i < end; i++) {

                if (data[i] != '%' || i + 1 < end && data[i + 1] != '{') {
                    buffer.Add(data[i]);
                    continue;
                }

                CharStream stream = new CharStream(data, i + 2, end);

                // find until '}'
                int closeIdx = stream.NextIndexOf('}');

                if (closeIdx == -1) {
                    buffer.Add(data[i]);
                    continue;
                }

                stream = new CharStream(data, i + 2, closeIdx);

                if (!stream.TryParseIdentifier(out CharSpan identifier)) {
                    buffer.Add(data[i]);
                    continue;
                }

                // todo -- more whitespace? more content? probably an error but right now its ignored

                i += 2 + identifier.Length;

                bool found = false;

                for (int j = 0; j < variables.size; j++) {
                    CharSpan varName = variables.array[j].key;

                    if (varName == identifier) {
                        CharSpan varValue = variables.array[j].value;
                        found = true;
                        replaced = true;
                        buffer.AddRange(varValue.data + varValue.rangeStart, varValue.Length);
                        break;
                    }

                }

                if (!found) {
                    // error? probably!
                    throw new MissingDiagnosticException();
                }
            }

            return replaced;

        }

        private static unsafe void ReplaceMixinVariableUsages(ref List_Char buffer, ref StyleContext styleContext, ref CharSpan rawValueSpan) {
            buffer.size = 0;
            buffer.EnsureCapacity(4 * rawValueSpan.Length);

            char* data = rawValueSpan.data;
            int start = rawValueSpan.rangeStart;
            int end = rawValueSpan.rangeEnd;

            for (int i = start; i < end; i++) {

                if (data[i] != '%' || i + 1 < end && data[i + 1] != '{') {
                    buffer.Add(data[i]);
                    continue;
                }

                CharStream stream = new CharStream(data, i + 2, end);

                // find until '}'
                int closeIdx = stream.NextIndexOf('}');

                if (closeIdx == -1) {
                    buffer.Add(data[i]);
                    continue;
                }

                stream = new CharStream(data, i + 2, closeIdx);

                // is this an error? maybe not
                if (!stream.TryParseIdentifier(out CharSpan identifier)) {
                    buffer.Add(data[i]);
                    continue;
                }

                i += 2 + identifier.Length;

                bool found = false;

                for (int j = 0; j < styleContext.mixinList.size; j++) {
                    CharSpan varName = styleContext.mixinList.array[j].key;

                    if (varName == identifier) {
                        CharSpan varValue = styleContext.mixinList.array[j].value;
                        found = true;
                        buffer.AddRange(varValue.data + varValue.rangeStart, varValue.Length);
                        break;
                    }

                }

                if (!found) {
                    // error? probably!
                    throw new MissingDiagnosticException();
                }

            }

        }

        private static unsafe void ReplaceConstUsages(ref List_Char buffer, ref StyleContext styleContext, ref CharSpan rawValueSpan) {

            buffer.size = 0;
            buffer.EnsureCapacity(4 * rawValueSpan.Length);

            char* data = rawValueSpan.data;
            int start = rawValueSpan.rangeStart;
            int end = rawValueSpan.rangeEnd;

            for (int i = start; i < end; i++) {

                if (data[i] != '@') {
                    buffer.Add(data[i]);
                    continue;
                }

                CharStream stream = new CharStream(data, i + 1, end);

                if (!stream.TryParseDottedIdentifier(out CharSpan identifier, out bool wasDotted)) {
                    buffer.Add(data[i]);
                    continue;
                    // is this an error? maybe not if its a path 
                }

                int advance = identifier.Length; // store advance since we'll change the identifier variable later 

                StyleFileShell shell = styleContext.shell;

                if (wasDotted) {
                    // find first dot instance
                    // locate an import with that same name
                    // should have been compiled already since we pre-scan imports

                    int dotIdx = identifier.IndexOf('.');

                    if (styleContext.shell.TryResolveAlias(new CharSpan(identifier.data, identifier.rangeStart, dotIdx), out StyleFileShell alias)) {
                        identifier = new CharSpan(identifier.data, dotIdx + 1, identifier.rangeEnd);
                        shell = alias;
                    }
                    else {
                        // unknown alias
                        throw new MissingDiagnosticException();
                    }

                }

                if (shell.TryGetLocalConstant(identifier, out int idx)) {
                    // copy value of identifier in 
                    int valueStart = styleContext.shell.constants[idx].value.start;
                    int valueLength = styleContext.shell.constants[idx].value.length;

                    buffer.AddRange(styleContext.shell.charBuffer + valueStart, valueLength);

                }
                else {
                    throw new MissingDiagnosticException("cannot find constant with name identifier in file");
                }

                i += advance;
            }

        }

        private void CompileMixin(ref PropertyResult result, ref StyleContext styleContext, MixinNode mixinUsage, CompiledStyleBlock parentBlock, ref int lastPropertyIdx, ref int lastTransitionIdx) {

            ParseBlockNode blockNode = styleContext.shell.blocks[mixinUsage.rootBlockIndex.id];

            BuildProperties(ref result, ref styleContext, parentBlock, blockNode, ref lastPropertyIdx, ref lastTransitionIdx);

            ParseBlockId ptr = blockNode.firstChild;

            while (ptr.id != 0) {
                CompiledStyleBlock child = CompileStyleBlock(ref styleContext, ptr, default);
                AddChildBlock(parentBlock, child);
                ptr = styleContext.shell.blocks[ptr.id].nextSibling;
            }

        }

        private static void AddChildBlock(CompiledStyleBlock parent, CompiledStyleBlock child) {
            if (parent.firstChild == null) {
                parent.firstChild = child;
            }
            else {
            
                CompiledStyleBlock ptr = parent.firstChild;
            
                while (ptr.nextSibling != null) {
                    ptr = ptr.nextSibling;
                }

                ptr.nextSibling = child;
            }
        }

        private struct StyleContext {

            public StyleFileShell shell;
            public StructList<CompiledProperty> propertyBuffer;
            public StructList<CompiledTransition> transitionList;
            public StructList<AnimationActionData> animationActionBuffer;
            public ManagedByteBuffer propertyValueBuffer;

            public MixinVariableId mixinVarStart;
            public int mixinVarCount;
            public StructList<MixinVariableUsage> mixinList;

        }

    }

}