using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Linq.Expressions;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing.Expression;
using UIForia.Systems;
using UIForia.Templates;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public class TemplateContextTreeDefinition { }

    public class SlotContent { }

    internal struct TemplateScope2 {

        public Application application;
        public UIView view;
        public int targetSiblingIndex;
        public int parentEnabledFlag;
        public int targetDepth;
        public SlotUsage slotUsage;
        public LinqBindingNode bindingNode;

        public TemplateScope2(Application application, LinqBindingNode bindingNode, in SlotUsage slotUsage) {
            this.application = application;
            this.view = null;
            this.targetSiblingIndex = 0;
            this.parentEnabledFlag = 0;
            this.targetDepth = 0;
            this.slotUsage = slotUsage;
            this.bindingNode = bindingNode;
        }

    }

    public class LinqStyleCompiler {

        public LinqBinding Compile(Type rootType, Type elementType, TemplateContextTreeDefinition ctx, in AttributeDefinition2 attributeDefinition) {
            return null;
        }

    }

    internal struct SlotOverrideDefinition {

        public int slotId;
        public int templateId;

    }
    
    internal struct SlotUsage {

        internal UIElement parentContext;
        internal int[] slotFnIds; // idx into array = slot id in template, value = template fn id

        public SlotUsage(UIElement parentContext, int[] slotFnIds) {
            this.parentContext = parentContext;
            this.slotFnIds = slotFnIds;
        }

    }

    public class CompiledTemplate {

        internal Expression<Func<UIElement, TemplateScope2, CompiledTemplate, UIElement>> buildExpression;
        internal Func<UIElement, TemplateScope2, CompiledTemplate, UIElement> createFn;
        internal LightList<LinqBinding> sharedBindings = new LightList<LinqBinding>();
        internal ProcessedType elementType;
        internal AttributeDefinition2[] attributes;
        internal StructList<SlotOverrideDefinition> slotInputs;

        public int templateId;

        internal Func<UIElement, TemplateScope2, CompiledTemplate, UIElement> Compile() {
            return buildExpression.Compile();
        }

        internal void Create(UIElement parent, TemplateScope2 scope) {
            if (createFn == null) {
                createFn = buildExpression.Compile();
            }

            createFn(parent, scope, null);
        }

    }

    public struct VariableGroup {

        public ParameterExpression targetElement;
        public ParameterExpression attributeArray;
        public ParameterExpression childArray;
        public ParameterExpression bindingNode;

    }

    public class CompilationContext {

        public IReadOnlyList<string> namespaces;
        public Dictionary<string, ParsedTemplate.AliasedUIStyleGroupContainer> sharedStyleMap;
        public Dictionary<string, UIStyleGroupContainer> implicitStyleMap;
        public UIStyleGroupContainer implicitRootStyle;
        public LightList<Expression> statements;
        public LightList<ParameterExpression> variables;
        public Expression applicationExpr;
        public Type rootType;
        public Type elementType;
        public TemplateContextTreeDefinition contextTree;
        public ParameterExpression rootParam;
        public ParameterExpression templateParam;
        public ParameterExpression scopeParam;
        public Expression sharedBindingsExpr;
        public LightStack<Expression> bindingNodeStack;

        public StructList<VariableGroup> variableGroups = new StructList<VariableGroup>();

        private int currentDepth;
        private int maxDepth;

        public CompilationContext() {
            this.statements = LightList<Expression>.Get();
            this.variables = LightList<ParameterExpression>.Get();
            this.rootParam = Expression.Parameter(typeof(UIElement), "root");
            this.scopeParam = Expression.Parameter(typeof(TemplateScope2), "scope");
            this.templateParam = Expression.Parameter(typeof(CompiledTemplate), "template");
            this.bindingNodeStack = new LightStack<Expression>();
            AddVariableGroup();
        }

        private void AddVariableGroup() {
            ParameterExpression targetElement;
            ParameterExpression childArray;

            if (currentDepth != 0) {
                targetElement = Expression.Parameter(typeof(UIElement), "targetElement_" + currentDepth);
                childArray = Expression.Parameter(typeof(UIElement[]), "childArray_" + currentDepth);
            }
            else {
                targetElement = rootParam;
                childArray = null;
            }

            ParameterExpression bindingNode = Expression.Parameter(typeof(LinqBindingNode), "bindingNode_" + currentDepth);

            if (currentDepth != 0) {
                variables.Add(targetElement);
                variables.Add(childArray);
            }

            variables.Add(bindingNode);

            variableGroups.Add(new VariableGroup() {
                targetElement = targetElement,
                childArray = childArray,
                bindingNode = bindingNode,
            });
        }

        public void PushScope() {
            currentDepth++;

            if (currentDepth > maxDepth) {
                maxDepth = currentDepth;
                AddVariableGroup();
            }
        }

        public ParameterExpression GetParentTargetElementVariable() {
            return variableGroups[currentDepth - 1].targetElement;
        }

        public ParameterExpression GetTargetElementVariable() {
            return variableGroups[currentDepth].targetElement;
        }

        public ParameterExpression GetBindingNodeVariable() {
            return variableGroups[currentDepth].bindingNode;
        }

        public ParameterExpression GetChildArrayVariable() {
            return variableGroups[currentDepth].childArray;
        }

        public void PopScope() {
            currentDepth--;
        }

        public ParameterExpression GetVariable(Type type, string name) {
            for (int i = 0; i < variables.Count; i++) {
                if (variables[i].Type == type && variables[i].Name == name) {
                    return variables[i];
                }
            }

            ParameterExpression variable = Expression.Parameter(type, name);
            variables.Add(variable);
            return variable;
        }

        public void PushBindingNode(Expression bindingNode) {
            bindingNodeStack.Push(bindingNode);
        }

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

        private static readonly ConstructorInfo s_ElementAttributeCtor = typeof(ElementAttribute).GetConstructor(new[] {typeof(string), typeof(string)});
        private static readonly ConstructorInfo s_TemplateScope_Ctor = typeof(TemplateScope2).GetConstructor(new[] {typeof(Application), typeof(LinqBindingNode), typeof(LightList<UIElement>)});
        private static readonly ConstructorInfo s_SlotInputDef_Ctor = typeof(SlotOverrideDefinition).GetConstructor(new[] {typeof(int), typeof(int)});

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
        private static readonly MethodInfo s_Application_HydrateTemplate = typeof(Application).GetMethod(nameof(Application.HydrateTemplate), BindingFlags.NonPublic | BindingFlags.Instance);


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

            ParameterExpression childArray = ctx.GetChildArrayVariable();

            // targetElement.children = LightList<UIElement>.GetMinSize(childCount);
            ctx.statements.Add(
                Expression.Assign(Expression.Field(ctx.GetParentTargetElementVariable(), s_Element_ChildrenList),
                    Expression.Call(null, s_LightList_Element_GetMinSize, Expression.Constant(node.children.Count)
                    )
                )
            );

            BinaryExpression assignChildArraySize = Expression.Assign(Expression.Field(Expression.Field(ctx.GetParentTargetElementVariable(), s_Element_ChildrenList), s_LightList_Element_Size), Expression.Constant(node.children.Count));
            ctx.statements.Add(assignChildArraySize);

            // var childArray = targetElement.children.array;
            BinaryExpression assignChildArray = Expression.Assign(childArray, Expression.Field(Expression.Field(ctx.GetParentTargetElementVariable(), s_Element_ChildrenList), s_LightList_Element_Array));
            ctx.statements.Add(assignChildArray);

            // childList[idx] = Visit()
            for (int i = 0; i < node.children.Count; i++) {
                ctx.statements.Add(
                    Expression.Assign(
                        Expression.ArrayAccess(childArray, Expression.Constant(i)),
                        Visit(node.children[i], ctx, retn)
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

        private static void LogCode(Expression expression, bool printNamespaces = true) {
            bool old = CSharpWriter.printNamespaces;
            CSharpWriter.printNamespaces = printNamespaces;
            string retn = expression.ToCSharpCode();
            CSharpWriter.printNamespaces = old;
            Debug.Log(retn);
        }

        private CompiledTemplate Compile(TemplateAST ast) {
            CompiledTemplate retn = new CompiledTemplate();

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

            ctx.namespaces = namespaces;

            ctx.PushBindingNode(scopeBindingNode);

            // create the root element bindings from <Content> tag
            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(processedType.rawType));
            ctx.statements.Add(Expression.Assign(ctx.rootParam, createRootExpression));
            ProcessAttributes(retn, null, ast.root.attributes, ctx, out bool hasBindings);
            // application.OnElementRegistered(root);
            // root.OnInitialize(); if needed
            BlockExpression createUnscopedBlock = Expression.Block(typeof(void), ctx.statements);
            ctx.statements.Clear();
            ctx.statements.Add(Expression.IfThen(Expression.Equal(ctx.rootParam, Expression.Constant(null)), createUnscopedBlock));

            ctx.PushScope();
            VisitChildren(retn, root, ctx);
            ctx.PopScope();

            ctx.statements.Add(ctx.rootParam); // this is the return value

            BlockExpression block = Expression.Block(typeof(UIElement), ctx.variables, ctx.statements);

            retn.buildExpression = Expression.Lambda<Func<UIElement, TemplateScope2, CompiledTemplate, UIElement>>(block, ctx.rootParam, ctx.scopeParam, ctx.templateParam);
            retn.elementType = ast.root.processedType;
            retn.attributes = ast.root.attributes.ToArray();

            LightList<string>.Release(ref namespaces);
            ctx.namespaces = null;

            //todo  release context


            application.templateCache.Add(retn);

            return retn;
        }

        private CompiledTemplate CompileSubTemplate(ProcessedType rootType, TemplateNode elementNode, CompilationContext templateContext) {
            CompiledTemplate retn = new CompiledTemplate();
            CompilationContext ctx = new CompilationContext();

            ctx.rootType = rootType.rawType;
            ctx.elementType = elementNode.processedType.rawType;
            ctx.contextTree = null;

            Expression scopeBindingNode = Expression.Field(ctx.scopeParam, s_ScopeBindingNodeField);
            ctx.applicationExpr = Expression.Field(ctx.scopeParam, s_Scope_ApplicationField);
            ctx.sharedBindingsExpr = Expression.Field(ctx.templateParam, s_Template_SharedBindings);

            ctx.namespaces = templateContext.namespaces;

            Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(elementNode.processedType.rawType));
            ctx.statements.Add(Expression.Assign(ctx.rootParam, createRootExpression));
//            ProcessAttributes(retn, null, elementNode.attributes, ctx, out bool hasBindings);

//            ctx.PushBindingNode(scopeBindingNode);

//            ctx.statements.Clear();
//
//            ctx.PushScope();
//            VisitChildren(retn, elementNode, ctx);
//            ctx.PopScope();

            ctx.statements.Add(ctx.rootParam); // this is the return value

            BlockExpression block = Expression.Block(typeof(UIElement), ctx.variables, ctx.statements);

            retn.buildExpression = Expression.Lambda<Func<UIElement, TemplateScope2, CompiledTemplate, UIElement>>(block, ctx.rootParam, ctx.scopeParam, ctx.templateParam);
            retn.elementType = elementNode.processedType;
            retn.attributes = null;

            //todo  release context
            LogCode(retn.buildExpression);
            application.templateCache.Add(retn);

            return retn;
        }

        private ParameterExpression Visit(TemplateNode templateNode, in CompilationContext ctx, CompiledTemplate template) {
            ProcessedType processedType = templateNode.processedType;
            Type type = processedType.rawType;

            ctx.elementType = type;

            ParameterExpression nodeExpr = ctx.GetTargetElementVariable();

            ctx.statements.Add(
                Expression.Assign(nodeExpr, Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(type)))
            );

            if (processedType.rawType == typeof(UISlotElement)) {
                // push slot into scope?    
            }

            if (processedType.rawType == typeof(UISlotContentTemplate)) {
                // replace default w/ content 
            }

            if (processedType.isContextProvider) {
                // update context tree
            }

            bool hasBindings;
            if (processedType.requiresTemplateExpansion) {
                // create all bindings
                CompiledTemplate compiled = GetCompiledTemplate(processedType.rawType);

                Expression bindingNode = ctx.bindingNodeStack.PeekUnchecked();

                // merge bindings, outer ones win, take the base bindings and replace duplicates with outer ones
                StructList<AttributeDefinition2> attributes = MergeAttributes(compiled.attributes, templateNode.attributes);

                ProcessAttributes(template, compiled, attributes, ctx, out hasBindings);

                Expression slotInput = null;

                if (templateNode.children != null && templateNode.children.Count > 0) {
                    // need root element type from current template
                    CompiledTemplate subTemplate = CompileSubTemplate(processedType, templateNode, ctx);
                    Expression slotList = CreateSlotList(compiled.slotInputs.Count);
                    for (int i = 0; i < subTemplate.slotInputs.Count; i++) {
                        ctx.statements.Add(
                            Expression.Assign(
                                Expression.ArrayAccess(slotList, Expression.Constant(subTemplate.slotInputs[i].slotId)),
                                Expression.Constant(subTemplate.slotInputs[i].templateId)
                            )
                        );
                    }

                    slotInput = Expression.New(s_SlotInputDef_Ctor, ctx.rootParam, slotList, Expression.Constant(compiled.slotInputs.Count));
                }
                else {
                    //slotInput = Expression.Default(typeof(SlotInputDefinition));
                }

                Expression templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr, bindingNode, slotInput);
                ctx.statements.Add(Expression.Call(ctx.applicationExpr, s_Application_HydrateTemplate, Expression.Constant(compiled.templateId), nodeExpr, templateScopeCtor));
                
                // todo -- release array if used
                
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

        private Expression CreateSlotList(int count) {
            return Expression.Call(null, s_ArrayPool_Int_GetMinSize, Expression.Constant(count));
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
                ctx.statements.Add(Expression.Assign(bindingNode,
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
                ctx.statements.Add(Expression.Assign(
                    Expression.Field(bindingNode, s_BindingNode_Bindings),
                    Expression.Call(ctx.sharedBindingsExpr, s_LinqBindingList_CreateListSpan, Expression.Constant(startBindingIndex), Expression.Constant(endBindingIndex))
                ));

                ctx.statements.Add(Expression.Assign(
                    Expression.Field(ctx.GetTargetElementVariable(), s_ElementBindingNode), bindingNode)
                );

                Expression lastBindingParent = ctx.bindingNodeStack.Peek();

                ctx.statements.Add(
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

            ctx.statements.Add(
                Expression.Assign(attributeList, Expression.Call(null, s_GetStructList_ElementAttr, Expression.Constant(cnt)))
            );

            Expression attrListAccess = Expression.MakeMemberAccess(attributeList, s_StructList_ElementAttr_Array);

            int idx = 0;

            ctx.statements.Add(
                Expression.Assign(
                    Expression.Field(attributeList, s_StructList_ElementAttr_Size), Expression.Constant(cnt)
                )
            );

            ctx.statements.Add(Expression.Assign(array, attrListAccess));

            // todo -- handle non const attributes

            for (int i = startIdx; i < endIndex; i++) {
                ref AttributeDefinition2 attr = ref attributeDefinitions[i];

                // attributeArray[idx] = new ElementAttribute(attr.key, attr.value);
                NewExpression newExpression = Expression.New(s_ElementAttributeCtor, Expression.Constant(attr.key), Expression.Constant(attr.value));
                Expression arrayIndex = Expression.ArrayAccess(array, Expression.Constant(idx++));

                ctx.statements.Add(
                    Expression.Assign(arrayIndex, newExpression)
                );
            }


            ctx.statements.Add(
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