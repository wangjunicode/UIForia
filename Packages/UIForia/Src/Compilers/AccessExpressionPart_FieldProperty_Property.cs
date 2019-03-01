using System;
using System.Reflection;
using UIForia.Util;

namespace UIForia.Compilers {

    public class AccessExpressionPart_FieldProperty_Property<TReturn, TInput, TNext> : AccessExpressionPart_FieldProperty<TReturn, TInput, TNext> {

        public AccessExpressionPart_FieldProperty_Property(PropertyInfo propertyInfo, AccessExpressionPart<TReturn, TNext> next) : base(next) {
            if (propertyInfo.GetGetMethod().IsStatic) {
                isStatic = true;
                if (next != null) {
                    Func<TNext> x = (Func<TNext>) ReflectionUtil.CreateStaticPropertyGetter(typeof(TInput), propertyInfo.Name);
                    getter = (input) => x();
                }
                else {
                    Func<TReturn> x = (Func<TReturn>) ReflectionUtil.CreateStaticPropertyGetter(typeof(TInput), propertyInfo.Name);
                    terminalGetter = (input) => x();
                }
            }
            else {
                if (next != null) {
                    getter = (Func<TInput, TNext>) ReflectionUtil.GetLinqPropertyAccessors(typeof(TInput), propertyInfo).getter;
                }
                else {
                    terminalGetter = (Func<TInput, TReturn>) ReflectionUtil.GetLinqPropertyAccessors(typeof(TInput), propertyInfo).getter;
                }
            }
        }

    }

}