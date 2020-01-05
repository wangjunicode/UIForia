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
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Templates;
using UIForia.Text;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateCompiler2 {

        private CompiledTemplateData templateData;
        private Dictionary<Type, CompiledTemplate> templateMap;
        private TemplateCache templateCache;
        private int contextId;

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

        private static readonly MethodInfo s_StyleSet_InternalInitialize = typeof(UIStyleSet).GetMethod(nameof(UIStyleSet.internal_Initialize), BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo s_TemplateMetaData_GetStyleById = typeof(TemplateMetaData).GetMethod(nameof(TemplateMetaData.GetStyleById), BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo s_LightList_UIStyleGroupContainer_PreSize = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.PreSize), BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_LightList_UIStyle_Release = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.Release), BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_LightList_UIStyleGroupContainer_Array = typeof(LightList<UIStyleGroupContainer>).GetField(nameof(LightList<UIStyleGroupContainer>.array), BindingFlags.Public | BindingFlags.Instance);

        private static readonly MethodInfo s_Application_CreateSlot = typeof(Application).GetMethod(nameof(Application.CreateSlot), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo s_Application_CreateSlot2 = typeof(Application).GetMethod(nameof(Application.CreateSlot2), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo s_Application_HydrateTemplate = typeof(Application).GetMethod(nameof(Application.HydrateTemplate), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo s_Application_s_ResolveSlotId = typeof(Application).GetMethod(nameof(Application.ResolveSlotId), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo s_InputHandlerGroup_AddMouseEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddMouseEvent));
        private static readonly MethodInfo s_InputHandlerGroup_AddKeyboardEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddKeyboardEvent));

        private static readonly PropertyInfo s_Element_IsEnabled = typeof(UIElement).GetProperty(nameof(UIElement.isEnabled));
        private static readonly FieldInfo s_Element_BindingNode = typeof(UIElement).GetField(nameof(UIElement.bindingNode));

        private static readonly MethodInfo s_LinqBindingNode_CreateLocalContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.CreateLocalContextVariable));
        private static readonly MethodInfo s_LinqBindingNode_GetLocalContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetLocalContextVariable));
        private static readonly MethodInfo s_LinqBindingNode_GetContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetContextVariable));

        private static readonly MethodInfo s_EventUtil_Subscribe = typeof(EventUtil).GetMethod(nameof(EventUtil.Subscribe));

        private static readonly Expression s_StringBuilderExpr = Expression.Field(null, typeof(TextUtil), "StringBuilder");
        private static readonly Expression s_StringBuilderClear = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(StringBuilder).GetMethod("Clear"));
        private static readonly Expression s_StringBuilderToString = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(StringBuilder).GetMethod("ToString", Type.EmptyTypes));
        private static readonly MethodInfo s_StringBuilder_AppendString = typeof(StringBuilder).GetMethod("Append", new Type[] {typeof(string)});

        private static readonly LinqCompiler s_TypeResolver = new LinqCompiler();

        public TemplateCompiler2(TemplateSettings settings) {
            this.templateCache = new TemplateCache(settings);
            this.templateMap = new Dictionary<Type, CompiledTemplate>();
            this.templateData = new CompiledTemplateData(settings);
        }

        public static CompiledTemplateData CompileTemplates(Type appRootType, TemplateSettings templateSettings) {
            TemplateCompiler2 instance = new TemplateCompiler2(templateSettings);

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

            ElementTemplateNode templateRootNode = templateCache.GetParsedTemplate(processedType);

            CompiledTemplate compiledTemplate = Compile(templateRootNode);

            templateMap[processedType.rawType] = compiledTemplate;

            return compiledTemplate;
        }

        private CompiledTemplate Compile(ElementTemplateNode templateRootNode) {
            CompiledTemplate retn = templateData.CreateTemplate(templateRootNode.templateShell.filePath);
            LightList<string> namespaces = LightList<string>.Get();

            if (templateRootNode.templateShell.usings != null) {
                for (int i = 0; i < templateRootNode.templateShell.usings.size; i++) {
                    namespaces.Add(templateRootNode.templateShell.usings[i].namespaceName);
                }
            }

            ProcessedType processedType = templateRootNode.processedType;

            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            CompilationContext ctx = new CompilationContext();
            ctx.rootType = processedType;
            ctx.rootParam = rootParam;
            ctx.templateScope = scopeParam;
            ctx.elementType = processedType;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.compiledTemplate = retn;
            ctx.Initialize(rootParam);

            for (int i = 0; i < templateRootNode.templateShell.styles.size; i++) {
                ref StyleDefinition styleDef = ref templateRootNode.templateShell.styles.array[i];

                StyleSheet sheet = templateData.ImportStyleSheet(styleDef);

                if (sheet != null) {
                    ctx.AddStyleSheet(styleDef.alias, sheet);
                }
            }

            if (!processedType.IsUnresolvedGeneric) {
                ctx.PushBlock();

                ctx.Comment("new " + TypeNameGenerator.GetTypeName(processedType.rawType));
                Expression createRootExpression = ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                    Expression.Constant(processedType.id),
                    Expression.Default(typeof(UIElement)), // root has no parent
                    Expression.Constant(templateRootNode.ChildCount),
                    Expression.Constant(0), //ast.root.GetAttributeCount()),
                    Expression.Constant(ctx.compiledTemplate.templateId)
                );

                // root = templateScope.application.CreateFromPool<Type>(attrCount, childCount);
                ctx.Assign(ctx.rootParam, createRootExpression);

                ctx.IfEqualsNull(ctx.rootParam, ctx.PopBlock());
            }

            VisitChildren(ctx, templateRootNode);
            ctx.Return(ctx.rootParam);
            LightList<string>.Release(ref namespaces);
            retn.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, scopeParam);
            return retn;
        }

        private void VisitChildren(CompilationContext ctx, TemplateNode2 templateNode) {
            if (templateNode.ChildCount == 0) {
                return;
            }

            ctx.PushScope();

            Expression parentChildList = Expression.Field(ctx.ParentExpr, s_Element_ChildrenList);

            Expression parentChildListArray = Expression.Field(parentChildList, s_LightList_Element_Array);

            // childList[idx] = Visit()
            for (int i = 0; i < templateNode.ChildCount; i++) {
                Expression visit = Visit(ctx, templateNode[i]);
                // will be null for stored templates
                if (visit != null) {
                    // childList.array[i] = targetElement_x;
                    ctx.Assign(Expression.ArrayAccess(parentChildListArray, Expression.Constant(i)), visit);
                }
            }

            ctx.PopScope();
        }

        private Expression Visit(CompilationContext ctx, TemplateNode2 templateNode) {
            if (templateNode.processedType.IsUnresolvedGeneric) {
                templateNode.processedType = ResolveGenericElementType(templateNode);
            }

            ProcessedType processedType = templateNode.processedType;
            ctx.elementType = processedType; // replace w/ stack push of (processedType, variableExpression)?
            // ctx.CommentNewLineBefore(templateNode.originalString ?? "");

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

        private Expression CompileTerminalNode(CompilationContext ctx, TerminalNode terminalNode) {
            throw new NotImplementedException();
        }

        private Expression CompileSlotDefinition(CompilationContext parentContext, SlotNode slotNode) {
            // we want to try to resolve the slot name. if we can't fall back, if fallback id is -1 then don't add a child
            CompiledSlot compiledSlot = templateData.CreateSlot(parentContext.compiledTemplate.filePath, slotNode.slotName, slotNode.slotType);
            slotNode.compiledSlotId = compiledSlot.slotId;
            
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
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            CompilationContext ctx = new CompilationContext();

            ParameterExpression slotRootParam = ctx.GetVariable(typeof(UISlotOverride), "slotRoot");
            ctx.rootType = parentContext.rootType;
            ctx.rootParam = slotRootParam;
            ctx.templateScope = scopeParam;
            ctx.elementType = slotNode.processedType;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.Initialize(slotRootParam);
            ctx.compiledTemplate = parentContext.compiledTemplate; // todo -- might be wrong

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

            compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, scopeParam);

            return nodeExpr;
        }

        private int CompileSlotOverride(CompilationContext parentContext, SlotNode slotOverrideNode) {
            CompiledSlot compiledSlot = templateData.CreateSlot(parentContext.compiledTemplate.filePath, slotOverrideNode.slotName, SlotType.Override);
            slotOverrideNode.compiledSlotId = compiledSlot.slotId;
            
            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            CompilationContext ctx = new CompilationContext();

            ParameterExpression slotRootParam = ctx.GetVariable(typeof(UISlotOverride), "slotRoot");
            ctx.rootType = parentContext.rootType;
            ctx.rootParam = slotRootParam;
            ctx.templateScope = scopeParam;
            ctx.elementType = slotOverrideNode.processedType;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.Initialize(slotRootParam);
            ctx.compiledTemplate = parentContext.compiledTemplate;

            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(TypeProcessor.GetProcessedType(typeof(UISlotOverride)).id),
                Expression.Default(typeof(UIElement)), // todo -- parent is null, fix this
                Expression.Constant(slotOverrideNode.ChildCount),
                Expression.Constant(slotOverrideNode.GetAttributeCount()),
                Expression.Constant(parentContext.compiledTemplate.templateId)
            );

            ctx.Assign(slotRootParam, Expression.Convert(createRootExpression, typeof(UISlotOverride)));

            VisitChildren(ctx, slotOverrideNode);

            ctx.Return(slotRootParam);

            compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, scopeParam);

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

            VisitChildren(ctx, textNode);

//            OutputAttributes(ctx, templateNode);
//            CompileElementData(templateNode, ctx);

            return nodeExpr;
        }

        private Expression CompileContainerNode(CompilationContext ctx, ContainerNode containerNode) {
            ParameterExpression nodeExpr = ctx.ElementExpr;
            ProcessedType processedType = containerNode.processedType;

            ctx.Comment("new " + TypeNameGenerator.GetTypeName(processedType.rawType));

            ctx.Assign(nodeExpr, CreateElement(ctx, containerNode));

            VisitChildren(ctx, containerNode);

//            OutputAttributes(ctx, templateNode);
//            CompileElementData(templateNode, ctx);

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

            if (expandedTemplateNode.slotOverrideNodes != null && expandedTemplateNode.slotOverrideNodes.size > 0) {
                slotUsageExpr = ctx.GetVariable(typeof(StructList<SlotUsage>), "slotUsage");

                LightList<SlotNode> slotOverrides = expandedTemplateNode.slotOverrideNodes;

                // find potentially exposed slots here
                // find defined slots in target template
                // for each slot usage that is not children

                ctx.Assign(slotUsageExpr, Expression.Call(null, s_SlotUsageList_PreSize, Expression.Constant(slotOverrides.size)));

                MemberExpression arrayAccess = Expression.MakeMemberAccess(slotUsageExpr, s_SlotUsageList_Array);

                LightList<SlotNode> acceptedSlots = expandedTemplateNode.elementRoot.slotDefinitionNodes;

                for (int i = 0; i < slotOverrides.size; i++) {
                    int slotId = CompileSlotOverride(ctx, slotOverrides.array[i]);

                    IndexExpression arrayIndex = Expression.ArrayAccess(arrayAccess, Expression.Constant(i));

                    ctx.Comment(slotOverrides[i].slotName);

                    ctx.Assign(arrayIndex, Expression.New(s_SlotUsage_Ctor,
                        Expression.Constant(slotOverrides[i].slotName),
                        Expression.Constant(slotId),
                        ctx.rootParam
                    ));
                }

                if (acceptedSlots != null) {
                    for (int i = 0; i < acceptedSlots.size; i++) {
                        if (acceptedSlots.array[i].slotType == SlotType.Children) {
                            continue;
                        }

                        if (expandedTemplateNode.elementRoot.HasSlotExternOverride(acceptedSlots.array[i].slotName, out SlotNode externNode)) {
                            ctx.AddStatement(Expression.Call(
                                ctx.templateScope,
                                s_TemplateScope_ForwardSlotDataWithFallback,
                                Expression.Constant(acceptedSlots.array[i].slotName),
                                slotUsageExpr,
                                ctx.rootParam, 
                                Expression.Constant(externNode.compiledSlotId))
                            );
                        }
                        else {
                            ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(
                                ctx.templateScope,
                                s_TemplateScope_ForwardSlotData,
                                Expression.Constant(acceptedSlots.array[i].slotName),
                                slotUsageExpr)
                            );
                        }
                    }
                }
            }

            Expression templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr, slotUsageExpr);

            ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_Application_HydrateTemplate, Expression.Constant(expandedTemplate.templateId), nodeExpr, templateScopeCtor));

            if (expandedTemplateNode.slotOverrideNodes != null && expandedTemplateNode.slotOverrideNodes.size > 0) {
                ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(slotUsageExpr, s_SlotUsageList_Release));
            }

            // VisitChildren(ctx, expandedTemplateNode);

            return nodeExpr;
        }

        private static Expression CreateElement(CompilationContext ctx, TemplateNode2 node) {
            return ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(node.processedType.id),
                ctx.ParentExpr,
                Expression.Constant(node.ChildCount),
                Expression.Constant(node.GetAttributeCount()),
                Expression.Constant(ctx.compiledTemplate.templateId)
            );
        }

        private static ProcessedType ResolveGenericElementType(TemplateNode2 templateNode) {
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

    }

}