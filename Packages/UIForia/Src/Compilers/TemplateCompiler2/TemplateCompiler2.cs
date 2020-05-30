using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;
using SlotType = UIForia.Parsing.SlotType;

namespace UIForia.Compilers {

    public class TemplateCompiler2 {

        private State state;

        private SizedArray<AttrInfo> scratchAttributes;

        private SizedArray<AttributeDefinition2> scratch1;
        private SizedArray<AttributeDefinition2> scratch2;

        private readonly AttributeCompiler attributeCompiler;

        private readonly Dictionary<ProcessedType, TemplateExpressionSet> compiledTemplates;

        public event Action<TemplateExpressionSet> onTemplateCompiled;

        private static readonly Dictionary<Type, MethodInfo> s_CreateVariableMethodCache = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> s_ReferenceVariableMethodCache = new Dictionary<Type, MethodInfo>();

        private readonly LightList<State> statePool;
        private readonly StructStack<State> stateStack;
        private readonly AttributeCompilerContext attrCompilerContext;

        private readonly TemplateLinqCompiler typeResolver;
        private readonly StringBuilder stringBuilder;

        public TemplateCompiler2() {
            this.stringBuilder = new StringBuilder();
            this.statePool = new LightList<State>();
            this.stateStack = new StructStack<State>();
            this.attrCompilerContext = new AttributeCompilerContext();
            this.attributeCompiler = new AttributeCompiler(attrCompilerContext);
            this.scratchAttributes = new SizedArray<AttrInfo>(16);
            this.compiledTemplates = new Dictionary<ProcessedType, TemplateExpressionSet>();
            this.typeResolver = new TemplateLinqCompiler(attrCompilerContext);
        }

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

        private TemplateRootNode templateRootNode {
            get => throw new NotImplementedException(); //state.templateRootNode);
        }

        private TemplateASTNode rootNode {
            get => state.templateFile.templateNodes[state.templateRootAST.templateIndex];
        }

        private TemplateFileShell fileShell {
            get => state.templateFile;
        }

        private struct State {

            public ParameterExpression systemParam;
            public TemplateCompilerContext context;
            public TemplateDataBuilder templateDataBuilder;

            public ProcessedType rootProcessedType;

            public LightStack<StructList<BindingVariableDesc>> variableStack;
            public TemplateASTRoot templateRootAST;
            public TemplateFileShell templateFile;

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
            TemplateFileShell templateFileShell = processedType.templateFileShell;

            // match processed type to a template root node
            TemplateASTRoot templateRootAST = templateFileShell.GetRootTemplateForType(processedType);

            if (statePool.size == 0) {
                return new State() {
                    context = new TemplateCompilerContext(),
                    templateDataBuilder = new TemplateDataBuilder(),
                    rootProcessedType = processedType,
                    templateRootAST = templateRootAST,
                    templateFile = templateFileShell,
                    variableStack = new LightStack<StructList<BindingVariableDesc>>()
                };
            }

            State retn = statePool.RemoveLast();
            retn.rootProcessedType = processedType;
            retn.templateRootAST = templateRootAST;
            retn.templateFile = templateFileShell;
            return retn;
        }

        private void PopState() {
            statePool.Add(state);
            state = stateStack.size == 0
                ? default
                : stateStack.Pop();
        }

        public TemplateExpressionSet CompileTemplate(ProcessedType processedType) {

            if (compiledTemplates.TryGetValue(processedType, out TemplateExpressionSet retn)) {
                return retn;
            }

            PushState(processedType);

            CompileEntryPoint(processedType);

            retn = templateDataBuilder.Build(processedType);

            compiledTemplates[processedType] = retn;

            PopState();

            onTemplateCompiled?.Invoke(retn);

            return retn;

        }
        
        private int CompileNode_New(in ConstructedChildData constructedChildData, StructList<AttrInfo> injectedAttributes) {
            context.Setup();

            state.systemParam = context.AddParameter<TemplateSystem>("system");

            TemplateExpressionSet innerTemplate = default;
            ProcessedType processedType = constructedChildData.processedType;
            int templateNodeId = constructedChildData.templateNodeId;
            ref TemplateASTNode templateNode = ref fileShell.templateNodes[templateNodeId];
            TemplateNodeType nodeType = templateNode.templateNodeType;

            scratchAttributes.size = 0;
            attrCompilerContext.Init(fileShell, templateNodeId, processedType, null, state.variableStack);

            switch (nodeType) {

                case TemplateNodeType.SlotDefine: {
                    // SlotNode slotNode = (SlotNode) templateNode;
                    // AttributeMerger.ConvertAttributeDefinitions(slotNode.attributes, ref scratchAttributes);
                    // an override chain stores the contexts of all in-scope root types for a slot
                    templateDataBuilder.CreateSlotOverrideChain(rootProcessedType, new TemplateNodeReference(fileShell, templateNodeId), scratchAttributes, null);
                    // attrCompilerContext.AddContextReference(rootProcessedType, templateRootNode);
                    // attrCompilerContext.AddContextReference(elementType, templateNode);
                    break;
                }

                case TemplateNodeType.SlotOverride: {
                    // SlotNode slotNode = (SlotNode) templateNode;
                    string slotName = fileShell.GetSlotName(templateNodeId);
                    SlotOverrideChain overrideChain = templateDataBuilder.GetSlotOverrideChain(slotName);

                    for (int i = 0; i < overrideChain.chain.Length; i++) {
                     //    attrCompilerContext.AddContextReference(overrideChain.chain[i].rootType, overrideChain.chain[i].slotNode);
                    }

                    // AttributeMerger.MergeSlotAttributes2(ref scratchAttributes, overrideChain);

                    break;
                }

                case TemplateNodeType.Text:
                case TemplateNodeType.TextSpan:
                case TemplateNodeType.Container: {

                    AttributeMerger.ConvertAttributeDefinitions(fileShell, templateNodeId, ref scratchAttributes);

                    // attrCompilerContext.AddContextReference(rootProcessedType, templateRootNode);
                    // attrCompilerContext.AddContextReference(processedType, templateNode);
                    break;
                }

                case TemplateNodeType.Expanded: {
                    // must come before context init since it will overwrite it
                    innerTemplate = CompileTemplate(processedType);

                    // attrCompilerContext.AddContextReference(rootProcessedType, templateRootNode);
                    // attrCompilerContext.AddContextReference(processedType, templateNode);

                    TemplateFileShell fileToExpand = processedType.templateFileShell;
                    TemplateASTRoot rootToExpand = fileToExpand.GetRootTemplateForType(processedType);

                    GatherAttributes(fileToExpand, rootToExpand.templateIndex, ref scratch1);
                    GatherAttributes(fileToExpand, rootToExpand.templateIndex, ref scratch2);

                    AttributeMerger.MergeExpandedAttributes2(scratch1, scratch2, ref scratchAttributes);

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeType), nodeType, null);
            }

            int attrCount = CountRealAttributes(scratchAttributes); // todo inject / inherit needs to account for that here
            int childCount = templateNode.childCount;
            int styleCount = 0; // todo

            StructList<ConstructedChildData> setupChildren = StructList<ConstructedChildData>.Get();

            if (nodeType == TemplateNodeType.Expanded) {
                // replace with call init hydrated? can setup template root context pointers
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeHydratedElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));
                InitializeElementAttributes(scratchAttributes); // todo -- bad dont use scratch here without initializing

                SetupSlotChildren(templateNodeId, innerTemplate, setupChildren);
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_HydrateElement, Expression.Constant(processedType.rawType)));

            }
            else if (nodeType == TemplateNodeType.SlotOverride) {
                context.AddStatement(ExpressionFactory.CallInstance(
                    system,
                    MemberData.TemplateSystem_InitializeSlotElement,
                    ExpressionUtil.GetIntConstant(attrCount),
                    ExpressionUtil.GetIntConstant(childCount),
                    ExpressionUtil.GetIntConstant(attrCompilerContext.references.size)
                ));
                InitializeElementAttributes(scratchAttributes); // todo -- bad dont use scratch here without initializing

                SetupElementChildren_New(system, templateNode, setupChildren);

            }
            else {

                // this statement can be memoized by attr & child count so we don't alloc / validate
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

                if (constructedChildData.processedType.IsTextElement && fileShell.IsTextConstant(templateNodeId)) {
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_SetText, Expression.Constant(GetStringContent(templateNodeId))));
                }

                InitializeElementAttributes(scratchAttributes); // todo -- bad dont use scratch here without initializing

                SetupElementChildren_New(system, templateNode, setupChildren);
            }

            bool alteredStateStack = CompileBindings(processedType, templateNodeId);

            string formattedTagName = fileShell.GetFormattedTagName(constructedChildData.processedType, templateNodeId);

            templateDataBuilder.SetElementTemplate(
                formattedTagName,
                templateNode.GetLineInfo(),
                constructedChildData.outputTemplateIndex,
                context.Build(fileShell.GetRawTagName(templateNodeId))
            );

            StructList<AttrInfo> injectedChildAttributes = StructList<AttrInfo>.Get();

            int elementCount = CompileChildren(setupChildren, injectedAttributes);

            if (alteredStateStack) {
                state.variableStack.Pop().Release();
            }

            injectedChildAttributes.Release();
            setupChildren.Release();
            return elementCount;
        }

        private static void GatherAttributes(TemplateFileShell shell, int templateNodeId, ref SizedArray<AttributeDefinition2> attributes) {
            ref TemplateASTNode templateNode = ref shell.templateNodes[templateNodeId];
            int start = templateNode.attributeRangeStart;
            int end = templateNode.attributeRangeEnd;
            int count = end - start;
            attributes.EnsureCapacity(count);
            int idx = 0;
            for (int i = start; i < count; i++) {
                attributes[idx++] = shell.attributeList[i];
            }
        }

        private string GetStringContent(int templateNodeId) {
            fileShell.TryGetTextContent(templateNodeId, out RangeInt textExpressionRange);
            stringBuilder.Clear();

            for (int i = textExpressionRange.start; i < textExpressionRange.end; i++) {
                stringBuilder.Append(fileShell.textExpressions[i].text);
            }

            return stringBuilder.ToString();
        }

        private void CompileNode(ProcessedType elementType, TemplateNode templateNode, int templateIndex, StructList<AttrInfo> injectedAttributes) {
            context.Setup();

            TemplateExpressionSet innerTemplate = default;

            TemplateNodeType nodeType = default; //GetNodeType(templateNode);
            state.systemParam = context.AddParameter<TemplateSystem>("system");

            scratchAttributes.size = 0;

            switch (nodeType) {

                case TemplateNodeType.SlotDefine: {
                    SlotNode slotNode = (SlotNode) templateNode;
                    AttributeMerger.ConvertAttributeDefinitions(slotNode.attributes, ref scratchAttributes);
                    //templateDataBuilder.CreateSlotOverrideChain(rootProcessedType, slotNode, scratchAttributes, null);
                   // attrCompilerContext.Init(nodeType, elementType, null, state.variableStack);
                    attrCompilerContext.AddContextReference(rootProcessedType, templateRootNode);
                    attrCompilerContext.AddContextReference(elementType, templateNode);
                    break;
                }

                case TemplateNodeType.SlotOverride: {
                    SlotNode slotNode = (SlotNode) templateNode;
                    SlotOverrideChain overrideChain = templateDataBuilder.GetSlotOverrideChain(slotNode.slotName);
                 //   attrCompilerContext.Init(nodeType, elementType, null, state.variableStack);

                    for (int i = 0; i < overrideChain.chain.Length; i++) {
                    //    attrCompilerContext.AddContextReference(overrideChain.chain[i].rootType, overrideChain.chain[i].slotNode);
                    }

                    AttributeMerger.MergeSlotAttributes2(ref scratchAttributes, overrideChain);

                    break;
                }

                case TemplateNodeType.Container: {

                    AttributeMerger.ConvertAttributeDefinitions(templateNode.attributes, ref scratchAttributes);

               //     attrCompilerContext.Init(nodeType, elementType, null, state.variableStack);
                    attrCompilerContext.AddContextReference(rootProcessedType, templateRootNode);
                    attrCompilerContext.AddContextReference(elementType, templateNode);
                    break;
                }

                case TemplateNodeType.Expanded: {
                    // must come before context init since it will overwrite it
                    innerTemplate = CompileTemplate(elementType);

                //    attrCompilerContext.Init(nodeType, elementType, null, state.variableStack);
                    attrCompilerContext.AddContextReference(rootProcessedType, templateRootNode);
                    attrCompilerContext.AddContextReference(elementType, templateNode);

                    TemplateRootNode toExpand = elementType.templateRootNode;

                    AttributeMerger.MergeExpandedAttributes2(templateNode.attributes, toExpand.attributes, ref scratchAttributes);

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeType), nodeType, null);
            }

            int attrCount = CountRealAttributes(scratchAttributes); // todo inject / inherit needs to account for that here
            int childCount = templateNode.children.size;
            int styleCount = 0; // todo

            StructList<ConstructedChildData> setupChildren = StructList<ConstructedChildData>.Get();

            if (nodeType == TemplateNodeType.Expanded) {
                // replace with call init hydrated? can setup template root context pointers
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeHydratedElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));
                InitializeElementAttributes(scratchAttributes); // todo -- bad dont use scratch here without initializing

                SetupSlotChildren(-9999, innerTemplate, setupChildren);
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_HydrateElement, Expression.Constant(elementType.rawType)));

            }
            else if (nodeType == TemplateNodeType.SlotOverride) {
                context.AddStatement(ExpressionFactory.CallInstance(
                    system,
                    MemberData.TemplateSystem_InitializeSlotElement,
                    ExpressionUtil.GetIntConstant(attrCount),
                    ExpressionUtil.GetIntConstant(childCount),
                    ExpressionUtil.GetIntConstant(attrCompilerContext.references.size)
                ));
                InitializeElementAttributes(scratchAttributes); // todo -- bad dont use scratch here without initializing

                SetupElementChildren(system, templateNode.children, ResolveRequiredType(templateNode.requireType), setupChildren);

            }
            else {
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

                if (templateNode is TextNode textNode && textNode.IsTextConstant()) {
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_SetText, Expression.Constant(textNode.GetStringContent())));
                }

                InitializeElementAttributes(scratchAttributes); // todo -- bad dont use scratch here without initializing

                SetupElementChildren(system, templateNode.children, ResolveRequiredType(templateNode.requireType), setupChildren);
            }

            bool alteredStateStack = CompileBindings(templateNode);

            // templateDataBuilder.SetElementTemplate(templateNode, templateIndex, context.Build(templateNode.GetTagName()));

            StructList<AttrInfo> injectedChildAttributes = StructList<AttrInfo>.Get();

            CompileChildren(setupChildren, injectedAttributes);

            if (alteredStateStack) {
                state.variableStack.Pop().Release();
            }

            injectedChildAttributes.Release();
            setupChildren.Release();

        }

        private void SetupSlotChildren(int templateNodeId, TemplateExpressionSet innerTemplate, StructList<ConstructedChildData> setupChildren) {

            return;
            ExpandedNode expandedNode = default;
            // only slot definitions can enforce required types or provide injected attributes
            // injected attribute scope is always the define scope

            // todo -- maybe parser should handle this after all
            if (expandedNode.children.size != 0) {

                SlotOverrideInfo[] overrideChain = innerTemplate.GetSlotOverrideChain("Children");

                if (overrideChain == null) {
                    // todo -- diagnostic
                    Debug.LogError("Cannot find a 'Children' slot to override");
                }
                else {
                    SlotNode implicitOverride = new SlotNode("Children", default, default, default, SlotType.Override) {
                        children = expandedNode.children,
                        parent = expandedNode,
                        root = expandedNode.root,
                        // requireType = declaredChildrenSlot.requireType,
                        // injectedAttributes = declaredChildrenSlot.injectedAttributes
                    };

                    // templateDataBuilder.CreateSlotOverrideChain(rootProcessedType, implicitOverride, scratchAttributes, overrideChain);

                    int idx = templateDataBuilder.GetNextTemplateIndex();
                    // setupChildren.Add(new ConstructedChildData(implicitOverride.processedType, implicitOverride, idx));
                    context.AddStatement(ExpressionFactory.CallInstance(
                            system,
                            MemberData.TemplateSystem_OverrideSlot,
                            ExpressionUtil.GetStringConstant("Children"),
                            ExpressionUtil.GetIntConstant(idx)
                        )
                    );
                }
            }

            for (int i = 0; i < expandedNode.slotOverrideNodes.size; i++) {

                SlotNode overrider = expandedNode.slotOverrideNodes.array[i];

                SlotOverrideInfo[] overrideChain = innerTemplate.GetSlotOverrideChain(overrider.slotName);

                if (overrideChain == null) {
                    // todo -- diagnostic
                    Debug.LogError("Cannot find to slot to override with name : " + overrider.slotName);
                    continue;
                }

                // overrider.requireType = toOverride.requireType; // todo -- ignoring definition of forward/override require types atm

                AttributeMerger.ConvertAttributeDefinitions(overrider.attributes, ref scratchAttributes);

                // templateDataBuilder.CreateSlotOverrideChain(rootProcessedType, overrider, scratchAttributes, overrideChain);

                int idx = templateDataBuilder.GetNextTemplateIndex();

                setupChildren.Add(new ConstructedChildData(overrider.processedType, -9999, idx));

                if (overrider.slotType == SlotType.Override) {
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_OverrideSlot, ExpressionUtil.GetStringConstant(overrider.slotName), ExpressionUtil.GetIntConstant(idx)));
                }
                else if (overrider.slotType == SlotType.Forward) {
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_ForwardSlot, ExpressionUtil.GetStringConstant(overrider.slotName), ExpressionUtil.GetIntConstant(idx)));
                }
                else {
                    // todo -- diagnostics
                    Debug.Log("Invalid slot");
                }

            }

        }

        private Type ResolveRequiredType(string typeExpression) {

            if (string.IsNullOrEmpty(typeExpression)) {
                return null;
            }

            Type requiredType = TypeResolver.Default.ResolveTypeExpression(rootProcessedType.rawType, templateRootNode.templateShell.referencedNamespaces, typeExpression);

            if (requiredType == null) {
                templateRootNode.templateShell.ReportError(default, $"Unable to resolve required child type `{typeExpression}`");
                return null;
            }

            if (!requiredType.IsInterface && !typeof(UIElement).IsAssignableFrom(requiredType)) {
                templateRootNode.templateShell.ReportError(default, $"When requiring an explicit child type, that type must either be an interface or a subclass of UIElement. {requiredType} was neither");
                return null;
            }

            return requiredType;
        }

        private void CompileEntryPoint(ProcessedType processedType) {

            // would be great if this just took in an array and wrote to the indices to 'create' elements
            // would still need to new up the actual class type (but maybe not for primitives like div/text? could those be structs or pooled somehow?)
            // then we have less of a tree to traverse with templates and more of a flat list? 
            // code size goes way up but maybe its much faster to create new elements then
            // at a minimum it would be cool to pre-create all the data that hangs off of elements in bulk, style sets, layout data, attributes
            context.Setup<UIElement>();

            state.systemParam = context.AddParameter<TemplateSystem>("system");
            ParameterExpression elementParam = context.GetVariable(processedType.rawType, "element");

            context.Assign(elementParam, ExpressionFactory.New(processedType.GetConstructor()));

            int attrCount = CountRealAttributes(rootNode);
            int childCount = rootNode.childCount;

            context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_InitializeEntryPoint, elementParam, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

            // AttributeMerger.ConvertAttributeDefinitions(templateRootNode.attributes, ref scratchAttributes);

            // InitializeElementAttributes(scratchAttributes); // can go through and pre-initialize all attributes in template now that we have a flat list

            // attrCompilerContext.Init(templateRootNode, TemplateNodeType.EntryPoint, processedType, null, state.variableStack);
            // attrCompilerContext.AddContextReference(processedType, templateRootNode);

            bool needPop = false; // CompileBindings(templateRootNode);

            context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_HydrateEntryPoint));

            context.AddStatement(elementParam);

            templateDataBuilder.SetEntryPoint(context.Build(processedType.GetType().GetTypeName()));

            int elementCount = CompileHydratePoint(rootNode, processedType);

            if (needPop) {
                state.variableStack.Pop().Release();
            }

        }

        private int CountRealAttributes(TemplateASTNode node) {
            if (node.attributeCount == 0) return 0;
            int count = 0;

            AttributeDefinition2[] attrList = fileShell.attributeList;
            for (int i = node.attributeRangeStart; i < node.attributeRangeEnd; i++) {
                if (attrList[i].type == AttributeType.Attribute) {
                    count++;
                }
            }

            return count;
        }

        private int CompileHydratePoint(TemplateASTNode node, ProcessedType processedType) {

            context.Setup();

            ParameterExpression systemParam = context.AddParameter<TemplateSystem>("system");

            StructList<ConstructedChildData> childrenSetup = StructList<ConstructedChildData>.Get();

            SetupElementChildren_New(systemParam, node, childrenSetup);

            templateDataBuilder.SetHydratePoint(context.Build(processedType.rawType.GetTypeName()));

            int elementCount = CompileChildren(childrenSetup, null);

            childrenSetup.Release();

            return elementCount;

        }

        private int CompileChildren(StructList<ConstructedChildData> childData, StructList<AttrInfo> injectedAttributes) {

            int elementCount = 0;
            
            for (int i = 0; i < childData.size; i++) {
                ref ConstructedChildData data = ref childData.array[i];
                elementCount += CompileNode_New(data, injectedAttributes);
            }

            return childData.size + elementCount;
        }

        private void SetupElementChildren_New(Expression systemParam, in TemplateASTNode parent, StructList<ConstructedChildData> output) {

            int childIndex = parent.firstChildIndex;

            TemplateASTNode[] templateNodes = fileShell.templateNodes;

            Type requiredType = ResolveRequiredType(fileShell.GetRequiredType(parent.index));

            while (childIndex != -1) {
                ref TemplateASTNode child = ref templateNodes[childIndex];
                childIndex = child.nextSiblingIndex;

                ProcessedType childProcessedType = ResolveProcessedType(child.index);

                if (requiredType != null && requiredType.IsAssignableFrom(childProcessedType.rawType)) {
                    // todo -- diagnostics
                    Debug.Log($"type {childProcessedType.rawType} doesnt match required type of {requiredType}");
                    continue;
                }

                int outputIndex = templateDataBuilder.GetNextTemplateIndex();

                output.AddUnsafe(new ConstructedChildData(childProcessedType, child.index, outputIndex));

                Expression childNew = childProcessedType.GetConstructorExpression();

                if ((child.templateNodeType & TemplateNodeType.Slot) != 0) {
                    Assert.IsTrue(child.templateNodeType == TemplateNodeType.SlotDefine);
                    string tagName = fileShell.GetRawTagName(child.index);
                    context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.TemplateSystem_AddSlotChild, childNew, ExpressionUtil.GetStringConstant(tagName), ExpressionUtil.GetIntConstant(outputIndex)));
                }
                else {
                    context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.TemplateSystem_AddChild, childNew, ExpressionUtil.GetIntConstant(outputIndex)));
                }

            }

        }

        private void SetupElementChildren(Expression systemParam, ReadOnlySizedArray<TemplateNode> children, Type requiredType, StructList<ConstructedChildData> output) {

            output.size = 0;
            output.EnsureCapacity(children.size);

            for (int i = 0; i < children.size; i++) {

                TemplateNode child = children.array[i];

                // todo -- do we need to finish merging attributes for this to be 100% correct?
                ProcessedType childProcessedType = ResolveProcessedType(child);

                if (requiredType != null && requiredType.IsAssignableFrom(childProcessedType.rawType)) {
                    // todo -- diagnostics
                    Debug.Log($"type {childProcessedType.rawType} doesnt match required type of {requiredType}");
                    continue;
                }

                int idx = templateDataBuilder.GetNextTemplateIndex();

                output.AddUnsafe(default); //new ConstructedChildData(childProcessedType, idx));

                Expression childNew = childProcessedType.GetConstructorExpression();

                switch (child) {
                    case SlotNode slotNode: {
                        Assert.IsTrue(slotNode.slotType == SlotType.Define);
                        context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.TemplateSystem_AddSlotChild, childNew, ExpressionUtil.GetStringConstant(slotNode.slotName), ExpressionUtil.GetIntConstant(idx)));
                        break;
                    }

                    default: {
                        context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.TemplateSystem_AddChild, childNew, ExpressionUtil.GetIntConstant(idx)));
                        break;
                    }

                }
            }

        }

        private void InitializeElementAttributes(ReadOnlySizedArray<AttrInfo> attributes) {

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

        private bool CompileBindings(ProcessedType processedType, int templateNodeId) {
            return false;
        }

        private bool CompileBindings(TemplateNode node) {

            attributeCompiler.CompileAttributes(scratchAttributes, attrCompilerContext);

            if (attrCompilerContext.bindingResult.HasValue) {

                BindingIndices bindingIds = templateDataBuilder.AddBindings(node, attrCompilerContext.bindingResult);

                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_SetBindings,
                        ExpressionUtil.GetIntConstant(bindingIds.updateIndex),
                        ExpressionUtil.GetIntConstant(bindingIds.lateUpdateIndex),
                        ExpressionUtil.GetIntConstant(bindingIds.constIndex),
                        ExpressionUtil.GetIntConstant(bindingIds.enableIndex),
                        ExpressionUtil.GetIntConstant(attrCompilerContext.bindingResult.localVariables.size)
                    )
                );
            }

            if (attrCompilerContext.bindingResult.inputHandlers.size > 0) {
                bool hasKeyboardEvent = false;

                for (int i = 0; i < attrCompilerContext.bindingResult.inputHandlers.size; i++) {

                    InputHandlerResult handler = attrCompilerContext.bindingResult.inputHandlers.array[i];
                    int idx = templateDataBuilder.GetNextInputHandlerIndex();
                    templateDataBuilder.SetInputHandlerFn(idx, handler);

                    switch (handler.eventClass) {

                        case InputEventClass.Mouse: {
                            MethodCallExpression expression = ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_AddMouseHandler,
                                ExpressionUtil.GetEnumConstant<InputEventType>((int) handler.descriptor.handlerType),
                                ExpressionUtil.GetEnumConstant<KeyboardModifiers>((int) handler.descriptor.modifiers),
                                ExpressionUtil.GetBoolConstant(handler.descriptor.requiresFocus),
                                ExpressionUtil.GetEnumConstant<EventPhase>((int) handler.descriptor.eventPhase),
                                ExpressionUtil.GetIntConstant(idx)
                            );
                            context.AddStatement(expression);
                            break;
                        }

                        case InputEventClass.Keyboard: {
                            hasKeyboardEvent = true;
                            MethodCallExpression expression = ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_AddKeyboardHandler,
                                ExpressionUtil.GetEnumConstant<InputEventType>((int) handler.descriptor.handlerType),
                                ExpressionUtil.GetEnumConstant<KeyboardModifiers>((int) handler.descriptor.modifiers),
                                ExpressionUtil.GetBoolConstant(handler.descriptor.requiresFocus),
                                ExpressionUtil.GetEnumConstant<EventPhase>((int) handler.descriptor.eventPhase),
                                ExpressionUtil.GetEnumConstant<KeyCode>((int) KeyCodeUtil.AnyKey),
                                Expression.Constant('\0'),
                                ExpressionUtil.GetIntConstant(idx)
                            );
                            context.AddStatement(expression);
                            break;
                        }

                        case InputEventClass.Drag: {
                            MethodCallExpression expression = ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_AddDragEventHandler,
                                ExpressionUtil.GetEnumConstant<InputEventType>((int) handler.descriptor.handlerType),
                                ExpressionUtil.GetEnumConstant<KeyboardModifiers>((int) handler.descriptor.modifiers),
                                ExpressionUtil.GetBoolConstant(handler.descriptor.requiresFocus),
                                ExpressionUtil.GetEnumConstant<EventPhase>((int) handler.descriptor.eventPhase),
                                ExpressionUtil.GetIntConstant(idx)
                            );
                            context.AddStatement(expression);
                            break;
                        }

                        case InputEventClass.DragCreate: {
                            MethodCallExpression expression = ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_AddDragCreateHandler,
                                ExpressionUtil.GetEnumConstant<KeyboardModifiers>((int) handler.descriptor.modifiers),
                                ExpressionUtil.GetBoolConstant(handler.descriptor.requiresFocus),
                                ExpressionUtil.GetEnumConstant<EventPhase>((int) handler.descriptor.eventPhase),
                                ExpressionUtil.GetIntConstant(idx)
                            );
                            context.AddStatement(expression);
                            break;
                        }

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                }

                if (hasKeyboardEvent) {
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.TemplateSystem_RegisterForKeyboardEvents));
                }
            }

            if (attrCompilerContext.bindingResult.localVariables.size == 0) return false;

            for (int i = 0; i < attrCompilerContext.bindingResult.localVariables.size; i++) {
                ref BindingVariableDesc localVariable = ref attrCompilerContext.bindingResult.localVariables.array[i];
                if (localVariable.kind == BindingVariableKind.Local) {

                    context.AddStatement(ExpressionFactory.CallInstance(
                        system,
                        GetCreateVariableMethod(localVariable.variableType),
                        ExpressionUtil.GetIntConstant(localVariable.index),
                        ExpressionUtil.GetStringConstant(localVariable.variableName))
                    );

                }
                else if (localVariable.kind == BindingVariableKind.Reference) {
                    context.AddStatement(ExpressionFactory.CallInstance(
                        system,
                        GetReferenceVariableMethod(localVariable.variableType),
                        ExpressionUtil.GetIntConstant(localVariable.index),
                        ExpressionUtil.GetStringConstant(localVariable.variableName))
                    );
                }
            }

            state.variableStack.Push(attrCompilerContext.bindingResult.localVariables);
            attrCompilerContext.bindingResult.localVariables = StructList<BindingVariableDesc>.Get();

            return true;

        }

        private static MethodInfo GetCreateVariableMethod(Type type) {
            if (s_CreateVariableMethodCache.TryGetValue(type, out MethodInfo generic)) {
                return generic;
            }

            generic = MemberData.TemplateSystem_CreateBindingVariable.MakeGenericMethod(type);

            s_CreateVariableMethodCache.Add(type, generic);
            return generic;
        }

        private static MethodInfo GetReferenceVariableMethod(Type type) {
            if (s_ReferenceVariableMethodCache.TryGetValue(type, out MethodInfo generic)) {
                return generic;
            }

            generic = MemberData.TemplateSystem_ReferenceBindingVariable.MakeGenericMethod(type);

            s_ReferenceVariableMethodCache.Add(type, generic);
            return generic;
        }

        private ProcessedType ResolveProcessedType(int nodeIndex) {

            ProcessedType processedType = fileShell.processedTypes[nodeIndex];
            if (processedType == null) {
                return null;
            }
            
            if (!processedType.IsUnresolvedGeneric) {
                return processedType;
            }

            throw new NotImplementedException();
            // return ResolveGenericElementType(fileShell.referencedNamespaces, rootProcessedType.rawType, templateNode);

        }

        private ProcessedType ResolveProcessedType(TemplateNode templateNode) {
            if (!templateNode.processedType.IsUnresolvedGeneric) {
                return templateNode.processedType;
            }

            return ResolveGenericElementType(templateNode.root.templateShell.referencedNamespaces, rootProcessedType.rawType, templateNode);

        }

        private static int CountRealAttributes(ReadOnlySizedArray<AttrInfo> attributes) {
            int count = 0;
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Attribute) {
                    count++;
                }
            }

            return count;
        }

        // todo make new compiler type for type resolver 
        private ProcessedType ResolveGenericElementType(IList<string> namespaces, Type rootType, TemplateNode templateNode) {
            ProcessedType processedType = templateNode.processedType;

            Type generic = processedType.rawType;
            Type[] arguments = processedType.rawType.GetGenericArguments();
            Type[] resolvedTypes = new Type[arguments.Length];

            typeResolver.Init();
            typeResolver.SetSignature(new Parameter(rootType, "__root", ParameterFlags.NeverNull));
            typeResolver.SetImplicitContext(typeResolver.GetParameter("__root"));
            typeResolver.Setup();

            if (templateNode.attributes.array == null) {
                throw TemplateCompileException.UnresolvedGenericElement(processedType, templateNode.TemplateNodeDebugData);
            }

            for (int i = 0; i < templateNode.attributes.size; i++) {
                ref AttributeDefinition attr = ref templateNode.attributes.array[i];

                if (attr.type != AttributeType.Property) continue;

                if (ReflectionUtil.IsField(generic, attr.key, out FieldInfo fieldInfo)) {
                    if (fieldInfo.FieldType.IsGenericParameter || fieldInfo.FieldType.IsGenericType || fieldInfo.FieldType.IsConstructedGenericType) {
                        if (ValidForGenericResolution(fieldInfo.FieldType)) {
                            HandleType(fieldInfo.FieldType, attr);
                        }
                    }
                }
                else if (ReflectionUtil.IsProperty(generic, attr.key, out PropertyInfo propertyInfo)) {
                    if (propertyInfo.PropertyType.IsGenericParameter || propertyInfo.PropertyType.IsGenericType || propertyInfo.PropertyType.IsConstructedGenericType) {
                        HandleType(propertyInfo.PropertyType, attr);
                    }
                }
            }

            for (int i = 0; i < arguments.Length; i++) {
                if (resolvedTypes[i] == null) {
                    throw TemplateCompileException.UnresolvedGenericElement(processedType, templateNode.TemplateNodeDebugData);
                }
            }

            Type newType = ReflectionUtil.CreateGenericType(processedType.rawType, resolvedTypes);
            ProcessedType retn = TypeProcessor.AddResolvedGenericElementType(newType, processedType);
            return retn;

            bool ValidForGenericResolution(Type checkType) {
                if (checkType.IsConstructedGenericType) {
                    Type[] args = checkType.GetGenericArguments();
                    for (int i = 0; i < args.Length; i++) {
                        if (args[i].IsConstructedGenericType) {
                            return false;
                        }
                    }
                }

                return true;
            }

            int GetTypeIndex(Type[] _args, string name) {
                for (int i = 0; i < _args.Length; i++) {
                    if (_args[i].Name == name) {
                        return i;
                    }
                }

                return -1;
            }

            void HandleType(Type inputType, in AttributeDefinition attr) {
                if (!inputType.ContainsGenericParameters) {
                    return;
                }

                if (inputType.IsConstructedGenericType) {
                    if (ReflectionUtil.IsAction(inputType) || ReflectionUtil.IsFunc(inputType)) {
                        return;
                    }

                    Type expressionType = typeResolver.GetExpressionType(attr.value);

                    Type[] typeArgs = expressionType.GetGenericArguments();
                    Type[] memberGenericArgs = inputType.GetGenericArguments();

                    Assert.AreEqual(memberGenericArgs.Length, typeArgs.Length);

                    for (int a = 0; a < memberGenericArgs.Length; a++) {
                        string genericName = memberGenericArgs[a].Name;
                        int typeIndex = GetTypeIndex(arguments, genericName);

                        if (typeIndex == -1) {
                            throw new TemplateCompileException(templateNode.TemplateNodeDebugData.tagName + templateNode.TemplateNodeDebugData.lineInfo);
                        }

                        Assert.IsTrue(typeIndex != -1);

                        if (resolvedTypes[typeIndex] != null) {
                            if (resolvedTypes[typeIndex] != typeArgs[a]) {
                                throw TemplateCompileException.DuplicateResolvedGenericArgument(templateNode.GetTagName(), inputType.Name, resolvedTypes[typeIndex], typeArgs[a]);
                            }
                        }

                        resolvedTypes[typeIndex] = typeArgs[a];
                    }
                }
                else {
                    string genericName = inputType.Name;
                    int typeIndex = GetTypeIndex(arguments, genericName);
                    Assert.IsTrue(typeIndex != -1);

                    Type type = typeResolver.GetExpressionType(attr.value);
                    if (resolvedTypes[typeIndex] != null) {
                        if (resolvedTypes[typeIndex] != type) {
                            throw TemplateCompileException.DuplicateResolvedGenericArgument(templateNode.GetTagName(), inputType.Name, resolvedTypes[typeIndex], type);
                        }
                    }

                    resolvedTypes[typeIndex] = type;
                }
            }
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

    }

    public enum BindingVariableKind {

        Local,
        Reference

    }
    
    public struct TemplateNodeReference {

        public readonly TemplateFileShell fileShell;
        public readonly int templateNodeId;

        public TemplateNodeReference(TemplateFileShell fileShell, int templateNodeId) {
            this.fileShell = fileShell;
            this.templateNodeId = templateNodeId;
        }
            
    }

}

// private void CompileSlotDefine(SlotNode slotNode, int slotId) {
//             
// AttributeMerger.ConvertAttributeDefinitions(slotNode.attributes, ref scratchAttributes);
//
// InitializeElementAttributes(scratchAttributes);
//
// int startIdx = SetupChildren(system, slotNode.children);
//
// CompileBindings(TemplateNodeType.SlotDefine, system, slotNode.processedType, slotNode);
//
// templateDataBuilder.SetElementTemplate(slotNode, slotId, context.Build(slotNode.GetTagName()));
//
// CompileChildren(startIdx, slotNode.children);
//
// }
//
// private void CompileSlotOverride(SlotNode slotNode, int slotId) {
// context.Setup();
//
// ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");
//
// int attrCount = slotNode.CountRealAttributes();
// int childCount = slotNode.children.size;
// int styleCount = 0; // todo
//
// context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));
// AttributeMerger.ConvertAttributeDefinitions(slotNode.attributes, ref scratchAttributes);
//
// InitializeElementAttributes(scratchAttributes);
//
// int startIdx = SetupChildren(systemParam, slotNode.children, ResolveRequiredType(slotNode.requireType));
//
// CompileBindings(TemplateNodeType.SlotDefine, systemParam, slotNode.processedType, slotNode);
//
// templateDataBuilder.SetElementTemplate(slotNode, slotId, context.Build(slotNode.GetTagName()));
//
// CompileChildren(startIdx, slotNode.children);
// }

// private void CompileContainerElement(ContainerNode containerNode, int templateId, StructList<AttrInfo> injectedAttributes) {
//          context.Setup();
//
//          ProcessedType processedType = ResolveProcessedType(containerNode);
//
//          int attrCount = containerNode.CountRealAttributes();
//          int childCount = containerNode.children.size;
//          int styleCount = 0; // todo
//
//          context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));
//
//          AttributeMerger.ConvertAttributeDefinitions(containerNode.attributes, ref scratchAttributes);
//
//          AttributeMerger.InjectAttributes(ref scratchAttributes, injectedAttributes);
//
//          InitializeElementAttributes(scratchAttributes);
//
//          int startIdx = SetupChildren(system, containerNode.children);
//
//          CompileBindings(TemplateNodeType.Standard, system, processedType, containerNode);
//
//          templateDataBuilder.SetElementTemplate(containerNode, templateId, context.Build(containerNode.GetTagName()));
//
//          CompileChildren(startIdx, containerNode.children);
//
//      }
//
//      private void CompileTextElement(TextNode textNode, int templateId) {
//
//          context.Setup();
//
//          ProcessedType processedType = ResolveProcessedType(textNode);
//
//          ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");
//
//          int attrCount = textNode.CountRealAttributes();
//          int childCount = textNode.children.size;
//          int styleCount = 0; // todo
//
//          context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));
//
//          AttributeMerger.ConvertAttributeDefinitions(textNode.attributes, ref scratchAttributes);
//
//          InitializeElementAttributes(scratchAttributes);
//
//          if (textNode.IsTextConstant()) {
//              context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.ElementSystem_SetText, Expression.Constant(textNode.GetStringContent())));
//          }
//
//          int startIdx = SetupChildren(systemParam, textNode.children);
//
//          CompileBindings(TemplateNodeType.Standard, systemParam, processedType, textNode);
//
//          templateDataBuilder.SetElementTemplate(textNode, templateId, context.Build(textNode.GetTagName()));
//
//          CompileChildren(startIdx, textNode.children);
//      } private void CompileExpandedNode(ExpandedNode expandedNode, int templateId) {
//
//     ProcessedType processedType = ResolveProcessedType(expandedNode);
//
//     TemplateExpressionSet innerTemplate = CompileTemplate(processedType);
//
//     context.Setup();
//
//     ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");
//
//     TemplateRootNode toExpand = processedType.templateRootNode;
//
//     AttributeMerger.MergeExpandedAttributes2(expandedNode.attributes, toExpand.attributes, ref scratchAttributes);
//
//     int attrCount = CountRealAttributes(scratchAttributes);
//
//     int childCount = toExpand.children.size;
//     int styleCount = 0; // todo
//
//     context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));
//
//     // setup slot overrides
//
//     SizedArray<TemplateNode> compileList = new SizedArray<TemplateNode>();
//
//     int startIdx = templateDataBuilder.templateIndex;
//
//     // only slot definitions can enforce required types or provide injected attributes
//     // injected attribute scope is always the define scope
//
//     if (expandedNode.children.size != 0) {
//         if (!toExpand.TryGetSlotNode("Children", out SlotNode declaredChildrenSlot)) {
//             // todo -- diagnostic
//             Debug.LogError("Cannot find a 'Children' slot to override");
//         }
//         else {
//             SlotNode implicitOverride = new SlotNode("Children", default, default, default, SlotType.Override) {
//                 children = expandedNode.children,
//                 parent = expandedNode,
//                 root = expandedNode.root,
//                 requireType = declaredChildrenSlot.requireType,
//                 injectedAttributes = declaredChildrenSlot.injectedAttributes
//             };
//
//             compileList.Add(implicitOverride);
//             context.AddStatement(ExpressionFactory.CallInstance(
//                 systemParam,
//                 MemberData.ElementSystem_OverrideSlot,
//                 ExpressionUtil.GetStringConstant("Children"),
//                 ExpressionUtil.GetIntConstant(templateDataBuilder.templateIndex++))
//             );
//         }
//     }
//
//     for (int i = 0; i < expandedNode.slotOverrideNodes.size; i++) {
//
//         SlotNode overrider = expandedNode.slotOverrideNodes.array[i];
//
//         if (!toExpand.TryGetSlotNode(overrider.slotName, out SlotNode toOverride)) {
//             // todo -- diagnostic
//             Debug.LogError("Cannot find to slot to override with name : " + overrider.slotName);
//             continue;
//         }
//
//         overrider.requireType = toOverride.requireType; // todo -- ignoring definition of forward/override require types atm
//
//         AttributeMerger.ConvertAttributeDefinitions(overrider.attributes, ref scratchAttributes);
//
//         // templateDataBuilder.CreateSlotOverrideChain(overrider.slotName, rootProcessedType, scratchAttributes, innerTemplate.GetSlotOverrideChain(overrider.slotName));
//
//         if (overrider.slotType == SlotType.Override) {
//             context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.ElementSystem_OverrideSlot, ExpressionUtil.GetStringConstant(overrider.slotName), ExpressionUtil.GetIntConstant(templateDataBuilder.templateIndex++)));
//             compileList.Add(overrider);
//         }
//         else if (overrider.slotType == SlotType.Forward) {
//             context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.ElementSystem_ForwardSlot, ExpressionUtil.GetStringConstant(overrider.slotName), ExpressionUtil.GetIntConstant(templateDataBuilder.templateIndex++)));
//             compileList.Add(overrider);
//         }
//         else {
//             // todo -- diagnostics
//             Debug.Log("Invalid slot");
//         }
//
//     }
//
//     InitializeElementAttributes(scratchAttributes);
//
//     // for each attribute I need to know where it came from, both the type and possibly index in context array
//     // for container and text elements depth is always 1 and points to the root context
//     // for expanded elements depth is always 2 and points to root and 'self'
//     // for slot elements depth is n (at least 2) and points to the last n contexts were n == forward levels + 2
//
//     // now I need to know the context stack at compile time...which we dont have atm
//     // need to traverse down the slot hierarchy until we the <define> for the slot case
//
//     CompileBindings(expandedNode);
//
//     context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.ElementSystem_HydrateElement, Expression.Constant(processedType.rawType)));
//
//     templateDataBuilder.SetElementTemplate(expandedNode, templateId, context.Build(expandedNode.GetTagName()));
//
//     // CompileChildren(startIdx, compileList);
//
// }