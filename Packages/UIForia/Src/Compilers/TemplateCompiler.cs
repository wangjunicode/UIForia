using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Parsing.Expression;
using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Compilers {

    public abstract class TemplateContext { }

    public class TemplateContextTreeDefinition { }

    public abstract class LinqBinding {

        public abstract void Execute(UIElement templateRoot, UIElement current, TemplateContext context);

    }

    public class LinqStyleCompiler {

        public LinqBinding Compile(Type rootType, Type elementType, TemplateContextTreeDefinition ctx, in AttributeDefinition2 attributeDefinition) {
            return null;
        }

    }

    public class CompiledTemplate {

        public Expression<Func<Application, UIElement>> buildExpression;

    }

    public class CompilationContext {

        public IReadOnlyList<string> namespaces;
        public Dictionary<string, ParsedTemplate.AliasedUIStyleGroupContainer> sharedStyleMap;
        public Dictionary<string, UIStyleGroupContainer> implicitStyleMap;
        public UIStyleGroupContainer implicitRootStyle;
        public LightList<Expression> statements;
        public LightList<ParameterExpression> variables;
        public ParameterExpression applicationParameter;
        public Type rootType;
        public Type elementType;
        public TemplateContextTreeDefinition contextTree;

    }

    public class TemplateCompiler {

        public Application application;
        private int varId;

        public LightList<LinqBinding> bindings;
        public LinqStyleCompiler styleCompiler;
        public LinqPropertyCompiler propertyCompiler;

        private static readonly MethodInfo s_CreateFromPool = typeof(Application).GetMethod("CreateElementFromPool");
        private static readonly MethodInfo s_BindingNodePool_Get = typeof(LinqBindingNode).GetMethod("Get", BindingFlags.Static);
        private static readonly MethodInfo s_GetStructList_ElementAttr = typeof(StructList<ElementAttribute>).GetMethod("Get", new[] {typeof(int)});
        private static readonly FieldInfo s_StructList_ElementAttr_Size = typeof(StructList<ElementAttribute>).GetField("size");
        private static readonly FieldInfo s_StructList_ElementAttr_Array = typeof(StructList<ElementAttribute>).GetField("array");
        private static readonly PropertyInfo s_LightList_LinqBindingNode_Count = typeof(LightList<ElementAttribute>).GetProperty("Count");
        private static readonly PropertyInfo s_LightList_LinqBindingNode_Array = typeof(LightList<ElementAttribute>).GetProperty("Array");
        private static readonly ConstructorInfo s_ElementAttributeCtor = typeof(ElementAttribute).GetConstructor(new[] {typeof(string), typeof(string)});

        public TemplateCompiler() {
            this.propertyCompiler = new LinqPropertyCompiler();
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

            ProcessedType processedType = TypeProcessor.ResolveTagName(root.typeLookup.typeName, namespaces);

            CompilationContext ctx = new CompilationContext();

            ctx.rootType = processedType.rawType;
            ctx.elementType = processedType.rawType;
            ctx.contextTree = null;

            ctx.applicationParameter = Expression.Parameter(typeof(Application), "application");
            ctx.statements = LightList<Expression>.Get();
            ctx.variables = LightList<ParameterExpression>.Get();
            ctx.namespaces = namespaces;

            Visit(root, ctx);

            // temp
            ctx.statements.Add(Expression.Default(typeof(UIElement)));

            BlockExpression block = Expression.Block(typeof(UIElement), ctx.variables, ctx.statements);

            retn.buildExpression = Expression.Lambda<Func<Application, UIElement>>(block, ctx.applicationParameter);
            return retn;
        }

        public Func<Application, UIElement> Compile() {
            return null;
        }

        private ParameterExpression Visit(TemplateNode templateNode, in CompilationContext ctx) {
            ProcessedType processedType = TypeProcessor.ResolveTagName(templateNode.typeLookup.typeName, ctx.namespaces);
            Type type = processedType.rawType;

            ParameterExpression nodeExpr = Expression.Parameter(type, type.Name + "_" + (varId++));

            ctx.variables.Add(nodeExpr);

            ctx.statements.Add(
                Expression.Assign(nodeExpr, Expression.Convert(Expression.Call(ctx.applicationParameter, s_CreateFromPool, Expression.Constant(type)), type))
            );

            if (templateNode.attributes != null && templateNode.attributes.Count > 0) {
                templateNode.attributes.Sort((a, b) => {
                    int aType = BitUtil.GetHighBits((int) a.attributeType);
                    int bType = BitUtil.GetHighBits((int) b.attributeType);
                    return aType - bType;
                });

                ProcessAttributes(templateNode, ctx);
            }

            if (processedType.isContextProvider) {
                // update context tree
            }

            if (processedType.requiresTemplateExpansion) { }
            else {
                if (templateNode.children != null && templateNode.children.Count > 0) {
                    for (int i = 0; i < templateNode.children.Count; i++) {
                        TemplateNode childNode = templateNode.children[i];

                        ParameterExpression childExpr = Visit(childNode, ctx);
                    }
                }
            }

            return nodeExpr;
        }

        private void ProcessAttributes(TemplateNode templateNode, in CompilationContext ctx) {
            int attrCount = templateNode.attributes.size;
            AttributeDefinition2[] attributeDefinitions = templateNode.attributes.array;

            int cntForType = 0;
            // todo -- might actually want low bits...or make a struct from 2 ushort enums for flags and for type...yeah do that
            AttributeType lastType = (AttributeType) BitUtil.GetHighBits((int) attributeDefinitions[0].attributeType);

            for (int i = 0; i < attrCount; i++) {
                AttributeType currentType = (AttributeType) BitUtil.GetHighBits((int) attributeDefinitions[i].attributeType);

                if (currentType != lastType) {
                    switch (lastType) {
                        case AttributeType.Attribute:
                            EmitAttributes(templateNode, ctx, i - cntForType, i);
                            break;

                        case AttributeType.Property:
                            EmitProperties(templateNode, ctx, i - cntForType, i);
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
        }

        private void EmitProperties(TemplateNode templateNode, in CompilationContext ctx, int startIdx, int endIndex) {
            AttributeDefinition2[] attributeDefinitions = templateNode.attributes.array;

            int cnt = endIndex - startIdx;

            // todo -- if a template holds its own array for bindings and that array is indexed into instead of shared... we need to do a pre-pass to collect sizes.
            // this means compilation should happen in 2 phases, first gather data & build binding array, second index into that array with LightList.ListSpan
            LightList<LinqBinding>.ListSpan span = new LightList<LinqBinding>.ListSpan();
            span.list = bindings;
            span.start = bindings.Count;
            for (int i = startIdx; i < endIndex; i++) {
                ref AttributeDefinition2 attr = ref attributeDefinitions[i];

                if (attr.attributeType != AttributeType.Property) {
                    continue;
                }

                // assume not const for now
                LinqBinding binding = propertyCompiler.Compile(ctx.rootType, ctx.elementType, ctx.contextTree, attr);

                // if binding is const, get its value, store it outside the bindings list, emit code using that value

                bindings.Add(binding);


                // bindings = LightLight<Binding>(cnt);
                // bindings[0] = bindings[idx].Get();
                // bindings[0] = bindings[idx].Get();
                // bindings[0] = bindings[idx].Get();
                // bindings[0] = bindings[idx].Get();
                // element.bindNode = new BindingNode(bindings);

                // at creation time I know the element & root & context which means I can run constant bindings and forget about them

                // element.coldData.bindingNode = BindingNode.Get(root, element, context, bindings);

                // difference is all sharing an array or each getting their own since bindings themselves are shared already where possible
                // if all bindings are shared -> use shared array from init data (or maybe better a span of a single array)

                // to support root level bindings we need 2 binding nodes or a switch
                // 2 nodes makes sense, which one runs first? probably the inner one so the outer can overwrite it
            }

            span.end = bindings.Count;

            if (span.end - span.start == 0) {
                return;
            }

            // todo -- don't introduce new variables, re-use old ones
            ParameterExpression bindingNode = Expression.Parameter(typeof(LinqBindingNode), "bindingNode+" + (++varId));
            ParameterExpression bindingNode_array = Expression.Parameter(typeof(LightList<LinqBindingNode>), "bindingArray_" + (++varId));

            ctx.variables.Add(bindingNode);
            ctx.variables.Add(bindingNode_array);
            
            ctx.statements.Add(Expression.Assign(bindingNode,
                Expression.Call(null, s_BindingNodePool_Get, Expression.Constant(cnt))
            ));

            ctx.statements.Add(Expression.Assign(bindingNode_array, Expression.Property(bindingNode, s_LightList_LinqBindingNode_Array)));

            // if for some reason we needed our own array, point to another array
            
            // bindingNode.listSpan = new ListSpan(bindings, start, end);
            
            for (int i = span.start; i < span.end; i++) {
                
            }
            
        }

        public class LinqBindingNode {

            public static LinqBindingNode Get(int count) {
                return new LinqBindingNode();
            }

        }

        private void EmitConstStyles(TemplateNode templateNode, in CompilationContext ctx, int startIdx, int endIndex) {
            int attrCount = templateNode.attributes.size;
            AttributeDefinition2[] attributeDefinitions = templateNode.attributes.array;

            int cnt = 0;
            for (int i = 0; i < attrCount; i++) {
                ref AttributeDefinition2 attr = ref attributeDefinitions[i];

                if (attr.attributeType != AttributeType.Style) {
                    continue;
                }

                if ((attr.attributeType & AttributeType.Binding) == 0) {
                    cnt++;
                }
                else {
                    LinqBinding binding = styleCompiler.Compile(ctx.rootType, ctx.elementType, ctx.contextTree, attr);

                    bindings.Add(binding);

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
            int attrCount = templateNode.attributes.size;
            AttributeDefinition2[] attributeDefinitions = templateNode.attributes.array;

            int cnt = 0;
            for (int i = 0; i < attrCount; i++) {
                ref AttributeDefinition2 attr = ref attributeDefinitions[i];

                if (attr.attributeType != AttributeType.Attribute) {
                    continue;
                }

                if ((attr.attributeType & AttributeType.Binding) == 0) {
                    cnt++;
                }
            }

            ParameterExpression attributeList = Expression.Parameter(typeof(StructList<ElementAttribute>), "staticAttrList_" + (varId++));

            ctx.variables.Add(attributeList);

            ctx.statements.Add(
                Expression.Assign(attributeList, Expression.Call(null, s_GetStructList_ElementAttr, Expression.Constant(cnt)))
            );

            Expression attrListAccess = Expression.MakeMemberAccess(attributeList, s_StructList_ElementAttr_Array);
            ParameterExpression array = Expression.Parameter(typeof(ElementAttribute[]), "array_" + (varId++));
            ctx.variables.Add(array);
            ctx.statements.Add(Expression.Assign(array, attrListAccess));
            int idx = 0;
            for (int i = 0; i < attrCount; i++) {
                ref AttributeDefinition2 attr = ref attributeDefinitions[i];

                if (attr.attributeType != AttributeType.Attribute) {
                    continue;
                }

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
        }

    }

}