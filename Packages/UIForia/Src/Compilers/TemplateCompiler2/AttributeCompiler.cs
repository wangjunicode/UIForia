using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Systems;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public class AttributeCompiler {

        private readonly TemplateLinqCompiler constCompiler;
        private readonly TemplateLinqCompiler enableCompiler;
        private readonly TemplateLinqCompiler updateCompiler;
        private readonly TemplateLinqCompiler lateCompiler;
        private readonly TemplateLinqCompiler inputHandlerCompiler;

        private static readonly string k_InputEventParameterName = "__evt";
        private static readonly Expression s_StringBuilderExpr = Expression.Field(null, typeof(StringUtil), nameof(StringUtil.s_CharStringBuilder));
        private static readonly Expression s_StringBuilderClear = ExpressionFactory.CallInstance(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod("Clear"));
        private static readonly Expression s_StringBuilderToString = ExpressionFactory.CallInstance(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod("ToString", Type.EmptyTypes));
        private static readonly RepeatKeyFnTypeWrapper s_RepeatKeyFnTypeWrapper = new RepeatKeyFnTypeWrapper();
        private static readonly Dictionary<Type, MethodInfo> s_BindingVariableGetterCache = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> s_BindingVariableSetterCache = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, Type> s_ContextVarTypeCache = new Dictionary<Type, Type>();

        private readonly StructList<PropertyChangeHandlerDesc> changeHandlers;
        private StructList<BindingVariableDesc> localVariables;

        private readonly AttributeCompilerContext compilerContext;

        public AttributeCompiler(AttributeCompilerContext compilerContext) {
            this.compilerContext = compilerContext;
            // this.compilerContext = new TemplateLinqCompilerContext();
            this.updateCompiler = new TemplateLinqCompiler(compilerContext);
            this.lateCompiler = new TemplateLinqCompiler(compilerContext);
            this.constCompiler = new TemplateLinqCompiler(compilerContext);
            this.enableCompiler = new TemplateLinqCompiler(compilerContext);
            this.inputHandlerCompiler = new TemplateLinqCompiler(compilerContext);
            this.changeHandlers = new StructList<PropertyChangeHandlerDesc>(16);
        }

        const AttributeType beforeUpdateTypes = AttributeType.Alias | AttributeType.Conditional | AttributeType.Context | AttributeType.Property;

        private static bool IsAttrBeforeUpdate(in AttrInfo attrInfo) {
            return (attrInfo.type & beforeUpdateTypes) != 0;
        }

        private TemplateLinqCompiler GetCompiler(bool isConst, in AttrInfo attrInfo) {
            if (isConst || (attrInfo.flags & AttributeFlags.Const) != 0) {
                return constCompiler;
            }

            // todo if const + or once or isConst + sync -> probably throw an error?
            return updateCompiler;
        }

        private void SetupCompilers(in AttrInfo attr) {
            compilerContext.Setup(attr);
            updateCompiler.Setup();
            lateCompiler.Setup();
            constCompiler.Setup();
            enableCompiler.Setup();
        }

        public void CompileAttributes(SizedArray<AttrInfo> attributes, AttributeCompilerContext compilerContext) {
            compilerContext.elementType.EnsureReflectionData();

            CompileInputHandlers(compilerContext.elementType);
            
            // [InvokeWhenDisabled]
            // Update() { } 
            // const + once bindings and unmarked bindings that are constant

            // todo -- only if needed
            updateCompiler.Init();
            lateCompiler.Init();
            constCompiler.Init();
            enableCompiler.Init();

            for (int i = 0; i < attributes.size; i++) {

                ref AttrInfo attr = ref attributes.array[i];

                if (!IsAttrBeforeUpdate(attr)) {
                    continue;
                }

                SetupCompilers(attr);

                ASTNode ast = ExpressionParser.Parse(attr.value);

                TemplateLinqCompiler compiler = GetCompiler(IsConstantExpression(ast), attr);

                switch (attr.type) {

                    case AttributeType.Conditional: {
                        compiler.SetImplicitContext(compiler.GetRoot(), ParameterFlags.NeverNull);
                        compiler.RawExpression(ExpressionFactory.CallInstance(
                                compiler.GetElement(),
                                MemberData.Element_SetEnabledInternal,
                                compiler.Value(ast)
                            )
                        );
                        break;
                    }

                    case AttributeType.Property: {
                        CompilePropertyBinding(compiler, ast, attr);
                        break;
                    }

                    case AttributeType.Alias: {
                        throw new NotImplementedException();
                        break;
                    }

                    case AttributeType.Context: {
                        CompileContextVariable(compiler, ast, attr);
                        break;
                    }

                }

            }

            // when resolving aliases, if alias was a context variable, determine if it is local or not

            CompileTextBinding(compilerContext.templateNode as TextNode);

            if (compilerContext.elementType.requiresUpdateFn) {
                updateCompiler.RawExpression(ExpressionFactory.CallInstance(updateCompiler.GetBindingNode(), MemberData.BindingNode_InvokeUpdate));
            }

            for (int i = 0; i < attributes.size; i++) {

                ref AttrInfo attr = ref attributes.array[i];

                if (IsAttrBeforeUpdate(attr)) {
                    continue;
                }

                SetupCompilers(attr);

                switch (attr.type) {
                    case AttributeType.Attribute:
                        if ((attr.flags & AttributeFlags.Const) == 0) {
                            ASTNode ast = ExpressionParser.Parse(attr.StrippedValue);
                            TemplateLinqCompiler compiler = GetCompiler(IsConstantExpression(ast), attr);
                            CompileAttributeBinding(compiler, attr);
                        }

                        break;

                    case AttributeType.Style:
                        break;

                    case AttributeType.Mouse:
                        // bindingResult.hasInputEvents = true;
                        CompileMouseInputBinding(attr);
                        break;

                    case AttributeType.Key:
                        // bindingResult.hasInputEvents = true;
                        CompileKeyboardInputBinding(attr);
                        break;

                    case AttributeType.Drag:
                        // bindingResult.hasInputEvents = true;
                        CompileDragBinding(attr);
                        break;
                }

            }

            // todo -- do this once per processed type, use result
            CompileInputHandlers();

            BuildLambda(updateCompiler, ref compilerContext.bindingResult.updateLambda);
            BuildLambda(lateCompiler, ref compilerContext.bindingResult.lateLambda);
            BuildLambda(enableCompiler, ref compilerContext.bindingResult.enableLambda);
            BuildLambda(constCompiler, ref compilerContext.bindingResult.constLambda);

        }

        private void CompileInputHandlers() {
            // StructList<InputHandler> handlers = InputCompiler.CompileInputAnnotations(processedType.rawType);
            //
            // const InputEventType k_KeyboardType = InputEventType.KeyDown | InputEventType.KeyUp | InputEventType.KeyHeldDown;
            // const InputEventType k_DragType = InputEventType.DragCancel | InputEventType.DragDrop | InputEventType.DragEnter | InputEventType.DragEnter | InputEventType.DragExit | InputEventType.DragHover | InputEventType.DragMove;
            //
            // bool hasHandlers = false;
            //
            // if (handlers != null) {
            //     hasHandlers = true;
            //
            //     for (int i = 0; i < handlers.size; i++) {
            //         ref InputHandler handler = ref handlers.array[i];
            //
            //         if (handler.descriptor.handlerType == InputEventType.DragCreate) {
            //             CompileDragCreateFromAttribute(handler);
            //         }
            //         else if ((handler.descriptor.handlerType & k_DragType) != 0) {
            //             CompileDragHandlerFromAttribute(handler);
            //         }
            //         else if ((handler.descriptor.handlerType & k_KeyboardType) != 0) {
            //             CompileKeyboardHandlerFromAttribute(handler);
            //         }
            //         else {
            //             CompileMouseHandlerFromAttribute(handler);
            //         }
            //     }
            // }
            //
            // if (!hasHandlers) {
            //     return;
            // }
            //
            // // Application.InputSystem.RegisterKeyboardHandler(element);
            // ParameterExpression elementVar = constCompiler.GetElement();
            // MemberExpression app = Expression.Property(elementVar, typeof(UIElement).GetProperty(nameof(UIElement.application)));
            // MemberExpression inputSystem = Expression.Property(app, typeof(Application).GetProperty(nameof(Application.InputSystem)));
            // MethodInfo method = typeof(InputSystem).GetMethod(nameof(InputSystem.RegisterKeyboardHandler));
            // constCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(inputSystem, method, elementVar));
        }

        private void CompileDragBinding(in AttrInfo attr) {
            InputHandlerDescriptor descriptor = InputCompiler.ParseDragDescriptor(attr.key);
            if (descriptor.handlerType == InputEventType.DragCreate) {
                CompileDragCreateBinding(attr, descriptor);
            }
            else {
                CompileDragEventBinding(attr, descriptor);
            }
        }

        private void CompileDragCreateBinding(in AttrInfo attr, in InputHandlerDescriptor descriptor) {

            LambdaExpression lambda = BuildInputTemplateBinding(MemberData.InputEventHolder_MouseInputEvent, attr, MemberData.InputEventHolder_DragCreateResult);

            compilerContext.AddInputBinding(InputEventClass.DragCreate, descriptor, lambda);
        }

        private void CompileDragEventBinding(in AttrInfo attr, in InputHandlerDescriptor descriptor) {
            LambdaExpression lambda = BuildInputTemplateBinding(MemberData.InputEventHolder_DragEvent, attr);

            compilerContext.AddInputBinding(InputEventClass.Drag, descriptor, lambda);
        }

        private void CompileMouseInputBinding(in AttrInfo attr) {

            LambdaExpression lambda = BuildInputTemplateBinding(MemberData.InputEventHolder_MouseInputEvent, attr);

            InputHandlerDescriptor descriptor = InputCompiler.ParseMouseDescriptor(attr.key);

            compilerContext.AddInputBinding(InputEventClass.Mouse, descriptor, lambda);

        }

        private void CompileKeyboardInputBinding(in AttrInfo attr) {

            LambdaExpression lambda = BuildInputTemplateBinding(MemberData.InputEventHolder_KeyboardInputEvent, attr);

            InputHandlerDescriptor descriptor = InputCompiler.ParseKeyboardDescriptor(attr.key);

            compilerContext.AddInputBinding(InputEventClass.Keyboard, descriptor, lambda);

        }

        private void CompileMouseHandlerFromMethodAttribute(in InputAttributeData attributeData) {

            LambdaExpression lambda = BuildInputMethodAttributeBinding(attributeData, MemberData.InputEventHolder_MouseInputEvent);

            compilerContext.AddInputBinding(InputEventClass.Mouse, attributeData.descriptor, lambda);
        }

        private void CompileKeyboardHandlerFromMethodAttribute(in InputAttributeData attributeData) {

            LambdaExpression lambda = BuildInputMethodAttributeBinding(attributeData, MemberData.InputEventHolder_KeyboardInputEvent);

            compilerContext.AddInputBinding(InputEventClass.Keyboard, attributeData.descriptor, lambda);
        }

        private void CompileDragHandlerFromAttribute(in InputAttributeData attributeData) {

            LambdaExpression lambda = BuildInputMethodAttributeBinding(attributeData, MemberData.InputEventHolder_DragEvent);

            compilerContext.AddInputBinding(InputEventClass.Drag, attributeData.descriptor, lambda);
        }

        private void CompileDragCreateFromAttribute(in InputAttributeData attributeData) {

            LambdaExpression lambda = BuildInputMethodAttributeBinding(attributeData, MemberData.InputEventHolder_MouseInputEvent, MemberData.InputEventHolder_DragCreateResult);

            compilerContext.AddInputBinding(InputEventClass.Drag, attributeData.descriptor, lambda);
        }

        private void CompileInputHandlers(ProcessedType processedType) {
            StructList<InputAttributeData> handlers = InputCompiler.CompileInputAnnotations(processedType.rawType);

            const InputEventType k_KeyboardType = InputEventType.KeyDown | InputEventType.KeyUp | InputEventType.KeyHeldDown;
            const InputEventType k_DragType = InputEventType.DragCancel | InputEventType.DragDrop | InputEventType.DragEnter | InputEventType.DragEnter | InputEventType.DragExit | InputEventType.DragHover | InputEventType.DragMove;

            if (handlers != null) {

                for (int i = 0; i < handlers.size; i++) {
                    ref InputAttributeData attributeData = ref handlers.array[i];

                    if (attributeData.descriptor.handlerType == InputEventType.DragCreate) {
                        CompileDragCreateFromAttribute(attributeData);
                    }
                    else if ((attributeData.descriptor.handlerType & k_DragType) != 0) {
                        CompileDragHandlerFromAttribute(attributeData);
                    }
                    else if ((attributeData.descriptor.handlerType & k_KeyboardType) != 0) {
                        CompileKeyboardHandlerFromMethodAttribute(attributeData);
                    }
                    else {
                        CompileMouseHandlerFromMethodAttribute(attributeData);
                    }
                }
            }
        }

        private LambdaExpression BuildInputMethodAttributeBinding(in InputAttributeData attributeData, FieldInfo evtAccessor, FieldInfo assignmentTarget = null) {

            if (!attributeData.methodInfo.IsPublic) {
                // todo -- diagnostic
                throw new TemplateCompileException($"{attributeData.methodInfo.DeclaringType}.{attributeData.methodInfo} must be marked as public in order to be referenced in a template expression");
            }

            inputHandlerCompiler.Init();
            inputHandlerCompiler.Setup();

            // todo -- seems like we could cache this somehow
            inputHandlerCompiler.SetSignature(new Parameter(typeof(LinqBindingNode), "bindingNode"), new Parameter(typeof(InputEventHolder), "__eventHolder"));

            Expression methodCall = null;
            if (attributeData.useEventParameter) {
                // todo -- seems like we could cache this somehow
                Expression field = Expression.Field(inputHandlerCompiler.GetParameterAtIndex(1), evtAccessor);
                if (attributeData.parameterType != field.Type) {
                    field = Expression.TypeAs(field, attributeData.parameterType);
                }

                methodCall = ExpressionFactory.CallInstance(inputHandlerCompiler.GetElement(), attributeData.methodInfo, field);
            }
            else {
                methodCall = ExpressionFactory.CallInstance(inputHandlerCompiler.GetElement(), attributeData.methodInfo);
            }

            if (assignmentTarget != null) {
                MemberExpression assign = Expression.Field(inputHandlerCompiler.GetParameterAtIndex(1), assignmentTarget);
                inputHandlerCompiler.Assign(assign, methodCall);
            }
            else {
                inputHandlerCompiler.RawExpression(methodCall);
            }

            return inputHandlerCompiler.BuildLambda();
        }

        private LambdaExpression BuildInputTemplateBinding(FieldInfo evtAccessor, in AttrInfo attr, FieldInfo assignmentTarget = null) {
            inputHandlerCompiler.Init();
            inputHandlerCompiler.Setup();

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
                    // todo -- diagnostic
                    throw TemplateCompileException.InvalidInputHandlerLambda(attr.value, n.signature.size);
                }

                astNode = n.body;
            }

            inputHandlerCompiler.SetImplicitContext(inputHandlerCompiler.GetRoot());
            inputHandlerCompiler.SetSignature(new Parameter(typeof(LinqBindingNode), "bindingNode"), new Parameter(typeof(InputEventHolder), "__eventHolder"));

            ParameterExpression variable = inputHandlerCompiler.AddVariable(evtAccessor.FieldType, eventName);

            inputHandlerCompiler.Assign(variable, Expression.MakeMemberAccess(inputHandlerCompiler.GetParameterAtIndex(1), evtAccessor));

            compilerContext.currentEvent = variable;

            try {
                if (assignmentTarget == null) {
                    inputHandlerCompiler.Statement(astNode);
                }
                else {
                    MemberExpression field = Expression.Field(inputHandlerCompiler.GetParameterAtIndex(1), assignmentTarget);
                    Expression value = inputHandlerCompiler.Value(astNode);
                    if (!typeof(DragEvent).IsAssignableFrom(value.Type)) {
                        // todo -- diagnostic
                    }

                    inputHandlerCompiler.Assign(field, value);
                }
            }
            catch (CompileException exception) {
                exception.SetExpression(attr.rawValue + " at " + attr.line + ": " + attr.column);
                throw new TemplateCompileException(exception.Message);
            }

            compilerContext.currentEvent = null;

            LambdaExpression lambda = inputHandlerCompiler.BuildLambda();

            return lambda;

        }

        private void CompileAttributeBinding(TemplateLinqCompiler compiler, in AttrInfo attr) {

            compiler.SetImplicitContext(compiler.GetRoot());

            ParameterExpression element = compiler.GetElement();
            Expression value = compiler.TypedValue(typeof(string), attr.StrippedValue);

            if (value.Type != typeof(string)) {
                value = ExpressionFactory.CallInstance(value, value.Type.GetMethod("ToString", Type.EmptyTypes));
            }

            compiler.RawExpression(ExpressionFactory.CallInstance(element, MemberData.Element_SetAttribute, ExpressionUtil.GetStringConstant(attr.key), value));
        }

        private void CompileContextVariable(TemplateLinqCompiler compiler, ASTNode astNode, in AttrInfo attr) {

            compiler.SetImplicitContext(compiler.GetRoot());
            Expression value = compiler.Value(astNode);

            if (!compilerContext.TryAddLocalVariable(attr.key, null, value.Type, out BindingVariableDesc variable)) {
                // todo -- diagnostic
                return;
            }

            // bindingNode.SetBindingVariable<T>(index, value);
            MethodInfo setContextVariable = GetBindingVariableSetter(value.Type);

            Expression indexExpr = ExpressionUtil.GetIntConstant(variable.index);

            compiler.RawExpression(ExpressionFactory.CallInstance(compiler.GetBindingNode(), setContextVariable, indexExpr, value));

        }

        private static void BuildLambda(TemplateLinqCompiler compiler, ref LambdaExpression lambdaExpression) {
            if (!compiler.HasStatements) {
                return;
            }

            try {
                lambdaExpression = compiler.BuildLambda();
            }
            catch (Exception e) {
                // todo -- diagnostic
                Debug.Log(e);
            }
        }

        private void CompilePropertyBinding(TemplateLinqCompiler compiler, ASTNode expr, in AttrInfo attr) {

            if (ReflectionUtil.IsEvent(compilerContext.elementType.rawType, attr.key, out EventInfo eventInfo)) {
                CompileEventBinding(compiler, attr, eventInfo, expr);
                return;
            }

            LHSStatementChain left;
            Expression right = null;

            ParameterExpression element = compiler.GetElement();
            ParameterExpression root = compiler.GetRoot();
            compiler.SetImplicitContext(element);

            try {
                left = compiler.AssignableStatement(attr.key);
            }
            catch (Exception e) {
                Debug.LogError(e); // todo -- diagnostic, unroll compiler changes 
                return;
            }

            compiler.SetImplicitContext(root);

            // todo -- action / delegate support
            if (ReflectionUtil.IsFunc(left.targetExpression.Type)) {
                Type[] generics = left.targetExpression.Type.GetGenericArguments();
                Type target = generics[generics.Length - 1];
                if (HasTypeWrapper(target, out ITypeWrapper wrapper)) {
                    right = compiler.TypeWrapStatement(wrapper, left.targetExpression.Type, expr);
                }
            }

            if (right == null) {
                Expression accessor = compiler.AccessorStatement(left.targetExpression.Type, expr);

                if (accessor is ConstantExpression) {
                    right = accessor;
                }
                else {
                    right = compiler.AddVariable(left.targetExpression.Type, "__value");
                    compiler.Assign(right, accessor);
                }
            }

            // if there is a change handler we need to check for changes
            // otherwise field values can be assigned w/o checking

            // todo -- handled dotted accessors like <Element property:someArray[i].value="4"/>, likely needs parser support
            if (compilerContext.elementType.TryGetChangeHandlers(attr.key, PropertyChangedType.BindingRead, changeHandlers)) {

                ParameterExpression old = compiler.AddVariable(left.targetExpression.Type, "__oldVal");

                compiler.RawExpression(Expression.Assign(old, left.targetExpression));

                compiler.IfNotEqual(left, right, () => {

                    compiler.Assign(left, right);

                    CompilePropertyChangeHandlers(compiler, PropertyChangeSource.BindingRead, element, right, old);

                });
            }
            else {
                compiler.Assign(left, right);
            }

            if ((attr.flags & AttributeFlags.Sync) != 0) {

                if (!compilerContext.TryAddLocalVariable("sync:" + attr.key, null, left.targetExpression.Type, out BindingVariableDesc variable)) {
                    return; // todo -- diagnostic
                }

                Expression indexExpression = ExpressionUtil.GetIntConstant(variable.index);
                MethodInfo setSyncVar = GetBindingVariableSetter(variable.variableType);

                compiler.RawExpression(ExpressionFactory.CallInstance(compiler.GetBindingNode(), setSyncVar, indexExpression, right));

                // todo -- assert is update

                CompilePropertyBindingSync(indexExpression, attr, right.Type);

            }

        }

        private static void CompileEventBinding(TemplateLinqCompiler compiler, in AttrInfo attr, EventInfo eventInfo, ASTNode astNode) {
            bool hasReturnType = ReflectionUtil.IsFunc(eventInfo.EventHandlerType);
            Type[] eventHandlerTypes = eventInfo.EventHandlerType.GetGenericArguments();
            Type returnType = hasReturnType ? eventHandlerTypes[eventHandlerTypes.Length - 1] : null;

            int parameterCount = eventHandlerTypes.Length;
            if (hasReturnType) {
                parameterCount--;
            }

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

            if (astNode.type == ASTNodeType.Identifier) {
                IdentifierNode idNode = (IdentifierNode) astNode;

                if (ReflectionUtil.IsField(compiler.GetRoot().Type, idNode.name, out FieldInfo fieldInfo)) {
                    if (eventInfo.EventHandlerType.IsAssignableFrom(fieldInfo.FieldType)) {

                        LinqCompiler closure = compiler.CreateClosure(parameters, returnType);

                        string statement = fieldInfo.Name + "(";

                        for (int i = 0; i < parameters.size; i++) {
                            statement += parameters.array[i].name;
                            if (i != parameters.size - 1) {
                                statement += ", ";
                            }
                        }

                        statement += ")";
                        closure.Statement(statement);
                        LambdaExpression lambda = closure.BuildLambda();
                        ParameterExpression evtFn = compiler.AddVariable(lambda.Type, "evtFn");
                        compiler.Assign(evtFn, lambda);
                        compiler.CallStatic(MemberData.EventUtil_Subscribe, compiler.GetElement(), Expression.Constant(attr.key), evtFn);
                        closure.Release();
                        LightList<Parameter>.Release(ref parameters);

                        return;
                    }
                }

                if (ReflectionUtil.IsProperty(compiler.GetRoot().Type, idNode.name, out PropertyInfo propertyInfo)) {
                    if (eventInfo.EventHandlerType.IsAssignableFrom(propertyInfo.PropertyType)) {
                        LinqCompiler closure = compiler.CreateClosure(parameters, returnType);
                        string statement = propertyInfo.Name + "(";

                        for (int i = 0; i < parameters.size; i++) {
                            statement += parameters.array[i].name;
                            if (i != parameters.size - 1) {
                                statement += ", ";
                            }
                        }

                        statement += ")";
                        closure.Statement(statement);
                        LambdaExpression lambda = closure.BuildLambda();
                        ParameterExpression evtFn = compiler.AddVariable(lambda.Type, "evtFn");
                        compiler.Assign(evtFn, lambda);
                        compiler.CallStatic(MemberData.EventUtil_Subscribe, compiler.GetElement(), Expression.Constant(attr.key), evtFn);
                        closure.Release();
                        LightList<Parameter>.Release(ref parameters);

                        return;
                    }
                }

                if (ReflectionUtil.IsMethod(compiler.GetRoot().Type, idNode.name, out MethodInfo methodInfo)) {
                    LinqCompiler closure = compiler.CreateClosure(parameters, returnType);

                    string statement = idNode.name + "(";

                    for (int i = 0; i < parameters.size; i++) {
                        statement += parameters.array[i].name;
                        if (i != parameters.size - 1) {
                            statement += ", ";
                        }
                    }

                    statement += ")";
                    closure.Statement(statement);
                    LambdaExpression lambda = closure.BuildLambda();
                    ParameterExpression evtFn = compiler.AddVariable(lambda.Type, "evtFn");
                    compiler.Assign(evtFn, lambda);
                    compiler.CallStatic(MemberData.EventUtil_Subscribe, compiler.GetElement(), Expression.Constant(attr.key), evtFn);
                    closure.Release();
                    LightList<Parameter>.Release(ref parameters);

                    return;
                }

                LightList<Parameter>.Release(ref parameters);

                throw new TemplateCompileException($"Error compiling event handler {attr.key}={attr.value}. {idNode.name} is not assignable to type {eventInfo.EventHandlerType}");
            }

            else if (astNode.type == ASTNodeType.AccessExpression) {
                MemberAccessExpressionNode accessNode = (MemberAccessExpressionNode) astNode;
                LinqCompiler closure = compiler.CreateClosure(parameters, returnType);
                string statement;

                if (accessNode.parts[accessNode.parts.size - 1] is InvokeNode) {
                    statement = attr.value;
                }
                else {
                    statement = attr.value + "(";

                    for (int i = 0; i < parameters.size; i++) {
                        statement += parameters.array[i].name;
                        if (i != parameters.size - 1) {
                            statement += ", ";
                        }
                    }

                    statement += ")";
                }

                closure.Statement(statement);
                LambdaExpression lambda = closure.BuildLambda();
                ParameterExpression evtFn = compiler.AddVariable(lambda.Type, "evtFn");
                compiler.Assign(evtFn, lambda);
                compiler.CallStatic(MemberData.EventUtil_Subscribe, compiler.GetElement(), Expression.Constant(attr.key), evtFn);
                closure.Release();
                LightList<Parameter>.Release(ref parameters);
            }
            else {
                LinqCompiler closure = compiler.CreateClosure(parameters, returnType);

                closure.Statement(astNode);
                LambdaExpression lambda = closure.BuildLambda();
                ParameterExpression evtFn = compiler.AddVariable(lambda.Type, "evtFn");
                compiler.Assign(evtFn, lambda);
                compiler.CallStatic(MemberData.EventUtil_Subscribe, compiler.GetElement(), Expression.Constant(attr.key), evtFn);
                closure.Release();
            }

            LightList<Parameter>.Release(ref parameters);
        }

        private void CompilePropertyChangeHandlers(TemplateLinqCompiler compiler, PropertyChangeSource changeSource, ParameterExpression element, Expression right, Expression prevValue) {
            for (int j = 0; j < changeHandlers.size; j++) {

                MethodInfo methodInfo = changeHandlers.array[j].methodInfo;
                ParameterInfo[] parameters = changeHandlers.array[j].parameterInfos;

                // todo -- support PropertyChangeSource (binding read vs sync)

                if (parameters.Length == 0) {
                    compiler.RawExpression(ExpressionFactory.CallInstance(element, methodInfo));
                    continue;
                }

                if (parameters.Length == 1 && parameters[0].ParameterType == right.Type) {
                    compiler.RawExpression(ExpressionFactory.CallInstance(element, methodInfo, prevValue));
                    continue;
                }

                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(PropertyChangeSource)) {
                    compiler.RawExpression(ExpressionFactory.CallInstance(element, methodInfo, Expression.Constant(changeSource)));
                    continue;
                }

                if (parameters.Length == 2 && parameters[0].ParameterType == typeof(PropertyChangeSource) && parameters[1].ParameterType == right.Type) {
                    compiler.RawExpression(ExpressionFactory.CallInstance(element, methodInfo, Expression.Constant(changeSource), prevValue));
                    continue;
                }

                if (parameters.Length == 2 && parameters[0].ParameterType == right.Type && parameters[1].ParameterType == typeof(PropertyChangeSource)) {
                    compiler.RawExpression(ExpressionFactory.CallInstance(element, methodInfo, prevValue, Expression.Constant(changeSource)));
                    continue;
                }

                // todo -- diagnostic
                throw TemplateCompileException.UnresolvedPropertyChangeHandler(methodInfo.Name, right.Type); // todo -- better error message
            }
        }

        private void CompilePropertyBindingSync(Expression indexExpression, in AttrInfo attr, Type syncVarType) {

            Expression right;

            ParameterExpression element = lateCompiler.GetElement();
            ParameterExpression root = lateCompiler.GetRoot();

            lateCompiler.SetImplicitContext(root);

            LHSStatementChain assignableStatement = lateCompiler.AssignableStatement(attr.value);

            Expression accessor = lateCompiler.AccessorStatement(assignableStatement.targetExpression.Type, attr.value);

            if (accessor is ConstantExpression) {
                right = accessor;
            }
            else {
                right = lateCompiler.AddVariable(assignableStatement.targetExpression.Type, "__right");
                lateCompiler.Assign(right, accessor);
            }

            lateCompiler.SetImplicitContext(root);

            Expression expr = ExpressionFactory.CallInstance(lateCompiler.GetBindingNode(), GetBindingVariableGetter(syncVarType), indexExpression);

            lateCompiler.SetImplicitContext(element);

            string key = attr.key;
            string val = attr.value;

            lateCompiler.IfEqual(expr, right, () => {
                // todo -- currently only supports fields, properties should also work
                lateCompiler.Assign(assignableStatement, Expression.MakeMemberAccess(element, element.Type.GetField(key)));

                if (lateCompiler.GetContextProcessedType().TryGetChangeHandlers(val, PropertyChangedType.Synchronized, changeHandlers)) {
                    CompilePropertyChangeHandlers(lateCompiler, PropertyChangeSource.Synchronized, root, right, right);
                }

            });
        }

        private void CompileTextBinding(TextNode textNode) {

            if (textNode?.textExpressionList == null || textNode.textExpressionList.size <= 0 || textNode.IsTextConstant()) {
                return;
            }

            StructList<TextExpression> expressionParts = textNode.textExpressionList;

            MemberExpression textValueExpr = Expression.Field(updateCompiler.GetElement(), MemberData.TextElement_Text);

            updateCompiler.SetImplicitContext(updateCompiler.GetRoot());

            if (expressionParts.size == 1) {

                Expression valueExpression = updateCompiler.Value(expressionParts[0].text);

                if (valueExpression.Type == typeof(string)) {
                    updateCompiler.RawExpression(
                        Expression.IfThen(
                            Expression.NotEqual(textValueExpr, valueExpression),
                            Expression.Block(ExpressionFactory.CallInstance(updateCompiler.GetElement(), MemberData.TextElement_SetText, valueExpression))
                        )
                    );
                    return;
                }

            }

            // todo -- remove namespaces? do we even need them?
            updateCompiler.AddNamespace("UIForia.Util");
            updateCompiler.AddNamespace("UIForia.Text");
            updateCompiler.RawExpression(s_StringBuilderClear);

            for (int i = 0; i < expressionParts.size; i++) {
                if (expressionParts[i].isExpression) {

                    Expression val = updateCompiler.Value(expressionParts[i].text);
                    if (val.Type.IsEnum) {
                        MethodCallExpression toString = ExpressionFactory.CallInstance(val, val.Type.GetMethod("ToString", Type.EmptyTypes));
                        updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendString, toString));
                        continue;
                    }

                    switch (Type.GetTypeCode(val.Type)) {
                        case TypeCode.Boolean:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendBool, val));
                            break;

                        case TypeCode.Byte:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendByte, val));
                            break;

                        case TypeCode.Char:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendChar, val));
                            break;

                        case TypeCode.Decimal:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendDecimal, val));
                            break;

                        case TypeCode.Double:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendDouble, val));
                            break;

                        case TypeCode.Int16:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendInt16, val));
                            break;

                        case TypeCode.Int32:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendInt32, val));
                            break;

                        case TypeCode.Int64:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendInt64, val));
                            break;

                        case TypeCode.SByte:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendSByte, val));
                            break;

                        case TypeCode.Single:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendFloat, val));
                            break;

                        case TypeCode.String:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendString, val));
                            break;

                        case TypeCode.UInt16:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendUInt16, val));
                            break;

                        case TypeCode.UInt32:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendUInt32, val));
                            break;

                        case TypeCode.UInt64:
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendUInt64, val));
                            break;

                        default:
                            // todo -- search for a ToString(CharStringBuilder) implementation and use that if possible
                            // maybe implement special cases for common unity types
                            MethodCallExpression toString = ExpressionFactory.CallInstance(val, val.Type.GetMethod("ToString", Type.EmptyTypes));
                            updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendString, toString));
                            break;
                    }
                }
                else {
                    updateCompiler.RawExpression(ExpressionFactory.CallInstance(s_StringBuilderExpr, MemberData.StringBuilder_AppendString, Expression.Constant(expressionParts[i].text)));
                }
            }

            // todo -- this needs to check the TextInfo for equality or whitespace mutations will be ignored and we will return false from equal!!!
            Expression e = updateCompiler.GetElement();
            Expression condition = ExpressionFactory.CallInstance(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.EqualsString), new[] {typeof(string)}), textValueExpr);
            condition = Expression.Equal(condition, Expression.Constant(false));
            ConditionalExpression ifCheck = Expression.IfThen(condition, Expression.Block(ExpressionFactory.CallInstance(e, MemberData.TextElement_SetText, s_StringBuilderToString)));

            updateCompiler.RawExpression(ifCheck);
            updateCompiler.RawExpression(s_StringBuilderClear);
        }

        private static bool HasTypeWrapper(Type type, out ITypeWrapper typeWrapper) {
            if (type == typeof(RepeatItemKey)) {
                typeWrapper = s_RepeatKeyFnTypeWrapper;
                return true;
            }

            typeWrapper = null;
            return false;
        }

        private static Type GetContextVariableType(Type generic) {
            if (s_ContextVarTypeCache.TryGetValue(generic, out Type variableType)) {
                return variableType;
            }

            variableType = typeof(ContextVariable<>).MakeGenericType(ReflectionUtil.GetTempTypeArray(generic));
            s_ContextVarTypeCache[generic] = variableType;
            return variableType;
        }

        public static MethodInfo GetBindingVariableSetter(Type type) {
            if (s_BindingVariableSetterCache.TryGetValue(type, out MethodInfo setter)) {
                return setter;
            }

            ReflectionUtil.TypeArray1[0] = type;
            setter = MemberData.BindingNode_SetBindingVariable.MakeGenericMethod(ReflectionUtil.TypeArray1);
            s_BindingVariableSetterCache.Add(type, setter);
            return setter;
        }

        public static MethodInfo GetBindingVariableGetter(Type type) {
            if (s_BindingVariableGetterCache.TryGetValue(type, out MethodInfo getter)) {
                return getter;
            }

            ReflectionUtil.TypeArray1[0] = type;
            getter = MemberData.BindingNode_GetBindingVariable.MakeGenericMethod(ReflectionUtil.TypeArray1);
            s_BindingVariableGetterCache.Add(type, getter);
            return getter;
        }

        private static bool IsConstantExpression(ASTNode n) {
            while (true) {
                switch (n) {
                    case LiteralNode _:
                        return true;

                    //not sure about this one 
                    case ParenNode parenNode:
                        return parenNode.accessExpression == null && IsConstantExpression(parenNode.expression);

                    //not sure about this one 
                    case UnaryExpressionNode unary:
                        n = unary.expression;
                        continue;

                    case OperatorNode binaryExpression:
                        return IsConstantExpression(binaryExpression.left) && IsConstantExpression(binaryExpression.right);
                }

                return false;

            }
        }

    }

    public struct BindingVariableDesc {

        public Type variableType;
        public Type originTemplateType;
        public string variableName;
        public int index;
        public BindingVariableKind kind;

    }

}