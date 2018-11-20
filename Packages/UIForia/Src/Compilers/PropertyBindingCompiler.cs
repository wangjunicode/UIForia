using System;
using System.Collections.Generic;
using System.Reflection;
using UIForia.Extensions;
using UIForia.Compilers.AliasSource;
using UIForia.Util;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using ElementCallback = System.Action<UIElement, string>;

namespace UIForia.Compilers {

    public class PropertyBindingCompiler {

        private static readonly Dictionary<Type, List<IAliasSource>> aliasMap = new Dictionary<Type, List<IAliasSource>>();

        public static readonly string EvtArgDefaultName = "$event";
        
        private static readonly Dictionary<Type, Dictionary<string, LightList<object>>> m_TypeMap = new Dictionary<Type, Dictionary<string, LightList<object>>>();

        public const string k_BindTo = "bindTo";
        public const string k_Once = "once";

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

        private Binding CompileBoundProperty(Type rootType, Type targetType, string property, string value) {
            // bind pointer is a field or property on the target element
            // target field/property type must match bind type
            EventInfo eventInfo = targetType.GetEvent(value);
            if (eventInfo == null) {
                throw new ParseException("Error compiling 'bindTo' expression. Tried to find an event with " +
                                         $"the name {value} on type {targetType.FullName} but none was found");
            }

            MethodInfo info = eventInfo.EventHandlerType.GetMethod("Invoke");
            Debug.Assert(info != null, nameof(info) + " != null");

            ParameterInfo[] parameters = info.GetParameters();
            if (parameters.Length != 1) {
                throw new ParseException($"Error compiling 'bindTo' expression on type {targetType.FullName}. " +
                                         "The bind target must be an event of type Action<T>, actual event type " +
                                         "has more than one parameter.");
            }

            Type genericArgument = parameters[0].ParameterType;

            FieldInfo fieldInfo = ReflectionUtil.GetFieldInfo(rootType, property);

            Dictionary<string, LightList<object>> actionMap = GetActionMap(rootType);

            if (fieldInfo != null) {
                LightList<object> list = actionMap?.GetOrDefault(property);
                if (list != null) {
                    ReflectionUtil.ObjectArray3[0] = fieldInfo;
                    ReflectionUtil.ObjectArray3[1] = eventInfo;
                    ReflectionUtil.ObjectArray3[2] = list;

                    ReflectionUtil.TypeArray2[0] = genericArgument;
                    ReflectionUtil.TypeArray2[1] = rootType;
                    return (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(AssignmentCallbackBinding_Root_Field_WithCallbacks<,>),
                        ReflectionUtil.TypeArray2,
                        ReflectionUtil.ObjectArray3
                    );
                }

                ReflectionUtil.ObjectArray2[0] = fieldInfo;
                ReflectionUtil.ObjectArray2[1] = eventInfo;
                return (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(AssignmentCallbackBinding_Root_Field<>),
                    genericArgument,
                    ReflectionUtil.ObjectArray2
                );
            }

            PropertyInfo propertyInfo = ReflectionUtil.GetPropertyInfo(rootType, property);
            if (propertyInfo != null) {
                LightList<object> list = actionMap?.GetOrDefault(property);
                if (list != null) {
                    ReflectionUtil.ObjectArray3[0] = propertyInfo;
                    ReflectionUtil.ObjectArray3[1] = eventInfo;
                    ReflectionUtil.ObjectArray3[2] = list;

                    ReflectionUtil.TypeArray2[0] = genericArgument;
                    ReflectionUtil.TypeArray2[1] = rootType;
                    return (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                        typeof(AssignmentCallbackBinding_Root_Property_WithCallbacks<,>),
                        ReflectionUtil.TypeArray2,
                        ReflectionUtil.ObjectArray3
                    );
                }

                ReflectionUtil.ObjectArray2[0] = propertyInfo;
                ReflectionUtil.ObjectArray2[1] = eventInfo;

                return (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                    typeof(AssignmentCallbackBinding_Root_Property<>),
                    genericArgument,
                    ReflectionUtil.ObjectArray2
                );
            }

            throw new ParseException($"Error compiling 'bindTo' expression on type {targetType.FullName}. " +
                                     $"Unable to find field or property called {property} on type {rootType.FullName}");
        }

        public Binding CompileAttribute(Type targetType, AttributeDefinition attributeDefinition) {
            string attrKey = attributeDefinition.key;
            string attrValue = attributeDefinition.value;

            EventInfo eventInfo = targetType.GetEvent(attrKey);

            if (eventInfo != null) {
                return CompileCallbackAttribute(attrValue, eventInfo);
            }

            if (attrKey.IndexOf(".", StringComparison.Ordinal) != -1) {
                string[] parts = attrKey.Split('.');
                string property = parts[0];
                string modifier = parts[1];
                if (modifier == k_BindTo) {
                    return CompileBoundProperty(compiler.context.rootType, targetType, property, attrValue);
                }

                if (modifier == k_Once) {
                    Binding binding = CompileBinding(targetType, property, attrValue);
                    if (binding == null) {
                        return null;
                    }

                    binding.bindingType = BindingType.Once;
                }
                
                if (property == "style") return null;

                throw new ParseException($"Unsupported attribute binding extension: '{attrKey}'");
            }

            return CompileBinding(targetType, attrKey, attrValue);
        }

        private Binding CompileBinding(Type targetType, string attrKey, string attrValue) {
            
            FieldInfo fieldInfo = ReflectionUtil.GetFieldInfo(targetType, attrKey);

            if (fieldInfo != null) {
                return CompileFieldAttribute(fieldInfo, targetType, attrKey, attrValue);
            }

            PropertyInfo propertyInfo = ReflectionUtil.GetPropertyInfo(targetType, attrKey);
            if (propertyInfo != null) {
                return CompilePropertyAttribute(propertyInfo, targetType, attrKey, attrValue);
            }
            
            throw new ParseException(attrKey + " is a not a field or property on type " + targetType);
        }

        private Binding CompilePropertyAttribute(PropertyInfo propertyInfo, Type targetType, string attrKey, string attrValue) {
            List<IAliasSource> aliasSources = aliasMap.GetOrDefault(propertyInfo.PropertyType);

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

            ReflectionUtil.LinqAccessor accessor = ReflectionUtil.GetLinqPropertyAccessors(targetType, propertyInfo.PropertyType, attrKey);

            ReflectionUtil.TypeArray2[0] = targetType;
            ReflectionUtil.TypeArray2[1] = propertyInfo.PropertyType;

            if (!propertyInfo.PropertyType.IsAssignableFrom(expression.YieldedType)) {
                UnityEngine.Debug.Log($"Error compiling binding: {attrKey}={attrValue}, Type {propertyInfo.PropertyType} is not assignable from {expression.YieldedType}");
                return null;
            }

            Dictionary<string, LightList<object>> actionMap = GetActionMap(targetType);
            
            // todo -- with callbacks
            
            ReflectionUtil.ObjectArray4[0] = attrKey;
            ReflectionUtil.ObjectArray4[1] = expression;
            ReflectionUtil.ObjectArray4[2] = accessor.fieldGetter;
            ReflectionUtil.ObjectArray4[3] = accessor.fieldSetter;
            return (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(PropertySetterBinding<,>),
                ReflectionUtil.TypeArray2,
                ReflectionUtil.ObjectArray4
            );
        }
        

        private Binding CompileFieldAttribute(FieldInfo fieldInfo, Type targetType, string attrKey, string attrValue) {
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

            ReflectionUtil.LinqAccessor accessor = ReflectionUtil.GetLinqFieldAccessors(targetType, fieldInfo.FieldType, attrKey);

            ReflectionUtil.TypeArray2[0] = targetType;
            ReflectionUtil.TypeArray2[1] = fieldInfo.FieldType;

            if (!fieldInfo.FieldType.IsAssignableFrom(expression.YieldedType)) {
                UnityEngine.Debug.Log($"Error compiling binding: {attrKey}={attrValue}, Type {fieldInfo.FieldType} is not assignable from {expression.YieldedType}");
                return null;
            }

            Dictionary<string, LightList<object>> actionMap = GetActionMap(targetType);

            LightList<object> list = actionMap?.GetOrDefault(attrKey);
            if (list != null) {
                ReflectionUtil.ObjectArray5[0] = attrKey;
                ReflectionUtil.ObjectArray5[1] = expression;
                ReflectionUtil.ObjectArray5[2] = accessor.fieldGetter;
                ReflectionUtil.ObjectArray5[3] = accessor.fieldSetter;
                ReflectionUtil.ObjectArray5[4] = list;

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

        private static LightList<object> GetHandlerList(Dictionary<string, LightList<object>> map, string name) {
            LightList<object> list = map.GetOrDefault(name);
            if (list == null) {
                list = new LightList<object>();
                map[name] = list;
            }

            return list;
        }

        private Dictionary<string, LightList<object>> GetActionMap(Type targetType) {
            Dictionary<string, LightList<object>> actionMap;
            if (!m_TypeMap.TryGetValue(targetType, out actionMap)) {
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
                            actionMap = new Dictionary<string, LightList<object>>();
                            m_TypeMap.Add(targetType, actionMap);
                        }

                        for (int j = 0; j < customAttributes.Length; j++) {
                            OnPropertyChanged attr = (OnPropertyChanged) customAttributes[j];
                            Type a = ReflectionUtil.GetOpenDelegateType(info);
                            GetHandlerList(actionMap, attr.propertyName).Add(ReflectionUtil.GetDelegate(a, info));
                        }
                    }
                }

                // use null as a marker in the dictionary regardless of whether or not we have actions registered
                m_TypeMap[targetType] = actionMap;
            }

            return actionMap;
        }

        private Binding CompileCallbackAttribute(string value, EventInfo eventInfo) {
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