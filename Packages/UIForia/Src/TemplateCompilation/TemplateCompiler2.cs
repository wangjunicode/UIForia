using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Style;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia.Compilers {

    internal class TemplateCompiler2 {

        private State state;

        private readonly UIForiaStyleCompiler styleCompiler;
        private readonly AttributeCompiler attributeCompiler;

        private readonly Dictionary<ProcessedType, TemplateExpressionSet> compiledTemplates;
        
        private static readonly Dictionary<Type, MethodInfo> s_CreateVariableMethodCache = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> s_ReferenceVariableMethodCache = new Dictionary<Type, MethodInfo>();

        private readonly LightList<State> statePool;
        private readonly StructStack<State> stateStack;
        private readonly AttributeCompilerContext attrCompilerContext;
        private readonly TemplateLinqCompiler typeResolver;
        private Diagnostics diagnostics;

        private ITemplateCompilationHandler compilationHandler;

        private LightList<TemplateExpressionSet> templateExpressionSets;
        private StructList<CompiledExpression> pendingCompilations;

        public TemplateCompiler2(ITemplateCompilationHandler compilationHandler) {
            this.styleCompiler = new UIForiaStyleCompiler();
            this.compilationHandler = compilationHandler;
            this.diagnostics = compilationHandler.Diagnostics;
            this.statePool = new LightList<State>();
            this.stateStack = new StructStack<State>();
            this.attrCompilerContext = new AttributeCompilerContext();
            this.attributeCompiler = new AttributeCompiler(attrCompilerContext);
            this.compiledTemplates = new Dictionary<ProcessedType, TemplateExpressionSet>();
            this.typeResolver = new TemplateLinqCompiler(attrCompilerContext);
            this.templateExpressionSets = null;
            this.pendingCompilations = null;

        }
        // private readonly AttributeCompilerContext attrCompilerContext;
        // private readonly TemplateLinqCompiler typeResolver;

        private ParameterExpression system {
            get => state.systemParam;
        }

        private TemplateCompilerContext context {
            get => state.context;
        }

        private TemplateDataBuilder templateDataBuilder {
            get => state.templateDataBuilder;
        }

        private ProcessedType rootProcessedType {
            get => state.rootProcessedType;
        }

        private TemplateASTNode rootNode {
            get => state.templateFile.templateNodes[state.templateRootAST.templateIndex];
        }

        private TemplateFileShell fileShell {
            get => state.templateFile;
        }

        private int compiledTemplateId {
            get => state.compiledTemplateId;
        }

        private struct State {

            public ParameterExpression systemParam;
            public TemplateCompilerContext context;
            public TemplateDataBuilder templateDataBuilder;

            public ProcessedType rootProcessedType;
            public LightList<string> namespaces;
            public StructList<StyleSheetReference> styleReferences;
            public LightStack<StructList<BindingVariableDesc>> variableStack;
            public TemplateASTRoot templateRootAST;
            public TemplateFileShell templateFile;
            public int compiledTemplateId;

        }

        private void PushState(ProcessedType processedType) {

            TemplateExpressionSet expressionSet = new TemplateExpressionSet() {
                processedType = processedType,
                index = templateExpressionSets.size
            };

            templateExpressionSets.Add(expressionSet);

            if (state.context == null) {
                state = GetState(expressionSet);
            }
            else {
                stateStack.Push(state);
                state = GetState(expressionSet);
            }

            for (int i = 0; i < fileShell.usings.Length; i++) {
                UsingDeclaration usingDecl = fileShell.usings[i];
                if (usingDecl.namespaceRange.length > 0) {
                    string namespaceName = fileShell.GetString(usingDecl.namespaceRange);
                    if (namespaceName != null) {
                        state.namespaces.Add(namespaceName);
                    }
                }
            }

            state.templateDataBuilder.Initialize(compilationHandler, expressionSet);

        }

        private State GetState(TemplateExpressionSet expressionSet) {
            ProcessedType processedType = expressionSet.processedType;
            TemplateFileShell shell = compilationHandler.GetTemplateForType(processedType);
            TemplateASTRoot template = shell.GetRootTemplateForType(processedType);

            if (statePool.size == 0) {
                return new State() {
                    context = new TemplateCompilerContext(diagnostics),
                    templateDataBuilder = new TemplateDataBuilder(),
                    rootProcessedType = processedType,
                    templateRootAST = template,
                    templateFile = shell,
                    namespaces = new LightList<string>(),
                    styleReferences = new StructList<StyleSheetReference>(),
                    variableStack = new LightStack<StructList<BindingVariableDesc>>(),
                    compiledTemplateId = expressionSet.index
                };
            }

            State retn = statePool.RemoveLast();
            retn.rootProcessedType = processedType;
            retn.templateRootAST = template;
            retn.templateFile = shell;
            retn.compiledTemplateId = expressionSet.index;
            retn.variableStack.Clear();
            retn.styleReferences.Clear();
            retn.namespaces.Clear();
            return retn;
        }

        private void PopState() {
            statePool.Add(state);
            state = stateStack.size == 0
                ? default
                : stateStack.Pop();
        }

        private void CompileStyleSheets() {
            if (fileShell.styles == null) {
                return;
            }

            UIModule module = compilationHandler.GetModuleForType(rootProcessedType);

            Assert.IsNotNull(module);

            // todo -- don't recompile style bodies for template re-use

            // todo -- support implicitly imported styles
            // todo -- support [ImportStyle] attributes

            for (int i = 0; i < fileShell.styles.Length; i++) {
                if (!string.IsNullOrEmpty(fileShell.GetString(fileShell.styles[i].sourceBody))) {
                    Debug.Log("Todo -- compile xml styles");
                }
                else {
                    StyleLookup lookup = new StyleLookup() {
                        elementLocation = rootProcessedType.elementPath,
                        elementType = rootProcessedType.rawType,
                        moduleLocation = module.location,
                        modulePath = module.path,
                        styleAlias = fileShell.GetString(fileShell.styles[i].alias),
                        stylePath = fileShell.GetString(fileShell.styles[i].path)
                    };

                    StyleLocation location = module.ResolveStyle(lookup);

                    // todo -- use style cache in the same way as template cache, will require new style format!
                    if (string.IsNullOrEmpty(location.filePath)) {
                        diagnostics.LogWarning("Cannot resolve style file: " + location.filePath);
                        continue;
                    }

                    StyleFileShell styleShell = compilationHandler.GetStyleShell(location);

                    styleCompiler.Compile(styleShell);
                    
                    // if (!File.Exists(location.filePath)) {
                    //     diagnostics.LogWarning($"Failed to location style file at {location.filePath}");
                    //     continue;
                    // }
                    //
                    // StyleSheet sheet = importer.ImportStyleSheetFromFile(location.filePath);
                    //
                    // state.styleReferences.Add(new StyleSheetReference() {
                    //     alias = lookup.styleAlias,
                    //     styleSheet = sheet
                    // });
                }
            }
        }

        public bool CompileApplication(ProcessedType entryType, out TemplateCompilationResult result) {

            pendingCompilations = new StructList<CompiledExpression>(128);
            templateExpressionSets = new LightList<TemplateExpressionSet>(32);

            CompileTemplate(entryType);

            if (diagnostics.HasErrors()) {
                result = default;
                return false;
            }

            result.successful = true;
            result.pendingCompilations = pendingCompilations;
            result.templateExpressionSets = templateExpressionSets;
            return true;
        }

        public TemplateExpressionSet CompileTemplate(ProcessedType processedType) {

            // might consider adding exposed slots & attributes to expression set since its basically all the compiled data about a template

            if (compiledTemplates.TryGetValue(processedType, out TemplateExpressionSet retn)) {
                return retn;
            }

            PushState(processedType);

            CompileStyleSheets();

            CompileEntryPoint(processedType); // todo -- only do this if has [EntryPoint] flag (for reducing compile time)

            retn = templateDataBuilder.Build();

            compiledTemplates[processedType] = retn;

            PopState();

            return retn;
        }

        private void CompileEntryPoint(ProcessedType processedType) {
            context.Setup<UIElement>();

            state.systemParam = context.AddParameter<TemplateSystem>("system");

            ParameterExpression elementParam = context.GetVariable(processedType.rawType, "element");

            Expression ctorExpression = processedType.GetConstructorExpression();

            if (ctorExpression == null) {
                diagnostics.LogError(processedType.rawType + "doesn't define a parameterless public constructor. This is a requirement for it to be used templates");
                return;
            }

            context.Assign(elementParam, ctorExpression);
            ref TemplateASTNode node = ref fileShell.templateNodes[fileShell.GetRootTemplateForType(processedType).templateIndex];

            int attrCount = CountRealAttributes(rootNode);

            context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeEntryPoint, elementParam, ExpressionUtil.GetIntConstant(attrCount)));

            StructList<AttrInfo> attributes = StructList<AttrInfo>.Get();

            AttributeMerger.ConvertToAttrInfo(fileShell, node.index, attributes);

            InitializeElementAttributes(node); // can go through and pre-initialize all attributes in template now that we have a flat list

            // attrCompilerContext.Init(templateRootNode, TemplateNodeType.EntryPoint, processedType, null, state.variableStack);
            // attrCompilerContext.AddContextReference(processedType, templateRootNode);

            bool needPop = CompileBindings(processedType, node);

            attributes.Release();

            context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_HydrateEntryPoint));

            context.AddStatement(elementParam);

            LambdaExpression entryPointFn = context.Build(processedType.rawType.GetTypeName());

            templateDataBuilder.SetEntryPoint(entryPointFn);

            CompileHydratePoint(rootNode, processedType);

            if (needPop) {
                state.variableStack.Pop().Release();
            }
        }

        private void CompileHydratePoint(TemplateASTNode node, ProcessedType processedType) {
            context.Setup();

            ParameterExpression systemParam = context.AddParameter<TemplateSystem>("system");

            StructList<ConstructedChildData> childrenSetup = StructList<ConstructedChildData>.Get();

            SetupElementChildren(systemParam, node, childrenSetup);

            LambdaExpression hydrateFn = context.Build(processedType.rawType.GetTypeName());

            templateDataBuilder.SetHydratePoint(hydrateFn);

            CompileChildren(childrenSetup, null);

            childrenSetup.Release();
        }

        private void CompileChildren(StructList<ConstructedChildData> childData, StructList<AttrInfo> injectedAttributes) {
            for (int i = 0; i < childData.size; i++) {
                ref ConstructedChildData data = ref childData.array[i];
                CompileNode(data, injectedAttributes);
            }
        }

        private bool GetCreateChildrenLazily(StructList<AttrInfo> attrList) {
            for (int i = attrList.size - 1; i >= 0; i--) {
                if (attrList.array[i].type == AttributeType.CreateLazy) {
                    return attrList.array[i].value == "true";
                }
            }

            return false;
        }

        private bool GetCreateDisabledAttribute(StructList<AttrInfo> attrList) {
            for (int i = 0; i < attrList.size; i++) {
                if (attrList.array[i].type == AttributeType.CreateDisabled) {
                    return true;
                }
            }

            return false;
        }

        private bool GetCreateDisabledAttribute(TemplateASTNode node) {

            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                if (fileShell.attributeList[i].type == AttributeType.CreateDisabled) {
                    return true;
                }
            }

            return false;
        }

        private string GetRequiredTypeAttribute(TemplateASTNode node, out LineInfo lineInfo) {

            lineInfo = default;
            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                if (fileShell.attributeList[i].type == AttributeType.RequireType) {
                    lineInfo = new LineInfo(fileShell.attributeList[i].line, fileShell.attributeList[i].column);
                    return fileShell.GetString(fileShell.attributeList[i].value);
                }
            }

            return null;
        }

        private bool GetGenericTypeAttribute(TemplateASTNode node, out string typeName, out LineInfo lineInfo) {

            lineInfo = default;
            typeName = null;

            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                if (fileShell.attributeList[i].type == AttributeType.GenericType) {
                    lineInfo = new LineInfo(fileShell.attributeList[i].line, fileShell.attributeList[i].column);
                    typeName = fileShell.GetString(fileShell.attributeList[i].value);
                    return true;
                }
            }

            return false;
        }

        private void SetupElementChildren_Slot(Expression systemParam, StructList<int> childrenIds, StructList<ConstructedChildData> output) {
            TemplateASTNode[] templateNodes = fileShell.templateNodes;

            Type requiredType = null; // ResolveRequiredType(parent); //fileShell.GetRequiredType(parent.index));
            for (int i = 0; i < childrenIds.size; i++) {
                int childIndex = childrenIds.array[i];
                ref TemplateASTNode child = ref templateNodes[childIndex];

                ProcessedType childProcessedType = fileShell.typeMappings[child.index];

                // todo -- fix this
                if (requiredType != null && requiredType.IsAssignableFrom(childProcessedType.rawType)) {
                    diagnostics.LogError(($"type {childProcessedType.rawType} doesnt match required type of {requiredType}"));
                    continue;
                }

                int outputIndex = templateDataBuilder.GetNextElementFunctionIndex();

                output.AddUnsafe(new ConstructedChildData(childProcessedType, child.index, outputIndex));

                Expression childNew = childProcessedType.GetConstructorExpression();

                if ((child.templateNodeType & TemplateNodeType.Slot) != 0) {
                    Assert.IsTrue(child.templateNodeType == TemplateNodeType.SlotDefine);
                    string tagName = fileShell.GetString(child.tagNameRange);
                    // todo -- option to not create if not overridden
                    // todo -- new expression can come from template system
                    context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.TemplateSystem_AddSlotChild, childNew, ExpressionUtil.GetStringConstant(tagName), ExpressionUtil.GetIntConstant(outputIndex)));
                }
                else if ((child.templateNodeType & TemplateNodeType.Meta) != 0) {
                    // todo -- 
                }
                else {
                    context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.TemplateSystem_AddChild, childNew, ExpressionUtil.GetIntConstant(outputIndex)));
                }
            }
        }

        // call appropriate AddXXXChild methods and allocate template slots for children setup
        // must be called when the parent element is COMPLETELY done setting itself up!!!
        private void SetupElementChildren(Expression systemParam, in TemplateASTNode parentNode, StructList<ConstructedChildData> output) {

            TemplateASTNode[] templateNodes = fileShell.templateNodes;

            int childIndex = parentNode.firstChildIndex;

            Type requiredType = ResolveRequiredType(parentNode, out LineInfo requiredTypeLineInfo);

            while (childIndex != -1) {
                ref TemplateASTNode child = ref templateNodes[childIndex];
                childIndex = child.nextSiblingIndex;

                ProcessedType childProcessedType = fileShell.typeMappings[child.index];

                if (childProcessedType.IsUnresolvedGeneric) {

                    childProcessedType = ResolveGenericElementType(child, childProcessedType);

                    if (childProcessedType == null || childProcessedType.IsUnresolvedGeneric) {
                        continue;
                    }

                }

                if (requiredType != null && !requiredType.IsAssignableFrom(childProcessedType.rawType)) {
                    LogError($"type {childProcessedType.rawType} doesnt match required type of {requiredType}", new LineInfo(child.lineNumber, child.columnNumber));
                    continue;
                }

                int outputIndex = templateDataBuilder.GetNextElementFunctionIndex();

                output.AddUnsafe(new ConstructedChildData(childProcessedType, child.index, outputIndex));

                Expression childNew = childProcessedType.GetConstructorExpression();

                if ((child.templateNodeType & TemplateNodeType.Slot) != 0) {
                    if (child.templateNodeType == TemplateNodeType.SlotDefine) {
                        string tagName = fileShell.GetString(child.tagNameRange);
                        context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.TemplateSystem_AddSlotChild, childNew, ExpressionUtil.GetStringConstant(tagName), ExpressionUtil.GetIntConstant(outputIndex)));
                    }
                    else {
                        // what do we do here?
                    }
                }
                else if ((child.templateNodeType & TemplateNodeType.Meta) != 0) {
                    // todo -- 
                }
                else {
                    context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.TemplateSystem_AddChild, childNew, ExpressionUtil.GetIntConstant(outputIndex)));
                }
            }
        }

        private void CompileNode(in ConstructedChildData constructedChildData, StructList<AttrInfo> injectedAttributes) {
            context.Setup();

            state.systemParam = context.AddParameter<TemplateSystem>("system");

            ProcessedType processedType = constructedChildData.processedType;
            int templateNodeId = constructedChildData.templateNodeId;
            ref TemplateASTNode templateNode = ref fileShell.templateNodes[templateNodeId];

            switch (processedType.archetype) {
                case ProcessedType.ElementArchetype.Template:
                    CompileTemplateNode(constructedChildData, templateNode);
                    return;

                case ProcessedType.ElementArchetype.Meta:
                    CompileMetaNode(constructedChildData, templateNode);
                    return;

                case ProcessedType.ElementArchetype.SlotDefine:
                    CompileSlotDefineNode(constructedChildData, templateNode);
                    return;

                case ProcessedType.ElementArchetype.SlotForward:
                    CompileSlotForwardNode(constructedChildData, templateNode);
                    return;

                case ProcessedType.ElementArchetype.SlotOverride: {
                    CompileSlotOverrideNode(constructedChildData, templateNode);
                    return;
                }

                case ProcessedType.ElementArchetype.Container: {
                    CompileContainerNode(constructedChildData, templateNode);
                    return;
                }

                case ProcessedType.ElementArchetype.Text: {
                    CompileTextNode(constructedChildData, templateNode);
                    return;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CompileMetaNode(ConstructedChildData constructedChildData, in TemplateASTNode templateNode) {
            throw new NotImplementedException("Meta");
        }

        private void CompileSlotDefineNode(ConstructedChildData constructedChildData, in TemplateASTNode templateNode) {

            int attrCount = CountRealAttributes(templateNode);

            EmitInitializeElement(templateNode, attrCount);
            InitializeElementAttributes(templateNode);

            StructList<ConstructedChildData> setupChildren = StructList<ConstructedChildData>.Get();
            StructList<AttrInfo> attributes = StructList<AttrInfo>.Get();
            StructList<AttrInfo> injectedChildAttributes = StructList<AttrInfo>.Get();

            AttributeMerger.ConvertToAttrInfo(fileShell, templateNode.index, attributes);

            bool alteredStateStack = CompileBindings(constructedChildData.processedType, templateNode);

            BuildElement(constructedChildData, templateNode, attributes, setupChildren);

            CompileChildren(setupChildren, injectedChildAttributes);

            if (alteredStateStack) {
                state.variableStack.Pop().Release();
            }

            injectedChildAttributes.Release();
            attributes.Release();
            setupChildren.Release();

        }

        private void BuildElement(ConstructedChildData constructedChildData, TemplateASTNode templateNode, StructList<AttrInfo> attributes, StructList<ConstructedChildData> setupChildren) {
            bool createChildrenLazily = GetCreateChildrenLazily(attributes);

            if (createChildrenLazily) {
                int idx = templateDataBuilder.GetNextElementFunctionIndex();
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_CreateChildrenIfEnabled, ExpressionUtil.GetIntConstant(idx)));
                BuildElementTemplate(constructedChildData.processedType, templateNode, constructedChildData.outputTemplateIndex);

                // new context needed since we create a new function
                context.Setup();
                state.systemParam = context.AddParameter<TemplateSystem>("system");
                SetupElementChildren(system, templateNode, setupChildren);
                // build
                LambdaExpression templateExpression = context.Build("LazyChildren");

                templateDataBuilder.SetElementTemplate(
                    "LazyChildren",
                    new LineInfo(templateNode.lineNumber, templateNode.columnNumber),
                    idx,
                    templateExpression
                );
            }
            else {
                SetupElementChildren(system, templateNode, setupChildren);
                BuildElementTemplate(constructedChildData.processedType, templateNode, constructedChildData.outputTemplateIndex);
            }
        }

        private void CompileSlotOverrides(SlotUsageSetup slotUsageSetup) {
            StructList<ConstructedChildData> childrenSetup = StructList<ConstructedChildData>.Get();

            if (slotUsageSetup.hasImplicitChildrenOverride) {
                context.Setup();
                state.systemParam = context.AddParameter<TemplateSystem>("system");
                int attrCount = 0;
                // todo -- attrs
                context.AddStatement(ExpressionFactory.CallInstance(
                    system,
                    MemberData.TemplateSystem_InitializeSlotElement,
                    ExpressionUtil.GetIntConstant(attrCount),
                    ExpressionUtil.GetIntConstant(1) // reference depth 
                ));

                SetupElementChildren_Slot(system, slotUsageSetup.childrenNodes, childrenSetup);
                LambdaExpression templateExpression = context.Build("Children");

                templateDataBuilder.SetElementTemplate(
                    "override:Children (Implicit)",
                    new LineInfo(-1, -1),
                    slotUsageSetup.implicitChildrenSlotId,
                    templateExpression
                );

                for (int i = 0; i < childrenSetup.size; i++) {
                    CompileNode(childrenSetup.array[i], null); // todo -- injected attributes
                }
            }

            for (int i = 0; i < slotUsageSetup.overrideNodes.size; i++) {
                childrenSetup.size = 0;
                context.Setup();
                state.systemParam = context.AddParameter<TemplateSystem>("system");
                SlotUsage usage = slotUsageSetup.overrideNodes.array[i];
                ref TemplateASTNode node = ref fileShell.templateNodes[usage.nodeIndex];

                int attrCount = 0; // todo -- attrs

                context.AddStatement(ExpressionFactory.CallInstance(
                    system,
                    MemberData.TemplateSystem_InitializeSlotElement,
                    ExpressionUtil.GetIntConstant(attrCount),
                    ExpressionUtil.GetIntConstant(1) // reference depth
                ));

                InitializeElementAttributes(node);
                SetupElementChildren(system, node, childrenSetup);

                LambdaExpression templateExpression = context.Build("Override");

                templateDataBuilder.SetElementTemplate(
                    "override:" + usage.slotName,
                    new LineInfo(-1, -1),
                    usage.templateIndex,
                    templateExpression
                );

                for (int j = 0; j < childrenSetup.size; j++) {
                    CompileNode(childrenSetup.array[j], null); // todo -- injected attributes
                }
            }

            childrenSetup.Release();
        }

        private void CompileTextNode(ConstructedChildData constructedChildData, in TemplateASTNode templateNode) {

            if (templateNode.childCount != 0) {
                ReportNonCriticalError("<Text> elements cannot have children. Ignoring children of <Text> node");
            }

            int attrCount = CountRealAttributes(templateNode);

            int templateNodeId = templateNode.index;

            EmitInitializeElement(templateNode, attrCount);

            if (fileShell.IsTextConstant(templateNodeId)) {
                StructList<TextContent> contents = StructList<TextContent>.Get();
                fileShell.TryGetTextContent(templateNodeId, contents);
                TextUtil.StringBuilder.Clear();

                for (int i = 0; i < contents.size; i++) {
                    TextUtil.StringBuilder.Append(fileShell.GetString(contents[i].stringRange));
                }

                context.AddStatement(ExpressionFactory.CallInstance(
                    system,
                    MemberData.TemplateSystem_SetText,
                    Expression.Constant(TextUtil.StringBuilder.ToString()))
                );

                contents.Release();
            }

            InitializeElementAttributes(templateNode);

            CompileBindings(constructedChildData.processedType, templateNode);

            BuildElementTemplate(constructedChildData.processedType, templateNode, constructedChildData.outputTemplateIndex);

            // todo -- text doesn't support children right now
        }

        private void BuildElementTemplate(ProcessedType processedType, in TemplateASTNode templateNode, int outputTemplateIndex) {
            string formattedTagName = GetFormattedTagName(processedType, templateNode);
            LambdaExpression templateExpression = context.Build(formattedTagName);

            templateDataBuilder.SetElementTemplate(
                formattedTagName,
                new LineInfo(templateNode.lineNumber, templateNode.columnNumber),
                outputTemplateIndex,
                templateExpression
            );

        }

        private void EmitInitializeElement(in TemplateASTNode templateNode, int attrCount) {

            if (GetCreateDisabledAttribute(templateNode)) {
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeElementDisabled, ExpressionUtil.GetIntConstant(attrCount)));
            }
            else {
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount)));
            }
        }

        private void CompileContainerNode(ConstructedChildData constructedChildData, in TemplateASTNode templateNode) {
            int attrCount = CountRealAttributes(templateNode);

            EmitInitializeElement(templateNode, attrCount);
            InitializeElementAttributes(templateNode);

            StructList<ConstructedChildData> setupChildren = StructList<ConstructedChildData>.Get();
            StructList<AttrInfo> attributes = StructList<AttrInfo>.Get();
            StructList<AttrInfo> injectedChildAttributes = StructList<AttrInfo>.Get();

            AttributeMerger.ConvertToAttrInfo(fileShell, templateNode.index, attributes);

            bool alteredStateStack = CompileBindings(constructedChildData.processedType, templateNode);

            bool createChildrenLazily = GetCreateChildrenLazily(attributes);

            if (createChildrenLazily) {
                int idx = templateDataBuilder.GetNextElementFunctionIndex();
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_CreateChildrenIfEnabled, ExpressionUtil.GetIntConstant(idx)));
                BuildElementTemplate(constructedChildData.processedType, templateNode, constructedChildData.outputTemplateIndex);
                // new context needed
                context.Setup();
                state.systemParam = context.AddParameter<TemplateSystem>("system");
                SetupElementChildren(system, templateNode, setupChildren);
                // build
                LambdaExpression templateExpression = context.Build("LazyChildren");

                templateDataBuilder.SetElementTemplate(
                    "LazyChildren",
                    new LineInfo(templateNode.lineNumber, templateNode.columnNumber),
                    idx,
                    templateExpression
                );
            }
            else {
                SetupElementChildren(system, templateNode, setupChildren);
                BuildElementTemplate(constructedChildData.processedType, templateNode, constructedChildData.outputTemplateIndex);
            }

            CompileChildren(setupChildren, injectedChildAttributes);

            if (alteredStateStack) {
                state.variableStack.Pop().Release();
            }

            injectedChildAttributes.Release();
            attributes.Release();
            setupChildren.Release();
        }

        private void CompileSlotForwardNode(ConstructedChildData constructedChildData, in TemplateASTNode templateNode) {
            throw new NotImplementedException("Slot forward not implement");
        }

        private void CompileSlotOverrideNode(ConstructedChildData constructedChildData, in TemplateASTNode templateNode) {
            int attrCount = 0;

            StructList<ConstructedChildData> setupChildren = StructList<ConstructedChildData>.Get();

            context.AddStatement(ExpressionFactory.CallInstance(
                system,
                MemberData.TemplateSystem_InitializeSlotElement,
                ExpressionUtil.GetIntConstant(attrCount),
                ExpressionUtil.GetIntConstant(attrCompilerContext.references.size)
            ));

            InitializeElementAttributes(templateNode);

            SetupElementChildren(system, templateNode, setupChildren);

            setupChildren.Release();
        }

        private void CompileTemplateNode(ConstructedChildData constructedChildData, in TemplateASTNode templateNode) {
            TemplateExpressionSet innerTemplate = CompileTemplate(constructedChildData.processedType);

            TemplateFileShell shell = compilationHandler.GetTemplateForType(constructedChildData.processedType);
            TemplateASTRoot innerTemplateRootNode = shell.GetRootTemplateForType(constructedChildData.processedType);
            // ref TemplateASTNode innerTemplateNode = ref shell.templateNodes[innerTemplateRootNode.templateIndex];

            StructList<AttrInfo> mergedAttributes = StructList<AttrInfo>.Get();

            AttributeMerger.MergeExpandedAttributes(shell, innerTemplateRootNode.templateIndex, fileShell, templateNode.index, mergedAttributes);
            int attrCount = CountRealAttributes(mergedAttributes);

            if (GetCreateDisabledAttribute(mergedAttributes)) {
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeHydratedElementDisabled, ExpressionUtil.GetIntConstant(attrCount)));
            }
            else {
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeHydratedElement, ExpressionUtil.GetIntConstant(attrCount)));
            }

            InitializeElementAttributes(mergedAttributes);

            if (!TrySetupSlotChildren(templateNode, innerTemplate, out SlotUsageSetup slotUsageSetup)) {
                // return? not sure how to handle this, maybe just ignore children of slot and keep compiling to collect errors
            }

            if (slotUsageSetup.hasImplicitChildrenOverride) {
                int slotOverrideTemplateIndex = templateDataBuilder.GetNextElementFunctionIndex();
                slotUsageSetup.implicitChildrenSlotId = slotOverrideTemplateIndex;
                context.AddStatement(ExpressionFactory.CallInstance(
                    system,
                    MemberData.TemplateSystem_OverrideSlot,
                    ExpressionUtil.GetStringConstant("Children"),
                    ExpressionUtil.GetIntConstant(slotOverrideTemplateIndex)
                ));
            }

            for (int i = 0; i < slotUsageSetup.overrideNodes.size; i++) {
                int slotOverrideTemplateIndex = templateDataBuilder.GetNextElementFunctionIndex();
                slotUsageSetup.overrideNodes.array[i].templateIndex = slotOverrideTemplateIndex;
                context.AddStatement(ExpressionFactory.CallInstance(
                    system,
                    MemberData.TemplateSystem_OverrideSlot,
                    ExpressionUtil.GetStringConstant(slotUsageSetup.overrideNodes.array[i].slotName),
                    ExpressionUtil.GetIntConstant(slotOverrideTemplateIndex)
                ));
            }

            context.AddStatement(ExpressionFactory.CallInstance(
                system,
                MemberData.TemplateSystem_HydrateElement,
                Expression.Constant(constructedChildData.processedType.rawType),
                ExpressionUtil.GetIntConstant(innerTemplate.index))
            );

            BuildElementTemplate(constructedChildData.processedType, templateNode, constructedChildData.outputTemplateIndex);

            CompileSlotOverrides(slotUsageSetup);
            mergedAttributes.Release();
        }

        public string GetFormattedTagName(ProcessedType processedType, in TemplateASTNode templateNode) {
            return processedType.tagName; // todo -- totally wrong
            // switch (node.templateNodeType) {
            //
            //     case TemplateNodeType.Unresolved:
            //         return "unresolved";
            //
            //     case TemplateNodeType.SlotDefine:
            //         return "define:" + fileShell.GetString(node.tagNameRange);
            //
            //     case TemplateNodeType.SlotForward:
            //         return "<forward:" + fileShell.GetString(node.tagNameRange);;
            //
            //     case TemplateNodeType.SlotOverride:
            //         return "override:" + fileShell.GetString(node.tagNameRange);;
            //
            //     case TemplateNodeType.Root:
            //         return processedType.tagName;
            //
            //     case TemplateNodeType.Expanded:
            //     case TemplateNodeType.Container:
            //     case TemplateNodeType.Text:{
            //         // todo -- might want to print out a nice generic string if type is generic
            //         string moduleName = GetModuleName(templateNodeIndex);
            //         string tagName = processedType.tagName; //fileShell.GetString(node.tagNameRange);;
            //         if (moduleName != null) {
            //             return moduleName + ":" + tagName;
            //         }
            //
            //         return tagName;
            //     }
            //
            //     case TemplateNodeType.Repeat:
            //         return "Repeat";
            // }
            //
            // return null;
        }

        private bool CompileBindings(ProcessedType processedType, in TemplateASTNode templateNode) {
            return false;
        }

        private Type ResolveRequiredType(in TemplateASTNode templateNode, out LineInfo lineInfo) {

            string typeExpression = GetRequiredTypeAttribute(templateNode, out lineInfo);

            if (string.IsNullOrEmpty(typeExpression)) {
                return null;
            }

            // todo -- list of referenced namespaces from template and implicitly from module
            Type requiredType = null;

            try {
                requiredType = TypeResolver.ResolveTypeExpression(rootProcessedType.rawType, null, typeExpression);
            }
            catch (TypeResolver.TypeResolutionException typeResolutionException) {
                LogError($"Unable to resolve required child type `{typeExpression}`. {typeResolutionException.Message}", lineInfo);
                return null; // not really sure what the correct action is here. probably just continue and assume no error happened. compilation fails if any errors were logged so should be ok.
            }

            if (requiredType == null) {
                LogError($"Unable to resolve required child type `{typeExpression}`", lineInfo);
                return null;
            }

            if (!requiredType.IsInterface && !typeof(UIElement).IsAssignableFrom(requiredType)) {
                LogError($"When requiring an explicit child type, that type must either be an interface or a subclass of UIElement. {requiredType} was neither", lineInfo);
                return null;
            }

            return requiredType;
        }

        private int CountRealAttributes(TemplateASTNode node) {
            if (node.attributeCount == 0) return 0;
            int count = 0;

            AttributeDefinition3[] attrList = fileShell.attributeList;
            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                if (attrList[i].type == AttributeType.Attribute) {
                    count++;
                }
            }

            return count;
        }

        private int CountRealAttributes(StructList<AttrInfo> attributes) {
            int count = 0;

            for (int i = 0; i < attributes.size; i++) {
                ref AttrInfo attr = ref attributes.array[i];
                if (attr.type != AttributeType.Attribute) {
                    continue;
                }

                count++;
            }

            return count;
        }

        private void InitializeElementAttributes(StructList<AttrInfo> attributes) {
            for (int i = 0; i < attributes.size; i++) {
                ref AttrInfo attr = ref attributes.array[i];

                if (attr.type != AttributeType.Attribute) {
                    continue;
                }

                context.AddStatement((attr.flags & AttributeFlags.Const) != 0
                    ? ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeStaticAttribute, ExpressionUtil.GetStringConstant(attr.key), ExpressionUtil.GetStringConstant(attr.value))
                    : ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeDynamicAttribute, ExpressionUtil.GetStringConstant(attr.key)));
            }
        }

        private void InitializeElementAttributes(in TemplateASTNode node) {
            AttributeDefinition3[] attributes = fileShell.attributeList;
            int end = node.attributeRangeEnd;

            for (int i = node.attributeRangeStart; i < end; i++) {
                ref AttributeDefinition3 attr = ref attributes[i];

                if (attr.type != AttributeType.Attribute) {
                    continue;
                }

                string key = fileShell.GetString(attr.key);
                string value = fileShell.GetString(attr.value);

                context.AddStatement((attr.flags & AttributeFlags.Const) != 0
                    ? ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeStaticAttribute, ExpressionUtil.GetStringConstant(key), ExpressionUtil.GetStringConstant(value))
                    : ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeDynamicAttribute, ExpressionUtil.GetStringConstant(key)));
            }
        }

        public struct SlotUsage {

            public int nodeIndex;
            public string slotName;
            public int templateIndex;

        }

        private struct SlotUsageSetup {

            public StructList<int> childrenNodes;
            public StructList<SlotUsage> overrideNodes;
            public StructList<SlotUsage> forwardNodes;
            public int implicitChildrenSlotId;
            public bool hasImplicitChildrenOverride => childrenNodes.size > 0;

            public void Release() {
                childrenNodes?.Release();
                overrideNodes?.Release();
                forwardNodes?.Release();
            }

        }

        private bool TrySetupSlotChildren(in TemplateASTNode templateNode, TemplateExpressionSet innerTemplate, out SlotUsageSetup slotSetup) {
            slotSetup = new SlotUsageSetup();

            int ptr = templateNode.firstChildIndex;

            // for all non <override> nodes lift them out and treat as a single template
            // for all explicit <override> nodes 
            // seperate into 3 groups: children overrides, override, forward

            StructList<int> childrenNodes = StructList<int>.Get();
            StructList<SlotUsage> overrideNodes = StructList<SlotUsage>.Get();
            StructList<SlotUsage> forwardNodes = StructList<SlotUsage>.Get();

            slotSetup.childrenNodes = childrenNodes;
            slotSetup.overrideNodes = overrideNodes;
            slotSetup.forwardNodes = forwardNodes;

            while (ptr != -1) {
                ref TemplateASTNode child = ref fileShell.templateNodes[ptr];

                if (child.templateNodeType == TemplateNodeType.SlotOverride) {
                    string slotName = fileShell.GetString(child.tagNameRange);
                    overrideNodes.Add(new SlotUsage() {
                        slotName = slotName,
                        nodeIndex = child.index
                    });
                }
                else if (child.templateNodeType == TemplateNodeType.SlotForward) {
                    string slotName = fileShell.GetString(child.tagNameRange);
                    forwardNodes.Add(new SlotUsage() {
                        slotName = slotName,
                        nodeIndex = child.index
                    });
                }
                else {
                    childrenNodes.Add(child.index);
                }

                ptr = child.nextSiblingIndex;
            }

            if (childrenNodes.size != 0 && overrideNodes.size != 0) {
                for (int i = 0; i < overrideNodes.size; i++) {
                    if (overrideNodes.array[i].slotName == "Children") {
                        LogError("You cannot provide an <override:Children> slot and use implicit children", new LineInfo(templateNode.lineNumber, templateNode.columnNumber));
                        slotSetup.Release();
                        return false;
                    }
                }
            }

            // todo -- validate slot names are valid

            return true;
        }

        private void LogError(string message, LineInfo lineInfo) {
            diagnostics.LogError(message, fileShell.filePath, lineInfo.line, lineInfo.column);
        }

        private void ReportNonCriticalError(string message) {
            // todo file & line number
            diagnostics.LogWarning(message);
        }

        private ProcessedType ResolveGenericElementType(TemplateASTNode templateNode, ProcessedType processedType) {

            if (GetGenericTypeAttribute(templateNode, out string typeName, out LineInfo lineInfo)) {

                if (string.IsNullOrEmpty(typeName)) {
                    return null;
                }

                return ResolveGenericElementTypeFromAttribute(templateNode, processedType, typeName, lineInfo);
            }
            else {
                throw new NotImplementedException("Todo -- resolve generic from fields / properties");
            }

            return null;

        }

        private ProcessedType ResolveGenericElementTypeFromAttribute(TemplateASTNode templateNode, ProcessedType processedType, string typeName, LineInfo attributeLineInfo) {
            LightList<string> strings = LightList<string>.Get();
            Type[] arguments = processedType.rawType.GetGenericArguments();
            Type[] resolvedTypes = new Type[arguments.Length];

            if (typeName.Contains("<")) {

                string replaceSpec = typeName.Replace("[", "<").Replace("]", ">");

                int ptr = 0;
                int rangeStart = 0;
                int depth = 0;

                while (ptr != replaceSpec.Length) {
                    char c = replaceSpec[ptr];
                    switch (c) {
                        case '<':
                            depth++;
                            break;

                        case '>':
                            depth--;
                            break;

                        case ',': {
                            if (depth == 0) {
                                strings.Add(replaceSpec.Substring(rangeStart, ptr));
                                rangeStart = ptr;
                            }

                            break;
                        }
                    }

                    ptr++;
                }

                if (rangeStart != ptr) {
                    strings.Add(replaceSpec.Substring(rangeStart, ptr));
                }

                if (arguments.Length != strings.size) {
                    LogError($"Unable to resolve generic type of tag <{GetFormattedTagName(processedType, templateNode)}> Expected {arguments.Length} arguments but was only provided {strings.size} {typeName}", attributeLineInfo);
                    return null;
                }

            }
            else {
                strings.Add(typeName);
            }

            LightList<string> namespaces = state.namespaces;

            for (int i = 0; i < strings.size; i++) {
                if (ExpressionParser.TryParseTypeName(strings[i], out TypeLookup typeLookup)) {
                    Type type = TypeResolver.ResolveType(typeLookup, namespaces);

                    if (type == null) {
                        LogError(ErrorMessages.UnresolvedType(typeLookup, namespaces), attributeLineInfo);
                        return null;
                    }

                    resolvedTypes[i] = type;
                }
                else {
                    LogError($"Unable to resolve generic type of tag <{GetFormattedTagName(processedType, templateNode)}>. Failed to parse generic specifier {strings[i]}. Original expression = {typeName}", attributeLineInfo);
                    return null;
                }
            }

            strings.Release();
            Type createdType = ReflectionUtil.CreateGenericType(processedType.rawType, resolvedTypes);
            return TypeProcessor.GetOrCreateGeneric(processedType, createdType);
        }

        private ProcessedType ResolveGenericElementType(IList<string> namespaces, Type rootType, TemplateASTNode templateNode) {
            // ProcessedType processedType = templateNode.processedType;

            // Type generic = processedType.rawType;
            // Type[] arguments = processedType.rawType.GetGenericArguments();
            // Type[] resolvedTypes = new Type[arguments.Length];
            //
            // typeResolver.Init();
            // typeResolver.SetSignature(new Parameter(rootType, "__root", ParameterFlags.NeverNull));
            // typeResolver.SetImplicitContext(typeResolver.GetParameter("__root"));
            // typeResolver.Setup();
            //
            // if (templateNode.attributes.array == null) {
            //     throw TemplateCompileException.UnresolvedGenericElement(processedType, templateNode.TemplateNodeDebugData);
            // }
            //
            // for (int i = 0; i < templateNode.attributes.size; i++) {
            //     ref AttributeDefinition attr = ref templateNode.attributes.array[i];
            //
            //     if (attr.type != AttributeType.Property) continue;
            //
            //     if (ReflectionUtil.IsField(generic, attr.key, out FieldInfo fieldInfo)) {
            //         if (fieldInfo.FieldType.IsGenericParameter || fieldInfo.FieldType.IsGenericType || fieldInfo.FieldType.IsConstructedGenericType) {
            //             if (ValidForGenericResolution(fieldInfo.FieldType)) {
            //                 HandleType(fieldInfo.FieldType, attr);
            //             }
            //         }
            //     }
            //     else if (ReflectionUtil.IsProperty(generic, attr.key, out PropertyInfo propertyInfo)) {
            //         if (propertyInfo.PropertyType.IsGenericParameter || propertyInfo.PropertyType.IsGenericType || propertyInfo.PropertyType.IsConstructedGenericType) {
            //             HandleType(propertyInfo.PropertyType, attr);
            //         }
            //     }
            // }
            //
            // for (int i = 0; i < arguments.Length; i++) {
            //     if (resolvedTypes[i] == null) {
            //         throw TemplateCompileException.UnresolvedGenericElement(processedType, templateNode.TemplateNodeDebugData);
            //     }
            // }
            //
            // Type newType = ReflectionUtil.CreateGenericType(processedType.rawType, resolvedTypes);
            // ProcessedType retn = TypeProcessor.AddResolvedGenericElementType(newType, processedType);
            // return retn;
            //
            // bool ValidForGenericResolution(Type checkType) {
            //     if (checkType.IsConstructedGenericType) {
            //         Type[] args = checkType.GetGenericArguments();
            //         for (int i = 0; i < args.Length; i++) {
            //             if (args[i].IsConstructedGenericType) {
            //                 return false;
            //             }
            //         }
            //     }
            //
            //     return true;
            // }
            //
            // int GetTypeIndex(Type[] _args, string name) {
            //     for (int i = 0; i < _args.Length; i++) {
            //         if (_args[i].Name == name) {
            //             return i;
            //         }
            //     }
            //
            //     return -1;
            // }
            //
            // void HandleType(Type inputType, in AttributeDefinition attr) {
            //     if (!inputType.ContainsGenericParameters) {
            //         return;
            //     }
            //
            //     if (inputType.IsConstructedGenericType) {
            //         if (ReflectionUtil.IsAction(inputType) || ReflectionUtil.IsFunc(inputType)) {
            //             return;
            //         }
            //
            //         Type expressionType = typeResolver.GetExpressionType(attr.value);
            //
            //         Type[] typeArgs = expressionType.GetGenericArguments();
            //         Type[] memberGenericArgs = inputType.GetGenericArguments();
            //
            //         Assert.AreEqual(memberGenericArgs.Length, typeArgs.Length);
            //
            //         for (int a = 0; a < memberGenericArgs.Length; a++) {
            //             string genericName = memberGenericArgs[a].Name;
            //             int typeIndex = GetTypeIndex(arguments, genericName);
            //
            //             if (typeIndex == -1) {
            //                 throw new TemplateCompileException(templateNode.TemplateNodeDebugData.tagName + templateNode.TemplateNodeDebugData.lineInfo);
            //             }
            //
            //             Assert.IsTrue(typeIndex != -1);
            //
            //             if (resolvedTypes[typeIndex] != null) {
            //                 if (resolvedTypes[typeIndex] != typeArgs[a]) {
            //                     throw TemplateCompileException.DuplicateResolvedGenericArgument(templateNode.GetTagName(), inputType.Name, resolvedTypes[typeIndex], typeArgs[a]);
            //                 }
            //             }
            //
            //             resolvedTypes[typeIndex] = typeArgs[a];
            //         }
            //     }
            //     else {
            //         string genericName = inputType.Name;
            //         int typeIndex = GetTypeIndex(arguments, genericName);
            //         Assert.IsTrue(typeIndex != -1);
            //
            //         Type type = typeResolver.GetExpressionType(attr.value);
            //         if (resolvedTypes[typeIndex] != null) {
            //             if (resolvedTypes[typeIndex] != type) {
            //                 throw TemplateCompileException.DuplicateResolvedGenericArgument(templateNode.GetTagName(), inputType.Name, resolvedTypes[typeIndex], type);
            //             }
            //         }
            //
            //         resolvedTypes[typeIndex] = type;
            //     }
            // }
            return null;
        }

        private struct ConstructedChildData {

            public readonly int templateNodeId;
            public readonly int outputTemplateIndex;
            public readonly ProcessedType processedType;

            public ConstructedChildData(ProcessedType p, int templateNodeId, int outputTemplateIndex) {
                this.processedType = p;
                this.outputTemplateIndex = outputTemplateIndex;
                this.templateNodeId = templateNodeId;
            }

        }

        public struct TemplateCompilationResult {

            public bool successful;
            public LightList<TemplateExpressionSet> templateExpressionSets;
            public StructList<CompiledExpression> pendingCompilations;

        }

        private static class ErrorMessages {

            public static string UnresolvedType(TypeLookup typeLookup, IReadOnlyList<string> searchedNamespaces = null) {
                string retn = string.Empty;
                if (searchedNamespaces != null) {
                    retn += " searched in the following namespaces: ";
                    for (int i = 0; i < searchedNamespaces.Count - 1; i++) {
                        retn += searchedNamespaces[i] + ",";
                    }

                    retn += searchedNamespaces[searchedNamespaces.Count - 1];
                }

                return $"Unable to resolve type {typeLookup}, are you missing a namespace?{retn}";
            }

        }

    }

}