using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UIForia.Attributes;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Templates;
using UIForia.Text;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public class TemplateCompiler {

        private int aliasIdGenerator;
        private CompiledTemplateData templateData;
        private readonly TemplateSettings settings;
        private readonly LinqCompiler enabledCompiler;
        private readonly LinqCompiler createdCompiler;
        private readonly LinqCompiler updateCompiler;
        private readonly LightStack<Type> compilationStack;
        private readonly XMLTemplateParser xmlTemplateParser;
        private readonly Dictionary<Type, CompiledTemplate> templateMap;
        private readonly LightStack<ContextVarAliasResolver> resolvers;
        private StructStack<ContextVariableDefinition> contextVarStack;

        private const string k_InputEventAliasName = "$evt";
        private const string k_InputEventParameterName = "__evt";
        private const string k_InputHandlerVarName = "__inputHandler";
        private const string k_CastElement = "__castElement";
        private const string k_CastRoot = "__castRoot";
        private static readonly char[] s_StyleSeparator = new char[] {' '};
        private static readonly LinqCompiler s_TypeResolver = new LinqCompiler();

        private static readonly MethodInfo s_CreateFromPool = typeof(Application).GetMethod(nameof(Application.CreateElementFromPoolWithType));
        private static readonly MethodInfo s_BindingNodePool_Get = typeof(LinqBindingNode).GetMethod("Get", BindingFlags.Static | BindingFlags.Public);
        private static readonly FieldInfo s_StructList_ElementAttr_Array = typeof(StructList<ElementAttribute>).GetField("array");

        private static readonly MethodInfo s_SlotUsageList_PreSize = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.PreSize), BindingFlags.Static | BindingFlags.Public);
        private static readonly FieldInfo s_SlotUsageList_Array = typeof(StructList<SlotUsage>).GetField("array", BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_SlotUsageList_Release = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.Release), BindingFlags.Instance | BindingFlags.Public);

        private static readonly ConstructorInfo s_SlotUsage_Ctor = typeof(SlotUsage).GetConstructor(new[] {typeof(string), typeof(int)});

        private static readonly ConstructorInfo s_TemplateScope_Ctor = typeof(TemplateScope).GetConstructor(new[] {typeof(Application), typeof(StructList<SlotUsage>), typeof(UIElement)});
        private static readonly FieldInfo s_TemplateScope_SlotList = typeof(TemplateScope).GetField(nameof(TemplateScope.slotInputs));
        private static readonly FieldInfo s_TemplateScope_ApplicationField = typeof(TemplateScope).GetField(nameof(TemplateScope.application));

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

        private int contextId;

        public TemplateCompiler(TemplateSettings settings) {
            this.settings = settings;
            this.templateMap = new Dictionary<Type, CompiledTemplate>();
            this.compilationStack = new LightStack<Type>();
            this.xmlTemplateParser = new XMLTemplateParser();
            this.updateCompiler = new LinqCompiler();
            this.enabledCompiler = new LinqCompiler();
            this.createdCompiler = new LinqCompiler();
            this.resolvers = new LightStack<ContextVarAliasResolver>();
            this.contextVarStack = new StructStack<ContextVariableDefinition>();

            resolvers.EnsureCapacity(8);
            resolvers.size = 3;
            resolvers.array[0] = default;
            resolvers.array[1] = default;
            resolvers.array[2] = default;
            Func<string, LinqCompiler, Expression> resolveAlias = ResolveAlias;

            this.createdCompiler.resolveAlias = resolveAlias;
            this.enabledCompiler.resolveAlias = resolveAlias;
            this.updateCompiler.resolveAlias = resolveAlias;
        }

        private int NextContextId => contextId++;

        public void CompileTemplates(Type rootType, CompiledTemplateData templateData) {
            this.templateData = templateData;

            if (!typeof(UIElement).IsAssignableFrom(rootType)) { }

            if (typeof(UIContainerElement).IsAssignableFrom(rootType)) { }

            if (typeof(UITextElement).IsAssignableFrom(rootType)) { }

            // CompiledTemplate root = GetCompiledTemplate();

            GetCompiledTemplate(TypeProcessor.GetProcessedType(rootType));

            // start at root template
            // parse / compile all other templates

            // we only compiled templates that are referenced in the app hierarchy
            // todo -- allow user to mark templates as 'always included'
            // templates shared across applications are compiled once per application in case one app changes but other doesn't.

            // templateData.SaveTemplates(templateMap.ToList());
        }

        private CompiledTemplate GetCompiledTemplate(ProcessedType processedType) {
            if (typeof(UIContainerElement).IsAssignableFrom(processedType.rawType)) {
                return null;
            }

            if ((typeof(UITextElement).IsAssignableFrom(processedType.rawType))) {
                return null;
            }

            if ((typeof(UIChildrenElement).IsAssignableFrom(processedType.rawType))) {
                return null;
            }

            if (templateMap.TryGetValue(processedType.rawType, out CompiledTemplate retn)) {
                return retn;
            }

            if (compilationStack.Contains(processedType.rawType)) {
                string recursion = "";
                for (int i = 0; i < compilationStack.Count; i++) {
                    recursion += compilationStack.PeekAtUnchecked(i);
                    if (i != compilationStack.Count) {
                        recursion += " -> ";
                    }
                }

                throw new CompileException("Template Recursion detected: " + recursion);
            }

            TemplateAST ast = null;

            TemplateDefinition templateDefinition = GetTemplateSource(processedType);

            if (templateDefinition.language == TemplateLanguage.XML) {
                ast = xmlTemplateParser.Parse(templateDefinition.contents, templateDefinition.filePath, processedType);
            }
            else {
                throw new NotImplementedException("Only XML templates are currently supported");
            }

            compilationStack.Push(processedType.rawType);

            CompiledTemplate compiledTemplate = Compile(ast);

            compilationStack.Pop();

            templateMap[processedType.rawType] = compiledTemplate;

            return compiledTemplate;
        }

        private TemplateDefinition GetTemplateSource(ProcessedType processedType) {
            TemplateAttribute templateAttr = processedType.templateAttr;

            switch (templateAttr.templateType) {
                case TemplateType.Internal: {
                    string templatePath = settings.GetInternalTemplatePath(templateAttr.template);
                    string file = settings.TryReadFile(templatePath);

                    if (file == null) {
                        throw new TemplateParseException(settings.templateResolutionBasePath, $"Cannot find template in (internal) path {templatePath}.");
                    }

                    return new TemplateDefinition() {
                        contents = file,
                        filePath = templateAttr.templateType == TemplateType.File ? processedType.rawType.AssemblyQualifiedName : templateAttr.template,
                        language = TemplateLanguage.XML
                    };
                }

                case TemplateType.File: {
                    string templatePath = settings.GetTemplatePath(templateAttr.template);
                    string file = settings.TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(settings.templateResolutionBasePath, $"Cannot find template in path {templatePath}.");
                    }

                    return new TemplateDefinition() {
                        contents = file,
                        filePath = templateAttr.template,
                        language = TemplateLanguage.XML
                    };
                }

                default:
                    return new TemplateDefinition() {
                        contents = templateAttr.template,
                        filePath = "NONE", // todo make unique
                        language = TemplateLanguage.XML
                    };
            }
        }

        private CompiledTemplate CompileRootTemplate(TemplateAST ast) {
            ProcessedType processedType = ast.root.processedType;

            TemplateDefinition templateDefinition = GetTemplateSource(processedType);

            if (compilationStack.Contains(processedType.rawType)) {
                string recursion = "";
                for (int i = 0; i < compilationStack.Count; i++) {
                    recursion += compilationStack.PeekAtUnchecked(i);
                    if (i != compilationStack.Count) {
                        recursion += " -> ";
                    }
                }

                throw new CompileException("Template Recursion detected: " + recursion);
            }

            compilationStack.Push(processedType.rawType);

            if (templateDefinition.language == TemplateLanguage.XML) {
                ast = xmlTemplateParser.Parse(templateDefinition.contents, templateDefinition.filePath, processedType);
            }
            else {
                throw new NotImplementedException("Only XML templates are currently supported");
            }

            CompiledTemplate compiledTemplate = Compile(ast);

            TemplateNode.Release(ref ast.root);

            compilationStack.Pop();

            templateMap[processedType.rawType] = compiledTemplate;

            return compiledTemplate;
        }


        private CompiledTemplate Compile(TemplateAST ast) {
            CompiledTemplate retn = templateData.CreateTemplate(ast.fileName);

            LightList<string> namespaces = LightList<string>.Get();
            if (ast.usings != null) {
                for (int i = 0; i < ast.usings.size; i++) {
                    namespaces.Add(ast.usings[i].namespaceName);
                }
            }

            ProcessedType processedType = ast.root.processedType;
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

            for (int i = 0; i < ast.styles.size; i++) {
                ref StyleDefinition styleDef = ref ast.styles.array[i];

                StyleSheet sheet = templateData.ImportStyleSheet(styleDef);

                if (sheet != null) {
                    ctx.AddStyleSheet(styleDef.alias, sheet);
                }
            }

            {
                ctx.PushBlock();

                ctx.CommentNewLineBefore("new " + processedType.rawType);
                Expression createRootExpression = ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                    Expression.Constant(processedType.id),
                    Expression.Default(typeof(UIElement)), // root has no parent
                    Expression.Constant(CountActualChildren(ast.root)),
                    Expression.Constant(ast.root.GetAttributeCount()),
                    Expression.Constant(ctx.compiledTemplate.templateId)
                );

                // root = templateScope.application.CreateFromPool<Type>(attrCount, childCount);
                ctx.Assign(ctx.rootParam, createRootExpression);

                ctx.IfEqualsNull(ctx.rootParam, ctx.PopBlock());
            }
            VisitChildren(ast.root, ctx, retn);
            ctx.Return(ctx.rootParam);
            LightList<string>.Release(ref namespaces);
            retn.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, scopeParam);
            return retn;
        }

        private void VisitChildren(TemplateNode node, CompilationContext ctx, CompiledTemplate retn) {
            if (node.children == null || node.children.Count <= 0) {
                return;
            }

            ctx.PushScope();
            
            Expression parentChildList = Expression.Field(ctx.ParentExpr, s_Element_ChildrenList);

            Expression parentChildListArray = Expression.Field(parentChildList, s_LightList_Element_Array);

            // childList[idx] = Visit()
            for (int i = 0; i < node.children.Count; i++) {
                Expression visit = Visit(node.children[i], ctx, retn);
                // will be null for stored templates
                if (visit != null) {
                    // childList.array[i] = targetElement_x;
                    ctx.Assign(Expression.ArrayAccess(parentChildListArray, Expression.Constant(i)), visit);
                }
            }

            ctx.PopScope();
        }

        private static ProcessedType ResolveGenericElementType(TemplateNode templateNode) {
            ProcessedType processedType = templateNode.processedType;

            GenericElementTypeResolvedByAttribute attr = processedType.rawType.GetCustomAttribute<GenericElementTypeResolvedByAttribute>();

            if (attr == null) {
                throw CompileException.GenericElementMissingResolver(processedType);
            }

            AttributeDefinition2[] attributes = templateNode.attributes.array;
            s_TypeResolver.Reset();

            s_TypeResolver.SetSignature(new Parameter(templateNode.RootType, "__root", ParameterFlags.NeverNull));
            s_TypeResolver.SetImplicitContext(s_TypeResolver.GetParameter("__root"));

            for (int i = 0; i < templateNode.attributes.size; i++) {
                if (attributes[i].type == AttributeType.Property) {
                    if (attributes[i].key == attr.propertyName) {
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
            }

            throw CompileException.UnresolvedGenericElement(processedType);
        }

        private ParameterExpression Visit(TemplateNode templateNode, in CompilationContext ctx, CompiledTemplate template) {
            ParameterExpression nodeExpr = ctx.ElementExpr;

            if (templateNode.processedType.IsUnresolvedGeneric) {
                templateNode.processedType = ResolveGenericElementType(templateNode);
            }

            ProcessedType processedType = templateNode.processedType;
            ctx.elementType = processedType; // replace w/ stack push of (processedType, variableExpression)?
            ctx.CommentNewLineBefore(templateNode.originalString);
            int trueAttrCount = templateNode.GetAttributeCount();

            int ctxVarCount = 0;

            for (int i = 0; i < templateNode.attributes.size; i++) {
                if (templateNode.attributes.array[i].type == AttributeType.ContextVariable) {
                    // assign an id 
                    // validate no conflicting aliases defined
                    // add to list, later, remove
                    // need to know if it is exposed outside the template or not
                    ContextVariableDefinition ctxVar = new ContextVariableDefinition() {
                        id = NextContextId,
                        name = templateNode.attributes.array[i].key,
                        isExposed = true
                    };

                    contextVarStack.Push(ctxVar);
                    ctxVarCount++;
                }
            }

            TemplateNodeType templateNodeType = templateNode.GetTemplateType();
            switch (templateNodeType) {
                // can be anywhere in the template except inside a template (<Template:Name> <SlotContent:xxx> would be illegal)
                case TemplateNodeType.SlotDefinition: {
                    return VisitSlotDefinition(templateNode, ctx, template);
                }

                case TemplateNodeType.SlotTemplate: {
                    return VisitTemplateSlot(templateNode, ctx, template);
                }

                // must be a direct child of the node the slot is exposed on
                case TemplateNodeType.SlotContent: {
                    // parent element is guaranteed to have been fully compiled. i should know the context stack for this slot at this point
                    // maybe save state of each stack for each slot input data
                    // while compiling we need a regular stack, 1 is fine.
                    // when compiling slot definition need to save current variable stack state so when its used later it can be restored
                    // actual data will be resolved at run time but ids, types, and aliases should be saved for later use.
                    // resolvers = StackUtil.Combine(currentStack, slot.resolverStack);

                    return VisitSlotContent(templateNode, ctx, template);
                }

                case TemplateNodeType.HydrateElement: {
                    // var oldStack = stack;
                    // stack = new stack
                    // when jumping into a new tplate we need to store current variable stack state, then restore after compiling.
                    CompiledTemplate expandedTemplate = GetCompiledTemplate(processedType); // probably needs a ref to old stack for slot alias resolution
                    // stack = old stack

                    ctx.Assign(nodeExpr, ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                        Expression.Constant(processedType.id),
                        ctx.ParentExpr,
                        Expression.Constant(templateNode.children.size),
                        Expression.Constant(trueAttrCount),
                        Expression.Constant(ctx.compiledTemplate.templateId)
                    ));

                    // MergeTemplateAttributes(templateNode, expandedTemplate.attributes, templateNode.attributes); todo -- enable this

                    OutputAttributes(ctx, templateNode);

                    CompileElementData(templateNode, ctx);

                    Expression slotUsageExpr = Expression.Default(typeof(StructList<SlotUsage>));

                    bool mustRecycle = false;
                    // if template has any children they must be slots
                    // also make sure the template exposes matching slot name (implicit children or named)
                    if (templateNode.children != null && templateNode.children.size > 0) {
                        // these children are eligible for context from the root and the internals of the hydrated parent template
                        StructList<CompileTimeSlotUsage> slotUsages = VisitHydratedChildren(templateNode, ctx, template, expandedTemplate);

                        // this might be 0 in the case where we create templates instead of children -- todo implement this feature
                        if (slotUsages.size > 0) {
                            mustRecycle = true;
                            slotUsageExpr = ctx.GetVariable(typeof(StructList<SlotUsage>), "slotUsage");
                            ctx.Assign(slotUsageExpr, Expression.Call(null, s_SlotUsageList_PreSize, Expression.Constant(slotUsages.size)));
                            MemberExpression arrayAccess = Expression.MakeMemberAccess(slotUsageExpr, s_SlotUsageList_Array);

                            for (int i = 0; i < slotUsages.size; i++) {
                                IndexExpression arrayIndex = Expression.ArrayAccess(arrayAccess, Expression.Constant(i));
                                ctx.Comment(slotUsages[i].variableName);
                                ctx.Assign(arrayIndex, Expression.New(s_SlotUsage_Ctor,
                                    Expression.Constant(slotUsages[i].slotName),
                                    Expression.Constant(slotUsages[i].slotId)
                                ));
                            }
                        }
                    }

                    // templateScope = new TemplateScope2(application, slotUsages ?? null);
                    Expression templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr, slotUsageExpr, Expression.Default(typeof(UIElement)));

                    // scope.application.HydrateTemplate(templateId, targetElement, templateScope)
                    ctx.CommentNewLineBefore(expandedTemplate.filePath);
                    ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_Application_HydrateTemplate, Expression.Constant(expandedTemplate.templateId), nodeExpr, templateScopeCtor));

                    if (mustRecycle) {
                        ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(slotUsageExpr, s_SlotUsageList_Release));
                    }

                    break;
                }

                case TemplateNodeType.TextElement: {
                    ctx.Assign(nodeExpr, ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                            Expression.Constant(processedType.id),
                            ctx.ParentExpr,
                            Expression.Constant(templateNode.children.size),
                            Expression.Constant(trueAttrCount),
                            Expression.Constant(ctx.compiledTemplate.templateId)
                        )
                    );

                    OutputAttributes(ctx, templateNode);
                    CompileElementData(templateNode, ctx);

                    if (templateNode.textContent != null && templateNode.textContent.size > 0) {
                        if (templateNode.IsTextConstant()) {
                            // ((UITextElement)element).text = "string value";
                            ctx.Assign(Expression.MakeMemberAccess(Expression.Convert(nodeExpr, typeof(UITextElement)), s_TextElement_Text), Expression.Constant(templateNode.GetStringContent()));
                        }
                    }

                    VisitChildren(templateNode, ctx, template);
                    return nodeExpr;
                }

                case TemplateNodeType.ContainerElement: {
                    ctx.CommentNewLineBefore("new " + processedType.rawType);
                    ctx.Assign(nodeExpr, ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                        Expression.Constant(processedType.id),
                        ctx.ParentExpr,
                        Expression.Constant(templateNode.children.size),
                        Expression.Constant(trueAttrCount),
                        Expression.Constant(ctx.compiledTemplate.templateId)
                    ));
                    OutputAttributes(ctx, templateNode);
                    CompileElementData(templateNode, ctx);
                    VisitChildren(templateNode, ctx, template);
                    return nodeExpr;
                }
            }

            for (int i = 0; i < ctxVarCount; i++) {
                contextVarStack.Pop();
            }

            return nodeExpr;
        }
        
        private StructList<CompileTimeSlotUsage> VisitHydratedChildren(TemplateNode node, CompilationContext ctx, CompiledTemplate template, CompiledTemplate expandedTemplate) {
            StructList<CompileTimeSlotUsage> slotList = StructList<CompileTimeSlotUsage>.Get();

            for (int i = 0; i < node.children.size; i++) {
                if (TryFindMatchingSlot(expandedTemplate.slotDefinitions, node.children.array[i], out SlotDefinition slotDefinition)) {
                    if (slotDefinition.IsTemplate) {
                        // get context variables off of slot definition
                        CompiledSlot slot = CompileSlot(node.children[i], ctx, template, slotDefinition);
                        slotList.Add(new CompileTimeSlotUsage(slot.slotName, slot.slotId, slot.GetVariableName()));
                    }
                    else {
                        
                        if (slotDefinition.contextAttributes != null) {
                            StructList<AttributeDefinition2> attrs = slotDefinition.contextAttributes.Clone();
                            attrs.AddRange(node.children[i].attributes);
                            node.children[i].attributes = attrs;
                        }

                        CompiledSlot slot = CompileSlot(node.children[i], ctx, template);
                        slotList.Add(new CompileTimeSlotUsage(slot.slotName, slot.slotId, slot.GetVariableName()));
                    }

                    continue;
                }

                throw CompileException.UnmatchedSlot(node.GetSlotName(), expandedTemplate.filePath);
            }

            return slotList;
        }

        private bool TryFindMatchingSlot(StructList<SlotDefinition> slotDefinitions, TemplateNode child, out SlotDefinition slotDefinition) {
            string slotName = child.GetSlotName();
            for (int i = 0; i < slotDefinitions.size; i++) {
                if (slotDefinitions.array[i].slotName == slotName) {
                    slotDefinition = slotDefinitions.array[i];
                    return true;
                }
            }

            slotDefinition = default;
            return false;
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

        private void CompileElementData(TemplateNode templateNode, CompilationContext ctx) {
            int count = 0;

            if (templateNode.attributes != null) {
                count = templateNode.attributes.size;
            }

            InitializeCompilers(templateNode.RootType, templateNode.ElementType);
            int updateBindingCount = 0;
            int enabledBindingCount = 0;
            int createdBindingCount = 0;

            StructList<AttributeDefinition2> perFrameStyles = null;
            StructList<DynamicStyleData> dynamicStyleData = null;
            StructList<AttributeDefinition2> inputList = null;

            // todo -- handle nested access <Element thing.value.x="144f" keydown="" key-filter:keydown.keyup.withfocus="[allDown(shift, c, k), NoneOf()]"/>
            // todo -- handle .read.write bindings
            // todo -- handle styles
            // todo -- handle input callbacks
            // todo -- handle context variables

            for (int i = 0; i < count; i++) {
                // ReSharper disable once PossibleNullReferenceException
                ref AttributeDefinition2 attr = ref templateNode.attributes.array[i];
                switch (attr.type) {
                    case AttributeType.Slot:
                        break;
                    case AttributeType.Context:
                    case AttributeType.ContextVariable: {
                        createdBindingCount++;

                        // todo -- error if context has this name in current hierarchy already for this template
                        ctx.PushContextVariable(attr.key);
                        createdCompiler.SetImplicitContext(createdCompiler.GetVariable(k_CastRoot));
                        Type expressionType = createdCompiler.GetExpressionType(attr.value);

                        Type type = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), expressionType);
                        ReflectionUtil.TypeArray2[0] = typeof(int);
                        ReflectionUtil.TypeArray2[1] = typeof(string);
                        ConstructorInfo ctor = type.GetConstructor(ReflectionUtil.TypeArray2);
                        int aliasId = NextContextId;
                        Expression contextVariable = Expression.New(ctor, Expression.Constant(aliasId), Expression.Constant(attr.key));
                        Expression access = Expression.MakeMemberAccess(createdCompiler.GetVariable(k_CastElement), s_Element_BindingNode);
                        Expression createVariable = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_CreateLocalContextVariable, contextVariable);

                        createdCompiler.RawExpression(createVariable);

                        PushContextVarAliasResolver(aliasId, attr.key, expressionType);

                        if ((attr.flags & AttributeFlags.Const) != 0) {
                            // already incremented created count
                            CompileAssignContextVariable(createdCompiler, attr, ctx, type);
                        }
                        else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                            enabledBindingCount++;
                            CompileAssignContextVariable(enabledCompiler, attr, ctx, type);
                        }
                        else {
                            updateBindingCount++;
                            CompileAssignContextVariable(updateCompiler, attr, ctx, type);
                        }

                        break;
                    }
                    case AttributeType.Alias: {
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
                                        Type type = updateCompiler.GetExpressionType(expr.text);
                                        // todo -- handle list types
                                        if (type != typeof(string) && !typeof(IList<string>).IsAssignableFrom(type) && !typeof(IList<UIStyleGroupContainer>).IsAssignableFrom(type)) {
                                            throw new CompileException("Invalid dynamic style type: " + type);
                                        }

                                        dynamicStyleData.Add(new DynamicStyleData(expr.text, type, false));
                                    }
                                    else {
                                        string[] parts = expr.text.Split(s_StyleSeparator, StringSplitOptions.RemoveEmptyEntries);

                                        foreach (string p in parts) {
                                            dynamicStyleData.Add(new DynamicStyleData(p, typeof(UIStyleGroupContainer), true));
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            // this is an instance style, todo -- handle .once etc
                            throw new NotImplementedException();
                        }

                        break;
                    }

                    case AttributeType.Attribute: {
                        if ((attr.flags & AttributeFlags.Const) != 0) {
                            continue; // todo -- make part of created instead? 
                        }
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

            if (templateNode.textContent != null && templateNode.textContent.size > 0 && !templateNode.IsTextConstant()) {
                updateBindingCount++;

                updateCompiler.AddNamespace("UIForia.Util");
                updateCompiler.AddNamespace("UIForia.Text");

                updateCompiler.SetImplicitContext(updateCompiler.GetVariable(k_CastRoot));
                StructList<TextExpression> expressionParts = templateNode.textContent;

                for (int i = 0; i < expressionParts.size; i++) {
                    // text joiner
                    // convert text expression outputs to an array 
                    // output = ["text", expression, "here"].Join();
                    // later -> visit any non const expressions and break apart by top level string-to-string + operator

                    if (expressionParts[i].isExpression) {
                        Expression val = updateCompiler.Value(expressionParts[i].text);
                        MethodCallExpression toString = ExpressionFactory.CallInstanceUnchecked(val, val.Type.GetMethod("ToString", Type.EmptyTypes));
                        updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendString, toString));
//                        updateCompiler.Statement($"TextUtil.StringBuilder.Append({expressionParts[i].text})");
                    }
                    else {
                        updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendString, Expression.Constant(expressionParts[i].text)));
                    }
                }

                Expression e = ExpressionFactory.Convert(updateCompiler.GetVariable(k_CastElement), typeof(UITextElement));
                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(e, s_TextElement_SetText, s_StringBuilderToString));
                updateCompiler.RawExpression(s_StringBuilderClear);
            }

            // if we have style bindings they need to run after Update() is called (or where it would have been called if it would have been present)
            if (dynamicStyleData != null) {
                updateBindingCount++;

                // todo -- to handle array case we can't use PreSize, need to call Add on the list since size will be dynamic
                ParameterExpression styleList = updateCompiler.AddVariable(
                    new Parameter<LightList<UIStyleGroupContainer>>("styleList", ParameterFlags.NeverNull),
                    ExpressionFactory.CallStaticUnchecked(s_LightList_UIStyleGroupContainer_PreSize, Expression.Constant(dynamicStyleData.size))
                );

                Expression styleListArray = Expression.MakeMemberAccess(styleList, s_LightList_UIStyleGroupContainer_Array);

                for (int s = 0; s < dynamicStyleData.size; s++) {
                    IndexExpression arrayIndex = Expression.ArrayAccess(styleListArray, Expression.Constant(s));
                    ref DynamicStyleData data = ref dynamicStyleData.array[s];

                    updateCompiler.Comment(data.text);

                    if (data.isConstant) {
                        int styleId = ctx.ResolveStyleName(data.text);
                        // todo use expression
                        updateCompiler.Assign(arrayIndex, $"__element.{nameof(UIElement.templateMetaData)}.GetStyleById({styleId})", false);
                    }
                    else {
                        Type type = data.returnType;
                        if (type == typeof(string)) {
                            // todo -- don't allocate w/ aliases, use char array where possible
                            // todo use expression
                            updateCompiler.Assign(arrayIndex, $"__element.{nameof(UIElement.templateMetaData)}.{nameof(TemplateMetaData.ResolveStyleByName)}({data.text})", false);
                        }
                    }
                }

                updateCompiler.SetNullCheckingEnabled(false); // todo use expression
                updateCompiler.Statement($"__element.style.{nameof(UIStyleSet.SetBaseStyles)}({styleList.Name})");
                updateCompiler.SetNullCheckingEnabled(true);
            }

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
            if (updateBindingCount + createdBindingCount + enabledBindingCount > 0) {
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
        }

        private enum AliasResolverType {

            MouseEvent,
            KeyEvent,
            TouchEvent,
            Element,
            Parent,
            ContextVariable,
            ControllerEvent,

            Root

        }

        private struct ContextVarAliasResolver {

            public string name;
            public string strippedName;
            public Type type;
            public int id;
            public AliasResolverType resolverType;

            public ContextVarAliasResolver(string name, Type type, int id, AliasResolverType resolverType) {
                this.name = name[0] == '$' ? name : '$' + name;
                this.strippedName = name.Substring(0);
                this.type = type;
                this.id = id;
                this.resolverType = resolverType;
            }

            public Expression Resolve(LinqCompiler compiler) {
                switch (resolverType) {
                    case AliasResolverType.MouseEvent:
                        return compiler.Value(k_InputEventParameterName + "." + nameof(GenericInputEvent.AsMouseInputEvent));

                    case AliasResolverType.KeyEvent:
                        return compiler.Value(k_InputEventParameterName + "." + nameof(GenericInputEvent.AsKeyInputEvent));

                    case AliasResolverType.TouchEvent:
                    case AliasResolverType.ControllerEvent:
                        throw new NotImplementedException();

                    case AliasResolverType.Element:
                        return compiler.GetVariable(k_CastElement);

                    case AliasResolverType.Root:
                        return compiler.GetVariable(k_CastRoot);

                    case AliasResolverType.Parent: // todo -- use expressions
                        return compiler.Value(k_CastElement + ".parent");

                    case AliasResolverType.ContextVariable: {
                        ParameterExpression el = compiler.GetVariable(k_CastElement);
                        Expression access = Expression.MakeMemberAccess(el, s_Element_BindingNode);
                        Expression call = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_GetContextVariable, Expression.Constant(id));
                        Type contextVarType = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), type);

                        UnaryExpression convert = Expression.Convert(call, contextVarType);
                        ParameterExpression variable = compiler.AddVariable(type, $"ctxvar_{strippedName}");

                        compiler.Assign(variable, Expression.MakeMemberAccess(convert, contextVarType.GetField("value")));
                        return variable;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

        }

        private void PushContextVarAliasResolver(int id, string name, Type type) {
            resolvers.Push(new ContextVarAliasResolver(name, type, id, AliasResolverType.ContextVariable));
        }

        private void PopAliasResolver() {
            resolvers.Pop();
        }

        private Expression ResolveAlias(string aliasName, LinqCompiler compiler) {
            for (int i = 0; i < resolvers.size; i++) {
                ContextVarAliasResolver resolver = resolvers.PeekAtUnchecked(i);
                if (resolver.name == aliasName) {
                    return resolver.Resolve(compiler);
                }
            }

            return null;
        }

        private void InitializeCompilers(Type rootType, Type elementType) {
            updateCompiler.Reset();
            enabledCompiler.Reset();
            createdCompiler.Reset();

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

        public static void CompileAssignContextVariable(LinqCompiler compiler, in AttributeDefinition2 attr, CompilationContext ctx, Type contextVarType) {
            //ContextVariable<T> ctxVar = (ContextVariable<T>)__castElement.bindingNode.GetContextVariable(id);
            //ctxVar.value = expression;
            compiler.SetImplicitContext(compiler.GetVariable(k_CastRoot));
            // todo -- convert to generic call: __castElement.bindingNode.SetContextVariable<string>(id, "hello")
            // need to use var method = type.GetMethod("name").MakeGenericMethod(type);
            Expression access = Expression.MakeMemberAccess(compiler.GetVariable(k_CastElement), s_Element_BindingNode);
            Expression call = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_GetLocalContextVariable, Expression.Constant(attr.key)); // todo -- maybe resolve by id instead
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
            LinqCompiler closure = compiler.CreateClosure(parameters, typeof(void));

            closure.Statement(attr.value);
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

            LightList<Parameter> parameters = LightList<Parameter>.Get();
            parameters.Add(new Parameter<GenericInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));

            resolvers.Push(new ContextVarAliasResolver(k_InputEventAliasName, typeof(MouseInputEvent), NextContextId, AliasResolverType.MouseEvent));

            compiler.SetImplicitContext(compiler.GetVariable(k_CastRoot));
            // todo -- eliminate generated closure by passing in template root and element from input system
            LinqCompiler closure = compiler.CreateClosure(parameters, typeof(void));

            closure.Statement(attr.value);
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
            LightList<Parameter>.Release(ref parameters);
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
                    compiler.SetImplicitContext(compiler.GetVariable(k_CastElement));
                }
                else {
                    compiler.SetImplicitContext(compiler.GetVariable(k_CastRoot));
                }

                compiler.Statement($"{k_CastElement}.SetEnabled({attr.value})");
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
            ExpressionFactory.CallInstanceUnchecked(element, s_UIElement_SetAttribute, Expression.Constant(attr.key), value);
//            compiler.Statement($"{k_CastElement}.SetAttribute('{attr.key}', {attr.StrippedValue})");
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

            StructList<ProcessedType.PropertyChangeHandlerDesc> changeHandlers = StructList<ProcessedType.PropertyChangeHandlerDesc>.Get();
            templateNode.processedType.GetChangeHandlers(attributeDefinition.key, changeHandlers);

            bool isProperty = ReflectionUtil.IsProperty(castElement.Type, attributeDefinition.key);

            // if there is a change handler or the member is a property we need to check for changes
            // otherwise field values can be assigned w/o checking
            if (changeHandlers.size > 0 || isProperty) {
                compiler.IfNotEqual(left, right, () => {
                    compiler.Assign(left, right);
                    for (int j = 0; j < changeHandlers.size; j++) {
                        compiler.Statement($"{k_CastElement}.{changeHandlers[j].methodInfo.Name}()");
                    }
                });
            }
            else {
                compiler.Assign(left, right);
            }

            compiler.EndIsolatedSection();
            changeHandlers.Release();
        }

        private ParameterExpression VisitSlotContent(TemplateNode templateNode, CompilationContext ctx, CompiledTemplate template) {
            ProcessedType processedType = templateNode.processedType;
            ctx.elementType = processedType;
            ParameterExpression nodeExpr = ctx.ElementExpr;

            // todo -- validate slot name exists, is not yet used, and is valid based on nesting hierarchy

            CompiledSlot compiledSlot = CompileSlot(templateNode, ctx, template);

            // needs a full recursive hydration for slot inputs I think, at least it needs to message available slots up to its template owner? maybe done in the parser
            return nodeExpr;
        }

        private ParameterExpression VisitTemplateSlot(TemplateNode templateNode, CompilationContext ctx, CompiledTemplate template) {
            // templateNode.ElementType = typeof(SlotTemplateElement);

            string slotName = templateNode.GetSlotName();

            // template slots need to generate an element immediately. this element has no children but a binding node that will define it based on it's 'slot:count' attribute expression
            // this element's children then become the actual slot data
            ctx.Comment("new " + nameof(SlotTemplateElement));
            Expression templateNodeExpr = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(TypeProcessor.GetProcessedType((typeof(SlotTemplateElement))).id),
                ctx.rootParam,
                Expression.Constant(0),
                Expression.Constant(templateNode.GetAttributeCount()),
                Expression.Constant(ctx.compiledTemplate.templateId)
            );

            // if has children, compile as default template 
            //     CompiledSlot compiledSlot = CompileSlot(templateNode, ctx, template);

//            SlotTemplateElement slotTemplateElement = (SlotTemplateElement) targetElement_1;
//            slotTemplateElement.templateId = Application.ResolveSlotId("children", scope.slotInputs, -1);
            ParameterExpression nodeExp = ctx.ElementExpr;
            ctx.Assign(ctx.ElementExpr, templateNodeExpr);

            MemberExpression field = Expression.Field(ExpressionFactory.Convert(ctx.ElementExpr, typeof(SlotTemplateElement)), typeof(SlotTemplateElement).GetField(nameof(SlotTemplateElement.templateId)));
            MemberExpression slotList = Expression.Field(ctx.templateScope, s_TemplateScope_SlotList);

            MethodCallExpression resolvedId = Expression.Call(null, s_Application_s_ResolveSlotId, Expression.Constant(slotName), slotList, Expression.Constant(-1));

            ctx.Assign(field, resolvedId);

//            StructList<AttributeDefinition2> contextVarAttributes = templateNode.RemoveContextVarAttributes();

            SlotDefinition slotDefinition = new SlotDefinition(slotName, null, SlotType.Template);
            // needs a ref to context variables

//            slotDefinition.contextAttributes = contextVarAttributes;
            template.AddSlotData(slotDefinition);

            CompileElementData(templateNode, ctx);

            return nodeExp;
        }

        // todo -- still need to figure out if slots should be elements or not, and if not what their behavior is
        private ParameterExpression VisitSlotDefinition(TemplateNode templateNode, CompilationContext ctx, CompiledTemplate template) {
            ProcessedType processedType = templateNode.processedType;
            ctx.elementType = processedType;
            string slotName = templateNode.GetSlotName();
            // slot names are unique per template (but not globally)
            if (template.TryGetSlotData(slotName, out _)) {
                throw new TemplateParseException("todo -- file", "Duplicate slot detected: " + templateNode.GetSlotName());
            }

            StructStack<ContextVariableDefinition> contextStack = template.contextStack?.Clone();
            SlotDefinition slotDefinition = new SlotDefinition(templateNode.GetSlotName(), contextStack, SlotType.Default);
            slotDefinition.contextAttributes = templateNode.attributes.Clone();
            slotDefinition.contextVariables = contextVarStack.Clone();

            template.AddSlotData(slotDefinition);

            ParameterExpression nodeExpr = ctx.ElementExpr;
            CompiledSlot compiledSlot = CompileSlot(templateNode, ctx, template, slotDefinition);
            Expression slotList = Expression.MakeMemberAccess(ctx.templateScope, s_TemplateScope_SlotList);
            Expression templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr, slotList, Expression.Default(typeof(UIElement)));
            Expression resolveSlot = Expression.Call(null, s_Application_s_ResolveSlotId, Expression.Constant(slotName), slotList, Expression.Constant(compiledSlot.slotId));
            ctx.Assign(ctx.ElementExpr, Expression.Call(ctx.applicationExpr, s_Application_CreateSlot, resolveSlot, ctx.rootParam, ctx.ParentExpr, templateScopeCtor));

            return nodeExpr;
        }

        private CompiledSlot CompileSlot(TemplateNode templateNode, CompilationContext parentCtx, CompiledTemplate template, SlotDefinition slotDefinition = default) {
            // want a fresh set of variables but keep the style / file / binding data / etc contexts
            CompiledSlot retn = templateData.CreateSlot(templateNode.astRoot.fileName, templateNode.GetSlotName(), templateNode.slotType);
            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");
            LightList<string> namespaces = LightList<string>.Get();

            // todo -- use parent context for style & imports n stuff
            CompilationContext ctx = new CompilationContext();
            ParameterExpression slotRootParam = ctx.GetVariable(typeof(UISlotOverride), "slotRoot");
            ctx.rootType = parentCtx.rootType;
            ctx.rootParam = slotRootParam;
            ctx.templateScope = scopeParam;
            ctx.elementType = templateNode.processedType;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.Initialize(slotRootParam);
            ctx.compiledTemplate = parentCtx.compiledTemplate; // todo -- might be wrong

            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(TypeProcessor.GetProcessedType(typeof(UISlotOverride)).id),
                Expression.Default(typeof(UIElement)), // todo -- parent is null, fix this
                Expression.Constant(templateNode.children.size), // todo -- probably not child count since slots aren't real children and might be actually be templates
                Expression.Constant(templateNode.GetAttributeCount()),
                Expression.Constant(parentCtx.compiledTemplate.templateId)
            );

//            if (slotDefinition.contextAttributes != null) {
//                for (int i = 0; i < slotDefinition.contextAttributes.size; i++) {
//                    AttributeDefinition2 attr = slotDefinition.contextAttributes.array[i];
//                    // binding has no idea what original inner context was, only outer
//                    // need to track root type? wouldn't have reference 
//                    // context variable needs an id on it to be resolved later
//                    // ids need to be unique
//                    // context var needs to refer to the inner root element to be properly resolved
//                    // when compiling a slot, need to restore the ctx list to what it was where slot was defined
////                    resolvers.Push(new ContextVariable<string>(id));
//                    templateNode.attributes.Add(attr);
//                }
//            }


            // slotRootParam = templateScope.application.CreateFromPool<Type>(attrCount, childCount);
            ctx.Assign(slotRootParam, Expression.Convert(createRootExpression, typeof(UISlotOverride)));
            CompileElementData(templateNode, ctx);

            VisitChildren(templateNode, ctx, template); // todo -- what if these are themselves slots?
            ctx.Return(slotRootParam);
            LightList<string>.Release(ref namespaces);
            retn.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, scopeParam);
            return retn;
        }

        private static int CountActualChildren(TemplateNode templateNode) {
            int cnt = 0;
            for (int i = 0; i < templateNode.children.size; i++) {
                switch (templateNode.children.array[i].GetTemplateType()) {
                    case TemplateNodeType.Root:
                    case TemplateNodeType.HydrateElement:
                    case TemplateNodeType.ContainerElement:
                    case TemplateNodeType.TextElement:
                    case TemplateNodeType.SlotDefinition:
                    case TemplateNodeType.SlotTemplate:
                        cnt++;
                        break;
                }
            }

            return cnt;
        }

        /// <summary>
        /// Merges all attributes from the root of a hydrated template with those declared at the template usage site. The outer ones override inner ones.
        /// </summary>
        /// <param name="templateNode"></param>
        /// <param name="innerList"></param>
        /// <param name="outer"></param>
        private static void MergeTemplateAttributes(TemplateNode templateNode, StructList<AttributeDefinition2> innerList, StructList<AttributeDefinition2> outer) {
            StructList<AttributeDefinition2> mergedAttributes = StructList<AttributeDefinition2>.GetMinSize(innerList.size + outer.size);

            // match on type & name, might have to track source also in case of binding context

            // add all outer ones
            mergedAttributes.AddRange(outer);
            int outerCount = outer.size;
            AttributeDefinition2[] mergedArray = mergedAttributes.array;

            AttributeDefinition2[] inner = innerList.array;
            for (int i = 0; i < inner.Length; i++) {
                // for each inner attribute
                // if no match found, add it, be sure to set context flag
                string key = inner[i].key;
                AttributeType attributeType = inner[i].type;

                bool contains = false;
                for (int j = 0; j < outerCount; j++) {
                    // if key and type matches, break out of the loop
                    if (mergedArray[j].key == key && mergedArray[j].type == attributeType) {
                        contains = true;
                        break;
                    }
                }

                if (!contains) {
                    AttributeDefinition2 attr = inner[i];
                    attr.flags |= AttributeFlags.RootContext;
                    mergedAttributes.Add(attr);
                }
            }

            templateNode.attributes.Release();
            templateNode.attributes = mergedAttributes;
        }

    }

}