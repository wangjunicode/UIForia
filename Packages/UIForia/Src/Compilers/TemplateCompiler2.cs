using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Linq.Expressions;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Systems;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateCompiler2 {

        private LinqCompiler linqCompiler;
        private LinqPropertyCompiler propertyCompiler;
        private Dictionary<Type, CompiledTemplate> templateMap;
        private LightStack<Type> compilationStack;
        private XMLTemplateParser xmlTemplateParser;

        private TemplateSettings settings;


        private static readonly MethodInfo s_CreateFromPool = typeof(Application).GetMethod(nameof(Application.CreateElementFromPoolWithType));
        private static readonly MethodInfo s_BindingNodePool_Get = typeof(LinqBindingNode).GetMethod("Get", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo s_GetStructList_ElementAttr = typeof(StructList<ElementAttribute>).GetMethod(nameof(StructList<ElementAttribute>.GetMinSize), new[] {typeof(int)});
        private static readonly FieldInfo s_StructList_ElementAttr_Size = typeof(StructList<ElementAttribute>).GetField("size");
        private static readonly FieldInfo s_StructList_ElementAttr_Array = typeof(StructList<ElementAttribute>).GetField("array");
        private static readonly FieldInfo s_Scope_ApplicationField = typeof(TemplateScope2).GetField("application");
        private static readonly FieldInfo s_ScopeBindingNodeField = typeof(TemplateScope2).GetField(nameof(TemplateScope2.bindingNode));
        private static readonly FieldInfo s_Scope_CompiledTemplate = typeof(TemplateScope2).GetField(nameof(TemplateScope2.compiledTemplate));

        private static readonly ConstructorInfo s_ElementAttributeCtor = typeof(ElementAttribute).GetConstructor(new[] {typeof(string), typeof(string)});
        private static readonly ConstructorInfo s_TemplateScope_Ctor = typeof(TemplateScope2).GetConstructor(new[] {typeof(Application), typeof(LinqBindingNode), typeof(StructList<SlotUsage>)});
        private static readonly FieldInfo s_TemplateScope_SlotInputList = typeof(TemplateScope2).GetField(nameof(TemplateScope2.slotInputs));
        private static readonly ConstructorInfo s_LexicalScope_Ctor = typeof(LexicalScope).GetConstructor(new[] {typeof(UIElement), typeof(CompiledTemplate), typeof(StructList<SlotUsage>)});
        private static readonly FieldInfo s_StructList_SlotUsage_Array = typeof(StructList<SlotUsage>).GetField(nameof(StructList<SlotUsage>.array));

        private static readonly FieldInfo s_ElementAttributeList = typeof(UIElement).GetField("attributes", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_Element_ChildrenList = typeof(UIElement).GetField(nameof(UIElement.children), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_LightList_Element_Array = typeof(LightList<UIElement>).GetField(nameof(LightList<UIElement>.array), BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_LightList_Element_Size = typeof(LightList<UIElement>).GetField(nameof(LightList<UIElement>.size), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo s_Application_HydrateTemplate = typeof(Application).GetMethod(nameof(Application.HydrateTemplate), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_TextElement_Text = typeof(UITextElement).GetField(nameof(UITextElement.text), BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo s_Application_CreateSlot = typeof(Application).GetMethod(nameof(Application.CreateSlot), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_Application_TemplateData = typeof(Application).GetField(nameof(Application.templateData), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_TemplateData_ContextProviderFns = typeof(TemplateData).GetField(nameof(TemplateData.contextProviderFns), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_TemplateData_SharedBindingList = typeof(TemplateData).GetField(nameof(TemplateData.sharedBindingFns), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        private static readonly MethodInfo s_BindingNode_AddChild = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.AddChild), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo s_BindingNode_SetContextProvider = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.SetContextProvider), BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_BindingNode_BindingList = typeof(LinqBindingNode).GetField(nameof(LinqBindingNode.bindings), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly ConstructorInfo s_SlotUsage_Ctor = typeof(SlotUsage).GetConstructor(new[] {typeof(string), typeof(int), typeof(LexicalScope)});

        private static readonly FieldInfo s_LexicalScope_root = typeof(LexicalScope).GetField(nameof(LexicalScope.root), BindingFlags.Instance | BindingFlags.Public);
        private static readonly FieldInfo s_LexicalScope_data = typeof(LexicalScope).GetField(nameof(LexicalScope.data), BindingFlags.Instance | BindingFlags.Public);
        private static readonly FieldInfo s_LexicalScope_SlotInputList = typeof(LexicalScope).GetField(nameof(LexicalScope.slotInputList), BindingFlags.Instance | BindingFlags.Public);

        private CompiledTemplateData templateData;

        public TemplateCompiler2(TemplateSettings settings) {
            this.settings = settings;
            this.linqCompiler = new LinqCompiler();
            this.propertyCompiler = new LinqPropertyCompiler(linqCompiler);
            this.templateMap = new Dictionary<Type, CompiledTemplate>();
            this.compilationStack = new LightStack<Type>();
            this.xmlTemplateParser = new XMLTemplateParser();
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

            Expression scopeBindingNode = Expression.Field(scopeParam, s_ScopeBindingNodeField);

            ctx.Initialize(rootParam, scopeBindingNode);

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

            UnityEngine.Debug.Log(retn.templateFn.ToCSharpCode());

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

            ctx.elementType = processedType;
            ctx.CommentNewLineBefore($"{templateNode.originalString}");
            int trueAttrCount = templateNode.GetAttributeCount();

            TemplateNodeType templateNodeType = templateNode.GetTemplateType();

            switch (templateNodeType) {
                case TemplateNodeType.SlotDefinition: {
                    return VisitSlotDefinition(templateNode, ctx, template);
                }

                case TemplateNodeType.SlotContent: {
                    break;
                }

                case TemplateNodeType.HydrateElement: {
                    Expression bindingNode = ctx.BindingNodeExpr;

                    CompiledTemplate expandedTemplate = GetCompiledTemplate(processedType);

                    ctx.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(type), ctx.ParentExpr, Expression.Constant(templateNode.children.size), Expression.Constant(trueAttrCount)));

                 //   MergeTemplateAttributes(templateNode, expandedTemplate.attributes, templateNode.attributes);

//                    OutputConstantAttributes(templateNode);


                    OutputAttributes(ctx, templateNode);

                    OutputBindings(ctx, templateNode);
                    
//                    ProcessBindings(templateNode, ctx, hasTextBindings);

                    // templateScope = new TemplateScope2(application, bindingNode, null);
                    Expression templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr, bindingNode, Expression.Default(typeof(StructList<SlotUsage>)));

                    // scope.application.HydrateTemplate(templateId, targetElement, templateScope)
                    ctx.AddStatement(Expression.Call(ctx.applicationExpr, s_Application_HydrateTemplate, Expression.Constant(expandedTemplate.templateId), nodeExpr, templateScopeCtor));

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
                    ctx.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(type), ctx.ParentExpr, Expression.Constant(templateNode.children.size),  Expression.Constant(trueAttrCount)));
                    
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

        private void OutputBindings(CompilationContext ctx, TemplateNode templateNode) {
            if (templateNode.GetBindingCount() > 0) {
                
                int attrIdx = 0;

                CompiledBinding binding = templateData.AddBinding(templateNode);
                LinqCompiler bindingCompiler = new LinqCompiler();
                
                for (int i = 0; i < templateNode.attributes.size; i++) {
                    
                }

                if (templateNode.processedType.requiresUpdateFn) {
                    bindingCompiler.Statement("__element.Update()");        
                }

                binding.bindingFn = bindingCompiler.BuildLambda();
                
                // scope.PushBindingNode(LinqBinding.Get(targetElement_x));

                // scope.PopBindingNode();

            }  
            
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
                    ctx.BindingNodeExpr,
                    ctx.ParentExpr,
                    ctx.rootParam,
                    ctx.templateData,
                    Expression.Constant(slotTemplateId)
                )
            );

            return nodeExpr;
        }

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

            ctx.Initialize(retnVal, bindingNodeParam);

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