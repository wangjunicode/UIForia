using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Attributes;
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
using UnityEngine.Assertions;

namespace UIForia.Compilers {

    public class TemplateCompiler {

        private const string k_InputEventAliasName = "evt";
        internal const string k_InputEventParameterName = "__evt";
        private static readonly char[] s_StyleSeparator = {' '};

        private readonly UIForiaLinqCompiler enabledCompiler;
        private readonly UIForiaLinqCompiler createdCompiler;
        private readonly UIForiaLinqCompiler updateCompiler;
        private readonly UIForiaLinqCompiler lateCompiler;
        private readonly UIForiaLinqCompiler typeResolver;

        private readonly CompiledTemplateData templateData;
        private readonly Dictionary<Type, CompiledTemplate> templateMap;
        private readonly TemplateCache templateCache;

        private int contextId = 1;
        private int NextContextId => contextId++;

        private readonly LightStack<LightStack<ContextVariableDefinition>> contextStack;

        private static readonly DynamicStyleListTypeWrapper s_DynamicStyleListTypeWrapper = new DynamicStyleListTypeWrapper();
        private static readonly RepeatKeyFnTypeWrapper s_RepeatKeyFnTypeWrapper = new RepeatKeyFnTypeWrapper();

        private static readonly MethodInfo s_CreateFromPool = typeof(Application).GetMethod(nameof(Application.CreateElementFromPoolWithType));
        private static readonly MethodInfo s_BindingNodePool_Get = typeof(LinqBindingNode).GetMethod("Get", BindingFlags.Static | BindingFlags.Public);
        private static readonly FieldInfo s_StructList_ElementAttr_Array = typeof(StructList<ElementAttribute>).GetField("array");

        private static readonly MethodInfo s_SlotUsageList_PreSize = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.PreSize), BindingFlags.Static | BindingFlags.Public);
        private static readonly FieldInfo s_SlotUsageList_Array = typeof(StructList<SlotUsage>).GetField("array", BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_SlotUsageList_Release = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.Release), BindingFlags.Instance | BindingFlags.Public);

        private static readonly ConstructorInfo s_SlotUsage_Ctor = typeof(SlotUsage).GetConstructor(new[] {typeof(string), typeof(int), typeof(UIElement)});

        private static readonly ConstructorInfo s_TemplateScope_Ctor = typeof(TemplateScope).GetConstructor(new[] {typeof(Application), typeof(StructList<SlotUsage>)});
        private static readonly FieldInfo s_TemplateScope_ApplicationField = typeof(TemplateScope).GetField(nameof(TemplateScope.application));
        private static readonly FieldInfo s_TemplateScope_InnerContext = typeof(TemplateScope).GetField(nameof(TemplateScope.innerSlotContext));
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
        private static readonly MethodInfo s_UIElement_OnBeforePropertyBindings = typeof(UIElement).GetMethod(nameof(UIElement.OnBeforePropertyBindings), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_UIElement_OnAfterPropertyBindings = typeof(UIElement).GetMethod(nameof(UIElement.OnAfterPropertyBindings), BindingFlags.Instance | BindingFlags.Public);
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
        internal static readonly FieldInfo s_UIElement_BindingNode = typeof(UIElement).GetField(nameof(UIElement.bindingNode));

        private static readonly MethodInfo s_LinqBindingNode_CreateLocalContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.CreateLocalContextVariable));
        private static readonly MethodInfo s_LinqBindingNode_GetLocalContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetLocalContextVariable));
        internal static readonly MethodInfo s_LinqBindingNode_GetContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetContextVariable));
        internal static readonly MethodInfo s_LinqBindingNode_GetRepeatItem = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetRepeatItem));
        internal static readonly FieldInfo s_LinqBindingNode_InnerContext = typeof(LinqBindingNode).GetField(nameof(LinqBindingNode.innerContext));

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

        private static readonly PropertyInfo s_GenericInputEvent_AsKeyInputEvent = typeof(GenericInputEvent).GetProperty(nameof(GenericInputEvent.AsKeyInputEvent));
        private static readonly PropertyInfo s_GenericInputEvent_AsMouseInputEvent = typeof(GenericInputEvent).GetProperty(nameof(GenericInputEvent.AsMouseInputEvent));

        private TemplateCompiler(TemplateSettings settings) {
            this.templateCache = new TemplateCache(settings);
            this.templateMap = new Dictionary<Type, CompiledTemplate>();
            this.templateData = new CompiledTemplateData(settings);
            this.updateCompiler = new UIForiaLinqCompiler();
            this.createdCompiler = new UIForiaLinqCompiler();
            this.enabledCompiler = new UIForiaLinqCompiler();
            this.lateCompiler = new UIForiaLinqCompiler();
            this.typeResolver = new UIForiaLinqCompiler();

            Func<string, LinqCompiler, Expression> resolveAlias = ResolveAlias;

            this.createdCompiler.resolveAlias = resolveAlias;
            this.enabledCompiler.resolveAlias = resolveAlias;
            this.updateCompiler.resolveAlias = resolveAlias;
            this.lateCompiler.resolveAlias = resolveAlias;
            this.typeResolver.resolveAlias = resolveAlias;

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
            if (templateMap.TryGetValue(processedType.rawType, out CompiledTemplate retn)) {
                return retn;
            }

            TemplateRootNode templateRootNode = templateCache.GetParsedTemplate(processedType);

            CompiledTemplate compiledTemplate = Compile(templateRootNode);

            return compiledTemplate;
        }

        private CompiledTemplate Compile(TemplateRootNode templateRootNode) {
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

            CompilationContext ctx = new CompilationContext(templateRootNode) {
                namespaces = namespaces,
                rootType = processedType,
                rootParam = rootParam,
                templateScope = scopeParam,
                applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField),
                compiledTemplate = retn,
                ContextExpr = rootParam
            };

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
                    Expression.Constant(CountRealAttributes(templateRootNode.attributes)),
                    Expression.Constant(ctx.compiledTemplate.templateId)
                );

                // root = templateScope.application.CreateFromPool<Type>(attrCount, childCount);
                ctx.Assign(ctx.rootParam, createRootExpression);

                ctx.IfEqualsNull(ctx.rootParam, ctx.PopBlock());
            }

            templateMap[processedType.rawType] = retn;
            ctx.templateRootNode = templateRootNode;

            ProcessAttrsAndVisitChildren(ctx, templateRootNode);
            // VisitChildren(ctx, templateRootNode);
            // UndoContextMods(mods);

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
                templateNode.processedType = ResolveGenericElementType(ctx.templateRootNode.ElementType, templateNode);
            }

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

        private Expression CompileRepeatNode(CompilationContext ctx, RepeatNode repeatNode) {
            if (repeatNode.HasProperty("count")) {
                if (repeatNode.HasProperty("list") || repeatNode.HasProperty("start") || repeatNode.HasProperty("end") || repeatNode.HasProperty("page") || repeatNode.HasProperty("pageSize")) {
                    throw CompileException.UnresolvedRepeatType("count", "list", "start", "page", "pageSize");
                }

                return CompileRepeatCount(ctx, repeatNode);
            }
            else if (repeatNode.HasProperty("list")) {
                if (repeatNode.HasProperty("count")) {
                    throw CompileException.UnresolvedRepeatType("count", "");
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
                Expression.Constant(CountRealAttributes(repeatNode.attributes)),
                Expression.Constant(ctx.compiledTemplate.templateId)
            ));

            MemberExpression templateSpawnIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.templateSpawnId)));
            MemberExpression templateRootContext = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.templateContextRoot)));
            MemberExpression scopeVar = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.scope)));
            MemberExpression indexVarIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.indexVarId)));

            StructList<ContextAliasActions> mods = CompileBindings(ctx, repeatNode, repeatNode.attributes);

            int spawnId = CompileChildrenAsTemplate(ctx, repeatNode, RepeatType.Count, out int _, out int indexVarId);

            UndoContextMods(mods);

            ctx.Assign(templateSpawnIdField, Expression.Constant(spawnId));
            ctx.Assign(templateRootContext, ctx.rootParam);
            ctx.Assign(scopeVar, ctx.templateScope);
            ctx.Assign(indexVarIdField, Expression.Constant(indexVarId));

            return nodeExpr;
        }

        private Expression CompileRepeatList(CompilationContext ctx, RepeatNode repeatNode) {
            ParameterExpression nodeExpr = ctx.ElementExpr;

            repeatNode.processedType = TypeProcessor.GetProcessedType(typeof(UIRepeatElement<>));
            repeatNode.processedType = ResolveGenericElementType(ctx.templateRootNode.ElementType, repeatNode);

            ctx.Comment("new " + TypeNameGenerator.GetTypeName(typeof(UIRepeatCountElement)));
            ctx.Assign(nodeExpr, ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(repeatNode.processedType.id),
                ctx.ParentExpr,
                Expression.Constant(0),
                Expression.Constant(CountRealAttributes(repeatNode.attributes)),
                Expression.Constant(ctx.compiledTemplate.templateId)
            ));

            MemberExpression templateSpawnIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.templateSpawnId)));
            MemberExpression templateRootContext = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.templateContextRoot)));
            MemberExpression scopeVar = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.scope)));
            MemberExpression itemVarIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.itemVarId)));
            MemberExpression indexVarIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.indexVarId)));

            StructList<ContextAliasActions> modifications = CompileBindings(ctx, repeatNode, repeatNode.attributes);

            int spawnId = CompileChildrenAsTemplate(ctx, repeatNode, RepeatType.List, out int itemVarId, out int indexVarId);

            UndoContextMods(modifications);

            ctx.Assign(templateSpawnIdField, Expression.Constant(spawnId));
            ctx.Assign(templateRootContext, ctx.rootParam);
            ctx.Assign(scopeVar, ctx.templateScope);
            ctx.Assign(indexVarIdField, Expression.Constant(indexVarId));
            ctx.Assign(itemVarIdField, Expression.Constant(itemVarId));

            return nodeExpr;
        }

        private int CompileChildrenAsTemplate(CompilationContext ctx, RepeatNode node, RepeatType repeatType, out int itemVarId, out int indexVarId) {
            if (node.ChildCount == 1) {
                return CompileRepeatTemplate(ctx, node, repeatType, out itemVarId, out indexVarId);
            }
            else {
                throw new NotImplementedException("Multi-Child Repeat");

                // todo -- insert a dummy node to wrap the child
                itemVarId = -1;
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
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.Initialize(slotRootParam);
            ctx.compiledTemplate = parentContext.compiledTemplate; // todo -- might be wrong
            ctx.ContextExpr = rootParam;

            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(TypeProcessor.GetProcessedType(typeof(UISlotOverride)).id),
                parentParam,
                Expression.Constant(slotNode.ChildCount),
                Expression.Constant(CountRealAttributes(slotNode.attributes)),
                Expression.Constant(parentContext.compiledTemplate.templateId)
            );

            ctx.Assign(slotRootParam, Expression.Convert(createRootExpression, typeof(UISlotOverride)));

            VisitChildren(ctx, slotNode);

            ctx.Return(slotRootParam);

            compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, parentParam, scopeParam);

            return nodeExpr;
        }

        private enum RepeatType {

            Count,
            List,

        }

        private int CompileRepeatTemplate(CompilationContext parentContext, RepeatNode repeatNode, RepeatType repeatType, out int itemVarId, out int indexVarId) {
            TemplateNode templateNode = repeatNode.children[0];

            CompiledSlot compiledSlot = templateData.CreateSlot(parentContext.compiledTemplate.filePath, parentContext.compiledTemplate.templateName, "__template__", SlotType.Template);

            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression parentParam = Expression.Parameter(typeof(UIElement), "parent");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            CompilationContext ctx = new CompilationContext(parentContext.templateRootNode);

            itemVarId = -1;
            indexVarId = NextContextId;

            if (repeatType != RepeatType.Count) {
                itemVarId = NextContextId;
                contextStack.Peek().Push(new ContextVariableDefinition() {
                    name = repeatNode.GetItemVariableName(),
                    id = itemVarId,
                    type = repeatNode.processedType.rawType.GetGenericArguments()[0],
                    variableType = AliasResolverType.RepeatItem
                });
            }

            contextStack.Peek().Push(new ContextVariableDefinition() {
                name = repeatNode.GetIndexVariableName(),
                id = indexVarId,
                type = typeof(int),
                variableType = AliasResolverType.RepeatIndex
            });

            ctx.rootType = parentContext.rootType;
            ctx.rootParam = rootParam;
            ctx.templateScope = scopeParam;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.compiledTemplate = parentContext.compiledTemplate;
            ctx.ContextExpr = rootParam;
            ctx.Initialize(parentParam);

            ctx.PushScope();

            ctx.Return(Visit(ctx, templateNode));

            contextStack.Peek().Pop();

            if (repeatType != RepeatType.Count) {
                contextStack.Peek().Pop();
            }

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
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.compiledTemplate = parentContext.compiledTemplate;
            ctx.ContextExpr = Expression.Field(scopeParam, s_TemplateScope_InnerContext);
            ctx.Initialize(slotRootParam);

            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(TypeProcessor.GetProcessedType(type).id),
                parentParam,
                Expression.Constant(slotOverrideNode.ChildCount),
                Expression.Constant(CountRealAttributes(slotOverrideNode.attributes)),
                Expression.Constant(parentContext.compiledTemplate.templateId)
            );

            ctx.Assign(slotRootParam, Expression.Convert(createRootExpression, type));

            ExposedVariableData exposedVariableData = new ExposedVariableData();
            exposedVariableData.rootType = definition.rootElementType;
            exposedVariableData.scopedVariables = definition.scopedVariables;
            exposedVariableData.exposedAttrs = definition.exposedAttributes ?? new AttributeDefinition[0];

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
            StructList<ContextAliasActions> contextMods = CompileBindings(ctx, node, node.attributes, exposedVariableData);

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

        private static int CountRealAttributes(StructList<AttributeDefinition> attributes) {
            if (attributes == null) return 0;

            int count = 0;

            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Attribute) {
                    count++;
                }
            }

            return count;
        }

        private Expression CompileExpandedNode(CompilationContext ctx, ExpandedTemplateNode expandedTemplateNode) {
            ProcessedType templateType = expandedTemplateNode.processedType;

            TemplateRootNode innerRoot = templateCache.GetParsedTemplate(templateType);

            CompiledTemplate innerTemplate = GetCompiledTemplate(templateType);

            ParameterExpression nodeExpr = ctx.ElementExpr;

            StructList<AttributeDefinition> attributes = RootAttributeMerger.MergeAttributes(innerRoot.attributes, expandedTemplateNode.attributes);

            ctx.Comment("new " + TypeNameGenerator.GetTypeName(templateType.rawType));
            ctx.Assign(nodeExpr, ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(expandedTemplateNode.processedType.id),
                ctx.ParentExpr,
                Expression.Constant(innerRoot.ChildCount),
                Expression.Constant(CountRealAttributes(attributes)),
                Expression.Constant(ctx.compiledTemplate.templateId)
            ));

            Expression slotUsageExpr = Expression.Default(typeof(StructList<SlotUsage>));

            // check slot overrides
            // make sure the target defines or exposes this slot name
            // if it does, add to slot usage

            // if not, error.

            // check slot exposures
            // if target does not define or expose this slot name, error
            // if exposing slot with default content, push into slot usage

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
            LightList<SlotNode> accepted = innerRoot.slotDefinitionNodes;
            LightList<SlotNode> inputSlots = ctx.templateRootNode.slotDefinitionNodes;
            bool needsRelease = false;

            if (hasOuterSlot) {
                LightList<SlotNode> slotOverrides = expandedTemplateNode.slotOverrideNodes;

                slotUsageExpr = ctx.GetVariable<StructList<SlotUsage>>("slotUsage");

                needsRelease = true;
                ctx.Assign(slotUsageExpr, Expression.Call(null, s_SlotUsageList_PreSize, Expression.Constant(slotOverrides.size)));

                for (int i = 0; i < slotOverrides.size; i++) {
                    SlotNode definition;

                    if (!innerRoot.DefinesSlot(slotOverrides.array[i].slotName, out definition)) {
                        throw CompileException.UnmatchedSlot(slotOverrides.array[i].slotName, slotOverrides.array[i].root.templateShell.filePath);
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

                    int slotId = CompileSlotOverride(ctx, match, accepted.array[i].compiledSlot);

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

            ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_Application_HydrateTemplate, Expression.Constant(innerTemplate.templateId), nodeExpr, templateScopeCtor));

            if (needsRelease) {
                ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(slotUsageExpr, s_SlotUsageList_Release));
            }

            ctx.innerTemplate = innerTemplate;

            CompileBindings(ctx, expandedTemplateNode, attributes);

            ctx.innerTemplate = null;

            return nodeExpr;
        }

        private void InitializeCompilers(LightList<string> namespaces, Type rootType, Type elementType) {
            updateCompiler.Reset();
            enabledCompiler.Reset();
            createdCompiler.Reset();
            lateCompiler.Reset();

            updateCompiler.SetNamespaces(namespaces);
            enabledCompiler.SetNamespaces(namespaces);
            createdCompiler.SetNamespaces(namespaces);
            lateCompiler.SetNamespaces(namespaces);

            Parameter p0 = new Parameter(typeof(UIElement), "__root", ParameterFlags.NeverNull);
            Parameter p1 = new Parameter(typeof(UIElement), "__element", ParameterFlags.NeverNull);

            updateCompiler.SetSignature(p0, p1);
            enabledCompiler.SetSignature(p0, p1);
            createdCompiler.SetSignature(p0, p1);
            lateCompiler.SetSignature(p0, p1);

            updateCompiler.Setup(rootType, elementType);
            enabledCompiler.Setup(rootType, elementType);
            createdCompiler.Setup(rootType, elementType);
            lateCompiler.Setup(rootType, elementType);
        }

        private static void InitializeAttributes(CompilationContext ctx, TemplateNode template, StructList<AttributeDefinition> attributes) {
            if (attributes == null) return;

            int attrIdx = 0;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

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

        private struct StyleRefInfo {

            public int styleId;
            public string styleName;
            public bool fromInnerContext;

        }

        private void CompileNonInstanceStyles(CompilationContext ctx, TemplateNode templateNode, StructList<AttributeDefinition> attributes, ref StructList<DynamicStyleData> dynamicStyleData) {
            StyleSheetReference[] styleRefs = ctx.compiledTemplate.templateMetaData.styleReferences;

            LightList<StyleRefInfo> styleIds = LightList<StyleRefInfo>.Get();

            if (styleRefs != null) {
                for (int i = 0; i < styleRefs.Length; i++) {
                    if (styleRefs[i].styleSheet.TryResolveStyleByTagName(templateNode.tagName, out int id)) {
                        styleIds.Add(new StyleRefInfo() {styleId = id, styleName = "implicit:<" + templateNode.tagName + ">"});
                    }
                }
            }

            if (attributes != null) {
                for (int i = 0; i < attributes.size; i++) {
                    ref AttributeDefinition attr = ref attributes.array[i];

                    if (attr.type != AttributeType.Style) {
                        continue;
                    }

                    StructList<TextExpression> list = StructList<TextExpression>.Get();
                    TextTemplateProcessor.ProcessTextExpressions(attr.value, list);

                    if (TextTemplateProcessor.TextExpressionIsConstant(list)) {
                        string[] parts = attr.value.Split(' ');

                        for (int p = 0; p < parts.Length; p++) {
                            int styleId = ctx.ResolveStyleNameWithFile(parts[p], out string styleDebugName);
                            if (styleId >= 0) {
                                styleIds.Add(new StyleRefInfo() {styleId = styleId, styleName = styleDebugName});
                            }
                        }
                    }
                    else {
                        dynamicStyleData = dynamicStyleData ?? StructList<DynamicStyleData>.Get();

                        for (int s = 0; s < styleIds.size; s++) {
                            dynamicStyleData.Add(new DynamicStyleData(styleIds.array[s].styleId));
                        }

                        for (int s = 0; s < list.size; s++) {
                            ref TextExpression expr = ref list.array[s];
                            if (expr.isExpression) {
                                dynamicStyleData.Add(new DynamicStyleData(expr.text, false));
                            }
                            else {
                                string[] parts = expr.text.Split(s_StyleSeparator, StringSplitOptions.RemoveEmptyEntries);

                                for (int index = 0; index < parts.Length; index++) {
                                    dynamicStyleData.Add(new DynamicStyleData(parts[index], true));
                                }
                            }
                        }
                    }

                    list.QuickRelease();
                }
            }

            if (styleIds.size == 0) {
                styleIds.Release();
                return;
            }

            Expression preSize = ExpressionFactory.CallStaticUnchecked(s_LightList_UIStyleGroupContainer_PreSize, Expression.Constant(styleIds.size));
            ParameterExpression styleList = ctx.GetVariable<LightList<UIStyleGroupContainer>>("styleList");
            ctx.Assign(styleList, preSize);

            Expression styleListArray = Expression.MakeMemberAccess(styleList, s_LightList_UIStyleGroupContainer_Array);
            MemberExpression metaData = Expression.Field(ctx.ElementExpr, s_UIElement_TemplateMetaData);

            for (int i = 0; i < styleIds.size; i++) {
                IndexExpression arrayIndex = Expression.ArrayAccess(styleListArray, Expression.Constant(i));
                ctx.Comment(styleIds.array[i].styleName);
                MethodCallExpression expr = ExpressionFactory.CallInstanceUnchecked(metaData, s_TemplateMetaData_GetStyleById, Expression.Constant(styleIds.array[i].styleId));
                ctx.Assign(arrayIndex, expr);
            }

            MemberExpression style = Expression.Field(ctx.ElementExpr, s_UIElement_StyleSet);
            MethodCallExpression initStyle = ExpressionFactory.CallInstanceUnchecked(style, s_StyleSet_InternalInitialize, styleList);
            ctx.AddStatement(initStyle);
            styleIds.Release();
        }

        // Binding order
        // - conditional
        // - BeforePropertyUpdates() -- if declared
        // - properties & context vars, in declared order 
        // - AfterPropertyUpdates() -- currently called Update()
        // - attributes
        // - styles
        // - AfterBindings()
        // - sync & change

        private StructList<ContextAliasActions> CompileBindings(CompilationContext ctx, TemplateNode templateNode, StructList<AttributeDefinition> attributes, ExposedVariableData exposedVariableData = null) {
            StructList<ContextAliasActions> contextModifications = null;

            InitializeCompilers(ctx.namespaces, ctx.templateRootNode.ElementType, templateNode.processedType.rawType);

            InitializeAttributes(ctx, templateNode, attributes);

            CompileExposedData(exposedVariableData, ref contextModifications);

            CompileConditionalBindings(templateNode, attributes);

            CompileBeforePropertyUpdates(templateNode.processedType);

            CompileAliases(attributes, ref contextModifications);

            CompilePropertyBindingsAndContextVariables(templateNode.processedType, attributes, ref contextModifications);

            CompileTextBinding(templateNode);

            CompileAfterPropertyUpdates(templateNode.processedType);

            CompileAttributeBindings(attributes);

            CompileInstanceStyleBindings(attributes);

            CompileStyleBindings(ctx, templateNode.tagName, attributes);

            // CompileAfterStyleBindings();

            CompileInputHandlers(templateNode.processedType, attributes);

            // CompileSyncWriteback();

            // CompileChangeHandlers();

            BuildBindings(ctx, templateNode);

            return contextModifications;
        }

        private void CompileExposedData(ExposedVariableData exposedData, ref StructList<ContextAliasActions> contextModifications) {
            if (exposedData == null) {
                return;
            }

            ParameterExpression element = createdCompiler.GetElement();
            MemberExpression bindingNode = Expression.Field(element, s_UIElement_BindingNode);
            MemberExpression innerContext = Expression.Field(bindingNode, typeof(LinqBindingNode).GetField(nameof(LinqBindingNode.innerContext)));

            ParameterExpression innerSlotContext_created = createdCompiler.AddVariable(exposedData.rootType, "__innerContext");
            createdCompiler.Assign(innerSlotContext_created, Expression.Convert(innerContext, exposedData.rootType));
            createdCompiler.SetImplicitContext(innerSlotContext_created);

            ParameterExpression innerSlotContext_update = updateCompiler.AddVariable(exposedData.rootType, "__innerContext");
            updateCompiler.Assign(innerSlotContext_update, Expression.Convert(innerContext, exposedData.rootType));
            updateCompiler.SetImplicitContext(innerSlotContext_update);

            contextModifications = contextModifications ?? StructList<ContextAliasActions>.Get();
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
                ref AttributeDefinition attr = ref exposedData.exposedAttrs[i];
                // bindingNode.CreateContextVariable<string>(id);
                ContextVariableDefinition variableDefinition = new ContextVariableDefinition();

                Type expressionType = createdCompiler.GetExpressionType(attr.value);

                variableDefinition.name = attr.key;
                variableDefinition.id = NextContextId;
                variableDefinition.type = expressionType;
                variableDefinition.variableType = AliasResolverType.ContextVariable;

                contextStack.Peek().Push(variableDefinition);

                contextModifications.Add(new ContextAliasActions() {
                    modType = ModType.Context,
                    name = variableDefinition.name
                });

                MethodCallExpression createVariable = CreateLocalContextVariableExpression(variableDefinition, out Type contextVarType);

                CompileAssignContextVariable(updateCompiler, attr, contextVarType, variableDefinition.id);

                createdCompiler.RawExpression(createVariable);
            }

            for (int i = 0; i < exposedData.scopedVariables.Length; i++) {
                contextStack.Peek().RemoveWhere(exposedData.scopedVariables[i], (closure, item) => item.id == closure.id);
            }
        }

        private void CompileConditionalBindings(TemplateNode templateNode, StructList<AttributeDefinition> attributes) {
            if (attributes == null) {
                return;
            }

            bool found = false;
            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                if (attr.type != AttributeType.Conditional) {
                    continue;
                }

                if (found) {
                    throw CompileException.MultipleConditionalBindings(templateNode.TemplateNodeDebugData);
                }

                if ((attr.flags & AttributeFlags.Const) != 0) {
                    CompileConditionalBinding(createdCompiler, attr);
                }
                else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                    CompileConditionalBinding(enabledCompiler, attr);
                }
                else {
                    CompileConditionalBinding(updateCompiler, attr);
                }

                found = true;
            }
        }

        private void CompileBeforePropertyUpdates(ProcessedType processedType) {
            if (processedType.requiresBeforePropertyUpdates) {
                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateCompiler.GetCastElement(), s_UIElement_OnBeforePropertyBindings));
            }
        }

        private void CompileAliases(StructList<AttributeDefinition> attributes, ref StructList<ContextAliasActions> contextModifications) {
            if (attributes == null) return;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

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
        }

        private void CompileAfterPropertyUpdates(ProcessedType processedType) {
            if (processedType.requiresUpdateFn) {
                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateCompiler.GetCastElement(), s_UIElement_OnAfterPropertyBindings));
                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateCompiler.GetCastElement(), s_UIElement_OnUpdate));
            }
        }

        private void CompilePropertyBindingsAndContextVariables(ProcessedType processedType, StructList<AttributeDefinition> attributes, ref StructList<ContextAliasActions> contextModifications) {
            if (attributes == null) return;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                switch (attr.type) {
                    case AttributeType.Context:
                        CompileContextVariable(attr, ref contextModifications);
                        break;

                    case AttributeType.Property: {
                        if (ReflectionUtil.IsEvent(processedType.rawType, attr.key, out EventInfo eventInfo)) {
                            CompileEventBinding(createdCompiler, attr, eventInfo);
                            continue;
                        }

                        if ((attr.flags & AttributeFlags.Sync) != 0) {
                            CompilePropertyBinding(updateCompiler, processedType, attr);
                            CompilePropertyBindingSync(lateCompiler, attr);
                        }
                        else if ((attr.flags & AttributeFlags.Const) != 0) {
                            CompilePropertyBinding(createdCompiler, processedType, attr);
                        }
                        else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                            CompilePropertyBinding(enabledCompiler, processedType, attr);
                        }
                        else {
                            CompilePropertyBinding(updateCompiler, processedType, attr);
                        }

                        break;
                    }
                }
            }
        }

        private void CompileContextVariable(in AttributeDefinition attr, ref StructList<ContextAliasActions> contextModifications) {
            SetImplicitContext(createdCompiler, attr);

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
            ReflectionUtil.TypeArray3[0] = typeof(int);
            ReflectionUtil.TypeArray3[1] = typeof(string);
            ReflectionUtil.TypeArray3[2] = expressionType;
            ConstructorInfo ctor = type.GetConstructor(ReflectionUtil.TypeArray3);

            Expression contextVariable = Expression.New(ctor, Expression.Constant(variableDefinition.id), Expression.Constant(attr.key), Expression.Default(expressionType));
            Expression access = Expression.MakeMemberAccess(createdCompiler.GetCastElement(), s_UIElement_BindingNode);
            Expression createVariable = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_CreateLocalContextVariable, contextVariable);

            createdCompiler.RawExpression(createVariable);

            if ((attr.flags & AttributeFlags.Const) != 0) {
                SetImplicitContext(createdCompiler, attr);
                CompileAssignContextVariable(createdCompiler, attr, type, variableDefinition.id);
            }
            else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                SetImplicitContext(enabledCompiler, attr);
                CompileAssignContextVariable(enabledCompiler, attr, type, variableDefinition.id);
            }
            else {
                SetImplicitContext(updateCompiler, attr);
                CompileAssignContextVariable(updateCompiler, attr, type, variableDefinition.id);
            }
        }

        private void CompileAttributeBindings(StructList<AttributeDefinition> attributes) {
            if (attributes == null) return;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                if (attr.type != AttributeType.Attribute) {
                    continue;
                }

                if ((attr.flags & AttributeFlags.Const) != 0) {
                    /* no op */
                }
                else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                    CompileAttributeBinding(enabledCompiler, attr);
                }
                else {
                    CompileAttributeBinding(updateCompiler, attr);
                }
            }
        }

        private void CompileInstanceStyleBindings(StructList<AttributeDefinition> attributes) {
            if (attributes == null) return;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];
                if (attr.type == AttributeType.InstanceStyle) {
                    CompileInstanceStyleBinding(updateCompiler, attr);
                }
            }
        }

        private void CompileStyleBindings(CompilationContext ctx, string tagName, StructList<AttributeDefinition> attributes) {
            // todo -- need to handle root case, not sure where to get each context from atm, inner context or outer

            if (attributes == null) return;

            StyleSheetReference[] styleRefs = ctx.compiledTemplate.templateMetaData.styleReferences;

            LightList<StyleRefInfo> styleIds = LightList<StyleRefInfo>.Get();

            if (styleRefs != null) {
                for (int i = 0; i < styleRefs.Length; i++) {
                    if (styleRefs[i].styleSheet.TryResolveStyleByTagName(tagName, out int id)) {
                        styleIds.Add(new StyleRefInfo() {styleId = id, styleName = "implicit:<" + tagName + ">"});
                    }
                }
            }

            styleRefs = ctx.innerTemplate?.templateMetaData.styleReferences;

            if (styleRefs != null) {
                for (int i = 0; i < styleRefs.Length; i++) {
                    if (styleRefs[i].styleSheet.TryResolveStyleByTagName(tagName, out int id)) {
                        styleIds.Add(new StyleRefInfo() {styleId = id, styleName = "implicit:<" + tagName + ">", fromInnerContext = true});
                    }
                }
            }

            StructList<TextExpression> list = StructList<TextExpression>.Get();

            int innerContextSplit = -1;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                if (attr.type != AttributeType.Style) {
                    continue;
                }

                TextTemplateProcessor.ProcessTextExpressions(attr.value, list);

                // should only ever be max 2 style nodes, 1 for inner context, 1 for outer
                Assert.IsTrue(innerContextSplit == -1);

                if ((attr.flags & AttributeFlags.InnerContext) != 0) {
                    innerContextSplit = list.size;
                }
            }


            if (list.size > 0) {
                if (TextTemplateProcessor.TextExpressionIsConstant(list)) {
                    for (int i = 0; i < list.size; i++) {
                        int styleId = -1;

                        bool fromInnerContext = i < innerContextSplit;

                        if (fromInnerContext) {
                            Assert.IsNotNull(ctx.innerTemplate);
                            styleId = ctx.innerTemplate.templateMetaData.ResolveStyleNameSlow(list.array[i].text);
                        }
                        else {
                            styleId = ctx.compiledTemplate.templateMetaData.ResolveStyleNameSlow(list.array[i].text);
                        }

                        if (styleId >= 0) {
                            styleIds.Add(new StyleRefInfo() {styleId = styleId, styleName = list.array[i].text, fromInnerContext = fromInnerContext});
                        }
                    }

                    ParameterExpression styleList = ctx.GetVariable<LightList<UIStyleGroupContainer>>("styleList");
                    ctx.Assign(styleList, ExpressionFactory.CallStaticUnchecked(s_LightList_UIStyleGroupContainer_PreSize, Expression.Constant(styleIds.size)));
                    Expression styleListArray = Expression.MakeMemberAccess(styleList, s_LightList_UIStyleGroupContainer_Array);
                    MemberExpression metaData = Expression.Field(ctx.ElementExpr, s_UIElement_TemplateMetaData);
                    MemberExpression bindingNode = Expression.Field(createdCompiler.GetElement(), s_UIElement_BindingNode);
                    MemberExpression innerMetaData = Expression.Field(bindingNode, s_LinqBindingNode_InnerContext);

                    for (int i = 0; i < styleIds.size; i++) {
                        ref StyleRefInfo styleRefInfo = ref styleIds.array[i];
                        
                        IndexExpression arrayIndex = Expression.ArrayAccess(styleListArray, Expression.Constant(i));
                        
                        ctx.Comment(styleRefInfo.styleName);

                        MemberExpression target = styleRefInfo.fromInnerContext ? innerMetaData : metaData;
                        
                        MethodCallExpression expr = ExpressionFactory.CallInstanceUnchecked(target, s_TemplateMetaData_GetStyleById, Expression.Constant(styleRefInfo.styleId));
                        
                        ctx.Assign(arrayIndex, expr);
                    }

                    MemberExpression style = Expression.Field(ctx.ElementExpr, s_UIElement_StyleSet);
                    MethodCallExpression initStyle = ExpressionFactory.CallInstanceUnchecked(style, s_StyleSet_InternalInitialize, styleList);
                    ctx.AddStatement(initStyle);
                    ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(styleList, s_LightList_UIStyleGroupContainer_Release));
                    styleIds.Release();
                }
                else {
                    // todo -- dynamic
                }
            }

            list.Release();
        }

        private void CompileInputHandlers(ProcessedType processedType, StructList<AttributeDefinition> attributes) {
            StructList<InputHandler> handlers = InputCompiler.CompileInputAnnotations(processedType.rawType);

            bool hasHandlers = false;


            if (handlers != null) {
                hasHandlers = true;

                LightList<Parameter> parameters = LightList<Parameter>.Get();

                parameters.Add(new Parameter<GenericInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
                for (int i = 0; i < handlers.size; i++) {
                    ref InputHandler handler = ref handlers.array[i];

                    LinqCompiler closure = createdCompiler.CreateClosure(parameters, typeof(void));

                    if (handler.useEventParameter) {
                        Expression toMouseEvent = Expression.Property(parameters[0].expression, s_GenericInputEvent_AsMouseInputEvent);
                        closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo, toMouseEvent));
                    }
                    else {
                        closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo));
                    }


                    LambdaExpression lambda = closure.BuildLambda();

                    MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddMouseEvent,
                        Expression.Constant(handler.descriptor.handlerType),
                        Expression.Constant(handler.descriptor.modifiers),
                        Expression.Constant(handler.descriptor.requiresFocus),
                        Expression.Constant(handler.descriptor.eventPhase),
                        lambda
                    );

                    createdCompiler.RawExpression(expression);
                    closure.Release();
                }

                LightList<Parameter>.Release(ref parameters);
            }

            const AttributeType k_InputType = AttributeType.Controller | AttributeType.Mouse | AttributeType.Key | AttributeType.Touch;

            if (attributes != null) {
                for (int i = 0; i < attributes.size; i++) {
                    ref AttributeDefinition attr = ref attributes.array[i];
                    if ((attr.type & k_InputType) == 0) {
                        continue;
                    }

                    hasHandlers = true;
                    switch (attr.type) {
                        case AttributeType.Mouse:
                            CompileMouseInputBinding(createdCompiler, attr);
                            break;

                        case AttributeType.Key:
                            CompileKeyboardInputBinding(createdCompiler, attr);
                            break;
                    }
                }
            }

            if (!hasHandlers) {
                return;
            }

            // inputList.QuickRelease();
            // Application.InputSystem.RegisterKeyboardHandler(element);
            // ParameterExpression elementVar = createdCompiler.GetElement();
            // MemberExpression app = Expression.Property(elementVar, typeof(UIElement).GetProperty(nameof(UIElement.application)));
            // MemberExpression inputSystem = Expression.Property(app, typeof(Application).GetProperty(nameof(Application.InputSystem)));
            // MethodInfo method = typeof(InputSystem).GetMethod(nameof(InputSystem.RegisterKeyboardHandler));
            // createdCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(inputSystem, method, elementVar));
        }

        private void BuildBindings(CompilationContext ctx, TemplateNode templateNode) {
            int createdBindingId = -1;
            int enabledBindingId = -1;
            int updateBindingId = -1;
            int lateBindingId = -1;

            // we always have 4 statements because of Initialize(), so only consider compilers with more than 4 statements

            if (createdCompiler.StatementCount > 0) {
                CompiledBinding createdBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnCreate);
                createdBinding.bindingFn = createdCompiler.BuildLambda();
                createdBindingId = createdBinding.bindingId;
            }

            if (enabledCompiler.StatementCount > 0) {
                CompiledBinding enabledBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnEnable);
                enabledBinding.bindingFn = enabledCompiler.BuildLambda();
                enabledBindingId = enabledBinding.bindingId;
            }

            if (updateCompiler.StatementCount > 0) {
                CompiledBinding updateBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnUpdate);
                updateBinding.bindingFn = updateCompiler.BuildLambda();
                updateBindingId = updateBinding.bindingId;
            }

            if (lateCompiler.StatementCount > 0) {
                CompiledBinding lateBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnLateUpdate);
                lateBinding.bindingFn = lateCompiler.BuildLambda();
                lateBindingId = lateBinding.bindingId;
            }

            // create binding node if needed
            if (createdBindingId >= 0 || updateBindingId >= 0 || enabledBindingId >= 0 || lateBindingId >= 0) {
                // scope.application.BindingNodePool.Get(root, element);
                ctx.AddStatement(ExpressionFactory.CallStaticUnchecked(s_BindingNodePool_Get,
                        ctx.applicationExpr,
                        ctx.rootParam,
                        ctx.ElementExpr,
                        ctx.ContextExpr,
                        Expression.Constant(createdBindingId),
                        Expression.Constant(enabledBindingId),
                        Expression.Constant(updateBindingId),
                        Expression.Constant(lateBindingId)
                    )
                );
            }
        }

        private MethodCallExpression CreateLocalContextVariableExpression(ContextVariableDefinition definition, out Type contextVarType) {
            Type type = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), definition.type);
            ReflectionUtil.TypeArray3[0] = typeof(int);
            ReflectionUtil.TypeArray3[1] = typeof(string);
            ReflectionUtil.TypeArray3[2] = definition.type;
            ConstructorInfo ctor = type.GetConstructor(ReflectionUtil.TypeArray3);

            Expression contextVariable = Expression.New(ctor, Expression.Constant(definition.id), Expression.Constant(definition.name), Expression.Default(definition.type));
            Expression access = Expression.MakeMemberAccess(createdCompiler.GetCastElement(), s_UIElement_BindingNode);
            contextVarType = type;
            return ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_CreateLocalContextVariable, contextVariable);
        }

        private void CompileDynamicStyleData(CompilationContext ctx, StructList<DynamicStyleData> dynamicStyleData) {
            if (dynamicStyleData == null) {
                return;
            }

            ParameterExpression castElement = updateCompiler.GetCastElement();

            // todo -- to handle array case we can't use PreSize, need to call Add on the list since size will be dynamic
            ParameterExpression styleList = updateCompiler.AddVariable(
                new Parameter<LightList<UIStyleGroupContainer>>("styleList", ParameterFlags.NeverNull),
                ExpressionFactory.CallStaticUnchecked(s_LightList_UIStyleGroupContainer_Get)
            );

            Expression templateContext = Expression.Field(castElement, s_UIElement_TemplateMetaData);

            updateCompiler.SetImplicitContext(updateCompiler.GetCastRoot());

            for (int s = 0; s < dynamicStyleData.size; s++) {
                ref DynamicStyleData data = ref dynamicStyleData.array[s];

                updateCompiler.Comment(data.text);

                if (data.isConstant) {
                    int styleId = data.styleId ?? ctx.ResolveStyleName(data.text);

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
        }

        private void CompileTextBinding(TemplateNode templateNode) {
            if (!(templateNode is TextNode textNode)) {
                return;
            }

            if (textNode.textExpressionList != null && textNode.textExpressionList.size > 0 && !textNode.IsTextConstant()) {
                updateCompiler.AddNamespace("UIForia.Util");
                updateCompiler.AddNamespace("UIForia.Text");
                updateCompiler.SetImplicitContext(updateCompiler.GetCastRoot());
                StructList<TextExpression> expressionParts = textNode.textExpressionList;

                MemberExpression textValueExpr = Expression.Field(updateCompiler.GetCastElement(), s_TextElement_Text);

                for (int i = 0; i < expressionParts.size; i++) {
                    if (expressionParts[i].isExpression) {
                        Expression val = updateCompiler.Value(expressionParts[i].text);
                        if (val.Type.IsEnum) {
                            MethodCallExpression toString = ExpressionFactory.CallInstanceUnchecked(val, val.Type.GetMethod("ToString", Type.EmptyTypes));
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendString, toString));
                            continue;
                        }

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

                // todo -- this needs to check the TextInfo for equality or whitespace mutations will be ignored and we will return false from equal!!!
                Expression e = updateCompiler.GetCastElement();
                Expression condition = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.EqualsString), new[] {typeof(string)}), textValueExpr);
                condition = Expression.Equal(condition, Expression.Constant(false));
                ConditionalExpression ifCheck = Expression.IfThen(condition, Expression.Block(ExpressionFactory.CallInstanceUnchecked(e, s_TextElement_SetText, s_StringBuilderToString)));

                updateCompiler.RawExpression(ifCheck);
                updateCompiler.RawExpression(s_StringBuilderClear);
            }
        }

        public static void CompileAssignContextVariable(UIForiaLinqCompiler compiler, in AttributeDefinition attr, Type contextVarType, int varId) {
            //ContextVariable<T> ctxVar = (ContextVariable<T>)__castElement.bindingNode.GetContextVariable(id);
            //ctxVar.value = expression;
            Expression access = Expression.MakeMemberAccess(compiler.GetCastElement(), s_UIElement_BindingNode);
            Expression call = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_GetContextVariable, Expression.Constant(varId));
            compiler.Comment(attr.key);
            Expression cast = Expression.Convert(call, contextVarType);
            ParameterExpression target = compiler.AddVariable(contextVarType, "ctxVar_" + attr.key);
            compiler.Assign(target, cast);
            MemberExpression valueField = Expression.Field(target, contextVarType.GetField(nameof(ContextVariable<object>.value)));
            compiler.Assign(valueField, compiler.Value(attr.value));
        }

        private void CompileKeyboardInputBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            LightList<Parameter> parameters = LightList<Parameter>.Get();
            parameters.Add(new Parameter<GenericInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));

            // todo -- handle this
            // resolvers.Push(new ContextVarAliasResolver(k_InputEventAliasName, typeof(KeyboardInputEvent), NextContextId, AliasResolverType.KeyEvent));

            compiler.SetImplicitContext(compiler.GetCastRoot());

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

            Expression target = Expression.Field(compiler.GetCastElement(), s_UIElement_inputHandlerGroup);

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
        }

        private void CompileMouseInputBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            // 1 list of event handler typeof(Action<InputEvent>);
            // input call appropriate conversion fn from input event before resolving $evt

            // todo -- eliminate generated closure by passing in template root and element from input system and doing casting as normal in the callback


            contextStack.Peek().Push(new ContextVariableDefinition() {
                id = NextContextId,
                name = k_InputEventAliasName,
                type = typeof(MouseInputEvent),
                variableType = AliasResolverType.MouseEvent
            });

            compiler.SetImplicitContext(compiler.GetCastRoot());
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

            InputHandlerDescriptor descriptor = InputCompiler.ParseMouseDescriptor(attr.key);

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(compiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddMouseEvent,
                Expression.Constant(descriptor.handlerType),
                Expression.Constant(descriptor.modifiers),
                Expression.Constant(descriptor.requiresFocus),
                Expression.Constant(descriptor.eventPhase),
                lambda
            );

            compiler.RawExpression(expression);

            closure.Release();
            contextStack.Peek().Pop();
        }

        private static void CompileEventBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr, EventInfo eventInfo) {
            bool hasReturnType = ReflectionUtil.IsFunc(eventInfo.EventHandlerType);
            Type[] eventHandlerTypes = eventInfo.EventHandlerType.GetGenericArguments();
            Type returnType = hasReturnType ? eventHandlerTypes[eventHandlerTypes.Length - 1] : null;

            int parameterCount = eventHandlerTypes.Length;
            if (hasReturnType) {
                parameterCount--;
            }

            SetImplicitContext(compiler, attr);
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
            compiler.CallStatic(s_EventUtil_Subscribe, compiler.GetCastElement(), Expression.Constant(attr.key), evtFn);
            closure.Release();
        }

        private static void CompileConditionalBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            // cannot have more than 1 conditional    
            try {
                ParameterExpression element = compiler.GetElement();

                compiler.BeginIsolatedSection();
                SetImplicitContext(compiler, attr);

                MethodCallExpression setEnabled = ExpressionFactory.CallInstanceUnchecked(element, s_UIElement_SetEnabled, compiler.Value(attr.value));
                compiler.RawExpression(setEnabled);
                compiler.CommentNewLineBefore($"if=\"{attr.value}\"");

                // if(!element.isEnabled) return
                compiler.IfEqual(Expression.MakeMemberAccess(element, s_Element_IsEnabled), Expression.Constant(false), () => { compiler.Return(); });
            }
            catch (Exception e) {
                compiler.EndIsolatedSection();
                Debug.LogError(e);
            }

            compiler.EndIsolatedSection();
        }

        private static void CompileAttributeBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            // __castElement.SetAttribute("attribute-name", computedValue);
            compiler.CommentNewLineBefore($"{attr.key}=\"{attr.value}\"");

            SetImplicitContext(compiler, attr);

            ParameterExpression element = compiler.GetElement();
            Expression value = compiler.Value(attr.StrippedValue);

            if (value.Type != typeof(string)) {
                value = ExpressionFactory.CallInstanceUnchecked(value, value.Type.GetMethod("ToString", Type.EmptyTypes));
            }

            compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(element, s_UIElement_SetAttribute, Expression.Constant(attr.key), value));
        }

        private static void CompileInstanceStyleBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attributeDefinition) {
            ParameterExpression castElement = compiler.GetCastElement();

            StyleState styleState = StyleState.Normal;

            string key = attributeDefinition.key;

            if ((attributeDefinition.flags & AttributeFlags.StyleStateActive) == AttributeFlags.StyleStateActive) {
                styleState = StyleState.Active;
            }
            else if ((attributeDefinition.flags & AttributeFlags.StyleStateFocus) == AttributeFlags.StyleStateFocus) {
                styleState = StyleState.Focused;
            }
            else if ((attributeDefinition.flags & AttributeFlags.StyleStateHover) == AttributeFlags.StyleStateHover) {
                styleState = StyleState.Hover;
            }

            MemberExpression field = Expression.Field(castElement, s_UIElement_StyleSet);

            compiler.BeginIsolatedSection();

            SetImplicitContext(compiler, attributeDefinition);

            compiler.CommentNewLineBefore($"style.{attributeDefinition.key}=\"{attributeDefinition.value}\"");

            Expression value = compiler.Value(attributeDefinition.value);

            if (!char.IsUpper(key[0])) {
                char[] keyChars = key.ToCharArray();
                keyChars[0] = char.ToUpper(keyChars[0]);
                key = new string(keyChars);
            }

            MethodInfo method = typeof(UIStyleSet).GetMethod("Set" + key);

            if (method == null) {
                throw CompileException.UnknownStyleMapping();
            }

            ParameterInfo[] parameters = method.GetParameters();

            // hack! for some reason because the type can be by ref (via in) it doesn't report as a generic type
            if (parameters[0].ParameterType.FullName.Contains("System.Nullable")) {
                if (!value.Type.IsNullableType()) {
                    Type targetType = parameters[0].ParameterType.GetGenericArguments()[0];
                    value = Expression.Convert(value, ReflectionUtil.CreateGenericType(typeof(Nullable<>), targetType));
                }
            }

            compiler.RawExpression(Expression.Call(field, method, value, Expression.Constant(styleState)));

            compiler.EndIsolatedSection();
        }

        private static bool HasTypeWrapper(Type type, out ITypeWrapper typeWrapper) {
            if (type == typeof(RepeatItemKey)) {
                typeWrapper = s_RepeatKeyFnTypeWrapper;
                return true;
            }

            typeWrapper = null;
            return false;
        }

        private void CompilePropertyBindingSync(UIForiaLinqCompiler compiler, in AttributeDefinition attributeDefinition) {
            LHSStatementChain left;
            Expression right = null;
            ParameterExpression castElement = compiler.GetCastElement();
            ParameterExpression castRoot = compiler.GetCastRoot();
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

            compiler.SetImplicitContext(castRoot);
            LHSStatementChain assignableStatement = compiler.AssignableStatement(attributeDefinition.value);

            compiler.SetImplicitContext(castElement);
            compiler.Assign(assignableStatement, Expression.Field(castElement, castElement.Type.GetField(attributeDefinition.key)));
            compiler.EndIsolatedSection();
        }

        private void CompilePropertyBinding(UIForiaLinqCompiler compiler, ProcessedType processedType, in AttributeDefinition attributeDefinition) {
            LHSStatementChain left;
            Expression right = null;
            ParameterExpression castElement = compiler.GetCastElement();
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

            SetImplicitContext(compiler, attributeDefinition);

            if (ReflectionUtil.IsFunc(left.targetExpression.Type)) {
                Type[] generics = left.targetExpression.Type.GetGenericArguments();
                Type target = generics[generics.Length - 1];
                if (HasTypeWrapper(target, out ITypeWrapper wrapper)) {
                    right = compiler.TypeWrapStatement(wrapper, left.targetExpression.Type, attributeDefinition.value);
                }
            }

            if (right == null) {
                Expression accessor = compiler.AccessorStatement(left.targetExpression.Type, attributeDefinition.value);

                if (accessor is ConstantExpression) {
                    right = accessor;
                }
                else {
                    right = compiler.AddVariable(left.targetExpression.Type, "__right");
                    compiler.Assign(right, accessor);
                }
            }

            // todo -- I can figure out if a value is constant using IsConstant(expr), use this information to push the expression onto the const compiler

            // todo -- late change handlers (OnPropertySynchronized)
            StructList<ProcessedType.PropertyChangeHandlerDesc> changeHandlers = StructList<ProcessedType.PropertyChangeHandlerDesc>.Get();
            processedType.GetChangeHandlers(attributeDefinition.key, changeHandlers);

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

                        if (parameters.Length == 1 && parameters[0].ParameterType == right.Type) {
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
                Expression.Constant(CountRealAttributes(node.attributes)),
                Expression.Constant(ctx.compiledTemplate.templateId)
            );
        }

        private ProcessedType ResolveGenericElementType(Type rootType, TemplateNode templateNode) {
            ProcessedType processedType = templateNode.processedType;

            Type generic = processedType.rawType;

            Type[] arguments = processedType.rawType.GetGenericArguments();

            Type[] resolvedTypes = new Type[arguments.Length];

            typeResolver.Reset();

            typeResolver.SetSignature(new Parameter(rootType, "__root", ParameterFlags.NeverNull));
            typeResolver.SetImplicitContext(typeResolver.GetParameter("__root"));
            typeResolver.resolveAlias = ResolveAlias;
            typeResolver.Setup(rootType, null);

            for (int i = 0; i < templateNode.attributes.size; i++) {
                ref AttributeDefinition attr = ref templateNode.attributes.array[i];

                // todo -- make sure this is not a property chain
                if (attr.type != AttributeType.Property) continue;

                if (ReflectionUtil.IsField(generic, attr.key, out FieldInfo fieldInfo)) {
                    HandleType(fieldInfo.FieldType, attr);
                }
                else if (ReflectionUtil.IsProperty(generic, attr.key, out PropertyInfo propertyInfo)) {
                    HandleType(propertyInfo.PropertyType, attr);
                }
            }

            for (int i = 0; i < arguments.Length; i++) {
                if (resolvedTypes[i] == null) {
                    throw CompileException.UnresolvedGenericElement(processedType);
                }
            }

            Type newType = ReflectionUtil.CreateGenericType(processedType.rawType, resolvedTypes);
            ProcessedType retn = TypeProcessor.AddResolvedGenericElementType(newType, processedType.templateAttr, processedType.tagName);
            return retn;

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

                    Type type = typeResolver.GetExpressionType(attr.value);
                    Type[] fieldArgs = inputType.GetGenericArguments();
                    Type[] typeArgs = type.GetGenericArguments();

                    Assert.AreEqual(fieldArgs.Length, typeArgs.Length);

                    for (int a = 0; a < fieldArgs.Length; a++) {
                        string genericName = fieldArgs[a].Name;
                        int typeIndex = GetTypeIndex(arguments, genericName);
                        Assert.IsTrue(typeIndex != -1);

                        if (resolvedTypes[typeIndex] != null) {
                            if (resolvedTypes[typeIndex] != typeArgs[a]) {
                                throw CompileException.DuplicateResolvedGenericArgument(templateNode.tagName, inputType.Name, resolvedTypes[typeIndex], typeArgs[a]);
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
                            throw CompileException.DuplicateResolvedGenericArgument(templateNode.tagName, inputType.Name, resolvedTypes[typeIndex], type);
                        }
                    }

                    resolvedTypes[typeIndex] = type;
                }
            }
        }

        private static void SetImplicitContext(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            if ((attr.flags & AttributeFlags.InnerContext) != 0) {
                compiler.SetImplicitContext(compiler.GetCastElement());
            }
            else {
                compiler.SetImplicitContext(compiler.GetCastRoot());
            }
        }


        private struct DynamicStyleData {

            public readonly bool isConstant;
            public readonly string text;
            public readonly int? styleId;

            public DynamicStyleData(string text, bool isConstant) {
                this.text = text;
                this.isConstant = isConstant;
                this.styleId = null;
            }

            public DynamicStyleData(int styleId) {
                this.text = null;
                this.isConstant = true;
                this.styleId = styleId;
            }

        }

    }

}