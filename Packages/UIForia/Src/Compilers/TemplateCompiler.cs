using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Linq.Expressions;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.LinqExpressions;
using UIForia.Parsing.Expression;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public class TemplateContextTreeDefinition { }

    internal struct TemplateScope2 {

        public Application application;
        public UIView view;
        public StructList<SlotUsage> slotInputs;
        public LinqBindingNode bindingNode;

        public TemplateScope2(Application application, LinqBindingNode bindingNode, StructList<SlotUsage> slotInputs) {
            this.application = application;
            this.view = null;
            this.slotInputs = slotInputs;
            this.bindingNode = bindingNode;
        }

        public SlotUsage FindMatchingSlot(string slotName) {
            for (int i = 0; i < slotInputs.size; i++) {
                if (slotInputs[i].slotName == slotName) {
                    return slotInputs[i];
                }
            }

            return default; // should never reach this
        }
        
        public bool HasMatchingSlot(string slotName) {
            for (int i = 0; i < slotInputs.size; i++) {
                if (slotInputs[i].slotName == slotName) {
                    return true;
                }
            }

            return false;
        }

    }

    public class LinqStyleCompiler {

        public LinqBinding Compile(Type rootType, Type elementType, TemplateContextTreeDefinition ctx, in AttributeDefinition2 attributeDefinition) {
            return null;
        }

    }

    public struct VariableGroup {

        public ParameterExpression targetElement;
        public ParameterExpression bindingNode;
        public ParameterExpression slotUsage;

    }

    public class TemplateCompiler {

        public Application application;
        private int varId;

        public LinqStyleCompiler styleCompiler;
        public LinqPropertyCompiler propertyCompiler;
        private LightStack<Type> compilationStack;
        private Dictionary<Type, CompiledTemplate> templateMap;
        private XMLTemplateParser xmlTemplateParser;

        private static readonly MethodInfo s_CreateFromPool = typeof(Application).GetMethod("CreateElementFromPool");
        private static readonly MethodInfo s_BindingNodePool_Get = typeof(LinqBindingNode).GetMethod("Get", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo s_GetStructList_ElementAttr = typeof(StructList<ElementAttribute>).GetMethod(nameof(StructList<ElementAttribute>.GetMinSize), new[] {typeof(int)});
        private static readonly FieldInfo s_StructList_ElementAttr_Size = typeof(StructList<ElementAttribute>).GetField("size");
        private static readonly FieldInfo s_StructList_ElementAttr_Array = typeof(StructList<ElementAttribute>).GetField("array");
        private static readonly FieldInfo s_Scope_ApplicationField = typeof(TemplateScope2).GetField("application");
        private static readonly FieldInfo s_ScopeBindingNodeField = typeof(TemplateScope2).GetField(nameof(TemplateScope2.bindingNode));
        private static readonly FieldInfo s_ScopeInputSlotField = typeof(TemplateScope2).GetField(nameof(TemplateScope2.slotInputs));
        private static readonly FieldInfo s_Element_Parent = typeof(UIElement).GetField(nameof(UIElement.parent), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly ConstructorInfo s_ElementAttributeCtor = typeof(ElementAttribute).GetConstructor(new[] {typeof(string), typeof(string)});
        private static readonly ConstructorInfo s_TemplateScope_Ctor = typeof(TemplateScope2).GetConstructor(new[] {typeof(Application), typeof(LinqBindingNode), typeof(LightList<UIElement>)});

        private static readonly MethodInfo s_ArrayPool_Int_GetMinSize = typeof(ArrayPool<int>).GetMethod(nameof(ArrayPool<int>.GetMinSize));

        private static readonly MethodInfo s_LinqBindingList_CreateListSpan = typeof(LightList<LinqBinding>).GetMethod("CreateSpan");
        private static readonly FieldInfo s_ElementAttributeList = typeof(UIElement).GetField("attributes", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_ElementBindingNode = typeof(UIElement).GetField("bindingNode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_BindingNode_Bindings = typeof(LinqBindingNode).GetField(nameof(LinqBindingNode.bindings), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo s_BindingNode_AddChild = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.AddChild), BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_Template_SharedBindings = typeof(CompiledTemplate).GetField(nameof(CompiledTemplate.sharedBindings), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_Element_ChildrenList = typeof(UIElement).GetField(nameof(UIElement.children), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_LightList_Element_Array = typeof(LightList<UIElement>).GetField(nameof(LightList<UIElement>.array), BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_LightList_Element_Size = typeof(LightList<UIElement>).GetField(nameof(LightList<UIElement>.size), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo s_LightList_Element_GetMinSize = typeof(LightList<UIElement>).GetMethod(nameof(LightList<UIElement>.GetMinSize), BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_LightList_Element_Release = typeof(LightList<UIElement>).GetMethod(nameof(LightList<UIElement>.Release), BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_Application_HydrateTemplate = typeof(Application).GetMethod(nameof(Application.HydrateTemplate), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo s_Application_CreateSubTemplate = typeof(Application).GetMethod(nameof(Application.CreateSubTemplate), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_TextElement_Text = typeof(UITextElement).GetField(nameof(UITextElement.text), BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo s_Scope_FindMatchingSlot = typeof(TemplateScope2).GetMethod(nameof(TemplateScope2.FindMatchingSlot), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo s_Scope_HasMatchingSlot = typeof(TemplateScope2).GetMethod(nameof(TemplateScope2.HasMatchingSlot), BindingFlags.Public | BindingFlags.Instance);

        private static readonly ConstructorInfo s_SlotUsage_Ctor = typeof(SlotUsage).GetConstructor(new Type[] {typeof(string), typeof(UIElement), typeof(CompiledTemplate)});
        private static readonly MethodInfo s_StructList_SlotUsage_Get = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.GetMinSize), new[] {typeof(int)});
        private static readonly FieldInfo s_StructList_SlotUsage_Size = typeof(StructList<SlotUsage>).GetField("size");
        private static readonly FieldInfo s_StructList_SlotUsage_Array = typeof(StructList<SlotUsage>).GetField("array");
        private static readonly MethodInfo s_StructList_SlotUsage_AddUnchecked = typeof(StructList<SlotUsage>).GetMethod(nameof(StructList<SlotUsage>.AddUnsafe));

        public TemplateCompiler(Application application) {
            this.application = application;
            this.propertyCompiler = new LinqPropertyCompiler();
            this.templateMap = new Dictionary<Type, CompiledTemplate>();
            this.compilationStack = new LightStack<Type>();
            this.xmlTemplateParser = new XMLTemplateParser(application);
        }

        private void VisitChildren(CompiledTemplate retn, TemplateNode node, CompilationContext ctx) {
            if (node.children == null || node.children.Count <= 0) {
                return;
            }

            Expression parentChildList = Expression.Field(ctx.GetParentTargetElementVariable(), s_Element_ChildrenList);
            Expression parentChildListArray = Expression.Field(parentChildList, s_LightList_Element_Array);

            // childList[idx] = Visit()
            for (int i = 0; i < node.children.Count; i++) {
                Expression visit = Visit(node.children[i], ctx, retn);
                ctx.AddStatement(
                    Expression.Assign(
                        Expression.ArrayAccess(parentChildListArray, Expression.Constant(i)),
                        visit
                    )
                );
            }
        }

        public CompiledTemplate GetCompiledTemplate(Type type) {
            return GetCompiledTemplate(TypeProcessor.GetProcessedType(type));
        }

        public CompiledTemplate GetCompiledTemplate(ProcessedType processedType) {
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

            compilationStack.Push(processedType.rawType);

            TemplateAST ast = xmlTemplateParser.Parse(processedType);

            CompiledTemplate compiledTemplate = Compile(ast);

            TemplateNode.Release(ref ast.root);

            compilationStack.Pop();

            templateMap[processedType.rawType] = compiledTemplate;

            return compiledTemplate;
        }

        private static void LogCode(Expression expression, bool printNamespaces = false) {
            bool old = CSharpWriter.printNamespaces;
            CSharpWriter.printNamespaces = printNamespaces;
            string retn = expression.ToCSharpCode();
            CSharpWriter.printNamespaces = old;
            Debug.Log(retn);
        }

        private CompiledTemplate Compile(TemplateAST ast) {
            CompiledTemplate retn = new CompiledTemplate();
            retn.childCount = ast.root.children.size;

            TemplateNode root = ast.root;
            LightList<string> namespaces = LightList<string>.Get();

            if (ast.usings != null) {
                for (int i = 0; i < ast.usings.size; i++) {
                    namespaces.Add(ast.usings[i].namespaceName);
                }
            }

            ProcessedType processedType = ast.root.processedType;

            CompilationContext ctx = new CompilationContext();

            ctx.rootType = processedType.rawType;
            ctx.elementType = processedType.rawType;
            ctx.contextTree = null;

            Expression scopeBindingNode = Expression.Field(ctx.scopeParam, s_ScopeBindingNodeField);
            ctx.applicationExpr = Expression.Field(ctx.scopeParam, s_Scope_ApplicationField);
            ctx.sharedBindingsExpr = Expression.Field(ctx.templateParam, s_Template_SharedBindings);

            ctx.PushBindingNode(scopeBindingNode);

            ctx.PushBlock();
            // create the root element bindings from <Content> tag
            // todo -- if we have a parent, use it instead of default(UIElement)
            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(processedType.rawType), Expression.Default(typeof(UIElement)), Expression.Constant(ast.root.children.size));
            ctx.AddStatement(Expression.Assign(ctx.rootParam, createRootExpression));
            ProcessAttributes(retn, null, ast.root.attributes, ctx, out bool hasBindings);
            // application.OnElementRegistered(root);
            // root.OnInitialize(); if needed
            BlockExpression createUnscopedBlock = ctx.PopBlock();

            ctx.AddStatement(Expression.IfThen(Expression.Equal(ctx.rootParam, Expression.Constant(null)), createUnscopedBlock));

            ctx.PushScope();
            VisitChildren(retn, root, ctx);
            ctx.PopScope();

            ctx.AddStatement(ctx.rootParam); // this is the return value

            BlockExpression block = Expression.Block(typeof(UIElement), ctx.variables, ctx.statementStacks.PeekAtUnchecked(0));

            retn.buildExpression = Expression.Lambda<Func<UIElement, TemplateScope2, CompiledTemplate, UIElement>>(block, ctx.rootParam, ctx.scopeParam, ctx.templateParam);
            retn.elementType = ast.root.processedType;
            retn.attributes = ast.root.attributes.ToArray();

            LightList<string>.Release(ref namespaces);

            //todo  release context

            application.templateCache.Add(retn);

            LogCode(retn.buildExpression);

            return retn;
        }

        private ParameterExpression CompileToReusableTemplate(TemplateNode templateNode, CompilationContext ctx, CompiledTemplate template) {
            ctx = ctx.Clone();

            // needs to clone template scope
            // inputs from scope need to be converted into functions so they can be re-used?
            // 

            // keep 1 array of slot inputs and compute offsets
            CompiledTemplate retn = new CompiledTemplate();

            retn.fileName = template.fileName;
            retn.childCount = templateNode.children.size;
            retn.elementType = templateNode.processedType;

            ProcessedType processedType = templateNode.processedType;

            ctx.rootType = processedType.rawType;
            ctx.elementType = processedType.rawType;
            ctx.contextTree = null;

            ctx.applicationExpr = Expression.Field(ctx.scopeParam, s_Scope_ApplicationField);
            ctx.sharedBindingsExpr = Expression.Field(ctx.templateParam, s_Template_SharedBindings);

            ctx.PushScope();
            VisitChildren(retn, templateNode, ctx);
            ctx.PopScope();

            ctx.AddStatement(ctx.rootParam); // this is the return value

            Debug.Assert(ctx.statementStacks.Count == 1);
            BlockExpression block = Expression.Block(typeof(UIElement), ctx.variables, ctx.statementStacks.Pop());

            retn.buildExpression = Expression.Lambda<Func<UIElement, TemplateScope2, CompiledTemplate, UIElement>>(block, ctx.rootParam, ctx.scopeParam, ctx.templateParam);

            //todo  release context

            application.templateCache.Add(retn);

            LogCode(retn.buildExpression);
            // return <Element templateFn="new TemplateFn(scope.root, templateFn, contextTree?)"/>
            return null;
        }

        private ParameterExpression VisitSlotDefinition(TemplateNode templateNode, CompilationContext ctx, CompiledTemplate template) {
            ProcessedType processedType = templateNode.processedType;
            Type type = processedType.rawType;

            ctx.elementType = type;

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
                if (ptr.processedType == typeof(UISlotDefinition)) {
                    slotData[idx++] = template.GetSlotId(ptr.slotName);

                    if (slotData.slotType == SlotType.Element && ptr.HasAttribute("template")) {
                        slotData.slotType = SlotType.Template;
                    }
                }

                ptr = ptr.parent;
            }

            ParameterExpression nodeExpr = ctx.GetTargetElementVariable();
            bool hasBindings;

            if (slotData.slotType == SlotType.Template) {
                return CompileToReusableTemplate(templateNode, ctx, template);
            }

            Expression slotUsage = Expression.Field(ctx.scopeParam, s_ScopeInputSlotField);
            Expression slotOverrides = Expression.Field(slotUsage, s_LightList_Element_Array);

            int slotId = template.AddSlotData(slotData);

            Expression slotsNotNull = Expression.NotEqual(slotUsage, Expression.Constant(null));

            Expression hasSlot = Expression.NotEqual(
                Expression.ArrayAccess(slotOverrides, Expression.Constant(slotId)), Expression.Constant(null)
            );

            Expression condition = Expression.Call(ctx.scopeParam, s_Scope_HasMatchingSlot, Expression.Constant(templateNode.slotName));

            // use default template block
            ctx.PushBlock();
            {
                ctx.AddStatement(
                    Expression.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(typeof(UISlotContent)), ctx.GetParentTargetElementVariable(), Expression.Constant(templateNode.children.size)))
                );

                ProcessAttributes(template, null, templateNode.attributes, ctx, out hasBindings);

                if (templateNode.children != null && templateNode.children.Count > 0) {
                    ctx.PushScope();
                    VisitChildren(template, templateNode, ctx);
                    ctx.PopScope();
                }
            }

            // use input template block
            ctx.PushBlock();
            {
                // todo -- hook up binding node 
                ctx.AddStatement(Expression.Assign(nodeExpr, Expression.Call(ctx.scopeParam, s_Scope_FindMatchingSlot, Expression.Constant(slotData.tagName))));
                ctx.AddStatement(Expression.Assign(nodeExpr, Expression.ArrayAccess(slotOverrides, Expression.Constant(slotId))));
                ctx.AddStatement(Expression.Assign(Expression.Field(nodeExpr, s_Element_Parent), ctx.GetParentTargetElementVariable()));
            }

            ctx.AddStatement(Expression.IfThenElse(condition, ctx.PopBlock(), ctx.PopBlock()));

            if (hasBindings) {
                ctx.bindingNodeStack.Pop();
            }

            return nodeExpr;
        }

        private ParameterExpression VisitSlotContent(TemplateNode templateNode, in CompilationContext ctx, CompiledTemplate template) {
            // replace default w/ content 
            // push into scope.inputSlot list at id where id == matching target template slot id
            ParameterExpression nodeExpr = ctx.GetTargetElementVariable();

            if (ctx.expansionStack.Count == 0) {
                throw new TemplateParseException(templateNode.astRoot.fileName, "<Slot> elements can only be used as children of elements defining a matching <DefineSlot> element. Did you mean to use <DefineSlot> instead?");
            }

            CompiledTemplate target = ctx.expansionStack.PeekUnchecked();

            if (!target.TryGetSlotData(templateNode.slotName, out SlotDefinition slotDefinition)) {
                throw new TemplateParseException(templateNode.astRoot.fileName, $"Slot with the name {templateNode.slotName} was not defined on template for type {target.elementType.rawType}. {target.GetValidSlotNameMessage()}");
            }

            if (slotDefinition.slotType == SlotType.Template) {
                CompileToReusableTemplate(templateNode, ctx, template);
                // slotUsageList.Add("slotId", ctx.rootParam, templateId)
            }

            ctx.AddStatement(
                Expression.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(typeof(UISlotContent)), ctx.GetParentTargetElementVariable(), Expression.Constant(templateNode.children.size)))
            );

            if (templateNode.children != null && templateNode.children.Count > 0) {
                ctx.PushScope();
                VisitChildren(template, templateNode, ctx);
                ctx.PopScope();
            }

            Expression slotUsage = ctx.GetParentSlotUsageVariable();
//            Expression slotInputArray = Expression.Field(slotUsage, s_LightList_Element_Array);

            Expression newSlotUsage = Expression.New(s_SlotUsage_Ctor, Expression.Constant(templateNode.slotName), nodeExpr, Expression.Default(typeof(CompiledTemplate)));
            ctx.AddStatement(Expression.Call(slotUsage, s_StructList_SlotUsage_AddUnchecked, newSlotUsage));
//            ctx.AddStatement(Expression.Assign(
//                    Expression.ArrayAccess(slotInputArray, Expression.Constant(ctx.expansionStack.Peek().GetSlotId(templateNode.slotName))),
//                    nodeExpr
//                )
//            );

            return nodeExpr;
        }

        private ParameterExpression Visit(TemplateNode templateNode, in CompilationContext ctx, CompiledTemplate template) {
            ProcessedType processedType = templateNode.processedType;

            if (processedType.rawType == typeof(UISlotDefinition)) {
                return VisitSlotDefinition(templateNode, ctx, template);
            }

            if (processedType.rawType == typeof(UISlotContent)) {
                return VisitSlotContent(templateNode, ctx, template);
            }

            Type type = processedType.rawType;
            ctx.elementType = type;

            ParameterExpression nodeExpr = ctx.GetTargetElementVariable();
            bool hasBindings;
            ctx.AddStatement(
                Expression.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(type), ctx.GetParentTargetElementVariable(), Expression.Constant(templateNode.children.size)))
            );

            if (typeof(UITextElement).IsAssignableFrom(type)) {
                ctx.AddStatement(Expression.Assign(
                    Expression.MakeMemberAccess(
                        Expression.Convert(nodeExpr, typeof(UITextElement)),
                        s_TextElement_Text
                    ),
                    Expression.Constant(templateNode.textContent)
                ));
            }

            if (processedType.isContextProvider) {
                // update context tree
            }

            if (processedType.requiresTemplateExpansion) {
                // create all bindings

                CompiledTemplate compiled = GetCompiledTemplate(processedType);

                ctx.expansionStack.Push(compiled);

                Expression bindingNode = ctx.bindingNodeStack.PeekUnchecked();

                // merge bindings, outer ones win, take the base bindings and replace duplicates with outer ones
                StructList<AttributeDefinition2> attributes = MergeAttributes(compiled.attributes, templateNode.attributes);

                ProcessAttributes(template, compiled, attributes, ctx, out hasBindings);

                Expression templateScopeCtor;
                if (templateNode.children != null && templateNode.children.Count > 0) {
                    ParameterExpression slotUsage = ctx.GetSlotUsageVariable();

                    ctx.AddStatement(Expression.Assign(slotUsage, Expression.Call(null, s_LightList_Element_GetMinSize, Expression.Constant(compiled.slotDefinitions.Count))));

                    ctx.PushScope();

                    LightList<string> slotList = LightList<string>.GetMinSize(templateNode.children.size);

                    for (int i = 0; i < templateNode.children.size; i++) {
                        VisitSlotContent(templateNode.children[i], ctx, template);
                        slotList.Add(templateNode.children[i].slotName);
                    }

                    compiled.ValidateSlotHierarchy(slotList);

                    LightList<string>.Release(ref slotList);

                    ctx.PopScope();

                    templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr, bindingNode, slotUsage);
                    ctx.AddStatement(Expression.Call(ctx.applicationExpr, s_Application_HydrateTemplate, Expression.Constant(compiled.templateId), nodeExpr, templateScopeCtor));
                    ctx.AddStatement(Expression.Call(null, s_LightList_Element_Release, slotUsage));
                }
                else {
                    templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr, bindingNode, Expression.Default(typeof(LightList<UIElement>)));
                    ctx.AddStatement(Expression.Call(ctx.applicationExpr, s_Application_HydrateTemplate, Expression.Constant(compiled.templateId), nodeExpr, templateScopeCtor));
                }

                ctx.expansionStack.Pop();
            }
            else {
                ProcessAttributes(template, null, templateNode.attributes, ctx, out hasBindings);

                if (templateNode.children != null && templateNode.children.Count > 0) {
                    ctx.PushScope();
                    VisitChildren(template, templateNode, ctx);
                    ctx.PopScope();
                }
            }


            if (hasBindings) {
                ctx.bindingNodeStack.Pop();
            }

            return nodeExpr;
        }


        private void ProcessAttributes(CompiledTemplate outerTemplate, CompiledTemplate innerTemplate, StructList<AttributeDefinition2> attributes, in CompilationContext ctx, out bool hasBindings) {
            int attrCount = attributes.size;

            hasBindings = false;
            if (attrCount == 0) {
                return;
            }

            attributes.Sort((a, b) => a.type - b.type);

            AttributeDefinition2[] attributeDefinitions = attributes.array;

            int startIdx = 0;
            bool hasAttrBindings = false;
            bool hasPropertyBindings = false;
            bool hasStyleBindings = false;
            for (int i = 0; i < attrCount; i++) {
                if (attrCount - 1 == i || attributeDefinitions[i].type != attributeDefinitions[i + 1].type) {
                    switch (attributeDefinitions[i].type) {
                        case AttributeType.Attribute:
                            EmitAttributes(attributes, ctx, startIdx, i + 1, out hasAttrBindings);
                            break;

                        case AttributeType.Property:
                            EmitProperties(outerTemplate, innerTemplate, attributes, ctx, startIdx, i + 1, out hasPropertyBindings);
                            break;

                        case AttributeType.Style:
                            EmitStyles(attributes, ctx, startIdx, i + 1, out hasStyleBindings);
                            break;
                    }

                    startIdx = i + 1;
                }
            }

            hasBindings = hasAttrBindings || hasPropertyBindings || hasStyleBindings;
        }

        private void EmitProperties(CompiledTemplate template, CompiledTemplate innerTemplate, StructList<AttributeDefinition2> attributes, in CompilationContext ctx, int startIdx, int endIndex, out bool hasBindings) {
            int cnt = endIndex - startIdx;

            hasBindings = false;
            if (cnt == 0) return;

            AttributeDefinition2[] attributeDefinitions = attributes.array;

            // todo -- if a template holds its own array for bindings and that array is indexed into instead of shared... we need to do a pre-pass to collect sizes.
            // this means compilation should happen in 2 phases, first gather data & build binding array, second index into that array with LightList.ListSpan
            int startBindingIndex = template.sharedBindings.size;

            bool hasNonSharedBindings = false;
            for (int i = startIdx; i < endIndex; i++) {
                ref AttributeDefinition2 attr = ref attributeDefinitions[i];

                // assume not const & shared for now
                LambdaExpression bindingExpression = propertyCompiler.BuildLambda(ctx.rootType, ctx.elementType, ctx.contextTree, attr);

                LinqBinding binding = new LinqPropertyBinding();

                // todo -- if binding is const, get its value, store it outside the bindings list, emit code using that value
                template.sharedBindings.Add(binding);

                if (!hasNonSharedBindings) {
                    hasNonSharedBindings = !binding.CanBeShared;
                }

                // at creation time I know the element & root & context which means I can run constant bindings and forget about them

                // difference is all sharing an array or each getting their own since bindings themselves are shared already where possible
                // if all bindings are shared -> use shared array from init data (or maybe better a span of a single array)

                // to support root level bindings we need 2 binding nodes or a switch
                // 2 nodes makes sense, which one runs first? probably the inner one so the outer can overwrite it
            }

            int endBindingIndex = template.sharedBindings.Count;

            if (endBindingIndex - startBindingIndex == 0) {
                return;
            }

            hasBindings = true;

            if (!hasNonSharedBindings) {
                ParameterExpression bindingNode = ctx.GetBindingNodeVariable();

                // bindingNode = new LinqBindingNode(root, element, ctx.contextStack.Peek());
                ctx.AddStatement(Expression.Assign(bindingNode,
                    Expression.Call(null, s_BindingNodePool_Get,
                        ctx.scopeParam,
                        ctx.rootParam,
                        ctx.GetTargetElementVariable(),
                        Expression.Default(typeof(TemplateContext)),
                        Expression.Default(typeof(LinqBinding)),
                        Expression.Default(typeof(LightList<LinqBinding>.ListSpan)
                        )
                    )
                ));
                // bindingNode.bindings = sharedBindings.CreateSpan(bindingStart, sharedBindings.Count);
                ctx.AddStatement(Expression.Assign(
                    Expression.Field(bindingNode, s_BindingNode_Bindings),
                    Expression.Call(ctx.sharedBindingsExpr, s_LinqBindingList_CreateListSpan, Expression.Constant(startBindingIndex), Expression.Constant(endBindingIndex))
                ));

                ctx.AddStatement(Expression.Assign(
                    Expression.Field(ctx.GetTargetElementVariable(), s_ElementBindingNode), bindingNode)
                );

                Expression lastBindingParent = ctx.bindingNodeStack.Peek();

                ctx.AddStatement(
                    Expression.Call(lastBindingParent, s_BindingNode_AddChild, bindingNode)
                );

                ctx.PushBindingNode(bindingNode);
            }
        }

        private void EmitStyles(StructList<AttributeDefinition2> attributes, in CompilationContext ctx, int startIdx, int endIndex, out bool hasBindings) {
            int attrCount = attributes.size;
            AttributeDefinition2[] attributeDefinitions = attributes.array;

            hasBindings = false;
            int cnt = endIndex - startIdx;
            if (cnt == 0) return;

            for (int i = 0; i < attrCount; i++) {
                ref AttributeDefinition2 attr = ref attributeDefinitions[i];

                if (attr.type != AttributeType.Style) {
                    continue;
                }

                if ((attr.flags & AttributeFlags.Binding) == 0) {
                    cnt++;
                }
                else {
                    LinqBinding binding = styleCompiler.Compile(ctx.rootType, ctx.elementType, ctx.contextTree, attr);


                    // to support root level bindings we need 2 binding nodes or a switch
                    // 2 nodes makes sense, which one runs first? probably the inner one so the outer can overwrite it
                }
            }

            ParameterExpression attributeList = Expression.Parameter(typeof(StructList<ElementAttribute>), "staticAttrList_" + (varId++));

            StyleBindingCompiler cmp = new StyleBindingCompiler();

            cmp.Compile(null, null, attributeDefinitions[0].key, attributeDefinitions[0].value);
        }

        private void EmitAttributes(StructList<AttributeDefinition2> attributes, in CompilationContext ctx, int startIdx, int endIndex, out bool hasBindings) {
            AttributeDefinition2[] attributeDefinitions = attributes.array;

            int cnt = endIndex - startIdx;
            hasBindings = false;

            if (cnt == 0) return;

            ParameterExpression attributeList = ctx.GetVariable(typeof(StructList<ElementAttribute>), "attributeList");
            ParameterExpression array = ctx.GetVariable(typeof(ElementAttribute[]), "attributeArray");

            ctx.AddStatement(
                Expression.Assign(attributeList, Expression.Call(null, s_GetStructList_ElementAttr, Expression.Constant(cnt)))
            );

            Expression attrListAccess = Expression.MakeMemberAccess(attributeList, s_StructList_ElementAttr_Array);

            int idx = 0;

            ctx.AddStatement(
                Expression.Assign(
                    Expression.Field(attributeList, s_StructList_ElementAttr_Size), Expression.Constant(cnt)
                )
            );

            ctx.AddStatement(Expression.Assign(array, attrListAccess));

            // todo -- handle non const attributes

            for (int i = startIdx; i < endIndex; i++) {
                ref AttributeDefinition2 attr = ref attributeDefinitions[i];

                // attributeArray[idx] = new ElementAttribute(attr.key, attr.value);
                NewExpression newExpression = Expression.New(s_ElementAttributeCtor, Expression.Constant(attr.key), Expression.Constant(attr.value));
                Expression arrayIndex = Expression.ArrayAccess(array, Expression.Constant(idx++));

                ctx.AddStatement(
                    Expression.Assign(arrayIndex, newExpression)
                );
            }


            ctx.AddStatement(
                Expression.Assign(
                    Expression.Field(ctx.GetTargetElementVariable(), s_ElementAttributeList),
                    attributeList
                )
            );
        }

        private static StructList<AttributeDefinition2> MergeAttributes(AttributeDefinition2[] inner, StructList<AttributeDefinition2> outer) {
            StructList<AttributeDefinition2> mergedAttributes = StructList<AttributeDefinition2>.GetMinSize(inner.Length + outer.size);

            // match on type & name, might have to track source also in case of binding context

            // add all outer ones
            mergedAttributes.AddRange(outer);

            int outerCount = outer.size;
            AttributeDefinition2[] mergedArray = mergedAttributes.array;

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

            return mergedAttributes;
        }

    }

}