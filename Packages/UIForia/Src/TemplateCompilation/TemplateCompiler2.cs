using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Text;
using UIForia.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia.Compilers {

    internal class TemplateCompiler2 {

        private State state;

        private SizedArray<AttrInfo> scratchAttributes;

        private SizedArray<AttributeDefinition2> scratch1;
        private SizedArray<AttributeDefinition2> scratch2;

        private readonly AttributeCompiler attributeCompiler;

        private readonly Dictionary<ProcessedType, TemplateExpressionSet> compiledTemplates;

        public event Action<TemplateExpressionSet> onTemplateCompiled;

        private StyleSheetImporter importer = new StyleSheetImporter("", new ResourceManager());

        private static readonly Dictionary<Type, MethodInfo> s_CreateVariableMethodCache = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> s_ReferenceVariableMethodCache = new Dictionary<Type, MethodInfo>();

        private readonly LightList<State> statePool;
        private readonly StructStack<State> stateStack;
        private readonly AttributeCompilerContext attrCompilerContext;
        private readonly TemplateLinqCompiler typeResolver;
        private Diagnostics diagnostics;
        private int templateNumber;

        private Compilation compilation;

        public TemplateCompiler2(Compilation compilation) {
            this.compilation = compilation;
            this.diagnostics = compilation.diagnostics;
            this.stringBuilder = new StringBuilder(512);
            this.statePool = new LightList<State>();
            this.stateStack = new StructStack<State>();
            this.attrCompilerContext = new AttributeCompilerContext();
            this.attributeCompiler = new AttributeCompiler(attrCompilerContext);
            this.scratchAttributes = new SizedArray<AttrInfo>(16);
            this.compiledTemplates = new Dictionary<ProcessedType, TemplateExpressionSet>();
            this.typeResolver = new TemplateLinqCompiler(attrCompilerContext);
            this.templateNumber = 0;
        }
        // private readonly AttributeCompilerContext attrCompilerContext;
        // private readonly TemplateLinqCompiler typeResolver;

        private readonly StringBuilder stringBuilder;

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

            public StructList<StyleSheetReference> styleReferences;
            public LightStack<StructList<BindingVariableDesc>> variableStack;
            public TemplateASTRoot templateRootAST;
            public TemplateFileShell templateFile;
            public int compiledTemplateId;

        }

        private void PushState(ProcessedType processedType) {
            if (state.context == null) {
                state = GetState(processedType);
            }
            else {
                stateStack.Push(state);
                state = GetState(processedType);
            }
        }

        private State GetState(ProcessedType processedType) {
            TemplateFileShell shell = compilation.templateMap[processedType];
            TemplateASTRoot template = shell.GetRootTemplateForType(processedType);

            if (statePool.size == 0) {
                return new State() {
                    context = new TemplateCompilerContext(diagnostics),
                    templateDataBuilder = new TemplateDataBuilder(),
                    rootProcessedType = processedType,
                    templateRootAST = template,
                    templateFile = shell,
                    styleReferences = new StructList<StyleSheetReference>(),
                    variableStack = new LightStack<StructList<BindingVariableDesc>>(),
                    compiledTemplateId = templateNumber++
                };
            }

            State retn = statePool.RemoveLast();
            retn.rootProcessedType = processedType;
            retn.templateRootAST = template;
            retn.templateFile = shell;
            retn.compiledTemplateId = templateNumber++;
            retn.variableStack.Clear();
            retn.styleReferences.Clear();
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

            compilation.moduleMap.TryGetValue(rootProcessedType, out UIModule module);

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
                    if (!string.IsNullOrEmpty(location.filePath)) {
                        if (!File.Exists(location.filePath)) {
                            diagnostics.LogWarning($"Failed to location style file at {location.filePath}");
                            continue;
                        }

                        StyleSheet sheet = importer.ImportStyleSheetFromFile(location.filePath);

                        state.styleReferences.Add(new StyleSheetReference() {
                            alias = lookup.styleAlias,
                            styleSheet = sheet
                        });
                    }
                }
            }
        }

        public TemplateExpressionSet CompileTemplate(ProcessedType processedType) {
            if (compiledTemplates.TryGetValue(processedType, out TemplateExpressionSet retn)) {
                return retn;
            }

            PushState(processedType);

            CompileStyleSheets();

            CompileEntryPoint(processedType);

            retn = templateDataBuilder.Build(processedType);

            compiledTemplates[processedType] = retn;

            PopState();

            onTemplateCompiled?.Invoke(retn);

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

            onCompilationReady?.Invoke(new Compilation.PendingCompilation() {
                expression = entryPointFn,
                type = CompileTarget.EntryPoint,
                targetIndex = 0,
                templateId = compiledTemplateId
            });

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
            //onTemplateCompiled?.Invoke(context.id, CompilationTarget.HydratePoint, 0, fn);

            onCompilationReady?.Invoke(new Compilation.PendingCompilation() {
                expression = hydrateFn,
                type = CompileTarget.HydratePoint,
                targetIndex = 0,
                templateId = compiledTemplateId
            });

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

        private string GetRequiredTypeAttribute(int nodeIdx) {
            ref TemplateASTNode node = ref fileShell.templateNodes[nodeIdx];
            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                if (fileShell.attributeList[i].type == AttributeType.RequireType) {
                    return fileShell.GetString(fileShell.attributeList[i].value);
                }
            }

            return null;
        }

        private void SetupElementChildren_Slot(Expression systemParam, StructList<int> childrenIds, StructList<ConstructedChildData> output) {
            TemplateASTNode[] templateNodes = fileShell.templateNodes;

            Type requiredType = null; //ResolveRequiredType(fileShell.GetRequiredType(parent.index));
            for (int i = 0; i < childrenIds.size; i++) {
                int childIndex = childrenIds.array[i];
                ref TemplateASTNode child = ref templateNodes[childIndex];

                ProcessedType childProcessedType = fileShell.typeMappings[child.index];

                // todo -- fix this
                if (requiredType != null && requiredType.IsAssignableFrom(childProcessedType.rawType)) {
                    diagnostics.LogError(($"type {childProcessedType.rawType} doesnt match required type of {requiredType}"));
                    continue;
                }

                int outputIndex = templateDataBuilder.GetNextTemplateIndex();

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
        private void SetupElementChildren(Expression systemParam, in TemplateASTNode parentNode, StructList<ConstructedChildData> output) {
            TemplateASTNode[] templateNodes = fileShell.templateNodes;

            int childIndex = parentNode.firstChildIndex;

            Type requiredType = null; //ResolveRequiredType(fileShell.GetRequiredType(parent.index));

            while (childIndex != -1) {
                ref TemplateASTNode child = ref templateNodes[childIndex];
                childIndex = child.nextSiblingIndex;

                ProcessedType childProcessedType = fileShell.typeMappings[child.index];

                // todo -- fix this
                if (requiredType != null && requiredType.IsAssignableFrom(childProcessedType.rawType)) {
                    diagnostics.LogError(($"type {childProcessedType.rawType} doesnt match required type of {requiredType}"));
                    continue;
                }

                int outputIndex = templateDataBuilder.GetNextTemplateIndex();

                output.AddUnsafe(new ConstructedChildData(childProcessedType, child.index, outputIndex));

                Expression childNew = childProcessedType.GetConstructorExpression();

                if ((child.templateNodeType & TemplateNodeType.Slot) != 0) {
                    if (child.templateNodeType == TemplateNodeType.SlotDefine) {
                        string tagName = fileShell.GetString(child.tagNameRange);
                        context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.TemplateSystem_AddSlotChild, childNew, ExpressionUtil.GetStringConstant(tagName), ExpressionUtil.GetIntConstant(outputIndex)));
                    }
                    else { }
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
            throw new NotImplementedException("Slot Define");
        }

        private void CompileSlotOverrides(SlotUsageSetup slotUsageSetup) {
            StructList<ConstructedChildData> childrenSetup = StructList<ConstructedChildData>.Get();

            if (slotUsageSetup.hasImplicitChildrenOverride) {
                context.Setup();
                state.systemParam = context.AddParameter<TemplateSystem>("system");
                int attrCount = 0;
                // todo -- attrs
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeSlotElement, ExpressionUtil.GetIntConstant(attrCount)));
                SetupElementChildren_Slot(system, slotUsageSetup.childrenNodes, childrenSetup);
                LambdaExpression templateExpression = context.Build("Children");

                templateDataBuilder.SetElementTemplate(
                    "override:Children (Implicit)",
                    new LineInfo(-1, -1),
                    slotUsageSetup.implicitChildrenSlotId,
                    templateExpression
                );

                onCompilationReady?.Invoke(new Compilation.PendingCompilation() {
                    expression = templateExpression,
                    type = CompileTarget.Element,
                    targetIndex = slotUsageSetup.implicitChildrenSlotId,
                    templateId = compiledTemplateId
                });

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

                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeSlotElement, ExpressionUtil.GetIntConstant(attrCount)));

                InitializeElementAttributes(node);
                SetupElementChildren(system, node, childrenSetup);

                LambdaExpression templateExpression = context.Build("Override");

                templateDataBuilder.SetElementTemplate(
                    "override:" + usage.slotName,
                    new LineInfo(-1, -1),
                    usage.templateIndex,
                    templateExpression
                );

                onCompilationReady?.Invoke(new Compilation.PendingCompilation() {
                    expression = templateExpression,
                    type = CompileTarget.Element,
                    targetIndex = usage.templateIndex,
                    templateId = compiledTemplateId
                });

                for (int j = 0; j < childrenSetup.size; j++) {
                    CompileNode(childrenSetup.array[j], null); // todo -- injected attributes
                }
            }

            childrenSetup.Release();
        }

        private void CompileTextNode(ConstructedChildData constructedChildData, in TemplateASTNode templateNode) {
            int attrCount = CountRealAttributes(templateNode);

            int templateNodeId = templateNode.index;

            context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount)));

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
            onCompilationReady?.Invoke(new Compilation.PendingCompilation() {
                expression = templateExpression,
                type = CompileTarget.Element,
                targetIndex = outputTemplateIndex,
                templateId = compiledTemplateId
            });
        }

        private void CompileContainerNode(ConstructedChildData constructedChildData, in TemplateASTNode templateNode) {
            int attrCount = CountRealAttributes(templateNode);

            StructList<ConstructedChildData> setupChildren = StructList<ConstructedChildData>.Get();
            context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount)));

            InitializeElementAttributes(templateNode);

            SetupElementChildren(system, templateNode, setupChildren);
            bool alteredStateStack = CompileBindings(constructedChildData.processedType, templateNode);


            StructList<AttrInfo> injectedChildAttributes = StructList<AttrInfo>.Get();

            BuildElementTemplate(constructedChildData.processedType, templateNode, constructedChildData.outputTemplateIndex);

            CompileChildren(setupChildren, injectedChildAttributes);

            if (alteredStateStack) {
                state.variableStack.Pop().Release();
            }

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


            TemplateFileShell shell = compilation.templateMap[constructedChildData.processedType];
            TemplateASTRoot innerTemplateRootNode = shell.GetRootTemplateForType(constructedChildData.processedType);
            ref TemplateASTNode innerTemplateNode = ref shell.templateNodes[innerTemplateRootNode.templateIndex];

            StructList<AttrInfo> mergedAttributes = StructList<AttrInfo>.Get();

            AttributeMerger.MergeExpandedAttributes(shell, innerTemplateRootNode.templateIndex, fileShell, rootNode.index, mergedAttributes);
            int attrCount = CountRealAttributes(mergedAttributes);

            context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeHydratedElement, ExpressionUtil.GetIntConstant(attrCount)));
            InitializeElementAttributes(mergedAttributes);


            if (!TrySetupSlotChildren(templateNode, innerTemplate, out SlotUsageSetup slotUsageSetup)) {
                // return? not sure how to handle this, maybe just ignore children of slot and keep compiling to collect errors
            }

            if (slotUsageSetup.hasImplicitChildrenOverride) {
                int slotOverrideTemplateIndex = templateDataBuilder.GetNextTemplateIndex();
                slotUsageSetup.implicitChildrenSlotId = slotOverrideTemplateIndex;
                context.AddStatement(ExpressionFactory.CallInstance(
                    system,
                    MemberData.TemplateSystem_OverrideSlot,
                    ExpressionUtil.GetStringConstant("Children"),
                    ExpressionUtil.GetIntConstant(slotOverrideTemplateIndex)
                ));
            }

            for (int i = 0; i < slotUsageSetup.overrideNodes.size; i++) {
                int slotOverrideTemplateIndex = templateDataBuilder.GetNextTemplateIndex();
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
                ExpressionUtil.GetIntConstant(constructedChildData.outputTemplateIndex))
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

        private Type ResolveRequiredType(string typeExpression) {
            if (string.IsNullOrEmpty(typeExpression)) {
                return null;
            }

            // todo -- list of referenced namespaces from template and implicitly from module
            Type requiredType = TypeResolver.ResolveTypeExpression(rootProcessedType.rawType, null, typeExpression);

            if (requiredType == null) {
                diagnostics.LogError($"Unable to resolve required child type `{typeExpression}`");
                return null;
            }

            if (!requiredType.IsInterface && !typeof(UIElement).IsAssignableFrom(requiredType)) {
                diagnostics.LogError($"When requiring an explicit child type, that type must either be an interface or a subclass of UIElement. {requiredType} was neither");
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

        public event Action<Compilation.PendingCompilation> onCompilationReady;

    }

}