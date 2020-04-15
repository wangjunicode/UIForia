using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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
        private readonly AttributeCompiler attributeCompiler;

        private readonly Dictionary<ProcessedType, TemplateExpressionSet> compiledTemplates;

        public event Action<TemplateExpressionSet> onTemplateCompiled;

        private static readonly Dictionary<Type, MethodInfo> s_CreateVariableMethodCache = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> s_ReferenceVariableMethodCache = new Dictionary<Type, MethodInfo>();

        private readonly LightList<State> statePool;
        private readonly StructStack<State> stateStack;
        private readonly AttributeCompilerContext attrCompilerContext;

        private readonly TemplateLinqCompiler typeResolver;

        public TemplateCompiler2() {
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
            get => state.templateRootNode;
        }

        private struct State {

            public ParameterExpression systemParam;
            public TemplateCompilerContext context;
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
                    context = new TemplateCompilerContext(),
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

            retn = templateDataBuilder.Build(processedType);

            compiledTemplates[processedType] = retn;

            PopState();

            onTemplateCompiled?.Invoke(retn);

            return retn;

        }

        private static TemplateNodeType GetNodeType(TemplateNode templateNode) {
            switch (templateNode) {
                case ContainerNode containerNode:
                case TextNode textNode:
                    return TemplateNodeType.Standard;

                case ExpandedNode expandedNode:
                    return TemplateNodeType.Expanded;

                case SlotNode slotNode: {
                    if (slotNode.slotType == SlotType.Define) {
                        return TemplateNodeType.SlotDefine;
                    }

                    return TemplateNodeType.SlotOverride;
                }

            }

            return default;
        }

        // resolve type
        // gather / handle attributes (inherit / inject)
        // init attribute array
        // maybe init style array
        // compile bindings
        // call initialize if not root
        // emit const text binding if present
        // push injected attributes if present
        // gather / setup children (validate slots if any, validate required type acceptable)
        // call hydrate if needed
        // write to template index
        // compile children
        // pop injected attributes if any

        private void CompileNode(ProcessedType elementType, TemplateNode templateNode, int templateIndex, StructList<AttrInfo> injectedAttributes) {
            context.Setup();

            TemplateExpressionSet innerTemplate = default;

            TemplateNodeType nodeType = GetNodeType(templateNode);
            state.systemParam = context.AddParameter<ElementSystem>("system");

            scratchAttributes.size = 0;

            switch (nodeType) {

                case TemplateNodeType.SlotDefine: {
                    SlotNode slotNode = (SlotNode) templateNode;
                    AttributeMerger.ConvertAttributeDefinitions(slotNode.attributes, ref scratchAttributes);
                    templateDataBuilder.CreateSlotOverrideChain(rootProcessedType, slotNode, scratchAttributes, null);
                    attrCompilerContext.Init(templateNode, nodeType, elementType, null, state.variableStack);
                    attrCompilerContext.AddContextReference(rootProcessedType, templateRootNode);
                    attrCompilerContext.AddContextReference(elementType, templateNode);
                    break;
                }

                case TemplateNodeType.SlotOverride: {
                    SlotNode slotNode = (SlotNode) templateNode;
                    SlotOverrideChain overrideChain = templateDataBuilder.GetSlotOverrideChain(slotNode.slotName);
                    attrCompilerContext.Init(templateNode, nodeType, elementType, null, state.variableStack);

                    for (int i = 0; i < overrideChain.chain.Length; i++) {
                        attrCompilerContext.AddContextReference(overrideChain.chain[i].rootType, overrideChain.chain[i].slotNode);
                    }

                    AttributeMerger.MergeSlotAttributes2(ref scratchAttributes, overrideChain);

                    break;
                }

                case TemplateNodeType.Standard: {

                    AttributeMerger.ConvertAttributeDefinitions(templateNode.attributes, ref scratchAttributes);

                    attrCompilerContext.Init(templateNode, nodeType, elementType, null, state.variableStack);
                    attrCompilerContext.AddContextReference(rootProcessedType, templateRootNode);
                    attrCompilerContext.AddContextReference(elementType, templateNode);
                    break;
                }

                case TemplateNodeType.Expanded: {
                    // must come before context init since it will overwrite it
                    innerTemplate = CompileTemplate(elementType);

                    attrCompilerContext.Init(templateNode, nodeType, elementType, null, state.variableStack);
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
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.ElementSystem_InitializeHydratedElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));
                InitializeElementAttributes(scratchAttributes); // todo -- bad dont use scratch here without initializing

                SetupSlotChildren(templateNode as ExpandedNode, innerTemplate, setupChildren);
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.ElementSystem_HydrateElement, Expression.Constant(elementType.rawType)));

            }
            else if (nodeType == TemplateNodeType.SlotOverride) {
                context.AddStatement(ExpressionFactory.CallInstance(
                    system,
                    MemberData.ElementSystem_InitializeSlotElement,
                    ExpressionUtil.GetIntConstant(attrCount),
                    ExpressionUtil.GetIntConstant(childCount),
                    ExpressionUtil.GetIntConstant(attrCompilerContext.rootVariables.size)
                ));
                InitializeElementAttributes(scratchAttributes); // todo -- bad dont use scratch here without initializing

                SetupElementChildren(system, templateNode.children, ResolveRequiredType(templateNode.requireType), setupChildren);

            }
            else {
                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.ElementSystem_InitializeElement, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

                if (templateNode is TextNode textNode && textNode.IsTextConstant()) {
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.ElementSystem_SetText, Expression.Constant(textNode.GetStringContent())));
                }

                InitializeElementAttributes(scratchAttributes); // todo -- bad dont use scratch here without initializing

                SetupElementChildren(system, templateNode.children, ResolveRequiredType(templateNode.requireType), setupChildren);
            }

            bool alteredStateStack = CompileBindings(templateNode);

            templateDataBuilder.SetElementTemplate(templateNode, templateIndex, context.Build(templateNode.GetTagName()));

            StructList<AttrInfo> injectedChildAttributes = StructList<AttrInfo>.Get();

            CompileChildren(setupChildren, injectedAttributes);

            if (alteredStateStack) {
                state.variableStack.Pop().Release();
            }

            injectedChildAttributes.Release();
            setupChildren.Release();

        }

        private void SetupSlotChildren(ExpandedNode expandedNode, TemplateExpressionSet innerTemplate, StructList<ConstructedChildData> setupChildren) {

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

                    templateDataBuilder.CreateSlotOverrideChain(rootProcessedType, implicitOverride, scratchAttributes, overrideChain);

                    int idx = templateDataBuilder.GetNextTemplateIndex();
                    setupChildren.Add(new ConstructedChildData(implicitOverride.processedType, implicitOverride, idx));
                    context.AddStatement(ExpressionFactory.CallInstance(
                            system,
                            MemberData.ElementSystem_OverrideSlot,
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

                templateDataBuilder.CreateSlotOverrideChain(rootProcessedType, overrider, scratchAttributes, overrideChain);

                int idx = templateDataBuilder.GetNextTemplateIndex();

                setupChildren.Add(new ConstructedChildData(overrider.processedType, overrider, idx));

                if (overrider.slotType == SlotType.Override) {
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.ElementSystem_OverrideSlot, ExpressionUtil.GetStringConstant(overrider.slotName), ExpressionUtil.GetIntConstant(idx)));
                }
                else if (overrider.slotType == SlotType.Forward) {
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.ElementSystem_ForwardSlot, ExpressionUtil.GetStringConstant(overrider.slotName), ExpressionUtil.GetIntConstant(idx)));
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

            context.Setup<UIElement>();

            state.systemParam = context.AddParameter<ElementSystem>("system");
            ParameterExpression elementParam = context.GetVariable(processedType.rawType, "element");

            context.Assign(elementParam, ExpressionFactory.New(processedType.GetConstructor()));

            int attrCount = templateRootNode.CountRealAttributes();
            int childCount = templateRootNode.ChildCount;

            context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.ElementSystem_InitializeEntryPoint, elementParam, ExpressionUtil.GetIntConstant(attrCount), ExpressionUtil.GetIntConstant(childCount)));

            AttributeMerger.ConvertAttributeDefinitions(templateRootNode.attributes, ref scratchAttributes);

            InitializeElementAttributes(scratchAttributes);

            attrCompilerContext.Init(templateRootNode, TemplateNodeType.EntryPoint, processedType, null, state.variableStack);
            attrCompilerContext.AddContextReference(processedType, templateRootNode);

            bool needPop = CompileBindings(templateRootNode);

            context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.ElementSystem_HydrateEntryPoint));

            context.AddStatement(elementParam);

            templateDataBuilder.SetEntryPoint(context.Build(processedType.GetType().GetTypeName()));

            CompileHydratePoint(templateRootNode);

            if (needPop) {
                state.variableStack.Pop().Release();
            }

        }

        private void CompileHydratePoint(TemplateRootNode templateRootNode) {

            context.Setup();

            ParameterExpression systemParam = context.AddParameter<ElementSystem>("system");

            StructList<ConstructedChildData> childrenSetup = StructList<ConstructedChildData>.Get();

            SetupElementChildren(systemParam, templateRootNode.children, ResolveRequiredType(templateRootNode.requireType), childrenSetup);

            templateDataBuilder.SetHydratePoint(context.Build(templateRootNode.GetType().GetTypeName()));

            CompileChildren(childrenSetup, null);

            childrenSetup.Release();

        }

        private void CompileChildren(StructList<ConstructedChildData> childData, StructList<AttrInfo> injectedAttributes) {

            for (int i = 0; i < childData.size; i++) {
                ref ConstructedChildData data = ref childData.array[i];
                CompileNode(data.processedType, data.templateNode, data.templateIndex, injectedAttributes);
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

                output.AddUnsafe(new ConstructedChildData(childProcessedType, child, idx));

                Expression childNew = childProcessedType.GetConstructorExpression();

                switch (child) {
                    case SlotNode slotNode: {
                        Assert.IsTrue(slotNode.slotType == SlotType.Define);
                        context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.ElementSystem_AddSlotChild, childNew, ExpressionUtil.GetStringConstant(slotNode.slotName), ExpressionUtil.GetIntConstant(idx)));
                        break;
                    }

                    default: {
                        context.AddStatement(ExpressionFactory.CallInstance(systemParam, MemberData.ElementSystem_AddChild, childNew, ExpressionUtil.GetIntConstant(idx)));
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
                    ? ExpressionFactory.CallInstance(system, MemberData.ElementSystem_InitializeStaticAttribute, ExpressionUtil.GetStringConstant(attr.key), ExpressionUtil.GetStringConstant(attr.value))
                    : ExpressionFactory.CallInstance(system, MemberData.ElementSystem_InitializeDynamicAttribute, ExpressionUtil.GetStringConstant(attr.key)));
            }

        }

        private bool CompileBindings(TemplateNode node) {

            attributeCompiler.CompileAttributes(scratchAttributes, attrCompilerContext);

            if (attrCompilerContext.bindingResult.HasValue) {

                BindingIndices bindingIds = templateDataBuilder.AddBindings(node, attrCompilerContext.bindingResult);

                context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.ElementSystem_SetBindings,
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
                            MethodCallExpression expression = ExpressionFactory.CallInstance(system, MemberData.ElementSystem_AddMouseHandler,
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
                            MethodCallExpression expression = ExpressionFactory.CallInstance(system, MemberData.ElementSystem_AddKeyboardHandler,
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
                            MethodCallExpression expression = ExpressionFactory.CallInstance(system, MemberData.ElementSystem_AddDragEventHandler,
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
                            MethodCallExpression expression = ExpressionFactory.CallInstance(system, MemberData.ElementSystem_AddDragCreateHandler,
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
                    context.AddStatement(ExpressionFactory.CallInstance(system, MemberData.ElementSystem_RegisterForKeyboardEvents));
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

            public readonly int templateIndex;
            public readonly TemplateNode templateNode;
            public readonly ProcessedType processedType;

            public ConstructedChildData(ProcessedType p, TemplateNode node, int templateIndex) {
                this.templateNode = node;
                this.processedType = p;
                this.templateIndex = templateIndex;
            }

        }

    }

    public enum BindingVariableKind {

        Local,
        Reference

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