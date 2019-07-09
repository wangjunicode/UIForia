using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Linq.Expressions;
using UIForia.Bindings;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Parsing.Expression;
using UIForia.Systems;
using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Compilers {

    public class TemplateContextTreeDefinition { }

    public class SlotContent { }

    public class TemplateScope2 {

        public Application application;
        public UIView view;
        public int targetSiblingIndex;
        public int parentEnabledFlag;
        public int targetDepth;
        public LightList<SlotContent> slotContents;
        public LightStack<LinqBindingNode> bindingStack;

    }

    public class LinqStyleCompiler {

        public LinqBinding Compile(Type rootType, Type elementType, TemplateContextTreeDefinition ctx, in AttributeDefinition2 attributeDefinition) {
            return null;
        }

    }

    public class CompiledTemplate {

        public Expression<Func<UIElement, TemplateScope2, CompiledTemplate, UIElement>> buildExpression;
        public LightList<LinqBinding> sharedBindings = new LightList<LinqBinding>();

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

        public StructList<VariableGroup> variableGroups = new StructList<VariableGroup>();

        private int currentDepth;
        private int maxDepth;

        public CompilationContext() {
            this.statements = LightList<Expression>.Get();
            this.variables = LightList<ParameterExpression>.Get();
            this.rootParam = Expression.Parameter(typeof(UIElement), "root");
            this.scopeParam = Expression.Parameter(typeof(TemplateScope2), "scope");
            this.templateParam = Expression.Parameter(typeof(CompiledTemplate), "template");
            AddVariableGroup();
        }

        private void AddVariableGroup() {
            ParameterExpression targetElement;
            ParameterExpression childArray;
            ParameterExpression bindingNode;

            if (currentDepth != 0) {
                targetElement = Expression.Parameter(typeof(UIElement), "targetElement_" + currentDepth);
                childArray = Expression.Parameter(typeof(UIElement[]), "childArray_" + currentDepth);
            }
            else {
                targetElement = rootParam;
                childArray = null;
            }

            bindingNode = Expression.Parameter(typeof(LinqBindingNode), "bindingNode_" + currentDepth);


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

    }

    public class TemplateCompiler {

        public Application application;
        private int varId;

        public LinqStyleCompiler styleCompiler;
        public LinqPropertyCompiler propertyCompiler;

        private static readonly MethodInfo s_CreateFromPool = typeof(Application).GetMethod("CreateElementFromPool");
        private static readonly MethodInfo s_BindingNodePool_Get = typeof(LinqBindingNode).GetMethod("Get", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo s_GetStructList_ElementAttr = typeof(StructList<ElementAttribute>).GetMethod("Get", new[] {typeof(int)});
        private static readonly FieldInfo s_StructList_ElementAttr_Size = typeof(StructList<ElementAttribute>).GetField("size");
        private static readonly FieldInfo s_StructList_ElementAttr_Array = typeof(StructList<ElementAttribute>).GetField("array");
        private static readonly FieldInfo s_Scope_ApplicationField = typeof(TemplateScope2).GetField("application");
        private static readonly ConstructorInfo s_ElementAttributeCtor = typeof(ElementAttribute).GetConstructor(new[] {typeof(string), typeof(string)});
        private static readonly MethodInfo s_LinqBindingList_CreateListSpan = typeof(LightList<LinqBinding>).GetMethod("CreateSpan");
        private static readonly FieldInfo s_ElementAttributeList = typeof(UIElement).GetField("attributes", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_ElementBindingNode = typeof(UIElement).GetField("bindingNode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_BindingNode_Bindings = typeof(LinqBindingNode).GetField("bindings", BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_Template_SharedBindings = typeof(CompiledTemplate).GetField(nameof(CompiledTemplate.sharedBindings), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_Element_ChildrenList = typeof(UIElement).GetField(nameof(UIElement.children), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_LightList_Element_Array = typeof(LightList<UIElement>).GetField(nameof(LightList<UIElement>.array), BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo s_LightList_Element_Size = typeof(LightList<UIElement>).GetField(nameof(LightList<UIElement>.size), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo s_LightList_Element_GetMinSize = typeof(LightList<UIElement>).GetMethod(nameof(LightList<UIElement>.GetMinSize), BindingFlags.Public | BindingFlags.Static);

        public TemplateCompiler() {
            this.propertyCompiler = new LinqPropertyCompiler();
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

            // var childArray = targetElement.children.array;
            BinaryExpression assignChildArray = Expression.Assign(childArray, Expression.Field(Expression.Field(ctx.GetParentTargetElementVariable(), s_Element_ChildrenList), s_LightList_Element_Array));
            CSharpWriter.IndentExpressions.Add(assignChildArray);
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

            // targetElement.children.array = childList
            ctx.statements.Add(
                Expression.Assign(Expression.Field(Expression.Field(ctx.GetTargetElementVariable(), s_Element_ChildrenList), s_LightList_Element_Array), childArray)
            );

            // targetElement.children.size = 2;
            var assignChildArraySize = Expression.Assign(Expression.Field(Expression.Field(ctx.GetTargetElementVariable(), s_Element_ChildrenList), s_LightList_Element_Size), Expression.Constant(node.children.Count));
            CSharpWriter.OutdentExpressions.Add(assignChildArraySize);
            ctx.statements.Add(assignChildArraySize);
        }

        public CompiledTemplate Compile(TemplateAST ast) {
            CompiledTemplate retn = new CompiledTemplate();

            TemplateNode root = ast.root;
            LightList<string> namespaces = LightList<string>.Get();

            if (ast.usings != null) {
                for (int i = 0; i < ast.usings.size; i++) {
                    namespaces.Add(ast.usings[i].namespaceName);
                }
            }

            ProcessedType processedType = root.typeLookup.resolvedType != null
                ? TypeProcessor.GetProcessedType(root.typeLookup.resolvedType)
                : TypeProcessor.ResolveTagName(root.typeLookup.typeName, namespaces);

            CompilationContext ctx = new CompilationContext();

            ctx.rootType = processedType.rawType;
            ctx.elementType = processedType.rawType;
            ctx.contextTree = null;


            ctx.applicationExpr = Expression.Field(ctx.scopeParam, s_Scope_ApplicationField);
            ctx.sharedBindingsExpr = Expression.Field(ctx.templateParam, s_Template_SharedBindings);

            ctx.namespaces = namespaces;

            { // create the root element bindings from <Content> tag
                Expression createRootExpression = Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(processedType.rawType));
                ctx.statements.Add(Expression.Assign(ctx.rootParam, Expression.Convert(createRootExpression, typeof(UIElement))));
                ProcessAttributes(retn, ast.root, ctx, true);
                // application.OnElementRegistered(root);
                // root.OnInitialize(); if needed
                BlockExpression createUnscopedBlock = Expression.Block(typeof(void), ctx.statements);
                ctx.statements.Clear();
                ctx.statements.Add(Expression.IfThen(Expression.Equal(ctx.rootParam, Expression.Constant(null)), createUnscopedBlock));
            }

            ctx.PushScope();
            VisitChildren(retn, root, ctx);
            ctx.PopScope();

            ctx.statements.Add(ctx.rootParam); // this is the return value

            BlockExpression block = Expression.Block(typeof(UIElement), ctx.variables, ctx.statements);

            retn.buildExpression = Expression.Lambda<Func<UIElement, TemplateScope2, CompiledTemplate, UIElement>>(block, ctx.rootParam, ctx.scopeParam, ctx.templateParam);
            return retn;
        }

        public Func<Application, UIElement> Compile() {
            return null;
        }

        private ParameterExpression Visit(TemplateNode templateNode, in CompilationContext ctx, CompiledTemplate template) {
            ProcessedType processedType = TypeProcessor.ResolveTagName(templateNode.typeLookup.typeName, ctx.namespaces);
            Type type = processedType.rawType;

            ParameterExpression nodeExpr = ctx.GetTargetElementVariable();

            ctx.statements.Add(
                Expression.Assign(nodeExpr, Expression.Convert(Expression.Call(ctx.applicationExpr, s_CreateFromPool, Expression.Constant(type)), typeof(UIElement)))
            );

            ProcessAttributes(template, templateNode, ctx, false);

            if (processedType.isContextProvider) {
                // update context tree
            }

            if (processedType.requiresTemplateExpansion) {
                // create all bindings
                TemplateAST ast = new XMLTemplateParser(application).Parse(processedType.GetTemplate(application.TemplateRootPath));
                // get bindings / attrs / styles / etch from ast
                // merge them with declared stuff
                // output all that
                // issue call to template.CreateScoped(instance, new Scope() { });
            }
            else { }

            if (templateNode.children != null && templateNode.children.Count > 0) {
                ctx.PushScope();
                VisitChildren(template, templateNode, ctx);
                ctx.PopScope();
            }

            return nodeExpr;
        }

        private void ProcessAttributes(CompiledTemplate template, TemplateNode templateNode, in CompilationContext ctx, bool isUnscoped) {
            int attrCount = templateNode.attributes.size;

            if (attrCount == 0) {
                return;
            }

            templateNode.attributes.Sort((a, b) => a.type - b.type);

            AttributeDefinition2[] attributeDefinitions = templateNode.attributes.array;

            int cntForType = 0;
            // todo -- might actually want low bits...or make a struct from 2 ushort enums for flags and for type...yeah do that
            AttributeType lastType = attributeDefinitions[0].type;

            for (int i = 0; i < attrCount; i++) {
                AttributeType currentType = attributeDefinitions[i].type;

                if (currentType != lastType) {
                    switch (lastType) {
                        case AttributeType.Attribute:
                            EmitAttributes(templateNode, ctx, i - cntForType, i);
                            break;

                        case AttributeType.Property:
                            EmitProperties(template, templateNode, ctx, i - cntForType, i);
                            break;

                        case AttributeType.Style:
                            EmitConstStyles(templateNode, ctx, i - cntForType, i);
                            break;
                    }

                    lastType = currentType;
                    cntForType = 0;
                }

                cntForType++;
            }

            if (cntForType > 0) {
                switch (lastType) {
                    case AttributeType.Attribute:
                        EmitAttributes(templateNode, ctx, attrCount - cntForType, attrCount);
                        break;

                    case AttributeType.Property:
                        EmitProperties(template, templateNode, ctx, attrCount - cntForType, attrCount);
                        break;

                    case AttributeType.Style:
                        EmitConstStyles(templateNode, ctx, attrCount - cntForType, attrCount);
                        break;
                }
            }
        }

        private void EmitProperties(CompiledTemplate template, TemplateNode templateNode, in CompilationContext ctx, int startIdx, int endIndex) {
            AttributeDefinition2[] attributeDefinitions = templateNode.attributes.array;

            int cnt = endIndex - startIdx;

            // todo -- if a template holds its own array for bindings and that array is indexed into instead of shared... we need to do a pre-pass to collect sizes.
            // this means compilation should happen in 2 phases, first gather data & build binding array, second index into that array with LightList.ListSpan
            int startBindingIndex = template.sharedBindings.size;

            bool hasNonSharedBindings = false;
            for (int i = startIdx; i < endIndex; i++) {
                ref AttributeDefinition2 attr = ref attributeDefinitions[i];

                // assume not const & shared for now
                LambdaExpression bindingExpression = propertyCompiler.BuildLambda(ctx.rootType, ctx.elementType, ctx.contextTree, attr);

                LinqBinding binding = new LinqPropertyBinding();

                // use reflection to create delegate type (meh)
                // use casting every frame every binding
                // use virtual fn overrides on a generic type


                // BindingNode<T, U> : BindingNode {} bindingNode.AddPropertyBinding(fn, shared);

                // todo -- if binding is const, get its value, store it outside the bindings list, emit code using that value
                template.sharedBindings.Add(binding);

                if (!hasNonSharedBindings) {
                    hasNonSharedBindings = !binding.CanBeShared;
                }

                // at creation time I know the element & root & context which means I can run constant bindings and forget about them

                // element.coldData.bindingNode = BindingNode.Get(root, element, context, bindings);

                // difference is all sharing an array or each getting their own since bindings themselves are shared already where possible
                // if all bindings are shared -> use shared array from init data (or maybe better a span of a single array)

                // to support root level bindings we need 2 binding nodes or a switch
                // 2 nodes makes sense, which one runs first? probably the inner one so the outer can overwrite it
            }

            int endBindingIndex = template.sharedBindings.Count;

            if (endBindingIndex - startBindingIndex == 0) {
                return;
            }

            if (!hasNonSharedBindings) {
                ParameterExpression bindingNode = ctx.GetVariable(typeof(LinqBindingNode), "bindingNode");

                // bindingNode = new LinqBindingNode(root, element, ctx.contextStack.Peek());
                ctx.statements.Add(Expression.Assign(bindingNode,
                    Expression.Call(null, s_BindingNodePool_Get, Expression.Constant(cnt))
                ));
                // bindingNode.bindings = sharedBindings.CreateSpan(bindingStart, sharedBindings.Count);
                ctx.statements.Add(Expression.Assign(
                    Expression.Field(bindingNode, s_BindingNode_Bindings),
                    Expression.Call(ctx.sharedBindingsExpr, s_LinqBindingList_CreateListSpan, Expression.Constant(0), Expression.Constant(1))
                ));

                ctx.statements.Add(Expression.Assign(
                    Expression.Field(ctx.GetTargetElementVariable(), s_ElementBindingNode), bindingNode)
                );
            }
        }

        private void EmitConstStyles(TemplateNode templateNode, in CompilationContext ctx, int startIdx, int endIndex) {
            int attrCount = templateNode.attributes.size;
            AttributeDefinition2[] attributeDefinitions = templateNode.attributes.array;

            int cnt = 0;
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

            // 2 functions, 1 to init, one to create
            // when pre-creating code to be run, need the template to still 'compile' in that it maps functions into it's binding arrays
            // those can then be referenced in the create function to build the data we need

            // void Init() {
            //    bindings[0] = new StyleBinding.Overflow((type)functions[0]);

            // template.bindings[idx] = compileStyleBinding(new StyleBinding() {fn, });

            // var instanceStyle = element.style.instanceStyle;
            // instanceStyle.normal.style[0] = new StyleProperty(StylePropertyId.Something, someValue, someOtherValue); 
            // instanceStyle.normal.style[0] = new StyleProperty(StylePropertyId.Something, someValue, someOtherValue); 
            // instanceStyle.normal.style[0] = new StyleProperty(StylePropertyId.Something, someValue, someOtherValue); 
            // instanceStyle.normal.style[0] = new StyleProperty(StylePropertyId.Something, someValue, someOtherValue); 
            // instanceStyle.normal.style[0] = new StyleProperty(StylePropertyId.Something, someValue, someOtherValue); 
            // instanceStyle.normal.style[0] = new StyleProperty(StylePropertyId.Something, someValue, someOtherValue); 
            // instanceStyle.normal.style.PropertyCount = count;
            // instanceStyle.binding[].
        }

        private void EmitAttributes(TemplateNode templateNode, in CompilationContext ctx, int startIdx, int endIndex) {
            AttributeDefinition2[] attributeDefinitions = templateNode.attributes.array;

            int cnt = endIndex - startIdx;

            ParameterExpression attributeList = ctx.GetVariable(typeof(StructList<ElementAttribute>), "attributeList");
            ParameterExpression array = ctx.GetVariable(typeof(ElementAttribute[]), "attributeArray");

            ctx.statements.Add(
                Expression.Assign(attributeList, Expression.Call(null, s_GetStructList_ElementAttr, Expression.Constant(cnt)))
            );

            Expression attrListAccess = Expression.MakeMemberAccess(attributeList, s_StructList_ElementAttr_Array);

            ctx.statements.Add(Expression.Assign(array, attrListAccess));
            int idx = 0;

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
                    Expression.Field(attributeList, s_StructList_ElementAttr_Size), Expression.Constant(cnt)
                )
            );

            ctx.statements.Add(
                Expression.Assign(
                    Expression.Field(ctx.GetTargetElementVariable(), s_ElementAttributeList),
                    attributeList
                )
            );
        }

    }

}