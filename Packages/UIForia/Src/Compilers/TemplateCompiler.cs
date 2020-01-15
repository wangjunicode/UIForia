using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Templates;
using UIForia.Text;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public class TemplateCompiler {

        private const string k_InputEventAliasName = "$evt";
        internal const string k_InputEventParameterName = "__evt";
        private const string k_InputHandlerVarName = "__inputHandler";
        internal const string k_CastElement = "__castElement";
        internal const string k_CastRoot = "__castRoot";
        private static readonly char[] s_StyleSeparator = {' '};

        private readonly LightStack<ContextVarAliasResolver> resolvers;

        private readonly LinqCompiler enabledCompiler;
        private readonly LinqCompiler createdCompiler;
        private readonly LinqCompiler updateCompiler;
        private CompiledTemplateData templateData;
        private Dictionary<Type, CompiledTemplate> templateMap;
        private TemplateCache templateCache;
        private int contextId = 1;
        private int NextContextId => contextId++;

        private LightStack<LightStack<ContextVariableDefinition>> contextStack;

        private static readonly DynamicStyleListTypeWrapper s_DynamicStyleListTypeWrapper = new DynamicStyleListTypeWrapper();

        private static readonly MethodInfo s_CreateFromPool = typeof(Application).GetMethod(nameof(Application.CreateElementFromPoolWithType));
        private static readonly MethodInfo s_BindingNodePool_Get = typeof(LinqBindingNode).GetMethod("Get", BindingFlags.Static | BindingFlags.Public);
        private static readonly FieldInfo s_StructList_ElementAttr_Array = typeof(StructList<ElementAttribute>).GetField("array");

        private static readonly MethodInfo s_SlotUsageList_PreSize = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.PreSize), BindingFlags.Static | BindingFlags.Public);
        private static readonly FieldInfo s_SlotUsageList_Array = typeof(StructList<SlotUsage>).GetField("array", BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_SlotUsageList_Release = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.Release), BindingFlags.Instance | BindingFlags.Public);

        private static readonly ConstructorInfo s_SlotUsage_Ctor = typeof(SlotUsage).GetConstructor(new[] {typeof(string), typeof(int), typeof(UIElement)});

        private static readonly ConstructorInfo s_TemplateScope_Ctor = typeof(TemplateScope).GetConstructor(new[] {typeof(Application), typeof(StructList<SlotUsage>)});
        private static readonly FieldInfo s_TemplateScope_SlotList = typeof(TemplateScope).GetField(nameof(TemplateScope.slotInputs));
        private static readonly FieldInfo s_TemplateScope_ApplicationField = typeof(TemplateScope).GetField(nameof(TemplateScope.application));
        private static readonly MethodInfo s_TemplateScope_ForwardSlotData = typeof(TemplateScope).GetMethod(nameof(TemplateScope.ForwardSlotUsage));
        private static readonly MethodInfo s_TemplateScope_ForwardSlotDataWithFallback = typeof(TemplateScope).GetMethod(nameof(TemplateScope.ForwardSlotUsageWithFallback));

        private static readonly ConstructorInfo s_ElementAttributeCtor = typeof(ElementAttribute).GetConstructor(new[] {typeof(string), typeof(string)});
        private static readonly FieldInfo s_ElementAttributeList = typeof(UIElement).GetField("attributes", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_Element_ChildrenList = typeof(UIElement).GetField(nameof(UIElement.children), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_LightList_Element_Array = typeof(LightList<UIElement>).GetField(nameof(LightList<UIElement>.array), BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_TextElement_Text = typeof(UITextElement).GetField(nameof(UITextElement.text), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly MethodInfo s_TextElement_SetText = typeof(UITextElement).GetMethod(nameof(UITextElement.SetText), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly FieldInfo s_UIElement_inputHandlerGroup = typeof(UIElement).GetField(nameof(UIElement.inputHandlers), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo s_UIElement_StyleSet = typeof(UIElement).GetField(nameof(UIElement.style), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly FieldInfo s_UIElement_TemplateMetaData = typeof(UIElement).GetField(nameof(UIElement.templateMetaData), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly MethodInfo s_UIElement_OnUpdate = typeof(UIElement).GetMethod(nameof(UIElement.OnUpdate), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_UIElement_SetAttribute = typeof(UIElement).GetMethod(nameof(UIElement.SetAttribute), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_UIElement_SetEnabled = typeof(UIElement).GetMethod(nameof(UIElement.SetEnabled), BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo s_StyleSet_InternalInitialize = typeof(UIStyleSet).GetMethod(nameof(UIStyleSet.internal_Initialize), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_StyleSet_SetBaseStyles = typeof(UIStyleSet).GetMethod(nameof(UIStyleSet.SetBaseStyles), BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo s_TemplateMetaData_GetStyleById = typeof(TemplateMetaData).GetMethod(nameof(TemplateMetaData.GetStyleById), BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo s_LightList_UIStyleGroupContainer_PreSize = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.PreSize), BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_LightList_UIStyleGroupContainer_Get = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.Get), BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_LightList_UIStyleGroupContainer_Release = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.Release), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo s_LightList_UIStyleGroupContainer_Add = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.Add), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo s_LightList_UIStyle_Release = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.Release), BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_LightList_UIStyleGroupContainer_Array = typeof(LightList<UIStyleGroupContainer>).GetField(nameof(LightList<UIStyleGroupContainer>.array), BindingFlags.Public | BindingFlags.Instance);

        private static readonly MethodInfo s_Application_CreateSlot2 = typeof(Application).GetMethod(nameof(Application.CreateSlot2), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo s_Application_HydrateTemplate = typeof(Application).GetMethod(nameof(Application.HydrateTemplate), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        private static readonly MethodInfo s_InputHandlerGroup_AddMouseEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddMouseEvent));
        private static readonly MethodInfo s_InputHandlerGroup_AddKeyboardEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddKeyboardEvent));

        private static readonly PropertyInfo s_Element_IsEnabled = typeof(UIElement).GetProperty(nameof(UIElement.isEnabled));
        internal static readonly FieldInfo s_Element_BindingNode = typeof(UIElement).GetField(nameof(UIElement.bindingNode));

        private static readonly MethodInfo s_LinqBindingNode_CreateLocalContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.CreateLocalContextVariable));
        private static readonly MethodInfo s_LinqBindingNode_GetLocalContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetLocalContextVariable));
        internal static readonly MethodInfo s_LinqBindingNode_GetContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetContextVariable));
        internal static readonly MethodInfo s_LinqBindingNode_GetRepeatItem = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetRepeatItem));

        private static readonly MethodInfo s_EventUtil_Subscribe = typeof(EventUtil).GetMethod(nameof(EventUtil.Subscribe));

        private static readonly MethodInfo s_DynamicStyleList_Flatten = typeof(DynamicStyleList).GetMethod(nameof(DynamicStyleList.Flatten));

        private static readonly Expression s_StringBuilderExpr = Expression.Field(null, typeof(StringUtil), nameof(StringUtil.s_CharStringBuilder));
        private static readonly Expression s_StringBuilderClear = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod("Clear"));
        private static readonly Expression s_StringBuilderToString = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod("ToString", Type.EmptyTypes));
        private static readonly MethodInfo s_StringBuilder_AppendString = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(string)});
        private static readonly MethodInfo s_StringBuilder_AppendCharacter = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(char)});
        private static readonly MethodInfo s_StringBuilder_AppendInt16 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(short)});
        private static readonly MethodInfo s_StringBuilder_AppendInt32 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(int)});
        private static readonly MethodInfo s_StringBuilder_AppendInt64 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(long)});
        private static readonly MethodInfo s_StringBuilder_AppendUInt16 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(ushort)});
        private static readonly MethodInfo s_StringBuilder_AppendUInt32 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(uint)});
        private static readonly MethodInfo s_StringBuilder_AppendUInt64 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(ulong)});
        private static readonly MethodInfo s_StringBuilder_AppendFloat = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(float)});
        private static readonly MethodInfo s_StringBuilder_AppendDouble = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(double)});
        private static readonly MethodInfo s_StringBuilder_AppendDecimal = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(decimal)});
        private static readonly MethodInfo s_StringBuilder_AppendByte = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(byte)});
        private static readonly MethodInfo s_StringBuilder_AppendSByte = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(sbyte)});
        private static readonly MethodInfo s_StringBuilder_AppendBool = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(bool)});
        private static readonly MethodInfo s_StringBuilder_AppendChar = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(char)});

        private static readonly LinqCompiler s_TypeResolver = new LinqCompiler();
        private static readonly PropertyInfo s_GenericInputEvent_AsKeyInputEvent = typeof(GenericInputEvent).GetProperty(nameof(GenericInputEvent.AsKeyInputEvent));
        private static readonly PropertyInfo s_GenericInputEvent_AsMouseInputEvent = typeof(GenericInputEvent).GetProperty(nameof(GenericInputEvent.AsMouseInputEvent));

        public TemplateCompiler(TemplateSettings settings) {
            this.templateCache = new TemplateCache(settings);
            this.templateMap = new Dictionary<Type, CompiledTemplate>();
            this.templateData = new CompiledTemplateData(settings);
            this.updateCompiler = new LinqCompiler();
            this.createdCompiler = new LinqCompiler();
            this.enabledCompiler = new LinqCompiler();
            this.resolvers = new LightStack<ContextVarAliasResolver>();

            resolvers.EnsureCapacity(8);
            resolvers.size = 3;
            resolvers.array[0] = default;
            resolvers.array[1] = default;
            resolvers.array[2] = default;
            Func<string, LinqCompiler, Expression> resolveAlias = ResolveAlias;

            this.createdCompiler.resolveAlias = resolveAlias;
            this.enabledCompiler.resolveAlias = resolveAlias;
            this.updateCompiler.resolveAlias = resolveAlias;

            this.contextStack = new LightStack<LightStack<ContextVariableDefinition>>();
        }

        public static CompiledTemplateData CompileTemplates(Type appRootType, TemplateSettings templateSettings) {
            TemplateCompiler instance = new TemplateCompiler(templateSettings);

            CompiledTemplateData compiledTemplateData = instance.CompileRoot(appRootType);

            return compiledTemplateData;
        }

        private CompiledTemplateData CompileRoot(Type appRootType) {
            // todo -- errors
            if (!typeof(UIElement).IsAssignableFrom(appRootType)) {
                throw new ArgumentException($"You can only create elements which are subclasses of UIElement. {appRootType} does not inherit from UIElement");
            }

            if (typeof(UIContainerElement).IsAssignableFrom(appRootType)) {
                throw new ArgumentException();
            }

            if (typeof(UITerminalElement).IsAssignableFrom(appRootType)) {
                throw new ArgumentException();
            }

            if (typeof(UITextElement).IsAssignableFrom(appRootType)) {
                throw new ArgumentException();
            }

            GetCompiledTemplate(TypeProcessor.GetProcessedType(appRootType));

            return templateData;
        }

        private CompiledTemplate GetCompiledTemplate(ProcessedType processedType) {
            // todo -- if this is a generic or unresolved generic we need to re-process it using concrete types (not sure if that's happening now)

            if (templateMap.TryGetValue(processedType.rawType, out CompiledTemplate retn)) {
                return retn;
            }

            ElementTemplateNode templateRootNode = templateCache.GetParsedTemplate(processedType);

            CompiledTemplate compiledTemplate = Compile(templateRootNode);

            templateMap[processedType.rawType] = compiledTemplate;

            return compiledTemplate;
        }

        private CompiledTemplate Compile(ElementTemplateNode templateRootNode) {
            CompiledTemplate retn = templateData.CreateTemplate(templateRootNode.templateShell.filePath, templateRootNode.templateName);
            LightList<string> namespaces = LightList<string>.Get();

            contextStack.Push(new LightStack<ContextVariableDefinition>());
            if (templateRootNode.templateShell.usings != null) {
                for (int i = 0; i < templateRootNode.templateShell.usings.size; i++) {
                    namespaces.Add(templateRootNode.templateShell.usings[i].namespaceName);
                }
            }

            ProcessedType processedType = templateRootNode.processedType;

            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            CompilationContext ctx = new CompilationContext(templateRootNode);
            ctx.namespaces = namespaces;
            ctx.rootType = processedType;
            ctx.rootParam = rootParam;
            ctx.templateScope = scopeParam;
            ctx.elementType = processedType;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.compiledTemplate = retn;
            ctx.ContextExpr = rootParam;

            ctx.Initialize(rootParam);

            for (int i = 0; i < templateRootNode.templateShell.styles.size; i++) {
                ref StyleDefinition styleDef = ref templateRootNode.templateShell.styles.array[i];

                StyleSheet sheet = templateData.ImportStyleSheet(styleDef);

                if (sheet != null) {
                    ctx.AddStyleSheet(styleDef.alias, sheet);
                }
            }

            if (ctx.styleSheets != null && ctx.styleSheets.size > 0) {
                retn.templateMetaData.styleReferences = ctx.styleSheets.ToArray();
            }

            if (!processedType.IsUnresolvedGeneric) {
                ctx.PushBlock();

                ctx.Comment("new " + TypeNameGenerator.GetTypeName(processedType.rawType));
                Expression createRootExpression = ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                    Expression.Constant(processedType.id),
                    Expression.Default(typeof(UIElement)), // root has no parent
                    Expression.Constant(templateRootNode.ChildCount),
                    Expression.Constant(0),
                    Expression.Constant(ctx.compiledTemplate.templateId)
                );

                // root = templateScope.application.CreateFromPool<Type>(attrCount, childCount);
                ctx.Assign(ctx.rootParam, createRootExpression);

                ctx.IfEqualsNull(ctx.rootParam, ctx.PopBlock());
            }

            ctx.templateRootNode = templateRootNode;
            VisitChildren(ctx, templateRootNode);
            ctx.Return(ctx.rootParam);
            LightList<string>.Release(ref namespaces);
            retn.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, scopeParam);
            contextStack.Pop();
            return retn;
        }

        private void VisitChildren(CompilationContext ctx, TemplateNode templateNode) {
            if (templateNode.ChildCount == 0) {
                return;
            }

            ctx.PushScope();

            Expression parentChildList = Expression.Field(ctx.ParentExpr, s_Element_ChildrenList);

            Expression parentChildListArray = Expression.Field(parentChildList, s_LightList_Element_Array);

            for (int i = 0; i < templateNode.ChildCount; i++) {
                Expression visit = Visit(ctx, templateNode[i]);
                // childList.array[i] = targetElement_x;
                ctx.Assign(Expression.ArrayAccess(parentChildListArray, Expression.Constant(i)), visit);
            }

            ctx.PopScope();
        }

        private Expression Visit(CompilationContext ctx, TemplateNode templateNode) {
            if (templateNode is RepeatNode repeatNode) {
                return CompileRepeatNode(ctx, repeatNode);
            }

            if (templateNode.processedType.IsUnresolvedGeneric) {
                templateNode.processedType = ResolveGenericElementType(templateNode);
            }

            ctx.elementType = templateNode.processedType;

            switch (templateNode) {
                case TextNode textNode:
                    return CompileTextNode(ctx, textNode);

                case ContainerNode containerNode:
                    return CompileContainerNode(ctx, containerNode);

                case SlotNode slotNode:
                    return CompileSlotDefinition(ctx, slotNode);

                case TerminalNode terminalNode:
                    return CompileTerminalNode(ctx, terminalNode);

                case ExpandedTemplateNode expandedTemplateNode:
                    return CompileExpandedNode(ctx, expandedTemplateNode);
            }

            return null;
        }

        public static readonly string[] repeatPropertyValues = {"list", "start", "end", "count", "page", "pageSize"};

        private Expression CompileRepeatNode(CompilationContext ctx, RepeatNode repeatNode) {
            if (repeatNode.HasProperty("count")) {
                if (repeatNode.HasProperty("list") || repeatNode.HasProperty("start") || repeatNode.HasProperty("end") || repeatNode.HasProperty("page") || repeatNode.HasProperty("pageSize")) {
                    throw CompileException.UnresolvedRepeatType("count", "list", "start", "page", "pageSize");
                }

                return CompileRepeatCount(ctx, repeatNode);
            }
            else if (repeatNode.HasProperty("list")) {
                if (repeatNode.HasProperty("count") || repeatNode.HasProperty("start") || repeatNode.HasProperty("end") || repeatNode.HasProperty("page") || repeatNode.HasProperty("pageSize")) {
                    throw CompileException.UnresolvedRepeatType("count", "start", "end", "page", "pageSize");
                }

                return CompileRepeatList(ctx, repeatNode);
            }
            else if (repeatNode.HasProperty("page") || repeatNode.HasProperty("pageSize")) { }

            throw new NotImplementedException();
        }

        private Expression CompileRepeatCount(CompilationContext ctx, RepeatNode repeatNode) {
            ParameterExpression nodeExpr = ctx.ElementExpr;

            repeatNode.processedType = TypeProcessor.GetProcessedType(typeof(UIRepeatCountElement));

            ctx.Comment("new " + TypeNameGenerator.GetTypeName(typeof(UIRepeatCountElement)));
            ctx.Assign(nodeExpr, ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(repeatNode.processedType.id),
                ctx.ParentExpr,
                Expression.Constant(0),
                Expression.Constant(repeatNode.GetAttributeCount()),
                Expression.Constant(ctx.compiledTemplate.templateId)
            ));

            MemberExpression templateSpawnIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.templateSpawnId)));
            MemberExpression templateRootContext = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.templateContextRoot)));
            MemberExpression scopeVar = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.scope)));
            MemberExpression indexVarIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.indexVarId)));

            CompileElementData(repeatNode, ctx);

            int spawnId = CompileChildrenAsTemplate(ctx, repeatNode, out int indexVarId);

            ctx.Assign(templateSpawnIdField, Expression.Constant(spawnId));
            ctx.Assign(templateRootContext, ctx.rootParam);
            ctx.Assign(scopeVar, ctx.templateScope);
            ctx.Assign(indexVarIdField, Expression.Constant(indexVarId));

            return nodeExpr;
        }

        private Expression CompileRepeatList(CompilationContext ctx, RepeatNode repeatNode) {
            ParameterExpression nodeExpr = ctx.ElementExpr;

            repeatNode.processedType = TypeProcessor.GetProcessedType(typeof(UIRepeatElement<>));
            repeatNode.processedType = ResolveGenericElementType(repeatNode);

            ctx.Comment("new " + TypeNameGenerator.GetTypeName(typeof(UIRepeatCountElement)));
            ctx.Assign(nodeExpr, ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(repeatNode.processedType.id),
                ctx.ParentExpr,
                Expression.Constant(0),
                Expression.Constant(repeatNode.GetAttributeCount()),
                Expression.Constant(ctx.compiledTemplate.templateId)
            ));

            MemberExpression templateSpawnIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.templateSpawnId)));
            MemberExpression templateRootContext = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.templateContextRoot)));
            MemberExpression scopeVar = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.scope)));
            MemberExpression indexVarIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.indexVarId)));

            int itemId = 8; // todo -- fix this

            Type itemType = repeatNode.processedType.rawType.GetGenericArguments()[0];

            CompileElementData(repeatNode, ctx);

            // resolvers.Push(new ContextVarAliasResolver("item", itemType, itemId, AliasResolverType.RepeatItem));

            int spawnId = CompileChildrenAsTemplate(ctx, repeatNode, out int indexVarId);

            // PopAliasResolver();

            ctx.Assign(templateSpawnIdField, Expression.Constant(spawnId));
            ctx.Assign(templateRootContext, ctx.rootParam);
            ctx.Assign(scopeVar, ctx.templateScope);
            ctx.Assign(indexVarIdField, Expression.Constant(indexVarId));

            return nodeExpr;
        }

        private int CompileChildrenAsTemplate(CompilationContext ctx, RepeatNode node, out int indexVarId) {
            if (node.ChildCount == 1) {
                return CompileRepeatTemplate(ctx, node, out indexVarId);
            }
            else {
                // todo -- insert a dummy node to wrap the child
                indexVarId = -1;
                return -999999;
            }
        }

        private Expression CompileTerminalNode(CompilationContext ctx, TerminalNode terminalNode) {
            throw new NotImplementedException();
        }

        private ScopedContextVariable[] CloneContextStack() {
            LightStack<ContextVariableDefinition> stack = contextStack.Peek();
            ScopedContextVariable[] clone = new ScopedContextVariable[stack.size];
            for (int i = 0; i < stack.size; i++) {
                clone[i] = new ScopedContextVariable() {
                    name = stack.array[i].GetName(),
                    id = stack.array[i].id,
                    type = stack.array[i].type
                };
            }

            return clone;
        }

        private Expression CompileSlotDefinition(CompilationContext parentContext, SlotNode slotNode) {
            // we want to try to resolve the slot name. if we can't fall back, if fallback id is -1 then don't add a child
            CompiledSlot compiledSlot = templateData.CreateSlot(parentContext.compiledTemplate.filePath, parentContext.compiledTemplate.templateName, slotNode.slotName, slotNode.slotType);
            compiledSlot.rootElementType = parentContext.rootType.rawType;

            slotNode.compiledSlot = compiledSlot;

            compiledSlot.scopedVariables = CloneContextStack();
            compiledSlot.exposedAttributes = slotNode.GetAttributes(AttributeType.Expose);

            Expression nodeExpr = parentContext.ElementExpr;

            Expression slotNameExpr = Expression.Constant(slotNode.slotName);

            parentContext.Assign(parentContext.ElementExpr, Expression.Call(
                parentContext.applicationExpr,
                s_Application_CreateSlot2,
                slotNameExpr,
                parentContext.templateScope,
                Expression.Constant(compiledSlot.slotId),
                parentContext.rootParam,
                parentContext.ParentExpr
            ));

            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression parentParam = Expression.Parameter(typeof(UIElement), "parent");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            CompilationContext ctx = new CompilationContext(parentContext.templateRootNode);

            ParameterExpression slotRootParam = ctx.GetVariable(typeof(UISlotOverride), "slotRoot");
            ctx.rootType = parentContext.rootType;
            ctx.rootParam = slotRootParam;
            ctx.templateScope = scopeParam;
            ctx.elementType = slotNode.processedType;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.Initialize(slotRootParam);
            ctx.compiledTemplate = parentContext.compiledTemplate; // todo -- might be wrong
            ctx.ContextExpr = rootParam;

            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(TypeProcessor.GetProcessedType(typeof(UISlotOverride)).id),
                Expression.Default(typeof(UIElement)), // todo -- parent is null, fix this
                Expression.Constant(slotNode.ChildCount),
                Expression.Constant(slotNode.GetAttributeCount()),
                Expression.Constant(parentContext.compiledTemplate.templateId)
            );

            ctx.Assign(slotRootParam, Expression.Convert(createRootExpression, typeof(UISlotOverride)));

            VisitChildren(ctx, slotNode);

            ctx.Return(slotRootParam);

            compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, parentParam, scopeParam);

            return nodeExpr;
        }

        private int CompileRepeatTemplate(CompilationContext parentContext, RepeatNode repeatNode, out int indexVarId) {
            TemplateNode templateNode = repeatNode.children[0];

            CompiledSlot compiledSlot = templateData.CreateSlot(parentContext.compiledTemplate.filePath, parentContext.compiledTemplate.templateName, "__template__", SlotType.Template);

            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression parentParam = Expression.Parameter(typeof(UIElement), "parent");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            CompilationContext ctx = new CompilationContext(parentContext.templateRootNode);

            indexVarId = NextContextId;

            // PushContextVarAliasResolver(indexVarId, "index", typeof(int));
            resolvers.Push(new ContextVarAliasResolver("item", repeatNode.processedType.rawType.GetGenericArguments()[0], 8, AliasResolverType.RepeatItem));

            ctx.rootType = parentContext.rootType;
            ctx.rootParam = rootParam;
            ctx.templateScope = scopeParam;
            ctx.elementType = templateNode.processedType;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.compiledTemplate = parentContext.compiledTemplate;
            ctx.ContextExpr = rootParam;
            ctx.Initialize(parentParam);

            ctx.PushScope();

            ctx.Return(Visit(ctx, templateNode));

            compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, parentParam, scopeParam);

            return compiledSlot.slotId;
        }

        private int CompileSlotOverride(CompilationContext parentContext, SlotNode slotOverrideNode, CompiledSlot definition, Type type = null) {
            if (type == null) type = typeof(UISlotOverride);

            CompiledSlot compiledSlot = templateData.CreateSlot(parentContext.compiledTemplate.filePath, parentContext.compiledTemplate.templateName, slotOverrideNode.slotName, SlotType.Override);

            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression parentParam = Expression.Parameter(typeof(UIElement), "parent");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            CompilationContext ctx = new CompilationContext(parentContext.templateRootNode);

            ParameterExpression slotRootParam = ctx.GetVariable(type, "slotRoot");
            ctx.rootType = parentContext.rootType;
            ctx.rootParam = rootParam;
            ctx.templateScope = scopeParam;
            ctx.elementType = slotOverrideNode.processedType;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.compiledTemplate = parentContext.compiledTemplate;
            ctx.ContextExpr = rootParam;
            ctx.Initialize(slotRootParam);

            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(TypeProcessor.GetProcessedType(type).id),
                parentParam,
                Expression.Constant(slotOverrideNode.ChildCount),
                Expression.Constant(slotOverrideNode.GetAttributeCount()),
                Expression.Constant(parentContext.compiledTemplate.templateId)
            );

            ctx.Assign(slotRootParam, Expression.Convert(createRootExpression, type));

            ExposedVariableData exposedVariableData = new ExposedVariableData();
            exposedVariableData.rootType = definition.rootElementType;
            exposedVariableData.scopedVariables = definition.scopedVariables;
            exposedVariableData.exposedAttrs = definition.exposedAttributes ?? new AttributeDefinition2[0];

            ProcessAttrsAndVisitChildren(ctx, slotOverrideNode, exposedVariableData);

            ctx.Return(slotRootParam);

            compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, parentParam, scopeParam);

            return compiledSlot.slotId;
        }

        private Expression CompileTextNode(CompilationContext ctx, TextNode textNode) {
            ParameterExpression nodeExpr = ctx.ElementExpr;
            ProcessedType processedType = textNode.processedType;

            ctx.Comment("new " + TypeNameGenerator.GetTypeName(processedType.rawType));

            ctx.Assign(nodeExpr, CreateElement(ctx, textNode));

            // ((UITextElement)element).text = "string value";
            if (textNode.textExpressionList != null && textNode.textExpressionList.size > 0) {
                if (textNode.IsTextConstant()) {
                    ctx.Assign(Expression.MakeMemberAccess(Expression.Convert(nodeExpr, typeof(UITextElement)), s_TextElement_Text), Expression.Constant(textNode.GetStringContent()));
                }
            }

            ProcessAttrsAndVisitChildren(ctx, textNode);

            return nodeExpr;
        }

        private void ProcessAttrsAndVisitChildren(CompilationContext ctx, TemplateNode node, ExposedVariableData exposedVariableData = null) {
            StructList<ContextAliasActions> contextMods = CompileElementData(node, ctx, exposedVariableData);

            VisitChildren(ctx, node);

            UndoContextMods(contextMods);
        }

        private enum ModType {

            Alias,
            Context

        }

        private struct ContextAliasActions {

            public ModType modType;
            public string name;

        }

        private void UndoContextMods(StructList<ContextAliasActions> mods) {
            if (mods == null || mods.size == 0) {
                return;
            }

            for (int i = 0; i < mods.size; i++) {
                ContextAliasActions mod = mods.array[i];
                if (mod.modType == ModType.Alias) {
                    ContextVariableDefinition definition = FindContextByName(mod.name);
                    // remove from name list
                    // assert is first
                    definition.nameList.RemoveLast();
                }
                else {
                    contextStack.Peek().Pop();
                }
            }
        }

        private ContextVariableDefinition FindContextByName(string name) {
            LightStack<ContextVariableDefinition> stack = contextStack.Peek();
            for (int j = stack.size - 1; j >= 0; j--) {
                ContextVariableDefinition definition = stack.array[j];
                if (definition.GetName() == name) {
                    return definition;
                }
            }

            return null;
        }

        private Expression CompileContainerNode(CompilationContext ctx, ContainerNode containerNode) {
            ParameterExpression nodeExpr = ctx.ElementExpr;
            ProcessedType processedType = containerNode.processedType;

            ctx.Comment("new " + TypeNameGenerator.GetTypeName(processedType.rawType));

            ctx.Assign(nodeExpr, CreateElement(ctx, containerNode));

            ProcessAttrsAndVisitChildren(ctx, containerNode);

            return nodeExpr;
        }

        private Expression CompileExpandedNode(CompilationContext ctx, ExpandedTemplateNode expandedTemplateNode) {
            CompiledTemplate expandedTemplate = GetCompiledTemplate(expandedTemplateNode.processedType);
            ParameterExpression nodeExpr = ctx.ElementExpr;

            ctx.Comment("new " + TypeNameGenerator.GetTypeName(expandedTemplateNode.processedType.rawType));
            ctx.Assign(nodeExpr, ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(expandedTemplateNode.processedType.id),
                ctx.ParentExpr,
                Expression.Constant(expandedTemplateNode.expandedRoot.ChildCount),
                Expression.Constant(expandedTemplateNode.GetAttributeCount()),
                Expression.Constant(ctx.compiledTemplate.templateId)
            ));

            Expression slotUsageExpr = Expression.Default(typeof(StructList<SlotUsage>));

            // for every template we will hydrate we need to fill in the slot usage list
            // there are 2 ways this happens: 1. we forward an existing input slot, 2. we add an entry to the input list
            // the template knows what it's input slots are. 

            // need to pass down any slots from this scope into the inner scope of expanded template
            // two cases this could be.
            // 1. we have an override
            // 2. we do not have an override

            // in the case of extern slots, if no override is provided in the outermost scope
            // we need to use the default that was defined in the inner one. 
            // this likely means the inner calls slot forwarding only with defaults?

            bool hasOuterSlot = expandedTemplateNode.slotOverrideNodes != null && expandedTemplateNode.slotOverrideNodes.size > 0;
            LightList<SlotNode> accepted = expandedTemplateNode.expandedRoot.slotDefinitionNodes;
            LightList<SlotNode> inputSlots = ctx.templateRootNode.slotDefinitionNodes;
            bool needsRelease = false;

            if (hasOuterSlot) {
                LightList<SlotNode> slotOverrides = expandedTemplateNode.slotOverrideNodes;

                slotUsageExpr = ctx.GetVariable<StructList<SlotUsage>>("slotUsage");

                needsRelease = true;
                ctx.Assign(slotUsageExpr, Expression.Call(null, s_SlotUsageList_PreSize, Expression.Constant(slotOverrides.size)));

                for (int i = 0; i < slotOverrides.size; i++) {
                    SlotNode definition;

                    if (!expandedTemplateNode.expandedRoot.DefinesSlot(slotOverrides.array[i].slotName, out definition)) {
                        throw CompileException.UnmatchedSlot(slotOverrides.array[i].slotName, slotOverrides.array[i].elementRoot.templateShell.filePath);
                    }

                    int slotId = CompileSlotOverride(ctx, slotOverrides.array[i], definition.compiledSlot);

                    MemberExpression arrayAccess = Expression.MakeMemberAccess(slotUsageExpr, s_SlotUsageList_Array);

                    IndexExpression arrayIndex = Expression.ArrayAccess(arrayAccess, Expression.Constant(i));

                    ctx.Comment(slotOverrides[i].slotName);

                    ctx.Assign(arrayIndex, Expression.New(s_SlotUsage_Ctor,
                        Expression.Constant(slotOverrides[i].slotName),
                        Expression.Constant(slotId),
                        ctx.rootParam
                    ));
                }
            }

            if (inputSlots != null && accepted != null) {
                if (!hasOuterSlot) {
                    slotUsageExpr = ctx.GetVariable<StructList<SlotUsage>>("slotUsage");
                    ctx.Assign(slotUsageExpr, Expression.Call(null, s_SlotUsageList_PreSize, Expression.Constant(accepted.size)));
                    needsRelease = true;
                }

                for (int i = 0; i < accepted.size; i++) {
                    SlotNode match = GetMatchingInputSlotNode(inputSlots, accepted.array[i]);

                    if (match == null) continue;

                    int slotId = CompileSlotOverride(ctx, match, null); // todo -- see what to replace null with

                    ctx.AddStatement(Expression.Call(
                        ctx.templateScope,
                        s_TemplateScope_ForwardSlotDataWithFallback,
                        Expression.Constant(accepted.array[i].slotName),
                        slotUsageExpr,
                        ctx.rootParam,
                        Expression.Constant(slotId))
                    );
                }
            }

            Expression templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr, slotUsageExpr);

            ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_Application_HydrateTemplate, Expression.Constant(expandedTemplate.templateId), nodeExpr, templateScopeCtor));

            if (needsRelease) {
                ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(slotUsageExpr, s_SlotUsageList_Release));
            }

            CompileElementData(expandedTemplateNode, ctx);

            return nodeExpr;
        }

        private void InitializeCompilers(LightList<string> namespaces, Type rootType, Type elementType) {
            updateCompiler.Reset();
            enabledCompiler.Reset();
            createdCompiler.Reset();

            updateCompiler.SetNamespaces(namespaces);
            enabledCompiler.SetNamespaces(namespaces);
            createdCompiler.SetNamespaces(namespaces);
            Parameter p0 = new Parameter(typeof(UIElement), "__root", ParameterFlags.NeverNull);
            Parameter p1 = new Parameter(typeof(UIElement), "__element", ParameterFlags.NeverNull);

            updateCompiler.SetSignature(p0, p1);
            enabledCompiler.SetSignature(p0, p1);
            createdCompiler.SetSignature(p0, p1);

            // todo -- each compiler needs to handle namespaces and alias resolvers 
            Parameter elementParameter = new Parameter(elementType, k_CastElement, ParameterFlags.NeverNull);
            Parameter rootParameter = new Parameter(rootType, k_CastRoot, ParameterFlags.NeverNull);

            updateCompiler.AddVariableUnchecked(elementParameter, ExpressionFactory.Convert(updateCompiler.GetParameter("__element"), elementType));
            updateCompiler.AddVariableUnchecked(rootParameter, ExpressionFactory.Convert(updateCompiler.GetParameter("__root"), rootType));
            enabledCompiler.AddVariableUnchecked(elementParameter, ExpressionFactory.Convert(enabledCompiler.GetParameter("__element"), elementType));
            enabledCompiler.AddVariableUnchecked(rootParameter, ExpressionFactory.Convert(enabledCompiler.GetParameter("__root"), rootType));
            createdCompiler.AddVariableUnchecked(elementParameter, ExpressionFactory.Convert(createdCompiler.GetParameter("__element"), elementType));
            createdCompiler.AddVariableUnchecked(rootParameter, ExpressionFactory.Convert(createdCompiler.GetParameter("__root"), rootType));

            resolvers.array[0] = new ContextVarAliasResolver("$element", elementType, -1, AliasResolverType.Element);
            resolvers.array[1] = new ContextVarAliasResolver("$parent", elementType, -2, AliasResolverType.Parent);
            resolvers.array[2] = new ContextVarAliasResolver("$root", elementType, -3, AliasResolverType.Root);
        }

        private static void OutputAttributes(CompilationContext ctx, TemplateNode template) {
            if (template.GetAttributeCount() > 0) {
                int attrIdx = 0;

                for (int i = 0; i < template.attributes.size; i++) {
                    ref AttributeDefinition2 attr = ref template.attributes.array[i];

                    if (template.attributes[i].type == AttributeType.Attribute) {
                        // targetElement_x.attributeList.array[x] = new ElementAttribute("key", "value"); will be empty string for attributes that are bound
                        MemberExpression listAccess = Expression.MakeMemberAccess(ctx.ElementExpr, s_ElementAttributeList);
                        MemberExpression arrayAccess = Expression.MakeMemberAccess(listAccess, s_StructList_ElementAttr_Array);
                        IndexExpression arrayIndex = Expression.ArrayAccess(arrayAccess, Expression.Constant(attrIdx++));

                        if ((attr.flags & AttributeFlags.Const) != 0) {
                            ctx.Assign(arrayIndex, Expression.New(s_ElementAttributeCtor, Expression.Constant(attr.key), Expression.Constant(attr.value)));
                        }
                        else {
                            ctx.Assign(arrayIndex, Expression.New(s_ElementAttributeCtor, Expression.Constant(attr.key), Expression.Constant(string.Empty)));
                        }
                    }
                }
            }
        }

        private StructList<ContextAliasActions> CompileElementData(TemplateNode templateNode, CompilationContext ctx, ExposedVariableData exposedData = null) {
            int count = 0;

            if (templateNode.attributes != null) {
                count = templateNode.attributes.size;
            }

            OutputAttributes(ctx, templateNode);

            InitializeCompilers(ctx.namespaces, ctx.templateRootNode.ElementType, templateNode.processedType.rawType);

            int updateBindingCount = 0;
            int enabledBindingCount = 0;
            int createdBindingCount = 0;

            StructList<AttributeDefinition2> perFrameStyles = null;
            StructList<DynamicStyleData> dynamicStyleData = null;
            StructList<AttributeDefinition2> inputList = null;
            StructList<ContextAliasActions> contextModifications = null;

            // todo -- handle nested access <Element thing.value.x="144f" keydown="" key-filter:keydown.keyup.withfocus="[allDown(shift, c, k), NoneOf()]"/>
            // todo -- handle .read.write bindings

            if (exposedData != null) {
                ParameterExpression innerSlotContext = createdCompiler.AddVariable(exposedData.rootType, "__innerContext");
                ParameterExpression element = createdCompiler.GetVariable(k_CastElement);
                MemberExpression bindingNode = Expression.Field(element, s_Element_BindingNode);
                MemberExpression innerContext = Expression.Field(bindingNode, typeof(LinqBindingNode).GetField(nameof(LinqBindingNode.innerContext)));
                createdCompiler.Assign(innerSlotContext, Expression.Convert(innerContext, exposedData.rootType));
                createdCompiler.SetImplicitContext(innerSlotContext);

                for (int i = 0; i < exposedData.scopedVariables.Length; i++) {
                    ScopedContextVariable exposed = exposedData.scopedVariables[i];
                    contextStack.Peek().Push(new ContextVariableDefinition() {
                        id = exposed.id,
                        name = exposed.name,
                        type = exposed.type,
                        variableType = AliasResolverType.ContextVariable
                    });
                }
                
                for (int i = 0; i < exposedData.exposedAttrs.Length; i++) {
                    ref AttributeDefinition2 attr = ref exposedData.exposedAttrs[i];
                    // bindingNode.CreateContextVariable<string>(id);
                    ContextVariableDefinition variableDefinition = new ContextVariableDefinition();

                    Type expressionType = createdCompiler.GetExpressionType(attr.value);

                    variableDefinition.name = attr.key;
                    variableDefinition.id = NextContextId;
                    variableDefinition.type = expressionType;
                    variableDefinition.variableType = AliasResolverType.ContextVariable;

                    contextStack.Peek().Push(variableDefinition);

                    Type type = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), expressionType);
                    ReflectionUtil.TypeArray2[0] = typeof(int);
                    ReflectionUtil.TypeArray2[1] = typeof(string);
                    ConstructorInfo ctor = type.GetConstructor(ReflectionUtil.TypeArray2);

                    Expression contextVariable = Expression.New(ctor, Expression.Constant(variableDefinition.id), Expression.Constant(attr.key));
                    Expression access = Expression.MakeMemberAccess(createdCompiler.GetVariable(k_CastElement), s_Element_BindingNode);
                    Expression createVariable = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_CreateLocalContextVariable, contextVariable);

                    createdCompiler.RawExpression(createVariable);
                }

                // updateCompiler.Assign(innerSlotContext, Expression.Convert(innerContext, exposedData.rootType));
                // updateCompiler.SetImplicitContext(innerSlotContext);

                createdBindingCount++;
                // updateBindingCount++;


                for (int i = 0; i < exposedData.scopedVariables.Length; i++) {
                    contextStack.Peek().Pop();
                }
            }

            for (int i = 0; i < count; i++) {
                ref AttributeDefinition2 attr = ref templateNode.attributes.array[i];
                if (attr.type != AttributeType.Alias) {
                    continue;
                }

                contextModifications = contextModifications ?? StructList<ContextAliasActions>.Get();
                contextModifications.Add(new ContextAliasActions() {
                    modType = ModType.Alias,
                    name = attr.key
                });

                ContextVariableDefinition contextVar = FindContextByName(attr.value.Trim());

                if (contextVar == null) {
                    throw CompileException.UnknownAlias(attr.key);
                }

                contextVar.PushAlias(attr.key);
            }

            for (int i = 0; i < count; i++) {
                // ReSharper disable once PossibleNullReferenceException
                ref AttributeDefinition2 attr = ref templateNode.attributes.array[i];
                switch (attr.type) {
                    case AttributeType.Alias:
                    case AttributeType.Slot:
                        break;
                    case AttributeType.Context:
                    case AttributeType.ContextVariable: {
                        if (attr.key == "element" || attr.key == "parent" || attr.key == "root") {
                            throw new CompileException($"`{attr.key} is a reserved name and cannot be used as a context variable name");
                        }

                        createdBindingCount++;
                        createdCompiler.SetImplicitContext(createdCompiler.GetVariable(k_CastRoot));
                        Type expressionType = createdCompiler.GetExpressionType(attr.value);

                        contextModifications = contextModifications ?? StructList<ContextAliasActions>.Get();

                        contextModifications.Add(new ContextAliasActions() {
                            modType = ModType.Context,
                            name = attr.key
                        });

                        LightStack<ContextVariableDefinition> ctxStack = contextStack.Peek();

                        ContextVariableDefinition variableDefinition = new ContextVariableDefinition();

                        variableDefinition.name = attr.key;
                        variableDefinition.id = NextContextId;
                        variableDefinition.type = expressionType;
                        variableDefinition.variableType = AliasResolverType.ContextVariable;

                        ctxStack.Push(variableDefinition);

                        Type type = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), expressionType);
                        ReflectionUtil.TypeArray2[0] = typeof(int);
                        ReflectionUtil.TypeArray2[1] = typeof(string);
                        ConstructorInfo ctor = type.GetConstructor(ReflectionUtil.TypeArray2);

                        Expression contextVariable = Expression.New(ctor, Expression.Constant(variableDefinition.id), Expression.Constant(attr.key));
                        Expression access = Expression.MakeMemberAccess(createdCompiler.GetVariable(k_CastElement), s_Element_BindingNode);
                        Expression createVariable = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_CreateLocalContextVariable, contextVariable);

                        createdCompiler.RawExpression(createVariable);

                        if ((attr.flags & AttributeFlags.Const) != 0) {
                            // already incremented created count
                            CompileAssignContextVariable(createdCompiler, attr, ctx, type, variableDefinition.id);
                        }
                        else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                            enabledBindingCount++;
                            CompileAssignContextVariable(enabledCompiler, attr, ctx, type, variableDefinition.id);
                        }
                        else {
                            updateBindingCount++;
                            CompileAssignContextVariable(updateCompiler, attr, ctx, type, variableDefinition.id);
                        }

                        break;
                    }
                    case AttributeType.Property: {
                        if (ReflectionUtil.IsEvent(templateNode.ElementType, attr.key, out EventInfo eventInfo)) {
                            createdBindingCount++;
                            CompileEventBinding(createdCompiler, attr, eventInfo);
                            continue;
                        }

                        if ((attr.flags & AttributeFlags.Const) != 0) {
                            createdBindingCount++;
                            CompilePropertyBinding(createdCompiler, templateNode, attr);
                        }
                        else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                            enabledBindingCount++;
                            CompilePropertyBinding(enabledCompiler, templateNode, attr);
                        }
                        else {
                            updateBindingCount++;
                            CompilePropertyBinding(updateCompiler, templateNode, attr);
                        }

                        break;
                    }

                    case AttributeType.Style: {
                        if ((attr.flags & AttributeFlags.StyleProperty) == 0) {
                            StructList<TextExpression> list = StructList<TextExpression>.Get();
                            TextTemplateProcessor.ProcessTextExpressions(attr.value, list);

                            if (TextTemplateProcessor.TextExpressionIsConstant(list)) {
                                createdBindingCount++;

                                string[] parts = attr.value.Split(' ');

                                ParameterExpression styleList = createdCompiler.AddVariable(new Parameter<LightList<UIStyleGroupContainer>>("styleList"), ExpressionFactory.CallStaticUnchecked(s_LightList_UIStyleGroupContainer_PreSize, Expression.Constant(parts.Length)));

                                Expression styleListArray = Expression.MakeMemberAccess(styleList, s_LightList_UIStyleGroupContainer_Array);

                                for (int p = 0; p < parts.Length; p++) {
                                    int styleId = ctx.ResolveStyleName(parts[p]);
                                    IndexExpression arrayIndex = Expression.ArrayAccess(styleListArray, Expression.Constant(p));
                                    createdCompiler.Comment(parts[p]);
                                    MemberExpression metaData = Expression.Field(createdCompiler.GetParameter("__element"), s_UIElement_TemplateMetaData);
                                    MethodCallExpression expr = ExpressionFactory.CallInstanceUnchecked(metaData, s_TemplateMetaData_GetStyleById, Expression.Constant(styleId));
                                    createdCompiler.RawExpression(Expression.Assign(arrayIndex, expr));
                                }

                                MemberExpression style = Expression.Field(createdCompiler.GetParameter("__element"), s_UIElement_StyleSet);
                                MethodCallExpression initStyle = ExpressionFactory.CallInstanceUnchecked(style, s_StyleSet_InternalInitialize, styleList);
                                createdCompiler.RawExpression(initStyle);

                                list.QuickRelease();
                            }
                            else {
                                dynamicStyleData = StructList<DynamicStyleData>.Get();
                                for (int s = 0; s < list.size; s++) {
                                    ref TextExpression expr = ref list.array[s];
                                    if (expr.isExpression) {
                                        dynamicStyleData.Add(new DynamicStyleData(expr.text, typeof(string), false));
                                    }
                                    else {
                                        string[] parts = expr.text.Split(s_StyleSeparator, StringSplitOptions.RemoveEmptyEntries);

                                        for (int index = 0; index < parts.Length; index++) {
                                            dynamicStyleData.Add(new DynamicStyleData(parts[index], typeof(UIStyleGroupContainer), true));
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            // instance style
                            // for the moment assume no shorthands or syntax sugar, pure c#
                            // style.backgroundColor = Color.red;

                            updateBindingCount++;
                            CompileStyleBinding(updateCompiler, templateNode, attr);
                        }

                        break;
                    }

                    case AttributeType.Attribute: {
                        if ((attr.flags & AttributeFlags.Const) != 0) { }
                        else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                            enabledBindingCount++;
                            CompileAttributeBinding(enabledCompiler, templateNode, attr);
                        }
                        else {
                            updateBindingCount++;
                            CompileAttributeBinding(updateCompiler, templateNode, attr);
                        }

                        break;
                    }
                    case AttributeType.Expose: {
                        // 
                        break;
                    }
                    case AttributeType.Event: {
                        break;
                    }
                    case AttributeType.Conditional: {
                        if ((attr.flags & AttributeFlags.Const) != 0) {
                            createdBindingCount++;
                            CompileConditionalBinding(createdCompiler, templateNode, attr);
                        }
                        else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                            enabledBindingCount++;
                            CompileConditionalBinding(enabledCompiler, templateNode, attr);
                        }
                        else {
                            updateBindingCount++;
                            CompileConditionalBinding(updateCompiler, templateNode, attr);
                        }

                        break;
                    }
                    case AttributeType.Key:
                    case AttributeType.Touch:
                    case AttributeType.Controller:
                    case AttributeType.Mouse: {
                        inputList = inputList ?? StructList<AttributeDefinition2>.Get();
                        inputList.Add(attr);
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (templateNode.processedType.requiresUpdateFn) {
                updateBindingCount++;
                // todo change this to not require OnUpdate to be virtual
                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateCompiler.GetVariable(k_CastElement), s_UIElement_OnUpdate));
            }

            updateBindingCount = CompileTextBinding(templateNode, updateBindingCount);

            // if we have style bindings they need to run after Update() is called (or where it would have been called if it would have been present)
            updateBindingCount = CompileDynamicStyleData(ctx, dynamicStyleData, updateBindingCount);

            if (perFrameStyles != null) {
                for (int i = 0; i < perFrameStyles.size; i++) {
                    ref AttributeDefinition2 attr = ref perFrameStyles.array[i];
                    // instance property
                    if ((attr.flags & AttributeFlags.StyleProperty) != 0) {
                        throw new NotImplementedException();
                        // var property = new StyleProperty(StylePropertyId.XXX, expr(root, element));
                        // if(!StyleProperty.IsEqual(element.style.state?.XXX, property) {
                        //    element.style.state.SetProperty(XXX, property);
                        // }
                    }
                    else {
                        // if any part return type is an array -> handle that differently
                        // for each part (deduplicated)

                        StructList<TextExpression> list = StructList<TextExpression>.Get();
                        TextTemplateProcessor.ProcessTextExpressions(attr.value, list);
                    }
                }

                perFrameStyles.QuickRelease();
            }

            if (inputList != null) {
                createdBindingCount++;
                // todo -- if element already has event handlers (from template internals or from c#) we need to grow that list (not yet implemented)
                createdCompiler.Assign(createdCompiler.Value(k_CastElement + "." + nameof(UIElement.inputHandlers)), Expression.New(typeof(InputHandlerGroup)));

                for (int i = 0; i < inputList.size; i++) {
                    switch (inputList.array[i].type) {
                        case AttributeType.Mouse:
                            CompileMouseInputBinding(createdCompiler, inputList.array[i]);
                            break;
                        case AttributeType.Key:
                            CompileKeyboardInputBinding(createdCompiler, inputList.array[i]);
                            break;
                    }
                }

                inputList.QuickRelease();
                // Application.InputSystem.RegisterKeyboardHandler(element);
                ParameterExpression elementVar = createdCompiler.GetVariable(k_CastElement);
                MemberExpression app = Expression.Property(elementVar, typeof(UIElement).GetProperty(nameof(UIElement.application)));
                MemberExpression inputSystem = Expression.Property(app, typeof(Application).GetProperty(nameof(Application.InputSystem)));
                MethodInfo method = typeof(InputSystem).GetMethod(nameof(InputSystem.RegisterKeyboardHandler));
                createdCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(inputSystem, method, elementVar));
            }

            int createdBindingId = -1;
            int enabledBindingId = -1;
            int updateBindingId = -1;

            if (createdBindingCount > 0) {
                CompiledBinding createdBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnCreate);
                createdBinding.bindingFn = createdCompiler.BuildLambda();
                createdBindingId = createdBinding.bindingId;
            }

            if (enabledBindingCount > 0) {
                CompiledBinding enabledBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnEnable);
                enabledBinding.bindingFn = enabledCompiler.BuildLambda();
                enabledBindingId = enabledBinding.bindingId;
            }

            if (updateBindingCount > 0) {
                CompiledBinding updateBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnUpdate);
                updateBinding.bindingFn = updateCompiler.BuildLambda();
                updateBindingId = updateBinding.bindingId;
            }

            // create binding node if needed
            if (updateCompiler.HasStatements || createdCompiler.HasStatements || enabledCompiler.HasStatements) {
                // scope.application.BindingNodePool.Get(root, element);
                ctx.AddStatement(ExpressionFactory.CallStaticUnchecked(s_BindingNodePool_Get,
                        ctx.applicationExpr,
                        ctx.rootParam,
                        ctx.ElementExpr,
                        ctx.ContextExpr,
                        Expression.Constant(createdBindingId),
                        Expression.Constant(enabledBindingId),
                        Expression.Constant(updateBindingId)
                    )
                );
            }

            return contextModifications;
        }


        private int CompileDynamicStyleData(CompilationContext ctx, StructList<DynamicStyleData> dynamicStyleData, int updateBindingCount) {
            if (dynamicStyleData == null) {
                return updateBindingCount;
            }

            ParameterExpression castElement = updateCompiler.GetVariable(k_CastElement);

            updateBindingCount++;

            // todo -- to handle array case we can't use PreSize, need to call Add on the list since size will be dynamic
            ParameterExpression styleList = updateCompiler.AddVariable(
                new Parameter<LightList<UIStyleGroupContainer>>("styleList", ParameterFlags.NeverNull),
                ExpressionFactory.CallStaticUnchecked(s_LightList_UIStyleGroupContainer_Get)
            );

            Expression templateContext = Expression.Field(castElement, s_UIElement_TemplateMetaData);

            updateCompiler.SetImplicitContext(updateCompiler.GetVariable(k_CastRoot));

            for (int s = 0; s < dynamicStyleData.size; s++) {
                ref DynamicStyleData data = ref dynamicStyleData.array[s];

                updateCompiler.Comment(data.text);

                if (data.isConstant) {
                    int styleId = ctx.ResolveStyleName(data.text);

                    Expression staticStyle = ExpressionFactory.CallInstanceUnchecked(templateContext, s_TemplateMetaData_GetStyleById, Expression.Constant(styleId));
                    MethodCallExpression addCall = ExpressionFactory.CallInstanceUnchecked(styleList, s_LightList_UIStyleGroupContainer_Add, staticStyle);
                    updateCompiler.RawExpression(addCall);
                }
                else {
                    s_DynamicStyleListTypeWrapper.styleList = styleList;

                    Expression dynamicStyleList = updateCompiler.TypeWrapStatement(s_DynamicStyleListTypeWrapper, typeof(DynamicStyleList), data.text);

                    updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(dynamicStyleList, s_DynamicStyleList_Flatten, templateContext, styleList));
                }
            }

            updateCompiler.SetNullCheckingEnabled(false);

            MemberExpression style = Expression.Field(castElement, s_UIElement_StyleSet);

            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(style, s_StyleSet_SetBaseStyles, styleList));
            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(styleList, s_LightList_UIStyleGroupContainer_Release));

            updateCompiler.SetNullCheckingEnabled(true);

            return updateBindingCount;
        }


        private int CompileTextBinding(TemplateNode templateNode, int bindingCount) {
            if (!(templateNode is TextNode textNode)) {
                return bindingCount;
            }

            if (textNode.textExpressionList != null && textNode.textExpressionList.size > 0 && !textNode.IsTextConstant()) {
                updateCompiler.AddNamespace("UIForia.Util");
                updateCompiler.AddNamespace("UIForia.Text");
                bindingCount++;
                updateCompiler.SetImplicitContext(updateCompiler.GetVariable(k_CastRoot));
                StructList<TextExpression> expressionParts = textNode.textExpressionList;

                MemberExpression textValueExpr = Expression.Field(updateCompiler.GetVariable(k_CastElement), s_TextElement_Text);

                for (int i = 0; i < expressionParts.size; i++) {
                    if (expressionParts[i].isExpression) {
                        Expression val = updateCompiler.Value(expressionParts[i].text);
                        switch (Type.GetTypeCode(val.Type)) {
                            case TypeCode.Boolean:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendBool, val));
                                break;
                            case TypeCode.Byte:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendByte, val));
                                break;
                            case TypeCode.Char:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendChar, val));
                                break;
                            case TypeCode.Decimal:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendDecimal, val));
                                break;
                            case TypeCode.Double:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendDouble, val));
                                break;
                            case TypeCode.Int16:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendInt16, val));
                                break;
                            case TypeCode.Int32:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendInt32, val));
                                break;
                            case TypeCode.Int64:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendInt64, val));
                                break;
                            case TypeCode.SByte:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendSByte, val));
                                break;
                            case TypeCode.Single:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendFloat, val));
                                break;
                            case TypeCode.String:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendString, val));
                                break;
                            case TypeCode.UInt16:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendUInt16, val));
                                break;
                            case TypeCode.UInt32:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendUInt32, val));
                                break;
                            case TypeCode.UInt64:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendUInt64, val));
                                break;

                            case TypeCode.Object:
                            case TypeCode.Empty:
                            case TypeCode.DBNull:
                            case TypeCode.DateTime:
                            default:
                                MethodCallExpression toString = ExpressionFactory.CallInstanceUnchecked(val, val.Type.GetMethod("ToString", Type.EmptyTypes));
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendString, toString));
                                break;
                        }
                    }
                    else {
                        updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendString, Expression.Constant(expressionParts[i].text)));
                    }
                }

                Expression e = updateCompiler.GetVariable(k_CastElement);
                Expression condition = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.EqualsString), new[] {typeof(string)}), textValueExpr);
                condition = Expression.Equal(condition, Expression.Constant(false));
                ConditionalExpression ifCheck = Expression.IfThen(condition, Expression.Block(ExpressionFactory.CallInstanceUnchecked(e, s_TextElement_SetText, s_StringBuilderToString)));

                // updateCompiler.RawExpression(ExpressionFactory.CallStaticUnchecked(typeof(CharStringBuilder).GetMethod("CompareStringBuilder_String"), s_StringBuilderExpr, s_TextElement_Text));
                // updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(e, s_TextElement_SetText, s_StringBuilderToString));
                updateCompiler.RawExpression(ifCheck);
                updateCompiler.RawExpression(s_StringBuilderClear);
            }

            return bindingCount;
        }


        public static void CompileAssignContextVariable(LinqCompiler compiler, in AttributeDefinition2 attr, CompilationContext ctx, Type contextVarType, int varId) {
            //ContextVariable<T> ctxVar = (ContextVariable<T>)__castElement.bindingNode.GetContextVariable(id);
            //ctxVar.value = expression;
            compiler.SetImplicitContext(compiler.GetVariable(k_CastRoot));
            // todo -- convert to generic call: __castElement.bindingNode.SetContextVariable<string>(id, "hello")
            // need to use var method = type.GetMethod("name").MakeGenericMethod(type);
            Expression access = Expression.MakeMemberAccess(compiler.GetVariable(k_CastElement), s_Element_BindingNode);
            compiler.Comment(attr.key);
            Expression call = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_GetContextVariable, Expression.Constant(varId));
            Expression cast = Expression.Convert(call, contextVarType);
            ParameterExpression target = compiler.AddVariable(contextVarType, $"ctxVar_{attr.key}");
            compiler.Assign(target, cast);
            compiler.Assign($"ctxVar_{attr.key}.value", compiler.Value(attr.value), false);
        }

        private void CompileKeyboardInputBinding(LinqCompiler compiler, in AttributeDefinition2 attr) {
            LightList<Parameter> parameters = LightList<Parameter>.Get();
            parameters.Add(new Parameter<GenericInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));

            resolvers.Push(new ContextVarAliasResolver(k_InputEventAliasName, typeof(KeyboardInputEvent), NextContextId, AliasResolverType.KeyEvent));

            compiler.SetImplicitContext(compiler.GetVariable(k_CastRoot));

            // todo -- eliminate generated closure by passing in template root and element from input system
            LinqCompiler closure = null;

            ASTNode astNode = ExpressionParser.Parse(attr.value);
            if (astNode.type == ASTNodeType.LambdaExpression) {
                LambdaExpressionNode n = (LambdaExpressionNode) astNode;

                if (n.signature.size == 0) {
                    parameters.Add(new Parameter<GenericInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
                    closure = compiler.CreateClosure(parameters, typeof(void));
                    closure.Statement(n.body);
                }
                else if (n.signature.size == 1) {
                    LambdaArgument signature = n.signature.array[0];

                    if (signature.type != null) {
                        Debug.LogWarning("Input handler lambda should not define a type");
                    }

                    Parameter parameter = parameters.AddReturn(new Parameter<GenericInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
                    closure = compiler.CreateClosure(parameters, typeof(void));
                    ParameterExpression variable = closure.AddVariable(typeof(KeyboardInputEvent), signature.identifier);
                    PropertyInfo property = s_GenericInputEvent_AsKeyInputEvent;
                    closure.Assign(variable, Expression.Property(parameter.expression, property));
                    closure.Statement(n.body);
                }
                else {
                    throw CompileException.InvalidInputHandlerLambda(attr, n.signature.size);
                }
            }
            else {
                parameters.Add(new Parameter<GenericInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
                closure = compiler.CreateClosure(parameters, typeof(void));
                closure.Statement(attr.value);
                LightList<Parameter>.Release(ref parameters);
            }

            LambdaExpression lambda = closure.BuildLambda();

            Expression target = Expression.Field(compiler.GetVariable(k_CastElement), s_UIElement_inputHandlerGroup);

            InputEventType evtType = 0;
            KeyboardModifiers modifiers = KeyboardModifiers.None;
            EventPhase eventPhase = EventPhase.Bubble;

            bool requireFocus = false;

            string evtTypeName = attr.key;

            if (attr.key.Contains(".")) {
                evtTypeName = attr.key.Substring(0, attr.key.IndexOf('.'));
                bool isCapture = attr.key.Contains(".capture");
                bool isShift = attr.key.Contains(".shift");
                bool isControl = attr.key.Contains(".ctrl") || attr.key.Contains(".control");
                bool isCommand = attr.key.Contains(".cmd") || attr.key.Contains(".command");
                bool isAlt = attr.key.Contains(".alt");

                requireFocus = attr.key.Contains(".focus");

                if (isShift) {
                    modifiers |= KeyboardModifiers.Shift;
                }

                if (isControl) {
                    modifiers |= KeyboardModifiers.Control;
                }

                if (isCommand) {
                    modifiers |= KeyboardModifiers.Command;
                }

                if (isAlt) {
                    modifiers |= KeyboardModifiers.Alt;
                }

                if (isCapture) {
                    eventPhase = EventPhase.Capture;
                }
            }

            switch (evtTypeName) {
                case "down":
                    evtType = InputEventType.KeyDown;
                    break;
                case "up":
                    evtType = InputEventType.KeyUp;
                    break;
                case "helddown":
                    evtType = InputEventType.KeyHeldDown;
                    break;
                default:
                    throw new CompileException("Invalid keyboard event in template: " + attr.key);
            }

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(target, s_InputHandlerGroup_AddKeyboardEvent,
                Expression.Constant(evtType),
                Expression.Constant(modifiers),
                Expression.Constant(requireFocus),
                Expression.Constant(eventPhase),
                Expression.Constant(KeyCodeUtil.AnyKey),
                Expression.Constant('\0'),
                lambda
            );

            compiler.RawExpression(expression);

            closure.Release();
            LightList<Parameter>.Release(ref parameters);
            resolvers.Pop();
        }

        private void CompileMouseInputBinding(LinqCompiler compiler, in AttributeDefinition2 attr) {
            // 1 list of event handler typeof(Action<InputEvent>);
            // input call appropriate conversion fn from input event before resolving $evt

            // todo -- eliminate generated closure by passing in template root and element from input system and doing casting as normal in the callback

            resolvers.Push(new ContextVarAliasResolver(k_InputEventAliasName, typeof(MouseInputEvent), NextContextId, AliasResolverType.MouseEvent));

            compiler.SetImplicitContext(compiler.GetVariable(k_CastRoot));
            LinqCompiler closure = null;
            LightList<Parameter> parameters = LightList<Parameter>.Get();

            ASTNode astNode = ExpressionParser.Parse(attr.value);
            if (astNode.type == ASTNodeType.LambdaExpression) {
                LambdaExpressionNode n = (LambdaExpressionNode) astNode;

                if (n.signature.size == 0) {
                    parameters.Add(new Parameter<GenericInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
                    closure = compiler.CreateClosure(parameters, typeof(void));
                    closure.Statement(n.body);
                }
                else if (n.signature.size == 1) {
                    LambdaArgument signature = n.signature.array[0];

                    if (signature.type != null) {
                        Debug.LogWarning("Input handler lambda should not define a type");
                    }

                    Parameter parameter = parameters.AddReturn(new Parameter<GenericInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
                    closure = compiler.CreateClosure(parameters, typeof(void));
                    ParameterExpression variable = closure.AddVariable(typeof(MouseInputEvent), signature.identifier);
                    closure.Assign(variable, Expression.Property(parameter.expression, s_GenericInputEvent_AsMouseInputEvent));
                    closure.Statement(n.body);
                }
                else {
                    throw CompileException.InvalidInputHandlerLambda(attr, n.signature.size);
                }
            }
            else {
                parameters.Add(new Parameter<GenericInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
                closure = compiler.CreateClosure(parameters, typeof(void));
                closure.Statement(attr.value);
                LightList<Parameter>.Release(ref parameters);
            }

            LambdaExpression lambda = closure.BuildLambda();

            Expression target = Expression.Field(compiler.GetVariable(k_CastElement), s_UIElement_inputHandlerGroup);

            InputEventType evtType = 0;
            KeyboardModifiers modifiers = KeyboardModifiers.None;
            EventPhase eventPhase = EventPhase.Bubble;

            bool requireFocus = false;

            string evtTypeName = attr.key;

            if (attr.key.Contains(".")) {
                evtTypeName = attr.key.Substring(0, attr.key.IndexOf('.'));
                bool isCapture = attr.key.Contains(".capture");
                bool isShift = attr.key.Contains(".shift");
                bool isControl = attr.key.Contains(".ctrl") || attr.key.Contains(".control");
                bool isCommand = attr.key.Contains(".cmd") || attr.key.Contains(".command");
                bool isAlt = attr.key.Contains(".alt");

                requireFocus = attr.key.Contains(".focus");

                if (isShift) {
                    modifiers |= KeyboardModifiers.Shift;
                }

                if (isControl) {
                    modifiers |= KeyboardModifiers.Control;
                }

                if (isCommand) {
                    modifiers |= KeyboardModifiers.Command;
                }

                if (isAlt) {
                    modifiers |= KeyboardModifiers.Alt;
                }

                if (isCapture) {
                    eventPhase = EventPhase.Capture;
                }
            }

            switch (evtTypeName) {
                case "click":
                    evtType = InputEventType.MouseClick;
                    break;
                case "down":
                    evtType = InputEventType.MouseDown;
                    break;
                case "up":
                    evtType = InputEventType.MouseUp;
                    break;
                case "enter":
                    evtType = InputEventType.MouseEnter;
                    break;
                case "exit":
                    evtType = InputEventType.MouseExit;
                    break;
                case "helddown":
                    evtType = InputEventType.MouseHeldDown;
                    break;
                case "move":
                    evtType = InputEventType.MouseMove;
                    break;
                case "hover":
                    evtType = InputEventType.MouseHover;
                    break;
                case "scroll":
                    evtType = InputEventType.MouseScroll;
                    break;
                case "context":
                    evtType = InputEventType.MouseContext;
                    break;
                default:
                    throw new CompileException("Invalid mouse event in template: " + attr.key);
            }

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(target, s_InputHandlerGroup_AddMouseEvent,
                Expression.Constant(evtType),
                Expression.Constant(modifiers),
                Expression.Constant(requireFocus),
                Expression.Constant(eventPhase),
                lambda
            );

            compiler.RawExpression(expression);

            closure.Release();
            resolvers.Pop();
        }

        private static void CompileEventBinding(LinqCompiler compiler, in AttributeDefinition2 attr, EventInfo eventInfo) {
            bool hasReturnType = ReflectionUtil.IsFunc(eventInfo.EventHandlerType);
            Type[] eventHandlerTypes = eventInfo.EventHandlerType.GetGenericArguments();
            Type returnType = hasReturnType ? eventHandlerTypes[eventHandlerTypes.Length - 1] : null;

            int parameterCount = eventHandlerTypes.Length;
            if (hasReturnType) {
                parameterCount--;
            }

            compiler.SetImplicitContext(compiler.GetVariable(k_CastRoot));
            LightList<Parameter> parameters = LightList<Parameter>.Get();

            IEnumerable<AliasGenericParameterAttribute> attrNameAliases = eventInfo.GetCustomAttributes<AliasGenericParameterAttribute>();
            for (int i = 0; i < parameterCount; i++) {
                string argName = "arg" + i;
                foreach (AliasGenericParameterAttribute a in attrNameAliases) {
                    if (a.parameterIndex == i) {
                        argName = a.aliasName;
                        break;
                    }
                }

                parameters.Add(new Parameter(eventHandlerTypes[i], argName));
            }

            LinqCompiler closure = compiler.CreateClosure(parameters, returnType);
            LightList<Parameter>.Release(ref parameters);
            closure.Statement(attr.value);
            LambdaExpression lambda = closure.BuildLambda();
            ParameterExpression evtFn = compiler.AddVariable(lambda.Type, "evtFn");
            compiler.Assign(evtFn, lambda);
            compiler.CallStatic(s_EventUtil_Subscribe, compiler.GetVariable(k_CastElement), Expression.Constant(attr.key), evtFn);
            closure.Release();
        }

        private static void CompileConditionalBinding(LinqCompiler compiler, TemplateNode templateNode, in AttributeDefinition2 attr) {
            // cannot have more than 1 conditional    
            try {
                ParameterExpression castElement = compiler.GetVariable(k_CastElement);

                compiler.BeginIsolatedSection();
                if ((attr.flags & AttributeFlags.RootContext) != 0) {
                    compiler.SetImplicitContext(castElement);
                }
                else {
                    compiler.SetImplicitContext(compiler.GetVariable(k_CastRoot));
                }

                MethodCallExpression setEnabled = ExpressionFactory.CallInstanceUnchecked(castElement, s_UIElement_SetEnabled, compiler.Value(attr.value));
                compiler.RawExpression(setEnabled);
                compiler.CommentNewLineBefore($"if=\"{attr.value}\"");

                // if(!element.isEnabled) return
                compiler.IfEqual(Expression.MakeMemberAccess(castElement, s_Element_IsEnabled), Expression.Constant(false), () => { compiler.Return(); });
            }
            catch (Exception e) {
                compiler.EndIsolatedSection();
                Debug.LogError(e);
            }

            compiler.EndIsolatedSection();
        }

        private static void CompileAttributeBinding(LinqCompiler compiler, TemplateNode templateNode, in AttributeDefinition2 attr) {
            // __castElement.SetAttribute("attribute-name", computedValue);
            compiler.CommentNewLineBefore($"{attr.key}=\"{attr.value}\"");
            if ((attr.flags & AttributeFlags.RootContext) != 0) {
                compiler.SetImplicitContext(compiler.GetVariable(k_CastElement));
            }
            else {
                compiler.SetImplicitContext(compiler.GetVariable(k_CastRoot));
            }

            ParameterExpression element = compiler.GetVariable(k_CastElement);
            Expression value = compiler.Value(attr.StrippedValue);
            if (value.Type != typeof(string)) {
                value = ExpressionFactory.CallInstanceUnchecked(value, value.Type.GetMethod("ToString", Type.EmptyTypes));
            }

            compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(element, s_UIElement_SetAttribute, Expression.Constant(attr.key), value));
        }

        private static bool IsConstant(Expression n) {
            while (true) {
                switch (n) {
                    case DefaultExpression _:
                    case ConstantExpression _:
                        return true;
                    case ConditionalExpression conditionalExpression:
                        return IsConstant(conditionalExpression.Test) && IsConstant(conditionalExpression.IfTrue) && IsConstant(conditionalExpression.IfFalse);
                    case UnaryExpression unary:
                        n = unary.Operand;
                        continue;
                    case BinaryExpression binaryExpression:
                        return IsConstant(binaryExpression.Left) && IsConstant(binaryExpression.Right);
                }

                return false;
            }
        }

        private static void CompileStyleBinding(LinqCompiler compiler, TemplateNode templateNode, in AttributeDefinition2 attributeDefinition) {
            ParameterExpression castElement = compiler.GetVariable(k_CastElement);
            ParameterExpression castRoot = compiler.GetVariable(k_CastRoot);


            StyleState styleState = StyleState.Normal;

            string key = attributeDefinition.key;
            if (key.Contains(".")) {
                if (key.StartsWith("hover.")) {
                    styleState = StyleState.Hover;
                    key = key.Substring("hover.".Length);
                }
                else if (key.StartsWith("focus.")) {
                    styleState = StyleState.Focused;
                    key = key.Substring("focus.".Length);
                }
                else if (key.StartsWith("active.")) {
                    styleState = StyleState.Focused;
                    key = key.Substring("active.".Length);
                }
                else {
                    throw CompileException.UnknownStyleState(templateNode.elementRoot.templateShell.filePath, new LineInfo(attributeDefinition.line, attributeDefinition.column), key.Split('.')[0]);
                }
            }

            MemberExpression field = Expression.Field(castElement, s_UIElement_StyleSet);

            compiler.BeginIsolatedSection();

            compiler.SetImplicitContext(castRoot);

            compiler.CommentNewLineBefore($"style.{attributeDefinition.key}=\"{attributeDefinition.value}\"");

            Expression value = compiler.Value(attributeDefinition.value);
            MethodInfo method = typeof(UIStyleSet).GetMethod("Set" + key);
            ParameterInfo[] parameters = method.GetParameters();

            if (!char.IsUpper(key[0])) {
                char[] keyChars = key.ToCharArray();
                keyChars[0] = char.ToUpper(keyChars[0]);
                key = new string(keyChars);
            }

            // hack! for some reason because the type can be by ref (via in) it doesn't report as a generic type
            if (parameters[0].ParameterType.FullName.Contains("System.Nullable")) {
                if (!value.Type.IsNullableType()) {
                    value = ExpressionFactory.Convert(value, ReflectionUtil.CreateGenericType(typeof(Nullable<>), value.Type));
                }
            }

            compiler.RawExpression(Expression.Call(field, method, value, Expression.Constant(styleState)));

            compiler.EndIsolatedSection();
        }

        private static void CompilePropertyBinding(LinqCompiler compiler, TemplateNode templateNode, in AttributeDefinition2 attributeDefinition) {
            LHSStatementChain left = null;
            Expression right = null;
            ParameterExpression castElement = compiler.GetVariable(k_CastElement);
            ParameterExpression castRoot = compiler.GetVariable(k_CastRoot);
            compiler.CommentNewLineBefore($"{attributeDefinition.key}=\"{attributeDefinition.value}\"");
            compiler.BeginIsolatedSection();
            try {
                compiler.SetImplicitContext(castElement);
                left = compiler.AssignableStatement(attributeDefinition.key);
            }
            catch (Exception e) {
                compiler.EndIsolatedSection();
                Debug.LogError(e);
                return;
            }

            //castElement.value = root.value
            compiler.SetImplicitContext(castRoot);

            Expression accessor = compiler.AccessorStatement(left.targetExpression.Type, attributeDefinition.value);

            if (accessor is ConstantExpression) {
                right = accessor;
            }
            else {
                right = compiler.AddVariable(left.targetExpression.Type, "__right");
                compiler.Assign(right, accessor);
            }

            // todo -- I can figure out if a value is constant using IsConstant(expr), use this information to push the expression onto the const compiler

            StructList<ProcessedType.PropertyChangeHandlerDesc> changeHandlers = StructList<ProcessedType.PropertyChangeHandlerDesc>.Get();
            templateNode.processedType.GetChangeHandlers(attributeDefinition.key, changeHandlers);

            bool isProperty = ReflectionUtil.IsProperty(castElement.Type, attributeDefinition.key);

            // if there is a change handler or the member is a property we need to check for changes
            // otherwise field values can be assigned w/o checking
            if (changeHandlers.size > 0 || isProperty) {
                ParameterExpression old = compiler.AddVariable(left.targetExpression.Type, "__oldVal");
                compiler.RawExpression(Expression.Assign(old, left.targetExpression));
                compiler.IfNotEqual(left, right, () => {
                    compiler.Assign(left, right);
                    for (int j = 0; j < changeHandlers.size; j++) {
                        MethodInfo methodInfo = changeHandlers.array[j].methodInfo;
                        ParameterInfo[] parameters = methodInfo.GetParameters();

                        if (parameters.Length == 0) {
                            compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(castElement, methodInfo));
                            continue;
                        }
                        else if (parameters.Length == 1 && parameters[0].ParameterType == right.Type) {
                            compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(castElement, methodInfo, old));
                            continue;
                        }

                        throw CompileException.UnresolvedPropertyChangeHandler(methodInfo.Name, right.Type);
                    }
                });
            }
            else {
                compiler.Assign(left, right);
            }

            compiler.EndIsolatedSection();
            changeHandlers.Release();
        }

        private Expression ResolveAlias(string aliasName, LinqCompiler compiler) {
            if (aliasName == "element") { }

            if (aliasName == "parent") { }

            if (aliasName == "root") { }

            ContextVariableDefinition contextVar = FindContextByName(aliasName);

            if (contextVar != null) {
                return contextVar.Resolve(compiler);
            }

            throw CompileException.UnknownAlias(aliasName);
        }

        private SlotNode GetMatchingInputSlotNode(LightList<SlotNode> inputSlots, SlotNode accepted) {
            for (int i = 0; i < inputSlots.size; i++) {
                // todo -- handle aliasing
                if (inputSlots.array[i].slotName == accepted.slotName) {
                    return inputSlots.array[i];
                }
            }

            return null;
        }

        private static Expression CreateElement(CompilationContext ctx, TemplateNode node) {
            return ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(node.processedType.id),
                ctx.ParentExpr,
                Expression.Constant(node.ChildCount),
                Expression.Constant(node.GetAttributeCount()),
                Expression.Constant(ctx.compiledTemplate.templateId)
            );
        }

        private static ProcessedType ResolveGenericElementType(TemplateNode templateNode) {
            ProcessedType processedType = templateNode.processedType;

            GenericElementTypeResolvedByAttribute attr = processedType.rawType.GetCustomAttribute<GenericElementTypeResolvedByAttribute>();

            if (attr == null) {
                throw CompileException.GenericElementMissingResolver(processedType);
            }

            AttributeDefinition2[] attributes = templateNode.attributes.array;
            s_TypeResolver.Reset();

            s_TypeResolver.SetSignature(new Parameter(templateNode.elementRoot.processedType.rawType, "__root", ParameterFlags.NeverNull));
            s_TypeResolver.SetImplicitContext(s_TypeResolver.GetParameter("__root"));

            for (int i = 0; i < templateNode.attributes.size; i++) {
                if (attributes[i].type == AttributeType.Property && attributes[i].key == attr.propertyName) {
                    if (attributes[i].value.Trim() == "default" || attributes[i].value.Trim() == "null") {
                        throw CompileException.UnresolvableGenericElement(processedType, attributes[i].value);
                    }

                    Type type = s_TypeResolver.GetExpressionType(attributes[i].value);
                    try {
                        Type[] genericArgs = type.GenericTypeArguments;
                        Type newType = ReflectionUtil.CreateGenericType(processedType.rawType, genericArgs);
                        ProcessedType retn = TypeProcessor.AddResolvedGenericElementType(newType, processedType.templateAttr, processedType.tagName);
                        templateNode.processedType = retn;
                        return retn;
                    }
                    catch (ArgumentException ex) {
                        throw ex;
                    }
                }
            }

            throw CompileException.UnresolvedGenericElement(processedType);
        }

        private struct DynamicStyleData {

            public readonly bool isConstant;
            public readonly Type returnType;
            public string text;

            public DynamicStyleData(string text, Type returnType, bool isConstant) {
                this.text = text;
                this.returnType = returnType;
                this.isConstant = isConstant;
            }

        }

    }

}