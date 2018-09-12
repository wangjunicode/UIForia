using System;
using System.Diagnostics;
using System.Reflection;

namespace Src.Compilers {

    public class PropertyBindingCompiler {

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

        public void SetContext(ContextDefinition context) {
            this.compiler.SetContext(context);
        }

        public Binding CompileAttribute(Type targetType, AttributeDefinition attributeDefinition) {
            string attrKey = attributeDefinition.key;
            string attrValue = attributeDefinition.value;

            EventInfo eventInfo = targetType.GetEvent(attrKey);
            
            if (eventInfo != null) {
                return CompileCallbackAttribute(attrKey, attrValue, eventInfo);
            }

            FieldInfo fieldInfo = ReflectionUtil.GetFieldInfoOrThrow(targetType, attrKey);

            if (ReflectionUtil.IsCallbackType(fieldInfo.FieldType)) {
                //todo -- figure out how to handle Func and Action fields
            }

            Expression expression = compiler.Compile(attrValue);
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