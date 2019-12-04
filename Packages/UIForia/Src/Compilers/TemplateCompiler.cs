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
        private readonly LightStack<AliasResolver> resolvers;

        private const string k_CastElement = "__castElement";
        private const string k_CastRoot = "__castRoot";
        private static readonly char[] s_StyleSeparator = new char[] {' '};

        private static readonly MethodInfo s_CreateFromPool = typeof(Application).GetMethod(nameof(Application.CreateElementFromPoolWithType));
        private static readonly MethodInfo s_BindingNodePool_Get = typeof(LinqBindingNode).GetMethod("Get", BindingFlags.Static | BindingFlags.Public);
        private static readonly FieldInfo s_StructList_ElementAttr_Array = typeof(StructList<ElementAttribute>).GetField("array");

        private static readonly MethodInfo s_SlotUsageList_PreSize = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.PreSize), BindingFlags.Static | BindingFlags.Public);
        private static readonly FieldInfo s_SlotUsageList_Array = typeof(StructList<SlotUsage>).GetField("array", BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo s_SlotUsageList_Release = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.Release), BindingFlags.Instance | BindingFlags.Public);

        private static readonly ConstructorInfo s_SlotUsage_Ctor = typeof(SlotUsage).GetConstructor(new[] {typeof(string), typeof(int)});

        private static readonly ConstructorInfo s_TemplateScope_Ctor = typeof(TemplateScope2).GetConstructor(new[] {typeof(Application), typeof(StructList<SlotUsage>)});
        private static readonly FieldInfo s_TemplateScope_SlotList = typeof(TemplateScope2).GetField(nameof(TemplateScope2.slotInputs));
        private static readonly FieldInfo s_TemplateScope_ApplicationField = typeof(TemplateScope2).GetField(nameof(TemplateScope2.application));

        private static readonly ConstructorInfo s_ElementAttributeCtor = typeof(ElementAttribute).GetConstructor(new[] {typeof(string), typeof(string)});
        private static readonly FieldInfo s_ElementAttributeList = typeof(UIElement).GetField("attributes", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_Element_ChildrenList = typeof(UIElement).GetField(nameof(UIElement.children), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_LightList_Element_Array = typeof(LightList<UIElement>).GetField(nameof(LightList<UIElement>.array), BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_TextElement_Text = typeof(UITextElement).GetField(nameof(UITextElement.text), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly MethodInfo s_LightList_UIStyleGroupContainer_PreSize = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.PreSize), BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_LightList_UIStyle_Release = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.Release), BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_LightList_UIStyleGroupContainer_Array = typeof(LightList<UIStyleGroupContainer>).GetField(nameof(LightList<UIStyleGroupContainer>.array), BindingFlags.Public | BindingFlags.Instance);

        private static readonly MethodInfo s_Application_CreateSlot = typeof(Application).GetMethod(nameof(Application.CreateSlot), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo s_Application_HydrateTemplate = typeof(Application).GetMethod(nameof(Application.HydrateTemplate), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo s_Application_s_ResolveSlotId = typeof(Application).GetMethod(nameof(Application.ResolveSlotId), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        private static readonly PropertyInfo s_Element_IsEnabled = typeof(UIElement).GetProperty(nameof(UIElement.isEnabled));
        private static readonly FieldInfo s_Element_BindingNode = typeof(UIElement).GetField(nameof(UIElement.bindingNode));

        private static readonly MethodInfo s_LinqBindingNode_CreateLocalContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.CreateLocalContextVariable));
        private static readonly MethodInfo s_LinqBindingNode_GetLocalContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetLocalContextVariable));
        private static readonly MethodInfo s_LinqBindingNode_GetContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetContextVariable));

        private static readonly MethodInfo s_EventUtil_Subscribe = typeof(EventUtil).GetMethod(nameof(EventUtil.Subscribe));

        private int contextId;

        public TemplateCompiler(TemplateSettings settings) {
            this.settings = settings;
            this.templateMap = new Dictionary<Type, CompiledTemplate>();
            this.compilationStack = new LightStack<Type>();
            this.xmlTemplateParser = new XMLTemplateParser();
            this.updateCompiler = new LinqCompiler();
            this.enabledCompiler = new LinqCompiler();
            this.createdCompiler = new LinqCompiler();
            this.resolvers = new LightStack<AliasResolver>();

            Func<string, LinqCompiler, Expression> resolveAlias = ResolveAlias;

            this.createdCompiler.resolveAlias = resolveAlias;
            this.enabledCompiler.resolveAlias = resolveAlias;
            this.updateCompiler.resolveAlias = resolveAlias;
        }

        private int NextContextId => contextId++;

        public virtual void CompileTemplates(Type rootType, CompiledTemplateData templateData) {
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

            retn.childCount = ast.root.children.size;

            LightList<string> namespaces = LightList<string>.Get();
            if (ast.usings != null) {
                for (int i = 0; i < ast.usings.size; i++) {
                    namespaces.Add(ast.usings[i].namespaceName);
                }
            }

            ProcessedType processedType = ast.root.processedType;
            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope2), "scope");

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

                Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                    Expression.Constant(processedType.rawType),
                    Expression.Default(typeof(UIElement)), // root has no parent
                    Expression.Constant(ast.root.children.size),
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
            for (int i = 0;
                i < node.children.Count;
                i++) {
                Expression visit = Visit(node.children[i], ctx, retn);
                // will be null for stored templates
                if (visit != null) {
                    // childList.array[i] = targetElement_x;
                    ctx.Assign(Expression.ArrayAccess(parentChildListArray, Expression.Constant(i)), visit);
                }
            }

            ctx.PopScope();
        }

        private ParameterExpression Visit(TemplateNode templateNode, in CompilationContext ctx, CompiledTemplate template) {
            ProcessedType processedType = templateNode.processedType;
            Type type = processedType.rawType;
            ParameterExpression nodeExpr = ctx.ElementExpr;
            ctx.elementType = processedType; // replace w/ stack push of (processedType, variableExpression)?
            ctx.CommentNewLineBefore($"{templateNode.originalString}");
            int trueAttrCount = templateNode.GetAttributeCount();

            TemplateNodeType templateNodeType = templateNode.GetTemplateType();
            switch (templateNodeType) {
                // can be anywhere in the template except inside a template (<Template:Name> <SlotContent:xxx> would be illegal)
                case TemplateNodeType.SlotDefinition: {
                    return VisitSlotDefinition(templateNode, ctx, template);
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
                    // when jumping into a new template we need to store current variable stack state, then restore after compiling.
                    CompiledTemplate expandedTemplate = GetCompiledTemplate(processedType); // probably needs a ref to old stack for slot alias resolution
                    // stack = old stack

                    ctx.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                        Expression.Constant(type),
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
                        StructList<CompileTimeSlotUsage> slotUsages = VisitHydratedChildren(templateNode, ctx, template);

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
                    Expression templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr, slotUsageExpr);

                    // scope.application.HydrateTemplate(templateId, targetElement, templateScope)
                    ctx.CommentNewLineBefore(expandedTemplate.filePath);
                    ctx.AddStatement(Expression.Call(ctx.applicationExpr, s_Application_HydrateTemplate, Expression.Constant(expandedTemplate.templateId), nodeExpr, templateScopeCtor));

                    if (mustRecycle) {
                        ctx.AddStatement(Expression.Call(slotUsageExpr, s_SlotUsageList_Release));
                    }

                    break;
                }

                case TemplateNodeType.TextElement: {
                    ctx.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                            Expression.Constant(type),
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
                    ctx.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                        Expression.Constant(type),
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

                case TemplateNodeType.Children: {
                    throw new NotImplementedException();
                    // todo -- I think this is never called since Children are really a slot
//                    ctx.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(type), ctx.ParentExpr, Expression.Constant(templateNode.children.size), Expression.Constant(trueAttrCount)));
//                    OutputAttributes(ctx, templateNode);
//                    CompileElementData(templateNode, ctx);
//                    VisitChildren(templateNode, ctx, template);
//                    return nodeExpr;
                }
            }

            return nodeExpr;
        }

        private StructList<CompileTimeSlotUsage> VisitHydratedChildren(TemplateNode node, CompilationContext ctx, CompiledTemplate template) {
            StructList<CompileTimeSlotUsage> slotList = StructList<CompileTimeSlotUsage>.Get();
            for (int i = 0;
                i < node.children.size;
                i++) {
                CompiledSlot slot = CompileSlot(node.children[i], ctx, template);
                slotList.Add(new CompileTimeSlotUsage(slot.slotName, slot.slotId, slot.GetVariableName()));
            }

            return slotList;
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

            // todo -- handle nested access <Element thing.value.x="144f" keydown="" key-filter:keydown.keyup.withfocus="[allDown(shift, c, k), NoneOf()]"/>
            // todo -- handle .read.write bindings
            // todo -- handle styles
            // todo -- handle input callbacks
            // todo -- handle context variables

            for (int i = 0; i < count; i++) {
                // ReSharper disable once PossibleNullReferenceException
                ref AttributeDefinition2 attr = ref templateNode.attributes.array[i];
                switch (attr.type) {
                    case AttributeType.Context:
                    case AttributeType.ContextVariable: {
                        createdBindingCount++;

                        // todo -- error if context has this name in current hierarchy already for this template
                        ctx.PushContextVariable(attr.key);

                        Type expressionType = createdCompiler.GetExpressionType(attr.value);

                        Type type = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), expressionType);
                        ReflectionUtil.TypeArray2[0] = typeof(int);
                        ReflectionUtil.TypeArray2[1] = typeof(string);
                        ConstructorInfo ctor = type.GetConstructor(ReflectionUtil.TypeArray2);
                        int aliasId = NextContextId;
                        Expression contextVariable = Expression.New(ctor, Expression.Constant(aliasId), Expression.Constant(attr.key));
                        Expression access = Expression.MakeMemberAccess(createdCompiler.GetVariable(k_CastElement), s_Element_BindingNode);
                        Expression createVariable = Expression.Call(access, s_LinqBindingNode_CreateLocalContextVariable, contextVariable);

                        createdCompiler.RawExpression(createVariable);

                        PushAliasResolver(aliasId, attr.key, expressionType);

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

                                ParameterExpression styleList = createdCompiler.AddVariable(new Parameter<LightList<UIStyle>>("styleList"), Expression.Call(null, s_LightList_UIStyleGroupContainer_PreSize, Expression.Constant(list.size)));

                                Expression styleListArray = Expression.MakeMemberAccess(styleList, s_LightList_UIStyleGroupContainer_Array);

                                for (int p = 0; p < parts.Length; p++) {
                                    int styleId = ctx.ResolveStyleName(parts[p]);
                                    IndexExpression arrayIndex = Expression.ArrayAccess(styleListArray, Expression.Constant(p));
                                    createdCompiler.Comment(parts[i]);
                                    createdCompiler.Assign(arrayIndex, $"__element.{nameof(UIElement.templateMetaData)}.GetStyleById({styleId})", false);
                                }

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
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


            if (templateNode.processedType.requiresUpdateFn) {
                updateBindingCount++;
                updateCompiler.Statement($"{k_CastElement}.{nameof(UIElement.OnUpdate)}()"); // todo change this to not require OnUpdate to be virtual
            }

            if (templateNode.textContent != null && templateNode.textContent.size > 0 && !templateNode.IsTextConstant()) {
                updateBindingCount++;

                updateCompiler.AddNamespace("UIForia.Util");
                updateCompiler.AddNamespace("UIForia.Text");
                updateCompiler.AddVariable(new Parameter<StringBuilder>("__stringBuilder", ParameterFlags.NeverNull), "TextUtil.StringBuilder");
                StructList<TextExpression> expressionParts = templateNode.textContent;

                for (int i = 0; i < expressionParts.size; i++) {
                    // text joiner
                    // convert text expression outputs to an array 
                    // output = ["text", expression, "here"].Join();
                    // later -> visit any non const expressions and break apart by top level string-to-string + operator

                    if (expressionParts[i].isExpression) {
                        updateCompiler.Statement($"__stringBuilder.Append({expressionParts[i].text})");
                    }
                    else {
                        updateCompiler.Statement($"__stringBuilder.Append('{expressionParts[i].text}')");
                    }
                }

                // todo -- either accept a StringBuilder here or create something else that acts like a string builder but doesn't allocate
                updateCompiler.Statement(k_CastElement + ".SetText(__stringBuilder.ToString())");
                updateCompiler.Statement("__stringBuilder.Clear()");
            }

            // if we have style bindings they need to run after Update() is called (or where it would have been called if it would have been present)
            if (dynamicStyleData != null) {
                updateBindingCount++;

                // todo -- to handle array case we can't use PreSize, need to call Add on the list since size will be dynamic
                ParameterExpression styleList = updateCompiler.AddVariable(
                    new Parameter<LightList<UIStyleGroupContainer>>("styleList", ParameterFlags.NeverNull),
                    Expression.Call(null, s_LightList_UIStyleGroupContainer_PreSize, Expression.Constant(dynamicStyleData.size))
                );

                Expression styleListArray = Expression.MakeMemberAccess(styleList, s_LightList_UIStyleGroupContainer_Array);

                for (int s = 0; s < dynamicStyleData.size; s++) {
                    IndexExpression arrayIndex = Expression.ArrayAccess(styleListArray, Expression.Constant(s));
                    ref DynamicStyleData data = ref dynamicStyleData.array[s];

                    updateCompiler.Comment(data.text);

                    if (data.isConstant) {
                        int styleId = ctx.ResolveStyleName(data.text);
                        updateCompiler.Assign(arrayIndex, $"__element.{nameof(UIElement.templateMetaData)}.GetStyleById({styleId})", false);
                    }
                    else {
                        Type type = data.returnType;
                        if (type == typeof(string)) {
                            // todo -- don't allocate w/ aliases, use char array where possible
                            updateCompiler.Assign(arrayIndex, $"__element.{nameof(UIElement.templateMetaData)}.{nameof(TemplateMetaData.ResolveStyleByName)}({data.text})", false);
                        }
                    }
                }

                updateCompiler.SetNullCheckingEnabled(false);
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
                ctx.AddStatement(Expression.Call(null, s_BindingNodePool_Get,
                        ctx.applicationExpr,
                        ctx.rootParam,
                        ctx.ElementExpr,
                        Expression.Constant(createdBindingId),
                        Expression.Constant(enabledBindingId),
                        Expression.Constant(updateBindingId)
                    )
                );
            }
        }

        public struct AliasResolver {

            public string name;
            public string strippedName;
            public Type type;
            public int id;

            public AliasResolver(string name, Type type, int id) {
                this.name = name[0] == '$' ? name : '$' + name;
                this.strippedName = name.Substring(0);
                this.type = type;
                this.id = id;
            }

            public Expression Resolve(LinqCompiler compiler) {
                ParameterExpression el = compiler.GetVariable(k_CastElement);
                Expression access = Expression.MakeMemberAccess(el, s_Element_BindingNode);
                Expression call = Expression.Call(access, s_LinqBindingNode_GetContextVariable, Expression.Constant(id));
                Type contextVarType = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), type);

                UnaryExpression convert = Expression.Convert(call, contextVarType);
                ParameterExpression variable = compiler.AddVariable(type, $"ctxvar_{strippedName}");

                compiler.Assign(variable, Expression.MakeMemberAccess(convert, contextVarType.GetField("value")));
                return variable;
            }

        }

        private void PushAliasResolver(int id, string name, Type type) {
            AliasResolver resolver = new AliasResolver(name, type, id);
            resolvers.Push(resolver);
        }

        private void PopAliasResolver() {
            resolvers.Pop();
        }

        private Expression ResolveAlias(string aliasName, LinqCompiler compiler) {
            for (int i = 0; i < resolvers.size; i++) {
                AliasResolver resolver = resolvers.PeekAtUnchecked(i);
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
            updateCompiler.SetSignature(
                new Parameter<UIElement>("__root", ParameterFlags.NeverNull),
                new Parameter<UIElement>("__element", ParameterFlags.NeverNull)
            );
            enabledCompiler.SetSignature(
                new Parameter<UIElement>("__root", ParameterFlags.NeverNull),
                new Parameter<UIElement>("__element", ParameterFlags.NeverNull)
            );
            createdCompiler.SetSignature(
                new Parameter<UIElement>("__root", ParameterFlags.NeverNull),
                new Parameter<UIElement>("__element", ParameterFlags.NeverNull)
            );

            // todo -- each compiler needs to handle namespaces and alias resolvers 
            Parameter elementParameter = new Parameter(elementType, k_CastElement, ParameterFlags.NeverNull);
            Parameter rootParameter = new Parameter(rootType, k_CastRoot, ParameterFlags.NeverNull);
            updateCompiler.AddVariable(elementParameter, Expression.Convert(updateCompiler.GetParameter("__element"), elementType));
            updateCompiler.AddVariable(rootParameter, Expression.Convert(updateCompiler.GetParameter("__root"), rootType));
            enabledCompiler.AddVariable(elementParameter, Expression.Convert(enabledCompiler.GetParameter("__element"), elementType));
            enabledCompiler.AddVariable(rootParameter, Expression.Convert(enabledCompiler.GetParameter("__root"), rootType));
            createdCompiler.AddVariable(elementParameter, Expression.Convert(createdCompiler.GetParameter("__element"), elementType));
            createdCompiler.AddVariable(rootParameter, Expression.Convert(createdCompiler.GetParameter("__root"), rootType));
        }

        public static void CompileAssignContextVariable(LinqCompiler compiler, in AttributeDefinition2 attr, CompilationContext ctx, Type contextVarType) {
            //ContextVariable<T> ctxVar = (ContextVariable<T>)__castElement.bindingNode.GetContextVariable(id);
            //ctxVar.value = expression;

            // todo -- convert to generic call: __castElement.bindingNode.SetContextVariable<string>(id, "hello")
            // need to use var method = type.GetMethod("name").MakeGenericMethod(type);
            Expression access = Expression.MakeMemberAccess(compiler.GetVariable(k_CastElement), s_Element_BindingNode);
            Expression call = Expression.Call(access, s_LinqBindingNode_GetLocalContextVariable, Expression.Constant(attr.key)); // todo -- maybe resolve by id instead
            Expression cast = Expression.Convert(call, contextVarType);
            ParameterExpression target = compiler.AddVariable(contextVarType, $"ctxVar_{attr.key}");
            compiler.Assign(target, cast);
            compiler.Assign($"ctxVar_{attr.key}.value", compiler.Value(attr.value), false);
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
                compiler.IfEqual(Expression.MakeMemberAccess(castElement, s_Element_IsEnabled), Expression.Constant(false), () => {
                    LabelTarget returnTarget = Expression.Label("early_out");
                    compiler.RawExpression(Expression.Return(returnTarget));
                });
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

            compiler.Statement($"{k_CastElement}.SetAttribute('{attr.key}', {attr.StrippedValue})");
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

        // todo -- still need to figure out if slots should be elements or not, and if not what their behavior is
        private ParameterExpression VisitSlotDefinition(TemplateNode templateNode, CompilationContext ctx, CompiledTemplate template) {
            ProcessedType processedType = templateNode.processedType;
            ctx.elementType = processedType;

            // slot names are unique per template (but not globally)
            if (template.TryGetSlotData(templateNode.slotName, out _)) {
                throw new TemplateParseException("todo -- file", "Duplicate slot detected: " + templateNode.slotName);
            }

            StructStack<ContextVariableDefinition> contextStack = template.contextStack?.Clone();
            template.AddSlotData(new SlotDefinition(templateNode.slotName, contextStack));
            ParameterExpression nodeExpr = ctx.ElementExpr;
            CompiledSlot compiledSlot = CompileSlot(templateNode, ctx, template);
            Expression slotList = Expression.MakeMemberAccess(ctx.templateScope, s_TemplateScope_SlotList);
            Expression templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr, slotList);
            Expression resolveSlot = Expression.Call(null, s_Application_s_ResolveSlotId, Expression.Constant(templateNode.slotName), slotList, Expression.Constant(compiledSlot.slotId));

            // todo -- do we need a root ref?
            ctx.Assign(ctx.ElementExpr, Expression.Call(ctx.applicationExpr, s_Application_CreateSlot, resolveSlot, ctx.ParentExpr, templateScopeCtor));
            return nodeExpr;
        }

        private CompiledSlot CompileSlot(TemplateNode templateNode, CompilationContext parentCtx, CompiledTemplate template) {
            // want a fresh set of variables but keep the style / file / binding data / etc contexts
            CompiledSlot retn = templateData.CreateSlot(templateNode.astRoot.fileName, templateNode.slotName, templateNode.slotType);
            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope2), "scope");
            LightList<string> namespaces = LightList<string>.Get();

            // todo -- use parent context for style & imports n stuff
            //            namespaces.AddRange(parentCtx.namespaces);
            CompilationContext ctx = new CompilationContext();
            ParameterExpression slotRootParam = ctx.GetVariable(typeof(UISlotContent), "slotRoot");
            ctx.rootType = parentCtx.rootType;
            ctx.rootParam = slotRootParam;
            ctx.templateScope = scopeParam;
            ctx.elementType = templateNode.processedType;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.Initialize(slotRootParam);
            ctx.compiledTemplate = parentCtx.compiledTemplate; // todo -- might be wrong

            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(typeof(UISlotContent)),
                Expression.Default(typeof(UIElement)),
                Expression.Constant(templateNode.children.size), // todo -- probably not child count since slots aren't real children and might be actually be templates
                Expression.Constant(templateNode.GetAttributeCount()),
                Expression.Constant(parentCtx.compiledTemplate.templateId)
            );


            // slotRootParam = templateScope.application.CreateFromPool<Type>(attrCount, childCount);
            ctx.Assign(slotRootParam, Expression.Convert(createRootExpression, typeof(UISlotContent)));
            VisitChildren(templateNode, ctx, template); // todo -- what if these are themselves slots?
            ctx.Return(slotRootParam);
            LightList<string>.Release(ref namespaces);
            retn.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, scopeParam);
            return retn;
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