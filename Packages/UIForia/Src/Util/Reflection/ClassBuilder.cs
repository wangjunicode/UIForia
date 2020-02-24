using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using UIForia.Compilers;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Parsing.Expressions.AstNodes;
using Debug = UnityEngine.Debug;

namespace UIForia.Util {

    public class ClassBuilder {

        private readonly AssemblyName assemblyName;
        private readonly AssemblyBuilder assemblyBuilder;
        private readonly ModuleBuilder moduleBuilder;
        private readonly Dictionary<string, Type> typeMap;
        private readonly LinqCompiler linqCompiler;

        public ClassBuilder() {
            this.linqCompiler = new LinqCompiler();
            this.assemblyName = new AssemblyName("UIForia.Generated");
            this.assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            this.moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            this.typeMap = new Dictionary<string, Type>();
        }

        public void Reset() {
            typeMap.Clear();
        }

        public Type CreateGenericRuntimeType(string id, Type baseType, ReflectionUtil.GenericTypeDefinition[] generics, IList<ReflectionUtil.FieldDefinition> fieldDefinitions, IList<string> namespaces) {
            if (typeMap.ContainsKey(id)) {
                return null; //todo -- exception
            }

            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                id,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                baseType
            );

            string[] typeNames = new string[generics.Length];

            for (int i = 0; i < generics.Length; i++) {
                typeNames[i] = generics[i].name;
            }

            GenericTypeParameterBuilder[] typeParams = typeBuilder.DefineGenericParameters(typeNames);

            for (int i = 0; i < fieldDefinitions.Count; i++) {
                // string typeName = fieldDefinitions[i].fieldType;
                //
                // Type fieldType = ResolveFieldTypeFromGenerics(typeName, typeParams);
                //
                // if (fieldType == null) {
                //     fieldType = TypeProcessor.ResolveTypeExpression(null, namespaces, typeName);
                // }
                //
                // typeBuilder.DefineField(fieldDefinitions[i].fieldName, fieldType, FieldAttributes.Public);
            }

            Type retn = typeBuilder.CreateType();
            typeMap[id] = retn;
            return retn;
        }

        private static Type ResolveFieldTypeFromGenerics(string fieldType, GenericTypeParameterBuilder[] typeParams) {
            for (int i = 0; i < typeParams.Length; i++) {
                if (fieldType == typeParams[i].Name) {
                    return typeParams[i];
                }
            }

            return null;
        }

        private static void GenerateMethodCallIL(FieldBuilder fieldBuilder, MethodInfo methodInfo, ILGenerator il, Type[] parameters) {
            il.Emit(OpCodes.Ldsfld, fieldBuilder);

            int argIdx = 0;
            // first 3 args can go directly in. assume we always get arg0 as 'this'
            for (argIdx = 0; argIdx < parameters.Length; argIdx++) {
                switch (argIdx) {
                    case 0:
                        il.Emit(OpCodes.Ldarg_0);
                        break;
                    case 1:
                        il.Emit(OpCodes.Ldarg_1);
                        break;
                    case 2:
                        il.Emit(OpCodes.Ldarg_2);
                        break;
                    case 3:
                        il.Emit(OpCodes.Ldarg_3);
                        break;
                    default:
                        il.Emit(OpCodes.Ldarg_S);
                        break;
                }
            }

            il.EmitCall(OpCodes.Callvirt, methodInfo, null);

            if (methodInfo.ReturnType == typeof(void)) {
                il.Emit(OpCodes.Nop);
            }
            // else {
            //     il.Emit(OpCodes.Stloc_0);
            //     il.Emit(OpCodes.Br_S, targetInstruction);
            //     il.MarkLabel(targetInstruction);
            //     il.Emit(OpCodes.Ldloc_0);
            // }

            il.Emit(OpCodes.Ret);

            // for (int i = 0; i < parameters.Length; i++) {
            //     if (parameters[i].IsByRef) {
            //         parameters[i] = parameters[i].GetElementType();
            //     }
            // }
            //
            //
            // LocalBuilder[] locals = new LocalBuilder[parameters.Length];
            //
            // for (int i = 0; i < parameters.Length; i++) {
            //     locals[i] = il.DeclareLocal(parameters[i], true);
            // }
            //
            // for (int i = 0; i < parameters.Length; i++) {
            //     il.Emit(OpCodes.Ldarg_1);
            //     EmitFastInt(il, i);
            //     il.Emit(OpCodes.Ldelem_Ref);
            //     EmitCastToReference(il, parameters[i]);
            //     il.Emit(OpCodes.Stloc, locals[i]);
            // }
            //
            // if (!methodInfo.IsStatic) {
            //     il.Emit(OpCodes.Ldarg_0);
            // }
            //
            // for (int i = 0; i < parameters.Length; i++) {
            //     if (parameters[i].IsByRef)
            //         il.Emit(OpCodes.Ldloca_S, locals[i]);
            //     else
            //         il.Emit(OpCodes.Ldloc, locals[i]);
            // }
            //
            // if (methodInfo.IsStatic) {
            //     il.EmitCall(OpCodes.Call, methodInfo, null);
            // }
            // else {
            //     il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            // }
            //
            // if (methodInfo.ReturnType == typeof(void)) {
            //     il.Emit(OpCodes.Ldnull);
            // }
            // else {
            //     EmitBoxIfNeeded(il, methodInfo.ReturnType);
            // }
            //
            // for (int i = 0; i < parameters.Length; i++) {
            //     if (parameters[i].IsByRef) {
            //         il.Emit(OpCodes.Ldarg_1);
            //         EmitFastInt(il, i);
            //         il.Emit(OpCodes.Ldloc, locals[i]);
            //         if (locals[i].LocalType.IsValueType) {
            //             il.Emit(OpCodes.Box, locals[i].LocalType);
            //         }
            //
            //         il.Emit(OpCodes.Stelem_Ref);
            //     }
            // }
            //
            // il.Emit(OpCodes.Ret);
        }

        private static void EmitCastToReference(ILGenerator il, System.Type type) {
            if (type.IsValueType) {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        private static void EmitBoxIfNeeded(ILGenerator il, System.Type type) {
            if (type.IsValueType) {
                il.Emit(OpCodes.Box, type);
            }
        }

        private static void EmitFastInt(ILGenerator il, int value) {
            switch (value) {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128) {
                il.Emit(OpCodes.Ldc_I4_S, (sbyte) value);
            }
            else {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }

        public Type CreateRuntimeType(string id, Type baseType, IList<ReflectionUtil.FieldDefinition> fields, IList<ReflectionUtil.MethodDefinition> methods, IList<string> namespaces) {
            if (typeMap.ContainsKey(id)) {
                return null;
            }

            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                id,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                baseType
            );

            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            if (fields != null) {
                for (int i = 0; i < fields.Count; i++) {
                    Type fieldType = TypeProcessor.ResolveType(fields[i].fieldType, (IReadOnlyList<string>) namespaces);

                    typeBuilder.DefineField(fields[i].fieldName, fieldType, FieldAttributes.Public);
                }
            }

            const MethodAttributes k_MethodAttributes = MethodAttributes.Public | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            StructList<ExpressionData> expressionData = StructList<ExpressionData>.Get();

            if (methods != null) {
                for (int i = 0; i < methods.Count; i++) {
                    ReflectionUtil.MethodDefinition methodDefinition = methods[i];

                    Type returnType = null;

                    if (methodDefinition.returnType.typeName != null && methodDefinition.returnType.typeName != "void") {
                        returnType = TypeProcessor.ResolveType(methodDefinition.returnType, (IReadOnlyList<string>) namespaces);
                    }

                    // todo -- allocate less
                    ResolvedParameter[] signature = new ResolvedParameter[methodDefinition.arguments.Length];
                    Type[] staticSignature = new Type[signature.Length + 1];
                    Type[] signatureTypes = new Type[methodDefinition.arguments.Length];

                    // because this type doesn't exist yet, making a delegate the references it fails upon type creation.
                    // instead we resort to treating 'this' as an object type and invoking methods via cast.
                    // this won't be the case for production code though
                    staticSignature[0] = typeof(object);
                    
                    for (int j = 0; j < methodDefinition.arguments.Length; j++) {
                        Type type = TypeProcessor.ResolveType(methodDefinition.arguments[i].type.Value, (IReadOnlyList<string>) namespaces);
                        signatureTypes[j] = type;
                        signature[j] = new ResolvedParameter() {
                            type = type,
                            name = methodDefinition.arguments[i].identifier
                        };
                        staticSignature[j + 1] = signature[j].type;
                    }

                    Type fnType = ReflectionUtil.GetClosedDelegateType(staticSignature, returnType ?? typeof(void));

                    FieldBuilder fnField = typeBuilder.DefineField("__" + methodDefinition.methodName, fnType, FieldAttributes.Public | FieldAttributes.Static);
                    
                    MethodBuilder method = typeBuilder.DefineMethod(
                        methodDefinition.methodName,
                        k_MethodAttributes,
                        CallingConventions.Standard,
                        returnType,
                        signatureTypes
                    );

                    GenerateMethodCallIL(fnField, fnType.GetMethod("Invoke"), method.GetILGenerator(), staticSignature);

                    expressionData.Add(new ExpressionData() {
                        body = methodDefinition.body,
                        signature = signature,
                        returnType = returnType,
                        name = methodDefinition.methodName
                    });
                }
            }

            Type retn = typeBuilder.CreateType();
            typeMap[id] = retn;

            FieldInfo[] fieldInfos = retn.GetFields(BindingFlags.Public | BindingFlags.Static);

            if (methods != null) {
                for (int i = 0; i < methods.Count; i++) {
                    linqCompiler.Reset();
                    ref ExpressionData exprData = ref expressionData.array[i];

                    LightList<Parameter> parameters = LightList<Parameter>.Get();

                    parameters.Add(new Parameter(typeof(object), "__thisObject", ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));

                    for (int j = 0; j < exprData.signature.Length; j++) {
                        ref ResolvedParameter parameter = ref exprData.signature[j];
                        parameters.Add(new Parameter(parameter.type, parameter.name));
                    }

                    Expression castExpr = Expression.Convert(parameters[0], retn);

                    linqCompiler.SetSignature(parameters, exprData.returnType);
                    linqCompiler.SetImplicitContext(parameters[0]);

                    ParameterExpression thisRef = linqCompiler.AddVariable(retn, "_this");
                    linqCompiler.RawExpression(Expression.Assign(thisRef, castExpr));
                    linqCompiler.SetImplicitContext(thisRef);
                    linqCompiler.StatementList(exprData.body);
                    Delegate fn = linqCompiler.Compile();
                    FieldInfo fieldInfo = null;
                    for (int j = 0; j < fieldInfos.Length; j++) {
                        if (fieldInfos[j].Name == "__" + exprData.name) {
                            fieldInfo = fieldInfos[j];
                            break;
                        }
                    }

                    // todo -- somehow need to save the lambda to be printed by code generator.
                    // probably in a dictionary on reflection util?
                    fieldInfo.SetValue(null, fn);
                }
            }

            expressionData.Release();
            return retn;
        }


        private struct ExpressionData {

            public ResolvedParameter[] signature;
            public Type returnType;
            public BlockNode body;
            public string name;

        }

        private struct ResolvedParameter {

            public Type type;
            public string name;

        }

        public Type GetCreatedType(string id) {
            Type type = null;
            typeMap.TryGetValue(id, out type);
            return type;
        }

        public bool TryCreateInstance<T>(string id, out T instance) {
            if (typeMap.TryGetValue(id, out Type toCreate)) {
                instance = (T) Activator.CreateInstance(toCreate);
                return true;
            }

            instance = default;
            return false;
        }

    }

}