using System;
using System.Collections.Generic;
using System.Reflection;
using Src.Compilers.AliasSource;
using Src.Extensions;
using Src.Util;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using ElementCallback = System.Action<UIElement, string>;

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

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class OnPropertyChanged : Attribute {

            public readonly string propertyName;

            public OnPropertyChanged(string propertyName) {
                this.propertyName = propertyName;
            }

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

            Dictionary<string, LightList<ElementCallback>> actionMap = null;
            if (!m_TypeMap.ContainsKey(targetType)) {
                MethodInfo[] methods = targetType.GetMethods(ReflectionUtil.InstanceBindFlags);

                for (int i = 0; i < methods.Length; i++) {
                    MethodInfo info = methods[i];
                    object[] customAttributes = info.GetCustomAttributes(typeof(OnPropertyChanged), true);

                    if (customAttributes.Length == 0) continue;

                    ParameterInfo[] parameterInfos = info.GetParameters();
                    if (!info.IsStatic && parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(string)) {
                        if (actionMap == null) {
                            actionMap = m_TypeMap.GetOrDefault(targetType);
                        }

                        if (actionMap == null) {
                            actionMap = new Dictionary<string, LightList<ElementCallback>>();
                            m_TypeMap.Add(targetType, actionMap);
                        }

                        for (int j = 0; j < customAttributes.Length; j++) {
                            OnPropertyChanged attr = (OnPropertyChanged) customAttributes[j];
                            GetHandlerList(actionMap, attr.propertyName).Add((ElementCallback) ReflectionUtil.GetDelegate(typeof(UIElement), info));
                        }
                    }
                }
            }

            LightList<ElementCallback> list = actionMap?.GetOrDefault(attrKey);
            if (list != null) {
                ReflectionUtil.ObjectArray5[0] = attrKey;
                ReflectionUtil.ObjectArray5[1] = expression;
                ReflectionUtil.ObjectArray5[2] = accessor.fieldGetter;
                ReflectionUtil.ObjectArray5[3] = accessor.fieldSetter;
                ReflectionUtil.ObjectArray5[4] = list.List;

                return (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(FieldSetterBinding_WithCallbacks<,>),
                    ReflectionUtil.TypeArray2,
                    ReflectionUtil.ObjectArray5
                );
            }

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

        private static readonly Dictionary<Type, Dictionary<string, LightList<ElementCallback>>> m_TypeMap = new Dictionary<Type, Dictionary<string, LightList<ElementCallback>>>();

        private static LightList<ElementCallback> GetHandlerList(Dictionary<string, LightList<ElementCallback>> map, string name) {
            LightList<ElementCallback> list = map.GetOrDefault(name);
            if (list == null) {
                list = new LightList<ElementCallback>();
                map[name] = list;
            }

            return list;
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