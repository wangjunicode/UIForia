using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;
using SlotType = UIForia.Parsing.SlotType;

namespace UIForia.Compilers {

    public class TemplateCompiler2 {

        private State state;

        private SizedArray<AttrInfo> scratchAttributes;
        private readonly AttributeCompiler attributeCompiler;
        private StructList<BindingVariableDesc> localVariableList;

        private SizedArray<TemplateContextReference> scratchContextReferences;
        private readonly Dictionary<ProcessedType, TemplateExpressionSet> compiledTemplates;

        public event Action<TemplateExpressionSet> onTemplateCompiled;

        private static readonly Dictionary<Type, MethodInfo> s_CreateVariableMethodCache = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> s_ReferenceVariableMethodCache = new Dictionary<Type, MethodInfo>();

        private readonly LightList<State> statePool;
        private readonly StructStack<State> stateStack;

        public TemplateCompiler2() {
            this.statePool = new LightList<State>();
            this.stateStack = new StructStack<State>();
            this.attributeCompiler = new AttributeCompiler();
            this.scratchAttributes = new SizedArray<AttrInfo>(16);
            this.scratchContextReferences = new SizedArray<TemplateContextReference>(8);
            this.compiledTemplates = new Dictionary<ProcessedType, TemplateExpressionSet>();
            this.localVariableList = new StructList<BindingVariableDesc>(16);
        }

        private CompilationContext2 context {
            get => state.context;
        }

        private TemplateDataBuilder templateDataBuilder {
            get => state.templateDataBuilder;
        }

        private ProcessedType rootProcessedType {
            get => state.rootProcessedType;
        }

        private TemplateRootNode templateRootNode {
            get => state.templateRootNode;
        }

        private struct State {

            public CompilationContext2 context;
            public TemplateDataBuilder templateDataBuilder;
            public ProcessedType rootProcessedType;
            public TemplateRootNode templateRootNode;
            public LightStack<StructList<BindingVariableDesc>> variableStack;

        }

        private void PushState(ProcessedType processedType, TemplateRootNode rootNode) {
            if (state.context == null) {
                state = GetState(processedType, rootNode);
            }
            else {
                stateStack.Push(state);
                state = GetState(processedType, rootNode);
            }
        }

        private State GetState(ProcessedType processedType, TemplateRootNode rootNode) {
            if (statePool.size == 0) {
                return new State() {
                    context = new CompilationContext2(),
                    templateDataBuilder = new TemplateDataBuilder(),
                    rootProcessedType = processedType,
                    templateRootNode = rootNode,
                    variableStack = new LightStack<StructList<BindingVariableDesc>>()
                };
            }

            State retn = statePool.RemoveLast();
            retn.rootProcessedType = processedType;
            retn.templateRootNode = rootNode;
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

            PushState(processedType, processedType.templateRootNode);

            CompileEntryPoint(processedType);

            CompileHydratePoint(processedType.templateRootNode);

            retn = templateDataBuilder.Build(processedType);

            compiledTemplates[processedType] = retn;

            PopState();

            onTemplateCompiled?.Invoke(retn);

            return retn;

        }

        private void CompileNode(TemplateNode target, int index) {
            switch (target) {
                case ContainerNode containerNode:
                    CompileContainerElement(containerNode, index);
                    break;

                case TextNode textNode:
                    CompileTextElement(textNode, index);
                    break;

                case ExpandedNode expandedNode:
                    CompileExpandedNode(expandedNode, index);
                    break;

                case SlotNode slotNode:
                    CompileSlotNode(slotNode, index);
                    break;

                case RepeatNode repeatNode:
                default:
                    return;
            }
        }

        private void CompileSlotNode(SlotNode slotNode, int index) {
            switch (slotNode.slotType) {

                case SlotType.Define:
                    CompileSlotDefine(slotNode, index);
                    break;

                case SlotType.Forward:
                    CompileSlotForward(slotNode, index);
                    break;

                case SlotType.Override:
                    CompileSlotOverride(slotNode, index);
                    break;

                case SlotType.Template:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CompileSlotDefine(SlotNode slotNode, int slotId) {
            context.Setup();

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            int attrCount = slotNode.CountRealAttributes();
            int childCount = slotNode.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

            AttributeMerger.ConvertAttributeDefinitions(slotNode.attributes, ref scratchAttributes);
            InitializeElementAttributes(systemParam, scratchAttributes);

            SetupChildren(systemParam, slotNode.children);

            templateDataBuilder.SetElementTemplate(slotNode, slotId, context.Build(slotNode.GetTagName()));

        }

        private void CompileSlotOverride(SlotNode slotNode, int slotId) {
            context.Setup();

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            int attrCount = slotNode.CountRealAttributes();
            int childCount = slotNode.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));
            AttributeMerger.ConvertAttributeDefinitions(slotNode.attributes, ref scratchAttributes);

            InitializeElementAttributes(systemParam, scratchAttributes);
            SetupChildren(systemParam, slotNode.children);

            templateDataBuilder.SetElementTemplate(slotNode, slotId, context.Build(slotNode.GetTagName()));
        }

        private void CompileSlotForward(SlotNode slotNode, int slotId) { }

        private void CompileEntryPoint(ProcessedType processedType) {

            context.Setup<UIElement>();

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");
            ParameterExpression elementParam = context.GetVariable(processedType.rawType, "element");

            context.Assign(elementParam, ExpressionFactory.New(processedType.GetConstructor()));

            int attrCount = templateRootNode.CountRealAttributes();
            int childCount = templateRootNode.ChildCount;

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeEntryPoint, elementParam, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));
            AttributeMerger.ConvertAttributeDefinitions(templateRootNode.attributes, ref scratchAttributes);
            InitializeElementAttributes(systemParam, scratchAttributes);

            scratchContextReferences.Clear();
            scratchContextReferences.Add(new TemplateContextReference(processedType, templateRootNode));

            CompileBindings(ElementBindingType.EntryPoint, systemParam, processedType, templateRootNode, scratchContextReferences);

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_HydrateEntryPoint));

            context.AddStatement(elementParam);

            templateDataBuilder.SetEntryPoint(context.Build(processedType.GetType().GetTypeName()));

        }

        private void CompileHydratePoint(TemplateRootNode templateRootNode) {

            context.Setup();

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            int startIdx = SetupChildren(systemParam, templateRootNode.children);

            templateDataBuilder.SetHydratePoint(context.Build(templateRootNode.GetType().GetTypeName()));

            CompileChildren(startIdx, templateRootNode.children);

        }

        private void CompileChildren(int templateStartIndex, SizedArray<TemplateNode> children) {

            bool needsPop = localVariableList.size != 0;

            if (needsPop) {
                state.variableStack.Push(localVariableList);
                localVariableList = StructList<BindingVariableDesc>.Get();
            }

            for (int i = 0; i < children.size; i++) {
                CompileNode(children.array[i], templateStartIndex + i);
            }

            if (needsPop) {
                StructList<BindingVariableDesc> list = state.variableStack.Pop();
                list.Release();
            }
        }

        private ProcessedType ResolveProcessedType(TemplateNode templateNode) {
            if (!templateNode.processedType.IsUnresolvedGeneric) {
                return templateNode.processedType;
            }

            throw new NotImplementedException("Resolve generic type");

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

        private void CompileExpandedNode(ExpandedNode expandedNode, int templateId) {

            ProcessedType processedType = ResolveProcessedType(expandedNode);

            TemplateExpressionSet innerTemplate = CompileTemplate(processedType);

            context.Setup();

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            TemplateRootNode toExpand = processedType.templateRootNode;

            AttributeMerger.MergeExpandedAttributes2(expandedNode.attributes, toExpand.attributes, ref scratchAttributes);

            int attrCount = CountRealAttributes(scratchAttributes);

            int childCount = toExpand.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

            // setup slot overrides

            for (int i = 0; i < expandedNode.slotOverrideNodes.size; i++) {

                SlotNode overrider = expandedNode.slotOverrideNodes.array[i];

                if (!toExpand.TryGetSlotNode(overrider.slotName, out SlotNode _)) {
                    // todo -- diagnostic
                    Debug.LogError("Cannot find to slot to override with name : " + overrider.slotName);
                    continue;
                }

                if (overrider.slotType == SlotType.Override) {
                    context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_OverrideSlot, ExpressionUtil.GetStringConstant(overrider.slotName), ExpressionUtil.GetIntConstant(i)));
                }
                else if (overrider.slotType == SlotType.Forward) {
                    context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_ForwardSlot, ExpressionUtil.GetStringConstant(overrider.slotName), ExpressionUtil.GetIntConstant(i)));
                }
                else {
                    // todo -- diagnostics
                    Debug.Log("Invalid slot");
                }

            }

            InitializeElementAttributes(systemParam, scratchAttributes);

            // for each attribute I need to know where it came from, both the type and possibly index in context array
            // for container and text elements depth is always 1 and points to the root context
            // for expanded elements depth is always 2 and points to root and 'self'
            // for slot elements depth is n (at least 2) and points to the last n contexts were n == forward levels + 2

            // now I need to know the context stack at compile time...which we dont have atm
            // need to traverse down the slot hierarchy until we the <define> for the slot case

            scratchContextReferences.Clear();
            scratchContextReferences.Add(new TemplateContextReference(rootProcessedType, templateRootNode));
            scratchContextReferences.Add(new TemplateContextReference(expandedNode.processedType, expandedNode));

            CompileBindings(ElementBindingType.Expanded, systemParam, processedType, expandedNode, scratchContextReferences);

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_HydrateElement, Expression.Constant(processedType.rawType)));

            templateDataBuilder.SetElementTemplate(expandedNode, templateId, context.Build(expandedNode.GetTagName()));

        }

        private void CompileBindings(ElementBindingType elementBindingType, Expression systemParam, ProcessedType processedType, TemplateNode node, ReadOnlySizedArray<TemplateContextReference> contextReferences) {
            AttributeSet attributeSet = new AttributeSet(scratchAttributes, elementBindingType, contextReferences);

            BindingResult bindingResult = new BindingResult();

            localVariableList.size = 0;
            bindingResult.localVariables = localVariableList;

            attributeCompiler.CompileAttributes(processedType, node, attributeSet, ref bindingResult, state.variableStack);

            if (!bindingResult.HasValue) {
                return;
            }

            BindingIndices bindingIds = templateDataBuilder.AddBindings(node, bindingResult);

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_SetBindings,
                    ExpressionUtil.GetIntConstant(bindingIds.updateIndex),
                    ExpressionUtil.GetIntConstant(bindingIds.lateUpdateIndex),
                    ExpressionUtil.GetIntConstant(bindingIds.constIndex),
                    ExpressionUtil.GetIntConstant(bindingIds.enableIndex),
                    ExpressionUtil.GetIntConstant(localVariableList.size)
                )
            );

            if (localVariableList.size == 0) return;

            for (int i = 0; i < localVariableList.size; i++) {
                ref BindingVariableDesc localVariable = ref localVariableList.array[i];
                if (localVariable.kind == BindingVariableKind.Local) {

                    context.AddStatement(ExpressionFactory.CallInstanceUnchecked(
                        systemParam,
                        GetCreateVariableMethod(localVariable.variableType),
                        ExpressionUtil.GetIntConstant(localVariable.index),
                        ExpressionUtil.GetStringConstant(localVariable.variableName))
                    );
                    
                }
                else if (localVariable.kind == BindingVariableKind.Reference) {
                    context.AddStatement(ExpressionFactory.CallInstanceUnchecked(
                        systemParam,
                        GetReferenceVariableMethod(localVariable.variableType),
                        ExpressionUtil.GetIntConstant(localVariable.index),
                        ExpressionUtil.GetStringConstant(localVariable.variableName))
                    );
                }
            }

        }

        private static MethodInfo GetCreateVariableMethod(Type type) {
            if (s_CreateVariableMethodCache.TryGetValue(type, out MethodInfo generic)) {
                return generic;
            }

            generic = MemberData.ElementSystem_CreateBindingVariable.MakeGenericMethod(type);

            s_CreateVariableMethodCache.Add(type, generic);
            return generic;
        }
        
        private static MethodInfo GetReferenceVariableMethod(Type type) {
            if (s_ReferenceVariableMethodCache.TryGetValue(type, out MethodInfo generic)) {
                return generic;
            }

            generic = MemberData.ElementSystem_ReferenceBindingVariable.MakeGenericMethod(type);

            s_ReferenceVariableMethodCache.Add(type, generic);
            return generic;
        }

        private void CompileContainerElement(ContainerNode containerNode, int templateId) {
            context.Setup();

            ProcessedType processedType = ResolveProcessedType(containerNode);

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            int attrCount = containerNode.CountRealAttributes();
            int childCount = containerNode.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

            AttributeMerger.ConvertAttributeDefinitions(containerNode.attributes, ref scratchAttributes);

            InitializeElementAttributes(systemParam, scratchAttributes);

            int startIdx = SetupChildren(systemParam, containerNode.children);

            scratchContextReferences.Clear();
            scratchContextReferences.Add(new TemplateContextReference(rootProcessedType, templateRootNode));
            scratchContextReferences.Add(new TemplateContextReference(containerNode.processedType, containerNode));

            CompileBindings(ElementBindingType.Expanded, systemParam, processedType, containerNode, scratchContextReferences);

            templateDataBuilder.SetElementTemplate(containerNode, templateId, context.Build(containerNode.GetTagName()));

            CompileChildren(startIdx, containerNode.children);

        }

        private void CompileTextElement(TextNode textNode, int templateId) {

            context.Setup();

            ProcessedType processedType = ResolveProcessedType(textNode);

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            int attrCount = textNode.CountRealAttributes();
            int childCount = textNode.children.size;
            int styleCount = 0; // todo

            context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

            AttributeMerger.ConvertAttributeDefinitions(textNode.attributes, ref scratchAttributes);

            InitializeElementAttributes(systemParam, scratchAttributes);

            if (textNode.IsTextConstant()) {
                context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_SetText, Expression.Constant(textNode.GetStringContent())));
            }

            int startIdx = SetupChildren(systemParam, textNode.children);

            scratchContextReferences.Clear();
            scratchContextReferences.Add(new TemplateContextReference(rootProcessedType, templateRootNode));
            scratchContextReferences.Add(new TemplateContextReference(textNode.processedType, textNode));

            CompileBindings(ElementBindingType.Expanded, systemParam, processedType, textNode, scratchContextReferences);

            templateDataBuilder.SetElementTemplate(textNode, templateId, context.Build(textNode.GetTagName()));

            CompileChildren(startIdx, textNode.children);
        }

        private int SetupChildren(Expression systemParam, ReadOnlySizedArray<TemplateNode> children) {

            int startIdx = templateDataBuilder.templateIndex;

            for (int i = 0; i < children.size; i++) {

                TemplateNode child = children.array[i];

                Expression childNew = child.processedType.GetConstructorExpression();

                int idx = templateDataBuilder.GetNextTemplateIndex();

                switch (child) {
                    case SlotNode slotNode: {
                        Assert.IsTrue(slotNode.slotType == SlotType.Define);
                        context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_AddSlotChild, childNew, ExpressionUtil.GetStringConstant(slotNode.slotName), ExpressionUtil.GetIntConstant(idx)));
                        break;
                    }

                    default: {
                        context.AddStatement(ExpressionFactory.CallInstanceUnchecked(systemParam, MemberData.ElementSystem_AddChild, childNew, ExpressionUtil.GetIntConstant(idx)));
                        break;
                    }

                }
            }

            return startIdx;
        }

        private void InitializeElementAttributes(Expression system, ReadOnlySizedArray<AttrInfo> attributes) {

            for (int i = 0; i < attributes.size; i++) {
                ref AttrInfo attr = ref attributes.array[i];

                if (attr.type != AttributeType.Attribute) {
                    continue;
                }

                context.AddStatement((attr.flags & AttributeFlags.Const) != 0
                    ? ExpressionFactory.CallInstanceUnchecked(system, MemberData.ElementSystem_InitializeStaticAttribute, ExpressionUtil.GetStringConstant(attr.key), ExpressionUtil.GetStringConstant(attr.value))
                    : ExpressionFactory.CallInstanceUnchecked(system, MemberData.ElementSystem_InitializeDynamicAttribute, ExpressionUtil.GetStringConstant(attr.key)));
            }

        }

    }

    public enum BindingVariableKind {

        Local,
        Reference

    }

}