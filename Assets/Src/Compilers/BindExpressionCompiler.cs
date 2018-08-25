using System;
using System.Reflection;

namespace Src.Compilers {

    public class BindExpressionCompiler {

        private readonly ContextDefinition context;
        private readonly ExpressionCompiler compiler;

        public BindExpressionCompiler(ContextDefinition context) {
            this.context = context;
            this.compiler = new ExpressionCompiler(context);
        }

        public Binding Compile(Type rootType, string attrKey, string attrValue) {

            FieldInfo fieldInfo = ReflectionUtil.GetFieldInfoOrThrow(rootType, attrKey);
            Expression expression = compiler.Compile(attrValue);
            ReflectionUtil.LinqAccessor accessor = ReflectionUtil.GetLinqAccessors(rootType, fieldInfo.FieldType, attrKey);

            ReflectionUtil.TypeArray2[0] = rootType;
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

        public Binding CompileTextBinding(string source) {
            return null;
        }

    }

    public class TextValueSetterBinding<U> : Binding where U : UITextElement {

        private readonly Expression<string> expression;

        public TextValueSetterBinding(Expression<string> expression) {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            UITextElement textElement = (UITextElement) element;
            string current = textElement.GetText();
            string newValue = expression.EvaluateTyped(context);
            if (current != newValue) {
                textElement.SetText(newValue);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

    // todo make overloads for numeric types, value types, and class type to avoid boxing
    public class FieldSetterBinding<U, T> : Binding where U : UIElement {

        private readonly Expression<T> expression;
        private readonly Func<U, T> getter;
        private readonly Func<U, T, T> setter;

        public FieldSetterBinding(Expression<T> expression, Func<U, T> getter, Func<U, T, T> setter) {
            this.expression = expression;
            this.getter = getter;
            this.setter = setter;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            U castElement = (U) element;
            T currentValue = getter(castElement);
            T newValue = expression.EvaluateTyped(context);
            if (currentValue.Equals(newValue)) {
                setter(castElement, newValue);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}