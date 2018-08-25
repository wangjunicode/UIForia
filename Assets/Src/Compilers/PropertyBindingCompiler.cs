using System;
using System.Reflection;

namespace Src.Compilers {

    public class PropertyBindingCompiler {

        private ContextDefinition context;
        private readonly ExpressionCompiler compiler;

        public PropertyBindingCompiler(ContextDefinition context) {
            this.context = context;
            this.compiler = new ExpressionCompiler(context);
        }

        public void SetContext(ContextDefinition context) {
            this.context = context;
            this.compiler.SetContext(context);
        }

        public Binding CompileAttribute(AttributeDefinition attributeDefinition) {
            string attrKey = attributeDefinition.key;
            string attrValue = attributeDefinition.value;
            
            FieldInfo fieldInfo = ReflectionUtil.GetFieldInfoOrThrow(context.rootType, attrKey);
            Expression expression = compiler.Compile(attrValue);
            ReflectionUtil.LinqAccessor accessor = ReflectionUtil.GetLinqAccessors(context.rootType, fieldInfo.FieldType, attrKey);

            ReflectionUtil.TypeArray2[0] = context.rootType;
            ReflectionUtil.TypeArray2[1] = fieldInfo.FieldType;

            ReflectionUtil.ObjectArray3[0] = expression;
            ReflectionUtil.ObjectArray3[1] = accessor.fieldGetter;
            ReflectionUtil.ObjectArray3[2] = accessor.fieldSetter;

            return (Binding) ReflectionUtil.CreateGenericInstanceFromOpenType(
                typeof(FieldSetterBinding<,>),
                ReflectionUtil.TypeArray2,
                ReflectionUtil.ObjectArray3
            );

        }

    }

  
}