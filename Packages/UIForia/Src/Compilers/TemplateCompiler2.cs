using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Linq.Expressions;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Systems;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public class TemplateCompiler2 {

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

        private static readonly MethodInfo s_CreateFromPool = typeof(Application).GetMethod(nameof(Application.CreateElementFromPoolWithType));
        private static readonly MethodInfo s_BindingNodePool_Get = typeof(LinqBindingNode).GetMethod("Get", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo s_GetStructList_ElementAttr = typeof(StructList<ElementAttribute>).GetMethod(nameof(StructList<ElementAttribute>.GetMinSize), new[] {typeof(int)});
        private static readonly FieldInfo s_StructList_ElementAttr_Size = typeof(StructList<ElementAttribute>).GetField("size");
        private static readonly FieldInfo s_StructList_ElementAttr_Array = typeof(StructList<ElementAttribute>).GetField("array");
        private static readonly FieldInfo s_Scope_ApplicationField = typeof(TemplateScope2).GetField("application");
        private static readonly FieldInfo s_Scope_CompiledTemplate = typeof(TemplateScope2).GetField(nameof(TemplateScope2.compiledTemplate));

        private static readonly ConstructorInfo s_ElementAttributeCtor = typeof(ElementAttribute).GetConstructor(new[] {typeof(string), typeof(string)});
        private static readonly ConstructorInfo s_TemplateScope_Ctor = typeof(TemplateScope2).GetConstructor(new[] {typeof(Application), typeof(LinqBindingNode), typeof(StructList<SlotUsage>)});
        private static readonly FieldInfo s_TemplateScope_SlotInputList = typeof(TemplateScope2).GetField(nameof(TemplateScope2.slotInputs));
        private static readonly ConstructorInfo s_LexicalScope_Ctor = typeof(LexicalScope).GetConstructor(new[] {typeof(UIElement), typeof(CompiledTemplate), typeof(StructList<SlotUsage>)});
        private static readonly FieldInfo s_StructList_SlotUsage_Array = typeof(StructList<SlotUsage>).GetField(nameof(StructList<SlotUsage>.array));

        private static readonly FieldInfo s_ElementAttributeList = typeof(UIElement).GetField("attributes", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_Element_ChildrenList = typeof(UIElement).GetField(nameof(UIElement.children), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_LightList_Element_Array = typeof(LightList<UIElement>).GetField(nameof(LightList<UIElement>.array), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo s_Application_HydrateTemplate = typeof(Application).GetMethod(nameof(Application.HydrateTemplate), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_TextElement_Text = typeof(UITextElement).GetField(nameof(UITextElement.text), BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo s_Application_CreateSlot = typeof(Application).GetMethod(nameof(Application.CreateSlot), BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly ConstructorInfo s_SlotUsage_Ctor = typeof(SlotUsage).GetConstructor(new[] {typeof(string), typeof(int), typeof(LexicalScope)});

        private static readonly FieldInfo s_LexicalScope_root = typeof(LexicalScope).GetField(nameof(LexicalScope.root), BindingFlags.Instance | BindingFlags.Public);
        private static readonly FieldInfo s_LexicalScope_data = typeof(LexicalScope).GetField(nameof(LexicalScope.data), BindingFlags.Instance | BindingFlags.Public);
        private static readonly FieldInfo s_LexicalScope_SlotInputList = typeof(LexicalScope).GetField(nameof(LexicalScope.slotInputList), BindingFlags.Instance | BindingFlags.Public);
        private static readonly PropertyInfo s_Element_IsEnabled = typeof(UIElement).GetProperty(nameof(UIElement.isEnabled));
        private static readonly FieldInfo s_Element_BindingNode = typeof(UIElement).GetField(nameof(UIElement.bindingNode));

        private static readonly MethodInfo s_LinqBindingNode_CreateLocalContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.CreateLocalContextVariable));
        private static readonly MethodInfo s_LinqBindingNode_GetLocalContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetLocalContextVariable));
        private static readonly MethodInfo s_LinqBindingNode_GetContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetContextVariable));
        
        private static readonly MethodInfo s_EventUtil_Subscribe = typeof(EventUtil).GetMethod(nameof(EventUtil.Subscribe));

        public TemplateCompiler2(TemplateSettings settings) {
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

        public void CompileTemplates(Type rootType, CompiledTemplateData templateData) {
            this.templateData = templateData;

            if (!typeof(UIElement).IsAssignableFrom(rootType)) { }

            if (typeof(UIContainerElement).IsAssignableFrom(rootType)) { }

            if (typeof(UITextElement).IsAssignableFrom(rootType)) { }

            // CompiledTemplate root = GetCompiledTemplate();

            GetCompiledTemplate(TypeProcessor.GetProcessedType(rootType));

//            foreach (KeyValuePair<Type,CompiledTemplate> keyValuePair in templateMap) {
//                templateData.AddTemplate();
//            }

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
            ctx.applicationExpr = Expression.Field(scopeParam, s_Scope_ApplicationField);
            ctx.templateData = Expression.Field(scopeParam, s_Scope_CompiledTemplate);
            ctx.compiledTemplate = retn;

            ctx.Initialize(rootParam);

            {
                ctx.PushBlock();

                Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                    Expression.Constant(processedType.rawType),
                    Expression.Default(typeof(UIElement)),
                    Expression.Constant(ast.root.children.size),
                    Expression.Constant(ast.root.GetAttributeCount())
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

        private ParameterExpression Visit(TemplateNode templateNode, in CompilationContext ctx, CompiledTemplate template) {
            ProcessedType processedType = templateNode.processedType;
            Type type = processedType.rawType;
            ParameterExpression nodeExpr = ctx.ElementExpr;

            ctx.elementType = processedType; // replace w/ stack push of (processedType, variableExpression)?

            ctx.CommentNewLineBefore($"{templateNode.originalString}");
            int trueAttrCount = templateNode.GetAttributeCount();

            TemplateNodeType templateNodeType = templateNode.GetTemplateType();

            switch (templateNodeType) {
                case TemplateNodeType.SlotDefinition: {
                    return VisitSlotDefinition(templateNode, ctx, template);
                }

                case TemplateNodeType.SlotContent: {
                    // parent element is guarenteed to have been fully compiled. i should know the context stack for this slot at this point
                    // maybe save state of each stack for each slot input data
                    // while compiling we need a regular stack, 1 is fine.
                    // when compiling slot definition need to save current variable stack state so when its used later it can be restored
                    // actual data will be resolved at run time but ids, types, and aliases should be saved for later use.
                    // resolvers = StackUtil.Combine(currentStack, slot.resolverStack);
                    break;
                }

                case TemplateNodeType.HydrateElement: {
                    // var oldStack = stack;
                    // stack = new stack
                    // when jumping into a new template we need to store current variable stack state, then restore after compiling.
                    CompiledTemplate expandedTemplate = GetCompiledTemplate(processedType); // probably needs a ref to old stack for slot alias resolution
                    // stack = old stack

                    ctx.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(type), ctx.ParentExpr, Expression.Constant(templateNode.children.size), Expression.Constant(trueAttrCount)));

                    //   MergeTemplateAttributes(templateNode, expandedTemplate.attributes, templateNode.attributes);

                    OutputAttributes(ctx, templateNode);
                    CompileElementData(templateNode, ctx);

                    // templateScope = new TemplateScope2(application, bindingNode, null);
                    Expression templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr, Expression.Default(typeof(LinqBindingNode)), Expression.Default(typeof(StructList<SlotUsage>)));

                    // scope.application.HydrateTemplate(templateId, targetElement, templateScope)
                    ctx.AddStatement(Expression.Call(ctx.applicationExpr, s_Application_HydrateTemplate, Expression.Constant(expandedTemplate.templateId), nodeExpr, templateScopeCtor));

                    ctx.AddStatement(Expression.Call(null, s_BindingNodePool_Get,
                            ctx.applicationExpr,
                            ctx.rootParam,
                            ctx.ElementExpr
                        )
                    );
                    break;
                }

                case TemplateNodeType.HydrateElementWithChildren: {
                    break;
                }

                case TemplateNodeType.TextElement: {
                    ctx.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(type), ctx.ParentExpr, Expression.Constant(templateNode.children.size), Expression.Constant(trueAttrCount)));
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
                    ctx.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(type), ctx.ParentExpr, Expression.Constant(templateNode.children.size), Expression.Constant(trueAttrCount)));
                    OutputAttributes(ctx, templateNode);
                    VisitChildren(templateNode, ctx, template);
                    return nodeExpr;
                }
            }

            return nodeExpr;
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

        public struct ElementCompilationData {

            public int contextVarCount;
            public int updateBindingId;
            public int enabledBindingId;
            public int createdBindingId;

        }

        private ElementCompilationData CompileElementData(TemplateNode templateNode, CompilationContext ctx) {
            int count = 0;

            if (templateNode.attributes != null) {
                count = templateNode.attributes.size;
            }

            InitializeCompilers(templateNode.RootType, templateNode.ElementType);

            ElementCompilationData retn = default;

            int updateBindingCount = 0;
            int enabledBindingCount = 0;
            int createdBindingCount = 0;

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
                        Expression contextVariable = Expression.New(type);
                        Expression access = Expression.MakeMemberAccess(createdCompiler.GetVariable(k_CastElement), s_Element_BindingNode);
                        Expression createVariable = Expression.Call(access, s_LinqBindingNode_CreateLocalContextVariable, contextVariable);

                        createdCompiler.RawExpression(createVariable);

                        PushAliasResolver(attr.key, expressionType);

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

            if (typeof(UITextElement).IsAssignableFrom(templateNode.processedType.rawType)) {
                // text binding here
            }

            if (templateNode.processedType.requiresUpdateFn) {
                updateBindingCount++;
                updateCompiler.Statement($"{k_CastElement}.{nameof(UIElement.OnUpdate)}()"); // todo change this to not require OnUpdate to be virtual
            }

            if (createdBindingCount > 0) {
                CompiledBinding createdBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnCreate);
                createdBinding.bindingFn = createdCompiler.BuildLambda();
                retn.createdBindingId = createdBinding.bindingId;
            }

            if (enabledBindingCount > 0) {
                CompiledBinding enabledBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnEnable);
                enabledBinding.bindingFn = enabledCompiler.BuildLambda();
                retn.enabledBindingId = enabledBinding.bindingId;
            }

            if (updateBindingCount > 0) {
                CompiledBinding updateBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnUpdate);
                updateBinding.bindingFn = updateCompiler.BuildLambda();
                retn.updateBindingId = updateBinding.bindingId;
            }

            return retn;
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

        private void PushAliasResolver(string name, Type type) {
            int id = aliasIdGenerator++;
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

            enabledCompiler.AddVariable(elementParameter, Expression.Convert(updateCompiler.GetParameter("__element"), elementType));
            enabledCompiler.AddVariable(rootParameter, Expression.Convert(updateCompiler.GetParameter("__root"), rootType));

            createdCompiler.AddVariable(elementParameter, Expression.Convert(updateCompiler.GetParameter("__element"), elementType));
            createdCompiler.AddVariable(rootParameter, Expression.Convert(updateCompiler.GetParameter("__root"), rootType));
        }

        public static void CompileAssignContextVariable(LinqCompiler compiler, in AttributeDefinition2 attr, CompilationContext ctx, Type contextVarType) {
            //ContextVariable<T> ctxVar = (ContextVariable<T>)__castElement.bindingNode.GetContextVariable(id);
            //ctxVar.value = expression;

            // todo -- convert to generic call: __castElement.bindingNode.SetContextVariable<string>(id, "hello")
            // need to use var method = type.GetMethod("name").MakeGenericMethod(type);

            Expression access = Expression.MakeMemberAccess(compiler.GetVariable(k_CastElement), s_Element_BindingNode);
            Expression call = Expression.Call(access, s_LinqBindingNode_GetLocalContextVariable, Expression.Constant("varName"));
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
                // if(!element.isEnabled)
                //     return;
                compiler.IfEqual(Expression.MakeMemberAccess(castElement, s_Element_IsEnabled), Expression.Constant(false), () => {
                    // goto retn;
                    compiler.RawExpression(Expression.Goto(compiler.GetReturnLabel()));
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

        private ParameterExpression VisitSlotDefinition(TemplateNode templateNode, CompilationContext ctx, CompiledTemplate template) {
            ProcessedType processedType = templateNode.processedType;

            ctx.elementType = processedType;

            if (template.TryGetSlotData(templateNode.slotName, out SlotDefinition slotData)) {
                // fail
                throw new TemplateParseException("todo -- file", "Duplicate slot detected: " + templateNode.slotName);
            }

            int idx = 0;
            TemplateNode ptr = templateNode.parent;
            slotData = new SlotDefinition(templateNode.slotName);
            slotData.slotType = templateNode.HasAttribute("template") ? SlotType.Template : SlotType.Element;

            while (ptr != null) {
                // more than 4 levels of nesting will explode fosho
                if (ptr.processedType.rawType == typeof(UISlotDefinition)) {
                    slotData[idx++] = template.GetSlotId(ptr.slotName);

                    if (slotData.slotType == SlotType.Element && ptr.HasAttribute("template")) {
                        slotData.slotType = SlotType.Template;
                    }
                }

                ptr = ptr.parent;
            }

            if (slotData.slotType == SlotType.Template) {
                // CompileToStoredSlot(templateNode, ctx, template);
                return null;
            }

            ParameterExpression nodeExpr = ctx.ElementExpr;

            // todo process attributes etc

            template.AddSlotData(slotData);

            int slotTemplateId = CompileSlot(templateNode, ctx, template);

            // 1. how do we check nested case?
            // 2. how do we pass slot usages to lexical scopes?
            // targetElement_1 = application.CreateSlot(templateScope.slotInputList, "slotName", linqBindingNode, parent, root, defaultSlotData, defaultTemplateId);
            ctx.Assign(
                ctx.ElementExpr,
                Expression.Call(ctx.applicationExpr, s_Application_CreateSlot,
                    ctx.templateScope != null
                        ? Expression.Field(ctx.templateScope, s_TemplateScope_SlotInputList) //scope.slotInputList
                        : Expression.Field(ctx.lexicalScope, s_LexicalScope_SlotInputList), // lexicalScope.slotInputList
                    Expression.Constant(templateNode.slotName),
                    Expression.Default(typeof(LinqBindingNode)),
                    ctx.ParentExpr,
                    ctx.rootParam,
                    ctx.templateData,
                    Expression.Constant(slotTemplateId)
                )
            );

            return nodeExpr;
        }

        // todo -- probably don't want a whole new Compilation context here
        private int CompileSlot(TemplateNode templateNode, CompilationContext parentCtx, CompiledTemplate template) {
            // want a fresh set of variables but keep the style / file / binding data / etc contexts
            CompilationContext ctx = new CompilationContext();
            // todo -- use parent context for style & imports n stuff

            ParameterExpression bindingNodeParam = Expression.Parameter(typeof(LinqBindingNode), "bindingNode");
            ParameterExpression parentParam = Expression.Parameter(typeof(UIElement), "parent");
            ParameterExpression appParam = Expression.Parameter(typeof(Application), "application");
            ParameterExpression lexicalScopeParam = Expression.Parameter(typeof(LexicalScope), "lexicalScope");
            ParameterExpression retnVal = Expression.Parameter(typeof(UIElement), "retn");

            ctx.applicationExpr = appParam;
            ctx.lexicalScope = lexicalScopeParam;
            ctx.templateData = Expression.Field(lexicalScopeParam, s_LexicalScope_data);
            ctx.rootParam = Expression.Parameter(typeof(UIElement), "root");

            ctx.variables.Add((ParameterExpression) ctx.rootParam);
            ctx.variables.Add(retnVal);

            ctx.Initialize(retnVal);

            ctx.AddStatement(Expression.Assign(ctx.rootParam, Expression.Field(lexicalScopeParam, s_LexicalScope_root)));

            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool,
                Expression.Constant(templateNode.processedType.rawType == typeof(UIChildrenElement)
                    ? typeof(UIChildrenElement)
                    : typeof(UISlotContent)),
                parentParam,
                Expression.Constant(templateNode.children.size)
            );

            ctx.AddStatement(Expression.Assign(retnVal, createRootExpression));

            VisitChildren(templateNode, ctx, template);

            ctx.AddStatement(retnVal);

            Expression<SlotUsageTemplate> lambda = Expression.Lambda<SlotUsageTemplate>(
                ctx.Finalize(typeof(UIElement)),
                appParam,
                bindingNodeParam,
                parentParam,
                lexicalScopeParam
            );

            throw new NotImplementedException();
            // return application.AddSlotUsageTemplate(lambda);
        }

        // todo -- process attributes! need to save them on SlotUsage?
        private Expression CompileSlotUsage(TemplateNode templateNode, CompilationContext parentCtx, CompiledTemplate template) {
            int slotTemplateId = CompileSlot(templateNode, parentCtx, template);

            // slotUsage = new SlotUsage("slotName", slotTemplateId, new LexicalScope(root, compiledTemplate, slotList));
            return Expression.New(s_SlotUsage_Ctor,
                Expression.Constant(templateNode.slotName),
                Expression.Constant(slotTemplateId),
                Expression.New(s_LexicalScope_Ctor,
                    parentCtx.rootParam,
                    Expression.Default(typeof(CompiledTemplate)), // todo -- need to reference the compiled template to get data for styles & bindings
                    parentCtx.templateScope != null
                        ? Expression.Field(parentCtx.templateScope, s_TemplateScope_SlotInputList)
                        : Expression.Field(parentCtx.lexicalScope, s_LexicalScope_SlotInputList)
                )
            );
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