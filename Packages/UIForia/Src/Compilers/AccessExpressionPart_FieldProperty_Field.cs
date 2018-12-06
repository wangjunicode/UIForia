using System;
using System.Reflection;

namespace UIForia.Compilers {

    public class AccessExpressionPart_FieldProperty_Field<TReturn, TInput, TNext> : AccessExpressionPart_FieldProperty<TReturn, TInput, TNext> {

        public AccessExpressionPart_FieldProperty_Field(FieldInfo fieldInfo, AccessExpressionPart<TReturn, TNext> next) : base(next) {
            if (fieldInfo.IsStatic) {
                isStatic = true;
                if (next != null) {
                    Func<TNext> x = (Func<TNext>) ReflectionUtil.CreateStaticFieldGetter(typeof(TInput), fieldInfo.Name);
                    getter = (input) => x();
                }
                else {
                    Func<TReturn> x = (Func<TReturn>) ReflectionUtil.CreateStaticFieldGetter(typeof(TInput), fieldInfo.Name);
                    terminalGetter = (input) => x();
                }
            }
            else {
                if (next != null) {
                    getter = (Func<TInput, TNext>) ReflectionUtil.GetLinqFieldAccessors(typeof(TInput), fieldInfo).getter;
                }
                else {
                    terminalGetter = (Func<TInput, TReturn>) ReflectionUtil.GetLinqFieldAccessors(typeof(TInput), fieldInfo).getter;
                }
            }
        }

    }

}