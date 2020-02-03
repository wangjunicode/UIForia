using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Templates;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia.Compilers {

    public class TemplateCompiler {

        internal const string k_InputEventParameterName = "__evt";
        private static readonly char[] s_StyleSeparator = {' '};

        private readonly UIForiaLinqCompiler enabledCompiler;
        private readonly UIForiaLinqCompiler createdCompiler;
        private readonly UIForiaLinqCompiler updateCompiler;
        private readonly UIForiaLinqCompiler lateCompiler;
        private readonly UIForiaLinqCompiler typeResolver;

        private Expression changeHandlerCurrentValue;
        private Expression changeHandlerPreviousValue;
        private Expression currentEvent;

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

        private static readonly ConstructorInfo s_TemplateScope_Ctor = typeof(TemplateScope).GetConstructor(new[] {typeof(Application), typeof(bool)});
        private static readonly FieldInfo s_TemplateScope_ApplicationField = typeof(TemplateScope).GetField(nameof(TemplateScope.application));
        private static readonly FieldInfo s_TemplateScope_InnerContext = typeof(TemplateScope).GetField(nameof(TemplateScope.innerSlotContext));
        private static readonly MethodInfo s_TemplateScope_AddSlotForward = typeof(TemplateScope).GetMethod(nameof(TemplateScope.AddSlotForward));
        private static readonly MethodInfo s_TemplateScope_AddSlotOverride = typeof(TemplateScope).GetMethod(nameof(TemplateScope.AddSlotOverride));

        private static readonly ConstructorInfo s_ElementAttributeCtor = typeof(ElementAttribute).GetConstructor(new[] {typeof(string), typeof(string)});
        private static readonly FieldInfo s_ElementAttributeList = typeof(UIElement).GetField("attributes", BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_TextElement_Text = typeof(UITextElement).GetField(nameof(UITextElement.text), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_TextElement_SetText = typeof(UITextElement).GetMethod(nameof(UITextElement.SetText), BindingFlags.Instance | BindingFlags.Public);

        private static readonly FieldInfo s_UIElement_StyleSet = typeof(UIElement).GetField(nameof(UIElement.style), BindingFlags.Instance | BindingFlags.Public);
        private static readonly FieldInfo s_UIElement_TemplateMetaData = typeof(UIElement).GetField(nameof(UIElement.templateMetaData), BindingFlags.Instance | BindingFlags.Public);
        private static readonly PropertyInfo s_UIElement_Application = typeof(UIElement).GetProperty(nameof(UIElement.application), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_UIElement_OnUpdate = typeof(UIElement).GetMethod(nameof(UIElement.OnUpdate), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_UIElement_OnBeforePropertyBindings = typeof(UIElement).GetMethod(nameof(UIElement.OnBeforePropertyBindings), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_UIElement_OnAfterPropertyBindings = typeof(UIElement).GetMethod(nameof(UIElement.OnAfterPropertyBindings), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_UIElement_SetAttribute = typeof(UIElement).GetMethod(nameof(UIElement.SetAttribute), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_UIElement_SetEnabled = typeof(UIElement).GetMethod(nameof(UIElement.SetEnabled), BindingFlags.Instance | BindingFlags.Public);
        private static readonly FieldInfo s_UIElement_Parent = typeof(UIElement).GetField(nameof(UIElement.parent), BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo s_StyleSet_InternalInitialize = typeof(UIStyleSet).GetMethod(nameof(UIStyleSet.internal_Initialize), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_StyleSet_SetBaseStyles = typeof(UIStyleSet).GetMethod(nameof(UIStyleSet.SetBaseStyles), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_StyleSet_AddBaseStyle = typeof(UIStyleSet).GetMethod(nameof(UIStyleSet.internal_AddBaseStyle));

        private static readonly MethodInfo s_TemplateMetaData_GetStyleById = typeof(TemplateMetaData).GetMethod(nameof(TemplateMetaData.GetStyleById), BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo s_LightList_UIStyleGroupContainer_Get = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.Get), BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_LightList_UIStyleGroupContainer_Release = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.Release), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo s_LightList_UIStyleGroupContainer_Add = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.Add), BindingFlags.Public | BindingFlags.Instance);

        private static readonly MethodInfo s_Application_CreateSlot = typeof(Application).GetMethod(nameof(Application.CreateSlot), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo s_Application_HydrateTemplate = typeof(Application).GetMethod(nameof(Application.HydrateTemplate), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo s_Application_GetTemplateMetaData = typeof(Application).GetMethod(nameof(Application.GetTemplateMetaData), BindingFlags.Public | BindingFlags.Instance);

        private static readonly MethodInfo s_InputHandlerGroup_AddMouseEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddMouseEvent));
        private static readonly MethodInfo s_InputHandlerGroup_AddDragCreator = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddDragCreator));
        private static readonly MethodInfo s_InputHandlerGroup_AddDragEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddDragEvent));
        private static readonly MethodInfo s_InputHandlerGroup_AddKeyboardEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddKeyboardEvent));

        private static readonly PropertyInfo s_Element_IsEnabled = typeof(UIElement).GetProperty(nameof(UIElement.isEnabled));
        internal static readonly FieldInfo s_UIElement_BindingNode = typeof(UIElement).GetField(nameof(UIElement.bindingNode));

        private static readonly MethodInfo s_LinqBindingNode_CreateLocalContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.CreateLocalContextVariable));
        internal static readonly MethodInfo s_LinqBindingNode_GetContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetContextVariable));
        internal static readonly MethodInfo s_LinqBindingNode_GetRepeatItem = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetRepeatItem));

        private static readonly MethodInfo s_EventUtil_Subscribe = typeof(EventUtil).GetMethod(nameof(EventUtil.Subscribe));

        private static readonly MethodInfo s_DynamicStyleList_Flatten = typeof(DynamicStyleList).GetMethod(nameof(DynamicStyleList.Flatten));

        private static readonly Expression s_StringBuilderExpr = Expression.Field(null, typeof(StringUtil), nameof(StringUtil.s_CharStringBuilder));
        private static readonly Expression s_StringBuilderClear = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod("Clear"));
        private static readonly Expression s_StringBuilderToString = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod("ToString", Type.EmptyTypes));
        private static readonly MethodInfo s_StringBuilder_AppendString = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(string)});
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

            ProcessedType appRoot = TypeProcessor.GetProcessedType(appRootType);

            TemplateRootNode templateRootNode = templateCache.GetParsedTemplate(appRoot);

            Compile(templateRootNode, true);

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

        private CompilationContext CompileTemplateMetaData(TemplateRootNode templateRootNode) {
            CompiledTemplate compiledTemplate = templateData.CreateTemplate(templateRootNode.templateShell.filePath, templateRootNode.templateName);

            LightList<string> namespaces = new LightList<string>(4);

            if (templateRootNode.templateShell.usings != null) {
                for (int i = 0; i < templateRootNode.templateShell.usings.size; i++) {
                    namespaces.Add(templateRootNode.templateShell.usings[i].namespaceName);
                }
            }

            ProcessedType processedType = templateRootNode.processedType;

            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            compiledTemplate.elementType = processedType;

            CompilationContext ctx = new CompilationContext(templateRootNode) {
                namespaces = namespaces,
                rootType = processedType,
                rootParam = rootParam,
                templateScope = scopeParam,
                applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField),
                compiledTemplate = compiledTemplate,
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
                compiledTemplate.templateMetaData.styleReferences = ctx.styleSheets.ToArray();
            }

            templateMap[processedType.rawType] = compiledTemplate;

            return ctx;
        }

        private CompiledTemplate Compile(TemplateRootNode templateRootNode, bool isRoot = false) {
            CompilationContext ctx = CompileTemplateMetaData(templateRootNode);
            contextStack.Push(new LightStack<ContextVariableDefinition>());

            ProcessedType processedType = templateRootNode.processedType;

            if (isRoot) {
                ctx.Comment("new " + TypeNameGenerator.GetTypeName(processedType.rawType));

                Expression createRootExpression = ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                    Expression.Constant(processedType.id),
                    Expression.Default(typeof(UIElement)), // root has no parent
                    Expression.Constant(templateRootNode.ChildCount),
                    Expression.Constant(CountRealAttributes(templateRootNode.attributes)),
                    Expression.Constant(ctx.compiledTemplate.templateId)
                );
                ctx.Assign(ctx.rootParam, createRootExpression);
                ProcessAttrsAndVisitChildren(ctx, templateRootNode);
            }
            else {
                VisitChildren(ctx, templateRootNode);
            }

            ctx.templateRootNode = templateRootNode;

            ctx.Return(ctx.rootParam);
            ctx.compiledTemplate.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), (ParameterExpression) ctx.rootParam, (ParameterExpression) ctx.templateScope);
            contextStack.Pop();
            return ctx.compiledTemplate;
        }

        private void VisitChildren(CompilationContext ctx, TemplateNode templateNode) {
            if (templateNode.ChildCount == 0) {
                return;
            }

            ctx.PushScope();

            //  Expression parentChildList = Expression.Field(ctx.ParentExpr, s_Element_ChildrenList);

            // Expression parentChildListArray = Expression.Field(parentChildList, s_LightList_Element_Array);

            for (int i = 0; i < templateNode.ChildCount; i++) {
                Visit(ctx, templateNode[i]);
                // childList.array[i] = targetElement_x;
                //ctx.Assign(Expression.ArrayAccess(parentChildListArray, Expression.Constant(i)), visit);
            }

            ctx.PopScope();
        }

        private Expression Visit(CompilationContext ctx, TemplateNode templateNode) {
            if (templateNode is RepeatNode repeatNode) {
                return CompileRepeatNode(ctx, repeatNode);
            }

            if (templateNode.processedType.IsUnresolvedGeneric) {
                templateNode.processedType = ResolveGenericElementType(ctx.namespaces, ctx.templateRootNode.ElementType, templateNode);
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

            ctx.CommentNewLineBefore("new " + TypeNameGenerator.GetTypeName(typeof(UIRepeatCountElement)));
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

            StructList<ContextAliasActions> mods = CompileBindings(ctx, repeatNode, repeatNode.attributes).contextModifications;

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
            repeatNode.processedType = ResolveGenericElementType(ctx.namespaces, ctx.templateRootNode.ElementType, repeatNode);

            ctx.CommentNewLineBefore("new " + TypeNameGenerator.GetTypeName(typeof(UIRepeatCountElement)));
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

            BindingOutput bindingOutput = CompileBindings(ctx, repeatNode, repeatNode.attributes);

            int spawnId = CompileChildrenAsTemplate(ctx, repeatNode, RepeatType.List, out int itemVarId, out int indexVarId);

            UndoContextMods(bindingOutput.contextModifications);

            ctx.Assign(templateSpawnIdField, Expression.Constant(spawnId));
            ctx.Assign(templateRootContext, ctx.rootParam);
            ctx.Assign(scopeVar, ctx.templateScope);
            ctx.Assign(indexVarIdField, Expression.Constant(indexVarId));
            ctx.Assign(itemVarIdField, Expression.Constant(itemVarId));

            return nodeExpr;
        }

        private int CompileChildrenAsTemplate(CompilationContext ctx, RepeatNode node, RepeatType repeatType, out int itemVarId, out int indexVarId) {
            return CompileRepeatTemplate(ctx, node, repeatType, out itemVarId, out indexVarId);
            // if (node.ChildCount == 1) {
            // }
            // else {
            // ContainerNode containerNode = new ContainerNode(node.root, node, TypeProcessor.GetProcessedType(typeof(RepeatMultiChildContainerElement)), null, default);
            // containerNode.children = LightList<TemplateNode>.Get();

            // for (int i = 0; i < node.ChildCount; i++) {
            // containerNode.children.Add(node.children[i]);
            // }

            // return CompileRepeatTemplate(ctx, node, containerNode, repeatType, out itemVarId, out indexVarId);
            // }
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

            parentContext.compiledTemplate.AddSlot(compiledSlot);

            compiledSlot.rootElementType = parentContext.rootType.rawType;
            compiledSlot.scopedVariables = CloneContextStack();
            compiledSlot.exposedAttributes = slotNode.GetAttributes(AttributeType.Expose);

            Expression nodeExpr = parentContext.ElementExpr;

            Expression slotNameExpr = Expression.Constant(slotNode.slotName);

            parentContext.Assign(parentContext.ElementExpr, Expression.Call(
                parentContext.applicationExpr,
                s_Application_CreateSlot,
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

            ParameterExpression slotRootParam = ctx.GetVariable(slotNode.processedType.rawType, "slotRoot");
            ctx.rootType = parentContext.rootType;
            ctx.rootParam = rootParam;
            ctx.templateScope = scopeParam;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.Initialize(slotRootParam);
            ctx.compiledTemplate = parentContext.compiledTemplate; // todo -- might be wrong
            ctx.ContextExpr = rootParam;
            ctx.namespaces = parentContext.namespaces;

            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(slotNode.processedType.id),
                parentParam,
                Expression.Constant(slotNode.ChildCount),
                Expression.Constant(CountRealAttributes(slotNode.attributes)),
                Expression.Constant(parentContext.compiledTemplate.templateId)
            );

            ctx.Assign(slotRootParam, Expression.Convert(createRootExpression, slotNode.processedType.rawType));

            StructList<ContextAliasActions> contextMods = CompileBindings(ctx, slotNode, slotNode.attributes, null).contextModifications;

            VisitChildren(ctx, slotNode);

            UndoContextMods(contextMods);

            ctx.Return(slotRootParam);

            compiledSlot.originalAttributes = slotNode.attributes;
            compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, parentParam, scopeParam);

            return nodeExpr;
        }

        // compile slot attributes
        // each attribute needs a depth attached to it to reference that context, not just inner/outer
        // might need to store referenced namespaces too
        // might need to store context stack per level
        private CompiledSlot CompileSlotOverride(CompilationContext parentContext, SlotNode slotOverrideNode, CompiledSlot toOverride, Type type = null) {
            if (type == null) type = slotOverrideNode.processedType.rawType;

            CompiledSlot compiledSlot = templateData.CreateSlot(parentContext.compiledTemplate.filePath, parentContext.compiledTemplate.templateName, slotOverrideNode.slotName, slotOverrideNode.slotType);

            compiledSlot.rootElementType = parentContext.rootType.rawType;
            compiledSlot.scopedVariables = CloneContextStack();
            compiledSlot.exposedAttributes = slotOverrideNode.GetAttributes(AttributeType.Expose);

            parentContext.compiledTemplate.AddSlot(compiledSlot);

            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression parentParam = Expression.Parameter(typeof(UIElement), "parent");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            StructList<AttributeDefinition> attributes = RootAttributeMerger.MergeAttributes(toOverride.originalAttributes, slotOverrideNode.attributes);

            CompilationContext ctx = new CompilationContext(parentContext.templateRootNode);

            ParameterExpression slotRootParam = ctx.GetVariable(type, "slotRoot");
            ctx.rootType = parentContext.rootType;
            ctx.rootParam = rootParam;
            ctx.templateScope = scopeParam;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.compiledTemplate = parentContext.compiledTemplate;
            ctx.ContextExpr = Expression.Field(scopeParam, s_TemplateScope_InnerContext);
            ctx.namespaces = parentContext.namespaces;

            ctx.Initialize(slotRootParam);

            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(TypeProcessor.GetProcessedType(type).id),
                parentParam,
                Expression.Constant(slotOverrideNode.ChildCount),
                Expression.Constant(CountRealAttributes(attributes)),
                Expression.Constant(parentContext.compiledTemplate.templateId)
            );

            ctx.Assign(slotRootParam, Expression.Convert(createRootExpression, type));

            LightList<ExposedVariableData> exposedVariableDataList = new LightList<ExposedVariableData>();

            if (toOverride.exposedVariableDataList != null) {
                exposedVariableDataList.AddRange(toOverride.exposedVariableDataList);
            }

            ExposedVariableData exposedVariableData = new ExposedVariableData();
            exposedVariableData.rootType = toOverride.rootElementType;
            exposedVariableData.scopedVariables = toOverride.scopedVariables;
            exposedVariableData.exposedAttrs = toOverride.exposedAttributes ?? new AttributeDefinition[0];
            exposedVariableData.originSlotId = toOverride.slotId;

            exposedVariableDataList.Add(exposedVariableData);

            compiledSlot.exposedVariableDataList = exposedVariableDataList;

            SlotNode node = slotOverrideNode;

            MethodInfo method = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.InitializeContextArray));
            StructList<ContextAliasActions> contextMods = CompileBindings(ctx, node, attributes, exposedVariableDataList).contextModifications;

            ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(
                Expression.Field(slotRootParam, s_UIElement_BindingNode),
                method,
                Expression.Constant(compiledSlot.slotName),
                ctx.templateScope,
                Expression.Constant(exposedVariableDataList.size))
            );

            VisitChildren(ctx, node);

            UndoContextMods(contextMods);

            ctx.Return(slotRootParam);

            compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, parentParam, scopeParam);

            return compiledSlot;
        }

        private enum RepeatType {

            Count,
            List,

        }

        private int CompileRepeatTemplate(CompilationContext parentContext, RepeatNode repeatNode, RepeatType repeatType, out int itemVarId, out int indexVarId) {
            CompiledSlot compiledSlot = templateData.CreateSlot(parentContext.compiledTemplate.filePath, parentContext.compiledTemplate.templateName, "__template__", SlotType.Template);

            throw new NotImplementedException("Re do repeat compilation, cannot be treated as a slot");
            // parentContext.compiledTemplate.AddSlot(compiledSlot);

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
            ctx.namespaces = parentContext.namespaces;

            ctx.Initialize(parentParam);

            ctx.PushScope();

            if (repeatNode.ChildCount != 1) {
                ctx.Assign(ctx.ElementExpr, ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                    Expression.Constant(TypeProcessor.GetProcessedType(typeof(RepeatMultiChildContainerElement)).id),
                    ctx.ParentExpr,
                    Expression.Constant(repeatNode.ChildCount),
                    Expression.Constant(0),
                    Expression.Constant(ctx.compiledTemplate.templateId)
                ));
                ctx.AddStatement(ExpressionFactory.CallStaticUnchecked(s_BindingNodePool_Get,
                        ctx.applicationExpr,
                        ctx.rootParam,
                        ctx.ElementExpr,
                        ctx.ContextExpr,
                        Expression.Constant(-1),
                        Expression.Constant(-1),
                        Expression.Constant(-1),
                        Expression.Constant(-1)
                    )
                );
                VisitChildren(ctx, repeatNode);
                ctx.Return(ctx.ElementExpr);
            }
            else {
                ctx.Return(Visit(ctx, repeatNode.children[0]));
            }

            contextStack.Peek().Pop();

            if (repeatType != RepeatType.Count) {
                contextStack.Peek().Pop();
            }

            compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, parentParam, scopeParam);

            return compiledSlot.slotId;
        }

        private Expression CompileTextNode(CompilationContext ctx, TextNode textNode) {
            ParameterExpression nodeExpr = ctx.ElementExpr;
            ProcessedType processedType = textNode.processedType;

            ctx.CommentNewLineBefore("new " + TypeNameGenerator.GetTypeName(processedType.rawType) + " " + textNode.lineInfo);

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

        private void ProcessAttrsAndVisitChildren(CompilationContext ctx, TemplateNode node, LightList<ExposedVariableData> exposedVariableData = null) {
            StructList<ContextAliasActions> contextMods = CompileBindings(ctx, node, node.attributes, exposedVariableData).contextModifications;

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

            ctx.CommentNewLineBefore("new " + TypeNameGenerator.GetTypeName(processedType.rawType));

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

        // todo -- not working for slots & exposing
        // context vars also seem not to work
        // one issue is that repeat uses regular slot compilation, might need to change
        // other issue is that we match slots by name, probably need unique ids for this
        // might want to pull out Children as a formal visit concept and not just a type distinction
        private Expression CompileExpandedNode(CompilationContext ctx, ExpandedTemplateNode expandedTemplateNode) {
            ProcessedType templateType = expandedTemplateNode.processedType;

            TemplateRootNode innerRoot = templateCache.GetParsedTemplate(templateType);

            CompiledTemplate innerTemplate = GetCompiledTemplate(templateType);

            ParameterExpression nodeExpr = ctx.ElementExpr;

            StructList<AttributeDefinition> attributes = RootAttributeMerger.MergeAttributes(innerRoot.attributes, expandedTemplateNode.attributes);

            ctx.CommentNewLineBefore("new " + TypeNameGenerator.GetTypeName(templateType.rawType));
            ctx.Assign(nodeExpr, ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(expandedTemplateNode.processedType.id),
                ctx.ParentExpr,
                Expression.Constant(innerRoot.ChildCount),
                Expression.Constant(CountRealAttributes(attributes)),
                Expression.Constant(ctx.compiledTemplate.templateId)
            ));

            bool hasForwardOrOverrides = expandedTemplateNode.slotOverrideNodes != null && expandedTemplateNode.slotOverrideNodes.size > 0;

            ParameterExpression hydrateScope = ctx.GetVariable<TemplateScope>("hydrateScope");

            Expression templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr, Expression.Constant(false));

            ctx.Assign(hydrateScope, templateScopeCtor);

            ctx.innerTemplate = innerTemplate;
            
            BindingOutput result = CompileBindings(ctx, expandedTemplateNode, attributes);

            if (hasForwardOrOverrides) {

                for (int i = 0; i < expandedTemplateNode.slotOverrideNodes.size; i++) {

                    SlotNode node = expandedTemplateNode.slotOverrideNodes.array[i];

                    CompiledSlot compiledSlot = CompileSlotOverride(ctx, node, innerTemplate.GetCompiledSlot(node.slotName));
                    
                    // its per slot... override is probably the correct place to create the array 
                    // ultimately we need the slot that runs to instantiate w/ references

                    if (node.slotType == SlotType.Children) {

                        if (innerTemplate.HasChildrenSlot()) {
                            ctx.AddStatement(Expression.Call(
                                hydrateScope,
                                s_TemplateScope_AddSlotForward,
                                ctx.templateScope,
                                Expression.Constant(compiledSlot.slotName), // todo -- alias as needed
                                ctx.rootParam,
                                Expression.Constant(compiledSlot.slotId))
                            );
                        }
                        else {
                            ctx.AddStatement(Expression.Call(
                                hydrateScope,
                                s_TemplateScope_AddSlotOverride,
                                Expression.Constant(compiledSlot.slotName), // todo -- alias as needed
                                ctx.rootParam,
                                Expression.Constant(compiledSlot.slotId))
                            );
                        }

                    }

                    else if (node.slotType == SlotType.Override) {

                        ctx.AddStatement(Expression.Call(
                            hydrateScope,
                            s_TemplateScope_AddSlotOverride,
                            Expression.Constant(compiledSlot.slotName), // todo -- alias as needed
                            ctx.rootParam,
                            Expression.Constant(compiledSlot.slotId))
                        );

                    }

                    else if (node.slotType == SlotType.Forward) {
                        ctx.AddStatement(Expression.Call(
                            hydrateScope,
                            s_TemplateScope_AddSlotForward,
                            ctx.templateScope,
                            Expression.Constant(compiledSlot.slotName), // todo -- alias as needed
                            ctx.rootParam,
                            Expression.Constant(compiledSlot.slotId))
                        );
                    }

                }

            }

            ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_Application_HydrateTemplate, Expression.Constant(innerTemplate.templateId), nodeExpr, hydrateScope));


            UndoContextMods(result.contextModifications);
            
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

        private static void InitializeAttributes(CompilationContext ctx, StructList<AttributeDefinition> attributes) {
            if (attributes == null) return;

            int attrIdx = 0;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                if (attr.type == AttributeType.Attribute) {
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

        private struct ChangeHandlerDefinition {

            public bool wasHandled;
            public AttributeDefinition attributeDefinition;
            public ContextVariableDefinition variableDefinition;

        }

        private static void GatherChangeHandlers(StructList<AttributeDefinition> attributes, ref StructList<ChangeHandlerDefinition> handlers) {
            if (attributes == null) {
                return;
            }

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];
                if (attr.type == AttributeType.ChangeHandler) {
                    handlers = handlers ?? StructList<ChangeHandlerDefinition>.Get();
                    handlers.Add(new ChangeHandlerDefinition() {
                        attributeDefinition = attr
                    });
                }
            }
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

        private struct BindingOutput {

            public bool hasBindingNode;
            public StructList<ContextAliasActions> contextModifications;

        }

        private BindingOutput CompileBindings(CompilationContext ctx, TemplateNode templateNode, StructList<AttributeDefinition> attributes, LightList<ExposedVariableData> exposedVariableData = null) {
            StructList<ContextAliasActions> contextModifications = null;

            StructList<ChangeHandlerDefinition> changeHandlerDefinitions = null;

            bool isRootTemplate = ctx.templateRootNode == templateNode;

            BindingOutput retn = default;

            // for template roots (which are not the app root!) we dont want to generate bindings in their own template definition functions
            // instead we let the usage site do that for us. We still need to provide context variables to our template, probably in a dry-run fashion.

            try {
                GatherChangeHandlers(attributes, ref changeHandlerDefinitions);

                InitializeCompilers(ctx.namespaces, ctx.templateRootNode.ElementType, templateNode.processedType.rawType);

                InitializeAttributes(ctx, attributes);

                CompileExposedData(exposedVariableData, ref contextModifications);

                CompileConditionalBindings(templateNode, attributes);

                CompileBeforePropertyUpdates(templateNode.processedType);

                CompileAliases(attributes, ref contextModifications);

                CompilePropertyBindingsAndContextVariables(ctx, templateNode.processedType, attributes, changeHandlerDefinitions, ref contextModifications);

                CompileTextBinding(templateNode);

                CompileRemainingChangeHandlerStores(templateNode.processedType.rawType, changeHandlerDefinitions);

                CompileAfterPropertyUpdates(templateNode.processedType);

                CompileAttributeBindings(attributes);

                CompileInstanceStyleBindings(isRootTemplate, attributes);

                CompileStyleBindings(ctx, templateNode.tagName, attributes);

                // CompileAfterStyleBindings();

                CompileInputHandlers(templateNode.processedType, attributes);

                CompileCheckChangeHandlers(changeHandlerDefinitions);

                retn.hasBindingNode = BuildBindings(ctx, templateNode);

                changeHandlerDefinitions?.Release();
            }
            catch (CompileException exception) {
                exception.SetFileName($"{ctx.compiledTemplate.filePath} {templateNode.TemplateNodeDebugData.tagName}{templateNode.TemplateNodeDebugData.lineInfo}");
                throw;
            }

            retn.contextModifications = contextModifications;
            return retn;
        }

        private void CompileCheckChangeHandlers(StructList<ChangeHandlerDefinition> changeHandlers) {
            if (changeHandlers == null) return;

            for (int i = 0; i < changeHandlers.size; i++) {
                CompileChangeHandlerCheck(changeHandlers.array[i]);
            }
        }

        private void CompileRemainingChangeHandlerStores(Type type, StructList<ChangeHandlerDefinition> changeHandlers) {
            if (changeHandlers == null) return;
            for (int i = 0; i < changeHandlers.size; i++) {
                ref ChangeHandlerDefinition handler = ref changeHandlers.array[i];

                if (handler.wasHandled) {
                    continue;
                }

                MemberExpression member = Expression.PropertyOrField(updateCompiler.GetCastElement(), handler.attributeDefinition.key);

                CompileChangeHandlerStore(type, member, ref handler);
            }
        }

        private void CompileChangeHandlerCheck(ChangeHandlerDefinition changeHandler) {
            ContextVariableDefinition variableDefinition = changeHandler.variableDefinition;
            ref AttributeDefinition attr = ref changeHandler.attributeDefinition;

            SetImplicitContext(lateCompiler, changeHandler.attributeDefinition);

            // late update reads from context variable and compares
            Expression access = Expression.MakeMemberAccess(lateCompiler.GetCastElement(), s_UIElement_BindingNode);
            Expression call = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_GetContextVariable, Expression.Constant(variableDefinition.id));
            lateCompiler.Comment(attr.key);
            Expression cast = Expression.Convert(call, variableDefinition.contextVarType);
            Expression target = lateCompiler.AddVariable(variableDefinition.contextVarType, "changeHandler_" + attr.key);
            lateCompiler.Assign(target, cast);
            Expression oldValue = Expression.Field(target, variableDefinition.contextVarType.GetField(nameof(ContextVariable<object>.value)));
            Expression newValue = Expression.PropertyOrField(lateCompiler.GetCastElement(), attr.key);
            string attrValue = attr.value;
            lateCompiler.IfNotEqual(oldValue, newValue, () => {
                // __castRoot.HandleChange();
                ASTNode astNode = ExpressionParser.Parse(attrValue);

                changeHandlerCurrentValue = newValue;
                changeHandlerPreviousValue = oldValue;

                if (astNode.type == ASTNodeType.LambdaExpression) {
                    throw new NotImplementedException("We do not support lambda syntax for onChange handlers yet");
                }

                // assume its a method, probably doesn't have to be once we support assignment
                lateCompiler.Statement(attrValue);

                changeHandlerCurrentValue = null;
                changeHandlerPreviousValue = null;
            });
        }

        private void CompileExposedData(LightList<ExposedVariableData> exposedDataList, ref StructList<ContextAliasActions> contextModifications) {

            if (exposedDataList == null) {
                return;
            }

            for (int x = 0; x < exposedDataList.size; x++) {
                ExposedVariableData exposedData = exposedDataList.array[x];

                if (exposedData.scopedVariables.Length == 0 && exposedData.exposedAttrs.Length == 0) {
                    continue;
                }

                // exposer needs to write & update

                ParameterExpression element = createdCompiler.GetElement();
                MemberExpression bindingNode = Expression.Field(element, s_UIElement_BindingNode);
                MemberExpression innerContext = Expression.Field(bindingNode, typeof(LinqBindingNode).GetField(nameof(LinqBindingNode.referencedContexts)));

                BinaryExpression idx = Expression.ArrayIndex(innerContext, Expression.Constant(x));

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

                if (exposedData.exposedAttrs.Length != 0) {

                    ParameterExpression innerSlotContext_update = updateCompiler.AddVariable(exposedData.rootType, "__innerContext");

                    updateCompiler.Assign(innerSlotContext_update, Expression.Convert(idx, exposedData.rootType));
                    updateCompiler.SetImplicitContext(innerSlotContext_update);

                    for (int i = 0; i < exposedData.exposedAttrs.Length; i++) {

                        ref AttributeDefinition attr = ref exposedData.exposedAttrs[i];
                        // bindingNode.CreateContextVariable<string>(id);
                        ContextVariableDefinition variableDefinition = new ContextVariableDefinition();

                        Type expressionType = updateCompiler.GetExpressionType(attr.value);

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
                }

                for (int i = 0; i < exposedData.scopedVariables.Length; i++) {
                    contextStack.Peek().RemoveWhere(exposedData.scopedVariables[i], (closure, item) => item.id == closure.id);
                }
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
            if (processedType.requiresAfterPropertyUpdates) {
                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateCompiler.GetCastElement(), s_UIElement_OnAfterPropertyBindings));
            }

            if (processedType.requiresUpdateFn) {
                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateCompiler.GetCastElement(), s_UIElement_OnUpdate));
            }
        }

        private void CompilePropertyBindingsAndContextVariables(CompilationContext ctx, ProcessedType processedType, StructList<AttributeDefinition> attributes, StructList<ChangeHandlerDefinition> changeHandlers,
            ref StructList<ContextAliasActions> contextModifications) {
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
                            CompilePropertyBinding(updateCompiler, processedType, attr, changeHandlers);
                            CompilePropertyBindingSync(lateCompiler, attr);
                        }
                        else if ((attr.flags & AttributeFlags.Const) != 0) {
                            CompilePropertyBinding(createdCompiler, processedType, attr, changeHandlers);
                        }
                        else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                            CompilePropertyBinding(enabledCompiler, processedType, attr, changeHandlers);
                        }
                        else {
                            CompilePropertyBinding(updateCompiler, processedType, attr, changeHandlers);
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

        private void CompileInstanceStyleBindings(bool isRoot, StructList<AttributeDefinition> attributes) {
            if (attributes == null) return;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];
                if (attr.type == AttributeType.InstanceStyle) {
                    CompileInstanceStyleBinding(updateCompiler, attr);
                }
            }
        }

        private void CompileStyleBindings(CompilationContext ctx, string tagName, StructList<AttributeDefinition> attributes) {
            StyleSheetReference[] styleRefs = ctx.compiledTemplate.templateMetaData.styleReferences;

            LightList<StyleRefInfo> styleIds = LightList<StyleRefInfo>.Get();

            tagName = tagName ?? "this"; // todo -- not sure if this is correct, kinda want to kill <this> styles anyway

            if (styleRefs != null) {
                for (int i = 0; i < styleRefs.Length; i++) {
                    if (styleRefs[i].styleSheet.TryResolveStyleByTagName(tagName, out int id)) {
                        id = ctx.compiledTemplate.templateMetaData.ResolveStyleByIdSlow(id);
                        styleIds.Add(new StyleRefInfo() {styleId = id, styleName = "implicit:<" + tagName + ">"});
                    }
                }
            }

            styleRefs = ctx.innerTemplate?.templateMetaData.styleReferences;

            if (styleRefs != null) {
                for (int i = 0; i < styleRefs.Length; i++) {
                    if (styleRefs[i].styleSheet.TryResolveStyleByTagName(tagName, out int id)) {
                        id = ctx.innerTemplate.templateMetaData.ResolveStyleByIdSlow(id);
                        styleIds.Add(new StyleRefInfo() {styleId = id, styleName = "implicit:<" + tagName + ">", fromInnerContext = true});
                    }
                }
            }

            StructList<TextExpression> list = StructList<TextExpression>.Get();

            int innerContextSplit = -1;

            if (attributes != null) {
                for (int i = 0; i < attributes.size; i++) {
                    ref AttributeDefinition attr = ref attributes.array[i];

                    if (attr.type != AttributeType.Style) {
                        continue;
                    }

                    TextTemplateProcessor.ProcessTextExpressions(attr.value, list);

                    // should only ever be max 2 style nodes, 1 for inner context, 1 for outer

                    if ((attr.flags & AttributeFlags.InnerContext) != 0) {
                        Assert.IsTrue(innerContextSplit == -1);
                        innerContextSplit = i;
                    }
                }
            }

            if (TextTemplateProcessor.TextExpressionIsConstant(list)) {
                CompileStaticSharedStyles(ctx, list, innerContextSplit, styleIds);
            }
            else {
                CompileDynamicSharedStyles(ctx, list, innerContextSplit, styleIds);
            }

            list.Release();
        }

        private void CompileStaticSharedStyles(CompilationContext ctx, StructList<TextExpression> list, int innerContextSplit, LightList<StyleRefInfo> styleIds) {
            for (int i = 0; i < list.size; i++) {
                bool fromInnerContext = i <= innerContextSplit;

                string text = list.array[i].text;
                string[] splitStyles = text.Split(s_StyleSeparator);

                for (int s = 0; s < splitStyles.Length; s++) {
                    string styleName = splitStyles[s];

                    int styleId;
                    if (fromInnerContext) {
                        Assert.IsNotNull(ctx.innerTemplate);
                        styleId = ctx.innerTemplate.templateMetaData.ResolveStyleNameSlow(styleName);
                    }
                    else {
                        styleId = ctx.compiledTemplate.templateMetaData.ResolveStyleNameSlow(styleName);
                    }

                    if (styleId >= 0) {
                        styleIds.Add(new StyleRefInfo() {styleId = styleId, styleName = list.array[i].text, fromInnerContext = fromInnerContext});
                    }
                }
            }

            if (styleIds.Count > 0) {

                Expression styleSet = Expression.Field(ctx.ElementExpr, s_UIElement_StyleSet);

                for (int i = 0; i < styleIds.size; i++) {
                    ref StyleRefInfo styleRefInfo = ref styleIds.array[i];

                    MethodInfo method = typeof(Application).GetMethod(nameof(Application.GetTemplateMetaData));
                    int metaDataId = styleRefInfo.fromInnerContext ? ctx.innerTemplate.templateId : ctx.compiledTemplate.templateId;

                    MethodCallExpression call = ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, method, Expression.Constant(metaDataId));
                    MethodCallExpression expr = ExpressionFactory.CallInstanceUnchecked(call, s_TemplateMetaData_GetStyleById, Expression.Constant(styleRefInfo.styleId));

                    ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(styleSet, s_StyleSet_AddBaseStyle, expr));
                    ctx.InlineComment(styleRefInfo.styleName + " (from template " + (styleRefInfo.fromInnerContext ? ctx.innerTemplate.filePath : ctx.compiledTemplate.filePath) + ")");
                }

                MemberExpression style = Expression.Field(ctx.ElementExpr, s_UIElement_StyleSet);
                MethodCallExpression initStyle = ExpressionFactory.CallInstanceUnchecked(style, s_StyleSet_InternalInitialize);
                ctx.AddStatement(initStyle);
            }

            styleIds.Release();
        }

        private void CompileDynamicSharedStyles(CompilationContext ctx, StructList<TextExpression> list, int innerContextSplit, LightList<StyleRefInfo> styleIds) {
            Expression metaData = Expression.Field(updateCompiler.GetElement(), s_UIElement_TemplateMetaData);
            Expression innerMetaData = null;

            if (ctx.innerTemplate != null) {
                innerMetaData = ExpressionFactory.CallInstanceUnchecked(Expression.Property(updateCompiler.GetElement(), s_UIElement_Application), s_Application_GetTemplateMetaData,
                    Expression.Constant(ctx.innerTemplate.templateMetaData.id));
            }

            ParameterExpression styleList = ctx.GetVariable<LightList<UIStyleGroupContainer>>("styleList");
            ctx.Assign(styleList, ExpressionFactory.CallStaticUnchecked(s_LightList_UIStyleGroupContainer_Get));

            Parameter updateStyleParam = new Parameter<LightList<UIStyleGroupContainer>>("styleList");
            ParameterExpression updateStyleList = updateCompiler.AddVariable(updateStyleParam, ExpressionFactory.CallStaticUnchecked(s_LightList_UIStyleGroupContainer_Get));

            // this makes sure we always use implicit styles
            for (int i = 0; i < styleIds.size; i++) {
                ref StyleRefInfo styleRefInfo = ref styleIds.array[i];

                ctx.Comment(styleRefInfo.styleName);

                Expression target = styleRefInfo.fromInnerContext ? innerMetaData : metaData;

                MethodCallExpression expr = ExpressionFactory.CallInstanceUnchecked(target, s_TemplateMetaData_GetStyleById, Expression.Constant(styleRefInfo.styleId));
                MethodCallExpression addCall = ExpressionFactory.CallInstanceUnchecked(styleList, s_LightList_UIStyleGroupContainer_Add, expr);

                ctx.AddStatement(addCall);
            }

            updateCompiler.SetNullCheckingEnabled(false);

            for (int i = 0; i < list.size; i++) {
                bool fromInnerContext = i <= innerContextSplit;

                updateCompiler.SetImplicitContext(fromInnerContext ? updateCompiler.GetCastElement() : updateCompiler.GetCastRoot());

                if (list.array[i].isExpression) {
                    Expression templateContext = fromInnerContext ? innerMetaData : metaData;

                    Expression dynamicStyleList = updateCompiler.TypeWrapStatement(s_DynamicStyleListTypeWrapper, typeof(DynamicStyleList), list.array[i].text);

                    updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(dynamicStyleList, s_DynamicStyleList_Flatten, templateContext, updateStyleList));
                }
                else {
                    string text = list.array[i].text;
                    string[] splitStyles = text.Split(s_StyleSeparator);

                    for (int s = 0; s < splitStyles.Length; s++) {
                        string styleName = splitStyles[s];

                        int styleId = -1;
                        if (fromInnerContext) {
                            Assert.IsNotNull(ctx.innerTemplate);
                            styleId = ctx.innerTemplate.templateMetaData.ResolveStyleNameSlow(styleName);
                        }
                        else {
                            styleId = ctx.compiledTemplate.templateMetaData.ResolveStyleNameSlow(styleName);
                        }

                        if (styleId >= 0) {
                            updateCompiler.Comment(styleName);
                            Expression target = fromInnerContext ? innerMetaData : metaData;
                            MethodCallExpression expr = ExpressionFactory.CallInstanceUnchecked(target, s_TemplateMetaData_GetStyleById, Expression.Constant(styleId));
                            MethodCallExpression addCall = ExpressionFactory.CallInstanceUnchecked(updateStyleList, s_LightList_UIStyleGroupContainer_Add, expr);
                            updateCompiler.RawExpression(addCall);
                        }
                    }
                }
            }

            MemberExpression styleSet = Expression.Field(updateCompiler.GetElement(), s_UIElement_StyleSet);
            MethodCallExpression setBaseStyles = ExpressionFactory.CallInstanceUnchecked(styleSet, s_StyleSet_SetBaseStyles, updateStyleList);

            updateCompiler.RawExpression(setBaseStyles);
            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateStyleList, s_LightList_UIStyleGroupContainer_Release));

            MemberExpression style = Expression.Field(ctx.ElementExpr, s_UIElement_StyleSet);
            MethodCallExpression initStyle = ExpressionFactory.CallInstanceUnchecked(style, s_StyleSet_InternalInitialize, styleList);
            ctx.AddStatement(initStyle);
            ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(styleList, s_LightList_UIStyleGroupContainer_Release));
            styleIds.Release();
            updateCompiler.SetNullCheckingEnabled(true);
        }

        private void CompileMouseHandlerFromAttribute(in InputHandler handler) {
            LightList<Parameter> parameters = LightList<Parameter>.Get();

            parameters.Add(new Parameter<MouseInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
            LinqCompiler closure = createdCompiler.CreateClosure(parameters, typeof(void));

            currentEvent = parameters[0];

            if (handler.useEventParameter) {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo, currentEvent));
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
            parameters.Release();
        }

        private void CompileKeyboardHandlerFromAttribute(in InputHandler handler) {
            LightList<Parameter> parameters = LightList<Parameter>.Get();

            parameters.Add(new Parameter<KeyboardInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
            LinqCompiler closure = createdCompiler.CreateClosure(parameters, typeof(void));

            if (handler.useEventParameter) {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo, parameters[0]));
            }
            else {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo));
            }

            LambdaExpression lambda = closure.BuildLambda();

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddKeyboardEvent,
                Expression.Constant(handler.descriptor.handlerType),
                Expression.Constant(handler.descriptor.modifiers),
                Expression.Constant(handler.descriptor.requiresFocus),
                Expression.Constant(handler.descriptor.eventPhase),
                Expression.Constant(handler.keyCode),
                Expression.Constant(handler.character),
                lambda
            );

            createdCompiler.RawExpression(expression);
            closure.Release();
            parameters.Release();
        }

        private void CompileDragHandlerFromAttribute(in InputHandler handler) {
            LightList<Parameter> parameters = LightList<Parameter>.Get();

            parameters.Add(new Parameter<DragEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
            LinqCompiler closure = createdCompiler.CreateClosure(parameters, typeof(void));

            if (handler.useEventParameter) {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo, parameters[0].expression));
            }
            else {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo));
            }

            LambdaExpression lambda = closure.BuildLambda();

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddDragEvent,
                Expression.Constant(handler.descriptor.handlerType),
                Expression.Constant(handler.descriptor.modifiers),
                Expression.Constant(handler.descriptor.requiresFocus),
                Expression.Constant(handler.descriptor.eventPhase),
                lambda
            );

            createdCompiler.RawExpression(expression);
            closure.Release();
            parameters.Release();
        }

        private void CompileDragCreateFromAttribute(in InputHandler handler) {
            LightList<Parameter> parameters = LightList<Parameter>.Get();

            parameters.Add(new Parameter<MouseInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));

            LinqCompiler closure = createdCompiler.CreateClosure(parameters, typeof(DragEvent));

            if (handler.useEventParameter) {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo, parameters[0].expression));
            }
            else {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo));
            }

            LambdaExpression lambda = closure.BuildLambda();

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddDragCreator,
                Expression.Constant(handler.descriptor.modifiers),
                Expression.Constant(handler.descriptor.requiresFocus),
                Expression.Constant(handler.descriptor.eventPhase),
                lambda
            );

            createdCompiler.RawExpression(expression);
            closure.Release();
            parameters.Release();
        }

        private void CompileInputHandlers(ProcessedType processedType, StructList<AttributeDefinition> attributes) {
            StructList<InputHandler> handlers = InputCompiler.CompileInputAnnotations(processedType.rawType);

            const InputEventType k_KeyboardType = InputEventType.KeyDown | InputEventType.KeyUp | InputEventType.KeyHeldDown;
            const InputEventType k_DragType = InputEventType.DragCancel | InputEventType.DragDrop | InputEventType.DragEnter | InputEventType.DragEnter | InputEventType.DragExit | InputEventType.DragHover | InputEventType.DragMove;

            bool hasHandlers = false;

            if (handlers != null) {
                hasHandlers = true;

                for (int i = 0; i < handlers.size; i++) {
                    ref InputHandler handler = ref handlers.array[i];

                    if (handler.descriptor.handlerType == InputEventType.DragCreate) {
                        CompileDragCreateFromAttribute(handler);
                    }
                    else if ((handler.descriptor.handlerType & k_DragType) != 0) {
                        CompileDragHandlerFromAttribute(handler);
                    }
                    else if ((handler.descriptor.handlerType & k_KeyboardType) != 0) {
                        CompileKeyboardHandlerFromAttribute(handler);
                    }
                    else {
                        CompileMouseHandlerFromAttribute(handler);
                    }
                }
            }

            const AttributeType k_InputType = AttributeType.Controller | AttributeType.Mouse | AttributeType.Key | AttributeType.Touch | AttributeType.Drag;

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

                        case AttributeType.Drag:
                            CompileDragBinding(createdCompiler, attr);
                            break;
                    }
                }
            }

            if (!hasHandlers) {
                return;
            }

            // Application.InputSystem.RegisterKeyboardHandler(element);
            ParameterExpression elementVar = createdCompiler.GetElement();
            MemberExpression app = Expression.Property(elementVar, typeof(UIElement).GetProperty(nameof(UIElement.application)));
            MemberExpression inputSystem = Expression.Property(app, typeof(Application).GetProperty(nameof(Application.InputSystem)));
            MethodInfo method = typeof(InputSystem).GetMethod(nameof(InputSystem.RegisterKeyboardHandler));
            createdCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(inputSystem, method, elementVar));
        }

        private bool BuildBindings(CompilationContext ctx, TemplateNode templateNode) {
            int createdBindingId = -1;
            int enabledBindingId = -1;
            int updateBindingId = -1;
            int lateBindingId = -1;

            // we always have 4 statements because of Initialize(), so only consider compilers with more than 4 statements

            if (createdCompiler.StatementCount > 0) {
                CompiledBinding createdBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnCreate);
                createdBinding.bindingFn = createdCompiler.BuildLambda();
                createdBindingId = createdBinding.bindingId;
                ctx.compiledTemplate.AddBinding(createdBinding);
            }

            if (enabledCompiler.StatementCount > 0) {
                CompiledBinding enabledBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnEnable);
                enabledBinding.bindingFn = enabledCompiler.BuildLambda();
                enabledBindingId = enabledBinding.bindingId;
                ctx.compiledTemplate.AddBinding(enabledBinding);
            }

            if (updateCompiler.StatementCount > 0) {
                CompiledBinding updateBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnUpdate);
                updateBinding.bindingFn = updateCompiler.BuildLambda();
                updateBindingId = updateBinding.bindingId;
                ctx.compiledTemplate.AddBinding(updateBinding);
            }

            if (lateCompiler.StatementCount > 0) {
                CompiledBinding lateBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnLateUpdate);
                lateBinding.bindingFn = lateCompiler.BuildLambda();
                lateBindingId = lateBinding.bindingId;
                ctx.compiledTemplate.AddBinding(lateBinding);
            }

            // create binding node if needed
            // todo -- some nodes like repeat children always need a binding node created. 
            // most nodes without bindings do not
            //if (createdBindingId >= 0 || updateBindingId >= 0 || enabledBindingId >= 0 || lateBindingId >= 0) {
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
            return true;
            //   }

            //   return false;
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
            definition.contextVarType = type;
            return ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_CreateLocalContextVariable, contextVariable);
        }

        private void CompileTextBinding(TemplateNode templateNode) {
            if (!(templateNode is TextNode textNode)) {
                return;
            }

            if (textNode.textExpressionList != null && textNode.textExpressionList.size > 0 && !textNode.IsTextConstant()) {
                updateCompiler.AddNamespace("UIForia.Util");
                updateCompiler.AddNamespace("UIForia.Text");
                StructList<TextExpression> expressionParts = textNode.textExpressionList;

                MemberExpression textValueExpr = Expression.Field(updateCompiler.GetCastElement(), s_TextElement_Text);

                for (int i = 0; i < expressionParts.size; i++) {
                    if (expressionParts[i].isExpression) {
                        updateCompiler.SetImplicitContext(updateCompiler.GetCastRoot());
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

            LambdaExpression lambda = BuildInputTemplateBinding<KeyboardInputEvent>(createdCompiler, attr);

            InputHandlerDescriptor descriptor = InputCompiler.ParseKeyboardDescriptor(attr.key);

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(compiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddKeyboardEvent,
                Expression.Constant(descriptor.handlerType),
                Expression.Constant(descriptor.modifiers),
                Expression.Constant(descriptor.requiresFocus),
                Expression.Constant(descriptor.eventPhase),
                Expression.Constant(KeyCodeUtil.AnyKey),
                Expression.Constant('\0'),
                lambda
            );

            compiler.RawExpression(expression);

        }

        private void CompileDragBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            InputHandlerDescriptor descriptor = InputCompiler.ParseDragDescriptor(attr.key);
            if (descriptor.handlerType == InputEventType.DragCreate) {
                CompileDragCreateBinding(attr, descriptor);
            }
            else {
                CompileDragEventBinding(attr, descriptor);
            }
        }

        private void CompileDragEventBinding(in AttributeDefinition attr, in InputHandlerDescriptor descriptor) {

            LambdaExpression lambda = BuildInputTemplateBinding<DragEvent>(createdCompiler, attr);

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddDragEvent,
                Expression.Constant(descriptor.handlerType),
                Expression.Constant(descriptor.modifiers),
                Expression.Constant(descriptor.requiresFocus),
                Expression.Constant(descriptor.eventPhase),
                lambda
            );

            createdCompiler.RawExpression(expression);
        }

        private LambdaExpression BuildInputTemplateBinding<T>(UIForiaLinqCompiler compiler, in AttributeDefinition attr, Type returnType = null) {
            SetImplicitContext(compiler, attr);

            ASTNode astNode = ExpressionParser.Parse(attr.value);
            string eventName = k_InputEventParameterName;

            if (astNode.type == ASTNodeType.LambdaExpression) {
                LambdaExpressionNode n = (LambdaExpressionNode) astNode;
                if (n.signature.size == 1) {
                    LambdaArgument signature = n.signature.array[0];
                    eventName = signature.identifier;
                    if (signature.type != null) {
                        Debug.LogWarning("Input handler lambda should not define a type");
                    }
                }
                else if (n.signature.size > 1) {
                    throw CompileException.InvalidInputHandlerLambda(attr, n.signature.size);
                }

                astNode = n.body;
            }

            LinqCompiler closure = compiler.CreateClosure(new Parameter<T>(eventName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds), returnType ?? typeof(void));

            currentEvent = closure.GetParameterAtIndex(0);

            try {
                if (returnType == null) {
                    closure.Statement(astNode);
                }
                else {
                    closure.Return(astNode);
                }
            }
            catch (CompileException exception) {
                exception.SetExpression(attr.rawValue + " at " + attr.line + ": " + attr.column);
                throw;
            }

            currentEvent = null;

            LambdaExpression lambda = closure.BuildLambda();
            closure.Release();

            return lambda;
        }

        private void CompileMouseInputBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {

            // todo -- eliminate generated closure by passing in template root and element from input system and doing casting as normal in the callback

            LambdaExpression lambda = BuildInputTemplateBinding<MouseInputEvent>(compiler, attr);

            InputHandlerDescriptor descriptor = InputCompiler.ParseMouseDescriptor(attr.key);

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(compiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddMouseEvent,
                Expression.Constant(descriptor.handlerType),
                Expression.Constant(descriptor.modifiers),
                Expression.Constant(descriptor.requiresFocus),
                Expression.Constant(descriptor.eventPhase),
                lambda
            );

            compiler.RawExpression(expression);

        }

        private void CompileDragCreateBinding(in AttributeDefinition attr, in InputHandlerDescriptor descriptor) {

            LambdaExpression lambda = BuildInputTemplateBinding<MouseInputEvent>(createdCompiler, attr, typeof(DragEvent));

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddDragCreator,
                Expression.Constant(descriptor.modifiers),
                Expression.Constant(descriptor.requiresFocus),
                Expression.Constant(descriptor.eventPhase),
                lambda
            );

            createdCompiler.RawExpression(expression);
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

        private void CompileInstanceStyleBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attributeDefinition) {
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

            ParameterExpression castElement = compiler.GetCastElement();

            MemberExpression field = Expression.Field(castElement, s_UIElement_StyleSet);

            compiler.BeginIsolatedSection();

            SetImplicitContext(compiler, attributeDefinition);

            compiler.CommentNewLineBefore($"style.{attributeDefinition.key}=\"{attributeDefinition.value}\"");

            // todo -- check if constant and pick different compiler
            // Debug.Log(attributeDefinition.key + ExpressionUtil.IsConstant(value));

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

            Expression value = compiler.Value(attributeDefinition.value);

            // hack! for some reason because the type can be by ref (via in) it doesn't report as a generic type
            if (parameters[0].ParameterType.FullName.Contains("System.Nullable")) {
                if (!value.Type.IsNullableType()) {
                    Type targetType = parameters[0].ParameterType.GetGenericArguments()[0];

                    if (targetType.IsByRef) {
                        targetType = targetType.GetElementType();
                    }

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
            ParameterExpression castElement = compiler.GetCastElement();
            ParameterExpression castRoot = compiler.GetCastRoot();
            compiler.CommentNewLineBefore($"{attributeDefinition.key}=\"{attributeDefinition.value}\"");
            compiler.BeginIsolatedSection();
            try {
                compiler.SetImplicitContext(castElement);
                compiler.AssignableStatement(attributeDefinition.key);
            }
            catch (Exception) {
                compiler.EndIsolatedSection();
                throw;
            }

            compiler.SetImplicitContext(castRoot);
            LHSStatementChain assignableStatement = compiler.AssignableStatement(attributeDefinition.value);

            compiler.SetImplicitContext(castElement);
            compiler.Assign(assignableStatement, Expression.Field(castElement, castElement.Type.GetField(attributeDefinition.key)));
            compiler.EndIsolatedSection();
        }

        private void CompilePropertyBinding(UIForiaLinqCompiler compiler, ProcessedType processedType, in AttributeDefinition attributeDefinition, StructList<ChangeHandlerDefinition> changeHandlerAttrs) {
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

            CompileChangeHandlerPropertyBindingStore(processedType.rawType, attributeDefinition, changeHandlerAttrs, right);

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

                        throw CompileException.UnresolvedPropertyChangeHandler(methodInfo.Name, right.Type); // todo -- better error message
                    }
                });
            }
            else {
                compiler.Assign(left, right);
            }

            compiler.EndIsolatedSection();
            changeHandlers.Release();
        }

        private void CompileChangeHandlerPropertyBindingStore(Type type, in AttributeDefinition attr, StructList<ChangeHandlerDefinition> changeHandlers, Expression value) {
            if (changeHandlers == null) return;

            for (int i = 0; i < changeHandlers.size; i++) {
                ref ChangeHandlerDefinition handler = ref changeHandlers.array[i];

                if (handler.attributeDefinition.key != attr.key) {
                    continue;
                }

                if (handler.wasHandled) {
                    return;
                }

                CompileChangeHandlerStore(type, value, ref handler);

                return;
            }
        }

        // todo accept compiler? or always use update?
        private void CompileChangeHandlerStore(Type type, Expression value, ref ChangeHandlerDefinition changeHandler) {
            ref AttributeDefinition attr = ref changeHandler.attributeDefinition;

            SetImplicitContext(createdCompiler, attr);
            SetImplicitContext(updateCompiler, attr);

            Type fieldOrPropertyType = ReflectionUtil.ResolveFieldOrPropertyType(type, attr.key);

            if (fieldOrPropertyType == null) {
                throw CompileException.UnresolvedFieldOrProperty(type, attr.key);
            }

            // create a local context variable
            ContextVariableDefinition variableDefinition = new ContextVariableDefinition();
            variableDefinition.name = attr.key;
            variableDefinition.id = NextContextId;
            variableDefinition.type = fieldOrPropertyType;
            variableDefinition.variableType = AliasResolverType.ChangeHandlerStorage;

            MethodCallExpression createVariable = CreateLocalContextVariableExpression(variableDefinition, out Type contextVarType);

            changeHandler.wasHandled = true;
            changeHandler.variableDefinition = variableDefinition;

            createdCompiler.RawExpression(createVariable);

            Expression access = Expression.MakeMemberAccess(updateCompiler.GetCastElement(), s_UIElement_BindingNode);
            Expression call = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_GetContextVariable, Expression.Constant(variableDefinition.id));
            updateCompiler.Comment(attr.key);
            Expression cast = Expression.Convert(call, contextVarType);
            ParameterExpression target = updateCompiler.AddVariable(contextVarType, "changeHandler_" + attr.key);
            updateCompiler.Assign(target, cast);
            MemberExpression valueField = Expression.Field(target, contextVarType.GetField(nameof(ContextVariable<object>.value)));
            updateCompiler.Assign(valueField, value);
        }

        private Expression ResolveAlias(string aliasName, LinqCompiler compiler) {
            if (aliasName == "oldValue") {
                if (changeHandlerPreviousValue == null) {
                    throw new CompileException("Invalid use of $oldValue, this alias is only available when used inside of an onChange handler");
                }

                return changeHandlerPreviousValue;
            }

            if (aliasName == "newValue") {
                if (changeHandlerCurrentValue == null) {
                    throw new CompileException("Invalid use of $newValue, this alias is only available when used inside of an onChange handler");
                }

                return changeHandlerCurrentValue;
            }

            if (aliasName == "element") {
                return ((UIForiaLinqCompiler) compiler).GetCastElement();
            }

            if (aliasName == "parent") {
                // todo -- should return the parent but ignore intrinsic elements like RepeatMulitChildContainer
                UIForiaLinqCompiler c = compiler as UIForiaLinqCompiler;
                System.Diagnostics.Debug.Assert(c != null, nameof(c) + " != null");
                return Expression.Field(c.GetElement(), s_UIElement_Parent);
            }

            if (aliasName == "evt") {
                if (currentEvent == null) {
                    throw new CompileException("Invalid use of $evt, this alias is only available when used inside of an event handler");
                }

                return currentEvent;
            }

            if (aliasName == "event") {
                if (currentEvent == null) {
                    throw new CompileException("Invalid use of $event, this alias is only available when used inside of an event handler");
                }

                return currentEvent;
            }

            if (aliasName == "root" || aliasName == "this") {
                return ((UIForiaLinqCompiler) compiler).GetCastRoot();
            }

            ContextVariableDefinition contextVar = FindContextByName(aliasName);

            if (contextVar != null) {
                return contextVar.Resolve(compiler);
            }

            throw CompileException.UnknownAlias(aliasName);
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

        private ProcessedType ResolveGenericElementType(IList<string> namespaces, Type rootType, TemplateNode templateNode) {
            ProcessedType processedType = templateNode.processedType;

            Type generic = processedType.rawType;

            Type[] arguments = processedType.rawType.GetGenericArguments();

            Type[] resolvedTypes = new Type[arguments.Length];

            typeResolver.Reset();

            typeResolver.SetNamespaces(namespaces);
            typeResolver.SetSignature(new Parameter(rootType, "__root", ParameterFlags.NeverNull));
            typeResolver.SetImplicitContext(typeResolver.GetParameter("__root"));
            typeResolver.resolveAlias = ResolveAlias;
            typeResolver.Setup(rootType, null);

            if (templateNode.attributes == null) {
                throw CompileException.UnresolvedGenericElement(processedType, templateNode.TemplateNodeDebugData);
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
                    throw CompileException.UnresolvedGenericElement(processedType, templateNode.TemplateNodeDebugData);
                }
            }

            Type newType = ReflectionUtil.CreateGenericType(processedType.rawType, resolvedTypes);
            ProcessedType retn = TypeProcessor.AddResolvedGenericElementType(newType, processedType.templateAttr, processedType.tagName);
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

                // List<IOption<T>> options
                // resolve T
                // options = (<IList<IOption<string>>)someExpression();

                // find out if we have a generic argument in our field
                // either field type is generic 
                // or field is a constructed generic type
                // if constructed 
                // recurse both sides
                // class StringList : List<String> {} 
                // have generic type defintion and expression
                // solve for generic type defs by extracting 'T' arguments from expression

                // dont care about type checking yet

                // need to 'step into' constructed types until non constructed found

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
                            throw new CompileException(templateNode.TemplateNodeDebugData.tagName + templateNode.TemplateNodeDebugData.lineInfo);
                        }

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

    }

}