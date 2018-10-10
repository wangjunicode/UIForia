using System;
using System.Collections.Generic;
using System.Reflection;
using Src.Compilers.AliasSource;
using Src.Extensions;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Src.Compilers {

    public class PropertyBindingCompiler {

        private static readonly Dictionary<Type, List<IAliasSource>> aliasMap = new Dictionary<Type, List<IAliasSource>>();

        public static readonly string EvtArgDefaultName = "$event";

        public static readonly string[] EvtArgNames = {
            "$eventArg0",
            "$eventArg1",
            "$eventArg2",
            "$eventArg3"
        };

        private readonly ExpressionCompiler compiler;

        public PropertyBindingCompiler(ContextDefinition context) {
            this.compiler = new ExpressionCompiler(context);
        }

        // todo -- maybe move this to the compiler itself so it can be used per-expression 
        static PropertyBindingCompiler() {
            AddTypedAliasSource(typeof(Color), new ColorAliasSource());
            AddTypedAliasSource(typeof(Color), new MethodAliasSource("rgb", ColorAliasSource.ColorConstructor));
            AddTypedAliasSource(typeof(Color), new MethodAliasSource("rgba", ColorAliasSource.ColorConstructorAlpha));
        }

        public void SetContext(ContextDefinition context) {
            this.compiler.SetContext(context);
        }


        public static void AddTypedAliasSource(Type type, IAliasSource aliasSource) {
            if (type == null || aliasSource == null) return;
            List<IAliasSource> list = aliasMap.GetOrDefault(type);
            if (list == null) {
                list = new List<IAliasSource>();
                aliasMap[type] = list;
            }

            list.Add(aliasSource);
        }

        public Binding CompileAttribute(Type targetType, AttributeDefinition attributeDefinition) {
            string attrKey = attributeDefinition.key;
            string attrValue = attributeDefinition.value;

            EventInfo eventInfo = targetType.GetEvent(attrKey);

            if (eventInfo != null) {
                return CompileCallbackAttribute(attrKey, attrValue, eventInfo);
            }

            FieldInfo fieldInfo = ReflectionUtil.GetFieldInfoOrThrow(targetType, attrKey);

            //todo -- figure out how to handle Func and Action fields

            List<IAliasSource> aliasSources = aliasMap.GetOrDefault(fieldInfo.FieldType);

            if (aliasSources != null) {
                for (int i = 0; i < aliasSources.Count; i++) {
                    compiler.context.AddConstAliasSource(aliasSources[i]);
                }
            }

            Expression expression = compiler.Compile(attrValue);

            if (aliasSources != null) {
                for (int i = 0; i < aliasSources.Count; i++) {
                    compiler.context.RemoveConstAliasSource(aliasSources[i]);
                }
            }

            ReflectionUtil.LinqAccessor accessor = ReflectionUtil.GetLinqAccessors(targetType, fieldInfo.FieldType, attrKey);

            ReflectionUtil.TypeArray2[0] = targetType;
            ReflectionUtil.TypeArray2[1] = fieldInfo.FieldType;

            ReflectionUtil.ObjectArray4[0] = attrKey;
            ReflectionUtil.ObjectArray4[1] = expression;
            ReflectionUtil.ObjectArray4[2] = accessor.fieldGetter;
            ReflectionUtil.ObjectArray4[3] = accessor.fieldSetter;

            return (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(FieldSetterBinding<,>),
                ReflectionUtil.TypeArray2,
                ReflectionUtil.ObjectArray4
            );
        }

        private Binding CompileCallbackAttribute(string key, string value, EventInfo eventInfo) {
            MethodInfo info = eventInfo.EventHandlerType.GetMethod("Invoke");
            Debug.Assert(info != null, nameof(info) + " != null");

            ParameterInfo[] delegateParameters = info.GetParameters();

            Type[] argTypes = new Type[delegateParameters.Length];

            for (int i = 0; i < delegateParameters.Length; i++) {
                argTypes[i] = delegateParameters[i].ParameterType;
                compiler.AddRuntimeAlias(EvtArgNames[i], typeof(string));
            }

            if (argTypes.Length > 0) {
                compiler.AddRuntimeAlias(EvtArgDefaultName, argTypes[0]);
            }

            Expression<Terminal> expression = compiler.Compile<Terminal>(value);
            if (argTypes.Length > 0) {
                compiler.RemoveRuntimeAlias(EvtArgDefaultName);
            }

            for (int i = 0; i < delegateParameters.Length; i++) {
                compiler.RemoveRuntimeAlias(EvtArgNames[i]);
            }

            ReflectionUtil.ObjectArray2[0] = expression;
            ReflectionUtil.ObjectArray2[1] = eventInfo;

            switch (argTypes.Length) {
                case 0:
                    return new CallbackBinding(expression, eventInfo);
                case 1:
                    return (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(CallbackBinding<>),
                        argTypes,
                        ReflectionUtil.ObjectArray2
                    );
                case 2:
                    return (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(CallbackBinding<,>),
                        argTypes,
                        ReflectionUtil.ObjectArray2
                    );
                    ;
                case 3:
                    return (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(CallbackBinding<,,>),
                        argTypes,
                        ReflectionUtil.ObjectArray2
                    );
                case 4:
                    return (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(CallbackBinding<,,,>),
                        argTypes,
                        ReflectionUtil.ObjectArray2
                    );
            }

            throw new Exception("Can't handle callbacks with more than four parameters");
        }

    }

}